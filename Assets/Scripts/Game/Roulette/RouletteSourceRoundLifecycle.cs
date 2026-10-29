using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // This is a PC execution ticket, not another NV game-state record.
        // Playing spans the native StatePlay offer, feature and result display.
        private enum SourceRoundPhase
        {
            None, Wheel, BaseRollup, DoubleOffer, DoublePlaying, DoubleExit, ResultDisplay
        }
        private enum SourceDoubleOfferChoice { None, Take, Enter }
        private SourceRoundPhase sourceRoundPhase;
        private SourceDoubleOfferChoice sourceDoubleOfferChoice;
        private bool sourceBaseHasDoubleOffer;
        private bool sourceDispatchingRackInput;
        private int sourceRoundEndCount;
        private long sourceOfferPromptStartedTimestamp;
        private long sourceEndingDelayPromptDeadline;
        private string sourceEndingDelayPromptPending = "";
        private string sourceEndingDelayPromptMessage = "";

        private bool SourceDoubleOfferActive
        {
            get { return sourceBaseHasDoubleOffer &&
                sourceDoubleOfferChoice == SourceDoubleOfferChoice.None &&
                (sourceRoundPhase == SourceRoundPhase.BaseRollup ||
                    sourceRoundPhase == SourceRoundPhase.DoubleOffer) &&
                session != null && session.DoubleAvailable; }
        }

        private bool SourceRoundBlocksOrdinaryInput
        {
            get { return sourceRoundPhase != SourceRoundPhase.None || LifecycleBlocksPlayerInput; }
        }

        private bool SourceOfferDecisionDispatchAllowed
        {
            get { return SourceDoubleOfferActive &&
                (sourceRoundPhase != SourceRoundPhase.BaseRollup ||
                    sourceDispatchingRackInput); }
        }

        private bool SourceOfferPlayAllowed
        {
            // An unpaid base award participates in native offer eligibility.
            // Legacy paid-base rounds retain their original wallet basis.
            get { return SourceDoubleOfferActive && session.TotalBet > 0 &&
                (decimal)session.TotalBet <= (decimal)session.Credit +
                    (PcUnpaidBaseProjection ? PcUnpaidBaseProjectionAward : 0) &&
                session.TotalBet <= session.Profile.MaximumTotalBet &&
                !PaytableTestActive && !OperatorFallbackActive && !pcCollectPending; }
        }

        private void ArmSourceRoundAfterCommit()
        {
            pcLifecycleColdRound = false;
            sourceRoundPhase = SourceRoundPhase.Wheel;
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.None;
            sourceBaseHasDoubleOffer = false;
            sourceOfferPromptStartedTimestamp = 0;
            ClearSourceEndingDelayPrompt();
            sourceGameplayState = SourceGameplayState.Playing;
            sourceBetStateEntryPending = false;
            gameOverMessageLatched = false;
        }

        private void RestoreSourceRoundLifecycle()
        {
            sourceRoundPhase = SourceRoundPhase.None;
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.None;
            sourceBaseHasDoubleOffer = false;
            sourceDispatchingRackInput = false;
            sourceOfferPromptStartedTimestamp = 0;
            ClearSourceEndingDelayPrompt();
            configuredAutoPlayFinishPending = false;
            configuredAutoPlayCheckPending = false;
            if (session == null) return;
            if (RestorePersistedRoundLifecycle()) return;
            if (resumePendingSpin && session.SpinPending && pendingSpin != null)
            {
                ArmSourceRoundAfterCommit();
                pcLifecycleColdRound = true;
                return;
            }
            if (recordedRoundPages.Count == 0) return;
            RecordedRound round = recordedRoundPages[0];
            // LoadProfile has already validated and reconstructed this exact
            // archive. A completed feature with a stale awaiting bit is
            // normalized by TryRestoreArchivedRound, never replayed as End.
            if (round == null || !round.present || !round.awaitingFinalPayout ||
                round.profile != session.Profile.Id || session.LastResult == null ||
                round.pocket != session.LastResult.Pocket ||
                (round.doubleUp != null && round.doubleUp.complete) ||
                (!session.DoubleAvailable && !session.InDoubleUp)) return;
            sourceBaseHasDoubleOffer = true;
            sourceGameplayState = SourceGameplayState.Playing;
            sourceBetStateEntryPending = false;
            sourceTableInputEnabled = false;
            gameOverMessageLatched = false;
            if (session.InDoubleUp)
                sourceRoundPhase = SourceRoundPhase.DoublePlaying;
            else
            {
                sourceRoundPhase = SourceRoundPhase.DoubleOffer;
                StartSourceOfferPrompt();
            }
        }

        private void BeginSourceBaseResult(RoundResult result)
        {
            if (sourceRoundPhase == SourceRoundPhase.None) return;
            // StatePlay decides on Double before the StateDoubleUp rollup,
            // including before a queued event can reset configured AutoData.
            ResolveSourceDoubleOffer();
            sourceBaseHasDoubleOffer = session.DoubleAvailable && DoubleUpInputAllowed;
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.None;
            sourceGameplayState = SourceGameplayState.Playing;
            sourceTableInputEnabled = false;
            gameOverMessageLatched = false;
            sourceRoundPhase = result.Returned > 0 ?
                SourceRoundPhase.BaseRollup : SourceRoundPhase.ResultDisplay;
            if (sourceBaseHasDoubleOffer)
            {
                // StateDoubleUp's initial purge precedes PerformRollup.
                PurgeSourcePlayerInputs();
                StartSourceOfferPrompt();
            }
        }

        // True means the caller must not start StateDisplayResults yet.
        // Full paid/credit meters have already been published by the caller.
        private bool CompleteSourceBaseRollup()
        {
            if (sourceRoundPhase != SourceRoundPhase.BaseRollup) return false;
            if (!sourceBaseHasDoubleOffer)
            {
                sourceRoundPhase = SourceRoundPhase.ResultDisplay;
                return false;
            }
            if (sourceDoubleOfferChoice == SourceDoubleOfferChoice.Enter)
                StartSourceDoubleFeature();
            else if (sourceDoubleOfferChoice == SourceDoubleOfferChoice.Take)
                BeginSourceRoundResultDisplay();
            else
                sourceRoundPhase = SourceRoundPhase.DoubleOffer;
            return true;
        }

        private void StartSourceOfferPrompt()
        {
            // ProView state 4 arms its first show after a 1 ms timer, then
            // flashes the native phrase for 2500 ms on / 250 ms off.
            sourceOfferPromptStartedTimestamp = OriginalRackClock.AfterMilliseconds(
                Stopwatch.GetTimestamp(), 1, Stopwatch.Frequency);
            proViewFlashStartedTimestamp = 0;
        }

        private string SourceOfferFlashingMessage(long now)
        {
            if (!SourceDoubleOfferActive || sourceOfferPromptStartedTimestamp == 0 ||
                now < sourceOfferPromptStartedTimestamp || paysPage >= 0) return "";
            long elapsed = OriginalRackClock.ElapsedMillisecondsLong(
                sourceOfferPromptStartedTimestamp, now, Stopwatch.Frequency);
            return elapsed % 2750L < 2500L ? "Press Gamble to Gamble" : "";
        }

        private void HandleSourceOfferPlay()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!SourceOfferDecisionDispatchAllowed || !SourceOfferPlayAllowed) return;
            if (paysPage >= 0)
            {
                ClosePays();
                CloseGameInfoOptions();
                PurgeSourcePlayerInputs();
                return;
            }
            TakeSourceDoubleOffer();
        }

        private void ServiceSourceEndingDelayPrompt(long now)
        {
            // Native DisplayPleaseWaitMessage sets state 9 on every loop.
            // SetState re-arms 1 ms even for the same state, but preserves
            // the last visible label until Flash publishes the next text.
            // Service the previous timer before the next loop re-arms it.
            if (sourceEndingDelayPromptDeadline != 0 &&
                now >= sourceEndingDelayPromptDeadline)
                sourceEndingDelayPromptMessage = sourceEndingDelayPromptPending;
            long remaining = gameDelayMilliseconds - OriginalRackClock.ElapsedMillisecondsLong(
                gameStartTimestamp, now, Stopwatch.Frequency);
            sourceEndingDelayPromptPending = "Please wait... " + (remaining / 1000L + 1L) + "s";
            sourceEndingDelayPromptDeadline = OriginalRackClock.AfterMilliseconds(now, 1, Stopwatch.Frequency);
        }

        private void ClearSourceEndingDelayPrompt()
        {
            sourceEndingDelayPromptDeadline = 0;
            sourceEndingDelayPromptPending = "";
            sourceEndingDelayPromptMessage = "";
        }

        private void TakeSourceDoubleOffer()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!SourceOfferDecisionDispatchAllowed || paysPage >= 0) return;
            PersistPcGameOffer(false);
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.Take;
            sourceOfferPromptStartedTimestamp = 0;
            ((RouletteSession)session).DeclineDoubleOffer();
            // The choice owns this purge. Finishing an unselected rack does
            // not purge a queued release or implicitly dismiss the offer.
            PurgeSourcePlayerInputs();
            if (sourceRoundPhase != SourceRoundPhase.BaseRollup)
                BeginSourceRoundResultDisplay();
        }

        private void EnterSourceDoubleOffer()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!SourceOfferDecisionDispatchAllowed || paysPage >= 0) return;
            PersistPcGameOffer(true);
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.Enter;
            sourceOfferPromptStartedTimestamp = 0;
            PurgeSourcePlayerInputs();
            if (sourceRoundPhase != SourceRoundPhase.BaseRollup)
                StartSourceDoubleFeature();
        }

        private void StartSourceDoubleFeature()
        {
            // Native decision state3 rechecks active Auto/Turbo after an
            // earlier selection and before invoking the feature.
            if (autoplay || configuredAutoPlay.TurboAutoPlay)
            {
                PersistPcGameOffer(false);
                ((RouletteSession)session).DeclineDoubleOffer();
                sourceDoubleOfferChoice = SourceDoubleOfferChoice.Take;
                BeginSourceRoundResultDisplay();
                return;
            }
            // Called after the full base rollup; the model preserves the
            // current archive's explicit paid or unpaid stake basis.
            BeginDoubleCore();
            sourceRoundPhase = SourceRoundPhase.DoublePlaying;
            sourceGameplayState = SourceGameplayState.Playing;
            sourceTableInputEnabled = false;
        }

        private void BeginSourceRoundResultDisplay()
        {
            sourceRoundPhase = SourceRoundPhase.ResultDisplay;
            sourceOfferPromptStartedTimestamp = 0;
            // DoubleUpComplete changes the current result's final award.
            // +514f already suppresses a second visible rollup.
            long paid = PcCurrentRoundAward;
            if (PcDoubleGamePayoutPending)
            {
                // StateDoubleUp case9 reconstructs the cycle from the received
                // result. Case11 displays credits+award; neither pays money.
                resultCycleStartedAt = paid > 0 && !TurboAutoPlayActive ? Time.realtimeSinceStartup : -1f;
                resultCycleStartedTimestamp = paid > 0 && !TurboAutoPlayActive ? Stopwatch.GetTimestamp() : 0;
                EnsurePcResultCycle();
                LifecycleCheckpoint("DoubleGameResultPresented");
            }
            BeginSourceResultDisplay(paid, false);
            ServiceSourceRoundLifecycle();
        }

        private void MarkSourceDoubleExit()
        {
            if (sourceRoundPhase == SourceRoundPhase.DoublePlaying)
                sourceRoundPhase = SourceRoundPhase.DoubleExit;
        }

        internal void ServiceSourceRoundLifecycle()
        {
            if (!ServicePcSasHostMessagesV1()) return;
            if(sasStartupRuntimePending){
                ServiceSasBaseOfferV4();
                // Double entry is restored, but decisions await its V4 receipt
                // controller. Base payout must not consume that feature owner.
                if(pcSasGamePaymentV2!=null&&pcSasGamePaymentV2.owner==PcGamePaymentOwner.Double)return;
                if(latestRoundLifecycle!=null&&latestRoundLifecycle.roundStage==PcRoundStage.Ended)ServiceSasIdleRuntimeV4(false);
                else if(latestRoundLifecycle!=null&&(latestRoundLifecycle.roundStage==PcRoundStage.AwaitEnd||latestRoundLifecycle.roundStage==PcRoundStage.EndPrepared))ServiceSasStartupRoundEndV4();
                else if(ServiceSasStartupBasePresentationV4())ServiceSasStartupBasePaymentV4();return;
            }
            // A selected Game with the packaged iDeck keep-alive service can
            // leave Idle inside StatePlay while App is suspended. A received
            // Feature result may therefore reach result display and its waits.
            // RunLoop's App gate still precedes StatePayout below.
            if (PcGameAppSuspended && (!pcAutoCompleteGameSelected ||
                (sourceRoundPhase != SourceRoundPhase.ResultDisplay && !PcDoubleGamePayoutPending))) return;
            try { ServiceSourceRoundLifecycleCore(); }
            catch (Exception error)
            {
                if (!HasTrustedLatestLifecycle) throw;
                FailPcRoundLifecycle("advanced", error);
            }
        }

        private void ServiceSourceRoundLifecycleCore()
        {
            if (pcAutoPlayStorageFaulted) return;
            if (session == null || PaytableTestActive || OperatorFallbackActive) return;
            if (HasTrustedLatestLifecycle && !pcLifecycleRuntimeReady) return;
            if (pcPaymentResumeEnter)
            {
                if (PcGameAppSuspended) return;
                pcPaymentResumeEnter = false;
                StartSourceDoubleFeature();
                return;
            }
            if (ServicePersistedRoundPresentation()) return;
            ServicePcResultDisplayWait();
            if (PcNativeDoubleFeatureBlocksCompletion) return;
            if (sourceRoundPhase == SourceRoundPhase.DoublePlaying &&
                (!session.InDoubleUp || PcDoubleGamePayoutPending))
                sourceRoundPhase = SourceRoundPhase.DoubleExit;
            if (sourceRoundPhase == SourceRoundPhase.DoubleExit)
            {
                if (session.InDoubleUp && !PcDoubleGamePayoutPending ||
                    doubleCollectPending || IsDoubleReveal || IsDoubleExit)
                    return;
                BeginSourceRoundResultDisplay();
            }
            if (sourceRoundPhase != SourceRoundPhase.ResultDisplay || spinning ||
                session.SpinPending || winRackActive || ResultDisplayActive ||
                session.DoubleAvailable || session.InDoubleUp && !PcDoubleGamePayoutPending || doubleCollectPending ||
                IsDoubleReveal || IsDoubleExit) return;
            // FeatureComplete is an independent received result. The main
            // loop reaches StatePayout only after StateDisplayResults returns.
            // Keep native0/Prepared durable while App or result waits block it.
            gameOverMessageLatched = true;
            if (PcPaymentTracked)
            {
                if (!ServicePcGamePayment()) return;
            }
            else
            {
                if (PcGameAppSuspended) return;
                if (PcDoubleGamePayoutPending) ApplyPcDoubleGamePayout();
                FinalizePendingHistoryRound();
            }
            if (PcGameAppSuspended) return;
            PersistAwaitEnd();
            long now = Stopwatch.GetTimestamp();
            if (!pcLifecycleColdRound && !OriginalRackClock.GameDelayExpired(gameStartTimestamp,
                now, gameDelayMilliseconds, Stopwatch.Frequency))
            {
                ServiceSourceEndingDelayPrompt(now);
                return;
            }
            if (sourceEndingDelayPromptDeadline != 0)
            {
                ClearSourceEndingDelayPrompt();
                // GameEndingDelay leaves the common flashing label in state 0.
                proViewFlashStartedTimestamp = 0;
            }
            CompleteSourceRoundEnd();
        }

        private void CompleteSourceRoundEnd()
        {
            if (sourceRoundPhase != SourceRoundPhase.ResultDisplay) return;
            if (HasTrustedLatestLifecycle)
            {
                CompletePersistedRoundEnd();
                return;
            }
            ExecuteSourceRoundEndV2(CreateSourceRoundEndIntentV2(Guid.NewGuid().ToString("N")), false);
            PublishSourceRoundEnd();
        }

        private void ResetSourceSelectedDemoOption()
        {
            // None of the forty supported show-type-1 menu choices uses
            // StateEnd's reserved enum-7 exception.
            if (demoShowMode <= 0 || demoMenuState == 0 || demoOutcome == null) return;
            demoOutcome.ClearSelection();
            ClearSourceSelectedDemoText();
        }

        internal void ServiceSourcePlayerInput()
        {
            if (pcAutoPlayStorageFaulted || PcGameOrdinaryInputBlocked) return;
            // ResultDisplay's WaitInput owns the queue while one of its
            // 35 ms waits or its non-interruptible 500 ms Delay is pending.
            if (pcResultDisplayWait != null && !pcResultDisplayWait.Completed) return;
            if (winRackActive && !autoplay)
            {
                if (!SourcePlayerInputs.InputAvailable(0xffdfffee, 0) ||
                    (!pcLifecycleColdRound && !OriginalRackClock.GameDelayExpired(gameStartTimestamp,
                        Stopwatch.GetTimestamp(), gameDelayMilliseconds, Stopwatch.Frequency)))
                    return;
                sourceDispatchingRackInput = true;
                try { SourcePlayerInputs.DispatchOne(); }
                finally { sourceDispatchingRackInput = false; }
                if (pcAutoPlayStorageFaulted || PcGameOrdinaryInputBlocked) return;
                if (winRackActive)
                {
                    winRackSnapped = true;
                    FinishWinRacking();
                    Debug.Log("ROULETTE_RACK_INPUT_SNAP: paid=" + winRackPaid +
                        " credit=" + session.Credit);
                }
                ServiceSourceRoundLifecycle();
                return;
            }
            SourcePlayerInputs.DispatchOne();
        }
    }
}
