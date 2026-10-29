using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Cold Base restoration after namespace, mirror and prior-history admission.
  // Exact consumed Pending and RNG ownership reconcile before publication.
  private RoundResult RestoreArchivedSasBaseV4(string readback,long maximumWin,long doubleMaximumWin,
   bool doubleEnabled,bool suitMode,IRouletteOutcomeSource source,RoundResult[] admittedPriorHistory)
  {
   return RestoreSasArchiveV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,source,admittedPriorHistory,false);
  }
  private RoundResult RestoreArchivedSasLifecycleV4(string readback,long maximumWin,long doubleMaximumWin,
   bool doubleEnabled,bool suitMode,IRouletteOutcomeSource source,RoundResult[] admittedPriorHistory)
  {
   return RestoreSasArchiveV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,source,admittedPriorHistory,true);
  }
  private RoundResult RestoreSasArchiveV4(string readback,long maximumWin,long doubleMaximumWin,
   bool doubleEnabled,bool suitMode,IRouletteOutcomeSource source,RoundResult[] admittedPriorHistory,bool lifecycleContinuation)
  {
   try
   {
    var previous=session as RouletteSession;
    if(temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||sasHistoryMirrorWriteInProgress||sasConsumedPendingWriteInProgress||previous==null||
     previous.LastResult!=null||previous.SpinPending||previous.InDoubleUp||previous.ResultPhase||previous.Bets.Count!=0||previous.History.Count!=0||
     !ReferenceEquals(source,previous.OutcomeSource)||profileIndex<0||profileIndex>=profiles.Length||
     PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
     throw new PcRoundLifecycleDataException("Cold SAS Base ownership is not ready");
    ValidatedSasBaseContainer proof;ValidatedSasDoubleEntry doubleEntry=null;
    var ownerRoot=new LifecycleJsonReader(readback).Read();
    if(ownerRoot.Members["featureNv"].Kind!='n')
    {
     if(lifecycleContinuation)throw new PcRoundLifecycleDataException("Double entry cannot own an End continuation");
     doubleEntry=ValidateArchivedSasDoubleActiveV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
     proof=doubleEntry.Container;
    }
    else if(lifecycleContinuation)ValidateArchivedSasLifecycleContainerV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof);
    else ValidateArchivedSasBaseContainerProofV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof);
    if(lifecycleContinuation&&proof.Lifecycle.roundStage!=PcRoundStage.EndPrepared&&proof.Lifecycle.roundStage!=PcRoundStage.Ended)
     throw new PcRoundLifecycleDataException("Cold SAS lifecycle restore requires a saved End owner");
    var checkpoint=proof.Checkpoint;if(checkpoint.Scope!=recordedHistoryKey)throw new PcRoundLifecycleDataException("Cold SAS scope differs from selected namespace");
    if(sasStartupRuntimePending)RequireSasStartupDisplayResourcesV4(proof.Display);
    RequireSasSuccessorDrawV4(proof);
    RequireSasConsumedIntentV4(proof);
    var restored=doubleEntry==null?CreateValidatedSasBaseSessionV4(proof,source,admittedPriorHistory):
     CreateValidatedSasDoubleActiveSessionV4(doubleEntry,source,admittedPriorHistory);
    var value=proof.Archive;var pages=proof.Rounds;var wrappers=proof.PageWrappers;
    var snapshots=value.historyState.snapshotsBase64.Select(Convert.FromBase64String).ToArray();
    string pockets=string.Join(",",value.historyState.pockets.Select(p=>p.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray());
    string mirror=string.Join(";",value.historyState.snapshotsBase64);
    if(lifecycleContinuation)ReconcileSasLifecycleHistoryMirrorsV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    else ReconcileSasHistoryMirrorsV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    var payment=proof.Payment.Clone();var display=proof.Display;var lifecycle=proof.Lifecycle;var overflow=proof.Overflow;
    recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,pages.Length);recordedSnapshots.Capacity=Math.Max(recordedSnapshots.Capacity,snapshots.Length);
    recordedPockets.Capacity=Math.Max(recordedPockets.Capacity,value.historyState.pockets.Length);sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,wrappers.Length);
    if(!ReferenceEquals(session,previous)||recordedHistoryKey!=checkpoint.Scope||profiles[profileIndex]!=proof.Suffix||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback||
     PlayerPrefs.GetString(recordedHistoryKey,"")!=pockets||PlayerPrefs.GetString(recordedHistoryKey+".Pages","")!=mirror)
     throw new PcRoundLifecycleDataException("Cold SAS durable ownership changed before publication");
    ReconcileSasConsumedPendingV4(readback);
    DeleteSasConsumedIntentV4(proof);
    if(pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||!ReferenceEquals(session,previous)||
     previous.LastResult!=null||previous.SpinPending||previous.InDoubleUp||previous.ResultPhase||previous.Bets.Count!=0||previous.History.Count!=0||
     recordedHistoryKey!=checkpoint.Scope||profileIndex<0||profileIndex>=profiles.Length||profiles[profileIndex]!=proof.Suffix||
     PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback||PlayerPrefs.GetString(recordedHistoryKey,"")!=pockets||
     PlayerPrefs.GetString(recordedHistoryKey+".Pages","")!=mirror)
     throw new PcRoundLifecycleDataException("Cold SAS ownership changed during consumed Pending reconciliation");
    session=restored;recordedRoundPages.Clear();recordedRoundPages.AddRange(pages);recordedSnapshots.Clear();recordedSnapshots.AddRange(snapshots);
    recordedPockets.Clear();recordedPockets.AddRange(value.historyState.pockets);sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(wrappers);
    recordedRawHistoryInvalid=false;
    latestRoundLifecycle=lifecycle;pcRoundArchiveSchemaVersion=4;pcDoubleFeatureNv=doubleEntry==null?null:doubleEntry.Feature.Clone();pcGamePayment=null;pcHistoryOverflow=null;
    pcSasGamePaymentV2=payment;sasDisplayStateV1=display;sasHistoryOverflowV2=overflow;consumedSasIntentDigest=checkpoint.ReadyDigest;
    return restored.LastResult;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-cold-base-restore",error);throw;}
  }
 }
}
