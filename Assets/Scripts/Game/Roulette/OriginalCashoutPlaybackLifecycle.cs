using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreenCashOut Show086c5330,Hide086c3ed0,Pause086c3d90,End086c3dd0.
    public static class OriginalCashoutPlaybackLifecycle
    {
        public sealed class Movie { public Action<float> SetFPS; public Action<int> SetRepeatCount; public Action Play,Stop; }
        public sealed class Sprite { public Action<uint> SetFrame; public Action Hide; public Action<bool> Show; public Func<Movie> AsMovie; }
        public sealed class Sound { public Action<int,int> Play; public Action Stop; }
        public sealed class Timer { public Action<uint,uint> SetPeriod; public Action<State> SetData; }
        public sealed class State
        {
            public bool Visible,ResourcesAvailable,ThreadedVideo,StaticBackground,Paused,NeedsShow=true;
            public Sprite Background,Foreground,Cascade,TextRoot,SecondaryTextRoot;
            public Sound Audio;
            public Timer AnimationTimer;
        }
        public sealed class Host
        {
            public Func<byte> GameStatusSecondByte;
            public Func<uint,bool> IsTiltActive;
            public Func<bool> IsMachineSuspended;
            public Func<uint,uint,Action<OriginalCashoutLayerAnimation.Timer>,Timer> CreateTimer;
            public Action<byte[],uint> Emit;
            public Action SetDefaults;
        }
        public static Sprite BindFrameLayer(OriginalCashoutMovieFrames frames,UnityEngine.UI.RawImage image,Movie movie)
        {
            if(frames==null||image==null)throw new ArgumentNullException();
            return new Sprite {SetFrame=index=>{frames.SetFrame(checked((int)index));image.texture=frames.Texture;},
                Hide=()=>image.gameObject.SetActive(false),Show=visible=>image.gameObject.SetActive(visible),AsMovie=()=>movie};
        }
        static void ResetLayer(Func<Sprite> get)
        {if(get()!=null){get().SetFrame(0);get().Hide();get().Show(true);}}
        static void PlayLayer(Func<Sprite> get)
        {if(get()!=null){get().AsMovie().SetFPS(24);get().AsMovie().SetRepeatCount(-1);get().AsMovie().Play();}}
        public static bool Show(State state,Host host)
        {
            if(state==null||host==null)throw new ArgumentNullException();
            if(!state.Visible||!state.NeedsShow||host.GameStatusSecondByte()!=0||host.IsTiltActive(0x1c000)||
                host.IsTiltActive(0x18000)||host.IsMachineSuspended())return false;
            ResetLayer(()=>state.Background);ResetLayer(()=>state.Foreground);ResetLayer(()=>state.Cascade);
            if(!state.ThreadedVideo)
            {
                if(state.AnimationTimer==null)
                {
                    state.AnimationTimer=host.CreateTimer(41,41,OriginalCashoutLayerAnimation.Animate);
                    state.AnimationTimer.SetData(state);
                }
                else state.AnimationTimer.SetPeriod(41,41);
            }
            else
            {
                if(!state.StaticBackground)PlayLayer(()=>state.Background);
                PlayLayer(()=>state.Foreground);PlayLayer(()=>state.Cascade);
            }
            if(state.Audio!=null)state.Audio.Play(100,999999);
            if(state.TextRoot!=null)state.TextRoot.Show(true);
            if(state.SecondaryTextRoot!=null)state.SecondaryTextRoot.Show(true);
            state.NeedsShow=false;return true;
        }
        public static bool Hide(State state)
        {
            if(state==null)throw new ArgumentNullException();
            state.NeedsShow=true;
            if(!state.ThreadedVideo){if(state.AnimationTimer!=null)state.AnimationTimer.SetPeriod(0,0);}
            else
            {
                if(state.Background!=null&&!state.StaticBackground)state.Background.AsMovie().Stop();
                if(state.Foreground!=null)state.Foreground.AsMovie().Stop();
                if(state.Cascade!=null)state.Cascade.AsMovie().Stop();
            }
            if(state.Audio!=null)state.Audio.Stop();
            if(state.TextRoot!=null)state.TextRoot.Hide();
            if(state.SecondaryTextRoot!=null)state.SecondaryTextRoot.Hide();
            return true;
        }
        public static void Pause(State state,Host host,bool paused)
        {
            if(state==null||host==null)throw new ArgumentNullException();
            state.Paused=paused;
            if(paused)Hide(state);else if(state.Visible)Show(state,host);
        }
        public static bool End(State state,Host host)
        {
            if(state==null||host==null)throw new ArgumentNullException();
            if(state.ResourcesAvailable)
            {
                if(state.Visible)
                {
                    state.Visible=false;
                    var raw=new byte[44];raw[0]=1;raw[2]=44;raw[24]=0;raw[25]=0x82;raw[26]=1;
                    host.Emit(raw,0);
                }
                Hide(state);
                if(state.AnimationTimer!=null)state.AnimationTimer.SetPeriod(0,0);
                state.Paused=false;host.SetDefaults();
            }
            return true;
        }
    }

    public static class OriginalCashoutPlaybackLifecycleValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut playback lifecycle regression "+checks);++checks;};
            // Every gate combination and owner presence combination, both video backends and static background.
            for(int gates=0;gates<64;++gates)for(int flags=0;flags<256;++flags)
            {
                bool visible=(gates&1)!=0,needs=(gates&2)!=0,blocked=(gates&4)!=0,tilt1=(gates&8)!=0,tilt2=(gates&16)!=0,suspended=(gates&32)!=0;
                bool threaded=(flags&1)!=0,still=(flags&2)!=0,bg=(flags&4)!=0,fg=(flags&8)!=0,cascade=(flags&16)!=0,
                    audio=(flags&32)!=0,root=(flags&64)!=0,secondary=(flags&128)!=0;
                var trace=new List<string>();
                Func<string,OriginalCashoutPlaybackLifecycle.Sprite> sprite=name=>new OriginalCashoutPlaybackLifecycle.Sprite {
                    SetFrame=i=>{check(i==0);trace.Add(name+".frame");},Hide=()=>trace.Add(name+".hide"),Show=b=>{check(b);trace.Add(name+".show");},
                    AsMovie=()=>{trace.Add(name+".cast");return new OriginalCashoutPlaybackLifecycle.Movie {
                        SetFPS=f=>{check(f==24);trace.Add(name+".fps");},SetRepeatCount=n=>{check(n==-1);trace.Add(name+".repeat");},
                        Play=()=>trace.Add(name+".play"),Stop=()=>trace.Add(name+".stop")};}};
                var state=new OriginalCashoutPlaybackLifecycle.State {Visible=visible,NeedsShow=needs,ThreadedVideo=threaded,StaticBackground=still,
                    Paused=true,ResourcesAvailable=false,Background=bg?sprite("bg"):null,Foreground=fg?sprite("fg"):null,Cascade=cascade?sprite("cascade"):null,
                    TextRoot=root?sprite("root"):null,SecondaryTextRoot=secondary?sprite("secondary"):null,
                    Audio=audio?new OriginalCashoutPlaybackLifecycle.Sound{Play=(a,b)=>{check(a==100&&b==999999);trace.Add("sound.play");},Stop=()=>trace.Add("sound.stop")}:null};
                var host=new OriginalCashoutPlaybackLifecycle.Host {
                    GameStatusSecondByte=()=>{trace.Add("status");return (byte)(blocked?255:0);},
                    IsTiltActive=mask=>{trace.Add("tilt:"+mask.ToString("x"));return mask==0x1c000?tilt1:tilt2;},
                    IsMachineSuspended=()=>{trace.Add("suspended");return suspended;},
                    CreateTimer=(a,b,callback)=>{check(a==41&&b==41&&callback==OriginalCashoutLayerAnimation.Animate);trace.Add("create");return new OriginalCashoutPlaybackLifecycle.Timer {
                        SetData=data=>{check(data==state&&state.AnimationTimer!=null);trace.Add("data");},
                        SetPeriod=(x,y)=>{check(x==y&&(x==0||x==41));trace.Add("timer:"+x);}};},
                    Emit=(bytes,p)=>{check(!state.Visible&&p==0&&bytes.Length==44);var expected=new byte[44];expected[0]=1;expected[2]=44;expected[25]=0x82;expected[26]=1;
                        check(Convert.ToBase64String(bytes)==Convert.ToBase64String(expected));trace.Add("event");},
                    SetDefaults=()=>{check(!state.Paused);trace.Add("defaults");}};
                var expectedShow=new List<string>();
                if(visible&&needs)expectedShow.Add("status");if(visible&&needs&&!blocked)expectedShow.Add("tilt:1c000");
                if(visible&&needs&&!blocked&&!tilt1)expectedShow.Add("tilt:18000");
                if(visible&&needs&&!blocked&&!tilt1&&!tilt2)expectedShow.Add("suspended");
                bool shown=visible&&needs&&!blocked&&!tilt1&&!tilt2&&!suspended;
                Action<string> reset=name=>expectedShow.AddRange(new[]{name+".frame",name+".hide",name+".show"});
                Action<string> play=name=>expectedShow.AddRange(new[]{name+".cast",name+".fps",name+".cast",name+".repeat",name+".cast",name+".play"});
                if(shown)
                {
                    if(bg)reset("bg");if(fg)reset("fg");if(cascade)reset("cascade");
                    if(!threaded)expectedShow.AddRange(new[]{"create","data"});else{if(bg&&!still)play("bg");if(fg)play("fg");if(cascade)play("cascade");}
                    if(audio)expectedShow.Add("sound.play");if(root)expectedShow.Add("root.show");if(secondary)expectedShow.Add("secondary.show");
                }
                check(OriginalCashoutPlaybackLifecycle.Show(state,host)==shown);check(string.Join("|",trace)==string.Join("|",expectedShow));
                check(state.Visible==visible&&state.Paused&&state.NeedsShow==(shown?false:needs));
                trace.Clear();check(!OriginalCashoutPlaybackLifecycle.Show(state,host));
                check(shown?trace.Count==0:string.Join("|",trace)==string.Join("|",expectedShow));
                var expectedHide=new List<string>();
                if(!threaded){if(shown)expectedHide.Add("timer:0");}
                else{if(bg&&!still)expectedHide.AddRange(new[]{"bg.cast","bg.stop"});if(fg)expectedHide.AddRange(new[]{"fg.cast","fg.stop"});if(cascade)expectedHide.AddRange(new[]{"cascade.cast","cascade.stop"});}
                if(audio)expectedHide.Add("sound.stop");if(root)expectedHide.Add("root.hide");if(secondary)expectedHide.Add("secondary.hide");
                trace.Clear();check(OriginalCashoutPlaybackLifecycle.Hide(state));check(string.Join("|",trace)==string.Join("|",expectedHide));check(state.Visible==visible&&state.NeedsShow);
                trace.Clear();check(OriginalCashoutPlaybackLifecycle.End(state,host));check(trace.Count==0&&state.Visible==visible&&state.Paused);
                state.ResourcesAvailable=true;trace.Clear();var expectedEnd=new List<string>();if(visible)expectedEnd.Add("event");expectedEnd.AddRange(expectedHide);
                if(!threaded&&shown)expectedEnd.Add("timer:0");expectedEnd.Add("defaults");
                check(OriginalCashoutPlaybackLifecycle.End(state,host));check(string.Join("|",trace)==string.Join("|",expectedEnd));check(!state.Visible&&!state.Paused&&state.NeedsShow);
            }
            // Existing timer restart and Pause's direct virtual Hide/Show behavior.
            var pauseTrace=new List<string>();var pausedState=new OriginalCashoutPlaybackLifecycle.State {Visible=true,AnimationTimer=new OriginalCashoutPlaybackLifecycle.Timer {
                SetPeriod=(a,b)=>{check(a==b);pauseTrace.Add("period:"+a);}}};
            var pauseHost=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>{check(!pausedState.Paused);pauseTrace.Add("status");return 0;},
                IsTiltActive=mask=>false,IsMachineSuspended=()=>false,CreateTimer=(a,b,c)=>{throw new Exception("Existing timer must be reused");}};
            OriginalCashoutPlaybackLifecycle.Pause(pausedState,pauseHost,true);check(pausedState.Paused&&pausedState.NeedsShow);
            OriginalCashoutPlaybackLifecycle.Pause(pausedState,pauseHost,false);check(!pausedState.Paused&&!pausedState.NeedsShow);
            check(string.Join("|",pauseTrace)=="period:0|status|period:41");
            pausedState.Visible=false;pauseTrace.Clear();OriginalCashoutPlaybackLifecycle.Pause(pausedState,pauseHost,false);check(pauseTrace.Count==0);
            // An existing timer is stopped by End even with threaded video; End does not destroy it.
            var threadedState=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true,Paused=true};
            var threadedTrace=new List<string>();var existing=new OriginalCashoutPlaybackLifecycle.Timer {
                SetPeriod=(a,b)=>{check(a==0&&b==0&&threadedState.Paused);threadedTrace.Add("stop");}};
            threadedState.AnimationTimer=existing;
            OriginalCashoutPlaybackLifecycle.End(threadedState,new OriginalCashoutPlaybackLifecycle.Host {SetDefaults=()=>threadedTrace.Add("defaults")});
            check(string.Join("|",threadedTrace)=="stop|defaults"&&threadedState.AnimationTimer==existing&&!threadedState.Paused);
            return checks;
        }
        public static int ValidateBoundFrames()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut bound frame lifecycle regression "+checks);++checks;};
            var manifest=OriginalCashoutMovieFrames.LoadManifest();
            var providers=new List<OriginalCashoutMovieFrames>();var images=new List<UnityEngine.UI.RawImage>();
            try
            {
                foreach(string name in new[]{"Bottom-1360x768","Top-1360x768","Cascade-Curve"})
                {
                    var provider=new OriginalCashoutMovieFrames(manifest,"/agk/picts/CashOutAnimation-"+name+".bik",UnityEngine.Application.streamingAssetsPath);
                    providers.Add(provider);provider.SetFrame(1);
                    var go=new UnityEngine.GameObject(name,typeof(UnityEngine.RectTransform),typeof(UnityEngine.CanvasRenderer),typeof(UnityEngine.UI.RawImage));
                    var image=go.GetComponent<UnityEngine.UI.RawImage>();images.Add(image);go.SetActive(false);
                }
                int creates=0,stops=0,events=0,defaults=0;
                var state=new OriginalCashoutPlaybackLifecycle.State {Visible=true,ResourcesAvailable=true,
                    Background=OriginalCashoutPlaybackLifecycle.BindFrameLayer(providers[0],images[0],null),
                    Foreground=OriginalCashoutPlaybackLifecycle.BindFrameLayer(providers[1],images[1],null),
                    Cascade=OriginalCashoutPlaybackLifecycle.BindFrameLayer(providers[2],images[2],null)};
                var host=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,
                    CreateTimer=(a,b,c)=>{++creates;return new OriginalCashoutPlaybackLifecycle.Timer {SetData=s=>{},SetPeriod=(x,y)=>{if(x==0&&y==0)++stops;}};},
                    Emit=(bytes,p)=>++events,SetDefaults=()=>++defaults};
                check(OriginalCashoutPlaybackLifecycle.Show(state,host));
                for(int i=0;i<3;++i)check(providers[i].FrameIndex==0&&images[i].texture==providers[i].Texture&&images[i].gameObject.activeSelf);
                check(!OriginalCashoutPlaybackLifecycle.Show(state,host)&&creates==1);
                OriginalCashoutPlaybackLifecycle.Pause(state,host,true);check(state.Paused&&state.NeedsShow&&stops==1&&events==0);
                for(int i=0;i<3;++i)check(images[i].gameObject.activeSelf&&providers[i].FrameIndex==0);
                OriginalCashoutPlaybackLifecycle.Pause(state,host,false);check(!state.Paused&&!state.NeedsShow&&creates==1);
                OriginalCashoutPlaybackLifecycle.End(state,host);check(!state.Visible&&state.NeedsShow&&stops==3&&events==1&&defaults==1);
                for(int i=0;i<3;++i)check(images[i].gameObject.activeSelf&&providers[i].Texture!=null);
            }
            finally
            {
                foreach(var image in images)UnityEngine.Object.DestroyImmediate(image.gameObject);
                foreach(var provider in providers)provider.Dispose();
            }
            return checks;
        }
    }
}
