using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreen::AddText (086c1850). CalcSize returns a signed16 width;
    // it measures the whole prefix, with the width of literal W reserved.
    public static class OriginalTextScreenWrapping
    {
        public static bool AddText(OriginalHandpayBannerLayout layout, byte[] text, int fontSlot,
            int fontHeight, int fontMode, bool bitmapFontPresent, Func<int, byte[], short> measure)
        {
            if (layout == null || text == null) throw new ArgumentNullException();
            if (fontSlot < 0 || fontSlot >= 13) throw new ArgumentOutOfRangeException(nameof(fontSlot));
            if (fontMode == 1 || fontMode == 2)
                return layout.AddSingleLine(text, fontSlot, fontHeight, bitmapFontPresent);
            if (measure == null) throw new ArgumentNullException(nameof(measure));
            // Native lazily creates the font when missing; the callback must use
            // that actual font, and added lines retain the created bitmap pointer.
            int width = layout.Pixels().Width;
            int reserved = measure(fontSlot, new byte[] { (byte)'W' });
            int length = Array.IndexOf(text, (byte)0); if (length < 0) length = text.Length;
            byte[] source = new byte[length]; Array.Copy(text, source, length);
            var current = new List<byte>();
            int lastSpace = 0, prefixLength = 0; bool result = false;
            for (int index = 0; index < source.Length; ++index)
            {
                current.Add(source[index]);
                if (source[index] == (byte)' ') { lastSpace = index; prefixLength = current.Count; }
                int measured = measure(fontSlot, current.ToArray());
                if (unchecked(measured + reserved) < width) continue;
                if (lastSpace != 0)
                {
                    current.RemoveRange(prefixLength, current.Count - prefixLength);
                    index = lastSpace;
                }
                else current.Add((byte)'-');
                result = layout.AddSingleLine(current.ToArray(), fontSlot, fontHeight, true);
                current.Clear(); lastSpace = 0;
            }
            if (current.Count != 0) result = layout.AddSingleLine(current.ToArray(), fontSlot, fontHeight, true);
            return result;
        }
    }
}
