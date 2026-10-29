using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed class OriginalCashoutThreadedMovie : IDisposable
    {
        public readonly OriginalCashoutMovieFrames Frames;
        public readonly OriginalCashoutThreadedMovieClock Clock;
        public readonly OriginalCashoutPlaybackLifecycle.Sprite Layer;
        readonly RawImage image;
        bool disposed;
        public OriginalCashoutThreadedMovie(OriginalCashoutMovieFrames frames,RawImage target,Func<long> timestamp,long frequency)
        {
            Frames=frames??throw new ArgumentNullException(nameof(frames));image=target??throw new ArgumentNullException(nameof(target));
            if(timestamp==null||frequency<=0)throw new ArgumentOutOfRangeException(nameof(frequency));
            Clock=new OriginalCashoutThreadedMovieClock(frames.Source.frameCount);
            var movie=new OriginalCashoutPlaybackLifecycle.Movie {SetFPS=Clock.SetFPS,SetRepeatCount=Clock.SetRepeatCount,
                Play=()=>{Clock.PlayAt(timestamp(),frequency);Present(Clock.Frame);},Stop=Clock.Stop};
            Layer=new OriginalCashoutPlaybackLifecycle.Sprite {SetFrame=index=>{Clock.SetFrame(checked((int)index));Present(Clock.Frame);},
                Hide=()=>image.gameObject.SetActive(false),Show=visible=>image.gameObject.SetActive(visible),AsMovie=()=>movie};
        }
        void Present(int frame){Frames.SetFrame(frame);image.texture=Frames.Texture;}
        public void TickAt(long timestamp,long frequency)
        {if(disposed)throw new ObjectDisposedException(nameof(OriginalCashoutThreadedMovie));Present(Clock.UpdateAt(timestamp,frequency));}
        public void Dispose(){if(disposed)return;disposed=true;Clock.Stop();Frames.Dispose();if(image!=null)image.texture=null;}
    }
    // Normal Unity Update supplies one shared monotonic timestamp to registered layers.
    // The containing screen owns Show/Pause/End; scene destruction releases frame textures.
    public sealed class OriginalCashoutThreadedMoviePump : MonoBehaviour
    {
        readonly List<OriginalCashoutThreadedMovie> layers=new List<OriginalCashoutThreadedMovie>();
        public OriginalCashoutThreadedMovie Bind(OriginalCashoutMovieFrames frames,RawImage image)
        {var layer=new OriginalCashoutThreadedMovie(frames,image,Stopwatch.GetTimestamp,Stopwatch.Frequency);layers.Add(layer);return layer;}
        void Update(){long now=Stopwatch.GetTimestamp();foreach(var layer in layers)layer.TickAt(now,Stopwatch.Frequency);}
        void OnDestroy(){foreach(var layer in layers)layer.Dispose();layers.Clear();}
    }
}
