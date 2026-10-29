using System;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Entry remains a strict subset of the independently checked active owner.
  private sealed class ValidatedSasDoubleEntry
  {
   public ValidatedSasBaseContainer Base,Container;
   public PcDoubleFeatureNvV1 Feature;
  }
  private static string EncodeSasDoubleEntryContainer(ValidatedSasBaseContainer prior,string[] pages,
   PcGamePaymentRecordV2 payment,PcDoubleFeatureNvV1 feature)
  {
   var root=prior.Root;string raw=prior.Encoded;
   return "{\"lifecycleSchemaVersion\":4,\"pages\":["+string.Join(",",pages)+"],\"lifecycle\":"+root.Members["lifecycle"].Text(raw)+
    ",\"featureNv\":"+(feature==null?"null":JsonUtility.ToJson(feature))+",\"payment\":"+EncodeSasPaymentV2(payment,prior.Checkpoint.Qualification.Eligible)+
    ",\"historyOverflow\":"+root.Members["historyOverflow"].Text(raw)+",\"historyState\":"+root.Members["historyState"].Text(raw)+
    ",\"displayState\":"+root.Members["displayState"].Text(raw)+",\"consumedSasIntentDigest\":"+root.Members["consumedSasIntentDigest"].Text(raw)+"}";
  }
  private static string BuildArchivedSasDoubleEntryV4(ValidatedSasBaseContainer prior,string featureId)
  {
   if(prior==null||prior.Payment.baseOffer!=PcGamePaymentOffer.Enter||prior.Payment.owner!=PcGamePaymentOwner.Base||
    prior.Payment.prepared||prior.Display!=null&&!prior.Display.hideCompleted)
    throw new PcRoundLifecycleDataException("SAS Double entry requires hidden unpaid Enter");
   var payment=PcGamePaymentTransitionsV2.TransferToDouble(prior.Payment,prior.Checkpoint.Qualification.Eligible,featureId);
   var round=ParsePcReceiptContext<RecordedRound>(JsonUtility.ToJson(prior.Rounds[0]));
   round.hasDoubleUp=true;round.doubleUp=new RecordedDouble{startingStake=payment.baseAward,suitMode=prior.SuitMode,steps=new RecordedDoubleStep[0]};
   var feature=new PcDoubleFeatureNvV1{featureId=featureId,roundArchiveId=round.roundArchiveId,spinJournalId=round.spinJournalId,
    profile=round.profile,savedState=2,suitMode=prior.SuitMode,resultDigest=PcDoubleFeatureResultDigest(round)};
   string[] pages=(string[])prior.PageWrappers.Clone();
   pages[0]="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(round)+",\"sas\":"+prior.Root.Members["pages"].Items[0].Members["sas"].Text(prior.Encoded)+"}";
   string encoded=EncodeSasDoubleEntryContainer(prior,pages,payment,feature);
   ValidateArchivedSasDoubleEntryV4(encoded,prior.Suffix,prior.MaximumWin,prior.DoubleMaximumWin,prior.DoubleEnabled,prior.SuitMode);
   return encoded;
  }
  private static ValidatedSasDoubleEntry ValidateArchivedSasDoubleEntryV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   var entry=ValidateArchivedSasDoubleActiveV4(encoded,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   if(entry.Feature.savedState!=2||entry.Feature.autoRestoreMarker||entry.Feature.stepCount!=0)
    throw new PcRoundLifecycleDataException("SAS Double entry is not untouched native state2");
   return entry;
  }
  private static ValidatedSasDoubleEntry ValidateArchivedSasDoubleActiveV4(string encoded,string suffix,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   var root=new LifecycleJsonReader(encoded).Read();ValidateLifecycleJsonShape(root,typeof(RecordedRoundArchiveV4),encoded);
   var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(encoded);
   if(value.lifecycleSchemaVersion!=4||root.Members["featureNv"].Kind!='{'||value.pages==null||value.pages.Length<1)
    throw new PcRoundLifecycleDataException("SAS Double entry envelope invalid");
   var feature=ParsePcReceiptContext<PcDoubleFeatureNvV1>(root.Members["featureNv"].Text(encoded));feature.Validate();
   var rounds=ValidateCheckedContainerPageHistory(root.Members["pages"],encoded,value);var round=rounds[0];
   if(!round.hasDoubleUp||round.doubleUp==null||round.doubleUp.steps==null||
    round.doubleUp.complete||round.doubleUp.tookWin||round.doubleUp.finalWin!=0||
    (feature.savedState!=2&&feature.savedState!=3&&feature.savedState!=5&&feature.savedState!=7)||
    feature.completionStage!=0||feature.completionAward!=0||!IsValidRecordedDoubleWithFeature(round.doubleUp,true))
    throw new PcRoundLifecycleDataException("SAS Double active feature state invalid or completion unsupported");
   SasFrozenInputsV1 frozen;var checkpoint=DecodeSasPageCheckpointAndInputsV4(root.Members["pages"].Items[0].Text(encoded),out frozen,true);
   var payment=DecodeSasPaymentV2(root.Members["payment"].Text(encoded),checkpoint.Qualification.Eligible);
   if(payment.owner!=PcGamePaymentOwner.Double||payment.baseOffer!=PcGamePaymentOffer.Transferred||payment.prepared||
    payment.featureId!=feature.featureId||round.doubleUp.startingStake!=payment.baseAward||round.doubleUp.suitMode!=suitMode)
    throw new PcRoundLifecycleDataException("SAS Double entry owner or frozen B differs");
   var life=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(encoded));RestoreLifecycleJsonNulls(root.Members["lifecycle"],life);
   ValidatePcDoubleFeatureNv(feature,round,life);
   // A semantic Base projection independently proves resources, A/B, history,
   // lifecycle and wallet. It is never passed off as prior serialized raw text.
   var basePayment=payment.Clone();basePayment.owner=PcGamePaymentOwner.Base;basePayment.baseOffer=PcGamePaymentOffer.Enter;basePayment.featureId=null;
   var baseRound=ParsePcReceiptContext<RecordedRound>(root.Members["pages"].Items[0].Members["round"].Text(encoded));
   baseRound.hasDoubleUp=false;baseRound.doubleUp=null;
   var pageRaws=root.Members["pages"].Items.Select(n=>n.Text(encoded)).ToArray();var basePages=(string[])pageRaws.Clone();
   basePages[0]="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(baseRound)+",\"sas\":"+root.Members["pages"].Items[0].Members["sas"].Text(encoded)+"}";
   var projection=new ValidatedSasBaseContainer{Root=root,Encoded=encoded,Checkpoint=checkpoint};
   string baseRaw=EncodeSasDoubleEntryContainer(projection,basePages,basePayment,null);ValidatedSasBaseContainer basis;
   ValidateArchivedSasBaseContainerProofV4(baseRaw,suffix,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out basis);
   if(basis.Display!=null&&!basis.Display.hideCompleted)throw new PcRoundLifecycleDataException("SAS Double entry precedes Hide");
   RequireSasPaymentSuccessorV2(basis.Payment,payment,checkpoint.Qualification.Eligible);
   var container=new ValidatedSasBaseContainer{Encoded=encoded,Root=root,Suffix=suffix,ReadySidecarRaw=basis.ReadySidecarRaw,
    MaximumWin=maximumWin,DoubleMaximumWin=doubleMaximumWin,DoubleEnabled=doubleEnabled,SuitMode=suitMode,Payment=payment,
    Checkpoint=basis.Checkpoint,Frozen=basis.Frozen,Basis=basis.Basis,Pending=basis.Pending,PageWrappers=pageRaws,Rounds=rounds,
    Archive=value,Lifecycle=life,Display=basis.Display,Overflow=basis.Overflow};
   var active=new ValidatedSasDoubleEntry{Base=basis,Container=container,Feature=feature};
   // Replay only committed suits from fixed B. Never draw or reevaluate A*M.
   CreateValidatedSasDoubleActiveSessionV4(active,new ReceiptNoDrawOutcome(),null);
   return active;
  }
  private static RouletteSession CreateValidatedSasDoubleEntrySessionV4(ValidatedSasDoubleEntry entry,
   IRouletteOutcomeSource source,RoundResult[] priorHistory)
  {
   var restored=CreateValidatedSasBaseSessionV4(entry.Base,source,priorHistory);
   restored.EnableDeferredDoubleSettlement();restored.StartDouble();
   if(restored.Credit!=entry.Container.Payment.WalletAfter||restored.DoubleStake!=entry.Container.Payment.baseAward||
    !restored.RoundPayoutPending||!restored.BaseSettlementDeferred||!restored.InDoubleUp||restored.DoubleAvailable||
    restored.DoubleRounds!=0||restored.LastDoubleCard!=-1)
    throw new PcRoundLifecycleDataException("SAS Double entry model differs from frozen owner");
   return restored;
  }
  private static int SasDoubleModelChoice(int choice)
  {return choice==5?-1:choice==4?-2:choice==3?0:choice==2?1:choice==0?2:3;}
  private static RouletteSession CreateValidatedSasDoubleActiveSessionV4(ValidatedSasDoubleEntry active,
   IRouletteOutcomeSource source,RoundResult[] priorHistory)
  {
   var restored=CreateValidatedSasBaseSessionV4(active.Base,source,priorHistory);
   restored.EnableDeferredDoubleSettlement();restored.StartDouble();
   foreach(var step in active.Container.Rounds[0].doubleUp.steps)
   {
    if(step==null||step.wager!=restored.DoubleStake||restored.DoubleChoiceTerminal)
     throw new PcRoundLifecycleDataException("SAS Double step does not follow fixed B or prior decision");
    bool won=restored.ReplayDoubleChoice(SasDoubleModelChoice(step.choice),step.dealer);
    if(step.won!=(won?restored.DoubleStake:0))throw new PcRoundLifecycleDataException("SAS Double committed award differs from replay");
   }
   if(restored.Credit!=active.Container.Payment.WalletAfter||!restored.InDoubleUp||
    !restored.BaseSettlementDeferred||!restored.RoundPayoutPending||!restored.DoubleSettlementDeferred)
    throw new PcRoundLifecycleDataException("SAS Double active model changed unpaid wallet");
   if((active.Feature.savedState==5||active.Feature.savedState==7)&&restored.DoubleStake<=0)
    throw new PcRoundLifecycleDataException("SAS Double Take has no award");
   return restored;
  }
  private string CommitColdSasDoubleEntryV4(string featureId)
  {
   if(!DoubleUpInputAllowed||temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||
    sasHistoryMirrorWriteInProgress||sasConsumedPendingWriteInProgress)
    throw new PcRoundLifecycleDataException("SAS Double entry writer faulted or reentered");
   sasInitialArchiveWriteInProgress=true;
   try
   {
    var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS Double entry has no session");
    string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");ValidatedSasBaseContainer prior;
    RequireColdSasBasePaymentOwnerProof(before,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,null,out prior);
    string proposed=BuildArchivedSasDoubleEntryV4(prior,featureId);
    var admitted=ValidateArchivedSasDoubleEntryV4(proposed,prior.Suffix,prior.MaximumWin,prior.DoubleMaximumWin,prior.DoubleEnabled,prior.SuitMode);
    var expected=CreateValidatedSasDoubleEntrySessionV4(admitted,local.OutcomeSource,null);
    recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,admitted.Container.Rounds.Length);
    sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,admitted.Container.PageWrappers.Length);
    if(PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("SAS Double predecessor changed before save");
    if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
    if(PlayerPrefs.GetString(key,"")!=proposed||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||!ReferenceEquals(local,session))
     throw new PcRoundLifecycleDataException("SAS Double entry readback or live owner changed");
    var saved=ValidateArchivedSasDoubleEntryV4(proposed,prior.Suffix,prior.MaximumWin,prior.DoubleMaximumWin,prior.DoubleEnabled,prior.SuitMode);
    RequireColdSasBasePaymentOwner(before,prior.MaximumWin,prior.DoubleMaximumWin,prior.DoubleEnabled,prior.SuitMode,null);
    local.EnableDeferredDoubleSettlement();local.StartDouble();local.RequireSameSasBaseSnapshot(expected);
    recordedRoundPages.Clear();recordedRoundPages.AddRange(saved.Container.Rounds);sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(saved.Container.PageWrappers);
    pcSasGamePaymentV2=saved.Container.Payment.Clone();pcDoubleFeatureNv=saved.Feature.Clone();
    return proposed;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-double-entry",error);throw;}
   finally{sasInitialArchiveWriteInProgress=false;}
  }
 }
}
