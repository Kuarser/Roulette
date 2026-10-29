using System;
namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectionPresentation
    {
        public static OriginalVerifyYesNoPresentation Create(int width,int height,OriginalHandpaySelectionText.Content content,
            Func<string,string> nativeLowercase,Func<byte[],string> nativeDecode,
            Func<int,byte[],short> nativeMeasureWidth)
        {
            if(content==null||nativeDecode==null||nativeMeasureWidth==null)throw new ArgumentNullException();
            var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(content.FontLanguage,3,nativeLowercase));
            if(font==null)throw new InvalidOperationException("Original handpay slot3 font unavailable");
            var layout=new OriginalVerifyTextLayout(0,width,0.6f);
            layout.AddText(content.AmountLine,3,font.Height,bytes=>nativeMeasureWidth(3,bytes));
            layout.AddText(content.OptionLine,3,font.Height,bytes=>nativeMeasureWidth(3,bytes));
            return new OriginalVerifyYesNoPresentation(width,height,layout,layout.Align(5,0,0),content.FontLanguage,
                nativeLowercase,nativeDecode,content.Yes,content.No,true,true,OriginalHandpaySelectionCoordinator.NativeScreenOrder);
        }
    }
}
