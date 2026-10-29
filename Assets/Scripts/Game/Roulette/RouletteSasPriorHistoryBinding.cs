using System;
using System.Linq;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Bind only the prior results retained in the new 50-result model history.
  // Older-page mathematical authority remains a separate admission requirement.
  private static void RequireSasPriorHistoryBinding(RecordedRound[] pages,int priorCount,RoundResult[] prior)
  {
   int retained=Math.Min(priorCount,49);
   if(priorCount<0||priorCount>50||pages==null||pages.Length!=Math.Min(priorCount+1,50)||prior==null||
    (prior.Length!=retained&&prior.Length!=priorCount))throw new PcRoundLifecycleDataException("Cold SAS preceding history dimensions differ");
   for(int i=0;i<retained;i++)
   {
    var page=pages[i+1];var result=prior[i];
    if(page==null||!page.present||result==null||result.Pocket!=page.pocket||result.Wager!=page.wager||result.Returned!=page.returned||result.Category!=page.category)
     throw new PcRoundLifecycleDataException("Cold SAS preceding financial result differs from page");
    var pays=page.winningPays??new RecordedPay[0];var overrides=page.historyPayOverrides??new RecordedHistoryPayOverride[0];
    if(result.WinningPays==null||result.WinningPays.Count!=pays.Length||
     !result.WinningPays.Zip(pays,(a,b)=>a!=null&&b!=null&&a.BetId==b.id&&a.Amount==b.amount).All(same=>same)||
     result.HistoryPayOverrides==null||result.HistoryPayOverrides.Count!=overrides.Length||
     !result.HistoryPayOverrides.Zip(overrides,(a,b)=>a!=null&&b!=null&&a.PayIndex==b.payIndex&&a.Amount==b.amount&&a.ProgressiveLevel==b.progressiveLevel&&a.Kind==b.kind).All(same=>same))
     throw new PcRoundLifecycleDataException("Cold SAS preceding pay metadata differs from page");
   }
  }
 }
}
