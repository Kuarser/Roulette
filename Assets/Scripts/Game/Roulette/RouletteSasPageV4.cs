using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  [Serializable] private sealed class RecordedPageV4 {public int originSchemaVersion;public RecordedRound round;public SasRoundRecordV1 sas;}
  private static RecordedRound[] ValidateV4PageCatalogue(string[] encodedPages)
  {
   if(encodedPages==null||encodedPages.Length>50)throw new PcRoundLifecycleDataException("V4 page catalogue dimensions invalid");
   var rounds=new RecordedRound[encodedPages.Length];var identities=new HashSet<string>(StringComparer.Ordinal);
   for(int i=0;i<encodedPages.Length;i++)rounds[i]=ValidateV4CatalogueEntry(new LifecycleJsonReader(encodedPages[i]).Read(),encodedPages[i],identities,null);
   return rounds;
  }
  private static RecordedRound ValidateV4CatalogueEntry(LifecycleJsonNode root,string source,HashSet<string> identities,RecordedPageV4 checkedPage)
  {
   if(root.Kind!='{')throw new PcRoundLifecycleDataException("V4 catalogue page must be an object");
   var page=checkedPage;
   if(page==null){ValidateLifecycleJsonShape(root,typeof(RecordedPageV4),source);page=JsonUtility.FromJson<RecordedPageV4>(root.Text(source));}
   if(page.originSchemaVersion<0||page.originSchemaVersion>4||root.Members["round"].Kind!='{'||(page.originSchemaVersion==4?root.Members["sas"].Kind!='{':root.Members["sas"].Kind!='n'))throw new PcRoundLifecycleDataException("V4 page origin and sidecar differ");
   var round=ParsePcReceiptContext<RecordedRound>(root.Members["round"].Text(source));
   if(page.originSchemaVersion==4)
   {
    if(!round.present)throw new PcRoundLifecycleDataException("New V4 page must contain a round");
    Guid id;foreach(string value in new[]{round.roundArchiveId,round.spinJournalId})if(string.IsNullOrEmpty(value)||!Guid.TryParseExact(value,"N",out id)||id==Guid.Empty||id.ToString("N")!=value)throw new PcRoundLifecycleDataException("New V4 page identity invalid");
    var record=page.sas;if(record.version!=1||record.roundArchiveId!=round.roundArchiveId||record.spinJournalId!=round.spinJournalId)throw new PcRoundLifecycleDataException("V4 page and sidecar identity differ");
   }
   foreach(string value in new[]{round.roundArchiveId,round.spinJournalId})if(!string.IsNullOrEmpty(value)&&!identities.Add(value))throw new PcRoundLifecycleDataException("V4 page catalogue identity duplicated");
   return round;
  }
  // Only called after strict validation of the entire RecordedRoundArchiveV4 tree.
  private static RecordedRound[] ValidateCheckedContainerPageHistory(LifecycleJsonNode pages,string source,RecordedRoundArchiveV4 archive)
  {
   if(pages.Kind!='['||pages.Items.Count>50||archive.pages==null||archive.pages.Length!=pages.Items.Count)throw new PcRoundLifecycleDataException("V4 page catalogue dimensions invalid");
   var rounds=new RecordedRound[pages.Items.Count];var identities=new HashSet<string>(StringComparer.Ordinal);
   for(int i=0;i<rounds.Length;i++)rounds[i]=ValidateV4CatalogueEntry(pages.Items[i],source,identities,archive.pages[i]);
   return ValidateV4PageHistoryState(rounds,archive.historyState);
  }
  private static RecordedRound[] ValidateV4PageHistory(string[] encodedPages,SasHistoryStateV1 state)
  {
   return ValidateV4PageHistoryState(ValidateV4PageCatalogue(encodedPages),state);
  }
  private static RecordedRound[] ValidateV4PageHistoryState(RecordedRound[] rounds,SasHistoryStateV1 state)
  {
   if(state==null||state.snapshotsBase64==null||state.snapshotsBase64.Length!=rounds.Length)throw new PcRoundLifecycleDataException("V4 page/history count differs");
   int[] pagePockets=new int[rounds.Length];
   for(int i=0;i<rounds.Length;i++)
   {
    if(rounds[i].present){pagePockets[i]=rounds[i].pocket;continue;}
    // Imported legacy placeholders have no authenticated round pocket.
    byte[] snapshot;try{snapshot=Convert.FromBase64String(state.snapshotsBase64[i]);}catch(Exception e){if(!(e is FormatException||e is ArgumentNullException))throw;throw new PcRoundLifecycleDataException("Legacy placeholder snapshot invalid");}
    if(snapshot.Length!=HistogramCapacity||snapshot[0]>36)throw new PcRoundLifecycleDataException("Legacy placeholder snapshot dimensions invalid");pagePockets[i]=snapshot[0];
   }
   ValidateSasHistoryState(state,pagePockets);return rounds;
  }
  private static string[] ImportLegacyPagesV4(string sourceArchive,IList<byte[]> snapshots)
  {
   if(snapshots==null)throw new PcRoundLifecycleDataException("Legacy import requires validated snapshots");
   RecordedRound[] rounds;PcRoundLifecycleRecord lifecycle;PcDoubleFeatureNvV1 feature;PcGamePaymentRecordV1 payment;RecordedHistoryOverflowV1 overflow;int schema;
   bool versioned=TryDecodeLifecycleArchiveWithPayment(sourceArchive,snapshots,out rounds,out lifecycle,out feature,out payment,out overflow,out schema);
   if(!versioned)
   {
    var root=new LifecycleJsonReader(sourceArchive).Read();if(root.Kind!='{')throw new PcRoundLifecycleDataException("Legacy page source must be archive object");
    var legacy=JsonUtility.FromJson<RecordedRoundArchive>(sourceArchive);if(legacy==null||legacy.rounds==null||legacy.rounds.Length!=snapshots.Count)throw new PcRoundLifecycleDataException("Legacy page source count differs");
    rounds=legacy.rounds.Select((r,i)=>IsValidRecordedRound(r,snapshots[i])?r:new RecordedRound()).ToArray();schema=0;
   }
   if(schema<0||schema>3)throw new PcRoundLifecycleDataException("Unsupported legacy page source");
   int origin=schema;return rounds.Select(r=>"{\"originSchemaVersion\":"+origin+",\"round\":"+JsonUtility.ToJson(r)+",\"sas\":null}").ToArray();
  }
  private static readonly string[] SasRoundRecordFields={"version","roundArchiveId","spinJournalId","adapterPolicyId","displayPolicyId","evaluationIdentity","qualification","displayObservation","recordedObservation","arithmeticObservation","evaluatedAward","postMultiplierAward","applied","recordedMultiplier","eligible","ineligibleReason","readyCommitDigest"};
  private static string EncodeSasRoundRecordV1(RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint)
  {
   var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);var decision=candidate.Sas;
   string projection=BuildSasCheckpointReadyProjection(candidate,checkpoint,frozen.paytableMaximumWager,frozen.tableCount,frozen.staticMaxBetLimit,frozen.adapterPolicyId,frozen.displayPolicyId);
   if(SasReadyCommitDigest(projection)!=checkpoint.ReadyDigest)throw new PcRoundLifecycleDataException("SAS round record digest differs");
   var record=new SasRoundRecordV1{version=1,roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,adapterPolicyId=frozen.adapterPolicyId,displayPolicyId=frozen.displayPolicyId,evaluationIdentity=checkpoint.FrozenInputs,qualification=checkpoint.QualificationReceipt.ToPayload(),displayObservation=checkpoint.Display==null?null:checkpoint.Display.ToPayload(),recordedObservation=checkpoint.Recorded==null?null:checkpoint.Recorded.ToPayload(),arithmeticObservation=checkpoint.Arithmetic==null?null:checkpoint.Arithmetic.ToPayload(),evaluatedAward=decision.Qualification.EvaluatedAward,postMultiplierAward=decision.PostMultiplierAward,applied=decision.Applied,recordedMultiplier=decision.RecordedMultiplier,eligible=decision.Qualification.Eligible,ineligibleReason=decision.Qualification.Failure.ToString(),readyCommitDigest=checkpoint.ReadyDigest};
   return WriteSasCanonicalObject(record,SasRoundRecordFields);
  }
  private static string CanonicalizeSasRoundRecordV1(string encoded)
  {
   var root=new LifecycleJsonReader(encoded).Read();ValidateLifecycleJsonShape(root,typeof(SasRoundRecordV1),encoded);var value=JsonUtility.FromJson<SasRoundRecordV1>(encoded);
   if(root.Members["displayObservation"].Kind=='n')value.displayObservation=null;if(root.Members["recordedObservation"].Kind=='n')value.recordedObservation=null;if(root.Members["arithmeticObservation"].Kind=='n')value.arithmeticObservation=null;
   return WriteSasCanonicalObject(value,SasRoundRecordFields);
  }
  // New-page codec only. Legacy imports require container provenance validation.
  private static string EncodeSasCandidatePageV4(string roundRaw,RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,bool awaitingPayout)
  {
   ValidateSasCandidateRoundPage(candidate,roundRaw,checkpoint,awaitingPayout);
   string decision=EncodeSasRoundRecordV1(candidate,checkpoint);
   return "{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(ParsePcReceiptContext<RecordedRound>(roundRaw))+",\"sas\":"+decision+"}";
  }
  private static RecordedRound DecodeSasCandidatePageV4(string encoded,RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,bool awaitingPayout)
  {
   var root=new LifecycleJsonReader(encoded).Read();ValidateLifecycleJsonShape(root,typeof(RecordedPageV4),encoded);
   var page=JsonUtility.FromJson<RecordedPageV4>(encoded);
   if(page.originSchemaVersion!=4||root.Members["round"].Kind!='{'||root.Members["sas"].Kind!='{')throw new PcRoundLifecycleDataException("New SAS page requires origin4 and decision sidecar");
   string roundRaw=root.Members["round"].Text(encoded);ValidateSasCandidateRoundPage(candidate,roundRaw,checkpoint,awaitingPayout);
   string expected=EncodeSasCandidatePageV4(roundRaw,candidate,checkpoint,awaitingPayout);var expectedRoot=new LifecycleJsonReader(expected).Read();
   string decision=CanonicalizeSasRoundRecordV1(root.Members["sas"].Text(encoded));
   if(decision!=expectedRoot.Members["sas"].Text(expected))throw new PcRoundLifecycleDataException("SAS page sidecar differs from frozen checkpoint");
   return ParsePcReceiptContext<RecordedRound>(roundRaw);
  }
 }
}
