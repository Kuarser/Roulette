using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    // Opt-in observer for the existing result-wait diagnostics. This file does
    // not replace the original unflagged fixture or any production payment.
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class DeferredPaymentSmokeObservation
        {
            public string point, paymentJson, rawJson, rawSha256;
            public long at, credit, returned, wagered, rounds, displayedCredit, displayedPaid;
            public int frame, ends, physicalHistory, visibleHistory, archiveSchema;
            public bool modelPending, resultDisplay, rack, spinning, resultPhase, resultsCleared;
            public int waits, timeouts, inputs, skipped;
            public long settledAt, waitStarted, waitDue, waitCompletedAt, highDelayStarted, highDelayDue;
            public bool waitCompleted;
        }

        [Serializable]
        private sealed class DeferredPaymentSmokeProof
        {
            public int schema = 1;
            public string testCase, historyKey, output;
            public long frequency, creditBefore, award, wager, finalDeposit;
            public bool successorSpin;
            public DeferredPaymentSmokeObservation[] observations;
        }

        private readonly List<DeferredPaymentSmokeObservation> deferredPaymentSmokeObservations =
            new List<DeferredPaymentSmokeObservation>();
        private Action<string> deferredPaymentSmokePriorCheckpoint;
        private Action deferredPaymentSmokeAfterPublished;
        private string deferredPaymentSmokeCase, deferredPaymentSmokeOutput, deferredPaymentSmokeRound;
        private bool deferredPaymentSmokeRecording;

        private bool DeferredPaymentSmokeEnabled
        {
            get { return Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteDeferredPaymentSmoke") >= 0; }
        }

        private void RequireDeferredPaymentSmoke(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Deferred payment smoke: " + message);
        }

        private void EnableDeferredPaymentSmokeFixture()
        {
            if (!DeferredPaymentSmokeEnabled) return;
            string[] args = Environment.GetCommandLineArgs();
            string[] flags = { "-roulettePromptDelayClockSmoke", "-rouletteConfiguredAutoPlaySmoke",
                "-rouletteAutoLossGateSmoke", "-rouletteAutoWinFilmGateSmoke",
                "-rouletteHighWinAutoSmoke", "-rouletteAutoCancelResultSmoke" };
            string[] cases = { "prompt_delay", "configured_turbo", "auto_loss", "auto_win",
                "active_highwin", "active_cancel" };
            int matches = 0;
            for (int i = 0; i < flags.Length; ++i)
                if (Array.IndexOf(args, flags[i]) >= 0) { ++matches; deferredPaymentSmokeCase = cases[i]; }
            if (deferredPaymentSmokeCase == "configured_turbo" &&
                Array.IndexOf(args, "-rouletteConfiguredAutoPlayHighWinCase") >= 0)
                deferredPaymentSmokeCase = "configured_highwin";
            RequireDeferredPaymentSmoke(matches == 1 &&
                Array.IndexOf(args, "-rouletteConfiguredAutoPlayFundingCase") < 0 &&
                Array.IndexOf(args, "-rouletteConfiguredAutoPlayFundStopEntryCase") < 0,
                "flag needs exactly one supported result-wait case");
            int outputIndex = Array.IndexOf(args, "-rouletteDeferredPaymentProof");
            RequireDeferredPaymentSmoke(outputIndex >= 0 && outputIndex + 1 < args.Length &&
                Path.IsPathRooted(args[outputIndex + 1]), "explicit proof output is missing");
            deferredPaymentSmokeOutput = Path.GetFullPath(args[outputIndex + 1]);
            RequireDeferredPaymentSmoke(!File.Exists(deferredPaymentSmokeOutput) && !temporaryHistory &&
                recordedHistoryKey.Contains(".Test.") && recordedRoundPages.Count == 0 &&
                session is RouletteSession && session.Credit == 1000 && session.Rounds == 0 &&
                !session.SpinPending && !deferredPaymentSmokeRecording,
                "fixture must start from the unused isolated 1000-credit session");
            ((RouletteSession)session).EnableDeferredBaseSettlement();
            deferredPaymentSmokePriorCheckpoint = pcLifecycleCheckpoint;
            pcLifecycleCheckpoint = DeferredPaymentSmokeCheckpoint;
            deferredPaymentSmokeRecording = true;
        }

        private void DeferredPaymentSmokeCheckpoint(string point)
        {
            if (deferredPaymentSmokePriorCheckpoint != null) deferredPaymentSmokePriorCheckpoint(point);
            if (!deferredPaymentSmokeRecording || pcGamePayment == null) return;
            if (point == "RoundOpenSaved" || point == "GamePaymentPreparedSaved" ||
                point == "GamePaymentReplySaved" || point == "GamePaymentMoneyAppliedSaved" ||
                point == "GamePaymentCreditNotifiedSaved" || point == "GamePaymentCompletionSaved" ||
                point == "GamePaymentZeroResolvedSaved" || point == "GamePaymentHistoryPublishedSaved")
            {
                DeferredPaymentSmokeObserve(point);
                if (point == "GamePaymentHistoryPublishedSaved" && deferredPaymentSmokeAfterPublished != null)
                {
                    Action action = deferredPaymentSmokeAfterPublished;
                    deferredPaymentSmokeAfterPublished = null;
                    action();
                }
            }
        }

        private void DeferredPaymentSmokeObserve(string point)
        {
            RequireDeferredPaymentSmoke(PcPaymentTracked && pcGamePayment.owner == PcGamePaymentOwner.Base &&
                !pcAutoPlayStorageFaulted, "observation has no live v3 base payment");
            if (deferredPaymentSmokeRound == null) deferredPaymentSmokeRound = pcGamePayment.roundArchiveId;
            RequireDeferredPaymentSmoke(deferredPaymentSmokeRound == pcGamePayment.roundArchiveId,
                "observer crossed into a second settled round");
            string rawJson = JsonUtility.ToJson(ReadRoundSmokeRaw());
            OriginalResultDisplayWaitClock wait = pcResultDisplayWait;
            deferredPaymentSmokeObservations.Add(new DeferredPaymentSmokeObservation {
                point = point, at = Stopwatch.GetTimestamp(), frame = Time.frameCount,
                credit = session.Credit, returned = session.Returned, wagered = session.Wagered,
                rounds = session.Rounds, displayedCredit = DisplayedCredit, displayedPaid = DisplayedPaid,
                ends = sourceRoundEndCount, physicalHistory = recordedRoundPages.Count,
                visibleHistory = CommittedHistoryCount(), archiveSchema = pcRoundArchiveSchemaVersion,
                modelPending = ((RouletteSession)session).RoundPayoutPending, resultDisplay = ResultDisplayActive,
                rack = winRackActive, spinning = spinning, resultPhase = session.ResultPhase,
                resultsCleared = currentGameResultsCleared, settledAt = resultSettledTimestamp,
                paymentJson = JsonUtility.ToJson(pcGamePayment), rawJson = rawJson, rawSha256 = RoundSmokeSha(rawJson),
                waits = wait == null ? -1 : wait.InitialWaits, timeouts = wait == null ? -1 : wait.TimeoutCount,
                inputs = wait == null ? -1 : wait.InputCount, skipped = wait == null ? -1 : wait.SkippedWaits,
                waitStarted = wait == null ? 0 : wait.WaitStartedAt,
                waitDue = wait == null ? 0 : wait.WaitDueTimestamp,
                waitCompleted = wait != null && wait.Completed,
                waitCompletedAt = wait == null ? 0 : wait.CompletedAt,
                highDelayStarted = wait == null ? 0 : wait.HighDelayStartedAt,
                highDelayDue = wait == null ? 0 : wait.HighDelayDueTimestamp
            });
        }

        private void ObserveDeferredPaymentSmokeUnpaid(long creditBefore, long award, long wager,
            bool resultWaitRequired)
        {
            if (!DeferredPaymentSmokeEnabled) return;
            RequireDeferredPaymentSmoke(PcPaymentTracked && pcGamePayment.creditBefore == creditBefore &&
                pcGamePayment.baseAward == award && !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                ((RouletteSession)session).RoundPayoutPending && session.Credit == creditBefore &&
                session.Returned == award && session.Wagered == wager && session.Rounds == 1 &&
                !winRackActive && DisplayedCredit == creditBefore + award && DisplayedPaid == award &&
                recordedRoundPages.Count == 1 && recordedRoundPages[0].creditAfter == creditBefore &&
                recordedRoundPages[0].returned == award && recordedRoundPages[0].awaitingFinalPayout &&
                CommittedHistoryCount() == 0 && sourceRoundEndCount == 0 &&
                (!resultWaitRequired || ResultDisplayActive && !pcGamePayment.prepared),
                "result wait paid early, lost its projected meters, or exposed history/End");
            DeferredPaymentSmokeObserve("UnpaidObserved");
        }

        private void RequireDeferredPaymentSmokeStillWaiting()
        {
            if (!DeferredPaymentSmokeEnabled || pcResultDisplayWait == null || pcResultDisplayWait.Completed) return;
            RequireDeferredPaymentSmoke(PcPaymentTracked && !pcGamePayment.prepared &&
                !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                session.Credit == pcGamePayment.creditBefore && CommittedHistoryCount() == 0 && sourceRoundEndCount == 0,
                "a pending 35ms/500ms result wait crossed StatePayout");
        }

        private IEnumerator WaitForDeferredPaymentSmokeIdle(Func<bool> condition, string label)
        {
            bool wasEnabled = sourceInputModule.enabled;
            sourceInputModule.enabled = true;
            long deadline = OriginalRackClock.AfterMilliseconds(Stopwatch.GetTimestamp(), 10000, Stopwatch.Frequency);
            try
            {
                while (!condition() && Stopwatch.GetTimestamp() < deadline)
                {
                    RequireDeferredPaymentSmoke(!pcAutoPlayStorageFaulted, "fault during " + label);
                    RequireDeferredPaymentSmokeStillWaiting();
                    yield return null;
                }
                RequireDeferredPaymentSmoke(condition(), "timeout at " + label);
            }
            finally { sourceInputModule.enabled = wasEnabled; }
        }

        private void CompleteDeferredPaymentSmoke(long creditBefore, long award, long wager,
            bool successorSpin, long finalDeposit = 0)
        {
            if (!DeferredPaymentSmokeEnabled) return;
            long successorWager = successorSpin ? wager : 0;
            RequireDeferredPaymentSmoke(PcPaymentTracked && pcGamePayment.IsResolved && pcGamePayment.historyPublished &&
                pcGamePayment.creditBefore == creditBefore && pcGamePayment.baseAward == award &&
                pcGamePayment.finalAward == award && !((RouletteSession)session).RoundPayoutPending &&
                session.Credit == creditBefore + award + finalDeposit - successorWager &&
                session.Returned == award && session.Wagered == wager && session.Rounds == 1 &&
                sourceRoundEndCount == 1 && CommittedHistoryCount() == 1 && recordedRoundPages.Count == 1 &&
                recordedRoundPages[0].creditAfter == creditBefore + award &&
                !recordedRoundPages[0].awaitingFinalPayout &&
                (!successorSpin || spinning && pendingSpin != null && pendingSpin.wager == wager &&
                    pendingSpin.creditBefore == creditBefore + award &&
                    pendingSpin.baseLatestRoundId == pcGamePayment.roundArchiveId),
                "final money, one publication/End, or successor debit differs");
            DeferredPaymentSmokeObserve("CompleteObserved");
            var proof = new DeferredPaymentSmokeProof { testCase = deferredPaymentSmokeCase,
                historyKey = recordedHistoryKey, output = deferredPaymentSmokeOutput,
                frequency = Stopwatch.Frequency, creditBefore = creditBefore, award = award,
                wager = wager, finalDeposit = finalDeposit, successorSpin = successorSpin,
                observations = deferredPaymentSmokeObservations.ToArray() };
            byte[] bytes = new UTF8Encoding(false).GetBytes(JsonUtility.ToJson(proof, true) + "\n");
            using (FileStream stream = new FileStream(deferredPaymentSmokeOutput, FileMode.CreateNew,
                FileAccess.Write, FileShare.Read)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            deferredPaymentSmokeRecording = false;
            pcLifecycleCheckpoint = deferredPaymentSmokePriorCheckpoint;
            Debug.Log("ROULETTE_DEFERRED_PAYMENT_SMOKE_PASSED: case=" + deferredPaymentSmokeCase +
                " creditBefore=" + creditBefore + " award=" + award + " wager=" + wager +
                " finalCredit=" + session.Credit + " ends=" + sourceRoundEndCount +
                " proof=" + RoundSmokeSha(Encoding.UTF8.GetString(bytes)));
        }

        private IEnumerator DeferredPaymentSmokePromptRound()
        {
            if (!DeferredPaymentSmokeEnabled) yield break;
            // The preceding prompt epoch matrix is a synchronous clock/UI
            // diagnostic. Exercise one genuine journalled round separately.
            EnableDeferredPaymentSmokeFixture();
            yield return StateEndUntil(() => sourceGameplayState == SourceGameplayState.Idle &&
                !configuredAutoPlayCheckPending && !SourceRoundBlocksOrdinaryInput,
                "deferred prompt initial Idle");
            for (int pocket = 0; pocket <= 36; ++pocket) session.PlaceBet(100 + pocket, 1);
            SaveStagedBetsIfChanged();
            BeginSpin();
            yield return StateEndUntil(() => session.Rounds == 1 && !winRackActive && SourceDoubleOfferActive,
                "deferred prompt unpaid result");
            ObserveDeferredPaymentSmokeUnpaid(963, 36, 37, false);
            yield return StateEndWaitForIdle(true);
            CompleteDeferredPaymentSmoke(963, 36, 37, false);
        }
    }
}
