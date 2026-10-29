using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreenCashOut::AnimateLayers 086c3c70. Image slot08 and Timer remain host-owned.
    public static class OriginalCashoutLayerAnimation
    {
        public sealed class Image { public Action Advance; }
        public sealed class Owner
        {
            public Func<Image> Background, Foreground, Cascade;
            public bool StaticBackground;
        }
        public sealed class Timer
        {
            public Func<Owner> GetData;
            public Action<uint,uint> SetPeriod;
            public Action ClearQueuedEvents;
        }
        public static void Animate(Timer timer)
        {
            if(timer==null)throw new ArgumentNullException(nameof(timer));
            var owner=timer.GetData();
            if(owner!=null&&owner.Background()!=null&&owner.Foreground()!=null)
            {
                if(!owner.StaticBackground)owner.Background().Advance();
                owner.Foreground().Advance();
                if(owner.Cascade()!=null)owner.Cascade().Advance();
            }
            else timer.SetPeriod(0,0);
            timer.ClearQueuedEvents();
        }
    }
    public static class OriginalCashoutLayerAnimationValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut animation regression "+checks);++checks;};
            for(int flags=0;flags<32;++flags)
            {
                bool present=(flags&1)!=0,bg=(flags&2)!=0,fg=(flags&4)!=0,cascade=(flags&8)!=0,still=(flags&16)!=0;
                var trace=new List<string>();var owner=new OriginalCashoutLayerAnimation.Owner {StaticBackground=still,
                    Background=()=>{trace.Add("getBg");return bg?new OriginalCashoutLayerAnimation.Image{Advance=()=>trace.Add("bg")}:null;},
                    Foreground=()=>{trace.Add("getFg");return fg?new OriginalCashoutLayerAnimation.Image{Advance=()=>trace.Add("fg")}:null;},
                    Cascade=()=>{trace.Add("getCascade");return cascade?new OriginalCashoutLayerAnimation.Image{Advance=()=>trace.Add("cascade")}:null;}};
                var timer=new OriginalCashoutLayerAnimation.Timer {GetData=()=>{trace.Add("data");return present?owner:null;},
                    SetPeriod=(a,b)=>{check(a==0&&b==0);trace.Add("stop");},ClearQueuedEvents=()=>trace.Add("clear")};
                var expected=new List<string>{"data"};
                if(present)expected.Add("getBg");if(present&&bg)expected.Add("getFg");
                if(present&&bg&&fg)
                {if(!still){expected.Add("getBg");expected.Add("bg");}expected.Add("getFg");expected.Add("fg");expected.Add("getCascade");if(cascade){expected.Add("getCascade");expected.Add("cascade");}}
                else expected.Add("stop");expected.Add("clear");
                OriginalCashoutLayerAnimation.Animate(timer);check(string.Join(",",trace)==string.Join(",",expected));
                trace.Clear();OriginalCashoutLayerAnimation.Animate(timer);check(string.Join(",",trace)==string.Join(",",expected));
            }
            // Native repeats getters for the actual call, so use their second result.
            int bgCalls=0,fgCalls=0,calls=0,clears=0;
            var changing=new OriginalCashoutLayerAnimation.Owner {Background=()=>new OriginalCashoutLayerAnimation.Image{Advance=()=>{check(bgCalls==2);++calls;}},
                Foreground=()=>new OriginalCashoutLayerAnimation.Image{Advance=()=>{check(fgCalls==2);++calls;}},Cascade=()=>null};
            var background=changing.Background;var foreground=changing.Foreground;
            changing.Background=()=>{++bgCalls;return background();};changing.Foreground=()=>{++fgCalls;return foreground();};
            OriginalCashoutLayerAnimation.Animate(new OriginalCashoutLayerAnimation.Timer {GetData=()=>changing,SetPeriod=(a,b)=>{throw new Exception("Unexpected stop");},ClearQueuedEvents=()=>++clears});
            check(calls==2&&clears==1);return checks;
        }
    }
}
