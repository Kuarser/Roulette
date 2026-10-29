using System;
using System.Linq;
using System.Collections.Generic;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static RouletteSession CreateArchivedSasBaseSessionV4(string encoded,string suffix,long maximumWin,
   long doubleMaximumWin,bool doubleEnabled,bool suitMode,IRouletteOutcomeSource source,RoundResult[] admittedPriorHistory)
  {
   ValidatedSasBaseContainer proof;
   ValidateArchivedSasBaseContainerProofV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof);
   return CreateValidatedSasBaseSessionV4(proof,source,admittedPriorHistory);
  }
  private static RouletteSession CreateValidatedSasBaseSessionV4(ValidatedSasBaseContainer proof,
   IRouletteOutcomeSource source,RoundResult[] admittedPriorHistory)
  {
   if(proof==null)throw new PcRoundLifecycleDataException("Cold SAS restore lacks container proof");
   var payment=proof.Payment;var checkpoint=proof.Checkpoint;var frozen=proof.Frozen;var basis=proof.Basis;
   string suffix=proof.Suffix;long maximumWin=proof.MaximumWin,doubleMaximumWin=proof.DoubleMaximumWin;
   bool doubleEnabled=proof.DoubleEnabled,suitMode=proof.SuitMode;
   if(admittedPriorHistory==null)admittedPriorHistory=ReconstructCompletedSasPriorHistory(proof);
   RequireSasPriorHistoryBinding(proof.Rounds,frozen.priorHistoryCount,admittedPriorHistory);
   var decision=new SasDecision(checkpoint.Qualification,checkpoint.Display==null?null:checkpoint.Display.Observation,checkpoint.Recorded==null?null:checkpoint.Recorded.Observation,checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.Observation);
   var restored=new RouletteSession(RouletteProfile.Load(suffix),frozen.creditBefore,source,doubleMaximumWin,16,suitMode,maximumWin);restored.DoubleEnabled=doubleEnabled;
   if(restored.DoubleMaximumWin!=doubleMaximumWin)throw new PcRoundLifecycleDataException("Resolved cold Double cap differs from admission");
   bool available=doubleEnabled&&decision.PostMultiplierAward>0&&(doubleMaximumWin==long.MaxValue||decision.PostMultiplierAward<(doubleMaximumWin>>(suitMode?2:1)))&&
    !payment.moneyApplied&&(payment.baseOffer==PcGamePaymentOffer.None||payment.baseOffer==PcGamePaymentOffer.Open||payment.baseOffer==PcGamePaymentOffer.Enter);
   restored.RestoreSasBaseSnapshot(basis,decision,frozen.creditBefore,payment.moneyApplied,frozen.denomination,frozen.remainderCents,frozen.selectedChip,
    frozen.bets.ToDictionary(b=>b.id,b=>b.chips.ToList()),frozen.evaluationOrder,frozen.priorWagered,frozen.priorReturned,frozen.priorRounds,frozen.priorWins,admittedPriorHistory,available);
   // The original NV queue survives StatePlay and settlement. Its frozen
   // Pending is already part of the admitted Base owner; recovery must not
   // invent an empty queue when the independent sidecar was never written.
   var pending=ParseSasPendingSpinRaw(frozen.pendingSpinRaw);
   if(pending.hasTableUndo)restored.RestoreTableUndoState(pending.tableLastTouched,DecodeTableUndo(pending.tableUndo));
   if(payment.baseOffer==PcGamePaymentOffer.Take)restored.DeclineDoubleOffer();
   if(restored.Credit!=payment.WalletAfter)throw new PcRoundLifecycleDataException("Restored wallet differs from durable payment");return restored;
  }
 }
}
