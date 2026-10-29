using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreenHandPay derives from TextScreen, whose normalized pixel conversion
    // uses (display - clip - border), float32 ratios and x87 truncation toward zero.
    public sealed class OriginalHandpayBannerLayout
    {
        public const int NativeOrder = 9100;
        public const uint BackgroundRgb = 0x0000ff, BorderRgb = 0xffff00;
        public sealed class Line
        {
            public byte[] Text;
            public int FontSlot, Height, Spacing, AnchorX, AnchorY, HorizontalAlignment, VerticalAlignment;
            public bool BitmapFontPresent;
        }
        public sealed class Geometry
        {
            public int X, Y, Width, Height;
            public bool ShowBorder;
            public OriginalSpriteAlignment.Rectangle Window, Body, Border;
        }
        private readonly List<Line> lines = new List<Line>();
        public IReadOnlyList<Line> Lines { get { return lines; } }
        public readonly int DisplayWidth, DisplayHeight;
        public int ClipX, ClipY, BorderWidth = 5, HorizontalAlignment, VerticalAlignment;
        public bool Centering;
        public float NormalizedX = 0.22f, NormalizedY = 0.41f, NormalizedWidth = 0.56f, NormalizedHeight = 0.156f;
        public int CurrentFontHeight { get; private set; }
        public int CurrentSpacing { get; private set; }
        public void ClearNativeLines() { lines.Clear(); Centering = false; }
        public void ClearLinesForUpdate() { lines.Clear(); }
        public void SetHandpayDefaults()
        {
            NormalizedX = 0.22f; NormalizedY = 0.41f; NormalizedWidth = 0.56f; NormalizedHeight = 0.156f;
            ClipX = ClipY = HorizontalAlignment = VerticalAlignment = 0; BorderWidth = 5;
        }
        public OriginalHandpayBannerLayout(int width, int height)
        {
            if (width < 1 || width > short.MaxValue || height < 1 || height > short.MaxValue)
                throw new ArgumentOutOfRangeException("Native signed16 display dimensions required");
            DisplayWidth = width; DisplayHeight = height;
        }
        private static short S16(int value) { return unchecked((short)value); }
        private static OriginalSpriteAlignment.Rectangle Rect(int left, int top, int right, int bottom)
        { return new OriginalSpriteAlignment.Rectangle { Left = S16(left), Top = S16(top), Right = S16(right), Bottom = S16(bottom) }; }
        private static int Pixel(float ratio, int available)
        {
            double value = (double)ratio * available;
            if (double.IsNaN(value) || double.IsInfinity(value) || value < int.MinValue || value > int.MaxValue)
                throw new ArgumentOutOfRangeException("Unsupported normalized pixel conversion");
            return (int)value;
        }
        public Geometry Pixels()
        {
            int availableX = unchecked(DisplayWidth - ClipX - BorderWidth), availableY = unchecked(DisplayHeight - ClipY - BorderWidth);
            int x = Pixel(NormalizedX, availableX), y = Pixel(NormalizedY, availableY);
            int width = Pixel(NormalizedWidth, availableX), height = Pixel(NormalizedHeight, availableY);
            return new Geometry
            {
                X = x, Y = y, Width = width, Height = height,
                ShowBorder = BorderWidth > 0,
                Window = Rect(ClipX, ClipY, DisplayWidth, DisplayHeight),
                Body = Rect(x, y, unchecked(x + width), unchecked(y + height)),
                Border = Rect(Math.Max(0, unchecked(x - BorderWidth)), Math.Max(0, unchecked(y - BorderWidth)),
                    Math.Min(DisplayWidth, unchecked(x + width + BorderWidth)), Math.Min(DisplayHeight, unchecked(y + height + BorderWidth)))
            };
        }
        // This is AddSingleLineText, not TextScreen::AddText wrapping. The caller
        // supplies actual native bitmap/TTF font height and bitmap pointer presence.
        public bool AddSingleLine(byte[] text, int fontSlot, int fontHeight, bool bitmapFontPresent)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (fontSlot < 0 || fontSlot >= 13) throw new ArgumentOutOfRangeException(nameof(fontSlot));
            CurrentFontHeight = fontHeight; CurrentSpacing = Centering ? 0 : fontHeight / 3;
            if (lines.Count >= 32) return false;
            int length = Array.IndexOf(text, (byte)0); if (length < 0) length = text.Length; length = Math.Min(length, 127);
            byte[] copied = new byte[length]; Array.Copy(text, copied, length);
            lines.Add(new Line { Text = copied, FontSlot = fontSlot, Height = fontHeight, Spacing = CurrentSpacing, BitmapFontPresent = bitmapFontPresent });
            return true;
        }
        public Geometry Align(Func<int, byte[], short> nativeMeasureWidth)
        {
            if (nativeMeasureWidth == null) throw new ArgumentNullException(nameof(nativeMeasureWidth));
            Geometry initial = Pixels(); int cursor = BorderWidth, widest = 0;
            foreach (Line line in lines)
            {
                cursor = unchecked(cursor + line.Spacing); CurrentFontHeight = line.Height;
                line.AnchorX = initial.Width / 2;
                line.AnchorY = unchecked(cursor + (line.Spacing > 0 ? line.Height / 2 : 0));
                line.HorizontalAlignment = HorizontalAlignment; line.VerticalAlignment = Centering ? -1 : VerticalAlignment;
                if (line.BitmapFontPresent) widest = Math.Max(widest, nativeMeasureWidth(line.FontSlot, (byte[])line.Text.Clone()));
                cursor = unchecked(cursor + line.Height);
            }
            if (initial.Height < cursor)
            {
                int available = unchecked(DisplayHeight - ClipY - BorderWidth);
                if (available == 0) throw new ArgumentOutOfRangeException("Zero native height denominator");
                NormalizedHeight = (float)(((double)cursor * 100.0 / available) / 100.0);
            }
            Geometry resized = Pixels();
            if (resized.Width < widest)
            {
                int available = unchecked(DisplayWidth - ClipX - BorderWidth);
                if (available == 0) throw new ArgumentOutOfRangeException("Zero native width denominator");
                NormalizedWidth = (float)((double)widest / available);
            }
            // Native Align recalculates panel pixels after growth, but does not
            // recalculate the anchors written earlier in this same call.
            return Pixels();
        }
    }
}
