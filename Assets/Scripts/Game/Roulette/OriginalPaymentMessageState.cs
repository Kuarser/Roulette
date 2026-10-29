using System;

namespace RouletteDeluxe
{
    // Only the original AU presentation fields. This owns no credit, paid meter,
    // IPC delivery receipt or durable journal. Callers must persist/deduplicate
    // accepted events before publishing their resulting presentation state.
    public sealed class OriginalPaymentMessageState
    {
        public double Collect { get; private set; }
        public double Cancel { get; private set; }
        public double TotalPaid { get; private set; }
        public bool CollectPending { get; private set; }

        public OriginalPaymentMessageState(double collect = 0, double cancel = 0,
            double totalPaid = 0, bool collectPending = false)
        {
            Finite(collect); Finite(cancel); Finite(totalPaid);
            Collect = collect; Cancel = cancel; TotalPaid = totalPaid;
            CollectPending = collectPending;
        }
        private static void Finite(double amount)
        { if (double.IsNaN(amount) || double.IsInfinity(amount)) throw new ArgumentOutOfRangeException("amount"); }

        // HandleUpdateDisplay's selected gate precedes the AU field gate.
        // Hopper and handpay update these fields; ticket/external do not.
        public OriginalPaymentMessageState DisplayUpdate(bool selected, bool auEnabled,
            int displayType, double amount)
        {
            if (!selected || !auEnabled || amount == 0) return this;
            if (displayType == 0x3400 || displayType == 0x2400)
                return new OriginalPaymentMessageState(amount, 0, TotalPaid, true);
            if (displayType != 0x3500 && displayType != 0x2500) return this;
            Finite(amount);
            var total = OriginalBinary80.FromDouble(TotalPaid);
            if (CollectPending) total = OriginalBinary80.Add(total, OriginalBinary80.FromDouble(Collect));
            // Native FST saves the intermediate double without rounding the
            // live x87 register; add the cancellation to that extended value.
            total = OriginalBinary80.Add(total, OriginalBinary80.FromDouble(amount));
            return new OriginalPaymentMessageState(0, amount, total.ToDouble(), false);
        }
        // The completion callback has neither the selected gate nor an AU
        // gate around the pending-field update. Displaying remains AU only.
        public OriginalPaymentMessageState CashoutCompleted()
        {
            if (!CollectPending) return this;
            double total = OriginalBinary80.Add(OriginalBinary80.FromDouble(TotalPaid),
                OriginalBinary80.FromDouble(Collect)).ToDouble();
            return new OriginalPaymentMessageState(Collect, Cancel, total, false);
        }
        // Invoke only after positive StartGame admission. Native clears three
        // doubles here, but does not clear the pending byte.
        public OriginalPaymentMessageState AdmittedStartGame()
        { return new OriginalPaymentMessageState(0, 0, 0, CollectPending); }

        public OriginalPaymentMessageUpdate Messages(bool auEnabled, OriginalMonetaryLocale locale,
            long denomination)
        {
            if (!auEnabled || (Collect == 0 && Cancel == 0)) return null;
            if (locale == null) throw new ArgumentNullException("locale");
            OriginalPaymentMessageSlot collect = null, cancel = null;
            if (Cancel != 0)
                cancel = new OriginalPaymentMessageSlot(Cancel == TotalPaid ? 399 : 400,
                    locale.FormatPaymentCredits(Cancel, denomination),
                    Cancel == TotalPaid ? null : locale.FormatPaymentCredits(TotalPaid, denomination));
            if (Collect != 0)
            {
                bool single = Collect == TotalPaid || TotalPaid == 0;
                var total = OriginalBinary80.FromDouble(TotalPaid);
                if (CollectPending) total = OriginalBinary80.Add(total, OriginalBinary80.FromDouble(Collect));
                collect = new OriginalPaymentMessageSlot(single ? 397 : 398,
                    locale.FormatPaymentCredits(Collect, denomination),
                    single ? null : locale.FormatPaymentCredits(total, denomination));
            }
            // An update touches slots 2/3 only. Slots 0/1 belong to the
            // existing state-2 vector and must not be removed by the caller.
            return new OriginalPaymentMessageUpdate(cancel ?? collect,
                cancel != null && collect != null ? collect : new OriginalPaymentMessageSlot(58, null, null));
        }
    }
    public sealed class OriginalPaymentMessageSlot
    {
        public readonly int PhraseId;
        public readonly string Amount, TotalPaid;
        internal OriginalPaymentMessageSlot(int phraseId, string amount, string totalPaid)
        { PhraseId = phraseId; Amount = amount; TotalPaid = totalPaid; }
    }
    public sealed class OriginalPaymentMessageUpdate
    {
        public const int State = 2, OnMilliseconds = 2500, OffMilliseconds = 250;
        public readonly OriginalPaymentMessageSlot Slot2, Slot3;
        internal OriginalPaymentMessageUpdate(OriginalPaymentMessageSlot slot2, OriginalPaymentMessageSlot slot3)
        { Slot2 = slot2; Slot3 = slot3; }
    }
}
