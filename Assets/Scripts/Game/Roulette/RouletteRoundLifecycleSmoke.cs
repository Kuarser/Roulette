using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // This observer survives the production fail-stop's StopAllCoroutines.
    public sealed class RouletteRoundLifecycleSmokeHost : MonoBehaviour { }

    public sealed partial class RouletteGame
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(IntPtr process, uint exitCode);

        private sealed class RoundLifecycleSmokeOutcome : IRouletteOutcomeSource, IRouletteOutcomeMapping, IDisposable
        {
            private readonly IRouletteOutcomeSource inner;
            internal int Pockets, Suits, Pocket = 27, Suit = 3;
            internal RoundLifecycleSmokeOutcome(IRouletteOutcomeSource source) { inner = source; }
            public int ResolvePocket(int raw) { return Pocket; }
            public int ResolveDoubleSuit(int raw) { return Suit; }
            public int DrawPocket()
            {
                int actual = inner.DrawPocket();
                ++Pockets;
                Debug.Log("ROULETTE_ROUND_NV_DRAW: kind=pocket count=" + Pockets +
                    " actual=" + actual + " fixture=" + Pocket);
                return Pocket;
            }
            public int DrawDoubleSuit()
            {
                int actual = inner.DrawDoubleSuit();
                ++Suits;
                Debug.Log("ROULETTE_ROUND_NV_DRAW: kind=suit count=" + Suits +
                    " actual=" + actual + " fixture=" + Suit);
                return Suit;
            }
            public void Dispose()
            { var disposable = inner as IDisposable; if (disposable != null) disposable.Dispose(); }
        }

        [Serializable]
        private sealed class RoundLifecycleSmokeRaw
        {
            public string envelope, pockets, pages, pending, staged, settledUndo, creditEvent, cleared;
            public string rng, auto, neighbor;
        }

        [Serializable]
        private sealed class RoundLifecycleSmokeReceipt
        {
            public int schema = 2;
            public string testCase, phase, checkpoint, exitMode, historyKey, rngPath, autoPath;
            public string previousReceiptSha256, rawSha256, financialArchive, result, undo, autoConfig;
            public RoundLifecycleSmokeRaw raw;
            public PcRoundLifecycleRecord lifecycle;
            public long credit, wagered, returned, rounds, wins, denomination, remainder, totalBet;
            public int archiveCount, pocketsDrawn, suitsDrawn, completeSpinCalls, recordPocketCalls, ends, checks;
            public int sourceState, sourcePhase, gameDelayMilliseconds, selectedDemo;
            public long gameStartTimestamp, resultDelayStart, resultDelayDeadline;
            public bool hasSession, spinPending, inDouble, doubleAvailable, spinning, rack, active;
            public bool configuredCheckPending, muted, fault, tableEnabled, optionalUndoFault;
            public bool sawRack, sawHighWait;
            public long observedHighStart, observedHighDeadline;
            public int archiveSchema, visibleHistory;
            public string paymentJson, featureJson;
            public bool roundPayoutPending;
            public long displayedCredit, displayedPaid;
            public int pendingPocket;
            public string faultOperation;
            public string[] checkpoints;
        }

        private static readonly Dictionary<string, string[]> RoundLifecycleSmokePhases =
            new Dictionary<string, string[]>(StringComparer.Ordinal) {
                { "loss_chain", new[] { "save", "await", "ended", "running", "complete", "reenter" } },
                { "win", new[] { "save", "load" } },
                { "game_delay", new[] { "save", "load" } },
                { "double_take", new[] { "offer", "feature", "final", "load" } },
                { "double_loss", new[] { "save", "load" } },
                { "double_cap", new[] { "save", "load" } },
                { "high_win", new[] { "save", "await", "load" } },
                { "high_win_purge", new[] { "save", "load" } },
                { "end_partial", new[] { "save", "auto", "turbo", "off", "purge", "ended", "complete" } },
                { "demo_only", new[] { "save", "load" } },
                { "idle_deposit", new[] { "save", "saved", "applied", "purged", "complete", "reenter", "collect_restore" } },
                { "successor", new[] { "save", "pending", "settled", "complete" } },
                { "bet_restore", new[] { "save", "play", "load" } },
                { "fault_schema", new[] { "save", "load" } },
                { "fault_financial", new[] { "save", "load" } },
                { "fault_auto", new[] { "save", "load" } },
                { "fault_successor", new[] { "save", "load" } },
                { "fault_end_save", new[] { "run", "recover" } },
                { "fault_idle_save", new[] { "run", "recover" } }
            };

        private RoundLifecycleSmokeOutcome roundLifecycleSmokeOutcome;
        private RoundLifecycleSmokeReceipt roundSmokePrevious;
        private RoundLifecycleSmokeRaw roundSmokeBoot, roundSmokeFaultBefore;
        private readonly List<string> roundSmokeEvents = new List<string>();
        private string roundSmokeCase, roundSmokePhase, roundSmokeOutput, roundSmokePreviousHash, roundSmokeTarget;
        private int roundSmokeChecks, roundSmokeCompleteSpins, roundSmokeRecordPockets, roundSmokeInjectedWrites;
        private bool roundSmokeConfigured, roundSmokeStarted, roundSmokeExiting, roundSmokeUndoFailed;
        private bool roundSmokeSawRack, roundSmokeSawHighWait, roundSmokeArmedCut;
        private float roundSmokeStartTime;
        private long roundSmokeHighStart, roundSmokeHighDeadline;
        private bool roundSmokeCapturedRack;

        private static string RoundSmokeArgument(string flag, bool required = true)
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, flag);
            if (at < 0 || at + 1 >= args.Length)
            {
                if (!required) return "";
                throw new InvalidOperationException("Round NV missing argument " + flag);
            }
            return args[at + 1];
        }

        private void RequireRoundSmoke(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Round NV " + roundSmokeCase + "/" +
                roundSmokePhase + ": " + message);
            ++roundSmokeChecks;
        }

        private static string RoundSmokeSha(string text)
        {
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""))).Replace("-", "");
        }

        private RoundLifecycleSmokeRaw ReadRoundSmokeRaw()
        {
            return new RoundLifecycleSmokeRaw {
                envelope = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1", ""),
                pockets = PlayerPrefs.GetString(recordedHistoryKey, ""),
                pages = PlayerPrefs.GetString(recordedHistoryKey + ".Pages", ""),
                pending = PlayerPrefs.GetString(PendingSpinKey, ""),
                staged = PlayerPrefs.GetString(StagedBetKey, ""),
                settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, ""),
                creditEvent = PlayerPrefs.GetString(PcCreditKey, ""),
                cleared = PlayerPrefs.GetString(GameResultsClearedKey, ""),
                rng = File.Exists(PcRngStateStore.PathForCurrentProcess) ? DemoCleanupRngBytes() : "missing", auto = AutoNvBytes(),
                neighbor = PlayerPrefs.GetInt(NeighborSelectionKey, -2).ToString(CultureInfo.InvariantCulture)
            };
        }

        private IRouletteOutcomeSource CreateRoundLifecycleSmokeOutcome(IRouletteOutcomeSource platform)
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteRoundLifecycleSmoke") < 0) return platform;
            roundLifecycleSmokeOutcome = new RoundLifecycleSmokeOutcome(platform);
            if (roundSmokeCase == "loss_chain" || roundSmokeCase == "game_delay" ||
                roundSmokeCase == "idle_deposit" || roundSmokeCase == "successor" ||
                roundSmokeCase == "demo_only" || roundSmokeCase == "end_partial" ||
                roundSmokeCase == "fault_auto" || roundSmokeCase == "fault_successor" ||
                roundSmokeCase == "fault_end_save" || roundSmokeCase == "fault_idle_save")
                roundLifecycleSmokeOutcome.Pocket = 16;
            return roundLifecycleSmokeOutcome;
        }

        private void ConfigureRoundLifecycleSmokeHooks()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteRoundLifecycleSmoke") < 0) return;
            RequireRoundSmoke(!roundSmokeConfigured, "hooks configured more than once");
            roundSmokeConfigured = true;
            roundSmokeCase = RoundSmokeArgument("-rouletteRoundLifecycleSmoke");
            roundSmokePhase = RoundSmokeArgument("-rouletteRoundLifecyclePhase");
            string[] phases;
            RequireRoundSmoke(RoundLifecycleSmokePhases.TryGetValue(roundSmokeCase, out phases) &&
                phases.Contains(roundSmokePhase), "unknown case/phase");
            RequireRoundSmoke(!temporaryHistory && recordedHistoryKey.Contains(".Test.") &&
                RoundSmokeArgument("-rouletteVolumeTestKey").StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires nonfresh isolated history, volume and RNG keys");
            roundSmokeOutput = Path.GetFullPath(RoundSmokeArgument("-rouletteRoundLifecycleReceipt"));
            RequireRoundSmoke(Path.IsPathRooted(roundSmokeOutput) && !File.Exists(roundSmokeOutput), "receipt output already exists");
            string previous = RoundSmokeArgument("-rouletteRoundLifecyclePreviousReceipt", false);
            int phaseIndex = Array.IndexOf(phases, roundSmokePhase);
            RequireRoundSmoke((phaseIndex == 0) == string.IsNullOrEmpty(previous), "wrong previous-receipt argument");
            if (!string.IsNullOrEmpty(previous))
            {
                string encoded = File.ReadAllText(previous, Encoding.UTF8);
                roundSmokePreviousHash = RoundSmokeSha(encoded);
                roundSmokePrevious = JsonUtility.FromJson<RoundLifecycleSmokeReceipt>(encoded);
                RequireRoundSmoke(roundSmokePrevious != null && (roundSmokePrevious.schema == 1 || roundSmokePrevious.schema == 2) &&
                    roundSmokePrevious.testCase == roundSmokeCase && roundSmokePrevious.phase == phases[phaseIndex - 1] &&
                    roundSmokePrevious.historyKey == recordedHistoryKey && roundSmokePrevious.raw != null &&
                    roundSmokePrevious.rawSha256 == RoundSmokeSha(JsonUtility.ToJson(roundSmokePrevious.raw)),
                    "previous receipt has wrong case, predecessor, namespace or raw digest");
            }
            roundSmokeBoot = ReadRoundSmokeRaw();
            if (roundSmokePrevious != null)
                RequireRoundSmoke(JsonUtility.ToJson(roundSmokeBoot) == JsonUtility.ToJson(roundSmokePrevious.raw),
                    "cold persistent bytes differ from the preceding closed receipt");
            pcLifecycleCheckpoint = RoundSmokeCheckpoint;
            pcLifecycleBeforeWrite = RoundSmokeBeforeWrite;
            pcAutoCompleteCheckpoint = point => RoundSmokeCheckpoint("AutoComplete:" + point);
            pcLifecycleForceFinancialRestoreFalse = roundSmokeCase == "fault_financial" && roundSmokePhase == "load";
            roundSmokeTarget = RoundSmokeTargetForPhase();
            roundSmokeArmedCut = roundSmokePrevious != null;
            Debug.Log("ROULETTE_ROUND_NV_BOOT: case=" + roundSmokeCase + " phase=" + roundSmokePhase +
                " raw=" + RoundSmokeSha(JsonUtility.ToJson(roundSmokeBoot)) + " previous=" + (roundSmokePreviousHash ?? "none"));
        }

        private string RoundSmokeTargetForPhase()
        {
            if (roundSmokeCase == "loss_chain")
            {
                if (roundSmokePhase == "save") return "RoundOpenSaved";
                if (roundSmokePhase == "await") return "AwaitEndSaved";
                if (roundSmokePhase == "ended") return "EndedSaved";
                if (roundSmokePhase == "running") return "IdleBeginSaved";
            }
            if (roundSmokePhase == "save" && (
                roundSmokeCase == "fault_schema" || roundSmokeCase == "fault_financial")) return "RoundOpenSaved";
            if (roundSmokeCase == "end_partial")
            {
                string[] points = { "EndPreparedSaved", "EndAuto0", "EndTurboFalse", "EndOffFalse", "EndPurged", "EndedSaved", "" };
                return points[Array.IndexOf(RoundLifecycleSmokePhases[roundSmokeCase], roundSmokePhase)];
            }
            if (roundSmokeCase == "fault_auto" && roundSmokePhase == "save") return "EndPreparedSaved";
            if (roundSmokeCase == "demo_only" && roundSmokePhase == "save") return "EndedSaved";
            if (roundSmokeCase == "high_win" && roundSmokePhase == "await") return "AwaitEndSaved";
            if ((roundSmokeCase == "high_win" || roundSmokeCase == "high_win_purge") && roundSmokePhase == "save") return "RoundOpenSaved";
            if (roundSmokeCase == "idle_deposit")
            {
                if (roundSmokePhase == "save") return "IdleBeginSaved";
                if (roundSmokePhase == "saved") return "IdleDepositSaved";
                if (roundSmokePhase == "applied") return "IdleDepositDoneSaved";
                if (roundSmokePhase == "purged") return "IdleCheckPurgedSaved";
            }
            if (roundSmokeCase == "successor")
            {
                if (roundSmokePhase == "save") return "IdlePrepareFinishedSaved";
                if (roundSmokePhase == "pending") return "PendingSaved";
                if (roundSmokePhase == "settled") return "RoundOpenSaved";
            }
            if (roundSmokeCase == "bet_restore" && roundSmokePhase == "play") return "RoundOpenSaved";
            if (roundSmokeCase == "fault_successor" && roundSmokePhase == "save") return "PendingSaved";
            bool legacyImmediate = roundSmokePrevious != null && RoundSmokeArchiveVersion(roundSmokePrevious.raw.envelope) == 1;
            if (roundSmokeCase == "double_take" && roundSmokePhase == "final") return legacyImmediate ?
                "DoubleTakeSaved" : "AutoComplete:Feature:FeatureCompletedSaved0";
            if ((roundSmokeCase == "double_loss" || roundSmokeCase == "double_cap") && roundSmokePhase == "save")
                return "AutoComplete:Feature:FeatureCompletedSaved0";
            return "";
        }

        private void RoundSmokeBeforeWrite(string boundary)
        {
            bool undo = roundSmokeCase == "loss_chain" && roundSmokePhase == "await" && boundary == "SettledUndo";
            bool end = roundSmokeCase == "fault_end_save" && roundSmokePhase == "run" && boundary == "Ended";
            bool idle = roundSmokeCase == "fault_idle_save" && roundSmokePhase == "run" && boundary == "IdleBegin";
            if (!undo && !end && !idle) return;
            ++roundSmokeInjectedWrites;
            RequireRoundSmoke(roundSmokeInjectedWrites == 1, "write injection was reached more than once");
            roundSmokeFaultBefore = ReadRoundSmokeRaw();
            roundSmokeUndoFailed = undo;
            Debug.Log("ROULETTE_ROUND_NV_INJECT: boundary=" + boundary + " beforeSetString=true optionalUndo=" + undo);
            throw new IOException("Round NV isolated before-write fixture at " + boundary);
        }

        private void RoundSmokeCheckpoint(string boundary)
        {
            roundSmokeEvents.Add(boundary);
            if (boundary == "CompleteSpin") ++roundSmokeCompleteSpins;
            if (boundary == "RecordPocket") ++roundSmokeRecordPockets;
            if (roundSmokeCase == "end_partial" && roundSmokePhase == "save" && boundary == "AwaitEndSaved")
            {
                RequireRoundSmoke(pcPlayerTransactionActive && HasTrustedLatestLifecycle &&
                    latestRoundLifecycle.roundStage == PcRoundStage.AwaitEnd && latestRoundLifecycle.endIntent == null,
                    "End transaction fixture must follow real PLAY and precede its saved intent");
                pcPlayerTransactionActive = false;
                Debug.Log("ROULETTE_ROUND_NV_PLATFORM_FIXTURE: transactionActive=false beforeEndIntent=true");
            }
            if (!roundSmokeArmedCut || string.IsNullOrEmpty(roundSmokeTarget) || boundary != roundSmokeTarget) return;
            RequireRoundSmoke(!pcRoundLifecycleStorageFaulted, "successful checkpoint followed a lifecycle fault");
            RequireRoundSmoke(HasTrustedLatestLifecycle, "checkpoint has no trusted lifecycle");
            if (roundSmokePrevious != null && roundSmokeCase != "successor" && roundSmokeCase != "bet_restore" &&
                !roundSmokeCase.StartsWith("double_", StringComparison.Ordinal) && boundary != "CollectCompletedSaved")
                RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            if (boundary == "RoundOpenSaved")
            {
                RequireRoundSmoke(PcPaymentTracked && latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen &&
                    pendingSpin != null && pendingSpin.id == latestRoundLifecycle.spinJournalId &&
                    PlayerPrefs.GetString(PendingSpinKey, "") != "" &&
                    roundLifecycleSmokeOutcome.Pockets == (roundSmokeCase == "successor" ? 0 : 1) &&
                    roundSmokeCompleteSpins == 1 && roundSmokeRecordPockets == 1,
                    "first evaluated archive lacks its same-id pending or one real evaluation");
                if (PcPaymentTracked)
                    RequireRoundSmoke(!pcGamePayment.prepared && !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                        ((RouletteSession)session).RoundPayoutPending &&
                        pcGamePayment.creditBefore == pendingSpin.creditBefore - pendingSpin.wager &&
                        session.Credit == pcGamePayment.creditBefore && recordedRoundPages[0].creditAfter == session.Credit &&
                        pcGamePayment.baseAward == session.LastResult.Returned &&
                        pcGamePayment.priorHistoryCount == pendingSpin.baseHistoryCount &&
                        CommittedHistoryCount() == pendingSpin.baseHistoryCount,
                        "v3 RoundOpen must record evaluated totals without paying or publishing");
                else RequireRoundSmoke(pcRoundArchiveSchemaVersion == 1 && pcGamePayment == null &&
                    !((RouletteSession)session).RoundPayoutPending &&
                    session.Credit == pendingSpin.creditBefore - pendingSpin.wager + session.LastResult.Returned,
                    "legacy RoundOpen no longer has its original paid-base balance");
            }
            if (roundSmokeCase == "loss_chain" && roundSmokePhase == "await")
                RequireRoundSmoke(roundSmokeUndoFailed && pendingSpin == null &&
                    PlayerPrefs.GetString(PendingSpinKey, "") == roundSmokePrevious.raw.pending &&
                    PlayerPrefs.GetString(SettledTableUndoKey, "") == roundSmokePrevious.raw.settledUndo,
                    "failed optional Undo did not retain the sole durable pending copy");
            if (roundSmokeCase == "loss_chain" && roundSmokePhase == "ended")
                RequireRoundSmoke(pendingSpin == null && PlayerPrefs.GetString(PendingSpinKey, "") == "" &&
                    !string.IsNullOrEmpty(PlayerPrefs.GetString(SettledTableUndoKey, "")), "second startup failed Undo handoff");
            if (roundSmokeCase == "fault_schema") PrepareRoundSmokeSchemaFault();
            else if (roundSmokeCase == "fault_auto")
                configuredAutoPlay.SetSecsToAutoCompleteWarning(configuredAutoPlay.SecsToAutoCompleteWarning + 1u);
            else if (roundSmokeCase == "fault_successor")
            {
                RequireRoundSmoke(pendingSpin != null && !string.IsNullOrEmpty(pendingSpin.baseLatestRoundId), "fault seed has no successor base");
                string raw = PlayerPrefs.GetString(PendingSpinKey, "");
                string changed = raw.Replace(pendingSpin.baseLatestRoundId, Guid.NewGuid().ToString("N"));
                RequireRoundSmoke(changed != raw, "successor fault did not change raw base");
                PlayerPrefs.SetString(PendingSpinKey, changed);
                PlayerPrefs.Save();
                Debug.Log("ROULETTE_ROUND_NV_FIXTURE_MUTATION: successorBase=true isolatedKey=true");
            }
            if (roundSmokeCase == "end_partial") RequireRoundSmokeEndPrefix(boundary);
            if (roundSmokeCase == "high_win" && roundSmokePhase == "await")
                RequireRoundSmoke(roundSmokeSawHighWait && roundSmokeHighStart > 0 &&
                    roundSmokeHighDeadline - roundSmokeHighStart == System.Diagnostics.Stopwatch.Frequency / 2 &&
                    System.Diagnostics.Stopwatch.GetTimestamp() >= roundSmokeHighDeadline && !autoplay,
                    "cold high win omitted its complete configured 500ms gate");
            if (roundSmokeCase == "idle_deposit" && boundary != "CollectCompletedSaved") RequireRoundSmokeDepositBoundary(boundary);
            if (boundary == "CollectCompletedSaved")
                RequireRoundSmoke(session.Credit == 0 && session.TotalBet == 0 && ReadPcCreditEvent().kind == "collect" &&
                    ReadPcCreditEvent().phase == 3 && latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                    latestRoundLifecycle.idleEntry.consumedCollectEventId != ReadPcCreditEvent().id,
                    "cashout gap cut was not after money/phase3 and before new Idle");
            if (boundary == "DoubleTerminalSaved" || boundary == "DoubleTakeSaved")
            {
                bool lost = roundSmokeCase == "double_loss";
                RequireRoundSmoke(pcRoundArchiveSchemaVersion == 1 && pcGamePayment == null && pcDoubleFeatureNv == null &&
                    !((RouletteSession)session).RoundPayoutPending &&
                    !session.InDoubleUp && !session.DoubleAvailable && sourceRoundEndCount == 0 &&
                    latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen && recordedRoundPages[0].awaitingFinalPayout &&
                    recordedRoundPages[0].doubleUp.complete && session.Credit == (lost ? 999 : 1071) &&
                    session.Returned == (lost ? 0 : 72) && roundLifecycleSmokeOutcome.Pockets == (roundSmokeCase == "double_take" ? 0 : 1) &&
                    roundLifecycleSmokeOutcome.Suits == (lost ? 2 : roundSmokeCase == "double_cap" ? 1 : 0),
                    "terminal feature save lacks its exact already-paid unresolved lifecycle boundary");
            }
            if (boundary == "AutoComplete:Feature:FeatureCompletedSaved0") RequireRoundSmokeNative0();
            if (roundSmokeCase == "successor" || roundSmokeCase == "bet_restore") RequireRoundSmokeSuccessorBoundary(boundary);
            RoundSmokeExit(boundary, "checkpoint");
        }

        private void PrepareRoundSmokeSchemaFault()
        {
            string raw = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1", "");
            int version = RoundSmokeArchiveVersion(raw);
            string header = "\"lifecycleSchemaVersion\":" + version.ToString(CultureInfo.InvariantCulture);
            RequireRoundSmoke((version == 1 || version == 2 || version == 3) && raw.Contains(header),
                "fault seed is not a generated supported archive");
            string corrupted = raw.Replace(header, "\"lifecycleSchemaVersion\":99");
            RequireRoundSmoke(corrupted != raw, "schema fault did not change its declared header");
            PlayerPrefs.SetString(recordedHistoryKey + ".RoundDetails.v1", corrupted);
            PlayerPrefs.Save();
            Debug.Log("ROULETTE_ROUND_NV_FIXTURE_MUTATION: unknownSchema=99 isolatedKey=true");
        }

        private void StartRoundLifecycleSmoke()
        {
            RequireRoundSmoke(roundSmokeConfigured && !roundSmokeStarted, "observer was not configured exactly once");
            roundSmokeStarted = true;
            roundSmokeStartTime = Time.realtimeSinceStartup;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var host = new GameObject("Round NV acceptance observer").AddComponent<RouletteRoundLifecycleSmokeHost>();
            host.StartCoroutine(RoundLifecycleSmoke());
        }

        private IEnumerator RoundLifecycleSmoke()
        {
            bool expectedFault = roundSmokeCase.StartsWith("fault_", StringComparison.Ordinal) &&
                (roundSmokePhase == "load" || roundSmokePhase == "run");
            if (pcRoundLifecycleStorageFaulted)
            {
                RequireRoundSmoke(expectedFault, "unexpected fault before Start");
                yield return RoundSmokeObserveFault();
                yield break;
            }
            RequireRoundSmoke(session != null && sourceInputModule != null && roundLifecycleSmokeOutcome != null &&
                !pcAutoPlayStorageFaulted && !autoplay && sourceRoundEndCount == 0 && SourcePlayerInputs.Count == 0 &&
                !ideckSecondDisplay && !topperDedicatedDisplay && pcPlayerVolumeKey.Contains(".Test."),
                "startup is not an isolated inactive single-display process");
            RequireRoundSmoke(demoShowMode == (roundSmokeCase == "demo_only" && roundSmokePhase == "load" ? 0 : 1),
                "wrong Demo availability for case");
            if (roundSmokePrevious != null)
            {
                RequireRoundSmokePaidUnchanged(roundSmokePrevious, true);
                RequireRoundSmoke(gameStartTimestamp == 0 && !autoplay &&
                    demoOutcome.SelectedOption == 0, "cold runtime inherited GameDelay/active/selected math state");
                RequireDemoSelectedText("", demoShowMode > 0, "Round NV cold constructor");
            }
            if (roundSmokePrevious == null)
            {
                RequireRoundSmoke(session.Credit == 1000 && session.Rounds == 0 && session.TotalBet == 0 &&
                    recordedRoundPages.Count == 0 && !HasTrustedLatestLifecycle &&
                    roundSmokeBoot.envelope == "" && roundSmokeBoot.pending == "", "seed requires unused keys");
                yield return StartGameLiveFrames(3);
                RequireRoundSmoke(sourceGameplayState == SourceGameplayState.Idle && !configuredAutoPlayCheckPending,
                    "real initial Idle did not finish");
            }
            if (roundSmokeCase == "loss_chain") yield return RoundSmokeLossChain();
            else if (roundSmokeCase == "win") yield return RoundSmokeWin();
            else if (roundSmokeCase == "end_partial") yield return RoundSmokeEndPartial();
            else if (roundSmokeCase == "demo_only") yield return RoundSmokeDemoOnly();
            else if (roundSmokeCase.StartsWith("fault_", StringComparison.Ordinal)) yield return RoundSmokeFaultCase();
            else yield return RoundSmokeExtendedCase();
            RequireRoundSmoke(roundSmokeExiting, "case returned without a verified receipt and exit");
        }

        private IEnumerator RoundSmokeLossChain()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
            }
            if (roundSmokePhase == "complete" || roundSmokePhase == "reenter")
            {
                string entry = latestRoundLifecycle.idleEntry.idleEntryId;
                yield return RoundSmokeUntilIdle();
                RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
                RequireRoundSmoke(sourceRoundEndCount == 0 && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                    (roundSmokePhase == "complete" ? latestRoundLifecycle.idleEntry.idleEntryId == entry :
                        latestRoundLifecycle.idleEntry.idleEntryId != entry), "Idle repeated End or used the wrong entry identity");
                yield return RoundSmokeStableFrames(4);
                RoundSmokeExit("IdleStable", "complete");
                yield break;
            }
            yield return RoundSmokeAwaitCut();
        }

        private IEnumerator RoundSmokeWin()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                yield return RoundSmokePlay();
                yield return RoundSmokeWait(() => winRackActive && latestRoundLifecycle != null &&
                    latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen, "real win rack before cut");
                RequireRoundSmoke(PcPaymentTracked && session.Credit == 999 && session.Returned == 36 &&
                    !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                    ((RouletteSession)session).RoundPayoutPending && CommittedHistoryCount() == 0 && sourceRoundEndCount == 0,
                    "new win rack changed the unpaid wallet or published its history");
                RoundSmokeExit("WinRack", "checkpoint");
                yield break;
            }
            RequireRoundSmoke(roundSmokePrevious.credit == (PcPaymentTracked ? 999 : 1035) && roundSmokePrevious.returned == 36 &&
                latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen, "win seed differs from its explicit settlement basis");
            yield return RoundSmokeUntilIdle();
            RequireRoundSmoke(roundSmokeSawRack && sourceRoundEndCount == 1 &&
                !configuredAutoPlay.AutoPlayOffWhenGameFinished, "cold win failed its real rerack or End consumption");
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            yield return RoundSmokeStableFrames(4);
            CaptureRoundSmokeImage("final");
            RoundSmokeExit("WinRestored", "complete");
        }

        private IEnumerator RoundSmokeEndPartial()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                configuredAutoPlay.SetOptions(true, true, true, true, true, 2);
                configuredAutoPlay.SetSecsToAutoCompleteWarning(17);
                configuredAutoPlay.SetSecsToAutoCompleteStart(29);
                configuredAutoPlay.SetAutoCompleteShowCountdown(true);
                configuredAutoPlay.SetDisableWhenGameFinished(true);
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(false);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
            }
            else
            {
                RequireRoundSmoke(pcPlayerTransactionActive && latestRoundLifecycle.endIntent != null &&
                    latestRoundLifecycle.endIntent.finishAuto && !latestRoundLifecycle.endIntent.transactionActive &&
                    !latestRoundLifecycle.endIntent.autoBefore.offWhenFinished,
                    "End test did not restore the precommitted changed-environment decision");
                if (roundSmokePhase == "purge")
                    SourcePlayerInputs.Enqueue(0x200010, () => { throw new InvalidOperationException("End purge dispatched fixture input"); });
                if (roundSmokePhase == "complete")
                {
                    yield return RoundSmokeUntilIdle();
                    RequireRoundSmoke(sourceRoundEndCount == 0, "already Ended process ran End again");
                    RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
                    yield return RoundSmokeStableFrames(4);
                    RoundSmokeExit("EndPartialComplete", "complete");
                    yield break;
                }
            }
            yield return RoundSmokeAwaitCut();
        }

        private IEnumerator RoundSmokeDemoOnly()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                yield return StartGameChooseDemo(1);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
                yield return RoundSmokeAwaitCut();
            }
            string config = AutoNvConfig(configuredAutoPlay);
            yield return RoundSmokeUntilIdle();
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            RequireRoundSmoke(configuredAutoPlay.DemoOnly && configuredAutoPlay.AutoPlay != 0 &&
                AutoNvConfig(configuredAutoPlay) == config && !latestRoundLifecycle.idleEntry.eligible &&
                !latestRoundLifecycle.idleEntry.depositRequired && ReadPcCreditEvent() == null && sourceRoundEndCount == 0,
                "DemoOnly cold entry funded, reset config or replayed End");
            RoundSmokeExit("DemoOnlyRejected", "complete");
        }

        private IEnumerator RoundSmokeFaultCase()
        {
            if (roundSmokePhase == "save" || roundSmokePhase == "run")
            {
                yield return RoundSmokePlaceBet(1);
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                if (roundSmokeCase == "fault_auto")
                    configuredAutoPlay.SetSecsToAutoCompleteWarning(17);
                if (roundSmokeCase == "fault_successor")
                {
                    yield return RoundSmokePlay();
                    yield return RoundSmokeUntilIdle();
                }
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
                if (roundSmokePhase == "save") yield return RoundSmokeAwaitCut();
            }
            if (roundSmokePhase == "recover")
            {
                yield return RoundSmokeUntilIdle();
                RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
                RoundSmokeExit("FaultRecovered", "complete");
                yield break;
            }
            else yield return RoundSmokeObserveFault();
        }

        private IEnumerator RoundSmokeExtendedCase()
        {
            if (roundSmokeCase == "game_delay") yield return RoundSmokeGameDelay();
            else if (roundSmokeCase.StartsWith("double_", StringComparison.Ordinal)) yield return RoundSmokeFeature();
            else if (roundSmokeCase.StartsWith("high_win", StringComparison.Ordinal)) yield return RoundSmokeHighWin();
            else if (roundSmokeCase == "idle_deposit") yield return RoundSmokeIdleDeposit();
            else if (roundSmokeCase == "successor") yield return RoundSmokeSuccessor();
            else if (roundSmokeCase == "bet_restore") yield return RoundSmokeBetRestore();
            else RequireRoundSmoke(false, "no implementation for case");
        }

        private IEnumerator RoundSmokeGameDelay()
        {
            RequireRoundSmoke(gameDelayMilliseconds >= 15000, "GameDelay needs a real positive configuration >=15000ms");
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                yield return RoundSmokePlay();
                yield return RoundSmokeWait(() => latestRoundLifecycle != null &&
                    latestRoundLifecycle.roundStage == PcRoundStage.AwaitEnd &&
                    ProViewFlashingMessage().StartsWith("Please wait... ", StringComparison.Ordinal), "live AwaitEnd delay");
                for (int i = 0; i < 3; ++i)
                {
                    RequireRoundSmoke(sourceRoundEndCount == 0 && gameStartTimestamp != 0 &&
                        !OriginalRackClock.GameDelayExpired(gameStartTimestamp, System.Diagnostics.Stopwatch.GetTimestamp(),
                            gameDelayMilliseconds, System.Diagnostics.Stopwatch.Frequency) &&
                        latestRoundLifecycle.roundStage == PcRoundStage.AwaitEnd && !winRackActive && !ResultDisplayActive,
                        "live positive GameDelay did not hold the saved AwaitEnd");
                    yield return StartGameLiveFrames(1);
                }
                CaptureRoundSmokeImage("wait");
                RoundSmokeExit("AwaitEndLiveDelay", "checkpoint");
                yield break;
            }
            RequireRoundSmoke(latestRoundLifecycle.roundStage == PcRoundStage.AwaitEnd && gameStartTimestamp == 0,
                "cold delay was rearmed before processing");
            yield return RoundSmokeUntilIdle();
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            RequireRoundSmoke(sourceRoundEndCount == 1 && !roundSmokeSawRack && !roundSmokeSawHighWait &&
                !ProViewFlashingMessage().StartsWith("Please wait... ", StringComparison.Ordinal) && gameStartTimestamp == 0,
                "cold AwaitEnd replayed presentation or old delay");
            yield return RoundSmokePlay();
            RequireRoundSmoke(gameStartTimestamp > 0 && session.Credit == 998 && session.Rounds == 1 &&
                pendingSpin.baseLatestRoundId == latestRoundLifecycle.roundArchiveId && roundLifecycleSmokeOutcome.Pockets == 1,
                "new real commit did not arm a new GameDelay and exact predecessor");
            RoundSmokeExit("NewCommitDelayArmed", "checkpoint");
        }

        private IEnumerator RoundSmokeFeature()
        {
            if (roundSmokeCase == "double_cap")
                RequireRoundSmoke(auNationalStandardsEnabled && session.BaseMaximumWin == 80 && session.DoubleMaximumWin == 80 &&
                    ((RouletteSession)session).BaseSettlementDeferred,
                    "cap seed and cold load both require -rouletteBaseWinLimitCredits 80 and deferred base settlement");
            if (roundSmokePrevious == null)
            {
                if (roundSmokeCase == "double_cap")
                {
                    Debug.Log("ROULETTE_ROUND_NV_CONFIGURED_MAXIMUM: base=80 double=80 coldArgumentsRequired=true");
                }
                yield return RoundSmokePlaceBet(1);
                yield return RoundSmokePlay();
                yield return RoundSmokeWait(() => sourceRoundPhase == SourceRoundPhase.DoubleOffer && !winRackActive,
                    "unresolved real Double offer");
                if (roundSmokeCase == "double_take")
                {
                    yield return RoundSmokeHoldFeature(false);
                    RoundSmokeExit("DoubleOffer", "checkpoint");
                    yield break;
                }
                yield return RoundSmokeEnterFeature();
                roundSmokeArmedCut = true;
                yield return RoundSmokeGuess(true);
                if (roundSmokeCase == "double_loss")
                {
                    RequireRoundSmoke(session.InDoubleUp && session.DoubleStake == 72, "first red did not double to 72");
                    yield return RoundSmokeGuess(false);
                    RequireRoundSmoke(session.InDoubleUp && session.Credit == 999 && session.Returned == 36 &&
                        ((RouletteSession)session).RoundPayoutPending && ((RouletteSession)session).DoubleChoiceTerminal &&
                        PcPaymentTracked && !pcGamePayment.moneyApplied && !recordedRoundPages[0].doubleUp.complete &&
                        roundLifecycleSmokeOutcome.Suits == 2 && IsDoubleReveal,
                        "loss decision paid before native0 and Game payout");
                }
                else
                    RequireRoundSmoke(session.InDoubleUp && session.Credit == 999 && session.Returned == 36 &&
                        ((RouletteSession)session).RoundPayoutPending && ((RouletteSession)session).DoubleChoiceTerminal &&
                        PcPaymentTracked && !pcGamePayment.moneyApplied && !recordedRoundPages[0].doubleUp.complete &&
                        roundLifecycleSmokeOutcome.Suits == 1 && IsDoubleReveal && doubleRevealMaximum,
                        "maximum decision paid before native0 and Game payout");
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            if (roundSmokeCase == "double_take" && roundSmokePhase == "feature")
            {
                yield return RoundSmokeHoldFeature(false);
                yield return RoundSmokeEnterFeature();
                yield return RoundSmokeGuess(true);
                yield return RoundSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal, "feature after winning suit");
                RequireRoundSmoke(session.Credit == 999 && session.Returned == 36 && session.DoubleStake == 72 &&
                    session.DoubleRounds == 1 && roundLifecycleSmokeOutcome.Pockets == 0 && roundLifecycleSmokeOutcome.Suits == 1,
                    "restored offer did not perform exactly one new suit draw");
                yield return RoundSmokeHoldFeature(true);
                RoundSmokeExit("DoublePlaying", "checkpoint");
                yield break;
            }
            if (roundSmokeCase == "double_take" && roundSmokePhase == "final")
            {
                yield return RoundSmokeHoldFeature(true);
                yield return RoundSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal, "cold TAKE ready");
                RefreshPortraitUi();
                QueueStartGameButton(uiDoubleTakeWinButton, 291, true);
                yield return RoundSmokeWait(() => doubleCollectPending, "TAKE accepted before its wait");
                RequireRoundSmoke(session.Credit == 999 && sourceRoundEndCount == 0, "TAKE skipped its deferred collect");
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            if (pcRoundArchiveSchemaVersion >= 2) RequireRoundSmokeNative0();
            else RequireRoundSmoke(!session.InDoubleUp && !session.DoubleAvailable &&
                pcGamePayment == null && !((RouletteSession)session).RoundPayoutPending &&
                latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen, "legacy final feature was not a paid cold restore");
            yield return RoundSmokeUntilIdle();
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            RequireRoundSmoke(sourceRoundEndCount == 1, "final feature did not End once");
            yield return RoundSmokeStableFrames(4);
            RoundSmokeExit("DoubleFinalRestored", "complete");
        }

        private IEnumerator RoundSmokeHoldFeature(bool playing)
        {
            RequireRoundSmoke(latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen &&
                (playing ? session.InDoubleUp && sourceRoundPhase == SourceRoundPhase.DoublePlaying :
                    session.DoubleAvailable && !session.InDoubleUp && sourceRoundPhase == SourceRoundPhase.DoubleOffer),
                "cold feature lost its unresolved phase");
            string state = StartGameDurableFingerprint();
            int pockets = roundLifecycleSmokeOutcome.Pockets, suits = roundLifecycleSmokeOutcome.Suits;
            yield return StartGameLiveFrames(4);
            RequireRoundSmoke(StartGameDurableFingerprint() == state && roundLifecycleSmokeOutcome.Pockets == pockets &&
                roundLifecycleSmokeOutcome.Suits == suits && sourceRoundEndCount == 0 && !configuredAutoPlayCheckPending &&
                sourceGameplayState == SourceGameplayState.Playing && !sourceTableInputEnabled,
                "unresolved feature advanced without a new input");
        }

        private IEnumerator RoundSmokeEnterFeature()
        {
            RequireRoundSmoke(SourceDoubleOfferActive, "Double entry has no offer");
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.D));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return RoundSmokeWait(() => session.InDoubleUp && SourcePlayerInputs.Count == 0, "real Double entry");
            RequireRoundSmoke(session.Credit == 999 && session.DoubleStake == 36 && sourceRoundEndCount == 0,
                "Double entry changed its exact 36 stake or wallet basis");
            RequireRoundSmoke(PcPaymentTracked ? pcGamePayment.owner == PcGamePaymentOwner.Double &&
                pcGamePayment.baseOffer == PcGamePaymentOffer.Transferred && !pcGamePayment.moneyApplied &&
                ((RouletteSession)session).RoundPayoutPending : pcGamePayment == null &&
                !((RouletteSession)session).RoundPayoutPending,
                "Double entry mixed unpaid v3 with legacy paid-base accounting");
        }

        private IEnumerator RoundSmokeGuess(bool red)
        {
            yield return RoundSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                "real feature guess gate");
            RefreshPortraitUi();
            int suits = roundLifecycleSmokeOutcome.Suits;
            QueueStartGameButton(red ? uiDoubleRedButton : uiDoubleBlackButton, red ? 292 : 293, true);
            yield return RoundSmokeWait(() => roundLifecycleSmokeOutcome.Suits == suits + 1, "one real suit draw");
            RequireRoundSmoke(IsDoubleReveal && sourceRoundEndCount == 0, "card skipped its reveal boundary");
        }

        private IEnumerator RoundSmokeHighWin()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(20);
                yield return StartGameChooseDemo(2);
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                configuredAutoPlay.SetSecsToAutoCompleteWarning(17);
                configuredAutoPlay.SetAutoCompleteShowCountdown(true);
                configuredAutoPlay.SetDisableWhenGameFinished(true);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            RequireRoundSmoke(session.Credit == (PcPaymentTracked && !pcGamePayment.moneyApplied ? 980 : 1700) &&
                session.Returned == 720, "high-win evaluation or wallet application differs");
            if (roundSmokeCase == "high_win_purge")
            {
                PcLifecycleAutoSnapshot before = PcLifecycleAutoSnapshot.Capture(configuredAutoPlay);
                RequireRoundSmoke(before.autoPlay != 0 && before.playerInput && before.turbo, "high purge seed lacks configured Turbo");
                SourcePlayerInputs.Enqueue(0x200010, () => { throw new InvalidOperationException("High-win entry purge dispatched input"); });
                yield return RoundSmokeUntilIdle();
                PcLifecycleAutoSnapshot expected = before.Purged(); expected.offWhenFinished = false;
                expected.disableWhenFinished = false; // Native End V2 consumes this independent flag.
                RequireRoundSmoke(PcLifecycleAutoSnapshot.Capture(configuredAutoPlay).Same(expected) &&
                    !roundSmokeSawHighWait && roundSmokeSawRack && sourceRoundEndCount == 1,
                    "entry purge did not control rerack/high wait or preserve independent options");
            }
            else if (roundSmokePhase == "await")
            {
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            else
            {
                RequireRoundSmoke(latestRoundLifecycle.roundStage == PcRoundStage.AwaitEnd, "third high-win phase is not AwaitEnd");
                yield return RoundSmokeUntilIdle();
                RequireRoundSmoke(!roundSmokeSawRack && !roundSmokeSawHighWait && sourceRoundEndCount == 1,
                    "AwaitEnd replayed its completed high-win display");
            }
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            yield return RoundSmokeStableFrames(4);
            RoundSmokeExit("HighWinRestored", "complete");
        }

        private void RequireRoundSmokeEndPrefix(string boundary)
        {
            PcRoundEndIntent intent = latestRoundLifecycle.endIntent;
            RequireRoundSmoke(intent != null && intent.finishAuto && !intent.transactionActive && intent.demoAvailable,
                "partial End lacks its original decision");
            PcLifecycleAutoSnapshot expected = intent.autoBefore.Clone();
            // Every later C cut follows the independent V2 Disable branch.
            // Its Cancel clears Start only when the saved timer object exists.
            if (intent.version == 2 && intent.finishDisable && boundary != "EndPreparedSaved")
            {
                expected.disableWhenFinished = false;
                if (intent.timerPresent) expected.secondsStart = 0;
            }
            if (boundary != "EndPreparedSaved") expected.autoPlay = 0;
            if (boundary == "EndTurboFalse" || boundary == "EndOffFalse" || boundary == "EndPurged" || boundary == "EndedSaved") expected.turbo = false;
            if (boundary == "EndOffFalse" || boundary == "EndPurged" || boundary == "EndedSaved") expected.offWhenFinished = false;
            if (boundary == "EndPurged" || boundary == "EndedSaved") expected = expected.Purged();
            RequireRoundSmoke(PcLifecycleAutoSnapshot.Capture(configuredAutoPlay).Same(expected),
                "End setter prefix or independent five fields differ at " + boundary);
            if (boundary == "EndPurged") RequireRoundSmoke(SourcePlayerInputs.Count == 0, "End did not consume queued input");
        }

        private void RequireRoundSmokeDepositBoundary(string boundary)
        {
            PcIdleEntry entry = latestRoundLifecycle.idleEntry;
            RequireRoundSmoke(entry != null && entry.state == PcIdleState.Running && entry.depositRequired && entry.eligible &&
                entry.creditBefore == 999 && entry.creditAfter == 2999 && sourceRoundEndCount == (roundSmokePhase == "save" ? 1 : 0),
                "deposit boundary lost its original operation or repeated End");
            if (roundSmokePrevious != null)
                RequireRoundSmoke(entry.idleEntryId == roundSmokePrevious.lifecycle.idleEntry.idleEntryId &&
                    entry.depositEventId == roundSmokePrevious.lifecycle.idleEntry.depositEventId,
                    "same deposit restart allocated a new operation");
            if (boundary == "IdleBeginSaved") RequireRoundSmoke(session.Credit == 999 && ReadPcCreditEvent() == null, "deposit ran before Begin cut");
            else
            {
                PcCreditEvent credit = ReadPcCreditEvent();
                RequireRoundSmoke(credit != null && credit.id == entry.depositEventId && credit.kind == "deposit" &&
                    credit.phase == 3 && credit.before == 999 && credit.after == 2999 &&
                    session.Credit == (boundary == "IdleDepositSaved" ? 999 : 2999), "deposit save/apply ordering differs");
            }
            if (boundary == "IdleCheckPurgedSaved")
                RequireRoundSmoke(!entry.prepareRequested && PcLifecycleAutoSnapshot.Capture(configuredAutoPlay).Same(entry.autoBefore.Purged()) &&
                    SourcePlayerInputs.Count == 0, "CheckAuto purge did not retain the prior funding and cancel Prepare");
        }

        private IEnumerator RoundSmokeIdleDeposit()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                yield return StartGameChooseDemo(1);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            if (roundSmokePhase == "purged")
                SourcePlayerInputs.Enqueue(0x200010, () => { throw new InvalidOperationException("Idle funding purge dispatched input"); });
            if (roundSmokePhase == "saved" || roundSmokePhase == "applied" || roundSmokePhase == "purged")
            {
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            if (roundSmokePhase == "collect_restore")
            {
                RequireRoundSmoke(session.Credit == 0 && ReadPcCreditEvent().kind == "collect" &&
                    ReadPcCreditEvent().phase == 3 && latestRoundLifecycle.idleEntry.state == PcIdleState.Pending &&
                    latestRoundLifecycle.idleEntry.consumedCollectEventId == ReadPcCreditEvent().id &&
                    latestRoundLifecycle.idleEntry.idleEntryId != roundSmokePrevious.lifecycle.idleEntry.idleEntryId &&
                    sourceGameplayState == SourceGameplayState.Idle && configuredAutoPlayCheckPending,
                    "collect phase3 cold gap did not override staged Bet with a new exact Idle");
                yield return RoundSmokeUntilIdle();
                PcIdleEntry restored = latestRoundLifecycle.idleEntry;
                RequireRoundSmoke(session.Credit == 2000 && session.TotalBet == 0 && restored.creditBefore == 0 &&
                    restored.creditAfter == 2000 && restored.depositRequired && restored.prepareRequested && !restored.prepareAccepted &&
                    ReadPcCreditEvent().id == restored.depositEventId && sourceRoundEndCount == 0 &&
                    roundLifecycleSmokeOutcome.Pockets == 0 && roundLifecycleSmokeOutcome.Suits == 0,
                    "new empty-table Idle did not fund once and refuse Draw");
                yield return RoundSmokeStableFrames(4);
                RoundSmokeExit("CollectedNewIdleComplete", "complete");
                yield break;
            }
            string oldEntry = latestRoundLifecycle.idleEntry.idleEntryId;
            yield return RoundSmokeUntilIdle();
            RequireRoundSmoke(session.Credit == 2999 && !latestRoundLifecycle.idleEntry.prepareAccepted &&
                roundLifecycleSmokeOutcome.Pockets == 0 && sourceRoundEndCount == 0,
                "funded old entry repeated a deposit, End or Draw");
            if (roundSmokePhase == "complete")
            {
                RequireRoundSmoke(latestRoundLifecycle.idleEntry.idleEntryId == oldEntry, "Running completion replaced its entry");
                yield return RoundSmokeStableFrames(4);
                RoundSmokeExit("DepositIdleComplete", "complete");
                yield break;
            }
            RequireRoundSmoke(latestRoundLifecycle.idleEntry.idleEntryId != oldEntry, "fresh legitimate Idle did not allocate a new entry");
            // Clear preserves its caller's source state. A real table release
            // must first request Bet and let the live FIFO service its entry.
            long betBefore = session.TotalBet;
            string rngBefore = DemoCleanupRngBytes();
            RefreshPortraitUi();
            var wagerRelease = QueueStartGameButton(uiBetButtons[27], 294, true);
            yield return RoundSmokeWait(() => sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "real FIFO wager enters Bet2");
            RequireSourcePointerReleased(wagerRelease);
            RequireRoundSmoke(session.TotalBet > betBefore && session.Credit == 2999 &&
                !configuredAutoPlayCheckPending && DemoCleanupRngBytes() == rngBefore &&
                roundLifecycleSmokeOutcome.Pockets == 0 && sourceRoundEndCount == 0,
                "Bet2 entry did not preserve the completed money and draw state");
            RefreshPortraitUi();
            var clearRelease = QueueStartGameButton(uiClearButton, 295, true);
            yield return StartGameDrainWithLiveModule();
            RequireSourcePointerReleased(clearRelease);
            RequireRoundSmoke(session.TotalBet == 0 && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && !configuredAutoPlayCheckPending &&
                JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey, "")).sourceGameplayState == 2,
                "real Clear did not preserve the entered and saved Bet2 empty table");
            yield return StartGameChooseDemo(1);
            roundSmokeTarget = "CollectCompletedSaved";
            roundSmokeArmedCut = true;
            RequestPcCollect();
            RequireRoundSmoke(pcCollectPending, "real collect request was refused");
            yield return RoundSmokeAwaitCut();
        }

        private void RequireRoundSmokeSuccessorBoundary(string boundary)
        {
            if (boundary == "IdlePrepareFinishedSaved")
                RequireRoundSmoke(latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Running && latestRoundLifecycle.idleEntry.prepareAccepted &&
                    pendingSpin == null && roundLifecycleSmokeOutcome.Pockets == 1 && session.Credit == 999,
                    "Prepare cut committed or failed its reserved successor");
            else if (boundary == "PendingSaved")
                RequireRoundSmoke(pendingSpin != null && pendingSpin.id == roundSmokePrevious.lifecycle.idleEntry.successorSpinId &&
                    pendingSpin.baseLatestRoundId == roundSmokePrevious.lifecycle.roundArchiveId &&
                    latestRoundLifecycle.idleEntry.step == PcIdleStep.DrawInFlight && session.Credit == 998 && session.Rounds == 1 &&
                    roundLifecycleSmokeOutcome.Pockets == 1 && roundSmokeCompleteSpins == 0 && sourceRoundEndCount == 0,
                    "successor did not commit one exact reserved pending");
            else if (boundary == "RoundOpenSaved")
                RequireRoundSmoke(latestRoundLifecycle.roundArchiveId != roundSmokePrevious.lifecycle.roundArchiveId &&
                    latestRoundLifecycle.spinJournalId == pendingSpin.id && pendingSpin.baseLatestRoundId == roundSmokePrevious.lifecycle.roundArchiveId &&
                    session.Rounds == 2 && recordedRoundPages.Count == 2 && session.Wagered == 2 &&
                    roundSmokeCompleteSpins == 1 && roundSmokeRecordPockets == 1 && sourceRoundEndCount == 0,
                    "new archive did not atomically replace its predecessor lifecycle");
        }

        private IEnumerator RoundSmokeSuccessor()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                configuredAutoPlay.SetOptions(true, false, true, true, false, 2);
                roundSmokeArmedCut = true;
                yield return RoundSmokePlay();
            }
            else if (roundSmokePhase == "settled")
            {
                RequireRoundSmoke(pendingSpin != null && session.SpinPending && pendingSpin.id == roundSmokePrevious.lifecycle.idleEntry.successorSpinId &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Complete && gameStartTimestamp == 0,
                    "cold pending did not complete the exact Idle handoff before display");
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            }
            else if (roundSmokePhase == "complete")
            {
                yield return RoundSmokeUntilIdle();
                RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
                RequireRoundSmoke(session.Credit == 998 && session.Rounds == 2 && sourceRoundEndCount == 1 &&
                    roundLifecycleSmokeOutcome.Pockets == 0, "completed successor started a third round");
                yield return RoundSmokeStableFrames(4);
                RoundSmokeExit("SuccessorComplete", "complete");
                yield break;
            }
            yield return RoundSmokeAwaitCut();
        }

        private IEnumerator RoundSmokeBetRestore()
        {
            if (roundSmokePhase == "save")
            {
                yield return RoundSmokePlaceBet(1);
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                yield return RoundSmokePlay();
                yield return RoundSmokeUntilIdle();
                yield return RoundSmokePlaceBet(1);
                yield return StartGameChooseDemo(2);
                configuredAutoPlay.SetSecsToAutoCompleteWarning(17);
                configuredAutoPlay.SetSecsToAutoCompleteStart(29);
                configuredAutoPlay.SetAutoCompleteShowCountdown(true);
                configuredAutoPlay.SetDisableWhenGameFinished(true);
                RequireRoundSmoke(sourceGameplayState == SourceGameplayState.Bet && !configuredAutoPlayCheckPending &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Complete, "post-End choice did not persist explicit Bet2");
                SaveStagedBetsIfChanged();
                RoundSmokeExit("ExplicitBetAfterEnd", "complete");
                yield break;
            }
            if (roundSmokePhase == "play")
            {
                string entry = latestRoundLifecycle.idleEntry.idleEntryId;
                string options = AutoNvConfig(configuredAutoPlay), bytes = AutoNvBytes();
                RequireRoundSmoke(sourceGameplayState == SourceGameplayState.Bet && !configuredAutoPlayCheckPending,
                    "explicit staged Bet became an Idle entry");
                yield return StartGameLiveFrames(4);
                RequireRoundSmoke(latestRoundLifecycle.idleEntry.idleEntryId == entry && AutoNvConfig(configuredAutoPlay) == options &&
                    AutoNvBytes() == bytes && sourceRoundEndCount == 0 && roundLifecycleSmokeOutcome.Pockets == 0,
                    "cold Bet2 reset the player's post-End configuration");
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                configuredAutoPlay.SetSecsToAutoCompleteStart(0); // explicit new choice suppresses the new offer
                yield return RoundSmokePlay();
                yield return RoundSmokeAwaitCut();
                yield break;
            }
            yield return RoundSmokeUntilIdle();
            RequireRoundSmokePaidUnchanged(roundSmokePrevious, false);
            RequireRoundSmoke(session.Credit == 1070 && session.Returned == 72 && session.Rounds == 2 &&
                recordedRoundPages[0].pocket == 27 && recordedRoundPages[1].pocket == 27 && sourceRoundEndCount == 1,
                "same-pocket manual successor associated with the wrong archived identity");
            yield return RoundSmokeStableFrames(4);
            RoundSmokeExit("BetSuccessorComplete", "complete");
        }

        private IEnumerator RoundSmokePlaceBet(int amount)
        {
            yield return StateEndPlaceBet(amount);
            RequireRoundSmoke(session.TotalBet == amount && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending, "real wager did not enter Bet");
        }

        private IEnumerator RoundSmokePlay()
        {
            RefreshPortraitUi();
            RequireRoundSmoke(SourcePlayInputAllowed && uiSpinButton.interactable && SourcePlayerInputs.Count == 0,
                "real PLAY is gated");
            int draws = roundLifecycleSmokeOutcome.Pockets;
            long rounds = session.Rounds;
            QueueStartGameButton(uiSpinButton, 286, true);
            yield return RoundSmokeWait(() => session.SpinPending && pendingSpin != null, "PLAY commitment");
            RequireRoundSmoke(roundLifecycleSmokeOutcome.Pockets == draws + 1 && session.Rounds == rounds &&
                sourceRoundPhase == SourceRoundPhase.Wheel, "PLAY failed to commit exactly one real draw");
        }

        private IEnumerator RoundSmokeWait(Func<bool> ready, string description, float timeout = 90f)
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!ready() && !pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline)
            {
                RoundSmokeObservePresentation();
                yield return null;
            }
            sourceInputModule.enabled = false;
            RequireRoundSmoke(ready() && !pcRoundLifecycleStorageFaulted, "live wait failed: " + description);
        }

        private void RoundSmokeObservePresentation()
        {
            if (winRackActive)
            {
                roundSmokeSawRack = true;
                if (roundSmokeCase == "win" && roundSmokePhase == "load" && !roundSmokeCapturedRack)
                {
                    roundSmokeCapturedRack = true;
                    CaptureRoundSmokeImage("rerack");
                }
            }
            if (configuredResultDelayDeadlineTimestamp != 0 &&
                System.Diagnostics.Stopwatch.GetTimestamp() < configuredResultDelayDeadlineTimestamp)
            {
                roundSmokeSawHighWait = true;
                roundSmokeHighStart = configuredResultDelayStartTimestamp;
                roundSmokeHighDeadline = configuredResultDelayDeadlineTimestamp;
                RequireRoundSmoke(!autoplay && configuredResultDelayStartTimestamp == resultSettledTimestamp,
                    "cold configured high-win gate inherited an active wait");
            }
        }

        private IEnumerator RoundSmokeUntilIdle()
        {
            yield return RoundSmokeWait(() => latestRoundLifecycle != null &&
                latestRoundLifecycle.roundStage == PcRoundStage.Ended && latestRoundLifecycle.idleEntry != null &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete && !configuredAutoPlayCheckPending &&
                sourceGameplayState == SourceGameplayState.Idle && sourceRoundPhase == SourceRoundPhase.None &&
                !session.SpinPending && !spinning && !winRackActive && !ResultDisplayActive &&
                SourcePlayerInputs.Count == 0, "real Ended/Complete Idle");
        }

        private IEnumerator RoundSmokeAwaitCut()
        {
            RequireRoundSmoke(!string.IsNullOrEmpty(roundSmokeTarget), "case has no exact cut target");
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 90f;
            while (!pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline)
            {
                RoundSmokeObservePresentation();
                yield return null;
            }
            RequireRoundSmoke(false, "checkpoint was not reached: " + roundSmokeTarget);
        }

        private IEnumerator RoundSmokeStableFrames(int count)
        {
            string before = StartGameDurableFingerprint();
            string envelope = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1", "");
            string config = AutoNvConfig(configuredAutoPlay), bytes = AutoNvBytes();
            int ends = sourceRoundEndCount, pockets = roundLifecycleSmokeOutcome.Pockets, suits = roundLifecycleSmokeOutcome.Suits;
            sourceInputModule.enabled = true;
            for (int i = 0; i < count; ++i)
            {
                yield return null;
                RequireRoundSmoke(!pcRoundLifecycleStorageFaulted && StartGameDurableFingerprint() == before &&
                    PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1", "") == envelope &&
                    AutoNvConfig(configuredAutoPlay) == config && AutoNvBytes() == bytes && sourceRoundEndCount == ends &&
                    roundLifecycleSmokeOutcome.Pockets == pockets && roundLifecycleSmokeOutcome.Suits == suits,
                    "later Idle frames changed committed state");
            }
            sourceInputModule.enabled = false;
        }

        private static int RoundSmokeArchiveVersion(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return 0;
            RecordedRoundArchiveV1 header = JsonUtility.FromJson<RecordedRoundArchiveV1>(raw);
            return header == null ? 0 : header.lifecycleSchemaVersion;
        }

        private void RequireRoundSmokeNative0()
        {
            bool lost = roundSmokeCase == "double_loss";
            long award = lost ? 0 : 72;
            int steps = lost ? 2 : 1;
            RecordedRound page = recordedRoundPages[0];
            RequireRoundSmoke((pcRoundArchiveSchemaVersion == 2 || pcRoundArchiveSchemaVersion == 3) &&
                pcDoubleFeatureNv != null && pcDoubleFeatureNv.savedState == 0 &&
                pcDoubleFeatureNv.completionStage == 1 && pcDoubleFeatureNv.completionAward == award &&
                pcDoubleFeatureNv.stepCount == steps && session.InDoubleUp && !session.DoubleAvailable &&
                session.Credit == 999 && session.Returned == 36 && session.DoubleStake == award &&
                page.hasDoubleUp && page.doubleUp.steps.Length == steps && !page.doubleUp.complete &&
                !page.doubleUp.tookWin && page.doubleUp.finalWin == 0 && page.returned == 36 &&
                page.creditAfter == (PcPaymentTracked ? 999 : 1035) &&
                page.awaitingFinalPayout && latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen && sourceRoundEndCount == 0,
                "native0 must retain a final result awaiting Game payout, not an already-paid page");
            if (PcPaymentTracked)
                RequireRoundSmoke(pcGamePayment.owner == PcGamePaymentOwner.Double && pcGamePayment.baseAward == 36 &&
                    pcGamePayment.creditBefore == 999 && !pcGamePayment.prepared && !pcGamePayment.moneyApplied &&
                    !pcGamePayment.historyPublished && ((RouletteSession)session).RoundPayoutPending &&
                    CommittedHistoryCount() == 0,
                    "v3 native0 prematurely prepared, paid or published its unpaid base");
            else RequireRoundSmoke(pcRoundArchiveSchemaVersion == 2 && pcGamePayment == null &&
                !((RouletteSession)session).RoundPayoutPending, "v2 native0 lost its explicit paid-base accounting");
            bool cold = roundSmokePhase == "load";
            RequireRoundSmoke(roundLifecycleSmokeOutcome.Pockets == (cold || roundSmokeCase == "double_take" ? 0 : 1) &&
                roundLifecycleSmokeOutcome.Suits == (cold || roundSmokeCase == "double_take" ? 0 : steps),
                "native0 repeated a wheel or suit draw");
        }

        private void RequireRoundSmokeV3RecoveryProgress(RoundLifecycleSmokeReceipt proof, bool startup)
        {
            var previous = JsonUtility.FromJson<RecordedRoundArchiveV3>(proof.raw.envelope);
            RequireRoundSmoke(proof.hasSession && session != null && previous != null && previous.payment != null &&
                previous.rounds != null && previous.rounds.Length > 0 && PcPaymentTracked,
                "v3 recovery lost its payment or current archive");
            PcGamePaymentRecordV1 prior = previous.payment;
            prior.Validate(); pcGamePayment.Validate();
            RequireRoundSmoke(pcGamePayment.version == prior.version && pcGamePayment.roundArchiveId == prior.roundArchiveId &&
                pcGamePayment.spinJournalId == prior.spinJournalId && pcGamePayment.profile == prior.profile &&
                pcGamePayment.operationId == prior.operationId && pcGamePayment.creditBefore == prior.creditBefore &&
                pcGamePayment.denomination == prior.denomination && pcGamePayment.remainderCents == prior.remainderCents &&
                pcGamePayment.baseAward == prior.baseAward && pcGamePayment.priorHistoryCount == prior.priorHistoryCount &&
                pcGamePayment.owner == prior.owner && pcGamePayment.featureId == prior.featureId &&
                (!prior.moneyApplied || pcGamePayment.moneyApplied) && (!prior.historyPublished || pcGamePayment.historyPublished),
                "v3 cold continuation changed immutable payment anchors or reversed durable progress");
            if (proof.schema == 2)
                RequireRoundSmoke(proof.archiveSchema == 3 && proof.paymentJson == JsonUtility.ToJson(prior) &&
                    proof.visibleHistory == Math.Min(50, prior.priorHistoryCount + (prior.historyPublished ? 1 : 0)) &&
                    proof.roundPayoutPending == !prior.moneyApplied,
                    "v3 predecessor payment fields disagree with its closed raw archive");
            if (startup)
                RequireRoundSmoke(JsonUtility.ToJson(pcGamePayment) == JsonUtility.ToJson(prior),
                    "cold construction advanced Game payment before the live observer");

            bool appliedNow = !prior.moneyApplied && pcGamePayment.moneyApplied;
            long expectedCredit = proof.credit + (appliedNow ? pcGamePayment.finalAward : 0);
            long expectedReturned = proof.returned + (appliedNow && pcGamePayment.owner == PcGamePaymentOwner.Double ?
                pcGamePayment.finalAward - prior.baseAward : 0);
            if (roundSmokeCase == "idle_deposit" && roundSmokePhase == "applied" &&
                proof.lifecycle != null && proof.lifecycle.idleEntry != null)
                expectedCredit = proof.lifecycle.idleEntry.creditAfter;
            RequireRoundSmoke(session.Credit == expectedCredit && session.Wagered == proof.wagered &&
                session.Returned == expectedReturned && session.Rounds == proof.rounds && session.Wins == proof.wins &&
                session.Denomination == proof.denomination && ((RouletteSession)session).MoneyRemainderCents == proof.remainder &&
                recordedRoundPages.Count == proof.archiveCount && DemoCleanupRngBytes() == proof.raw.rng &&
                roundLifecycleSmokeOutcome.Pockets == 0 && roundLifecycleSmokeOutcome.Suits == 0 &&
                roundSmokeCompleteSpins == 0 && roundSmokeRecordPockets == 0 &&
                ((RouletteSession)session).RoundPayoutPending == !pcGamePayment.moneyApplied &&
                CommittedHistoryCount() == Math.Min(50, prior.priorHistoryCount + (pcGamePayment.historyPublished ? 1 : 0)),
                "v3 recovery changed money beyond one final award, reevaluated, redrew or published prematurely");
            RequireRoundSmoke(PlayerPrefs.GetString(recordedHistoryKey, "") == proof.raw.pockets &&
                PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "") == proof.raw.pages,
                "v3 recovery changed persistent pocket/snapshot pairs");

            RecordedRoundArchive archived = JsonUtility.FromJson<RecordedRoundArchive>(proof.financialArchive);
            RecordedRound expected = archived.rounds[0];
            if (appliedNow)
            {
                expected.creditAfter = checked(expected.creditAfter + pcGamePayment.finalAward);
                if (pcGamePayment.owner == PcGamePaymentOwner.Double)
                {
                    RequireRoundSmoke(previous.featureNv != null && previous.featureNv.savedState == 0 &&
                        previous.featureNv.completionStage == 1 && previous.featureNv.completionAward == pcGamePayment.finalAward &&
                        pcDoubleFeatureNv != null && pcDoubleFeatureNv.completionStage == 2 && pcDoubleFeatureNv.savedState == 0 &&
                        pcDoubleFeatureNv.featureId == previous.featureNv.featureId,
                        "Double application lacks its exact saved native0 award");
                    expected.returned = pcGamePayment.finalAward;
                    expected.totalReturned = expectedReturned;
                    expected.doubleUp.complete = true;
                    expected.doubleUp.tookWin = roundSmokeCase == "double_take";
                    expected.doubleUp.finalWin = pcGamePayment.finalAward;
                }
            }
            expected.awaitingFinalPayout = !pcGamePayment.historyPublished;
            RequireRoundSmoke(DemoCleanupArchive(false) == JsonUtility.ToJson(archived),
                "v3 recovery changed financial pages beyond its one application and publication");
            if (proof.lifecycle != null && latestRoundLifecycle != null)
                RequireRoundSmoke(latestRoundLifecycle.roundArchiveId == proof.lifecycle.roundArchiveId &&
                    latestRoundLifecycle.profile == proof.lifecycle.profile &&
                    latestRoundLifecycle.spinJournalId == proof.lifecycle.spinJournalId,
                    "v3 continuation changed round/profile/spin identity");
            if (startup) RequireRoundSmoke(sourceRoundEndCount == 0, "cold v3 restore executed End before observation");
        }

        private void RequireRoundSmokePaidUnchanged(RoundLifecycleSmokeReceipt proof, bool startup)
        {
            if (proof != null && proof.raw != null && RoundSmokeArchiveVersion(proof.raw.envelope) == 3)
            {
                RequireRoundSmokeV3RecoveryProgress(proof, startup);
                return;
            }
            if (!startup && proof != null && proof.raw != null && RoundSmokeArchiveVersion(proof.raw.envelope) == 2)
            {
                var previous = JsonUtility.FromJson<RecordedRoundArchiveV2>(proof.raw.envelope);
                if (previous.featureNv != null && previous.featureNv.completionStage == 1 &&
                    pcDoubleFeatureNv != null && pcDoubleFeatureNv.completionStage == 2)
                {
                    RequireRoundSmokeV2PaymentProgress(proof, previous);
                    return;
                }
            }
            RequireRoundSmoke(pcRoundArchiveSchemaVersion == 1 || pcRoundArchiveSchemaVersion == 2,
                "legacy paid continuation acquired a v3 archive");
            long expectedCredit = proof == null ? -1 : proof.credit;
            if (roundSmokeCase == "idle_deposit" && roundSmokePhase == "applied" && proof != null &&
                proof.lifecycle != null && proof.lifecycle.idleEntry != null)
                expectedCredit = proof.lifecycle.idleEntry.creditAfter;
            RequireRoundSmoke(proof != null && proof.hasSession && session != null &&
                session.Credit == expectedCredit && session.Wagered == proof.wagered && session.Returned == proof.returned &&
                session.Rounds == proof.rounds && session.Wins == proof.wins && session.Denomination == proof.denomination &&
                ((RouletteSession)session).MoneyRemainderCents == proof.remainder &&
                recordedRoundPages.Count == proof.archiveCount &&
                DemoCleanupRngBytes() == proof.raw.rng && roundLifecycleSmokeOutcome.Pockets == 0 &&
                roundLifecycleSmokeOutcome.Suits == 0 && roundSmokeCompleteSpins == 0 && roundSmokeRecordPockets == 0,
                "paid recovery changed money, totals, archive count, RNG or performed settlement/draw");
            RequireRoundSmoke(PlayerPrefs.GetString(recordedHistoryKey, "") == proof.raw.pockets &&
                PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "") == proof.raw.pages,
                "paid recovery changed persistent pocket/snapshot history");
            RecordedRoundArchive archived = JsonUtility.FromJson<RecordedRoundArchive>(proof.financialArchive);
            foreach (RecordedRound round in archived.rounds) round.awaitingFinalPayout = false;
            RequireRoundSmoke(DemoCleanupArchive(true) == JsonUtility.ToJson(archived),
                "paid recovery changed archived financial contents beyond completed offer normalization");
            if (proof.lifecycle != null && latestRoundLifecycle != null)
                RequireRoundSmoke(latestRoundLifecycle.roundArchiveId == proof.lifecycle.roundArchiveId &&
                    latestRoundLifecycle.profile == proof.lifecycle.profile &&
                    latestRoundLifecycle.spinJournalId == proof.lifecycle.spinJournalId,
                    "paid continuation changed round/profile/spin identity");
            if (startup)
                RequireRoundSmoke(sourceRoundEndCount == 0, "cold restore executed End before the observer was ready");
        }

        private void RequireRoundSmokeV2PaymentProgress(RoundLifecycleSmokeReceipt proof, RecordedRoundArchiveV2 previous)
        {
            PcDoubleFeatureNvV1 prior = previous.featureNv;
            long award = prior.completionAward;
            RecordedRoundArchive expected = JsonUtility.FromJson<RecordedRoundArchive>(proof.financialArchive);
            RecordedRound page = expected.rounds[0];
            long returned = proof.returned - page.returned + award;
            RequireRoundSmoke(pcRoundArchiveSchemaVersion == 2 && pcGamePayment == null &&
                !((RouletteSession)session).RoundPayoutPending && prior.savedState == 0 && prior.completionStage == 1 &&
                page.creditAfter == proof.credit + page.doubleUp.startingStake &&
                session.Credit == proof.credit + award && session.Returned == returned &&
                session.Wagered == proof.wagered && session.Rounds == proof.rounds && session.Wins == proof.wins &&
                session.Denomination == proof.denomination && ((RouletteSession)session).MoneyRemainderCents == proof.remainder &&
                recordedRoundPages.Count == proof.archiveCount && DemoCleanupRngBytes() == proof.raw.rng &&
                roundLifecycleSmokeOutcome.Pockets == 0 && roundLifecycleSmokeOutcome.Suits == 0 &&
                roundSmokeCompleteSpins == 0 && roundSmokeRecordPockets == 0,
                "legacy v2 native0 application changed its paid-base wallet beyond one final award");
            RequireRoundSmoke(pcDoubleFeatureNv.savedState == 0 && pcDoubleFeatureNv.completionStage == 2 &&
                pcDoubleFeatureNv.completionAward == award && pcDoubleFeatureNv.featureId == prior.featureId &&
                pcDoubleFeatureNv.roundArchiveId == prior.roundArchiveId && pcDoubleFeatureNv.spinJournalId == prior.spinJournalId &&
                pcDoubleFeatureNv.profile == prior.profile && pcDoubleFeatureNv.stepCount == prior.stepCount &&
                pcDoubleFeatureNv.suitMode == prior.suitMode && pcDoubleFeatureNv.autoRestoreMarker == prior.autoRestoreMarker &&
                latestRoundLifecycle.roundArchiveId == proof.lifecycle.roundArchiveId &&
                latestRoundLifecycle.spinJournalId == proof.lifecycle.spinJournalId,
                "legacy v2 application replaced its native result or round identity");
            page.creditAfter = proof.credit + award;
            page.returned = award; page.totalReturned = returned;
            page.doubleUp.complete = true; page.doubleUp.tookWin = roundSmokeCase == "double_take";
            page.doubleUp.finalWin = award; page.awaitingFinalPayout = false;
            RequireRoundSmoke(DemoCleanupArchive(false) == JsonUtility.ToJson(expected) &&
                PlayerPrefs.GetString(recordedHistoryKey, "") == proof.raw.pockets &&
                PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "") == proof.raw.pages,
                "legacy v2 completion changed financial pages beyond its deferred award");
        }

        private IEnumerator RoundSmokeObserveFault()
        {
            if (!pcRoundLifecycleStorageFaulted)
            {
                if (sourceInputModule != null) sourceInputModule.enabled = true;
                float deadline = Time.realtimeSinceStartup + 90f;
                while (!pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            }
            RequireRoundSmoke(pcRoundLifecycleStorageFaulted && pcAutoPlayStorageFaulted && !autoplay &&
                !configuredAutoPlayCheckPending && !sourceTableInputEnabled &&
                (sourceInputModule == null || !sourceInputModule.enabled), "expected lifecycle fault did not stop dispatch");
            RoundLifecycleSmokeRaw before = ReadRoundSmokeRaw();
            if (roundSmokeFaultBefore != null)
                RequireRoundSmoke(JsonUtility.ToJson(before) == JsonUtility.ToJson(roundSmokeFaultBefore),
                    "before-write failure changed persistent candidates");
            else
                RequireRoundSmoke(before.envelope == roundSmokeBoot.envelope && before.rng == roundSmokeBoot.rng &&
                    before.auto == roundSmokeBoot.auto && before.pending == roundSmokeBoot.pending,
                    "loaded fault rewrote protected input evidence");
            int pockets = roundLifecycleSmokeOutcome == null ? 0 : roundLifecycleSmokeOutcome.Pockets;
            int suits = roundLifecycleSmokeOutcome == null ? 0 : roundLifecycleSmokeOutcome.Suits;
            int ends = sourceRoundEndCount, complete = roundSmokeCompleteSpins;
            for (int i = 0; i < 4; ++i)
            {
                yield return null;
                RequireRoundSmoke(JsonUtility.ToJson(ReadRoundSmokeRaw()) == JsonUtility.ToJson(before) &&
                    !autoplay && !sourceTableInputEnabled && sourceRoundEndCount == ends && roundSmokeCompleteSpins == complete &&
                    (roundLifecycleSmokeOutcome == null || roundLifecycleSmokeOutcome.Pockets == pockets &&
                        roundLifecycleSmokeOutcome.Suits == suits), "faulted live frames continued business effects");
            }
            RequireRoundSmoke(portraitCanvas == null || !portraitCanvas.gameObject.activeSelf,
                "fault left a playable main canvas visible");
            string expectedOperation = roundSmokeCase == "fault_schema" ? "loaded" :
                roundSmokeCase == "fault_auto" ? "advanced" : roundSmokePhase == "run" ? "saved" : "restored";
            RequireRoundSmoke(pcAutoPlayStorageFaultOperation == expectedOperation, "fault category differs");
            if ((roundSmokeCase == "fault_schema" && roundSmokePhase == "load") ||
                (roundSmokeCase == "fault_end_save" && roundSmokePhase == "run"))
                yield return CaptureRoundSmokeFaultImage();
            RoundSmokeExit("ExpectedFault", "expected_fault");
        }

        private string RoundSmokeImagePath(string shot)
        {
            string prefix = RoundSmokeArgument("-rouletteRoundLifecycleCapturePrefix", false);
            if (string.IsNullOrEmpty(prefix)) return "";
            string path = Path.GetFullPath(prefix + "_" + shot + ".png");
            RequireRoundSmoke(!File.Exists(path), "capture output already exists");
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            return path;
        }

        private void ReportRoundSmokeImage(string shot, string path)
        {
            RequireRoundSmoke(File.Exists(path) && new FileInfo(path).Length > 0, "capture was not written");
            string digest;
            using (SHA256 sha = SHA256.Create())
                digest = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
            Debug.Log("ROULETTE_ROUND_NV_CAPTURE: case=" + roundSmokeCase + " phase=" + roundSmokePhase +
                " shot=" + shot + " path=" + path + " sha256=" + digest);
        }

        private void CaptureRoundSmokeImage(string shot)
        {
            string path = RoundSmokeImagePath(shot);
            if (string.IsNullOrEmpty(path)) return;
            RefreshPortraitUi();
            CaptureActiveScreenOffscreen(path);
            ReportRoundSmokeImage(shot, path);
        }

        private IEnumerator CaptureRoundSmokeFaultImage()
        {
            string path = RoundSmokeImagePath("fault");
            if (string.IsNullOrEmpty(path)) yield break;
            yield return null;
            ScreenCapture.CaptureScreenshot(path, 2);
            float deadline = Time.realtimeSinceStartup + 10f;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < deadline) yield return null;
            ReportRoundSmokeImage("fault", path);
        }

        private RoundLifecycleSmokeReceipt CaptureRoundSmokeReceipt(string point, string mode)
        {
            var proof = new RoundLifecycleSmokeReceipt {
                testCase = roundSmokeCase, phase = roundSmokePhase, checkpoint = point, exitMode = mode,
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess,
                autoPath = PcAutoPlayStatePath, previousReceiptSha256 = roundSmokePreviousHash ?? "",
                raw = ReadRoundSmokeRaw(), lifecycle = latestRoundLifecycle,
                autoConfig = AutoNvConfig(configuredAutoPlay),
                archiveCount = recordedRoundPages.Count,
                pocketsDrawn = roundLifecycleSmokeOutcome == null ? 0 : roundLifecycleSmokeOutcome.Pockets,
                suitsDrawn = roundLifecycleSmokeOutcome == null ? 0 : roundLifecycleSmokeOutcome.Suits,
                completeSpinCalls = roundSmokeCompleteSpins, recordPocketCalls = roundSmokeRecordPockets,
                ends = sourceRoundEndCount, checks = roundSmokeChecks, hasSession = session != null,
                sourceState = (int)sourceGameplayState, sourcePhase = (int)sourceRoundPhase,
                spinning = spinning, rack = winRackActive, active = autoplay, muted = pcGameSoundsMuted,
                configuredCheckPending = configuredAutoPlayCheckPending, tableEnabled = sourceTableInputEnabled,
                fault = pcRoundLifecycleStorageFaulted, faultOperation = pcAutoPlayStorageFaultOperation ?? "",
                optionalUndoFault = roundSmokeUndoFailed, checkpoints = roundSmokeEvents.ToArray(),
                sawRack = roundSmokeSawRack, sawHighWait = roundSmokeSawHighWait,
                observedHighStart = roundSmokeHighStart, observedHighDeadline = roundSmokeHighDeadline,
                gameStartTimestamp = gameStartTimestamp, gameDelayMilliseconds = gameDelayMilliseconds,
                resultDelayStart = configuredResultDelayStartTimestamp, resultDelayDeadline = configuredResultDelayDeadlineTimestamp,
                selectedDemo = demoOutcome == null ? 0 : demoOutcome.SelectedOption,
                archiveSchema = pcRoundArchiveSchemaVersion, visibleHistory = CommittedHistoryCount(),
                paymentJson = pcGamePayment == null ? "" : JsonUtility.ToJson(pcGamePayment),
                featureJson = pcDoubleFeatureNv == null ? "" : JsonUtility.ToJson(pcDoubleFeatureNv),
                roundPayoutPending = session is RouletteSession && ((RouletteSession)session).RoundPayoutPending,
                displayedCredit = session == null ? -1 : DisplayedCredit, displayedPaid = session == null ? -1 : DisplayedPaid
            };
            if (session != null)
            {
                proof.credit = session.Credit; proof.wagered = session.Wagered; proof.returned = session.Returned;
                proof.rounds = session.Rounds; proof.wins = session.Wins; proof.denomination = session.Denomination;
                proof.remainder = ((RouletteSession)session).MoneyRemainderCents; proof.totalBet = session.TotalBet;
                proof.spinPending = session.SpinPending; proof.inDouble = session.InDoubleUp; proof.doubleAvailable = session.DoubleAvailable;
                proof.pendingPocket = session.SpinPending ? session.PendingPocket : -1;
                proof.result = session.LastResult == null ? "" : DemoCleanupResult(session.LastResult);
                proof.financialArchive = DemoCleanupArchive(false); proof.undo = StateBetCleanupUndoFingerprint();
            }
            proof.rawSha256 = RoundSmokeSha(JsonUtility.ToJson(proof.raw));
            return proof;
        }

        private void RoundSmokeExit(string checkpoint, string mode)
        {
            RequireRoundSmoke(!roundSmokeExiting, "duplicate receipt exit");
            roundSmokeExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            RoundLifecycleSmokeReceipt proof = CaptureRoundSmokeReceipt(checkpoint, mode);
            string directory = Path.GetDirectoryName(roundSmokeOutput);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            using (var stream = new FileStream(roundSmokeOutput, FileMode.CreateNew, FileAccess.Write,
                FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(encoded);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            Debug.Log("ROULETTE_ROUND_NV_PASSED: case=" + roundSmokeCase + " phase=" + roundSmokePhase +
                " point=" + checkpoint + " mode=" + mode + " checks=" + roundSmokeChecks +
                " credit=" + proof.credit + " rounds=" + proof.rounds + " ends=" + proof.ends +
                " pockets=" + proof.pocketsDrawn + " suits=" + proof.suitsDrawn + " completeSpin=" + proof.completeSpinCalls +
                " recordPocket=" + proof.recordPocketCalls + " fault=" + proof.fault.ToString().ToLowerInvariant() +
                " receipt=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_ROUND_NV_EXIT: api=" + (mode == "checkpoint" ? "TerminateProcess" : "Application.Quit") +
                " code=0 mode=" + mode);
            // A cut must not run shutdown or flush any later cached Prefs.
            // Ordinary completed/fault-observed processes use normal shutdown.
            if (mode == "checkpoint")
            {
                RequireRoundSmoke(roundSmokeConfigured && recordedHistoryKey.Contains(".Test.") &&
                    Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteRoundLifecycleSmoke") >= 0,
                    "native cut requires the explicit isolated smoke context");
                // Mono's Environment.Exit can wait on Unity worker threads.
                // The receipt is closed before this current-process-only cut.
                TerminateProcess(GetCurrentProcess(), 0);
                throw new InvalidOperationException("Round NV current-process cut returned, Win32=" + Marshal.GetLastWin32Error());
            }
            else Application.Quit(0);
        }
    }
}
