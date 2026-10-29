using System;
using System.Threading;
namespace RouletteDeluxe.SasDraftV1
{
    // Volatile process state, deliberately absent from the payment JSON.
    public sealed class SasPaymentServiceGateV1
    {
        private readonly int ownerThread=Thread.CurrentThread.ManagedThreadId;
        private long lastEventFrame=-1;
        public bool Entered {get;private set;}
        public bool TryBeginEvent(long frame,bool appSuspended,bool hideCompleted,bool publishing)
        {
            if(Thread.CurrentThread.ManagedThreadId!=ownerThread)throw new InvalidOperationException("SAS service must run on its owner thread");
            if(frame<0||frame<lastEventFrame)throw new PcRoundLifecycleDataException("SAS service frame regressed");
            if(!Entered)
            {
                if(appSuspended||!hideCompleted)return false;Entered=true;
            }
            if(frame==lastEventFrame||(publishing&&appSuspended))return false;
            lastEventFrame=frame;return true;
        }
    }
}
