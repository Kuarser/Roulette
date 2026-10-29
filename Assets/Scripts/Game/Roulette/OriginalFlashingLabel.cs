using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // Recovered FlashingLabel vector/phase semantics and Trigger one-shot
    // rescheduling. The caller supplies renderer actions and suspension state.
    public sealed class OriginalFlashingLabel
    {
        private sealed class Message
        {
            internal string Text = "", Font = "";
            internal uint On, Off;
        }
        private readonly List<List<Message>> states = new List<List<Message>>();
        private readonly OriginalPeriodicIpcTimer timer = new OriginalPeriodicIpcTimer();
        private readonly long frequency;
        private uint armedMilliseconds, armedDelay, on, off;
        private bool phase, show = true, servicing;
        public int State { get; private set; }
        public int Cursor { get; private set; }
        public string Text { get; private set; }
        public string Font { get; private set; }
        public bool Visible { get; private set; }
        public bool OnPhase { get { return phase; } }
        public bool Running { get { return timer.Running; } }
        public uint DelayMilliseconds { get { return armedDelay; } }
        public ulong Callbacks { get; private set; }
        public OriginalFlashingLabel(long clockFrequency)
        {
            if (clockFrequency <= 0) throw new ArgumentOutOfRangeException("clockFrequency");
            frequency = clockFrequency;
            Text = ""; Font = ""; Visible = true;
            states.Add(new List<Message> { new Message() });
        }
        public void SetMessage(int state, int slot, string text, string font, uint onMilliseconds, uint offMilliseconds)
        {
            if (state < 0 || slot < 0 || state > 4095 || slot > 4095) throw new ArgumentOutOfRangeException("state");
            if (text == null || font == null) throw new ArgumentNullException("text");
            while (states.Count <= state) states.Add(new List<Message>());
            while (states[state].Count <= slot) states[state].Add(new Message());
            states[state][slot] = new Message { Text = text, Font = font, On = onMilliseconds, Off = offMilliseconds };
            // Native SetMessage does not reset the active cursor, arm or durations.
        }
        public int MessageCount(int state) { return states[state].Count; }
        public void ApplyPaymentUpdate(OriginalPaymentMessageUpdate update,
            Func<OriginalPaymentMessageSlot, string> formatPhrase, string font, long timestamp)
        {
            if (update == null) return; // Native no-message branch does not clear slots.
            if (formatPhrase == null) throw new ArgumentNullException("formatPhrase");
            string first = formatPhrase(update.Slot2), second = formatPhrase(update.Slot3);
            if (first == null || second == null || font == null) throw new ArgumentNullException("phrase");
            Milliseconds(timestamp);
            SetMessage(2, 2, first, font, 2500, 250);
            SetMessage(2, 3, second, font, 2500, 250);
            SetState(2, timestamp);
        }
        public void SetState(int state, long timestamp)
        {
            Milliseconds(timestamp);
            if (state < 0 || state >= states.Count || (state != 0 && states[state].Count == 0))
                throw new ArgumentOutOfRangeException("state");
            State = state; Cursor = 0; phase = false;
            if (state == 0) { Text = ""; timer.Stop(); armedDelay = 0; return; }
            on = states[state][0].On; off = states[state][0].Off;
            Arm(timestamp, 1);
        }
        public void SetShow(bool value)
        {
            if (value == show) return;
            show = value; Visible = value;
        }
        private uint Milliseconds(long timestamp)
        {
            if (timestamp < 0) throw new ArgumentOutOfRangeException("timestamp");
            return (uint)(decimal.Floor((decimal)timestamp * 1000m / frequency) % 4294967296m);
        }
        private void Arm(long timestamp, uint delay)
        {
            armedMilliseconds = Milliseconds(timestamp);
            if (delay == uint.MaxValue) { timer.Stop(); armedDelay = 0; return; }
            armedDelay = delay == 0 ? 1u : delay;
            timer.Arm(timestamp, armedDelay, 0, frequency);
        }
        public bool Service(long timestamp, bool suspended)
        { return Service(timestamp, suspended, null, () => timestamp); }
        public bool Service(long timestamp, bool suspended, Action<OriginalFlashingLabel> apply, Func<long> afterClock)
        {
            if (servicing) throw new InvalidOperationException("Flashing trigger recursion is not admitted");
            if (afterClock == null) throw new ArgumentNullException("afterClock");
            Milliseconds(timestamp);
            long due;
            if (!timer.TryReceive(timestamp, out due)) return false;
            ulong generation = timer.Generation;
            servicing = true;
            try
            {
                phase = !phase; Callbacks++;
                if (!suspended)
                {
                    if (phase)
                    {
                        var message = states[State][Cursor];
                        Cursor = (Cursor + 1) % states[State].Count;
                        Text = message.Text;
                        // Bitmap ScaledLabel retains its font on an empty font token.
                        if (message.Font.Length != 0) Font = message.Font;
                        if (show) Visible = true;
                    }
                    else Visible = false;
                }
                uint requested = phase ? on : off;
                if (apply != null) apply(this);
                // Arm called inside a native handler overrides its returned delay.
                if (timer.Generation != generation) return true;
                long after = afterClock();
                if (after < timestamp) throw new ArgumentOutOfRangeException("afterClock");
                uint elapsed = unchecked(Milliseconds(after) - armedMilliseconds);
                if (requested != 0 && requested != uint.MaxValue && elapsed > armedDelay)
                {
                    uint late = elapsed - armedDelay;
                    requested = late < requested ? requested - late : 1;
                }
                Arm(after, requested);
                return true;
            }
            finally { servicing = false; }
        }
    }
}
