using System;
using System.Linq;
using System.Collections.Generic;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    // Immutable pending settlement calculation. No archive, wallet or live result
    // publication; the durable round coordinator must commit before publishing.
    public sealed class RoulettePendingSettlementCandidate
    {
        internal readonly RouletteSession Owner;
        public readonly long CreditBefore, WageredBefore, ReturnedBefore, RoundsBefore;
        public readonly int WinsBefore;
        public readonly long CreditAfterPayout, CreditAtPublication, WageredAfter, ReturnedAfter, RoundsAfter;
        public readonly int WinsAfter;
        public readonly bool Deferred, DoubleAvailable;
        public readonly long Denomination, MoneyRemainderCents, CreditBeforeCents, CreditAfterPayoutCents, WagerCents, AwardCents;
        public readonly SasDecision Sas;
        private readonly RoundResult result;
        private readonly RoundResult baseResult;
        public readonly int PriorHistoryCount;
        public readonly string ProfileId, PaytableId, PaytableResourceSha256, GameConfigResourceSha256;
        public readonly long SelectedChip;
        public readonly int PaytableMaximumWager;
        private readonly Dictionary<int,long> bets;
        private readonly Dictionary<int,List<long>> stacks;
        private readonly int[] evaluationOrder;
        private readonly RoundResult[] priorHistory;
        private readonly RouletteProfile profileSnapshot;
        internal RoulettePendingSettlementCandidate(RouletteSession owner,RoundResult basis,SasDecision sas,bool doubleAvailable)
        {
            if(owner==null || basis==null)throw new ArgumentNullException();
            long award=basis.Returned;
            if(sas!=null)
            {
                if(sas.Qualification.EvaluatedAward!=basis.Returned || sas.Qualification.Wager!=basis.Wager)
                    throw new ArgumentException("SAS decision differs from pending base award or wager");
                award=sas.PostMultiplierAward;
            }
            if(award<0)throw new ArgumentOutOfRangeException("award");
            ProfileId=owner.Profile.Id;PaytableId=owner.Profile.PaytableId;PaytableResourceSha256=owner.Profile.PaytableResourceSha256;GameConfigResourceSha256=owner.Profile.GameConfigResourceSha256;SelectedChip=owner.Chip;PriorHistoryCount=owner.History.Count;
            PaytableMaximumWager=owner.Profile.MaximumTotalBet;
            profileSnapshot=CopyProfile(owner.Profile);
            baseResult=Copy(basis);
            priorHistory=owner.History.Select(Copy).ToArray();
            bets=new Dictionary<int,long>(owner.Bets);
            stacks=owner.BetChipStacks.ToDictionary(p=>p.Key,p=>new List<long>(p.Value));
            evaluationOrder=owner.BetEvaluationOrder;
            Owner=owner;Sas=sas;CreditBefore=owner.Credit;WageredBefore=owner.Wagered;
            ReturnedBefore=owner.Returned;RoundsBefore=owner.Rounds;WinsBefore=owner.Wins;
            if(CreditBefore<0 || WageredBefore<0 || ReturnedBefore<0 || RoundsBefore<0 || WinsBefore<0 || WinsBefore>RoundsBefore)
                throw new ArgumentException("Invalid settlement accounting baseline");
            Denomination=owner.Denomination;MoneyRemainderCents=owner.MoneyRemainderCents;
            if(Denomination<1 || MoneyRemainderCents<0 || MoneyRemainderCents>=Denomination)
                throw new ArgumentException("Invalid settlement money baseline");
            Deferred=owner.BaseSettlementDeferred;
            CreditAfterPayout=checked(CreditBefore+award);
            CreditBeforeCents=checked(CreditBefore*Denomination+MoneyRemainderCents);
            CreditAfterPayoutCents=checked(CreditAfterPayout*Denomination+MoneyRemainderCents);
            WagerCents=checked(basis.Wager*Denomination);AwardCents=checked(award*Denomination);
            CreditAtPublication=Deferred?CreditBefore:CreditAfterPayout;
            WageredAfter=checked(WageredBefore+basis.Wager);ReturnedAfter=checked(ReturnedBefore+award);
            RoundsAfter=checked(RoundsBefore+1);WinsAfter=checked(WinsBefore+(basis.Returned>0?1:0));
            DoubleAvailable=doubleAvailable;result=Copy(basis);result.Returned=award;
        }
        // Admission/publication guard only; no wallet, RNG or archive mutation.
        public void RequireCurrentPendingBaseline(RouletteSession session)
        {
            if(session==null || !ReferenceEquals(session,Owner) || !session.SpinPending || !SameProfile(profileSnapshot,session.Profile) || session.Profile.Id!=ProfileId || session.Profile.PaytableId!=PaytableId ||
                session.Profile.PaytableResourceSha256!=PaytableResourceSha256 || session.Profile.GameConfigResourceSha256!=GameConfigResourceSha256 ||
                session.Credit!=CreditBefore || session.Wagered!=WageredBefore || session.Returned!=ReturnedBefore ||
                session.Rounds!=RoundsBefore || session.Wins!=WinsBefore || session.Denomination!=Denomination ||
                session.MoneyRemainderCents!=MoneyRemainderCents || session.Chip!=SelectedChip ||
                session.BaseSettlementDeferred!=Deferred || session.History.Count!=priorHistory.Length)
                throw new InvalidOperationException("Pending settlement accounting baseline changed");
            if(session.Bets.Count!=bets.Count || session.BetChipStacks.Count!=stacks.Count || !session.BetEvaluationOrder.SequenceEqual(evaluationOrder))
                throw new InvalidOperationException("Pending settlement betting baseline changed");
            foreach(var pair in bets){long amount;if(!session.Bets.TryGetValue(pair.Key,out amount)||amount!=pair.Value)throw new InvalidOperationException("Pending bet changed");}
            foreach(var pair in stacks){List<long> chips;if(!session.BetChipStacks.TryGetValue(pair.Key,out chips)||chips==null||!chips.SequenceEqual(pair.Value))throw new InvalidOperationException("Pending chip stack changed");}
            for(int i=0;i<priorHistory.Length;i++)if(!SameResult(priorHistory[i],session.History[i]))throw new InvalidOperationException("Pending history baseline changed");
            var current=session.BuildPendingSettlementCandidate(Sas);
            if(!SameResult(baseResult,current.baseResult)||current.DoubleAvailable!=DoubleAvailable)
                throw new InvalidOperationException("Pending evaluated outcome changed");
        }
        internal void RequireInstalledBaseIdentity(RouletteSession session)
        {
            if(session==null||!ReferenceEquals(session,Owner)||!SameProfile(profileSnapshot,session.Profile)||
                session.Chip!=SelectedChip||session.BaseSettlementDeferred!=Deferred||!SameResult(result,session.LastResult)||
                session.History.Count!=Math.Min(priorHistory.Length+1,50)||!SameResult(result,session.History[0])||
                session.Bets.Count!=bets.Count||session.BetChipStacks.Count!=stacks.Count||!session.BetEvaluationOrder.SequenceEqual(evaluationOrder))
                throw new InvalidOperationException("Installed SAS base identity changed");
            foreach(var pair in bets){long amount;if(!session.Bets.TryGetValue(pair.Key,out amount)||amount!=pair.Value)throw new InvalidOperationException("Installed SAS bet changed");}
            foreach(var pair in stacks){List<long> chips;if(!session.BetChipStacks.TryGetValue(pair.Key,out chips)||chips==null||!chips.SequenceEqual(pair.Value))throw new InvalidOperationException("Installed SAS chips changed");}
            for(int i=0;i<Math.Min(priorHistory.Length,49);i++)if(!SameResult(priorHistory[i],session.History[i+1]))throw new InvalidOperationException("Installed SAS history changed");
        }
        private static RouletteProfile CopyProfile(RouletteProfile p)
        {
            var copy=new RouletteProfile{Id=p.Id,PaytableId=p.PaytableId,MaximumSpotBet=p.MaximumSpotBet,MaximumTotalBet=p.MaximumTotalBet,MinimumRtp=p.MinimumRtp,MaximumRtp=p.MaximumRtp,Denominations=(int[])p.Denominations.Clone(),CategoryDividers=(double[])p.CategoryDividers.Clone()};
            foreach(var pair in p.Records){var r=pair.Value;copy.Records.Add(pair.Key,new WinRecord{Id=r.Id,Type=r.Type,MinimumBet=r.MinimumBet,BetMultiple=r.BetMultiple,WinLevel=r.WinLevel,ReturnMultiple=r.ReturnMultiple,Numbers=(int[])r.Numbers.Clone()});}return copy;
        }
        internal static bool SameProfile(RouletteProfile a,RouletteProfile b)
        {
            if(b==null||a.Id!=b.Id||a.PaytableId!=b.PaytableId||a.MaximumSpotBet!=b.MaximumSpotBet||a.MaximumTotalBet!=b.MaximumTotalBet||a.MinimumRtp!=b.MinimumRtp||a.MaximumRtp!=b.MaximumRtp||b.Denominations==null||b.CategoryDividers==null||!a.Denominations.SequenceEqual(b.Denominations)||!a.CategoryDividers.SequenceEqual(b.CategoryDividers)||a.Records.Count!=b.Records.Count)return false;
            foreach(var pair in a.Records){WinRecord y;var x=pair.Value;if(!b.Records.TryGetValue(pair.Key,out y)||y==null||x.Id!=y.Id||x.Type!=y.Type||x.MinimumBet!=y.MinimumBet||x.BetMultiple!=y.BetMultiple||x.WinLevel!=y.WinLevel||x.ReturnMultiple!=y.ReturnMultiple||y.Numbers==null||!x.Numbers.SequenceEqual(y.Numbers))return false;}return true;
        }
        internal static bool SameResult(RoundResult a,RoundResult b)
        {
            if(a==null||b==null)return a==b;
            return a.Pocket==b.Pocket&&a.Wager==b.Wager&&a.Returned==b.Returned&&a.Category==b.Category&&a.WinLimitReached==b.WinLimitReached&&
                a.WinningPays.Count==b.WinningPays.Count&&a.WinningPays.Zip(b.WinningPays,(x,y)=>x.BetId==y.BetId&&x.Amount==y.Amount&&x.WinLevel==y.WinLevel).All(x=>x)&&
                a.HistoryPayOverrides.Count==b.HistoryPayOverrides.Count&&a.HistoryPayOverrides.Zip(b.HistoryPayOverrides,(x,y)=>x.PayIndex==y.PayIndex&&x.Amount==y.Amount&&x.ProgressiveLevel==y.ProgressiveLevel&&x.Kind==y.Kind).All(x=>x);
        }
        public Dictionary<int,long> CopyBets(){return new Dictionary<int,long>(bets);}
        public Dictionary<int,List<long>> CopyChipStacks(){return stacks.ToDictionary(p=>p.Key,p=>new List<long>(p.Value));}
        public int[] CopyEvaluationOrder(){return (int[])evaluationOrder.Clone();}
        internal RoundResult[] CopyPriorHistory(){return priorHistory.Select(Copy).ToArray();}
        public RoundResult CopyBaseResult(){return Copy(baseResult);}
        public RoundResult CopyResult(){return Copy(result);}
        private static RoundResult Copy(RoundResult value)
        {
            return new RoundResult{Pocket=value.Pocket,Wager=value.Wager,Returned=value.Returned,
                Category=value.Category,WinLimitReached=value.WinLimitReached,
                WinningPays=value.WinningPays.Select(p=>new WinningPayRecord{BetId=p.BetId,Amount=p.Amount,WinLevel=p.WinLevel}).ToList(),
                HistoryPayOverrides=value.HistoryPayOverrides.Select(p=>new HistoryPayOverride{PayIndex=p.PayIndex,Amount=p.Amount,ProgressiveLevel=p.ProgressiveLevel,Kind=p.Kind}).ToList()};
        }
    }
}
