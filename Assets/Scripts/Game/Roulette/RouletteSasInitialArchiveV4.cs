using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable] private sealed class RecordedRoundArchiveV4
        {
            public int lifecycleSchemaVersion;
            public RecordedPageV4[] pages;
            public PcRoundLifecycleRecord lifecycle;
            public PcDoubleFeatureNvV1 featureNv;
            public PcGamePaymentRecordV2 payment;
            public SasHistoryOverflowV2 historyOverflow;
            public SasHistoryStateV1 historyState;
            public SasDisplayStateV1 displayState;
            public string consumedSasIntentDigest;
        }
        // First commit only, requiring admitted predecessor pages/history. It is
        // not a standalone historical archive decoder or a PlayerPrefs writer.
        private static string BuildSasInitialArchiveV4(RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
            string newPage,string[] priorPages,SasHistoryStateV1 priorHistory,string displayRaw,string priorArchiveRaw,string priorPocketsRaw,string priorSnapshotsRaw)
        {
            if(priorPages==null||priorPages.Length!=candidate.PriorHistoryCount)
                throw new PcRoundLifecycleDataException("V4 predecessor page count differs from settlement baseline");
            RequireSasInitialCandidatePredecessorEvidence(candidate,checkpoint,priorPages,priorHistory,priorArchiveRaw,priorPocketsRaw,priorSnapshotsRaw);
            var payment=BuildSasInitialPayment(candidate,checkpoint,newPage,true);
            DecodeSasCheckpointDisplay(displayRaw,checkpoint);
            var history=BuildSasPageHistorySuccessor(priorPages,priorHistory,newPage);
            var lifecycle=new PcRoundLifecycleRecord{roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,
                profile=candidate.ProfileId,roundStage=PcRoundStage.RoundOpen};
            string lifecycleRaw=EncodeLifecycleJsonValue(lifecycle);
            string encoded="{\"lifecycleSchemaVersion\":4,\"pages\":["+string.Join(",",history.CopyPages())+
                "],\"lifecycle\":"+lifecycleRaw+",\"featureNv\":null,\"payment\":"+EncodeSasPaymentV2(payment,candidate.Sas.Applied)+
                ",\"historyOverflow\":"+(history.OverflowRaw??"null")+",\"historyState\":"+JsonUtility.ToJson(history.CopyHistory())+
                ",\"displayState\":"+displayRaw+",\"consumedSasIntentDigest\":"+QuoteSasCanonical(checkpoint.ReadyDigest)+"}";
            ValidateSasInitialArchiveV4(encoded,candidate,checkpoint,newPage,priorPages,priorHistory);return encoded;
        }
        private static void ValidateSasInitialArchiveV4(string encoded,RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
            string expectedNewPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            var root=new LifecycleJsonReader(encoded).Read();if(root.Kind!='{')throw new PcRoundLifecycleDataException("V4 archive must be object");
            ValidateLifecycleJsonShape(root,typeof(RecordedRoundArchiveV4),encoded);
            if(priorPages==null||priorPages.Length!=candidate.PriorHistoryCount||root.Members["pages"].Kind!='['||root.Members["historyState"].Kind!='{')
                throw new PcRoundLifecycleDataException("Initial V4 pages or history missing");
            var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(encoded);
            if(value.lifecycleSchemaVersion!=4||value.consumedSasIntentDigest!=checkpoint.ReadyDigest||root.Members["featureNv"].Kind!='n')
                throw new PcRoundLifecycleDataException("Initial V4 archive version, digest or feature differs");
            var pages=root.Members["pages"].Items.Select(n=>n.Text(encoded)).ToArray();
            var expectedHistory=BuildSasPageHistorySuccessor(priorPages,priorHistory,expectedNewPage);
            if(!pages.SequenceEqual(expectedHistory.CopyPages())||JsonUtility.ToJson(value.historyState)!=JsonUtility.ToJson(expectedHistory.CopyHistory()))
                throw new PcRoundLifecycleDataException("Initial V4 archive successor differs from predecessor");
            string overflow=root.Members["historyOverflow"].Text(encoded);
            if(overflow!=(expectedHistory.OverflowRaw??"null"))throw new PcRoundLifecycleDataException("Initial V4 overflow differs");
            ValidateSasHistoryOverflow(overflow=="null"?null:overflow,pages,value.historyState);
            var latest=DecodeSasCandidatePageV4(pages[0],candidate,checkpoint,true);
            var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(encoded));
            RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
            ValidateLifecycleLatestIdentity(lifecycle,latest);ValidateRoundLifecyclePayload(lifecycle,latest);
            if(lifecycle.roundStage!=PcRoundStage.RoundOpen)throw new PcRoundLifecycleDataException("Initial V4 lifecycle already advanced");
            DecodeSasInitialPayment(root.Members["payment"].Text(encoded),pages[0],candidate,checkpoint,true);
            DecodeSasCheckpointDisplay(root.Members["displayState"].Text(encoded),checkpoint);
        }
    }
}
