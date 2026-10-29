using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectedValidation
    {
        [Serializable] public sealed class Vector{public string initial,payload,prepared;public string[] trace;public int result;public bool selected,disabled,first,last,jur,mutation;}
        [Serializable] public sealed class Vectors{public Vector[] cases;}
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public Action<byte[]> Commit;
            public int Size{get{return Bytes.Length;}}public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1&&Commit!=null)Commit(Read(20,BitConverter.ToInt32(Bytes,4)));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        public static int Validate(string root)
        {
            var vectors=JsonUtility.FromJson<Vectors>(File.ReadAllText(Path.Combine(root,"body_vectors_v1.json")));int checks=0,index=0;
            Action<bool> check=b=>{if(!b)throw new Exception("HandPaySelected case="+index+" check="+checks);++checks;};
            foreach(var v in vectors.cases)
            {
                var d=new Device();var t=new List<string>();
                using(var owner=new OriginalHandpaySelectionPersistence(d,Convert.FromBase64String(v.initial),s=>{}))
                {
                    d.Commit=b=>t.Add("save:"+Convert.ToBase64String(b));OriginalHandpaySelected.Screen current=null;int tilts=0;
                    var screen=new OriginalHandpaySelected.Screen{SetLanguage=s=>t.Add("lang:"+s),AddText=(b,n)=>t.Add("text:"+Convert.ToBase64String(b)+":"+n),SetYesText=b=>t.Add("yes:"+Convert.ToBase64String(b)),SetNoText=b=>t.Add("no:"+Convert.ToBase64String(b)),SetAmount=n=>t.Add("amount:"+n),SetOrder=n=>t.Add("order:"+n),Init=()=>t.Add("init"),Delete=()=>t.Add("delete"),DisplayResult=()=>{t.Add("display:"+v.result);if(v.mutation)owner.CurrentPayload.SelectionAmount=111;return v.result;}};
                    var h=new OriginalHandpaySelected.Host{
                        TiltPending=code=>{check(code==0x6020);bool b=tilts++==0?v.first:v.last;t.Add("tilt:"+(b?1:0));return b;},Emit=(b,f)=>{check(f==0);t.Add("event:"+Convert.ToBase64String(b));},CreateScreen=()=>{t.Add("create");return screen;},ReadManagerScreen2C4=()=>current,AssignManagerScreen2C4=s=>{current=s;t.Add("assign:"+(s==null?0:1));},
                        JurisdictionFlag448=()=>{t.Add("jur:"+(v.jur?1:0));return v.jur;},MachineFlag188=()=>{t.Add("disabled:"+(v.disabled?1:0));return v.disabled;},SelectedLanguage=()=>{t.Add("language");return "zh";},
                        FormatCredit=(n,b,d2,c)=>{t.Add("format:"+n+":"+(b?1:0)+":"+d2+":"+(c?1:0));if(v.mutation)owner.CurrentPayload.SelectionAmount=-11;},
                        Translate=(key,args,flag)=>{t.Add("translate:"+key+":"+string.Join(",",args)+":"+(flag?1:0));if(key=="cannot_be_paid_from_the_hopper"){if(v.mutation)owner.CurrentPayload.SelectionAmount=-13;return Encoding.ASCII.GetBytes("Amount");}return Encoding.ASCII.GetBytes(key=="Please_touch_an_option"?"Option":key=="Hand_Pay"?(v.mutation?"Hand\0 Pay":"Hand Pay"):"Continue Game Play");}};
                    bool selected;
                    if(index%2==0)selected=OriginalHandpaySelected.Apply(owner,h);
                    else
                    {
                        var session=OriginalHandpaySelected.Begin(owner,h);check(Convert.ToBase64String(owner.PayloadBytes)==v.prepared&&ReferenceEquals(current,screen));owner.ValidateStorage();
                        selected=session.Complete(current.DisplayResult());bool repeated=false;try{session.Complete(0);}catch(InvalidOperationException){repeated=true;}check(repeated);
                    }
                    check(selected==v.selected&&current==null);if(string.Join("|",t)!=string.Join("|",v.trace))throw new Exception("HandPaySelected trace case="+index+" actual="+string.Join("|",t)+" expected="+string.Join("|",v.trace));++checks;
                    check(Convert.ToBase64String(owner.PayloadBytes)==v.payload&&owner.State.HandpaySelectedByte==127&&owner.State.InProgress==0&&owner.State.ResidualSubstate==3);owner.ValidateStorage();var reference=owner.State;owner.Reload();check(ReferenceEquals(reference,owner.State)&&Convert.ToBase64String(owner.PayloadBytes)==v.payload);
                }
                using(var cold=new OriginalHandpaySelectionPersistence(d,new byte[135],s=>{})){check(Convert.ToBase64String(cold.PayloadBytes)==v.payload);cold.ValidateStorage();}++index;
            }
            // A genuine display fault leaves the prepared screen and durable
            // InProgress1/residual3; no invented deletion/tilt cleanup/selection.
            {
                var d=new Device();using(var owner=new OriginalHandpaySelectionPersistence(d,Convert.FromBase64String(vectors.cases[0].initial),s=>{}))
                {int delete=0,disabled=0;OriginalHandpaySelected.Screen current=null;var h=DiagnosticHost(s=>current=s,()=>current);h.CreateScreen=()=>new OriginalHandpaySelected.Screen{SetLanguage=s=>{},AddText=(b,n)=>{},SetYesText=b=>{},SetNoText=b=>{},SetAmount=n=>{},SetOrder=n=>{},Init=()=>{},Delete=()=>++delete,DisplayResult=()=>{throw new InvalidOperationException("display fault");}};h.MachineFlag188=()=>{++disabled;return false;};bool fault=false;try{OriginalHandpaySelected.Apply(owner,h);}catch(InvalidOperationException){fault=true;}check(fault&&current!=null&&delete==0&&disabled==0);owner.Reload();check(owner.State.InProgress==1&&owner.State.ResidualSubstate==3&&owner.State.HandpaySelectedByte==127);}
            }
            // Translation callback replaces manager2c4: later virtual operations
            // and deletion target the fresh pointer, not the initially created one.
            {
                using(var owner=new OriginalHandpaySelectionPersistence(new Device(),Convert.FromBase64String(vectors.cases[0].initial),s=>{}))
                {int a=0,b=0;OriginalHandpaySelected.Screen current=null;var one=DiagnosticScreen(()=>++a);var two=DiagnosticScreen(()=>++b);var h=DiagnosticHost(s=>current=s,()=>current);h.CreateScreen=()=>one;h.Translate=(key,args,f)=>{current=two;return Encoding.ASCII.GetBytes("x y");};check(OriginalHandpaySelected.Apply(owner,h)&&a==0&&b==1&&current==null);}
            }
            OriginalHandpaySelected.Screen flowScreen=null;checks+=OriginalCreditOutStateFlowValidation.ValidateHandpayBodyBinding(DiagnosticHost(s=>flowScreen=s,()=>flowScreen));
            Debug.Log("HANDPAY_SELECTED_VALIDATED cases="+index+" checks="+checks);return checks;
        }
        private static OriginalHandpaySelected.Screen DiagnosticScreen(Action delete)
        {return new OriginalHandpaySelected.Screen{SetLanguage=s=>{},AddText=(b,n)=>{},SetYesText=b=>{},SetNoText=b=>{},SetAmount=n=>{},SetOrder=n=>{},Init=()=>{},Delete=delete,DisplayResult=()=>1};}
        private static OriginalHandpaySelected.Host DiagnosticHost(Action<OriginalHandpaySelected.Screen> assign,Func<OriginalHandpaySelected.Screen> read)
        {return new OriginalHandpaySelected.Host{TiltPending=code=>true,Emit=(b,f)=>{},CreateScreen=()=>DiagnosticScreen(()=>{}),ReadManagerScreen2C4=read,AssignManagerScreen2C4=assign,JurisdictionFlag448=()=>false,MachineFlag188=()=>false,SelectedLanguage=()=>"en",FormatCredit=(n,b,d,c)=>{},Translate=(key,args,f)=>Encoding.ASCII.GetBytes("x y")};}
    }
}
