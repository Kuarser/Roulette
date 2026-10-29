using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private SasPaymentServiceGateV1 sasPaymentServiceGate=new SasPaymentServiceGateV1();
        private readonly int sasPaymentServiceOwnerThread=System.Threading.Thread.CurrentThread.ManagedThreadId;
        private string sasPaymentServiceGateRoundId;
        private SasPaymentServiceGateV1 CurrentSasPaymentServiceGate()
        {
            if(System.Threading.Thread.CurrentThread.ManagedThreadId!=sasPaymentServiceOwnerThread)
                throw new InvalidOperationException("SAS payment service must stay on its Game owner thread");
            if(pcSasGamePaymentV2==null)throw new PcRoundLifecycleDataException("SAS service has no round owner");
            if(sasPaymentServiceGateRoundId!=pcSasGamePaymentV2.roundArchiveId)
            {sasPaymentServiceGate=new SasPaymentServiceGateV1();sasPaymentServiceGateRoundId=pcSasGamePaymentV2.roundArchiveId;}
            return sasPaymentServiceGate;
        }
        private bool ServiceColdSasBasePaymentEventV4(bool publishing,Func<PcGamePaymentRecordV2,PcGamePaymentRecordV2> transition,
            long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,RoundResult[] priorHistory)
        {
            if(sasInitialArchiveWriteFaulted||pcRoundLifecycleStorageFaulted||pcSasGamePaymentV2==null||transition==null)
                throw new PcRoundLifecycleDataException("Cold SAS payment service missing or faulted");
            if(!CurrentSasPaymentServiceGate().TryBeginEvent(Time.frameCount,PcGameAppSuspended,
                sasDisplayStateV1==null||sasDisplayStateV1.hideCompleted,publishing))return false;
            try
            {
                var next=transition(pcSasGamePaymentV2.Clone());
                if(next==null||publishing!=(next.historyPublished&&!pcSasGamePaymentV2.historyPublished))
                    throw new PcRoundLifecycleDataException("Cold SAS publication gate differs from event");
                CommitColdSasBasePaymentV4(next,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory);return true;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-cold-payment-service",error);throw;}
        }
        private bool ServiceSasBasePaymentEventV4(bool publishing,Func<PcGamePaymentRecordV2,PcGamePaymentRecordV2> transition,
            RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,string initialArchive,string initialPage,
            string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            if(sasInitialArchiveWriteFaulted||pcSasGamePaymentV2==null||transition==null)
                throw new PcRoundLifecycleDataException("SAS payment service missing or faulted");
            if(!CurrentSasPaymentServiceGate().TryBeginEvent(Time.frameCount,PcGameAppSuspended,
                sasDisplayStateV1==null||sasDisplayStateV1.hideCompleted,publishing))return false;
            try
            {
                var next=transition(pcSasGamePaymentV2.Clone());
                if(next==null||publishing!=(next.historyPublished&&!pcSasGamePaymentV2.historyPublished))
                    throw new PcRoundLifecycleDataException("SAS event publication gate differs from transition");
                string saved=SaveSasBasePaymentArchiveV4(next,candidate,checkpoint,initialArchive,initialPage,priorPages,priorHistory);
                InstallSasBasePaymentArchiveV4(saved,candidate,checkpoint,initialPage,priorPages,priorHistory);return true;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-base-payment-service",error);throw;}
        }
    }
}
