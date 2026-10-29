using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCashoutSelectionValidation
    {
        [Serializable] private class Cases {public Vector[] cases;}
        [Serializable] private class Read {public string key;public long[] values;}
        [Serializable] private class Vector {public bool hopper;public int mask,type;public long amount,threshold,limit,coin;public Read[] reads;public string trace,setupTrace,after;}
        private sealed class Device:IOriginalNvDevice
        {
            public bool Fail;private byte[] bytes=new byte[0];public int Size {get{return bytes.Length;}}
            public byte[] Read(int offset,int count){var b=new byte[count];Array.Copy(bytes,offset,b,0,count);return b;}
            public void Write(int offset,byte[] b){if(Fail)throw new OriginalNvFault("Injected cashout save failure");Array.Copy(b,0,bytes,offset,b.Length);}
            public void SetSize(int size){Array.Resize(ref bytes,size);}
        }
        private sealed class Context
        {
            public readonly object OriginalVoucher=new object();public readonly List<string> Trace=new List<string>();
            public readonly OriginalCreditOutCashoutSelection.Host Host;
            private readonly Vector vector;private readonly Dictionary<string,int> counters=new Dictionary<string,int>();
            private readonly Dictionary<string,long[]> overrides=new Dictionary<string,long[]>();
            private static readonly Dictionary<string,int> bits=new Dictionary<string,int>{{"m218",0},{"cfc",1},{"m1bc",2},{"m140",3},{"j1f8",6},{"j5c",7},{"j42c",8},{"j428",9},{"s58",10}};
            public Context(Vector value)
            {
                vector=value;foreach(var read in value.reads)overrides.Add(read.key,read.values);
                Host=new OriginalCreditOutCashoutSelection.Host(offset=>Get("m"+offset.ToString("x"))!=0,
                    offset=>Get("m"+offset.ToString("x")),(offset,amount)=>{
                        if(offset!=0x328)throw new Exception("Wrong MachineData check offset");Trace.Add("q328:"+amount.ToString(CultureInfo.InvariantCulture));
                        return (vector.mask&(1<<(amount==vector.amount?4:5)))!=0;
                    },offset=>Get("j"+offset.ToString("x"))!=0,offset=>Get("c"+offset.ToString("x"))!=0,()=>{
                        if(Get("s58")==0)return null;
                        if(counters["s58"]==1)return FirstVoucher;
                        return SecondVoucher;
                    });
            }
            private bool FirstVoucher(ref long amount,object source){throw new Exception("First Voucher lookup result must not be invoked");}
            private bool SecondVoucher(ref long amount,object source)
            {
                if(amount!=vector.amount||!ReferenceEquals(source,OriginalVoucher))throw new Exception("Voucher arguments changed");
                Trace.Add("v8:"+amount.ToString(CultureInfo.InvariantCulture));amount=123456789;return (vector.mask&(1<<11))!=0;
            }
            private long Get(string key)
            {
                Trace.Add(key);int count;counters.TryGetValue(key,out count);counters[key]=count+1;
                long[] values;if(overrides.TryGetValue(key,out values))return values[Math.Min(count,values.Length-1)];
                if(key=="m10c")return vector.threshold;if(key=="m148")return vector.limit;if(key=="mb4")return vector.coin;
                return (vector.mask&(1<<bits[key]))!=0?1:0;
            }
        }
        private static byte[] Bytes(string hex){var b=new byte[hex.Length/2];for(int i=0;i<b.Length;++i)b[i]=Convert.ToByte(hex.Substring(i*2,2),16);return b;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public static int Validate(string path)
        {
            int checks=0;Action<bool> check=value=>{if(!value)throw new Exception("Cashout selection regression "+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==65541);
            var seed=new byte[135];for(int i=0;i<seed.Length;++i)seed[i]=(byte)(i*19+7);
            var types=new HashSet<int>();
            foreach(var v in vectors.cases)
            {
                var c=new Context(v);int type=v.hopper?OriginalCreditOutCashoutSelection.HopperPrimary(c.Host,v.amount,c.OriginalVoucher)
                    :OriginalCreditOutCashoutSelection.PrinterPrimary(c.Host,v.amount,c.OriginalVoucher);
                check(type==v.type);check(string.Join("|",c.Trace)==v.trace);types.Add(type);
                c=new Context(v);var device=new Device();var expected=Bytes(v.after);
                using(var owner=new OriginalHandpaySelectionPersistence(device,seed,message=>{}))
                {
                    check(owner.SelectAndSetupCashOut(c.Host,v.hopper,v.amount,c.OriginalVoucher)==v.type);
                    check(string.Join("|",c.Trace)==v.setupTrace);check(Equal(owner.PayloadBytes,expected));
                    owner.ValidateStorage();owner.Reload();check(Equal(owner.PayloadBytes,expected));
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                {cold.ValidateStorage();check(Equal(cold.PayloadBytes,expected));}
            }
            check(types.SetEquals(new[]{11,12,15,16,18,25}));
            // Failed Save must propagate, with the previous durable payload recoverable.
            var faultDevice=new Device();var context=new Context(vectors.cases[0]);
            using(var owner=new OriginalHandpaySelectionPersistence(faultDevice,seed,message=>{}))
            {
                faultDevice.Fail=true;bool failed=false;
                try{owner.SelectAndSetupCashOut(context.Host,true,vectors.cases[0].amount,context.OriginalVoucher);}catch(OriginalNvFault){failed=true;}
                check(failed);faultDevice.Fail=false;owner.Reload();check(Equal(owner.PayloadBytes,seed));
            }
            // Disappearing second Voucher lookup has no managed successful selection.
            int lookups=0;OriginalCreditOutCashoutSelection.VoucherCheck dummy=(ref long a,object b)=>true;
            var vanished=new OriginalCreditOutCashoutSelection.Host(offset=>offset==0x1bc,offset=>1,(offset,a)=>false,
                offset=>false,offset=>false,()=>++lookups==1?dummy:null);
            bool rejected=false;try{OriginalCreditOutCashoutSelection.HopperPrimary(vanished,995,null);}catch(InvalidOperationException){rejected=true;}
            check(rejected&&lookups==2);
            Debug.Log("CASHOUT_SELECTION_VALIDATED cases="+vectors.cases.Length+" checks="+checks+" types=11,12,15,16,18,25");return checks;
        }
    }
}
