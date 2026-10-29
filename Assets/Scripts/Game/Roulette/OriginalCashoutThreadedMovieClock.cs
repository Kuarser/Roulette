using System;

namespace RouletteDeluxe
{
    // Forward, silent, infinite movie path used by CashOut. Native renderer
    // SSprite2DMovie SetFrame/PlayNow/Stop/UpdateForward, BAL_MS230C.
    public sealed class OriginalCashoutThreadedMovieClock
    {
        public readonly int FrameCount;
        public float FPS {get;private set;}
        public long FrameMicroseconds {get;private set;}
        public long LoopMicroseconds {get;private set;}
        public int Frame {get;private set;}
        public int LastCalculatedFrame {get;private set;}=-1;
        public int Iteration {get;private set;}
        public long AnchorMicroseconds {get;private set;}
        public long CarryMicroseconds {get;private set;}
        public bool Playing {get;private set;}
        public bool Stopped {get;private set;}
        bool infinite;
        public OriginalCashoutThreadedMovieClock(int count)
        {if(count<=0)throw new ArgumentOutOfRangeException(nameof(count));FrameCount=count;}
        public void SetFPS(float fps)
        {
            if(float.IsNaN(fps)||float.IsInfinity(fps)||fps<=0||fps>1000000)throw new ArgumentOutOfRangeException(nameof(fps));
            FPS=fps;FrameMicroseconds=(long)(1000000d/fps);LoopMicroseconds=(long)(FrameCount*1000000d/fps);
        }
        public void SetRepeatCount(int count)
        {if(count!=-1)throw new NotSupportedException("CashOut forward clock requires native repeat count -1");infinite=true;}
        public void SetFrame(int frame)
        {
            if(frame<0||frame>=FrameCount)throw new ArgumentOutOfRangeException(nameof(frame));
            if(frame==Frame)return;
            if(FrameMicroseconds==0)throw new InvalidOperationException("FPS required before movie seek");
            CarryMicroseconds=checked(FrameMicroseconds*frame);Frame=frame;LastCalculatedFrame=frame;Stopped=false;
        }
        public void PlayAt(long timestamp,long frequency)
        {
            if(Playing)return;
            if(FrameMicroseconds==0||!infinite)throw new InvalidOperationException("CashOut FPS and repeat count required");
            long now=OriginalFooterMovieClock.MicrosecondsAt(timestamp,frequency);
            if(Stopped){SetFrame(0);CarryMicroseconds=0;AnchorMicroseconds=0;LastCalculatedFrame=-1;Stopped=false;}
            AnchorMicroseconds=now;Playing=true;
        }
        public void Stop()
        {Iteration=0;Playing=false;Stopped=true;}
        public int UpdateAt(long timestamp,long frequency)
        {
            if(!Playing)return Frame;
            long now=OriginalFooterMovieClock.MicrosecondsAt(timestamp,frequency);
            if(now<AnchorMicroseconds)throw new ArgumentOutOfRangeException(nameof(timestamp));
            long elapsed=now-AnchorMicroseconds;
            long candidate=checked(elapsed+CarryMicroseconds)/FrameMicroseconds;
            if(candidate>=FrameCount)
            {
                long loops=candidate/FrameCount;candidate-=loops*FrameCount;
                CarryMicroseconds=checked(CarryMicroseconds-loops*LoopMicroseconds);
                // Native increments iteration once per update, even after skipping several loops.
                Iteration=unchecked(Iteration+1);
            }
            if(candidate!=LastCalculatedFrame)
            {CarryMicroseconds=checked(CarryMicroseconds+elapsed);AnchorMicroseconds=now;}
            if(candidate<0||candidate>=FrameCount)throw new InvalidOperationException("Native signed frame result outside qualified CashOut range");
            Frame=(int)candidate;LastCalculatedFrame=Frame;return Frame;
        }
    }
}
