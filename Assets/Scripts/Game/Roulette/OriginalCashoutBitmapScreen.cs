using System;
using UnityEngine;
namespace RouletteDeluxe
{
    // Actual AGK TextScreenCashOut owner binding for CreditOutMgr's existing
    // DisplayTextScreen router. Translation, byte locale and window roots are
    // caller interfaces; this owner does not select/pay a financial request.
    public sealed class OriginalCashoutBitmapScreen:IDisposable
    {
        public readonly OriginalCashoutDisplay.State State;
        public readonly OriginalHandpayBannerLayout Layout;
        public readonly OriginalCashoutBitmapLabels Labels;
        public readonly OriginalHandpayTextScreenRouter.Screen RouterScreen;
        private readonly OriginalCashoutDisplay.Host display;
        private readonly OriginalCashoutPlaybackLifecycle.Host life;
        private readonly Func<int,byte[],short> measure;
        private readonly Func<byte[],byte[]> translate;
        private OriginalTextScreenFontLanguage fontLanguageOwner;
        private readonly string initialLanguage;
        private readonly OriginalCashoutFontBinding fontBinding;
        public string FontLanguage {get{return fontLanguageOwner==null?initialLanguage:System.Text.Encoding.UTF8.GetString(fontLanguageOwner.Language);}}
        public OriginalCashoutBitmapScreen(Transform root,int width,int height,OriginalCashoutDisplay.State state,
            OriginalCashoutFontBinding fonts,OriginalCashoutDisplay.Host displayHost,OriginalCashoutPlaybackLifecycle.Host lifecycle,
            string initialFontLanguage,Func<byte[],string> decode,Func<int,byte[],short> nativeMeasure,Func<byte[],byte[]> translateSelectedLanguage)
        {
            if(root==null||state==null||fonts==null||displayHost==null||lifecycle==null||initialFontLanguage==null||decode==null||nativeMeasure==null||translateSelectedLanguage==null)throw new ArgumentNullException();
            if(state.FontMode!=0||fonts.FontMode!=0)throw new InvalidOperationException("This CashOut owner requires native AGK mode0");
            State=state;display=displayHost;life=lifecycle;measure=nativeMeasure;translate=translateSelectedLanguage;initialLanguage=initialFontLanguage;fontBinding=fonts;
            Layout=new OriginalHandpayBannerLayout(width,height);Defaults();
            Labels=new OriginalCashoutBitmapLabels(root,state,fonts,Layout.Lines,decode);Labels.Bind(display);
            RouterScreen=new OriginalHandpayTextScreenRouter.Screen {Defaults=Defaults,SetLanguage=language=>
                {if(fontLanguageOwner!=null)fontLanguageOwner.SetLanguage(System.Text.Encoding.UTF8.GetBytes(language));else if(language!=FontLanguage)throw new InvalidOperationException("Native SetLang requires an explicit lowercase and font release owner");},
                AddText=AddText,AddTranslatedText=(key,slot)=>AddText(translate((byte[])key.Clone()),slot),
                SetOrder=order=>State.Layout.Order=order,Init=Init,Display=()=>OriginalCashoutDisplay.Display(State,display)};
        }
        public void BindFontLanguage(Func<int,int> nativeLowercase,Action<int,object> releaseOwnedFont)
        {
            if(fontLanguageOwner!=null)throw new InvalidOperationException("Font language owner already bound");
            fontLanguageOwner=new OriginalTextScreenFontLanguage(State,fontBinding,System.Text.Encoding.UTF8.GetBytes(initialLanguage),nativeLowercase,releaseOwnedFont);
        }
        public void Defaults()
        {
            State.Layout.SetDefaults();Layout.NormalizedX=Layout.NormalizedY=0;Layout.NormalizedWidth=Layout.NormalizedHeight=1;
            Layout.ClipX=Layout.ClipY=Layout.BorderWidth=Layout.HorizontalAlignment=Layout.VerticalAlignment=0;Layout.Centering=true;
        }
        public void AddText(byte[] text,int slot)
        {
            if(text==null)throw new ArgumentNullException(nameof(text));
            var font=Labels.Font(slot);OriginalTextScreenWrapping.AddText(Layout,text,slot,font.Height,0,true,measure);
            SynchronizeLines();
        }
        private void SynchronizeLines()
        {
            State.Lines.Clear();foreach(var line in Layout.Lines)State.Lines.Add(new OriginalCashoutDisplay.Line {Text=(byte[])line.Text.Clone(),FontSlot=line.FontSlot,Height=line.Height});
        }
        public void Init()
        {
            Layout.Align(measure);SynchronizeLines();State.Cursor=Layout.Lines.Count;
            State.Layout.X=Layout.NormalizedX;State.Layout.Y=Layout.NormalizedY;State.Layout.Width=Layout.NormalizedWidth;State.Layout.Height=Layout.NormalizedHeight;
            var pixels=Layout.Pixels();State.Layout.PixelX=pixels.X;State.Layout.PixelY=pixels.Y;State.Layout.PixelWidth=pixels.Width;State.Layout.PixelHeight=pixels.Height;
        }
        // Clear086c4070 only clears labels/line count when resources are ready;
        // it preserves fonts, roots, visibility, pause and centering.
        public bool Clear()
        {
            if(State.Playback.ResourcesAvailable){int count=State.Lines.Count;Labels.ClearNativeLabels();Layout.ClearLinesForUpdate();State.Lines.Clear();State.Cursor=count;}return true;
        }
        public bool End(){return OriginalCashoutPlaybackLifecycle.End(State.Playback,life);}
        public void Dispose(){Labels.Dispose();}
    }
}
