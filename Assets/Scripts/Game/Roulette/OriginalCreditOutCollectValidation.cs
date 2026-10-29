using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCollectValidation
    {
        [Serializable] private class Cases {public Vector[] cases;}
        [Serializable] private class Read {public string key;public long[] values;}
        [Serializable] private class Vector {public int mask,stateInitial,stateAfter,game,flagAfter;public long[] frame;public long minimum;public int[] modes;public Read[] reads;public string before,after,trace;public string[] saves,events;public bool result;}
        private sealed class Device:IOriginalNvDevice
        {
            private byte[] bytes=new byte[0];public bool Fail;public Action<byte[]> Commit;
            public int Size{get{return bytes.Length;}}
            public byte[] Read(int offset,int count){var b=new byte[count];Array.Copy(bytes,offset,b,0,count);return b;}
            public void Write(int offset,byte[] b)
            {
                if(Fail)throw new OriginalNvFault("Injected Collect NV failure");Array.Copy(b,0,bytes,offset,b.Length);
                if(Commit!=null&&offset==0&&b.Length==1&&b[0]==1)Commit(Read(20,135));
            }
            public void SetSize(int size){Array.Resize(ref bytes,size);}
        }
        private sealed class Context
        {
            public readonly object Voucher=new object();public readonly OriginalCreditOutCollect.Host Host;
            public readonly OriginalCreditOutCollect.Runtime Runtime;public readonly List<string> Trace=new List<string>();
            public readonly List<byte[]> Events=new List<byte[]>(),Saves=new List<byte[]>();public int State;
            private readonly Vector vector;private readonly Dictionary<string,int> counts=new Dictionary<string,int>();
            private readonly Dictionary<string,long[]> overrides=new Dictionary<string,long[]>();
            private static readonly Dictionary<string,int> bits=new Dictionary<string,int>{{"m218",0},{"cfc",1},{"m1bc",2},{"m140",3},{"j1f8",6},{"j5c",7},{"j42c",8},{"j428",9},{"s58",10},{"of4",12},{"ob0",13},{"g24",14},{"m2c8",15},{"m2fc",16},{"m390",17},{"combine",18}};
            public Context(Vector v)
            {
                vector=v;State=v.stateInitial;foreach(var r in v.reads)overrides.Add(r.key,r.values);
                Runtime=new OriginalCreditOutCollect.Runtime{MinimumCollectAmount=v.minimum,ManagerEventField=0x89abcdef,
                    FinishPendingByte=(byte)((v.mask&(1<<21))!=0?1:0),Field29A=(byte)((v.mask&(1<<20))!=0?7:0)};
                var priority=new OriginalCreditOutCashoutSelection.Host(o=>Get("m"+o.ToString("x"))!=0,o=>Get("m"+o.ToString("x")),Check,
                    o=>Get("j"+o.ToString("x"))!=0,o=>Get("c"+o.ToString("x"))!=0,()=>{
                        if(Get("s58")==0)return null;if(counts["s58"]==1)return FirstVoucher;return SecondVoucher;
                    });
                Host=new OriginalCreditOutCollect.Host{Priority=priority,
                    ArmFinishTrigger=()=>Trace.Add("arm:0"),CreditInOperation3C=()=>Trace.Add("in3c"),FinishStates=()=>Trace.Add("finish"),
                    ReadManagerState=()=>{Trace.Add("state");return State;},AssignManagerState=s=>{State=s;Trace.Add("assign:"+s);},
                    ReadGameFieldE0=()=>{Trace.Add("gamee0");return v.game;},ManagerFlag=o=>Get("o"+o.ToString("x"))!=0,
                    GlobalCreditOutFlag=o=>Get("g"+o.ToString("x"))!=0,
                    GlobalCreditOutOperation=(o,b)=>{if(o!=0xd4||b)throw new Exception("Wrong Collect prefix operation");Trace.Add("gd4:0");},
                    ManagerAmountOperation=(o,a)=>{if(o!=0x10c)throw new Exception("Wrong Collect amount operation");Trace.Add("op10c:"+a.ToString(CultureInfo.InvariantCulture));},
                    CreditAmount=o=>Get("c"+o.ToString("x")),ExternalAmount=o=>Get("e"+o.ToString("x")),
                    ExternalCollect=a=>{Trace.Add("e20:"+a.ToString(CultureInfo.InvariantCulture));return (v.mask&(1<<19))!=0;},
                    CombineCashable=()=>Get("combine")!=0,MachineInt=o=>(int)Get("i"+o.ToString("x")),
                    Emit=(b,flags)=>{if(flags!=0)throw new Exception("Wrong event dispatch flags");Trace.Add("event");Events.Add((byte[])b.Clone());}};
            }
            public void Committed(byte[] data){Trace.Add("save");Saves.Add(data);}
            private bool FirstVoucher(ref long amount,object v){throw new Exception("First Voucher lookup invoked");}
            private bool SecondVoucher(ref long amount,object v)
            {
                if(!ReferenceEquals(v,Voucher))throw new Exception("Voucher identity changed");Trace.Add("v8:"+amount.ToString(CultureInfo.InvariantCulture));amount=-77;
                return (vector.mask&(1<<11))!=0;
            }
            private bool Check(int offset,long value)
            {
                if(offset!=0x328)throw new Exception("Wrong machine amount check");string key="q328";Trace.Add(key+":"+value.ToString(CultureInfo.InvariantCulture));
                int count;counts.TryGetValue(key,out count);counts[key]=count+1;long[] values;
                if(overrides.TryGetValue(key,out values))return values[Math.Min(count,values.Length-1)]!=0;
                return (vector.mask&(1<<(value==500?5:4)))!=0;
            }
            private long Get(string key)
            {
                Trace.Add(key);int count;counts.TryGetValue(key,out count);counts[key]=count+1;long[] values;
                if(overrides.TryGetValue(key,out values))return values[Math.Min(count,values.Length-1)];
                switch(key){case "c18":return vector.frame[2];case "c1c":return vector.frame[3];case "c20":return vector.frame[4];case "e2c":return vector.frame[1];
                    case "m10c":case "m2f4":return 1000;case "m148":return 500;case "mb4":return 100;
                    case "i1fc":return vector.modes[0];case "i1e4":return vector.modes[1];case "i1ec":return vector.modes[2];case "i20c":return vector.modes[3];}
                return (vector.mask&(1<<bits[key]))!=0?1:0;
            }
        }
        private static byte[] Bytes(string hex){var b=new byte[hex.Length/2];for(int i=0;i<b.Length;++i)b[i]=Convert.ToByte(hex.Substring(i*2,2),16);return b;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public static int Validate(string path)
        {
            int checks=0,index=0;Action<bool> check=b=>{if(!b)throw new Exception("Collect regression vector="+index+" check="+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==24094);
            foreach(var v in vectors.cases)
            {
                var device=new Device();var context=new Context(v);var seed=Bytes(v.before);
                using(var owner=new OriginalHandpaySelectionPersistence(device,seed,message=>{}))
                {
                    var stateReference=owner.State;device.Commit=context.Committed;
                    check(owner.Collect(context.Runtime,context.Host,v.frame[0],(v.mask&(1<<23))!=0,context.Voucher)==v.result);
                    check(string.Join("|",context.Trace)==v.trace);check(context.State==v.stateAfter&&context.Runtime.Field29A==v.flagAfter);
                    check(context.Saves.Count==v.saves.Length&&context.Events.Count==v.events.Length);
                    for(int i=0;i<v.saves.Length;++i)check(Equal(context.Saves[i],Bytes(v.saves[i])));
                    for(int i=0;i<v.events.Length;++i)check(Equal(context.Events[i],Bytes(v.events[i])));
                    check(Equal(owner.PayloadBytes,Bytes(v.after)));check(ReferenceEquals(owner.State,stateReference)&&owner.State.HandpaySelectedByte==owner.PayloadBytes[0x5d]);
                    owner.ValidateStorage();owner.Reload();check(Equal(owner.PayloadBytes,Bytes(v.after)));device.Commit=null;
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                {cold.ValidateStorage();check(Equal(cold.PayloadBytes,Bytes(v.after)));}++index;
            }
            // Save failure occurs before prefix operations, and the shared State
            // reference reflects the attempted byte until Reload recovers it.
            var fault=new Device();var c=new Context(vectors.cases[0]);c.Runtime.FinishPendingByte=0;
            var initial=Bytes(vectors.cases[0].before);
            using(var owner=new OriginalHandpaySelectionPersistence(fault,initial,message=>{}))
            {
                fault.Fail=true;bool failed=false;try{owner.Collect(c.Runtime,c.Host,995,false,c.Voucher);}catch(OriginalNvFault){failed=true;}
                check(failed&&c.Trace.Count==0&&owner.State.HandpaySelectedByte==1);fault.Fail=false;owner.Reload();check(Equal(owner.PayloadBytes,initial));
            }
            // An external operation throwing must leave its already-saved pending
            // state2 recoverable; no successful reset or Finish is invented.
            var pendingVector=new Vector{mask=(1<<12)|(1<<14)|(1<<15),frame=new long[]{995,0,0,995,0},modes=new[]{2,2,2,2},reads=new Read[0]};
            var pending=new Context(pendingVector);var pendingDevice=new Device();var zero=new byte[135];
            using(var owner=new OriginalHandpaySelectionPersistence(pendingDevice,zero,message=>{}))
            {
                pending.Host.ExternalCollect=a=>{throw new IOException("Injected external operation failure");};bool failed=false;
                try{owner.Collect(pending.Runtime,pending.Host,995,false,pending.Voucher);}catch(IOException){failed=true;}
                check(failed);owner.Reload();check(owner.CurrentPayload.ExternalTransferState==2&&owner.CurrentPayload.CashOutAmount==995&&pending.State==0);
            }
            using(var cold=new OriginalHandpaySelectionPersistence(pendingDevice,new byte[135],message=>{}))
            {check(cold.CurrentPayload.ExternalTransferState==2&&cold.CurrentPayload.CollectSelectedByte==1);cold.ValidateStorage();}
            Debug.Log("COLLECT_VALIDATED cases="+vectors.cases.Length+" checks="+checks);return checks;
        }
    }
}
