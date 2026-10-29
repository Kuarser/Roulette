using System.Diagnostics;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private OriginalPaysFlashClock pcPaysFlash;
        private decimal pcPaysOpenedCredit;
        private bool PcPaysReturnPending
        { get { return pcPaysFlash != null && pcPaysFlash.Exists && pcPaysFlash.Returning; } }

        private decimal PcPaysCredit
        {
            get
            {
                RouletteSession local = session as RouletteSession;
                return session == null ? 0m : local == null ? session.Credit :
                    (decimal)local.MoneyCents / session.Denomination;
            }
        }

        private void EnsurePcPaysFlash()
        {
            if (paysPage < 0) return;
            if (pcPaysFlash == null || !pcPaysFlash.Exists)
            {
                pcPaysFlash = new OriginalPaysFlashClock(paysPage, 3,
                    Stopwatch.GetTimestamp(), Stopwatch.Frequency);
                pcPaysOpenedCredit = PcPaysCredit;
            }
            else pcPaysFlash.SetPage(paysPage);
        }

        private void StopPcPaysFlash()
        {
            if (pcPaysFlash != null) pcPaysFlash.Stop();
            pcPaysFlash = null;
        }

        private void SuspendPcPaysFlash(bool suspended)
        {
            if (pcPaysFlash != null)
                pcPaysFlash.SuspendAt(suspended, Stopwatch.GetTimestamp());
        }

        private void ServicePcPaysFlash()
        {
            if (paysPage < 0) { StopPcPaysFlash(); return; }
            EnsurePcPaysFlash();
            OriginalPaysFlashAction action = pcPaysFlash.UpdateAt(Stopwatch.GetTimestamp(),
                pcPaysOpenedCredit != PcPaysCredit, PaytableTestActive);
            if (action == OriginalPaysFlashAction.HidePays) { ClosePays(); return; }
            if (action == OriginalPaysFlashAction.RequestReturn) { ReturnFromPays(); return; }
            if (action != OriginalPaysFlashAction.Buttons) return;
            SetPcPaysSelected(uiPaysBack, pcPaysFlash.PreviousSelected);
            SetPcPaysSelected(uiPaysExit, pcPaysFlash.ExitSelected);
            SetPcPaysSelected(uiPaysNext, pcPaysFlash.NextSelected);
        }

        private static void SetPcPaysSelected(Button button, bool selected)
        {
            if (button == null) return;
            var frames = button.GetComponent<RouletteOriginalButtonFrames>();
            if (frames != null) frames.SetSelected(selected);
        }
    }
}
