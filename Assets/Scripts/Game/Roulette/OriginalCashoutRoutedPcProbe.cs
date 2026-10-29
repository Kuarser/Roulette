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
    public sealed class OriginalCashoutRoutedPcProbe:MonoBehaviour
    {
        [Serializable] public sealed class Capture {public string label,file;public int backgroundFrame,foregroundFrame;public bool textVisible;public OriginalCashoutBitmapLabels.Snapshot[] labels;}
        [Serializable] public sealed class Report {public string engine,backend;public int width,height,checks,displayEvents,endEvents;public bool financialExecution;public Capture[] captures;}
        readonly List<Capture> captures=new List<Capture>();int checks,displayEvents,endEvents;string output;
        OriginalCashoutThreadedMoviePump pump;OriginalCashoutThreadedMovie background,foreground;
        OriginalCashoutPlaybackLifecycle.State playback;OriginalCashoutPlaybackLifecycle.Host life;
        OriginalCashoutDisplay.State display;OriginalCashoutDisplay.Host host;OriginalCashoutBitmapLabels labels;OriginalCashoutBitmapScreen owner;OriginalCashoutFontBinding binding;OriginalLanguageTranslator translator;OriginalPlatformMonetaryLocale money;string language="en";
        float deadline;
        void Check(bool good){if(!good)throw new Exception("CashOut text/movie PC check "+checks);++checks;}
        void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline){Debug.LogError("CASHOUT_ROUTED_PC_TIMEOUT");Application.Quit(9);}}
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
        void Route(bool voucher,bool detail)
        {
            var route=new OriginalHandpayTextScreenRouter(new OriginalHandpayTextScreenRouter.Host {Manager3aa=true,Selected=()=>true,GameMode=()=>false,
                Alternate230=owner.RouterScreen,TwoLetterLanguage=()=>language=="en"?"en":language.Substring(0,2)});
            var content=OriginalHandpayBannerText.Create(995,voucher,false,true,true,v=>money.FormatBytes(v,100,false,0));
            var fourth=detail?Encoding.ASCII.GetBytes("Handpay_Required"):null;
            Check(route.Display(content.Title,content.Body,content.Amount,fourth));
            Check(!route.Display(content.Title,content.Body,content.Amount,fourth));
            Check(route.CurrentScreen==owner.RouterScreen&&route.MessageDisplayed&&display.Layout.Order==9100&&owner.FontLanguage=="en");
            var rows=labels.Describe();Check(rows.Length==3+(detail?1:0)+(language!="en"?1:0)&&rows[0].FontSlot==7&&rows[1].FontSlot==7&&rows[2].FontSlot==8);
        }
        short Measure(int slot,byte[] bytes)
        {
            var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int used=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);
            return unchecked((short)binding.LoadQualifiedAgkFont(slot).MeasureWidth(new string(chars,0,used)));
        }
        IEnumerator Run()
        {
            Check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-cashoutRoutedOutput");Check(index>=0&&index+1<args.Length);
            output=Path.GetFullPath(args[index+1]);Check(!Directory.Exists(output));Directory.CreateDirectory(output);
            yield return null;yield return new WaitForEndOfFrame();Check(Screen.width==1360&&Screen.height==768);
            var canvasObject=new GameObject("CashOut text/movie acceptance canvas",typeof(RectTransform),typeof(Canvas));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.pixelPerfect=true;
            pump=new GameObject("CashOut movie pump").AddComponent<OriginalCashoutThreadedMoviePump>();pump.enabled=false;var manifest=OriginalCashoutMovieFrames.LoadManifest();
            var bgImage=Image(canvasObject.transform,"Background");var fgImage=Image(canvasObject.transform,"Foreground");
            background=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Bottom-1360x768.bik",Application.streamingAssetsPath),bgImage);
            foreground=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Top-1360x768.bik",Application.streamingAssetsPath),fgImage);
            playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true,Background=background.Layer,Foreground=foreground.Layer};
            life=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,
                Emit=(bytes,p)=>{Check(bytes.Length==44&&bytes[25]==0x82&&p==0);++endEvents;},SetDefaults=()=>owner.Defaults()};
            display=new OriginalCashoutDisplay.State {Playback=playback,Background=OriginalCashoutDisplay.BindImage(bgImage),Foreground=OriginalCashoutDisplay.BindImage(fgImage)};display.Layout.SetDefaults();
            string fontRoot=Path.Combine(Application.streamingAssetsPath,"Original/CashOutFontFiles");
            binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),OriginalCashoutFontDescription.LocalReader(path=>
            {
                const string prefix="/agk/fonts/";if(!path.StartsWith(prefix,StringComparison.Ordinal)||path.Substring(prefix.Length).Contains("/")||path.Contains("\\"))throw new ArgumentException("Exact original font path required");
                return Path.Combine(fontRoot,path.Substring(prefix.Length));
            }));
            var setup=new OriginalCashoutTypographySetup();setup.ConstructorFontType(false,2,binding.SetUseFontType);
            setup.ConfigurePrimaryFonts(true,"/agk/fonts/CashOutFont-Msg-1360x768.fnt","/agk/fonts/CashOutFont-Amt-1360x768.fnt","/agk/fonts/CashOutFont-Trans-1360x768.fnt",1360,768,binding.Bind());
            binding.SetCustomFont(11,"/agk/fonts/CashOutFont-Amt-1360x768.fnt",16);binding.SetCustomFont(10,"/agk/fonts/CashOutFont-Msg-Small-1360x768.fnt",16);binding.SetCustomFont(12,"/agk/fonts/CashOutFont-Trans-Small-1360x768.fnt",16);
            Check(binding.Sizes.Values[7]==48&&binding.Sizes.Values[8]==96&&binding.Sizes.Values[9]==25&&File.Exists(Path.Combine(fontRoot,"manifest.json")));
            translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));
            money=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
            var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("");host=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>owner.Layout.Centering,LookupTextBounds=table.Lookup,
                Emit=(bytes,p)=>{Check(bytes.Length==44&&bytes[25]==0x81&&p==0);++displayEvents;},Show=()=>OriginalCashoutPlaybackLifecycle.Show(playback,life)};
            owner=new OriginalCashoutBitmapScreen(canvasObject.transform,1360,768,display,binding,host,life,"en",Encoding.UTF8.GetString,Measure,
                key=>Encoding.UTF8.GetBytes(translator.Translate(Encoding.UTF8.GetString(key),language,false,new string[0],null,null,null)));
            labels=owner.Labels;
            // Native label order = root+1; threaded foreground order = root+3.
            fgImage.transform.SetAsLastSibling();Check(bgImage.transform.GetSiblingIndex()<labels.Root.transform.GetSiblingIndex()&&fgImage.transform.GetSiblingIndex()>labels.Root.transform.GetSiblingIndex());
            Route(false,false);
            Check(playback.Visible&&labels.Root.activeSelf&&displayEvents==1);Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0);
            yield return new WaitForEndOfFrame();SaveCapture("initial");
            foreground.Layer.SetFrame(16);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==16&&labels.Root.activeSelf);SaveCapture("occlusion");
            foreground.Layer.SetFrame(0);yield return new WaitForEndOfFrame();pump.enabled=true;
            yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0&&background.Clock.Playing&&foreground.Clock.Playing);SaveCapture("advancing");Check(!OriginalCashoutPlaybackLifecycle.Show(playback,life));
            OriginalCashoutPlaybackLifecycle.Pause(playback,life,true);int bg=background.Frames.FrameIndex,fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg&&!background.Clock.Playing&&!foreground.Clock.Playing&&!labels.Root.activeSelf);SaveCapture("paused");
            pump.enabled=false;OriginalCashoutPlaybackLifecycle.Pause(playback,life,false);Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0&&labels.Root.activeSelf);
            yield return new WaitForEndOfFrame();SaveCapture("restarted");
            var primary=display.Fonts[7];Check(owner.Clear()&&display.Cursor==3&&display.Labels.Count==0&&display.Lines.Count==0&&display.Fonts[7]==primary);
            language="zh";Route(false,true);Check(displayEvents==2&&labels.Root.activeSelf&&background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0);
            yield return new WaitForEndOfFrame();SaveCapture("zh_detail");
            Check(owner.Clear()&&display.Cursor==5&&display.Labels.Count==0&&display.Fonts[7]==primary);
            language="en";Route(true,true);Check(displayEvents==3&&labels.Root.activeSelf);
            yield return new WaitForEndOfFrame();SaveCapture("voucher_detail");pump.enabled=true;
            yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0);
            Check(owner.End());bg=background.Frames.FrameIndex;fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();Check(!playback.Visible&&endEvents==1&&!labels.Root.activeSelf&&!background.Clock.Playing&&!foreground.Clock.Playing&&background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg);SaveCapture("ended");
            Check(display.Lines.Count==4&&display.Labels.Count==4);Check(owner.Clear()&&display.Cursor==4&&display.Labels.Count==0&&display.Fonts[7]==primary);owner.Dispose();Destroy(pump.gameObject);yield return null;Check(background.Frames.Texture==null&&foreground.Frames.Texture==null&&display.Labels.Count==0);
            File.WriteAllText(Path.Combine(output,"player_report.json"),JsonUtility.ToJson(new Report {engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),width=Screen.width,height=Screen.height,checks=checks,displayEvents=displayEvents,endEvents=endEvents,captures=captures.ToArray()},true)+"\n");
            Debug.Log("CASHOUT_ROUTED_PC_PASSED checks="+checks+" captures="+captures.Count);deadline=0;Application.Quit(0);
        }
    }
}
