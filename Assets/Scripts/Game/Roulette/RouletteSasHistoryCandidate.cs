using System;
using System.Linq;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private sealed class SasPageHistoryCandidate
  {
   private readonly string[] pages;private readonly string historyRaw;
   public readonly string OverflowRaw;
   public SasPageHistoryCandidate(string[] value,SasHistoryStateV1 history,string overflow){pages=(string[])value.Clone();historyRaw=UnityEngine.JsonUtility.ToJson(history);OverflowRaw=overflow;}
   public string[] CopyPages(){return (string[])pages.Clone();}
   public SasHistoryStateV1 CopyHistory(){return UnityEngine.JsonUtility.FromJson<SasHistoryStateV1>(historyRaw);}
  }
  [Serializable] private sealed class SasHistoryOverflowV2 {public RecordedPageV4 page;public string snapshotBase64;}
  private static void ValidateSasHistoryOverflow(string overflowRaw,string[] pages,SasHistoryStateV1 history)
  {
   ValidateV4PageHistory(pages,history);if(overflowRaw==null)return;
   if(pages.Length!=50)throw new PcRoundLifecycleDataException("V4 overflow requires a full page catalogue");
   var root=new LifecycleJsonReader(overflowRaw).Read();ValidateLifecycleJsonShape(root,typeof(SasHistoryOverflowV2),overflowRaw);
   if(root.Members["page"].Kind!='{')throw new PcRoundLifecycleDataException("V4 overflow page missing");
   string removed=root.Members["page"].Text(overflowRaw);ValidateV4PageCatalogue(new[]{removed});
   var value=UnityEngine.JsonUtility.FromJson<SasHistoryOverflowV2>(overflowRaw);byte[] snapshot;
   try{snapshot=Convert.FromBase64String(value.snapshotBase64);}catch(Exception e){if(!(e is FormatException||e is ArgumentNullException))throw;throw new PcRoundLifecycleDataException("V4 overflow snapshot encoding invalid");}
   if(snapshot.Length!=HistogramCapacity||snapshot[0]>36)throw new PcRoundLifecycleDataException("V4 overflow snapshot dimensions invalid");
   var removedState=new SasHistoryStateV1{pockets=snapshot.TakeWhile(p=>p!=255).Select(p=>(int)p).ToArray(),snapshotsBase64=new[]{value.snapshotBase64}};
   ValidateV4PageHistory(new[]{removed},removedState);
   byte[] oldest=Convert.FromBase64String(history.snapshotsBase64[49]);for(int i=0;i<HistogramCapacity-1;i++)if(oldest[i+1]!=snapshot[i])throw new PcRoundLifecycleDataException("V4 overflow chronology differs");
   foreach(string page in pages)ValidateV4PageCatalogue(new[]{page,removed});
  }
  private static SasPageHistoryCandidate BuildSasPageHistorySuccessor(string[] priorPages,SasHistoryStateV1 priorHistory,string newPage)
  {
   ValidateV4PageHistory(priorPages,priorHistory);var added=ValidateV4PageCatalogue(new[]{newPage})[0];
   if(UnityEngine.JsonUtility.FromJson<RecordedPageV4>(newPage).originSchemaVersion!=4||!added.present)throw new PcRoundLifecycleDataException("New successor page must have origin4");
   int[] priorPockets=priorHistory.snapshotsBase64.Select(s=>(int)Convert.FromBase64String(s)[0]).ToArray();
   var nextHistory=BuildSasHistorySuccessor(priorHistory,priorPockets,added.pocket);string[] nextPages=new[]{newPage}.Concat(priorPages).Take(50).ToArray();ValidateV4PageHistory(nextPages,nextHistory);
   string overflow=priorPages.Length==50?"{\"page\":"+priorPages[49]+",\"snapshotBase64\":"+QuoteSasCanonical(priorHistory.snapshotsBase64[49])+"}":null;
   ValidateSasHistoryOverflow(overflow,nextPages,nextHistory);
   return new SasPageHistoryCandidate(nextPages,nextHistory,overflow);
  }
 }
}
