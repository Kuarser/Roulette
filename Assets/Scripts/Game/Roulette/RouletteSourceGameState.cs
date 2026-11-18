namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // This records native state transitions independently of the retained
        // wager and the PC settlement phase. Clear/Undo can leave zero bets in
        // StateBet; an archived wager can remain on the table in StateIdle.
        // Playing is the PC phase spanning native accepted state 3 and state
        // 5's ordinary Play. Only Idle/Bet use these values in staged storage;
        // the PC pending journal starts after the ordinary draw commitment.
        private enum SourceGameplayState { Idle = 1, Bet = 2, Playing = 3 }
        private SourceGameplayState sourceGameplayState = SourceGameplayState.Idle;
        private bool sourceBetStateEntryPending;

        private bool SourceCreditMeterNonzero
        {
            get {
                RouletteSession local = session as RouletteSession;
                return local != null ? local.MoneyCents != 0 :
                    session != null && session.Credit != 0;
            }
        }

        private bool SourceAuIdlePaidVisible
        {
            // Game::UpdateMeters state 1/2 calls ShowPaidMeters(true) for
            // AU even when +0x254 is zero. No prior winning round is needed.
            // Other native phases retain their existing presentation rules.
            get { return auNationalStandardsEnabled && session != null &&
                !spinning && !session.InDoubleUp && !IsDoubleReveal && !IsDoubleExit &&
                (sourceGameplayState == SourceGameplayState.Idle ||
                    sourceGameplayState == SourceGameplayState.Bet); }
        }

        private void RestoreSourceTableInput()
        {
            sourceTableInputEnabled = gameOverMessageLatched && SourceCreditMeterNonzero;
        }

        private void RequestSourceBetState()
        {
            // Roulette::SetBet only assigns state 2 with nonzero credit.
            // State::operator= ignores an assignment of the existing state.
            if (!SourceCreditMeterNonzero ||
                sourceGameplayState == SourceGameplayState.Bet) return;
            sourceGameplayState = SourceGameplayState.Bet;
            sourceBetStateEntryPending = true;
        }

        internal void ServiceSourceBetStateEntry()
        {
            if (pcAutoPlayStorageFaulted || PcGameAppSuspended) return;
            if (!sourceBetStateEntryPending || session == null || spinning ||
                SourceRoundBlocksOrdinaryInput ||
                session.InDoubleUp || PaytableTestActive || OperatorFallbackActive)
                return;
            sourceBetStateEntryPending = false;
            if (sourceGameplayState != SourceGameplayState.Bet) return;
            // RunLoop enters here after the event callback returns. This must
            // run before dispatching the next queued event, never inside the
            // first wager callback or after every additional wager.
            sourceTableInputEnabled = true;
            RaiseFlowBetOpened();
            ApplyClearedGameResultsPresentation(false);
            SaveClearedGameResults();
            RestartSourcePrompt(PromptKind.PressSpin);
            // Game +70 -> VideoApp -> virtual +74 Game::PurgeInputs: releases
            // first, then the remaining events and configured Auto reset.
            PurgeSourcePlayerInputs();
            EnableSourceDemoMode();
        }

        private bool SourcePlayInputAllowed
        {
            get { return !pcAutoPlayStorageFaulted && PcAutoCompletePlayPlatformAllowed && (SourceDoubleOfferActive ? SourceOfferPlayAllowed :
                !SourceRoundBlocksOrdinaryInput && session != null && session.TotalBet > 0 &&
                session.TotalBet <= session.Credit &&
                session.TotalBet <= session.Profile.MaximumTotalBet &&
                !rackSnapPointerNeedsRelease && !spinning && !ResultDisplayActive &&
                !session.InDoubleUp && !IsDoubleReveal && !IsDoubleExit &&
                paysPage < 0 && !demoMenuVisible && !autoplay &&
                !PaytableTestActive && !OperatorFallbackActive && !pcCollectPending); }
        }
    }
}
