using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutResidualStateValidation
    {
        [Serializable] public sealed class Vector {public string initial,payload,durable;public int state,field3ab,expectedField3ab,manager;public int[] modes,flags;public bool game,mgs,mutation;public string[] trace;}
        [Serializable] public sealed class Vectors {public Vector[] cases;}
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public Action<byte[]> Commit;
            public int Size{get{return Bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1&&Commit!=null)Commit(Read(20,BitConverter.ToInt32(Bytes,4)));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        public static int Validate(string vectorsPath,string directory)
        {
            var vectors=JsonUtility.FromJson<Vectors>(File.ReadAllText(vectorsPath));int checks=0,index=0;
            Action<bool> check=b=>{if(!b)throw new Exception("Residual regression case="+index+" check="+checks);++checks;};
            foreach(var v in vectors.cases)
            {
                var device=new Device();var trace=new List<string>();var runtime=new OriginalCreditOutCollect.Runtime{ResidualEventField1C=0x87654321,ResidualEventField78=0xfedcba98,Field3AB=(byte)v.field3ab};
                using(var owner=new OriginalHandpaySelectionPersistence(device,Convert.FromBase64String(v.initial),s=>{}))
                {
                    var selectionReference=owner.State;int mi=0,fi=0,manager=26;device.Commit=b=>trace.Add("save:"+Convert.ToBase64String(b));
                    var h=new OriginalCreditOutResidualState.Host{
                        MachineFlag=o=>{check(o==0x188);int n=v.flags[Math.Min(fi++,v.flags.Length-1)];trace.Add("flag:"+n);return n!=0;},
                        MachineInt=o=>{check(o==0x204);int n=v.modes[Math.Min(mi++,v.modes.Length-1)];trace.Add("mode:"+n);return n;},
                        GameFlag370=()=>{trace.Add("game370:"+(v.game?1:0));return v.game;},MGSScreenDisplayed=()=>{trace.Add("mgs:"+(v.mgs?1:0));return v.mgs;},
                        CreditAmount0=()=>{trace.Add("credit");return long.MinValue+1;},GameOperation=o=>trace.Add("game:"+o),AssignManagerState=n=>{manager=n;trace.Add("manager:"+n);},
                        MachineOperation=(o,b)=>trace.Add("machine:"+o+":"+(b?1:0)),GameOperation1F0=n=>{trace.Add("game1f0:"+n);if(v.mutation)owner.CurrentPayload.ResidualSubstateValue=3;},
                        DisplayCashoutFailed=n=>trace.Add("failed:"+n),DisplayOptionHandpayCancel=()=>trace.Add("option"),ArmTrigger=(o,n)=>trace.Add("arm:"+o+":"+n),
                        Emit=(b,f)=>{check(f==0);trace.Add("event:"+Convert.ToBase64String(b));}};
                    check(!owner.ProcessResidualState(runtime,h));
                    if(string.Join("|",trace)!=string.Join("|",v.trace))throw new Exception("Residual trace case="+index+" actual="+string.Join("|",trace)+" expected="+string.Join("|",v.trace));++checks;
                    check(Convert.ToBase64String(owner.PayloadBytes)==v.payload);check(manager==v.manager&&runtime.Field3AB==v.expectedField3ab);
                    if(v.payload==v.durable)owner.ValidateStorage();else{bool fault=false;try{owner.ValidateStorage();}catch(OriginalNvFault){fault=true;}check(fault);}
                    owner.Reload();owner.ValidateStorage();check(Convert.ToBase64String(owner.PayloadBytes)==v.durable&&ReferenceEquals(selectionReference,owner.State));
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],s=>{})){check(Convert.ToBase64String(cold.PayloadBytes)==v.durable);cold.ValidateStorage();}++index;
            }
            // Actual two-copy file transport and cold reopen, not just memory callbacks.
            if(Directory.Exists(directory))throw new Exception("Fresh residual file directory required");Directory.CreateDirectory(directory);
            foreach(int state in new[]{4,5,9})
            {
                string path=Path.Combine(directory,"residual-"+state+".bin");byte[] expected;
                var v=Array.Find(vectors.cases,x=>x.state==state);
                using(var owner=OriginalHandpaySelectionPersistence.OpenFile(path,Convert.FromBase64String(v.initial),s=>{}))
                {var h=DiagnosticHost();owner.ProcessResidualState(new OriginalCreditOutCollect.Runtime(),h);owner.ValidateStorage();expected=owner.PayloadBytes;}
                using(var cold=OriginalHandpaySelectionPersistence.OpenFile(path,new byte[135],s=>{})){check(Convert.ToBase64String(cold.PayloadBytes)==Convert.ToBase64String(expected));cold.ValidateStorage();}
            }
            // Event failure propagates before explicit second Save/manager assignment/outer arm.
            {
                var d=new Device();var v=Array.Find(vectors.cases,x=>x.state==4);int saves=0,arms=0,manager=26;
                using(var owner=new OriginalHandpaySelectionPersistence(d,Convert.FromBase64String(v.initial),s=>{}))
                {d.Commit=b=>++saves;var h=DiagnosticHost();h.Emit=(b,f)=>{throw new InvalidOperationException("injected event failure");};h.ArmTrigger=(o,n)=>++arms;h.AssignManagerState=n=>manager=n;bool failed=false;
                 try{owner.ProcessResidualState(new OriginalCreditOutCollect.Runtime(),h);}catch(InvalidOperationException){failed=true;}
                 check(failed&&saves==1&&arms==0&&manager==26&&owner.CurrentPayload.CashOutType==15);owner.Reload();check(owner.CurrentPayload.CashOutType==15);}
            }
            checks+=OriginalCreditOutStateFlowValidation.ValidateResidualBinding(DiagnosticHost());
            Debug.Log("RESIDUAL_STATE_VALIDATED cases="+index+" checks="+checks);return checks;
        }
        // Diagnostic operations only. Production requires genuine host owners.
        private static OriginalCreditOutResidualState.Host DiagnosticHost()
        {return new OriginalCreditOutResidualState.Host{MachineFlag=o=>false,MachineInt=o=>0,GameFlag370=()=>false,MGSScreenDisplayed=()=>false,CreditAmount0=()=>37,GameOperation=o=>{},AssignManagerState=n=>{},MachineOperation=(o,b)=>{},GameOperation1F0=n=>{},DisplayCashoutFailed=n=>{},DisplayOptionHandpayCancel=()=>{},ArmTrigger=(o,n)=>{},Emit=(b,f)=>{}};}
    }
}
