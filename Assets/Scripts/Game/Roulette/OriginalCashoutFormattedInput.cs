using System;
using System.IO;
namespace RouletteDeluxe
{
    // Original libstdc++ 6.0.16 formatted short extraction, default decimal
    // flags and classic C locale. This is byte input: no BOM decoding/removal.
    public sealed class OriginalCashoutFormattedInput:IDisposable
    {
        public const int Bad=1,Eof=2,Fail=4;
        readonly Stream stream;int lookahead=-2;bool disposed;
        public int State {get;private set;}
        public long Consumed {get;private set;}
        public bool Closed {get{return disposed;}}
        public OriginalCashoutFormattedInput(Stream input){stream=input??throw new ArgumentNullException(nameof(input));}
        OriginalCashoutFormattedInput(){State=Fail;}
        int Peek(){if(lookahead==-2)lookahead=stream.ReadByte();return lookahead;}
        void Take(){lookahead=-2;++Consumed;}
        static bool Space(int c){return c==32||c>=9&&c<=13;}
        public void Extract(ref short value)
        {
            if(disposed)throw new ObjectDisposedException(nameof(OriginalCashoutFormattedInput));
            if(State!=0){State|=Fail;return;}
            bool sentry=false;
            try
            {
                int c=Peek();while(Space(c)){Take();c=Peek();}
                if(c<0){State|=Eof|Fail;return;}sentry=true;
                bool negative=c=='-';if(negative||c=='+'){Take();c=Peek();}
                uint limit=negative?0x80000000u:0x7fffffffu,magnitude=0;bool any=false,overflow=false;
                while(c>='0'&&c<='9')
                {
                    uint digit=(uint)(c-'0');any=true;
                    if(magnitude>(limit-digit)/10)overflow=true;
                    else if(!overflow)magnitude=magnitude*10+digit;
                    Take();c=Peek();
                }
                int error=c<0?Eof:0;int result;
                if(!any){result=0;error|=Fail;}
                else if(overflow){result=negative?int.MinValue:int.MaxValue;error|=Fail;}
                else result=negative?unchecked(-(int)magnitude):(int)magnitude;
                if(result<short.MinValue){value=short.MinValue;error|=Fail;}
                else if(result>short.MaxValue){value=short.MaxValue;error|=Fail;}
                else value=(short)result;
                State|=error;
            }
            catch(IOException){State|=Bad;if(!sentry)State|=Fail;}
        }
        public void Dispose()
        {
            if(disposed)return;disposed=true;
            // Native directly closes filebuf and ignores its return. No second
            // state check changes the rectangle after the caller's check.
            if(stream!=null)try{stream.Dispose();}catch(IOException){}
        }
        public OriginalCashoutConfiguration.Input Bind()
        {return new OriginalCashoutConfiguration.Input {Extract=Extract,State=()=>State,CloseAndDestroy=Dispose};}
        public static OriginalCashoutFormattedInput OpenLocal(string path,int mode)
        {
            if(mode!=8)throw new ArgumentException("CashOut native input mode8 required");
            int nul=path.IndexOf('\0');if(nul>=0)path=path.Substring(0,nul);
            try{return new OriginalCashoutFormattedInput(new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));}
            catch(IOException){return new OriginalCashoutFormattedInput();}
            catch(UnauthorizedAccessException){return new OriginalCashoutFormattedInput();}
            catch(ArgumentException){return new OriginalCashoutFormattedInput();}
        }
        public static OriginalCashoutConfiguration.Host LocalHost(Func<string,string> resolve=null)
        {return new OriginalCashoutConfiguration.Host {Open=(path,mode)=>OpenLocal(resolve==null?path:resolve(path),mode).Bind()};}
    }
}
