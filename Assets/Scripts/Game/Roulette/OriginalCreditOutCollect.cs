using System;
namespace RouletteDeluxe
{
    // Legacy Collect081fd830. Host owns State, events, device/configuration and
    // FinishStates. Interface offsets retain identities without guessed settings.
    public static class OriginalCreditOutCollect
    {
        public sealed class Runtime
        {
            public byte FinishPendingByte,Field29A,Field3AB,Field3AA,Field23C,Field298,Field299;
            public int Field2C8;
            public long MinimumCollectAmount;
            public uint ManagerEventField,ResidualEventField1C,ResidualEventField78;
        }
        public sealed class Host
        {
            public OriginalCreditOutCashoutSelection.Host Priority;
            public Action ArmFinishTrigger,CreditInOperation3C,FinishStates;
            public Func<int> ReadManagerState;
            public Action<int> AssignManagerState;
            public Func<int> ReadGameFieldE0;
            public Func<int,bool> ManagerFlag,GlobalCreditOutFlag;
            public Action<int,bool> GlobalCreditOutOperation;
            public Action<int,long> ManagerAmountOperation;
            public Func<int,long> CreditAmount,ExternalAmount;
            public Func<long,bool> ExternalCollect;
            public Func<bool> CombineCashable;
            public Func<int,int> MachineInt;
            public Action<byte[],uint> Emit;
            internal Host CopyBindings() { return (Host)MemberwiseClone(); }
            internal void Validate()
            {
                if(Priority==null||ArmFinishTrigger==null||CreditInOperation3C==null||FinishStates==null||ReadManagerState==null||
                    AssignManagerState==null||ReadGameFieldE0==null||ManagerFlag==null||GlobalCreditOutFlag==null||GlobalCreditOutOperation==null||
                    ManagerAmountOperation==null||CreditAmount==null||ExternalAmount==null||ExternalCollect==null||CombineCashable==null||MachineInt==null||Emit==null)
                    throw new ArgumentException("Complete legacy Collect host required");
            }
        }
        private static bool Finish(OriginalHandpaySelectionPersistence owner,Host h,int type,long amount)
        {
            owner.SetupSelectedCashOut(type,amount,()=>h.Priority.MachineAmount(0xb4),()=>h.Priority.MachineAmount(0x148));
            h.AssignManagerState(4);h.FinishStates();return true;
        }
        private static void Put(byte[] bytes,int offset,ulong value,int size)
        {for(int i=0;i<size;++i)bytes[offset+i]=(byte)(value>>(i*8));}
        private static void BelowMinimum(Host h,uint field,long amount)
        {
            var first=OriginalHandpaySelectionEvents.Residual(field,amount);h.Emit(first,0);
            var second=new byte[44];Put(second,0,0x1000,2);Put(second,2,44,2);Put(second,0x12,0x20,2);
            Put(second,0x18,0x6010,4);Put(second,0x1c,unchecked((ulong)amount),8);h.Emit(second,0);
        }
        public static bool Apply(OriginalHandpaySelectionPersistence owner,Runtime runtime,Host h,long amount,bool forced,object originalVoucher)
        {
            if(owner==null)throw new ArgumentNullException(nameof(owner));if(runtime==null)throw new ArgumentNullException(nameof(runtime));
            if(h==null)throw new ArgumentNullException(nameof(h));h.Validate();
            if(runtime.FinishPendingByte!=0){h.ArmFinishTrigger();return false;}
            owner.CurrentPayload.CollectSelectedByte=1;owner.SaveCurrentPayload();
            h.GlobalCreditOutOperation(0xd4,false);h.CreditInOperation3C();
            if(h.ReadManagerState()!=0){h.FinishStates();return true;}
            if(h.ReadGameFieldE0()>0)return Finish(owner,h,0x0f,amount);
            if(amount<runtime.MinimumCollectAmount){BelowMinimum(h,runtime.ManagerEventField,amount);return false;}
            if(!h.ManagerFlag(0xf4))return false;
            long external=h.ExternalAmount(0x2c),remaining=unchecked(amount-external);
            long nonCash=h.CreditAmount(0x18),cash=h.CreditAmount(0x1c),separate=h.CreditAmount(0x20);
            if(h.CombineCashable()){cash=unchecked(cash+separate-external);separate=0;}
            else {separate=unchecked(h.CreditAmount(0x1c)-external);cash=unchecked(h.CreditAmount(0x20)-external);}
            if(forced)
            {
                int forcedType=(h.Priority.MachineCheckAmount(0x328,remaining)&&h.Priority.MachineFlag(0x140))||!h.Priority.MachineFlag(0x1bc)?0x0f:0x0c;
                return Finish(owner,h,forcedType,remaining);
            }
            nonCash=unchecked(nonCash-external);int type=0;long candidate=remaining;
            if(!h.GlobalCreditOutFlag(0x24))
            {
                if(cash==0&&separate==0&&nonCash>0){h.ManagerAmountOperation(0x10c,nonCash);return false;}
                type=0x0f;candidate=unchecked(cash+separate);
            }
            else if(!h.ManagerFlag(0xb0)&&owner.CurrentPayload.ExternalTransferBlockedByte==0&&h.Priority.MachineFlag(0x2c8)&&
                (!h.Priority.MachineFlag(0x2fc)||remaining<=h.Priority.MachineAmount(0x2f4)))
            {
                owner.CurrentPayload.ExternalTransferState=2;owner.CurrentPayload.CashOutAmount=remaining;owner.SaveCurrentPayload();
                if(h.ExternalCollect(remaining))type=0x14;
                owner.CurrentPayload.ExternalTransferState=0;owner.SaveCurrentPayload();
            }
            if(h.CombineCashable()||separate<=0)goto CheckNonCash;
            if(type!=0)goto CheckCandidate;
            if(h.MachineInt(0x1fc)!=2){candidate=cash;goto CheckNonCash;}
            if((h.Priority.MachineCheckAmount(0x328,separate)&&h.Priority.MachineFlag(0x140))||!h.Priority.MachineFlag(0x1bc)||
                (h.ManagerFlag(0xb0)&&cash>0))
            {candidate=unchecked(separate+cash);goto CheckNonCash;}
            if(h.MachineInt(0x20c)==2&&cash!=0)
            {if(h.MachineInt(0x20c)==2)candidate=cash;goto CheckNonCash;}
            return Finish(owner,h,0x0d,separate);
        CheckNonCash:
            if(type==0&&nonCash>0)
            {
                if(h.MachineInt(0x1e4)==2)
                {
                    if(h.MachineInt(0x1ec)==2&&(!h.Priority.MachineCheckAmount(0x328,nonCash)||!h.Priority.MachineFlag(0x140))&&
                        h.Priority.MachineFlag(0x1bc)&&(runtime.Field29A==0||cash<1)&&(!h.ManagerFlag(0xb0)||cash<1))
                    {
                        if(h.MachineInt(0x20c)!=2||cash==0)return Finish(owner,h,0x0e,nonCash);
                        if(h.MachineInt(0x20c)==2)candidate=cash;
                    }
                    else if(separate==0)
                    {
                        candidate=cash;if(cash==0){h.ManagerAmountOperation(0x10c,nonCash);runtime.Field29A=0;return false;}
                    }
                }
                else candidate=cash;
            }
        CheckCandidate:
            if(candidate<1){runtime.Field29A=0;return false;}
            if(type==0)type=h.Priority.MachineFlag(0x390)?OriginalCreditOutCashoutSelection.HopperPrimary(h.Priority,candidate,originalVoucher)
                :OriginalCreditOutCashoutSelection.PrinterPrimary(h.Priority,candidate,originalVoucher);
            return Finish(owner,h,type,candidate);
        }
    }
}
