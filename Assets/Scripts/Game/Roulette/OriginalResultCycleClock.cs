using System;

namespace RouletteDeluxe
{
    // RouletteCycleResults::Start/Suspend/Cycle and bally::Trigger::Triggered.
    // The owner applies presentation side effects; this owns only the retained
    // record/phase and one live IPC receipt at a time. Nothing is persisted.
    public sealed class OriginalResultCycleClock
    {
        private readonly OriginalPeriodicIpcTimer timer = new OriginalPeriodicIpcTimer();
        private readonly long frequency;
        private readonly uint cycleDelayMilliseconds;
        private long armedAtMilliseconds;
        private uint armedDelayMilliseconds;
        private bool showOnNextReceipt = true;

        public int RecordCount { get; private set; }
        public bool Active { get; private set; }
        public bool Paused { get; private set; }
        public bool Visible { get; private set; }
        public int Index { get; private set; }
        public int VisibleIndex { get { return Visible ? Index : -1; } }
        public long NextExpirationTimestamp { get { return timer.NextExpirationTimestamp; } }
        public ulong CallbackCount { get; private set; }

        public OriginalResultCycleClock(int recordCount, uint cycleDelayMilliseconds,
            long timestamp, long frequency)
        {
            if (recordCount <= 0) throw new ArgumentOutOfRangeException("recordCount");
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency");
            RecordCount = recordCount;
            this.frequency = frequency;
            this.cycleDelayMilliseconds = cycleDelayMilliseconds;
            Active = true;
            // Source Start passes 0 to Trigger, whose constructor uses 1 ms.
            // ShowWin therefore happens on its first callback, not in Start.
            Arm(timestamp, 1);
        }

        private long MillisecondsAt(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            return Decimal.ToInt64(Decimal.Floor((decimal)timestamp * 1000m / frequency));
        }

        private void Arm(long timestamp, uint delayMilliseconds)
        {
            long nowMilliseconds = MillisecondsAt(timestamp);
            timer.Arm(timestamp, delayMilliseconds, 0, frequency);
            armedAtMilliseconds = nowMilliseconds;
            armedDelayMilliseconds = delayMilliseconds;
        }

        public bool UpdateAt(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (!Active || Paused) return false;
            long due;
            if (!timer.TryReceive(timestamp, out due)) return false;

            if (showOnNextReceipt) Visible = true;
            else
            {
                Visible = false;
                Index = Index + 1 == RecordCount ? 0 : Index + 1;
            }
            showOnNextReceipt = !showOnNextReceipt;
            CallbackCount = checked(CallbackCount + 1);

            // Trigger rearms from the callback's returned delay. It subtracts
            // lateness from that delay, clamping an already overdue next
            // interval to 1 ms. No loop drains or skips presentation phases.
            if (cycleDelayMilliseconds == uint.MaxValue) return true;
            uint nextDelay = cycleDelayMilliseconds;
            if (nextDelay == 0) nextDelay = 1;
            else
            {
                long elapsed = MillisecondsAt(timestamp) - armedAtMilliseconds;
                if (elapsed > armedDelayMilliseconds)
                {
                    long late = elapsed - armedDelayMilliseconds;
                    nextDelay = late >= nextDelay ? 1 : nextDelay - (uint)late;
                }
            }
            Arm(timestamp, nextDelay);
            return true;
        }

        public bool PauseAt(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (!Active || Paused) return false;
            timer.Stop();
            Paused = true;
            return true;
        }

        public bool ResumeAt(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (!Active || !Paused) return false;
            Arm(timestamp, 1);
            Paused = false;
            return true;
        }

        public void Stop()
        {
            timer.Delete();
            Active = false;
            Paused = false;
            Visible = false;
            Index = 0;
            showOnNextReceipt = false;
        }
    }
}
