using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutFinishStatesValidation
    {
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public readonly List<byte[]> Saves=new List<byte[]>();
            public int Size{get{return Bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1)Saves.Add(Read(20,Bytes.Length==72?26:135));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        private static bool In(int n,params int[] set){foreach(int v in set)if(n==v)return true;return false;}
        private static void Put64(byte[] b,int o,long value){ulong raw=unchecked((ulong)value);for(int i=0;i<8;++i)b[o+i]=(byte)(raw>>(i*8));}
        public static int Validate()
        {
            int checks=0,cases=0;Action<bool> check=b=>{if(!b)throw new Exception("FinishStates regression "+checks);++checks;};
            foreach(int state in Range(256))foreach(byte flag299 in new byte[]{0,7})foreach(byte flag298 in new byte[]{0,7})foreach(int kind in new[]{-1,0,11,25})foreach(long residual in new[]{-1L,0L,1L,long.MinValue,long.MaxValue})
            {
                var sd=new Device();var pd=new Device();var bytes=new byte[135];bytes[0]=unchecked((byte)kind);bytes[1]=bytes[2]=bytes[3]=(byte)(kind<0?255:0);Put64(bytes,4,995);Put64(bytes,36,residual);
                using(var root=new OriginalPersistentState())using(var owner=new OriginalHandpaySelectionPersistence(pd,bytes,message=>{}))
                {
                    root.Open(()=>sd,false,message=>{},false,null);root.Assign(state);sd.Saves.Clear();pd.Saves.Clear();var trace=new List<string>();OriginalPersistentState held=null;
                    var h=new OriginalCreditOutFinishStates.Host{ManagerState=root,PayloadOwner=owner,Runtime=new OriginalCreditOutCollect.Runtime{Field299=flag299,Field298=flag298},
                        StateOperation=(n,copy)=>{check(n==state&&copy.Value==state&&copy.ReferenceCount==3&&copy.Slot==0);held=copy;trace.Add("call:"+n);},
                        DisplayHandPay=amount=>{check(amount==995&&root.ReferenceCount==3);trace.Add("hand");},
                        DisplayTicketReceipt=amount=>{check(amount==(residual>0?residual:995)&&root.ReferenceCount==2);trace.Add("receipt");},
                        ProcessResidualGamble=()=>{check(root.ReferenceCount==2);trace.Add("residual");}};
                    using(var input=root.Copy()){check(OriginalCreditOutFinishStates.Apply(input,h));check(root.ReferenceCount==2);}
                    string expected="";int final=state;
                    if(state==3)expected="hand";else if(state==26)expected="residual";else if(state==27)expected="receipt";
                    else if(In(state,13,14,17)){expected="call:"+state;final=kind!=0?27:0;}
                    else if(In(state,8,9,10,15,16,21,25,28,29,32)){expected="call:"+state;final=0;}
                    else if(state==30||state==31){if(flag299!=0&&flag298==0)expected="call:"+state;final=0;}
                    else if(In(state,1,2,4,5,6,7,11,12,18,19,20,23,24))expected="call:"+state;
                    check(string.Join("|",trace)==expected&&root.Value==final&&root.ReferenceCount==1);
                    check(sd.Saves.Count==(state==final?0:1)&&pd.Saves.Count==0);if(sd.Saves.Count>0)check(sd.Saves[0][0]==1&&sd.Saves[0][1]==final);
                    if(held!=null){bool disposed=false;try{int n=held.Value;}catch(ObjectDisposedException){disposed=true;}check(disposed);}
                    root.ValidateStorage();owner.ValidateStorage();root.Reload();check(root.Value==final);
                }
                using(var cold=new OriginalPersistentState()){cold.Open(()=>sd,false,message=>{},false,null);check(cold.Value==(In(state,13,14,17)?(kind!=0?27:0):In(state,8,9,10,15,16,21,25,28,29,30,31,32)?0:state));}++cases;
            }
            // The post-handler kind is read after its State copy is disposed.
            foreach(int value in new[]{13,14,17})foreach(int kind in new[]{0,11})
            {
                var sd=new Device();var pd=new Device();using(var state=new OriginalPersistentState())using(var payload=new OriginalHandpaySelectionPersistence(pd,new byte[135],message=>{}))
                {
                    state.Open(()=>sd,false,message=>{},false,null);state.Assign(value);OriginalPersistentState held=null;
                    var host=new OriginalCreditOutFinishStates.Host{ManagerState=state,PayloadOwner=payload,Runtime=new OriginalCreditOutCollect.Runtime(),
                        StateOperation=(n,s)=>{held=s;payload.CurrentPayload.CashOutType=kind;payload.SaveCurrentPayload();},DisplayHandPay=x=>{throw new Exception("Unexpected handpay");},DisplayTicketReceipt=x=>{throw new Exception("Unexpected receipt");},ProcessResidualGamble=()=>{throw new Exception("Unexpected residual");}};
                    check(OriginalCreditOutFinishStates.Apply(state,host));check(state.Value==(kind==0?0:27));bool disposed=false;try{int n=held.Value;}catch(ObjectDisposedException){disposed=true;}check(disposed);
                    state.ValidateStorage();payload.ValidateStorage();
                }
            }
            // Handler exception must release its argument copy before propagating;
            // no automatic State0 assignment may occur after the failed body.
            var faultStateDevice=new Device();var faultPayloadDevice=new Device();using(var state=new OriginalPersistentState())using(var payload=new OriginalHandpaySelectionPersistence(faultPayloadDevice,new byte[135],message=>{}))
            {
                state.Open(()=>faultStateDevice,false,message=>{},false,null);state.Assign(9);faultStateDevice.Saves.Clear();var host=new OriginalCreditOutFinishStates.Host{ManagerState=state,PayloadOwner=payload,Runtime=new OriginalCreditOutCollect.Runtime(),StateOperation=(n,s)=>{throw new InvalidOperationException("handler fault");},DisplayHandPay=x=>{},DisplayTicketReceipt=x=>{},ProcessResidualGamble=()=>{}};
                bool failed=false;try{OriginalCreditOutFinishStates.Apply(state,host);}catch(InvalidOperationException e){failed=e.Message=="handler fault";}check(failed&&state.Value==9&&state.ReferenceCount==1&&faultStateDevice.Saves.Count==0);
            }
            UnityEngine.Debug.Log("FINISH_STATES_VALIDATED cases="+cases+" checks="+checks);return checks;
        }
        private static IEnumerable<int> Range(int count){for(int i=0;i<count;++i)yield return i;}
    }
}
