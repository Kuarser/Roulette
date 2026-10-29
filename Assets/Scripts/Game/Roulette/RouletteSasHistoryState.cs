using System;
using System.Linq;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private static SasHistoryStateV1 BuildSasHistorySuccessor(SasHistoryStateV1 prior,int[] priorPagePockets,int pocket)
  {
   if(pocket<0||pocket>36)throw new PcRoundLifecycleDataException("SAS successor pocket invalid");
   ValidateSasHistoryState(prior,priorPagePockets);
   int[] pockets=new[]{pocket}.Concat(prior.pockets).Take(HistogramCapacity).ToArray();
   byte[] newest=Enumerable.Range(0,HistogramCapacity).Select(i=>i<pockets.Length?(byte)pockets[i]:(byte)255).ToArray();
   var next=new SasHistoryStateV1{pockets=pockets,snapshotsBase64=new[]{Convert.ToBase64String(newest)}.Concat(prior.snapshotsBase64).Take(50).ToArray()};
   ValidateSasHistoryState(next,new[]{pocket}.Concat(priorPagePockets).Take(50).ToArray());return next;
  }
  private static byte[][] ValidateSasHistoryState(SasHistoryStateV1 state,int[] pagePockets)
  {
   if(state==null||state.pockets==null||state.snapshotsBase64==null||pagePockets==null||state.pockets.Length>HistogramCapacity||state.snapshotsBase64.Length>50||state.snapshotsBase64.Length!=pagePockets.Length||state.pockets.Any(p=>p<0||p>36))throw new PcRoundLifecycleDataException("SAS history dimensions or pocket values invalid");
   var snapshots=new byte[state.snapshotsBase64.Length][];
   for(int i=0;i<snapshots.Length;i++)
   {
    try{snapshots[i]=Convert.FromBase64String(state.snapshotsBase64[i]);}catch(Exception e){if(!(e is FormatException||e is ArgumentNullException))throw;throw new PcRoundLifecycleDataException("SAS history snapshot encoding invalid");}
    var snapshot=snapshots[i];if(snapshot.Length!=HistogramCapacity||pagePockets[i]<0||pagePockets[i]>36||snapshot[0]!=pagePockets[i])throw new PcRoundLifecycleDataException("SAS history snapshot differs from page pocket");
    bool padding=false;foreach(byte value in snapshot){if(value==255)padding=true;else if(value>36||padding)throw new PcRoundLifecycleDataException("SAS snapshot pocket or padding invalid");}
    if(i==0){for(int j=0;j<HistogramCapacity;j++)if(snapshot[j]!=(j<state.pockets.Length?state.pockets[j]:255))throw new PcRoundLifecycleDataException("SAS histogram differs from latest snapshot");}
    else{for(int j=0;j<HistogramCapacity-1;j++)if(snapshots[i-1][j+1]!=snapshot[j])throw new PcRoundLifecycleDataException("SAS adjacent snapshots disagree");}
   }
   return snapshots;
  }
 }
}
