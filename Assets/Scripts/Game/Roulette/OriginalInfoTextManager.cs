using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Native081ec670 KillTextScreen /081ec710 TimerCallback. Trigger clock and
    // Video.Flush are actual caller dependencies; the list is owned here.
    public sealed class OriginalInfoTextManager
    {
        private readonly List<OriginalCreditOutResidualScreens.Screen> screens=new List<OriginalCreditOutResidualScreens.Screen>();
        private readonly Action videoFlush;
        private readonly Action<OriginalCreditOutResidualScreens.Screen,uint> arm;
        private readonly Action<OriginalCreditOutResidualScreens.Screen,Func<uint>> setHandler;
        public IReadOnlyList<OriginalCreditOutResidualScreens.Screen> Screens{get{return screens;}}
        public OriginalInfoTextManager(Action videoFlush,Action<OriginalCreditOutResidualScreens.Screen,uint> arm,
            Action<OriginalCreditOutResidualScreens.Screen,Func<uint>> setHandler)
        {this.videoFlush=videoFlush??throw new ArgumentNullException(nameof(videoFlush));this.arm=arm??throw new ArgumentNullException(nameof(arm));this.setHandler=setHandler??throw new ArgumentNullException(nameof(setHandler));}
        public void Publish(OriginalCreditOutResidualScreens.Screen screen)
        {if(screen==null)throw new ArgumentNullException(nameof(screen));screens.Add(screen);}
        public void SetTimerHandler(OriginalCreditOutResidualScreens.Screen screen)
        {if(screen==null)throw new ArgumentNullException(nameof(screen));setHandler(screen,()=>TimerCallback(screen));}
        public void Arm(OriginalCreditOutResidualScreens.Screen screen,uint milliseconds){arm(screen,milliseconds);}
        public void Kill(OriginalCreditOutResidualScreens.Screen screen)
        {
            if(screen==null||screen.Clear==null||screen.End==null||screen.Delete==null)throw new ArgumentException("Actual InfoText screen destruction bindings required");
            screen.Clear();screen.End();videoFlush();arm(screen,uint.MaxValue);
            for(int i=0;i<screens.Count;++i)if(ReferenceEquals(screens[i],screen)){screens.RemoveAt(i);break;}
            screen.Delete();
        }
        public uint TimerCallback(OriginalCreditOutResidualScreens.Screen screen){Kill(screen);return uint.MaxValue;}
    }
}
