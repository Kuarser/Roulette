using System;

namespace RouletteDeluxe
{
    // ProViewButtonPanel starts this numeric-label fade when Stop enters a
    // new Outro. Its sprite timer survives Play/Hide and is independent of
    // the movie's lifetime. The caller owns those visibility decisions.
    public sealed class OriginalPaidFadeClock
    {
        public const int IntervalMilliseconds = 33;
        public static readonly float Threshold = BitConverter.ToSingle(
            BitConverter.GetBytes(0x3F7FBE77u), 0);
        public static readonly float Step = BitConverter.ToSingle(
            BitConverter.GetBytes(0x3D088889u), 0);

        private long activeFrequency;
        private long lastArmMilliseconds;
        private long lastObservedTimestamp;
        private bool servicing;

        public float Alpha { get; private set; } = 1f;
        public bool Running { get; private set; }
        public int CallbackCount { get; private set; }
        public long LastArmTimestamp { get; private set; }
        public int DelayMilliseconds { get; private set; }

        public bool TryStartAt(long now, long frequency)
        {
            CheckNotServicing();
            long milliseconds = MillisecondsAt(now, frequency);
            if (Running) ValidateRunningTime(now, frequency);
            if (!(Alpha > Threshold)) return false;
            Alpha = 0f;
            CallbackCount = 0;
            ArmAt(now, milliseconds, frequency);
            return true;
        }

        // Stop applies alpha zero to both labels before SetTimer constructs
        // its Trigger and samples the initial arm timestamp.
        public bool TryStartAt(long now, long frequency, Action<float> applyAlpha,
            Func<long> armClock)
        {
            if (applyAlpha == null) throw new ArgumentNullException("applyAlpha");
            if (armClock == null) throw new ArgumentNullException("armClock");
            CheckNotServicing();
            MillisecondsAt(now, frequency);
            if (Running) ValidateRunningTime(now, frequency);
            if (!(Alpha > Threshold)) return false;
            servicing = true;
            try
            {
                Alpha = 0f;
                CallbackCount = 0;
                applyAlpha(Alpha);
                long arm = armClock();
                if (arm < now) throw new ArgumentOutOfRangeException("armTimestamp");
                ArmAt(arm, MillisecondsAt(arm, frequency), frequency);
                return true;
            }
            finally
            {
                servicing = false;
            }
        }

        private void ArmAt(long now, long milliseconds, long frequency)
        {
            Running = true;
            LastArmTimestamp = now;
            lastArmMilliseconds = milliseconds;
            lastObservedTimestamp = now;
            activeFrequency = frequency;
            DelayMilliseconds = IntervalMilliseconds;
        }

        // Deterministic service with a zero-cost event handler.
        public bool UpdateAt(long now, long frequency)
        {
            return UpdateCore(now, frequency, now, null, null);
        }

        // The native Trigger samples its next anchor after the handler has
        // returned. Tests can supply that separate timestamp directly.
        public bool UpdateAt(long now, long frequency, long serviceAfterTimestamp)
        {
            if (serviceAfterTimestamp < now)
                throw new ArgumentOutOfRangeException("serviceAfterTimestamp");
            MillisecondsAt(serviceAfterTimestamp, frequency);
            return UpdateCore(now, frequency, serviceAfterTimestamp, null, null);
        }

        // Production applies the new alpha before sampling the post-handler
        // clock. A final callback cancels the timer and never reads that clock.
        public bool UpdateAt(long now, long frequency, Action<float> applyAlpha,
            Func<long> serviceAfterClock)
        {
            if (applyAlpha == null) throw new ArgumentNullException("applyAlpha");
            if (serviceAfterClock == null) throw new ArgumentNullException("serviceAfterClock");
            return UpdateCore(now, frequency, now, applyAlpha, serviceAfterClock);
        }

        private bool UpdateCore(long now, long frequency, long knownAfterTimestamp,
            Action<float> applyAlpha, Func<long> serviceAfterClock)
        {
            CheckNotServicing();
            MillisecondsAt(now, frequency);
            if (!Running) return false;
            ValidateRunningTime(now, frequency);
            lastObservedTimestamp = now;
            // IPC::CreateTimer is external in the recovered binary. Service
            // no earlier than its requested whole delay; native lateness
            // compensation below separately uses absolute milliseconds.
            long requestedTicks = frequency / 1000L * DelayMilliseconds +
                (frequency % 1000L * DelayMilliseconds + 999L) / 1000L;
            if (now - LastArmTimestamp < requestedTicks) return false;

            servicing = true;
            try
            {
                // The source stores one single-precision addition per event.
                // Never convert elapsed time into several callbacks here.
                Alpha = Alpha + Step;
                ++CallbackCount;
                if (Alpha > Threshold)
                {
                    Alpha = 1f;
                    Running = false;
                    DelayMilliseconds = 0;
                }
                if (applyAlpha != null) applyAlpha(Alpha);
                if (!Running) return true;

                long after = serviceAfterClock == null ? knownAfterTimestamp :
                    serviceAfterClock();
                if (after < now)
                    throw new ArgumentOutOfRangeException("serviceAfterTimestamp");
                long afterMilliseconds = MillisecondsAt(after, frequency);
                long late = afterMilliseconds - lastArmMilliseconds - DelayMilliseconds;
                int nextDelay = late <= 0 ? IntervalMilliseconds :
                    late >= IntervalMilliseconds ? 1 :
                    IntervalMilliseconds - (int)late;
                LastArmTimestamp = after;
                lastArmMilliseconds = afterMilliseconds;
                lastObservedTimestamp = after;
                DelayMilliseconds = nextDelay;
                return true;
            }
            finally
            {
                servicing = false;
            }
        }

        private void ValidateRunningTime(long now, long frequency)
        {
            if (frequency != activeFrequency)
                throw new ArgumentException("A running fade needs the same clock frequency.",
                    "frequency");
            if (now < lastObservedTimestamp)
                throw new ArgumentOutOfRangeException("now");
        }

        private void CheckNotServicing()
        {
            if (servicing)
                throw new InvalidOperationException("Paid fade callbacks cannot recursively service their timer.");
        }

        private static long MillisecondsAt(long timestamp, long frequency)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency");
            // Trigger samples each absolute millisecond value before taking
            // their difference for lateness. Flooring elapsed ticks would
            // lose a crossed millisecond edge during the callback.
            long remainder = timestamp % frequency;
            long fraction = remainder <= long.MaxValue / 1000L ?
                remainder * 1000L / frequency :
                (long)((decimal)remainder * 1000m / frequency);
            // The decimal fallback only avoids an intermediate integer
            // overflow for exceptionally large frequencies. It retains the
            // exact integer millisecond floor without float/double epochs.
            return checked(timestamp / frequency * 1000L + fraction);
        }
    }
}
