using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RouletteDeluxe
{
    // OS libsdg bally::FormatMoney, ELF59c0; distinct from Game strfmon.
    // Returns native string bytes: its numeric construction reverses bytes,
    // including multibyte separators. This class owns no credit or payment state.
    public sealed class OriginalPlatformMonetaryLocale
    {
        private readonly byte[] currency, internationalCurrency, point, separator,
            grouping, positiveSign, negativeSign;
        private readonly int fraction, internationalFraction, positivePosition,
            negativePosition;
        private readonly bool positiveBefore, negativeBefore, positiveSpace, negativeSpace;
        public string ResourceSha256 { get; private set; }

        private OriginalPlatformMonetaryLocale(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length < 192 || Word(bytes, 0) != 0x20031111 || Word(bytes, 4) != 46)
                throw new InvalidDataException("Original LC_MONETARY header is invalid");
            int[] offsets = new int[47];
            for (int i = 0; i < 46; ++i)
            {
                uint offset = Word(bytes, 8 + i * 4);
                if (offset < 192 || offset >= bytes.Length || (i > 0 && offset <= offsets[i - 1]))
                    throw new InvalidDataException("Original LC_MONETARY offset is invalid");
                offsets[i] = (int)offset;
            }
            offsets[46] = bytes.Length;
            internationalCurrency = TextBytes(bytes, offsets, 0);
            currency = TextBytes(bytes, offsets, 1);
            point = TextBytes(bytes, offsets, 2);
            separator = TextBytes(bytes, offsets, 3);
            // Preserve the terminator for native GroupingInfo's repeat rule.
            byte[] groups = TextBytes(bytes, offsets, 4);
            grouping = new byte[groups.Length + 1];
            Array.Copy(groups, grouping, groups.Length);
            positiveSign = TextBytes(bytes, offsets, 5);
            negativeSign = TextBytes(bytes, offsets, 6);
            internationalFraction = Scalar(bytes, offsets, 7);
            fraction = Scalar(bytes, offsets, 8);
            positiveBefore = Scalar(bytes, offsets, 9) != 0;
            positiveSpace = Scalar(bytes, offsets, 10) != 0;
            negativeBefore = Scalar(bytes, offsets, 11) != 0;
            negativeSpace = Scalar(bytes, offsets, 12) != 0;
            positivePosition = Scalar(bytes, offsets, 13);
            negativePosition = Scalar(bytes, offsets, 14);
            using (var sha = SHA256.Create())
                ResourceSha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }

        public static OriginalPlatformMonetaryLocale Read(byte[] bytes)
        { return new OriginalPlatformMonetaryLocale(bytes); }

        private static uint Word(byte[] b, int at)
        { return (uint)b[at] | (uint)b[at + 1] << 8 | (uint)b[at + 2] << 16 | (uint)b[at + 3] << 24; }

        private static int Scalar(byte[] b, int[] o, int i)
        {
            if (o[i + 1] - o[i] != 1) throw new InvalidDataException("Monetary scalar has wrong size");
            // Original MOVSX treats char fields as signed; no strfmon defaults.
            return unchecked((sbyte)b[o[i]]);
        }

        private static byte[] TextBytes(byte[] b, int[] o, int i)
        {
            int length = o[i + 1] - o[i] - 1;
            if (length < 0 || b[o[i] + length] != 0)
                throw new InvalidDataException("Monetary text is not terminated");
            for (int k = 0; k < length; ++k)
                if (b[o[i] + k] == 0) throw new InvalidDataException("Monetary text has embedded terminator");
            byte[] result = new byte[length]; Array.Copy(b, o[i], result, 0, length);
            return result;
        }

        public byte[] FormatBytes(long amount, uint divisor, bool international, uint width)
        {
            if (width > int.MaxValue) throw new ArgumentOutOfRangeException("width", "Managed array limit");
            string digits = amount.ToString(CultureInfo.InvariantCulture);
            if (amount < 0) digits = digits.Substring(1); // also safe for Int64.MinValue
            int index = digits.Length - 1;
            int precision = international ? internationalFraction : fraction;
            uint remaining = unchecked((uint)precision);
            var number = new List<byte>();
            while (divisor > 1)
            {
                uint next = unchecked(remaining - 1);
                if (next < 126)
                {
                    number.Add(index < 0 ? (byte)'0' : (byte)digits[index]);
                    remaining = next;
                }
                --index;
                divisor /= 10;
            }
            if (precision > 0) number.AddRange(point);
            remaining = unchecked(remaining - 1);
            while (remaining < 126)
            {
                number.Insert(0, (byte)'0');
                remaining = unchecked(remaining - 1);
            }
            int groupIndex = 0, count = 0;
            int groupSize = NextGroup(ref groupIndex);
            if (index < 0) number.Add((byte)'0');
            while (index >= 0)
            {
                number.Add((byte)digits[index]);
                ++count;
                if (groupSize > 0 && index > 0 && count % groupSize == 0)
                {
                    number.AddRange(separator);
                    count = 0;
                    groupSize = NextGroup(ref groupIndex);
                }
                --index;
            }
            number.Reverse();
            bool negative = amount < 0;
            byte[] result = Place(number.ToArray(), international ? internationalCurrency : currency,
                negative ? negativeSign : positiveSign,
                negative ? negativePosition : positivePosition,
                negative ? negativeBefore : positiveBefore,
                negative ? negativeSpace : positiveSpace);
            if (result.Length < width)
            {
                byte[] padded = new byte[(int)width];
                int padding = padded.Length - result.Length;
                for (int i = 0; i < padding; ++i) padded[i] = (byte)' ';
                Array.Copy(result, 0, padded, padding, result.Length); result = padded;
            }
            return result;
        }

        public string FormatText(long amount, uint divisor, bool international, uint width)
        {
            // Invalid UTF8 from native byte reversal is not silently repaired.
            return new UTF8Encoding(false, true).GetString(FormatBytes(amount, divisor, international, width));
        }

        private int NextGroup(ref int index)
        {
            int size = 0;
            if (index >= 0)
            {
                byte next = grouping[0];
                if (next == 0 || next == 127) return 0;
                int position = 0;
                while (true)
                {
                    ++position; size = unchecked((sbyte)next);
                    if (index < position) break;
                    next = grouping[position];
                    if (next == 0) return size;
                    if (next == 127) return 0;
                }
            }
            if (index < grouping.Length - 2) ++index;
            return size;
        }

        private static byte[] Place(byte[] number, byte[] symbol, byte[] sign,
            int position, bool before, bool spaced)
        {
            byte[] space = spaced ? new byte[] { 32 } : new byte[0];
            byte[][] pieces;
            switch (position)
            {
                case 0: pieces = before ? new[] { new byte[] { 40 }, symbol, space, number, new byte[] { 41 } }
                    : new[] { new byte[] { 40 }, number, space, symbol, new byte[] { 41 } }; break;
                case 1: pieces = before ? new[] { sign, symbol, space, number }
                    : new[] { sign, number, space, symbol }; break;
                case 2: pieces = before ? new[] { symbol, space, number, sign }
                    : new[] { number, space, symbol, sign }; break;
                case 3: pieces = before ? new[] { sign, symbol, space, number }
                    : new[] { number, sign, space, symbol }; break;
                case 4: pieces = before ? new[] { symbol, space, sign, number }
                    : new[] { number, space, symbol, sign }; break;
                default: pieces = new[] { number }; break;
            }
            var result = new List<byte>();
            foreach (byte[] piece in pieces) result.AddRange(piece);
            while (result.Count > 0 && result[result.Count - 1] == 32) result.RemoveAt(result.Count - 1);
            return result.ToArray();
        }
    }
}
