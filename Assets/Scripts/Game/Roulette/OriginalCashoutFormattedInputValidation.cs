using System;
using System.IO;
using System.Text;
using System.Globalization;
namespace RouletteDeluxe
{
    public static class OriginalCashoutFormattedInputValidation
    {
        sealed class FaultStream:Stream
        {
            readonly int after;int reads;
            public FaultStream(int successfulReads){after=successfulReads;}
            public override int ReadByte(){if(reads++>=after)throw new IOException("Injected byte read failure");return '1';}
            public override bool CanRead=>true;public override bool CanSeek=>false;public override bool CanWrite=>false;
            public override long Length=>0;public override long Position{get=>0;set=>throw new NotSupportedException();}
            public override int Read(byte[] b,int o,int c){throw new NotSupportedException();}public override long Seek(long o,SeekOrigin s){throw new NotSupportedException();}
            public override void Flush(){}public override void SetLength(long n){throw new NotSupportedException();}public override void Write(byte[] b,int o,int c){throw new NotSupportedException();}
        }
        public static int Validate(string root)
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut formatted input check "+checks);++checks;};
            if(Directory.Exists(root))throw new Exception("Fresh fixture directory required");Directory.CreateDirectory(root);
            for(int n=short.MinValue;n<=short.MaxValue;++n)for(int variant=0;variant<3;++variant)
            {
                string text=(variant==2?"\t\r\n\v\f ":"")+(n>=0?"+":"-")+"000"+Math.Abs(n).ToString(CultureInfo.InvariantCulture)+(variant==0?"":variant==1?" ":"!");
                using(var input=new OriginalCashoutFormattedInput(new MemoryStream(Encoding.ASCII.GetBytes(text))))
                {
                    short value=123;input.Extract(ref value);check(value==n);check(input.State==(variant==0?2:0));
                    check(input.Consumed==text.Length-(variant==0?0:1));short next=77;input.Extract(ref next);
                    check(input.State==(variant==2?4:6));check(next==(variant==2?0:77));
                }
            }
            string[] tokens={"32768 ","-32769 ","2147483648 ","-2147483649 ","999999999999999999999999999999999999 ","-999999999999999999999999999999999999 ","+","-","++1","-+1","a","\0","0x11 ","0123 ","1.5 ","\u00a01 ","-0 "};
            short[] values={32767,-32768,32767,-32768,32767,-32768,0,0,0,0,0,0,0,123,1,0,0};
            int[] states={4,4,4,4,4,4,6,6,4,4,4,4,0,0,0,4,0};
            for(int i=0;i<tokens.Length;++i)using(var input=new OriginalCashoutFormattedInput(new MemoryStream(Encoding.UTF8.GetBytes(tokens[i]))))
            {short value=99;input.Extract(ref value);check(value==values[i]&&input.State==states[i]);}
            for(int i=0;i<2;++i)using(var input=new OriginalCashoutFormattedInput(new FaultStream(i)))
            {short value=88;input.Extract(ref value);check(value==88&&input.State==(i==0?5:1));}
            string[] configs={"1 2 3 4","1 2 3 4\n"," \t1\r2\n3\v4\f","1 2 3 4junk","1 2 3 4\0","1 2 3",""," ","1 2 3 32768 ","1 2 3 -32769 ","-32768 0 0 32767 ","\ufeff1 2 3 4\n","1 2 3 4.25","1 2 3 0x9","1 2 3 +4 "};
            bool[] valid={false,true,true,true,true,false,false,false,false,false,true,false,true,true,true};int fileCases=0;
            for(int i=0;i<configs.Length;++i)
            {
                string path=Path.Combine(root,"config_"+i+".cfg");File.WriteAllBytes(path,Encoding.UTF8.GetBytes(configs[i]));
                var owner=new OriginalCashoutDisplay.State {Left=8,Top=9,Right=10,Bottom=11};var table=new OriginalCashoutTextBoundsTable();
                OriginalCashoutFormattedInput actual=null;
                OriginalCashoutConfiguration.Read(owner,table,path+"\0ignored",new OriginalCashoutConfiguration.Host {Open=(p,m)=>{actual=OriginalCashoutFormattedInput.OpenLocal(p,m);return actual.Bind();}});
                ++fileCases;check(actual.Closed&&table.Entries.Count==0);
                if(!valid[i])check(owner.Left==0&&owner.Top==0&&owner.Right==0&&owner.Bottom==0);
                else check(owner.Left==(i==10?-32768:1)&&owner.Top==(i==10?0:2)&&owner.Right==(i==10?0:3)&&owner.Bottom==(i==10?32767:i==13?0:4));
            }
            var missing=new OriginalCashoutDisplay.State {Left=99,Top=98,Right=97,Bottom=96};var shared=new OriginalCashoutTextBoundsTable();
            OriginalCashoutConfiguration.Read(missing,shared,Path.Combine(root,"absent.cfg"),OriginalCashoutFormattedInput.LocalHost());
            check(missing.Left==0&&missing.Top==0&&missing.Right==0&&missing.Bottom==0&&shared.Entries.Count==0);
            var empty=new OriginalCashoutDisplay.State();OriginalCashoutConfiguration.Read(empty,shared,"",OriginalCashoutFormattedInput.LocalHost());check(shared.Entries.Count==4);
            // Default C-byte parsing is independent of the CLR thread culture.
            var old=CultureInfo.CurrentCulture;
            try{CultureInfo.CurrentCulture=new CultureInfo("ar-SA");using(var input=new OriginalCashoutFormattedInput(new MemoryStream(Encoding.ASCII.GetBytes("+123 ")))){short value=0;input.Extract(ref value);check(value==123&&input.State==0);}}
            finally{CultureInfo.CurrentCulture=old;}
            check(fileCases==15);return checks;
        }
    }
}
