using System;
using System.IO;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // A new draw belongs to the ended predecessor, not to its consumed Base.
  // This proof is read-only and is rebuilt at each archive/cleanup boundary.
  private bool RequireSasSuccessorDrawV4(ValidatedSasBaseContainer proof)
  {
   var receipt=pcOutcome==null?null:pcOutcome.Receipt;
   if(receipt==null||JsonUtility.ToJson(receipt)==proof.Frozen.retainedDrawReceiptRaw||receipt.context.kind!=1)return false;
   if(proof.Checkpoint.Scope!=recordedHistoryKey||proof.Lifecycle.roundStage!=PcRoundStage.Ended||
    !proof.Payment.IsResolved||!proof.Payment.historyPublished)
    throw new PcRoundLifecycleDataException("SAS successor draw precedes paid End");
   PcRngDrawReceipt.Validate(receipt);string format;
   var context=DecodePcSpinDrawContext(receipt.context.contextJson,receipt,out format);var p=context.pending;
   if(format==null||p.profileIndex!=profileIndex||p.profile!=proof.Frozen.profile||
    p.baseLatestRoundId!=proof.Lifecycle.roundArchiveId||p.id==proof.Lifecycle.spinJournalId||
    p.baseHistoryCount!=proof.Rounds.Length||p.baseLatestPocket!=proof.Rounds[0].pocket||
    context.baseLatestPage!=JsonUtility.ToJson(proof.Rounds[0])||context.wagered!=proof.Rounds[0].totalWagered||
    context.returned!=proof.Rounds[0].totalReturned||context.rounds!=proof.Rounds[0].totalRounds||context.wins!=proof.Rounds[0].totalWins||
    context.creditEvent!=PlayerPrefs.GetString(PcCreditKey,""))
    throw new PcRoundLifecycleDataException("SAS successor context differs from paid predecessor");
   ValidateIdleLifecycleSuccessor(proof.Lifecycle,p.id);
   foreach(var round in proof.Rounds)if(round.spinJournalId==p.id||round.roundArchiveId==p.id)
    throw new PcRoundLifecycleDataException("SAS successor reuses an archived identity");
   var credit=ReadPcCreditEvent();long wallet=proof.Payment.WalletAfter,denomination=proof.Payment.denomination,remainder=proof.Payment.remainderCents;
   if(credit!=null)
   {
    if(credit.version!=2||credit.phase!=3||credit.anchor!=proof.Payment.roundArchiveId||credit.profile!=p.profile||
     credit.baseCredit!=wallet||credit.baseDenomination!=denomination||credit.baseRemainderCents!=remainder)
     throw new PcRoundLifecycleDataException("SAS successor credit owner differs");
    wallet=credit.after;denomination=credit.afterDenomination;remainder=credit.afterRemainderCents;
   }
   if(p.creditBefore!=wallet||p.denomination!=denomination||p.moneyRemainderCents!=remainder)
    throw new PcRoundLifecycleDataException("SAS successor financial baseline differs");
   string pendingRaw=PlayerPrefs.GetString(PendingSpinKey,"");PendingSpinRecord saved=null;
   if(PlayerPrefs.HasKey(PendingSpinKey))
   {
    string pendingFormat;saved=DecodePendingSpinEnvelope(pendingRaw,out pendingFormat);
    if(pendingFormat!=format)throw new PcRoundLifecycleDataException("SAS successor Pending format differs");
    RequireMatchingReceiptPending(saved,context,receipt);
    if(saved.hasWheelStopIndex&&(saved.wheelStopIndex<0||saved.wheelStopIndex>=37))
     throw new PcRoundLifecycleDataException("SAS successor wheel stop invalid");
   }
   if(File.Exists(PcSasIntentStatePath))
   {
    if(saved==null)throw new PcRoundLifecycleDataException("SAS successor intent lacks durable Pending");
    // ACK is process-local and is intentionally absent on cold admission.
    // Receipt/context/Pending have already matched without effects. Require
    // the durable wheel stop here; startup ACK follows financial restoration
    // and precedes the existing active-intent reader.
    if(!saved.hasWheelStopIndex)throw new PcRoundLifecycleDataException("SAS successor intent lacks saved wheel stop");
    var intent=SasIntentCheckpointCodecV1.DecodeForSpin(File.ReadAllBytes(PcSasIntentStatePath),recordedHistoryKey,p.id);
    var expected=CreateValidatedSasBaseSessionV4(proof,((RouletteSession)session).OutcomeSource,null);
    expected.ApplyPlatformMoneyState(wallet,denomination,remainder);
    expected.RestorePendingSpin(saved.pocket,wallet,denomination,p.selectedChip,DecodeStagedBets(p.bets),p.evaluationOrder,remainder);
    var candidate=expected.BuildPendingSettlementCandidate();
    ValidateSasEvaluatedInputs(intent.FrozenInputs,candidate,pendingRaw,recordedHistoryKey,intent.RoundId,p.id,
     PcReceiptTextDigest(proof.Encoded),PcReceiptTextDigest(PlayerPrefs.GetString(recordedHistoryKey,"")),
     PcReceiptTextDigest(PlayerPrefs.GetString(recordedHistoryKey+".Pages","")),receipt);
   }
   return true;
  }
 }
}
