using System;

namespace RouletteDeluxe
{
    // CFeatureObj::TransitionMovement 0x081100d0; PlayFeatPvt 0x0811c520
    // in RCGTopbox. Movement uses 33 ms ticks and float accumulation.
    public sealed class OriginalFeatureTransition
    {
        private float x, y, dx, dy;
        private readonly short endX, endY;
        private readonly bool sequential;
        private bool active = true;
        private bool finished;
        public const uint StopOpcode = uint.MaxValue;
        public const uint NextMilliseconds = 33;
        public Func<bool> IsOwnerActive;

        public OriginalFeatureTransition(short startX, short startY,
            short endX, short endY, int delayMilliseconds, bool sequential)
        {
            if(delayMilliseconds <= 0)throw new ArgumentOutOfRangeException("delayMilliseconds");
            x=startX;y=startY;this.endX=endX;this.endY=endY;this.sequential=sequential;
            // Native integer division precedes conversion to float; a small
            // positive delay can therefore produce infinity/NaN increments.
            float count=delayMilliseconds/33;
            dx=(endX-(int)startX)/count;dy=(endY-(int)startY)/count;
        }
        public void Stop() {active=false;}
        public uint Tick(Action<short,short> move,Action notify)
        {
            if(move==null||notify==null)throw new ArgumentNullException();
            if(!active||finished||IsOwnerActive!=null&&!IsOwnerActive())return StopOpcode;
            move(unchecked((short)(int)Math.Floor(x)),unchecked((short)(int)Math.Floor(y)));
            if(x==endX&&y==endY)
            {
                finished=true;
                if(!sequential)notify();
                return StopOpcode;
            }
            x=x+dx;y=y+dy;
            if(dx<0 ? x<endX : endX<x){x=endX;dx=0;}
            if(dy<0 ? y<endY : endY<y){y=endY;dy=0;}
            return NextMilliseconds;
        }
    }
}
