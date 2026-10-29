using System;
using System.Numerics;

namespace RouletteDeluxe
{
    // Finite binary arithmetic with the original x87 64-bit significand,
    // round-to-nearest/even. Feature money never approaches exponent limits.
    // NaNs, infinities and subnormals are deliberately outside this contract.
    public struct OriginalFeatureFloat80 : IComparable<OriginalFeatureFloat80>
    {
        private readonly BigInteger mantissa;
        private readonly int exponent;
        private OriginalFeatureFloat80(BigInteger m,int e) {mantissa=m;exponent=e;}
        public static OriginalFeatureFloat80 FromInt64(long n) {return Round(n,BigInteger.One,64);}
        public static OriginalFeatureFloat80 FromDouble(double n)
        {
            ulong bits=unchecked((ulong)BitConverter.DoubleToInt64Bits(n));
            int e=(int)((bits>>52)&2047);ulong m=bits&0xfffffffffffffUL;
            if(e==2047)throw new ArgumentOutOfRangeException("n","Finite original Feature value required");
            if(e==0&&m==0)return new OriginalFeatureFloat80();
            BigInteger value=e==0?m:m|(1UL<<52);
            if((bits>>63)!=0)value=-value;
            return new OriginalFeatureFloat80(value,e==0?-1074:e-1023-52);
        }
        private static int Bits(BigInteger n)
        {
            byte[] bytes=n.ToByteArray();int i=bytes.Length-1;
            while(i>0&&bytes[i]==0)i--;int b=0;byte top=bytes[i];
            while(top!=0){top>>=1;b++;}return i*8+b;
        }
        private static OriginalFeatureFloat80 Round(BigInteger n,BigInteger d,int precision)
        {
            if(d.IsZero)throw new DivideByZeroException();
            bool negative=n.Sign*d.Sign<0;n=BigInteger.Abs(n);d=BigInteger.Abs(d);
            if(n.IsZero)return new OriginalFeatureFloat80();
            int power=Bits(n)-Bits(d);
            if(power>=0){if(n<(d<<power))power--;}
            else if((n<<-power)<d)power--;
            int shift=precision-1-power;
            BigInteger denominator=d,numerator=n;
            if(shift>=0)numerator<<=shift;else denominator<<=-shift;
            BigInteger remainder;BigInteger m=BigInteger.DivRem(numerator,denominator,out remainder);
            int compare=(remainder*2).CompareTo(denominator);
            if(compare>0||(compare==0&&!m.IsEven))m++;
            if(Bits(m)>precision){m>>=1;shift--;}
            return new OriginalFeatureFloat80(negative?-m:m,-shift);
        }
        private void Fraction(out BigInteger n,out BigInteger d)
        {
            n=mantissa;d=BigInteger.One;
            if(exponent>=0)n<<=exponent;else d<<=-exponent;
        }
        public OriginalFeatureFloat80 Add(OriginalFeatureFloat80 other)
        {
            BigInteger a,b,c,d;Fraction(out a,out b);other.Fraction(out c,out d);
            return Round(a*d+c*b,b*d,64);
        }
        public OriginalFeatureFloat80 Negate() {return new OriginalFeatureFloat80(-mantissa,exponent);}
        public OriginalFeatureFloat80 Subtract(OriginalFeatureFloat80 other) {return Add(other.Negate());}
        public OriginalFeatureFloat80 Divide(OriginalFeatureFloat80 other)
        {
            BigInteger a,b,c,d;Fraction(out a,out b);other.Fraction(out c,out d);return Round(a*d,b*c,64);
        }
        public int Sign {get{return mantissa.Sign;}}
        public int CompareTo(OriginalFeatureFloat80 other)
        {
            BigInteger a,b,c,d;Fraction(out a,out b);other.Fraction(out c,out d);return (a*d).CompareTo(c*b);
        }
        public long TruncateInt64()
        {
            BigInteger n,d;Fraction(out n,out d);BigInteger result=n/d;
            // FISTP's integer-indefinite result for an out-of-range conversion.
            return result<long.MinValue||result>long.MaxValue?long.MinValue:(long)result;
        }
        public double ToDouble()
        {
            BigInteger n,d;Fraction(out n,out d);OriginalFeatureFloat80 rounded=Round(n,d,53);
            return (double)rounded.mantissa*Math.Pow(2,rounded.exponent);
        }
    }
}
