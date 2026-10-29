using System;
namespace RouletteDeluxe
{
    // Legacy082017b0 state5. Ticket operations remain real caller-owned payment bodies.
    public static class OriginalCreditOutCashoutVoucherState
    {
        public sealed class Host
        {
            public Func<int,long,int> PayTicket;
            public Func<int,bool> MachineFlag,JurisdictionFlag,ControlFlag;
            public Func<int,long> MachineAmount;
            public Action<long> GameOperation64;
            public Action CreditInOperation34,CashOutContinueState;
            public Action<int> AssignManagerState;
            public Action<byte[],uint> Emit;
            internal Host CopyBindings() { return (Host)MemberwiseClone(); }
            internal void Validate()
            {
                if(PayTicket==null||MachineFlag==null||JurisdictionFlag==null||ControlFlag==null||MachineAmount==null||GameOperation64==null||
                    CreditInOperation34==null||CashOutContinueState==null||AssignManagerState==null||Emit==null)throw new ArgumentException("Complete CashOutVoucherState host required");
            }
        }
        private static void Setup(OriginalHandpaySelectionPersistence owner,Host h,int type)
        {owner.SetupSelectedCashOut(type,owner.CurrentPayload.CashOutAmount,()=>h.MachineAmount(0xb4),()=>h.MachineAmount(0x148));}
        public static bool Apply(OriginalHandpaySelectionPersistence owner,OriginalCreditOutCollect.Runtime runtime,Host h)
        {
            if(owner==null)throw new ArgumentNullException(nameof(owner));if(runtime==null)throw new ArgumentNullException(nameof(runtime));
            if(h==null)throw new ArgumentNullException(nameof(h));h.Validate();
            int type=owner.CurrentPayload.CashOutType;
            if(type==12||type==13)
            {
                if(h.PayTicket(type,owner.CurrentPayload.CashOutAmount)==3)goto Next;
                if(h.MachineFlag(0x218)&&owner.CurrentPayload.CashOutAmount<=h.MachineAmount(0x10c)&&h.ControlFlag(0xfc))
                {Setup(owner,h,11);h.GameOperation64(0);goto Next;}
                if(h.JurisdictionFlag(0x1f8)&&h.MachineFlag(0x218)&&h.MachineAmount(0x148)>0&&h.MachineAmount(0x148)<owner.CurrentPayload.CashOutAmount&&
                    h.MachineAmount(0x148)<=h.MachineAmount(0x10c)&&h.ControlFlag(0xfc))
                {Setup(owner,h,16);h.GameOperation64(0);goto Next;}
                goto Fallback;
            }
            if(type==14)
            {
                int result=h.PayTicket(14,owner.CurrentPayload.CashOutAmount);
                runtime.Field29A=(byte)(result==3?0:1);if(result!=3)h.CreditInOperation34();goto Next;
            }
            if(type==25)
            {
                if(h.PayTicket(12,h.MachineAmount(0x148))==3)goto Next;
                if(h.MachineFlag(0x218))
                {
                    if(owner.CurrentPayload.CashOutAmount>h.MachineAmount(0x10c)&&h.MachineAmount(0x148)>h.MachineAmount(0x10c))goto Fallback;
                    if(h.MachineAmount(0x148)%h.MachineAmount(0xb4)!=0)goto Fallback;
                    if(h.ControlFlag(0xfc)){Setup(owner,h,16);h.GameOperation64(0);goto Next;}
                }
                goto Fallback;
            }
            goto Next;
        Fallback:
            Setup(owner,h,15);h.Emit(OriginalCreditOutCashoutState.Tilt(0x15600,0x20,owner.CurrentPayload.CashOutAmount),0);
        Next:
            h.AssignManagerState(6);h.CashOutContinueState();return true;
        }
    }
}
