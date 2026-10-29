using System;
namespace RouletteDeluxe
{
    // InfoTextScreen inherits the recovered ordinary TextScreenHandPay owner.
    // Actual AGK Arial/Simsun font binding, layout and sprites are shared here.
    public sealed class OriginalResidualFailedBitmapScreen
    {
        public readonly OriginalHandpayBitmapScreen Bitmap;
        public readonly OriginalCreditOutResidualScreens.Screen Screen;
        public bool Deleted{get;private set;}
        public OriginalResidualFailedBitmapScreen(int width,int height,OriginalCashoutFontBinding fonts,
            Func<byte[],string> decode,Func<int,byte[],short> measure,Func<string,string[],byte[]> translate,
            Func<int,int> lowercase,Action<int,object> releaseFont,Action<byte[],uint> emit)
        {
            if(translate==null)throw new ArgumentNullException(nameof(translate));
            Bitmap=new OriginalHandpayBitmapScreen(width,height,fonts,decode,measure,b=>translate(System.Text.Encoding.UTF8.GetString(b),new string[0]),lowercase,releaseFont,emit);
            Screen=new OriginalCreditOutResidualScreens.Screen{
                SetPosition=(x,y)=>{Bitmap.Layout.NormalizedX=x;Bitmap.Layout.NormalizedY=y;},SetSize=(w,h)=>{Bitmap.Layout.NormalizedWidth=w;Bitmap.Layout.NormalizedHeight=h;},
                SetOrder=n=>Bitmap.RouterScreen.SetOrder(n),SetLanguage=s=>Bitmap.RouterScreen.SetLanguage(s),AddText=Bitmap.AddText,
                AddTranslatedText=(key,args,slot)=>Bitmap.AddText(translate(key,args),slot),Init=Bitmap.Init,Display=()=>Bitmap.DisplayOwner.Lifecycle.Display(),
                Clear=()=>Bitmap.Clear(),End=()=>Bitmap.End(),Delete=()=>{
                    if(Bitmap.DisplayOwner.Lifecycle.RootPresent||Bitmap.DisplayOwner.Lifecycle.Visible)throw new InvalidOperationException("Native End required before managed delete");Deleted=true;
                }
            };
        }
    }
}
