using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace RouletteDeluxe
{
    public static class OriginalCashoutBitmapLabelsValidation
    {
        [Serializable] public sealed class Capture {public string file;public OriginalCashoutBitmapLabels.Snapshot[] labels;public bool textVisible;}
        [Serializable] public sealed class Report {public string engine,backend;public int checks;public bool financialExecution;public Capture[] captures;}
        public static int Validate(string originalFonts,string output)
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut bitmap labels check "+checks);++checks;};
            check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            check(!Directory.Exists(output));Directory.CreateDirectory(output);
            var binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),OriginalCashoutFontDescription.LocalReader(path=>Path.Combine(originalFonts,path.Substring("/agk/fonts/".Length))));
            foreach(int slot in new[]{7,8,9,10,12})binding.SetCustomFont(slot,"/agk/fonts/CashOutFont-"+(slot==8?"Amt":slot==9||slot==12?"Trans":"Msg")+(slot>=10?"-Small":"")+"-1360x768.fnt",16);
            var state=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true}};
            state.Layout.SetDefaults();
            var layout=new OriginalHandpayBannerLayout(1360,768){BorderWidth=0,Centering=true,NormalizedX=0,NormalizedY=0,NormalizedWidth=1,NormalizedHeight=1};
            var cameraObject=new GameObject("CashOut glyph test camera");cameraObject.layer=31;var camera=cameraObject.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.cullingMask=1<<31;
            var canvasObject=new GameObject("CashOut native root",typeof(RectTransform),typeof(Canvas));canvasObject.layer=31;
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.pixelPerfect=true;
            var target=new RenderTexture(1360,768,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
            var captures=new System.Collections.Generic.List<Capture>();
            OriginalCashoutBitmapLabels labels=null;OriginalCashoutMovieFrames frames=null;
            try
            {
                var movieObject=new GameObject("Original BG frame zero",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));movieObject.layer=31;movieObject.transform.SetParent(canvasObject.transform,false);
                var movie=movieObject.GetComponent<RawImage>();movie.raycastTarget=false;movie.rectTransform.anchorMin=movie.rectTransform.anchorMax=new Vector2(0,1);movie.rectTransform.pivot=new Vector2(0,1);movie.rectTransform.sizeDelta=new Vector2(1360,768);movie.rectTransform.anchoredPosition=Vector2.zero;
                frames=new OriginalCashoutMovieFrames(OriginalCashoutMovieFrames.LoadManifest(),"/agk/picts/CashOutAnimation-Bottom-1360x768.bik",Application.streamingAssetsPath);frames.SetFrame(0);movie.texture=frames.Texture;
                labels=new OriginalCashoutBitmapLabels(canvasObject.transform,state,binding,layout.Lines,Encoding.UTF8.GetString);
                var life=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,Emit=(raw,p)=>{},SetDefaults=()=>{}};
                var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("");int events=0;
                var host=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>layout.Centering,LookupTextBounds=table.Lookup,
                    Emit=(raw,p)=>{check(raw.Length==44&&raw[25]==0x81&&p==0);++events;},Show=()=>OriginalCashoutPlaybackLifecycle.Show(state.Playback,life)};
                labels.Bind(host);
                Action<string> save=name=>
                {
                    foreach(var r in canvasObject.GetComponentsInChildren<RectTransform>(true))r.gameObject.layer=31;
                    Canvas.ForceUpdateCanvases();camera.Render();var prior=RenderTexture.active;var image=new Texture2D(1360,768,TextureFormat.RGBA32,false);
                    try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1360,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());}
                    finally{RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(image);}
                    captures.Add(new Capture {file=name+".png",labels=labels.Describe(),textVisible=labels.Root.activeSelf});
                };
                string[] text={"Call Attendant","RM9.95","HAND PAY"};int[] height={63,115,30};
                for(int i=0;i<3;++i){layout.AddSingleLine(Encoding.UTF8.GetBytes(text[i]),7+i,height[i],true);state.Lines.Add(new OriginalCashoutDisplay.Line {Text=Encoding.UTF8.GetBytes(text[i]),FontSlot=7+i,Height=height[i]});}
                layout.Align(labels.Measure);check(OriginalCashoutDisplay.Display(state,host)&&events==1&&state.Cursor==3&&state.Playback.Visible);
                var snapshot=labels.Describe();int[] top={280,343,458};
                for(int i=0;i<3;++i){check(snapshot[i].Text==text[i]&&snapshot[i].Height==height[i]&&snapshot[i].Bounds.Top==top[i]&&snapshot[i].Bounds.Left==680-snapshot[i].Width/2);check(snapshot[i].GlyphCount==text[i].Length);}
                save("primary");OriginalCashoutPlaybackLifecycle.Hide(state.Playback);check(!labels.Root.activeSelf&&state.Playback.Visible&&state.Playback.NeedsShow);save("hidden");
                OriginalCashoutPlaybackLifecycle.Show(state.Playback,life);check(labels.Root.activeSelf);save("shown");
                // Native lazy small-font replacement retains source line slot and
                // remeasures/re-aligns the label using the new font height/width.
                layout.ClearLinesForUpdate();state.Lines.Clear();string longText="Call Attendant Call Attendant Call Attendant";
                layout.AddSingleLine(Encoding.UTF8.GetBytes(longText),7,63,true);state.Lines.Add(new OriginalCashoutDisplay.Line {Text=Encoding.UTF8.GetBytes(longText),FontSlot=7,Height=63});layout.Align(labels.Measure);
                state.Fonts[10]=null;check(OriginalCashoutDisplay.Display(state,host)&&events==2&&state.Fonts[10]!=null);
                snapshot=labels.Describe();check(snapshot.Length==1&&snapshot[0].FontSlot==7&&snapshot[0].Height==33&&snapshot[0].Bounds.Top==353&&snapshot[0].Bounds.Left==680-snapshot[0].Width/2);save("small");
                // Alignment arithmetic is checked on the live label and actual font,
                // including short coordinate wrapping and all native alignment cases.
                layout.ClearLinesForUpdate();state.Lines.Clear();layout.AddSingleLine(Encoding.UTF8.GetBytes("Odd"),7,63,true);state.Lines.Add(new OriginalCashoutDisplay.Line {Text=Encoding.UTF8.GetBytes("Odd"),FontSlot=7,Height=63});
                var line=layout.Lines[0];int cases=0;
                foreach(int x in new[]{-32768,-1,0,101,32767})foreach(int y in new[]{-32768,-1,0,203,32767})foreach(int h in new[]{-1,0,1,99})foreach(int v in new[]{-1,0,1,99})
                {
                    ++cases;line.AnchorX=x;line.AnchorY=y;line.HorizontalAlignment=h;line.VerticalAlignment=v;labels.CreateDisplayTextLines(65539);
                    snapshot=labels.Describe();int anchorY=unchecked((short)(y+65539)),expectedX=x-(h==-1?0:h==1?snapshot[0].Width:snapshot[0].Width/2),expectedY=anchorY-(v==-1?0:v==1?63:31);
                    check(snapshot[0].Bounds.Left==unchecked((short)expectedX)&&snapshot[0].Bounds.Top==unchecked((short)expectedY)&&snapshot[0].Bounds.Right==unchecked((short)(expectedX+snapshot[0].Width))&&snapshot[0].Bounds.Bottom==unchecked((short)(expectedY+63)));
                    var before=snapshot[0].Bounds;state.Labels[0].SetBounds(new OriginalSpriteAlignment.Rectangle());check(state.Labels[0].GetBounds().Left==before.Left&&state.Labels[0].GetBounds().Top==before.Top);
                }
                check(cases==400);labels.Dispose();labels.Dispose();check(state.Labels.Count==0);
                File.WriteAllText(Path.Combine(output,"editor_report.json"),JsonUtility.ToJson(new Report {engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),checks=checks,captures=captures.ToArray()},true)+"\n");
            }
            finally
            {
                if(labels!=null)labels.Dispose();if(frames!=null)frames.Dispose();camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(canvasObject);UnityEngine.Object.DestroyImmediate(cameraObject);
            }
            return checks;
        }
    }
}
