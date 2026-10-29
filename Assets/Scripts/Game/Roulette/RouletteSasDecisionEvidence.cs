using System;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static readonly string[] SasDecisionFields={"version","scope","roundArchiveId","spinJournalId","profile","adapterPolicyId","displayPolicyId","paytableSha256","paytableMaximumWager","tableCount","staticMaxBetLimit","effectiveMaximumWager","evaluatedAward","roundWager","postMultiplierAward","applied","recordedMultiplier","qualification","display","recorded","arithmetic"};
  private static readonly string[] SasObservationFields={"point","configGeneration","configRaw32Base64","configRaw32Sha256","eventSource","eventId","eventSequence","runtimePendingDisableObserved"};
  private static string QuoteSasCanonical(string value)
  {
   if(value==null)return "null";
   var text=new StringBuilder("\"");
   foreach(char ch in value)
   {
    if(ch=='"' || ch=='\\')text.Append('\\').Append(ch);
    else if(ch<32 || char.IsSurrogate(ch))text.Append("\\u").Append(((int)ch).ToString("X4",CultureInfo.InvariantCulture));
    else text.Append(ch);
   }
   return text.Append('"').ToString();
  }
  private static string WriteSasCanonicalObject(object value,string[] fields)
  {
   if(value==null)return "null";
   var encoded=new StringBuilder("{");
   for(int i=0;i<fields.Length;i++)
   {
    if(i!=0)encoded.Append(',');encoded.Append(QuoteSasCanonical(fields[i])).Append(':');
    var field=value.GetType().GetField(fields[i]);object item=field.GetValue(value);
    if(field.FieldType==typeof(string))encoded.Append(QuoteSasCanonical((string)item));
    else if(field.FieldType==typeof(bool))encoded.Append((bool)item?"true":"false");
    else if(field.FieldType==typeof(PcSasObservationV1))encoded.Append(WriteSasCanonicalObject(item,SasObservationFields));
    else if(field.FieldType==typeof(int)||field.FieldType==typeof(long))encoded.Append(Convert.ToString(item,CultureInfo.InvariantCulture));
    else throw new PcRoundLifecycleDataException("Unsupported canonical SAS field type");
   }
   return encoded.Append('}').ToString();
  }
  private static string EncodeSasDecisionEvidence(PcSasDecisionEvidenceV1 value)
  {
   if(value==null)throw new ArgumentNullException("value");
   string encoded=WriteSasCanonicalObject(value,SasDecisionFields);
   DecodeSasDecisionEvidence(encoded,value.scope,value.roundArchiveId,value.spinJournalId);
   return encoded;
  }
  private static string CanonicalizeSasDecisionEvidence(string encoded,string scope,string roundId,string spinId)
  {
   DecodeSasDecisionEvidence(encoded,scope,roundId,spinId);
   var root=new LifecycleJsonReader(encoded).Read();var value=JsonUtility.FromJson<PcSasDecisionEvidenceV1>(encoded);
   if(root.Members["display"].Kind=='n')value.display=null;
   if(root.Members["recorded"].Kind=='n')value.recorded=null;
   if(root.Members["arithmetic"].Kind=='n')value.arithmetic=null;
   return EncodeSasDecisionEvidence(value);
  }
  private static SasDecision DecodeSasDecisionEvidence(string encoded,string expectedScope,string expectedRoundId,string expectedSpinId)
  {
   var root=new LifecycleJsonReader(encoded).Read();
   if(root.Kind!='{')throw new PcRoundLifecycleDataException("SAS decision root must be object");
   ValidateLifecycleJsonShape(root,typeof(PcSasDecisionEvidenceV1),encoded);
   var value=JsonUtility.FromJson<PcSasDecisionEvidenceV1>(encoded);
   if(value.version!=1 || string.IsNullOrWhiteSpace(expectedScope) || string.IsNullOrWhiteSpace(expectedRoundId) || string.IsNullOrWhiteSpace(expectedSpinId) ||
    value.scope!=expectedScope || value.roundArchiveId!=expectedRoundId || value.spinJournalId!=expectedSpinId || value.roundArchiveId==value.spinJournalId ||
    string.IsNullOrWhiteSpace(value.profile) || string.IsNullOrWhiteSpace(value.adapterPolicyId)||string.IsNullOrWhiteSpace(value.displayPolicyId)||!SasConfigEnvelopeCodecV3.IsDigest(value.paytableSha256))
    throw new PcRoundLifecycleDataException("SAS decision identity or resource provenance invalid");
   int maximum=SasEffectiveMaximumWager.Resolve(value.paytableMaximumWager,value.tableCount,value.staticMaxBetLimit);
   if(value.effectiveMaximumWager!=maximum)throw new PcRoundLifecycleDataException("SAS effective maximum differs from resource inputs");
   var qualification=new SasQualification(DecodeSasObservation(value.qualification,SasObservationPoint.Qualification),value.evaluatedAward,value.roundWager,maximum);
   var decision=new SasDecision(qualification,root.Members["display"].Kind=='n'?null:DecodeSasObservation(value.display,SasObservationPoint.Display),root.Members["recorded"].Kind=='n'?null:DecodeSasObservation(value.recorded,SasObservationPoint.Recorded),root.Members["arithmetic"].Kind=='n'?null:DecodeSasObservation(value.arithmetic,SasObservationPoint.Arithmetic));
   if(value.applied!=decision.Applied || value.recordedMultiplier!=decision.RecordedMultiplier || value.postMultiplierAward!=decision.PostMultiplierAward)
    throw new PcRoundLifecycleDataException("SAS archived decision differs from reconstructed arithmetic");
   return decision;
  }
  private static void ValidateCandidateSasDecisionEvidence(RoulettePendingSettlementCandidate candidate,
   string encoded,string scope,string roundId,string spinId,string paytableHash,
   int paytableMaximum,int tableCount,long staticCap,string adapterPolicy,string displayPolicy)
  {
   if(candidate==null || candidate.Sas==null)throw new PcRoundLifecycleDataException("SAS candidate required");
   var actual=DecodeSasDecisionEvidence(encoded,scope,roundId,spinId);
   var value=JsonUtility.FromJson<PcSasDecisionEvidenceV1>(encoded);
   if(!SasConfigEnvelopeCodecV3.IsDigest(candidate.PaytableResourceSha256) || paytableHash!=candidate.PaytableResourceSha256 ||
    value.profile!=candidate.ProfileId || value.paytableSha256!=candidate.PaytableResourceSha256 ||
    value.paytableMaximumWager!=paytableMaximum || value.tableCount!=tableCount || value.staticMaxBetLimit!=staticCap ||
    value.adapterPolicyId!=adapterPolicy || value.displayPolicyId!=displayPolicy)
    throw new PcRoundLifecycleDataException("SAS resource or policy differs from frozen inputs");
   var expected=candidate.Sas;
   if(actual.Qualification.EvaluatedAward!=expected.Qualification.EvaluatedAward ||
    actual.Qualification.Wager!=expected.Qualification.Wager || actual.Qualification.EffectiveMaximumWager!=expected.Qualification.EffectiveMaximumWager ||
    actual.PostMultiplierAward!=candidate.CopyResult().Returned || actual.Applied!=expected.Applied || actual.RecordedMultiplier!=expected.RecordedMultiplier)
    throw new PcRoundLifecycleDataException("SAS decision differs from settlement candidate");
   RequireSameSasObservation(expected.Qualification.Observation,actual.Qualification.Observation);
   RequireSameSasObservation(expected.Display,actual.Display);RequireSameSasObservation(expected.Recorded,actual.Recorded);RequireSameSasObservation(expected.Arithmetic,actual.Arithmetic);
  }
  private static void ValidateSasDecisionProvenance(string encoded,SasConfigObservationReceiptV1[] frozen)
  {
   if(frozen==null)throw new PcRoundLifecycleDataException("Frozen provenance required");
   var root=new LifecycleJsonReader(encoded).Read();ValidateLifecycleJsonShape(root,typeof(PcSasDecisionEvidenceV1),encoded);
   var value=JsonUtility.FromJson<PcSasDecisionEvidenceV1>(encoded);
   var names=new[]{"qualification","display","recorded","arithmetic"};
   var values=new[]{value.qualification,value.display,value.recorded,value.arithmetic};int index=0;
   for(int i=0;i<names.Length;i++)
   {
    if(root.Members[names[i]].Kind=='n')continue;
    if(index>=frozen.Length || frozen[index]==null)throw new PcRoundLifecycleDataException("Frozen observation catalogue missing");
    var expected=frozen[index++];var actual=values[i];
    RequireSameSasObservation(expected.Observation,DecodeSasObservation(actual,(SasObservationPoint)i));
    if(actual.eventSource!=expected.EventSource || actual.eventId!=expected.EventId || actual.eventSequence!=expected.EventSequence)
        throw new PcRoundLifecycleDataException("SAS event provenance differs from captured receipt");
   }
   if(index!=frozen.Length)throw new PcRoundLifecycleDataException("Unexpected frozen observations");
  }
  private static void RequireSameSasObservation(SasObservation expected,SasObservation actual)
  {
   if(expected==null || actual==null){if(expected!=actual)throw new PcRoundLifecycleDataException("SAS observation presence differs");return;}
   if(expected.Point!=actual.Point || expected.Generation!=actual.Generation ||
    expected.RuntimePendingDisableObserved!=actual.RuntimePendingDisableObserved || !expected.Payload.CopyBytes().SequenceEqual(actual.Payload.CopyBytes()))
    throw new PcRoundLifecycleDataException("SAS candidate configuration observation differs");
  }
  private static SasObservation DecodeSasObservation(PcSasObservationV1 value,SasObservationPoint point)
  {
   if(value==null){if(point==SasObservationPoint.Qualification)throw new PcRoundLifecycleDataException("Qualification missing");return null;}
   if(value.point!=point.ToString() || value.configGeneration<0 || value.eventSequence<0 || string.IsNullOrWhiteSpace(value.eventSource)||string.IsNullOrWhiteSpace(value.eventId))
    throw new PcRoundLifecycleDataException("SAS observation point or provenance invalid");
   byte[] raw=Convert.FromBase64String(value.configRaw32Base64);
   if(raw.Length!=32 || Convert.ToBase64String(raw)!=value.configRaw32Base64 || SasConfigSnapshotCodecV2.Digest(raw)!=value.configRaw32Sha256)
    throw new PcRoundLifecycleDataException("SAS observation bytes or hash invalid");
   return new SasObservation(point,value.configGeneration,new SasPayload(raw),value.runtimePendingDisableObserved);
  }
 }
}
