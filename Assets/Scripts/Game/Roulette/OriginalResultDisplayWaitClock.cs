using System;

namespace RouletteDeluxe
{
    public enum OriginalResultDisplayWaitPhase
    {
        AutoWait,
        HighWinDelay,
        Completed
    }

    public enum OriginalResultDisplayWaitOutcome
    {
        None,
        Timeout,
        Input
    }

    public interface IOriginalResultDisplayWaitHost
    {
        bool GameSuspended { get; }
        bool AutoPlayActive { get; }
        bool ConfiguredAutoPlayEligible { get; }
        long CurrentAward { get; }
        bool InputAvailable { get; }
        void DispatchOneInput();
        long Timestamp { get; }
    }

    // StateDisplayResults owns 20/30 separate WaitInput(35) calls. Each
    // UpdateAt represents one returned App::Idle(true) service, not elapsed
    // loop iterations. The caller must not service a newly armed wait from
    // the same Idle return. Getters never advance the loop or dispatch input.
    public sealed class OriginalResultDisplayWaitClock
    {
        public const int WaitMilliseconds = 35;
        public const int HighWinDelayMilliseconds = 500;
        private readonly IOriginalResultDisplayWaitHost host;
        private readonly long frequency;
        private long lastObservedTimestamp;
        private bool servicing;

        public OriginalResultDisplayWaitPhase Phase { get; private set; }
        public int InitialWaits { get; private set; }
        public int RemainingWaits { get; private set; }
        public int TimeoutCount { get; private set; }
        public int InputCount { get; private set; }
        public int SkippedWaits { get; private set; }
        public OriginalResultDisplayWaitOutcome LastOutcome { get; private set; }
        public long WaitStartedAt { get; private set; }
        // These timestamps retain the last arm after the phase changes.
        // Phase determines whether the corresponding wait is still pending.
        public long WaitDueTimestamp { get; private set; }
        public long HighDelayStartedAt { get; private set; }
        public long HighDelayDueTimestamp { get; private set; }
        public long CompletedAt { get; private set; }
        public bool Completed { get { return Phase == OriginalResultDisplayWaitPhase.Completed; } }

        public OriginalResultDisplayWaitClock(long paidAtEntry, long timestamp,
            long frequency, IOriginalResultDisplayWaitHost host)
        {
            // Native chooses win by nonzero award. PC money accepts only the
            // nonnegative domain, so zero/nonzero maps to loss/win here.
            if (paidAtEntry < 0) throw new ArgumentOutOfRangeException("paidAtEntry");
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency");
            if (host == null) throw new ArgumentNullException("host");
            this.host = host;
            this.frequency = frequency;
            lastObservedTimestamp = timestamp;
            InitialWaits = paidAtEntry == 0 ? 20 : 30;
            RemainingWaits = InitialWaits;
            if (host.AutoPlayActive) ArmWait(timestamp);
            else FinishAutoLoop(timestamp, false);
        }

        // The caller supplies one actually returned Idle service. A selected
        // keep-alive Game can return while App is suspended. That App state
        // is not a universal gate here: WaitInput checks Game-local suspend
        // only on its timeout path, and App::Delay has no such condition.
        public bool UpdateAt(long timestamp)
        {
            if (servicing) throw new InvalidOperationException("Result wait service is already active.");
            ValidateTimestamp(timestamp);
            lastObservedTimestamp = timestamp;
            if (Completed) return false;

            servicing = true;
            try
            {
                if (Phase == OriginalResultDisplayWaitPhase.HighWinDelay)
                {
                    // App::Delay(500,true) never polls or dispatches input.
                    if (timestamp < HighDelayDueTimestamp) return false;
                    CompleteAt(timestamp);
                    return true;
                }

                // WaitInput checks the trigger first, then InputAvailable
                // with filter (0,0). A timeout retains all queued events.
                OriginalResultDisplayWaitOutcome outcome;
                if (timestamp >= WaitDueTimestamp && !host.GameSuspended)
                    outcome = OriginalResultDisplayWaitOutcome.Timeout;
                else if (host.InputAvailable)
                {
                    host.DispatchOneInput();
                    outcome = OriginalResultDisplayWaitOutcome.Input;
                }
                else return false;

                --RemainingWaits;
                LastOutcome = outcome;
                if (outcome == OriginalResultDisplayWaitOutcome.Timeout) ++TimeoutCount;
                else ++InputCount;

                // A callback may change Auto/configuration/award, or suspend
                // App. Native code still returns this current wait and tests
                // the loop condition before its next blocking Idle call.
                // Turning Auto off never cancels an in-flight WaitInput.
                if (RemainingWaits > 0 && host.AutoPlayActive)
                    ArmWait(ReadAfterServiceTimestamp(timestamp));
                else FinishAutoLoop(timestamp, true);
                return true;
            }
            finally
            {
                servicing = false;
            }
        }

        private void FinishAutoLoop(long timestamp, bool sampleAfterService)
        {
            // Native synchronously skips the remaining for-loop bodies once
            // active Auto is off. Keep that count distinct from returned waits.
            SkippedWaits += RemainingWaits;
            RemainingWaits = 0;
            // Read eligibility once, after the active loop actually returns.
            // Later input/configuration changes cannot cancel this Delay.
            bool highWinDelay = host.ConfiguredAutoPlayEligible && host.CurrentAward > 500;
            long enteredAt = sampleAfterService ? ReadAfterServiceTimestamp(timestamp) : timestamp;
            if (highWinDelay)
            {
                long due = Deadline(enteredAt, HighWinDelayMilliseconds);
                HighDelayStartedAt = enteredAt;
                HighDelayDueTimestamp = due;
                Phase = OriginalResultDisplayWaitPhase.HighWinDelay;
            }
            else CompleteAt(enteredAt);
        }

        private void ArmWait(long timestamp)
        {
            long due = Deadline(timestamp, WaitMilliseconds);
            WaitStartedAt = timestamp;
            WaitDueTimestamp = due;
            Phase = OriginalResultDisplayWaitPhase.AutoWait;
        }

        private void CompleteAt(long timestamp)
        {
            CompletedAt = timestamp;
            Phase = OriginalResultDisplayWaitPhase.Completed;
        }

        private long ReadAfterServiceTimestamp(long timestamp)
        {
            long after = host.Timestamp;
            if (after < timestamp || after < lastObservedTimestamp)
                throw new ArgumentOutOfRangeException("host.Timestamp");
            lastObservedTimestamp = after;
            return after;
        }

        private void ValidateTimestamp(long timestamp)
        {
            if (timestamp < 0 || timestamp < lastObservedTimestamp)
                throw new ArgumentOutOfRangeException("timestamp");
        }

        private long Deadline(long timestamp, int milliseconds)
        {
            // Ceiling preserves the requested delay at non-integral tick
            // frequencies without multiplying a long timestamp by 1000.
            long ticks = checked(frequency / 1000L * milliseconds +
                (frequency % 1000L * milliseconds + 999L) / 1000L);
            return checked(timestamp + ticks);
        }
    }
}
