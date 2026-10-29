using System;
using System.Diagnostics;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool PcSasBaseOfferOwnerV4
  {
   get {return sasStartupRuntimePending&&pcRoundArchiveSchemaVersion==4&&pcSasGamePaymentV2!=null&&
    pcSasGamePaymentV2.owner==PcGamePaymentOwner.Base&&!pcSasGamePaymentV2.prepared&&!pcSasGamePaymentV2.moneyApplied&&
    !pcSasGamePaymentV2.historyPublished&&latestRoundLifecycle!=null&&latestRoundLifecycle.roundStage==PcRoundStage.RoundOpen&&
    latestRoundLifecycle.roundArchiveId==pcSasGamePaymentV2.roundArchiveId&&latestRoundLifecycle.spinJournalId==pcSasGamePaymentV2.spinJournalId;}
  }
  private bool PcSasBaseOfferInputAllowedV4
  {
   get {return PcSasBaseOfferOwnerV4&&pcLifecycleRuntimeReady&&!pcRoundLifecycleStorageFaulted&&!sasInitialArchiveWriteFaulted&&
    !pcAutoPlayStorageFaulted&&!sasInitialArchiveWriteInProgress&&!sasHistoryMirrorWriteInProgress&&!sasConsumedPendingWriteInProgress&&
    pendingSpin==null&&!resumePendingSpin&&session is RouletteSession&&session.ResultPhase&&!session.SpinPending&&!session.InDoubleUp&&
    (sourceRoundPhase==SourceRoundPhase.BaseRollup||sourceRoundPhase==SourceRoundPhase.DoubleOffer);}
  }
  private void CommitSasBaseOfferV4(PcGamePaymentOffer choice)
  {
   if(!PcSasBaseOfferOwnerV4||PcGameAppSuspended||pcGameResumePurgingInputs)
    throw new PcRoundLifecycleDataException("V4 Base offer has no admitted active owner");
   var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("V4 Base offer has no PC session");
   ValidatedSasBaseContainer proof;
   var old=RequireColdSasBasePaymentOwnerProof(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1",""),
    local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,null,out proof);
   bool applied=proof.Checkpoint.Qualification.Eligible;
   PcGamePaymentRecordV2 next;
   if(choice==PcGamePaymentOffer.Open)next=PcGamePaymentTransitionsV2.OpenOffer(old,applied);
   else if(choice==PcGamePaymentOffer.Take)next=PcGamePaymentTransitionsV2.TakeOffer(old,applied);
   else if(choice==PcGamePaymentOffer.Enter)next=PcGamePaymentTransitionsV2.EnterOffer(old,applied);
   else throw new PcRoundLifecycleDataException("Unknown V4 Base offer choice");
   CommitColdSasBasePaymentV4(next,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,null);
  }
  private void ResolveSasBaseOfferEligibilityV4()
  {
   if(!PcSasBaseOfferOwnerV4)return;
   var local=session as RouletteSession;if(local==null||!local.ResultPhase||local.SpinPending||local.InDoubleUp)return;
   if(pcSasGamePaymentV2.baseOffer==PcGamePaymentOffer.None&&local.DoubleAvailable)
    CommitSasBaseOfferV4(PcGamePaymentOffer.Open);
   // V2 stores an explicit rejection as Open -> Take. These PC journal
   // transitions happen before the native offer UI can be exposed.
   if(pcSasGamePaymentV2.baseOffer==PcGamePaymentOffer.Open&&(!DoubleUpInputAllowed||!local.DoubleAvailable))
   {CommitSasBaseOfferV4(PcGamePaymentOffer.Take);sourceBaseHasDoubleOffer=false;sourceOfferPromptStartedTimestamp=0;}
  }
  private void PersistSasBaseOfferChoiceV4(bool enter)
  {CommitSasBaseOfferV4(enter?PcGamePaymentOffer.Enter:PcGamePaymentOffer.Take);}
  private void ServiceSasBaseOfferV4()
  {
   if(!PcSasBaseOfferOwnerV4||PcGameAppSuspended||pcGameResumePurgingInputs||!pcLifecycleRuntimeReady||
    pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted)return;
   try
   {
    ResolveSasBaseOfferEligibilityV4();
    if(pcPaymentResumeEnter)
    {StartSourceDoubleFeature();pcPaymentResumeEnter=false;return;}
    if(session.DoubleAvailable&&pcLifecycleResumePresentation)
    {
     var local=(RouletteSession)session;
     RequireColdSasBasePaymentOwner(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1",""),
      local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,null);
     sourceBaseHasDoubleOffer=true;sourceDoubleOfferChoice=SourceDoubleOfferChoice.None;
     ServicePersistedRoundPresentation();StartSourceOfferPrompt();
    }
   }
   catch(Exception error){FailPcRoundLifecycle("sas-base-offer",error);throw;}
  }
  private void BeginSasDoubleEntryPresentationV4()
  {
   CommitColdSasDoubleEntryV4(Guid.NewGuid().ToString("N"));
   doubleEntranceStartedAt=Time.realtimeSinceStartup;doubleEntranceStartedTimestamp=Stopwatch.GetTimestamp();
   doubleIdleAnimationStartedAt=Time.realtimeSinceStartup+1f;
   doubleIdleAnimationStartedTimestamp=OriginalRackClock.AfterMilliseconds(doubleEntranceStartedTimestamp,1000,Stopwatch.Frequency);
  }
 }
}
