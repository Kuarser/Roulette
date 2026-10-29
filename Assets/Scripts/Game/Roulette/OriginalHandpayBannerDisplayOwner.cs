using System;

namespace RouletteDeluxe
{
    // Owns actual managed sprite composition through original lifecycle order.
    public sealed class OriginalHandpayBannerDisplayOwner
    {
        public OriginalHandpayBannerPresentation Presentation { get; private set; }
        public readonly OriginalTextScreenLifecycle Lifecycle;
        public readonly OriginalTextScreenUpdate Update;
        public bool ResetForUpdate()
        { if(Presentation!=null)Update.Language=Presentation.FontLanguage;return Update.ResetForUpdate(); }
        public bool UpdateDisplay(string language,Func<string,string> lowercase,Func<byte[],string> decode)
        {
            if(Presentation==null)throw new InvalidOperationException("Native root/body required for update");
            Presentation.UpdateBitmapDisplay(Update,language,lowercase,decode);return true;
        }
        public OriginalHandpayBannerDisplayOwner(OriginalHandpayBannerLayout layout,
            Func<OriginalHandpayBannerPresentation> createPresentation, Action<byte[],uint> emit)
        {
            if (layout == null || createPresentation == null || emit == null) throw new ArgumentNullException();
            Update = new OriginalTextScreenUpdate(layout);
            Lifecycle = new OriginalTextScreenLifecycle(new OriginalTextScreenLifecycle.Host {
                CreateDisplaySprites = () => { Presentation = createPresentation(); },
                SetRootVisible = visible => { if (Presentation != null) Presentation.RootVisible = visible; },
                ClearSprites = () => { if (Presentation != null) Presentation.ClearNativeSprites(); else layout.ClearNativeLines(); },
                ReleaseRoot = () => { Presentation = null; }, SetDefaults = layout.SetHandpayDefaults, Emit = emit });
        }
    }
}
