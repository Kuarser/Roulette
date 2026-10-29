using System;

namespace RouletteDeluxe.Validation
{
    public static class PcVirtualPaymentPresentationReceiptReference
    {
        private static void Require(bool ok, string reason)
        { if (!ok) throw new Exception("Virtual presentation receipt: " + reason); }
        public static int Validate()
        {
            int checks = 0; string identity = new string('A', 64);
            var before = new OriginalPaymentMessageState();
            var started = PcVirtualPaymentPresentationReceipt.Begin(identity, true, 995, 100, before);
            Require(started.After.Collect == 9.95 && started.After.CollectPending && !started.Completed, "explicit hopper update"); checks++;
            var restored = PcVirtualPaymentPresentationReceipt.Decode(started.Encode(), identity, true, 995, 100, before);
            Require(restored.After.CollectPending && restored.Encode() == started.Encode(), "pending bit-exact restore"); checks++;
            var complete = restored.Complete();
            Require(complete.After.TotalPaid == 9.95 && !complete.After.CollectPending && before.TotalPaid == 0,
                "completion without modifying predecessor"); checks++;
            Require(ReferenceEquals(complete, complete.Complete()), "repeat completion does not accumulate"); checks++;
            restored = PcVirtualPaymentPresentationReceipt.Decode(complete.Encode(), identity, true, 995, 100, before);
            Require(restored.Completed && ReferenceEquals(restored, restored.Complete()), "cold completion is already consumed"); checks++;
            var next = PcVirtualPaymentPresentationReceipt.Begin(new string('B', 64), true, 200, 100, complete.After).Complete();
            Require(next.After.TotalPaid == 11.95 && next.After.Collect == 2, "multiple transaction totals"); checks++;
            var nonAu = PcVirtualPaymentPresentationReceipt.Begin(identity, false, 995, 100, before).Complete();
            Require(nonAu.After.TotalPaid == 0 && nonAu.After.Collect == 0, "non-AU does not invent messages"); checks++;
            var wideBefore = new OriginalPaymentMessageState(0, 0, 9007199254740992d, false);
            var wide = PcVirtualPaymentPresentationReceipt.Begin(identity, true, 1, 1, wideBefore);
            Require(PcVirtualPaymentPresentationReceipt.Decode(wide.Encode(), identity, true, 1, 1, wideBefore).After.Collect == 1,
                "wide predecessor bits preserved"); checks++;
            for (int kind = 0; kind < 6; kind++)
            {
                bool rejected = false;
                try
                {
                    string raw = complete.Encode();
                    if (kind == 4) { var bytes = Convert.FromBase64String(raw); bytes[10] ^= 1; raw = Convert.ToBase64String(bytes); }
                    if (kind == 5) raw = raw.Substring(0, raw.Length - 4);
                    PcVirtualPaymentPresentationReceipt.Decode(raw, kind == 0 ? new string('B', 64) : identity,
                        kind != 1, kind == 2 ? 996 : 995, kind == 3 ? 1 : 100, before);
                }
                catch (Exception e) { if (e is ArgumentException || e is System.IO.IOException || e is System.IO.InvalidDataException || e is FormatException) rejected = true; else throw; }
                Require(rejected, "wrong source facts or corrupt payload " + kind); checks++;
            }
            bool wrongPredecessor = false;
            try { PcVirtualPaymentPresentationReceipt.Decode(complete.Encode(), identity, true, 995, 100, complete.After); }
            catch (System.IO.InvalidDataException) { wrongPredecessor = true; }
            Require(wrongPredecessor, "payload cannot admit its own predecessor"); checks++;
            return checks;
        }
    }
}
