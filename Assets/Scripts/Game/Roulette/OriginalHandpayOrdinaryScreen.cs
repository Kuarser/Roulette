using System;

namespace RouletteDeluxe
{
    // Actual bitmap composition for CreditOutMgr's ordinary TextScreenHandPay22c.
    public sealed class OriginalHandpayOrdinaryScreen
    {
        public readonly OriginalHandpayBannerLayout Layout;
        public readonly OriginalHandpayBannerDisplayOwner Owner;
        public readonly OriginalHandpayTextScreenRouter.Screen Screen;
        public string Language { get; private set; }
        public int Order { get; private set; }
        public OriginalHandpayOrdinaryScreen(int width,int height,OriginalLanguageTranslator translator,
            Func<string,string> lowercase,Func<string,byte[]> encode,Func<byte[],string> decode,
            Func<string,int,byte[],short> measure,Action<byte[],uint> emit)
        {
            if(translator==null||lowercase==null||encode==null||decode==null||measure==null||emit==null)
                throw new ArgumentNullException();
            Layout=new OriginalHandpayBannerLayout(width,height);Language="en";
            Func<int,byte[],short> currentMeasure=(slot,bytes)=>measure(Language,slot,bytes);
            Owner=new OriginalHandpayBannerDisplayOwner(Layout,
                ()=>new OriginalHandpayBannerPresentation(Layout,Language,lowercase,decode),emit);
            Action<byte[],int> add=(bytes,slot)=>{
                var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(Language,slot,lowercase));
                if(font==null)throw new InvalidOperationException("Original bitmap font missing");
                OriginalTextScreenWrapping.AddText(Layout,bytes,slot,font.Height,0,true,currentMeasure);
            };
            Screen=new OriginalHandpayTextScreenRouter.Screen {
                Defaults=Layout.SetHandpayDefaults,SetLanguage=value=>Language=value,
                AddText=add,AddTranslatedText=(key,slot)=>add(OriginalHandpayBannerText.Translate(key,translator,Language,encode),slot),
                SetOrder=value=>Order=value,Init=()=>Layout.Align(currentMeasure),Display=()=>Owner.Lifecycle.Display()
            };
        }
    }
}
