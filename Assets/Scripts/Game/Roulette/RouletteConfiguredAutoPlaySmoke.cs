using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class ConfiguredSmokeOutcome : IRouletteOutcomeSource
        {
            private readonly int pocket;
            public int PocketRequests;
            public int SuitRequests;
            public ConfiguredSmokeOutcome(int value) { pocket = value; }
            public int DrawPocket() { ++PocketRequests; return pocket; }
            public int DrawDoubleSuit()
            {
                ++SuitRequests;
                throw new InvalidOperationException("Configured Auto diagnostic reached feature RNG");
            }
        }

        private sealed class ConfiguredReceiptSmokeOutcome : IRouletteOutcomeSource
        {
            private readonly IRouletteOutcomeSource source;
            public int PocketRequests;
            public int SuitRequests;
            public int LastRawPocket = -1;
            public ConfiguredReceiptSmokeOutcome(IRouletteOutcomeSource value)
            {
                if (value == null) throw new ArgumentNullException("value");
                source = value;
            }
            public int DrawPocket()
            {
                ++PocketRequests;
                LastRawPocket = source.DrawPocket();
                return LastRawPocket;
            }
            public int DrawDoubleSuit()
            {
                ++SuitRequests;
                return source.DrawDoubleSuit();
            }
        }

        private static void RequireConfiguredSmoke(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Configured Auto Player: " + message);
        }

        private static bool SameConfiguredSmokeData(OriginalAutoPlayData left,
            OriginalAutoPlayData right)
        {
            return left.AutoPlay == right.AutoPlay &&
                left.ReplenishCredits == right.ReplenishCredits &&
                left.PlayerInput == right.PlayerInput && left.Turbo == right.Turbo &&
                left.DemoOnly == right.DemoOnly && left.TurboOption == right.TurboOption &&
                left.AutoPlayOffWhenGameFinished == right.AutoPlayOffWhenGameFinished &&
                left.SecsToAutoCompleteStart == right.SecsToAutoCompleteStart;
        }

        private PcRngDrawReceipt RequireConfiguredSmokeReceipt(
            ConfiguredReceiptSmokeOutcome counted, string operationId)
        {
            PcRngStoredState stored = PcRngStateStore.LoadStrict(PcRngStateStore.PathForCurrentProcess);
            PcRngDrawReceipt live = pcOutcome.Receipt;
            RequireConfiguredSmoke(stored != null && stored.Version == 3 && stored.Receipt != null &&
                live != null && live.context.kind == 1 && live.context.scope == recordedHistoryKey &&
                live.context.operationId == operationId && live.effectiveResult == 27 &&
                live.rawResult >= 0 && live.rawResult <= 36 && live.rawResult == counted.LastRawPocket &&
                counted.PocketRequests == 1 && counted.SuitRequests == 0 &&
                JsonUtility.ToJson(live) == JsonUtility.ToJson(stored.Receipt),
                "one real pocket draw must retain its matching durable receipt and effective Demo pocket 27");
            return live;
        }

        // Each invocation owns an intentionally persistent .Test. transaction.
        // Default: inactive configured Turbo wins 36 and skips the rack.
        // Add -rouletteConfiguredAutoPlayFundingCase for a losing pocket-27
        // round, then a queued-input cancellation after the 2000-credit deposit.
        // Add -rouletteConfiguredAutoPlayHighWinCase for a 20-credit straight
        // paying 720, with the separate configured-only 500 ms result delay.
        // FundStopEntryCase exercises active fund-stop to configured state 1
        // through the live module, depositing 2000 without starting a round.
        // Use distinct history/RNG/volume test keys; omit -rouletteFreshHistory.
        private IEnumerator ConfiguredAutoPlaySmoke()
        {
            yield return null;
            bool funding = Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteConfiguredAutoPlayFundingCase") >= 0;
            bool highWin = Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteConfiguredAutoPlayHighWinCase") >= 0;
            bool fundStopEntry = Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteConfiguredAutoPlayFundStopEntryCase") >= 0;
            RequireConfiguredSmoke((funding ? 1 : 0) + (highWin ? 1 : 0) +
                (fundStopEntry ? 1 : 0) <= 1, "choose one configured Auto case");
            RequireConfiguredSmoke(Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteVisualVariant") < 0, "omit visual override for demo math/path proof");
            string caseName = funding ? "funding" : highWin ? "highwin" : "turbo";
            long wager = highWin ? 20 : 1;
            long expectedReturn = funding ? 0 : highWin ? 720 : 36;
            long expectedCredit = 1000 - wager + expectedReturn;
            RequireConfiguredSmoke(demoShowMode == 1 && demoMenuRoot != null &&
                demoOutcome != null && sourceInputModule != null && session != null &&
                !autoplay && autoRoutine == null && !spinning && !winRackActive &&
                !session.SpinPending && !session.InDoubleUp && !ResultDisplayActive &&
                !pcCollectPending && !temporaryHistory && session.Credit == 1000 &&
                session.TotalBet == 0 && session.Rounds == 0 && session.Denomination == 1 &&
                recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                ReadPcCreditEvent() == null && pendingSpin == null &&
                SourcePlayerInputs.Count == 0 && pcOutcome != null && pcOutcome.Receipt == null &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires unused persistent .Test. history/volume, isolated RNG and idle 1000 credits");

            IRouletteSession savedSession = session;
            IRouletteOutcomeSource savedOutcome = outcome;
            DemoPocketOutcomeSource savedDemo = demoOutcome;
            OriginalAutoPlayData savedConfiguration = configuredAutoPlay;
            bool savedModuleEnabled = sourceInputModule.enabled;
            bool savedMuted = pcGameSoundsMuted;
            bool savedTransaction = pcPlayerTransactionActive;
            bool savedTableInput = sourceTableInputEnabled;
            bool savedFinishPending = configuredAutoPlayFinishPending;
            int savedVolume = pcPlayerVolumeValue;
            bool savedVolumeExists = PlayerPrefs.HasKey(pcPlayerVolumeKey);
            int savedVolumePreference = PlayerPrefs.GetInt(pcPlayerVolumeKey, 0);
            long savedVolumeTimeout = pcPlayerVolumeIdleDeadlineTimestamp;
            string savedSessionBefore = PaytableTestLiveFingerprint(savedSession);
            int checks = 0;
            bool persistentFixtureStarted = false;
            // Count the actual production source. Its prepared draw and writer
            // own the receipt; Demo performs the ordinary effective mapping.
            var fixedSource = new ConfiguredReceiptSmokeOutcome(pcOutcome);
            var fixtureDemo = new DemoPocketOutcomeSource(fixedSource);
            try
            {
                // Disable only automatic input/service for deterministic boundary
                // placement. All menu and PLAY actions below still use real
                // uGUI raycasts, dispatched through the production source FIFO.
                sourceInputModule.enabled = false;
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayCheckPending = false;
                configuredAutoPlayFinishPending = false;
                pcPlayerTransactionActive = true;
                sourceTableInputEnabled = true;
                demoOutcome = fixtureDemo;
                outcome = fixtureDemo;
                session = new RouletteSession(savedSession.Profile, 1000, fixtureDemo) {
                    DoubleEnabled = true };
                EnableDeferredPaymentSmokeFixture();
                pcPlayerVolumeValue = 5;
                PlayerPrefs.SetInt(pcPlayerVolumeKey, 5);
                SetPcGameSoundsMuted(false);
                string noRoundBefore = PaytableTestLiveFingerprint(session);

                yield return ConfiguredSmokeChooseDemo(1);
                RequireConfiguredSmoke(configuredAutoPlay.AutoPlay == 1 &&
                    configuredAutoPlay.ReplenishCredits && configuredAutoPlay.PlayerInput &&
                    configuredAutoPlay.DemoOnly && !configuredAutoPlay.Turbo &&
                    !autoplay && autoRoutine == null && !pcGameSoundsMuted &&
                    session.Credit == 1000 && !configuredAutoPlayCheckPending,
                    "Auto menu must configure without toggling active Auto or funding");
                ++checks;

                yield return ConfiguredSmokeChooseDemo(30); // page 3, pocket 27
                RequireConfiguredSmoke(fixtureDemo.SelectedPocket == 27 &&
                    fixtureDemo.SelectedOption == 30 && fixtureDemo.ArmedVisualPath == 27,
                    "actual page-three fixed option did not queue pocket 27");
                ++checks;
                yield return ConfiguredSmokeChooseDemo(2);
                RequireConfiguredSmoke(configuredAutoPlay.TurboAutoPlay && !autoplay &&
                    autoRoutine == null && pcGameSoundsMuted && AudioListener.volume == 0f &&
                    fixtureDemo.SelectedPocket == -1 && fixtureDemo.SelectedOption == 2 &&
                    fixtureDemo.ArmedVisualPath == 27 && session.Credit == 1000,
                    "Turbo menu must replace mathematical selection, retain path and configure without an active toggle");
                ++checks;
                OriginalAutoPlayData turboBeforeNormal = configuredAutoPlay.Clone();
                yield return ConfiguredSmokeChooseDemo(0);
                RequireConfiguredSmoke(SameConfiguredSmokeData(configuredAutoPlay, turboBeforeNormal) &&
                    pcGameSoundsMuted && AudioListener.volume == 0f &&
                    fixtureDemo.SelectedPocket == -1 && fixtureDemo.SelectedOption == 0 &&
                    fixtureDemo.ArmedVisualPath == 27 && !autoplay,
                    "Normal menu must preserve configured Turbo/mute/path while replacing mathematical selection");
                ++checks;

                ToggleAuto();
                RequireConfiguredSmoke(autoplay && configuredAutoPlay.TurboAutoPlay,
                    "active toggle fixture did not enable active AutoPlay");
                ToggleAuto();
                if (autoRoutine != null) StopCoroutine(autoRoutine);
                autoRoutine = null;
                RequireConfiguredSmoke(!autoplay &&
                    SameConfiguredSmokeData(configuredAutoPlay, turboBeforeNormal) && pcGameSoundsMuted,
                    "active AutoPlay OFF must retain configured Turbo and mute");
                ++checks;

                int releaseCleanup = 0, releaseDispatch = 0;
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease,
                    () => ++releaseDispatch, () => ++releaseCleanup);
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease,
                    () => ++releaseDispatch, () => ++releaseCleanup);
                RequireConfiguredSmoke(PurgeSourcePlayerInputs() && releaseCleanup == 2 &&
                    releaseDispatch == 0 && SourcePlayerInputs.Count == 0 &&
                    SameConfiguredSmokeData(configuredAutoPlay, turboBeforeNormal) && pcGameSoundsMuted,
                    "release-only queue must clean capture without resetting configured AutoData");
                ++checks;
                checks += ConfiguredSmokeHighWinDelay();
                RequireConfiguredSmoke(PaytableTestLiveFingerprint(session) == noRoundBefore &&
                    fixedSource.PocketRequests == 0 && fixedSource.SuitRequests == 0 &&
                    PaytableTestLiveFingerprint(savedSession) == savedSessionBefore,
                    "menu, queue and timing fixtures changed funds/history or requested an outcome");
                ++checks;

                if (fundStopEntry)
                {
                    // The fixture owns its persisted deposit even though no
                    // round is started. Keep that matching session on exit.
                    persistentFixtureStarted = true;
                    yield return ConfiguredSmokeFundStopEntry(fixedSource);
                    Debug.Log("ROULETTE_CONFIGURED_AUTOPLAY_PASSED: case=fundstopentry " +
                        "menu=actual-uGUI activeFundStopToCase1=automatic " +
                        "funded=2000 credit=3000 rounds=0 draws=0 tableLocked=true " +
                        "endRequestRetained=true");
                }
                else
                {
                // The Normal replacement/retained-path contract was checked
                // above without a draw. Re-select through the actual menu so
                // this round has a deterministic effective result while its
                // underlying raw draw and durable receipt remain real.
                yield return ConfiguredSmokeChooseDemo(30);
                RequireConfiguredSmoke(fixtureDemo.SelectedOption == 30 &&
                    fixtureDemo.SelectedPocket == 27 && fixtureDemo.ArmedVisualPath == 27 &&
                    SameConfiguredSmokeData(configuredAutoPlay, turboBeforeNormal) &&
                    configuredAutoPlay.TurboAutoPlay && !autoplay && autoRoutine == null &&
                    pcGameSoundsMuted && AudioListener.volume == 0f &&
                    fixedSource.PocketRequests == 0 && fixedSource.SuitRequests == 0 &&
                    pcOutcome.Receipt == null && PcReceiptEnabled &&
                    !session.Profile.Records[100].Covers(27) &&
                    session.Profile.Records[127].Covers(27) &&
                    session.Profile.Records[127].ReturnMultiple == 36,
                    "fixed Demo selection must preserve configuration, mute and inactive Auto without drawing");
                ++checks;
                session.PlaceBet(funding ? 100 : 127, wager);
                if (!funding && !highWin)
                {
                    configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                    Debug.Log("ROULETTE_CONFIGURED_AUTO_CALL_LEVEL_END_REQUEST: case=turbo true");
                }
                SaveStagedBetsIfChanged();
                RefreshPortraitUi();
                yield return null;
                PointerEventData playHeld = null;
                SourcePlayerInputs.Enqueue(0x200001,
                    () => playHeld = PressThroughUiRaycast(uiSpinButton));
                RequireConfiguredSmoke(SourcePlayerInputs.DispatchOne(), "manual PLAY queue was empty");
                persistentFixtureStarted = spinning;
                RequireConfiguredSmoke(spinning && !autoplay &&
                    configuredAutoPlay.TurboAutoPlay && wheelTurboSnapRequested &&
                    wheelStage == WheelStage.EaseOut && fixedSource.PocketRequests == 1 &&
                    wheelVariant == 27 && fixtureDemo.ArmedVisualPath == -1 &&
                    fixedSource.SuitRequests == 0 && session.Credit == 1000 - wager &&
                    !sourceTableInputEnabled,
                    "manual configured Turbo must start and snap while active Auto remains false");
                ++checks;
                RequireConfiguredSmoke(pendingSpin != null,
                    "configured real draw did not save its pending spin");
                PcRngDrawReceipt committedReceipt = RequireConfiguredSmokeReceipt(fixedSource, pendingSpin.id);
                string committedReceiptJson = JsonUtility.ToJson(committedReceipt);
                ++checks;
                ReleaseUiPointer(uiSpinButton, playHeld);
                bool sawRack = false;
                long timeout = System.Diagnostics.Stopwatch.GetTimestamp() +
                    30L * System.Diagnostics.Stopwatch.Frequency;
                while (spinning && System.Diagnostics.Stopwatch.GetTimestamp() < timeout)
                {
                    sawRack |= winRackActive;
                    yield return null;
                }
                RequireConfiguredSmoke(!spinning && session.Rounds == 1 &&
                    session.LastResult != null && session.LastResult.Pocket == 27 &&
                    session.LastResult.Returned == expectedReturn &&
                    session.Credit == (DeferredPaymentSmokeEnabled ? 1000 - wager : expectedCredit) && !autoplay &&
                    configuredAutoPlay.TurboAutoPlay == (DeferredPaymentSmokeEnabled || funding || highWin) &&
                    !winRackActive && !sawRack &&
                    wheelStage == WheelStage.Done && resultCycleStartedTimestamp == 0 &&
                    winnerDeadlineTimestamp == 0 && configuredAutoPlayCheckPending ==
                        (!DeferredPaymentSmokeEnabled && !highWin) &&
                    sourceRoundEndCount == (DeferredPaymentSmokeEnabled || highWin ? 0 : 1) &&
                    fixedSource.PocketRequests == 1 && fixedSource.SuitRequests == 0 &&
                    recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                    !session.DoubleAvailable && recordedRoundPages[0].awaitingFinalPayout == DeferredPaymentSmokeEnabled &&
                    pendingSpin == null && !sourceTableInputEnabled,
                    "configured-only Turbo did not settle once with rack/cycle skipped");
                ++checks;
                ObserveDeferredPaymentSmokeUnpaid(1000 - wager, expectedReturn, wager, highWin);
                if (highWin)
                {
                    long frequency = System.Diagnostics.Stopwatch.Frequency;
                    long deadline = resultSettledTimestamp + frequency / 2;
                    RequireConfiguredSmoke(configuredResultDelayStartTimestamp == resultSettledTimestamp &&
                        configuredResultDelayDeadlineTimestamp == deadline &&
                        pcResultDisplayWait != null &&
                        pcResultDisplayWait.Phase == OriginalResultDisplayWaitPhase.HighWinDelay &&
                        pcResultDisplayWait.TimeoutCount == 0 && pcResultDisplayWait.InputCount == 0 &&
                        ResultDisplayActive,
                        "actual configured-only 720 return must wait 500 ms without an active-Auto base wait");
                    ++checks;
                    if (System.Diagnostics.Stopwatch.GetTimestamp() + frequency / 10 < deadline)
                    {
                        yield return StartGameLiveFrames(2);
                        RequireConfiguredSmoke(!configuredAutoPlayCheckPending && sourceRoundEndCount == 0 && !spinning &&
                            session.Rounds == 1 && fixedSource.PocketRequests == 1,
                            "configured state-entry service escaped the actual high-win delay");
                        ++checks;
                    }
                }
                Debug.Log("ROULETTE_CONFIGURED_AUTO_SETTLED_SNAPSHOT: case=" +
                    caseName + " active=false configAuto=" +
                    configuredAutoPlay.AutoPlay + " turbo=" + configuredAutoPlay.Turbo +
                    " credit=" + session.Credit + " draws=" + fixedSource.PocketRequests +
                    " rackSkipped=true cycleSkipped=true");

                // The module is still disabled: a real completed round's case-1
                // request must remain pending until the explicit service below.
                yield return null;
                RequireConfiguredSmoke(configuredAutoPlayCheckPending == (!DeferredPaymentSmokeEnabled && !highWin) &&
                    session.Credit == (DeferredPaymentSmokeEnabled ? 1000 - wager : expectedCredit) &&
                    session.Rounds == 1 && !spinning && ReadPcCreditEvent() == null,
                    "automatic state-entry service escaped the controlled handoff");
                ++checks;
                if (funding)
                {
                    int cancelledDispatch = 0;
                    SourcePlayerInputs.Enqueue(0x200001, () => ++cancelledDispatch);
                    yield return StartGameLiveFrames(2);
                    PcCreditEvent deposit = ReadPcCreditEvent();
                    RequireConfiguredSmoke(deposit != null && deposit.kind == "deposit" &&
                        deposit.before == 999 && deposit.after == 2999 && deposit.phase == 3 &&
                        session.Credit == 2999 && session.Rounds == 1 && !spinning &&
                        !autoplay && configuredAutoPlay.AutoPlay == 0 &&
                        !configuredAutoPlay.ReplenishCredits && !configuredAutoPlay.Turbo &&
                        configuredAutoPlay.PlayerInput && !configuredAutoPlay.DemoOnly &&
                        configuredAutoPlay.TurboOption == 0 && !pcGameSoundsMuted &&
                        Math.Abs(AudioListener.volume - 1f) < .00001f && pcPlayerVolumeValue == 5 &&
                        SourcePlayerInputs.Count == 0 && cancelledDispatch == 0 &&
                        !configuredAutoPlayCheckPending && fixedSource.PocketRequests == 1 &&
                        fixedSource.SuitRequests == 0 && !session.DoubleAvailable &&
                        !recordedRoundPages[0].awaitingFinalPayout &&
                        sourceTableInputEnabled,
                        "case-1 must fund before queued down cancels configuration and restores gain");
                    ++checks;
                    string depositId = deposit.id;
                    yield return StartGameLiveFrames(2);
                    RequireConfiguredSmoke(!SourcePlayerInputs.DispatchOne() &&
                        ReadPcCreditEvent().id == depositId && session.Credit == 2999 &&
                        session.Rounds == 1 && !spinning && fixedSource.PocketRequests == 1,
                        "cancelled input or repeated service generated another deposit/round");
                    ++checks;
                }
                else if (highWin)
                {
                    long deadline = configuredResultDelayDeadlineTimestamp;
                    while (System.Diagnostics.Stopwatch.GetTimestamp() < deadline)
                        yield return null;
                    int cancelledDispatch = 0;
                    bool cancellationQueued = false;
                    if (DeferredPaymentSmokeEnabled)
                    {
                        // Queue at the real publication boundary, before End/Idle.
                        // Earlier queueing can dispatch during the new multi-frame
                        // payment adapter, before the original fund-then-purge path.
                        deferredPaymentSmokeAfterPublished = () => {
                            RequireDeferredPaymentSmoke(session.Credit == expectedCredit &&
                                pcGamePayment.IsResolved && pcGamePayment.historyPublished &&
                                sourceRoundEndCount == 0 && ReadPcCreditEvent() == null,
                                "high-win publication did not precede Idle funding");
                            SourcePlayerInputs.Enqueue(0x200001, () => ++cancelledDispatch);
                            cancellationQueued = true;
                        };
                        yield return WaitForDeferredPaymentSmokeIdle(() => sourceRoundEndCount == 1 &&
                            !configuredAutoPlayCheckPending && session.Credit == 3700 &&
                            sourceRoundPhase == SourceRoundPhase.None && SourcePlayerInputs.Count == 0,
                            "configured high-win StatePayout/Idle funding");
                        RequireDeferredPaymentSmoke(cancellationQueued, "publication did not queue the cancellation");
                    }
                    else
                    {
                        SourcePlayerInputs.Enqueue(0x200001, () => ++cancelledDispatch);
                        yield return StartGameLiveFrames(2);
                    }
                    PcCreditEvent deposit = ReadPcCreditEvent();
                    RequireConfiguredSmoke(!configuredAutoPlayCheckPending && !spinning &&
                        session.Rounds == 1 && session.Credit == 3700 &&
                        configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                        fixedSource.PocketRequests == 1 && SourcePlayerInputs.Count == 0 &&
                        fixedSource.SuitRequests == 0 && !session.DoubleAvailable &&
                        !recordedRoundPages[0].awaitingFinalPayout &&
                        cancelledDispatch == 0 && sourceTableInputEnabled &&
                        deposit != null && deposit.kind == "deposit" &&
                        deposit.before == 1700 && deposit.after == 3700 && deposit.phase == 3 &&
                        !pcGameSoundsMuted &&
                        Math.Abs(AudioListener.volume - 1f) < .00001f,
                        "after high-win delay, funding must precede input cancellation and table reopening without replay");
                    ++checks;
                }
                else yield return StateEndWaitForIdle(false);
                RequireConfiguredSmoke(sourceRoundEndCount == 1 && sourceRoundPhase == SourceRoundPhase.None,
                    "configured fixture never crossed exactly one actual StateEnd");
                if (DeferredPaymentSmokeEnabled && !highWin)
                    RequireConfiguredSmoke(configuredAutoPlay.AutoPlay == 0 &&
                        !configuredAutoPlay.TurboAutoPlay && !configuredAutoPlay.Turbo &&
                        !configuredAutoPlay.AutoPlayOffWhenGameFinished && !autoplay,
                        "deferred Turbo End did not consume its original off-on-finish request");
                RequireConfiguredSmoke(recordedRoundPages.Count == 1 &&
                    JsonUtility.ToJson(RequireConfiguredSmokeReceipt(fixedSource,
                        recordedRoundPages[0].spinJournalId)) == committedReceiptJson,
                    "configured completion replaced the real wheel receipt or drew a feature result");
                ++checks;

                CompleteDeferredPaymentSmoke(1000 - wager, expectedReturn, wager, false, highWin ? 2000 : 0);
                Debug.Log("ROULETTE_CONFIGURED_AUTOPLAY_PASSED: case=" +
                    caseName + " checks=" + checks +
                    " menu=actual-uGUI active=false fixed27Retained=true menuDraws=0 " +
                    "releaseOnlyNoReset=true configuredDelayBoundary=500 " +
                    "rounds=1 draws=1 finalCredit=" + session.Credit +
                    " declinedDouble=true featureDraws=0 archiveFinal=true" +
                    " receipt=durable effectivePocket=27 rawPocket=" + committedReceipt.rawResult +
                    (funding ? " fundedBeforeReset=true gainRestored=true tableReopened=true" :
                    highWin ? " actualHighWinDelayMs=500 fundedBeforeReset=true tableReopened=true" : " rackSkipped=true"));
                }
            }
            finally
            {
                if (autoRoutine != null) StopCoroutine(autoRoutine);
                autoRoutine = null;
                autoplay = false;
                configuredAutoPlayCheckPending = false;
                SourcePlayerInputs.Purge(0, 0);
                CloseDemoMenu();
                // A .Test. round/deposit intentionally keeps its matching
                // session and persistent records. Otherwise restore the exact
                // original session reference used by the isolated menu fixture.
                if (!persistentFixtureStarted) session = savedSession;
                outcome = savedOutcome;
                demoOutcome = savedDemo;
                configuredAutoPlay = savedConfiguration;
                pcPlayerTransactionActive = savedTransaction;
                sourceTableInputEnabled = savedTableInput;
                configuredAutoPlayFinishPending = savedFinishPending;
                pcPlayerVolumeValue = savedVolume;
                pcPlayerVolumeIdleDeadlineTimestamp = savedVolumeTimeout;
                if (savedVolumeExists) PlayerPrefs.SetInt(pcPlayerVolumeKey, savedVolumePreference);
                else PlayerPrefs.DeleteKey(pcPlayerVolumeKey);
                PlayerPrefs.Save();
                SetPcGameSoundsMuted(savedMuted);
                sourceInputModule.enabled = savedModuleEnabled;
                RefreshPortraitUi();
            }
            Application.Quit();
        }

        private IEnumerator ConfiguredSmokeFundStopEntry(ConfiguredReceiptSmokeOutcome fixedSource)
        {
            yield return ConfiguredSmokeChooseDemo(1);
            RequireConfiguredSmoke(configuredAutoPlay.AutoPlay == 1 &&
                configuredAutoPlay.ReplenishCredits && !configuredAutoPlay.Turbo &&
                !autoplay && autoRoutine == null && !configuredAutoPlayCheckPending &&
                sourceTableInputEnabled && session.TotalBet == 0 && session.Credit == 1000 &&
                ReadPcCreditEvent() == null,
                "fund-stop entry must begin with configuration only and no deposit");
            // From here the live input module must service the transition.
            // Do not assign a pending flag or invoke ServiceConfiguredAutoPlay.
            configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            sourceInputModule.enabled = true;
            ToggleAuto();
            RequireConfiguredSmoke(autoplay, "ordinary active Auto did not turn on");
            long timeout = System.Diagnostics.Stopwatch.GetTimestamp() +
                5L * System.Diagnostics.Stopwatch.Frequency;
            while ((autoplay || autoRoutine != null || configuredAutoPlayCheckPending || session.Credit != 3000) &&
                System.Diagnostics.Stopwatch.GetTimestamp() < timeout)
                yield return null;
            PcCreditEvent deposit = ReadPcCreditEvent();
            RequireConfiguredSmoke(!autoplay && autoRoutine == null &&
                !configuredAutoPlayCheckPending && !spinning && !session.SpinPending &&
                session.Rounds == 0 && session.TotalBet == 0 && session.Credit == 3000 &&
                sourceRoundEndCount == 0 && sourceRoundPhase == SourceRoundPhase.None &&
                configuredAutoPlay.AutoPlay == 1 && configuredAutoPlay.ReplenishCredits &&
                configuredAutoPlay.AutoPlayOffWhenGameFinished && !configuredAutoPlayFinishPending &&
                !sourceTableInputEnabled && !TableTouchInputEnabled() &&
                fixedSource.PocketRequests == 0 && fixedSource.SuitRequests == 0 &&
                recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                pendingSpin == null && deposit != null && deposit.kind == "deposit" &&
                deposit.before == 1000 && deposit.after == 3000 && deposit.phase == 3,
                "active fund rejection did not automatically dispatch the configured state-1 entry");
            string depositId = deposit.id;
            yield return null;
            yield return null;
            RequireConfiguredSmoke(!autoplay && !configuredAutoPlayCheckPending &&
                !spinning && session.Rounds == 0 && session.Credit == 3000 &&
                !sourceTableInputEnabled && ReadPcCreditEvent().id == depositId &&
                configuredAutoPlay.AutoPlayOffWhenGameFinished &&
                fixedSource.PocketRequests == 0,
                "failed configured start retried without a new state entry");
            sourceInputModule.enabled = false;
        }

        private IEnumerator ConfiguredSmokeChooseDemo(int option)
        {
            RequireConfiguredSmoke(SourcePlayerInputs.Count == 0, "menu fixture had stale input");
            ToggleDemoMenu();
            ShowDemoMenuPage(option / DemoOptionsPerPage);
            yield return null;
            RequireConfiguredSmoke(demoMenuVisible, "demo menu did not open");
            Button button = demoOptionButtons[option % DemoOptionsPerPage];
            PointerEventData held = null;
            SourcePlayerInputs.Enqueue(0x200001, () => {
                RequireConfiguredSmoke(SourcePlayerInputs.Count == 0,
                    "current menu down was not removed before its callback");
                held = PressThroughUiRaycast(button);
            });
            RequireConfiguredSmoke(SourcePlayerInputs.DispatchOne() && held != null &&
                !demoMenuVisible && SourcePlayerInputs.Count == 0,
                "source queue did not dispatch a real menu-down choice");
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease,
                () => ReleaseUiPointer(button, held));
            RequireConfiguredSmoke(SourcePlayerInputs.DispatchOne(), "menu release queue was empty");
        }

        private sealed class ResultWaitSmokeHost : IOriginalResultDisplayWaitHost
        {
            internal long Now, Award;
            internal bool Suspended, Auto, Eligible;
            internal int Inputs, Dispatched;
            public bool GameSuspended { get { return Suspended; } }
            public bool AutoPlayActive { get { return Auto; } }
            public bool ConfiguredAutoPlayEligible { get { return Eligible; } }
            public long CurrentAward { get { return Award; } }
            public bool InputAvailable { get { return Inputs > 0; } }
            public long Timestamp { get { return Now; } }
            public void DispatchOneInput() { --Inputs; ++Dispatched; }
        }

        private int ConfiguredSmokeHighWinDelay()
        {
            // Deterministic shared-clock checks; the live 720-credit fixture
            // above separately exercises the Game host and real timer service.
            var host = new ResultWaitSmokeHost { Now = 10000, Award = 500, Eligible = true };
            var clock = new OriginalResultDisplayWaitClock(500, host.Now, 1000, host);
            RequireConfiguredSmoke(clock.Completed && clock.HighDelayDueTimestamp == 0,
                "exactly 500 returned added a configured high-win delay");
            host.Award = 501;
            clock = new OriginalResultDisplayWaitClock(501, host.Now, 1000, host);
            host.Now = 10499; clock.UpdateAt(host.Now);
            RequireConfiguredSmoke(!clock.Completed && clock.HighDelayStartedAt == 10000 &&
                clock.HighDelayDueTimestamp == 10500,
                "configured-only high-win delay returned before its fresh 500 ms");
            host.Now = 10500; clock.UpdateAt(host.Now);
            RequireConfiguredSmoke(clock.Completed && clock.CompletedAt == host.Now,
                "configured-only high-win delay did not return at its timer receipt");
            host.Now = 20000; host.Auto = true; host.Eligible = false;
            clock = new OriginalResultDisplayWaitClock(501, host.Now, 1000, host);
            for (int i = 0; i < 30; ++i) { host.Now = clock.WaitDueTimestamp; clock.UpdateAt(host.Now); }
            RequireConfiguredSmoke(clock.Completed && clock.TimeoutCount == 30 &&
                clock.CompletedAt == 21050 && clock.HighDelayDueTimestamp == 0,
                "active-only Auto added a configured delay or skipped fresh waits");
            host.Now = 30000; host.Eligible = true;
            clock = new OriginalResultDisplayWaitClock(501, host.Now, 1000, host);
            host.Auto = false; host.Now = 30034; clock.UpdateAt(host.Now);
            RequireConfiguredSmoke(clock.Phase == OriginalResultDisplayWaitPhase.AutoWait &&
                clock.TimeoutCount == 0 && clock.HighDelayDueTimestamp == 0,
                "turning Auto off cancelled the in-flight 35 ms wait");
            host.Now = 30035; clock.UpdateAt(host.Now);
            host.Eligible = false; host.Inputs = 1; host.Now = 30534; clock.UpdateAt(host.Now);
            RequireConfiguredSmoke(clock.Phase == OriginalResultDisplayWaitPhase.HighWinDelay &&
                clock.HighDelayStartedAt == 30035 && clock.HighDelayDueTimestamp == 30535 &&
                host.Inputs == 1 && host.Dispatched == 0,
                "configured high-win delay changed after entry or consumed queued input");
            return 6;
        }

    }
}
