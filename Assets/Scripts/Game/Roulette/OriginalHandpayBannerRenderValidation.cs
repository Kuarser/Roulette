using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalHandpayBannerRenderValidation
    {
        public static void Validate()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/native_handpay_layout_v1/banner_render_v2"));
            if(Directory.Exists(output))throw new Exception("Frozen render output exists");Directory.CreateDirectory(output);
            int checks=0;
            Action<bool> check=b=>{if(!b)throw new Exception("Banner render regression "+checks);++checks;};
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));
            var money=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
            foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(540,960)})
                foreach(bool translated in new[]{false,true})
                {
                    var text=OriginalHandpayBannerText.Create(995,false,translated,false,false,v=>Encoding.UTF8.GetBytes(money.FormatText(v,100,false,0)));
                    string language=translated?"zh":"en";
                    // Explicit UTF8 prefix fixture: unfinished trailing sequence
                    // stays in Decoder state. This is not a native LC_CTYPE claim.
                    Func<int,byte[],short> measure=(slot,bytes)=>{
                        var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];
                        int used=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);
                        var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(language,slot,s=>s.ToLowerInvariant()));
                        return unchecked((short)font.MeasureWidth(new string(chars,0,used)));
                    };
                    var screen=OriginalHandpayBannerPresentation.CreateOrdinary(size.x,size.y,text,translator,language,s=>s.ToLowerInvariant(),Encoding.UTF8.GetBytes,Encoding.UTF8.GetString,measure);
                    check(screen.LabelCount>=2 && screen.CenteringOffset==0);check(screen.Layout.Lines[0].FontSlot==1);
                    var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();
                    var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);var saved=RenderTexture.active;
                    try
                    {
                        screen.DrawTo(target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                        File.WriteAllBytes(Path.Combine(output,size.x+"x"+size.y+"_"+(translated?"zh":"en")+".png"),image.EncodeToPNG());
                        var body=screen.Geometry.Body;
                        Color blue=image.GetPixel(body.Left+1,size.y-1-body.Top);Color yellow=image.GetPixel(body.Left-2,size.y-1-body.Top);
                        check(blue.r<.01f&&blue.g<.01f&&blue.b>.99f);check(yellow.r>.99f&&yellow.g>.99f&&yellow.b<.01f);check(image.GetPixel(0,0).a<.01f);
                        int white=0;for(int y=body.Top;y<body.Bottom;++y)for(int x=body.Left;x<body.Right;++x){var c=image.GetPixel(x,size.y-1-y);if(c.r>.9f&&c.g>.9f&&c.b>.9f)++white;}check(white>100);
                    }
                    finally {RenderTexture.active=saved;UnityEngine.Object.DestroyImmediate(image);target.Release();UnityEngine.Object.DestroyImmediate(target);}
                }
            Debug.Log("HANDPAY_BANNER_GPU_VALIDATED: captures=4 checks="+checks+" output="+output);
        }
    }
}
