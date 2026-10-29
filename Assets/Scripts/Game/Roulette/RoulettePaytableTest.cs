using Binary80 = RouletteDeluxe.OriginalBinary80;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace RouletteDeluxe
{
    // Paytable diagnostics own their table, wheel and result. This type has
    // no outcome source, balance, round history, journal or platform writer.
    public sealed class RoulettePaytableTest
    {
        private readonly RouletteProfile profile;
        private readonly Dictionary<int, long> bets = new Dictionary<int, long>();
        private readonly List<int> evaluationOrder = new List<int>();
        private readonly int[] stops;
        private readonly int maximumTotalBet, maximumSpotBet;
        private readonly long maximumWin;
        private readonly Binary80 step;
        private readonly double storedStep;
        public ReadOnlyDictionary<int, long> Bets { get; private set; }
        public int[] EvaluationOrder { get { return evaluationOrder.ToArray(); } }
        public int Chip { get; private set; } = 1;
        public int TotalBet { get; private set; }
        public int Pocket { get; private set; }
        public double WheelAngle { get; private set; }
        public float MainRenderAngle { get; private set; }
        public float SpindleRenderAngle { get; private set; }
        public double BallAngle { get; private set; }
        public RoundResult Result { get; private set; }

        // Zero limits mean unbounded. The operator/platform supplies these
        // independently of the live player's balance and bet configuration.
        public RoulettePaytableTest(RouletteProfile profile, int[] wheelStops,
            int maximumTotalBet, int maximumSpotBet, long maximumWin,
            double framesPerStop)
        {
            if (profile == null) throw new ArgumentNullException("profile");
            if (wheelStops == null || wheelStops.Length != 37 ||
                !wheelStops.OrderBy(n => n).SequenceEqual(Enumerable.Range(0, 37)))
                throw new ArgumentException("Invalid source wheel stops");
            if (maximumTotalBet < 0 || maximumSpotBet < 0 || maximumWin < 0 ||
                double.IsNaN(framesPerStop) || double.IsInfinity(framesPerStop) || framesPerStop <= 0)
                throw new ArgumentOutOfRangeException("Invalid diagnostic limits");
            this.profile = profile;
            stops = (int[])wheelStops.Clone();
            this.maximumTotalBet = maximumTotalBet;
            this.maximumSpotBet = maximumSpotBet;
            this.maximumWin = maximumWin;
            Bets = new ReadOnlyDictionary<int, long>(bets);
            step = Binary80.Divide(Binary80.Integer(360), Binary80.Integer(stops.Length));
            storedStep = step.ToDouble();
            BallAngle = Binary80.Divide(step, Binary80.FromDouble(framesPerStop)).ToDouble();
            Nudge(); // Roulette::DisplayPaytableTest nudges before displaying.
        }

        public void SelectChip(int value)
        {
            if (value != 1 && value != 2 && value != 5 && value != 10)
                throw new ArgumentOutOfRangeException("value");
            Chip = value;
            Result = null; // Table event 12 hides the test result graphics.
        }

        public void PlaceBet(int id)
        {
            // PayTestHandleRouletteTableEvent destroys graphics before
            // handling any table event, including rejected touches.
            Result = null;
            WinRecord record;
            if (!profile.Records.TryGetValue(id, out record))
                throw new ArgumentException("Unknown diagnostic bet");
            long prior;
            bets.TryGetValue(id, out prior);
            long next = checked(prior + Chip);
            if (next < record.MinimumBet || next % record.BetMultiple != 0)
                throw new InvalidOperationException("Invalid diagnostic bet amount");
            if (maximumTotalBet != 0 && Chip > maximumTotalBet - TotalBet)
                throw new BetRejectedException(BetRejectionReason.TotalLimit);
            // HandleTableEvent's inactive-spot release branch checks only
            // the total cap; the spot cap is checked while stacking state 5.
            if (prior > 0 && maximumSpotBet != 0 && next > maximumSpotBet)
                throw new BetRejectedException(BetRejectionReason.SpotLimit);
            int total = checked(TotalBet + Chip);
            bets[id] = next;
            // Direct touch removes and rehooks an already active BetSpot.
            evaluationOrder.Remove(id);
            evaluationOrder.Add(id);
            TotalBet = total;
        }

        public void Clear()
        {
            bets.Clear();
            evaluationOrder.Clear();
            TotalBet = 0;
            Result = null;
        }

        public int Nudge()
        {
            ClearEvaluation();
            // 0x08279a90: x87 64-significand-bit intermediates, double field
            // stores, float renderer calls; +0.05f and truncate for lookup.
            Binary80 angle = Binary80.Add(Binary80.FromDouble(WheelAngle), step);
            Binary80 fullTurn = Binary80.Integer(360);
            while (Binary80.Compare(angle, fullTurn) >= 0)
                angle = Binary80.Subtract(angle, fullTurn);
            WheelAngle = angle.ToDouble();
            MainRenderAngle = angle.ToFloat();
            SpindleRenderAngle = (float)WheelAngle;
            Binary80 lookup = Binary80.Add(Binary80.Divide(
                Binary80.FromDouble(WheelAngle), Binary80.FromDouble(storedStep)),
                Binary80.FromDouble((double)0.05f));
            int index = lookup.Truncate();
            Pocket = stops[index < stops.Length ? index : 0];
            return Pocket;
        }

        public void ClearEvaluation()
        {
            // Roulette::PaytableTestButtonPress clears its cycle and pay
            // label before dispatching every button, including NUDGE.
            Result = null;
        }

        public RoundResult Evaluate()
        {
            long totalPay = 0;
            var pays = new List<WinningPayRecord>();
            foreach (int id in evaluationOrder)
            {
                WinRecord record = profile.Records[id];
                if (!record.Covers(Pocket)) continue;
                long amount = checked(bets[id] * record.ReturnMultiple);
                totalPay = checked(totalPay + amount);
                // The test path caps Result::totalPay after each Eval, while
                // individual pays retain the original amount and swap order.
                if (maximumWin != 0 && totalPay > maximumWin) totalPay = maximumWin;
                pays.Add(new WinningPayRecord { BetId = id, Amount = amount, WinLevel = record.WinLevel });
                RouletteSession.SortSourceWinningPays(pays);
            }
            Result = new RoundResult { Pocket = Pocket, Wager = TotalBet,
                Returned = totalPay, WinningPays = pays };
            return Result;
        }

    }
}
