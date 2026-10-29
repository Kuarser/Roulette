using System;
namespace RouletteDeluxe
{
    // Native081fced0/082031d0 dispatch. Handler bodies remain genuine caller
    // operations; every State argument copy shares the real persistent NV owner.
    public static class OriginalCreditOutFinishStates
    {
        public sealed class Host
        {
            public OriginalPersistentState ManagerState;
            public OriginalHandpaySelectionPersistence PayloadOwner;
            public OriginalCreditOutCollect.Runtime Runtime;
            public Action<int,OriginalPersistentState> StateOperation;
            public Action<long> DisplayHandPay,DisplayTicketReceipt;
            public Action ProcessResidualGamble;
            internal Host CopyBindings() { return (Host)MemberwiseClone(); }
            internal void Validate()
            {
                if(ManagerState==null||PayloadOwner==null||Runtime==null||StateOperation==null||DisplayHandPay==null||DisplayTicketReceipt==null||ProcessResidualGamble==null)
                    throw new ArgumentException("Complete original FinishStates owner and handlers required");
            }
        }
        private static void Call(Host h,int state,OriginalPersistentState input)
        {using(var copy=input.Copy())h.StateOperation(state,copy);}
        public static bool Apply(OriginalPersistentState input,Host h)
        {
            if(input==null)throw new ArgumentNullException(nameof(input));if(h==null)throw new ArgumentNullException(nameof(h));h.Validate();
            int state=input.Value;
            switch(state)
            {
                case 3:
                    using(var copy=input.Copy())h.DisplayHandPay(h.PayloadOwner.CurrentPayload.CashOutAmount);return true;
                case 26:h.ProcessResidualGamble();return true;
                case 27:
                    long amount=h.PayloadOwner.SelectionAmount;
                    h.DisplayTicketReceipt(amount>0?amount:h.PayloadOwner.CurrentPayload.CashOutAmount);return true;
                case 13:case 14:case 17:
                    Call(h,state,input);h.ManagerState.Assign(h.PayloadOwner.CurrentPayload.CashOutType!=0?27:0);return true;
                case 8:case 9:case 10:case 15:case 16:case 21:case 25:case 28:case 29:case 32:
                    Call(h,state,input);h.ManagerState.Assign(0);return true;
                case 30:case 31:
                    if(h.Runtime.Field299!=0&&h.Runtime.Field298==0)Call(h,state,input);
                    h.ManagerState.Assign(0);return true;
                case 1:case 2:case 4:case 5:case 6:case 7:case 11:case 12:case 18:case 19:case 20:case 23:case 24:
                    Call(h,state,input);return true;
                // Native0,22 and all unknown byte values return1 without operation.
                default:return true;
            }
        }
    }
}
