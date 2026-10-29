using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // A transaction-local proof: never cached across events or persistence callbacks.
  private sealed class ValidatedSasBaseContainer
  {
   public string Encoded,Suffix,ReadySidecarRaw;public long MaximumWin,DoubleMaximumWin;public bool DoubleEnabled,SuitMode;
   public PcGamePaymentRecordV2 Payment;public SasIntentCheckpointV1 Checkpoint;public SasFrozenInputsV1 Frozen;
   public RoundResult Basis;public LifecycleJsonNode Root;public PendingSpinRecord Pending;
   public string[] PageWrappers;public RecordedRound[] Rounds;public RecordedRoundArchiveV4 Archive;
   public PcRoundLifecycleRecord Lifecycle;public SasDisplayStateV1 Display;public SasHistoryOverflowV2 Overflow;
  }
  // Independent latest Base-owner container checks. Older pages retain their
  // origin validators; this entry does not restore money or admit Double owners.
  private static PcGamePaymentRecordV2 ValidateArchivedSasBaseContainerV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   ValidatedSasBaseContainer proof;
   return ValidateArchivedSasBaseContainerProofV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof);
  }
  private static PcGamePaymentRecordV2 ValidateArchivedSasBaseContainerProofV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,out ValidatedSasBaseContainer proof,ValidatedSasBaseContainer adjacentReady=null)
  {
   return ValidateArchivedSasContainerProofV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof,adjacentReady,false);
  }
  private static PcGamePaymentRecordV2 ValidateArchivedSasLifecycleContainerV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,out ValidatedSasBaseContainer proof)
  {
   return ValidateArchivedSasContainerProofV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof,null,true);
  }
  private static PcGamePaymentRecordV2 ValidateArchivedSasContainerProofV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,out ValidatedSasBaseContainer proof,
   ValidatedSasBaseContainer adjacentReady,bool lifecycleContinuation)
  {
   proof=null;var containerTimer=BeginColdPaymentTiming();
   var root=new LifecycleJsonReader(encoded).Read();if(root.Kind!='{')throw new PcRoundLifecycleDataException("Archived container must be object");
   ValidateLifecycleJsonShape(root,typeof(RecordedRoundArchiveV4),encoded);
   var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(encoded);
   if(value.lifecycleSchemaVersion!=4||root.Members["featureNv"].Kind!='n'||root.Members["pages"].Kind!='['||root.Members["pages"].Items.Count<1||root.Members["historyState"].Kind!='{')
    throw new PcRoundLifecycleDataException("Archived Base container shape invalid");
   ColdPaymentTiming(containerTimer,"container-root-shape");
   string[] pages=root.Members["pages"].Items.Select(n=>n.Text(encoded)).ToArray();var rounds=ValidateCheckedContainerPageHistory(root.Members["pages"],encoded,value);
   ColdPaymentTiming(containerTimer,"container-history");
   string latest=pages[0];
   string sidecarRaw=root.Members["pages"].Items[0].Members["sas"].Text(encoded);
   SasIntentCheckpointV1 checkpoint;SasFrozenInputsV1 frozen;RoundResult basis;PendingSpinRecord pending;
   if(adjacentReady==null)
   {
    checkpoint=DecodeSasPageCheckpointAndInputsV4(latest,out frozen,true);
    ColdPaymentTiming(containerTimer,"container-checkpoint");
    basis=EvaluateDecodedSasBaseV4(frozen,suffix,maximumWin);
    ValidateDecodedSasReadyDigestV4(checkpoint,frozen,basis,doubleMaximumWin,doubleEnabled,suitMode,out pending);
   }
   else
   {
    // Immediate synchronous successor/owner checks use this proof. Check byte identity,
    // admission and current resource hashes; payment/history/lifecycle checks below remain independent.
    if(adjacentReady.ReadySidecarRaw!=sidecarRaw||adjacentReady.Suffix!=suffix||adjacentReady.MaximumWin!=maximumWin||
     adjacentReady.DoubleMaximumWin!=doubleMaximumWin||adjacentReady.DoubleEnabled!=doubleEnabled||adjacentReady.SuitMode!=suitMode)
     throw new PcRoundLifecycleDataException("Adjacent Ready proof differs from container admission");
    checkpoint=adjacentReady.Checkpoint;frozen=adjacentReady.Frozen;basis=adjacentReady.Basis;pending=adjacentReady.Pending;
    var currentProfile=RouletteProfile.Load(suffix);
    if(currentProfile.PaytableResourceSha256!=frozen.paytableSha256||currentProfile.GameConfigResourceSha256!=frozen.gameConfigSha256)
     throw new PcRoundLifecycleDataException("Adjacent Ready proof resources changed");
    ColdPaymentTiming(containerTimer,"container-adjacent-ready");
   }
   if(pages.Length!=Math.Min(frozen.priorHistoryCount+1,50))throw new PcRoundLifecycleDataException("Archived page count differs from prior history");
   ColdPaymentTiming(containerTimer,"container-base-ready");
   var decision=new SasDecision(checkpoint.Qualification,checkpoint.Display==null?null:checkpoint.Display.Observation,checkpoint.Recorded==null?null:checkpoint.Recorded.Observation,checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.Observation);
   var payment=DecodeSasPaymentV2(root.Members["payment"].Text(encoded),decision.Applied);
   RequireSasBaseOfferEligibility(payment,doubleEnabled&&decision.PostMultiplierAward>0&&
    (doubleMaximumWin==long.MaxValue||decision.PostMultiplierAward<(doubleMaximumWin>>(suitMode?2:1))));
   if(payment.owner!=PcGamePaymentOwner.Base||payment.roundArchiveId!=checkpoint.RoundId||payment.spinJournalId!=checkpoint.SpinId||payment.profile!=frozen.profile||
    payment.operationId!=checkpoint.RoundId+":game-payout"||payment.priorHistoryCount!=frozen.priorHistoryCount||payment.creditBefore!=frozen.creditBefore||
    payment.denomination!=frozen.denomination||payment.remainderCents!=frozen.remainderCents||payment.baseAward!=decision.PostMultiplierAward||
    payment.sasRoundDigest!=checkpoint.ReadyDigest||value.consumedSasIntentDigest!=checkpoint.ReadyDigest)
    throw new PcRoundLifecycleDataException("Archived Base payment identity differs");
   var pageRoot=new LifecycleJsonReader(latest).Read();var round=ParsePcReceiptContext<RecordedRound>(pageRoot.Members["round"].Text(latest));RestoreLifecycleJsonNulls(pageRoot.Members["round"],round);

   RequireArchivedSasBaseRound(round,frozen,basis,decision,pending,payment.WalletAfter,payment.historyPublished);
   ColdPaymentTiming(containerTimer,"container-payment-round");
   var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(encoded));RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
   ValidateLifecycleLatestIdentity(lifecycle,round);ValidateRoundLifecyclePayload(lifecycle,round);
   bool lifecycleStageAllowed=payment.historyPublished?
    lifecycle.roundStage==PcRoundStage.AwaitEnd||(lifecycleContinuation&&(lifecycle.roundStage==PcRoundStage.EndPrepared||lifecycle.roundStage==PcRoundStage.Ended)):
    lifecycle.roundStage==PcRoundStage.RoundOpen;
   if(!lifecycleStageAllowed)throw new PcRoundLifecycleDataException("Archived Base lifecycle stage differs");
   string overflow=root.Members["historyOverflow"].Text(encoded);if((overflow!="null")!=(frozen.priorHistoryCount==50&&!payment.historyPublished))throw new PcRoundLifecycleDataException("Published Base retains overflow");
   ValidateSasHistoryOverflow(overflow=="null"?null:overflow,pages,value.historyState);
   var display=DecodeSasCheckpointDisplay(root.Members["displayState"].Text(encoded),checkpoint);
   if(payment.prepared&&display!=null&&!display.hideCompleted)throw new PcRoundLifecycleDataException("Archived payment precedes Hide");
   ColdPaymentTiming(containerTimer,"container-lifecycle-overflow-display");
   ValidateCompletedSasPriorPages(pages,rounds,checkpoint.Scope,frozen,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   ValidateCompletedSasOverflowV4(overflow=="null"?null:overflow,pages,checkpoint.Scope,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   proof=new ValidatedSasBaseContainer{Encoded=encoded,Root=root,Suffix=suffix,ReadySidecarRaw=sidecarRaw,MaximumWin=maximumWin,DoubleMaximumWin=doubleMaximumWin,
    DoubleEnabled=doubleEnabled,SuitMode=suitMode,Payment=payment.Clone(),Checkpoint=checkpoint,Frozen=frozen,Basis=basis,Pending=pending,PageWrappers=pages,Rounds=rounds,Archive=value,
    Lifecycle=lifecycle,Display=display,Overflow=overflow=="null"?null:value.historyOverflow};
   return payment.Clone();
  }
 }
}
