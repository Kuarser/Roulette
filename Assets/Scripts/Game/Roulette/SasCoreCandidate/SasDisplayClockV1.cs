using System;
namespace RouletteDeluxe.SasDraftV1
{
    // One process clock. Only returned display state is durable; tick origins
    // and frequency never enter the archive and are recreated after cold load.
    public sealed class SasDisplayClockV1
    {
        public static SasDisplayClockV1 Start(SasDisplayStateV1 state)
        {return new SasDisplayClockV1(state,System.Diagnostics.Stopwatch.GetTimestamp(),System.Diagnostics.Stopwatch.Frequency);}
        public SasDisplayStateV1 CaptureNow(){return Capture(System.Diagnostics.Stopwatch.GetTimestamp());}
        private readonly long anchor,frequency,baseMilliseconds;
        private long lastSample;
        private long lastCapturedMilliseconds;
        private SasDisplayStateV1 saved;
        public SasDisplayClockV1(SasDisplayStateV1 state,long ticks,long ticksPerSecond)
        {
            if(state==null)throw new ArgumentNullException("state");state.Validate();
            if(!state.shown||ticks<0||ticksPerSecond<1)throw new PcRoundLifecycleDataException("SAS display clock initial state invalid");
            saved=state.Clone();baseMilliseconds=lastCapturedMilliseconds=state.accruedVisibleMilliseconds;anchor=lastSample=ticks;frequency=ticksPerSecond;
        }
        public SasDisplayStateV1 Capture(long ticks)
        {
            if(ticks<lastSample)throw new PcRoundLifecycleDataException("SAS monotonic display clock regressed");lastSample=ticks;
            if(saved.hideCompleted)return saved.Clone();
            long elapsed=checked((long)((decimal)(ticks-anchor)*1000/frequency));
            var next=saved.Clone();next.accruedVisibleMilliseconds=checked(baseMilliseconds+elapsed);
            SasDisplayProgressV1.RequireSuccessor(saved,next);lastCapturedMilliseconds=next.accruedVisibleMilliseconds;return next;
        }
        public void AcceptSaved(SasDisplayStateV1 state)
        {
            SasDisplayProgressV1.RequireSuccessor(saved,state);
            if(state.accruedVisibleMilliseconds!=lastCapturedMilliseconds)throw new PcRoundLifecycleDataException("Saved display duration was not captured by this clock");
            if(state.presentationGeneration!=saved.presentationGeneration)throw new PcRoundLifecycleDataException("Display reconstruction requires new process clock");
            saved=state.Clone();
        }
    }
}
