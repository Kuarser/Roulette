using System;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private bool PcDoubleGamePayoutPending { get { return pcDoubleFeature != null &&
            pcDoubleFeatureNv != null && pcDoubleFeatureNv.completionStage == 1 &&
            pcDoubleFeature.GamePayoutPending; } }
        private bool PcDoublePayoutPresentationActive { get { return PcDoubleGamePayoutPending &&
            sourceRoundPhase == SourceRoundPhase.ResultDisplay; } }
        private long PcCurrentRoundAward { get { return PcDoubleGamePayoutPending ?
            pcDoubleFeatureNv.completionAward : session == null || session.LastResult == null ?
            0 : session.LastResult.Returned; } }

        private void ApplyPcDoubleGamePayout()
        {
            RequireLifecycle((!PcGameAppSuspended || PcPaymentTracked && pcGamePayoutEntered) && pcLifecycleRuntimeReady &&
                sourceRoundPhase == SourceRoundPhase.ResultDisplay && !ResultDisplayActive &&
                PcDoubleGamePayoutPending && pcDoubleFeatureNv.savedState == 0 &&
                !pcDoubleFeature.Active && !pcDoubleFeature.TimerRunning,
                "Double payment crossed its ordinary Game StatePayout boundary");
            LifecycleCheckpoint("DoubleGamePayoutStarted");
            pcDoubleFeature.CommitPendingGamePayout();
            RequireLifecycle(!PcDoubleGamePayoutPending && pcDoubleFeatureNv.completionStage == 2 &&
                !session.InDoubleUp, "Double Game payout failed to apply its prepared result");
            LifecycleCheckpoint("DoubleGamePayoutCompleted");
        }

        private long PcNativeDoubleCurrentAward { get { return session == null ? 0 : session.DoubleStake; } }
        private bool PcNativeDoubleMaximumReached { get { return session is RouletteSession &&
            ((RouletteSession)session).DoubleChoiceTerminal && session.DoubleStake > 0; } }

        private PcDoubleFeatureNvV1 CommitPcNativeDoubleCompletion(PcDoubleFeatureNvV1 prepared)
        {
            RequireLifecycle((!PcGameAppSuspended || PcPaymentTracked && pcGamePayoutEntered) && pcLifecycleRuntimeReady &&
                sourceRoundPhase == SourceRoundPhase.ResultDisplay && !ResultDisplayActive,
                "Double financial commit is outside Game StatePayout");
            RequireLifecycle(prepared != null && (pcRoundArchiveSchemaVersion == 2 || pcRoundArchiveSchemaVersion == 3) &&
                pcDoubleFeatureNv != null && recordedRoundPages.Count > 0,
                "Double completion has no versioned feature archive");
            prepared.Validate();
            RequireLifecycle(prepared.completionStage == 1 && prepared.savedState == 0 &&
                prepared.featureId == pcDoubleFeatureNv.featureId &&
                prepared.roundArchiveId == pcDoubleFeatureNv.roundArchiveId &&
                prepared.spinJournalId == pcDoubleFeatureNv.spinJournalId &&
                prepared.profile == pcDoubleFeatureNv.profile &&
                prepared.stepCount == pcDoubleFeatureNv.stepCount &&
                prepared.suitMode == pcDoubleFeatureNv.suitMode &&
                prepared.autoRestoreMarker == pcDoubleFeatureNv.autoRestoreMarker,
                "Double completion identity differs");
            if (pcDoubleFeatureNv.completionStage == 2)
            {
                ValidatePcDoubleFeatureNv(pcDoubleFeatureNv, recordedRoundPages[0], latestRoundLifecycle);
                RequireLifecycle(pcDoubleFeatureNv.completionAward == prepared.completionAward &&
                    recordedRoundPages[0].doubleUp.complete &&
                    recordedRoundPages[0].doubleUp.finalWin == prepared.completionAward,
                    "Double completion replay has a different amount");
                return pcDoubleFeatureNv.Clone();
            }
            RequireLifecycle(JsonUtility.ToJson(prepared) == JsonUtility.ToJson(pcDoubleFeatureNv),
                "Double completion was not durably prepared");
            var local = session as RouletteSession;
            RecordedRound round = recordedRoundPages[0];
            RequireLifecycle(local != null && local.DoubleSettlementDeferred && local.InDoubleUp &&
                round.roundArchiveId == prepared.roundArchiveId && round.spinJournalId == prepared.spinJournalId &&
                round.doubleUp != null && !round.doubleUp.complete &&
                round.doubleUp.steps.Length == prepared.stepCount &&
                local.DoubleStake == prepared.completionAward,
                "Double completion financial state differs from its prepared feature");
            bool take = !local.DoubleChoiceTerminal;
            local.CompleteDoubleFeature(prepared.completionAward);
            round.doubleUp.tookWin = take;
            round.doubleUp.complete = true;
            round.doubleUp.finalWin = prepared.completionAward;
            round.returned = local.LastResult.Returned;
            round.creditAfter = local.Credit;
            round.totalReturned = local.Returned;
            round.totalWins = local.Wins;
            round.awaitingFinalPayout = PcPaymentTracked;
            PcDoubleFeatureNvV1 applied = prepared.Clone();
            applied.completionStage = 2;
            applied.resultDigest = PcDoubleFeatureResultDigest(round);
            // The archive is the authoritative PC money record. Publish the
            // financial page and Applied marker in one durable replacement.
            pcDoubleFeatureNv = applied;
            if (PcPaymentTracked)
                SavePcGamePayment(PcGamePaymentTransitions.Applied(pcGamePayment), "MoneyAppliedSaved");
            else SaveRecordedRoundPages();
            LifecycleCheckpoint("DoubleFeatureCompletionApplied");
            return applied.Clone();
        }
    }
}
