using System;
namespace RouletteDeluxe
{
    // Original libstdc++ 6.0.16 narrow num_get<uint32>, fresh decimal stream.
    // Admitted ASCII classic locale, full std::string bytes (including NUL).
    public static class OriginalConfigurationUInt32Stream
    {
        public static uint Extract(byte[] raw,uint previous)
        {
            if(raw==null)throw new ArgumentNullException(nameof(raw));
            for(int i=0;i<raw.Length;++i)if(raw[i]>127)throw new ArgumentException("ASCII classic locale input required",nameof(raw));
            int at=0;while(at<raw.Length&&(raw[at]==32||(raw[at]>=9&&raw[at]<=13)))++at;
            if(at==raw.Length)return previous; // Failed sentry never calls num_get.
            bool negative=false;
            if(raw[at]==43||raw[at]==45){negative=raw[at]==45;++at;}
            int first=at;uint value=0;bool overflow=false;
            while(at<raw.Length&&raw[at]>=48&&raw[at]<=57)
            {
                uint digit=(uint)(raw[at]-48);
                if(value>(uint.MaxValue-digit)/10)overflow=true;
                if(!overflow)value=value*10+digit;
                ++at;
            }
            if(at==first)return 0;
            if(overflow)return uint.MaxValue; // Saturation precedes applying the sign.
            return negative?unchecked(0U-value):value;
        }
    }
}
