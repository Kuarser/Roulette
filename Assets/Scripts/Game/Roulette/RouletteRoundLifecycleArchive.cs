using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class RecordedRoundArchiveV1
        {
            public int lifecycleSchemaVersion;
            public RecordedRound[] rounds;
            public PcRoundLifecycleRecord lifecycle;
        }

        [Serializable]
        private sealed class RecordedRoundArchiveV2
        {
            public int lifecycleSchemaVersion;
            public RecordedRound[] rounds;
            public PcRoundLifecycleRecord lifecycle;
            public PcDoubleFeatureNvV1 featureNv;
        }

        [Serializable]
        private sealed class RecordedRoundArchiveV3
        {
            public int lifecycleSchemaVersion;
            public RecordedRound[] rounds;
            public PcRoundLifecycleRecord lifecycle;
            public PcDoubleFeatureNvV1 featureNv;
            public PcGamePaymentRecordV1 payment;
            public RecordedHistoryOverflowV1 historyOverflow;
        }

        private PcRoundLifecycleRecord latestRoundLifecycle;
        private int pcRoundArchiveSchemaVersion = 1;
        private PcDoubleFeatureNvV1 pcDoubleFeatureNv;
        private PcGamePaymentRecordV1 pcGamePayment;

        private bool HasTrustedLatestLifecycle
        {
            // Only strictly decoded or explicitly created receipts are
            // published here. Fault handlers can query provenance even when
            // a later attempted transition has made the live value invalid.
            get { return latestRoundLifecycle != null; }
        }

        private PcRoundLifecycleRecord GetLatestLifecycleForRestore()
        {
            if (latestRoundLifecycle == null) return null;
            if (recordedRoundPages.Count == 0)
                throw new PcRoundLifecycleDataException("Round lifecycle has no latest page");
            ValidateLifecycleLatestIdentity(latestRoundLifecycle, recordedRoundPages[0]);
            ValidateRoundLifecyclePayload(latestRoundLifecycle, recordedRoundPages[0]);
            if (pcRoundArchiveSchemaVersion == 3)
                ValidatePcGamePayment(pcGamePayment, recordedRoundPages[0], latestRoundLifecycle, pcDoubleFeatureNv);
            return latestRoundLifecycle;
        }

        // Parsing never publishes pages, changes the current receipt, or writes
        // PlayerPrefs. False means a syntactically valid headerless legacy object.
        private static bool TryDecodeLifecycleArchive(string encoded,
            IList<byte[]> snapshots, out RecordedRound[] validatedRounds,
            out PcRoundLifecycleRecord validatedLifecycle)
        {
            PcDoubleFeatureNvV1 feature;
            int schemaVersion;
            bool claimed = TryDecodeLifecycleArchiveWithFeature(encoded, snapshots,
                out validatedRounds, out validatedLifecycle, out feature, out schemaVersion);
            if (claimed && schemaVersion != 1)
                throw new PcRoundLifecycleDataException("Feature archive requires the feature-aware decoder");
            return claimed;
        }

        private static bool TryDecodeLifecycleArchiveWithFeature(string encoded,
            IList<byte[]> snapshots, out RecordedRound[] validatedRounds,
            out PcRoundLifecycleRecord validatedLifecycle, out PcDoubleFeatureNvV1 validatedFeature,
            out int validatedSchemaVersion)
        {
            PcGamePaymentRecordV1 payment;
            RecordedHistoryOverflowV1 overflow;
            bool claimed = TryDecodeLifecycleArchiveWithPayment(encoded, snapshots,
                out validatedRounds, out validatedLifecycle, out validatedFeature,
                out payment, out overflow, out validatedSchemaVersion);
            if (claimed && validatedSchemaVersion == 3)
                throw new PcRoundLifecycleDataException("Payment archive requires the payment-aware decoder");
            return claimed;
        }

        private static bool TryDecodeLifecycleArchiveWithPayment(string encoded,
            IList<byte[]> snapshots, out RecordedRound[] validatedRounds,
            out PcRoundLifecycleRecord validatedLifecycle, out PcDoubleFeatureNvV1 validatedFeature,
            out PcGamePaymentRecordV1 validatedPayment, out RecordedHistoryOverflowV1 validatedOverflow,
            out int validatedSchemaVersion)
        {
            validatedRounds = null;
            validatedLifecycle = null;
            validatedFeature = null;
            validatedPayment = null;
            validatedOverflow = null;
            validatedSchemaVersion = 0;
            LifecycleJsonNode root = new LifecycleJsonReader(encoded).Read();
            if (root.Kind != '{')
                throw new PcRoundLifecycleDataException("Round archive root is not an object");
            bool hasVersion = root.Members.ContainsKey("lifecycleSchemaVersion");
            bool hasLifecycle = root.Members.ContainsKey("lifecycle");
            bool hasFeature = root.Members.ContainsKey("featureNv");
            bool hasPayment = root.Members.ContainsKey("payment");
            bool hasOverflow = root.Members.ContainsKey("historyOverflow");
            if (!hasVersion && !hasLifecycle && !hasFeature && !hasPayment && !hasOverflow)
            {
                LifecycleJsonNode legacyRounds;
                if (!root.Members.TryGetValue("rounds", out legacyRounds) || legacyRounds.Kind != '[')
                    throw new PcRoundLifecycleDataException("Headerless archive is not an explicit legacy rounds object");
                return false;
            }
            LifecycleJsonNode version;
            LifecycleJsonNode rounds;
            LifecycleJsonNode lifecycle;
            LifecycleJsonNode feature = null;
            LifecycleJsonNode payment = null;
            LifecycleJsonNode overflow = null;
            if (!root.Members.TryGetValue("lifecycleSchemaVersion", out version) || version.Kind != '0' ||
                (version.Text(encoded) != "1" && version.Text(encoded) != "2" && version.Text(encoded) != "3") ||
                !root.Members.TryGetValue("rounds", out rounds) || rounds.Kind != '[' ||
                !root.Members.TryGetValue("lifecycle", out lifecycle) || lifecycle.Kind != '{')
                throw new PcRoundLifecycleDataException("Round lifecycle envelope is incomplete or unsupported");
            int schemaVersion = int.Parse(version.Text(encoded), CultureInfo.InvariantCulture);
            if ((schemaVersion == 1 && (root.Members.Count != 3 || hasFeature)) ||
                (schemaVersion == 2 && (root.Members.Count != 4 ||
                    !root.Members.TryGetValue("featureNv", out feature))) ||
                (schemaVersion == 3 && (root.Members.Count != 6 ||
                    !root.Members.TryGetValue("featureNv", out feature) ||
                    !root.Members.TryGetValue("payment", out payment) || payment.Kind != '{' ||
                    !root.Members.TryGetValue("historyOverflow", out overflow))))
                throw new PcRoundLifecycleDataException("Round lifecycle envelope fields differ from its version");
            ValidateLifecycleJsonShape(lifecycle, typeof(PcRoundLifecycleRecord), encoded);
            if (schemaVersion >= 2) ValidateLifecycleJsonShape(feature, typeof(PcDoubleFeatureNvV1), encoded);
            if (schemaVersion == 3)
            {
                ValidateLifecycleJsonShape(payment, typeof(PcGamePaymentRecordV1), encoded);
                ValidateLifecycleJsonShape(overflow, typeof(RecordedHistoryOverflowV1), encoded);
            }
            // V1 is only written from typed pages, including legacy history
            // carried forward with its default fields. Require that complete
            // wire shape so missing or mistyped money cannot become zero.
            ValidateLifecycleJsonShape(rounds, typeof(RecordedRound[]), encoded);
            RecordedRound[] archiveRounds;
            PcRoundLifecycleRecord archiveLifecycle;
            PcDoubleFeatureNvV1 archiveFeature = null;
            PcGamePaymentRecordV1 archivePayment = null;
            RecordedHistoryOverflowV1 archiveOverflow = null;
            try
            {
                if (schemaVersion == 1)
                {
                    RecordedRoundArchiveV1 archive = JsonUtility.FromJson<RecordedRoundArchiveV1>(encoded);
                    if (archive == null || archive.lifecycleSchemaVersion != 1)
                        throw new PcRoundLifecycleDataException("Round lifecycle V1 archive is missing");
                    archiveRounds = archive.rounds;
                    archiveLifecycle = archive.lifecycle;
                }
                else if (schemaVersion == 2)
                {
                    RecordedRoundArchiveV2 archive = JsonUtility.FromJson<RecordedRoundArchiveV2>(encoded);
                    if (archive == null || archive.lifecycleSchemaVersion != 2)
                        throw new PcRoundLifecycleDataException("Round lifecycle V2 archive is missing");
                    archiveRounds = archive.rounds;
                    archiveLifecycle = archive.lifecycle;
                    archiveFeature = feature.Kind == 'n' ? null : archive.featureNv;
                }
                else
                {
                    RecordedRoundArchiveV3 archive = JsonUtility.FromJson<RecordedRoundArchiveV3>(encoded);
                    if (archive == null || archive.lifecycleSchemaVersion != 3 || archive.payment == null)
                        throw new PcRoundLifecycleDataException("Round lifecycle V3 archive is missing");
                    archiveRounds = archive.rounds;
                    archiveLifecycle = archive.lifecycle;
                    archiveFeature = feature.Kind == 'n' ? null : archive.featureNv;
                    archivePayment = archive.payment;
                    archiveOverflow = overflow.Kind == 'n' ? null : archive.historyOverflow;
                }
            }
            catch (Exception error)
            {
                throw new PcRoundLifecycleDataException("Round lifecycle archive could not be decoded", error);
            }
            if (archiveLifecycle == null)
                throw new PcRoundLifecycleDataException("Round lifecycle archive is missing its receipt");
            RestoreLifecycleJsonNulls(lifecycle, archiveLifecycle);
            if (schemaVersion >= 2)
            {
                if (feature.Kind != 'n' && archiveFeature == null)
                    throw new PcRoundLifecycleDataException("Round feature receipt could not be decoded");
                RestoreLifecycleJsonNulls(feature, archiveFeature);
            }
            if (schemaVersion == 3)
            {
                RestoreLifecycleJsonNulls(payment, archivePayment);
                if (overflow.Kind != 'n' && archiveOverflow == null)
                    throw new PcRoundLifecycleDataException("History overflow could not be decoded");
                RestoreLifecycleJsonNulls(overflow, archiveOverflow);
                if (archiveRounds == null || archiveRounds.Length != rounds.Items.Count)
                    throw new PcRoundLifecycleDataException("Payment archive page shape differs");
                for (int i = 0; i < archiveRounds.Length; ++i)
                    RestoreLifecycleJsonNulls(rounds.Items[i], archiveRounds[i]);
            }
            ValidateLifecycleArchiveWithPayment(archiveRounds, snapshots, archiveLifecycle,
                archiveFeature, archivePayment, archiveOverflow, schemaVersion);
            validatedRounds = archiveRounds;
            validatedLifecycle = archiveLifecycle;
            validatedFeature = archiveFeature;
            validatedPayment = archivePayment;
            validatedOverflow = archiveOverflow;
            validatedSchemaVersion = schemaVersion;
            return true;
        }

        private static void ValidateLifecycleLatestIdentity(PcRoundLifecycleRecord lifecycle,
            RecordedRound latest)
        {
            if (lifecycle == null)
                throw new PcRoundLifecycleDataException("Round lifecycle receipt is missing");
            lifecycle.ValidateIdentity();
            if (latest == null || !latest.present || !latest.totalsRecorded ||
                !string.Equals(latest.roundArchiveId, lifecycle.roundArchiveId, StringComparison.Ordinal) ||
                !string.Equals(latest.spinJournalId, lifecycle.spinJournalId, StringComparison.Ordinal) ||
                !string.Equals(latest.profile, lifecycle.profile, StringComparison.Ordinal))
                throw new PcRoundLifecycleDataException("Round lifecycle does not match the latest paid page");
        }

        private static void ValidateLifecycleArchive(RecordedRound[] rounds,
            IList<byte[]> snapshots, PcRoundLifecycleRecord lifecycle,
            PcDoubleFeatureNvV1 feature, int schemaVersion)
        {
            if (schemaVersion != 1 && schemaVersion != 2)
                throw new PcRoundLifecycleDataException("Payment archive requires the payment-aware validator");
            ValidateLifecycleArchiveWithPayment(rounds, snapshots, lifecycle, feature, null, null, schemaVersion);
        }

        private static void ValidateLifecycleArchiveWithPayment(RecordedRound[] rounds,
            IList<byte[]> snapshots, PcRoundLifecycleRecord lifecycle,
            PcDoubleFeatureNvV1 feature, PcGamePaymentRecordV1 payment,
            RecordedHistoryOverflowV1 overflow, int schemaVersion)
        {
            if (schemaVersion < 1 || schemaVersion > 3 ||
                (schemaVersion == 3 ? payment == null : payment != null || overflow != null))
                throw new PcRoundLifecycleDataException("Round payment metadata differs from its envelope version");
            if (rounds == null || snapshots == null || rounds.Length < 1 ||
                rounds.Length > 50 || rounds.Length != snapshots.Count)
                throw new PcRoundLifecycleDataException("Round lifecycle archive and history counts disagree");
            ValidateLifecycleLatestIdentity(lifecycle, rounds[0]);
            // Preserve the raw feature/stage boundary before the old validator
            // normalizes completed feature amounts and awaiting flags.
            ValidateRoundLifecyclePayload(lifecycle, rounds[0]);
            // Explicit feature metadata is checked before legacy validators
            // can normalize old money or presentation fields.
            if (schemaVersion >= 2) ValidatePcDoubleFeatureNv(feature, rounds[0], lifecycle);
            if (schemaVersion == 3) ValidatePcGamePayment(payment, rounds[0], lifecycle, feature);
            var identities = new HashSet<string>(StringComparer.Ordinal);
            var spinIdentities = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < rounds.Length; ++i)
            {
                byte[] snapshot = snapshots[i];
                if (snapshot == null || snapshot.Length != HistogramCapacity)
                    throw new PcRoundLifecycleDataException("Round lifecycle history snapshot is invalid");
                foreach (byte value in snapshot)
                    if (value > 36 && value != 0xff)
                        throw new PcRoundLifecycleDataException("Round lifecycle history contains an invalid pocket");
                RecordedRound round = rounds[i];
                if (round == null)
                    throw new PcRoundLifecycleDataException("Round lifecycle archive contains a null page");
                if (schemaVersion == 3 && i > 0 && (round.awaitingFinalPayout ||
                    !IsEmptyRecordedDouble(round.doubleUp) && !round.doubleUp.complete))
                    throw new PcRoundLifecycleDataException("Payment predecessor history is not committed");
                if (!round.present)
                {
                    if (i == 0 || !IsDefaultLifecyclePage(round))
                        throw new PcRoundLifecycleDataException("Round lifecycle archive contains an invalid placeholder");
                    continue;
                }
                bool deferredFeature = i == 0 && schemaVersion >= 2 && feature != null;
                bool strictPageRequired = deferredFeature || schemaVersion == 3;
                string strictPage = strictPageRequired ? JsonUtility.ToJson(round) : null;
                if (!IsValidRecordedRoundWithFeature(round, snapshot, deferredFeature))
                    throw new PcRoundLifecycleDataException("Round lifecycle archive contains an invalid paid page");
                if (strictPageRequired && JsonUtility.ToJson(round) != strictPage)
                    throw new PcRoundLifecycleDataException("Versioned current result requires legacy normalization");
                if (!string.IsNullOrEmpty(round.roundArchiveId) &&
                    (!PcRoundLifecycleRecord.IsCanonicalId(round.roundArchiveId) ||
                     !identities.Add(round.roundArchiveId)))
                    throw new PcRoundLifecycleDataException("Round lifecycle archive has an invalid or duplicate round identity");
                if (!string.IsNullOrEmpty(round.spinJournalId) &&
                    (!PcRoundLifecycleRecord.IsCanonicalId(round.spinJournalId) ||
                     !spinIdentities.Add(round.spinJournalId)))
                    throw new PcRoundLifecycleDataException("Round lifecycle archive has an invalid or duplicate spin identity");
            }
            ValidateLifecycleLatestIdentity(lifecycle, rounds[0]);
            ValidateRoundLifecyclePayload(lifecycle, rounds[0]);
            if (schemaVersion == 3)
            {
                ValidatePcGamePayment(payment, rounds[0], lifecycle, feature);
                RequireLifecycle(rounds.Length == Math.Min(payment.priorHistoryCount + 1, 50) &&
                    (overflow != null) == (!payment.historyPublished && payment.priorHistoryCount == 50),
                    "Payment history count or required overflow differs from its predecessor");
                ValidatePcHistoryOverflow(overflow, rounds, snapshots, !payment.historyPublished);
            }
        }

        private static bool IsDefaultLifecyclePage(RecordedRound round)
        {
            // JsonUtility may materialize a null nested class as an empty
            // instance. Both representations describe the old empty page.
            foreach (FieldInfo field in typeof(RecordedRound).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                object value = field.GetValue(round);
                if (value == null) continue;
                if (field.FieldType == typeof(string))
                {
                    if (((string)value).Length != 0) return false;
                }
                else if (field.FieldType.IsArray)
                {
                    if (((Array)value).Length != 0) return false;
                }
                else if (field.FieldType == typeof(RecordedDouble))
                {
                    RecordedDouble feature = (RecordedDouble)value;
                    if (!IsEmptyRecordedDouble(feature) || feature.suitMode) return false;
                }
                else if (!field.FieldType.IsValueType ||
                    !value.Equals(Activator.CreateInstance(field.FieldType))) return false;
            }
            return true;
        }

        private string EncodeRoundArchiveForSave()
        {
            return EncodeRoundLifecycleArchiveWithPayment(recordedRoundPages.ToArray(),
                recordedSnapshots, latestRoundLifecycle, pcDoubleFeatureNv, pcGamePayment,
                pcHistoryOverflow, pcRoundArchiveSchemaVersion);
        }

        // The candidate is cloned by decoding, so legacy normalization cannot
        // mutate the live archive while validating an attempted save.
        private static string EncodeRoundLifecycleArchive(RecordedRound[] rounds,
            IList<byte[]> snapshots, PcRoundLifecycleRecord lifecycle)
        {
            return EncodeRoundLifecycleArchiveWithFeature(rounds, snapshots, lifecycle, null, 1);
        }

        private static string EncodeRoundLifecycleArchiveWithFeature(RecordedRound[] rounds,
            IList<byte[]> snapshots, PcRoundLifecycleRecord lifecycle, PcDoubleFeatureNvV1 feature, int schemaVersion)
        {
            if (schemaVersion != 1 && schemaVersion != 2)
                throw new PcRoundLifecycleDataException("Payment archive requires the payment-aware encoder");
            return EncodeRoundLifecycleArchiveWithPayment(rounds, snapshots, lifecycle,
                feature, null, null, schemaVersion);
        }

        private static string EncodePcHistoryOverflow(RecordedHistoryOverflowV1 overflow)
        {
            return overflow == null ? "null" : "{\"round\":" + JsonUtility.ToJson(overflow.round) +
                ",\"snapshotBase64\":" + EncodeLifecycleJsonValue(overflow.snapshotBase64) + "}";
        }

        private static string EncodeRoundLifecycleArchiveWithPayment(RecordedRound[] rounds,
            IList<byte[]> snapshots, PcRoundLifecycleRecord lifecycle, PcDoubleFeatureNvV1 feature,
            PcGamePaymentRecordV1 payment, RecordedHistoryOverflowV1 overflow, int schemaVersion)
        {
            string legacy = JsonUtility.ToJson(new RecordedRoundArchive { rounds = rounds });
            if (schemaVersion < 1 || schemaVersion > 3)
                throw new PcRoundLifecycleDataException("Round lifecycle save version is unsupported");
            if ((lifecycle == null || schemaVersion == 1) && feature != null)
                throw new PcRoundLifecycleDataException("Round feature receipt requires its V2 lifecycle envelope");
            if (schemaVersion == 3 ? lifecycle == null || payment == null : payment != null || overflow != null)
                throw new PcRoundLifecycleDataException("Round payment receipt requires its V3 lifecycle envelope");
            if (lifecycle == null) return legacy;
            string header = "{\"lifecycleSchemaVersion\":" + schemaVersion.ToString(CultureInfo.InvariantCulture) + ",";
            string encoded = header +
                legacy.Substring(1, legacy.Length - 2) + ",\"lifecycle\":" +
                EncodeLifecycleJsonValue(lifecycle) +
                (schemaVersion >= 2 ? ",\"featureNv\":" + EncodeLifecycleJsonValue(feature) : "") +
                (schemaVersion == 3 ? ",\"payment\":" + EncodeLifecycleJsonValue(payment) +
                    ",\"historyOverflow\":" + EncodePcHistoryOverflow(overflow) : "") + "}";
            RecordedRound[] verifiedRounds;
            PcRoundLifecycleRecord verifiedLifecycle;
            PcDoubleFeatureNvV1 verifiedFeature;
            PcGamePaymentRecordV1 verifiedPayment;
            RecordedHistoryOverflowV1 verifiedOverflow;
            int verifiedSchemaVersion;
            if (!TryDecodeLifecycleArchiveWithPayment(encoded, snapshots, out verifiedRounds,
                out verifiedLifecycle, out verifiedFeature, out verifiedPayment,
                out verifiedOverflow, out verifiedSchemaVersion) || verifiedSchemaVersion != schemaVersion)
                throw new PcRoundLifecycleDataException("Round lifecycle save lost its version header");
            string normalized = JsonUtility.ToJson(new RecordedRoundArchive { rounds = verifiedRounds });
            return header + normalized.Substring(1, normalized.Length - 2) +
                ",\"lifecycle\":" + EncodeLifecycleJsonValue(verifiedLifecycle) +
                (schemaVersion >= 2 ? ",\"featureNv\":" + EncodeLifecycleJsonValue(verifiedFeature) : "") +
                (schemaVersion == 3 ? ",\"payment\":" + EncodeLifecycleJsonValue(verifiedPayment) +
                    ",\"historyOverflow\":" + EncodePcHistoryOverflow(verifiedOverflow) : "") + "}";
        }

        private void StageRecordedRoundArchive()
        {
            if (temporaryHistory) return;
            PlayerPrefs.SetString(recordedHistoryKey + ".RoundDetails.v1", EncodeRoundArchiveForSave());
        }

        private void ValidateCanBeginSuccessorRound()
        {
            PcRoundLifecycleRecord predecessor = GetLatestLifecycleForRestore();
            if (predecessor != null && predecessor.roundStage != PcRoundStage.Ended)
                throw new PcRoundLifecycleDataException("A new round cannot precede the current round's End commit");
            if (predecessor != null) ValidateLifecycleBeginIdle(predecessor);
        }

        // Call before inserting newPage into recordedRoundPages. RecordPocket
        // may already have inserted the corresponding history snapshot.
        private void ValidateLifecycleReplacement(RecordedRound newPage, PendingSpinRecord settledSpin)
        {
            if (newPage == null || !newPage.present || !newPage.totalsRecorded ||
                !PcRoundLifecycleRecord.IsCanonicalId(newPage.roundArchiveId) ||
                string.IsNullOrEmpty(newPage.profile) ||
                settledSpin == null || !PcRoundLifecycleRecord.IsCanonicalId(settledSpin.id) ||
                newPage.spinJournalId != settledSpin.id || newPage.profile != settledSpin.profile)
                throw new PcRoundLifecycleDataException("A new lifecycle lacks its settled spin identity");
            PcRoundLifecycleRecord predecessor = GetLatestLifecycleForRestore();
            if (predecessor != null && predecessor.roundStage != PcRoundStage.Ended)
                throw new PcRoundLifecycleDataException("Successor cannot replace an unfinished lifecycle");
            RecordedRound previous = recordedRoundPages.Count == 0 ? null : recordedRoundPages[0];
            string previousId = previous == null ? "" : previous.roundArchiveId ?? "";
            if ((predecessor != null || !string.IsNullOrEmpty(settledSpin.baseLatestRoundId)) &&
                (settledSpin.baseLatestRoundId ?? "") != previousId)
                throw new PcRoundLifecycleDataException("Successor spin does not name its preceding archive");
            foreach (RecordedRound page in recordedRoundPages)
                if (page != null && (page.roundArchiveId == newPage.roundArchiveId ||
                    page.spinJournalId == settledSpin.id))
                    throw new PcRoundLifecycleDataException("Successor round reuses an archived identity");
            if (predecessor != null)
                ValidateIdleLifecycleSuccessor(predecessor, settledSpin.id);
        }

        private PcRoundLifecycleRecord CreateRoundOpenLifecycle(RecordedRound newPage,
            PendingSpinRecord settledSpin)
        {
            ValidateLifecycleReplacement(newPage, settledSpin);
            var lifecycle = new PcRoundLifecycleRecord {
                roundArchiveId = newPage.roundArchiveId, profile = newPage.profile,
                spinJournalId = newPage.spinJournalId, roundStage = PcRoundStage.RoundOpen
            };
            ValidateLifecycleLatestIdentity(lifecycle, newPage);
            ValidateRoundLifecyclePayload(lifecycle, newPage);
            return lifecycle;
        }

        // CLR type metadata cannot change within a domain. Cache only field descriptors;
        // every JSON value, member count and range is still checked on each invocation.
        private static readonly Dictionary<Type, FieldInfo[]> lifecycleJsonFieldMetadata = new Dictionary<Type, FieldInfo[]>();
        private static readonly object lifecycleJsonFieldMetadataLock = new object();
        private static FieldInfo[] LifecycleJsonFields(Type type)
        {
            lock (lifecycleJsonFieldMetadataLock)
            {
                FieldInfo[] fields;
                if (lifecycleJsonFieldMetadata.TryGetValue(type, out fields)) return fields;
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                Array.Sort(fields, (left, right) => string.CompareOrdinal(left.Name, right.Name));
                lifecycleJsonFieldMetadata.Add(type, fields);
                return fields;
            }
        }

        private static FieldInfo[] LifecycleEndJsonFields(int version)
        {
            string[] names;
            if (version == 1)
                names = new[] { "autoBefore", "demoAvailable", "finishAuto", "transactionActive", "version" };
            else if (version == 2)
                names = new[] { "autoBefore", "bonusGamesEnabled", "cancelAutoOperationId",
                    "cancelDisableOperationId", "demoAvailable", "finishAuto", "finishDisable",
                    "timerPresent", "transactionActive", "version" };
            else throw new PcRoundLifecycleDataException("Lifecycle End version is unsupported");
            var fields = new FieldInfo[names.Length];
            for (int i = 0; i < names.Length; ++i)
                fields[i] = typeof(PcRoundEndIntent).GetField(names[i], BindingFlags.Instance | BindingFlags.Public);
            return fields;
        }

        private static FieldInfo[] LifecycleValueJsonFields(object value)
        {
            PcRoundEndIntent intent = value as PcRoundEndIntent;
            return intent == null ? LifecycleJsonFields(value.GetType()) : LifecycleEndJsonFields(intent.version);
        }

        private static void ValidateLifecycleJsonShape(LifecycleJsonNode node, Type type, string encoded)
        {
            if (node.Kind == 'n' && !type.IsValueType) return;
            if (type.IsArray)
            {
                if (node.Kind != '[')
                    throw new PcRoundLifecycleDataException("Lifecycle array field has the wrong JSON type");
                foreach (LifecycleJsonNode item in node.Items)
                    ValidateLifecycleJsonShape(item, type.GetElementType(), encoded);
                return;
            }
            if (type == typeof(string))
            {
                if (node.Kind != '"') throw new PcRoundLifecycleDataException("Lifecycle text field has the wrong JSON type");
                return;
            }
            if (type == typeof(bool))
            {
                if (node.Kind != 't' && node.Kind != 'f')
                    throw new PcRoundLifecycleDataException("Lifecycle boolean field has the wrong JSON type");
                return;
            }
            if (type.IsEnum || type == typeof(int) || type == typeof(uint) || type == typeof(long))
            {
                long value;
                if (node.Kind != '0' || !long.TryParse(node.Text(encoded), NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out value) ||
                    ((type.IsEnum || type == typeof(int)) && (value < int.MinValue || value > int.MaxValue)) ||
                    (type == typeof(uint) && (value < 0 || value > uint.MaxValue)))
                    throw new PcRoundLifecycleDataException("Lifecycle integer field is missing, mistyped, or out of range");
                return;
            }
            if (type == typeof(float))
            {
                float value;
                if (node.Kind != '0' || !float.TryParse(node.Text(encoded), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out value) || float.IsNaN(value) || float.IsInfinity(value))
                    throw new PcRoundLifecycleDataException("Lifecycle floating field is mistyped or out of range");
                return;
            }
            if (node.Kind != '{' || type.IsValueType)
                throw new PcRoundLifecycleDataException("Lifecycle payload has the wrong JSON type");
            FieldInfo[] fields = LifecycleJsonFields(type);
            if (type == typeof(PcRoundEndIntent))
            {
                LifecycleJsonNode version;
                if (!node.Members.TryGetValue("version", out version) || version.Kind != '0' ||
                    (version.Text(encoded) != "1" && version.Text(encoded) != "2"))
                    throw new PcRoundLifecycleDataException("Lifecycle End version is missing or unsupported");
                fields = LifecycleEndJsonFields(version.Text(encoded) == "1" ? 1 : 2);
            }
            if (node.Members.Count != fields.Length)
                throw new PcRoundLifecycleDataException("Lifecycle payload fields are incomplete or unsupported");
            foreach (FieldInfo field in fields)
            {
                LifecycleJsonNode value;
                if (!node.Members.TryGetValue(field.Name, out value))
                    throw new PcRoundLifecycleDataException("Lifecycle payload is missing " + field.Name);
                ValidateLifecycleJsonShape(value, field.FieldType, encoded);
            }
        }

        private static void RestoreLifecycleJsonNulls(LifecycleJsonNode node, object value)
        {
            if (value == null || node.Kind != '{') return;
            foreach (FieldInfo field in LifecycleValueJsonFields(value))
            {
                LifecycleJsonNode member = node.Members[field.Name];
                if (member.Kind == 'n') field.SetValue(value, null);
                else if (member.Kind == '{') RestoreLifecycleJsonNulls(member, field.GetValue(value));
            }
        }

        private static PcRoundLifecycleRecord CloneRoundLifecycle(PcRoundLifecycleRecord value)
        {
            if (value == null) return null;
            string encoded = EncodeLifecycleJsonValue(value);
            LifecycleJsonNode node = new LifecycleJsonReader(encoded).Read();
            ValidateLifecycleJsonShape(node, typeof(PcRoundLifecycleRecord), encoded);
            PcRoundLifecycleRecord clone;
            try { clone = JsonUtility.FromJson<PcRoundLifecycleRecord>(encoded); }
            catch (Exception error)
            {
                throw new PcRoundLifecycleDataException("Round lifecycle copy could not be decoded", error);
            }
            if (clone == null) throw new PcRoundLifecycleDataException("Round lifecycle copy is missing");
            RestoreLifecycleJsonNulls(node, clone);
            clone.ValidateIdentity();
            return clone;
        }

        // JsonUtility does not consistently preserve null nested classes. The
        // small receipt encoder writes their actual null values explicitly.
        // Financial pages continue to use their unchanged JsonUtility shape.
        private static string EncodeLifecycleJsonValue(object value)
        {
            if (value == null) return "null";
            if (value is string) return QuoteLifecycleJson((string)value);
            if (value is bool) return (bool)value ? "true" : "false";
            Type type = value.GetType();
            if (type.IsEnum) return Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
            if (value is int || value is uint || value is long)
                return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
            var encoded = new StringBuilder("{");
            bool comma = false;
            foreach (FieldInfo field in LifecycleValueJsonFields(value))
            {
                if (comma) encoded.Append(',');
                comma = true;
                encoded.Append(QuoteLifecycleJson(field.Name)).Append(':')
                    .Append(EncodeLifecycleJsonValue(field.GetValue(value)));
            }
            return encoded.Append('}').ToString();
        }

        private static string QuoteLifecycleJson(string value)
        {
            var encoded = new StringBuilder("\"");
            foreach (char character in value)
            {
                if (character == '"' || character == '\\') encoded.Append('\\').Append(character);
                else if (character < 0x20) encoded.Append("\\u").Append(((int)character).ToString("x4"));
                else encoded.Append(character);
            }
            return encoded.Append('"').ToString();
        }

        private sealed class LifecycleJsonNode
        {
            internal char Kind;
            internal int Start;
            internal int End;
            internal Dictionary<string, LifecycleJsonNode> Members;
            internal List<LifecycleJsonNode> Items;
            internal string Text(string source) { return source.Substring(Start, End - Start); }
        }

        // A bounded JSON grammar reader. Names are decoded before duplicate
        // checks, so escaped spellings cannot hide a version or a second field.
        private sealed class LifecycleJsonReader
        {
            private readonly string source;
            private int position;
            internal LifecycleJsonReader(string source) { this.source = source; }

            internal LifecycleJsonNode Read()
            {
                if (string.IsNullOrEmpty(source)) Fail();
                LifecycleJsonNode result = Value(0);
                White();
                if (position != source.Length) Fail();
                return result;
            }

            private LifecycleJsonNode Value(int depth)
            {
                if (depth > 64) Fail();
                White();
                if (position == source.Length) Fail();
                char token = source[position];
                var node = new LifecycleJsonNode { Kind = token, Start = position };
                if (token == '{')
                {
                    ++position;
                    node.Members = new Dictionary<string, LifecycleJsonNode>(StringComparer.Ordinal);
                    White();
                    if (!Take('}'))
                    {
                        do
                        {
                            White();
                            string name = String(true);
                            White();
                            if (!Take(':') || node.Members.ContainsKey(name)) Fail();
                            node.Members.Add(name, Value(depth + 1));
                            White();
                            if (Take('}')) break;
                            if (!Take(',')) Fail();
                        } while (true);
                    }
                }
                else if (token == '[')
                {
                    ++position;
                    node.Items = new List<LifecycleJsonNode>();
                    White();
                    if (!Take(']'))
                    {
                        do
                        {
                            node.Items.Add(Value(depth + 1));
                            White();
                            if (Take(']')) break;
                            if (!Take(',')) Fail();
                        } while (true);
                    }
                }
                else if (token == '"') String(false);
                else if (token == 't') Literal("true");
                else if (token == 'f') Literal("false");
                else if (token == 'n') Literal("null");
                else
                {
                    node.Kind = '0';
                    Number();
                }
                node.End = position;
                return node;
            }

            private string String(bool decode)
            {
                if (!Take('"')) Fail();
                StringBuilder value = decode ? new StringBuilder() : null;
                while (position < source.Length)
                {
                    char character = source[position++];
                    if (character == '"') return decode ? value.ToString() : null;
                    if (character < 0x20) Fail();
                    if (character == '\\')
                    {
                        if (position == source.Length) Fail();
                        character = source[position++];
                        switch (character)
                        {
                            case '"': case '\\': case '/': break;
                            case 'b': character = '\b'; break;
                            case 'f': character = '\f'; break;
                            case 'n': character = '\n'; break;
                            case 'r': character = '\r'; break;
                            case 't': character = '\t'; break;
                            case 'u':
                                int code = 0;
                                for (int index = 0; index < 4; ++index)
                                {
                                    if (position == source.Length) Fail();
                                    char digit = source[position++];
                                    int hex = digit >= '0' && digit <= '9' ? digit - '0' :
                                        digit >= 'a' && digit <= 'f' ? digit - 'a' + 10 :
                                        digit >= 'A' && digit <= 'F' ? digit - 'A' + 10 : -1;
                                    if (hex < 0) Fail();
                                    code = code * 16 + hex;
                                }
                                character = (char)code;
                                break;
                            default: Fail(); break;
                        }
                    }
                    if (decode) value.Append(character);
                }
                Fail();
                return null;
            }

            private void Number()
            {
                Take('-');
                if (!Take('0'))
                {
                    if (position == source.Length || source[position] < '1' || source[position] > '9') Fail();
                    while (position < source.Length && Digit(source[position])) ++position;
                }
                if (Take('.')) Digits();
                if (Take('e') || Take('E'))
                {
                    if (!Take('+')) Take('-');
                    Digits();
                }
            }

            private void Digits()
            {
                int start = position;
                while (position < source.Length && Digit(source[position])) ++position;
                if (start == position) Fail();
            }

            private static bool Digit(char value) { return value >= '0' && value <= '9'; }
            private void Literal(string value)
            {
                if (source.Length - position < value.Length ||
                    string.CompareOrdinal(source, position, value, 0, value.Length) != 0) Fail();
                position += value.Length;
            }
            private void White()
            {
                while (position < source.Length && (source[position] == ' ' || source[position] == '\t' ||
                    source[position] == '\r' || source[position] == '\n')) ++position;
            }
            private bool Take(char value)
            {
                if (position == source.Length || source[position] != value) return false;
                ++position;
                return true;
            }
            private void Fail()
            {
                throw new PcRoundLifecycleDataException("Round archive JSON is invalid at character " + position);
            }
        }
    }
}
