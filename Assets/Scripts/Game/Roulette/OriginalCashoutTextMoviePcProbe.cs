using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace RouletteDeluxe
{
    // Isolated executable acceptance fixture, no normal game/financial owner.
    public sealed class OriginalCashoutTextMoviePcProbe:MonoBehaviour
    {
        [Serializable] public sealed class Capture {public string label,file;public int backgroundFrame,foregroundFrame;public bool textVisible;public OriginalCashoutBitmapLabels.Snapshot[] labels;}
        [Serializable] public sealed class Report {public string engine,backend;public int width,height,checks,displayEvents,endEvents;public bool financialExecution;public Capture[] captures;}
        readonly List<Capture> captures=new List<Capture>();int checks,displayEvents,endEvents;string output;
        OriginalCashoutThreadedMoviePump pump;OriginalCashoutThreadedMovie background,foreground;
        OriginalCashoutPlaybackLifecycle.State playback;OriginalCashoutPlaybackLifecycle.Host life;
        OriginalCashoutDisplay.State display;OriginalCashoutDisplay.Host host;OriginalHandpayBannerLayout layout;OriginalCashoutBitmapLabels labels;
        float deadline;
        void Check(bool good){if(!good)throw new Exception("CashOut text/movie PC check "+checks);++checks;}
        void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline){Debug.LogError("CASHOUT_TEXT_MOVIE_PC_TIMEOUT");Application.Quit(9);}}
        IEnumerator Start()
        {
            deadline=Time.realtimeSinceStartup+90;var run=Run();
            while(true){object current=null;bool more;try{more=run.MoveNext();if(more)current=run.Current;}catch(Exception error){Debug.LogException(error);Application.Quit(8);yield break;}if(!more)yield break;yield return current;}
        }
        void SaveCapture(string label)
        {
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
            string file=label+".png";File.WriteAllBytes(Path.Combine(output,file),image.EncodeToPNG());Destroy(image);
            captures.Add(new Capture {label=label,file=file,backgroundFrame=background.Frames.FrameIndex,foregroundFrame=foreground.Frames.FrameIndex,textVisible=labels.Root.activeSelf,labels=labels.Describe()});
        }
        RawImage Image(Transform root,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));go.transform.SetParent(root,false);
            var image=go.GetComponent<RawImage>();image.raycastTarget=false;var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.sizeDelta=new Vector2(1360,768);return image;
        }
        void Add(string text,int slot,int height)
        {
            byte[] bytes=Encoding.UTF8.GetBytes(text);Check(layout.AddSingleLine(bytes,slot,height,true));display.Lines.Add(new OriginalCashoutDisplay.Line {Text=bytes,FontSlot=slot,Height=height});
        }
        IEnumerator Run()
        {
            Check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-cashoutTextMovieOutput");Check(index>=0&&index+1<args.Length);
            output=Path.GetFullPath(args[index+1]);Check(!Directory.Exists(output));Directory.CreateDirectory(output);
            yield return null;yield return new WaitForEndOfFrame();Check(Screen.width==1360&&Screen.height==768);
            var canvasObject=new GameObject("CashOut text/movie acceptance canvas",typeof(RectTransform),typeof(Canvas));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.pixelPerfect=true;
            pump=new GameObject("CashOut movie pump").AddComponent<OriginalCashoutThreadedMoviePump>();pump.enabled=false;var manifest=OriginalCashoutMovieFrames.LoadManifest();
            var bgImage=Image(canvasObject.transform,"Background");var fgImage=Image(canvasObject.transform,"Foreground");
            background=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Bottom-1360x768.bik",Application.streamingAssetsPath),bgImage);
            foreground=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Top-1360x768.bik",Application.streamingAssetsPath),fgImage);
            playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true,Background=background.Layer,Foreground=foreground.Layer};
            life=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,
                Emit=(bytes,p)=>{Check(bytes.Length==44&&bytes[25]==0x82&&p==0);++endEvents;},SetDefaults=()=>display.Layout.SetDefaults()};
            display=new OriginalCashoutDisplay.State {Playback=playback,Background=OriginalCashoutDisplay.BindImage(bgImage),Foreground=OriginalCashoutDisplay.BindImage(fgImage)};display.Layout.SetDefaults();
            string fontRoot=Path.Combine(Application.streamingAssetsPath,"Original/CashOutFontFiles");
            var binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),OriginalCashoutFontDescription.LocalReader(path=>
            {
                const string prefix="/agk/fonts/";if(!path.StartsWith(prefix,StringComparison.Ordinal)||path.Substring(prefix.Length).Contains("/")||path.Contains("\\"))throw new ArgumentException("Exact original font path required");
                return Path.Combine(fontRoot,path.Substring(prefix.Length));
            }));
            var setup=new OriginalCashoutTypographySetup();setup.ConstructorFontType(false,2,binding.SetUseFontType);
            setup.ConfigurePrimaryFonts(true,"/agk/fonts/CashOutFont-Msg-1360x768.fnt","/agk/fonts/CashOutFont-Amt-1360x768.fnt","/agk/fonts/CashOutFont-Trans-1360x768.fnt",1360,768,binding.Bind());
            binding.SetCustomFont(10,"/agk/fonts/CashOutFont-Msg-Small-1360x768.fnt",16);binding.SetCustomFont(12,"/agk/fonts/CashOutFont-Trans-Small-1360x768.fnt",16);
            Check(binding.Sizes.Values[7]==48&&binding.Sizes.Values[8]==96&&binding.Sizes.Values[9]==25&&File.Exists(Path.Combine(fontRoot,"manifest.json")));
            layout=new OriginalHandpayBannerLayout(1360,768){BorderWidth=0,Centering=true,NormalizedX=0,NormalizedY=0,NormalizedWidth=1,NormalizedHeight=1};
            labels=new OriginalCashoutBitmapLabels(canvasObject.transform,display,binding,layout.Lines,Encoding.UTF8.GetString);
            // Native label order = root+1; threaded foreground order = root+3.
            fgImage.transform.SetAsLastSibling();Check(bgImage.transform.GetSiblingIndex()<labels.Root.transform.GetSiblingIndex()&&fgImage.transform.GetSiblingIndex()>labels.Root.transform.GetSiblingIndex());
            var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("");host=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>layout.Centering,LookupTextBounds=table.Lookup,
                Emit=(bytes,p)=>{Check(bytes.Length==44&&bytes[25]==0x81&&p==0);++displayEvents;},Show=()=>OriginalCashoutPlaybackLifecycle.Show(playback,life)};labels.Bind(host);
            Add("Call Attendant",7,63);Add("RM9.95",8,115);Add("HAND PAY",9,30);layout.Align(labels.Measure);
            Check(OriginalCashoutDisplay.Display(display,host)&&playback.Visible&&labels.Root.activeSelf&&displayEvents==1);Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0);
            yield return new WaitForEndOfFrame();SaveCapture("initial");
            foreground.Layer.SetFrame(16);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==16&&labels.Root.activeSelf);SaveCapture("occlusion");
            foreground.Layer.SetFrame(0);yield return new WaitForEndOfFrame();pump.enabled=true;
            yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0&&background.Clock.Playing&&foreground.Clock.Playing);SaveCapture("advancing");Check(!OriginalCashoutPlaybackLifecycle.Show(playback,life));
            OriginalCashoutPlaybackLifecycle.Pause(playback,life,true);int bg=background.Frames.FrameIndex,fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg&&!background.Clock.Playing&&!foreground.Clock.Playing&&!labels.Root.activeSelf);SaveCapture("paused");
            pump.enabled=false;OriginalCashoutPlaybackLifecycle.Pause(playback,life,false);Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0&&labels.Root.activeSelf);
            yield return new WaitForEndOfFrame();SaveCapture("restarted");
            layout.ClearLinesForUpdate();display.Lines.Clear();Add("Call Attendant Call Attendant Call Attendant",7,63);layout.Align(labels.Measure);
            Check(OriginalCashoutDisplay.Display(display,host)&&displayEvents==2&&display.Fonts[10]!=null);var description=labels.Describe();Check(description.Length==1&&description[0].Height==33&&description[0].RenderFontSlot==10&&description[0].Top==353);
            yield return new WaitForEndOfFrame();SaveCapture("small");pump.enabled=true;
            yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0);
            OriginalCashoutPlaybackLifecycle.End(playback,life);bg=background.Frames.FrameIndex;fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();Check(!playback.Visible&&endEvents==1&&!labels.Root.activeSelf&&!background.Clock.Playing&&!foreground.Clock.Playing&&background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg);SaveCapture("ended");
            labels.Dispose();Destroy(pump.gameObject);yield return null;Check(background.Frames.Texture==null&&foreground.Frames.Texture==null&&display.Labels.Count==0);
            File.WriteAllText(Path.Combine(output,"player_report.json"),JsonUtility.ToJson(new Report {engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),width=Screen.width,height=Screen.height,checks=checks,displayEvents=displayEvents,endEvents=endEvents,captures=captures.ToArray()},true)+"\n");
            Debug.Log("CASHOUT_TEXT_MOVIE_PC_PASSED checks="+checks+" captures="+captures.Count);deadline=0;Application.Quit(0);
        }
    }
}
