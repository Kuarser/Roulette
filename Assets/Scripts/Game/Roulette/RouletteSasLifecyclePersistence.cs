using System;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool sasLifecycleWriteInProgress;
  private Action<string,string> sasLifecycleWriteForTest;
  private ValidatedSasBaseContainer RequireSasLifecycleOwnerV4(string encoded)
  {
   var local=session as RouletteSession;
   if(local==null||pcRoundArchiveSchemaVersion!=4||pcRoundLifecycleStorageFaulted||profileIndex<0||profileIndex>=profiles.Length)
    throw new PcRoundLifecycleDataException("V4 lifecycle owner unavailable");
   ValidatedSasBaseContainer proof;
   ValidateArchivedSasLifecycleContainerV4(encoded,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out proof);
   if(proof.Checkpoint.Scope!=recordedHistoryKey||proof.Checkpoint.ReadyDigest!=consumedSasIntentDigest||
    EncodeLifecycleJsonValue(proof.Lifecycle)!=EncodeLifecycleJsonValue(latestRoundLifecycle)||
    pcSasGamePaymentV2==null||WriteValidatedSasPaymentV2(proof.Payment)!=WriteValidatedSasPaymentV2(pcSasGamePaymentV2)||
    !proof.PageWrappers.SequenceEqual(sasRecordedPageWrappersV4)||
    recordedRoundPages.Count!=proof.Rounds.Length||!recordedRoundPages.Zip(proof.Rounds,(a,b)=>JsonUtility.ToJson(a)==JsonUtility.ToJson(b)).All(same=>same)||
    !recordedPockets.SequenceEqual(proof.Archive.historyState.pockets)||
    !recordedSnapshots.Select(Convert.ToBase64String).SequenceEqual(proof.Archive.historyState.snapshotsBase64)||
    JsonUtility.ToJson(sasDisplayStateV1)!=JsonUtility.ToJson(proof.Display)||
    JsonUtility.ToJson(sasHistoryOverflowV2)!=JsonUtility.ToJson(proof.Overflow)||pcDoubleFeatureNv!=null||pcGamePayment!=null||pcHistoryOverflow!=null)
    throw new PcRoundLifecycleDataException("V4 lifecycle cached owner differs from archive");
   if(proof.Lifecycle.roundStage!=PcRoundStage.Ended)
   {
    if(pendingSpin!=null||resumePendingSpin)throw new PcRoundLifecycleDataException("V4 End has a pending successor");
    var expected=CreateValidatedSasBaseSessionV4(proof,local.OutcomeSource,null);local.RequireSameSasBaseSnapshot(expected);
   }
   return proof;
  }
  private static void RequireSasLifecycleAdvance(PcRoundLifecycleRecord before,PcRoundLifecycleRecord after)
  {
   if(before.roundArchiveId!=after.roundArchiveId||before.spinJournalId!=after.spinJournalId||before.profile!=after.profile||
    !(before.roundStage==PcRoundStage.AwaitEnd&&after.roundStage==PcRoundStage.EndPrepared||
      before.roundStage==PcRoundStage.EndPrepared&&after.roundStage==PcRoundStage.Ended||
      before.roundStage==PcRoundStage.Ended&&after.roundStage==PcRoundStage.Ended))
    throw new PcRoundLifecycleDataException("V4 lifecycle stage transition invalid");
   if(before.roundStage==PcRoundStage.Ended&&after.roundStage==PcRoundStage.Ended)
   {
    var old=before.idleEntry;var next=after.idleEntry;
    bool reset=old.state==PcIdleState.Complete&&!old.prepareAccepted&&next.state==PcIdleState.Pending;
    bool begin=old.state==PcIdleState.Pending&&next.state==PcIdleState.Running&&next.step==PcIdleStep.Begin;
    bool progress=old.state==PcIdleState.Running&&
     (next.state==PcIdleState.Running&&(int)next.step==(int)old.step+1||
      next.state==PcIdleState.Complete&&next.step==PcIdleStep.Complete&&
       (old.step==PcIdleStep.DrawInFlight||old.step==PcIdleStep.IdleEffectsDone&&!old.prepareAccepted));
    if(!(reset||begin||progress)||(!reset&&(old.idleEntryId!=next.idleEntryId||old.depositEventId!=next.depositEventId||old.successorSpinId!=next.successorSpinId)))
     throw new PcRoundLifecycleDataException("V4 Idle progress or identity differs");
   }
   if(before.endIntent!=null&&EncodeLifecycleJsonValue(before.endIntent)!=EncodeLifecycleJsonValue(after.endIntent))
    throw new PcRoundLifecycleDataException("V4 saved End decision changed");
  }
  private void SaveSasLifecycleTransitionV4(PcRoundLifecycleRecord candidate)
  {
   if(sasLifecycleWriteInProgress||pcRoundLifecycleStorageFaulted)throw new PcRoundLifecycleDataException("V4 lifecycle writer faulted or reentered");
   sasLifecycleWriteInProgress=true;
   try
   {
    string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");
    var proof=RequireSasLifecycleOwnerV4(before);var next=CloneRoundLifecycle(candidate);
    RequireSasLifecycleAdvance(proof.Lifecycle,next);ValidateLifecycleLatestIdentity(next,proof.Rounds[0]);ValidateRoundLifecyclePayload(next,proof.Rounds[0]);
    var span=proof.Root.Members["lifecycle"];string original=span.Text(before);int start=before.IndexOf(original,span.Start,StringComparison.Ordinal);
    if(start!=span.Start)throw new PcRoundLifecycleDataException("V4 lifecycle span differs");
    string proposed=before.Substring(0,start)+EncodeLifecycleJsonValue(next)+before.Substring(start+original.Length);
    ValidatedSasBaseContainer proposedProof;
    var local=(RouletteSession)session;
    ValidateArchivedSasLifecycleContainerV4(proposed,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out proposedProof);
    string boundary=next.roundStage==PcRoundStage.Ended&&next.idleEntry.state!=PcIdleState.Pending?"Idle"+next.idleEntry.step:next.roundStage.ToString();
    LifecycleBeforeWrite(boundary);
    if(!ReferenceEquals(session,local)||PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("V4 lifecycle predecessor changed before save");
    RequireSasLifecycleOwnerV4(before);
    if(sasLifecycleWriteForTest!=null)sasLifecycleWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
    if(PlayerPrefs.GetString(key,"")!=proposed)throw new PcRoundLifecycleDataException("V4 lifecycle readback differs");
    // Fresh validation after the persistence seam; the old cached owner must still match.
    ValidateArchivedSasLifecycleContainerV4(proposed,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out proposedProof);
    if(!ReferenceEquals(session,local))throw new PcRoundLifecycleDataException("V4 lifecycle session changed during save");
    RequireSasLifecycleOwnerV4(before);
    latestRoundLifecycle=proposedProof.Lifecycle;++recordedHistoryVersion;
    Debug.Log("ROULETTE_SAS_LIFECYCLE_SAVED: round="+next.roundArchiveId+" stage="+next.roundStage);
    LifecycleCheckpoint(boundary+"Saved");
   }
   catch(Exception error){FailPcRoundLifecycle("sas-lifecycle",error);throw;}
   finally{sasLifecycleWriteInProgress=false;}
  }
 }
}
