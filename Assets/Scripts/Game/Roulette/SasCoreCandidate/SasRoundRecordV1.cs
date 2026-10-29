using System;
namespace RouletteDeluxe.SasDraftV1
{
 [Serializable] public sealed class SasRoundRecordV1
 {
  public int version;
  public string roundArchiveId,spinJournalId,adapterPolicyId,displayPolicyId,evaluationIdentity;
  public PcSasObservationV1 qualification,displayObservation,recordedObservation,arithmeticObservation;
  public long evaluatedAward,postMultiplierAward;
  public bool applied;public int recordedMultiplier;public bool eligible;
  public string ineligibleReason,readyCommitDigest;
 }
}
