using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class StateEndOutcome : IRouletteOutcomeSource, IRouletteOutcomeMapping, IDisposable
        {
            private readonly RouletteGame game;
            private readonly IRouletteOutcomeSource platform;
            internal int Pockets, Suits, Pocket = 27, ExpectedIndex;
            internal bool Continuing;
            internal StateEndOutcome(RouletteGame owner, IRouletteOutcomeSource inner)
            { game = owner; platform = inner; }
            // Continuing affects assertions only; Draw still returns Pocket.
            public int ResolvePocket(int raw) { return Pocket; }
            public int ResolveDoubleSuit(int raw) { return 3; }
            public int DrawPocket()
            {
                game.RequireDemoSelectedText("", game.demoShowMode > 0, "StateEnd draw");
                RequireStateEnd(game.demoOutcome.SelectedOption == ExpectedIndex,
                    "accepted draw consumed Auto/Turbo index before StateEnd");
                if (Continuing && Pockets == 1)
                    RequireStateEnd(game.sourceRoundEndCount == 1 && ExpectedIndex == 0 &&
                        game.configuredAutoPlay.AutoPlay == 1 && game.configuredAutoPlay.Turbo &&
                        game.session.Credit == 2998,
                        "first End did not preserve configuration and fund the next committed draw");
                int actual = platform.DrawPocket();
                ++Pockets;
                Debug.Log("ROULETTE_STATE_END_DRAW: pocketCount=" + Pockets + " actual=" + actual +
                    " fixture=" + Pocket + " index=" + game.demoOutcome.SelectedOption +
                    " ends=" + game.sourceRoundEndCount + " configured=" + game.configuredAutoPlay.AutoPlay);
                return Pocket;
            }
            public int DrawDoubleSuit()
            {
                int actual = platform.DrawDoubleSuit();
                ++Suits;
                Debug.Log("ROULETTE_STATE_END_DRAW: suitCount=" + Suits + " actual=" + actual + " fixture=3");
                return 3;
            }
            public void Dispose()
            { var disposable = platform as IDisposable; if (disposable != null) disposable.Dispose(); }
        }

        private StateEndOutcome stateEndOutcome;
        private IRouletteOutcomeSource CreateStateEndLifecycleOutcome(IRouletteOutcomeSource platform)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-rouletteStateEndLifecycleSmoke") < 0 &&
                Array.IndexOf(args, "-rouletteStateEndLifecycleLoadSmoke") < 0) return platform;
            stateEndOutcome = new StateEndOutcome(this, platform);
            return stateEndOutcome;
        }

        [Serializable]
        private sealed class StateEndReceipt
        {
            public string testCase, phase, economy, archive, rng, undo, pending, staged, settledUndo, creditEvent;
            public long credit, wagered, returned, rounds, wins;
            public int archiveCount;
        }
        private string StateEndReceiptKey { get { return recordedHistoryKey + ".StateEndLifecycleFixture.v1"; } }
        private static void RequireStateEnd(bool valid, string message)
        { if (!valid) throw new InvalidOperationException("StateEnd lifecycle: " + message); }

        private IEnumerator StateEndLifecycleSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStateEndLifecycleCase");
            RequireStateEnd(at >= 0 && at + 1 < args.Length, "missing case");
            string testCase = args[at + 1], phase = "run";
            bool load = Array.IndexOf(args, "-rouletteStateEndLifecycleLoadSmoke") >= 0;
            RequireStateEnd(new[] { "loss_pending", "auto_skip", "auto_continue", "double_loss", "double_take", "double_cap" }.Contains(testCase) &&
                demoShowMode == (testCase == "loss_pending" ? 0 : 1) &&
                stateEndOutcome != null && session != null && sourceInputModule != null &&
                !temporaryHistory && recordedHistoryKey.Contains(".Test.") && pcPlayerVolumeKey.Contains(".Test.") &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) &&
                SourcePlayerInputs.Count == 0 && sourceRoundEndCount == 0,
                "requires isolated keys, startup RNG wrapper and unused process End counter");
            bool module = sourceInputModule.enabled;
            try
            {
                sourceInputModule.enabled = false;
                if (load)
                {
                    StateEndReceipt proof = JsonUtility.FromJson<StateEndReceipt>(PlayerPrefs.GetString(StateEndReceiptKey, ""));
                    RequireStateEnd(proof != null && proof.testCase == testCase, "missing receipt");
                    RequireStateEndReceipt(proof);
                    if (proof.phase == "completed")
                    {
                        phase = "completed_reload";
                        RequireStateEnd(sourceRoundPhase == SourceRoundPhase.None && !session.InDoubleUp &&
                            !session.DoubleAvailable && pendingSpin == null, "completed load armed an old round");
                        string before = StartGameDurableFingerprint();
                        yield return StartGameLiveFrames(4);
                        RequireStateEnd(StartGameDurableFingerprint() == before && sourceRoundEndCount == 0 &&
                            stateEndOutcome.Pockets == 0 && stateEndOutcome.Suits == 0,
                            "completed load redrew, repaid or repeated StateEnd");
                    }
                    else if (testCase == "loss_pending")
                    {
                        phase = "load";
                        RequireStateEnd(proof.phase == "pending" && spinning && session.SpinPending &&
                            session.PendingPocket == 16 && sourceRoundPhase == SourceRoundPhase.Wheel,
                            "pending startup failed to retain its round ticket");
                        yield return StateEndWaitForIdle(false);
                        RequireStateEndTotals(998, 2, 0, 2, 0, 1);
                        RequireStateEnd(stateEndOutcome.Pockets == 0 && stateEndOutcome.Suits == 0 &&
                            DemoCleanupRngBytes() == proof.rng && StateBetCleanupUndoFingerprint() == proof.undo &&
                            JsonUtility.ToJson(new RecordedRoundArchive { rounds = recordedRoundPages.Skip(1).ToArray() }) == proof.archive,
                            "pending completion redrew or rewrote the prior archive/Undo");
                        yield return StateEndStableIdle(1);
                        SaveStateEndReceipt(testCase, "completed");
                        CaptureStateEnd("idle");
                    }
                    else
                    {
                        RequireStateEnd(testCase == "double_take", "unsupported load case");
                        if (proof.phase == "offer")
                        {
                            phase = "load_offer_save_feature";
                            RequireStateEnd(sourceRoundPhase == SourceRoundPhase.DoubleOffer &&
                                session.DoubleAvailable && !session.InDoubleUp && session.DoubleStake == 36,
                                "offer load became completed or replayed the award");
                            yield return StateEndHoldOffer();
                            yield return StateEndEnterDouble();
                            yield return StateEndGuess(true);
                            RequireStateEnd(session.InDoubleUp && session.DoubleStake == 72 && session.DoubleRounds == 1 &&
                                session.Credit == 999 && sourceRoundEndCount == 0 && stateEndOutcome.Pockets == 0 &&
                                stateEndOutcome.Suits == 1 && DemoCleanupRngBytes() != proof.rng,
                                "restored offer did not consume exactly one real suit draw");
                            yield return StateEndUntil(() => DoubleDecisionReady && !IsDoubleReveal, "winning feature choice");
                            SaveStateEndReceipt(testCase, "feature");
                            CaptureStateEnd("feature");
                        }
                        else
                        {
                            phase = "load_feature_take";
                            RequireStateEnd(proof.phase == "feature" && sourceRoundPhase == SourceRoundPhase.DoublePlaying &&
                                session.InDoubleUp && session.DoubleStake == 72 && session.DoubleRounds == 1,
                                "active feature was not restored");
                            yield return StateEndUntil(() => DoubleDecisionReady && !IsDoubleReveal, "restored feature ready");
                            RefreshPortraitUi();
                            QueueStartGameButton(uiDoubleTakeWinButton, 221, true);
                            yield return StateEndUntil(() => doubleCollectPending, "actual TAKE Down");
                            RequireStateEnd(session.Credit == 999 && sourceRoundEndCount == 0 &&
                                sourceRoundPhase != SourceRoundPhase.None, "TAKE paid or ended before its 100ms wait");
                            yield return StateEndWaitForIdle(false);
                            RequireStateEndTotals(1071, 1, 72, 1, 1, 1);
                            RequireStateEnd(stateEndOutcome.Pockets == 0 && stateEndOutcome.Suits == 0 &&
                                DemoCleanupRngBytes() == proof.rng && StateBetCleanupUndoFingerprint() == proof.undo,
                                "TAKE redrew RNG or changed Undo");
                            yield return StateEndStableIdle(1);
                            SaveStateEndReceipt(testCase, "completed");
                            CaptureStateEnd("idle");
                        }
                    }
                }
                else
                {
                    RequireStateEnd(session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        recordedRoundPages.Count == 0 && !PlayerPrefs.HasKey(StateEndReceiptKey) &&
                        sourceRoundPhase == SourceRoundPhase.None && !spinning && pendingSpin == null &&
                        configuredAutoPlay.AutoPlay == 0, "first phase requires unused keys");
                    if (testCase == "double_cap")
                    {
                        RequireStateEnd(auNationalStandardsEnabled, "cap requires AU timers");
                        session = new RouletteSession(session.Profile, 1000, demoOutcome, doubleMaximumWin: 80) { DoubleEnabled = true };
                        Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_MAXIMUM: maximum=80");
                    }
                    stateEndOutcome.Pocket = testCase == "loss_pending" || testCase == "auto_continue" ? 16 : 27;
                    yield return StateEndPlaceBet(1);
                    if (testCase == "loss_pending")
                    {
                        phase = "save";
                        int originalDelay = gameDelayMilliseconds;
                        gameDelayMilliseconds = 15000;
                        Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_GAME_DELAY: milliseconds=15000 noRack=true");
                        yield return StateEndPlay();
                        yield return StateEndUntil(() => session.Rounds == 1 &&
                            sourceRoundPhase == SourceRoundPhase.ResultDisplay && !ResultDisplayActive,
                            "no-rack outer9 wait");
                        RequireStateEnd(sourceRoundEndCount == 0 && !OriginalRackClock.GameDelayExpired(
                            gameStartTimestamp, System.Diagnostics.Stopwatch.GetTimestamp(), gameDelayMilliseconds,
                            System.Diagnostics.Stopwatch.Frequency), "no-rack End bypassed its positive original deadline");
                        yield return StateEndUntil(() => ProViewFlashingMessage().StartsWith("Please wait... ",
                            StringComparison.Ordinal), "native outer9 wait text", 2f);
                        string waitText = ProViewFlashingMessage();
                        RequireStateEnd(System.Text.RegularExpressions.Regex.IsMatch(waitText,
                            "^Please wait\\.\\.\\. ([1-9]|1[0-5])s$") && uiGameOver.Value == waitText &&
                            sourceRoundEndCount == 0, "outer9 wait text or visible remaining seconds differ: " + waitText);
                        CaptureStateEnd("wait");
                        yield return StateEndWaitForIdle(false);
                        gameDelayMilliseconds = originalDelay;
                        RequireStateEndTotals(999, 1, 0, 1, 0, 1);
                        yield return StateEndStableIdle(1);
                        yield return StateEndPlay();
                        RequireStateEnd(session.Credit == 998 && session.Rounds == 1 && spinning &&
                            pendingSpin != null && stateEndOutcome.Pockets == 2 && sourceRoundEndCount == 1,
                            "second pending round did not commit once");
                        SaveStateEndReceipt(testCase, "pending");
                    }
                    else if (testCase == "auto_skip") yield return StateEndAutoSkip();
                    else if (testCase == "auto_continue") yield return StateEndAutoContinue();
                    else if (testCase == "double_loss") yield return StateEndDoubleLoss();
                    else
                    {
                        yield return StateEndPlay();
                        yield return StateEndUntil(() => sourceRoundPhase == SourceRoundPhase.DoubleOffer && !winRackActive,
                            "live offer");
                        yield return StateEndHoldOffer();
                        if (testCase == "double_take")
                        {
                            phase = "save_offer";
                            yield return StateEndOfferInputs();
                            SaveStateEndReceipt(testCase, "offer");
                            CaptureStateEnd("offer");
                        }
                        else
                        {
                            yield return StateEndEnterDouble();
                            yield return StateEndGuess(true);
                            RequireStateEnd(!session.InDoubleUp && IsDoubleReveal && doubleRevealMaximum &&
                                session.Credit == 1071 && sourceRoundEndCount == 0 &&
                                doubleRevealUntil - Time.realtimeSinceStartup > 2.5f &&
                                Math.Abs(doubleRevealUntil - doubleExitStartedAt) < .02f &&
                                Math.Abs(doubleExitUntil - doubleExitStartedAt - .01f) < .002f,
                                "cap did not retain the visual terminal phase after model payout");
                            CaptureStateEnd("cap_reveal");
                            yield return StateEndWaitForIdle(false);
                            RequireStateEndTotals(1071, 1, 72, 1, 1, 1);
                            RequireStateEnd(stateEndOutcome.Pockets == 1 && stateEndOutcome.Suits == 1,
                                "cap used extra outcome draws");
                            yield return StateEndStableIdle(1);
                            CaptureStateEnd("idle");
                        }
                    }
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = module;
            }
            Debug.Log((load ? "ROULETTE_STATE_END_LIFECYCLE_LOAD_PASSED: " : "ROULETTE_STATE_END_LIFECYCLE_SAVE_PASSED: ") +
                "case=" + testCase + " phase=" + phase + " credit=" + session.Credit + " rounds=" + session.Rounds +
                " ends=" + sourceRoundEndCount + " pockets=" + stateEndOutcome.Pockets + " suits=" + stateEndOutcome.Suits);
            Application.Quit();
        }

        private IEnumerator StateEndUntil(Func<bool> condition, string boundary, float seconds = 60f)
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            sourceInputModule.enabled = false;
            RequireStateEnd(condition(), "timeout at " + boundary + " phase=" + sourceRoundPhase +
                " ends=" + sourceRoundEndCount + " queue=" + SourcePlayerInputs.Count);
            RefreshPortraitUi();
        }

        private IEnumerator StateEndAutoContinue()
        {
            yield return StartGameChooseDemo(2);
            stateEndOutcome.ExpectedIndex = 2;
            stateEndOutcome.Continuing = true;
            yield return StateEndPlay();
            // This changes only the wrapper expectation. Production Reset at
            // the first real End must supply index zero at the next draw.
            stateEndOutcome.ExpectedIndex = 0;
            yield return StateEndUntil(() => stateEndOutcome.Pockets == 2, "configured continuation after End");
            RequireStateEnd(sourceRoundEndCount == 1 && spinning && session.SpinPending && session.Rounds == 1 &&
                configuredAutoPlay.AutoPlay == 1 && configuredAutoPlay.Turbo && demoOutcome.SelectedOption == 0 &&
                session.Credit == 2998, "normal End canceled config or failed its second draw");
            PcCreditEvent deposit = ReadPcCreditEvent();
            RequireStateEnd(deposit != null && deposit.kind == "deposit" && deposit.before == 999 &&
                deposit.after == 2999 && deposit.phase == 3, "continued Idle did not preserve its exact deposit");
            configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_END_REQUEST: afterPocket=2 true");
            yield return StateEndWaitForIdle(false);
            RequireStateEndTotals(2998, 2, 0, 2, 0, 2);
            RequireStateEnd(stateEndOutcome.Pockets == 2 && stateEndOutcome.Suits == 0 &&
                configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                !configuredAutoPlay.AutoPlayOffWhenGameFinished && !pcGameSoundsMuted && !autoplay,
                "second End did not consume the explicit stop request");
            yield return StateEndStableIdle(2);
            CaptureStateEnd("idle");
        }

        private IEnumerator StateEndAutoSkip()
        {
            yield return StartGameChooseDemo(1);
            stateEndOutcome.ExpectedIndex = 1;
            configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_END_REQUEST: firstRound=true secs=0");
            yield return StateEndPlay();
            yield return StateEndWaitForIdle(false);
            RequireStateEndTotals(1035, 1, 36, 1, 1, 1);
            RequireStateEnd(stateEndOutcome.Pockets == 1 && stateEndOutcome.Suits == 0 &&
                configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.AutoPlayOffWhenGameFinished &&
                !autoplay && !pcGameSoundsMuted, "requested Auto finish differs");
            yield return StateEndStableIdle(1);
            yield return StateEndPlaceBet(20);
            yield return StartGameChooseDemo(2);
            stateEndOutcome.ExpectedIndex = 2;
            pcPlayerTransactionActive = false;
            configuredAutoPlay.SetSecsToAutoCompleteStart(7);
            Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_TRANSACTION: active=false off=false secs=7 timerAbsent=true");
            yield return StateEndPlay();
            yield return StateEndUntil(() => sourceRoundPhase == SourceRoundPhase.ResultDisplay &&
                ResultDisplayActive && !winRackActive && session.Rounds == 2, "actual high-win result wait");
            RequireStateEnd(sourceRoundEndCount == 1 && demoOutcome.SelectedOption == 2 &&
                configuredAutoPlay.AutoPlay == 1 && configuredAutoPlay.Turbo && pcGameSoundsMuted,
                "Turbo finished before the original high-win wait");
            var probe = uiBetButtons[1].gameObject.AddComponent<RouletteStartGameReleaseProbe>();
            bool observed = false;
            probe.ObservedUp = data => {
                observed = demoOutcome.SelectedOption == 0 && demoSelectedText == "" &&
                    demoSelectedTextShown && ActiveDemoSelectedGlyphs().Length == 0 &&
                    configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                    !configuredAutoPlay.AutoPlayOffWhenGameFinished && !autoplay && !pcGameSoundsMuted &&
                    configuredAutoPlay.ReplenishCredits && configuredAutoPlay.DemoOnly &&
                    configuredAutoPlay.PlayerInput && configuredAutoPlay.SecsToAutoCompleteStart == 7 &&
                    sourceRoundEndCount == 1 && session.Credit == 1735 && stateEndOutcome.Pockets == 2;
            };
            try
            {
                Vector2 point = StartGameButtonPoint(uiBetButtons[1]);
                sourceInputModule.QueuePointerSample(225, point, true, false, true, true);
                RequireStateEnd(SourcePlayerInputs.DispatchOne(), "End release probe could not capture its real pointer");
                PointerEventData held = sourceInputModule.QueuePointerSample(225, point, false, true, false, true);
                SourcePlayerInputs.Enqueue(0x200010, () => { throw new InvalidOperationException("End tail was dispatched instead of purged"); });
                RequireStateEnd(SourcePlayerInputs.Count == 2 && sourceRoundEndCount == 1,
                    "End probe did not retain release plus nonrelease tail");
                Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_INPUT_SERVICE_BOUNDARY: actualResultDisplay=true originalDeadline=true");
                float timeout = Time.realtimeSinceStartup + 4f;
                while (ResultDisplayActive && Time.realtimeSinceStartup < timeout) yield return null;
                RequireStateEnd(!ResultDisplayActive && sourceRoundEndCount == 1,
                    "disabled adapter unexpectedly serviced StateEnd or wait did not expire");
                ServiceSourceRoundLifecycle();
                RequireSourcePointerReleased(held);
                RequireStateEnd(probe.UpCount == 1 && probe.LastUpPosition == Vector2.zero && observed &&
                    SourcePlayerInputs.Count == 0 && sourceRoundEndCount == 2 &&
                    configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.ReplenishCredits &&
                    configuredAutoPlay.PlayerInput && !configuredAutoPlay.Turbo &&
                    !configuredAutoPlay.DemoOnly && configuredAutoPlay.TurboOption == 0 &&
                    configuredAutoPlay.SecsToAutoCompleteStart == 7 && configuredAutoPlayCheckPending,
                    "End Reset/Finish/release-purge/tail-reset order differs");
            }
            finally { Destroy(probe); pcPlayerTransactionActive = true; }
            yield return StateEndWaitForIdle(false);
            RequireStateEndTotals(1735, 21, 756, 2, 2, 2);
            RequireStateEnd(stateEndOutcome.Pockets == 2 && stateEndOutcome.Suits == 0 && ReadPcCreditEvent() == null,
                "finish entered Double, deposited or drew a third round");
            yield return StateEndStableIdle(2);
            CaptureStateEnd("idle");
        }

        private IEnumerator StateEndOfferInputs()
        {
            string before = StartGameDurableFingerprint();
            long chip = session.Chip;
            RefreshPortraitUi();
            Button[] forbidden = { uiBetButtons[1], uiChipSelections[1].GetComponent<Button>(),
                uiClearButton, uiUndoButton };
            foreach (Button button in forbidden)
            {
                RequireStateEnd(button != null && !button.interactable, "offer exposes forbidden button " +
                    (button == null ? "null" : button.name));
                if (button.IsActive())
                {
                    QueueStartGameButton(button, 226, true);
                    yield return StartGameDrainWithLiveModule();
                }
            }
            // Show the existing PC iDeck view to exercise its actual Auto and
            // Collect controls using the same captured touch/FIFO adapter.
            SetIDeckPreview(true);
            yield return null;
            RefreshDiagnosticUi();
            foreach (Button button in new[] { ideckAutoPlayButton, ideckCashoutButton })
            {
                RequireStateEnd(button.IsActive() && !button.interactable, "iDeck offer gate is absent: " + button.name);
                QueueStartGameButton(button, 228, true);
                yield return StartGameDrainWithLiveModule();
            }
            SetIDeckPreview(false);
            yield return null;
            RefreshPortraitUi();
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.F8));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return StartGameDrainWithLiveModule();
            RequireStateEnd(!demoMenuVisible && !autoConfirmPending && !collectConfirmPending &&
                session.Chip == chip && StartGameDurableFingerprint() == before &&
                sourceRoundEndCount == 0 && sourceRoundPhase == SourceRoundPhase.DoubleOffer,
                "forbidden offer input mutated money, configuration or lifecycle");
            yield return ConfiguredOrderUiClick(uiInfoButton);
            RequireStateEnd(gameInfoOptionsOpen, "offer INFO disabled");
            yield return ConfiguredOrderUiClick(uiGameInfoSeePays);
            RequireStateEnd(paysPage == 0 && sourceRoundEndCount == 0, "offer SEE PAYS did not open");
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.D));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return StartGameDrainWithLiveModule();
            RequireStateEnd(paysPage == 0 && !session.InDoubleUp && session.DoubleAvailable &&
                sourceRoundPhase == SourceRoundPhase.DoubleOffer && sourceRoundEndCount == 0 &&
                StartGameDurableFingerprint() == before,
                "hardware Double escaped the Pays offer gate");
            SetIDeckPreview(true);
            yield return null;
            RefreshDiagnosticUi();
            RequireStateEnd(ideckOfferTakeWinButton.IsActive() && ideckOfferDoubleUpButton.IsActive() &&
                !ideckOfferTakeWinButton.interactable && !ideckOfferDoubleUpButton.interactable,
                "Pays left TAKE or DOUBLE offer UI enabled");
            QueueStartGameButton(ideckOfferTakeWinButton, 230, true);
            yield return StartGameDrainWithLiveModule();
            RequireStateEnd(paysPage == 0 && !session.InDoubleUp && session.DoubleAvailable &&
                sourceRoundPhase == SourceRoundPhase.DoubleOffer && sourceRoundEndCount == 0 &&
                StartGameDurableFingerprint() == before, "Pays TAKE touch dismissed or mutated the offer");
            SetIDeckPreview(false);
            yield return null;
            RefreshPortraitUi();
            int tailDispatches = 0;
            var playProbe = uiSpinButton.gameObject.AddComponent<RouletteStartGameReleaseProbe>();
            PointerEventData paysPlay = QueueStartGameButton(uiSpinButton, 229, true);
            SourcePlayerInputs.Enqueue(0x200010, () => { ++tailDispatches; DispatchSourceButton(KeyCode.Space); });
            try
            {
                yield return StartGameDrainWithLiveModule();
                RequireSourcePointerReleased(paysPlay);
                RequireStateEnd(paysPage < 0 && tailDispatches == 0 && SourcePlayerInputs.Count == 0 &&
                    playProbe.UpCount == 1 && playProbe.LastUpPosition == Vector2.zero &&
                    session.DoubleAvailable && sourceRoundPhase == SourceRoundPhase.DoubleOffer &&
                    sourceRoundEndCount == 0 && StartGameDurableFingerprint() == before,
                    "PLAY in Pays failed to close and purge without declining its offer");
            }
            finally { Destroy(playProbe); }
            yield return ConfiguredOrderUiClick(uiInfoButton);
            yield return ConfiguredOrderUiClick(uiGameInfoSeePays);
            RequireStateEnd(paysPage == 0, "offer Pays did not reopen for its EXIT control");
            yield return ConfiguredOrderUiClick(uiPaysExit);
            yield return StartGameLiveFrames(2);
            RequireStateEnd(paysPage < 0 && session.DoubleAvailable && !winRackActive &&
                sourceRoundPhase == SourceRoundPhase.DoubleOffer && sourceRoundEndCount == 0 &&
                StartGameDurableFingerprint() == before && demoMenuState == 2,
                "INFO/Pays return reracked, repaid or completed the waiting offer");
        }

        private IEnumerator StateEndDoubleLoss()
        {
            yield return StateEndPlay();
            yield return StateEndUntil(() => winRackActive && SourceDoubleOfferActive, "offer during base rack");
            int originalDelay = gameDelayMilliseconds;
            gameDelayMilliseconds = (int)OriginalRackClock.ElapsedMillisecondsLong(gameStartTimestamp,
                System.Diagnostics.Stopwatch.GetTimestamp(), System.Diagnostics.Stopwatch.Frequency) + 200;
            Debug.Log("ROULETTE_STATE_END_CALL_LEVEL_RACK_DELAY: remaining=200ms");
            try
            {
                SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.D));
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
                yield return StartGameLiveFrames(1);
                RequireStateEnd(SourcePlayerInputs.Count == 2 && !session.InDoubleUp && winRackActive &&
                    sourceRoundEndCount == 0 && session.Credit == (PcPaymentTracked ? 999 : 1035) &&
                    (!PcPaymentTracked || !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished),
                    "predeadline rack dispatched the queued Double decision");
                yield return StateEndUntil(() => session.InDoubleUp, "rack D snap plus entry");
                RequireStateEnd(winRackSnapped && !winRackActive && winRackPaid == 36 &&
                    session.Credit == 999 && session.DoubleStake == 36 && stateEndOutcome.Suits == 0 &&
                    stateEndOutcome.Pockets == 1 && sourceRoundEndCount == 0 && SourcePlayerInputs.Count == 0,
                    "same rack event did not publish full paid and enter Double once");
            }
            finally { gameDelayMilliseconds = originalDelay; }
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return StartGameDrainWithLiveModule();
            RequireStateEnd(stateEndOutcome.Suits == 0 && stateEndOutcome.Pockets == 1 && session.Credit == 999,
                "late D release replayed the feature");
            yield return StateEndGuess(true);
            RequireStateEnd(session.InDoubleUp && session.DoubleStake == 72 && session.Credit == 999 &&
                sourceRoundEndCount == 0, "winning feature step ended the round");
            yield return StateEndGuess(false);
            RequireStateEnd(!session.InDoubleUp && session.Credit == 999 && session.Returned == 0 &&
                sourceRoundEndCount == 0 && IsDoubleReveal, "losing model completion was treated as StateEnd");
            yield return StateEndWaitForIdle(false);
            RequireStateEndTotals(999, 1, 0, 1, 1, 1);
            yield return StateEndStableIdle(1);
            yield return StateEndPlay();
            yield return StateEndUntil(() => sourceRoundPhase == SourceRoundPhase.DoubleOffer && !winRackActive,
                "second base offer");
            RequireStateEnd(session.Credit == 1034 && session.Rounds == 2 && session.Returned == 36 &&
                stateEndOutcome.Pockets == 2 && stateEndOutcome.Suits == 2 && sourceRoundEndCount == 1,
                "second real base win differs");
            RefreshPortraitUi();
            PointerEventData play = QueueStartGameButton(uiSpinButton, 227, true);
            yield return StateEndWaitForIdle(false);
            RequireSourcePointerReleased(play);
            RequireStateEndTotals(1034, 2, 36, 2, 2, 2);
            RequireStateEnd(stateEndOutcome.Pockets == 2 && pendingSpin == null, "offer PLAY committed a new bet");
            string declined = StartGameDurableFingerprint();
            PointerEventData laterUp = sourceInputModule.QueuePointerSample(227, StartGameButtonPoint(uiSpinButton),
                false, true, false, true);
            yield return StartGameDrainWithLiveModule();
            RequireSourcePointerReleased(laterUp);
            yield return StartGameLiveFrames(2);
            RequireStateEnd(StartGameDurableFingerprint() == declined && sourceRoundEndCount == 2 &&
                stateEndOutcome.Pockets == 2, "purged or later physical PLAY Up committed a new round");
            yield return StartGameChooseDemo(19);
            yield return StartGameLiveFrames(2);
            RequireDemoSelectedText("16", true, "choice after prior End");
            RequireStateEnd(demoOutcome.SelectedOption == 19 && demoOutcome.ArmedVisualPath == 16 &&
                sourceRoundEndCount == 2, "old End reset the independent next choice");
            stateEndOutcome.ExpectedIndex = 19;
            yield return StateEndPlay();
            RequireStateEnd(session.PendingPocket == 16 && demoOutcome.SelectedOption == 0,
                "second independent PLAY failed to consume the next fixed pocket");
            yield return StateEndWaitForIdle(false);
            RequireStateEndTotals(1033, 3, 36, 3, 2, 3);
            RequireStateEnd(stateEndOutcome.Pockets == 3 && stateEndOutcome.Suits == 2, "two-gesture flow drew extra outcomes");
            yield return StateEndStableIdle(3);
            CaptureStateEnd("idle");
        }

        // Explicitly completing a cleanup fixture may decline its offer.
        // Feature lifecycle fixtures pass false and must make their own choice.
        private IEnumerator StateEndWaitForIdle(bool declineOffer)
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 60f;
            bool sent = false;
            do
            {
                if (declineOffer && !sent && sourceRoundPhase == SourceRoundPhase.DoubleOffer && !winRackActive)
                {
                    RefreshPortraitUi();
                    RequireStateEnd(uiSpinButton.interactable, "completed fixture offer PLAY unavailable");
                    QueueStartGameButton(uiSpinButton, 220, true);
                    sent = true;
                }
                yield return null;
            }
            while ((sourceRoundPhase != SourceRoundPhase.None || spinning || winRackActive || ResultDisplayActive ||
                configuredAutoPlayCheckPending || configuredAutoPlayFinishPending ||
                sourceGameplayState != SourceGameplayState.Idle || SourcePlayerInputs.Count != 0) &&
                Time.realtimeSinceStartup < deadline);
            sourceInputModule.enabled = false;
            RequireStateEnd(sourceRoundPhase == SourceRoundPhase.None && !spinning && !winRackActive &&
                !ResultDisplayActive && !configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending &&
                sourceGameplayState == SourceGameplayState.Idle && SourcePlayerInputs.Count == 0,
                "actual round did not enter Idle; phase=" + sourceRoundPhase);
            RefreshPortraitUi();
        }

        private IEnumerator StateEndPlaceBet(int amount)
        {
            RefreshPortraitUi();
            if (session.TotalBet > 0) { DemoCleanupClick(uiClearButton); RefreshPortraitUi(); }
            DemoCleanupClick(uiChipSelections[amount == 20 ? 3 : 0].GetComponent<Button>());
            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[27]);
            if (sourceBetStateEntryPending) yield return StartGameWaitForBetEntry();
            if (amount == 20) { RefreshPortraitUi(); DemoCleanupClick(uiBetButtons[27]); }
            RequireStateEnd(session.TotalBet == amount && session.Bets.Count == 1 && session.Bets[127] == amount,
                "real wager fixture differs");
        }

        private IEnumerator StateEndPlay()
        {
            RefreshPortraitUi();
            long rounds = session.Rounds;
            int pockets = stateEndOutcome.Pockets;
            RequireStateEnd(SourcePlayInputAllowed && uiSpinButton.interactable, "real PLAY is gated");
            QueueStartGameButton(uiSpinButton, 222, true);
            yield return StateEndUntil(() => spinning && session.SpinPending, "actual PLAY commitment");
            RequireStateEnd(pendingSpin != null && session.Rounds == rounds && stateEndOutcome.Pockets == pockets + 1 &&
                sourceRoundPhase == SourceRoundPhase.Wheel && SourcePlayerInputs.Count == 0,
                "PLAY did not commit one pending wheel round");
        }

        private IEnumerator StateEndHoldOffer()
        {
            RequireStateEnd(session.DoubleAvailable && !session.InDoubleUp &&
                sourceRoundPhase == SourceRoundPhase.DoubleOffer && sourceRoundEndCount == 0,
                "missing unfinished offer");
            string before = StartGameDurableFingerprint();
            yield return StartGameLiveFrames(4);
            RequireStateEnd(StartGameDurableFingerprint() == before && sourceRoundEndCount == 0 &&
                sourceGameplayState == SourceGameplayState.Playing && !sourceTableInputEnabled &&
                demoMenuState == 2 && !configuredAutoPlayCheckPending &&
                recordedRoundPages[0].awaitingFinalPayout, "offer incorrectly entered End/Idle");
        }

        private IEnumerator StateEndEnterDouble()
        {
            RequireStateEnd(SourceDoubleOfferActive, "D needs a live offer");
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.D));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            yield return StateEndUntil(() => session.InDoubleUp && SourcePlayerInputs.Count == 0, "actual Double entry");
            RequireStateEnd(session.Credit == 999 && session.DoubleStake == 36 &&
                sourceRoundPhase == SourceRoundPhase.DoublePlaying && sourceRoundEndCount == 0 && stateEndOutcome.Suits == 0,
                "Double entry redrew, paid or ended the base round");
            yield return StateEndUntil(() => DoubleDecisionReady && !IsDoubleReveal, "Double decision ready");
        }

        private IEnumerator StateEndGuess(bool red)
        {
            yield return StateEndUntil(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                "guess available");
            int suits = stateEndOutcome.Suits;
            QueueStartGameButton(red ? uiDoubleRedButton : uiDoubleBlackButton, red ? 223 : 224, true);
            yield return StateEndUntil(() => stateEndOutcome.Suits == suits + 1, "actual card guess");
            RequireStateEnd(IsDoubleReveal && sourceRoundEndCount == 0, "guess ended before reveal/exit");
        }

        private IEnumerator StateEndStableIdle(int ends)
        {
            string before = StartGameDurableFingerprint();
            yield return StartGameLiveFrames(4);
            RequireStateEnd(StartGameDurableFingerprint() == before && sourceRoundEndCount == ends &&
                sourceRoundPhase == SourceRoundPhase.None && !session.DoubleAvailable && !session.InDoubleUp,
                "later Idle replayed a completed round");
            RequireStateEnd(!ProViewFlashingMessage().StartsWith("Please wait... ", StringComparison.Ordinal),
                "completed Idle retained the prior GameDelay message");
            RequireDemoSelectedText("", demoShowMode > 0, "completed round");
            RequireStateEnd(demoOutcome.SelectedOption == 0, "End did not reset selection");
        }

        private void RequireStateEndTotals(long credit, long wagered, long returned, long rounds, long wins, int ends)
        {
            RequireStateEnd(session.Credit == credit && session.Wagered == wagered && session.Returned == returned &&
                session.Rounds == rounds && session.Wins == wins && sourceRoundEndCount == ends &&
                recordedRoundPages.Count == rounds && recordedRoundPages.All(round => !round.awaitingFinalPayout) &&
                pendingSpin == null && !session.SpinPending,
                "totals differ: credit=" + session.Credit + " wagered=" + session.Wagered + " returned=" + session.Returned +
                " rounds=" + session.Rounds + " wins=" + session.Wins + " ends=" + sourceRoundEndCount);
        }

        private void SaveStateEndReceipt(string testCase, string phase)
        {
            var proof = new StateEndReceipt { testCase = testCase, phase = phase,
                economy = StartGameRestoredEconomy(), archive = DemoCleanupArchive(false), rng = DemoCleanupRngBytes(),
                undo = StateBetCleanupUndoFingerprint(), pending = PlayerPrefs.GetString(PendingSpinKey, ""),
                staged = PlayerPrefs.GetString(StagedBetKey, ""), settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, ""),
                creditEvent = StartGameCreditEventFingerprint(), credit = session.Credit, wagered = session.Wagered,
                returned = session.Returned, rounds = session.Rounds, wins = session.Wins, archiveCount = recordedRoundPages.Count };
            PlayerPrefs.SetString(StateEndReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
        }

        private void RequireStateEndReceipt(StateEndReceipt proof)
        {
            var differences = new List<string>();
            StartGameReceiptDifference(differences, "economy", proof.economy, DemoCleanupEconomy());
            StartGameReceiptDifference(differences, "archive", proof.archive, DemoCleanupArchive(false));
            StartGameReceiptDifference(differences, "RNG", proof.rng, DemoCleanupRngBytes());
            StartGameReceiptDifference(differences, "Undo", proof.undo, StateBetCleanupUndoFingerprint());
            string pending = PlayerPrefs.GetString(PendingSpinKey, "");
            if (!string.IsNullOrEmpty(proof.pending) && !string.IsNullOrEmpty(pending))
                StartGameReceiptDifference(differences, "pending", StateBetPendingCommitFingerprint(proof.pending),
                    StateBetPendingCommitFingerprint(pending));
            else StartGameReceiptDifference(differences, "pending", proof.pending, pending);
            StartGameReceiptDifference(differences, "staged", proof.staged, PlayerPrefs.GetString(StagedBetKey, ""));
            StartGameReceiptDifference(differences, "settledUndo", proof.settledUndo, PlayerPrefs.GetString(SettledTableUndoKey, ""));
            StartGameReceiptDifference(differences, "creditEvent", proof.creditEvent, StartGameCreditEventFingerprint());
            RequireStateEnd(differences.Count == 0 && stateEndOutcome.Pockets == 0 && stateEndOutcome.Suits == 0,
                "startup receipt differs: " + string.Join("; ", differences.ToArray()));
        }

        private void CaptureStateEnd(string suffix)
        {
            RefreshPortraitUi();
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStateEndLifecycleCapturePrefix");
            RequireStateEnd(at >= 0 && at + 1 < args.Length, "capture prefix missing");
            string path = args[at + 1] + "_" + suffix + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            CaptureActiveScreenOffscreen(path);
        }
    }
}
