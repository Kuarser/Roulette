using System;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private int ServiceSasDisplayHideV4(SasDisplayClockV1 clock,RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
            string initialArchive,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory,long? controlledTicks=null)
        {
            if(sasDisplayStateV1==null)return 0;
            if(sasDisplayStateV1.hideCompleted)return 0;
            if(clock==null)throw new PcRoundLifecycleDataException("SAS hide lacks process clock");
            try
            {
                var captured=controlledTicks.HasValue?clock.Capture(controlledTicks.Value):clock.CaptureNow();
                int delay=SasDisplayProgressV1.ServicedDelayMilliseconds(captured);
                if(delay==0)captured=SasDisplayProgressV1.Hide(captured);
                if(captured.hideCompleted||captured.accruedVisibleMilliseconds-sasDisplayStateV1.accruedVisibleMilliseconds>=100)
                {
                    SaveSasDisplayProgressV4(captured,candidate,checkpoint,initialArchive,initialPage,priorPages,priorHistory);
                    clock.AcceptSaved(captured);
                }
                return delay;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-display-hide",error);throw;}
        }
    }
}
