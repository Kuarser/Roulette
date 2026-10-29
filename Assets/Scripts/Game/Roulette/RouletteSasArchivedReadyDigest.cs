using System;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Resource-backed internal consistency check. The digest is not an external
  // trust anchor; cold restoration must additionally admit container ownership.
  private static string ValidateArchivedSasReadyDigestV4(string pageRaw,string admittedProfileSuffix,
   long admittedMaximumWin,long admittedDoubleMaximumWin,bool admittedDoubleEnabled,bool admittedSuitMode)
  {
   SasFrozenInputsV1 frozen;var checkpoint=DecodeSasPageCheckpointAndInputsV4(pageRaw,out frozen);
   var basis=EvaluateDecodedSasBaseV4(frozen,admittedProfileSuffix,admittedMaximumWin);
   PendingSpinRecord pending;return ValidateDecodedSasReadyDigestV4(checkpoint,frozen,basis,admittedDoubleMaximumWin,admittedDoubleEnabled,admittedSuitMode,out pending);
  }
  private static string ValidateDecodedSasReadyDigestV4(SasIntentCheckpointV1 checkpoint,SasFrozenInputsV1 frozen,RoundResult basis,
   long admittedDoubleMaximumWin,bool admittedDoubleEnabled,bool admittedSuitMode,out PendingSpinRecord admittedPending)
  {
   admittedPending=null;
   var readyTimer=BeginColdPaymentTiming();
   if(admittedDoubleMaximumWin<1)throw new PcRoundLifecycleDataException("Archived Double limit must be admitted");
   if(frozen.setupPolicyId!="PcSingleTableNoStaticCapV1"||frozen.tableCount!=1||frozen.staticMaxBetLimit!=0||
    frozen.adapterPolicyId!="SingleThreadKnownDispatchV1"||frozen.displayPolicyId!="StrictNativeType4"||
    frozen.effectiveMaximumWager!=SasEffectiveMaximumWager.Resolve(frozen.paytableMaximumWager,1,0)||
    frozen.creditBefore<0||frozen.priorWagered<0||frozen.priorReturned<0||frozen.priorRounds<0||frozen.priorWins<0||
    frozen.priorWins>frozen.priorRounds||frozen.priorHistoryCount<0||frozen.priorHistoryCount>50||!frozen.deferred||
    frozen.remainderCents<0||frozen.remainderCents>=frozen.denomination||
    PcReceiptTextDigest(frozen.pendingSpinRaw)!=frozen.pendingSpinSha256||
    !SasConfigEnvelopeCodecV3.IsDigest(frozen.priorArchiveRawSha256)||!SasConfigEnvelopeCodecV3.IsDigest(frozen.priorPocketsRawSha256)||!SasConfigEnvelopeCodecV3.IsDigest(frozen.priorPagesRawSha256))
    throw new PcRoundLifecycleDataException("Archived frozen setup or accounting invalid");
   ColdPaymentTiming(readyTimer,"ready-frozen-setup");
   string pendingFormat;var pending=ParseSasPendingSpinRaw(frozen.pendingSpinRaw,out pendingFormat);
   if(pending.version!=1||pending.profileIndex<0||pending.id!=checkpoint.SpinId||pending.profile!=frozen.profile||pending.pocket!=basis.Pocket||pending.wager!=basis.Wager||
    pending.creditBefore!=checked(frozen.creditBefore+basis.Wager)||pending.denomination!=frozen.denomination||
    pending.moneyRemainderCents!=frozen.remainderCents||pending.selectedChip!=frozen.selectedChip||
    JsonUtility.ToJson(new FrozenBetCatalogueV1{bets=pending.bets})!=JsonUtility.ToJson(new FrozenBetCatalogueV1{bets=frozen.bets})||
    pending.evaluationOrder==null||!pending.evaluationOrder.SequenceEqual(frozen.evaluationOrder))
    throw new PcRoundLifecycleDataException("Archived frozen pending differs from calculation");
   ColdPaymentTiming(readyTimer,"ready-pending");
   var receipt=ParsePcReceiptContext<PcRngDrawReceipt>(frozen.retainedDrawReceiptRaw);PcRngDrawReceipt.Validate(receipt);
   if(receipt.context.scope!=checkpoint.Scope||receipt.context.profile!=frozen.profile||receipt.context.operationId!=checkpoint.SpinId||receipt.context.kind!=1||receipt.effectiveResult!=basis.Pocket)
    throw new PcRoundLifecycleDataException("Archived draw receipt identity differs");
   string receiptFormat;var context=ParsePcSpinDrawContextRaw(receipt.context.contextJson,out receiptFormat);if(pendingFormat!=receiptFormat||context.version!=1||context.pending==null||!pending.hasWheelStopIndex||pending.wheelStopIndex<0||pending.wheelStopIndex>=37)throw new PcRoundLifecycleDataException("Archived wheel context or format invalid");RequireMatchingReceiptPending(pending,context,receipt);
   if(context.wagered!=frozen.priorWagered||context.returned!=frozen.priorReturned||context.rounds!=frozen.priorRounds||context.wins!=frozen.priorWins)
    throw new PcRoundLifecycleDataException("Archived draw receipt accounting differs");
   ColdPaymentTiming(readyTimer,"ready-receipt-context");
   Func<long,bool> doubleAvailable=stake=>admittedDoubleEnabled&&stake>0&&
    (admittedDoubleMaximumWin==long.MaxValue||stake<(admittedDoubleMaximumWin>>(admittedSuitMode?2:1)));
   if(frozen.baseDoubleAvailable!=doubleAvailable(basis.Returned))throw new PcRoundLifecycleDataException("Archived base Double offer differs");
   var decision=new SasDecision(checkpoint.Qualification,checkpoint.Display==null?null:checkpoint.Display.Observation,
    checkpoint.Recorded==null?null:checkpoint.Recorded.Observation,checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.Observation);
   var evidence=new PcSasDecisionEvidenceV1{version=1,scope=checkpoint.Scope,roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,profile=frozen.profile,
    adapterPolicyId=frozen.adapterPolicyId,displayPolicyId=frozen.displayPolicyId,paytableSha256=frozen.paytableSha256,paytableMaximumWager=frozen.paytableMaximumWager,
    tableCount=1,staticMaxBetLimit=0,effectiveMaximumWager=frozen.effectiveMaximumWager,evaluatedAward=basis.Returned,roundWager=basis.Wager,
    postMultiplierAward=decision.PostMultiplierAward,applied=decision.Applied,recordedMultiplier=decision.RecordedMultiplier,
    qualification=checkpoint.QualificationReceipt.ToPayload(),display=checkpoint.Display==null?null:checkpoint.Display.ToPayload(),
    recorded=checkpoint.Recorded==null?null:checkpoint.Recorded.ToPayload(),arithmetic=checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.ToPayload()};
   string decisionRaw=EncodeSasDecisionEvidence(evidence);
   ColdPaymentTiming(readyTimer,"ready-decision-evidence");
   var text=new StringBuilder("{\"projectionVersion\":1");
   Action<string,string> raw=(name,value)=>text.Append(',').Append(QuoteSasCanonical(name)).Append(':').Append(value);
   Action<string,long> number=(name,value)=>raw(name,value.ToString(CultureInfo.InvariantCulture));
   raw("pendingSpinRaw",QuoteSasCanonical(frozen.pendingSpinRaw));raw("pendingSpinSha256",QuoteSasCanonical(frozen.pendingSpinSha256));
   raw("priorArchiveRawSha256",QuoteSasCanonical(frozen.priorArchiveRawSha256));raw("priorPocketsRawSha256",QuoteSasCanonical(frozen.priorPocketsRawSha256));raw("priorPagesRawSha256",QuoteSasCanonical(frozen.priorPagesRawSha256));
   number("creditBefore",frozen.creditBefore);number("denomination",frozen.denomination);number("remainderCents",frozen.remainderCents);
   number("priorWagered",frozen.priorWagered);number("priorReturned",frozen.priorReturned);number("priorRounds",frozen.priorRounds);number("priorWins",frozen.priorWins);number("priorHistoryCount",frozen.priorHistoryCount);number("selectedChip",frozen.selectedChip);
   raw("deferred","true");raw("doubleAvailable",doubleAvailable(decision.PostMultiplierAward)?"true":"false");
   raw("bets","["+string.Join(",",frozen.bets.OrderBy(b=>b.id).Select(b=>JsonUtility.ToJson(b)).ToArray())+"]");
   raw("evaluationOrder","["+string.Join(",",frozen.evaluationOrder.Select(n=>n.ToString(CultureInfo.InvariantCulture)).ToArray())+"]");
   number("pocket",basis.Pocket);number("wager",basis.Wager);number("evaluatedAward",basis.Returned);number("category",basis.Category);raw("winLimitReached",basis.WinLimitReached?"true":"false");
   raw("winningPays","["+string.Join(",",basis.WinningPays.Select(pay=>"{\"id\":"+pay.BetId.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+pay.Amount.ToString(CultureInfo.InvariantCulture)+",\"winLevel\":"+pay.WinLevel.ToString(CultureInfo.InvariantCulture)+"}").ToArray())+"]");
   raw("decision",decisionRaw);string projection=text.Append('}').ToString();
   if(SasReadyCommitDigest(projection)!=checkpoint.ReadyDigest)throw new PcRoundLifecycleDataException("Archived ReadyCommit digest differs from resource-backed projection");
   ColdPaymentTiming(readyTimer,"ready-projection-digest");
   admittedPending=pending;return projection;
  }
  [Serializable] private sealed class FrozenBetCatalogueV1 { public RecordedBet[] bets; }
 }
}
