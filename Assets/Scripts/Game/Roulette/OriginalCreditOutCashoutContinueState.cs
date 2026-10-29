using System;
using System.Globalization;
namespace RouletteDeluxe
{
    // Original08200b30; actual State, hardware, translator and screen owners are mandatory.
    public static class OriginalCreditOutCashoutContinueState
    {
        public sealed class Host
        {
            public Action<int> GameOperation,AssignManagerState;
            public Action<int,bool> MachineOperation,ManagerBoolOperation;
            public Action<int,long> ManagerAmountOperation;
            public Func<int> ReadGameFieldE0;
            public Func<int,bool> MachineFlag,JurisdictionFlag,ControlFlag;
            public Func<int,long> MachineAmount,CreditAmount;
            public Func<int,int> MachineInt;
            public Func<long> ExternalAmount2C;
            public Func<byte> TiltSnapshotByte2;
            public Func<uint,bool> TiltActive;
            public Func<bool> CoinOutReady,ExternalFlag68;
            public Func<long,long,bool> ExternalOperation10;
            public Action<long> CoinOutOperation8,DisplayHandPay;
            public Action CoinInOperation4,FinishStates;
            public Action<int,uint> ArmTrigger;
            public Func<string,string[],bool,string> Translate;
            public Action<string,string> DisplayMessage;
            public Func<string,string,object> CreateInfoScreen;
            public Action<object> PublishInfoScreen,DisplayInfoScreen;
            public Action<object,uint> ArmInfoTrigger;
            public bool SoftValidationEnabled;
            public Action<string> NvDeviceContextLog;
            public Action<byte[],uint> Emit;
            internal Host CopyBindings() { return (Host)MemberwiseClone(); }
            internal void Validate()
            {
                if(GameOperation==null||AssignManagerState==null||MachineOperation==null||ManagerBoolOperation==null||ManagerAmountOperation==null||
                  ReadGameFieldE0==null||MachineFlag==null||JurisdictionFlag==null||ControlFlag==null||MachineAmount==null||CreditAmount==null||MachineInt==null||
                  ExternalAmount2C==null||TiltSnapshotByte2==null||TiltActive==null||CoinOutReady==null||ExternalFlag68==null||ExternalOperation10==null||
                  CoinOutOperation8==null||DisplayHandPay==null||CoinInOperation4==null||FinishStates==null||ArmTrigger==null||Translate==null||DisplayMessage==null||
                  CreateInfoScreen==null||PublishInfoScreen==null||DisplayInfoScreen==null||ArmInfoTrigger==null||Emit==null||
                  (SoftValidationEnabled&&NvDeviceContextLog==null))throw new ArgumentException("Complete CashOutContinueState host required");
            }
        }
        public static bool DisplayRemainingPromotionalCredits(Host h)
        {
            long amount=unchecked(h.CreditAmount(0x18)-h.ExternalAmount2C());
            if(amount<=0||h.MachineInt(0x20c)!=2)return false;
            amount=unchecked(h.CreditAmount(0x18)-h.ExternalAmount2C());
            string body=h.Translate("Credits",new[]{amount.ToString(CultureInfo.InvariantCulture)},true);
            string title=h.Translate("Remaining_Promotional",new string[0],true);
            object screen=h.CreateInfoScreen(title,body);
            if(screen==null)throw new InvalidOperationException("Native InfoTextScreen owner missing");
            h.PublishInfoScreen(screen);h.DisplayInfoScreen(screen);h.ArmInfoTrigger(screen,5000);return true;
        }
        public static bool Apply(OriginalHandpaySelectionPersistence owner,OriginalCreditOutCollect.Runtime r,Host h)
        {
            if(owner==null)throw new ArgumentNullException(nameof(owner));if(r==null)throw new ArgumentNullException(nameof(r));
            if(h==null)throw new ArgumentNullException(nameof(h));h.Validate();
            if(unchecked((uint)(owner.CurrentPayload.ResidualSubstateValue-4))<2)
            {h.GameOperation(0x204);h.MachineOperation(0x19c,true);}
            switch(owner.CurrentPayload.CashOutType)
            {
                case 11:
                    if(owner.CurrentPayload.CashOutCoinCount<=0)
                    {
                        if(owner.SelectionAmount>0)
                        {
                            if(r.Field3AA!=0)h.ManagerBoolOperation(0xbc,true);
                            h.AssignManagerState(26);
                            if(owner.CurrentPayload.ResidualSubstateValue!=2){owner.CurrentPayload.ResidualSubstateValue=2;owner.SaveCurrentPayload();}
                            if(owner.CurrentPayload.Field1B8!=0)r.Field3AB=1;
                            else if(r.Field3AB==0){h.FinishStates();return true;}
                            h.ArmTrigger(0x318,0);return true;
                        }
                        if(r.Field3AA!=0)h.ManagerBoolOperation(0xbc,true);
                        DisplayRemainingPromotionalCredits(h);
                        owner.CurrentPayload.CashOutType=0;owner.CurrentPayload.CashOutAmount=0;owner.CurrentPayload.CashOutCoinCount=0;owner.CurrentPayload.SelectionAmount=0;
                        owner.SaveCurrentPayload();h.Emit(OriginalHandpaySelectionEvents.Residual(0x6070,0),0);h.AssignManagerState(0);return true;
                    }
                    owner.CurrentPayload.CollectSelectedByte=0;owner.SaveCurrentPayload();goto CoinOut;
                case 12:case 13:
                    h.ManagerAmountOperation(0x108,owner.CurrentPayload.CashOutAmount);h.ManagerBoolOperation(0xe0,false);return true;
                case 14:
                    if(r.Field29A==0)h.ManagerAmountOperation(0x108,owner.CurrentPayload.CashOutAmount);
                    else
                    {
                        h.ManagerAmountOperation(0x10c,owner.CurrentPayload.CashOutAmount);h.GameOperation(0x22c);r.Field29A=0;
                        owner.CurrentPayload.CashOutType=0;owner.CurrentPayload.CashOutAmount=0;owner.SaveCurrentPayload();h.AssignManagerState(0);
                    }
                    break;
                case 15:
                    if(h.ReadGameFieldE0()!=0)break;
                    byte tilt=h.TiltSnapshotByte2();
                    if(h.JurisdictionFlag(0x444)&&tilt==0&&h.CreditAmount(0)<=h.MachineAmount(0xfc)&&r.Field23C==0)
                    {
                        if(!h.MachineFlag(0x190)){if(r.Field2C8==0)h.ArmTrigger(0x384,0);return true;}
                        if(owner.CurrentPayload.InProgressValue!=0)
                        {
                            owner.SoftValidateStorage(h.SoftValidationEnabled,"CreditOutMgr.cpp::CashOutContinueState(6309)",h.NvDeviceContextLog);
                            owner.CurrentPayload.InProgressValue=0;owner.SaveCurrentPayload();
                        }
                        return true;
                    }
                    if(h.ControlFlag(0x184))h.CoinInOperation4();h.DisplayHandPay(owner.CurrentPayload.CashOutAmount);break;
                case 16:
                    if(owner.CurrentPayload.CashOutCoinCount<=0)goto ResidualHandpay;goto CoinOut;
                case 18:
                    if(owner.CurrentPayload.W2GDisplayedByte!=0)
                    {
                        string body=h.Translate("Pay_With_Currency",new[]{owner.CurrentPayload.CashOutAmount.ToString(CultureInfo.InvariantCulture)},true);
                        string title=h.Translate("Call_Attendant",new string[0],true);h.DisplayMessage(title,body);
                    }
                    break;
                case 20:
                    if(!h.ExternalFlag68()&&!h.TiltActive(0x6020)&&h.ExternalOperation10(owner.CurrentPayload.CashOutAmount,owner.CurrentPayload.CashOutAmount))h.ManagerBoolOperation(0xc4,false);
                    break;
                case 25:
                    if(owner.CurrentPayload.CashOutCoinCount!=0){h.ManagerAmountOperation(0x108,h.MachineAmount(0x148));return true;}
                    if(!h.TiltActive(0x15600))
                    {
                        long residual=owner.SelectionAmount;long processed=unchecked(owner.CurrentPayload.CashOutAmount-residual);
                        var packet=OriginalCreditOutCashoutState.Tilt(0x15600,0x20,residual);ulong raw=unchecked((ulong)processed);
                        for(int i=0;i<8;++i)packet[0x2c+i]=(byte)(raw>>(i*8));h.Emit(packet,0);
                    }
                    goto ResidualHandpay;
            }
            return true;
        CoinOut:
            if(h.CoinOutReady())h.CoinOutOperation8(owner.CurrentPayload.CashOutCoinCount);return true;
        ResidualHandpay:
            h.ManagerBoolOperation(0xbc,false);h.DisplayHandPay(owner.SelectionAmount);return true;
        }
    }
}
