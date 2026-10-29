using System;
namespace RouletteDeluxe
{
    // FinishStates state4 target08202150. Caller owns the original NewSubState,
    // State assignment/copy and CashOutVoucherState; no fake completion here.
    public static class OriginalCreditOutCashoutState
    {
        public sealed class Host
        {
            public Func<int> ReadSubState,ReadGameFieldE0;
            public Action<int> AssignSubState,AssignManagerState;
            public Func<bool> ExternalFlag80;
            public Action<long,bool> GameOperation4C;
            public Action<long> GameOperation64;
            public Action<byte[],uint> Emit;
            public Action CashOutVoucherState;
            internal Host CopyBindings() { return (Host)MemberwiseClone(); }
            internal void Validate()
            {
                if(ReadSubState==null||ReadGameFieldE0==null||AssignSubState==null||AssignManagerState==null||ExternalFlag80==null||
                    GameOperation4C==null||GameOperation64==null||Emit==null||CashOutVoucherState==null)throw new ArgumentException("Complete CashOutState host required");
            }
        }
        internal static byte[] Tilt(uint code,ushort size,long amount)
        {
            var b=new byte[185];b[1]=1;b[2]=185;b[0x12]=(byte)size;b[0x13]=(byte)(size>>8);
            for(int i=0;i<4;++i)b[0x18+i]=(byte)(code>>(i*8));ulong v=unchecked((ulong)amount);for(int i=0;i<8;++i)b[0x1c+i]=(byte)(v>>(i*8));return b;
        }
        public static bool Apply(OriginalHandpaySelectionPersistence owner,OriginalCreditOutCollect.Runtime runtime,Host h)
        {
            if(owner==null)throw new ArgumentNullException(nameof(owner));if(runtime==null)throw new ArgumentNullException(nameof(runtime));
            if(h==null)throw new ArgumentNullException(nameof(h));h.Validate();
            // NewSubState creation/copy is a caller-owned native State boundary.
            if(owner.CurrentPayload.ExternalTransferBlockedByte!=0&&!h.ExternalFlag80())
            {h.AssignSubState(2);owner.CurrentPayload.ExternalTransferBlockedByte=0;owner.SaveCurrentPayload();}
            if(owner.SelectionAmount>0&&owner.CurrentPayload.CashOutType==11&&owner.CurrentPayload.ResidualSubstateValue!=1)
            {owner.CurrentPayload.ResidualSubstateValue=1;owner.SaveCurrentPayload();}
            int sub=h.ReadSubState();
            if(sub==0)
            {h.Emit(OriginalHandpaySelectionEvents.Residual(runtime.ManagerEventField,owner.CurrentPayload.CashOutAmount),0);h.AssignSubState(1);}
            if(sub==0||sub==1){h.GameOperation4C(0,true);h.AssignSubState(2);}
            else if(sub!=2)goto Next;
            int type=owner.CurrentPayload.CashOutType;
            if(type==15)
            {if(h.ReadGameFieldE0()==0)h.Emit(Tilt(0x15600,0x20,owner.CurrentPayload.CashOutAmount),0);}
            else if(type==18)h.Emit(Tilt(0x15700,8,owner.CurrentPayload.CashOutAmount),0);
            else if(type==11||type==16)
            {
                h.GameOperation64(0);
                if(owner.CurrentPayload.CashOutType==11&&owner.CurrentPayload.Field1B8!=0&&owner.SelectionAmount>0)runtime.Field3AB=1;
            }
        Next:
            h.AssignManagerState(5);h.CashOutVoucherState();return true;
        }
    }
}
