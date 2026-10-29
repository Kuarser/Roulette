using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutCashoutContinueStateValidation
    {
        [Serializable] private class Cases{public Vector[] cases;}
        [Serializable] private class Config{public bool juris,machine,control,tilt2,tiltActive,external,externalResult,coinReady,soft;}
        [Serializable] private class Vector{public int game,field29a,field3aa,field3ab,field23c,field2c8,machineInt,after29a,after3ab,manager;public long limit,credit,externalAmount,creditSecond,externalSecond;public Config config;public bool softThrow;public string before,after,trace;public string[] saves,events;}
        private sealed class Device:IOriginalNvDevice
        {
            private byte[] bytes=new byte[0];public Action<byte[]> Commit;public Action SoftSize;
            public int Size{get{if(SoftSize!=null){var call=SoftSize;SoftSize=null;call();}return bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,bytes,o,b.Length);if(Commit!=null&&o==0&&b.Length==1&&b[0]==1)Commit(Read(20,135));}
            public void SetSize(int n){Array.Resize(ref bytes,n);}
        }
        private static byte[] Bytes(string hex){var b=new byte[hex.Length/2];for(int i=0;i<b.Length;++i)b[i]=Convert.ToByte(hex.Substring(i*2,2),16);return b;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public static int Validate(string path)
        {
            int checks=0,index=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOutContinueState regression vector="+index+" check="+checks);++checks;};
            var vectors=JsonUtility.FromJson<Cases>(File.ReadAllText(path));check(vectors.cases.Length==16001);
            foreach(var v in vectors.cases)
            {
                var trace=new List<string>();var saves=new List<byte[]>();var events=new List<byte[]>();int manager=6,creditReads=0,externalReads=0;
                var runtime=new OriginalCreditOutCollect.Runtime{Field29A=(byte)v.field29a,Field3AA=(byte)v.field3aa,Field3AB=(byte)v.field3ab,Field23C=(byte)v.field23c,Field2C8=v.field2c8};var device=new Device();
                using(var owner=new OriginalHandpaySelectionPersistence(device,Bytes(v.before),message=>{}))
                {
                    var reference=owner.State;device.Commit=b=>{trace.Add("save");saves.Add(b);};
                    var screen=new object();var c=v.config;
                    var host=new OriginalCreditOutCashoutContinueState.Host{
                        GameOperation=o=>trace.Add("game:"+o),AssignManagerState=n=>{manager=n;trace.Add("state:"+n);},
                        MachineOperation=(o,f)=>trace.Add("machineop:"+o+":"+(f?1:0)),ManagerBoolOperation=(o,f)=>trace.Add("mb:"+o+":"+(f?1:0)),
                        ManagerAmountOperation=(o,amount)=>trace.Add("ma:"+o+":"+amount),ReadGameFieldE0=()=>{trace.Add("gamee0");return v.game;},
                        MachineFlag=o=>{check(o==0x190);trace.Add("machineflag:"+o);return c.machine;},
                        JurisdictionFlag=o=>{check(o==0x444);trace.Add("juris:"+o);return c.juris;},ControlFlag=o=>{check(o==0x184);trace.Add("control:"+o);return c.control;},
                        MachineAmount=o=>{check(o==0xfc||o==0x148);trace.Add("machineamount:"+o);return v.limit;},
                        CreditAmount=o=>{check(o==0||o==0x18);trace.Add("credit:"+o);return o==0||creditReads++==0?v.credit:v.creditSecond;},
                        MachineInt=o=>{check(o==0x20c);trace.Add("machineint:"+o);return v.machineInt;},ExternalAmount2C=()=>{trace.Add("externalamount");return externalReads++==0?v.externalAmount:v.externalSecond;},
                        TiltSnapshotByte2=()=>{trace.Add("tiltsnapshot");return (byte)(c.tilt2?1:0);},TiltActive=code=>{check(code==0x6020||code==0x15600);trace.Add("tilt:"+code);return c.tiltActive;},
                        CoinOutReady=()=>{trace.Add("coinready");return c.coinReady;},ExternalFlag68=()=>{trace.Add("external68");return c.external;},
                        ExternalOperation10=(x,y)=>{trace.Add("external10:"+x+":"+y);return c.externalResult;},CoinOutOperation8=n=>trace.Add("coinout:"+n),
                        DisplayHandPay=n=>trace.Add("hand:"+n),CoinInOperation4=()=>trace.Add("coinin"),FinishStates=()=>{check(manager==26);trace.Add("finish");},
                        ArmTrigger=(o,n)=>trace.Add("arm:"+o+":"+n),Translate=(key,parameters,f)=>{check(f);trace.Add("translate:"+key+":"+string.Join(",",parameters));return key+"["+string.Join(",",parameters)+"]";},
                        DisplayMessage=(title,body)=>trace.Add("message:"+title+":"+body),CreateInfoScreen=(title,body)=>{trace.Add("create:"+title+":"+body);return screen;},
                        PublishInfoScreen=o=>{check(ReferenceEquals(o,screen));trace.Add("publish");},DisplayInfoScreen=o=>{check(ReferenceEquals(o,screen));trace.Add("displayinfo");},
                        ArmInfoTrigger=(o,n)=>{check(ReferenceEquals(o,screen));trace.Add("arminfo:"+n);},SoftValidationEnabled=c.soft,
                        NvDeviceContextLog=text=>{check(text=="CreditOutMgr.cpp::CashOutContinueState(6309)");trace.Add("softlog");if(v.softThrow)throw new InvalidOperationException("Injected context fault");device.SoftSize=()=>trace.Add("softsize");},
                        Emit=(bytes,flags)=>{check(flags==0);trace.Add("event");events.Add((byte[])bytes.Clone());}};
                    bool fault=false;try{check(owner.CashOutContinueState(runtime,host));}catch(InvalidOperationException e){check(e.Message=="Injected context fault");fault=true;}
                    check(fault==v.softThrow);check(string.Join("|",trace)==v.trace);check(runtime.Field29A==v.after29a&&runtime.Field3AB==v.after3ab&&manager==v.manager);
                    check(saves.Count==v.saves.Length&&events.Count==v.events.Length);for(int i=0;i<saves.Count;++i)check(Equal(saves[i],Bytes(v.saves[i])));
                    for(int i=0;i<events.Count;++i)check(Equal(events[i],Bytes(v.events[i])));check(Equal(owner.PayloadBytes,Bytes(v.after)));
                    check(ReferenceEquals(reference,owner.State)&&owner.State.ResidualSubstate==owner.CurrentPayload.ResidualSubstateValue);
                    owner.ValidateStorage();owner.Reload();check(Equal(owner.PayloadBytes,Bytes(v.after)));device.Commit=null;
                }
                using(var cold=new OriginalHandpaySelectionPersistence(device,new byte[135],message=>{}))
                {cold.ValidateStorage();check(Equal(cold.PayloadBytes,Bytes(v.after)));}++index;
            }
            Debug.Log("CASHOUT_CONTINUE_VALIDATED cases="+vectors.cases.Length+" checks="+checks);return checks;
        }
    }
}
