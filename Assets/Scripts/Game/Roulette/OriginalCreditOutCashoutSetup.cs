using System;

namespace RouletteDeluxe
{
    // Legacy SetupNvDataForSelectedCashOutType, ELF VA081e65c0.
    // Callbacks read the current MachineData values on every native read.
    public static class OriginalCreditOutCashoutSetup
    {
        public static void Apply(OriginalCreditOutNvPayload payload,int type,long amount,
            Func<long> readCoinValue,Func<long> readLimit,Action save)
        {
            if(payload==null)throw new ArgumentNullException(nameof(payload));
            if(readCoinValue==null)throw new ArgumentNullException(nameof(readCoinValue));
            if(readLimit==null)throw new ArgumentNullException(nameof(readLimit));
            if(save==null)throw new ArgumentNullException(nameof(save));
            payload.CashOutType=type;payload.CashOutAmount=amount;
            switch(type)
            {
                case 0x0b:
                    payload.CashOutCoinCount=amount/readCoinValue();
                    payload.SelectionAmount=amount%readCoinValue();
                    break;
                case 0x0c:case 0x0d:case 0x0e:case 0x0f:case 0x14:
                    payload.CashOutCoinCount=0;payload.SelectionAmount=0;break;
                case 0x10:
                    payload.CashOutCoinCount=readLimit()/readCoinValue();
                    payload.SelectionAmount=unchecked(amount-payload.CashOutCoinCount*readCoinValue());
                    break;
                case 0x12:
                    payload.CashOutCoinCount=0;payload.SelectionAmount=0;payload.W2GDisplayedByte=1;break;
                case 0x19:
                    payload.CashOutCoinCount=readLimit();
                    payload.SelectionAmount=unchecked(amount-readLimit());break;
                // Unrecognized native enum values retain both existing amount fields.
            }
            save();
        }
    }
}
