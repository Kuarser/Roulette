using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RouletteDeluxe
{
    // Original glibc LC_MONETARY fields, and the game's FormatCurrency %n path.
    // This is not a general strfmon format-string interpreter or wallet owner.
    public sealed class OriginalMonetaryLocale
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly string currency, point, separator, positiveSign, negativeSign;
        private readonly byte[] grouping;
        private readonly int fraction, positiveBefore, negativeBefore, positiveSpace,
            negativeSpace, positivePosition, negativePosition;
        public string ResourceSha256 { get; private set; }
        public string CurrencySymbol { get { return currency; } }
        public int FractionDigits { get { return fraction; } }

        private OriginalMonetaryLocale(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length < 192 || Word(bytes, 0) != 0x20031111 || Word(bytes, 4) != 46)
                throw new InvalidDataException("Original LC_MONETARY header is invalid");
            int[] offsets = new int[47];
            for (int i = 0; i < 46; i++)
            {
                uint offset = Word(bytes, 8 + i * 4);
                if (offset < 192 || offset >= bytes.Length || (i > 0 && offset <= offsets[i - 1]))
                    throw new InvalidDataException("Original LC_MONETARY offset is invalid");
                offsets[i] = (int)offset;
            }
            offsets[46] = bytes.Length;
            currency = Text(bytes, offsets, 1); point = Text(bytes, offsets, 2);
            separator = Text(bytes, offsets, 3); positiveSign = Text(bytes, offsets, 5);
            negativeSign = Text(bytes, offsets, 6);
            int groupLength = offsets[5] - offsets[4];
            grouping = new byte[groupLength]; Array.Copy(bytes, offsets[4], grouping, 0, groupLength);
            if (grouping[groupLength - 1] != 0) throw new InvalidDataException("Monetary grouping is not terminated");
            fraction = Byte(bytes, offsets, 8); if (fraction == 255) fraction = 2;
            // All 136 supplied resources use zero or two fractional digits.
            if (fraction != 0 && fraction != 2) throw new InvalidDataException("Unadmitted monetary fractional precision");
            positiveBefore = Byte(bytes, offsets, 9) == 0 ? 0 : 1;
            positiveSpace = Space(Byte(bytes, offsets, 10));
            negativeBefore = Byte(bytes, offsets, 11) == 0 ? 0 : 1;
            negativeSpace = Space(Byte(bytes, offsets, 12));
            positivePosition = Position(Byte(bytes, offsets, 13));
            negativePosition = Position(Byte(bytes, offsets, 14));
            using (var sha = SHA256.Create()) ResourceSha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }
        public static OriginalMonetaryLocale Read(byte[] bytes) { return new OriginalMonetaryLocale(bytes); }
        private static uint Word(byte[] b, int at)
        { return (uint)b[at] | (uint)b[at + 1] << 8 | (uint)b[at + 2] << 16 | (uint)b[at + 3] << 24; }
        private static int Byte(byte[] b, int[] o, int i)
        { if (o[i + 1] - o[i] != 1) throw new InvalidDataException("Monetary scalar has wrong size"); return b[o[i]]; }
        private static string Text(byte[] b, int[] o, int i)
        {
            int end = o[i + 1] - 1;
            if (b[end] != 0) throw new InvalidDataException("Monetary text is not terminated");
            for (int k = o[i]; k < end; k++) if (b[k] == 0) throw new InvalidDataException("Monetary text has embedded terminator");
            return Utf8.GetString(b, o[i], end - o[i]);
        }
        private static int Space(int n)
        { if (n == 255) return 0; if (n > 2) throw new InvalidDataException("Invalid currency spacing"); return n; }
        private static int Position(int n)
        { if (n == 255) return 1; if (n > 4) throw new InvalidDataException("Invalid currency sign position"); return n; }

        public long PaymentMinorUnits(double credits, long denomination)
        {
            if (double.IsInfinity(credits) || double.IsNaN(credits) || denomination <= 0)
                throw new ArgumentOutOfRangeException("credits");
            return PaymentMinorUnits(OriginalBinary80.FromDouble(credits), denomination);
        }
        internal long PaymentMinorUnits(OriginalBinary80 credits, long denomination)
        {
            if (denomination <= 0) throw new ArgumentOutOfRangeException("denomination");
            var amount = OriginalBinary80.Multiply(credits, OriginalBinary80.Integer64(denomination));
            return OriginalBinary80.Add(amount, OriginalBinary80.FromDouble(0.01)).TruncateInt64();
        }
        internal string FormatPaymentCredits(OriginalBinary80 credits, long denomination)
        { return FormatMinorUnits(PaymentMinorUnits(credits, denomination)); }
        public string FormatPaymentCredits(double credits, long denomination)
        { return FormatMinorUnits(PaymentMinorUnits(credits, denomination)); }

        public string FormatMinorUnits(long minorUnits)
        {
            // Original CheckLocaleDigits truncates zero-digit currency before FDIV.
            if (fraction == 0) minorUnits = minorUnits / 100 * 100;
            double value = OriginalBinary80.Divide(OriginalBinary80.Integer64(minorUnits), OriginalBinary80.Integer(100)).ToDouble();
            bool negative = value < 0;
            BigInteger decimalValue = DecimalRoundedMagnitude(value, fraction);
            string digits = decimalValue.ToString(CultureInfo.InvariantCulture);
            if (fraction > 0) digits = digits.PadLeft(fraction + 1, '0');
            string whole = fraction == 0 ? digits : digits.Substring(0, digits.Length - fraction);
            string number = Group(whole) + (fraction == 0 ? "" : point + digits.Substring(digits.Length - fraction));
            return Place(number, negative);
        }
        private static BigInteger DecimalRoundedMagnitude(double value, int fraction)
        {
            ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value)) & 0x7fffffffffffffffUL;
            int encodedExponent = (int)(bits >> 52);
            BigInteger numerator = bits & 0x000fffffffffffffUL;
            if (encodedExponent != 0) numerator |= new BigInteger(0x0010000000000000UL);
            int exponent = encodedExponent == 0 ? -1074 : encodedExponent - 1023 - 52;
            numerator *= BigInteger.Pow(10, fraction);
            if (exponent >= 0) return numerator << exponent;
            BigInteger divisor = BigInteger.One << -exponent, remainder;
            BigInteger result = BigInteger.DivRem(numerator, divisor, out remainder);
            int midpoint = (remainder << 1).CompareTo(divisor);
            return midpoint > 0 || (midpoint == 0 && !result.IsEven) ? result + 1 : result;
        }
        private string Group(string digits)
        {
            if (separator.Length == 0 || grouping[0] == 0 || grouping[0] >= 127) return digits;
            var parts = new List<string>(); int end = digits.Length, index = 0, size = grouping[0];
            while (end > size)
            {
                parts.Add(digits.Substring(end - size, size)); end -= size;
                int next = grouping[Math.Min(index + 1, grouping.Length - 1)];
                if (next >= 127) break;
                if (next != 0) { size = next; index++; }
            }
            parts.Add(digits.Substring(0, end)); parts.Reverse(); return string.Join(separator, parts.ToArray());
        }
        private string Place(string number, bool negative)
        {
            string sign = negative ? (negativeSign.Length == 0 ? "-" : negativeSign) : positiveSign;
            bool before = (negative ? negativeBefore : positiveBefore) != 0;
            int position = negative ? negativePosition : positivePosition;
            int space = negative ? negativeSpace : positiveSpace;
            if (space == 2 && (position == 0 || (position == 1 && !before) || (position == 2 && before))) space = 0;
            // Assemble currency/sign as a unit; type 1 separates that unit from
            // the number, type 2 separates adjacent currency and sign.
            string result;
            if (position == 3 || position == 4)
            {
                string unit = position == 3 ? sign + (space == 2 ? " " : "") + currency
                    : currency + (space == 2 ? " " : "") + sign;
                result = before ? unit + (space == 1 ? " " : "") + number
                    : number + (space == 1 ? " " : "") + unit;
            }
            else
            {
                result = before ? currency + (space == 1 ? " " : "") + number
                    : number + (space == 1 ? " " : "") + currency;
                if (position == 1) result = sign + (space == 2 ? " " : "") + result;
                if (position == 2) result += (space == 2 ? " " : "") + sign;
                if (position == 0 && negative) result = "(" + result + ")";
            }
            return result;
        }
    }
}
