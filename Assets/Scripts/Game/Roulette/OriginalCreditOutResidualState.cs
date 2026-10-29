using System;
namespace RouletteDeluxe
{
    // Legacy081f8ca0 and four bodies. Real native UI/MGS/Game/Credit owners
    // remain mandatory; residual selection never completes a financial transfer.
    public static class OriginalCreditOutResidualState
    {
        public sealed class Host
        {
            public Func<int,bool> MachineFlag;
            public Func<int,int> MachineInt;
            public Func<bool> GameFlag370,MGSScreenDisplayed;
            public Func<long> CreditAmount0;
            public Action<int> GameOperation,AssignManagerState;
            public Action<int,bool> MachineOperation;
            public Action<long> GameOperation1F0,DisplayCashoutFailed;
            public Action DisplayOptionHandpayCancel;
            public Action<int,uint> ArmTrigger;
            public Action<byte[],uint> Emit;
            internal Host CopyBindings(){return (Host)MemberwiseClone();}
            internal void Validate()
            {
                if(MachineFlag==null||MachineInt==null||GameFlag370==null||MGSScreenDisplayed==null||CreditAmount0==null||GameOperation==null||AssignManagerState==null||
                   MachineOperation==null||GameOperation1F0==null||DisplayCashoutFailed==null||DisplayOptionHandpayCancel==null||ArmTrigger==null||Emit==null)
                    throw new ArgumentException("Complete original residual owners required");
            }
        }
        private static byte[] Event(uint code,long amount,bool active,ushort subtype)
        {var b=OriginalHandpaySelectionEvents.Residual(code,amount);b[6]=(byte)(active?1:0);b[18]=(byte)subtype;b[19]=(byte)(subtype>>8);return b;}
        private static void Emit(Host h,byte[] packet){h.Emit(packet,0);}
        public static bool Apply(OriginalHandpaySelectionPersistence owner,OriginalCreditOutCollect.Runtime runtime,Host h)
        {
            if(owner==null||runtime==null||h==null)throw new ArgumentNullException();h.Validate();
            switch(owner.CurrentPayload.ResidualSubstateValue)
            {
                case 2:Option(owner,runtime,h,false);break;
                case 4:Handpay(owner,h);break;
                case 5:Ticket(owner,h);break;
                case 8:h.ArmTrigger(0x360,100);break;
                case 9:End(owner,h);h.AssignManagerState(0);break;
            }
            h.ArmTrigger(0x318,0);return false; // Native method returns0; FinishStates returns1.
        }
        private static void Setup(OriginalHandpaySelectionPersistence owner,int kind,long amount)
        {owner.SetupSelectedCashOut(kind,amount,()=>{throw new Exception("Residual Setup must not read coin");},()=>{throw new Exception("Residual Setup must not read limit");});}
        internal static bool Option(OriginalHandpaySelectionPersistence owner,OriginalCreditOutCollect.Runtime r,Host h,bool deferModal)
        {
            if(owner.SelectionAmount>0&&owner.SelectionAmount==owner.CurrentPayload.CashOutAmount&&!h.MachineFlag(0x188))
            {
                bool win=owner.CurrentPayload.CashOutType==2;var packet=Event(win?r.ResidualEventField78:r.ResidualEventField1C,owner.SelectionAmount,false,0);
                packet[0]=0x26;packet[1]=4;packet[6]=(byte)(win?2:1);Emit(h,packet);
                if(!win)Emit(h,Event(0x6070,0,false,0));
            }
            if(h.MachineFlag(0x188))
            {
                if((h.MachineInt(0x204)&8)!=0){if(owner.CurrentPayload.ResidualSubstateValue!=5){owner.CurrentPayload.ResidualSubstateValue=5;owner.SaveCurrentPayload();}}
                else SelectHandpay(owner);
            }
            else if(h.MachineInt(0x204)==1)
            {if(owner.CurrentPayload.ResidualSubstateValue!=9){owner.CurrentPayload.ResidualSubstateValue=9;owner.SaveCurrentPayload();}}
            else if(h.MachineInt(0x204)==3){if(deferModal)return true;h.DisplayOptionHandpayCancel();}
            else if((h.MachineInt(0x204)&4)==0)SelectHandpay(owner);
            else
            {
                owner.CurrentPayload.CashOutType=0;owner.SaveCurrentPayload();
                bool mgs=h.GameFlag370()&&h.MGSScreenDisplayed();
                if(mgs)h.GameOperation(0x48c);
                else if(r.Field3AB!=0&&owner.CurrentPayload.ResidualSubstateValue!=3)
                {h.GameOperation1F0(owner.SelectionAmount);if(owner.CurrentPayload.ResidualSubstateValue!=3){owner.CurrentPayload.ResidualSubstateValue=3;owner.SaveCurrentPayload();}}
                h.AssignManagerState(0);
            }
            r.Field3AB=0;return false;
        }
        private static void SelectHandpay(OriginalHandpaySelectionPersistence owner)
        {if(owner.CurrentPayload.ResidualSubstateValue!=4){owner.CurrentPayload.CollectSelectedByte=1;owner.CurrentPayload.ResidualSubstateValue=4;owner.SaveCurrentPayload();}}
        private static void Handpay(OriginalHandpaySelectionPersistence owner,Host h)
        {
            h.MachineOperation(0x19c,true);long amount=owner.SelectionAmount!=0?owner.SelectionAmount:owner.CurrentPayload.CashOutAmount;Setup(owner,15,amount);
            bool active=false;if(h.MachineInt(0x204)!=1&&h.MachineInt(0x204)!=3){active=true;Emit(h,Event(0x6086,h.CreditAmount0(),true,1));}
            owner.SaveCurrentPayload();Emit(h,Event(0x6080,owner.CurrentPayload.CashOutAmount,active,1));Emit(h,OriginalCreditOutCashoutState.Tilt(0x15600,32,owner.CurrentPayload.CashOutAmount));h.AssignManagerState(6);
        }
        private static void Ticket(OriginalHandpaySelectionPersistence owner,Host h)
        {
            h.GameOperation(0x204);h.MachineOperation(0x19c,true);long amount=owner.SelectionAmount!=0?owner.SelectionAmount:owner.CurrentPayload.CashOutAmount;Setup(owner,12,amount);
            Emit(h,Event(0x6086,h.CreditAmount0(),true,1));owner.SaveCurrentPayload();h.AssignManagerState(5);
        }
        private static void End(OriginalHandpaySelectionPersistence owner,Host h)
        {
            if(owner.SelectionAmount>0&&h.MachineInt(0x204)==1)h.DisplayCashoutFailed(owner.SelectionAmount);
            bool subtype=false;if(h.MachineInt(0x204)!=1&&h.MachineInt(0x204)!=3){subtype=true;Emit(h,Event(0x6086,h.CreditAmount0(),true,1));}
            owner.CurrentPayload.ResidualSubstateValue=0;owner.CurrentPayload.CashOutType=0;owner.CurrentPayload.CashOutAmount=0;owner.CurrentPayload.CashOutCoinCount=0;owner.CurrentPayload.SelectionAmount=0;owner.SaveCurrentPayload();
            Emit(h,Event(0x6070,0,false,(ushort)(subtype?1:0)));h.MachineOperation(0x19c,false);
        }
    }
}
