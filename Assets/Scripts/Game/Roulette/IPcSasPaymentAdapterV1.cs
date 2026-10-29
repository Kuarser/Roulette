using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    // Process adapter only: it owns no wallet or archive. Every argument is a
    // detached copy of the admitted durable receipt. Try methods must be
    // nonblocking; false means that no response/acknowledgment is available.
    // A production transport must deduplicate each operationId + event kind
    // across restart before it can be installed here.
    public interface IPcSasPaymentAdapterV1
    {
        PcSasObservationV1 Observe(PcGamePaymentRecordV2 saved, SasObservationPoint point);
        bool TryQuery(PcGamePaymentRecordV2 saved, out int response);
        bool TryDispatch(PcGamePaymentRecordV2 saved, out int returned);
        bool TryOrdinaryReply(PcGamePaymentRecordV2 saved, out int reply);
        bool TryNotifyCredit(PcGamePaymentRecordV2 saved);
        bool TryReceiveCompletion(PcGamePaymentRecordV2 saved);
    }
}
