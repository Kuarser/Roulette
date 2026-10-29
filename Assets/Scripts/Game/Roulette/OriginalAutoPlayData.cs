using System;

namespace RouletteDeluxe
{
    // The original GameAutoPlayData is separate from active Game+0x5110.
    // 0x080f0ac0 initializes these fields to zero; 0x080f0d60 writes the
    // six options below. A live PC owner may attach scalar persistence;
    // active-state dispatch, native NV/IPC and timers belong to the caller.
    public sealed class OriginalAutoPlayData
    {
        public int AutoPlay { get; private set; }                 // +0x50
        public bool ReplenishCredits { get; private set; }        // +0x54
        public bool DemoOnly { get; private set; }                // +0x55
        public bool PlayerInput { get; private set; }             // +0x56
        public bool Turbo { get; private set; }                   // +0x57
        public byte TurboOption { get; private set; }             // +0x58
        public bool AutoPlayOffWhenGameFinished { get; private set; } // +0x59
        public uint SecsToAutoCompleteWarning { get; private set; } // +0x5a
        public uint SecsToAutoCompleteStart { get; private set; }  // +0x5e
        public bool AutoCompleteShowCountdown { get; private set; } // +0x62
        public bool DisableWhenGameFinished { get; private set; } // +0x63
        private Action<OriginalAutoPlayData> persist;

        // Game::TurboAutoPlay, 0x080f9e10, tests configured AutoPlay and
        // configured Turbo. The active round's AutoPlay byte is not read.
        public bool TurboAutoPlay
        {
            get { return AutoPlay != 0 && Turbo; }
        }

        public OriginalAutoPlayData Clone()
        {
            // A diagnostic snapshot never inherits the live owner's writer.
            var copy = (OriginalAutoPlayData)MemberwiseClone();
            copy.persist = null;
            return copy;
        }

        internal void AttachPersistence(Action<OriginalAutoPlayData> writer)
        {
            persist = writer;
        }

        private void Persist()
        {
            // Native setters save even when the assigned value is unchanged.
            // The owner decides how a failed write stops live dispatch.
            if (persist != null) persist(this);
        }

        public void SetOptions(bool autoPlay, bool replenishCredits,
            bool playerInput, bool turbo, bool demoOnly, byte turboOption)
        {
            AutoPlay = autoPlay ? 1 : 0;
            ReplenishCredits = replenishCredits;
            PlayerInput = playerInput;
            Turbo = turbo;
            DemoOnly = demoOnly;
            TurboOption = turboOption;
            Persist();
        }

        // GameAutoPlayData::SetAutoPlay, 0x080f0e80, stores the full int.
        public void SetAutoPlay(int value) { AutoPlay = value; Persist(); }

        // These setters mirror the independent native fields. They do not
        // implicitly clear a demo option or change an active game state.
        public void SetReplenishCredits(bool value) { ReplenishCredits = value; Persist(); }
        public void SetAutoPlayDemoOnly(bool value) { DemoOnly = value; Persist(); }
        public void SetPlayerInput(bool value) { PlayerInput = value; Persist(); }
        public void SetTurbo(bool value) { Turbo = value; Persist(); }
        public void SetTurboOption(byte value) { TurboOption = value; Persist(); }
        public void SetAutoPlayOffWhenGameFinished(bool value)
        {
            AutoPlayOffWhenGameFinished = value;
            Persist();
        }
        public void SetSecsToAutoCompleteWarning(uint value)
        {
            SecsToAutoCompleteWarning = value;
            Persist();
        }
        public void SetSecsToAutoCompleteStart(uint value)
        {
            SecsToAutoCompleteStart = value;
            Persist();
        }
        public void SetAutoCompleteShowCountdown(bool value)
        {
            AutoCompleteShowCountdown = value;
            Persist();
        }
        public void SetDisableWhenGameFinished(bool value)
        {
            DisableWhenGameFinished = value;
            Persist();
        }

        // Game::PurgeInputs(filter), 0x080f9070, first removes release
        // events 0x200004 and 0x200020. The caller supplies whether its
        // remaining filter removed input. The current dispatched event is
        // already out of the native queue and is not itself a removal.
        // True requests TurnOnAllSounds; it does not toggle active AutoPlay.
        public bool ResetForPurgedInput(bool demoAvailable,
            bool removedNonReleaseInput)
        {
            if (!demoAvailable || !removedNonReleaseInput || !PlayerInput)
                return false;
            SetOptions(false, false, true, false, false, 0);
            return true;
        }

        // Game::StateEnd, 0x08148490: at 0x08148693 it clears configured
        // AutoPlay/Turbo and consumes OffWhenGameFinished, then the game
        // separately clears active AutoPlay and cancels auto-completion.
        // PlayerTransactionStatus is an external status, not a credit test.
        // True requests the caller's source sound-on/active-state handling.
        public bool FinishGame(bool transactionActive)
        {
            if (!AutoPlayOffWhenGameFinished && transactionActive) return false;
            SetAutoPlay(0);
            SetTurbo(false);
            SetAutoPlayOffWhenGameFinished(false);
            return true;
        }
    }
}
