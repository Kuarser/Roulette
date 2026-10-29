using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public sealed class OriginalVideoInputEvent
    {
        public uint Type, ButtonMask;
        public ushort Size;
        public int Seconds, Nanoseconds, Display;
        public OriginalSpriteInputTarget.Point Point;
    }
    // Managed host must supply validity, clock and windows in native order.
    // Event disposal is observable; it does not claim original pointer lifetime.
    public sealed class OriginalVideoInputDispatch
    {
        public struct ClockSample { public int Seconds, Microseconds; }
        public delegate OriginalVideoInputEvent Handler(OriginalVideoInputEvent input, OriginalSpriteInputTarget origin);
        public readonly List<OriginalSpriteInputTarget> Windows = new List<OriginalSpriteInputTarget>();
        public readonly Queue<OriginalVideoInputEvent> InputQueue = new Queue<OriginalVideoInputEvent>();
        private readonly Dictionary<int, OriginalSpriteInputTarget> touches = new Dictionary<int, OriginalSpriteInputTarget>();
        private readonly Dictionary<OriginalSpriteInputTarget, Handler> handlers = new Dictionary<OriginalSpriteInputTarget, Handler>();
        private readonly Func<OriginalSpriteInputTarget, bool> valid;
        private readonly Func<ClockSample> clock;
        private readonly Action<OriginalVideoInputEvent> dispose;
        public int CurrentDisplay { get; private set; }
        public OriginalSpriteInputTarget ButtonsCaptured { get; private set; }
        public OriginalVideoInputDispatch(Func<OriginalSpriteInputTarget, bool> validity, Func<ClockSample> nativeClock,
            Action<OriginalVideoInputEvent> eventDisposed)
        { valid = validity ?? throw new ArgumentNullException("validity"); clock = nativeClock ?? throw new ArgumentNullException("nativeClock"); dispose = eventDisposed ?? throw new ArgumentNullException("eventDisposed"); }
        public void SetHandler(OriginalSpriteInputTarget target, Handler handler) { handlers[target] = handler; }
        public void RemoveHandler(OriginalSpriteInputTarget target) { handlers.Remove(target); }
        public OriginalSpriteInputTarget TouchCaptured(int display)
        { OriginalSpriteInputTarget target; return touches.TryGetValue(display - 1, out target) ? target : null; }
        public void CaptureTouches(OriginalSpriteInputTarget target)
        { if (CurrentDisplay < 1) throw new InvalidOperationException("Native capture Fault0x10: display<1"); touches[CurrentDisplay - 1] = target; }
        public void CaptureButtons(OriginalSpriteInputTarget target) { ButtonsCaptured = target; }
        public bool ProcessOne()
        { if (InputQueue.Count == 0) return false; Dispatch(InputQueue.Dequeue()); return true; }
        public OriginalVideoInputEvent Synthetic(uint type, uint buttonMask)
        {
            var stamp = clock();
            return new OriginalVideoInputEvent { Type = type, Size = (ushort)(type == 0x200020 ? 0x14 : 0x1c),
                Seconds = stamp.Seconds, Nanoseconds = unchecked(stamp.Microseconds * 1000), ButtonMask = buttonMask,
                Point = new OriginalSpriteInputTarget.Point(0, 0), Display = type == 0x200020 ? 0 : 1 };
        }
        public void Bubble(OriginalSpriteInputTarget target, OriginalVideoInputEvent input)
        {
            var origin = target;
            while (target != null && valid(target))
            {
                if ((target.Flags & 0x20000) != 0) { target = target.Parent; continue; }
                Handler handler; var next = handlers.TryGetValue(target, out handler) && handler != null ? handler(input, origin) : input;
                if (!valid(target)) break;
                if (!ReferenceEquals(input, next)) dispose(input);
                if (next == null) return;
                if (IsTouch(next.Type))
                    next.Point = new OriginalSpriteInputTarget.Point(unchecked((short)(next.Point.X + target.Rectangle.Left)),
                        unchecked((short)(next.Point.Y + target.Rectangle.Top)));
                target = target.Parent; input = next;
            }
            dispose(input);
        }
        private static bool IsTouch(uint type) { return type == 0x200001 || type == 0x200002 || type == 0x200004; }
        public void Dispatch(OriginalVideoInputEvent input)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (input.Type == 0x200010 || input.Type == 0x200020)
            {
                var captured = ButtonsCaptured;
                if (captured != null)
                {
                    if (!valid(captured)) { ButtonsCaptured = null; if (input.Type == 0x200020) return; }
                    else if (input.Type == 0x200020) { ButtonsCaptured = null; Bubble(captured, input); return; }
                    else { Bubble(captured, Synthetic(0x200020, captured.ButtonMask)); ButtonsCaptured = null; }
                }
                foreach (var window in Windows)
                { var target = window.FindButton(input.Type, input.ButtonMask); if (target != null) { Bubble(target, input); return; } }
                Bubble(null, input); return;
            }
            if (!IsTouch(input.Type)) { dispose(input); return; }
            CurrentDisplay = input.Display;
            if (CurrentDisplay < 1) throw new InvalidOperationException("Native dispatch Fault0x10: display<1");
            var touch = TouchCaptured(CurrentDisplay);
            if (touch != null)
            {
                if (!valid(touch))
                {
                    if (input.Type == 0x200002) return;
                    touches[CurrentDisplay - 1] = null;
                    if (input.Type == 0x200004) return;
                }
                if (input.Type != 0x200001)
                {
                    input.Point = touch.ToLocal(input.Point);
                    if (input.Type == 0x200004) touches[CurrentDisplay - 1] = null;
                    Bubble(touch, input); return;
                }
                Bubble(touch, Synthetic(0x200004, 0)); touches[CurrentDisplay - 1] = null;
            }
            var point = input.Point;
            var selected = OriginalSpriteInputTarget.FindWindow(Windows, ref point);
            if (selected != null) input.Point = point;
            Bubble(selected, input);
        }
        public void AttachVerifyButton(OriginalSpriteInputTarget target, OriginalVerifyButtonInput input, bool yesButton, bool ignoreDrags)
        {
            SetHandler(target, (value, origin) => {
                var effect = input.HandleAdmittedEvent(yesButton, value.Type, ignoreDrags);
                if (effect.CaptureTouches) CaptureTouches(target);
                if (effect.CaptureButtons) CaptureButtons(target);
                if (!effect.ReturnEvent) return null;
                return effect.ForwardedType == value.Type ? value : Synthetic(effect.ForwardedType, 0);
            });
        }
    }
}
