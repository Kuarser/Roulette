using System;

namespace RouletteDeluxe
{
    // Raw NV2Type<CreditOutMgrNvram> data, not the NV2 device/container header.
    // The native writer streams exactly0x87 bytes from old manager+0x160.
    public sealed class OriginalCreditOutNvPayload
    {
        public const int Length=0x87, SelectionAmountOffset=0x24, InProgressOffset=0x48,
            SelectedOffset=0x5d, ResidualSubstateOffset=0x63;
        private readonly byte[] payload;
        public OriginalCreditOutNvPayload(byte[] source)
        { if(source==null||source.Length!=Length)throw new ArgumentException("Exact native135-byte payload required");payload=(byte[])source.Clone(); }
        public byte[] Bytes{get{return (byte[])payload.Clone();}}
        // DisplayW2GHandPayScreen writes manager+0x1b1 before its first Save.
        public byte W2GDisplayedByte { get { return payload[0x51]; } set { payload[0x51]=value; } }
        private uint Read32(int offset)
        { return (uint)payload[offset]|((uint)payload[offset+1]<<8)|((uint)payload[offset+2]<<16)|((uint)payload[offset+3]<<24); }
        private void Write32(int offset,int value)
        { uint raw=unchecked((uint)value);for(int i=0;i<4;++i)payload[offset+i]=(byte)(raw>>(i*8)); }
        private long Read64(int offset)
        { return unchecked((long)((ulong)Read32(offset)|((ulong)Read32(offset+4)<<32))); }
        private void Write64(int offset,long value)
        { Write32(offset,unchecked((int)value));Write32(offset+4,unchecked((int)(value>>32))); }
        public int CashOutType { get { return unchecked((int)Read32(0)); } internal set { Write32(0,value); } }
        public long CashOutAmount { get { return Read64(4); } internal set { Write64(4,value); } }
        public long CashOutCoinCount { get { return Read64(0x0c); } internal set { Write64(0x0c,value); } }
        public int ExternalTransferState { get { return unchecked((int)Read32(0x4c)); } internal set { Write32(0x4c,value); } }
        public byte ExternalTransferBlockedByte { get { return payload[0x55]; } internal set { payload[0x55]=value; } }
        public byte Field1B8 { get { return payload[0x58]; } }
        internal int InProgressValue { get { return unchecked((int)Read32(InProgressOffset)); } set { Write32(InProgressOffset,value); } }
        internal int ResidualSubstateValue { get { return unchecked((int)Read32(ResidualSubstateOffset)); } set { Write32(ResidualSubstateOffset,value); } }
        public byte CollectSelectedByte { get { return payload[SelectedOffset]; } internal set { payload[SelectedOffset]=value; } }
        public long SelectionAmount
        { get{return Read64(SelectionAmountOffset);} internal set { Write64(SelectionAmountOffset,value); } }
        public OriginalHandpaySelectionNvState ReadSelection()
        { return new OriginalHandpaySelectionNvState{InProgress=unchecked((int)Read32(InProgressOffset)),
            ResidualSubstate=unchecked((int)Read32(ResidualSubstateOffset)),HandpaySelectedByte=payload[SelectedOffset]}; }
        public void UpdateSelection(OriginalHandpaySelectionNvState state)
        {
            if(state==null)throw new ArgumentNullException("state");
            Write32(InProgressOffset,state.InProgress);payload[SelectedOffset]=state.HandpaySelectedByte;
            Write32(ResidualSubstateOffset,state.ResidualSubstate);
        }
        // Existing host remains responsible for native NV2 Write/flush/header.
        public Action<OriginalHandpaySelectionNvState> BindWriter(Action<byte[]> writePayload)
        {
            if(writePayload==null)throw new ArgumentNullException("writePayload");
            return state=>{UpdateSelection(state);writePayload(Bytes);};
        }
    }
    public static class OriginalCreditOutNvPayloadValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=value=>{if(!value)throw new Exception("CreditOut NV payload regression "+checks);++checks;};
            var bytes=new byte[OriginalCreditOutNvPayload.Length];for(int i=0;i<bytes.Length;++i)bytes[i]=(byte)(i*19+7);
            for(int i=0;i<8;++i)bytes[0x24+i]=(byte)(0xfedcba9876543210UL>>(8*i));
            bytes[0x48]=1;bytes[0x49]=bytes[0x4a]=bytes[0x4b]=0;bytes[0x5d]=0x7f;
            bytes[0x63]=3;bytes[0x64]=bytes[0x65]=bytes[0x66]=0;
            var payload=new OriginalCreditOutNvPayload(bytes);bytes[0]=0;
            check(payload.Bytes[0]==7&&payload.SelectionAmount==unchecked((long)0xfedcba9876543210UL));
            var state=payload.ReadSelection();check(state.InProgress==1&&state.ResidualSubstate==3&&state.HandpaySelected&&state.HandpaySelectedByte==0x7f);
            byte[] before=payload.Bytes;payload.UpdateSelection(state);check(Equal(before,payload.Bytes));
            state.InProgress=-1;state.ResidualSubstate=int.MinValue;state.HandpaySelected=true;payload.UpdateSelection(state);
            var after=payload.Bytes;check(after[0x48]==255&&after[0x4b]==255&&after[0x5d]==1&&after[0x66]==128);
            for(int i=0;i<before.Length;++i)if(!((i>=0x48&&i<0x4c)||i==0x5d||(i>=0x63&&i<0x67)))
                check(before[i]==after[i]);
            int writes=0;var writer=payload.BindWriter(data=>{++writes;data[0]=0;});writer(state);
            check(writes==1&&payload.Bytes[0]==7);
            bool rejected=false;try{new OriginalCreditOutNvPayload(new byte[134]);}catch(ArgumentException){rejected=true;}check(rejected);
            return checks;
        }
        private static bool Equal(byte[] a,byte[] b){for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
    }
}
