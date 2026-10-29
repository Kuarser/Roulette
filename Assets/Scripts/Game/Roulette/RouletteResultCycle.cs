using System;
using System.Diagnostics;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // FileDefs::FileDefs initializes CycleResultDelay to 1500 ms. The
        // packaged game filedefs.xml does not override that tag.
        private const float OriginalCycleResultDelaySeconds = 1.5f;
        private float resultCycleStartedAt = -1f;
        private long resultCycleStartedTimestamp;
        private long winnerDeadlineTimestamp;
        private long resultDisplayDeadlineTimestamp;
        private bool currentTableResultMarkerCleared;
        private OriginalResultCycleClock pcResultCycleClock;
        private long pcResultCycleClockOrigin;

        private void ServicePcResultCycle()
        {
            EnsurePcResultCycle();
            if (pcResultCycleClock != null)
                pcResultCycleClock.UpdateAt(Stopwatch.GetTimestamp());
        }

        private void EnsurePcResultCycle()
        {
            if (resultCycleStartedTimestamp == 0 || currentGameResultsCleared ||
                session == null || session.LastResult == null ||
                session.LastResult.WinningPays.Count == 0)
            {
                if (pcResultCycleClock != null) pcResultCycleClock.Stop();
                pcResultCycleClock = null;
                pcResultCycleClockOrigin = 0;
                return;
            }
            if (pcResultCycleClock != null && pcResultCycleClockOrigin == resultCycleStartedTimestamp) return;
            pcResultCycleClockOrigin = resultCycleStartedTimestamp;
            pcResultCycleClock = new OriginalResultCycleClock(session.LastResult.WinningPays.Count,
                1500, resultCycleStartedTimestamp, Stopwatch.Frequency);
            // InitCycleResults can create a new owner after Game.Suspend has
            // already paused the previous one. Start does not inherit a global
            // pause value; only a subsequent Suspend call pauses this owner.
        }

        private void SuspendPcResultCycle(bool suspended)
        {
            EnsurePcResultCycle();
            if (pcResultCycleClock == null) return;
            if (suspended) pcResultCycleClock.PauseAt(Stopwatch.GetTimestamp());
            else pcResultCycleClock.ResumeAt(Stopwatch.GetTimestamp());
        }

        private void ApplyClearedGameResultsPresentation(bool restartingFooter,
            bool restoring = false)
        {
            if (!restoring) ClearPcCollectMeter();
            currentGameResultsCleared = true;
            // HideMarkers is transient. RestoreFromNV can reconstruct the
            // prior number marker from saved wheel stops and retained bets,
            // independently of the newly emptied current award.
            // +0x91ab (edited bets) and +0x919d (completed game) are separate
            // from the PC betting/feature phase. A rejected bet keeps both
            // source flags unchanged, allowing the number marker on restore.
            currentTableResultMarkerCleared = !restoring || session == null ||
                sourceBetsChangedSincePlay ||
                (!gameOverMessageLatched && !session.InDoubleUp) || session.TotalBet == 0;
            // +0x254 destroys RouletteCycleResults, including its messages,
            // winning bet sprites and total-win meters. LastResult belongs
            // to the PC settlement archive and must retain its paid amount.
            resultCycleStartedAt = -1f;
            resultCycleStartedTimestamp = 0;
            EnsurePcResultCycle();
            // ClearTotalWin calls AnimMessageManager::Stop(false), which
            // stops the active message as well as its extra animations.
            // Preserve message eligibility for the next explicit restart.
            sourcePromptStopped = true;
            failurePrompt = PromptKind.None;
            promptStartAfterFailure = PromptKind.None;
            promptStartNow = PromptKind.None;
            promptPhase = PromptPlaybackPhase.None;
            shownPrompt = PromptKind.None;
            // RestoreFromNV explicitly calls SetMessages with no delay.
            // Startup subsequently enters its normal idle scheduling; an
            // in-session profile restore already has a UI and resumes here.
            if (restoring) RestartSourcePromptAfterCreditChange();
            // CanDoubleUpResults rejects the zero live award. Finalize the
            // PC's outstanding offer metadata without changing credits,
            // feature configuration, or the archived settlement amount.
            ResolveSourceDoubleOffer();
            // ClearPaid restarts gameplay animation in source state 1 when
            // ShouldDisplayGoodLuckMessage accepts the non-AU jurisdiction.
            if (restartingFooter && !auNationalStandardsEnabled && session != null &&
                sourceGameplayState == SourceGameplayState.Idle)
                StartFooterMovie();
        }

        private static long ResultClockDeadline(long start, float seconds,
            long frequency)
        {
            if (seconds <= 0f || frequency <= 0) return start;
            return checked(start + (long)Math.Ceiling(seconds * (double)frequency));
        }

        private static bool ResultClockActive(long deadline, long now)
        {
            return deadline != 0 && now < deadline;
        }

        private static double CycleElapsedSeconds(long started, long now,
            long frequency)
        {
            if (frequency <= 0 || now <= started) return 0d;
            return (double)(now - started) / frequency;
        }

        private static int WinPayAtTime(RoundResult result, double elapsedSeconds)
        {
            if (result == null || result.WinningPays == null ||
                result.WinningPays.Count == 0 || elapsedSeconds < 0d)
                return -1;
            long halfCycle = (long)Math.Floor(
                elapsedSeconds / OriginalCycleResultDelaySeconds);
            if ((halfCycle & 1) != 0) return -1;
            int index = (int)((halfCycle / 2) % result.WinningPays.Count);
            return result.WinningPays[index].BetId;
        }

        private int VisibleWinningPayId()
        {
            if (session == null || currentGameResultsCleared || !session.ResultPhase || spinning ||
                session.LastResult == null || PcCurrentRoundAward <= 0 ||
                TurboAutoPlayActive || resultCycleStartedTimestamp == 0)
                return -1;
            // InitCycleResults excludes Turbo, but ordinary active Auto uses
            // the same Start/Trigger/Cycle sequence as manual play.
            EnsurePcResultCycle();
            int index = pcResultCycleClock == null ? -1 : pcResultCycleClock.VisibleIndex;
            return index < 0 ? -1 : session.LastResult.WinningPays[index].BetId;
        }

        private WinningPayRecord VisibleWinningPayRecord()
        {
            int id = VisibleWinningPayId();
            if (id < 0 || session.LastResult == null) return null;
            foreach (WinningPayRecord pay in session.LastResult.WinningPays)
                if (pay.BetId == id) return pay;
            return null;
        }
    }
}
