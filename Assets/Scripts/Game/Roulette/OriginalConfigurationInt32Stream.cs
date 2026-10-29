using System;
namespace RouletteDeluxe
{
    // Fresh narrow decimal stream, ASCII classic locale; full std::string bytes.
    public static class OriginalConfigurationInt32Stream
    {
        public static int Extract(byte[] raw,int? previous)
        {
            int? result=ExtractTarget(raw,previous);
            if(!result.HasValue)throw new InvalidOperationException("Original uninitialized stream target requires caller storage");return result.Value;
        }
        public static int? ExtractTarget(byte[] raw,int? previous)
        {
            if(raw==null)throw new ArgumentNullException(nameof(raw));
            for(int i=0;i<raw.Length;++i)if(raw[i]>127)throw new ArgumentException("ASCII classic locale input required",nameof(raw));
            int at=0;while(at<raw.Length&&(raw[at]==32||(raw[at]>=9&&raw[at]<=13)))++at;
            if(at==raw.Length)return previous;
            bool negative=false;if(raw[at]==43||raw[at]==45){negative=raw[at]==45;++at;}
            int first=at;uint value=0,limit=negative?0x80000000U:0x7fffffffU;bool overflow=false;
            while(at<raw.Length&&raw[at]>=48&&raw[at]<=57)
            {uint digit=(uint)(raw[at]-48);if(value>(limit-digit)/10)overflow=true;if(!overflow)value=value*10+digit;++at;}
            if(at==first)return 0;if(overflow)value=limit;
            return unchecked((int)(negative?0U-value:value));
        }
    }
}
