using System;

namespace RouletteDeluxe
{
    public enum OriginalPaysFlashAction { None, Buttons, RequestReturn, HidePays }

    // Current package: four ordinary Pays pages, no entrance/exit movement.
    // Game +2e00 is a Trigger, independent of the shared +2e04 timeout.
    public sealed class OriginalPaysFlashClock
    {
        public const uint InitialDelayMilliseconds = 16;
        public const uint FlashDelayMilliseconds = 1000;
        private readonly OriginalPeriodicIpcTimer timer = new OriginalPeriodicIpcTimer();
        private readonly long frequency;
        private long armedAtMilliseconds;
        private uint armedDelayMilliseconds;

        public long ArmedAtTimestamp { get; private set; }
        public uint ArmedDelayMilliseconds { get { return armedDelayMilliseconds; } }
        public bool Exists { get; private set; }
        public bool Returning { get; private set; }
        public int Page { get; private set; }
        public int LastPage { get; private set; }
        public int State { get; private set; }
        public bool PreviousSelected { get; private set; }
        public bool ExitSelected { get; private set; }
        public bool NextSelected { get; private set; }
        public ulong CallbackCount { get; private set; }
        public ulong Generation { get { return timer.Generation; } }
        public long NextExpirationTimestamp { get { return timer.NextExpirationTimestamp; } }

        public OriginalPaysFlashClock(int page, int lastPage, long timestamp, long frequency)
        {
            if (lastPage < 0) throw new ArgumentOutOfRangeException("lastPage");
            if (page < 0 || page > lastPage) throw new ArgumentOutOfRangeException("page");
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (frequency <= 0) throw new ArgumentOutOfRangeException("frequency");
            this.frequency = frequency;
            Page = page;
            LastPage = lastPage;
            Exists = true;
            Arm(timestamp, InitialDelayMilliseconds);
        }

        public void SetPage(int page)
        {
            if (page < 0 || page > LastPage) throw new ArgumentOutOfRangeException("page");
            // Navigation changes the page and 60s timeout, not +8764 or this Trigger.
            Page = page;
        }

        public bool SuspendAt(bool suspended, long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (!Exists) return false;
            // Game::Suspend always arms existing objects, even for repeated values.
            if (suspended) timer.Stop();
            else Arm(timestamp, 1);
            return true;
        }

        public bool RequestReturnAt(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            if (!Exists || Returning) return false;
            Returning = true;
            // A timeout can rearm this Trigger after Game suspended it.
            // Do not reject delivery based on a cached App-suspended bool.
            Arm(timestamp, 1);
            return true;
        }

        public OriginalPaysFlashAction UpdateAt(long timestamp, bool creditChanged = false,
            bool paytableDiagnostics = false)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            long due;
            if (!Exists || !timer.TryReceive(timestamp, out due)) return OriginalPaysFlashAction.None;
            CallbackCount = checked(CallbackCount + 1);
            if (Returning)
            {
                Stop();
                return OriginalPaysFlashAction.HidePays;
            }
            if (creditChanged)
            {
                RearmReturnedDelay(timestamp, 1);
                return OriginalPaysFlashAction.RequestReturn;
            }
            if (paytableDiagnostics)
            {
                RearmReturnedDelay(timestamp, FlashDelayMilliseconds);
                return OriginalPaysFlashAction.None;
            }

            int group = LastPage == 0 ? 7 : Page == 0 ? 1 : Page == LastPage ? 5 : 3;
            if (State == group)
            {
                State = group + 1;
                PreviousSelected = false;
                ExitSelected = true;
                NextSelected = false;
            }
            else
            {
                State = group;
                PreviousSelected = group == 3 || group == 5;
                ExitSelected = false;
                NextSelected = group == 1 || group == 3;
            }
            RearmReturnedDelay(timestamp, FlashDelayMilliseconds);
            return OriginalPaysFlashAction.Buttons;
        }

        private long MillisecondsAt(long timestamp)
        {
            return Decimal.ToInt64(Decimal.Floor((decimal)timestamp * 1000m / frequency));
        }

        private void Arm(long timestamp, uint delayMilliseconds)
        {
            ArmedAtTimestamp = timestamp;
            armedAtMilliseconds = MillisecondsAt(timestamp);
            armedDelayMilliseconds = delayMilliseconds;
            timer.Arm(timestamp, delayMilliseconds, 0, frequency);
        }

        private void RearmReturnedDelay(long timestamp, uint delayMilliseconds)
        {
            long late = MillisecondsAt(timestamp) - armedAtMilliseconds - armedDelayMilliseconds;
            uint delay = late > 0 ? late >= delayMilliseconds ? 1 :
                delayMilliseconds - (uint)late : delayMilliseconds;
            Arm(timestamp, delay);
        }

        public void Stop()
        {
            timer.Delete();
            Exists = false;
            Returning = false;
            // Hide/disable does not clear the persistent Button::Selected bits.
        }
    }
}
