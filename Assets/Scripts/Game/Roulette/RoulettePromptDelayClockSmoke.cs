using System;
using System.Collections;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private int promptDelayChecks;

        private void AssertPromptDelayAt(long now, long frequency, bool visible,
            PromptKind kind, int frame, string before, string description)
        {
            RefreshPromptUiAt(now, frequency);
            PromptKind actualKind = failurePrompt == PromptKind.None ? shownPrompt : failurePrompt;
            if (promptRoot.gameObject.activeSelf != visible || uiPromptButton.interactable ||
                visible && (actualKind != kind || uiPrompt.texture != PromptFrames(kind, false)[frame]))
                throw new InvalidOperationException("Prompt delay boundary differs: " + description);
            if (PaytableTestLiveFingerprint(session) != before)
                throw new InvalidOperationException("Prompt delay changed funds/history/RNG: " + description);
            ++promptDelayChecks;
        }

        private IEnumerator PromptDelayClockSmoke()
        {
            yield return null;
            IRouletteSession saved = session;
            string originalFingerprint = PaytableTestLiveFingerprint(saved);
            long savedStartup = promptReadyTimestamp;
            long savedDelayedPlay = promptDelayedPlayTimestamp;
            long savedSettled = resultSettledTimestamp;
            long savedResultDeadline = resultDisplayDeadlineTimestamp;
            long savedFlash = proViewFlashStartedTimestamp;
            OriginalResultDisplayWaitClock savedWait = pcResultDisplayWait;
            bool savedWaitServicing = pcResultDisplayWaitServicing;
            bool savedWaitPromptScheduled = pcResultDisplayWaitPromptScheduled;
            int savedWaitFrame = pcResultDisplayWaitFrame;
            long savedConfiguredStart = configuredResultDelayStartTimestamp;
            long savedConfiguredDeadline = configuredResultDelayDeadlineTimestamp;
            bool savedAutoplay = autoplay, savedRack = winRackActive;
            bool savedGameOver = gameOverMessageLatched;
            promptDelayChecks = 0;
            try
            {
                pcResultDisplayWait = null;
                pcResultDisplayWaitServicing = false;
                pcResultDisplayWaitPromptScheduled = false;
                pcResultDisplayWaitFrame = Time.frameCount;
                foreach (long days in new[] {0L, 180L, 730L})
                    foreach (long frequency in new[] {1000L, 10000000L})
                    {
                        long epoch = days * 24L * 60L * 60L * frequency;
                        string label = "days=" + days + " frequency=" + frequency;
                        autoplay = false; winRackActive = false; gameOverMessageLatched = false;
                        session = new RouletteSession(RouletteProfile.Load("97"),100,
                            new PaytableTestForbiddenOutcome());
                        ResetBetPromptAfterSuccess();
                        ScheduleIdlePromptAt(epoch,frequency);
                        string before = PaytableTestLiveFingerprint(session);
                        foreach (int ms in new[] {0,500,999})
                            AssertPromptDelayAt(epoch + ms * frequency / 1000L, frequency,
                                false, PromptKind.None,0,before,"startup " + ms + " " + label);
                        AssertPromptDelayAt(epoch + frequency - 1, frequency,
                            false,PromptKind.None,0,before,"startup last tick " + label);
                        foreach (int ms in new[] {1000,1041,1042,1043})
                            AssertPromptDelayAt(epoch + ms * frequency / 1000L, frequency,
                                true,PromptKind.PlaceBets,ms < 1042 ? 0 : 1,before,
                                "startup ready " + ms + " " + label);

                        foreach (int pocket in new[] {0,16})
                        {
                            session = new RouletteSession(RouletteProfile.Load("97"),100,
                                new FixedPocketSmokeOutcome(pocket));
                            session.PlaceBet(100,1); session.Spin();
                            if (!session.ResultPhase || session.LastResult == null ||
                                (session.LastResult.Returned > 0) != (pocket == 0))
                                throw new InvalidOperationException("Win/loss result fixture differs");
                            before = PaytableTestLiveFingerprint(session);
                            gameOverMessageLatched = true;
                            resultSettledTimestamp = epoch;
                            autoplay = false; winRackActive = false;
                            ScheduleResultIdlePrompt(frequency);
                            ResetBetPromptAfterSuccess();
                            foreach (int ms in new[] {0,500,999})
                                AssertPromptDelayAt(epoch + ms * frequency / 1000L,frequency,
                                    false,PromptKind.None,0,before,"manual result " + pocket + " " + ms + " " + label);
                            AssertPromptDelayAt(epoch + frequency - 1,frequency,
                                false,PromptKind.None,0,before,"result last tick " + pocket + " " + label);
                            AssertPromptDelayAt(epoch + frequency,frequency,
                                true,PromptKind.GameOver,0,before,"manual result ready " + pocket + " " + label);
                            AssertPromptDelayAt(epoch + frequency + 42*frequency/1000L,frequency,
                                true,PromptKind.GameOver,1,before,"manual result frame 1 " + pocket + " " + label);

                            ResetBetPromptAfterSuccess(); winRackActive = true;
                            AssertPromptDelayAt(epoch + 3*frequency,frequency,
                                false,PromptKind.None,0,before,"rack blocks " + pocket + " " + label);
                            winRackActive = false;
                            AssertPromptDelayAt(epoch + 3*frequency,frequency,
                                true,PromptKind.GameOver,0,before,"rack released " + pocket + " " + label);

                            ResetBetPromptAfterSuccess(); autoplay = true;
                            // This fixture supplies an actual loop-return time
                            // and verifies its following one-second prompt.
                            // Fresh 35ms iterations are covered by the pure
                            // wait suite; this is not a total-wait deadline test.
                            int actualReturnMs = pocket == 0 ? 1437 : 917;
                            ScheduleIdlePromptAt(epoch + actualReturnMs*frequency/1000L, frequency);
                            int readyMs = actualReturnMs+1000;
                            AssertPromptDelayAt(epoch + 999*frequency/1000L,frequency,
                                false,PromptKind.None,0,before,"auto before idle delay " + pocket + " " + label);
                            AssertPromptDelayAt(epoch + readyMs*frequency/1000L - 1,frequency,
                                false,PromptKind.None,0,before,"auto last tick " + pocket + " " + label);
                            AssertPromptDelayAt(epoch + readyMs*frequency/1000L,frequency,
                                true,PromptKind.GameOver,0,before,"auto ready " + pocket + " " + label);
                            autoplay = false;
                        }

                        // Stop(false) cancels timer +0x20, not delayed Play
                        // timer +0x24. At expiry, the manager calls Begin,
                        // but the collection rejects the already playing
                        // same movie: frame 11 must continue, not reset to 0.
                        session = new RouletteSession(RouletteProfile.Load("97"),100,
                            new PaytableTestForbiddenOutcome());
                        gameOverMessageLatched = false; ResetBetPromptAfterSuccess();
                        ScheduleIdlePromptAt(epoch,frequency);
                        RestartSourcePromptAfterCreditChangeAt(epoch + frequency/2);
                        before = PaytableTestLiveFingerprint(session);
                        foreach (int ms in new[] {500,999,1000,1041,1042})
                            AssertPromptDelayAt(epoch + ms*frequency/1000L,frequency,
                                true,PromptKind.PlaceBets,(ms-500)/42,
                                before,"pending timer preserves same Begin " + ms + " " + label);
                        if (promptDelayedPlayTimestamp != 0)
                            throw new InvalidOperationException("Delayed Play timer did not expire once");

                        // A Begin already completed at 714 ms is in Hold:
                        // PlayMessage's state-3 gate must reject the replay.
                        ResetBetPromptAfterSuccess(); ScheduleIdlePromptAt(epoch,frequency);
                        RestartSourcePromptAt(PromptKind.PlaceBets,epoch);
                        int beginFrames = PromptFrames(PromptKind.PlaceBets,false).Length;
                        int beginEndMs = beginFrames*42;
                        if (beginEndMs >= 1000)
                            throw new InvalidOperationException("Hold-timer fixture requires source Begin shorter than 1000 ms");
                        AssertPromptDelayAt(epoch,frequency,true,PromptKind.PlaceBets,0,
                            before,"early restart Begin " + label);
                        AssertPromptDelayAt(epoch + beginEndMs*frequency/1000L,frequency,
                            true,PromptKind.PlaceBets,beginFrames-1,before,"early restart Hold " + label);
                        AssertPromptDelayAt(epoch + frequency,frequency,true,PromptKind.PlaceBets,
                            beginFrames-1,before,"pending timer ignored in Hold " + label);

                        // The collection guard also preserves a rejection Begin.
                        RouletteProfile limited = RouletteProfile.Load("97");
                        limited.MaximumTotalBet = 1;
                        session = new RouletteSession(limited,100,new PaytableTestForbiddenOutcome());
                        session.PlaceBet(100,1); ResetBetPromptAfterSuccess();
                        ScheduleIdlePromptAt(epoch,frequency);
                        QueueBetFailurePrompt(BetRejectionReason.TotalLimit);
                        failurePromptStartedTimestamp = epoch + frequency/2;
                        before = PaytableTestLiveFingerprint(session);
                        foreach (int ms in new[] {500,999,1000,1042})
                            AssertPromptDelayAt(epoch + ms*frequency/1000L,frequency,
                                true,PromptKind.BetLimitReached,(ms-500)/42,
                                before,"pending timer preserves same failure Begin " + ms + " " + label);
                        ResetBetPromptAfterSuccess();
                    }

                // Source named messages and credit notification use delay 0.
                session = new RouletteSession(RouletteProfile.Load("97"),100,
                    new FixedPocketSmokeOutcome(16));
                session.PlaceBet(100,1); session.Spin();
                gameOverMessageLatched = false; autoplay = false; winRackActive = false;
                string unchanged = PaytableTestLiveFingerprint(session);
                long realFrequency = System.Diagnostics.Stopwatch.Frequency;
                ScheduleIdlePromptAt(System.Diagnostics.Stopwatch.GetTimestamp(),realFrequency);
                long beforeRestart = System.Diagnostics.Stopwatch.GetTimestamp();
                RestartSourcePrompt(PromptKind.PlaceBets);
                long afterRestart = System.Diagnostics.Stopwatch.GetTimestamp();
                if (promptReadyTimestamp < beforeRestart || promptReadyTimestamp > afterRestart)
                    throw new InvalidOperationException("Named message retained delayed startup");
                AssertPromptDelayAt(afterRestart,realFrequency,true,PromptKind.PlaceBets,0,
                    unchanged,"named restart delay zero");
                promptReadyTimestamp = OriginalRackClock.AfterMilliseconds(
                    afterRestart,1000,realFrequency);
                beforeRestart = System.Diagnostics.Stopwatch.GetTimestamp();
                RestartSourcePromptAfterCreditChange();
                afterRestart = System.Diagnostics.Stopwatch.GetTimestamp();
                if (promptReadyTimestamp < beforeRestart || promptReadyTimestamp > afterRestart)
                    throw new InvalidOperationException("Credit restart retained delayed startup");
                AssertPromptDelayAt(afterRestart,realFrequency,true,PromptKind.GameOver,0,
                    unchanged,"credit restart delay zero");

                // Exercise the real ToggleAuto callback with an explicit
                // in-flight wait. The current WaitInput must still return;
                // only then can the following one-second prompt be scheduled.
                ResetBetPromptAfterSuccess(); autoplay = true;
                proViewFlashStartedTimestamp = 0;
                long waitStarted = System.Diagnostics.Stopwatch.GetTimestamp();
                var cancelHost = new ResultWaitSmokeHost {
                    Now = waitStarted, Award = session.LastResult.Returned, Auto = true
                };
                pcResultDisplayWait = new OriginalResultDisplayWaitClock(cancelHost.Award,
                    waitStarted, realFrequency, cancelHost);
                pcResultDisplayWaitServicing = false;
                pcResultDisplayWaitPromptScheduled = false;
                pcResultDisplayWaitFrame = Time.frameCount;
                promptReadyTimestamp = long.MaxValue;
                promptDelayedPlayTimestamp = 0;
                long currentWaitDue = pcResultDisplayWait.WaitDueTimestamp;
                long beforeCancel = System.Diagnostics.Stopwatch.GetTimestamp();
                ToggleAuto();
                long afterCancel = System.Diagnostics.Stopwatch.GetTimestamp();
                cancelHost.Auto = autoplay;
                if (autoplay || !ResultDisplayActive || pcResultDisplayWait.Completed ||
                    promptReadyTimestamp != long.MaxValue || pcResultDisplayWaitPromptScheduled ||
                    proViewFlashStartedTimestamp < beforeCancel ||
                    proViewFlashStartedTimestamp > afterCancel)
                    throw new InvalidOperationException("Auto cancellation bypassed its in-flight result wait");
                cancelHost.Now = currentWaitDue - 1;
                if (pcResultDisplayWait.UpdateAt(cancelHost.Now) || pcResultDisplayWait.Completed ||
                    promptReadyTimestamp != long.MaxValue)
                    throw new InvalidOperationException("Cancelled current result wait returned before its own timer");
                cancelHost.Now = currentWaitDue;
                if (!pcResultDisplayWait.UpdateAt(cancelHost.Now) || !pcResultDisplayWait.Completed ||
                    pcResultDisplayWait.TimeoutCount != 1 || pcResultDisplayWait.InputCount != 0 ||
                    pcResultDisplayWait.RemainingWaits != 0 ||
                    pcResultDisplayWait.SkippedWaits != pcResultDisplayWait.InitialWaits - 1)
                    throw new InvalidOperationException("Cancelled result wait did not return once and skip inactive loops");
                PublishPcResultDisplayWait();
                if (!pcResultDisplayWaitPromptScheduled || ResultDisplayActive ||
                    promptReadyTimestamp != OriginalRackClock.AfterMilliseconds(currentWaitDue,1000,realFrequency))
                    throw new InvalidOperationException("Auto cancellation prompt did not start at the actual wait return");
                long cancellationDeadline = promptReadyTimestamp;
                AssertPromptDelayAt(cancellationDeadline-1,realFrequency,false,PromptKind.None,0,
                    unchanged,"auto cancellation last tick");
                AssertPromptDelayAt(cancellationDeadline,realFrequency,true,PromptKind.GameOver,0,
                    unchanged,"auto cancellation ready");

                // The old float comparison releases this one-second delay
                // at 999 ms after 180 days, and loses +1 entirely at 730 days.
                // Compare actual 32-bit stores. A local float expression
                // need not materialize the persisted field's rounding.
                uint oldDeadlineBits = BitConverter.ToUInt32(BitConverter.GetBytes(
                    (float)(180d*24*60*60+1d)),0);
                uint oldNowBits = BitConverter.ToUInt32(BitConverter.GetBytes(
                    (float)(180d*24*60*60+.999d)),0);
                uint oldLongStartBits = BitConverter.ToUInt32(BitConverter.GetBytes(
                    (float)(730d*24*60*60)),0);
                uint oldLongDeadlineBits = BitConverter.ToUInt32(BitConverter.GetBytes(
                    (float)(730d*24*60*60+1d)),0);
                if (oldNowBits < oldDeadlineBits || oldLongDeadlineBits != oldLongStartBits)
                    throw new InvalidOperationException("Old float defect fixtures no longer distinguish the withdrawn gates");
                Debug.Log("ROULETTE_PROMPT_OLD_FLOAT_STORES: 180day deadline=" +
                    oldDeadlineBits.ToString("X8") + " at999ms=" + oldNowBits.ToString("X8") +
                    " 730day start=" + oldLongStartBits.ToString("X8") +
                    " deadline=" + oldLongDeadlineBits.ToString("X8"));
            }
            finally
            {
                session = saved; promptReadyTimestamp = savedStartup;
                promptDelayedPlayTimestamp = savedDelayedPlay;
                resultSettledTimestamp = savedSettled;
                resultDisplayDeadlineTimestamp = savedResultDeadline;
                proViewFlashStartedTimestamp = savedFlash;
                pcResultDisplayWait = savedWait;
                pcResultDisplayWaitServicing = savedWaitServicing;
                pcResultDisplayWaitPromptScheduled = savedWaitPromptScheduled;
                pcResultDisplayWaitFrame = savedWaitFrame;
                configuredResultDelayStartTimestamp = savedConfiguredStart;
                configuredResultDelayDeadlineTimestamp = savedConfiguredDeadline;
                autoplay = savedAutoplay; winRackActive = savedRack;
                gameOverMessageLatched = savedGameOver;
                ResetBetPromptAfterSuccess(); RefreshPortraitUi();
            }
            if (PaytableTestLiveFingerprint(saved) != originalFingerprint)
                throw new InvalidOperationException("Prompt delay diagnostic altered formal session");
            // Keep the epoch/UI matrix and its no-mutation assertion above.
            // The opt-in G variant additionally owns a real deferred round.
            if (DeferredPaymentSmokeEnabled) yield return DeferredPaymentSmokePromptRound();
            Debug.Log("ROULETTE_PROMPT_DELAY_CLOCK_PASSED: " + promptDelayChecks +
                " live UI checks; 0/180/730-day epochs; 1kHz/10MHz; startup/actual-result-return last tick; win/loss/rack/auto/current-wait-cancel; pending Play timer same-Begin guard/Hold/failure; zero-delay restarts; state/RNG isolation");
            Application.Quit();
        }
    }
}
