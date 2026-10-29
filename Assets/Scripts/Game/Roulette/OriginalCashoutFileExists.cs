using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    // FileExists(std::string const&),086c4e00. It opens the exact pathname.
    public static class OriginalCashoutFileExists
    {
        public sealed class Host
        {
            public Func<string,uint,bool> Open;
            public Func<uint> CurrentState;
            public Action<uint> ClearState;
            public Func<bool> IsOpen, Close;
            public Action Release;
        }
        public static bool Probe(string path,Host host)
        {
            if(path==null||host==null)throw new ArgumentNullException();
            // C++ filebuf::open consumes c_str(), even if std::string has embedded NUL.
            int nul=path.IndexOf('\0');if(nul>=0)path=path.Substring(0,nul);
            bool result=false;
            try
            {
                if(host.Open(path,8))host.ClearState(0);else host.ClearState(host.CurrentState()|4);
                result=host.IsOpen();
                if(result&&!host.Close())host.ClearState(host.CurrentState()|4);
                return result;
            }
            finally {try{host.Close();}finally{host.Release();}}
        }
        // Regular files in an explicit local mapping; no extension aliases/mount inference.
        public static bool ProbeLocalRegularFile(string path)
        {
            FileStream file=null;uint state=0;
            return Probe(path,new Host {
                Open=(name,mode)=>{try{file=File.Open(name,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);return true;}
                    catch(IOException){return false;}catch(UnauthorizedAccessException){return false;}},
                CurrentState=()=>state,ClearState=value=>state=value,IsOpen=()=>file!=null,
                Close=()=>{if(file==null)return false;file.Dispose();file=null;return true;},Release=()=>{}
            });
        }
    }
    public static class OriginalCashoutFileExistsValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut FileExists regression "+checks);++checks;};
            foreach(uint initial in new uint[]{0,2,4,8})for(int flags=0;flags<8;++flags)
            {
                bool open=(flags&1)!=0,isOpen=(flags&2)!=0,close=(flags&4)!=0;uint state=initial;var trace=new List<string>();
                var expected=new List<string>{"open","clear:"+(open?0:initial|4),"isOpen"};
                if(isOpen){expected.Add("close");if(!close)expected.Add("clear:"+((open?0:initial|4)|4));}
                expected.Add("close");expected.Add("release");
                bool result=OriginalCashoutFileExists.Probe("literal.x\0ignored.bik",new OriginalCashoutFileExists.Host {
                    Open=(name,mode)=>{check(name=="literal.x"&&mode==8);trace.Add("open");return open;},
                    CurrentState=()=>state,ClearState=value=>{state=value;trace.Add("clear:"+value);},IsOpen=()=>{trace.Add("isOpen");return isOpen;},
                    Close=()=>{trace.Add("close");return close;},Release=()=>trace.Add("release")});
                check(result==isOpen);check(string.Join(",",trace)==string.Join(",",expected));
            }
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/native_handpay_layout_v1/file_probe_samples_v1"));Directory.CreateDirectory(root);
            string empty=Path.Combine(root,"empty.x");if(!File.Exists(empty))File.WriteAllBytes(empty,new byte[0]);
            check(new FileInfo(empty).Length==0&&OriginalCashoutFileExists.ProbeLocalRegularFile(empty));
            string fnt=Path.GetFullPath(Path.Combine(Application.dataPath,"../../bal/BAL_OS366J/agk/fonts/CashOutFont-Amt-1360x768.fnt"));
            check(OriginalCashoutFileExists.ProbeLocalRegularFile(fnt));check(!OriginalCashoutFileExists.ProbeLocalRegularFile(Path.ChangeExtension(fnt,".x")));
            check(OriginalCashoutFileExists.ProbeLocalRegularFile(fnt+"\0ignored"));return checks;
        }
    }
}
