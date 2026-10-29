using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireSasLiveBasePaymentBaseline(IRouletteSession live,RoulettePendingSettlementCandidate candidate,PcGamePaymentRecordV2 payment)
        {
            var local=live as RouletteSession;
            if(local==null||candidate==null||payment==null||!object.ReferenceEquals(local,candidate.Owner)||
                local.Profile.Id!=payment.profile||local.Profile.PaytableResourceSha256!=candidate.PaytableResourceSha256||
                local.Profile.GameConfigResourceSha256!=candidate.GameConfigResourceSha256||
                local.Denomination!=payment.denomination||local.MoneyRemainderCents!=payment.remainderCents||
                local.Credit!=payment.WalletAfter||local.Rounds!=candidate.RoundsAfter||local.Wagered!=candidate.WageredAfter||
                local.Returned!=candidate.ReturnedAfter||local.Wins!=candidate.WinsAfter||!local.ResultPhase||local.SpinPending||local.InDoubleUp||
                local.RoundPayoutPending==payment.moneyApplied||local.LastResult==null||
                local.LastResult.Pocket!=candidate.CopyResult().Pocket||local.LastResult.Wager!=candidate.CopyResult().Wager||
                local.LastResult.Returned!=payment.baseAward)
                throw new PcRoundLifecycleDataException("SAS live base-payment identity, money or result changed");
            candidate.RequireInstalledBaseIdentity(local);
        }
    }
}
