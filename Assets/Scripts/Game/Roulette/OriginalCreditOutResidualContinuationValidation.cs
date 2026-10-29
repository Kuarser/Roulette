using System;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutResidualContinuationValidation
    {
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public Action<byte[]> Commit;
            public int Size{get{return Bytes.Length;}}public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1&&Commit!=null)Commit(Read(20,BitConverter.ToInt32(Bytes,4)));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        private sealed class Fixture:IDisposable
        {
            public readonly OriginalPersistentState State=new OriginalPersistentState();public readonly OriginalHandpaySelectionPersistence Payload;
            public readonly OriginalCreditOutCollect.Runtime Runtime=new OriginalCreditOutCollect.Runtime{Field3AB=7,ResidualEventField1C=0x12345678,ResidualEventField78=0xfedcba98};
            public readonly List<string> Trace=new List<string>();public readonly OriginalCreditOutResidualState.Host Residual;public readonly OriginalCreditOutResidualScreens.OptionHost Option;public readonly OriginalHandpaySelected.Host Selected;
            public OriginalHandpaySelected.Screen Current;public bool ThrowDisplay,ThrowResidualEvent;
            public Fixture(int state,int mode,bool flag,int result,bool disabled,long amount,bool pending,bool mutate)
            {
                var sd=new Device();State.Open(()=>sd,false,s=>{},false,null);State.Assign(26);sd.Commit=b=>Trace.Add("S:"+Convert.ToBase64String(b));
                byte[] b2=new byte[135];for(int i=0;i<135;++i)b2[i]=(byte)(i*19+7);Put(b2,0,11,4);Put(b2,4,995,8);Put(b2,12,-7,8);Put(b2,36,amount,8);Put(b2,72,0,4);Put(b2,99,state,4);b2[93]=127;
                var d=new Device();Payload=new OriginalHandpaySelectionPersistence(d,b2,s=>{});d.Commit=b=>Trace.Add("D:"+Convert.ToBase64String(b));
                Residual=new OriginalCreditOutResidualState.Host{MachineFlag=o=>{Trace.Add("flag");return flag;},MachineInt=o=>{Trace.Add("mode");return mode;},GameFlag370=()=>false,MGSScreenDisplayed=()=>false,CreditAmount0=()=>995,GameOperation=o=>Trace.Add("g:"+o),AssignManagerState=n=>State.Assign(n),MachineOperation=(o,v)=>Trace.Add("m:"+o+":"+v),GameOperation1F0=n=>Trace.Add("g1f0:"+n),DisplayCashoutFailed=n=>Trace.Add("failed:"+n),ArmTrigger=(o,n)=>Trace.Add("arm:"+o+":"+n),Emit=(b,f)=>{if(ThrowResidualEvent)throw new InvalidOperationException("residual event fault");Trace.Add("E:"+Convert.ToBase64String(b));}};
                Func<OriginalHandpaySelected.Screen> make=()=>new OriginalHandpaySelected.Screen{SetLanguage=s=>Trace.Add("lang"),AddText=(b,n)=>Trace.Add("text:"+n),SetYesText=b=>Trace.Add("yes"),SetNoText=b=>Trace.Add("no"),SetAmount=n=>Trace.Add("amount:"+n),SetOrder=n=>Trace.Add("order:"+n),Init=()=>Trace.Add("init"),Delete=()=>Trace.Add("delete"),DisplayResult=()=>{if(ThrowDisplay)throw new InvalidOperationException("display fault");Trace.Add("display:"+result);if(mutate)Payload.CurrentPayload.ResidualSubstateValue=disabled||result!=0?4:9;return result;}};
                if(pending)Current=make();
                Selected=new OriginalHandpaySelected.Host{TiltPending=code=>{Trace.Add("tilt");return true;},Emit=(b,f)=>Trace.Add("tiltevent"),CreateScreen=()=>{Trace.Add("create");return make();},ReadManagerScreen2C4=()=>Current,AssignManagerScreen2C4=s=>{Current=s;Trace.Add("ptr:"+(s==null?0:1));},JurisdictionFlag448=()=>false,MachineFlag188=()=>{Trace.Add("disabled");return disabled;},SelectedLanguage=()=>"en",FormatCredit=(n,x,d,y)=>Trace.Add("format:"+n),Translate=(key,args,n)=>{Trace.Add("tr:"+key+":"+string.Join(",",args));return System.Text.Encoding.ASCII.GetBytes("x y");}};
                Option=new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>{Trace.Add("pending");return Current!=null;},GameOperation=o=>Trace.Add("og:"+o),MachineOperation=(o,v)=>Trace.Add("om:"+o+":"+v),HandPaySelected=()=>OriginalHandpaySelected.Apply(Payload,Selected),ProcessResidualGamble=()=>Payload.ProcessResidualState(Runtime,Residual),ReadManagerEvent18=()=>0x87654321,Emit=(b,f)=>Trace.Add("OE:"+Convert.ToBase64String(b))};
                Residual.DisplayOptionHandpayCancel=()=>OriginalCreditOutResidualScreens.Option(Payload,Option);
            }
            public void Dispose(){Payload.Dispose();State.Dispose();}
        }
        private static void Put(byte[] b,int o,long v,int n){for(int i=0;i<n;++i)b[o+i]=(byte)((ulong)v>>(i*8));}
        public static void Validate()
        {
            int checks=0,cases=0,pendingCases=0;Action<bool> check=b=>{if(!b)throw new Exception("Residual continuation case="+cases+" check="+checks);++checks;};
            foreach(int state in new[]{0,2,4,5,8,9})foreach(int mode in new[]{0,1,3,4,8})foreach(bool flag in new[]{false,true})foreach(int result in new[]{-1,0,1})foreach(bool disabled in new[]{false,true})foreach(long amount in new[]{0L,37L,995L})foreach(bool pending in new[]{false,true})foreach(bool mutate in new[]{false,true})
            {
                using(var sync=new Fixture(state,mode,flag,result,disabled,amount,pending,mutate))using(var async=new Fixture(state,mode,flag,result,disabled,amount,pending,mutate))
                {
                    sync.Payload.ProcessResidualState(sync.Runtime,sync.Residual);
                    var assign=async.Residual.AssignManagerState;var residual=async.Option.ProcessResidualGamble;
                    using(var continuation=OriginalCreditOutResidualContinuation.Begin(async.State,async.Payload,async.Runtime,async.Residual,async.Option,async.Selected))
                    {
                        if(continuation!=null)
                        {
                            ++pendingCases;check(state==2&&mode==3&&!flag&&!pending&&async.State.Value==26&&async.State.ReferenceCount==2&&async.Payload.State.InProgress==1&&async.Payload.State.ResidualSubstate==3&&async.Runtime.Field3AB==7);
                            check(!async.Trace.Exists(s=>s.StartsWith("arm:")||s.StartsWith("disabled")||s=="delete"||s.StartsWith("OE:")));async.Payload.ValidateStorage();async.State.ValidateStorage();
                            int nativeResult=async.Current.DisplayResult();check(continuation.Complete(nativeResult)==(disabled||result!=0));bool repeat=false;try{continuation.Complete(nativeResult);}catch(InvalidOperationException){repeat=true;}check(repeat);
                        }
                    }
                    check(string.Join("|",sync.Trace)==string.Join("|",async.Trace));check(Convert.ToBase64String(sync.Payload.PayloadBytes)==Convert.ToBase64String(async.Payload.PayloadBytes)&&sync.State.Value==async.State.Value&&sync.Runtime.Field3AB==async.Runtime.Field3AB&&async.State.ReferenceCount==1);
                    check(assign==async.Residual.AssignManagerState&&residual==async.Option.ProcessResidualGamble);async.Payload.ValidateStorage();async.State.ValidateStorage();++cases;
                }
            }
            // A display exception unwinds State only; pending financial/NV/UI
            // data is not silently cleared and outer arm318 has not happened.
            using(var f=new Fixture(2,3,false,0,false,37,false,false))
            {using(var continuation=OriginalCreditOutResidualContinuation.Begin(f.State,f.Payload,f.Runtime,f.Residual,f.Option,f.Selected)){f.ThrowDisplay=true;bool fault=false;try{f.Current.DisplayResult();}catch(InvalidOperationException){fault=true;}check(fault&&f.State.ReferenceCount==2&&f.Runtime.Field3AB==7&&f.Payload.State.InProgress==1);}
             check(f.State.ReferenceCount==1&&f.Current!=null&&f.Payload.State.ResidualSubstate==3&&!f.Trace.Exists(s=>s.StartsWith("arm:")));}
            using(var f=new Fixture(2,3,false,0,false,37,false,false))
            {using(var continuation=OriginalCreditOutResidualContinuation.Begin(f.State,f.Payload,f.Runtime,f.Residual,f.Option,f.Selected)){f.ThrowResidualEvent=true;bool fault=false;try{continuation.Complete(0);}catch(InvalidOperationException){fault=true;}check(fault&&f.State.ReferenceCount==1&&f.Runtime.Field3AB==7&&f.Current==null&&!f.Trace.Exists(s=>s.StartsWith("arm:")));}}
            Debug.Log("RESIDUAL_CONTINUATION_VALIDATED cases="+cases+" pendingCases="+pendingCases+" checks="+checks);
        }
    }
}
