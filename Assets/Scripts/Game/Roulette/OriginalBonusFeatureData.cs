using System;

namespace RouletteDeluxe
{
    // Roulette::StateBonus state 1 (08198c15..08198fc6). Award records
    // are the original 42-byte records; only six nonzero Feature IDs enter
    // the dispatch list. This method does not create Feature providers.
    public static class OriginalBonusFeatureData
    {
        public sealed class Host
        {
            public Func<long> CurrentDenomination;
            public Func<long> WinLimit;
            public Func<byte> CurrencySymbolPrecedes;
            public Func<byte[]> CurrentLanguage;
            public Action<int,bool> SetFeatureDataBit;
            public Action SaveFeatureData;
            public Action InitFeatures;
        }
        public static bool Prepare(byte[] game,byte[] resultHeader,byte[][] pays,Host host)
        {
            if(game==null||game.Length<0x8000||resultHeader==null||resultHeader.Length<0x40||pays==null)
                throw new ArgumentException("Complete original Bonus data required");
            if(host==null||host.CurrentDenomination==null||host.WinLimit==null||host.CurrencySymbolPrecedes==null||
                host.CurrentLanguage==null||host.SetFeatureDataBit==null||host.SaveFeatureData==null||host.InitFeatures==null)
                throw new ArgumentException("Complete original Bonus owner required");
            int count=BitConverter.ToInt32(resultHeader,0x3c);
            // Native allocates 256 award slots. Invalid oversized input would
            // overwrite adjacent data; managed restoration rejects that input.
            if(count>256||count>pays.Length)throw new ArgumentException("Original award storage overflow");
            for(int i=0;i<count;i++)if(pays[i]==null||pays[i].Length!=42)
                throw new ArgumentException("Original Bonus pay must be exactly 42 bytes");
            for(int i=0;i<256;i++)
            {
                int o=0xbd3+12*i;
                game[o]=game[o+1]=0xff;
                for(int j=4;j<12;j++)game[o+j]=0xff;
                game[0x17d3+2*i]=0xff;game[0x17d4+2*i]=0;
            }
            int features=0;bool any=false;
            for(int i=0;i<count;i++)
            {
                byte[] pay=pays[i];int o=0xbd3+12*i;
                Buffer.BlockCopy(pay,5,game,o,2);Buffer.BlockCopy(pay,9,game,o+4,8);
                if(pay[0]==0)continue;
                any=true;
                if(features>=6)continue;
                game[0xbcd+features]=pay[0];
                Buffer.BlockCopy(pay,0,game,0xa5c+42*features,42);
                Write(game,0xb58+4*features,BitConverter.GetBytes(i));features++;
            }
            if(!any)return false;
            game[0xa55]=(byte)features;game[0xbb4]=0;
            Copy(game,0x4f68,0xb7c,8);game[0xb76]=game[0xb77]=0;
            Copy(game,0x910,0x1b3e,8);Copy(game,0x264,0xb78,4);
            Copy(game,0x4f64,0x1b38,2);Copy(game,0x4f64,0x1b3a,4);
            Write(game,0xb8c,BitConverter.GetBytes(host.CurrentDenomination()));
            long limit=host.WinLimit();Write(game,0xb84,BitConverter.GetBytes(limit==0?2000000000.0:(double)limit));
            Copy(game,0x8c5,0xbac,4);Zero(game,0xbb0,4);
            Buffer.BlockCopy(resultHeader,0x34,game,0xb94,8);
            Zero(game,0xba0,4);Zero(game,0xbb5,4);Zero(game,0xbc1,4);
            Copy(game,0x8d1,0xb9c,4);Copy(game,0x8c9,0xbbd,4);
            game[0x1ad7]=game[0x268];game[0x1ad8]=game[0x269];
            game[0x1ada]=unchecked((byte)(host.CurrencySymbolPrecedes()^1));
            game[0x1eb3]=0;game[0xbca]=0;
            CopyString(game,0x7fc0,0x1af4);
            byte[] language=host.CurrentLanguage();
            if(language==null||Array.IndexOf(language,(byte)0)<0)throw new ArgumentException("Original language needs a NUL terminator");
            WriteString(game,0x1af4,language,0);
            CopyString(game,0x102,0x1b18);
            for(int i=0;i<8;i++)game[0xba4+i]=0xff;
            game[0xbc9]=0;game[0xbcb]=game[0xbcc]=0;
            host.SetFeatureDataBit(0xb5,false);host.SaveFeatureData();host.InitFeatures();
            return true;
        }
        private static void Copy(byte[] game,int from,int to,int length) {Buffer.BlockCopy(game,from,game,to,length);}
        private static void Write(byte[] game,int to,byte[] value) {Buffer.BlockCopy(value,0,game,to,value.Length);}
        private static void Zero(byte[] game,int to,int length) {Array.Clear(game,to,length);}
        private static void CopyString(byte[] game,int from,int to) {WriteString(game,to,game,from);}
        private static void WriteString(byte[] game,int to,byte[] source,int from)
        {
            int end=Array.IndexOf(source,(byte)0,from);
            if(end<0||to+end-from>=game.Length)throw new ArgumentException("Truncated original Bonus string");
            Buffer.BlockCopy(source,from,game,to,end-from+1);
        }
    }
}
