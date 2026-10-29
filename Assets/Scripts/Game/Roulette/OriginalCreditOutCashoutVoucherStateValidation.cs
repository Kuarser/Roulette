using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCashoutVoucherStateValidation
    {
        [Serializable] private class Cases{public Vector[] cases;}
        [Serializable] private class Vector{public int kind,result,flagAfter;public bool machine,juris,control;public long coin,limit,maximum;public string before,after,trace;public string[] saves,events;}
        private sealed class Device:IOriginalNvDevice
        {
            private byte[] bytes=new byte[0];public Action<byte[]> Commit;
            public int Size{get{return bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,bytes,o,b.Length);if(Commit!=null&&o==0&&b.Length==1&&b[0]==1)Commit(Read(20,135));}
            public void SetSize(int n){Array.Resize(ref bytes,n);}
        }
        private static byte[] Bytes(string hex){var b=new byte[hex.Length/2];for(int i=0;i<b.Length;++i)b[i]=Convert.ToByte(hex.Substring(i*2,2),16);return b;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public static int Validate(string path)
        {
            int checks=0,index=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOutVoucherState regression vector="+index+" check="+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==7680);
            foreach(var v in vectors.cases)
            {
                var trace=new List<string>();var saves=new List<byte[]>();var events=new List<byte[]>();int manager=5;
                var runtime=new OriginalCreditOutCollect.Runtime{Field29A=7};var device=new Device();
                using(var owner=new OriginalHandpaySelectionPersistence(device,Bytes(v.before),message=>{}))
                {
                    var reference=owner.State;device.Commit=b=>{trace.Add("save");saves.Add(b);};
                    var host=new OriginalCreditOutCashoutVoucherState.Host{
                        PayTicket=(k,amount)=>{trace.Add("pay:"+k+":"+amount);return v.result;},
                        MachineFlag=o=>{trace.Add("mf:"+o);return v.machine;},JurisdictionFlag=o=>{trace.Add("jf:"+o);return v.juris;},
                        ControlFlag=o=>{trace.Add("cf:"+o);return v.control;},MachineAmount=o=>{trace.Add("ma:"+o);if(o==0xb4)return v.coin;if(o==0x148)return v.limit;if(o==0x10c)return v.maximum;throw new Exception("Unexpected machine read");},
                        GameOperation64=amount=>{check(amount==0);trace.Add("game64:0");},CreditInOperation34=()=>trace.Add("creditin34"),
                        Emit=(bytes,f)=>{check(f==0);trace.Add("event");events.Add((byte[])bytes.Clone());},AssignManagerState=value=>{manager=value;trace.Add("manager:"+value);},
                        CashOutContinueState=()=>{check(manager==6);trace.Add("continuestate");}};
                    check(owner.CashOutVoucherState(runtime,host));check(string.Join("|",trace)==v.trace);check(runtime.Field29A==v.flagAfter);
                    check(saves.Count==v.saves.Length&&events.Count==v.events.Length);for(int i=0;i<saves.Count;++i)check(Equal(saves[i],Bytes(v.saves[i])));
                    for(int i=0;i<events.Count;++i)check(Equal(events[i],Bytes(v.events[i])));check(Equal(owner.PayloadBytes,Bytes(v.after)));
                    check(ReferenceEquals(reference,owner.State)&&owner.State.ResidualSubstate==owner.CurrentPayload.ResidualSubstateValue);
                    owner.ValidateStorage();owner.Reload();check(Equal(owner.PayloadBytes,Bytes(v.after)));device.Commit=null;
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                {cold.ValidateStorage();check(Equal(cold.PayloadBytes,Bytes(v.after)));}++index;
            }
            Debug.Log("CASHOUT_VOUCHER_STATE_VALIDATED cases="+vectors.cases.Length+" checks="+checks);return checks;
        }
    }
}
