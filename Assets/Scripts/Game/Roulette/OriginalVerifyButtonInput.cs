using System;

namespace RouletteDeluxe
{
    public static class OriginalVerifyButtonText
    {
        public static byte[] Copy(byte[] input)
        {
            if (input == null) throw new ArgumentNullException("input");
            int length = Array.IndexOf(input, (byte)0); if (length < 0) length = input.Length;
            length = Math.Min(length, 63);
            var result = new byte[length + 1]; Array.Copy(input, result, length); return result;
        }
    }
    // Events must already have been admitted by the host Sprite/Video route.
    // Captures and synthetic timestamp/payload construction remain host effects.
    public sealed class OriginalVerifyButtonInput
    {
        public struct Effect
        {
            public bool CaptureTouches, CaptureButtons, CallbackInvoked, ReturnEvent;
            public uint ForwardedType;
            public OriginalTextButtonPresentationState.Update Presentation;
        }
        private readonly OriginalTextButtonPresentationState yes, no;
        public bool Closed { get; private set; }
        public bool YesSelected { get; private set; }
        public bool NoSelected { get; private set; }
        public bool ExitSelected { get; private set; }
        public OriginalVerifyButtonInput(OriginalTextButtonPresentationState yesState, OriginalTextButtonPresentationState noState)
        { yes = yesState ?? throw new ArgumentNullException("yesState"); no = noState ?? throw new ArgumentNullException("noState"); }
        public Effect HandleAdmittedEvent(bool yesButton, uint type, bool ignoreDrags)
        {
            var state = yesButton ? yes : no;
            var effect = new Effect { ForwardedType = type, ReturnEvent = true };
            if (type == 0x200002) { effect.ReturnEvent = !ignoreDrags; return effect; }
            if (type == 0x300001) { effect.Presentation = state.Release(); effect.ReturnEvent = false; return effect; }
            if (type == 0x200001)
            { effect.Presentation = state.Press(true, true); effect.CaptureTouches = true; }
            else if (type == 0x200004)
            { if (state.Pressed) effect.Presentation = state.Release(); }
            else if (type == 0x200010)
            { effect.Presentation = state.Press(true, true); effect.CaptureButtons = true; effect.ForwardedType = 0x200001; }
            else if (type == 0x200020)
            { effect.Presentation = state.Release(); effect.ForwardedType = 0x200004; }
            // Native Sprite handler forwards every remaining event unchanged;
            // VerifyScreen callbacks set flags without testing event type/state.
            effect.CallbackInvoked = true; Closed = true;
            if (yesButton) YesSelected = true; else NoSelected = true;
            return effect;
        }
        public void SelectExit() { ExitSelected = true; }
        public bool TryGetDisplayResult(out int result)
        {
            result = 0;
            if (!Closed && !ExitSelected) return false;
            if (YesSelected) result = 1;
            if (NoSelected) result = 0;
            if (ExitSelected) result = -1;
            return true;
        }
    }
    public static class OriginalVerifyButtonInputValidation
    {
        public static int Validate()
        {
            int checks = 0;
            Action<bool> check = condition => { if (!condition) throw new Exception("Verify input regression " + checks); ++checks; };
            var bytes = new byte[80]; for (int i = 0; i < bytes.Length; ++i) bytes[i] = 65;
            var text = OriginalVerifyButtonText.Copy(bytes); check(text.Length == 64 && text[62] == 65 && text[63] == 0);
            text = OriginalVerifyButtonText.Copy(new byte[] {65,32,66,0,10}); check(text.Length == 4 && text[1] == 32 && text[3] == 0);
            text = OriginalVerifyButtonText.Copy(new byte[] {65,10,66}); check(text.Length == 4 && text[1] == 10);
            check(OriginalVerifyButtonText.Copy(new byte[0]).Length == 1);
            var yes = new OriginalTextButtonPresentationState(); var no = new OriginalTextButtonPresentationState();
            yes.Init(); no.Init(); var input = new OriginalVerifyButtonInput(yes, no); int result;
            check(!input.TryGetDisplayResult(out result));
            var e = input.HandleAdmittedEvent(true, 0x200002, true); check(!e.ReturnEvent && !e.CallbackInvoked && !input.Closed);
            e = input.HandleAdmittedEvent(true, 0x200002, false); check(e.ReturnEvent && !e.CallbackInvoked && !input.Closed);
            e = input.HandleAdmittedEvent(true, 0x200001, true);
            check(e.CaptureTouches && e.CallbackInvoked && e.Presentation.Frame == 2 && e.Presentation.SoundRequested);
            check(input.TryGetDisplayResult(out result) && result == 1 && input.YesSelected);
            e = input.HandleAdmittedEvent(true, 0x200004, true); check(e.CallbackInvoked && e.Presentation.Frame == 0 && !yes.Pressed);
            e = input.HandleAdmittedEvent(false, 0x200010, true);
            check(e.CaptureButtons && !e.CaptureTouches && e.ForwardedType == 0x200001 && no.Pressed);
            check(input.TryGetDisplayResult(out result) && result == 0 && input.YesSelected && input.NoSelected);
            e = input.HandleAdmittedEvent(false, 0x200020, true); check(e.ForwardedType == 0x200004 && !no.Pressed && e.CallbackInvoked);
            input.SelectExit(); check(input.TryGetDisplayResult(out result) && result == -1);
            input = new OriginalVerifyButtonInput(yes, no); yes.Press(true, false);
            e = input.HandleAdmittedEvent(true, 0x300001, true); check(!e.ReturnEvent && !e.CallbackInvoked && !input.Closed && !yes.Pressed);
            yes.Enabling(false); e = input.HandleAdmittedEvent(true, 0x200001, true);
            check(!e.Presentation.UpdateImage && e.CaptureTouches && e.CallbackInvoked && yes.State == 3);
            input = new OriginalVerifyButtonInput(yes, no); e = input.HandleAdmittedEvent(false, 0x1234, true);
            check(e.CallbackInvoked && e.ForwardedType == 0x1234 && input.TryGetDisplayResult(out result) && result == 0);
            return checks;
        }
    }
}
