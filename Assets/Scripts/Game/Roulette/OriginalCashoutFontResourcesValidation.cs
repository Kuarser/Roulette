using System;
using System.Globalization;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalCashoutFontResourcesValidation
    {
        public static void Validate()
        {
            var reference=Resources.Load<TextAsset>("Original/Fonts/CashOut/source_glyph_reference");
            if(reference==null)throw new Exception("CashOut font reference missing");
            int fonts=0,glyphs=0;
            foreach(string row in reference.text.Split('\n'))
            {
                if(row.Length==0)continue;var fields=row.TrimEnd('\r').Split(' ');var font=OriginalBitmapFont.Load(fields[1]);
                if(font==null)throw new Exception("CashOut font missing "+fields[1]);
                if(fields[0]=="F")
                {
                    if(font.Height!=Parse(fields[2])||font.PageCount!=Parse(fields[3])||font.GlyphCount!=Parse(fields[4]))
                        throw new Exception("CashOut font dimensions differ "+fields[1]);++fonts;
                }
                else if(fields[0]=="G")
                {
                    Texture2D texture;Rect uv;int width;
                    if(!font.TryGetGlyph(Parse(fields[2]),out texture,out uv,out width)||texture==null||width!=Parse(fields[3]))
                        throw new Exception("CashOut declared glyph differs "+fields[1]+" "+fields[2]);++glyphs;
                }
                else throw new Exception("Unknown CashOut reference row");
            }
            if(fonts!=13||glyphs!=34663)throw new Exception("Incomplete defined CashOut corpus");
            foreach(string name in new[]{"CashOutFont-Msg-1080x1920","CashOutFont-Msg-1920x1080","CashOutFont-Trans-1080x1920","CashOutFont-Trans-1920x1080"})
                if(Resources.Load<TextAsset>("Original/Fonts/CashOut/"+name+"_metrics")!=null)
                    throw new Exception("Uninitialized native font rectangles must not be substituted "+name);
            Debug.Log("CASHOUT_FONT_RESOURCES_VALIDATED: fonts="+fonts+" declaredGlyphs="+glyphs);
        }
        private static int Parse(string s){return int.Parse(s,CultureInfo.InvariantCulture);}
    }
}
