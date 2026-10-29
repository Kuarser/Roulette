using System;

namespace RouletteDeluxe
{
    // Process-local model of the source IPC periodic timer's observable receipt
    // contract. Expirations accumulate while the consumer is late; each Poll
    // receives one message. Source: kernel timer_tick/sys_ipc_receivex and
    // IPC::Poll, audited in outputs/auto_complete_double_reverse.
    // No clock, callback, Unity object, persistence or game state is owned here.
    public sealed class OriginalPeriodicIpcTimer
    {
        private long epoch;
        private long frequency;
        private uint delayMilliseconds;
        private uint periodMilliseconds;
        private ulong receivedCount;

        public bool Running { get; private set; }
        public ulong Generation { get; private set; }
        public ulong NextExpirationOrdinal { get { return receivedCount; } }
        // The next unreceived expiration, including an already pending one.
        public long NextExpirationTimestamp { get; private set; }
        public long LastReceivedDueTimestamp { get; private set; }

        public void Arm(long now, uint delayMs, uint periodMs, long frequency)
        {
            if (now < 0) throw new ArgumentOutOfRangeException("now");
            if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency");
            Invalidate();
            this.epoch = now;
            this.frequency = frequency;
            // TimerObj constructor and SetPeriod substitute period for a zero
            // initial delay. SetPeriod(0, 0) stops the timer.
            delayMilliseconds = delayMs == 0 ? periodMs : delayMs;
            periodMilliseconds = periodMs;
            if (delayMilliseconds == 0) return;
            try
            {
                NextExpirationTimestamp = DueTimestamp(0);
                Running = true;
            }
            catch (OverflowException)
            {
                Invalidate();
                throw;
            }
        }

        public void Stop() { Invalidate(); }
        public void Delete() { Invalidate(); }

        private void Invalidate()
        {
            Running = false;
            NextExpirationTimestamp = 0;
            LastReceivedDueTimestamp = 0;
            receivedCount = 0;
            Generation = checked(Generation + 1);
        }

        private long DueTimestamp(ulong ordinal)
        {
            // Round the cumulative duration once, so clocks such as 1001 Hz
            // do not gain one rounded tick on every 250 ms period.
            decimal milliseconds = (decimal)delayMilliseconds +
                (decimal)ordinal * periodMilliseconds;
            decimal ticks = Decimal.Ceiling(milliseconds * frequency / 1000m);
            return checked(epoch + Decimal.ToInt64(ticks));
        }

        public ulong PendingCountAt(long now)
        {
            if (!Running || now < NextExpirationTimestamp) return 0;
            if (periodMilliseconds == 0) return 1;
            // All products are decimal integers within the 96-bit range for
            // representable Int64 timestamps. Floor is inverse to DueTimestamp.
            decimal elapsedMilliseconds = Decimal.Floor(
                ((decimal)now - epoch) * 1000m / frequency);
            decimal total = Decimal.Floor(
                (elapsedMilliseconds - delayMilliseconds) / periodMilliseconds) + 1m;
            return checked(Decimal.ToUInt64(total) - receivedCount);
        }

        public bool TryReceive(long now, out long consumedDue)
        {
            consumedDue = 0;
            if (!Running || now < NextExpirationTimestamp) return false;
            long due = NextExpirationTimestamp;
            try
            {
                receivedCount = checked(receivedCount + 1);
                if (periodMilliseconds == 0)
                {
                    Running = false;
                    NextExpirationTimestamp = 0;
                }
                else NextExpirationTimestamp = DueTimestamp(receivedCount);
            }
            catch (OverflowException)
            {
                // A host timestamp horizon cannot silently saturate, wrap,
                // or retain a permanently due timer.
                Invalidate();
                throw;
            }
            LastReceivedDueTimestamp = due;
            consumedDue = due;
            return true;
        }
    }
}
