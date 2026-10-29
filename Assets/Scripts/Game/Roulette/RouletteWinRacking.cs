using System;
using System.Collections;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalRackClock
    {
        public static long AfterMilliseconds(long timestamp, int milliseconds,
            long frequency)
        {
            if (milliseconds < 0 || frequency <= 0)
                throw new ArgumentOutOfRangeException("milliseconds");
            // A requested wait must not finish before its whole millisecond.
            long ticks = checked(((long)milliseconds * frequency + 999L) / 1000L);
            return checked(timestamp + ticks);
        }

        public static int ElapsedMilliseconds(long startTimestamp,
            long nowTimestamp, long frequency)
        {
            long elapsed = ElapsedMillisecondsLong(startTimestamp, nowTimestamp,
                frequency);
            return elapsed >= int.MaxValue ? int.MaxValue : (int)elapsed;
        }

        public static long ElapsedMillisecondsLong(long startTimestamp,
            long nowTimestamp, long frequency)
        {
            if (frequency <= 0)
                throw new ArgumentOutOfRangeException("frequency");
            long elapsed = nowTimestamp - startTimestamp;
            if (elapsed <= 0) return 0;
            double milliseconds = Math.Floor((double)elapsed * 1000d / frequency);
            return milliseconds >= long.MaxValue ? long.MaxValue : (long)milliseconds;
        }

        public static bool GameDelayExpired(long gameStartTimestamp,
            long nowTimestamp, int gameDelayMilliseconds, long frequency)
        {
            if (gameDelayMilliseconds < 0)
                throw new ArgumentOutOfRangeException("gameDelayMilliseconds");
            return gameDelayMilliseconds == 0 || ElapsedMilliseconds(
                gameStartTimestamp, nowTimestamp, frequency) >= gameDelayMilliseconds;
        }

        // PerformRollup discounts a service interval that took more than
        // twice the requested delay. It moves the rack's start clock forward
        // by that interval instead of spending the entire remaining budget.
        public static int StalledServiceMilliseconds(int durationMilliseconds,
            int previousRemainingMilliseconds, int previousIntervalMilliseconds,
            int currentRemainingMilliseconds, int requestedDelayMilliseconds)
        {
            if (previousRemainingMilliseconds >= durationMilliseconds ||
                previousIntervalMilliseconds == 0) return 0;
            long interval = (long)previousRemainingMilliseconds -
                            currentRemainingMilliseconds;
            return interval > (long)requestedDelayMilliseconds * 2L &&
                   interval <= int.MaxValue ? (int)interval : 0;
        }
    }

    public sealed partial class RouletteGame
    {
        private static readonly long[] RackTierMaximum =
            { 29, 99, 199, 499, 999, 2999, 4999, 9999, long.MaxValue };
        private static readonly int[] RackDurationMilliseconds =
            { 957, 2194, 2583, 4361, 9708, 26122, 34928, 80050, 161000 };
        private static readonly float[] RackVolume =
            { .34f, .34f, .34f, .34f, .34f, .40f, .40f, .44f, .52f };
        private AudioSource rackAudioSource;
        private AudioClip[] rackSounds;
        private bool winRackActive;
        private long winRackPaid;
        private long winRackFinal;
        private long winRackBaseCredit;
        private float winRackStartedAt;
        private long winRackStartTimestamp;
        private long winRackNextServiceTimestamp;
        private int winRackDelayMilliseconds;
        private long winRackNextPaid;
        private long winRackIncrement;
        private bool winRackAutoSkip;
        private bool winRackSnapped;
        private int rackSnapFrame = -1;
        private bool rackSnapPointerNeedsRelease;
        private long gameStartTimestamp;
        private int gameDelayMilliseconds;
        private int winRackTier;
        private int winRackPreviousRemainingMilliseconds;
        private int winRackPreviousIntervalMilliseconds;

        private static int WinRackTier(long paid)
        {
            for (int i = 0; i < RackTierMaximum.Length; ++i)
                if (paid <= RackTierMaximum[i]) return i;
            return RackTierMaximum.Length - 1;
        }

        private long DisplayedCredit
        {
            get { return winRackActive ? winRackBaseCredit + winRackPaid :
                PcUnpaidBaseProjection ? checked(session.Credit + PcUnpaidBaseProjectionAward) :
                PcDoublePayoutPresentationActive ? checked(session.Credit + PcCurrentRoundAward) : session.Credit; }
        }

        private long DisplayedPaid
        {
            // PerformRollup sends each manual service amount to the Paid
            // meter at virtual +0x304; AutoPlay skips to the final value.
            get { return currentGameResultsCleared ? 0 : winRackActive ? winRackPaid :
                !session.ResultPhase || session.LastResult == null ? 0 :
                PcDoublePayoutPresentationActive ? PcCurrentRoundAward : session.LastResult.Returned; }
        }

        private void LoadWinRacking()
        {
            rackSounds = new AudioClip[RackTierMaximum.Length];
            for (int i = 0; i < rackSounds.Length; ++i)
                rackSounds[i] = Resources.Load<AudioClip>("Audio/WinRacking/" + (i + 1));
            rackAudioSource = gameObject.AddComponent<AudioSource>();
            rackAudioSource.playOnAwake = false;
        }

        private void StartWinRacking(long paid)
        {
            // Game::PerformRollup stops the gameplay panel animation before
            // the paid meter begins advancing. The original Outro still
            // shows GOOD LUCK! in its opening frames.
            StopFooterMovie();
            winRackFinal = paid;
            winRackPaid = 0;
            winRackBaseCredit = PcUnpaidBaseProjection ? session.Credit : session.Credit - paid;
            winRackTier = WinRackTier(paid);
            long nominalSteps = Math.Max(1,
                RackDurationMilliseconds[winRackTier] / 35);
            winRackIncrement = Math.Max(1, paid / nominalSteps);
            winRackDelayMilliseconds = paid / nominalSteps < 1 ?
                (int)Math.Max(1L, RackDurationMilliseconds[winRackTier] / paid) : 35;
            winRackNextPaid = 1;
            winRackAutoSkip = autoplay;
            winRackSnapped = false;
            winRackPreviousRemainingMilliseconds = int.MaxValue;
            winRackPreviousIntervalMilliseconds = 0;
            winRackStartedAt = Time.realtimeSinceStartup;
            winRackStartTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            winRackNextServiceTimestamp = OriginalRackClock.AfterMilliseconds(
                winRackStartTimestamp, winRackDelayMilliseconds,
                System.Diagnostics.Stopwatch.Frequency);
            winRackActive = true;
            if (rackAudioSource != null)
            {
                rackAudioSource.Stop();
                rackAudioSource.volume = RackVolume[winRackTier];
                rackAudioSource.clip = rackSounds[winRackTier];
                if (rackAudioSource.clip != null) rackAudioSource.Play();
                else Debug.LogError("Missing original win-rack WAV tier " + (winRackTier + 1));
            }
            Debug.Log("ROULETTE_RACK_START: tier=" + (winRackTier + 1) +
                      " paid=" + paid + " durationMs=" + RackDurationMilliseconds[winRackTier] +
                      " stepMs=" + winRackDelayMilliseconds +
                      " autoSkip=" + winRackAutoSkip);
        }

        private void CompleteTurboWinWithoutRack(long paid)
        {
            // PerformRollup returns before setting up a fixed-time rack when
            // Game::TurboAutoPlay is true. InitCycleResults also skips its
            // winner animation, but StateDisplayResults still runs its
            // AutoPlay result-wait service loops.
            // Turbo skips PerformRollup's rack, but Game::StatePayout still
            // stops the gameplay panel for a positive return.
            StopFooterMovie();
            winRackActive = false;
            winRackFinal = paid;
            winRackPaid = paid;
            if (!CompleteSourceBaseRollup()) BeginSourceResultDisplay(paid, true);
            Debug.Log("ROULETTE_TURBO_RACK_SKIPPED: paid=" + paid +
                      " credit=" + session.Credit);
            ServiceSourceRoundLifecycle();
        }

        private void TickWinRacking()
        {
            if (PcGameAppSuspended) return;
            if (!winRackActive) return;
            long nowTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            if (nowTimestamp < winRackNextServiceTimestamp) return;
            if (autoplay)
            {
                FinishWinRacking();
                return;
            }
            winRackPaid = Math.Min(winRackNextPaid, winRackFinal);
            if (winRackNextPaid > winRackFinal - winRackIncrement)
            {
                FinishWinRacking();
                return;
            }
            winRackNextPaid += winRackIncrement;
            AdaptWinRackStep(nowTimestamp);
            winRackNextServiceTimestamp = OriginalRackClock.AfterMilliseconds(
                nowTimestamp, winRackDelayMilliseconds,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private bool TrySnapWinRackingInput(bool filteredInputAvailable,
            bool pointerDown = false, Action inputAction = null)
        {
            if (!filteredInputAvailable || !winRackActive || autoplay ||
                (!pcLifecycleColdRound && !OriginalRackClock.GameDelayExpired(gameStartTimestamp,
                    System.Diagnostics.Stopwatch.GetTimestamp(),
                    gameDelayMilliseconds, System.Diagnostics.Stopwatch.Frequency)))
                return false;
            // The original WaitInput filter accepts the queued touch-down
            // (0x200001) or button-down (0x200010) event before sprite hit
            // dispatch. After GameDelay expires, the snap hook processes
            // input and the rack publishes the complete Paid/Credit meters.
            sourceDispatchingRackInput = true;
            try { if (inputAction != null) inputAction(); }
            finally { sourceDispatchingRackInput = false; }
            winRackSnapped = true;
            rackSnapFrame = Time.frameCount;
            rackSnapPointerNeedsRelease = pointerDown;
            FinishWinRacking();
            Debug.Log("ROULETTE_RACK_INPUT_SNAP: paid=" + winRackPaid +
                      " credit=" + session.Credit);
            return true;
        }

        private IEnumerator RackInputSnapSmoke()
        {
            yield return new WaitUntil(() => winRackActive);
            if (winRackFinal != 36 || winRackPaid != 0 || winRackAutoSkip)
                throw new InvalidOperationException(
                    "Rack input-snap fixture did not reach manual source tier 2");
            int configuredDelay = gameDelayMilliseconds;
            bool useSourceQueue = UsesSourcePlayerInput;
            bool savedModuleEnabled = sourceInputModule != null && sourceInputModule.enabled;
            if (useSourceQueue && (sourceInputModule == null || uiZeroBetButton == null ||
                SourcePlayerInputs.Count != 0))
                throw new InvalidOperationException("Rack source-input fixture needs an idle queue and table UI");
            try
            {
                if (useSourceQueue) sourceInputModule.enabled = false;
                int elapsed = OriginalRackClock.ElapsedMilliseconds(
                    gameStartTimestamp, System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency);
                gameDelayMilliseconds = elapsed + 1000;
                if (TrySnapWinRackingInput(true) || !winRackActive ||
                    winRackPaid != 0)
                    throw new InvalidOperationException(
                        "A touch bypassed the original minimum game delay");
                bool snapped;
                if (useSourceQueue)
                {
                    Canvas.ForceUpdateCanvases();
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(null,
                        uiZeroBetButton.transform.TransformPoint(
                            ((RectTransform)uiZeroBetButton.transform).rect.center));
                    long wager = session.TotalBet, credit = session.Credit, rounds = session.Rounds;
                    sourceInputModule.QueuePointerSample(
                        UnityEngine.EventSystems.PointerInputModule.kMouseLeftId,
                        point, true, false, true, false);
                    UnityEngine.EventSystems.PointerEventData pointer = sourceInputModule.QueuePointerSample(
                        UnityEngine.EventSystems.PointerInputModule.kMouseLeftId,
                        point, false, true, false, false);
                    if (SourcePlayerInputs.Count != 2)
                        throw new InvalidOperationException("Rack source-input fixture did not queue its down and release");
                    ServiceSourcePlayerInput();
                    if (SourcePlayerInputs.Count != 2 || !winRackActive || winRackPaid != 0)
                        throw new InvalidOperationException("Rack WaitInput dispatched before the shared game delay expired");
                    gameDelayMilliseconds = 0;
                    ServiceSourcePlayerInput();
                    snapped = winRackSnapped;
                    if (SourcePlayerInputs.Count != 1 || session.TotalBet != wager ||
                        session.Credit != credit || session.Rounds != rounds)
                        throw new InvalidOperationException("Rack snap purged the release or placed an unintended wager");
                    ServiceSourcePlayerInput();
                    RequireSourcePointerReleased(pointer);
                    if (SourcePlayerInputs.Count != 0)
                        throw new InvalidOperationException("Offer WaitInput did not consume the retained release");
                }
                else
                {
                    gameDelayMilliseconds = 0;
                    snapped = TrySnapWinRackingInput(true, true);
                }
                if (!snapped || winRackActive ||
                    !winRackSnapped || winRackPaid != 36 ||
                    DisplayedCredit != session.Credit ||
                    DisplayedPaid != 36)
                    throw new InvalidOperationException(
                        "Touch input did not publish full rack meters after delay");
                if (!SourceDoubleOfferActive || gameOverMessageLatched || TableTouchInputEnabled())
                    throw new InvalidOperationException(
                        "Rack touch dismissed the real offer or reopened the table");
                if (PrepareSourceSpin(false) || spinning || session.SpinPending)
                    throw new InvalidOperationException(
                        "A new wager bypassed the source offer wait");
                TakeSourceDoubleOffer();
                ServiceSourceRoundLifecycle();
                ServiceConfiguredAutoPlay();
                if (SourceDoubleOfferActive || sourceRoundEndCount != 1 ||
                    session.SpinPending || session.Rounds != 1 || !gameOverMessageLatched)
                    throw new InvalidOperationException("TAKE did not close exactly the settled round");
                yield return null;
                yield return null;
                if (rackSnapPointerNeedsRelease || !TableTouchInputEnabled())
                    throw new InvalidOperationException(
                        "Table input stayed locked after the rack touch ended");
                Debug.Log("ROULETTE_RACK_INPUT_SNAP_PASSED: paid=36 credit=" +
                          session.Credit + " sourceQueue=" + useSourceQueue);
            }
            finally
            {
                gameDelayMilliseconds = configuredDelay;
                if (useSourceQueue) sourceInputModule.enabled = savedModuleEnabled;
            }
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private void AdaptWinRackStep(long nowTimestamp)
        {
            // PerformRollup recalculates from its monotonic start, the unpaid
            // credits and the number of remaining whole service intervals.
            long unpaid = winRackFinal - winRackNextPaid;
            if (unpaid <= 0) return;
            int elapsedMs = OriginalRackClock.ElapsedMilliseconds(
                winRackStartTimestamp, nowTimestamp,
                System.Diagnostics.Stopwatch.Frequency);
            int remainingMs = RackDurationMilliseconds[winRackTier] - elapsedMs;
            int stalledMs = OriginalRackClock.StalledServiceMilliseconds(
                RackDurationMilliseconds[winRackTier],
                winRackPreviousRemainingMilliseconds,
                winRackPreviousIntervalMilliseconds, remainingMs,
                winRackDelayMilliseconds);
            if (stalledMs != 0)
            {
                winRackStartTimestamp = OriginalRackClock.AfterMilliseconds(
                    winRackStartTimestamp, stalledMs,
                    System.Diagnostics.Stopwatch.Frequency);
                remainingMs = RackDurationMilliseconds[winRackTier] -
                    OriginalRackClock.ElapsedMilliseconds(winRackStartTimestamp,
                        nowTimestamp, System.Diagnostics.Stopwatch.Frequency);
            }
            winRackPreviousIntervalMilliseconds =
                winRackPreviousRemainingMilliseconds == int.MaxValue ?
                    1 : winRackPreviousRemainingMilliseconds - remainingMs;
            winRackPreviousRemainingMilliseconds = remainingMs;
            if (remainingMs <= winRackDelayMilliseconds)
            {
                winRackIncrement = unpaid;
                winRackDelayMilliseconds = Math.Max(1, remainingMs);
                return;
            }
            int remainingSteps = remainingMs / winRackDelayMilliseconds;
            long increment = unpaid / Math.Max(1, remainingSteps);
            if (increment > 0)
                winRackIncrement = increment;
            else
            {
                winRackIncrement = 1;
                winRackDelayMilliseconds = (int)Math.Max(1, remainingMs / unpaid);
            }
        }

        private void FinishWinRacking()
        {
            winRackPaid = winRackFinal;
            winRackActive = false;
            if (!CompleteSourceBaseRollup()) BeginSourceResultDisplay(winRackFinal, false);
            if ((autoplay || winRackSnapped) && rackAudioSource != null &&
                rackAudioSource.isPlaying)
                StartCoroutine(FadeWinRackSound());
            Debug.Log("ROULETTE_RACK_COMPLETE: paid=" + winRackPaid +
                      " credit=" + session.Credit + " elapsedMs=" +
                      OriginalRackClock.ElapsedMilliseconds(winRackStartTimestamp,
                          System.Diagnostics.Stopwatch.GetTimestamp(),
                          System.Diagnostics.Stopwatch.Frequency));
            ServiceSourceRoundLifecycle();
        }

        private void BeginSourceResultDisplay(long paid, bool turbo)
        {
            // A normal offer/feature returns here only after its decision and
            // visual exit. The final award chooses the native 20/30 waits.
            if (sourceRoundPhase == SourceRoundPhase.None) ResolveSourceDoubleOffer();
            if (!pcLifecycleColdDisplayEntry) PurgeSourcePlayerInputs();
            pcLifecycleColdDisplayEntry = false;
            resultSettledAt = Time.realtimeSinceStartup;
            resultSettledTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            proViewFlashStartedTimestamp = autoplay ? 0 :
                resultSettledTimestamp;
            winnerUntil = paid > 0 && !turbo ? resultSettledAt + WinningResultVisualSeconds : 0f;
            long resultClockStart = resultSettledTimestamp;
            winnerDeadlineTimestamp = paid > 0 && !turbo ? ResultClockDeadline(resultClockStart,
                WinningResultVisualSeconds, System.Diagnostics.Stopwatch.Frequency) : 0;
            BeginPcResultDisplayWait(paid);
            pressSpinAt = resultSettledAt + (paid == 0 ? 2.5f : 6f);
            if (sourceRoundPhase != SourceRoundPhase.None)
                configuredAutoPlayFinishPending = true;
        }

        private IEnumerator FadeWinRackSound()
        {
            float started = Time.realtimeSinceStartup;
            float initial = rackAudioSource.volume;
            while (rackAudioSource.isPlaying &&
                   Time.realtimeSinceStartup - started < .4f)
            {
                rackAudioSource.volume = initial *
                    (1f - (Time.realtimeSinceStartup - started) / .4f);
                yield return null;
            }
            rackAudioSource.Stop();
            rackAudioSource.volume = initial;
        }
    }
}
