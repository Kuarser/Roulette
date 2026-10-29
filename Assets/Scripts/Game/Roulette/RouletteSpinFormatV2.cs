using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  [Serializable] private sealed class PcSpinDrawContextV2
  {public int version;public string legacyContextRaw,legacyContextSha256;public int roundFormat;}
  [Serializable] private sealed class PendingSpinEnvelopeV2
  {public int version;public PendingSpinRecord pending;public string formatContextRaw,formatContextSha256;}
  [Serializable] private sealed class PcSpinContextVersion {public int version;}
  private string pcPreparedSpinContextRaw,pcSpinRoundFormatContextRaw;
  private bool PcPendingUsesV4 {get{return pcSpinRoundFormatContextRaw!=null;}}

  // The durable host catalogue records first local SAS enable even if a later
  // disable leaves M=0/1. A started legacy round never consults this again.
  private bool SelectPcSpinV4Format()
  {
   if(temporaryHistory)return false;
   bool v4=pcRoundArchiveSchemaVersion==4||sasRecordedPageWrappersV4.Count!=0;
   if(sasHostConfigCoordinator!=null)
   {
    var stored=RequirePcSasHostConfigurationReadback();
    v4|=stored.CopyReceipts().Any(r=>SasHostMessage.Decode(r.Event.CopyWire()).Enabled);
   }
   if(v4&&!PcReceiptEnabled)throw new PcRoundLifecycleDataException("V4 round format requires the PC RNG receipt owner");
   return v4;
  }
  private string EncodePcSpinFormatContextV2(string legacyRaw)
  {
   var legacy=ParsePcReceiptContext<PcSpinDrawContext>(legacyRaw);ValidatePcSpinDrawContext(legacy,null);
   return JsonUtility.ToJson(new PcSpinDrawContextV2{version=2,legacyContextRaw=legacyRaw,
    legacyContextSha256=PcReceiptTextDigest(legacyRaw),roundFormat=4});
  }
  private PcSpinDrawContext DecodePcSpinFormatContextV2(string raw,PcRngDrawReceipt receipt)
  {
   string formatRaw;var legacy=ParsePcSpinDrawContextRaw(raw,out formatRaw);
   RequirePcReceipt(formatRaw==raw,"Expected V4 wheel format wrapper");
   ValidatePcSpinDrawContext(legacy,receipt);return legacy;
  }
  private static PcSpinDrawContext ParsePcSpinDrawContextRaw(string raw,out string formatRaw)
  {
   formatRaw=null;var header=JsonUtility.FromJson<PcSpinContextVersion>(raw);
   RequirePcReceipt(header!=null&&(header.version==1||header.version==2),"unsupported wheel context version");
   if(header.version==1)return ParsePcReceiptContext<PcSpinDrawContext>(raw);
   var format=ParsePcReceiptContext<PcSpinDrawContextV2>(raw);
   RequirePcReceipt(format.version==2&&format.roundFormat==4&&format.legacyContextSha256==PcReceiptTextDigest(format.legacyContextRaw),
    "V4 wheel format wrapper or raw digest differs");
   var legacy=ParsePcReceiptContext<PcSpinDrawContext>(format.legacyContextRaw);
   RequirePcReceipt(legacy.version==1&&legacy.pending!=null,"V4 wheel wrapper lacks frozen v1 context");
   formatRaw=raw;return legacy;
  }
  private PcSpinDrawContext DecodePcSpinDrawContext(string raw,PcRngDrawReceipt receipt,out string formatRaw)
  {
   var value=ParsePcSpinDrawContextRaw(raw,out formatRaw);ValidatePcSpinDrawContext(value,receipt);return value;
  }
  private PendingSpinRecord DecodePendingSpinEnvelope(string raw,out string formatRaw)
  {
   formatRaw=null;var header=JsonUtility.FromJson<PcSpinContextVersion>(raw);
   RequirePcReceipt(header!=null&&(header.version==1||header.version==2),"unsupported Pending envelope version");
   // Preserve the historical Pending reader's optional fields and meaning.
   if(header.version==1)return JsonUtility.FromJson<PendingSpinRecord>(raw);
   var pending=ParseSasPendingSpinRaw(raw,out formatRaw);
   DecodePcSpinFormatContextV2(formatRaw,null);return pending;
  }
  private static PendingSpinRecord ParseSasPendingSpinRaw(string raw)
  {string formatRaw;return ParseSasPendingSpinRaw(raw,out formatRaw);}
  private static PendingSpinRecord ParseSasPendingSpinRaw(string raw,out string formatRaw)
  {
   formatRaw=null;var header=JsonUtility.FromJson<PcSpinContextVersion>(raw);
   RequirePcReceipt(header!=null&&(header.version==1||header.version==2),"unsupported SAS Pending version");
   if(header.version==1)return ParsePcReceiptContext<PendingSpinRecord>(raw);
   var value=ParsePcUnpaidReceiptContext<PendingSpinEnvelopeV2>(raw);
   RequirePcReceipt(value.version==2&&value.pending!=null&&value.formatContextSha256==PcReceiptTextDigest(value.formatContextRaw),
    "V4 Pending format digest differs");
   string receiptFormat;var context=ParsePcSpinDrawContextRaw(value.formatContextRaw,out receiptFormat);var pending=value.pending;
   RequirePcReceipt(receiptFormat==value.formatContextRaw,"V4 Pending contains a legacy format context");
   RequirePcReceipt(pending.pocket>=0&&pending.pocket<=36&&
    (pending.hasWheelStopIndex?pending.wheelStopIndex>=0&&pending.wheelStopIndex<37:pending.wheelStopIndex==0)&&
    JsonUtility.ToJson(pending.CopyForReceiptComparison())==JsonUtility.ToJson(context.pending),
    "V4 Pending identity or money differs from frozen format context");
   formatRaw=value.formatContextRaw;return pending;
  }
  private string EncodePendingSpinEnvelope(PendingSpinRecord pending,string formatRaw)
  {
   if(formatRaw==null)return JsonUtility.ToJson(pending);
   string encoded=JsonUtility.ToJson(new PendingSpinEnvelopeV2{version=2,pending=pending,formatContextRaw=formatRaw,
    formatContextSha256=PcReceiptTextDigest(formatRaw)});
   string verified;DecodePendingSpinEnvelope(encoded,out verified);
   RequirePcReceipt(verified==formatRaw,"Pending writer lost exact format context");return encoded;
  }
  private void SavePcPendingSpinValue(PendingSpinRecord pending,string formatRaw)
  {
   string encoded=EncodePendingSpinEnvelope(pending,formatRaw);
   PlayerPrefs.SetString(PendingSpinKey,encoded);PlayerPrefs.Save();
   RequirePcReceipt(PlayerPrefs.GetString(PendingSpinKey,"")==encoded,"Pending save readback differs");
  }
  private void RequirePendingSpinFormatMatchesReceipt(PcRngDrawReceipt receipt,string expectedFormatRaw)
  {
   RequirePcReceipt(pcSpinRoundFormatContextRaw==expectedFormatRaw,"Pending round format conflicts with its RNG receipt");
   if(expectedFormatRaw==null)return;
   RequirePcReceipt(receipt!=null&&receipt.context.contextJson==expectedFormatRaw,"Pending lacks original V4 format receipt");
   string savedFormat;var saved=DecodePendingSpinEnvelope(PlayerPrefs.GetString(PendingSpinKey,""),out savedFormat);
   RequirePcReceipt(savedFormat==expectedFormatRaw&&JsonUtility.ToJson(saved)==JsonUtility.ToJson(pendingSpin),
    "Pending durable format or record differs before RNG acknowledgement");
  }
 }
}
