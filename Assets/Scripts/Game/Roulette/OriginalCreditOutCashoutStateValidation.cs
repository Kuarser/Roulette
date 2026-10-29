using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCashoutStateValidation
    {
        [Serializable] private class Cases{public Vector[] cases;}
        [Serializable] private class Vector{public int kind,sub,game,subAfter,flagAfter;public bool external;public string before,after,trace;public string[] saves,events;}
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
            int checks=0,index=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOutState regression vector="+index+" check="+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==19200);
            foreach(var v in vectors.cases)
            {
                var trace=new List<string>();var saves=new List<byte[]>();var events=new List<byte[]>();int sub=v.sub,manager=4;
                var runtime=new OriginalCreditOutCollect.Runtime{ManagerEventField=0x89abcdef,Field3AB=7};var device=new Device();
                using(var owner=new OriginalHandpaySelectionPersistence(device,Bytes(v.before),message=>{}))
                {
                    var reference=owner.State;device.Commit=b=>{trace.Add("save");saves.Add(b);};
                    var host=new OriginalCreditOutCashoutState.Host{ReadSubState=()=>{trace.Add("readsub");return sub;},AssignSubState=s=>{sub=s;trace.Add("sub:"+s);},
                        ReadGameFieldE0=()=>{trace.Add("gamee0");return v.game;},ExternalFlag80=()=>{trace.Add("external80");return v.external;},
                        GameOperation4C=(a,b)=>{check(a==0&&b);trace.Add("game4c:0:1");},GameOperation64=a=>{check(a==0);trace.Add("game64:0");},
                        Emit=(b,f)=>{check(f==0);trace.Add("event");events.Add((byte[])b.Clone());},AssignManagerState=s=>{manager=s;trace.Add("manager:"+s);},
                        CashOutVoucherState=()=>{check(manager==5);trace.Add("voucherstate");}};
                    check(owner.CashOutState(runtime,host));check(string.Join("|",trace)==v.trace);check(sub==v.subAfter&&runtime.Field3AB==v.flagAfter);
                    check(saves.Count==v.saves.Length&&events.Count==v.events.Length);for(int i=0;i<saves.Count;++i)check(Equal(saves[i],Bytes(v.saves[i])));
                    for(int i=0;i<events.Count;++i)check(Equal(events[i],Bytes(v.events[i])));check(Equal(owner.PayloadBytes,Bytes(v.after)));
                    check(ReferenceEquals(reference,owner.State)&&owner.State.ResidualSubstate==owner.CurrentPayload.ResidualSubstateValue);
                    owner.ValidateStorage();owner.Reload();check(Equal(owner.PayloadBytes,Bytes(v.after)));device.Commit=null;
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                {cold.ValidateStorage();check(Equal(cold.PayloadBytes,Bytes(v.after)));}++index;
            }
            Debug.Log("CASHOUT_STATE_VALIDATED cases="+vectors.cases.Length+" checks="+checks);return checks;
        }
    }
}
