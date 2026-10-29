using System;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool sasIdleRuntimeDispatchV4,sasIdleRuntimeServicingV4;
  private int sasIdleRuntimeFrameV4=-1;
  private void RequireSasIdleConfigReceiptV4(PcIdleEntry entry)
  {
   var saved=RequirePcSasHostConfigurationReadback();
   if(saved.IdleReceipt==null||saved.IdleReceipt.Scope!=recordedHistoryKey||saved.IdleReceipt.IdleEntryId!=entry.idleEntryId)
    throw new PcRoundLifecycleDataException("V4 Idle execution lacks durable configuration completion");
  }
  private bool ServiceSasIdleRuntimeV4(bool beginNewEntry)
  {
   if(pcRoundArchiveSchemaVersion!=4||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||
    !pcLifecycleRuntimeReady||!pcAutoCompleteGameSelected||PcGameAppSuspended||pcGameResumePurgingInputs||
    PaytableTestActive||OperatorFallbackActive)return false;
   if(sasIdleRuntimeServicingV4)throw new PcRoundLifecycleDataException("V4 Idle runtime reentered");
   if(sasIdleRuntimeFrameV4==Time.frameCount)return false;
   sasIdleRuntimeFrameV4=Time.frameCount;sasIdleRuntimeServicingV4=true;sasStartupRuntimePending=true;
   try
   {
    if(sourceRoundPhase!=SourceRoundPhase.None||pendingSpin!=null||resumePendingSpin||spinning||session==null||
     session.SpinPending||session.InDoubleUp||pcCollectPending)
     throw new PcRoundLifecycleDataException("V4 Idle execution lacks native Idle owner");
    string raw=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");
    var proof=RequireSasLifecycleOwnerV4(raw);var local=(RouletteSession)session;
    if(proof.Lifecycle.roundStage!=PcRoundStage.Ended||!proof.Payment.IsResolved||!proof.Payment.historyPublished)
     throw new PcRoundLifecycleDataException("V4 Idle precedes paid End");
    RequireSasPaidIdleSessionV4(raw,local);
    var entry=proof.Lifecycle.idleEntry;
    if(entry.state==PcIdleState.Complete&&!beginNewEntry)
    {
     if(entry.prepareAccepted)throw new PcRoundLifecycleDataException("V4 completed Idle lost its successor");
     RequireSasIdleConfigReceiptV4(entry);sasStartupRuntimePending=false;return true;
    }
    if(entry.state==PcIdleState.Complete)BeginLifecycleIdleEntry();
    if(!ServiceSasStartupIdleConfigurationV4()||PcGameAppSuspended)return false;
    RequireSasIdleConfigReceiptV4(CopyIdleEntry());
    sasIdleRuntimeDispatchV4=true;
    ServicePersistedIdleEntryCore();
    if(pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted)return false;
    raw=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");proof=RequireSasLifecycleOwnerV4(raw);
    if(proof.Lifecycle.idleEntry.state!=PcIdleState.Complete)
     throw new PcRoundLifecycleDataException("V4 Idle execution did not save completion");
    if(local.SpinPending)
    {
     if(pendingSpin==null||!RequireSasSuccessorDrawV4(proof))throw new PcRoundLifecycleDataException("V4 Idle lacks admitted successor handoff");
    }
    else RequireSasPaidIdleSessionV4(raw,local);
    sasStartupRuntimePending=false;
    Debug.Log("ROULETTE_SAS_IDLE_RUNTIME_READY: idle="+proof.Lifecycle.idleEntry.idleEntryId+" step="+
     proof.Lifecycle.idleEntry.step+" successor="+local.SpinPending+" wallet="+local.Credit);
    return true;
   }
   catch(Exception error){FailPcRoundLifecycle("sas-idle-runtime",error);return false;}
   finally{sasIdleRuntimeDispatchV4=false;sasIdleRuntimeServicingV4=false;}
  }
 }
}
