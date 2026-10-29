using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireSasBaseOfferEligibility(PcGamePaymentRecordV2 payment,bool available)
        {
            if(payment.baseOffer!=PcGamePaymentOffer.None&&!available)
                throw new PcRoundLifecycleDataException("SAS Base offer was not available for the admitted settlement");
        }
        // First-commit payment only. Later Double/payment pages require their
        // lifecycle and feature validators; they cannot enter through this path.
        private static PcGamePaymentRecordV2 BuildSasInitialPayment(RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string pageRaw,bool awaitingPayout)
        {
            if(!awaitingPayout)throw new PcRoundLifecycleDataException("Initial V4 page must await payment publication");
            DecodeSasCandidatePageV4(pageRaw,candidate,checkpoint,awaitingPayout);
            if(!candidate.Deferred)throw new PcRoundLifecycleDataException("V4 payment requires deferred wallet publication");
            var payment=new PcGamePaymentRecordV2{roundArchiveId=checkpoint.RoundId,spinJournalId=checkpoint.SpinId,
                profile=candidate.ProfileId,operationId=checkpoint.RoundId+":game-payout",priorHistoryCount=candidate.PriorHistoryCount,
                creditBefore=candidate.CreditBefore,denomination=candidate.Denomination,remainderCents=candidate.MoneyRemainderCents,
                baseAward=candidate.CopyResult().Returned,sasRoundDigest=checkpoint.ReadyDigest};
            payment.Validate(candidate.Sas.Applied);return payment;
        }
        private static PcGamePaymentRecordV2 DecodeSasInitialPayment(string paymentRaw,string pageRaw,
            RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,bool awaitingPayout)
        {
            var expected=BuildSasInitialPayment(candidate,checkpoint,pageRaw,awaitingPayout);
            var actual=DecodeSasPaymentV2(paymentRaw,candidate.Sas.Applied);
            if(EncodeSasPaymentV2(actual,candidate.Sas.Applied)!=EncodeSasPaymentV2(expected,candidate.Sas.Applied))
                throw new PcRoundLifecycleDataException("Initial V4 payment differs from ReadyCommit candidate");
            return actual;
        }
    }
}
