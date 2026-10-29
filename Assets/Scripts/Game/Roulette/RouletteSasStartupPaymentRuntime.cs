using System;
using RouletteDeluxe.SasDraftV1;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private IPcSasPaymentAdapterV1 sasStartupPaymentAdapter;
        private bool sasStartupPaymentServicing;
        private long sasStartupPaymentRetryDue;
        private string sasStartupPaymentRetryRound;
        private int sasStartupPaymentServiceFrame=-1;

        private void BindSasStartupPaymentAdapterV1(IPcSasPaymentAdapterV1 adapter)
        {
            if(adapter==null || sasStartupPaymentServicing || !sasStartupRuntimePending ||
                pcRoundArchiveSchemaVersion!=4 || pcRoundLifecycleStorageFaulted || sasInitialArchiveWriteFaulted ||
                sasStartupPaymentAdapter!=null)
                throw new PcRoundLifecycleDataException("SAS startup payment adapter cannot bind");
            sasStartupPaymentAdapter=adapter;
            // This does not release ordinary input or permit legacy successors.
        }

        private bool ServiceSasStartupBasePaymentV4()
        {
            if(!sasStartupRuntimePending ||
                pcRoundLifecycleStorageFaulted || sasInitialArchiveWriteFaulted)return false;
            if(sasStartupPaymentServicing)throw new PcRoundLifecycleDataException("SAS startup payment service reentered");
            sasStartupPaymentServicing=true;
            try
            {
                var local=session as RouletteSession;
                if(local==null || pcRoundArchiveSchemaVersion!=4 || pcSasGamePaymentV2==null ||
                    pcSasGamePaymentV2.owner!=PcGamePaymentOwner.Base)
                    throw new PcRoundLifecycleDataException("SAS startup payment has no Base owner");
                var current=pcSasGamePaymentV2;
                if(current.historyPublished)return true;
                // Double decisions and feature ownership require their own
                // controller; the payout driver cannot silently take an offer.
                if(local.DoubleAvailable || current.baseOffer==PcGamePaymentOffer.Enter)return false;
                bool publishing=current.IsResolved;
                if(sasStartupPaymentRetryRound!=current.roundArchiveId)
                {sasStartupPaymentRetryRound=current.roundArchiveId;sasStartupPaymentRetryDue=0;}
                if(!CurrentSasPaymentServiceGate().TryBeginEvent(Time.frameCount,PcGameAppSuspended,
                    sasDisplayStateV1==null || sasDisplayStateV1.hideCompleted,publishing))return false;
                sasStartupPaymentServiceFrame=Time.frameCount;
                // Reserve the serviced frame even while the ordinary 10 ms
                // retry is pending. A second caller in this frame cannot turn
                // an elapsed deadline into another request attempt.
                if(current.route==PcGamePaymentRouteV2.Ordinary && current.prepared && current.finalAward>0 &&
                    current.replyValue==0 && Stopwatch.GetTimestamp()<sasStartupPaymentRetryDue)return false;

                string key=recordedHistoryKey+".RoundDetails.v1", before=PlayerPrefs.GetString(key,"");
                ValidatedSasBaseContainer proof;
                var admitted=RequireColdSasBasePaymentOwnerProof(before,local.BaseMaximumWin,local.DoubleMaximumWin,
                    local.DoubleEnabled,local.DoubleSuitMode,null,out proof);
                bool applied=proof.Checkpoint.Qualification.Eligible;
                var virtualAdapter=sasStartupPaymentAdapter as PcVirtualSasPaymentAdapterV1;
                if(sasStartupPaymentAdapter==null||virtualAdapter!=null&&virtualAdapter.RoundArchiveId!=admitted.roundArchiveId)
                {
                    // The ordinary PC game already uses local virtual credits.
                    // Bind the same no-external-effect policy only after fresh
                    // archive admission; explicit transports keep their binding.
                    sasStartupPaymentAdapter=new PcVirtualSasPaymentAdapterV1(admitted,applied,
                        (saved,point)=>CapturePcSasLiveObservationV1(point));
                    Debug.Log("ROULETTE_SAS_PC_VIRTUAL_ADAPTER_BOUND: round="+admitted.roundArchiveId);
                }
                var adapter=sasStartupPaymentAdapter;
                var next=NextSasStartupPaymentEvent(admitted,applied,adapter);
                // Adapter callbacks are an external boundary even if they
                // returned 'waiting'. No effect may use a changed live owner.
                if(!ReferenceEquals(local,session) || !ReferenceEquals(adapter,sasStartupPaymentAdapter) ||
                    PlayerPrefs.GetString(key,"")!=before)
                    throw new PcRoundLifecycleDataException("SAS startup payment owner changed during adapter event");
                RequireColdSasBasePaymentOwner(before,local.BaseMaximumWin,local.DoubleMaximumWin,
                    local.DoubleEnabled,local.DoubleSuitMode,null);
                if(next==null)return false;
                CommitColdSasBasePaymentV4(next,local.BaseMaximumWin,local.DoubleMaximumWin,
                    local.DoubleEnabled,local.DoubleSuitMode,null);
                if(admitted.route==PcGamePaymentRouteV2.Ordinary && admitted.prepared && admitted.finalAward>0 &&
                    admitted.replyValue==0 && next.replyValue==0 && !next.moneyApplied)
                    sasStartupPaymentRetryDue=OriginalRackClock.AfterMilliseconds(Stopwatch.GetTimestamp(),10,Stopwatch.Frequency);
                return next.historyPublished;
            }
            catch(Exception error)
            {sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-startup-payment-runtime",error);throw;}
            finally{sasStartupPaymentServicing=false;}
        }

        private static PcGamePaymentRecordV2 NextSasStartupPaymentEvent(PcGamePaymentRecordV2 saved,bool applied,IPcSasPaymentAdapterV1 adapter)
        {
            // All external calls receive independent copies. Route and Packet
            // observations occur at their own live event boundaries.
            if(saved.IsResolved)return PcGamePaymentTransitionsV2.Publish(saved,applied);
            if(!saved.prepared)return PcGamePaymentTransitionsV2.Prepare(saved,applied,saved.baseAward,
                adapter.Observe(saved.Clone(),SasObservationPoint.Route));
            int reply;
            if(saved.route==PcGamePaymentRouteV2.Sas)
            {
                if(!saved.queryPrepared)return PcGamePaymentTransitionsV2.PrepareQuery(saved,applied);
                if(!saved.queryResponseSaved)return adapter.TryQuery(saved.Clone(),out reply)?
                    PcGamePaymentTransitionsV2.SaveQueryResponse(saved,applied,reply):null;
                if(saved.finalAward>0)
                {
                    if(!saved.packetPrepared)return PcGamePaymentTransitionsV2.PreparePacket(saved,applied,
                        adapter.Observe(saved.Clone(),SasObservationPoint.Packet));
                    if(!saved.dispatchReturned)return adapter.TryDispatch(saved.Clone(),out reply)?
                        PcGamePaymentTransitionsV2.SaveDispatchReturn(saved,applied,reply):null;
                }
            }
            else if(saved.finalAward>0 && saved.replyValue==0)return adapter.TryOrdinaryReply(saved.Clone(),out reply)?
                PcGamePaymentTransitionsV2.SaveOrdinaryReply(saved,applied,reply):null;
            if(!saved.moneyApplied)return PcGamePaymentTransitionsV2.Apply(saved,applied);
            if(saved.finalAward==0)return PcGamePaymentTransitionsV2.ResolveZero(saved,applied);
            if(!saved.creditNotified)return adapter.TryNotifyCredit(saved.Clone())?
                PcGamePaymentTransitionsV2.NotifyCredit(saved,applied):null;
            return adapter.TryReceiveCompletion(saved.Clone())?PcGamePaymentTransitionsV2.Complete(saved,applied):null;
        }
    }
}
