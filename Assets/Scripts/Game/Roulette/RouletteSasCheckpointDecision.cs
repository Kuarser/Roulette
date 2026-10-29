using System;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static RoulettePendingSettlementCandidate BindSasSettlementCheckpoint(RoulettePendingSettlementCandidate basis,SasIntentCheckpointV1 checkpoint,long admittedMaximumWager)
  {
   if(basis==null||basis.Sas!=null||checkpoint==null||checkpoint.Qualification==null||admittedMaximumWager<1)throw new PcRoundLifecycleDataException("SAS completed decision requires admitted base candidate");
   var q=checkpoint.Qualification;var result=basis.CopyBaseResult();
   if(q.EvaluatedAward!=result.Returned||q.Wager!=result.Wager||q.EffectiveMaximumWager!=admittedMaximumWager)throw new PcRoundLifecycleDataException("SAS checkpoint qualification differs from admitted round");
   if(q.Eligible?(checkpoint.Stage!=SasRoundIntentStageV1.ArithmeticSaved&&checkpoint.Stage!=SasRoundIntentStageV1.ReadyCommit):checkpoint.Stage!=SasRoundIntentStageV1.ReadyCommit)throw new PcRoundLifecycleDataException("SAS decision is not durably complete");
   basis.RequireCurrentPendingBaseline(basis.Owner);
   var decision=new SasDecision(q,checkpoint.Display==null?null:checkpoint.Display.Observation,checkpoint.Recorded==null?null:checkpoint.Recorded.Observation,checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.Observation);
   return basis.Owner.BuildPendingSettlementCandidate(decision);
  }
  private static string BuildSasCheckpointReadyProjection(RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
   int paytableMaximum,int tableCount,long staticCap,string adapterPolicy,string displayPolicy)
  {
   if(candidate==null||candidate.Sas==null||checkpoint==null||checkpoint.Stage!=SasRoundIntentStageV1.ReadyCommit)throw new PcRoundLifecycleDataException("SAS ReadyCommit candidate required");
   var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);var decision=candidate.Sas;
   if(frozen.frozenInputsVersion!=2||frozen.paytableMaximumWager!=candidate.PaytableMaximumWager||paytableMaximum!=frozen.paytableMaximumWager||tableCount!=frozen.tableCount||staticCap!=frozen.staticMaxBetLimit||adapterPolicy!=frozen.adapterPolicyId||displayPolicy!=frozen.displayPolicyId||frozen.setupPolicyId!="PcSingleTableNoStaticCapV1")throw new PcRoundLifecycleDataException("SAS restore setup differs from frozen PC policy");
   var evidence=new PcSasDecisionEvidenceV1{version=1,scope=checkpoint.Scope,roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,profile=candidate.ProfileId,
    adapterPolicyId=adapterPolicy,displayPolicyId=displayPolicy,paytableSha256=candidate.PaytableResourceSha256,paytableMaximumWager=paytableMaximum,tableCount=tableCount,staticMaxBetLimit=staticCap,effectiveMaximumWager=SasEffectiveMaximumWager.Resolve(paytableMaximum,tableCount,staticCap),
    evaluatedAward=decision.Qualification.EvaluatedAward,roundWager=decision.Qualification.Wager,postMultiplierAward=decision.PostMultiplierAward,applied=decision.Applied,recordedMultiplier=decision.RecordedMultiplier,
    qualification=checkpoint.QualificationReceipt.ToPayload(),display=checkpoint.Display==null?null:checkpoint.Display.ToPayload(),recorded=checkpoint.Recorded==null?null:checkpoint.Recorded.ToPayload(),arithmetic=checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.ToPayload()};
   var observations=decision.Applied?new[]{checkpoint.QualificationReceipt,checkpoint.Display,checkpoint.Recorded,checkpoint.Arithmetic}:new[]{checkpoint.QualificationReceipt};
   return BuildSasReadyCommitProjection(candidate,EncodeSasDecisionEvidence(evidence),checkpoint.Scope,checkpoint.RoundId,checkpoint.SpinId,candidate.PaytableResourceSha256,paytableMaximum,tableCount,staticCap,adapterPolicy,displayPolicy,observations,frozen.pendingSpinRaw,frozen.pendingSpinSha256,frozen.priorArchiveRawSha256,frozen.priorPocketsRawSha256,frozen.priorPagesRawSha256);
  }
  private RoulettePendingSettlementCandidate RestoreSasReadyCandidateFromFrozen(SasIntentCheckpointV1 checkpoint)
  {
   if(checkpoint==null)throw new PcRoundLifecycleDataException("SAS checkpoint required");
   var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
   if(frozen.frozenInputsVersion!=2||frozen.setupPolicyId!="PcSingleTableNoStaticCapV1"||frozen.tableCount!=1||frozen.staticMaxBetLimit!=0||frozen.adapterPolicyId!="SingleThreadKnownDispatchV1"||frozen.displayPolicyId!="StrictNativeType4"||frozen.effectiveMaximumWager!=SasEffectiveMaximumWager.Resolve(frozen.paytableMaximumWager,frozen.tableCount,frozen.staticMaxBetLimit))throw new PcRoundLifecycleDataException("SAS frozen setup unsupported or inconsistent");
   return RestoreSasReadyCandidate(checkpoint,frozen.paytableMaximumWager,frozen.tableCount,frozen.staticMaxBetLimit,frozen.adapterPolicyId,frozen.displayPolicyId);
  }
  private RoulettePendingSettlementCandidate RestoreSasReadyCandidate(SasIntentCheckpointV1 checkpoint,int paytableMaximum,int tableCount,long staticCap,string adapterPolicy,string displayPolicy)
  {
   var candidate=RestoreSasSettlementCandidate(checkpoint,SasEffectiveMaximumWager.Resolve(paytableMaximum,tableCount,staticCap));
   if(SasReadyCommitDigest(BuildSasCheckpointReadyProjection(candidate,checkpoint,paytableMaximum,tableCount,staticCap,adapterPolicy,displayPolicy))!=checkpoint.ReadyDigest)throw new PcRoundLifecycleDataException("SAS ReadyCommit digest differs from restored candidate");
   return candidate;
  }
  private RoulettePendingSettlementCandidate RestoreSasSettlementCandidate(SasIntentCheckpointV1 checkpoint,long admittedMaximumWager)
  {return BindSasSettlementCheckpoint(RestoreSasEvaluatedCandidate(checkpoint),checkpoint,admittedMaximumWager);}
 }
}
