using System;
using System.Collections.Generic;
using System.IO;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutStateFlowValidation
    {
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public Action<byte[]> Commit;public bool FailWrite;
            public int Size{get{return Bytes.Length;}}
            public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){if(FailWrite)throw new OriginalNvFault("Injected flow write fault");Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1&&Commit!=null)Commit(Read(20,BitConverter.ToInt32(Bytes,4)));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        private sealed class Fixture:IDisposable
        {
            internal readonly Device StateDevice=new Device(),DataDevice=new Device();internal readonly OriginalPersistentState State=new OriginalPersistentState();
            internal readonly OriginalHandpaySelectionPersistence Payload;internal readonly OriginalCreditOutCollect.Runtime Runtime=new OriginalCreditOutCollect.Runtime{ManagerEventField=0x89abcdef};
            internal readonly List<string> Trace=new List<string>();internal readonly List<byte[]> Events=new List<byte[]>(),StateSaves=new List<byte[]>(),DataSaves=new List<byte[]>();
            internal readonly OriginalCreditOutStateFlow Flow;internal int Game,TicketResult=3;internal bool EventFault;
            internal readonly OriginalCreditOutCashoutState.Host H4;internal readonly OriginalCreditOutCollect.Host HC;
            internal Fixture(int start,int kind,long count,long residual,bool block,bool residualFlag,OriginalCreditOutResidualState.Host residualTemplate=null,
                OriginalCreditOutResidualScreens.OptionHost optionTemplate=null,OriginalCreditOutResidualScreens.FailedHost failedTemplate=null,OriginalHandpaySelected.Host selectedTemplate=null)
            {
                State.Open(()=>StateDevice,false,message=>{},false,null);State.Assign(start);
                var bytes=new byte[135];Write32(bytes,0,kind);Write64(bytes,4,995);Write64(bytes,12,count);Write64(bytes,36,residual);bytes[81]=1;bytes[85]=(byte)(block?7:0);bytes[88]=(byte)(residualFlag?7:0);bytes[93]=1;
                Payload=new OriginalHandpaySelectionPersistence(DataDevice,bytes,message=>{});
                StateDevice.Commit=bytes2=>{StateSaves.Add(bytes2);Trace.Add("S:"+bytes2[0]+":"+bytes2[1]+":"+bytes2[2]);};
                DataDevice.Commit=bytes2=>{DataSaves.Add(bytes2);Trace.Add("D:"+BitConverter.ToInt32(bytes2,0));};
                Action<int> game=o=>Trace.Add("game:"+o);Action<byte[],uint> emit=(bytes2,flags)=>{if(flags!=0)throw new Exception("Unexpected event flag");Trace.Add("event:"+bytes2.Length);if(EventFault)throw new InvalidOperationException("event fault");Events.Add((byte[])bytes2.Clone());};
                H4=new OriginalCreditOutCashoutState.Host{ReadSubState=()=>{throw new Exception("Unbound template sub read");},AssignSubState=n=>{throw new Exception("Unbound template sub assign");},AssignManagerState=n=>{throw new Exception("Unbound template manager assign");},CashOutVoucherState=()=>{throw new Exception("Unbound template state5");},ExternalFlag80=()=>{Trace.Add("external80");return false;},ReadGameFieldE0=()=>{Trace.Add("gamee0");return Game;},GameOperation4C=(amount,f)=>{if(amount!=0||!f)throw new Exception("Invalid Game4c");Trace.Add("game4c");},GameOperation64=amount=>{if(amount!=0)throw new Exception("Invalid Game64");Trace.Add("game64");},Emit=emit};
                var h5=new OriginalCreditOutCashoutVoucherState.Host{PayTicket=(n,amount)=>{Trace.Add("ticket:"+n+":"+amount);return TicketResult;},MachineFlag=o=>{Trace.Add("mf:"+o);return false;},JurisdictionFlag=o=>{Trace.Add("jf:"+o);return false;},ControlFlag=o=>{Trace.Add("cf:"+o);return false;},MachineAmount=o=>{Trace.Add("machine:"+o);return o==0xb4?7:100;},GameOperation64=amount=>Trace.Add("game64"),CreditInOperation34=()=>Trace.Add("creditin34"),Emit=emit};
                var screen=new object();var h6=new OriginalCreditOutCashoutContinueState.Host{GameOperation=game,MachineOperation=(o,f)=>Trace.Add("mo:"+o+":"+f),ManagerBoolOperation=(o,f)=>Trace.Add("mb:"+o+":"+f),ManagerAmountOperation=(o,amount)=>Trace.Add("ma:"+o+":"+amount),ReadGameFieldE0=()=>{Trace.Add("gamee0");return Game;},MachineFlag=o=>false,JurisdictionFlag=o=>{Trace.Add("jf:"+o);return false;},ControlFlag=o=>{Trace.Add("cf:"+o);return false;},MachineAmount=o=>100,CreditAmount=o=>{Trace.Add("credit:"+o);return 0;},MachineInt=o=>0,ExternalAmount2C=()=>{Trace.Add("externalamount");return 0;},TiltSnapshotByte2=()=>{Trace.Add("tiltsnapshot");return 0;},TiltActive=code=>{Trace.Add("tilt:"+code);return false;},CoinOutReady=()=>{Trace.Add("coinready");return true;},ExternalFlag68=()=>{Trace.Add("external68");return false;},ExternalOperation10=(x,y)=>{Trace.Add("external10:"+x+":"+y);return false;},CoinOutOperation8=n=>Trace.Add("coinout:"+n),DisplayHandPay=n=>Trace.Add("hand:"+n),CoinInOperation4=()=>Trace.Add("coinin"),ArmTrigger=(o,n)=>Trace.Add("arm:"+o+":"+n),Translate=(key,args,f)=>{Trace.Add("translate:"+key+":"+string.Join(",",args));return key;},DisplayMessage=(title,body)=>Trace.Add("message:"+title+":"+body),CreateInfoScreen=(title,body)=>screen,PublishInfoScreen=o=>Trace.Add("publish"),DisplayInfoScreen=o=>Trace.Add("info"),ArmInfoTrigger=(o,n)=>Trace.Add("infoarm:"+n),Emit=emit};
                var priority=new OriginalCreditOutCashoutSelection.Host(o=>false,o=>o==0xb4?7:100,(o,amount)=>false,o=>false,o=>false,()=>null);
                HC=new OriginalCreditOutCollect.Host{Priority=priority,ArmFinishTrigger=()=>Trace.Add("finisharm"),CreditInOperation3C=()=>Trace.Add("creditin3c"),ReadGameFieldE0=()=>Game,ManagerFlag=o=>o==0xf4,GlobalCreditOutFlag=o=>false,GlobalCreditOutOperation=(o,f)=>Trace.Add("global:"+o+":"+f),ManagerAmountOperation=(o,amount)=>Trace.Add("collectma:"+o+":"+amount),CreditAmount=o=>995,ExternalAmount=o=>0,ExternalCollect=amount=>false,CombineCashable=()=>false,MachineInt=o=>0,Emit=emit};
                var finish=new OriginalCreditOutFinishStates.Host{StateOperation=(n,input)=>{throw new Exception("Unrestored real handler "+n);},DisplayHandPay=amount=>Trace.Add("directhand:"+amount),DisplayTicketReceipt=amount=>Trace.Add("receipt:"+amount),ProcessResidualGamble=()=>Trace.Add("residualstate")};
                Flow=new OriginalCreditOutStateFlow(State,Payload,Runtime,HC,H4,h5,h6,finish,residualTemplate,optionTemplate,failedTemplate,selectedTemplate);
            }
            public void Dispose(){Payload.Dispose();State.Dispose();}
        }
        public static int ValidateResidualBinding(OriginalCreditOutResidualState.Host template)
        {
            int checks=0;var original=template.AssignManagerState;template.AssignManagerState=n=>{throw new Exception("Template manager must be rebound");};
            foreach(int residual in new[]{8,9})using(var f=new Fixture(26,11,0,37,false,false,template))
            {
                f.Payload.CurrentPayload.ResidualSubstateValue=residual;f.Payload.SaveCurrentPayload();f.Trace.Clear();
                if(!f.Flow.Finish()||f.State.Value!=(residual==9?0:26)||f.State.ReferenceCount!=1)throw new Exception("Residual real State binding");++checks;
                f.State.ValidateStorage();f.Payload.ValidateStorage();
                if(residual==9&&(f.Payload.CurrentPayload.CashOutAmount!=0||f.Payload.SelectionAmount!=0))throw new Exception("Residual same payload binding");++checks;
            }
            if(template.AssignManagerState==original)throw new Exception("Residual template identity");++checks;template.AssignManagerState=original;return checks;
        }
        public static int ValidateResidualScreenBinding()
        {
            int checks=0;
            foreach(bool selected in new[]{false,true})
            {
                int modes=0,displayed=0,published=0,armed=0;var trace=new List<string>();var packets=new List<byte[]>();
                var screen=new OriginalCreditOutResidualScreens.Screen{SetPosition=(x,y)=>{},SetSize=(x,y)=>{},SetOrder=n=>{},AddText=(b,n)=>{},SetLanguage=l=>{},AddTranslatedText=(key,args,n)=>{},Init=()=>trace.Add("init"),Display=()=>{++displayed;trace.Add("display");}};
                var failed=new OriginalCreditOutResidualScreens.FailedHost{FormatCredit=(n,b,d,c)=>System.Text.Encoding.ASCII.GetBytes(n.ToString()),CreateInfoScreen=()=>screen,ReadManagerScreen228=()=>screen,JurisdictionFlag448=()=>false,SelectedLanguage=()=>"en",SetInfoTimerHandler=o=>trace.Add("handler"),PublishInfoScreen=o=>{++published;trace.Add("publish");},ArmInfoTrigger=(o,n)=>{if(n!=2500)throw new Exception("Failed trigger");++armed;trace.Add("arm2500");}};
                var residual=new OriginalCreditOutResidualState.Host{MachineFlag=o=>false,MachineInt=o=>++modes<=2?3:1,GameFlag370=()=>false,MGSScreenDisplayed=()=>false,CreditAmount0=()=>995,GameOperation=o=>trace.Add("game:"+o),AssignManagerState=n=>{throw new Exception("Template manager");},MachineOperation=(o,b)=>trace.Add("machine:"+b),GameOperation1F0=n=>{},DisplayCashoutFailed=n=>{throw new Exception("Template failed screen");},DisplayOptionHandpayCancel=()=>{throw new Exception("Template option screen");},ArmTrigger=(o,n)=>trace.Add("arm:"+o+":"+n),Emit=(b,f)=>{packets.Add(b);trace.Add("event:"+BitConverter.ToUInt32(b,24));}};
                var option=new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>false,GameOperation=o=>trace.Add("optiongame:"+o),MachineOperation=(o,b)=>trace.Add("optionmachine:"+b),HandPaySelected=()=>selected,ProcessResidualGamble=()=>{throw new Exception("Template residual body");},ReadManagerEvent18=()=>0xfedcba98,Emit=(b,f)=>{packets.Add(b);trace.Add("optionevent");}};
                var templateOption=option.ProcessResidualGamble;var templateFailed=residual.DisplayCashoutFailed;var templateDisplay=residual.DisplayOptionHandpayCancel;
                using(var f=new Fixture(26,11,0,37,false,false,residual,option,failed))
                {
                    f.Payload.CurrentPayload.ResidualSubstateValue=2;f.Payload.SaveCurrentPayload();f.Trace.Clear();f.Runtime.Field3AB=7;
                    if(!f.Flow.Finish()||f.State.ReferenceCount!=1||f.Runtime.Field3AB!=0)throw new Exception("Bound residual screen ownership");++checks;
                    if(selected)
                    {if(f.State.Value!=26||f.Payload.CurrentPayload.ResidualSubstateValue!=4||displayed!=0||packets.Count!=0||f.DataSaves.Count!=3)throw new Exception("Bound selected screen path");++checks;}
                    else
                    {
                        if(f.State.Value!=0||f.Payload.CurrentPayload.ResidualSubstateValue!=0||f.Payload.CurrentPayload.CashOutAmount!=0||displayed!=1||published!=1||armed!=1||packets.Count!=2)throw new Exception("Bound cancel failed screen path");++checks;
                        if(BitConverter.ToUInt32(packets[1],24)!=0xfedcba98||BitConverter.ToInt64(packets[1],28)!=0||trace.IndexOf("init")>=trace.IndexOf("publish")||trace.IndexOf("publish")>=trace.IndexOf("display")||trace.IndexOf("display")>=trace.IndexOf("arm2500"))throw new Exception("Bound failure/event sequence");++checks;
                    }
                    f.State.ValidateStorage();f.Payload.ValidateStorage();
                }
                if(option.ProcessResidualGamble!=templateOption||residual.DisplayCashoutFailed!=templateFailed||residual.DisplayOptionHandpayCancel!=templateDisplay)throw new Exception("Residual screen template mutation");++checks;
            }
            return checks;
        }
        public static int ValidateHandpayBodyBinding(OriginalHandpaySelected.Host body)
        {
            var residual=new OriginalCreditOutResidualState.Host{MachineFlag=o=>false,MachineInt=o=>3,GameFlag370=()=>false,MGSScreenDisplayed=()=>false,CreditAmount0=()=>995,GameOperation=o=>{},AssignManagerState=n=>{throw new Exception("Unbound manager");},MachineOperation=(o,b)=>{},GameOperation1F0=n=>{},DisplayCashoutFailed=n=>{},DisplayOptionHandpayCancel=()=>{throw new Exception("Unbound option");},ArmTrigger=(o,n)=>{},Emit=(b,f)=>{}};
            var option=new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>body.ReadManagerScreen2C4()!=null,GameOperation=o=>{},MachineOperation=(o,b)=>{},HandPaySelected=()=>{throw new Exception("Unbound selection");},ProcessResidualGamble=()=>{throw new Exception("Unbound residual");},ReadManagerEvent18=()=>0,Emit=(b,f)=>{}};var original=option.HandPaySelected;
            using(var f=new Fixture(26,11,0,37,false,false,residual,option,null,body))
            {
                f.Payload.CurrentPayload.ResidualSubstateValue=2;f.Payload.SaveCurrentPayload();
                if(!f.Flow.Finish()||f.State.Value!=26||f.State.ReferenceCount!=1||f.Payload.State.InProgress!=0||f.Payload.State.ResidualSubstate!=4||f.DataSaves.Count!=6||body.ReadManagerScreen2C4()!=null)throw new Exception("HandPaySelected real owner binding");
                f.Payload.ValidateStorage();f.State.ValidateStorage();
            }
            if(option.HandPaySelected!=original)throw new Exception("Selection template mutation");return 2;
        }
        public static int ValidateHandpayUnityFlow(OriginalHandpaySelected.Host body,OriginalHandpaySelectedUnityScreen ui,string choice,Action<string> capture,string snapshots)
        {
            int checks=0,arms=0,games=0;bool interrupted=choice=="interrupt";bool mismatch=choice=="mismatch";
            var residual=new OriginalCreditOutResidualState.Host{MachineFlag=o=>false,MachineInt=o=>3,GameFlag370=()=>false,MGSScreenDisplayed=()=>false,CreditAmount0=()=>995,GameOperation=o=>{},AssignManagerState=n=>{throw new Exception("Unbound manager");},MachineOperation=(o,b)=>{},GameOperation1F0=n=>{},DisplayCashoutFailed=n=>{},DisplayOptionHandpayCancel=()=>{throw new Exception("Unbound option");},ArmTrigger=(o,n)=>{if(o!=0x318||n!=0)throw new Exception("Trigger mismatch");++arms;},Emit=(b,f)=>{}};
            var option=new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>body.ReadManagerScreen2C4()!=null,GameOperation=o=>++games,MachineOperation=(o,b)=>{},HandPaySelected=()=>{throw new Exception("Unbound selection");},ProcessResidualGamble=()=>{throw new Exception("Unbound residual");},ReadManagerEvent18=()=>0x89abcdef,Emit=(b,f)=>{}};
            using(var f=new Fixture(26,11,0,995,false,false,residual,option,null,body))
            {
                f.Payload.CurrentPayload.ResidualSubstateValue=2;f.Payload.SaveCurrentPayload();f.Runtime.Field3AB=7;
                var iterator=f.Flow.RunResidualSelection(()=>mismatch?null:ui);var disposable=(IDisposable)iterator;
                try
                {
                    if(mismatch)
                    {
                        bool fault=false;try{iterator.MoveNext();}catch(InvalidOperationException){fault=true;}
                        if(!fault||f.State.ReferenceCount!=1||f.Payload.State.InProgress!=1||f.Runtime.Field3AB!=7||arms!=0||ui.Visible)throw new Exception("Mismatched UI unwinding");++checks;
                    }
                    else
                    {
                        if(!iterator.MoveNext()||!ui.Visible||f.State.ReferenceCount!=2||f.Payload.State.InProgress!=1||f.Payload.State.ResidualSubstate!=3||f.Runtime.Field3AB!=7||arms!=0)throw new Exception("Unity flow suspension");++checks;capture("idle");
                        if(interrupted)
                        {disposable.Dispose();if(f.State.ReferenceCount!=1||f.Runtime.Field3AB!=7||f.Payload.State.InProgress!=1||arms!=0||!ui.Visible)throw new Exception("Interrupted UI continuation cleanup");++checks;}
                        else
                        {
                            if(choice=="exit")ui.Presentation.Input.SelectExit();else{var button=choice=="yes"?ui.Yes:ui.No;ui.DispatchPointer(0x200001,(short)(button.Rectangle.Left+5),(short)(button.Rectangle.Top+5));}
                            capture("pressed");if(iterator.MoveNext())throw new Exception("Result did not resume native continuation");
                            bool selected=choice!="no";
                            if(f.State.ReferenceCount!=1||f.State.Value!=(selected?26:0)||f.Payload.State.InProgress!=0||f.Payload.State.ResidualSubstate!=(selected?4:0)||f.Payload.State.HandpaySelectedByte!=1||f.Runtime.Field3AB!=0||arms!=(selected?1:2)||games!=(selected?1:2)||ui.Visible||!ui.Deleted||body.ReadManagerScreen2C4()!=null)throw new Exception("Unity flow completed native state");++checks;capture("closed");
                        }
                    }
                    f.Payload.ValidateStorage();f.State.ValidateStorage();
                    File.WriteAllBytes(snapshots+"-state.bin",f.State.DeviceImage);File.WriteAllBytes(snapshots+"-cashout.bin",f.Payload.ReadDeviceImage());
                    using(var cold=OriginalPersistentState.OpenFile(snapshots+"-state.bin",s=>{},false,null)){if(cold.Value!=f.State.Value||cold.ReferenceCount!=1)throw new Exception("Flow State cold transport");cold.ValidateStorage();++checks;}
                    using(var cold=OriginalHandpaySelectionPersistence.OpenFile(snapshots+"-cashout.bin",new byte[135],s=>{})){if(Convert.ToBase64String(cold.PayloadBytes)!=Convert.ToBase64String(f.Payload.PayloadBytes))throw new Exception("Flow cashout cold transport");cold.ValidateStorage();++checks;}
                }
                finally{disposable.Dispose();}
            }
            return checks;
        }
        private static void Write32(byte[] b,int o,int n){for(int i=0;i<4;++i)b[o+i]=(byte)((uint)n>>(i*8));}
        private static void Write64(byte[] b,int o,long n){for(int i=0;i<8;++i)b[o+i]=(byte)((ulong)n>>(i*8));}
        private static bool Has(List<string> trace,string item){return trace.Contains(item);}
        public static int Validate(string directory)
        {
            int checks=0,cases=0;Action<bool> check=b=>{if(!b)throw new Exception("Bound flow regression "+checks);++checks;};
            foreach(int start in new[]{4,5,6})foreach(int kind in new[]{-1,0,11,12,13,14,15,16,18,20,25})foreach(long count in new[]{0L,3L})foreach(long residual in new[]{0L,37L})foreach(bool block in new[]{false,true})foreach(bool flag in new[]{false,true})foreach(int result in new[]{0,3})
            {
                var fixture=new Fixture(start,kind,count,residual,block,flag);fixture.TicketResult=result;var subReader=fixture.H4.ReadSubState;var managerReader=fixture.HC.ReadManagerState;
                check(fixture.Flow.Finish());int final=fixture.State.Value;
                int effectiveKind=(start!=6&&result!=3&&(kind==12||kind==13||kind==25))?15:kind;
                int expected=effectiveKind==11&&count==0?(residual>0?26:0):effectiveKind==14&&(start!=6&&result!=3)?0:6;
                check(final==expected&&fixture.State.ReferenceCount==1);check(ReferenceEquals(subReader,fixture.H4.ReadSubState)&&ReferenceEquals(managerReader,fixture.HC.ReadManagerState));
                check(!Has(fixture.Trace,"Unbound template"));
                if(start==4)
                {
                    check(fixture.StateSaves.Count>=3&&fixture.StateSaves[0][0]==2&&fixture.StateSaves[0][1]==4&&fixture.StateSaves[0][2]==0);
                    check(fixture.StateSaves[1][2]==(block?2:1));check(Has(fixture.Trace,"game4c")==!block);
                    int index=block?2:3;check(fixture.StateSaves[index][0]==1&&fixture.StateSaves[index][1]==5&&fixture.StateSaves[index][2]==2);
                    check(fixture.State.PayloadBytes[2]==2);
                    if(!block){check(fixture.Events.Count>0&&fixture.Events[0].Length==44&&BitConverter.ToUInt32(fixture.Events[0],24)==0x89abcdef&&BitConverter.ToInt64(fixture.Events[0],28)==995);}
                    check(fixture.Payload.CurrentPayload.ExternalTransferBlockedByte==0);
                }
                else check(fixture.State.PayloadBytes[2]==0);
                if(effectiveKind==11&&count>0)check(Has(fixture.Trace,"coinout:3")&&fixture.Payload.CurrentPayload.CollectSelectedByte==0);
                if(effectiveKind==11&&count==0&&residual>0)check(fixture.Payload.CurrentPayload.ResidualSubstateValue==2&&(Has(fixture.Trace,"residualstate")||Has(fixture.Trace,"arm:792:0")));
                if(effectiveKind==15)check(Has(fixture.Trace,"hand:995"));
                if(effectiveKind==16&&count==0)check(Has(fixture.Trace,"hand:"+residual));
                if(effectiveKind==18)check(Has(fixture.Trace,"message:Call_Attendant:Pay_With_Currency"));
                if(effectiveKind==25&&count>0)check(Has(fixture.Trace,"ma:264:100"));
                fixture.State.ValidateStorage();fixture.Payload.ValidateStorage();var expectedPayload=fixture.Payload.PayloadBytes;var sd=fixture.StateDevice;var pd=fixture.DataDevice;fixture.Dispose();
                using(var cold=new OriginalPersistentState()){cold.Open(()=>sd,false,message=>{},false,null);check(cold.Value==expected&&cold.Cursor==0&&cold.ReferenceCount==1);}
                using(var cold=new OriginalHandpaySelectionPersistence(pd,new byte[135],message=>{})){check(Equal(cold.PayloadBytes,expectedPayload));cold.ValidateStorage();}++cases;
            }
            using(var f=new Fixture(0,0,0,0,false,false)){f.Game=1;check(f.Flow.Collect(995,false,null));check(f.State.Value==6&&f.Payload.CurrentPayload.CashOutType==15&&f.Payload.CurrentPayload.CashOutAmount==995&&f.Payload.CurrentPayload.CollectSelectedByte==1);check(f.Trace[0]=="D:0"&&f.State.ReferenceCount==1&&f.State.PayloadBytes[2]==2);f.State.ValidateStorage();f.Payload.ValidateStorage();}
            using(var f=new Fixture(0,0,0,0,false,false)){check(f.Flow.Collect(995,true,null));check(f.State.Value==6&&Has(f.Trace,"hand:995")&&f.State.ReferenceCount==1);}
            // Failed event preserves the substate creation, but no substate1/2,
            // manager5/6 or downstream body is fabricated; cold state resumes4.
            using(var f=new Fixture(4,15,0,0,false,false))
            {f.EventFault=true;bool fault=false;try{f.Flow.Finish();}catch(InvalidOperationException e){fault=e.Message=="event fault";}check(fault&&f.State.ReferenceCount==1&&f.State.Value==4&&f.State.PayloadBytes[0]==2&&f.State.PayloadBytes[2]==0&&f.StateSaves.Count==1);f.State.ValidateStorage();}
            // Native substate assignment fails before clearing/saving blocked NV.
            using(var f=new Fixture(4,15,0,0,true,false))
            {int commits=0;f.StateDevice.Commit=b=>{++commits;f.StateDevice.FailWrite=true;};bool fault=false;try{f.Flow.Finish();}catch(OriginalNvFault){fault=true;}check(fault&&commits==1&&f.State.ReferenceCount==1&&f.Payload.CurrentPayload.ExternalTransferBlockedByte==7&&f.DataSaves.Count==0);f.StateDevice.FailWrite=false;f.State.Reload();check(f.State.Value==4&&f.State.PayloadBytes[2]==0);}
            using(var f=new Fixture(4,15,0,0,false,false))
            {check(f.Flow.Finish());check(string.Join("|",f.Trace)=="S:2:4:0|event:44|S:2:4:1|game4c|S:2:4:2|gamee0|event:185|S:1:5:2|S:1:6:2|gamee0|tiltsnapshot|jf:1092|cf:388|hand:995");}
            using(var f=new Fixture(4,14,0,0,false,false))
            {f.TicketResult=0;check(f.Flow.Finish());check(string.Join("|",f.Trace)=="S:2:4:0|event:44|S:2:4:1|game4c|S:2:4:2|S:1:5:2|ticket:14:995|creditin34|S:1:6:2|ma:268:995|game:556|D:0|S:1:0:2");}
            if(Directory.Exists(directory))throw new Exception("Flow file directory must be fresh");Directory.CreateDirectory(directory);
            using(var f=new Fixture(4,15,0,0,false,false)){f.Flow.Finish();File.WriteAllBytes(Path.Combine(directory,"state.bin"),f.State.DeviceImage);File.WriteAllBytes(Path.Combine(directory,"cashout.bin"),f.Payload.ReadDeviceImage());}
            using(var state=OriginalPersistentState.OpenFile(Path.Combine(directory,"state.bin"),message=>{},false,null))using(var payload=OriginalHandpaySelectionPersistence.OpenFile(Path.Combine(directory,"cashout.bin"),new byte[135],message=>{})){check(state.Value==6&&state.PayloadBytes[2]==2&&payload.CurrentPayload.CashOutType==15);state.ValidateStorage();payload.ValidateStorage();}
            UnityEngine.Debug.Log("BOUND_CREDITOUT_FLOW_VALIDATED cases="+cases+" checks="+checks);return checks;
        }
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
    }
}
