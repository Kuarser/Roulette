using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed class OriginalCashoutMoviePcProbe:MonoBehaviour
    {
        [Serializable] public sealed class Capture {public string label,file;public int backgroundFrame,foregroundFrame;}
        [Serializable] public sealed class Report {public string engine,backend;public int width,height,checks;public bool financialExecution;public Capture[] captures;}
        readonly List<Capture> captures=new List<Capture>();int checks;string output;
        OriginalCashoutThreadedMoviePump pump;OriginalCashoutThreadedMovie background,foreground;
        OriginalCashoutPlaybackLifecycle.State state;OriginalCashoutPlaybackLifecycle.Host host;
        float deadline;
        void Check(bool good){if(!good)throw new Exception("CashOut PC probe check "+checks);++checks;}
        void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline){Debug.LogError("CASHOUT_MOVIE_PC_TIMEOUT");Application.Quit(9);}}
        IEnumerator Start()
        {
            deadline=Time.realtimeSinceStartup+90;
            var run=Run();
            while(true)
            {
                object current=null;bool more;
                try{more=run.MoveNext();if(more)current=run.Current;}
                catch(Exception error){Debug.LogException(error);Application.Quit(8);yield break;}
                if(!more)yield break;yield return current;
            }
        }
        void SaveCapture(string label)
        {
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
            string file=label+".png";File.WriteAllBytes(Path.Combine(output,file),image.EncodeToPNG());Destroy(image);
            captures.Add(new Capture {label=label,file=file,backgroundFrame=background.Frames.FrameIndex,foregroundFrame=foreground.Frames.FrameIndex});
        }
        RawImage Image(Transform root,string name)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));go.transform.SetParent(root,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            return go.GetComponent<RawImage>();
        }
        IEnumerator Run()
        {
            Check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-cashoutMovieOutput");Check(index>=0&&index+1<args.Length);
            output=Path.GetFullPath(args[index+1]);Check(!Directory.Exists(output));Directory.CreateDirectory(output);
            yield return null;yield return new WaitForEndOfFrame();Check(Screen.width==1360&&Screen.height==768);
            var canvasObject=new GameObject("CashOut probe canvas",typeof(RectTransform),typeof(Canvas));
            var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.pixelPerfect=true;
            pump=new GameObject("CashOut movie pump").AddComponent<OriginalCashoutThreadedMoviePump>();pump.enabled=false;
            var manifest=OriginalCashoutMovieFrames.LoadManifest();
            background=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Bottom-1360x768.bik",Application.streamingAssetsPath),Image(canvasObject.transform,"Background"));
            foreground=pump.Bind(new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-Top-1360x768.bik",Application.streamingAssetsPath),Image(canvasObject.transform,"Foreground"));
            state=new OriginalCashoutPlaybackLifecycle.State {Visible=true,ResourcesAvailable=true,ThreadedVideo=true,Background=background.Layer,Foreground=foreground.Layer};
            int events=0;host=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,
                Emit=(bytes,p)=>++events,SetDefaults=()=>{}};
            Check(OriginalCashoutPlaybackLifecycle.Show(state,host));Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0);
            yield return new WaitForEndOfFrame();SaveCapture("initial");
            pump.enabled=true;yield return new WaitForSecondsRealtime(.35f);yield return new WaitForEndOfFrame();
            Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0&&background.Clock.Playing&&foreground.Clock.Playing);
            SaveCapture("advancing");Check(!OriginalCashoutPlaybackLifecycle.Show(state,host));
            OriginalCashoutPlaybackLifecycle.Pause(state,host,true);int bg=background.Frames.FrameIndex,fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
            Check(background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg&&!background.Clock.Playing&&!foreground.Clock.Playing);SaveCapture("paused");
            pump.enabled=false;OriginalCashoutPlaybackLifecycle.Pause(state,host,false);
            Check(background.Frames.FrameIndex==0&&foreground.Frames.FrameIndex==0&&background.Clock.Playing&&foreground.Clock.Playing);
            yield return new WaitForEndOfFrame();SaveCapture("restarted");
            pump.enabled=true;yield return new WaitForSecondsRealtime(.2f);yield return new WaitForEndOfFrame();
            Check(background.Frames.FrameIndex>0&&foreground.Frames.FrameIndex>0);
            OriginalCashoutPlaybackLifecycle.End(state,host);bg=background.Frames.FrameIndex;fg=foreground.Frames.FrameIndex;
            yield return new WaitForSecondsRealtime(.15f);yield return new WaitForEndOfFrame();
            Check(!state.Visible&&events==1&&!background.Clock.Playing&&!foreground.Clock.Playing&&background.Frames.FrameIndex==bg&&foreground.Frames.FrameIndex==fg);SaveCapture("ended");
            Destroy(pump.gameObject);yield return null;Check(background.Frames.Texture==null&&foreground.Frames.Texture==null);
            File.WriteAllText(Path.Combine(output,"player_report.json"),JsonUtility.ToJson(new Report {engine=Application.unityVersion,
                backend=SystemInfo.graphicsDeviceType.ToString(),width=Screen.width,height=Screen.height,checks=checks,captures=captures.ToArray()},true)+"\n");
            Debug.Log("CASHOUT_MOVIE_PC_PASSED checks="+checks+" captures="+captures.Count);deadline=0;Application.Quit(0);
        }
    }
}

