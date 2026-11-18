using System;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private OriginalAutoPlayData configuredAutoPlay = new OriginalAutoPlayData();
        private readonly OriginalPlayerInputQueue sourcePlayerInputs =
            new OriginalPlayerInputQueue();
        private RouletteSourceInputModule sourceInputModule;
        private bool configuredAutoPlayCheckPending;
        private bool configuredAutoPlayFinishPending;
        private long configuredResultDelayDeadlineTimestamp;
        private long configuredResultDelayStartTimestamp;
        private bool sourceTableInputEnabled = true;
        // Game constructs transaction status as true. An eventual platform
        // adapter must supply HandlePlayerTransactionStatus, not infer it from
        // the amount in the PC credit meter.
        private bool pcPlayerTransactionActive = true;

        internal bool UsesSourcePlayerInput
        {
            get { return sourceInputModule != null && !PaytableTestActive &&
                !OperatorFallbackActive; }
        }

        internal OriginalPlayerInputQueue SourcePlayerInputs
        {
            get { return sourcePlayerInputs; }
        }

        internal bool PcGameOrdinaryInputAllowedForModule
        {
            get { return !PcGameOrdinaryInputBlocked; }
        }
        internal bool PcSasRuntimeServicePendingForModule
        {get{return sasStartupRuntimePending&&!pcRoundLifecycleStorageFaulted&&!sasInitialArchiveWriteFaulted;}}

        private bool ConfiguredAutoPlayEligible
        {
            get { return configuredAutoPlay.AutoPlay != 0 &&
                (demoShowMode > 0 || !configuredAutoPlay.DemoOnly) &&
                // 轮盘流程模式下由宿主 FSM 推进一局；这里只关闭自动转动，
                // 仍保留 Idle 交接（idleEntry 收尾）以保证押分窗口可重开。
                !Model.Instance.rouletteFlow; }
        }

        private bool DoubleUpInputAllowed
        {
            get
            {
                // ClearTotalWin replaces the live result with a zero award.
                // The archived award cannot supply a later Double Up offer.
                if (currentGameResultsCleared) return false;
                // Roulette::StatePlay case 0xb uses both configured and
                // active AutoPlay, with a separate auto-complete branch.
                return configuredAutoPlay.AutoPlayOffWhenGameFinished ?
                    configuredAutoPlay.SecsToAutoCompleteStart != 0 :
                    !autoplay && configuredAutoPlay.AutoPlay == 0;
            }
        }

        private bool PurgeSourcePlayerInputs(uint mask = 0, uint value = 0, bool? demoAvailable = null)
        {
            bool restoreSounds;
            bool removed = sourcePlayerInputs.PurgeGameInputs(configuredAutoPlay,
                demoAvailable ?? (demoShowMode > 0), mask, value, out restoreSounds);
            if (restoreSounds)
            {
                SetPcGameSoundsMuted(false);
                Debug.Log("ROULETTE_CONFIGURED_AUTO_INPUT_RESET: active=" + autoplay);
            }
            return removed;
        }

        private void ResolveSourceDoubleOffer()
        {
            if(pcRoundArchiveSchemaVersion==4){ResolveSasBaseOfferEligibilityV4();return;}
            var local = session as RouletteSession;
            if (local == null || local.InDoubleUp || local.SpinPending ||
                (!local.ResultPhase && !currentGameResultsCleared)) return;
            if (local.ResultPhase && !DoubleUpInputAllowed && local.DoubleAvailable)
                local.DeclineDoubleOffer();
            if (PcPaymentTracked)
            {
                ResolvePcGameOfferEligibility();
                return;
            }
            // The PC archives settlement before finishing a visible rack.
            // Persist the later feature decision before state-entry deposits.
            if (recordedRoundPages.Count == 0 || session.LastResult == null) return;
            RecordedRound round = recordedRoundPages[0];
            bool awaiting = local.ResultPhase && local.DoubleAvailable && DoubleUpInputAllowed;
            if (round == null || !round.present || round.pocket != session.LastResult.Pocket ||
                (round.totalsRecorded && round.totalRounds != session.Rounds) ||
                round.awaitingFinalPayout == awaiting) return;
            round.awaitingFinalPayout = awaiting;
            SaveRecordedRoundPages();
        }

        // RunLoop invokes CheckAutoPlay on entry to state 1. Selecting a
        // menu option while its StateIdle loop is running only changes the
        // configuration; it does not request an active AutoPlay toggle.
        internal void ServiceConfiguredAutoPlay()
        {
            if (PcGameAppSuspended) return;
            if (pcAutoPlayStorageFaulted) return;
            ServiceSourceRoundLifecycle();
            if (pcAutoPlayStorageFaulted) return;
            if (HasTrustedLatestLifecycle && configuredAutoPlayCheckPending &&
                sourceRoundPhase == SourceRoundPhase.None && !spinning &&
                session != null && !session.SpinPending && !pcCollectPending &&
                !demoMenuVisible && paysPage < 0 && !PaytableTestActive && !OperatorFallbackActive)
            {
                ServicePersistedIdleEntry();
                return;
            }
            if (!configuredAutoPlayCheckPending || session == null || spinning ||
                SourceRoundBlocksOrdinaryInput ||
                ResultDisplayActive || session.InDoubleUp || IsDoubleReveal ||
                IsDoubleExit || pcCollectPending || demoMenuVisible || paysPage >= 0 ||
                PaytableTestActive || OperatorFallbackActive) return;
            configuredAutoPlayCheckPending = false;
            sourceGameplayState = SourceGameplayState.Idle;
            sourceBetStateEntryPending = false;
            bool preparedSpin = false;
            if (ConfiguredAutoPlayEligible)
            {
                // CheckAutoPlay funds before purging. A queued player event
                // can therefore cancel the configured start after the deposit.
                ReplenishDemoAutoplayCredits();
                PurgeSourcePlayerInputs();
                if (configuredAutoPlay.AutoPlay != 0 || configuredAutoPlay.Turbo)
                {
                    Debug.Log("ROULETTE_CONFIGURED_AUTO_PLAY: active=" + autoplay +
                        " turbo=" + configuredAutoPlay.TurboAutoPlay);
                    preparedSpin = PrepareSourceSpin(true);
                }
            }
            // Roulette::DoPlay disables the table before StartGame checks
            // funds. StateIdle enables it only when configured AutoPlay is
            // zero, including after a queued input cancels that option.
            if (configuredAutoPlay.AutoPlay == 0)
                sourceTableInputEnabled = SourceCreditMeterNonzero;
            // RunLoop still calls StateIdle after CheckAutoPlay, even when
            // that callback accepted a spin or refused it. Its second purge
            // and Demo enable therefore also apply to configured starts.
            PurgeSourcePlayerInputs();
            EnableSourceDemoMode();
            // Native StatePlay's purge and draw follow this StateIdle entry,
            // including when CheckAutoPlay changed the game state to 3.
            if (preparedSpin) CommitSourceSpin();
            SaveStagedBetsIfChanged();
        }

        internal void BeforeSourcePointerDispatch(uint eventType)
        {
            // Rack WaitInput owns dispatch ordering in ServiceSourcePlayerInput.
            // A pointer prefix must not finish/purge before its offer callback.
        }

        internal void DispatchSourceButton(KeyCode key)
        {
            if (pcAutoPlayStorageFaulted || PcGameOrdinaryInputBlocked) return;
            if (key == KeyCode.F8) { ToggleDemoMenu(); return; }
            if (session == null || session.InDoubleUp || ideckPreview ||
                topperPreview || histogramPreview || historyView || demoMenuVisible ||
                IsDoubleReveal || IsDoubleExit) return;
            if (key == KeyCode.Space)
            {
                if (SourceDoubleOfferActive) { HandleSourceOfferPlay(); return; }
                if (spinning) RequestWheelSkillStop();
                else if (SourcePlayInputAllowed) BeginSpin();
            }
            else if (key == KeyCode.D && SourceDoubleOfferActive)
                EnterSourceDoubleOffer();
            else if (key == KeyCode.D && !spinning && !ResultDisplayActive &&
                DoubleUpInputAllowed && session.DoubleAvailable) Try(BeginDouble);
        }

    }
}
