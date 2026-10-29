using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public static class OriginalNv2SoftValidation
    {
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public readonly List<string> Trace=new List<string>();
            public int ExtraSize;public int Size{get{Trace.Add("size");return Bytes.Length+ExtraSize;}}
            public byte[] Read(int o,int n){Trace.Add("read");var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Trace.Add("write");Array.Copy(b,0,Bytes,o,b.Length);}
            public void SetSize(int n){Trace.Add("setsize");Array.Resize(ref Bytes,n);}
        }
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("NV2 SoftValidate regression "+checks);++checks;};
            foreach(int length in new[]{0,1,2,7,20,44,135,256})for(int seed=0;seed<8;++seed)
            {
                var current=new byte[length];for(int i=0;i<length;++i)current[i]=(byte)(i*19+seed);var d=new Device();
                var store=new OriginalNv2Storage(d,()=>{d.Trace.Add("serialize");return current;},b=>current=b,message=>{});store.Reload();d.Trace.Clear();
                Action<string> log=context=>{check(context=="CreditOutMgr.cpp::CashOutContinueState(6309)");d.Trace.Add("log");};
                string context="CreditOutMgr.cpp::CashOutContinueState(6309)";
                byte[] image=(byte[])d.Bytes.Clone();store.SoftValidate(false,context,null);check(d.Trace.Count==0);
                store.SoftValidate(true,context,log);check(string.Join("|",d.Trace)=="log|size|serialize");check(Equal(image,d.Bytes));d.Trace.Clear();
                d.ExtraSize=1;ExpectFault(check,d,store,context,log,"File size corrupted","log|size");d.ExtraSize=0;
                var original=current;current=new byte[length+1];ExpectFault(check,d,store,context,log,"File Headers size corrupted","log|size|serialize");current=original;
                if(length>0)
                {
                    current[0]^=1;ExpectFault(check,d,store,context,log,"Data1 checksum failed","log|size|serialize");current[0]^=1;
                    // SoftValidate deliberately does not read disk payload or header.
                    d.Bytes[20]^=1;store.SoftValidate(true,context,log);check(string.Join("|",d.Trace)=="log|size|serialize");d.Trace.Clear();
                    bool hardFailed=false;try{store.Validate();}catch(OriginalNvFault){hardFailed=true;}check(hardFailed);d.Bytes[20]^=1;d.Trace.Clear();
                }
                d.Bytes[1]^=1;store.SoftValidate(true,context,log);check(string.Join("|",d.Trace)=="log|size|serialize");d.Bytes[1]^=1;d.Trace.Clear();
                store.Save1();d.Trace.Clear();ExpectFault(check,d,store,context,log,"File state corrupted","log|size");store.Save2();d.Trace.Clear();
                bool injected=false;try{store.SoftValidate(true,context,text=>{d.Trace.Add("logfail");throw new InvalidOperationException("context fault");});}catch(InvalidOperationException){injected=true;}
                check(injected&&string.Join("|",d.Trace)=="logfail");d.Trace.Clear();
                store.SoftValidate(false,context,text=>{throw new Exception("disabled callback");});check(d.Trace.Count==0);store.Validate();
            }
            var ownerDevice=new Device();using(var owner=new OriginalHandpaySelectionPersistence(ownerDevice,new byte[135],message=>{}))
            {var reference=owner.State;ownerDevice.Trace.Clear();owner.SoftValidateStorage(true,"owner",s=>{check(s=="owner");ownerDevice.Trace.Add("log");});
             check(string.Join("|",ownerDevice.Trace)=="log|size"&&ReferenceEquals(reference,owner.State));owner.ValidateStorage();}
            return checks;
        }
        private static void ExpectFault(Action<bool> check,Device d,OriginalNv2Storage store,string context,Action<string> log,string suffix,string expected)
        {
            bool failed=false;try{store.SoftValidate(true,context,log);}catch(OriginalNvFault e){failed=true;check(e.NativeCode==2&&e.Message=="NV2::SoftValidate: "+suffix);}
            check(failed&&string.Join("|",d.Trace)==expected);d.Trace.Clear();
        }
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
    }
}
