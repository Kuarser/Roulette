using System;

namespace RouletteDeluxe
{
    public static class OriginalSpriteAlignment
    {
        public struct Rectangle { public short Left, Top, Right, Bottom; }
        public static bool Align(short anchorX, short anchorY, short width, short height,
            int horizontal, int vertical, short previousLeft, short previousTop, out Rectangle rectangle)
        {
            int left = anchorX, top = anchorY;
            if (horizontal != -1) left -= horizontal == 1 ? width : width / 2;
            if (vertical != -1) top -= vertical == 1 ? height : height / 2;
            bool moved = previousLeft != left || previousTop != top;
            rectangle = new Rectangle { Left = unchecked((short)left), Top = unchecked((short)top),
                Right = unchecked((short)(width + unchecked((short)left))),
                Bottom = unchecked((short)(height + unchecked((short)top))) };
            return moved;
        }
    }
    public sealed class OriginalSpriteFrameState
    {
        public int Frame { get; private set; }
        public readonly int FrameCount;
        public OriginalSpriteFrameState(int currentFrame, int frameCount)
        { Frame = currentFrame; FrameCount = frameCount; }
        public bool SetFrame(int requested)
        {
            if (Frame == requested) return false;
            Frame = requested < 0 || requested >= FrameCount ? 0 : requested;
            return true;
        }
    }
    // Presentation state only. Sound availability/volume, event ownership,
    // MNG playback and Sprite input capture belong to the native host bridge.
    public sealed class OriginalTextButtonPresentationState
    {
        public struct Update
        { public bool UpdateImage, SoundRequested; public int Frame; public short TextX, TextY; }
        public int State { get; private set; }
        public bool Pressed { get { return State == 2; } }
        public bool Active { get { return State == 1 || State == 2; } }
        public OriginalTextButtonPresentationState() { State = 3; }
        private Update Image(bool sound)
        { return new Update { UpdateImage = true, Frame = State, TextX = (short)(Pressed ? 2 : 0),
            TextY = (short)(Pressed ? 2 : 0), SoundRequested = sound }; }
        public Update Init() { State = 0; return Image(false); }
        public Update Enabling(bool value) { State = value ? 0 : 3; return Image(false); }
        public Update Flash(bool value) { State = value ? 1 : 0; return Image(false); }
        public Update Press(bool down, bool requestSound)
        {
            if (State == 1 || State == 3 || (State == 0 && !down) || (State == 2 && down))
                return new Update();
            if (State == 0) { State = 2; return Image(requestSound); }
            State = 0; return Image(false);
        }
        public Update Release() { return Press(false, false); }
    }
    public static class OriginalSpriteButtonGeometryValidation
    {
        private static int checks;
        private static void Require(bool condition)
        { if (!condition) throw new Exception("Original sprite/button regression " + checks); ++checks; }
        public static int Validate()
        {
            checks = 0;
            OriginalSpriteAlignment.Rectangle r;
            Require(OriginalSpriteAlignment.Align(100, 200, 5, 7, 0, 0, 0, 0, out r) &&
                r.Left == 98 && r.Top == 197 && r.Right == 103 && r.Bottom == 204);
            Require(!OriginalSpriteAlignment.Align(100, 200, 5, 7, 0, 0, 98, 197, out r));
            Require(OriginalSpriteAlignment.Align(100, 200, 5, 7, -1, 1, 0, 0, out r) && r.Left == 100 && r.Top == 193);
            Require(OriginalSpriteAlignment.Align(100, 200, -5, -7, 0, 0, 0, 0, out r) && r.Left == 102 && r.Top == 203);
            Require(OriginalSpriteAlignment.Align(100, 200, 5, 7, 99, 99, 0, 0, out r) && r.Left == 98 && r.Top == 197);
            Require(OriginalSpriteAlignment.Align(32767, 0, -2, 1, 1, -1, 0, 0, out r) && r.Left == -32767 && r.Right == 32767);
            Require(OriginalSpriteAlignment.Align(32767, 0, -2, 1, 1, -1, -32767, 0, out r));
            var frames = new OriginalSpriteFrameState(0, 4);
            Require(!frames.SetFrame(0));
            Require(frames.SetFrame(2) && frames.Frame == 2);
            Require(frames.SetFrame(-1) && frames.Frame == 0);
            Require(frames.SetFrame(4) && frames.Frame == 0);
            Require(frames.SetFrame(4) && frames.Frame == 0);
            var button = new OriginalTextButtonPresentationState();
            Require(button.State == 3 && !button.Pressed && !button.Active && !button.Press(true, true).UpdateImage);
            Require(button.Init().Frame == 0 && button.State == 0 && !button.Active);
            var update = button.Press(true, true);
            Require(update.UpdateImage && update.Frame == 2 && update.TextX == 2 && update.TextY == 2 &&
                update.SoundRequested && button.Pressed && button.Active);
            Require(!button.Press(true, true).UpdateImage);
            update = button.Press(false, true);
            Require(update.Frame == 0 && update.TextX == 0 && !update.SoundRequested);
            Require(button.Flash(true).Frame == 1 && button.Active && !button.Pressed);
            Require(!button.Press(true, true).UpdateImage && !button.Release().UpdateImage && button.State == 1);
            Require(button.Enabling(false).Frame == 3 && !button.Active);
            Require(button.Flash(false).Frame == 0 && button.State == 0);
            Require(button.Enabling(true).UpdateImage && button.State == 0);
            Require(!button.Press(true, false).SoundRequested && button.Release().Frame == 0);
            return checks;
        }
    }
}
