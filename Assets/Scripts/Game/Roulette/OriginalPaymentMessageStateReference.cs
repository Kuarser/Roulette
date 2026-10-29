using System;

namespace RouletteDeluxe.Validation
{
    public static class OriginalPaymentMessageStateReference
    {
        private static void Require(bool ok, string message)
        { if (!ok) throw new Exception("Original payment message state: " + message); }
        public static int Validate(OriginalMonetaryLocale rm)
        {
            int checks = 0;
            var empty = new OriginalPaymentMessageState();
            foreach (int type in new[] { 0x2400, 0x2500, 0x3400, 0x3500 })
            {
                Require(ReferenceEquals(empty, empty.DisplayUpdate(false, true, type, 5)), "unselected update"); checks++;
                Require(ReferenceEquals(empty, empty.DisplayUpdate(true, false, type, 5)), "non-AU update"); checks++;
                Require(ReferenceEquals(empty, empty.DisplayUpdate(true, true, type, 0)), "zero update"); checks++;
            }
            foreach (int type in new[] { 0, 0x2100, 0x2600, 0x2750, 0x3600, 0x3750 })
            { Require(ReferenceEquals(empty, empty.DisplayUpdate(true, true, type, 5)), "unrelated type"); checks++; }
            Require(empty.Messages(true, rm, 100) == null, "empty leaves existing slots"); checks++;
            foreach (int collectType in new[] { 0x2400, 0x3400 })
            {
                var collect = empty.DisplayUpdate(true, true, collectType, 9.95);
                Require(collect.Collect == 9.95 && collect.Cancel == 0 && collect.TotalPaid == 0 && collect.CollectPending,
                    "initial collect state"); checks++;
                var message = collect.Messages(true, rm, 100);
                Require(message.Slot2.PhraseId == 397 && message.Slot2.Amount == "RM9.95" &&
                    message.Slot2.TotalPaid == null && message.Slot3.PhraseId == 58, "initial collect slots"); checks++;
                Require(collect.Messages(false, null, 0) == null, "non-AU hides restored fields"); checks++;
                var completed = collect.CashoutCompleted();
                Require(completed.TotalPaid == 9.95 && !completed.CollectPending && completed.Collect == 9.95,
                    "completion retains collect"); checks++;
                Require(ReferenceEquals(completed, completed.CashoutCompleted()), "completion repeated without pending"); checks++;
                var next = completed.DisplayUpdate(true, true, collectType, 2);
                message = next.Messages(true, rm, 100);
                Require(next.TotalPaid == 9.95 && message.Slot2.PhraseId == 398 &&
                    message.Slot2.TotalPaid == "RM11.95", "new collect total includes pending"); checks++;
                var reset = next.AdmittedStartGame();
                Require(reset.Collect == 0 && reset.Cancel == 0 && reset.TotalPaid == 0 && reset.CollectPending &&
                    reset.Messages(true, rm, 100) == null, "admitted start retains pending byte"); checks++;
                Require(reset.CashoutCompleted().TotalPaid == 0, "retained pending with zero collect"); checks++;
                foreach (int cancelType in new[] { 0x2500, 0x3500 })
                {
                    var canceled = next.DisplayUpdate(true, true, cancelType, 3);
                    message = canceled.Messages(true, rm, 100);
                    Require(canceled.Collect == 0 && canceled.Cancel == 3 && canceled.TotalPaid == 14.95 &&
                        !canceled.CollectPending && message.Slot2.PhraseId == 400 &&
                        message.Slot2.Amount == "RM3.00" && message.Slot2.TotalPaid == "RM14.95" &&
                        message.Slot3.PhraseId == 58, "cancel accounts pending and amount"); checks++;
                    var firstCancel = empty.DisplayUpdate(true, true, cancelType, 3);
                    Require(firstCancel.Messages(true, rm, 100).Slot2.PhraseId == 399, "first cancel single phrase"); checks++;
                    Require(firstCancel.DisplayUpdate(true, true, cancelType, 3).TotalPaid == 6,
                        "native does not deduplicate cancel transport"); checks++;
                }
            }
            var both = new OriginalPaymentMessageState(2, 3, 4, true).Messages(true, rm, 100);
            Require(both.Slot2.PhraseId == 400 && both.Slot3.PhraseId == 398 && both.Slot3.TotalPaid == "RM6.00",
                "restored both fields use cancel then collect"); checks++;
            var unusual = new OriginalPaymentMessageState(4, 0, 4, true).Messages(true, rm, 100);
            Require(unusual.Slot2.PhraseId == 397, "native phrase equality tests stored total"); checks++;
            var wide = new OriginalPaymentMessageState(1, 0, 9007199254740992d, true);
            Require(wide.Messages(true, rm, 1).Slot2.TotalPaid == "RM90,071,992,547,409.94",
                "80-bit total passed directly to cash conversion"); checks++;
            Require(wide.DisplayUpdate(true, true, 0x3500, 1).TotalPaid == 9007199254740994d,
                "FST does not round live cancel sum"); checks++;
            Require(wide.CashoutCompleted().TotalPaid == 9007199254740992d,
                "completion FSTP rounds stored sum separately"); checks++;
            foreach (double invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                bool rejected = false;
                try { wide.DisplayUpdate(true, true, 0x3500, invalid); } catch (ArgumentOutOfRangeException) { rejected = true; }
                Require(rejected && wide.TotalPaid == 9007199254740992d && wide.Collect == 1 && wide.CollectPending,
                    "finite admission rejects without changing original state"); checks++;
            }
            Require(OriginalPaymentMessageUpdate.State == 2 && OriginalPaymentMessageUpdate.OnMilliseconds == 2500 &&
                OriginalPaymentMessageUpdate.OffMilliseconds == 250, "original state and durations"); checks++;
            return checks;
        }
    }
}
