using System;
using System.Diagnostics;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private OriginalDoubleFeatureController pcDoubleFeature;
        private long pcDoubleFeatureDue;
        private int pcDoubleFeatureDueOpcode;
        private long pcDoubleFeatureDueGeneration;
        private long pcDoubleFeatureArmedAt;
        private int pcDoubleFeatureDelay;
        private OriginalDoubleFeatureView pcDoubleFeatureView;
        private bool pcDoubleFeatureLegacyUnknown;

        private bool PcNativeDoubleFeatureTracked { get { return pcDoubleFeature != null; } }
        private bool PcNativeDoubleFeatureDecisionReady { get { return pcDoubleFeature != null &&
            pcDoubleFeature.DecisionReady; } }
        private bool PcNativeDoubleFeatureBlocksCompletion { get { return pcDoubleFeature != null &&
            pcDoubleFeature.Active; } }
        private bool PcNativeDoubleFeatureVisible { get { return pcDoubleFeature != null &&
            pcDoubleFeature.Active && !pcDoubleFeature.Suspended; } }
        private bool PcNativeDoubleFeatureAutoTake { get { return PcNativeDoubleFeatureVisible &&
            pcDoubleFeatureView == OriginalDoubleFeatureView.AutoTake; } }
        private bool PcNativeDoubleFeatureReveal { get { return PcNativeDoubleFeatureVisible &&
            (pcDoubleFeatureView == OriginalDoubleFeatureView.RevealWin ||
             pcDoubleFeatureView == OriginalDoubleFeatureView.RevealLoss ||
             pcDoubleFeatureView == OriginalDoubleFeatureView.RevealMaximum); } }
        private bool PcNativeDoubleFeatureExiting { get { return PcNativeDoubleFeatureVisible &&
            pcDoubleFeatureView == OriginalDoubleFeatureView.Exit; } }
        private bool DoubleFeaturePresentationVisible { get { return PcNativeDoubleFeatureTracked ?
            PcNativeDoubleFeatureVisible : session != null &&
            (session.InDoubleUp || IsDoubleReveal || IsDoubleExit); } }

        private static PcDoubleFeatureNvV1 ClonePcDoubleFeatureNv(PcDoubleFeatureNvV1 value)
        {
            return value == null ? null : value.Clone();
        }

        private static string PcDoubleFeatureResultDigest(RecordedRound round)
        {
            if (round == null || round.doubleUp == null)
                throw new PcRoundLifecycleDataException("Double Feature digest has no feature record");
            return PcReceiptTextDigest(JsonUtility.ToJson(round.doubleUp));
        }

        // Must run before the legacy page validator can normalize money.
        // This helper does not change the page or calculate financial results.
        private static void ValidatePcDoubleFeatureNv(PcDoubleFeatureNvV1 nv,
            RecordedRound latest, PcRoundLifecycleRecord lifecycle)
        {
            if (nv == null) return;
            nv.Validate();
            RequireLifecycle(latest != null && lifecycle != null && latest.hasDoubleUp &&
                latest.doubleUp != null && latest.doubleUp.steps != null &&
                nv.roundArchiveId == latest.roundArchiveId &&
                nv.spinJournalId == latest.spinJournalId && nv.profile == latest.profile &&
                nv.roundArchiveId == lifecycle.roundArchiveId &&
                nv.spinJournalId == lifecycle.spinJournalId && nv.profile == lifecycle.profile,
                "Double Feature NV has no matching current round");
            RecordedDouble feature = latest.doubleUp;
            RequireLifecycle(nv.suitMode == feature.suitMode && nv.stepCount == feature.steps.Length &&
                nv.resultDigest == PcDoubleFeatureResultDigest(latest),
                "Double Feature NV result identity differs from its archive");
            RequireLifecycle(nv.savedState != 6,
                "The PC native Feature writer never persists live exit state6");
            RequireLifecycle(feature.steps.Length == 0 || feature.steps[feature.steps.Length - 1] != null,
                "Double Feature NV references a missing committed step");
            long award = feature.steps.Length == 0 ? feature.startingStake :
                feature.steps[feature.steps.Length - 1].won;
            RequireLifecycle(award >= 0, "Double Feature NV has a negative current award");
            if (nv.completionStage < 2)
            {
                RequireLifecycle(!feature.complete && !feature.tookWin &&
                    lifecycle.roundStage == PcRoundStage.RoundOpen,
                    "An active Double Feature has already completed financially");
                if (nv.savedState == 5 || nv.savedState == 7)
                    RequireLifecycle(award > 0, "A saved Take state has no positive award");
                if (nv.completionStage == 1)
                    RequireLifecycle(nv.completionAward == award,
                        "Double completion intent differs from its committed result");
            }
            else
            {
                RequireLifecycle(feature.complete && feature.finalWin == nv.completionAward,
                    "Applied Double completion differs from its financial page");
                RequireLifecycle(lifecycle.roundStage == PcRoundStage.RoundOpen || nv.savedState == 0,
                    "Round continuation precedes the native Double state0 save");
            }
        }

        private void SavePcDoubleFeatureNv(PcDoubleFeatureNvV1 candidate, string boundary)
        {
            try
            {
                if (recordedRoundPages.Count == 0)
                    throw new PcRoundLifecycleDataException("Double Feature has no archive to save");
                ValidatePcDoubleFeatureNv(candidate, recordedRoundPages[0], latestRoundLifecycle);
                LifecycleBeforeWrite("DoubleFeature:" + boundary);
                if (pcRoundArchiveSchemaVersion != 3) pcRoundArchiveSchemaVersion = 2;
                pcDoubleFeatureNv = candidate.Clone();
                SaveRecordedRoundPages();
                AutoCompleteCheckpoint("Feature:" + boundary);
            }
            catch (Exception error)
            {
                pcDoubleFeatureDue = 0;
                FailPcRoundLifecycle("double-feature:" + boundary, error);
                throw;
            }
        }

        // Call after the new RecordedDouble candidate is populated, in place
        // of its initial SaveRecordedRoundPages. The host performs that save.
        private void BeginPcNativeDoubleFeature(bool auto)
        {
            if (recordedRoundPages.Count == 0 || !HasTrustedLatestLifecycle)
                throw new PcRoundLifecycleDataException("Native Double requires a current round identity");
            RecordedRound round = recordedRoundPages[0];
            if (round.doubleUp == null || round.doubleUp.steps == null ||
                round.doubleUp.steps.Length != 0 || round.doubleUp.complete)
                throw new PcRoundLifecycleDataException("Native Double has no new feature candidate");
            if (pcDoubleFeatureNv != null && pcDoubleFeatureNv.roundArchiveId == round.roundArchiveId)
                throw new PcRoundLifecycleDataException("Native Double instance already exists for this round");
            var candidate = new PcDoubleFeatureNvV1 {
                featureId = Guid.NewGuid().ToString("N"), roundArchiveId = round.roundArchiveId,
                spinJournalId = round.spinJournalId, profile = round.profile,
                savedState = 2, suitMode = round.doubleUp.suitMode, stepCount = 0,
                resultDigest = PcDoubleFeatureResultDigest(round)
            };
            ValidatePcDoubleFeatureNv(candidate, round, latestRoundLifecycle);
            if (PcPaymentTracked)
                pcGamePayment = PcGamePaymentTransitions.TransferToDouble(pcGamePayment, candidate.featureId);
            pcDoubleFeatureLegacyUnknown = false;
            pcDoubleFeature = new OriginalDoubleFeatureController(candidate, new PcDoubleFeatureHost(this));
            pcDoubleFeature.Begin(auto);
        }

        // The candidate step has been filled by the existing receipt path.
        // The null-controller case is D1 reconciliation before live UI startup.
        private void RecordPcNativeDoubleChoice(bool won, bool terminal)
        {
            if (recordedRoundPages.Count == 0 || recordedRoundPages[0].doubleUp == null)
                throw new PcRoundLifecycleDataException("Native Double result has no page");
            RecordedDouble feature = recordedRoundPages[0].doubleUp;
            int count = feature.steps == null ? -1 : feature.steps.Length;
            string digest = PcDoubleFeatureResultDigest(recordedRoundPages[0]);
            if (pcDoubleFeature != null)
            {
                pcDoubleFeature.ChoiceCommitted(won, terminal, count, digest);
                return;
            }
            RequireLifecycle(pcDoubleFeatureNv != null && pcDoubleFeatureNv.completionStage == 0 &&
                pcDoubleFeatureNv.savedState == 2 && count == pcDoubleFeatureNv.stepCount + 1 &&
                count > 0 && count <= 16 && (won || terminal),
                "Recovered native Double result has no preceding ready phase");
            PcDoubleFeatureNvV1 candidate = pcDoubleFeatureNv.Clone();
            candidate.stepCount = count;
            candidate.resultDigest = digest;
            SavePcDoubleFeatureNv(candidate, "ChoiceResultSaved");
            candidate.savedState = 3;
            SavePcDoubleFeatureNv(candidate, "RevealSaved3");
        }

        private void BeginPcNativeDoubleTake()
        {
            if (pcDoubleFeature == null)
                throw new PcRoundLifecycleDataException("Native Double take lacks its live phase");
            pcDoubleFeature.Take(false);
        }

        private void ResetPcNativeDoubleFeatureForSuccessor()
        {
            RequireLifecycle(pcDoubleFeatureNv == null || pcDoubleFeatureNv.completionStage == 2 &&
                pcDoubleFeatureNv.savedState == 0, "A successor cannot replace an unfinished native Double");
            pcDoubleFeatureNv = null;
            pcDoubleFeature = null;
            pcDoubleFeatureDue = 0;
            pcDoubleFeatureLegacyUnknown = false;
        }

        // Call after financial/RNG reconciliation and before ordinary input
        // dispatch. A v1 archive cannot establish its lost native phase.
        private void RestorePcNativeDoubleFeature()
        {
            pcDoubleFeature = null;
            pcDoubleFeatureDue = 0;
            pcDoubleFeatureLegacyUnknown = false;
            // V4 has an independent archive writer and RNG context admission.
            // The legacy Feature host must not downgrade its saved owner to V2.
            if (pcRoundArchiveSchemaVersion == 4) return;
            if (recordedRoundPages.Count == 0 || session == null) return;
            RecordedRound latest = recordedRoundPages[0];
            if (pcDoubleFeatureNv == null)
            {
                pcDoubleFeatureLegacyUnknown = latest.hasDoubleUp && latest.doubleUp != null &&
                    !latest.doubleUp.complete;
                return;
            }
            ValidatePcDoubleFeatureNv(pcDoubleFeatureNv, latest, latestRoundLifecycle);
            pcDoubleFeature = new OriginalDoubleFeatureController(pcDoubleFeatureNv,
                new PcDoubleFeatureHost(this));
            pcDoubleFeature.Resume(configuredAutoPlay.AutoPlay != 0);
        }

        private void SuspendPcAutoCompleteFeatures()
        {
            if (pcDoubleFeature != null && pcDoubleFeature.Active)
            {
                pcDoubleFeature.Suspend();
                ClearPcDoubleFeaturePresentation();
                AutoCompleteCheckpoint("Feature:Suspended");
            }
        }

        private void ResumePcAutoCompleteFeatures()
        {
            if (pcRoundArchiveSchemaVersion == 4 && sasStartupRuntimePending) return;
            if (pcDoubleFeatureLegacyUnknown || pcDoubleFeature == null && session != null &&
                session.InDoubleUp)
                throw new PcRoundLifecycleDataException("Automatic Feature resume lacks a recorded native phase");
            if (pcDoubleFeature == null || !pcDoubleFeature.Active) return;
            pcDoubleFeature.Resume(configuredAutoPlay.AutoPlay != 0);
            AutoCompleteCheckpoint("Feature:Resumed");
        }

        private void ServicePcAutoCompleteFeatureContinuation()
        {
            if (pcDoubleFeature == null || pcDoubleFeature.Faulted || pcAutoPlayStorageFaulted ||
                pcDoubleFeatureDue == 0 || Stopwatch.GetTimestamp() < pcDoubleFeatureDue) return;
            int opcode = pcDoubleFeatureDueOpcode;
            long generation = pcDoubleFeatureDueGeneration;
            AutoCompleteCheckpoint("Feature:TimerDelivering");
            pcDoubleFeatureDue = 0;
            bool consumed = pcDoubleFeature.DeliverTimer(opcode, generation);
            AutoCompleteCheckpoint("Feature:Timer:" + opcode + ":" + consumed);
        }

        private void ClearPcDoubleFeaturePresentation()
        {
            showingDoubleHelp = false;
            doubleHelpUntilTimestamp = 0;
            doubleCollectPending = false;
            doubleRevealUntil = 0;
            doubleRevealUntilTimestamp = 0;
            doubleExitStartedAt = float.PositiveInfinity;
            doubleExitUntil = float.PositiveInfinity;
            doubleExitStartedTimestamp = long.MaxValue;
            doubleExitUntilTimestamp = long.MaxValue;
            doubleIdleAnimationStartedAt = float.PositiveInfinity;
            doubleIdleAnimationStartedTimestamp = long.MaxValue;
        }

        private void PresentPcDoubleFeature(OriginalDoubleFeatureView view, int duration)
        {
            pcDoubleFeatureView = view;
            long now = Stopwatch.GetTimestamp();
            float nowSeconds = Time.realtimeSinceStartup;
            ClearPcDoubleFeaturePresentation();
            doubleEntranceStartedAt = float.NegativeInfinity;
            if (view == OriginalDoubleFeatureView.Entrance)
            {
                doubleEntranceStartedAt = nowSeconds;
                doubleEntranceStartedTimestamp = now;
            }
            else if (view == OriginalDoubleFeatureView.Ready)
            {
                doubleIdleAnimationStartedAt = nowSeconds;
                doubleIdleAnimationStartedTimestamp = now;
            }
            else if (view == OriginalDoubleFeatureView.Take || view == OriginalDoubleFeatureView.AutoTake)
            {
                doubleCollectPending = true;
                if (doubleButtonPressSound != null) audioSource.PlayOneShot(doubleButtonPressSound, .7f);
            }
            else if (view == OriginalDoubleFeatureView.RevealWin ||
                view == OriginalDoubleFeatureView.RevealLoss || view == OriginalDoubleFeatureView.RevealMaximum)
            {
                RecordedDouble feature = recordedRoundPages[0].doubleUp;
                RecordedDoubleStep step = feature.steps[feature.steps.Length - 1];
                doubleRevealChoice = step.choice == 5 ? -1 : step.choice == 4 ? -2 :
                    step.choice == 3 ? 0 : step.choice == 2 ? 1 : step.choice == 0 ? 2 : 3;
                doubleRevealWon = view != OriginalDoubleFeatureView.RevealLoss;
                doubleRevealMaximum = view == OriginalDoubleFeatureView.RevealMaximum;
                doubleRevealUntil = nowSeconds + duration / 1000f;
                doubleRevealUntilTimestamp = OriginalRackClock.AfterMilliseconds(now,
                    duration, Stopwatch.Frequency);
                AudioClip sound = doubleRevealWon ? doubleWinSound : doubleLoseSound;
                if (sound != null) audioSource.PlayOneShot(sound, .7f);
            }
            else if (view == OriginalDoubleFeatureView.Exit)
            {
                doubleExitStartedAt = nowSeconds;
                doubleExitUntil = nowSeconds + duration / 1000f;
                doubleExitStartedTimestamp = now;
                doubleExitUntilTimestamp = OriginalRackClock.AfterMilliseconds(now,
                    duration, Stopwatch.Frequency);
                MarkSourceDoubleExit();
            }
            AutoCompleteCheckpoint("Feature:View:" + view);
        }

        private sealed class PcDoubleFeatureHost : IOriginalDoubleFeatureHost
        {
            private readonly RouletteGame game;
            internal PcDoubleFeatureHost(RouletteGame game) { this.game = game; }
            public long CurrentAward { get { return game.PcNativeDoubleCurrentAward; } }
            public bool MaximumReached { get { return game.PcNativeDoubleMaximumReached; } }
            public bool AuNationalStandards { get { return game.auNationalStandardsEnabled; } }
            public int ExitDelayMilliseconds { get { return game.auNationalStandardsEnabled ||
                game.session.DoubleSuitMode ? 10 : 1000; } }
            public void SaveNativeState(PcDoubleFeatureNvV1 state, string boundary)
            {
                game.SavePcDoubleFeatureNv(state, boundary);
            }
            public void SetTimer(int delayMilliseconds, int opcode, long generation)
            {
                game.pcDoubleFeatureDueOpcode = opcode;
                game.pcDoubleFeatureDueGeneration = generation;
                game.pcDoubleFeatureArmedAt = Stopwatch.GetTimestamp();
                game.pcDoubleFeatureDelay = delayMilliseconds;
                game.pcDoubleFeatureDue = delayMilliseconds == 0 ? 0 : OriginalRackClock.AfterMilliseconds(
                    game.pcDoubleFeatureArmedAt, delayMilliseconds, Stopwatch.Frequency);
                game.AutoCompleteCheckpoint(delayMilliseconds == 0 ? "Feature:TimerCleared" : "Feature:TimerArmed");
            }
            public void Present(OriginalDoubleFeatureView view, int durationMilliseconds)
            {
                game.PresentPcDoubleFeature(view, durationMilliseconds);
            }
            public PcDoubleFeatureNvV1 CommitFeatureCompletion(PcDoubleFeatureNvV1 prepared)
            {
                try { return game.CommitPcNativeDoubleCompletion(prepared); }
                catch (Exception error)
                {
                    game.pcDoubleFeatureDue = 0;
                    game.FailPcRoundLifecycle("double-feature-completion", error);
                    throw;
                }
            }
        }
    }
}
