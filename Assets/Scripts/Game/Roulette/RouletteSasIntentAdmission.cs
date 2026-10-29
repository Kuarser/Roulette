using System;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static void RequireSasIntentDrawAdmission(PcRngDrawReceipt receipt,bool unacknowledged,
   string scope,string profile,string spinId,int pocket,bool hasStop,int stopIndex)
  {
   if(receipt==null || unacknowledged || string.IsNullOrWhiteSpace(scope)||string.IsNullOrWhiteSpace(profile)||string.IsNullOrWhiteSpace(spinId)||
    !hasStop || stopIndex<0 || stopIndex>=37 || pocket<0 || pocket>36)
    throw new PcRoundLifecycleDataException("SAS intent requires acknowledged retained draw and saved wheel stop");
   PcRngDrawReceipt.Validate(receipt);
   if(receipt.context.kind!=1 || receipt.context.scope!=scope || receipt.context.profile!=profile ||
    receipt.context.operationId!=spinId || receipt.effectiveResult!=pocket)
    throw new PcRoundLifecycleDataException("SAS intent draw identity differs from PendingSpin");
  }
  private string CaptureSasIntentPendingRaw(RoulettePendingSettlementCandidate candidate)
  {
   if(candidate==null)throw new ArgumentNullException("candidate");
   candidate.RequireCurrentPendingBaseline(session as RouletteSession);
   string raw=CaptureSasIntentPendingRaw();
   RequireSasCandidatePendingRaw(candidate,raw,pendingSpin.id);
   return raw;
  }
  private static void RequireSasCandidatePendingRaw(RoulettePendingSettlementCandidate candidate,string raw,string spinId)
  {
   if(candidate==null||candidate.Sas!=null)throw new PcRoundLifecycleDataException("Initial SAS intent requires base evaluated candidate");
   var saved=ParseSasPendingSpinRaw(raw);var basis=candidate.CopyBaseResult();
   if(saved.version!=1||string.IsNullOrWhiteSpace(spinId)||saved.id!=spinId||saved.id.Length!=32||saved.profile!=candidate.ProfileId||saved.profileIndex<0||
    saved.pocket!=basis.Pocket||!saved.hasWheelStopIndex||saved.wheelStopIndex<0||saved.wheelStopIndex>=37||
    saved.creditBefore!=checked(candidate.CreditBefore+basis.Wager)||saved.wager!=basis.Wager||saved.denomination!=candidate.Denomination||
    saved.moneyRemainderCents!=candidate.MoneyRemainderCents||saved.selectedChip!=candidate.SelectedChip||saved.bets==null||saved.evaluationOrder==null||
    !saved.evaluationOrder.SequenceEqual(candidate.CopyEvaluationOrder()))throw new PcRoundLifecycleDataException("SAS pending scalar or evaluation order differs from candidate");
   var bets=candidate.CopyBets();var stacks=candidate.CopyChipStacks();
   if(saved.bets.Length!=bets.Count||saved.bets.Select(b=>b.id).Distinct().Count()!=saved.bets.Length)throw new PcRoundLifecycleDataException("SAS pending bet catalogue differs");
   foreach(var bet in saved.bets){long amount;if(!bets.TryGetValue(bet.id,out amount)||amount!=bet.amount||bet.chips==null||!bet.chips.SequenceEqual(stacks[bet.id]))throw new PcRoundLifecycleDataException("SAS pending bet or chip stack differs");}
  }
  private string CaptureSasIntentPendingRaw()
  {
   if(temporaryHistory || pcOutcome==null || pendingSpin==null || session==null || !session.SpinPending ||
    pcPreparedSpinDraw!=null || pcRestoredSpinDraw!=null)
    throw new PcRoundLifecycleDataException("SAS intent pending ownership not ready");
   string raw=PlayerPrefs.GetString(PendingSpinKey,"");
   string pendingFormat;var saved=ParseSasPendingSpinRaw(raw,out pendingFormat);
   if(JsonUtility.ToJson(saved)!=JsonUtility.ToJson(pendingSpin))throw new PcRoundLifecycleDataException("Saved pending differs from live journal");
   var receipt=pcOutcome.Receipt;
   RequireSasIntentDrawAdmission(receipt,pcOutcome.HasUnacknowledgedReceipt,recordedHistoryKey,session.Profile.Id,saved.id,session.PendingPocket,saved.hasWheelStopIndex,saved.wheelStopIndex);
   string receiptFormat;var context=DecodePcSpinDrawContext(receipt.context.contextJson,receipt,out receiptFormat);
   if(pendingFormat!=receiptFormat)throw new PcRoundLifecycleDataException("SAS intent format differs from retained draw");
   RequireMatchingReceiptPending(saved,context,receipt);
   RequirePcReceipt(context.baseLatestPage==PcReceiptLatestPage && saved.baseHistoryCount==recordedRoundPages.Count &&
    saved.baseLatestPocket==(recordedRoundPages.Count==0?-1:recordedRoundPages[0].pocket) &&
    context.creditEvent==PlayerPrefs.GetString(PcCreditKey,""),"SAS draw predecessor differs from loaded history or credit receipt");
   if(HasTrustedLatestLifecycle){var prior=GetLatestLifecycleForRestore();RequirePcReceipt(prior.roundStage==PcRoundStage.Ended&&prior.roundArchiveId==saved.baseLatestRoundId,"SAS draw lacks completed strong predecessor");ValidateIdleLifecycleSuccessor(prior,saved.id);}
   return raw;
  }
 }
}

