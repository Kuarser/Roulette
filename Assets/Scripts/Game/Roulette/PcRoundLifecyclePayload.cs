using System;

namespace RouletteDeluxe
{
    // PC recovery receipts. These are not native NV or platform transactions.
    [Serializable]
    public sealed class PcLifecycleAutoSnapshot
    {
        public int autoPlay;
        public bool replenishCredits;
        public bool demoOnly;
        public bool playerInput;
        public bool turbo;
        public int turboOption;
        public bool offWhenFinished;
        public uint secondsWarning;
        public uint secondsStart;
        public bool showCountdown;
        public bool disableWhenFinished;

        public static PcLifecycleAutoSnapshot Capture(OriginalAutoPlayData value)
        {
            return new PcLifecycleAutoSnapshot {
                autoPlay = value.AutoPlay, replenishCredits = value.ReplenishCredits,
                demoOnly = value.DemoOnly, playerInput = value.PlayerInput,
                turbo = value.Turbo, turboOption = value.TurboOption,
                offWhenFinished = value.AutoPlayOffWhenGameFinished,
                secondsWarning = value.SecsToAutoCompleteWarning,
                secondsStart = value.SecsToAutoCompleteStart,
                showCountdown = value.AutoCompleteShowCountdown,
                disableWhenFinished = value.DisableWhenGameFinished
            };
        }

        public PcLifecycleAutoSnapshot Clone()
        {
            return (PcLifecycleAutoSnapshot)MemberwiseClone();
        }

        public bool Same(PcLifecycleAutoSnapshot other)
        {
            return other != null && autoPlay == other.autoPlay &&
                replenishCredits == other.replenishCredits && demoOnly == other.demoOnly &&
                playerInput == other.playerInput && turbo == other.turbo &&
                turboOption == other.turboOption && offWhenFinished == other.offWhenFinished &&
                secondsWarning == other.secondsWarning && secondsStart == other.secondsStart &&
                showCountdown == other.showCountdown && disableWhenFinished == other.disableWhenFinished;
        }

        public PcLifecycleAutoSnapshot Purged()
        {
            var result = Clone();
            result.autoPlay = 0;
            result.replenishCredits = false;
            result.demoOnly = false;
            result.playerInput = true;
            result.turbo = false;
            result.turboOption = 0;
            return result;
        }
    }

    [Serializable]
    public sealed class PcRoundEndIntent
    {
        public int version;
        public bool finishAuto;
        public bool transactionActive;
        public bool demoAvailable;
        public PcLifecycleAutoSnapshot autoBefore;
        // V2 adds the independent Disable branch and snapshots the timer
        // object, which is not equivalent to Start != 0 after expiration.
        // The archive codec emits only the five original fields for V1.
        public bool finishDisable;
        public bool bonusGamesEnabled;
        public bool timerPresent;
        public string cancelDisableOperationId;
        public string cancelAutoOperationId;
    }

    public enum PcIdleState { Pending = 1, Running = 2, Complete = 3 }
    public enum PcIdleStep
    {
        None = 0, Begin = 1, DepositDone = 2, CheckPurged = 3,
        PrepareFinished = 4, IdleEffectsDone = 5, DrawInFlight = 6, Complete = 7
    }

    [Serializable]
    public sealed class PcIdleEntry
    {
        public int version;
        public string idleEntryId;
        public string depositEventId;
        public string successorSpinId;
        public string consumedCollectEventId;
        public PcIdleState state;
        public PcIdleStep step;
        public PcLifecycleAutoSnapshot autoBefore;
        public bool eligible;
        public bool demoAvailable;
        public bool depositRequired;
        public long creditBefore;
        public long creditAfter;
        public long denomination;
        public long remainderCents;
        public string priorCreditEventId;
        public string priorCreditEventDigest;
        public bool prepareRequested;
        public bool prepareAccepted;
        public bool tableInputEnabled;
    }
}
