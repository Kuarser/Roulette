using System;
using System.Text;
namespace RouletteDeluxe
{
    // CreditOutMgr+22c is TextScreenHandPay. This binds its ordinary router
    // interfaces to the existing native layout, presentation and lifecycle.
    public sealed class OriginalHandpayBitmapScreen
    {
        public readonly OriginalHandpayBannerLayout Layout;
        public readonly OriginalCashoutDisplay.State FontState;
        public readonly OriginalHandpayBannerDisplayOwner DisplayOwner;
        public readonly OriginalHandpayTextScreenRouter.Screen RouterScreen;
        public int Order {get;private set;}
        private readonly OriginalCashoutFontBinding fonts;
        private readonly OriginalTextScreenFontLanguage language;
        private readonly Func<byte[],string> decode;
        private readonly Func<int,byte[],short> measure;
        private readonly Func<byte[],byte[]> translate;
        public string FontLanguage {get{return Encoding.UTF8.GetString(language.Language);}}
        public OriginalHandpayBitmapScreen(int width,int height,OriginalCashoutFontBinding fontBinding,
            Func<byte[],string> nativeDecode,Func<int,byte[],short> nativeMeasure,Func<byte[],byte[]> translateSelectedLanguage,
            Func<int,int> nativeLowercase,Action<int,object> releaseFont,Action<byte[],uint> emit)
        {
            fonts=fontBinding??throw new ArgumentNullException(nameof(fontBinding));if(fonts.FontMode!=0)throw new InvalidOperationException("Ordinary AGK screen requires mode0");
            decode=nativeDecode??throw new ArgumentNullException(nameof(nativeDecode));measure=nativeMeasure??throw new ArgumentNullException(nameof(nativeMeasure));translate=translateSelectedLanguage??throw new ArgumentNullException(nameof(translateSelectedLanguage));
            FontState=new OriginalCashoutDisplay.State();OriginalTextScreenFontLanguage.InitializeEnglishFiles(FontState,fonts);
            language=new OriginalTextScreenFontLanguage(FontState,fonts,Encoding.ASCII.GetBytes("en"),nativeLowercase,releaseFont);
            Layout=new OriginalHandpayBannerLayout(width,height);Defaults();
            DisplayOwner=new OriginalHandpayBannerDisplayOwner(Layout,()=>new OriginalHandpayBannerPresentation(Layout,FontLanguage,value=>value,decode),emit);
            RouterScreen=new OriginalHandpayTextScreenRouter.Screen {Defaults=Defaults,SetLanguage=value=>language.SetLanguage(Encoding.UTF8.GetBytes(value)),
                AddText=AddText,AddTranslatedText=(key,slot)=>AddText(translate((byte[])key.Clone()),slot),SetOrder=value=>Order=value,Init=Init,
                Display=()=>DisplayOwner.Lifecycle.Display()};
        }
        public void Defaults(){Layout.SetHandpayDefaults();Order=OriginalHandpayBannerLayout.NativeOrder;}
        public void AddText(byte[] text,int slot)
        {
            if(text==null)throw new ArgumentNullException(nameof(text));if(slot<0||slot>=7)throw new InvalidOperationException("Ordinary presentation requires native slots0..6");
            var font=FontState.Fonts[slot] as OriginalBitmapFont;
            if(font==null){font=fonts.LoadQualifiedAgkFont(slot);FontState.Fonts[slot]=font;}
            OriginalTextScreenWrapping.AddText(Layout,text,slot,font.Height,0,true,measure);SynchronizeLines();
        }
        private void SynchronizeLines()
        {
            FontState.Lines.Clear();foreach(var line in Layout.Lines)FontState.Lines.Add(new OriginalCashoutDisplay.Line {Text=(byte[])line.Text.Clone(),FontSlot=line.FontSlot,Height=line.Height});
        }
        public void Init(){Layout.Align(measure);SynchronizeLines();FontState.Cursor=Layout.Lines.Count;}
        public bool Clear(){FontState.Cursor=Layout.Lines.Count;bool result=DisplayOwner.Lifecycle.Clear();SynchronizeLines();return result;}
        public bool End(){FontState.Cursor=Layout.Lines.Count;bool result=DisplayOwner.Lifecycle.End();SynchronizeLines();Order=OriginalHandpayBannerLayout.NativeOrder;return result;}
    }
}
