using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Read-only plan. The new Base and its retained draw bind the immediate
  // predecessor. A prior auxiliary copy is obsolete only after that new
  // authoritative archive exists. Never apply its money or queue again.
  private Dictionary<string,string> PlanSasPredecessorJournalsV4(string encoded,RouletteSession limits)
  {
   var plan=new Dictionary<string,string>();
   var root=new LifecycleJsonReader(encoded).Read();
   if(root.Members["featureNv"].Kind!='n')return plan;
   ValidatedSasBaseContainer proof;
   ValidateArchivedSasLifecycleContainerV4(encoded,profiles[profileIndex],limits.BaseMaximumWin,
    limits.DoubleMaximumWin,limits.DoubleEnabled,limits.DoubleSuitMode,out proof);
   var receipt=ParsePcReceiptContext<PcRngDrawReceipt>(proof.Frozen.retainedDrawReceiptRaw);
   string format;var context=ParsePcSpinDrawContextRaw(receipt.context.contextJson,out format);var pending=context.pending;
   if(proof.Rounds.Length<2||string.IsNullOrEmpty(context.baseLatestPage))return plan;
   var prior=proof.Rounds[1];
   if(proof.Checkpoint.Scope!=recordedHistoryKey||context.baseLatestPage!=JsonUtility.ToJson(prior)||
    pending.baseLatestRoundId!=prior.roundArchiveId||pending.baseHistoryCount!=proof.Frozen.priorHistoryCount||
    pending.baseLatestPocket!=prior.pocket||context.rounds!=prior.totalRounds||context.wagered!=prior.totalWagered||
    context.returned!=prior.totalReturned||context.wins!=prior.totalWins)
    throw new PcRoundLifecycleDataException("SAS retired journals lack exact immediate predecessor");
   if(PlayerPrefs.HasKey(SettledTableUndoKey))
   {
    string raw=PlayerPrefs.GetString(SettledTableUndoKey);var undo=ParsePcReceiptContext<SettledTableUndoRecord>(raw);
    if(undo.roundArchiveId!=proof.Payment.roundArchiveId)
    {
     if(undo.version!=1||undo.profile!=prior.profile||undo.roundArchiveId!=prior.roundArchiveId)
      throw new PcRoundLifecycleDataException("Retired SAS Undo anchor differs");
     var oldRoot=new LifecycleJsonReader(proof.PageWrappers[1]).Read();
     if(oldRoot.Members["originSchemaVersion"].Text(proof.PageWrappers[1])=="4")
     {
      SasFrozenInputsV1 oldFrozen;DecodeSasPageCheckpointAndInputsV4(proof.PageWrappers[1],out oldFrozen);
      var oldPending=ParseSasPendingSpinRaw(oldFrozen.pendingSpinRaw);
      if(!oldPending.hasTableUndo||JsonUtility.ToJson(undo)!=JsonUtility.ToJson(new SettledTableUndoRecord{
       version=1,profile=prior.profile,roundArchiveId=prior.roundArchiveId,
       tableLastTouched=oldPending.tableLastTouched,tableUndo=oldPending.tableUndo}))
       throw new PcRoundLifecycleDataException("Retired SAS Undo differs from frozen predecessor queue");
     }
     // Legacy imported archives have no frozen queue; validate their optional
     // old copy against the archived table, then retire it without applying.
     var old=new RouletteSession(RouletteProfile.Load(prior.profile.Substring("RouletteDeluxeSZ".Length)),0,limits.OutcomeSource);
     old.RestoreStagedBets(0,prior.denomination,prior.selectedChip,DecodeStagedBets(prior.bets),undo.tableLastTouched,DecodeTableUndo(undo.tableUndo),prior.evaluationOrder);
     plan.Add(SettledTableUndoKey,raw);
    }
   }
   if(PlayerPrefs.HasKey(GameResultsClearedKey))
   {
    string raw=PlayerPrefs.GetString(GameResultsClearedKey);var view=ParsePcReceiptContext<ClearedGameResultsRecord>(raw);
    if(view.roundArchiveId!=proof.Payment.roundArchiveId)
    {
     if(view.version!=1||!view.cleared||view.profile!=prior.profile||view.roundArchiveId!=prior.roundArchiveId||view.legacyDigest!="")
      throw new PcRoundLifecycleDataException("Retired SAS presentation differs from predecessor");
     plan.Add(GameResultsClearedKey,raw);
    }
   }
   if(PlayerPrefs.HasKey(PcCreditKey))
   {
    string raw=PlayerPrefs.GetString(PcCreditKey);var credit=ParsePcReceiptContext<PcCreditEvent>(raw);
    if(credit.anchor!=proof.Payment.roundArchiveId)
    {
     credit=ReadPcCreditEvent();
     if(raw!=context.creditEvent||credit==null||credit.version!=2||credit.phase!=3||credit.anchor!=prior.roundArchiveId||
      credit.profile!=pending.profile||credit.after!=pending.creditBefore||credit.afterDenomination!=pending.denomination||
      credit.afterRemainderCents!=pending.moneyRemainderCents)
      throw new PcRoundLifecycleDataException("Retired SAS credit differs from retained draw context");
     plan.Add(PcCreditKey,raw);
    }
   }
   if(PlayerPrefs.HasKey(StagedBetKey))
   {
    string raw=PlayerPrefs.GetString(StagedBetKey);var table=ParsePcReceiptContext<StagedBetRecord>(raw);
    if(table.baseLatestRoundId!=proof.Payment.roundArchiveId)
    {
     if(table.version!=1||table.profile!=pending.profile||table.profileIndex!=pending.profileIndex||
      table.baseLatestRoundId!=prior.roundArchiveId||table.baseHistoryCount!=pending.baseHistoryCount||table.baseLatestPocket!=prior.pocket||
      table.credit!=pending.creditBefore||table.denomination!=pending.denomination||table.moneyRemainderCents!=pending.moneyRemainderCents||
      table.selectedChip!=pending.selectedChip||!table.hasUndoState||!pending.hasTableUndo||table.lastTouched!=pending.tableLastTouched||
      table.sourceGameplayState<0||table.sourceGameplayState>2||
      JsonUtility.ToJson(new FrozenBetCatalogueV1{bets=table.bets})!=JsonUtility.ToJson(new FrozenBetCatalogueV1{bets=pending.bets})||
      JsonUtility.ToJson(new SettledTableUndoRecord{tableUndo=table.undo})!=JsonUtility.ToJson(new SettledTableUndoRecord{tableUndo=pending.tableUndo})||
      table.evaluationOrder==null||pending.evaluationOrder==null||
      !(table.evaluationOrder.SequenceEqual(pending.evaluationOrder)||table.evaluationOrder.OrderBy(id=>id).SequenceEqual(pending.evaluationOrder)))
      throw new PcRoundLifecycleDataException("Retired SAS staged table differs from committed draw");
     var staged=new RouletteSession(limits.Profile,pending.creditBefore,limits.OutcomeSource);
     staged.RestoreStagedBets(table.credit,table.denomination,table.selectedChip,DecodeStagedBets(table.bets),table.lastTouched,
      DecodeTableUndo(table.undo),table.evaluationOrder,false,table.moneyRemainderCents);
     plan.Add(StagedBetKey,raw);
    }
   }
   if(plan.Count>0)
   {
    if(pcOutcome==null||pcOutcome.Receipt==null||JsonUtility.ToJson(pcOutcome.Receipt)!=proof.Frozen.retainedDrawReceiptRaw||
     PlayerPrefs.HasKey(PendingSpinKey)&&PlayerPrefs.GetString(PendingSpinKey)!=proof.Frozen.pendingSpinRaw)
     throw new PcRoundLifecycleDataException("SAS journal retirement lacks actual retained draw/Pending ownership");
    RequireSasConsumedIntentV4(proof);
    RequireSasStartupDisplayResourcesV4(proof.Display);
   }
   return plan;
  }
  private void RetireSasPredecessorJournalsV4(string encoded,Dictionary<string,string> plan)
  {
   if(plan.Count==0)return;
   if(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1")!=encoded)
    throw new PcRoundLifecycleDataException("SAS archive changed before retiring journals");
   foreach(var item in plan)if(PlayerPrefs.GetString(item.Key)!=item.Value)
    throw new PcRoundLifecycleDataException("SAS predecessor journal changed before retirement");
   var root=new LifecycleJsonReader(encoded).Read();SasFrozenInputsV1 frozen;
   DecodeSasPageCheckpointAndInputsV4(root.Members["pages"].Items[0].Text(encoded),out frozen);
   var deleted=new HashSet<string>();
   foreach(var item in plan)
   {
    LifecycleBeforeWrite("SasRetiredJournal");
    if(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1")!=encoded||pcOutcome==null||pcOutcome.Receipt==null||
     JsonUtility.ToJson(pcOutcome.Receipt)!=frozen.retainedDrawReceiptRaw||
     PlayerPrefs.HasKey(PendingSpinKey)&&PlayerPrefs.GetString(PendingSpinKey)!=frozen.pendingSpinRaw)
     throw new PcRoundLifecycleDataException("SAS retired journal owner changed at persistence seam");
    foreach(var remaining in plan)if(deleted.Contains(remaining.Key)?PlayerPrefs.HasKey(remaining.Key):
     !PlayerPrefs.HasKey(remaining.Key)||PlayerPrefs.GetString(remaining.Key)!=remaining.Value)
     throw new PcRoundLifecycleDataException("SAS retirement set changed at persistence seam");
    PlayerPrefs.DeleteKey(item.Key);PlayerPrefs.Save();
    if(PlayerPrefs.HasKey(item.Key))throw new PcRoundLifecycleDataException("SAS retired journal deletion readback differs");
    deleted.Add(item.Key);
    Debug.Log("ROULETTE_SAS_PREDECESSOR_JOURNAL_RETIRED: key="+item.Key);
    LifecycleCheckpoint("SasRetiredJournalSaved");
   }
   if(plan.ContainsKey(PcCreditKey))pcCreditOverride=null;
   if(plan.ContainsKey(StagedBetKey)){stagedBets=null;stagedBetsJson=null;}
  }
 }
}
