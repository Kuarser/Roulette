using System;
using System.Collections.Generic;
using System.Globalization;
namespace RouletteDeluxe
{
    // Shared bitmapFontSize map, native constructor 086c6d50. Font format
    // detection and SetCustomFont retain their native owner interfaces.
    public sealed class OriginalCashoutTypographySetup
    {
        public readonly Dictionary<string,int> Heights=new Dictionary<string,int>(StringComparer.Ordinal);
        public void ConstructorFontType(bool usingVectorFonts,int configuredType,Action<int> setUseFontType)
        {
            if(!usingVectorFonts){setUseFontType(0);return;}
            if(configuredType==1||configuredType==2)setUseFontType(configuredType);
            Heights["CashOutFont-Amt-1920x1080"]=150;Heights["CashOutFont-Amt-1080x1920"]=150;
            Heights["CashOutFont-Amt-1360x768"]=105;Heights["CashOutFont-Amt-768x1360"]=85;
            Heights["CashOutFont-Msg-1920x1080"]=80;Heights["CashOutFont-Msg-1080x1920"]=55;
            Heights["CashOutFont-Msg-1360x768"]=55;Heights["CashOutFont-Msg-768x1360"]=40;
            Heights["CashOutFont-Trans-1920x1080"]=35;Heights["CashOutFont-Trans-1080x1920"]=35;
            Heights["CashOutFont-Trans-1360x768"]=25;Heights["CashOutFont-Trans-768x1360"]=20;
        }
        public sealed class FontHost
        {
            public Func<string,bool> IsBitmapFontFormat;
            public Action<int,string,int> SetCustomFont;
            public Action<int,uint> SetCustomFontColor;
        }
        public void ConfigurePrimaryFonts(bool resourcesAvailable,string message,string amount,string translation,short width,short height,FontHost host)
        {
            if(!resourcesAvailable)return;
            string suffix=width.ToString(CultureInfo.InvariantCulture)+"x"+height.ToString(CultureInfo.InvariantCulture);
            string[] files={message,amount,translation};string[] kinds={"Msg","Amt","Trans"};
            for(int i=0;i<3;++i)
            {
                bool bitmap=host.IsBitmapFontFormat(files[i]);int size=16;
                if(bitmap)
                {
                    string key="CashOutFont-"+kinds[i]+"-"+suffix;
                    if(!Heights.TryGetValue(key,out size)){size=0;Heights.Add(key,0);} // std::map::operator[].
                }
                host.SetCustomFont(7+i,files[i],size);
                if(bitmap&&i==1)host.SetCustomFontColor(8,0xffd400);
            }
        }
    }
    // This restores the original configuration read/control protocol. The
    // actual locale/num_get parser is a native-equivalent host boundary.
    public static class OriginalCashoutConfiguration
    {
        public delegate void ExtractSigned16(ref short value);
        public sealed class Input
        {
            public ExtractSigned16 Extract;
            public Func<int> State;
            public Action CloseAndDestroy;
        }
        public sealed class Host {public Func<string,int,Input> Open;}
        public static void Read(OriginalCashoutDisplay.State owner,OriginalCashoutTextBoundsTable table,string file,Host host)
        {
            if(file==null)throw new ArgumentNullException(nameof(file));
            owner.Left=owner.Top=owner.Right=owner.Bottom=0;
            if(file.Length==0){table.ConstructorDefaults(file);return;}
            int nul=file.IndexOf('\0');string path=nul<0?file:file.Substring(0,nul);
            Input input=host.Open(path,8);
            try
            {
                input.Extract(ref owner.Left);input.Extract(ref owner.Top);input.Extract(ref owner.Right);input.Extract(ref owner.Bottom);
                if(input.State()!=0)owner.Left=owner.Top=owner.Right=owner.Bottom=0;
            }
            finally{input.CloseAndDestroy();}
        }
    }
}
