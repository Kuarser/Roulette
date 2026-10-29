using System;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCashoutSetupValidation
    {
        [Serializable] private class Cases { public Vector[] cases; }
        [Serializable] private class Vector { public int type;public long amount;public long[] coinValues,limits;public string before,after,trace; }
        private sealed class Device : IOriginalNvDevice
        {
            private byte[] bytes=new byte[0];public int Size {get{return bytes.Length;}}
            public byte[] Read(int offset,int count){var result=new byte[count];Array.Copy(bytes,offset,result,0,count);return result;}
            public void Write(int offset,byte[] data){Array.Copy(data,0,bytes,offset,data.Length);}
            public void SetSize(int size){Array.Resize(ref bytes,size);}
        }
        private static byte[] Bytes(string hex) { var b=new byte[hex.Length/2];for(int i=0;i<b.Length;++i)b[i]=Convert.ToByte(hex.Substring(i*2,2),16);return b; }
        public static int Validate(string path)
        {
            int checks=0;Action<bool> check=v=>{if(!v)throw new Exception("Cashout setup regression "+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==867);
            foreach(var v in vectors.cases)
            {
                var original=Bytes(v.before);var expected=Bytes(v.after);var p=new OriginalCreditOutNvPayload(original);
                int c=0,l=0,s=0;string trace="";
                OriginalCreditOutCashoutSetup.Apply(p,v.type,v.amount,()=>{trace+="c";return v.coinValues[c++];},()=>{trace+="l";return v.limits[l++];},()=>{
                    trace+="s";++s;var atSave=p.Bytes;for(int i=0;i<expected.Length;++i)check(atSave[i]==expected[i]);
                });
                check(trace==v.trace&&s==1);check(p.CashOutType==v.type&&p.CashOutAmount==v.amount);
                var result=p.Bytes;for(int i=0;i<expected.Length;++i)check(result[i]==expected[i]);
                var device=new Device();
                using(var owner=new OriginalHandpaySelectionPersistence(device,original,message=>{}))
                {
                    c=l=0;owner.SetupSelectedCashOut(v.type,v.amount,()=>v.coinValues[c++],()=>v.limits[l++]);owner.ValidateStorage();owner.Reload();
                    result=owner.PayloadBytes;for(int i=0;i<expected.Length;++i)check(result[i]==expected[i]);
                    check(owner.SelectionAmount==p.SelectionAmount);
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                { result=cold.PayloadBytes;for(int i=0;i<expected.Length;++i)check(result[i]==expected[i]); }
            }
            // Invalid division is an explicit managed boundary; no successful NV save.
            var bad=new OriginalCreditOutNvPayload(new byte[135]);int writes=0;bool rejected=false;
            try { OriginalCreditOutCashoutSetup.Apply(bad,11,995,()=>0,()=>0,()=>++writes); }catch(DivideByZeroException){rejected=true;}
            check(rejected&&writes==0);
            Debug.Log("CASHOUT_SETUP_VALIDATED cases="+vectors.cases.Length+" checks="+checks);
            return checks;
        }
    }
}
