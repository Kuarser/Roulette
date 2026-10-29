using System;
namespace RouletteDeluxe
{
    // Original CFGStringConverter09400300/094000e0/09400020: 32-bit long,
    // base 0 and retained errno. Explicit ASCII C-locale admission only.
    public static class OriginalCfgIntegerConverter
    {
        public sealed class State{public int Errno;}
        private static bool Space(byte b){return b==32||(b>=9&&b<=13);}
        private static int Digit(byte b){if(b>=48&&b<=57)return b-48;if(b>=65&&b<=70)return b-65+10;if(b>=97&&b<=102)return b-97+10;return -1;}
        private static uint Parse(byte[] supplied,State state,bool signed,out int end)
        {
            if(supplied==null||state==null)throw new ArgumentNullException();
            int length=Array.IndexOf(supplied,(byte)0);if(length<0)length=supplied.Length;
            for(int j=0;j<length;++j)if(supplied[j]>127)throw new ArgumentException("ASCII C-locale integer input required");
            int at=0;while(at<length&&Space(supplied[at]))++at;bool negative=false;
            if(at<length&&(supplied[at]==43||supplied[at]==45)){negative=supplied[at]==45;++at;}
            int radix=10;
            if(at<length&&supplied[at]==48)
            {
                radix=8;
                if(at+2<length&&(supplied[at+1]==120||supplied[at+1]==88)&&Digit(supplied[at+2])>=0)
                {radix=16;at+=2;}
            }
            int start=at;ulong value=0;bool overflow=false;ulong limit=signed?(negative?2147483648UL:2147483647UL):4294967295UL;
            while(at<length)
            {
                int digit=Digit(supplied[at]);if(digit<0||digit>=radix)break;
                if(value>(limit-(ulong)digit)/(ulong)radix)overflow=true;
                if(!overflow)value=value*(ulong)radix+(ulong)digit;++at;
            }
            end=at==start?0:at;if(end==0)return 0;
            if(overflow){state.Errno=34;return signed?(negative?0x80000000u:0x7fffffffu):uint.MaxValue;}
            uint result=(uint)value;return negative?unchecked(0u-result):result;
        }
        private static uint Convert(byte[] raw,byte[] message,State state,bool signed,bool narrow)
        {
            if(message==null)throw new ArgumentNullException();int end;uint value=Parse(raw,state,signed,out end);
            bool atNul=end==raw.Length||raw[end]==0;
            if(state.Errno==34||raw.Length==0||!atNul||(narrow&&value>=256))throw new OriginalCfgDictionaryException(message);
            return value;
        }
        public static int ToInt32(byte[] raw,byte[] message,State state){return unchecked((int)Convert(raw,message,state,true,false));}
        public static uint ToUInt32(byte[] raw,byte[] message,State state){return Convert(raw,message,state,false,false);}
        public static byte ToUInt8(byte[] raw,byte[] message,State state){return (byte)Convert(raw,message,state,false,true);}
    }
}
