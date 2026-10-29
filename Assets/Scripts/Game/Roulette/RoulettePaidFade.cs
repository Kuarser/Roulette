using System.Diagnostics;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // This is process-local presentation state. A restored settled game
        // starts fully lit, just as a newly constructed source ProView does.
        private OriginalPaidFadeClock paidFadeClock = new OriginalPaidFadeClock();
        private float PaidOpacity { get { return paidFadeClock.Alpha; } }

        private void ApplyPaidOpacity(float alpha)
        {
            if (uiPaid != null) uiPaid.Opacity = alpha;
            if (uiCashPaid != null) uiCashPaid.Opacity = alpha;
            // ProViewHistory is a separately constructed source panel with
            // its own alpha. Main-panel fades must not alter its labels.
        }

        private void ServicePaidFade()
        {
            if (!paidFadeClock.Running) return;
            paidFadeClock.UpdateAt(Stopwatch.GetTimestamp(), Stopwatch.Frequency,
                ApplyPaidOpacity, Stopwatch.GetTimestamp);
        }

        // Explicit timestamps keep boundary diagnostics independent of the
        // renderer's frame rate. Production uses the after-callback clock.
        private void ServicePaidFadeAt(long now, long frequency)
        {
            if (paidFadeClock.UpdateAt(now, frequency)) ApplyPaidOpacity(PaidOpacity);
        }
    }
}
