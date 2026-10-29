using System;
using System.IO;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool sasPendingProducerInProgress;
  private Action<string> sasPendingProducerCheckpointForTest;
  private string PcSasIntentStatePath {get{return PcSasConfigStatePath+".intent_v1.bin";}}
  private void SasProducerCheckpoint(string boundary)
  {if(sasPendingProducerCheckpointForTest!=null)sasPendingProducerCheckpointForTest(boundary);}
  private SasIntentCheckpointV1 LoadPendingSasIntentV4()
  {
   if(!File.Exists(PcSasIntentStatePath))return null;
   if(!PcPendingUsesV4||pendingSpin==null)throw new PcRoundLifecycleDataException("SAS intent has no admitted format4 Pending owner");
   var discovered=SasIntentCheckpointCodecV1.DecodeForSpin(File.ReadAllBytes(PcSasIntentStatePath),recordedHistoryKey,pendingSpin.id);
   var saved=new SasIntentCheckpointFileStoreV1(PcSasIntentStatePath,recordedHistoryKey,discovered.RoundId,discovered.SpinId).Load();
   if(saved==null)throw new PcRoundLifecycleDataException("SAS intent disappeared during cold admission");
   var basis=RestoreSasEvaluatedCandidate(saved);
   string formatRaw;var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(saved.FrozenInputs);
   ParseSasPendingSpinRaw(frozen.pendingSpinRaw,out formatRaw);
   if(formatRaw==null||formatRaw!=pcSpinRoundFormatContextRaw)throw new PcRoundLifecycleDataException("SAS production intent lacks original format4 provenance");
   basis.RequireCurrentPendingBaseline(session as RouletteSession);return saved;
  }
  private void ValidatePendingSasIntentAtStartupV4()
  {if(File.Exists(PcSasIntentStatePath))LoadPendingSasIntentV4();}
  private SasIntentCheckpointV1 NewSasProducerStage(SasIntentCheckpointV1 saved,SasRoundIntentStageV1 stage,
   SasQualification qualification,SasConfigObservationReceiptV1 qr,SasConfigObservationReceiptV1 display,string ready)
  {return new SasIntentCheckpointV1(saved.Scope,saved.RoundId,saved.SpinId,saved.FrozenInputs,saved.Revision+1,stage,qualification,qr,display,false,null,null,ready);}
  private RoundResult CompleteSasPendingIntentV4()
  {
   if(sasPendingProducerInProgress||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||temporaryHistory||
    !PcPendingUsesV4||!PcReceiptEnabled||pendingSpin==null||PcGameAppSuspended||!session.SpinPending)
    throw new PcRoundLifecycleDataException("SAS Pending producer has no admitted active owner");
   sasPendingProducerInProgress=true;
   try
   {
    var local=(RouletteSession)session;string pendingRaw=CaptureSasIntentPendingRaw();var saved=LoadPendingSasIntentV4();
    if(saved==null)
    {
     saved=CreateSasEvaluatedCheckpoint(Guid.NewGuid().ToString("N"));
     saved=new SasIntentCheckpointFileStoreV1(PcSasIntentStatePath,recordedHistoryKey,saved.RoundId,saved.SpinId).Save(-1,saved);
     SasProducerCheckpoint("EvaluatedSaved");
    }
    var store=new SasIntentCheckpointFileStoreV1(PcSasIntentStatePath,recordedHistoryKey,saved.RoundId,saved.SpinId);
    var basis=RestoreSasEvaluatedCandidate(saved);var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(saved.FrozenInputs);
    if(saved.Stage==SasRoundIntentStageV1.EvaluatedSaved)
    {
     RequirePcSasHostConfigurationReadback();var qr=sasHostConfigCoordinator.CaptureObservation(SasObservationPoint.Qualification);
     var baseResult=basis.CopyBaseResult();var q=new SasQualification(qr.Observation,baseResult.Returned,baseResult.Wager,frozen.effectiveMaximumWager);
     saved=store.Save(saved.Revision,NewSasProducerStage(saved,SasRoundIntentStageV1.QualificationSaved,q,qr,null,null));SasProducerCheckpoint("QualificationSaved");
    }
    if(saved.Qualification.Eligible)
    {
     if(saved.Stage==SasRoundIntentStageV1.QualificationSaved)
     {
      RequirePcSasHostConfigurationReadback();var display=sasHostConfigCoordinator.CaptureObservation(SasObservationPoint.Display);
      saved=store.Save(saved.Revision,NewSasProducerStage(saved,SasRoundIntentStageV1.ShowPlanned,saved.Qualification,saved.QualificationReceipt,display,null));SasProducerCheckpoint("ShowPlanned");
     }
     if(saved.Stage!=SasRoundIntentStageV1.ShowPlanned||saved.Display==null)
      throw new PcRoundLifecycleDataException("SAS Pending has no admitted native Show plan");
     // Original Type4 admission precedes Show success. No fabricated receipt.
     RequireSasStartupDisplayResourcesV4(new SasDisplayStateV1{roundId=saved.RoundId,policyId=frozen.displayPolicyId,displayMultiplier=saved.Display.Observation.Payload.Multiplier});
     throw new PcRoundLifecycleDataException("Native SAS Show/Hide producer is not installed");
    }
    if(saved.Stage==SasRoundIntentStageV1.QualificationSaved)
    {
     // Only the final digest is persisted; this temporary projection is local.
     var planned=NewSasProducerStage(saved,SasRoundIntentStageV1.ReadyCommit,saved.Qualification,saved.QualificationReceipt,null,new string('0',64));
     var candidate=BindSasSettlementCheckpoint(basis,planned,frozen.effectiveMaximumWager);
     string digest=SasReadyCommitDigest(BuildSasCheckpointReadyProjection(candidate,planned,frozen.paytableMaximumWager,frozen.tableCount,frozen.staticMaxBetLimit,frozen.adapterPolicyId,frozen.displayPolicyId));
     saved=store.Save(saved.Revision,NewSasProducerStage(saved,SasRoundIntentStageV1.ReadyCommit,saved.Qualification,saved.QualificationReceipt,null,digest));SasProducerCheckpoint("ReadyCommit");
    }
    if(saved.Stage!=SasRoundIntentStageV1.ReadyCommit)throw new PcRoundLifecycleDataException("SAS Pending has unsupported incomplete stage");
    var admitted=RestoreSasReadyCandidateFromFrozen(saved);
    string before=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");
    var history=new SasHistoryStateV1{pockets=recordedPockets.ToArray(),snapshotsBase64=recordedSnapshots.Select(Convert.ToBase64String).ToArray()};
    string[] prior=pcRoundArchiveSchemaVersion==4?sasRecordedPageWrappersV4.ToArray():before.Length==0?new string[0]:ImportLegacyPagesV4(before,recordedSnapshots);
    var round=BuildPendingSettlementRoundPage(admitted,saved.RoundId,saved.SpinId,true,pendingSpin.wheelStopIndex,(float)wheelRenderedAngle,wheelBallPosition.x,wheelBallPosition.y,true);
    string page=EncodeSasCandidatePageV4(JsonUtility.ToJson(round),admitted,saved,true);
    string readback=SaveSasInitialArchiveV4(admitted,saved,page,prior,history,"null");SasProducerCheckpoint("ArchiveSaved");
    ValidatedSasBaseContainer proof;
    ValidateArchivedSasBaseContainerProofV4(readback,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out proof);RequireSasConsumedIntentV4(proof);
    if(!ReferenceEquals(local,session)||PlayerPrefs.GetString(PendingSpinKey,"")!=pendingRaw)throw new PcRoundLifecycleDataException("SAS producer live or Pending owner changed before installation");
    admitted.RequireCurrentPendingBaseline(local);
    ReconcileSasHistoryMirrorsV4(readback,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode);
    var retired=PlanSasPredecessorJournalsV4(readback,local);
    RetireSasPredecessorJournalsV4(readback,retired);
    var result=InstallSasInitialArchiveV4(readback,admitted,saved,page,prior,history);sasStartupRuntimePending=true;SasProducerCheckpoint("Installed");
    ReconcileSasConsumedPendingV4(readback);pcSpinRoundFormatContextRaw=null;DeleteSasConsumedIntentV4(proof);SasProducerCheckpoint("ConsumedIntentDeleted");
    Debug.Log("ROULETTE_SAS_PENDING_PRODUCED: round="+saved.RoundId+" spin="+saved.SpinId+" award="+result.Returned+" wallet="+session.Credit+" countersPublished=true walletPaid=false format=4");return result;
   }
   catch(Exception error){FailPcRoundLifecycle("sas-pending-producer",error);throw;}
   finally{sasPendingProducerInProgress=false;}
  }
  private SasIntentCheckpointV1 RequireSasConsumedIntentV4(ValidatedSasBaseContainer proof)
  {
   if(!File.Exists(PcSasIntentStatePath))return null;
   if(proof==null||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=proof.Encoded||proof.Checkpoint.Scope!=recordedHistoryKey)throw new PcRoundLifecycleDataException("Consumed SAS intent has no authoritative archive owner");
   if(RequireSasSuccessorDrawV4(proof))return null;
   var saved=new SasIntentCheckpointFileStoreV1(PcSasIntentStatePath,recordedHistoryKey,proof.Checkpoint.RoundId,proof.Checkpoint.SpinId).Load();
   if(saved==null||!SasIntentCheckpointCodecV1.Encode(saved).SequenceEqual(SasIntentCheckpointCodecV1.Encode(proof.Checkpoint)))throw new PcRoundLifecycleDataException("Consumed SAS intent differs from authoritative ReadyCommit");return saved;
  }
  private void DeleteSasConsumedIntentV4(ValidatedSasBaseContainer proof)
  {var saved=RequireSasConsumedIntentV4(proof);if(saved!=null)new SasIntentCheckpointFileStoreV1(PcSasIntentStatePath,recordedHistoryKey,saved.RoundId,saved.SpinId).DeleteVerifiedReady(saved);}
 }
}
