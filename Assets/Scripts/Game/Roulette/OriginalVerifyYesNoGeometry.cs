using System;

namespace RouletteDeluxe
{
    public sealed class OriginalVerifyYesNoGeometry
    {
        public OriginalSpriteAlignment.Rectangle Panel { get; private set; }
        public OriginalSpriteAlignment.Rectangle Border { get; private set; }
        public OriginalSpriteAlignment.Rectangle YesButton { get; private set; }
        public OriginalSpriteAlignment.Rectangle NoButton { get; private set; }
        public bool ShowYes { get; private set; }
        public bool ShowNo { get; private set; }
        public bool ReturnImmediately { get; private set; }
        public readonly int PanelWidth, PanelHeight;
        public OriginalVerifyYesNoGeometry(int displayWidth, int displayHeight, int lineCount, bool yes, bool no)
        {
            if (displayWidth < 1 || displayWidth > short.MaxValue || displayHeight < 1 || displayHeight > short.MaxValue)
                throw new ArgumentOutOfRangeException("displayWidth", "Positive native signed16 viewport only");
            if (lineCount < 0 || lineCount > 32) throw new ArgumentOutOfRangeException("lineCount");
            short left = (short)Pixels(0.2f, displayWidth), top = (short)Pixels(0.33f, displayHeight);
            PanelWidth = Pixels(0.6f, displayWidth); PanelHeight = Pixels(0.3f, displayHeight);
            short right = unchecked((short)(left + (short)PanelWidth)), bottom = unchecked((short)(top + (short)PanelHeight));
            Panel = new OriginalSpriteAlignment.Rectangle { Left = left, Top = top, Right = right, Bottom = bottom };
            Border = new OriginalSpriteAlignment.Rectangle { Left = unchecked((short)(left - 4)), Top = unchecked((short)(top - 4)),
                Right = unchecked((short)(right + 4)), Bottom = unchecked((short)(bottom + 4)) };
            ReturnImmediately = lineCount == 0;
            ShowYes = yes && !ReturnImmediately; ShowNo = no && !ReturnImmediately;
            short y = unchecked((short)(top + (short)PanelHeight - PanelHeight * 10 / 100));
            OriginalSpriteAlignment.Rectangle rect;
            if (ShowYes)
            {
                short x = unchecked((short)(left + PanelWidth * (no ? 10 : 50) / 100));
                OriginalSpriteAlignment.Align(x, y, 120, 60, no ? -1 : 0, 1, 0, 0, out rect);
                YesButton = rect;
            }
            if (ShowNo)
            {
                short x = unchecked((short)(right - PanelWidth * (yes ? 10 : 50) / 100));
                OriginalSpriteAlignment.Align(x, y, 120, 60, yes ? 1 : 0, 1, 0, 0, out rect);
                NoButton = rect;
            }
        }
        private static int Pixels(float normalized, int extent)
        { return (int)(((double)normalized * 100d * (float)extent) / 100d); }
    }
    public static class OriginalTextButtonLabelGeometry
    {
        public struct Anchor { public short X, Y; public int HorizontalAlignment, VerticalAlignment; }
        public static Anchor[] Layout(byte[] nativeText, short width, short height)
        {
            if (nativeText == null) throw new ArgumentNullException("nativeText");
            int count = 1;
            foreach (byte value in nativeText)
            { if (value == 0) break; if (value == 10) ++count; }
            if (count > short.MaxValue) throw new ArgumentOutOfRangeException("nativeText", "Native line-count overflow unqualified");
            int spacing = count == 2 ? height / 10 : count == 3 ? height / 20 : 0;
            short step = unchecked((short)((height - spacing * 2) / (count + 1)));
            var result = new Anchor[count];
            for (int i = 0; i < count; ++i)
                result[i] = new Anchor { X = unchecked((short)(width >> 1)), Y = unchecked((short)(spacing + (i + 1) * step)),
                    HorizontalAlignment = 0, VerticalAlignment = 0 };
            return result;
        }
    }
    public static class OriginalVerifyYesNoGeometryValidation
    {
        private static int checks;
        private static void Require(bool condition)
        { if (!condition) throw new Exception("Original YesNo geometry regression " + checks); ++checks; }
        public static int Validate()
        {
            checks = 0;
            var g = new OriginalVerifyYesNoGeometry(800, 600, 2, true, true);
            Require(g.Panel.Left == 160 && g.Panel.Top == 198 && g.Panel.Right == 640 && g.Panel.Bottom == 378);
            Require(g.Border.Left == 156 && g.Border.Top == 194 && g.Border.Right == 644 && g.Border.Bottom == 382);
            Require(g.ShowYes && g.ShowNo && g.YesButton.Left == 208 && g.YesButton.Top == 300 && g.YesButton.Right == 328);
            Require(g.NoButton.Left == 472 && g.NoButton.Right == 592 && g.NoButton.Top == 300);
            g = new OriginalVerifyYesNoGeometry(800, 600, 2, true, false);
            Require(g.ShowYes && !g.ShowNo && g.YesButton.Left == 340 && g.YesButton.Right == 460);
            g = new OriginalVerifyYesNoGeometry(800, 600, 2, false, true);
            Require(!g.ShowYes && g.ShowNo && g.NoButton.Left == 340 && g.NoButton.Right == 460);
            g = new OriginalVerifyYesNoGeometry(800, 600, 0, true, true);
            Require(g.ReturnImmediately && !g.ShowYes && !g.ShowNo);
            g = new OriginalVerifyYesNoGeometry(540, 960, 2, true, true);
            Require(g.Panel.Left == 108 && g.Panel.Top == 316 && g.PanelWidth == 324 && g.PanelHeight == 288);
            Require(g.YesButton.Left == 140 && g.YesButton.Top == 516 && g.NoButton.Left == 280 && g.NoButton.Top == 516);
            var lines = OriginalTextButtonLabelGeometry.Layout(new byte[0], 120, 60);
            Require(lines.Length == 1 && lines[0].X == 60 && lines[0].Y == 30);
            lines = OriginalTextButtonLabelGeometry.Layout(new byte[] { 65, 10, 66 }, 120, 60);
            Require(lines.Length == 2 && lines[0].Y == 22 && lines[1].Y == 38);
            lines = OriginalTextButtonLabelGeometry.Layout(new byte[] { 65, 10, 66, 10, 67 }, 120, 60);
            Require(lines.Length == 3 && lines[0].Y == 16 && lines[1].Y == 29 && lines[2].Y == 42);
            lines = OriginalTextButtonLabelGeometry.Layout(new byte[] { 10, 10, 10 }, 120, 60);
            Require(lines.Length == 4 && lines[0].Y == 12 && lines[3].Y == 48);
            lines = OriginalTextButtonLabelGeometry.Layout(new byte[] { 65, 0, 10 }, 121, 61);
            Require(lines.Length == 1 && lines[0].X == 60 && lines[0].Y == 30);
            lines = OriginalTextButtonLabelGeometry.Layout(new byte[0], -5, -7);
            Require(lines[0].X == -3 && lines[0].Y == -3);
            return checks;
        }
    }
}
