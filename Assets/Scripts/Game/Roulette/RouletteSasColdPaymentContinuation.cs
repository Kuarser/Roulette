using System;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static System.Diagnostics.Stopwatch BeginColdPaymentTiming()
  {
#if UNITY_EDITOR
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"-sasColdPaymentTiming")>=0)return System.Diagnostics.Stopwatch.StartNew();
#endif
   return null;
  }
  private static void ColdPaymentTiming(System.Diagnostics.Stopwatch timer,string stage)
  {
   if(timer==null)return;
   Debug.Log("SAS_COLD_PAYMENT_TIMING stage="+stage+" milliseconds="+timer.Elapsed.TotalMilliseconds.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
   timer.Restart();
  }
  private PcGamePaymentRecordV2 RequireColdSasBasePaymentOwner(string before,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,RoundResult[] priorHistory,ValidatedSasBaseContainer adjacentReady=null)
  {
   ValidatedSasBaseContainer proof;
   return RequireColdSasBasePaymentOwnerProof(before,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory,out proof,adjacentReady);
  }
  private PcGamePaymentRecordV2 RequireColdSasBasePaymentOwnerProof(string before,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,
   RoundResult[] priorHistory,out ValidatedSasBaseContainer proof,ValidatedSasBaseContainer adjacentReady=null)
  {
   proof=null;
   var local=session as RouletteSession;if(local==null||pcRoundArchiveSchemaVersion!=4||pcSasGamePaymentV2==null||pendingSpin!=null||resumePendingSpin||
    profileIndex<0||profileIndex>=profiles.Length)throw new PcRoundLifecycleDataException("Cold payment has no live Base owner");
   var ownerTimer=BeginColdPaymentTiming();
   var old=ValidateArchivedSasBaseContainerProofV4(before,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof,adjacentReady);
   ColdPaymentTiming(ownerTimer,"owner-container-validation");
   string[] pages=proof.PageWrappers;var checkpoint=proof.Checkpoint;
   pcSasGamePaymentV2.Validate(checkpoint.Qualification.Eligible);
   if(checkpoint.Scope!=recordedHistoryKey||consumedSasIntentDigest!=checkpoint.ReadyDigest||!sasRecordedPageWrappersV4.SequenceEqual(pages)||
    WriteValidatedSasPaymentV2(old)!=WriteValidatedSasPaymentV2(pcSasGamePaymentV2))
    throw new PcRoundLifecycleDataException("Cold payment cached owner differs from archive");
   var value=proof.Archive;var rounds=proof.Rounds;
   if(recordedRoundPages.Count!=rounds.Length||!recordedRoundPages.Zip(rounds,(a,b)=>JsonUtility.ToJson(a)==JsonUtility.ToJson(b)).All(same=>same)||
    !recordedPockets.SequenceEqual(value.historyState.pockets)||!recordedSnapshots.Select(bytes=>Convert.ToBase64String(bytes)).SequenceEqual(value.historyState.snapshotsBase64)||
    JsonUtility.ToJson(sasDisplayStateV1)!=JsonUtility.ToJson(proof.Display))
    throw new PcRoundLifecycleDataException("Cold payment history or display cache differs");
   var life=proof.Lifecycle;
   if(EncodeLifecycleJsonValue(latestRoundLifecycle)!=EncodeLifecycleJsonValue(life)||pcGamePayment!=null||pcDoubleFeatureNv!=null||pcHistoryOverflow!=null||
    JsonUtility.ToJson(sasHistoryOverflowV2)!=JsonUtility.ToJson(proof.Overflow))
    throw new PcRoundLifecycleDataException("Cold payment lifecycle cache differs");
   ColdPaymentTiming(ownerTimer,"owner-cache-validation");
   var expected=CreateValidatedSasBaseSessionV4(proof,local.OutcomeSource,priorHistory);
   local.RequireSameSasBaseSnapshot(expected);ColdPaymentTiming(ownerTimer,"owner-session-reconstruction");return old;
  }
  private string CommitColdSasBasePaymentV4(PcGamePaymentRecordV2 next,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,RoundResult[] priorHistory)
  {
   if(next==null||temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||sasHistoryMirrorWriteInProgress||sasConsumedPendingWriteInProgress)
    throw new PcRoundLifecycleDataException("Cold payment writer faulted or reentered");
   sasInitialArchiveWriteInProgress=true;
   try
   {
    var commitTimer=BeginColdPaymentTiming();
    string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");
    ValidatedSasBaseContainer predecessorProof,successorProof;
    var old=RequireColdSasBasePaymentOwnerProof(before,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory,out predecessorProof);var local=(RouletteSession)session;
    if(next.prepared&&local.DoubleAvailable)throw new PcRoundLifecycleDataException("Cold Base payment still has an untaken Double offer");
    ColdPaymentTiming(commitTimer,"commit-predecessor-owner");
    string proposed=BuildArchivedSasBasePaymentSuccessorV4(before,predecessorProof,next,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out successorProof);
    ColdPaymentTiming(commitTimer,"commit-successor-build-validation");
    // These objects were prepared and checked by the successor container validator.
    string[] wrappers=successorProof.PageWrappers;var rounds=successorProof.Rounds;
    var life=successorProof.Lifecycle;
    var payment=successorProof.Payment.Clone();
    recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,rounds.Length);sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,wrappers.Length);
    ColdPaymentTiming(commitTimer,"commit-successor-cache-preparation");
    if(PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("Cold payment predecessor changed before save");
    if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
    if(PlayerPrefs.GetString(key,"")!=proposed||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted)throw new PcRoundLifecycleDataException("Cold payment save readback differs");
    ColdPaymentTiming(commitTimer,"commit-save-readback");
    ValidatedSasBaseContainer readbackProof;
    ValidateArchivedSasBaseContainerProofV4(proposed,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out readbackProof);
    ColdPaymentTiming(commitTimer,"commit-readback-container-validation");
    RequireColdSasBasePaymentOwner(before,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory,readbackProof);
    ColdPaymentTiming(commitTimer,"commit-post-save-owner");
    if(old.baseOffer!=PcGamePaymentOffer.Take&&payment.baseOffer==PcGamePaymentOffer.Take&&local.DoubleAvailable)local.DeclineDoubleOffer();
    if(!old.moneyApplied&&payment.moneyApplied)local.CompleteBaseGamePayout(payment.finalAward);
    if(local.Credit!=payment.WalletAfter)throw new PcRoundLifecycleDataException("Cold payment installed wallet differs");
    recordedRoundPages.Clear();recordedRoundPages.AddRange(rounds);sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(wrappers);
    pcSasGamePaymentV2=payment;latestRoundLifecycle=life;sasHistoryOverflowV2=successorProof.Overflow;
    ColdPaymentTiming(commitTimer,"commit-live-install");
    return proposed;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-cold-base-payment",error);throw;}
   finally{sasInitialArchiveWriteInProgress=false;}
  }
  private static string BuildArchivedSasBasePaymentSuccessorV4(string before,ValidatedSasBaseContainer validatedPredecessor,PcGamePaymentRecordV2 next,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,out ValidatedSasBaseContainer successorProof)
  {
   successorProof=null;
   // The sole caller has just validated this exact predecessor and its live owner, with no callback between.
   if(validatedPredecessor==null||validatedPredecessor.Encoded!=before)throw new PcRoundLifecycleDataException("Cold payment validated predecessor missing or changed");
   var old=validatedPredecessor.Payment;
   var root=validatedPredecessor.Root;string[] pages=(string[])validatedPredecessor.PageWrappers.Clone();
   var checkpoint=validatedPredecessor.Checkpoint;bool applied=checkpoint.Qualification.Eligible;RequireSasPaymentSuccessorV2(old,next,applied);
   var pageRoot=root.Members["pages"].Items[0];var round=ParsePcReceiptContext<RecordedRound>(pageRoot.Members["round"].Text(before));RestoreLifecycleJsonNulls(pageRoot.Members["round"],round);
   round.creditAfter=next.WalletAfter;round.awaitingFinalPayout=!next.historyPublished;
   pages[0]="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(round)+",\"sas\":"+pageRoot.Members["sas"].Text(before)+"}";
   var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(before));RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
   lifecycle.roundStage=next.historyPublished?PcRoundStage.AwaitEnd:PcRoundStage.RoundOpen;
   string encoded="{\"lifecycleSchemaVersion\":4,\"pages\":["+string.Join(",",pages)+"],\"lifecycle\":"+EncodeLifecycleJsonValue(lifecycle)+
    ",\"featureNv\":null,\"payment\":"+EncodeSasPaymentV2(next,applied)+",\"historyOverflow\":"+(next.historyPublished?"null":root.Members["historyOverflow"].Text(before))+
    ",\"historyState\":"+root.Members["historyState"].Text(before)+",\"displayState\":"+root.Members["displayState"].Text(before)+
    ",\"consumedSasIntentDigest\":"+root.Members["consumedSasIntentDigest"].Text(before)+"}";
   ValidateArchivedSasBaseContainerProofV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out successorProof,validatedPredecessor);return encoded;
  }
 }
}
