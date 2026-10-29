using System;
using System.Numerics;
using System.Globalization;

namespace RouletteDeluxe
{
    // Finite x87 arithmetic with the source's nearest/even, 64-bit significand.
    // Stores are rounded separately, as FST does not round the live register.
    internal struct OriginalBinary80
    {
        private BigInteger mantissa;
        private int exponent;
        public static OriginalBinary80 Integer(int n) { return Round(n, BigInteger.One, 0); }
        public static OriginalBinary80 Integer64(long n) { return Round(n, BigInteger.One, 0); }
        public static OriginalBinary80 FromDouble(double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value)) throw new ArgumentOutOfRangeException("value");
            ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
            int e = (int)((bits >> 52) & 2047);
            BigInteger m = bits & 0x000fffffffffffffUL;
            if (e != 0) m |= 0x0010000000000000UL;
            if ((bits >> 63) != 0) m = -m;
            return new OriginalBinary80 { mantissa = m, exponent = e == 0 ? -1074 : e - 1023 - 52 };
        }
        private static int Bits(BigInteger n)
        {
            int count = 0;
            while (n != BigInteger.Zero) { n >>= 1; ++count; }
            return count;
        }
        private static OriginalBinary80 Round(BigInteger numerator, BigInteger denominator, int baseExponent, int precision = 64)
        {
            if (denominator.IsZero) throw new DivideByZeroException();
            if (numerator.IsZero) return new OriginalBinary80();
            int sign = numerator.Sign * denominator.Sign;
            numerator = BigInteger.Abs(numerator); denominator = BigInteger.Abs(denominator);
            int e = Bits(numerator) - Bits(denominator);
            if (e >= 0 ? numerator < (denominator << e) : (numerator << -e) < denominator) --e;
            int shift = precision - 1 - e;
            BigInteger n = shift >= 0 ? numerator << shift : numerator;
            BigInteger d = shift >= 0 ? denominator : denominator << -shift;
            BigInteger remainder;
            BigInteger m = BigInteger.DivRem(n, d, out remainder);
            int midpoint = (remainder << 1).CompareTo(d);
            if (midpoint > 0 || (midpoint == 0 && !m.IsEven)) ++m;
            if (Bits(m) > precision) { m >>= 1; ++e; }
            return new OriginalBinary80 { mantissa = sign < 0 ? -m : m, exponent = baseExponent + e - precision + 1 };
        }
        public static int Compare(OriginalBinary80 a, OriginalBinary80 b)
        {
            int e = Math.Min(a.exponent, b.exponent);
            return (a.mantissa << (a.exponent-e)).CompareTo(b.mantissa << (b.exponent-e));
        }
        public static OriginalBinary80 Add(OriginalBinary80 a, OriginalBinary80 b)
        {
            int e = Math.Min(a.exponent, b.exponent);
            return Round((a.mantissa << (a.exponent-e)) + (b.mantissa << (b.exponent-e)), BigInteger.One, e);
        }
        public static OriginalBinary80 Subtract(OriginalBinary80 a, OriginalBinary80 b)
        {
            int e = Math.Min(a.exponent, b.exponent);
            return Round((a.mantissa << (a.exponent-e)) - (b.mantissa << (b.exponent-e)), BigInteger.One, e);
        }
        public static OriginalBinary80 Multiply(OriginalBinary80 a, OriginalBinary80 b)
        { return Round(a.mantissa * b.mantissa, BigInteger.One, a.exponent+b.exponent); }
        public static OriginalBinary80 Divide(OriginalBinary80 a, OriginalBinary80 b)
        { return Round(a.mantissa, b.mantissa, a.exponent-b.exponent); }
        private double Stored(int precision)
        {
            OriginalBinary80 r = Round(mantissa, BigInteger.One, exponent, precision);
            double m = (double)(ulong)BigInteger.Abs(r.mantissa);
            return (r.mantissa.Sign < 0 ? -m : m) * Math.Pow(2, r.exponent);
        }
        public double ToDouble() { return Stored(53); }
        public float ToFloat() { return (float)Stored(24); }
        public int Truncate()
        {
            BigInteger m = BigInteger.Abs(mantissa);
            m = exponent < 0 ? m >> -exponent : m << exponent;
            return (int)(mantissa.Sign < 0 ? -m : m);
        }
        public long TruncateInt64()
        {
            BigInteger m = BigInteger.Abs(mantissa);
            m = exponent < 0 ? m >> -exponent : m << exponent;
            return (long)(mantissa.Sign < 0 ? -m : m);
        }
        // printf %Lf formats the live extended value, without a double store.
        internal string FormatFixed(int digits)
        {
            if (digits < 0 || digits > 10) throw new ArgumentOutOfRangeException("digits");
            BigInteger n = BigInteger.Abs(mantissa) * BigInteger.Pow(10, digits);
            BigInteger d = BigInteger.One;
            if (exponent >= 0) n <<= exponent; else d <<= -exponent;
            BigInteger remainder, rounded = BigInteger.DivRem(n, d, out remainder);
            int midpoint = (remainder << 1).CompareTo(d);
            if (midpoint > 0 || midpoint == 0 && !rounded.IsEven) ++rounded;
            string value = rounded.ToString(CultureInfo.InvariantCulture).PadLeft(digits + 1, '0');
            if (digits != 0) value = value.Insert(value.Length - digits, ".");
            return (mantissa.Sign < 0 ? "-" : "") + value;
        }
    }
}
