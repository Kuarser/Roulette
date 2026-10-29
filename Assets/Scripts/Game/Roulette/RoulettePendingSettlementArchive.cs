using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static void ValidateSasCandidateRoundPage(RoulettePendingSettlementCandidate candidate,string encoded,SasIntentCheckpointV1 checkpoint,bool awaitingPayout)
  {
   if(candidate==null||candidate.Sas==null||checkpoint==null||checkpoint.Stage!=SasRoundIntentStageV1.ReadyCommit)throw new PcRoundLifecycleDataException("SAS archive page requires ReadyCommit candidate");
   var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
   if(SasReadyCommitDigest(BuildSasCheckpointReadyProjection(candidate,checkpoint,frozen.paytableMaximumWager,frozen.tableCount,frozen.staticMaxBetLimit,frozen.adapterPolicyId,frozen.displayPolicyId))!=checkpoint.ReadyDigest)throw new PcRoundLifecycleDataException("SAS archive candidate digest differs");
   var pending=ParseSasPendingSpinRaw(frozen.pendingSpinRaw);var page=ParsePcReceiptContext<RecordedRound>(encoded);
   var expected=BuildPendingSettlementRoundPage(candidate,checkpoint.RoundId,checkpoint.SpinId,pending.hasWheelStopIndex,pending.wheelStopIndex,page.wheelAngle,page.ballX,page.ballY,awaitingPayout);
   if(JsonUtility.ToJson(page)!=JsonUtility.ToJson(expected))throw new PcRoundLifecycleDataException("SAS archive page differs from frozen settlement candidate");
  }
  // Pure page candidate only. It is not accepted by the existing V3 SAS-unaware
  // decoder and is not published or saved until the V4 sidecar validator exists.
  private static RecordedRound BuildPendingSettlementRoundPage(
   RoulettePendingSettlementCandidate candidate,string archiveId,string journalId,
   bool wheelStopped,int wheelStopIndex,float wheelPhase,float ballX,float ballY,bool awaitingPayout)
  {
   if(candidate==null || string.IsNullOrWhiteSpace(archiveId) || string.IsNullOrWhiteSpace(journalId) ||
    (wheelStopped && (wheelStopIndex<0 || wheelStopIndex>=37)) ||
    float.IsNaN(wheelPhase)||float.IsInfinity(wheelPhase)||float.IsNaN(ballX)||float.IsInfinity(ballX)||float.IsNaN(ballY)||float.IsInfinity(ballY)||
    (awaitingPayout && !candidate.Deferred))throw new ArgumentException("Invalid pending settlement page context");
   var result=candidate.CopyResult();var stacks=candidate.CopyChipStacks();
   var bets=candidate.CopyBets().OrderBy(p=>p.Key).Select(p=>new RecordedBet{id=p.Key,amount=p.Value,chips=stacks[p.Key].ToArray()}).ToArray();
   return new RecordedRound{present=true,pocket=result.Pocket,wager=result.Wager,returned=result.Returned,category=result.Category,
    denomination=candidate.Denomination,creditAfter=candidate.CreditAtPublication,moneyRemainderCents=candidate.MoneyRemainderCents,
    totalsRecorded=true,totalWagered=candidate.WageredAfter,totalReturned=candidate.ReturnedAfter,totalRounds=candidate.RoundsAfter,totalWins=candidate.WinsAfter,
    wheelAngle=wheelPhase,ballX=ballX,ballY=ballY,sourceWheelStopRecorded=wheelStopped,sourceWheelStopIndex=wheelStopped?wheelStopIndex:0,
    profile=candidate.ProfileId,selectedChip=candidate.SelectedChip,bets=bets,
    winningPays=result.WinningPays.Select(p=>new RecordedPay{id=p.BetId,amount=p.Amount}).ToArray(),
    historyPayOverrides=result.HistoryPayOverrides.Select(p=>new RecordedHistoryPayOverride{payIndex=p.PayIndex,amount=p.Amount,progressiveLevel=p.ProgressiveLevel,kind=p.Kind}).ToArray(),
    evaluationOrder=candidate.CopyEvaluationOrder(),sourceTableBlock=Convert.ToBase64String(EncodeSourceTableBlock(bets)),
    spinJournalId=journalId,roundArchiveId=archiveId,awaitingFinalPayout=awaitingPayout};
  }
 }
}
