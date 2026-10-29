using System;
namespace RouletteDeluxe.SasDraftV1
{
    [Serializable]
    public sealed class SasDisplayStateV1
    {
        public string roundId,policyId;
        public bool shown,hideCompleted;
        public long accruedVisibleMilliseconds;
        public int displayMultiplier;
        public long presentationGeneration;
        public SasDisplayStateV1 Clone(){return (SasDisplayStateV1)MemberwiseClone();}
        public void Validate()
        {
            if(!PcRoundLifecycleRecord.IsCanonicalId(roundId)||(policyId!="StrictNativeType4"&&policyId!="PcNeutralTextV1")||
                displayMultiplier<0||displayMultiplier>255||accruedVisibleMilliseconds<0||presentationGeneration<0||
                (shown?presentationGeneration==0:accruedVisibleMilliseconds!=0||presentationGeneration!=0||hideCompleted)||
                (hideCompleted&&accruedVisibleMilliseconds<5000))
                throw new PcRoundLifecycleDataException("SAS display state invalid");
        }
    }
    public static class SasDisplayProgressV1
    {
        public static void RequireSuccessor(SasDisplayStateV1 before,SasDisplayStateV1 after)
        {
            if(before==null||after==null)throw new PcRoundLifecycleDataException("SAS display successor missing");before.Validate();after.Validate();
            if(!before.shown||!after.shown||before.roundId!=after.roundId||before.policyId!=after.policyId||before.displayMultiplier!=after.displayMultiplier||
                after.accruedVisibleMilliseconds<before.accruedVisibleMilliseconds||(before.hideCompleted&&!after.hideCompleted))
                throw new PcRoundLifecycleDataException("SAS display successor changes frozen identity or regresses");
            if(after.presentationGeneration!=before.presentationGeneration)
            {
                if(before.hideCompleted||after.hideCompleted||before.presentationGeneration==long.MaxValue||
                    after.presentationGeneration!=before.presentationGeneration+1||after.accruedVisibleMilliseconds!=before.accruedVisibleMilliseconds)
                    throw new PcRoundLifecycleDataException("SAS display reconstruction successor invalid");
            }
            if(before.hideCompleted&&after.accruedVisibleMilliseconds!=before.accruedVisibleMilliseconds)
                throw new PcRoundLifecycleDataException("Completed SAS display continues accruing");
        }
        public static SasDisplayStateV1 Shown(SasDisplayStateV1 state)
        {
            state.Validate();if(state.shown)throw new PcRoundLifecycleDataException("SAS display already shown");
            var next=state.Clone();next.shown=true;next.presentationGeneration=1;next.Validate();return next;
        }
        // Caller supplies only elapsed time in the current process since its last
        // saved progress. No wall time or cross-process monotonic origin is stored.
        public static SasDisplayStateV1 Accrue(SasDisplayStateV1 state,long elapsedMilliseconds)
        {
            state.Validate();if(!state.shown||elapsedMilliseconds<0)throw new PcRoundLifecycleDataException("SAS display elapsed time invalid");
            if(state.hideCompleted)return state.Clone();var next=state.Clone();next.accruedVisibleMilliseconds=checked(state.accruedVisibleMilliseconds+elapsedMilliseconds);next.Validate();return next;
        }
        public static int ServicedDelayMilliseconds(SasDisplayStateV1 state)
        {state.Validate();if(!state.shown)throw new PcRoundLifecycleDataException("SAS display not shown");return state.hideCompleted||state.accruedVisibleMilliseconds>=5000?0:(int)Math.Min(100,5000-state.accruedVisibleMilliseconds);}
        public static SasDisplayStateV1 Hide(SasDisplayStateV1 state)
        {
            state.Validate();if(!state.shown||state.accruedVisibleMilliseconds<5000)throw new PcRoundLifecycleDataException("SAS hide precedes visible deadline");
            var next=state.Clone();next.hideCompleted=true;next.Validate();return next;
        }
        public static SasDisplayStateV1 Reconstructed(SasDisplayStateV1 state)
        {
            state.Validate();if(!state.shown||state.hideCompleted)throw new PcRoundLifecycleDataException("SAS display cannot reconstruct");
            var next=state.Clone();next.presentationGeneration=checked(state.presentationGeneration+1);next.Validate();return next;
        }
    }
}
