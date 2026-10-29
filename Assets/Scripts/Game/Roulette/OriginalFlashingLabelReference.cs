using System;

namespace RouletteDeluxe.Validation
{
    public static class OriginalFlashingLabelReference
    {
        private static void Require(bool value, string message)
        { if (!value) throw new Exception("Original FlashingLabel: " + message); }
        public static int Validate()
        {
            int checks = 0;
            var label = new OriginalFlashingLabel(1000);
            label.SetMessage(2, 0, "Game Over", "original", 2500, 250);
            label.SetMessage(2, 2, "COLLECT RM9.95", "original", 2500, 250);
            label.SetMessage(2, 3, "Game Over", "original", 2500, 250);
            Require(label.MessageCount(2) == 4, "blank gap is retained"); checks++;
            label.SetState(2, 0);
            Require(label.Text == "" && !label.OnPhase && label.Cursor == 0 && label.DelayMilliseconds == 1,
                "state reset does not immediately cycle"); checks++;
            Require(!label.Service(0, false), "one millisecond initial delay"); checks++;
            Require(label.Service(1, false) && label.Text == "Game Over" && label.Cursor == 1 && label.Visible &&
                label.OnPhase && label.DelayMilliseconds == 2500, "first callback"); checks++;
            Require(!label.Service(2500, false), "on boundary has not expired"); checks++;
            Require(label.Service(2501, false) && !label.Visible && label.Cursor == 1 && label.DelayMilliseconds == 250,
                "off preserves cursor and text"); checks++;
            Require(label.Service(2751, false) && label.Text == "" && label.Font == "original" && label.Cursor == 2,
                "blank slot participates; blank font retains bitmap font"); checks++;
            Require(label.Service(5251, true) && !label.OnPhase && label.Visible && label.Cursor == 2,
                "suspended off does not hide"); checks++;
            Require(label.Service(5501, true) && label.OnPhase && label.Visible && label.Cursor == 2 && label.Text == "",
                "suspended on does not cycle"); checks++;
            Require(label.Service(8001, false) && !label.Visible && label.Cursor == 2, "resume uses current phase"); checks++;
            Require(label.Service(8251, false) && label.Text == "COLLECT RM9.95" && label.Cursor == 3,
                "resume consumes next slot"); checks++;
            label.SetShow(false);
            label.Service(10751, false); label.Service(11001, false);
            Require(!label.Visible && label.Text == "Game Over" && label.Cursor == 0, "hidden still cycles and wraps"); checks++;
            label.SetShow(true);
            Require(label.Visible, "show flag acts immediately even during off"); checks++;
            label.SetMessage(2, 0, "Changed", "newfont", 100, 50);
            Require(label.Cursor == 0 && label.DelayMilliseconds == 2500, "message write does not rearm"); checks++;
            label.SetState(2, 12000);
            label.Service(12001, false);
            Require(label.Text == "Changed" && label.DelayMilliseconds == 100 && label.Font == "newfont",
                "state loads timing from first slot"); checks++;
            label.Service(12151, false);
            Require(label.DelayMilliseconds == 1 && label.Callbacks == 11, "late callback saturates to one ms without synthetic catchup"); checks++;
            label.SetState(2, 13000);
            label.Service(13001, false, null, () => 13011);
            Require(label.DelayMilliseconds == 90, "handler duration contributes to lateness"); checks++;
            bool clockRead = false;
            label.Service(13101, false, value => value.SetState(0, 13101), () => { clockRead = true; return 13101; });
            Require(!clockRead && !label.Running && label.Text == "" && label.State == 0,
                "explicit handler arm overrides returned delay"); checks++;
            var wrap = new OriginalFlashingLabel(1000);
            wrap.SetMessage(1, 0, "wrap", "", 2500, 250);
            wrap.SetState(1, 4294967295L);
            Require(wrap.Service(4294967296L, false) && wrap.DelayMilliseconds == 2500, "native uint millisecond wrap"); checks++;
            var fractional = new OriginalFlashingLabel(1001);
            fractional.SetMessage(1, 0, "fraction", "", 2500, 250);
            fractional.SetState(1, 0);
            Require(!fractional.Service(1, false) && fractional.Service(2, false), "non-integral timer delay rounds up"); checks++;
            label.ApplyPaymentUpdate(new OriginalPaymentMessageUpdate(
                new OriginalPaymentMessageSlot(397, "RM1.00", null), new OriginalPaymentMessageSlot(58, null, null)),
                slot => slot.PhraseId == 58 ? "Game Over" : "COLLECT " + slot.Amount, "original", 14000);
            Require(label.MessageCount(2) == 4 && label.Cursor == 0 && label.DelayMilliseconds == 1,
                "payment update preserves original slots and resets state"); checks++;
            label.ApplyPaymentUpdate(null, null, null, -1);
            Require(label.Running && label.Cursor == 0, "no payment leaves label unchanged"); checks++;
            return checks;
        }
    }
}
