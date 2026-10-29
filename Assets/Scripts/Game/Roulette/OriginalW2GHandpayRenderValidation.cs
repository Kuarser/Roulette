using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalW2GHandpayRenderValidation
    {
        public static void Validate()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/native_handpay_layout_v1/w2g_render_v1"));
            string[] args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-nativeBannerOutput");
            if(index>=0){if(index+1>=args.Length)throw new Exception("Fresh output path required");output=Path.GetFullPath(args[index+1]);}
            if(Directory.Exists(output))throw new Exception("Frozen W2G render output exists");Directory.CreateDirectory(output);
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("W2G GPU regression "+checks);++checks;};
            foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(540,960)})
            for(int sample=0;sample<4;++sample)
            {
                bool translated=sample==3;string language=translated?"zh":"en";
                var events=new List<byte[]>();var nv=new OriginalCreditOutNvPayload(new byte[135]);var saves=new List<byte[]>();
                Func<string,int,byte[],short> measure=(lang,slot,bytes)=>{
                    // Explicit UTF8 prefix fixture; native LC_CTYPE is still a host dependency.
                    var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int used=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);
                    var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(lang,slot,s=>s.ToLowerInvariant()));
                    return unchecked((short)font.MeasureWidth(new string(chars,0,used)));
                };
                var screen=new OriginalHandpayOrdinaryScreen(size.x,size.y,translator,s=>s.ToLowerInvariant(),Encoding.UTF8.GetBytes,Encoding.UTF8.GetString,
                    measure,(raw,flags)=>{check(flags==0);events.Add(raw);});
                var router=new OriginalHandpayTextScreenRouter(new OriginalHandpayTextScreenRouter.Host {
                    Ordinary=screen.Screen,Manager3aa=false,Translate=()=>translated,SelectedLanguage=()=>language});
                long amount=sample==1?0:995;
                OriginalW2GHandpay.Display(amount,new OriginalW2GHandpay.Host {
                    Nv=nv,Save=b=>saves.Add(b),FormatMoney=(a,c,d,s)=>{check(a==amount&&c&&d==100&&s);return Encoding.ASCII.GetBytes(a==0?"RM0.00":"RM9.95");},
                    GamePrize=()=>sample==1,Voucher=()=>sample==2,Translate=()=>translated,ThresholdEnabled=()=>sample==2,
                    ResetToCredits=sample==2,DisplayText=(t,b,c,d)=>{check(router.Display(t,b,c,d));}
                });
                check(saves.Count==1&&saves[0][0x51]==1&&saves[0][0x5d]==0);
                check(screen.Owner.Presentation!=null&&screen.Owner.Lifecycle.Visible&&router.MessageDisplayed&&screen.Order==9100);
                check(screen.Language==language&&screen.Layout.Lines.Count>=2&&screen.Layout.Lines[0].FontSlot==1);
                if(sample==2||sample==3)check(screen.Layout.Lines[screen.Layout.Lines.Count-1].FontSlot==3);
                string prefix=size.x+"x"+size.y+"_case"+sample;
                File.WriteAllLines(Path.Combine(output,prefix+"_labels.txt"),screen.Owner.Presentation.DescribeLabels());
                var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();
                var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);var active=RenderTexture.active;
                try
                {
                    screen.Owner.Presentation.DrawTo(target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                    File.WriteAllBytes(Path.Combine(output,prefix+"_display.png"),image.EncodeToPNG());
                    var body=screen.Owner.Presentation.Geometry.Body;var blue=image.GetPixel(body.Left+1,size.y-1-body.Top);var yellow=image.GetPixel(body.Left-2,size.y-1-body.Top);
                    check(blue.r<.01f&&blue.g<.01f&&blue.b>.99f);check(yellow.r>.99f&&yellow.g>.99f&&yellow.b<.01f);
                    int white=0;foreach(var c in image.GetPixels32())if(c.r>240&&c.g>240&&c.b>240)++white;check(white>100);
                    screen.Owner.Lifecycle.End();check(!screen.Owner.Lifecycle.Visible&&screen.Owner.Presentation==null);
                    RenderTexture.active=target;GL.Clear(true,true,Color.clear);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                    File.WriteAllBytes(Path.Combine(output,prefix+"_ended.png"),image.EncodeToPNG());
                    check(events.Count==2&&events[0][24]==0&&events[0][25]==0x81&&events[1][25]==0x82);
                }
                finally{RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);target.Release();UnityEngine.Object.DestroyImmediate(target);}
                var rows=new List<string>();foreach(var raw in events)rows.Add(BitConverter.ToString(raw).Replace("-",""));
                File.WriteAllLines(Path.Combine(output,prefix+"_events.txt"),rows);
            }
            Debug.Log("W2G_HANDPAY_GPU_VALIDATED: captures=16 checks="+checks+" output="+output);
        }
    }
}
