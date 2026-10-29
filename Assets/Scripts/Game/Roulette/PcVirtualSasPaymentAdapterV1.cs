using System;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 // PC virtual-credit policy: no IPC, ledger, wallet or external money effect.
 // Query classification is ordinary (0), dispatch/ordinary return is accepted
 // (1). Every call is a deterministic candidate; the archive journals events.
 public sealed class PcVirtualSasPaymentAdapterV1 : IPcSasPaymentAdapterV1
 {
  private readonly PcGamePaymentRecordV2 origin;
  private readonly bool applied;
  private readonly Func<PcGamePaymentRecordV2,SasObservationPoint,PcSasObservationV1> observe;
  public string RoundArchiveId {get{return origin.roundArchiveId;}}
  public PcVirtualSasPaymentAdapterV1(PcGamePaymentRecordV2 owner,bool sasApplied,
   Func<PcGamePaymentRecordV2,SasObservationPoint,PcSasObservationV1> observer)
  {
   if(owner==null||observer==null)throw new ArgumentNullException("PC virtual SAS owner or observer");
   owner.Validate(sasApplied);origin=owner.Clone();applied=sasApplied;observe=observer;
  }
  private void RequireOwner(PcGamePaymentRecordV2 saved)
  {
   if(saved==null)throw new PcRoundLifecycleDataException("PC virtual SAS payment missing");saved.Validate(applied);
   if(saved.roundArchiveId!=origin.roundArchiveId||saved.spinJournalId!=origin.spinJournalId||saved.profile!=origin.profile||
    saved.operationId!=origin.operationId||saved.sasRoundDigest!=origin.sasRoundDigest||saved.baseAward!=origin.baseAward||
    saved.creditBefore!=origin.creditBefore||saved.denomination!=origin.denomination||saved.remainderCents!=origin.remainderCents||
    saved.priorHistoryCount!=origin.priorHistoryCount||saved.owner!=origin.owner||saved.featureId!=origin.featureId)
    throw new PcRoundLifecycleDataException("PC virtual SAS immutable owner differs");
  }
  private static void Require(bool value,string message)
  {if(!value)throw new PcRoundLifecycleDataException("PC virtual SAS "+message);}
  public PcSasObservationV1 Observe(PcGamePaymentRecordV2 saved,SasObservationPoint point)
  {
   RequireOwner(saved);
   Require(point==SasObservationPoint.Route&&!saved.prepared||point==SasObservationPoint.Packet&&saved.prepared&&
    saved.route==PcGamePaymentRouteV2.Sas&&saved.queryResponseSaved&&saved.finalAward>0&&!saved.packetPrepared,
    "observation is outside its admitted event");
   var result=observe(saved.Clone(),point);
   PcGamePaymentRecordV2.DecodeObservation(result,point);
   // Return an independent carrier even if an observer retains its own object.
   return new PcSasObservationV1{point=result.point,configGeneration=result.configGeneration,
    configRaw32Base64=result.configRaw32Base64,configRaw32Sha256=result.configRaw32Sha256,eventSource=result.eventSource,
    eventId=result.eventId,eventSequence=result.eventSequence,runtimePendingDisableObserved=result.runtimePendingDisableObserved};
  }
  public bool TryQuery(PcGamePaymentRecordV2 saved,out int response)
  {
   RequireOwner(saved);Require(saved.route==PcGamePaymentRouteV2.Sas&&saved.queryPrepared&&!saved.queryResponseSaved,
    "query precedes its durable intent or follows its response");response=0;return true;
  }
  public bool TryDispatch(PcGamePaymentRecordV2 saved,out int returned)
  {
   RequireOwner(saved);Require(saved.route==PcGamePaymentRouteV2.Sas&&saved.packetPrepared&&!saved.dispatchReturned,
    "dispatch precedes its durable packet or follows its return");returned=1;return true;
  }
  public bool TryOrdinaryReply(PcGamePaymentRecordV2 saved,out int reply)
  {
   RequireOwner(saved);Require(saved.prepared&&saved.route==PcGamePaymentRouteV2.Ordinary&&saved.finalAward>0&&
    saved.replyValue==0&&!saved.moneyApplied,"ordinary reply is outside its durable request");reply=1;return true;
  }
  public bool TryNotifyCredit(PcGamePaymentRecordV2 saved)
  {
   RequireOwner(saved);Require(saved.moneyApplied&&saved.finalAward>0&&!saved.creditNotified,"credit event is outside saved Apply");return true;
  }
  public bool TryReceiveCompletion(PcGamePaymentRecordV2 saved)
  {
   RequireOwner(saved);Require(saved.moneyApplied&&saved.finalAward>0&&saved.creditNotified&&!saved.completionReceived,
    "completion precedes Credit or follows saved completion");return true;
  }
 }
}
