using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalTextScreenFontLoadingValidation
    {
        [Serializable] private sealed class Reference {public string[] english,chinese;}
        [Serializable] public sealed class Capture {public string language,file;public OriginalCashoutBitmapLabels.Snapshot[] labels;}
        [Serializable] public sealed class Report {public int checks,displayEvents,endEvents,fontReleases;public string engine,backend;public bool financialExecution;public Capture[] captures;}
        public static int Validate(string nativeReference,string output)
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("TextScreen ordinary font loading check "+checks);++checks;};
            check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);check(!Directory.Exists(output));Directory.CreateDirectory(output);
            var reference=JsonUtility.FromJson<Reference>(nativeReference);int events=0,ends=0,releases=0;
            var state=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true}};
            var binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),path=>null);
            var cameraObject=new GameObject("Ordinary font loading camera");cameraObject.layer=31;var camera=cameraObject.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.cullingMask=1<<31;
            var canvasObject=new GameObject("Explicit font root",typeof(RectTransform),typeof(Canvas));canvasObject.layer=31;var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;canvas.pixelPerfect=true;
            var target=new RenderTexture(1360,768,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
            OriginalCashoutBitmapScreen screen=null;var captures=new List<Capture>();
            try
            {
                Func<int,byte[],short> measure=(slot,bytes)=>{var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int count=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);return unchecked((short)binding.LoadQualifiedAgkFont(slot).MeasureWidth(new string(chars,0,count)));};
                var life=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,Emit=(raw,p)=>{check(raw[25]==0x82&&p==0);++ends;},SetDefaults=()=>screen.Defaults()};
                var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("");var display=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>true,LookupTextBounds=table.Lookup,
                    Emit=(raw,p)=>{check(raw[25]==0x81&&p==0);++events;},Show=()=>OriginalCashoutPlaybackLifecycle.Show(state.Playback,life)};
                screen=new OriginalCashoutBitmapScreen(canvasObject.transform,1360,768,state,binding,display,life,"uninitialized",Encoding.UTF8.GetString,measure,key=>key);
                screen.BindFontLanguage(v=>v>=65&&v<=90?v+32:v,(slot,value)=>{check(slot<7&&value is OriginalBitmapFont);++releases;});
                int stage=0;
                foreach(string language in new[]{"en","zh","zh","chinese","zh_CN"})
                {
                    screen.Clear();var before=(object[])state.Fonts.Clone();int beforeReleases=releases;
                    screen.RouterScreen.SetLanguage(language);bool chinese=language=="zh"||language=="chinese";string text=chinese?"召唤服务员":"Call Attendant";
                    if(stage==2){check(releases==beforeReleases);for(int slot=0;slot<7;++slot)check(state.Fonts[slot]==before[slot]);}
                    else{for(int slot=0;slot<13;++slot)check(state.Fonts[slot]==null);}
                    for(int slot=0;slot<7;++slot)
                    {
                        string file=(chinese?reference.chinese:reference.english)[slot];check(binding.FileName(slot)==file);
                        screen.AddText(Encoding.UTF8.GetBytes(text),slot);var expected=OriginalBitmapFont.Load("OS366J/"+Path.GetFileNameWithoutExtension(file));
                        check(expected!=null&&state.Fonts[slot]==expected&&screen.Layout.Lines[slot].Height==expected.Height);
                        screen.AddText(new byte[0],slot);check(state.Lines.Count==slot+1); // No extra line; cached font survives empty text.
                    }
                    screen.Init();screen.RouterScreen.Display();check(events==stage+1&&state.Cursor==7&&screen.Labels.Root.activeSelf);
                    var rows=screen.Labels.Describe();check(rows.Length==7);int height=0;foreach(var line in state.Lines)height+=line.Height;int top=384-height/2;
                    for(int slot=0;slot<7;++slot){check(rows[slot].Text==text&&rows[slot].FontSlot==slot&&rows[slot].RenderFontSlot==slot&&rows[slot].Top==top&&rows[slot].Left==680-rows[slot].Width/2);top+=rows[slot].Height;}
                    if(chinese)check(state.Fonts[0]==state.Fonts[1]&&state.Fonts[1]==state.Fonts[2]&&rows[1].RenderFontSlot==1&&rows[2].RenderFontSlot==2);
                    foreach(var rect in canvasObject.GetComponentsInChildren<RectTransform>(true))rect.gameObject.layer=31;Canvas.ForceUpdateCanvases();camera.Render();var prior=RenderTexture.active;var image=new Texture2D(1360,768,TextureFormat.RGBA32,false);
                    string fileName="stage_"+stage+".png";
                    try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1360,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,fileName),image.EncodeToPNG());}
                    finally{RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(image);}
                    captures.Add(new Capture {language=language,file=fileName,labels=rows});++stage;
                }
                // Display's lazy replacement passes the font cached at slot+3.
                // Exercise that branch after the five ordinary-font captures.
                state.Labels[0].SetFont(state.Fonts[3]);var changed=screen.Labels.Describe()[0];
                check(changed.FontSlot==0&&changed.RenderFontSlot==3&&changed.Height==((OriginalBitmapFont)state.Fonts[3]).Height);
                check(screen.End()&&ends==1&&state.Lines.Count==7&&!screen.Labels.Root.activeSelf&&events==5);check(releases==21);
                File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(new Report {checks=checks,displayEvents=events,endEvents=ends,fontReleases=releases,engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),captures=captures.ToArray()},true)+"\n");
            }
            finally{if(screen!=null)screen.Dispose();camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(canvasObject);UnityEngine.Object.DestroyImmediate(cameraObject);}
            return checks;
        }
    }
}
