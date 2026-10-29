using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalHandpayBannerUpdateRenderValidation
    {
        public static void Validate()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/native_handpay_layout_v1/banner_update_render_v5"));
            string[] arguments=Environment.GetCommandLineArgs();int outputIndex=Array.IndexOf(arguments,"-nativeBannerOutput");
            if(outputIndex>=0&&outputIndex+1<arguments.Length)output=Path.GetFullPath(arguments[outputIndex+1]);
            if(Directory.Exists(output))throw new Exception("Frozen update output exists");Directory.CreateDirectory(output);
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("Banner update GPU regression "+checks);++checks;};
            foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(540,960)})
            {
                var layout=new OriginalHandpayBannerLayout(size.x,size.y);var events=new List<byte[]>();
                Func<int,byte[],short> measure=(slot,bytes)=>unchecked((short)OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource("en",slot,s=>s)).MeasureWidth(Encoding.ASCII.GetString(bytes)));
                Action<string,int> add=(value,slot)=>{
                    var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource("en",slot,s=>s));
                    OriginalTextScreenWrapping.AddText(layout,Encoding.ASCII.GetBytes(value),slot,font.Height,0,true,measure);
                };
                add("Call Attendant",1);add("HAND PAY RM9.95",1);layout.Align(measure);
                var owner=new OriginalHandpayBannerDisplayOwner(layout,()=>new OriginalHandpayBannerPresentation(layout,"en",s=>s,Encoding.ASCII.GetString),
                    (raw,flags)=>{check(flags==0&&raw.Length==44);events.Add((byte[])raw.Clone());});
                var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();
                Action<string,bool> capture=(name,hasText)=>{
                    if(owner.Presentation!=null)File.WriteAllLines(Path.Combine(output,size.x+"x"+size.y+"_"+name+"_labels.txt"),owner.Presentation.DescribeLabels());
                    if(owner.Presentation!=null)owner.Presentation.DrawTo(target);
                    else {var before=RenderTexture.active;RenderTexture.active=target;GL.Clear(true,true,Color.clear);RenderTexture.active=before;}
                    var saved=RenderTexture.active;var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);
                    try{
                        RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                        int white=0,opaque=0;foreach(var c in image.GetPixels32()){if(c.a!=0)++opaque;if(c.r>240&&c.g>240&&c.b>240)++white;}
                        check(hasText?white>100&&opaque>100:white==0&&opaque==0);
                        File.WriteAllBytes(Path.Combine(output,size.x+"x"+size.y+"_"+name+".png"),image.EncodeToPNG());
                    }finally{RenderTexture.active=saved;UnityEngine.Object.DestroyImmediate(image);}
                };
                try{
                    owner.Lifecycle.Display();capture("display",true);check(events.Count==1&&owner.Presentation.LabelCount==2);
                    owner.Lifecycle.Hide();capture("hidden",false);check(events.Count==2&&!owner.Lifecycle.Visible);
                    owner.Lifecycle.Show();capture("shown",true);check(events.Count==3&&owner.Lifecycle.Visible);
                    owner.Presentation.Refresh(measure);capture("refreshed",true);
                    var origin=OriginalTextScreenRefresh.GetPosition(layout);
                    check(owner.Presentation.Geometry.Body.Left==origin.X&&owner.Presentation.Geometry.Body.Top==origin.Y);
                    check(owner.Presentation.Geometry.Border.Left==origin.BorderX&&owner.Presentation.Geometry.Border.Top==origin.BorderY&&events.Count==3);
                    owner.ResetForUpdate();add("Please Wait",3);layout.Align(measure);owner.UpdateDisplay("en",s=>s,Encoding.ASCII.GetString);
                    capture("one_line",true);check(events.Count==3&&owner.Presentation.LabelCount==2&&layout.Lines.Count==1);
                    owner.ResetForUpdate();add("Call Attendant",1);add("HAND PAY RM12.34",1);add("Processing",4);layout.Align(measure);
                    owner.UpdateDisplay("en",s=>s,Encoding.ASCII.GetString);capture("three_lines",true);
                    check(owner.Presentation.LabelCount==3&&events.Count==3);
                    owner.Lifecycle.Clear();capture("cleared",false);check(owner.Lifecycle.RootPresent&&owner.Lifecycle.Visible&&layout.Lines.Count==0);
                    owner.Lifecycle.End();capture("ended",false);check(owner.Presentation==null&&!owner.Lifecycle.RootPresent&&!owner.Lifecycle.Visible&&events.Count==4);
                    uint[] expected={0x18100,0x18200,0x18100,0x18200};
                    for(int i=0;i<events.Count;++i){var raw=new byte[44];raw[0]=1;raw[2]=44;for(int j=0;j<4;++j)raw[24+j]=(byte)(expected[i]>>(j*8));
                        check(Convert.ToBase64String(raw)==Convert.ToBase64String(events[i]));}
                    File.WriteAllText(Path.Combine(output,size.x+"x"+size.y+"_events.txt"),string.Join("\n",events.ConvertAll(b=>BitConverter.ToString(b).Replace("-","")))+"\n");
                }finally{target.Release();UnityEngine.Object.DestroyImmediate(target);}
            }
            Debug.Log("HANDPAY_BANNER_UPDATE_GPU_VALIDATED: captures=16 checks="+checks+" output="+output);
        }
    }
}
