using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Compatibility-only fixture import. These are closed C acceptance
        // receipts from before native Feature NV and RNG result receipts.
        // The import runs before any production reader or session is created.
        // No live session, feature phase, payout or compatibility mode is set.
        [Serializable]
        private sealed class AutoCompleteLegacyEvent
        {
            public string point, autoConfig;
            public long timestamp, credit, returned;
            public int roundStage, endVersion;
            public bool inDouble, deferred, timer, featureNv;
        }

        [Serializable]
        private sealed class AutoCompleteLegacyFile
        {
            public string path, sha256;
            public bool exists;
        }

        [Serializable]
        private sealed class AutoCompleteLegacyPreference
        {
            public string key, value;
            public bool exists;
        }

        [Serializable]
        private sealed class AutoCompleteLegacyDefaults
        {
            public AutoCompleteLegacyFile[] files;
            public AutoCompleteLegacyPreference[] preferences;
        }

        [Serializable]
        private sealed class AutoCompleteLegacyProof
        {
            public int schema = 1;
            public string testCase, phase, checkpoint, exitMode;
            public string historyKey, rngPath, autoPath, volumeKey;
            public string fixturePath, fixtureSha256, fixtureRawSha256, sourceHistoryKey;
            public string previousProofSha256, bootRawJson, bootRawSha256;
            public string rawJson, rawSha256, rngCandidate, autoCandidate, financialArchive;
            public RoundLifecycleSmokeRaw raw;
            public PcRoundLifecycleRecord lifecycle;
            public AutoCompleteLegacyDefaults defaultsBefore, defaultsAfter;
            public AutoCompleteLegacyEvent[] events;
            public long credit, wagered, returned, rounds, wins, denomination, remainder, stake;
            public int checks, ends, archiveCount, archiveSchema, pockets, suits;
            public int completeSpinCalls, recordPocketCalls, takeSaves, nativeCompletions;
            public bool importedFixture, emptyBeforeImport, sourceUnchanged, defaultsUnchanged;
            public bool inDouble, deferred, fault, featureNv, legacyUnknown;
        }

        private string acLegacyCase, acLegacyPhase, acLegacyFixturePath, acLegacyProofPath;
        private string acLegacyFixtureSha, acLegacySourceRawSha, acLegacyPreviousSha = "";
        private string acLegacyVolumeKey;
        private RoundLifecycleSmokeReceipt acLegacySource;
        private RecordedRound acLegacySourcePage;
        private AutoCompleteLegacyProof acLegacyPrevious;
        private RoundLifecycleSmokeRaw acLegacyBoot;
        private AutoCompleteLegacyDefaults acLegacyDefaultsBefore;
        private readonly List<AutoCompleteLegacyEvent> acLegacyEvents = new List<AutoCompleteLegacyEvent>();
        private bool acLegacyConfigured, acLegacyStarted, acLegacyExiting, acLegacyImported;
        private bool acLegacyEmptyBeforeImport, acLegacySawLegacyEnd;
        private int acLegacyChecks, acLegacyPockets, acLegacySuits;
        private int acLegacyCompleteSpins, acLegacyRecordPockets, acLegacyTakeSaves, acLegacyNativeCompletions;

        private void RequireAcLegacy(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("AutoComplete legacy " +
                acLegacyCase + "/" + acLegacyPhase + ": " + message);
            ++acLegacyChecks;
        }

        // Call inside LoadRecordedHistory after namespace selection and before
        // the first PlayerPrefs read, InitializePcAutoPlayState or PcOutcome.
        private void ConfigureAutoCompleteLegacySmoke()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-rouletteAutoCompleteLegacySmoke") < 0) return;
            RequireAcLegacy(!acLegacyConfigured, "configured twice");
            acLegacyConfigured = true;
            acLegacyCase = RoundSmokeArgument("-rouletteAutoCompleteLegacySmoke");
            acLegacyPhase = RoundSmokeArgument("-rouletteAutoCompleteLegacyPhase");
            RequireAcLegacy(new[] { "end_v1", "active_v1", "paid_v1" }.Contains(acLegacyCase), "unknown case");
            RequireAcLegacy(acLegacyPhase == "run" || acLegacyPhase == "reload", "unknown phase");
            RequireAcLegacy(args.Count(arg => arg.StartsWith("-roulette", StringComparison.Ordinal) &&
                arg.EndsWith("Smoke", StringComparison.Ordinal)) == 1,
                "compatibility import cannot share another smoke observer");
            acLegacyVolumeKey = RoundSmokeArgument("-rouletteVolumeTestKey");
            RequireAcLegacy(!temporaryHistory &&
                recordedHistoryKey.StartsWith(RecordedHistoryKey + ".Test.", StringComparison.Ordinal) &&
                acLegacyVolumeKey.StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcAutoPlayStatePath).StartsWith("roulette_autoplay_test_", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires isolated persistent history, volume, AutoData and RNG");
            RequireAcLegacy(session == null && pcAutoPlayStateStore == null && pcOutcome == null,
                "fixture import was called after a production reader was created");
            acLegacyFixturePath = RoundSmokeArgument("-rouletteAutoCompleteLegacyFixture");
            acLegacyProofPath = RoundSmokeArgument("-rouletteAutoCompleteLegacyProof");
            RequireAcLegacy(Path.IsPathRooted(acLegacyFixturePath) && File.Exists(acLegacyFixturePath) &&
                Path.IsPathRooted(acLegacyProofPath) && !File.Exists(acLegacyProofPath),
                "fixture must exist and proof must be a new absolute path");
            acLegacyFixturePath = Path.GetFullPath(acLegacyFixturePath);
            acLegacyProofPath = Path.GetFullPath(acLegacyProofPath);
            ReadAcLegacySource();
            RequireAcLegacy(recordedHistoryKey != acLegacySource.historyKey &&
                !string.Equals(Path.GetFullPath(PcAutoPlayStatePath), acLegacySource.autoPath, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Path.GetFullPath(PcRngStateStore.PathForCurrentProcess), acLegacySource.rngPath, StringComparison.OrdinalIgnoreCase),
                "fixture source namespace must remain untouched");
            acLegacyDefaultsBefore = CaptureAcLegacyDefaults();
            string previousPath = RoundSmokeArgument("-rouletteAutoCompleteLegacyPreviousProof", false);
            RequireAcLegacy((acLegacyPhase == "run") == string.IsNullOrEmpty(previousPath), "wrong predecessor argument");
            if (acLegacyPhase == "run") ImportAcLegacySource();
            acLegacyBoot = ReadRoundSmokeRaw();
            if (acLegacyPhase == "reload")
            {
                RequireAcLegacy(Path.IsPathRooted(previousPath), "previous proof must be absolute");
                byte[] bytes = File.ReadAllBytes(previousPath);
                acLegacyPreviousSha = RngSmokeBytesSha(bytes);
                acLegacyPrevious = JsonUtility.FromJson<AutoCompleteLegacyProof>(Encoding.UTF8.GetString(bytes));
                RequireAcLegacy(acLegacyPrevious != null && acLegacyPrevious.schema == 1 &&
                    acLegacyPrevious.testCase == acLegacyCase && acLegacyPrevious.phase == "run" &&
                    acLegacyPrevious.exitMode == "natural" && acLegacyPrevious.importedFixture &&
                    acLegacyPrevious.historyKey == recordedHistoryKey && acLegacyPrevious.volumeKey == acLegacyVolumeKey &&
                    acLegacyPrevious.rngPath == PcRngStateStore.PathForCurrentProcess &&
                    acLegacyPrevious.autoPath == PcAutoPlayStatePath &&
                    acLegacyPrevious.fixtureSha256 == acLegacyFixtureSha &&
                    acLegacyPrevious.fixtureRawSha256 == acLegacySourceRawSha &&
                    acLegacyPrevious.raw != null && acLegacyPrevious.rawJson == JsonUtility.ToJson(acLegacyPrevious.raw) &&
                    acLegacyPrevious.rawSha256 == RoundSmokeSha(acLegacyPrevious.rawJson) &&
                    JsonUtility.ToJson(acLegacyBoot) == acLegacyPrevious.rawJson &&
                    AcLegacyCandidate(PcRngStateStore.PathForCurrentProcess) == acLegacyPrevious.rngCandidate &&
                    AcLegacyCandidate(PcAutoPlayStatePath) == acLegacyPrevious.autoCandidate,
                    "cold reload bytes or closed predecessor identity differ");
            }
            else RequireAcLegacy(JsonUtility.ToJson(acLegacyBoot) == JsonUtility.ToJson(acLegacySource.raw),
                "import changed a fixture byte before production load");
            pcLifecycleCheckpoint = AcLegacyCheckpoint;
            pcAutoCompleteCheckpoint = AcLegacyCheckpoint;
            Debug.Log("ROULETTE_AUTO_COMPLETE_LEGACY_BOOT: case=" + acLegacyCase + " phase=" + acLegacyPhase +
                " imported=" + acLegacyImported.ToString().ToLowerInvariant() +
                " fixture=" + acLegacyFixtureSha + " raw=" + RoundSmokeSha(JsonUtility.ToJson(acLegacyBoot)) +
                " previous=" + (acLegacyPreviousSha == "" ? "none" : acLegacyPreviousSha));
        }

        private void ReadAcLegacySource()
        {
            string expectedSha, expectedRaw, sourceCase, sourcePhase;
            if (acLegacyCase == "end_v1")
            {
                expectedSha = "A7C2F96450FD35427C8274ABD3A5552ACB3A1B2C104F3DCCF4A44F6E78DDFBDE";
                expectedRaw = "7686CE4D029D1A6D254396E9FEAAFDE6A64E1FB933400E45B702475656C0C6E8";
                sourceCase = "end_partial"; sourcePhase = "save";
            }
            else if (acLegacyCase == "active_v1")
            {
                expectedSha = "1C80D6D5ED2185A386A092A52BE03C2E0A0958B7680615B0795E83BBD08BA494";
                expectedRaw = "CB276F82E21D94E645CFC23898D530996C289F38AD8DEE0F5F82D61FA9F6C466";
                sourceCase = "double_take"; sourcePhase = "feature";
            }
            else
            {
                expectedSha = "4D223B2E9BC9B8FD5BEA0CEC83CDD94CB3729F57A40F1EEA26731AB4AAEEA002";
                expectedRaw = "9FB4AFEECFEF98115CC0AFAFE8585CC7AF108F219164E91B3C5EAA822BA87CD1";
                sourceCase = "double_loss"; sourcePhase = "save";
            }
            byte[] bytes = File.ReadAllBytes(acLegacyFixturePath);
            acLegacyFixtureSha = RngSmokeBytesSha(bytes);
            RequireAcLegacy(acLegacyFixtureSha == expectedSha, "fixture is not the pinned frozen C receipt");
            acLegacySource = JsonUtility.FromJson<RoundLifecycleSmokeReceipt>(Encoding.UTF8.GetString(bytes));
            RequireAcLegacy(acLegacySource != null && acLegacySource.schema == 1 && acLegacySource.raw != null &&
                acLegacySource.testCase == sourceCase && acLegacySource.phase == sourcePhase &&
                acLegacySource.exitMode == "checkpoint" && !acLegacySource.fault && acLegacySource.hasSession &&
                acLegacySource.archiveCount == 1 && acLegacySource.credit == 999 &&
                acLegacySource.wagered == 1 && acLegacySource.rounds == 1 &&
                acLegacySource.rawSha256 == expectedRaw &&
                RoundSmokeSha(JsonUtility.ToJson(acLegacySource.raw)) == expectedRaw,
                "frozen receipt identity or original raw digest differs");
            acLegacySourceRawSha = expectedRaw;
            RecordedRound[] pages;
            PcRoundLifecycleRecord lifecycle;
            var snapshots = acLegacySource.raw.pages.Split(';').Where(value => value.Length != 0)
                .Select(Convert.FromBase64String).ToList();
            RequireAcLegacy(TryDecodeLifecycleArchive(acLegacySource.raw.envelope, snapshots, out pages, out lifecycle) &&
                pages != null && pages.Length == 1 && lifecycle != null,
                "frozen C data is not a strict schema-one lifecycle archive");
            acLegacySourcePage = pages[0];
            RequireAcLegacy(lifecycle.roundArchiveId == acLegacySource.lifecycle.roundArchiveId &&
                lifecycle.spinJournalId == acLegacySource.lifecycle.spinJournalId &&
                acLegacySource.raw.pending == "" && acLegacySource.raw.staged == "" &&
                acLegacySource.raw.creditEvent == "" && acLegacySource.raw.cleared == "",
                "frozen fixture has an unexpected pending business operation");
            byte[] rng = Convert.FromBase64String(acLegacySource.raw.rng);
            RequireAcLegacy(rng.Length > 8 && BitConverter.ToUInt32(rng, 0) == 0x31474e52u &&
                BitConverter.ToUInt32(rng, 4) == 2, "fixture RNG must be original receipt-free v2 bytes");
            if (acLegacyCase == "end_v1")
                RequireAcLegacy(lifecycle.roundStage == PcRoundStage.EndPrepared && lifecycle.endIntent != null &&
                    lifecycle.endIntent.version == 1 && lifecycle.endIntent.finishAuto &&
                    !lifecycle.endIntent.transactionActive && lifecycle.endIntent.autoBefore.secondsStart == 29 &&
                    lifecycle.endIntent.autoBefore.disableWhenFinished && !acLegacySourcePage.hasDoubleUp,
                    "fixture lacks the original prepared End v1 decision");
            else
                RequireAcLegacy(lifecycle.roundStage == PcRoundStage.RoundOpen && lifecycle.endIntent == null &&
                    acLegacySourcePage.hasDoubleUp && acLegacySourcePage.doubleUp != null &&
                    acLegacySourcePage.doubleUp.complete == (acLegacyCase == "paid_v1") &&
                    acLegacySourcePage.doubleUp.steps.Length == (acLegacyCase == "active_v1" ? 1 : 2),
                    "fixture lacks the selected original Double state");
        }

        private string[] AcLegacyPreferenceKeys()
        {
            return new[] { recordedHistoryKey + ".RoundDetails.v1", recordedHistoryKey,
                recordedHistoryKey + ".Pages", PendingSpinKey, StagedBetKey, SettledTableUndoKey,
                PcCreditKey, GameResultsClearedKey, NeighborSelectionKey };
        }

        private void ImportAcLegacySource()
        {
            foreach (string key in AcLegacyPreferenceKeys())
                RequireAcLegacy(!PlayerPrefs.HasKey(key), "import target preference already exists: " + key);
            RequireAcLegacy(!PlayerPrefs.HasKey(acLegacyVolumeKey) &&
                !File.Exists(PcRngStateStore.PathForCurrentProcess) && !File.Exists(PcRngStateStore.PathForCurrentProcess + ".new") &&
                !File.Exists(PcAutoPlayStatePath) && !File.Exists(PcAutoPlayStatePath + ".new"),
                "import target namespace is not empty");
            acLegacyEmptyBeforeImport = true;
            RoundLifecycleSmokeRaw raw = acLegacySource.raw;
            byte[] rng = Convert.FromBase64String(raw.rng), auto = Convert.FromBase64String(raw.auto);
            int neighbor;
            RequireAcLegacy(int.TryParse(raw.neighbor, NumberStyles.Integer, CultureInfo.InvariantCulture, out neighbor) &&
                neighbor >= -2 && neighbor <= 36, "fixture neighbor field is invalid");
            WriteAcLegacyNewBytes(PcRngStateStore.PathForCurrentProcess, rng);
            WriteAcLegacyNewBytes(PcAutoPlayStatePath, auto);
            PcRngStoredState stored = PcRngStateStore.LoadStrict(PcRngStateStore.PathForCurrentProcess);
            OriginalAutoPlayData detached;
            RequireAcLegacy(stored != null && stored.Version == 2 && stored.Receipt == null &&
                new PcAutoPlayStateStore(PcAutoPlayStatePath).TryLoad(out detached) &&
                AutoNvConfig(detached) == acLegacySource.autoConfig,
                "production readers rejected imported original binary records");
            string[] keys = AcLegacyPreferenceKeys();
            string[] values = { raw.envelope, raw.pockets, raw.pages, raw.pending, raw.staged,
                raw.settledUndo, raw.creditEvent, raw.cleared };
            for (int i = 0; i < values.Length; ++i)
                if (values[i].Length != 0) PlayerPrefs.SetString(keys[i], values[i]);
            if (neighbor != -2) PlayerPrefs.SetInt(NeighborSelectionKey, neighbor);
            PlayerPrefs.Save();
            acLegacyImported = true;
            RequireAcLegacy(JsonUtility.ToJson(CaptureAcLegacyDefaults()) == JsonUtility.ToJson(acLegacyDefaultsBefore),
                "fixture import changed default player data");
            Debug.Log("ROULETTE_AUTO_COMPLETE_LEGACY_IMPORT: case=" + acLegacyCase +
                " fixture=" + acLegacyFixtureSha + " sourceRaw=" + acLegacySourceRawSha +
                " originalBytes=true emptyNamespace=true compatibilityFixture=true history=" + recordedHistoryKey);
        }

        private static void WriteAcLegacyNewBytes(string path, byte[] bytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
        }

        private static string AcLegacyCandidate(string path)
        { return File.Exists(path + ".new") ? Convert.ToBase64String(File.ReadAllBytes(path + ".new")) : "missing"; }

        private AutoCompleteLegacyDefaults CaptureAcLegacyDefaults()
        {
            string[] files = { "roulette_autoplay_v1.bin", "roulette_autoplay_v1.bin.new",
                "roulette_platform_rng_v1.bin", "roulette_platform_rng_v1.bin.new" };
            var preferences = new List<AutoCompleteLegacyPreference>();
            foreach (string suffix in new[] { "", ".Pages", ".RoundDetails.v1", ".PendingSpin.v1",
                ".StagedBets.v1", ".SettledTableUndo.v1", ".PcCredit.v1", ".CurrentResultPresentation.v1" })
            {
                string key = RecordedHistoryKey + suffix;
                preferences.Add(new AutoCompleteLegacyPreference { key = key,
                    exists = PlayerPrefs.HasKey(key), value = PlayerPrefs.GetString(key, "") });
            }
            foreach (string key in new[] { "RouletteDeluxeSZ.SelectedNeighbor.v1", DefaultPcPlayerVolumeKey, DefaultPcCashMeterKey })
                preferences.Add(new AutoCompleteLegacyPreference { key = key, exists = PlayerPrefs.HasKey(key),
                    value = PlayerPrefs.GetInt(key, int.MinValue).ToString(CultureInfo.InvariantCulture) });
            return new AutoCompleteLegacyDefaults {
                preferences = preferences.ToArray(),
                files = files.Select(name => {
                    string path = Path.Combine(Application.persistentDataPath, name);
                    bool exists = File.Exists(path);
                    return new AutoCompleteLegacyFile { path = path, exists = exists,
                        sha256 = exists ? RngSmokeBytesSha(File.ReadAllBytes(path)) : "" };
                }).ToArray()
            };
        }

        private void AcLegacyCheckpoint(string point)
        {
            if (!acLegacyConfigured || acLegacyExiting) return;
            var local = session as RouletteSession;
            acLegacyEvents.Add(new AutoCompleteLegacyEvent {
                point = point, timestamp = System.Diagnostics.Stopwatch.GetTimestamp(),
                autoConfig = configuredAutoPlay == null ? "" : AutoNvConfig(configuredAutoPlay),
                credit = session == null ? -1 : session.Credit, returned = session == null ? -1 : session.Returned,
                inDouble = session != null && session.InDoubleUp, deferred = local != null && local.DoubleSettlementDeferred,
                roundStage = latestRoundLifecycle == null ? -1 : (int)latestRoundLifecycle.roundStage,
                endVersion = latestRoundLifecycle == null || latestRoundLifecycle.endIntent == null ? 0 : latestRoundLifecycle.endIntent.version,
                timer = PcAutoCompleteTimerExists, featureNv = pcDoubleFeatureNv != null
            });
            if (point == "CompleteSpin") ++acLegacyCompleteSpins;
            if (point == "RecordPocket") ++acLegacyRecordPockets;
            if (point == "DoubleFeatureCompletionApplied") ++acLegacyNativeCompletions;
            if (point == "DoubleTakeSaved")
            {
                ++acLegacyTakeSaves;
                RequireAcLegacy(acLegacyCase == "active_v1" && acLegacyPhase == "run" &&
                    session.Credit == 1071 && session.Returned == 72 && !session.InDoubleUp &&
                    local != null && !local.DoubleSettlementDeferred && pcDoubleFeatureNv == null,
                    "legacy manual TAKE did not apply its one original-mode payment");
            }
            if (acLegacyCase == "end_v1" && acLegacyPhase == "run" && point == "EndedSaved")
            {
                PcLifecycleAutoSnapshot expected = acLegacySource.lifecycle.endIntent.autoBefore.Clone();
                expected.autoPlay = 0; expected.turbo = false; expected.offWhenFinished = false;
                RequireAcLegacy(latestRoundLifecycle.endIntent.version == 1 &&
                    PcLifecycleAutoSnapshot.Capture(configuredAutoPlay).Same(expected) && !PcAutoCompleteTimerExists &&
                    !acLegacyEvents.Any(item => item.point == "EndDisableCancelled" || item.point == "EndDisableFalse"),
                    "old End v1 acquired new Disable/timer cancellation semantics");
                acLegacySawLegacyEnd = true;
            }
        }

        // Call after normal Start restored native Feature/AutoComplete state and
        // made pcLifecycleRuntimeReady visible; also call in its fault branch.
        private void StartAutoCompleteLegacySmoke()
        {
            if (!acLegacyConfigured) return;
            RequireAcLegacy(!acLegacyStarted, "observer started twice");
            acLegacyStarted = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            if (pcOutcome != null) pcOutcome.ConfigureReceiptDiagnostics((point, receipt) => {
                if (point != "saved") return;
                if (receipt.context.kind == 1) ++acLegacyPockets; else ++acLegacySuits;
            });
            new GameObject("AutoComplete legacy compatibility observer").AddComponent<RouletteRoundLifecycleSmokeHost>()
                .StartCoroutine(AutoCompleteLegacySmoke());
        }

        private IEnumerator AcLegacyWait(Func<bool> condition, string label, float seconds = 90f)
        {
            RequireAcLegacy(sourceInputModule != null, "real source input module is unavailable");
            sourceInputModule.enabled = true;
            float until = Time.realtimeSinceStartup + seconds;
            while (!condition() && !pcAutoPlayStorageFaulted && Time.realtimeSinceStartup < until) yield return null;
            sourceInputModule.enabled = false;
            RequireAcLegacy(condition() && !pcAutoPlayStorageFaulted, "wait failed: " + label);
        }

        private void AcLegacyCheckFinancial(long credit, long returned)
        {
            var local = session as RouletteSession;
            RequireAcLegacy(local != null && !local.DoubleSettlementDeferred && !pcAutoPlayStorageFaulted &&
                session.Credit == credit && session.Returned == returned &&
                session.Wagered == acLegacySource.wagered && session.Rounds == acLegacySource.rounds &&
                session.Wins == acLegacySource.wins && session.Denomination == acLegacySource.denomination &&
                local.MoneyRemainderCents == acLegacySource.remainder &&
                recordedRoundPages.Count == acLegacySource.archiveCount && pcRoundArchiveSchemaVersion == 1 &&
                pcDoubleFeatureNv == null && !PcNativeDoubleFeatureTracked &&
                latestRoundLifecycle.roundArchiveId == acLegacySource.lifecycle.roundArchiveId &&
                latestRoundLifecycle.spinJournalId == acLegacySource.lifecycle.spinJournalId &&
                DemoCleanupRngBytes() == acLegacySource.raw.rng &&
                PlayerPrefs.GetString(recordedHistoryKey, "") == acLegacySource.raw.pockets &&
                PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "") == acLegacySource.raw.pages &&
                acLegacyPockets == 0 && acLegacySuits == 0 && acLegacyCompleteSpins == 0 &&
                acLegacyRecordPockets == 0 && acLegacyNativeCompletions == 0,
                "legacy restore changed money, mode, identity, archive count or RNG");
        }

        private IEnumerator AutoCompleteLegacySmoke()
        {
            RequireAcLegacy(!pcAutoPlayStorageFaulted && session != null && HasTrustedLatestLifecycle &&
                sourceRoundEndCount == 0, "production startup did not restore the fixture without End");
            long finalCredit = acLegacyCase == "active_v1" ? 1071 : 999;
            long finalReturned = acLegacyCase == "active_v1" ? 72 : 0;
            if (acLegacyPhase == "reload")
            {
                AcLegacyCheckFinancial(finalCredit, finalReturned);
                RequireAcLegacy(!session.InDoubleUp && !session.DoubleAvailable &&
                    latestRoundLifecycle.roundStage == PcRoundStage.Ended,
                    "closed legacy completion was not a terminal cold restore");
            }
            else
            {
                AcLegacyCheckFinancial(acLegacySource.credit, acLegacySource.returned);
                if (acLegacyCase == "active_v1")
                {
                    RequireAcLegacy(session.InDoubleUp && session.DoubleStake == 72 && session.DoubleRounds == 1 &&
                        pcDoubleFeatureLegacyUnknown && !PcAutoCompleteTimerExists &&
                        latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen,
                        "legacy active Double was not restored as an unresolved original-mode feature");
                    string before = JsonUtility.ToJson(ReadRoundSmokeRaw());
                    yield return StartGameLiveFrames(4);
                    RequireAcLegacy(JsonUtility.ToJson(ReadRoundSmokeRaw()) == before && sourceRoundEndCount == 0,
                        "legacy active feature advanced without manual input");
                    yield return AcLegacyWait(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                        "legacy manual TAKE gate");
                    RefreshPortraitUi();
                    RequireAcLegacy(uiDoubleTakeWinButton != null && uiDoubleTakeWinButton.interactable,
                        "legacy TAKE UI is unavailable");
                    QueueStartGameButton(uiDoubleTakeWinButton, 430, true);
                    yield return AcLegacyWait(() => doubleCollectPending, "legacy manual TAKE accepted");
                    AcLegacyCheckFinancial(999, 36);
                }
                else if (acLegacyCase == "end_v1")
                    RequireAcLegacy(latestRoundLifecycle.roundStage == PcRoundStage.EndPrepared &&
                        latestRoundLifecycle.endIntent.version == 1 && pcPlayerTransactionActive,
                        "old End intent or changed cold transaction environment was not retained");
                else
                    RequireAcLegacy(!session.InDoubleUp && !session.DoubleAvailable &&
                        recordedRoundPages[0].doubleUp.complete && !recordedRoundPages[0].doubleUp.tookWin,
                        "already-paid old terminal feature became an active Double");
            }
            yield return AcLegacyWait(() => sourceRoundPhase == SourceRoundPhase.None &&
                sourceGameplayState == SourceGameplayState.Idle && !configuredAutoPlayCheckPending &&
                latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete, "legacy completion and real Idle");
            AcLegacyCheckFinancial(finalCredit, finalReturned);
            RequireAcLegacy(sourceRoundEndCount == (acLegacyPhase == "run" ? 1 : 0) &&
                acLegacyTakeSaves == (acLegacyCase == "active_v1" && acLegacyPhase == "run" ? 1 : 0) &&
                !session.InDoubleUp && !session.DoubleAvailable && !autoplay,
                "legacy completion repeated End/TAKE or remained active");
            if (acLegacyCase == "end_v1" && acLegacyPhase == "run")
                RequireAcLegacy(acLegacySawLegacyEnd, "actual old End v1 setter checkpoint was not observed");
            RecordedRound expected = JsonUtility.FromJson<RecordedRound>(JsonUtility.ToJson(acLegacySourcePage));
            expected.awaitingFinalPayout = false;
            if (acLegacyCase == "active_v1")
            {
                expected.returned = 72; expected.creditAfter = 1071; expected.totalReturned = 72;
                expected.doubleUp.complete = true; expected.doubleUp.tookWin = true; expected.doubleUp.finalWin = 72;
            }
            RequireAcLegacy(JsonUtility.ToJson(recordedRoundPages[0]) == JsonUtility.ToJson(expected),
                "legacy financial page changed outside its original TAKE/terminal normalization");
            yield return StartGameLiveFrames(4);
            AcLegacyCheckFinancial(finalCredit, finalReturned);
            RequireAcLegacy(sourceRoundEndCount == (acLegacyPhase == "run" ? 1 : 0), "later frames repeated End");
            AcLegacyExit(acLegacyPhase == "reload" ? "StableReload" :
                acLegacyCase == "end_v1" ? "LegacyEndCompleted" :
                acLegacyCase == "active_v1" ? "LegacyTakeCompleted" : "LegacyPaidRestored");
        }

        private void AcLegacyExit(string point)
        {
            RequireAcLegacy(!acLegacyExiting, "duplicate exit");
            acLegacyExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            AutoCompleteLegacyDefaults defaultsAfter = CaptureAcLegacyDefaults();
            RequireAcLegacy(JsonUtility.ToJson(acLegacyDefaultsBefore) == JsonUtility.ToJson(defaultsAfter) &&
                RngSmokeBytesSha(File.ReadAllBytes(acLegacyFixturePath)) == acLegacyFixtureSha,
                "default data or frozen source receipt changed");
            var local = (RouletteSession)session;
            var proof = new AutoCompleteLegacyProof {
                testCase = acLegacyCase, phase = acLegacyPhase, checkpoint = point, exitMode = "natural",
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess,
                autoPath = PcAutoPlayStatePath, volumeKey = acLegacyVolumeKey,
                fixturePath = acLegacyFixturePath, fixtureSha256 = acLegacyFixtureSha,
                fixtureRawSha256 = acLegacySourceRawSha, sourceHistoryKey = acLegacySource.historyKey,
                previousProofSha256 = acLegacyPreviousSha, bootRawJson = JsonUtility.ToJson(acLegacyBoot),
                raw = ReadRoundSmokeRaw(), rngCandidate = AcLegacyCandidate(PcRngStateStore.PathForCurrentProcess),
                autoCandidate = AcLegacyCandidate(PcAutoPlayStatePath), lifecycle = latestRoundLifecycle,
                financialArchive = JsonUtility.ToJson(new RecordedRoundArchive { rounds = recordedRoundPages.ToArray() }),
                defaultsBefore = acLegacyDefaultsBefore, defaultsAfter = defaultsAfter, events = acLegacyEvents.ToArray(),
                credit = session.Credit, wagered = session.Wagered, returned = session.Returned,
                rounds = session.Rounds, wins = session.Wins, denomination = session.Denomination,
                remainder = local.MoneyRemainderCents, stake = session.DoubleStake,
                checks = acLegacyChecks, ends = sourceRoundEndCount, archiveCount = recordedRoundPages.Count,
                archiveSchema = pcRoundArchiveSchemaVersion, pockets = acLegacyPockets, suits = acLegacySuits,
                completeSpinCalls = acLegacyCompleteSpins, recordPocketCalls = acLegacyRecordPockets,
                takeSaves = acLegacyTakeSaves, nativeCompletions = acLegacyNativeCompletions,
                importedFixture = acLegacyImported, emptyBeforeImport = acLegacyEmptyBeforeImport,
                sourceUnchanged = true, defaultsUnchanged = true, inDouble = session.InDoubleUp,
                deferred = local.DoubleSettlementDeferred, fault = pcAutoPlayStorageFaulted,
                featureNv = pcDoubleFeatureNv != null, legacyUnknown = pcDoubleFeatureLegacyUnknown
            };
            proof.bootRawSha256 = RoundSmokeSha(proof.bootRawJson);
            proof.rawJson = JsonUtility.ToJson(proof.raw);
            proof.rawSha256 = RoundSmokeSha(proof.rawJson);
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            WriteAcLegacyNewBytes(acLegacyProofPath, new UTF8Encoding(false).GetBytes(encoded));
            Debug.Log("ROULETTE_AUTO_COMPLETE_LEGACY_PASSED: case=" + acLegacyCase + " phase=" + acLegacyPhase +
                " point=" + point + " mode=natural checks=" + acLegacyChecks + " credit=" + proof.credit +
                " returned=" + proof.returned + " rounds=" + proof.rounds + " ends=" + proof.ends +
                " takes=" + proof.takeSaves + " pockets=" + proof.pockets + " suits=" + proof.suits +
                " imported=" + proof.importedFixture.ToString().ToLowerInvariant() + " fault=false proof=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_AUTO_COMPLETE_LEGACY_EXIT: api=Application.Quit code=0 mode=natural");
            Application.Quit(0);
        }
    }
}
