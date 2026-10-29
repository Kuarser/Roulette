using System;
using System.Globalization;

namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectionText
    {
        public sealed class Content
        { public string FontLanguage; public byte[] AmountLine, OptionLine, Yes, No; }
        public static byte[] FirstSpaceToNewline(byte[] input)
        {
            if(input==null)throw new ArgumentNullException("input");
            var output=(byte[])input.Clone();
            for(int i=0;i<output.Length&&output[i]!=0;++i)if(output[i]==32){output[i]=10;break;}
            return output;
        }
        // Amount is the original CreditOutMgr184/188 selection value. Host
        // supplies jurisdiction448, selected language and exact byte encoding.
        public static Content Build(OriginalLanguageTranslator translator,long selectionAmount,string selectedLanguage,
            bool jurisdictionAllowsLanguage,Func<long,string> formatCurrency,Func<string,byte[]> encode,Action unavailable)
        {
            if(translator==null||encode==null)throw new ArgumentNullException();
            Func<string,string[],byte[]> translate=(key,args)=>encode(translator.Translate(key,selectedLanguage,
                !jurisdictionAllowsLanguage,args,formatCurrency,null,unavailable));
            var empty=new string[0];
            return new Content{FontLanguage=jurisdictionAllowsLanguage?selectedLanguage:"en",
                AmountLine=translate("cannot_be_paid_from_the_hopper",new[]{selectionAmount.ToString(CultureInfo.InvariantCulture)}),
                OptionLine=translate("Please_touch_an_option",empty),
                Yes=OriginalVerifyButtonText.Copy(FirstSpaceToNewline(translate("Hand_Pay",empty))),
                No=OriginalVerifyButtonText.Copy(FirstSpaceToNewline(translate("Continue_Game_Play",empty)))};
        }
    }
    public sealed class OriginalHandpaySelectionNvState
    {
        public int InProgress, ResidualSubstate;
        public byte HandpaySelectedByte;
        public bool HandpaySelected{get{return HandpaySelectedByte!=0;}set{HandpaySelectedByte=(byte)(value?1:0);}}
        public OriginalHandpaySelectionNvState Copy()
        { return new OriginalHandpaySelectionNvState{InProgress=InProgress,ResidualSubstate=ResidualSubstate,HandpaySelectedByte=HandpaySelectedByte}; }
    }
    // Selection stage only. Does not mint payment receipts or complete money.
    // Host callbacks own original receiver/NV/event operations.
    public sealed class OriginalHandpaySelectionCoordinator
    {
        public sealed class Host
        {
            public Action BeforeResidualSelection, PrepareScreen, ReleaseScreenAndTiltEvents, ResidualGamble, AfterResidualGamble, EmitResidualEvent;
            public Action<bool> SetTransactionInProgress;
            public Action<OriginalHandpaySelectionNvState> Save;
            public Func<bool> MachineDisabled;
            internal void Validate()
            {
                if(BeforeResidualSelection==null||PrepareScreen==null||ReleaseScreenAndTiltEvents==null||ResidualGamble==null||
                    AfterResidualGamble==null||EmitResidualEvent==null||SetTransactionInProgress==null||Save==null||MachineDisabled==null)
                    throw new ArgumentException("All original selection host operations must be bound");
            }
        }
        public readonly OriginalHandpaySelectionNvState State;
        private readonly Host host;
        public bool ScreenPresent{get;private set;}
        public const int NativeScreenOrder=0x23a5;
        public OriginalHandpaySelectionCoordinator(OriginalHandpaySelectionNvState state,Host boundHost)
        { State=state??throw new ArgumentNullException("state");host=boundHost??throw new ArgumentNullException("boundHost");host.Validate(); }
        private void Save(){host.Save(State.Copy());}
        public bool Begin()
        {
            if(ScreenPresent)return false;
            host.BeforeResidualSelection();host.SetTransactionInProgress(true);host.PrepareScreen();ScreenPresent=true;
            State.InProgress=1;Save();
            if(State.ResidualSubstate!=3){State.ResidualSubstate=3;Save();}
            return true;
        }
        public bool Complete(int displayResult)
        {
            if(!ScreenPresent)throw new InvalidOperationException("Selection screen is not active");
            State.InProgress=0;Save();host.ReleaseScreenAndTiltEvents();ScreenPresent=false;
            bool selected=host.MachineDisabled()||displayResult!=0;
            if(!selected)
            {
                if(State.ResidualSubstate!=9){State.ResidualSubstate=9;Save();}
                host.ResidualGamble();host.AfterResidualGamble();host.EmitResidualEvent();
            }
            else
            {
                State.HandpaySelected=true;Save();
                if(State.ResidualSubstate!=4){State.HandpaySelected=true;State.ResidualSubstate=4;Save();}
            }
            return selected;
        }
    }
}
