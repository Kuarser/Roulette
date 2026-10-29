using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class RngReceiptSmokeProof
        {
            public int schema = 2;
            public string testCase, phase, checkpoint, exitMode, historyKey, rngPath, autoPath;
            public string previousProofSha256, rawSha256, rawJson, rngCandidateBase64, rngCandidateSha256, oracleSha256;
            public RoundLifecycleSmokeRaw raw;
            public PcRngDrawReceipt target;
            public long credit, wagered, returned, rounds, wins, denomination, remainder, totalBet;
            public int archiveCount, sourceState, sourcePhase, ends, pocketAttempts, suitAttempts, pocketCommits, suitCommits;
            public int completeSpinCalls, recordPocketCalls, checks, doubleRounds, selectedDemo, targetDrawOrdinal;
            public bool fault, hasSession, spinPending, inDouble, fixtureMutation, targetCommitted;
            public bool active, tableEnabled, configuredCheckPending;
            public string faultOperation;
            public long doubleStake, gameStartTimestamp;
            public int archiveSchema, visibleHistory;
            public string paymentJson, featureJson;
            public bool roundPayoutPending;
            public long displayedCredit, displayedPaid;
            public string[] checkpoints;
        }

        [Serializable]
        private sealed class RngReceiptSmokeOracle
        {
            public string oracle;
            public uint seed;
            public int rawPocket, rawSuit;
            public uint[] futureAfterSuit;
        }

        private static readonly Dictionary<string, string[]> RngReceiptSmokePhases =
            new Dictionary<string, string[]>(StringComparer.Ordinal) {
                { "first", new[] { "save", "rebuild", "pending", "settled", "complete", "reload" } },
                { "manual", new[] { "save", "rebuild", "pending", "settled", "complete", "reload" } },
                { "idle", new[] { "save", "rebuild", "pending", "settled", "complete", "reload" } },
                { "demo", new[] { "save", "rebuild", "pending", "settled", "complete", "reload" } },
                { "double_win", new[] { "save", "replay", "take", "complete", "reload" } },
                { "double_loss", new[] { "save", "replay", "complete", "reload" } },
                { "double_cap", new[] { "save", "replay", "complete", "reload" } },
                { "fault_scope", new[] { "save", "load" } },
                { "fault_profile", new[] { "save", "load" } },
                { "fault_id", new[] { "save", "load" } },
                { "fault_baseline", new[] { "save", "load" } },
                { "fault_hash", new[] { "save", "load" } },
                { "fault_version", new[] { "save", "load" } },
                { "fault_replace", new[] { "run", "recover" } },
                { "fault_pending", new[] { "save", "fail", "recover" } }
            };

        private string rngSmokeCase, rngSmokePhase, rngSmokeOutput, rngSmokePreviousHash, rngSmokeCut;
        private string rngSmokeOracleHash = "", rngSmokeFaultCandidate;
        private RngReceiptSmokeProof rngSmokePrevious;
        private RngReceiptSmokeOracle rngSmokeOracle;
        private RoundLifecycleSmokeRaw rngSmokeBoot, rngSmokeFaultBefore;
        private PcRngDrawReceipt rngSmokeTarget;
        private readonly List<string> rngSmokeEvents = new List<string>();
        private int rngSmokeChecks, rngSmokePocketAttempts, rngSmokeSuitAttempts, rngSmokePocketCommits, rngSmokeSuitCommits;
        private int rngSmokeCompleteSpins, rngSmokeRecordPockets, rngSmokeOrdinal;
        private bool rngSmokeConfigured, rngSmokeStarted, rngSmokeExiting, rngSmokeArmed, rngSmokeFixture, rngSmokeTargetCommitted;

        private bool RngSmokeDouble { get { return rngSmokeCase != null && rngSmokeCase.StartsWith("double_", StringComparison.Ordinal); } }
        private bool RngSmokeExpectedFault { get { return rngSmokePhase == "load" || rngSmokePhase == "fail" || rngSmokePhase == "run"; } }

        private void RequireRngSmoke(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("RNG receipt smoke " + rngSmokeCase + "/" + rngSmokePhase + ": " + description);
            ++rngSmokeChecks;
        }

        private static string RngSmokeBytesSha(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        private static string RngSmokeCandidate()
        {
            string path = PcRngStateStore.PathForCurrentProcess + ".new";
            return File.Exists(path) ? Convert.ToBase64String(File.ReadAllBytes(path)) : "missing";
        }

        // These hooks are installed before the production restore reads its inputs.
        // The previous proof only checks bytes; it never supplies business state.
        private void ConfigureRngReceiptSmokeHooks()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteRngReceiptSmoke") < 0) return;
            RequireRngSmoke(!rngSmokeConfigured, "hooks configured twice");
            rngSmokeConfigured = true;
            rngSmokeCase = RoundSmokeArgument("-rouletteRngReceiptSmoke");
            rngSmokePhase = RoundSmokeArgument("-rouletteRngReceiptPhase");
            string[] phases;
            RequireRngSmoke(RngReceiptSmokePhases.TryGetValue(rngSmokeCase, out phases) && phases.Contains(rngSmokePhase), "unknown case/phase");
            RequireRngSmoke(!temporaryHistory && recordedHistoryKey.Contains(".Test.") &&
                RoundSmokeArgument("-rouletteVolumeTestKey").StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires isolated nonfresh history, volume and RNG");
            string requested = RoundSmokeArgument("-rouletteRngReceiptProof");
            RequireRngSmoke(Path.IsPathRooted(requested), "proof path must be absolute");
            rngSmokeOutput = Path.GetFullPath(requested);
            RequireRngSmoke(!File.Exists(rngSmokeOutput), "proof already exists");
            string previous = RoundSmokeArgument("-rouletteRngReceiptPreviousProof", false);
            int phaseIndex = Array.IndexOf(phases, rngSmokePhase);
            RequireRngSmoke((phaseIndex == 0) == string.IsNullOrEmpty(previous), "wrong predecessor argument");
            if (!string.IsNullOrEmpty(previous))
            {
                string json = File.ReadAllText(previous, Encoding.UTF8);
                rngSmokePreviousHash = RoundSmokeSha(json);
                rngSmokePrevious = JsonUtility.FromJson<RngReceiptSmokeProof>(json);
                RequireRngSmoke(rngSmokePrevious != null && (rngSmokePrevious.schema == 1 || rngSmokePrevious.schema == 2) &&
                    rngSmokePrevious.testCase == rngSmokeCase && rngSmokePrevious.phase == phases[phaseIndex - 1] &&
                    rngSmokePrevious.historyKey == recordedHistoryKey && rngSmokePrevious.raw != null &&
                    rngSmokePrevious.rawJson == JsonUtility.ToJson(rngSmokePrevious.raw) &&
                    rngSmokePrevious.rawSha256 == RoundSmokeSha(rngSmokePrevious.rawJson), "invalid closed predecessor proof");
                rngSmokeTarget = PcRngDrawReceipt.Clone(rngSmokePrevious.target);
                rngSmokeTargetCommitted = rngSmokePrevious.targetCommitted;
                rngSmokeOrdinal = rngSmokePrevious.targetDrawOrdinal;
                rngSmokeFixture = rngSmokePrevious.fixtureMutation;
            }
            rngSmokeBoot = ReadRoundSmokeRaw();
            if (rngSmokePrevious != null)
                RequireRngSmoke(JsonUtility.ToJson(rngSmokeBoot) == rngSmokePrevious.rawJson &&
                    RngSmokeCandidate() == rngSmokePrevious.rngCandidateBase64, "cold input bytes differ from closed proof");
            if (RngSmokeDouble)
            {
                byte[] bytes = File.ReadAllBytes(RoundSmokeArgument("-rouletteRngReceiptOracle"));
                rngSmokeOracleHash = RngSmokeBytesSha(bytes);
                rngSmokeOracle = JsonUtility.FromJson<RngReceiptSmokeOracle>(Encoding.UTF8.GetString(bytes));
                RequireRngSmoke(rngSmokeOracle != null && rngSmokeOracle.seed == 5489 && rngSmokeOracle.rawPocket == 30 &&
                    rngSmokeOracle.rawSuit == 0 && rngSmokeOracle.futureAfterSuit != null && rngSmokeOracle.futureAfterSuit.Length == 1300 &&
                    rngSmokeOracle.oracle == "MSVC std::mt19937; ELF Boost quotient range; no mix", "independent oracle differs");
                if (rngSmokePrevious != null) RequireRngSmoke(rngSmokePrevious.oracleSha256 == rngSmokeOracleHash, "cold oracle changed");
            }
            pcLifecycleCheckpoint = RngSmokeCheckpoint;
            pcLifecycleBeforeWrite = RngSmokeBeforeWrite;
            // Native Feature saves have their own observer. Keep the exact
            // source boundary in the proof; native0 is still an unpaid result.
            pcAutoCompleteCheckpoint = point => RngSmokeCheckpoint("AutoComplete:" + point);
            rngSmokeCut = rngSmokePhase == "rebuild" ? "RngPendingRebuilt" :
                rngSmokePhase == "pending" ? "RngPendingAccepted" : rngSmokePhase == "settled" ? "RoundOpenSaved" :
                rngSmokePhase == "replay" ? "DoubleReceiptAppliedSaved" : rngSmokePhase == "take" ?
                    (RngSmokeDoubleContextVersion(rngSmokeTarget) == 1 ? "DoubleTakeSaved" :
                        "AutoComplete:Feature:FeatureCompletedSaved0") : "";
            Debug.Log("ROULETTE_RNG_RECEIPT_BOOT: case=" + rngSmokeCase + " phase=" + rngSmokePhase +
                " raw=" + RoundSmokeSha(JsonUtility.ToJson(rngSmokeBoot)) + " previous=" + (rngSmokePreviousHash ?? "none"));
        }

        private PcOutcomeSource CreateRngReceiptSmokeOutcome()
        {
            if (!rngSmokeConfigured || !RngSmokeDouble) return new PcOutcomeSource();
            RequireRngSmoke(rngSmokeOracle != null, "controlled schedule has no independent oracle");
            uint[] snapshot = rngSmokePrevious == null ? new OriginalPlatformRngCore(rngSmokeOracle.seed).ExportSnapshot() : null;
            Debug.Log("ROULETTE_RNG_RECEIPT_CONTROLLED_SCHEDULE: seed=5489 mix=false realDraw=true realWriter=true oracle=" + rngSmokeOracleHash);
            return PcOutcomeSource.CreateReceiptDiagnostic(snapshot, PcRngStateStore.PathForCurrentProcess);
        }

        private void AttachRngReceiptSmokeHooks()
        {
            if (rngSmokeConfigured) pcOutcome.ConfigureReceiptDiagnostics(RngSmokeDrawCheckpoint);
        }

        private void RngSmokeDrawCheckpoint(string point, PcRngDrawReceipt candidate)
        {
            bool pocket = candidate.context.kind == 1;
            rngSmokeEvents.Add("Rng:" + point + ":" + candidate.context.kind);
            if (point == "before-replace")
            {
                if (pocket) ++rngSmokePocketAttempts; else ++rngSmokeSuitAttempts;
                if (rngSmokeCase == "fault_replace" && rngSmokePhase == "run")
                {
                    RequireRngSmoke(pocket && rngSmokePocketAttempts == 1 && rngSmokePocketCommits == 0, "wrong replacement fault draw");
                    rngSmokeTarget = PcRngDrawReceipt.Clone(candidate);
                    rngSmokeTargetCommitted = false;
                    rngSmokeOrdinal = 1;
                    rngSmokeFaultBefore = ReadRoundSmokeRaw();
                    rngSmokeFaultCandidate = RngSmokeCandidate();
                    RequireRngSmoke(rngSmokeFaultCandidate != "missing", "replacement fault lacks flushed candidate");
                    Debug.Log("ROULETTE_RNG_RECEIPT_INJECT: boundary=before-replace candidateFlushed=true mainUnchanged=true");
                    throw new IOException("RNG receipt smoke before-replace failure");
                }
                return;
            }
            RequireRngSmoke(point == "saved", "unknown native checkpoint");
            if (pocket) ++rngSmokePocketCommits; else ++rngSmokeSuitCommits;
            bool targeted = RngSmokeDouble ? !pocket : rngSmokeArmed && pocket;
            if (rngSmokeCase == "idle") targeted = pocket && pcLifecycleIdleDispatch;
            if (rngSmokeCase == "demo" && candidate.rawResult == candidate.effectiveResult) targeted = false;
            if (!targeted) return;
            rngSmokeTarget = PcRngDrawReceipt.Clone(candidate);
            rngSmokeTargetCommitted = true;
            rngSmokeOrdinal = pocket ? rngSmokePocketCommits : rngSmokeSuitCommits;
            var stored = PcRngStateStore.LoadStrict(PcRngStateStore.PathForCurrentProcess);
            RequireRngSmoke(stored != null && stored.Version == 3 &&
                JsonUtility.ToJson(stored.Receipt) == JsonUtility.ToJson(candidate), "saved envelope lacks its exact joint receipt");
            if (pocket)
            {
                PcSpinDrawContext context = JsonUtility.FromJson<PcSpinDrawContext>(candidate.context.contextJson);
                RequireRngSmoke(pendingSpin == null && string.IsNullOrEmpty(PlayerPrefs.GetString(PendingSpinKey, "")) &&
                    session.Credit == context.pending.creditBefore - context.pending.wager && session.Rounds == context.rounds &&
                    recordedRoundPages.Count == context.pending.baseHistoryCount &&
                    candidate.context.operationId == context.pending.id, "saved cut followed result journal or settlement");
                if (rngSmokeCase == "idle") RequireRngSmoke(latestRoundLifecycle.idleEntry.step == PcIdleStep.DrawInFlight &&
                    latestRoundLifecycle.idleEntry.successorSpinId == candidate.context.operationId, "Idle draw lost reserved identity");
                if (rngSmokeCase == "demo") RequireRngSmoke(context.demoOption == 30 && candidate.effectiveResult == 27 &&
                    candidate.rawResult != candidate.effectiveResult, "Demo draw lacks distinct raw/effective values");
            }
            else
            {
                PcDoubleDrawContext context = RngSmokeReadDoubleContext(candidate);
                RequireRngSmoke(candidate.rawResult == rngSmokeOracle.rawSuit && candidate.effectiveResult == rngSmokeOracle.rawSuit &&
                    session.InDoubleUp && session.DoubleRounds == 0 && session.DoubleStake == 36 &&
                    session.Credit == context.creditBefore && PcReceiptLatestPage == context.priorPageJson &&
                    rngSmokePocketCommits == 1 && rngSmokeSuitCommits == 1, "saved suit cut applied the guess or changed its dealer");
                RngSmokeCheckOracle();
            }
            if (rngSmokePhase == "save")
            {
                RngSmokeMutateFixture();
                RngSmokeExit("RngSaved", "checkpoint");
            }
        }

        private void RngSmokeMutateFixture()
        {
            if (!rngSmokeCase.StartsWith("fault_", StringComparison.Ordinal) || rngSmokeCase == "fault_pending") return;
            string path = PcRngStateStore.PathForCurrentProcess;
            if (rngSmokeCase == "fault_hash" || rngSmokeCase == "fault_version")
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (rngSmokeCase == "fault_hash") bytes[bytes.Length - 1] ^= 1;
                else
                {
                    Array.Copy(BitConverter.GetBytes(99), 0, bytes, 4, 4);
                    using (var sha = SHA256.Create()) Array.Copy(sha.ComputeHash(bytes, 0, bytes.Length - 32), 0, bytes, bytes.Length - 32, 32);
                }
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
                { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            }
            else
            {
                PcRngStoredState state = PcRngStateStore.LoadStrict(path);
                PcRngDrawReceipt changed = PcRngDrawReceipt.Clone(state.Receipt);
                if (rngSmokeCase == "fault_scope") changed.context.scope += ".wrong";
                else if (rngSmokeCase == "fault_profile") changed.context.profile = "receipt-wrong-profile";
                else
                {
                    var context = JsonUtility.FromJson<PcSpinDrawContext>(changed.context.contextJson);
                    if (rngSmokeCase == "fault_id") context.pending.id = Guid.NewGuid().ToString("N");
                    else if (rngSmokeCase == "fault_baseline") ++context.pending.creditBefore;
                    else RequireRngSmoke(false, "unknown mutation fixture");
                    changed.context.contextJson = JsonUtility.ToJson(context);
                }
                changed.contextSha256 = PcRngDrawReceipt.ContextDigest(changed.context);
                PcRngStateStore.SaveWithReceipt(path, state.Snapshot, changed);
            }
            rngSmokeFixture = true;
            Debug.Log("ROULETTE_RNG_RECEIPT_FIXTURE_MUTATION: case=" + rngSmokeCase + " isolatedFile=true recomputeContext=" +
                (rngSmokeCase != "fault_hash" && rngSmokeCase != "fault_version"));
        }

        private void RngSmokeBeforeWrite(string boundary)
        {
            if (rngSmokeCase != "fault_pending" || rngSmokePhase != "fail" || boundary != "RngPendingRebuild") return;
            rngSmokeFaultBefore = ReadRoundSmokeRaw();
            rngSmokeFaultCandidate = RngSmokeCandidate();
            Debug.Log("ROULETTE_RNG_RECEIPT_INJECT: boundary=RngPendingRebuild beforeSetString=true");
            throw new IOException("RNG receipt smoke pending-before-write failure");
        }

        private void RngSmokeCheckpoint(string point)
        {
            rngSmokeEvents.Add(point);
            if (point == "CompleteSpin") ++rngSmokeCompleteSpins;
            if (point == "RecordPocket") ++rngSmokeRecordPockets;
            if (string.IsNullOrEmpty(rngSmokeCut) || point != rngSmokeCut) return;
            RequireRngSmoke(!pcRoundLifecycleStorageFaulted && rngSmokeTarget != null, "cut has no valid target");
            if (RngSmokeDouble) RngSmokeCheckDouble(point);
            else RngSmokeCheckWheel(point);
            RngSmokeExit(point, "checkpoint");
        }

        private void RngSmokeCheckNoDraw()
        {
            RequireRngSmoke(rngSmokePocketAttempts == 0 && rngSmokeSuitAttempts == 0 && rngSmokePocketCommits == 0 &&
                rngSmokeSuitCommits == 0 && rngSmokePrevious != null && ReadRoundSmokeRaw().rng == rngSmokePrevious.raw.rng,
                "cold continuation drew again or changed the joint RNG envelope");
        }

        private void RngSmokeCheckWheel(string point)
        {
            var context = JsonUtility.FromJson<PcSpinDrawContext>(rngSmokeTarget.context.contextJson);
            PendingSpinRecord original = context.pending;
            RequireRngSmoke(original.profile == session.Profile.Id && original.wager == 1 && original.bets.Length == 1 &&
                original.bets[0].id == 127 && original.bets[0].amount == 1, "independent straight-27 fixture changed");
            if (rngSmokePrevious != null && rngSmokeCase != "fault_replace") RngSmokeCheckNoDraw();
            bool beforeDebit = point == "RngPendingRebuilt";
            bool evaluated = point == "RoundOpenSaved" || point == "WheelComplete" || point == "StableReload" ||
                point == "PendingFaultRecovered" || point == "WriteFaultRecovered";
            if (point == "RoundOpenSaved")
                RequireRngSmoke(PcPaymentTracked, "a newly evaluated wheel must create the v3 payment archive");
            long payout = rngSmokeTarget.effectiveResult == 27 ? 36 : 0;
            bool moneyApplied = evaluated && (!PcPaymentTracked || pcGamePayment.moneyApplied);
            long expectedCredit = original.creditBefore - (beforeDebit ? 0 : original.wager) + (moneyApplied ? payout : 0);
            RequireRngSmoke(session.Credit == expectedCredit && session.Wagered == context.wagered + (evaluated ? 1 : 0) &&
                session.Returned == context.returned + (evaluated ? payout : 0) && session.Rounds == context.rounds + (evaluated ? 1 : 0) &&
                session.Wins == context.wins + (evaluated && payout > 0 ? 1 : 0) && session.Denomination == original.denomination &&
                ((RouletteSession)session).MoneyRemainderCents == original.moneyRemainderCents, "wheel money differs from independent straight-bet arithmetic");
            RequireRngSmoke(recordedRoundPages.Count == Math.Min(50, original.baseHistoryCount + (evaluated ? 1 : 0)), "wheel archive count differs");
            if (evaluated)
            {
                RecordedRound page = recordedRoundPages[0];
                RequireRngSmoke(page.spinJournalId == original.id && page.pocket == rngSmokeTarget.effectiveResult &&
                    page.wager == 1 && page.returned == payout && latestRoundLifecycle.spinJournalId == original.id,
                    "evaluated page uses another operation or result");
                if (PcPaymentTracked)
                {
                    RequireRngSmoke(pcGamePayment.owner == PcGamePaymentOwner.Base &&
                        pcGamePayment.spinJournalId == original.id && pcGamePayment.roundArchiveId == page.roundArchiveId &&
                        pcGamePayment.creditBefore == original.creditBefore - original.wager &&
                        pcGamePayment.baseAward == payout && pcGamePayment.priorHistoryCount == original.baseHistoryCount &&
                        page.creditAfter == expectedCredit && page.awaitingFinalPayout == !pcGamePayment.historyPublished &&
                        ((RouletteSession)session).RoundPayoutPending == !pcGamePayment.moneyApplied &&
                        CommittedHistoryCount() == Math.Min(50, original.baseHistoryCount + (pcGamePayment.historyPublished ? 1 : 0)),
                        "v3 wheel evaluation, wallet or publication boundary differs");
                    if (point == "RoundOpenSaved")
                        RequireRngSmoke(!pcGamePayment.prepared && !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished,
                            "RoundOpen evaluation prematurely paid or published history");
                    else RequireRngSmoke(pcGamePayment.moneyApplied && pcGamePayment.IsResolved && pcGamePayment.historyPublished,
                        "completed v3 wheel lacks resolved and published payment");
                }
                else RequireRngSmoke(pcRoundArchiveSchemaVersion == 1 && pcGamePayment == null &&
                    !((RouletteSession)session).RoundPayoutPending && page.creditAfter == expectedCredit,
                    "legacy wheel no longer uses its explicit paid-base archive");
            }
            if (point == "RngPendingRebuilt" || point == "RngPendingAccepted" || point == "RoundOpenSaved")
                RequireRngSmoke(pendingSpin != null && pendingSpin.id == original.id && pendingSpin.pocket == rngSmokeTarget.effectiveResult &&
                    PlayerPrefs.GetString(PendingSpinKey, "") != "", "same-id journal absent at cut");
            if (point == "RngPendingRebuilt" || point == "RngPendingAccepted")
                RequireRngSmoke(rngSmokeCompleteSpins == 0 && rngSmokeRecordPockets == 0 && sourceRoundEndCount == 0 &&
                    (beforeDebit || session.SpinPending), "pending restore performed paid effects");
            if (point == "RoundOpenSaved") RequireRngSmoke(rngSmokeCompleteSpins == 1 && rngSmokeRecordPockets == 1 &&
                sourceRoundEndCount == 0 && latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen, "settled cut missed exactly one real settlement");
            if (point == "WheelComplete" || point == "StableReload" || point == "PendingFaultRecovered" || point == "WriteFaultRecovered")
                RequireRngSmoke(!session.SpinPending && pendingSpin == null && !recordedRoundPages[0].awaitingFinalPayout &&
                    latestRoundLifecycle.roundStage == PcRoundStage.Ended, "completed wheel remains pending");
        }

        private void RngSmokeCheckOracle()
        {
            PcRngStoredState state = PcRngStateStore.LoadStrict(PcRngStateStore.PathForCurrentProcess);
            RequireRngSmoke(state != null && state.Receipt != null && state.Receipt.context.kind == 2 &&
                state.Receipt.rawResult == rngSmokeOracle.rawSuit, "D2 envelope lacks oracle dealer");
            // Only an offline clone advances. The playable provider is untouched.
            OriginalPlatformRngCore clone = OriginalPlatformRngCore.FromSnapshot(state.Snapshot);
            for (int i = 0; i < rngSmokeOracle.futureAfterSuit.Length; ++i)
                RequireRngSmoke(clone.NextRaw() == rngSmokeOracle.futureAfterSuit[i], "independent future oracle word " + i + " differs");
        }

        private int RngSmokeDoubleContextVersion(PcRngDrawReceipt receipt)
        {
            RequireRngSmoke(receipt != null && receipt.context != null && receipt.context.kind == 2,
                "Double proof has no suit context");
            var header = JsonUtility.FromJson<PcDoubleContextVersion>(receipt.context.contextJson);
            RequireRngSmoke(header != null && (header.version == 1 || header.version == 2 || header.version == 3),
                "Double proof has an unsupported business context");
            return header.version;
        }

        private PcDoubleDrawContext RngSmokeReadDoubleContext(PcRngDrawReceipt receipt)
        {
            int version = RngSmokeDoubleContextVersion(receipt);
            PcDoubleDrawContext financial;
            string nativeJson = "", nativeDigest = "", featureId = "";
            if (version == 1)
            {
                financial = JsonUtility.FromJson<PcDoubleDrawContext>(receipt.context.contextJson);
                RequireRngSmoke(pcRoundArchiveSchemaVersion == 1 && pcDoubleFeatureNv == null && pcGamePayment == null &&
                    !((RouletteSession)session).RoundPayoutPending, "v1 Double proof lost its legacy paid-base meaning");
            }
            else if (version == 2)
            {
                var wrapped = JsonUtility.FromJson<PcDoubleDrawContextV2>(receipt.context.contextJson);
                RequireRngSmoke(wrapped != null && wrapped.version == 2 && wrapped.settlementMode == 2 &&
                    pcRoundArchiveSchemaVersion == 2 && pcDoubleFeatureNv != null && pcGamePayment == null &&
                    !((RouletteSession)session).RoundPayoutPending, "v2 Double proof lost its deferred paid-base meaning");
                financial = wrapped.financial; nativeJson = wrapped.priorFeatureNvJson;
                nativeDigest = wrapped.priorFeatureNvDigest; featureId = wrapped.featureId;
            }
            else
            {
                var wrapped = JsonUtility.FromJson<PcDoubleDrawContextV3>(receipt.context.contextJson);
                RequireRngSmoke(wrapped != null && wrapped.version == 3 && wrapped.settlementMode == 3 &&
                    wrapped.unpaidBase && PcPaymentTracked && pcDoubleFeatureNv != null &&
                    wrapped.priorPaymentDigest == RoundSmokeSha(wrapped.priorPaymentJson),
                    "v3 Double proof lost its explicit unpaid wrapper or payment digest");
                financial = wrapped.financial; nativeJson = wrapped.priorFeatureNvJson;
                nativeDigest = wrapped.priorFeatureNvDigest; featureId = wrapped.featureId;
                var prior = JsonUtility.FromJson<PcGamePaymentRecordV1>(wrapped.priorPaymentJson);
                RequireRngSmoke(prior != null && prior.version == 1 && !prior.prepared && !prior.moneyApplied &&
                    !prior.historyPublished && prior.owner == PcGamePaymentOwner.Double &&
                    prior.baseOffer == PcGamePaymentOffer.Transferred && prior.featureId == featureId &&
                    prior.creditBefore == 999 && prior.baseAward == 36 && prior.priorHistoryCount == 0 &&
                    pcGamePayment.version == prior.version && pcGamePayment.roundArchiveId == prior.roundArchiveId &&
                    pcGamePayment.spinJournalId == prior.spinJournalId && pcGamePayment.profile == prior.profile &&
                    pcGamePayment.operationId == prior.operationId && pcGamePayment.priorHistoryCount == prior.priorHistoryCount &&
                    pcGamePayment.creditBefore == prior.creditBefore && pcGamePayment.denomination == prior.denomination &&
                    pcGamePayment.remainderCents == prior.remainderCents && pcGamePayment.baseAward == prior.baseAward &&
                    pcGamePayment.owner == prior.owner && pcGamePayment.baseOffer == prior.baseOffer &&
                    pcGamePayment.featureId == prior.featureId,
                    "v3 Double proof changed its unpaid payment identity or immutable money basis");
                if (financial != null)
                    RequireRngSmoke(prior.roundArchiveId == financial.roundArchiveId && prior.spinJournalId == financial.spinJournalId &&
                        prior.profile == receipt.context.profile && prior.creditBefore == financial.creditBefore &&
                        prior.denomination == financial.denomination && prior.remainderCents == financial.remainderCents,
                        "v3 Double payment differs from its financial context");
            }
            RequireRngSmoke(financial != null && financial.version == 1 &&
                financial.priorPageDigest == RoundSmokeSha(financial.priorPageJson),
                "Double financial context or unchanged prior-page digest differs");
            if (version > 1)
            {
                RequireRngSmoke(nativeDigest == RoundSmokeSha(nativeJson), "Double prior-native digest differs");
                var native = JsonUtility.FromJson<PcDoubleFeatureNvV1>(nativeJson);
                RequireRngSmoke(native != null && native.version == 1 && native.featureId == featureId &&
                    native.savedState == 2 && native.completionStage == 0 &&
                    native.roundArchiveId == financial.roundArchiveId && native.spinJournalId == financial.spinJournalId &&
                    native.profile == receipt.context.profile && native.stepCount == financial.featureStep &&
                    native.suitMode == financial.suitMode && pcDoubleFeatureNv.featureId == featureId,
                    "Double prior-native record differs from its decision");
            }
            return financial;
        }

        private void RngSmokeCheckDouble(string point)
        {
            RngSmokeCheckNoDraw();
            RngSmokeCheckOracle();
            PcDoubleDrawContext context = RngSmokeReadDoubleContext(rngSmokeTarget);
            int version = RngSmokeDoubleContextVersion(rngSmokeTarget);
            bool lost = rngSmokeCase == "double_loss";
            bool afterTake = rngSmokeCase == "double_win" && (point == "DoubleTakeSaved" ||
                point == "AutoComplete:Feature:FeatureCompletedSaved0" || point == "DoubleComplete" || point == "StableReload");
            bool terminal = lost || rngSmokeCase == "double_cap" || afterTake;
            bool applied = version == 1 ? terminal : pcDoubleFeatureNv.completionStage == 2;
            long award = lost ? 0 : 72;
            long credit = applied ? 999 + award : 999;
            long returned = applied ? award : 36;
            long pageCredit = version < 3 && !applied ? 1035 : credit;
            RecordedRound page = recordedRoundPages[0];
            RequireRngSmoke(context.stake == 36 && context.featureStep == 0 && context.creditBefore == 999 &&
                context.choice == (lost ? -1 : -2) && rngSmokeTarget.rawResult == 0 && rngSmokeTarget.effectiveResult == 0,
                "D2 target differs from preselected oracle decision");
            RequireRngSmoke(session.Credit == credit && session.Wagered == 1 && session.Returned == returned && session.Rounds == 1 &&
                session.Wins == 1 && session.InDoubleUp == !applied &&
                page.spinJournalId == context.spinJournalId && page.roundArchiveId == context.roundArchiveId &&
                page.hasDoubleUp && page.doubleUp.steps.Length == 1 && page.doubleUp.steps[0].dealer == 0 &&
                page.doubleUp.steps[0].choice == (lost ? 5 : 4) && page.doubleUp.steps[0].won == (lost ? 0 : 72) &&
                page.doubleUp.complete == applied && page.doubleUp.tookWin == (applied && afterTake) &&
                page.doubleUp.finalWin == (applied ? award : 0) && page.creditAfter == pageCredit,
                "D2 financial/page replay differs");
            RequireRngSmoke(rngSmokeCompleteSpins == 0 && rngSmokeRecordPockets == 0 &&
                (applied || session.DoubleStake == award && session.DoubleRounds == 1), "D2 replay repeated base settlement or lost active stake");
            if (version == 1)
                RequireRngSmoke(applied == terminal && !((RouletteSession)session).RoundPayoutPending,
                    "legacy v1 terminal decision is no longer its original immediate settlement");
            else
            {
                bool native0 = point == "AutoComplete:Feature:FeatureCompletedSaved0";
                bool complete = point == "DoubleComplete" || point == "StableReload";
                RequireRngSmoke(pcDoubleFeatureNv.savedState == (native0 || complete ? 0 : 3) &&
                    pcDoubleFeatureNv.completionStage == (complete ? 2 : native0 ? 1 : 0),
                    "native Double decision, native0 and applied phases were collapsed");
                if (native0)
                    RequireRngSmoke(terminal && !applied && pcDoubleFeatureNv.completionAward == award && sourceRoundEndCount == 0,
                        "native0 already paid or used another final award");
                if (version == 3)
                    RequireRngSmoke(pcGamePayment.moneyApplied == applied && pcGamePayment.historyPublished == complete &&
                        pcGamePayment.prepared == complete && (!complete || pcGamePayment.IsResolved) &&
                        ((RouletteSession)session).RoundPayoutPending == !applied &&
                        page.awaitingFinalPayout == !complete && CommittedHistoryCount() == (complete ? 1 : 0),
                        "v3 Double model, payment or history publication differs");
                else RequireRngSmoke(pcGamePayment == null && !((RouletteSession)session).RoundPayoutPending,
                    "legacy v2 acquired an unpaid-base payment");
            }
        }

        private void StartRngReceiptSmoke()
        {
            if (!rngSmokeConfigured) return;
            RequireRngSmoke(!rngSmokeStarted, "observer started twice");
            rngSmokeStarted = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            new GameObject("RNG receipt acceptance observer").AddComponent<RouletteRoundLifecycleSmokeHost>()
                .StartCoroutine(RngReceiptSmoke());
        }

        private IEnumerator RngReceiptSmoke()
        {
            if (pcRoundLifecycleStorageFaulted)
            {
                RequireRngSmoke(RngSmokeExpectedFault, "unexpected startup fault");
                yield return RngSmokeObserveFault();
                yield break;
            }
            RequireRngSmoke(session != null && pcOutcome != null && sourceInputModule != null && !pcAutoPlayStorageFaulted &&
                !autoplay && sourceRoundEndCount == 0 && SourcePlayerInputs.Count == 0 && !ideckSecondDisplay && !topperDedicatedDisplay,
                "startup not isolated and inactive");
            if (rngSmokePrevious != null)
                RequireRngSmoke(gameStartTimestamp == 0 && demoOutcome.SelectedOption == 0, "cold boot retained live Demo/timing state");
            if (rngSmokePrevious == null)
            {
                RequireRngSmoke(session.Credit == 1000 && session.Rounds == 0 && recordedRoundPages.Count == 0 && session.Profile.Id == "RouletteDeluxeSZ97",
                    "fresh namespace has wrong financial/profile baseline");
                // The real initial Idle entry purges queued player events.
                // Let that entry finish before sampling the first wager.
                yield return StartGameLiveFrames(3);
                RequireRngSmoke(sourceGameplayState == SourceGameplayState.Idle && !configuredAutoPlayCheckPending &&
                    SourcePlayerInputs.Count == 0 && session.TotalBet == 0 && session.Credit == 1000 && session.Rounds == 0 &&
                    sourceRoundEndCount == 0 && rngSmokePocketAttempts == 0 && rngSmokeSuitAttempts == 0,
                    "real initial Idle did not finish without a wager or draw");
                if (RngSmokeDouble) yield return RngSmokeSeedDouble();
                else yield return RngSmokeSeedWheel();
                if (rngSmokeCase == "fault_replace") { yield return RngSmokeObserveFault(); yield break; }
                yield return RngSmokeAwaitCut();
                yield break;
            }
            if (RngSmokeExpectedFault) { yield return RngSmokeObserveFault(); yield break; }
            if (rngSmokeCase == "fault_replace")
            {
                RequireRngSmoke(!rngSmokeTargetCommitted && PcRngStateStore.LoadStrict(PcRngStateStore.PathForCurrentProcess).Receipt == null &&
                    session.Credit == 1000 && session.Rounds == 0 && session.TotalBet == 1, "old main did not recover unchanged");
                // Restored staged Bet enters through the real input service,
                // whose entry purge must finish before sampling a new PLAY.
                yield return RngSmokeWait(() => !sourceBetStateEntryPending &&
                    !configuredAutoPlayCheckPending && SourcePlayerInputs.Count == 0, "restored Bet entry before new PLAY");
                RequireRngSmoke(sourceGameplayState == SourceGameplayState.Bet && !sourceBetStateEntryPending &&
                    !configuredAutoPlayCheckPending && SourcePlayerInputs.Count == 0 &&
                    session.Credit == 1000 && session.TotalBet == 1 && session.Rounds == 0 &&
                    !session.SpinPending && pendingSpin == null && sourceRoundEndCount == 0 &&
                    rngSmokePocketAttempts == 0 && rngSmokeSuitAttempts == 0 &&
                    rngSmokePocketCommits == 0 && rngSmokeSuitCommits == 0 &&
                    rngSmokeCompleteSpins == 0 && rngSmokeRecordPockets == 0 &&
                    ReadRoundSmokeRaw().rng == rngSmokeBoot.rng && RngSmokeCandidate() == rngSmokePrevious.rngCandidateBase64,
                    "restored Bet entry changed old RNG/candidate or performed business effects");
                rngSmokeArmed = true;
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                yield return RngSmokePlay();
                yield return RngSmokeUntilIdle();
                RequireRngSmoke(rngSmokeTargetCommitted && rngSmokePocketCommits == 1 && rngSmokePocketAttempts == 1 &&
                    rngSmokeTarget.context.operationId != rngSmokePrevious.target.context.operationId &&
                    rngSmokeCompleteSpins == 1 && rngSmokeRecordPockets == 1 && sourceRoundEndCount == 1,
                    "replacement recovery reused failed identity or duplicated effects");
                RngSmokeCheckWheel("WriteFaultRecovered");
                yield return RngSmokeStableFrames();
                RngSmokeExit("WriteFaultRecovered", "complete");
                yield break;
            }
            if (rngSmokePhase == "take")
            {
                RngSmokeCheckDouble("BeforeTake");
                yield return RngSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0, "cold TAKE ready");
                RefreshPortraitUi();
                QueueStartGameButton(uiDoubleTakeWinButton, 305, true);
                yield return RngSmokeAwaitCut();
                yield break;
            }
            if (!string.IsNullOrEmpty(rngSmokeCut))
            {
                if (rngSmokePhase == "settled") configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                yield return RngSmokeAwaitCut();
                yield break;
            }
            bool reload = rngSmokePhase == "reload";
            if (!RngSmokeDouble && !reload) configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            yield return RngSmokeUntilIdle();
            RequireRngSmoke(sourceRoundEndCount == (reload ? 0 : 1), "cold continuation repeated or omitted End");
            string point = reload ? "StableReload" : RngSmokeDouble ? "DoubleComplete" :
                rngSmokeCase == "fault_pending" ? "PendingFaultRecovered" : "WheelComplete";
            if (RngSmokeDouble) RngSmokeCheckDouble(point);
            else RngSmokeCheckWheel(point);
            if (!RngSmokeDouble)
                RequireRngSmoke(rngSmokeCompleteSpins == (rngSmokeCase == "fault_pending" ? 1 : 0) &&
                    rngSmokeRecordPockets == rngSmokeCompleteSpins, "cold final phase repeated wheel settlement");
            yield return RngSmokeStableFrames();
            if (rngSmokePhase == "complete" && (rngSmokeCase == "demo" || rngSmokeCase == "double_win" || rngSmokeCase == "double_loss"))
                RngSmokeCapture("complete");
            RngSmokeExit(point, "complete");
        }

        private IEnumerator RngSmokeSeedWheel()
        {
            yield return RngSmokePlaceBet();
            if (rngSmokeCase != "idle") configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            if (rngSmokeCase == "manual")
            {
                yield return RngSmokePlay();
                yield return RngSmokeUntilIdle();
                RequireRngSmoke(rngSmokePocketCommits == 1 && sourceRoundEndCount == 1, "manual baseline did not End exactly once");
                yield return RngSmokePlaceBet();
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            }
            if (rngSmokeCase == "idle")
            {
                configuredAutoPlay.SetOptions(true, false, true, true, false, 2);
                RequireRngSmoke(configuredAutoPlay.AutoPlay != 0 && !configuredAutoPlay.AutoPlayOffWhenGameFinished,
                    "idle successor fixture requires AutoPlay armed and OffWhenGameFinished cleared");
            }
            rngSmokeArmed = rngSmokeCase != "idle";
            if (rngSmokeCase == "demo")
            {
                // A naturally equal raw result proves no override difference.
                // Complete that real round and try a new real menu/PLAY cycle.
                for (int attempt = 0; attempt < 8; ++attempt)
                {
                    yield return StartGameChooseDemo(30);
                    yield return RngSmokePlay();
                    yield return RngSmokeUntilIdle();
                    yield return RngSmokePlaceBet();
                    configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                }
                RequireRngSmoke(false, "eight genuine Demo draws all equaled the selected result");
            }
            else yield return RngSmokePlay();
        }

        private IEnumerator RngSmokeSeedDouble()
        {
            yield return RngSmokePlaceBet();
            yield return StartGameChooseDemo(30);
            yield return RngSmokePlay();
            RequireRngSmoke(pcOutcome.Receipt.rawResult == rngSmokeOracle.rawPocket && pcOutcome.Receipt.effectiveResult == 27,
                "independent first draw did not map through real Demo");
            yield return RngSmokeWait(() => SourceDoubleOfferActive && !winRackActive, "real Double offer");
            RequireRngSmoke(PcPaymentTracked && !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                ((RouletteSession)session).RoundPayoutPending && session.Credit == 999 && session.Returned == 36 &&
                session.Rounds == 1 && sourceRoundEndCount == 0 && CommittedHistoryCount() == 0 && DisplayedCredit == 1035,
                "new base golden evaluation did not retain its unpaid wallet and display projection");
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.D));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return RngSmokeWait(() => session.InDoubleUp && DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                "real Double decision");
            RequireRngSmoke(session.Credit == 999 && session.DoubleStake == 36 && session.DoubleRounds == 0,
                "Double entry changed the oracle stake");
            RefreshPortraitUi();
            bool red = rngSmokeCase == "double_loss";
            QueueStartGameButton(red ? uiDoubleRedButton : uiDoubleBlackButton, red ? 306 : 307, true);
        }

        private IEnumerator RngSmokePlaceBet()
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiChipSelections[0].GetComponent<UnityEngine.UI.Button>());
            RefreshPortraitUi();
            if (session.TotalBet > 0)
            {
                // A retained settled table is still Idle. A real table input
                // enters Bet before Clear becomes a legal table operation.
                if (sourceGameplayState == SourceGameplayState.Idle)
                {
                    QueueStartGameButton(uiBetButtons[27], 308, true);
                    yield return RngSmokeWait(() => sourceGameplayState == SourceGameplayState.Bet &&
                        !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "retained table enters Bet");
                }
                RefreshPortraitUi();
                QueueStartGameButton(uiClearButton, 309, true);
                yield return RngSmokeWait(() => session.TotalBet == 0 && SourcePlayerInputs.Count == 0, "real Clear before new wager");
            }
            RefreshPortraitUi();
            QueueStartGameButton(uiBetButtons[27], 310, true);
            yield return RngSmokeWait(() => session.TotalBet == 1 && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "real one-chip straight wager");
            RequireRngSmoke(session.Bets.Count == 1 && session.Bets[127] == 1, "one-chip straight table differs");
        }

        private IEnumerator RngSmokePlay()
        {
            RefreshPortraitUi();
            RequireRngSmoke(SourcePlayInputAllowed && uiSpinButton.interactable && SourcePlayerInputs.Count == 0, "real PLAY gated");
            QueueStartGameButton(uiSpinButton, 304, true);
            yield return RngSmokeWait(() => session.SpinPending && pendingSpin != null, "actual PLAY commit", rngSmokeCase == "fault_replace" && rngSmokePhase == "run");
        }

        private IEnumerator RngSmokeWait(Func<bool> ready, string description, bool allowFault = false)
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 90f;
            while (!ready() && !pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            sourceInputModule.enabled = false;
            RequireRngSmoke(ready() && !pcRoundLifecycleStorageFaulted || allowFault && pcRoundLifecycleStorageFaulted, "wait failed: " + description);
        }

        private IEnumerator RngSmokeUntilIdle()
        {
            yield return RngSmokeWait(() => latestRoundLifecycle != null && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry != null && latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                !configuredAutoPlayCheckPending && sourceGameplayState == SourceGameplayState.Idle && sourceRoundPhase == SourceRoundPhase.None &&
                !session.SpinPending && !spinning && !winRackActive && !ResultDisplayActive && SourcePlayerInputs.Count == 0, "actual Ended/Complete Idle");
        }

        private IEnumerator RngSmokeAwaitCut()
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 90f;
            while (!pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            RequireRngSmoke(false, "required exact checkpoint not reached: " + rngSmokeCut);
        }

        private IEnumerator RngSmokeStableFrames()
        {
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw()), money = StartGameDurableFingerprint();
            int ends = sourceRoundEndCount, pockets = rngSmokePocketAttempts, suits = rngSmokeSuitAttempts;
            sourceInputModule.enabled = true;
            for (int i = 0; i < 4; ++i)
            {
                yield return null;
                RequireRngSmoke(!pcRoundLifecycleStorageFaulted && JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw &&
                    StartGameDurableFingerprint() == money && sourceRoundEndCount == ends &&
                    rngSmokePocketAttempts == pockets && rngSmokeSuitAttempts == suits, "stable live frames changed committed state");
            }
            sourceInputModule.enabled = false;
        }

        private IEnumerator RngSmokeObserveFault()
        {
            if (!pcRoundLifecycleStorageFaulted)
            {
                if (sourceInputModule != null) sourceInputModule.enabled = true;
                float deadline = Time.realtimeSinceStartup + 90f;
                while (!pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            }
            RequireRngSmoke(pcRoundLifecycleStorageFaulted && pcAutoPlayStorageFaulted && !autoplay && !configuredAutoPlayCheckPending &&
                !sourceTableInputEnabled && (sourceInputModule == null || !sourceInputModule.enabled) &&
                (portraitCanvas == null || !portraitCanvas.gameObject.activeSelf), "fault did not stop dispatch/presentation");
            RoundLifecycleSmokeRaw before = ReadRoundSmokeRaw();
            string candidate = RngSmokeCandidate();
            if (rngSmokeFaultBefore != null)
                RequireRngSmoke(JsonUtility.ToJson(before) == JsonUtility.ToJson(rngSmokeFaultBefore) && candidate == rngSmokeFaultCandidate,
                    "failed write changed protected inputs");
            else RequireRngSmoke(before.envelope == rngSmokeBoot.envelope && before.rng == rngSmokeBoot.rng &&
                before.auto == rngSmokeBoot.auto && before.pending == rngSmokeBoot.pending, "loaded fault rewrote protected input evidence");
            string operation = rngSmokeCase == "fault_hash" || rngSmokeCase == "fault_version" ? "rng-loaded" :
                rngSmokeCase == "fault_replace" ? "committed" : "restored";
            RequireRngSmoke(pcAutoPlayStorageFaultOperation == operation, "fault operation differs");
            RequireRngSmoke(sourceRoundEndCount == 0 && rngSmokeCompleteSpins == 0 && rngSmokeRecordPockets == 0 &&
                rngSmokePocketCommits == 0 && rngSmokeSuitCommits == 0 && rngSmokeSuitAttempts == 0 &&
                rngSmokePocketAttempts == (rngSmokeCase == "fault_replace" ? 1 : 0), "fault performed forbidden business effects");
            for (int i = 0; i < 4; ++i)
            {
                yield return null;
                RequireRngSmoke(JsonUtility.ToJson(ReadRoundSmokeRaw()) == JsonUtility.ToJson(before) && RngSmokeCandidate() == candidate &&
                    !autoplay && !sourceTableInputEnabled && sourceRoundEndCount == 0 && rngSmokeCompleteSpins == 0 &&
                    rngSmokeRecordPockets == 0 && rngSmokePocketCommits == 0 && rngSmokeSuitCommits == 0,
                    "faulted live frames resumed effects");
            }
            if (rngSmokeCase == "fault_scope" || rngSmokeCase == "fault_replace") yield return RngSmokeCaptureFault();
            RngSmokeExit("ExpectedFault", "expected_fault");
        }

        private string RngSmokeImagePath(string shot)
        {
            string prefix = RoundSmokeArgument("-rouletteRngReceiptCapturePrefix", false);
            if (string.IsNullOrEmpty(prefix)) return "";
            string path = Path.GetFullPath(prefix + "_" + shot + ".png");
            RequireRngSmoke(!File.Exists(path), "capture already exists");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path;
        }

        private void RngSmokeReportImage(string shot, string path)
        {
            RequireRngSmoke(File.Exists(path) && new FileInfo(path).Length > 0, "capture missing");
            Debug.Log("ROULETTE_RNG_RECEIPT_CAPTURE: case=" + rngSmokeCase + " phase=" + rngSmokePhase + " shot=" + shot +
                " path=" + path + " sha256=" + RngSmokeBytesSha(File.ReadAllBytes(path)));
        }

        private void RngSmokeCapture(string shot)
        {
            string path = RngSmokeImagePath(shot);
            if (path == "") return;
            RefreshPortraitUi(); CaptureActiveScreenOffscreen(path); RngSmokeReportImage(shot, path);
        }

        private IEnumerator RngSmokeCaptureFault()
        {
            string path = RngSmokeImagePath("fault");
            if (path == "") yield break;
            yield return null;
            ScreenCapture.CaptureScreenshot(path, 2);
            float deadline = Time.realtimeSinceStartup + 10f;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < deadline) yield return null;
            RngSmokeReportImage("fault", path);
        }

        private void RngSmokeExit(string point, string mode)
        {
            RequireRngSmoke(!rngSmokeExiting, "duplicate exit");
            rngSmokeExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var proof = new RngReceiptSmokeProof {
                testCase = rngSmokeCase, phase = rngSmokePhase, checkpoint = point, exitMode = mode,
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess, autoPath = PcAutoPlayStatePath,
                previousProofSha256 = rngSmokePreviousHash ?? "", raw = ReadRoundSmokeRaw(), target = rngSmokeTarget,
                rngCandidateBase64 = RngSmokeCandidate(), oracleSha256 = rngSmokeOracleHash,
                archiveCount = recordedRoundPages.Count, sourceState = (int)sourceGameplayState, sourcePhase = (int)sourceRoundPhase,
                ends = sourceRoundEndCount, pocketAttempts = rngSmokePocketAttempts, suitAttempts = rngSmokeSuitAttempts,
                pocketCommits = rngSmokePocketCommits, suitCommits = rngSmokeSuitCommits,
                completeSpinCalls = rngSmokeCompleteSpins, recordPocketCalls = rngSmokeRecordPockets, checks = rngSmokeChecks,
                fault = pcRoundLifecycleStorageFaulted, faultOperation = pcAutoPlayStorageFaultOperation ?? "", hasSession = session != null,
                active = autoplay, tableEnabled = sourceTableInputEnabled, configuredCheckPending = configuredAutoPlayCheckPending,
                fixtureMutation = rngSmokeFixture, targetCommitted = rngSmokeTargetCommitted, targetDrawOrdinal = rngSmokeOrdinal,
                gameStartTimestamp = gameStartTimestamp, selectedDemo = demoOutcome == null ? 0 : demoOutcome.SelectedOption,
                archiveSchema = pcRoundArchiveSchemaVersion, visibleHistory = CommittedHistoryCount(),
                paymentJson = pcGamePayment == null ? "" : JsonUtility.ToJson(pcGamePayment),
                featureJson = pcDoubleFeatureNv == null ? "" : JsonUtility.ToJson(pcDoubleFeatureNv),
                roundPayoutPending = session is RouletteSession && ((RouletteSession)session).RoundPayoutPending,
                displayedCredit = session == null ? -1 : DisplayedCredit, displayedPaid = session == null ? -1 : DisplayedPaid,
                checkpoints = rngSmokeEvents.ToArray()
            };
            if (session != null)
            {
                proof.credit = session.Credit; proof.wagered = session.Wagered; proof.returned = session.Returned;
                proof.rounds = session.Rounds; proof.wins = session.Wins; proof.denomination = session.Denomination;
                proof.remainder = ((RouletteSession)session).MoneyRemainderCents; proof.totalBet = session.TotalBet;
                proof.spinPending = session.SpinPending; proof.inDouble = session.InDoubleUp;
                proof.doubleStake = session.DoubleStake; proof.doubleRounds = session.DoubleRounds;
            }
            proof.rawJson = JsonUtility.ToJson(proof.raw); proof.rawSha256 = RoundSmokeSha(proof.rawJson);
            proof.rngCandidateSha256 = proof.rngCandidateBase64 == "missing" ? "" : RngSmokeBytesSha(Convert.FromBase64String(proof.rngCandidateBase64));
            Directory.CreateDirectory(Path.GetDirectoryName(rngSmokeOutput));
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            using (var stream = new FileStream(rngSmokeOutput, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(encoded);
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            Debug.Log("ROULETTE_RNG_RECEIPT_PASSED: case=" + rngSmokeCase + " phase=" + rngSmokePhase + " point=" + point +
                " mode=" + mode + " checks=" + rngSmokeChecks + " credit=" + proof.credit + " rounds=" + proof.rounds + " ends=" + proof.ends +
                " pockets=" + proof.pocketCommits + " suits=" + proof.suitCommits + " completeSpin=" + proof.completeSpinCalls +
                " recordPocket=" + proof.recordPocketCalls + " fault=" + proof.fault.ToString().ToLowerInvariant() + " proof=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_RNG_RECEIPT_EXIT: api=" + (mode == "checkpoint" ? "TerminateProcess" : "Application.Quit") + " code=0 mode=" + mode);
            if (mode == "checkpoint")
            {
                RequireRngSmoke(rngSmokeConfigured && recordedHistoryKey.Contains(".Test."), "native cut lacks isolated flag");
                TerminateProcess(GetCurrentProcess(), 0);
                throw new InvalidOperationException("RNG receipt current-process cut returned");
            }
            Application.Quit(0);
        }
    }
}
