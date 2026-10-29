using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static void RequireArchivedSasBaseRound(RecordedRound round,SasFrozenInputsV1 frozen,RoundResult basis,
   SasDecision decision,PendingSpinRecord pending,long walletAfter,bool published)
  {
   var expected=new RecordedRound{present=true,pocket=basis.Pocket,wager=basis.Wager,returned=decision.PostMultiplierAward,category=basis.Category,
    denomination=frozen.denomination,creditAfter=walletAfter,moneyRemainderCents=frozen.remainderCents,totalsRecorded=true,
    totalWagered=checked(frozen.priorWagered+basis.Wager),totalReturned=checked(frozen.priorReturned+decision.PostMultiplierAward),totalRounds=checked(frozen.priorRounds+1),totalWins=checked(frozen.priorWins+(basis.Returned>0?1:0)),
    wheelAngle=round.wheelAngle,ballX=round.ballX,ballY=round.ballY,sourceWheelStopRecorded=true,sourceWheelStopIndex=pending.wheelStopIndex,profile=frozen.profile,selectedChip=frozen.selectedChip,
    bets=frozen.bets,winningPays=basis.WinningPays.Select(p=>new RecordedPay{id=p.BetId,amount=p.Amount}).ToArray(),historyPayOverrides=new RecordedHistoryPayOverride[0],
    evaluationOrder=frozen.evaluationOrder,sourceTableBlock=Convert.ToBase64String(EncodeSourceTableBlock(frozen.bets)),spinJournalId=frozen.spinJournalId,roundArchiveId=frozen.roundArchiveId,awaitingFinalPayout=!published};
   if(float.IsNaN(round.wheelAngle)||float.IsInfinity(round.wheelAngle)||float.IsNaN(round.ballX)||float.IsInfinity(round.ballX)||float.IsNaN(round.ballY)||float.IsInfinity(round.ballY)||JsonUtility.ToJson(round)!=JsonUtility.ToJson(expected))
    throw new PcRoundLifecycleDataException("Archived SAS Base financial round differs");
  }
  // Reconstruct only completed SAS Base pages with admitted resources and limits.
  // Legacy and Double financial history requires its own admission path.
  private static RoundResult ValidateCompletedSasPriorBasePageV4(string encoded,string scope,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   SasFrozenInputsV1 frozen;var checkpoint=DecodeSasPageCheckpointAndInputsV4(encoded,out frozen);
   if(checkpoint.Scope!=scope)throw new PcRoundLifecycleDataException("Prior SAS page namespace differs");
   var basis=EvaluateDecodedSasBaseV4(frozen,suffix,maximumWin);
   PendingSpinRecord pending;ValidateDecodedSasReadyDigestV4(checkpoint,frozen,basis,doubleMaximumWin,doubleEnabled,suitMode,out pending);
   var decision=new SasDecision(checkpoint.Qualification,checkpoint.Display==null?null:checkpoint.Display.Observation,
    checkpoint.Recorded==null?null:checkpoint.Recorded.Observation,checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.Observation);
   var root=new LifecycleJsonReader(encoded).Read();var round=ParsePcReceiptContext<RecordedRound>(root.Members["round"].Text(encoded));
   RestoreLifecycleJsonNulls(root.Members["round"],round);
   RequireArchivedSasBaseRound(round,frozen,basis,decision,pending,checked(frozen.creditBefore+decision.PostMultiplierAward),true);
   var result=new RoundResult{Pocket=basis.Pocket,Wager=basis.Wager,Returned=decision.PostMultiplierAward,
    Category=basis.Category,WinLimitReached=basis.WinLimitReached};
   foreach(var pay in basis.WinningPays)result.WinningPays.Add(new WinningPayRecord{BetId=pay.BetId,Amount=pay.Amount,WinLevel=pay.WinLevel});
   return result;
  }
  private static void ValidateCompletedSasPriorPages(string[] pages,RecordedRound[] rounds,string scope,SasFrozenInputsV1 latest,
   string suffix,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   var newer=latest;
   for(int i=1;i<pages.Length;i++)
   {
    var root=new LifecycleJsonReader(pages[i]).Read();
    if(root.Members["originSchemaVersion"].Text(pages[i])!="4"){newer=null;continue;}
    ValidateCompletedSasPriorBasePageV4(pages[i],scope,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    var previous=rounds[i];
    if(newer!=null&&(newer.priorWagered!=previous.totalWagered||newer.priorReturned!=previous.totalReturned||
     newer.priorRounds!=previous.totalRounds||newer.priorWins!=previous.totalWins))
     throw new PcRoundLifecycleDataException("Prior SAS accounting differs from next round baseline");
    SasFrozenInputsV1 preceding;DecodeSasPageCheckpointAndInputsV4(pages[i],out preceding,true);newer=preceding;
    // A legacy intervening page does not carry the SAS frozen baseline.
    if(i+1<pages.Length&&new LifecycleJsonReader(pages[i+1]).Read().Members["originSchemaVersion"].Text(pages[i+1])!="4")newer=null;
   }
  }
  private static void ValidateCompletedSasOverflowV4(string overflowRaw,string[] pages,string scope,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   if(overflowRaw==null)return;
   var overflow=new LifecycleJsonReader(overflowRaw).Read();string removed=overflow.Members["page"].Text(overflowRaw);
   var removedRoot=new LifecycleJsonReader(removed).Read();
   if(removedRoot.Members["originSchemaVersion"].Text(removed)!="4")return;
   ValidateCompletedSasPriorBasePageV4(removed,scope,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   var oldestRoot=new LifecycleJsonReader(pages[pages.Length-1]).Read();
   if(oldestRoot.Members["originSchemaVersion"].Text(pages[pages.Length-1])!="4")return;
   SasFrozenInputsV1 oldest;DecodeSasPageCheckpointAndInputsV4(pages[pages.Length-1],out oldest,true);
   var round=ParsePcReceiptContext<RecordedRound>(removedRoot.Members["round"].Text(removed));
   if(oldest.priorWagered!=round.totalWagered||oldest.priorReturned!=round.totalReturned||oldest.priorRounds!=round.totalRounds||oldest.priorWins!=round.totalWins)
    throw new PcRoundLifecycleDataException("SAS overflow accounting differs from oldest retained baseline");
  }
  private static RoundResult[] ReconstructCompletedSasPriorHistory(ValidatedSasBaseContainer proof)
  {
   return proof.PageWrappers.Skip(1).Select(page=>ValidateCompletedSasPriorBasePageV4(page,proof.Checkpoint.Scope,proof.Suffix,
    proof.MaximumWin,proof.DoubleMaximumWin,proof.DoubleEnabled,proof.SuitMode)).ToArray();
  }
 }
}
