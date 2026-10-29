using System;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Admit auxiliary journals against the independently decoded paid Base,
  // before archive restoration can acknowledge a draw or repair any mirror.
  // This does not release the V4 Idle controller or ordinary-input gate.
  private void RequireSasPaidIdleJournalsV4(string encoded,RouletteSession limits,Dictionary<string,string> retired=null)
  {
   bool creditExists=PlayerPrefs.HasKey(PcCreditKey)&&(retired==null||!retired.ContainsKey(PcCreditKey));
   bool tableExists=PlayerPrefs.HasKey(StagedBetKey)&&(retired==null||!retired.ContainsKey(StagedBetKey));
   bool presentationExists=PlayerPrefs.HasKey(GameResultsClearedKey)&&(retired==null||!retired.ContainsKey(GameResultsClearedKey));
   bool undoExists=PlayerPrefs.HasKey(SettledTableUndoKey)&&(retired==null||!retired.ContainsKey(SettledTableUndoKey));
   if(!creditExists&&!tableExists&&!presentationExists&&!undoExists)return;
   CreateSasPaidIdleJournalSessionV4(encoded,limits,retired);
  }
  private RouletteSession CreateSasPaidIdleJournalSessionV4(string encoded,RouletteSession limits,Dictionary<string,string> retired=null)
  {
   bool creditExists=PlayerPrefs.HasKey(PcCreditKey)&&(retired==null||!retired.ContainsKey(PcCreditKey));
   bool tableExists=PlayerPrefs.HasKey(StagedBetKey)&&(retired==null||!retired.ContainsKey(StagedBetKey));
   bool presentationExists=PlayerPrefs.HasKey(GameResultsClearedKey)&&(retired==null||!retired.ContainsKey(GameResultsClearedKey));
   ValidatedSasBaseContainer proof;
   ValidateArchivedSasLifecycleContainerV4(encoded,profiles[profileIndex],limits.BaseMaximumWin,
    limits.DoubleMaximumWin,limits.DoubleEnabled,limits.DoubleSuitMode,out proof);
   if(proof.Checkpoint.Scope!=recordedHistoryKey||proof.Lifecycle.roundStage!=PcRoundStage.Ended||
    !proof.Payment.IsResolved||!proof.Payment.historyPublished)
    throw new PcRoundLifecycleDataException("V4 auxiliary journals precede paid End");
   if(presentationExists)
   {
    var view=ParsePcReceiptContext<ClearedGameResultsRecord>(PlayerPrefs.GetString(GameResultsClearedKey,""));
    if(view.version!=1||!view.cleared||view.profile!=proof.Frozen.profile||view.roundArchiveId!=proof.Payment.roundArchiveId||view.legacyDigest!="")
     throw new PcRoundLifecycleDataException("V4 cleared presentation differs from paid Base anchor");
   }
   var expected=CreateValidatedSasBaseSessionV4(proof,limits.OutcomeSource,null);
   if(PlayerPrefs.HasKey(SettledTableUndoKey)&&(retired==null||!retired.ContainsKey(SettledTableUndoKey)))
   {
    string raw=PlayerPrefs.GetString(SettledTableUndoKey,"");
    var undo=ParsePcReceiptContext<SettledTableUndoRecord>(raw);
    if(undo.version!=1||undo.profile!=proof.Frozen.profile||undo.roundArchiveId!=proof.Payment.roundArchiveId||
     undo.tableLastTouched!=expected.LastTouchedBet||JsonUtility.ToJson(undo)!=JsonUtility.ToJson(new SettledTableUndoRecord{
      version=1,profile=proof.Frozen.profile,roundArchiveId=proof.Payment.roundArchiveId,
      tableLastTouched=expected.LastTouchedBet,tableUndo=EncodeTableUndo(expected)}))
     throw new PcRoundLifecycleDataException("V4 settled Undo differs from frozen paid table owner");
    expected.RestoreTableUndoState(undo.tableLastTouched,DecodeTableUndo(undo.tableUndo));
   }
   PcCreditEvent credit=null;
   if(creditExists)
   {
    string raw=PlayerPrefs.GetString(PcCreditKey,"");
    ValidateLifecycleJsonShape(new LifecycleJsonReader(raw).Read(),typeof(PcCreditEvent),raw);
    credit=ReadPcCreditEvent();
    if(credit==null||credit.version!=2||credit.anchor!=proof.Payment.roundArchiveId||
     credit.profile!=expected.Profile.Id||credit.baseCredit!=proof.Payment.WalletAfter||
     credit.baseDenomination!=expected.Denomination||credit.baseRemainderCents!=expected.MoneyRemainderCents)
     throw new PcRoundLifecycleDataException("V4 credit journal differs from paid Base anchor");
    var idle=proof.Lifecycle.idleEntry;
    if(credit.kind=="collect"&&idle.consumedCollectEventId!=credit.id&&
     (idle.state!=PcIdleState.Complete||idle.prepareAccepted))
     throw new PcRoundLifecycleDataException("V4 collect journal precedes completed Idle");
    expected.ApplyPlatformMoneyState(credit.after,credit.afterDenomination,credit.afterRemainderCents);
    if(credit.kind=="denom")expected.SetChip(1);
    if(credit.kind=="collect"&&expected.ResultPhase)expected.NextRound();
   }
   if(tableExists)
   {
    string raw=PlayerPrefs.GetString(StagedBetKey,"");
    ValidateLifecycleJsonShape(new LifecycleJsonReader(raw).Read(),typeof(StagedBetRecord),raw);
    var table=JsonUtility.FromJson<StagedBetRecord>(raw);
    if(table==null||table.version!=1||table.profile!=expected.Profile.Id||table.profileIndex!=profileIndex||
     table.baseHistoryCount!=proof.Rounds.Length||table.baseLatestPocket!=proof.Rounds[0].pocket||
     table.baseLatestRoundId!=proof.Payment.roundArchiveId||table.bets==null||table.bets.Length>157||
     table.sourceGameplayState<0||table.sourceGameplayState>2||table.denomination<1||table.moneyRemainderCents<0||
     table.moneyRemainderCents>=table.denomination||
     (credit==null?table.credit!=expected.Credit||table.denomination!=expected.Denomination||table.moneyRemainderCents!=expected.MoneyRemainderCents:
      !(table.credit==credit.baseCredit&&table.denomination==credit.baseDenomination&&table.moneyRemainderCents==credit.baseRemainderCents||
        table.credit==credit.before&&table.denomination==credit.beforeDenomination&&table.moneyRemainderCents==credit.beforeRemainderCents||
        table.credit==credit.after&&table.denomination==credit.afterDenomination&&table.moneyRemainderCents==credit.afterRemainderCents)))
     throw new PcRoundLifecycleDataException("V4 staged table differs from paid Base anchor or credit journal");
    var stacks=DecodeStagedBets(table.bets);
    if(credit!=null&&credit.kind=="denom"&&stacks.Count!=0)
     throw new PcRoundLifecycleDataException("V4 denomination journal has a staged wager");
    bool denominationChanged=credit!=null&&credit.kind=="denom";
    expected.RestoreStagedBets(table.credit,credit==null?table.denomination:expected.Denomination,
     denominationChanged?1:table.selectedChip,stacks,denominationChanged?-1:table.hasUndoState?table.lastTouched:-1,
     denominationChanged||!table.hasUndoState?new RouletteSession.SavedUndoState[0]:DecodeTableUndo(table.undo),
     table.evaluationOrder,credit!=null,credit==null?table.moneyRemainderCents:expected.MoneyRemainderCents);
   }
   return expected;
  }
  private void RequireSasPaidIdleSessionV4(string encoded,RouletteSession local)
  {
   var expected=CreateSasPaidIdleJournalSessionV4(encoded,local);
   int[] savedOrder=null;
   if(PlayerPrefs.HasKey(StagedBetKey))savedOrder=JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey)).evaluationOrder;
   local.RequireSameSasIdleSnapshot(expected,savedOrder);
  }
 }
}
