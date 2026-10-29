using System;

namespace RouletteDeluxe
{
    // RngMgr::GetUnsignedRandArray 083c0b90 normalizes reversed endpoints.
    // Its 32-bit Boost generator 083bef90 returns a singleton without a draw,
    // directly returns a full-domain draw, and otherwise rejects a high tail.
    public static class OriginalUnsignedRngRange
    {
        public static bool TryMap(uint raw,uint lower,uint upper,out uint result)
        {
            Normalize(ref lower,ref upper);
            ulong count=(ulong)upper-lower+1;
            if(count==1){result=lower;return true;}
            ulong bucket=0x100000000UL/count;
            ulong value=raw/bucket;
            if(value>=count){result=0;return false;}
            result=unchecked(lower+(uint)value);return true;
        }
        public static uint DrawInclusive(uint lower,uint upper,Func<uint> nextRaw)
        {
            if(nextRaw==null)throw new ArgumentNullException("nextRaw");
            Normalize(ref lower,ref upper);
            if(lower==upper)return lower;
            for(;;)
            {
                uint result;if(TryMap(nextRaw(),lower,upper,out result))return result;
            }
        }
        private static void Normalize(ref uint lower,ref uint upper)
        {if(upper<lower){uint saved=lower;lower=upper;upper=saved;}}
    }
}
