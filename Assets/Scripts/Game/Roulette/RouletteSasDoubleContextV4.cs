using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  [Serializable] private sealed class PcDoubleDrawContextV4
  {
   public int version,settlementMode,archiveVersion;
   public bool unpaidBase;
   public string featureId,roundArchiveId,spinJournalId,sasRoundDigest,formatContextDigest;
   public string priorPaymentV2Raw,priorPaymentV2Sha256,priorSasRoundRaw,priorSasRoundSha256,priorFeatureRaw,priorFeatureSha256;
   public PcDoubleDrawContext financial;
  }
  private static bool IsSasDoubleReceiptV4(PcRngDrawReceipt receipt)
  {
   if(receipt==null||receipt.context==null)return false;
   var header=JsonUtility.FromJson<PcDoubleContextVersion>(receipt.context.contextJson);
   return header!=null&&header.version==4;
  }
  private static string SasDoubleFormatDigest(ValidatedSasDoubleEntry prior)
  {
   var envelope=ParsePcUnpaidReceiptContext<PendingSpinEnvelopeV2>(prior.Container.Frozen.pendingSpinRaw);
   RequirePcReceipt(envelope.version==2&&envelope.formatContextRaw!=null&&
    envelope.formatContextSha256==PcReceiptTextDigest(envelope.formatContextRaw),"SAS Double requires original Pending2 format");
   return envelope.formatContextSha256;
  }
  private static string EncodeSasDoubleRawContainer(ValidatedSasBaseContainer source,string[] pages,string paymentRaw,string featureRaw)
  {
   var root=source.Root;string raw=source.Encoded;
   return "{\"lifecycleSchemaVersion\":4,\"pages\":["+string.Join(",",pages)+"],\"lifecycle\":"+root.Members["lifecycle"].Text(raw)+
    ",\"featureNv\":"+featureRaw+",\"payment\":"+paymentRaw+",\"historyOverflow\":"+root.Members["historyOverflow"].Text(raw)+
    ",\"historyState\":"+root.Members["historyState"].Text(raw)+",\"displayState\":"+root.Members["displayState"].Text(raw)+
    ",\"consumedSasIntentDigest\":"+root.Members["consumedSasIntentDigest"].Text(raw)+"}";
  }
  private PcDoubleDrawContextV4 ReadSasDoubleContextV4(PcRngDrawReceipt receipt,string currentRaw,
   long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,out ValidatedSasDoubleEntry prior)
  {
   RequirePcReceipt(receipt!=null&&receipt.context!=null&&receipt.context.kind==2&&receipt.context.scope==recordedHistoryKey,
    "SAS Double receipt kind or scope differs");PcRngDrawReceipt.Validate(receipt);
   var v=ParsePcUnpaidReceiptContext<PcDoubleDrawContextV4>(receipt.context.contextJson);var f=v.financial;
   RequirePcReceipt(v.version==4&&v.archiveVersion==4&&v.settlementMode==4&&v.unpaidBase&&f!=null&&
    LifecycleGuid(v.featureId)&&v.roundArchiveId==f.roundArchiveId&&v.spinJournalId==f.spinJournalId&&
    v.priorPaymentV2Sha256==PcReceiptTextDigest(v.priorPaymentV2Raw)&&
    v.priorSasRoundSha256==PcReceiptTextDigest(v.priorSasRoundRaw)&&
    v.priorFeatureSha256==PcReceiptTextDigest(v.priorFeatureRaw),"SAS Double context raw provenance differs");
   var current=ValidateArchivedSasDoubleActiveV4(currentRaw,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   var c=current.Container;
   RequirePcReceipt(c.ReadySidecarRaw==v.priorSasRoundRaw&&c.Checkpoint.ReadyDigest==v.sasRoundDigest&&
    SasDoubleFormatDigest(current)==v.formatContextDigest&&c.Payment.featureId==v.featureId&&
    c.Payment.roundArchiveId==v.roundArchiveId&&c.Payment.spinJournalId==v.spinJournalId&&
    c.Root.Members["payment"].Text(currentRaw)==v.priorPaymentV2Raw,
    "SAS Double context differs from current frozen SAS or unpaid owner");
   var pages=(string[])c.PageWrappers.Clone();
   pages[0]="{\"originSchemaVersion\":4,\"round\":"+f.priorPageJson+",\"sas\":"+v.priorSasRoundRaw+"}";
   string before=EncodeSasDoubleRawContainer(c,pages,v.priorPaymentV2Raw,v.priorFeatureRaw);
   prior=ValidateArchivedSasDoubleActiveV4(before,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
   var probe=CreateValidatedSasDoubleActiveSessionV4(prior,new ReceiptNoDrawOutcome(),null);
   RequirePcReceipt(prior.Feature.savedState==2&&prior.Feature.completionStage==0&&prior.Feature.featureId==v.featureId&&
    f.version==1&&f.profileIndex==profileIndex&&probe.Profile.Id==receipt.context.profile&&
    string.Equals(f.rngPath,Path.GetFullPath(PcRngStateStore.PathForCurrentProcess),StringComparison.OrdinalIgnoreCase)&&f.priorPageDigest==PcReceiptTextDigest(f.priorPageJson)&&
    f.featureStep==prior.Feature.stepCount&&f.featureStep==probe.DoubleRounds&&
    f.choice>=-2&&f.choice<=3&&(f.choice<0||suitMode)&&!probe.DoubleChoiceTerminal&&
    f.stake==probe.DoubleStake&&f.creditBefore==probe.Credit&&f.denomination==probe.Denomination&&
    f.remainderCents==probe.MoneyRemainderCents&&f.wagered==probe.Wagered&&f.returned==probe.Returned&&
    f.rounds==probe.Rounds&&f.wins==probe.Wins&&f.lastDealer==probe.LastDoubleCard&&
    f.baseMaximumWin==probe.BaseMaximumWin&&f.doubleMaximumWin==probe.DoubleMaximumWin&&
    f.doubleMaximumRounds==probe.DoubleMaximumRounds&&f.creditEvent==PlayerPrefs.GetString(PcCreditKey,""),
    "SAS Double financial decision differs from independently restored B");
   string currentRound=c.Root.Members["pages"].Items[0].Members["round"].Text(currentRaw);
   if(currentRound==f.priorPageJson)
    RequirePcReceipt(c.Root.Members["featureNv"].Text(currentRaw)==v.priorFeatureRaw,"SAS Double prior native raw differs");
   else
   {
    RouletteSession result;string successor=BuildSasDoubleChoiceArchiveV4(prior,v,receipt,out result);
    var successorRoot=new LifecycleJsonReader(successor).Read();
    var expectedRound=ParsePcUnpaidReceiptContext<RecordedRound>(successorRoot.Members["pages"].Items[0].Members["round"].Text(successor));
    RequirePcReceipt(current.Feature.stepCount==f.featureStep+1&&current.Feature.resultDigest==PcDoubleFeatureResultDigest(expectedRound)&&
     JsonUtility.ToJson(c.Rounds[0])==JsonUtility.ToJson(expectedRound),"SAS Double current successor differs from retained decision");
   }
   return v;
  }
  private ValidatedSasDoubleEntry RequireLiveSasDoubleOwnerV4(string before)
  {
   var local=session as RouletteSession;
   if(local==null||temporaryHistory||pcRoundArchiveSchemaVersion!=4||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||PcGameAppSuspended||
    sasInitialArchiveWriteInProgress||sasHistoryMirrorWriteInProgress||sasConsumedPendingWriteInProgress||pendingSpin!=null||resumePendingSpin||
    pcGamePayment!=null||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=before)
    throw new PcRoundLifecycleDataException("SAS Double live archive ownership is not ready");
   var prior=ValidateArchivedSasDoubleActiveV4(before,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode);
   RequireLiveSasDoubleSnapshotV4(prior);
   return prior;
  }
  private void RequireLiveSasDoubleSnapshotV4(ValidatedSasDoubleEntry prior)
  {
   var local=session as RouletteSession;var c=prior.Container;
   RequirePcReceipt(local!=null&&!pcRoundLifecycleStorageFaulted&&!sasInitialArchiveWriteFaulted&&!PcGameAppSuspended&&
    pcGamePayment==null&&pendingSpin==null&&!resumePendingSpin&&!sasHistoryMirrorWriteInProgress&&!sasConsumedPendingWriteInProgress&&
    pcRoundArchiveSchemaVersion==4&&profiles[profileIndex]==c.Suffix&&local.BaseMaximumWin==c.MaximumWin&&
    local.DoubleMaximumWin==c.DoubleMaximumWin&&local.DoubleEnabled==c.DoubleEnabled&&local.DoubleSuitMode==c.SuitMode,
    "SAS Double current limits or profile changed");
   RequirePcReceipt(c.Checkpoint.Scope==recordedHistoryKey&&recordedRoundPages.Count==c.Rounds.Length&&
    sasRecordedPageWrappersV4.Count==c.PageWrappers.Length&&pcSasGamePaymentV2!=null&&pcDoubleFeatureNv!=null&&
    WriteValidatedSasPaymentV2(pcSasGamePaymentV2)==WriteValidatedSasPaymentV2(c.Payment)&&
    JsonUtility.ToJson(pcDoubleFeatureNv)==JsonUtility.ToJson(prior.Feature)&&
    JsonUtility.ToJson(latestRoundLifecycle)==JsonUtility.ToJson(c.Lifecycle)&&
    consumedSasIntentDigest==c.Checkpoint.ReadyDigest,"SAS Double live payment or native anchors differ");
   for(int n=0;n<c.Rounds.Length;n++)RequirePcReceipt(JsonUtility.ToJson(recordedRoundPages[n])==JsonUtility.ToJson(c.Rounds[n])&&
    sasRecordedPageWrappersV4[n]==c.PageWrappers[n],"SAS Double live page differs");
   var expected=CreateValidatedSasDoubleActiveSessionV4(prior,local.OutcomeSource,null);local.RequireSameSasBaseSnapshot(expected);
  }
  private void PrepareSasDoubleDrawV4(int choice,long wager)
  {
   RequirePcReceipt(PcReceiptEnabled&&pcOutcome!=null,"SAS Double requires durable PC RNG");
   string before=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");var prior=RequireLiveSasDoubleOwnerV4(before);
   var local=(RouletteSession)session;var c=prior.Container;
   RequirePcReceipt(prior.Feature.savedState==2&&prior.Feature.completionStage==0&&!local.DoubleChoiceTerminal&&
    wager==local.DoubleStake&&choice>=-2&&choice<=3&&(choice<0||local.DoubleSuitMode),"SAS Double decision is unavailable");
   string pageRaw=c.Root.Members["pages"].Items[0].Members["round"].Text(before);
   var f=new PcDoubleDrawContext{version=1,rngPath=Path.GetFullPath(PcRngStateStore.PathForCurrentProcess),profileIndex=profileIndex,
    roundArchiveId=c.Payment.roundArchiveId,spinJournalId=c.Payment.spinJournalId,featureStep=local.DoubleRounds,choice=choice,
    suitMode=local.DoubleSuitMode,stake=wager,creditBefore=local.Credit,denomination=local.Denomination,remainderCents=local.MoneyRemainderCents,
    wagered=local.Wagered,returned=local.Returned,rounds=local.Rounds,wins=local.Wins,lastDealer=local.LastDoubleCard,
    baseMaximumWin=local.BaseMaximumWin,doubleMaximumWin=local.DoubleMaximumWin,doubleMaximumRounds=local.DoubleMaximumRounds,
    priorPageJson=pageRaw,priorPageDigest=PcReceiptTextDigest(pageRaw),creditEvent=PlayerPrefs.GetString(PcCreditKey,"")};
   string paymentRaw=c.Root.Members["payment"].Text(before),featureRaw=c.Root.Members["featureNv"].Text(before);
   var v=new PcDoubleDrawContextV4{version=4,settlementMode=4,archiveVersion=4,unpaidBase=true,featureId=prior.Feature.featureId,
    roundArchiveId=f.roundArchiveId,spinJournalId=f.spinJournalId,sasRoundDigest=c.Checkpoint.ReadyDigest,formatContextDigest=SasDoubleFormatDigest(prior),
    priorPaymentV2Raw=paymentRaw,priorPaymentV2Sha256=PcReceiptTextDigest(paymentRaw),priorSasRoundRaw=c.ReadySidecarRaw,
    priorSasRoundSha256=PcReceiptTextDigest(c.ReadySidecarRaw),priorFeatureRaw=featureRaw,priorFeatureSha256=PcReceiptTextDigest(featureRaw),financial=f};
   var operation=new PcRngOperationContext{schema=1,operationId=Guid.NewGuid().ToString("N"),scope=recordedHistoryKey,
    profile=local.Profile.Id,kind=2,contextJson=JsonUtility.ToJson(v)};
   // Validate the exact context before preparing or requesting a suit.
   var candidate=new PcRngDrawReceipt{context=operation,contextSha256=PcRngDrawReceipt.ContextDigest(operation),rawResult=0,effectiveResult=0};
   ValidatedSasDoubleEntry checkedPrior;ReadSasDoubleContextV4(candidate,before,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out checkedPrior);
   if(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=before)throw new PcRoundLifecycleDataException("SAS Double predecessor changed before RNG prepare");
   pcOutcome.Prepare(operation,demoOutcome.ResolveDoubleSuit);pcPreparedDoubleOperation=operation;
  }
  private static string BuildSasDoubleChoiceArchiveV4(ValidatedSasDoubleEntry prior,PcDoubleDrawContextV4 context,
   PcRngDrawReceipt receipt,out RouletteSession expected)
  {
   expected=CreateValidatedSasDoubleActiveSessionV4(prior,new ReceiptNoDrawOutcome(),null);
   bool won=expected.ReplayDoubleChoice(context.financial.choice,receipt.effectiveResult);
   var c=prior.Container;var round=ParsePcUnpaidReceiptContext<RecordedRound>(context.financial.priorPageJson);
   var steps=round.doubleUp.steps.ToList();steps.Add(new RecordedDoubleStep{choice=PcRecordedDoubleChoice(context.financial.choice),
    dealer=receipt.effectiveResult,wager=context.financial.stake,won=won?expected.DoubleStake:0});round.doubleUp.steps=steps.ToArray();
   var nv=prior.Feature.Clone();nv.stepCount=steps.Count;nv.resultDigest=PcDoubleFeatureResultDigest(round);
   var pages=(string[])c.PageWrappers.Clone();pages[0]="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(round)+",\"sas\":"+context.priorSasRoundRaw+"}";
   return EncodeSasDoubleRawContainer(c,pages,context.priorPaymentV2Raw,JsonUtility.ToJson(nv));
  }
  private void ApplySasDoubleReceiptV4(PcRngDrawReceipt receipt)
  {
   string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");var current=RequireLiveSasDoubleOwnerV4(before);
   var local=(RouletteSession)session;ValidatedSasDoubleEntry prior;
   var context=ReadSasDoubleContextV4(receipt,before,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out prior);
   RouletteSession expected;string proposed=BuildSasDoubleChoiceArchiveV4(prior,context,receipt,out expected);
   var next=ValidateArchivedSasDoubleActiveV4(proposed,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode);
   string currentRound=current.Container.Root.Members["pages"].Items[0].Members["round"].Text(before);
   if(currentRound!=context.financial.priorPageJson)
   {
    // A saved result may already have entered reveal or returned to ready.
    RequirePcReceipt(current.Feature.stepCount==context.financial.featureStep+1&&
     JsonUtility.ToJson(current.Container.Rounds[0])==JsonUtility.ToJson(next.Container.Rounds[0])&&
     current.Feature.featureId==next.Feature.featureId&&current.Feature.resultDigest==next.Feature.resultDigest,
     "SAS Double saved successor differs from its retained receipt");
    pcOutcome.Acknowledge(receipt.context.operationId,receipt.contextSha256);pcPreparedDoubleOperation=null;return;
   }
   RequirePcReceipt(current.Container.Root.Members["featureNv"].Text(before)==context.priorFeatureRaw,
    "SAS Double unapplied feature raw differs");
   recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,next.Container.Rounds.Length);
   sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,next.Container.PageWrappers.Length);
   sasInitialArchiveWriteInProgress=true;
   try
   {
    if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
    if(PlayerPrefs.GetString(key,"")!=proposed||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||!ReferenceEquals(local,session))
     throw new PcRoundLifecycleDataException("SAS Double choice readback or live owner changed");
    // Recheck the complete old in-memory owner after the persistence callback.
    // The archive is now the successor; the old proof still checks the model.
    RequireLiveSasDoubleSnapshotV4(current);
    next=ValidateArchivedSasDoubleActiveV4(proposed,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode);
    local.ReplayDoubleChoice(context.financial.choice,receipt.effectiveResult);
    var liveExpected=CreateValidatedSasDoubleActiveSessionV4(next,local.OutcomeSource,null);local.RequireSameSasBaseSnapshot(liveExpected);
    recordedRoundPages.Clear();recordedRoundPages.AddRange(next.Container.Rounds);sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(next.Container.PageWrappers);
    pcDoubleFeatureNv=next.Feature.Clone();
    pcOutcome.Acknowledge(receipt.context.operationId,receipt.contextSha256);pcPreparedDoubleOperation=null;
    Debug.Log("ROULETTE_SAS_DOUBLE_RESULT_SAVED: id="+receipt.context.operationId+" step="+next.Feature.stepCount+" suit="+receipt.effectiveResult+" wallet="+local.Credit);
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-double-choice",error);throw;}
   finally{sasInitialArchiveWriteInProgress=false;}
  }
  // Used by the V4 owner. UI remains gated until completion and payout exist.
  private void CommitSasDoubleDrawV4(int choice)
  {
   try
   {
    PrepareSasDoubleDrawV4(choice,session.DoubleStake);pcOutcome.DrawDoubleSuit();ApplySasDoubleReceiptV4(pcOutcome.Receipt);
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-double-draw",error);throw;}
  }
 }
}
