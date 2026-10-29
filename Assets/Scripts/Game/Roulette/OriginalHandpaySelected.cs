using System;
using System.Globalization;
namespace RouletteDeluxe
{
    // Native081f4130 body only. Original081f9110 owns Game/Machine operations,
    // selected4/cancel9 transitions and subsequent residual/event processing.
    public static class OriginalHandpaySelected
    {
        public sealed class Screen
        {
            public Action<string> SetLanguage;
            public Action<byte[],int> AddText;
            public Action<byte[]> SetYesText,SetNoText;
            public Action<long> SetAmount;
            public Action<int> SetOrder;
            public Action Init,Delete;
            public Func<int> DisplayResult;
        }
        public sealed class Host
        {
            public Func<uint,bool> TiltPending;
            public Action<byte[],uint> Emit;
            public Func<Screen> CreateScreen,ReadManagerScreen2C4;
            public Action<Screen> AssignManagerScreen2C4;
            public Func<bool> JurisdictionFlag448,MachineFlag188;
            public Func<string> SelectedLanguage;
            public Action<long,bool,int,bool> FormatCredit;
            public Func<string,string[],bool,byte[]> Translate;
            internal Host CopyBindings(){return (Host)MemberwiseClone();}
            internal void Validate()
            {if(TiltPending==null||Emit==null||CreateScreen==null||ReadManagerScreen2C4==null||AssignManagerScreen2C4==null||JurisdictionFlag448==null||MachineFlag188==null||SelectedLanguage==null||FormatCredit==null||Translate==null)throw new ArgumentException("Genuine HandPaySelected owners required");}
        }
        public sealed class Session
        {
            private readonly OriginalHandpaySelectionPersistence owner;
            private readonly Host host;
            private readonly long capturedAmount;
            private bool completed;
            internal Session(OriginalHandpaySelectionPersistence owner,Host host,long capturedAmount){this.owner=owner;this.host=host;this.capturedAmount=capturedAmount;}
            // Async Unity input may suspend after Begin and invoke this once the
            // genuine VerifyYesNo display returns. It must preserve manager2c4.
            public bool Complete(int displayResult)
            {
                if(completed)throw new InvalidOperationException("HandPaySelected session already completed");
                owner.CurrentPayload.InProgressValue=0;owner.SaveCurrentPayload();
                var screen=host.ReadManagerScreen2C4();if(screen!=null){screen.Delete();host.AssignManagerScreen2C4(null);}
                if(host.TiltPending(0x6020))host.Emit(OriginalHandpaySelectionEvents.Tilt(false,capturedAmount),0);
                bool disabled=host.MachineFlag188();completed=true;return disabled||displayResult!=0;
            }
        }
        private static byte[] Button(byte[] text)
        {
            if(text==null)throw new InvalidOperationException("Native translated string absent");var b=(byte[])text.Clone();
            // Native std::string::find searches full string, including after an
            // embedded zero; button setter subsequently handles its C buffer.
            int at=Array.IndexOf(b,(byte)32);if(at>=0)b[at]=10;return b;
        }
        public static Session Begin(OriginalHandpaySelectionPersistence owner,Host h)
        {
            if(owner==null||h==null)throw new ArgumentNullException();h.Validate();
            long captured=owner.SelectionAmount;
            if(!h.TiltPending(0x6020))h.Emit(OriginalHandpaySelectionEvents.Tilt(true,captured),0);
            h.AssignManagerScreen2C4(h.CreateScreen());
            if(h.JurisdictionFlag448()){string language=h.SelectedLanguage();h.ReadManagerScreen2C4().SetLanguage(language);}
            h.FormatCredit(owner.SelectionAmount,true,100,true);
            long amount=owner.SelectionAmount;
            var line=h.Translate("cannot_be_paid_from_the_hopper",new[]{amount.ToString(CultureInfo.InvariantCulture)},true);h.ReadManagerScreen2C4().AddText(line,3);
            var option=h.Translate("Please_touch_an_option",new string[0],true);h.ReadManagerScreen2C4().AddText(option,3);
            var yes=Button(h.Translate("Hand_Pay",new string[0],true));h.ReadManagerScreen2C4().SetYesText(yes);
            var no=Button(h.Translate("Continue_Game_Play",new string[0],true));h.ReadManagerScreen2C4().SetNoText(no);
            h.ReadManagerScreen2C4().SetAmount(amount);h.ReadManagerScreen2C4().SetOrder(0x23a5);h.ReadManagerScreen2C4().Init();
            owner.CurrentPayload.InProgressValue=1;owner.SaveCurrentPayload();
            if(owner.CurrentPayload.ResidualSubstateValue!=3){owner.CurrentPayload.ResidualSubstateValue=3;owner.SaveCurrentPayload();}
            return new Session(owner,h,captured);
        }
        public static bool Apply(OriginalHandpaySelectionPersistence owner,Host h)
        {var session=Begin(owner,h);int result=h.ReadManagerScreen2C4().DisplayResult();return session.Complete(result);}
    }
}
