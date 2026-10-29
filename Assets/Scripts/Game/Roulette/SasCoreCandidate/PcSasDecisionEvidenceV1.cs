using System;
namespace RouletteDeluxe.SasDraftV1
{
 [Serializable] public sealed class PcSasObservationV1
 {
  public string point;
  public long configGeneration;
  public string configRaw32Base64,configRaw32Sha256,eventSource,eventId;
  public long eventSequence;
  public bool runtimePendingDisableObserved;
 }
 [Serializable] public sealed class PcSasDecisionEvidenceV1
 {
  public int version;
  public string scope,roundArchiveId,spinJournalId,profile,adapterPolicyId,displayPolicyId,paytableSha256;
  public int paytableMaximumWager,tableCount;
  public long staticMaxBetLimit,effectiveMaximumWager,evaluatedAward,roundWager,postMultiplierAward;
  public bool applied;
  public int recordedMultiplier;
  public PcSasObservationV1 qualification,display,recorded,arithmetic;
 }
}
