using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // One persistent isolated round per invocation. Use option 0/1/2 to
        // replace a fixed-27 math choice while preserving path 27; option 30
        // tests the fixed choice itself and 39 tests gaff-only visual path 36.
        private IEnumerator DemoMathPathSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int option = 0;
            int optionArgument = Array.IndexOf(args, "-rouletteDemoMathPathOption");
            if (optionArgument >= 0 && (optionArgument + 1 >= args.Length ||
                !int.TryParse(args[optionArgument + 1], out option)))
                throw new InvalidOperationException("Demo math/path option is invalid");
            RequireConfiguredSmoke(option == 0 || option == 1 || option == 2 ||
                option == 30 || option == 39, "math/path case must be 0, 1, 2, 30 or 39");
            RequireConfiguredSmoke(demoShowMode == 1 && demoOutcome != null &&
                sourceInputModule != null && !auNationalStandardsEnabled &&
                !temporaryHistory && !autoplay && autoRoutine == null && !spinning &&
                session != null && session.Credit == 1000 && session.TotalBet == 0 &&
                session.Rounds == 0 && session.Denomination == 1 && !session.SpinPending &&
                !session.InDoubleUp && !winRackActive && !ResultDisplayActive &&
                recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                Array.IndexOf(args, "-rouletteVisualVariant") < 0 &&
                SourcePlayerInputs.Count == 0 && pendingSpin == null &&
                ReadPcCreditEvent() == null,
                "math/path needs fresh persistent isolated keys, ordinary PC and no visual override");

            IRouletteSession savedSession = session;
            IRouletteOutcomeSource savedOutcome = outcome;
            DemoPocketOutcomeSource savedDemo = demoOutcome;
            OriginalWheelPathRandom savedVisualRandom = wheelVisualRandom;
            OriginalAutoPlayData savedConfig = configuredAutoPlay;
            bool savedModule = sourceInputModule.enabled;
            bool savedMuted = pcGameSoundsMuted;
            bool roundStarted = false;
            var counted = new ConfiguredSmokeOutcome(16);
            var fixtureDemo = new DemoPocketOutcomeSource(counted);
            int expectedPath = option == 39 ? 36 : 27;
            int expectedPocket = option >= 3 ? option - 3 : 16;
            try
            {
                sourceInputModule.enabled = false;
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayCheckPending = false;
                configuredAutoPlayFinishPending = false;
                sourceTableInputEnabled = true;
                demoOutcome = fixtureDemo;
                outcome = fixtureDemo;
                wheelVisualRandom = new OriginalWheelPathRandom(1);
                session = new RouletteSession(savedSession.Profile, 1000, fixtureDemo) {
                    DoubleEnabled = true };
                SetPcGameSoundsMuted(false);
                string before = PaytableTestLiveFingerprint(session);

                yield return ConfiguredSmokeChooseDemo(option == 39 ? 39 : 30);
                RequireConfiguredSmoke(fixtureDemo.SelectedPocket == expectedPath &&
                    fixtureDemo.ArmedVisualPath == expectedPath,
                    "fixed menu did not set separate math/path selections");
                if (option < 3) yield return ConfiguredSmokeChooseDemo(option);
                RequireConfiguredSmoke(fixtureDemo.SelectedOption == option &&
                    fixtureDemo.SelectedPocket == (option >= 3 ? expectedPocket : -1) &&
                    fixtureDemo.ArmedVisualPath == expectedPath &&
                    counted.PocketRequests == 0 && counted.SuitRequests == 0 &&
                    PaytableTestLiveFingerprint(session) == before && !autoplay &&
                    autoRoutine == null && !configuredAutoPlayCheckPending,
                    "menu math/path selection changed money/RNG or started active Auto");

                session.PlaceBet(100, 1); // zero loses against 16, 27 and 36
                if (option == 1 || option == 2)
                {
                    // Explicit caller request stops configured continuation
                    // only when the actual StateEnd consumes this round.
                    configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                    Debug.Log("ROULETTE_DEMO_MATH_PATH_CALL_LEVEL_END_REQUEST: true");
                }
                SaveStagedBetsIfChanged();
                RefreshPortraitUi();
                yield return null;
                PointerEventData held = null;
                SourcePlayerInputs.Enqueue(0x200001,
                    () => held = PressThroughUiRaycast(uiSpinButton));
                RequireConfiguredSmoke(SourcePlayerInputs.DispatchOne(), "PLAY queue was empty");
                roundStarted = spinning;
                RequireConfiguredSmoke(spinning && session.Credit == 999 &&
                    counted.PocketRequests == 1 && counted.SuitRequests == 0 &&
                    wheelOutcome == expectedPocket && wheelVariant == expectedPath &&
                    fixtureDemo.ArmedVisualPath == -1 && fixtureDemo.SelectedPocket == -1 &&
                    fixtureDemo.SelectedOption == (option >= 3 ? 0 : option) &&
                    wheelTurboSnapRequested == (option == 2),
                    "actual SetStop did not separate mathematical outcome and one-shot visual path");
                ReleaseUiPointer(uiSpinButton, held);

                // The source's armed branch skips libc rand entirely. These
                // untouched seed-1 outputs detect even one accidental advance.
                RequireConfiguredSmoke(wheelVisualRandom.NextRaw() == 1804289383 &&
                    wheelVisualRandom.NextRaw() == 846930886,
                    "armed SetStop advanced ordinary visual RNG");
                if (option != 2)
                {
                    SourcePlayerInputs.Enqueue(0x200010,
                        () => DispatchSourceButton(KeyCode.Space));
                    RequireConfiguredSmoke(SourcePlayerInputs.DispatchOne() &&
                        wheelSkillStopRequested && wheelStage == WheelStage.EaseOut,
                        "source physical START did not enter skill-stop ease-out");
                }
                yield return StartGameWaitForActualSettlement();
                RequireConfiguredSmoke(!spinning && session.Rounds == 1 &&
                    session.LastResult != null && session.LastResult.Pocket == expectedPocket &&
                    session.LastResult.Returned == 0 && session.Credit == 999 &&
                    recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                    !recordedRoundPages[0].awaitingFinalPayout && !session.DoubleAvailable &&
                    counted.PocketRequests == 1 && counted.SuitRequests == 0 &&
                    pendingSpin == null && !autoplay && !configuredAutoPlayCheckPending &&
                    sourceRoundEndCount == 1 && fixtureDemo.SelectedOption == 0 &&
                    ReadPcCreditEvent() == null,
                    "separated math/path round did not settle exactly once");
                Debug.Log("ROULETTE_DEMO_MATH_PATH_PASSED: option=" + option +
                    " mathPocket=" + expectedPocket + " visualPath=" + expectedPath +
                    " mathDraws=1 visualDraws=0 armConsumed=true menuReset=" +
                    fixtureDemo.SelectedOption + " returned=0 credit=999 rounds=1");
            }
            finally
            {
                CloseDemoMenu();
                if (!roundStarted) session = savedSession;
                outcome = savedOutcome;
                demoOutcome = savedDemo;
                wheelVisualRandom = savedVisualRandom;
                configuredAutoPlay = savedConfig;
                sourceInputModule.enabled = savedModule;
                SetPcGameSoundsMuted(savedMuted);
            }
            Application.Quit();
        }
    }
}
