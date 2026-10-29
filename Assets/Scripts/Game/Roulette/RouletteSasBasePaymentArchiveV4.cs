using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static string BuildSasBasePaymentArchiveV4(string initialArchive,PcGamePaymentRecordV2 payment,string displayRaw,
            RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            ValidateSasInitialArchiveV4(initialArchive,candidate,checkpoint,initialPage,priorPages,priorHistory);
            var pageRoot=new LifecycleJsonReader(initialPage).Read();var round=ParsePcReceiptContext<RecordedRound>(pageRoot.Members["round"].Text(initialPage));
            round.creditAfter=payment.WalletAfter;round.awaitingFinalPayout=!payment.historyPublished;
            string page="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(round)+",\"sas\":"+pageRoot.Members["sas"].Text(initialPage)+"}";
            var history=BuildSasPageHistorySuccessor(priorPages,priorHistory,initialPage);var pages=history.CopyPages();pages[0]=page;
            var life=new PcRoundLifecycleRecord{roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,profile=candidate.ProfileId,
                roundStage=payment.historyPublished?PcRoundStage.AwaitEnd:PcRoundStage.RoundOpen};
            string encoded="{\"lifecycleSchemaVersion\":4,\"pages\":["+string.Join(",",pages)+"],\"lifecycle\":"+EncodeLifecycleJsonValue(life)+
                ",\"featureNv\":null,\"payment\":"+EncodeSasPaymentV2(payment,candidate.Sas.Applied)+",\"historyOverflow\":"+
                (payment.historyPublished?"null":history.OverflowRaw??"null")+",\"historyState\":"+JsonUtility.ToJson(history.CopyHistory())+
                ",\"displayState\":"+displayRaw+",\"consumedSasIntentDigest\":"+QuoteSasCanonical(checkpoint.ReadyDigest)+"}";
            ValidateSasBasePaymentArchiveV4(encoded,candidate,checkpoint,initialPage,priorPages,priorHistory);return encoded;
        }
        // Base owner continuation against admitted immutable settlement evidence.
        // Double owner and independent historical recovery use separate validators.
        private static void ValidateSasBasePaymentArchiveV4(string encoded,RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            var root=new LifecycleJsonReader(encoded).Read();if(root.Kind!='{')throw new PcRoundLifecycleDataException("V4 base payment archive missing");
            ValidateLifecycleJsonShape(root,typeof(RecordedRoundArchiveV4),encoded);
            var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(encoded);
            if(value.lifecycleSchemaVersion!=4||value.consumedSasIntentDigest!=checkpoint.ReadyDigest||root.Members["featureNv"].Kind!='n'||
                root.Members["pages"].Kind!='['||root.Members["historyState"].Kind!='{'||priorPages==null||priorPages.Length!=candidate.PriorHistoryCount)
                throw new PcRoundLifecycleDataException("V4 base payment envelope differs");
            var payment=DecodeSasPaymentV2(root.Members["payment"].Text(encoded),candidate.Sas.Applied);
            RequireSasBaseOfferEligibility(payment,candidate.DoubleAvailable);
            var expected=BuildSasInitialPayment(candidate,checkpoint,initialPage,true);
            if(payment.owner!=PcGamePaymentOwner.Base||payment.roundArchiveId!=expected.roundArchiveId||payment.spinJournalId!=expected.spinJournalId||
                payment.profile!=expected.profile||payment.operationId!=expected.operationId||payment.priorHistoryCount!=expected.priorHistoryCount||
                payment.creditBefore!=expected.creditBefore||payment.denomination!=expected.denomination||payment.remainderCents!=expected.remainderCents||
                payment.baseAward!=expected.baseAward||payment.sasRoundDigest!=expected.sasRoundDigest)
                throw new PcRoundLifecycleDataException("V4 base payment immutable evidence differs");
            if(payment.moneyApplied&&payment.finalAward>0&&payment.route==PcGamePaymentRouteV2.Ordinary&&payment.replyValue==0)
                throw new PcRoundLifecycleDataException("V4 ordinary money precedes accepted reply");
            var history=BuildSasPageHistorySuccessor(priorPages,priorHistory,initialPage);
            string[] pages=root.Members["pages"].Items.Select(n=>n.Text(encoded)).ToArray();
            ValidateV4PageHistory(pages,value.historyState);
            if(pages.Length!=history.CopyPages().Length||!pages.Skip(1).SequenceEqual(history.CopyPages().Skip(1))||
                JsonUtility.ToJson(value.historyState)!=JsonUtility.ToJson(history.CopyHistory()))
                throw new PcRoundLifecycleDataException("V4 base payment history changed");
            var pageRoot=new LifecycleJsonReader(pages[0]).Read();var round=ParsePcReceiptContext<RecordedRound>(pageRoot.Members["round"].Text(pages[0]));
            if(round.creditAfter!=payment.WalletAfter||round.awaitingFinalPayout==payment.historyPublished)
                throw new PcRoundLifecycleDataException("V4 wallet or publication differs from payment");
            round.creditAfter=candidate.CreditAtPublication;round.awaitingFinalPayout=true;
            string normalized="{\"originSchemaVersion\":4,\"round\":"+JsonUtility.ToJson(round)+",\"sas\":"+pageRoot.Members["sas"].Text(pages[0])+"}";
            DecodeSasCandidatePageV4(normalized,candidate,checkpoint,true);
            string overflow=root.Members["historyOverflow"].Text(encoded);
            if(overflow!=(payment.historyPublished?"null":history.OverflowRaw??"null"))throw new PcRoundLifecycleDataException("V4 overflow publication differs");
            ValidateSasHistoryOverflow(overflow=="null"?null:overflow,pages,value.historyState);
            var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(encoded));RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
            var publishedRound=ParsePcReceiptContext<RecordedRound>(pageRoot.Members["round"].Text(pages[0]));
            ValidateLifecycleLatestIdentity(lifecycle,publishedRound);ValidateRoundLifecyclePayload(lifecycle,publishedRound);
            if(lifecycle.roundStage!=(payment.historyPublished?PcRoundStage.AwaitEnd:PcRoundStage.RoundOpen))
                throw new PcRoundLifecycleDataException("V4 base payment lifecycle differs");
            var display=DecodeSasCheckpointDisplay(root.Members["displayState"].Text(encoded),checkpoint);
            if(payment.prepared&&display!=null&&!display.hideCompleted)throw new PcRoundLifecycleDataException("V4 payment precedes display hide");
        }
    }
}
