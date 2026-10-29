using System;
using System.Globalization;
using System.Text;
namespace RouletteDeluxe
{
    // Original081f9110/081f84b0. Screen, Game, timer and financial operations
    // belong to genuine caller owners. These methods do not complete payment.
    public static class OriginalCreditOutResidualScreens
    {
        public sealed class OptionHost
        {
            public Func<bool> PendingScreen2C4;
            public Action<int> GameOperation;
            public Action<int,bool> MachineOperation;
            public Func<bool> HandPaySelected;
            public Action ProcessResidualGamble;
            public Func<uint> ReadManagerEvent18;
            public Action<byte[],uint> Emit;
            internal OptionHost CopyBindings(){return (OptionHost)MemberwiseClone();}
            internal void Validate()
            {if(GameOperation==null||MachineOperation==null||HandPaySelected==null||ProcessResidualGamble==null||ReadManagerEvent18==null||Emit==null)throw new ArgumentException("Original residual option operations required");}
        }
        public sealed class Screen
        {
            public Action<float,float> SetPosition,SetSize;
            public Action<int> SetOrder;
            public Action<string> SetLanguage;
            public Action<byte[],int> AddText;
            public Action<string,string[],int> AddTranslatedText;
            public Action Init,Display,Clear,End,Delete;
        }
        public sealed class FailedHost
        {
            // Credit+2c(destination, amount, 1, 100, 1), returned C string bytes.
            public Func<long,bool,int,bool,byte[]> FormatCredit;
            public Func<Screen> CreateInfoScreen,ReadManagerScreen228;
            public Func<bool> JurisdictionFlag448;
            public Func<string> SelectedLanguage;
            public Action<Screen> SetInfoTimerHandler,PublishInfoScreen;
            public Action<Screen,uint> ArmInfoTrigger;
            internal FailedHost CopyBindings(){return (FailedHost)MemberwiseClone();}
            internal void Validate()
            {if(FormatCredit==null||CreateInfoScreen==null||ReadManagerScreen228==null||JurisdictionFlag448==null||SelectedLanguage==null||SetInfoTimerHandler==null||PublishInfoScreen==null||ArmInfoTrigger==null)throw new ArgumentException("Original residual failure screen owners required");}
        }
        public static void Option(OriginalHandpaySelectionPersistence owner,OptionHost h)
        {
            if(owner==null||h==null||h.PendingScreen2C4==null)throw new ArgumentNullException();
            if(h.PendingScreen2C4())return;h.Validate();
            h.GameOperation(0x204);h.MachineOperation(0x19c,true);
            FinishOptionSelection(owner,h,h.HandPaySelected());
        }
        internal static void FinishOptionSelection(OriginalHandpaySelectionPersistence owner,OptionHost h,bool selected)
        {
            if(!selected)
            {
                if(owner.CurrentPayload.ResidualSubstateValue!=9){owner.CurrentPayload.ResidualSubstateValue=9;owner.SaveCurrentPayload();}
                h.ProcessResidualGamble();h.GameOperation(0x208);
                h.Emit(OriginalHandpaySelectionEvents.Residual(h.ReadManagerEvent18(),owner.CurrentPayload.CashOutAmount),0);
            }
            else
            {
                owner.CurrentPayload.CollectSelectedByte=1;owner.SaveCurrentPayload();
                if(owner.CurrentPayload.ResidualSubstateValue!=4)
                {owner.CurrentPayload.CollectSelectedByte=1;owner.CurrentPayload.ResidualSubstateValue=4;owner.SaveCurrentPayload();}
            }
        }
        private static byte[] CString(byte[] bytes)
        {
            if(bytes==null)throw new InvalidOperationException("Original formatter returned no buffer");
            int n=Array.IndexOf(bytes,(byte)0);if(n<0)n=bytes.Length;var b=new byte[n];Array.Copy(bytes,b,n);return b;
        }
        private static byte[] ResidualLine(byte[] currency)
        {
            var prefix=CString(currency);var suffix=Encoding.ASCII.GetBytes(" of Residual Credits");
            // Native stack uses64-byte formatted currency and128-byte joined line;
            // reject out-of-contract host output instead of reproducing overflow.
            if(prefix.Length>=64)throw new ArgumentException("Original formatted credit must fit64-byte buffer");
            var b=new byte[prefix.Length+suffix.Length];Array.Copy(prefix,b,prefix.Length);Array.Copy(suffix,0,b,prefix.Length,suffix.Length);return b;
        }
        public static bool Failed(OriginalCreditOutCollect.Runtime runtime,long amount,FailedHost h)
        {
            if(runtime==null||h==null)throw new ArgumentNullException();if(runtime.Field23C!=0)return false;h.Validate();
            byte[] line=ResidualLine(h.FormatCredit(amount,true,100,true));var screen=h.CreateInfoScreen();
            if(screen==null||screen.SetPosition==null||screen.SetSize==null||screen.SetOrder==null||screen.Init==null||screen.Display==null)throw new ArgumentException("Actual InfoText screen required");
            h.SetInfoTimerHandler(screen);screen.SetPosition(0.15f,0.4f);screen.SetSize(0.7f,0.2f);screen.SetOrder(0x238c);
            if(h.JurisdictionFlag448())
            {
                // Original reads manager+228 for each separate virtual operation,
                // deliberately preserving its distinct target and dynamic rebinding.
                string language=h.SelectedLanguage();h.ReadManagerScreen228().SetLanguage(language);
                h.ReadManagerScreen228().AddTranslatedText("of_Residual_Credits",new[]{amount.ToString(CultureInfo.InvariantCulture)},1);
                h.ReadManagerScreen228().AddTranslatedText("Cannot_Be_Redeemed",new string[0],1);
                h.ReadManagerScreen228().AddTranslatedText("Please_Continue_Play",new string[0],1);
            }
            else
            {
                screen.AddText(line,1);screen.AddText(Encoding.ASCII.GetBytes("Cannot be Redeemed."),1);screen.AddText(Encoding.ASCII.GetBytes("Please Continue Play."),1);
            }
            screen.Init();h.PublishInfoScreen(screen);screen.Display();h.ArmInfoTrigger(screen,2500);return true;
        }
    }
}
