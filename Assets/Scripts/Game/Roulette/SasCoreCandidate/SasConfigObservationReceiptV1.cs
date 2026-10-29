using System;
namespace RouletteDeluxe.SasDraftV1
{
 public sealed class SasConfigObservationReceiptV1
 {
  public readonly SasObservation Observation;
  public readonly string EventSource,EventId;
  public readonly long EventSequence;
  internal SasConfigObservationReceiptV1(SasObservation observation,string source,string id,long sequence)
  {
   if(observation==null || string.IsNullOrWhiteSpace(source)||string.IsNullOrWhiteSpace(id)||sequence<0)throw new ArgumentException("Invalid frozen observation provenance");
   Observation=observation;EventSource=source;EventId=id;EventSequence=sequence;
  }
  public PcSasObservationV1 ToPayload()
  {
   byte[] raw=Observation.Payload.CopyBytes();
   return new PcSasObservationV1{point=Observation.Point.ToString(),configGeneration=Observation.Generation,
    configRaw32Base64=Convert.ToBase64String(raw),configRaw32Sha256=SasConfigSnapshotCodecV2.Digest(raw),
    eventSource=EventSource,eventId=EventId,eventSequence=EventSequence,runtimePendingDisableObserved=Observation.RuntimePendingDisableObserved};
  }
 }
}
