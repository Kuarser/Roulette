using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public static class OriginalVerifyFontSelection
    {
        private static readonly string[] Arial = {
            "arial-bold-20-white", "arial-bold-16-white", "arial-bold-14-white",
            "arial-bold-12-white", "arial-bold-10-white", "arial-bold-8-white", "arial-bold-6-white" };
        private static readonly string[] Simsun = {
            "simsun-14-white-uni", "simsun-14-white-uni", "simsun-14-white-uni",
            "simsun-12-white-uni", "simsun-10-white-uni", "simsun-8-white-uni", "simsun-6-white-uni" };
        private static bool Chinese(string language, Func<string, string> nativeLowercase)
        {
            if (language == null) throw new ArgumentNullException("language");
            if (nativeLowercase == null) throw new ArgumentNullException("nativeLowercase");
            // Native SetLang uses C tolower. Its LC_CTYPE-dependent mapping
            // must be supplied by the host, rather than .NET casing.
            string lower = nativeLowercase(language);
            return lower == "zh" || lower == "chinese";
        }
        public static string TextResource(string language, int slot, Func<string, string> nativeLowercase)
        {
            if (slot < 0 || slot >= 7) throw new ArgumentOutOfRangeException("slot");
            return "OS366J/" + (Chinese(language, nativeLowercase) ? Simsun : Arial)[slot];
        }
        public static string ButtonResource(string language, Func<string, string> nativeLowercase)
        { return "OS366J/" + (Chinese(language, nativeLowercase) ? "simsun-10-black-uni" : "arial-bold-8-black"); }
    }

    // VerifyScreen AddText/Align byte contract. The callback supplies the
    // original signed16 CalcSize width, including its LC_CTYPE behavior.
    public sealed class OriginalVerifyTextLayout
    {
        public sealed class Line
        {
            private readonly byte[] bytes;
            public readonly int FontSlot, FontHeight, Spacing;
            internal Line(byte[] value, int slot, int height)
            { bytes = value; FontSlot = slot; FontHeight = height; Spacing = height / 3; }
            public byte[] Bytes { get { return (byte[])bytes.Clone(); } }
        }
        public struct Placement
        { public int X, Y, HorizontalAlignment, VerticalAlignment; }
        private readonly List<Line> lines = new List<Line>();
        private readonly int displayLeft, displayRight;
        private readonly float normalizedWidth;
        public int LastFontHeight { get; private set; }
        public int LastSpacing { get; private set; }
        public int Count { get { return lines.Count; } }
        public Line this[int index] { get { return lines[index]; } }
        public OriginalVerifyTextLayout(int left, int right, float width)
        {
            if (float.IsNaN(width) || float.IsInfinity(width) || width < 0 || width > 1)
                throw new ArgumentOutOfRangeException("width", "Native constructor range only");
            displayLeft = left; displayRight = right; normalizedWidth = width;
        }
        private int Percent { get { return (int)((double)normalizedWidth * 100d); } }
        public int EffectiveWidth
        { get { return unchecked((displayRight - displayLeft) * Percent) / 100 - 20; } }
        public bool AddText(byte[] input, int fontSlot, int fontHeight, Func<byte[], short> calcSize)
        {
            if (input == null || calcSize == null) throw new ArgumentNullException();
            if (fontSlot < 0 || fontSlot >= 7) throw new ArgumentOutOfRangeException("fontSlot");
            LastFontHeight = fontHeight; LastSpacing = fontHeight / 3;
            if (Count >= 32) return false;
            int length = Array.IndexOf(input, (byte)0);
            if (length < 0) length = input.Length;
            byte[] remaining = Slice(input, 0, length);
            do
            {
                int split;
                if (remaining.Length > 127)
                {
                    // This branch does not prepare a remainder or measure.
                    lines.Add(new Line(Slice(remaining, 0, 127), fontSlot, fontHeight));
                    return true;
                }
                if (EffectiveWidth > calcSize((byte[])remaining.Clone())) split = remaining.Length;
                else if (EffectiveWidth < 1) split = 0;
                else
                {
                    int consumed = 0, width = 0, lastSpace = 0;
                    while (consumed < remaining.Length && width < EffectiveWidth)
                    {
                        byte value = remaining[consumed];
                        if (value == 0x20) lastSpace = consumed;
                        width = unchecked(width + calcSize(new byte[] { value }));
                        ++consumed;
                    }
                    split = lastSpace == 0 || lastSpace == consumed - 1 ? consumed : lastSpace;
                }
                lines.Add(new Line(Slice(remaining, 0, split), fontSlot, fontHeight));
                remaining = Slice(remaining, split, remaining.Length - split);
                // The outer native call ignores a recursive AddText false.
            } while (remaining.Length != 0 && Count < 32);
            return true;
        }
        public Placement[] Align(int styleOffset, int horizontalAlignment, int verticalAlignment)
        {
            var result = new Placement[Count];
            if (Count == 0) return result;
            int cursor = unchecked(styleOffset + lines[0].Spacing);
            int x = unchecked((displayRight - displayLeft) * Percent) / 200;
            for (int i = 0; i < Count; ++i)
            {
                Line line = lines[i];
                result[i] = new Placement { X = x, Y = unchecked(cursor + line.Spacing),
                    HorizontalAlignment = horizontalAlignment, VerticalAlignment = verticalAlignment };
                cursor = unchecked(cursor + line.Spacing + line.FontHeight);
            }
            LastFontHeight = lines[Count - 1].FontHeight;
            return result;
        }
        private static byte[] Slice(byte[] source, int start, int count)
        { var result = new byte[count]; Array.Copy(source, start, result, 0, count); return result; }
    }
}
