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
        private sealed class StartGameSmokeOutcome : IRouletteOutcomeSource, IDisposable
        {
            private readonly IRouletteOutcomeSource platformOutcome;
            public int Pockets, Suits;
            public StartGameSmokeOutcome(IRouletteOutcomeSource inner)
            {
                if (inner == null) throw new ArgumentNullException("inner");
                platformOutcome = inner;
            }
            public int DrawPocket()
            {
                // Consume and persist the real isolated PC stream. Only the
                // returned fixture pocket is controlled, so draw/save and
                // pending-recovery assertions exercise production RNG state.
                int drawn = platformOutcome.DrawPocket();
                ++Pockets;
                Debug.Log("ROULETTE_START_GAME_COUNTED_PC_DRAW: request=" + Pockets +
                    " actual=" + drawn + " fixture=16");
                return 16;
            }
            public int DrawDoubleSuit()
            {
                ++Suits;
                throw new InvalidOperationException("StartGame diagnostic reached feature RNG");
            }
            public void Dispose()
            {
                var disposable = platformOutcome as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        [Serializable]
        private sealed class StartGameStateBetReceipt
        {
            public string testCase, phase, economy, archive, rng, undo;
            public string pending, staged, cleared, settledUndo, creditEvent;
            public bool ordinary;
            public int sourceState, neighbor, neighborNv;
            public long credit, wagered, returned, rounds;
        }

        private string StartGameStateBetReceiptKey
        {
            get { return recordedHistoryKey + ".StartGameStateBetFixture.v1"; }
        }

        private static void RequireStartGameStateBet(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("StartGame/StateBet: " + message);
        }

        // Real user-input gates and actual source-queue transitions are kept
        // separate from the explicitly named call-level StartGame rejection.
        // Each case has an independent persistent .Test. key and receipt.
        private IEnumerator StartGameStateBetSmoke(bool load)
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStartGameStateBetCase");
            RequireStartGameStateBet(at >= 0 && at + 1 < args.Length, "missing case");
            string testCase = args[at + 1];
            bool ordinary = Array.IndexOf(args, "-rouletteStartGameStateBetOrdinary") >= 0;
            RequireStartGameStateBet(new[] { "zero", "credit", "entry", "accept" }.Contains(testCase) &&
                (!ordinary || testCase == "accept"), "unknown case or unsupported ordinary case");
            RequireStartGameStateBet(demoShowMode == (ordinary ? 0 : 1) &&
                sourceInputModule != null && demoOutcome != null && session != null &&
                !temporaryHistory && recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                Array.IndexOf(args, "-rouletteVisualVariant") < 0 &&
                !autoplay && autoRoutine == null && !session.InDoubleUp &&
                SourcePlayerInputs.Count == 0,
                "requires isolated history/volume/RNG and the real source input module");

            bool moduleEnabled = sourceInputModule.enabled;
            string completedPhase = "";
            try
            {
                sourceInputModule.enabled = false;
                if (load)
                {
                    var proof = JsonUtility.FromJson<StartGameStateBetReceipt>(
                        PlayerPrefs.GetString(StartGameStateBetReceiptKey, ""));
                    RequireStartGameStateBet(proof != null && proof.testCase == testCase &&
                        proof.ordinary == ordinary, "missing or mismatched persistent receipt");
                    if (testCase == "accept" && proof.phase == "pending")
                    {
                        yield return StartGameStateBetResume(proof);
                        completedPhase = "resumed";
                    }
                    else
                    {
                        RequireStartGameReceipt(proof);
                        RequireStartGameStateBet(!spinning && !session.SpinPending && pendingSpin == null &&
                            (int)sourceGameplayState == proof.sourceState,
                            "reload changed its durable source state or resumed a completed round");
                        long restoredPaid = testCase == "accept" && proof.phase == "settled" && !ordinary ? 36 : 0;
                        if (testCase == "accept" && proof.phase == "settled")
                            RequireStartGameAcceptedAutoState(ordinary, false, 0);
                        RequireStartGameAuPaid(restoredPaid, "reload " + proof.phase);
                        yield return StartGameLiveFrames(2);
                        RequireStartGameReceipt(proof);
                        RequireStartGameStateBet((int)sourceGameplayState == proof.sourceState &&
                            !sourceBetStateEntryPending,
                            "automatic reload entry changed its persisted source state");
                        if (testCase == "accept" && proof.phase == "settled")
                            RequireStartGameAcceptedAutoState(ordinary, false, 0);
                        RequireStartGameAuPaid(restoredPaid, "reload after live entry " + proof.phase);
                        if (testCase == "entry" && proof.phase == "entry_direct")
                        {
                            RequireStartGameStateBet(sourceGameplayState == SourceGameplayState.Idle &&
                                session.TotalBet == 0,
                                "direct Undo to zero did not reload as Idle");
                            yield return StartGameGroupUndoAfterReload(proof);
                            completedPhase = "entry_group_saved";
                        }
                        else
                        {
                            RequireStartGameStateBet(proof.phase == "rejected" ||
                                proof.phase == "entry_group" || proof.phase == "settled",
                                "unexpected completed receipt phase");
                            CaptureStartGameStateBet("reload_" + proof.phase);
                            completedPhase = "reload_" + proof.phase;
                        }
                    }
                }
                else
                {
                    RequireStartGameStateBet(!spinning && !session.SpinPending && pendingSpin == null &&
                        session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                        ReadPcCreditEvent() == null && !PlayerPrefs.HasKey(StartGameStateBetReceiptKey) &&
                        sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                        configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlayCheckPending,
                        "save requires unused fixture keys and a fresh Idle state");
                    RequireStartGameAuPaid(0, "fresh Idle");
                    RequireStartGameStateBet(ReferenceEquals(outcome, demoOutcome) &&
                        demoOutcome.SelectedOption == 0 && demoOutcome.ArmedVisualPath == -1,
                        "fresh counted PC source must begin with the original unarmed Normal outcome");
                    // Capture before replacing the outer demo reference; the
                    // wrapper must never call itself through the game field.
                    IRouletteOutcomeSource platformOutcome = outcome;
                    var counted = new StartGameSmokeOutcome(platformOutcome);
                    demoOutcome = new DemoPocketOutcomeSource(counted);
                    outcome = demoOutcome;
                    session = new RouletteSession(session.Profile, testCase == "credit" ? 4 : 1000,
                        demoOutcome) { DoubleEnabled = true };
                    if (testCase == "zero" || testCase == "credit")
                    {
                        yield return StartGameRejectedFixture(testCase, counted);
                        completedPhase = "rejected_saved";
                    }
                    else if (testCase == "entry")
                    {
                        yield return StartGameEntryFixture(counted);
                        completedPhase = "entry_direct_saved";
                    }
                    else
                    {
                        yield return StartGameAcceptedFixture(ordinary, counted);
                        completedPhase = "pending_saved";
                    }
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            // The runner waits for natural exit and rereads the final log.
            // Emit success after cleanup so a finally failure cannot pass.
            Debug.Log((load ? "ROULETTE_START_GAME_STATE_BET_LOAD_PASSED: " :
                "ROULETTE_START_GAME_STATE_BET_SAVE_PASSED: ") +
                "case=" + testCase + " phase=" + completedPhase + " ordinary=" + ordinary +
                " credit=" + session.Credit + " rounds=" + session.Rounds +
                " sourceState=" + (int)sourceGameplayState);
            Application.Quit();
        }

        private IEnumerator StartGameRejectedFixture(string testCase, StartGameSmokeOutcome counted)
        {
            if (testCase == "credit")
            {
                yield return StartGameChooseDemo(19); // fixed pocket 16
                for (int i = 0; i < 3; ++i)
                {
                    RefreshPortraitUi();
                    DemoCleanupClick(uiBetButtons[27]);
                    if (sourceBetStateEntryPending) yield return StartGameWaitForBetEntry();
                }
                RequireStartGameStateBet(session.Credit == 4 && session.TotalBet == 3 &&
                    ((RouletteSession)session).ExportUndoHistory().Length == 3,
                    "real low-credit input did not stage three one-chip operations");
                yield return StartGamePlayAndSettle();
                RequireStartGameStateBet(session.Credit == 1 && session.TotalBet == 3 &&
                    session.Rounds == 1 && session.Wagered == 3 && session.Returned == 0 &&
                    session.LastResult.Pocket == 16 && counted.Pockets == 1 && counted.Suits == 0 &&
                    sourceGameplayState == SourceGameplayState.Idle,
                    "real loss did not create the retained unaffordable wager");
                // Native Idle starts its messages after a real 1000 ms
                // delay. Wait for that clock before proving Stop(false).
                long promptDeadline = System.Diagnostics.Stopwatch.GetTimestamp() +
                    4L * System.Diagnostics.Stopwatch.Frequency;
                while (System.Diagnostics.Stopwatch.GetTimestamp() < promptReadyTimestamp &&
                    System.Diagnostics.Stopwatch.GetTimestamp() < promptDeadline)
                    yield return null;
                RequireStartGameStateBet(promptReadyTimestamp > 0 &&
                    System.Diagnostics.Stopwatch.GetTimestamp() >= promptReadyTimestamp,
                    "settled Idle prompt did not reach its actual delayed start");
                RefreshPortraitUi();
            }
            if (testCase == "zero")
            {
                // Keep the nonempty arm proof separate from the live-result
                // rejection below: choosing Demo itself clears that result.
                yield return StartGameChooseDemo(30);
                StateBetCleanupHoldPreview(27, PointerInputModule.kMouseLeftId, false);
                StateBetCleanupReleasePreview(PointerInputModule.kMouseLeftId, false);
                RequireStartGameStateBet(session.TotalBet == 0 && selectedNeighbor == 27 &&
                    PlayerPrefs.GetInt(NeighborSelectionKey, -2) == 27,
                    "actual preview did not prepare the neighbor selection without a wager: bet=" +
                    session.TotalBet + " selected=" + selectedNeighbor + " nv=" +
                    PlayerPrefs.GetInt(NeighborSelectionKey, -2) + " table=" + TableTouchInputEnabled() +
                    " button=" + uiBetButtons[27].interactable);
            }
            RefreshPortraitUi();
            if (testCase == "credit")
                RequireStartGameStateBet(!currentGameResultsCleared &&
                    uiTableNumberMark.gameObject.activeSelf && tableMarkedPocket == 16 &&
                    !sourcePromptStopped && promptRoot.gameObject.activeSelf && uiPrompt.texture != null,
                    "low-credit rejection did not begin from its still-live settled result and prompt");
            RequireStartGameStateBet(!uiSpinButton.interactable && !SourcePlayInputAllowed &&
                sourceTableInputEnabled && demoMenuState == 3,
                "invalid wager did not disable both user PLAY routes");
            string gateProof = StartGameDurableFingerprint();
            int neighborBefore = selectedNeighbor;
            int neighborNvBefore = PlayerPrefs.GetInt(NeighborSelectionKey, -2);
            bool clearedBefore = currentGameResultsCleared;
            OriginalAutoPlayData configBefore = configuredAutoPlay.Clone();
            bool muteBefore = pcGameSoundsMuted;
            int drawsBefore = counted.Pockets;
            int selectedOptionBefore = demoOutcome.SelectedOption;
            int armedPathBefore = demoOutcome.ArmedVisualPath;
            RequireStartGameStateBet(testCase == "zero" ?
                selectedOptionBefore == 30 && armedPathBefore == 27 :
                selectedOptionBefore == 0 && armedPathBefore == -1,
                "rejection fixture did not retain its intended unconsumed or already consumed arms");
            RequireStartGameAuPaid(0, testCase + " before user gate");
            CaptureStartGameStateBet("before_gate");

            PointerEventData gatePointer = DemoCleanupQueue(uiSpinButton);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 1 &&
                !spinning && !session.SpinPending && sourceTableInputEnabled && demoMenuState == 3,
                "disabled real PLAY down entered DoPlay or consumed its queued release");
            DemoCleanupDrainRelease(gatePointer);
            // This is the same FIFO callback captured by the physical button
            // adapter, not a direct BeginSpin call or a forged enabled sprite.
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(KeyCode.Space));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 1 &&
                sourceTableInputEnabled && demoMenuState == 3 && !spinning && !session.SpinPending,
                "disabled source button route bypassed the native PLAY sprite gate");
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 0 &&
                StartGameDurableFingerprint() == gateProof && selectedNeighbor == neighborBefore &&
                PlayerPrefs.GetInt(NeighborSelectionKey, -2) == neighborNvBefore &&
                currentGameResultsCleared == clearedBefore && counted.Pockets == drawsBefore &&
                counted.Suits == 0 && demoOutcome.SelectedOption == selectedOptionBefore &&
                demoOutcome.ArmedVisualPath == armedPathBefore,
                "disabled user input changed funds/history/Undo/RNG, selection or a live result");
            if (testCase == "credit")
                RequireStartGameStateBet(!currentGameResultsCleared &&
                    uiTableNumberMark.gameObject.activeSelf && !sourcePromptStopped,
                    "disabled user input cleared the live result before the call-level rejection");

            // The native enabled gate makes ordinary empty/poor-credit PLAY
            // unreachable. This deliberately named call-level fixture covers
            // StartGame's defensive rejection without claiming user reachability.
            Vector2 playPoint = StartGameButtonPoint(uiSpinButton);
            const int mouse = PointerInputModule.kMouseLeftId;
            sourceInputModule.QueuePointerSample(mouse, playPoint, true, false, true, false);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne(), "call-level capture down missing");
            sourceInputModule.QueuePointerSample(mouse, playPoint + Vector2.right,
                false, false, true, false);
            PointerEventData retained = sourceInputModule.QueuePointerSample(mouse, playPoint,
                false, true, false, false);
            RequireStartGameStateBet(SourcePlayerInputs.Count == 2 && retained.pointerPress != null,
                "call-level rejection lacks a real captured move/release backlog");
            string accountingBefore = StartGameAccountingFingerprint();
            SourceGameplayState stateBefore = sourceGameplayState;
            bool resultBefore = session.ResultPhase;
            bool editedBefore = sourceBetsChangedSincePlay;
            Debug.Log("ROULETTE_START_GAME_CALL_LEVEL_FIXTURE: case=" + testCase +
                " ordinaryUserInput=false enabledGateAlreadyVerified=true");
            InvokeStartGameCallLevelFixture();
            RequireStartGameStateBet(!spinning && !session.SpinPending && pendingSpin == null &&
                !sourceTableInputEnabled && demoMenuState == 2 && !demoMenuVisible &&
                sourceGameplayState == stateBefore && !sourceBetStateEntryPending &&
                !configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending &&
                currentGameResultsCleared && sourcePromptStopped &&
                session.ResultPhase == resultBefore && sourceBetsChangedSincePlay == editedBefore &&
                selectedNeighbor == -1 && PlayerPrefs.GetInt(NeighborSelectionKey, -2) == -1 &&
                SourcePlayerInputs.Count == 2 && retained.pointerPress != null &&
                StartGameAccountingFingerprint() == accountingBefore && counted.Pockets == drawsBefore &&
                counted.Suits == 0 && demoOutcome.SelectedOption == selectedOptionBefore &&
                demoOutcome.ArmedVisualPath == armedPathBefore &&
                SameConfiguredSmokeData(configuredAutoPlay, configBefore) && pcGameSoundsMuted == muteBefore,
                "call-level rejection failed precheck cleanup, purged input, changed state or consumed money/RNG/arms");
            RefreshPortraitUi();
            RequireStartGameStateBet(!uiTableNumberMark.gameObject.activeSelf && sourcePromptStopped &&
                currentGameResultsCleared,
                "call-level rejection did not immediately hide the current number marker and prompt");
            RequireStartGameAuPaid(0, testCase + " immediate call-level rejection");
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.DispatchOne(),
                "rejection lost its retained physical move/up");
            RequireSourcePointerReleased(retained);
            RequireStartGameStateBet(SourcePlayerInputs.Count == 0 &&
                StartGameAccountingFingerprint() == accountingBefore && counted.Pockets == drawsBefore,
                "retained move/up repeated the rejected commitment");
            yield return StartGameLiveFrames(2);
            StartGameDispatchKey(KeyCode.F8);
            RequireStartGameStateBet(!demoMenuVisible && demoMenuState == 2 &&
                !sourceTableInputEnabled && sourceGameplayState == stateBefore &&
                !sourceBetStateEntryPending && StartGameAccountingFingerprint() == accountingBefore &&
                counted.Pockets == drawsBefore && demoOutcome.SelectedOption == selectedOptionBefore &&
                demoOutcome.ArmedVisualPath == armedPathBefore,
                "failed StartGame silently reentered Idle/Bet, enabled Demo or changed its commitment");
            RefreshPortraitUi();
            RequireStartGameStateBet(DisplayedPaid == 0 && VisibleWinningPayId() == -1 &&
                resultCycleStartedTimestamp == 0 && uiTotalWinAmount.Value == "" &&
                uiLinePays.Value == "" && uiGamePays.Value == "" &&
                !uiTableNumberMark.gameObject.activeSelf && sourcePromptStopped,
                "failed StartGame retained live result presentation");
            RequireStartGameAuPaid(0, testCase + " rejected after live frames");
            SaveStartGameReceipt(testCase, "rejected", false);
            CaptureStartGameStateBet("after_call_level_reject");
        }

        private void InvokeStartGameCallLevelFixture()
        {
            BeginSpin();
        }

        private IEnumerator StartGameEntryFixture(StartGameSmokeOutcome counted)
        {
            yield return StartGameChooseDemo(2);
            RequireStartGameStateBet(configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted &&
                !autoplay && demoMenuState == 3 && sourceGameplayState == SourceGameplayState.Idle,
                "real Turbo menu did not prepare an idle configured fixture");
            OriginalAutoPlayData expectedReset = configuredAutoPlay.Clone();
            expectedReset.SetOptions(false, false, true, false, false, 0);
            string rngBefore = DemoCleanupRngBytes();
            var releaseProbe = uiBetButtons[1].gameObject.AddComponent<RouletteStartGameReleaseProbe>();
            int autoAtRelease = -1;
            bool mutedAtRelease = false;
            releaseProbe.ObservedUp = data => {
                autoAtRelease = configuredAutoPlay.AutoPlay;
                mutedAtRelease = pcGameSoundsMuted;
            };
            try
            {
                Vector2 firstPoint = StartGameButtonPoint(uiBetButtons[27]);
                Vector2 secondPoint = StartGameButtonPoint(uiBetButtons[1]);
                sourceInputModule.QueuePointerSample(93, firstPoint, true, false, true, true);
                RequireStartGameStateBet(SourcePlayerInputs.DispatchOne(), "first table down missing");
                sourceInputModule.QueuePointerSample(94, secondPoint, true, false, true, true);
                RequireStartGameStateBet(SourcePlayerInputs.DispatchOne(), "second captured table down missing");
                PointerEventData first = sourceInputModule.QueuePointerSample(93, firstPoint,
                    false, true, false, true);
                sourceInputModule.QueuePointerSample(94, secondPoint + Vector2.right,
                    false, false, true, true);
                PointerEventData second = sourceInputModule.QueuePointerSample(94, secondPoint,
                    false, true, false, true);
                RequireStartGameStateBet(SourcePlayerInputs.Count == 3 && second.pointerPress != null &&
                    session.TotalBet == 0 && sourceGameplayState == SourceGameplayState.Idle,
                    "held table inputs committed before release");
                RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && session.TotalBet == 1 &&
                    session.Bets.Count == 1 && session.Bets[127] == 1 &&
                    sourceGameplayState == SourceGameplayState.Bet && sourceBetStateEntryPending &&
                    SourcePlayerInputs.Count == 2 && second.pointerPress != null &&
                    configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted && releaseProbe.UpCount == 0 &&
                    ((RouletteSession)session).ExportUndoHistory().Length == 1 &&
                    JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey, ""))
                        .sourceGameplayState == 2,
                    "first callback did not finish its table/Undo/NV before deferring StateBet entry");
                RequireSourcePointerReleased(first);
                yield return StartGameWaitForBetEntry();
                RequireStartGameStateBet(SourcePlayerInputs.Count == 0 && session.TotalBet == 1 &&
                    session.Bets.Count == 1 && session.Bets[127] == 1 &&
                    ((RouletteSession)session).ExportUndoHistory().Length == 1 &&
                    releaseProbe.UpCount == 1 && releaseProbe.LastUpPosition == Vector2.zero &&
                    autoAtRelease == 1 && mutedAtRelease &&
                    SameConfiguredSmokeData(configuredAutoPlay, expectedReset) && !pcGameSoundsMuted &&
                    demoMenuState == 3 && sourceTableInputEnabled &&
                    sourceGameplayState == SourceGameplayState.Bet && !sourceBetStateEntryPending &&
                    DemoCleanupRngBytes() == rngBefore && counted.Pockets == 0 && counted.Suits == 0 &&
                    ReadPcCreditEvent() == null,
                    "live StateBet entry did not release capture before reset or discarded the wrong wager");
                RequireSourcePointerReleased(second);
                RequireStartGameStateBet(second.pointerEnter == null, "purged touch retained pointerEnter");
                PointerEventData physicalUp = sourceInputModule.QueuePointerSample(94, secondPoint,
                    false, true, false, true);
                RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && releaseProbe.UpCount == 1 &&
                    session.TotalBet == 1, "late physical release repeated the purged table action");
                RequireSourcePointerReleased(physicalUp);
            }
            finally
            {
                Destroy(releaseProbe);
            }
            string firstBetProof = StartGameAccountingFingerprint();
            yield return StartGameChooseDemo(0); // actual menu must be usable after entry
            RequireStartGameStateBet(demoMenuState == 3 && !demoMenuVisible &&
                demoOutcome.SelectedOption == 0 && StartGameAccountingFingerprint() == firstBetProof,
                "StateBet entry did not leave a usable Demo menu or menu input changed the wager");

            QueueStartGameButton(uiBetButtons[27], 95, true);
            QueueStartGameButton(uiBetButtons[1], 96, true);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.DispatchOne() &&
                session.TotalBet == 2 && !sourceBetStateEntryPending && SourcePlayerInputs.Count == 2 &&
                sourceGameplayState == SourceGameplayState.Bet,
                "additional same-StateBet wager incorrectly requested entry or purged its successor");
            yield return StartGameDrainWithLiveModule();
            RequireStartGameStateBet(session.TotalBet == 3 && session.Bets[127] == 2 &&
                session.Bets[101] == 1 && ((RouletteSession)session).ExportUndoHistory().Length == 3 &&
                !sourceBetStateEntryPending && counted.Pockets == 0 && DemoCleanupRngBytes() == rngBefore,
                "same-state additional input was purged instead of dispatched exactly once");
            RefreshPortraitUi();
            DemoCleanupClick(uiClearButton);
            RequireStartGameStateBet(session.TotalBet == 0 && !session.CanUndo &&
                sourceGameplayState == SourceGameplayState.Bet && !sourceBetStateEntryPending,
                "Clear to zero incorrectly entered Idle or requested another StateBet entry");
            RequireStartGameAuPaid(0, "Clear to empty Bet");

            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[27]);
            RequireStartGameStateBet(session.TotalBet == 1 && !sourceBetStateEntryPending,
                "wager after Clear reentered the unchanged StateBet");
            yield return StartGameChooseDemo(19);
            yield return StartGamePlayAndSettle();
            RequireStartGameStateBet(sourceGameplayState == SourceGameplayState.Idle &&
                session.Credit == 999 && session.Wagered == 1 && session.Returned == 0 &&
                session.Rounds == 1 && session.LastResult.Pocket == 16 && counted.Pockets == 1,
                "direct-Undo precursor did not settle its one actual losing round");
            string settledArchive = DemoCleanupArchive(false);
            string settledRng = DemoCleanupRngBytes();
            RefreshPortraitUi();
            DemoCleanupClick(uiUndoButton);
            RequireStartGameStateBet(session.TotalBet == 0 && !session.CanUndo &&
                !((RouletteSession)session).LastUndoRequestsBetState &&
                sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                session.Credit == 999 && session.Rounds == 1 && DemoCleanupArchive(false) == settledArchive &&
                DemoCleanupRngBytes() == settledRng && counted.Pockets == 1 && counted.Suits == 0 &&
                JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey, ""))
                    .sourceGameplayState == 1,
                "actual direct Undo to zero changed source state or settled money/history/RNG");
            RequireStartGameAuPaid(0, "direct Undo to empty Idle");
            SaveStartGameReceipt("entry", "entry_direct", false);
            CaptureStartGameStateBet("entry_direct_idle");
        }

        private IEnumerator StartGameGroupUndoAfterReload(StartGameStateBetReceipt prior)
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiVoisinsButton);
            RequireStartGameStateBet(session.TotalBet == 9 && session.CanUndo &&
                sourceGameplayState == SourceGameplayState.Bet && sourceBetStateEntryPending,
                "actual grouped wager did not leave restored Idle through SetBet");
            yield return StartGameWaitForBetEntry();
            yield return StartGameChooseDemo(30); // 27 is outside Voisins
            yield return StartGamePlayAndSettle();
            RequireStartGameStateBet(session.Credit == prior.credit - 9 &&
                session.Wagered == prior.wagered + 9 && session.Returned == prior.returned &&
                session.Rounds == prior.rounds + 1 && session.LastResult.Pocket == 27 &&
                session.LastResult.Wager == 9 && session.LastResult.Returned == 0 &&
                sourceGameplayState == SourceGameplayState.Idle,
                "actual group round did not settle once before Undo");
            string archive = DemoCleanupArchive(false);
            string rng = DemoCleanupRngBytes();
            RefreshPortraitUi();
            DemoCleanupClick(uiUndoButton);
            RequireStartGameStateBet(session.TotalBet == 0 && !session.CanUndo &&
                ((RouletteSession)session).LastUndoRequestsBetState &&
                sourceGameplayState == SourceGameplayState.Bet && sourceBetStateEntryPending &&
                session.Credit == prior.credit - 9 && session.Rounds == prior.rounds + 1 &&
                DemoCleanupArchive(false) == archive && DemoCleanupRngBytes() == rng &&
                JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey, ""))
                    .sourceGameplayState == 2,
                "actual grouped Undo to zero did not request and persist StateBet");
            yield return StartGameWaitForBetEntry();
            RequireStartGameStateBet(sourceGameplayState == SourceGameplayState.Bet &&
                session.TotalBet == 0 && DemoCleanupArchive(false) == archive && DemoCleanupRngBytes() == rng,
                "group-Undo entry changed settlement or inferred Idle from an empty table");
            RequireStartGameAuPaid(0, "group Undo to empty Bet");
            SaveStartGameReceipt("entry", "entry_group", false);
            CaptureStartGameStateBet("entry_group_bet");
        }

        private IEnumerator StartGameAcceptedFixture(bool ordinary, StartGameSmokeOutcome counted)
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[27]);
            RequireStartGameStateBet(session.TotalBet == 1 && sourceBetStateEntryPending,
                "actual first wager did not request StateBet");
            yield return StartGameWaitForBetEntry();
            if (!ordinary)
            {
                yield return StartGameChooseDemo(2);
                yield return StartGameChooseDemo(30);
                // Configured Turbo now survives a process restart. Request
                // its real StateEnd stop before saving this one-round fixture;
                // PLAY's release-only purge must preserve that request too.
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            }
            RequireStartGameAcceptedAutoState(ordinary, true, 0);
            OriginalAutoPlayData configuration = configuredAutoPlay.Clone();
            bool muted = pcGameSoundsMuted;
            string undo = StateBetCleanupUndoFingerprint();
            string rng = DemoCleanupRngBytes();
            RefreshPortraitUi();
            RequireStartGameStateBet(SourcePlayInputAllowed && uiSpinButton.interactable &&
                (ordinary ? demoMenuState == 0 : demoMenuState == 3), "valid PLAY did not enable its real gate");
            PointerEventData pointer = QueueStartGameButton(uiSpinButton, 97, true);
            RequireStartGameStateBet(SourcePlayerInputs.Count == 2 && SourcePlayerInputs.DispatchOne(),
                "real PLAY did not dispatch its sampled down/up pair");
            RequireStartGameStateBet(SourcePlayerInputs.Count == 0,
                "real PLAY down did not purge its queued release: count=" + SourcePlayerInputs.Count);
            RequireStartGameStateBet(spinning && session.SpinPending &&
                session.PendingPocket == (ordinary ? 16 : 27) && session.Credit == 999 &&
                session.TotalBet == 1 && session.Rounds == 0 && pendingSpin != null &&
                StateBetCleanupUndoFingerprint() == undo,
                "real PLAY did not commit one pending round and retained Undo: spinning=" + spinning +
                " pending=" + session.SpinPending + " pocket=" + session.PendingPocket +
                " credit=" + session.Credit + " wager=" + session.TotalBet + " rounds=" + session.Rounds);
            RequireStartGameStateBet(
                sourceGameplayState == SourceGameplayState.Playing && !sourceBetStateEntryPending &&
                !sourceTableInputEnabled && (ordinary ? demoMenuState == 0 : demoMenuState == 2),
                "real PLAY retained the wrong source gate/state: state=" + (int)sourceGameplayState +
                " entry=" + sourceBetStateEntryPending + " table=" + sourceTableInputEnabled +
                " demo=" + demoMenuState);
            RequireStartGameStateBet(SameConfiguredSmokeData(configuredAutoPlay, configuration) &&
                pcGameSoundsMuted == muted, "release-only successful purge changed configured Auto or sound");
            RequireStartGameStateBet(counted.Pockets == 1 && counted.Suits == 0 &&
                demoOutcome.ArmedVisualPath == -1,
                "successful commitment changed draw count or failed to consume its visual arm: pocketDraws=" +
                counted.Pockets + " suitDraws=" + counted.Suits + " arm=" + demoOutcome.ArmedVisualPath);
            RequireStartGameStateBet(DemoCleanupRngBytes() != rng,
                "the counted production PC draw did not advance its isolated persisted RNG file");
            RequireSourcePointerReleased(pointer);
            RequireStartGameStateBet(pointer.pointerEnter == null, "successful touch retained hover capture");
            string commitment = PlayerPrefs.GetString(PendingSpinKey, "");
            string committedRng = DemoCleanupRngBytes();
            PointerEventData repeatedUp = sourceInputModule.QueuePointerSample(97,
                StartGameButtonPoint(uiSpinButton), false, true, false, true);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 0 &&
                session.Credit == 999 && session.PendingPocket == (ordinary ? 16 : 27) &&
                PlayerPrefs.GetString(PendingSpinKey, "") == commitment &&
                DemoCleanupRngBytes() == committedRng && counted.Pockets == 1 && counted.Suits == 0,
                "physical up repeated a successful PLAY commitment or draw");
            RequireSourcePointerReleased(repeatedUp);
            SaveStartGameReceipt("accept", "pending", ordinary);
            CaptureStartGameStateBet("pending_saved");
        }

        private IEnumerator StartGameStateBetResume(StartGameStateBetReceipt proof)
        {
            RequireStartGameAcceptedAutoState(proof.ordinary, true, 0);
            RequireStartGameStateBet(spinning && session.SpinPending && pendingSpin != null &&
                session.Credit == 999 && session.Rounds == 0 && session.TotalBet == 1 &&
                session.PendingPocket == (proof.ordinary ? 16 : 27) &&
                sourceGameplayState == SourceGameplayState.Playing &&
                StateBetPendingCommitFingerprint(PlayerPrefs.GetString(PendingSpinKey, "")) ==
                    StateBetPendingCommitFingerprint(proof.pending) &&
                StateBetCleanupUndoFingerprint() == proof.undo && DemoCleanupRngBytes() == proof.rng,
                "pending startup did not preserve its state, stake, Undo, commitment or RNG");
            yield return StartGameWaitForActualSettlement();
            RequireStartGameStateBet(session.Credit == (proof.ordinary ? 999 : 1035) &&
                session.Wagered == 1 && session.Returned == (proof.ordinary ? 0 : 36) &&
                session.Rounds == 1 && session.Wins == (proof.ordinary ? 0 : 1) &&
                session.TotalBet == 1 && recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                session.LastResult.Pocket == (proof.ordinary ? 16 : 27) && pendingSpin == null &&
                !session.SpinPending && sourceGameplayState == SourceGameplayState.Idle &&
                !sourceBetStateEntryPending && StateBetCleanupUndoFingerprint() == proof.undo &&
                DemoCleanupRngBytes() == proof.rng && ReadPcCreditEvent() == null,
                "actual pending resume repaid, redrew, lost Undo or failed to enter Idle");
            RequireStartGameAcceptedAutoState(proof.ordinary, false, 1);
            RequireStartGameAuPaid(proof.ordinary ? 0 : 36, "accepted round settled after resume");
            if (!proof.ordinary)
            {
                // Turbo can reach real StateEnd before the independent
                // 12-frame footer Outro and independent Paid fade finish.
                // Capture only after both live clocks complete, preserving every
                // durable value and the single already-completed End.
                string settled = StartGameDurableFingerprint();
                float deadline = Time.realtimeSinceStartup + 3f;
                sourceInputModule.enabled = true;
                try
                {
                    do
                    {
                        yield return null;
                        RequireStartGameStateBet(StartGameDurableFingerprint() == settled,
                            "waiting for the live footer Outro/Paid fade changed the completed receipt");
                        RequireStartGameAcceptedAutoState(false, false, 1);
                        RefreshPortraitUi();
                    }
                    while ((CurrentFooterMovieFrame() != null || paidFadeClock.Running) &&
                        Time.realtimeSinceStartup < deadline);
                }
                finally { sourceInputModule.enabled = false; }
                RefreshPortraitUi();
                RequireStartGameStateBet(CurrentFooterMovieFrame() == null &&
                    footerMoviePhase == FooterMoviePhase.Hidden && !uiFooterMovie.gameObject.activeSelf &&
                    !paidFadeClock.Running && PaidOpacity == 1f &&
                    uiPaid.Opacity == 1f && uiCashPaid.Opacity == 1f &&
                    uiPaid.Visible && uiPaid.Value == "36" && uiPaid.GlyphCount == 2,
                    "restored winning round did not finish its live Outro/Paid fade with visible paid 36");
            }
            SaveStartGameReceipt("accept", "settled", proof.ordinary);
            CaptureStartGameStateBet("pending_resumed");
        }

        private void RequireStartGameAcceptedAutoState(bool ordinary, bool pending, int endCount)
        {
            var expected = new OriginalAutoPlayData();
            if (!ordinary)
            {
                expected.SetOptions(pending, true, true, pending, true, 0);
                expected.SetAutoPlayOffWhenGameFinished(pending);
            }
            RequireStartGameStateBet(SameConfiguredSmokeData(configuredAutoPlay, expected) &&
                configuredAutoPlay.SecsToAutoCompleteWarning == expected.SecsToAutoCompleteWarning &&
                configuredAutoPlay.AutoCompleteShowCountdown == expected.AutoCompleteShowCountdown &&
                configuredAutoPlay.DisableWhenGameFinished == expected.DisableWhenGameFinished &&
                pcGameSoundsMuted == expected.TurboAutoPlay && !autoplay && autoRoutine == null &&
                sourceRoundEndCount == endCount,
                "accepted fixture did not retain pending Auto config or consume only its real End stop: " +
                "pending=" + pending + " ordinary=" + ordinary + " auto=" + configuredAutoPlay.AutoPlay +
                " turbo=" + configuredAutoPlay.Turbo + " off=" +
                configuredAutoPlay.AutoPlayOffWhenGameFinished + " ends=" + sourceRoundEndCount);
        }

        private IEnumerator StartGamePlayAndSettle()
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiSpinButton);
            RequireStartGameStateBet(spinning && session.SpinPending && pendingSpin != null &&
                sourceGameplayState == SourceGameplayState.Playing,
                "real PLAY did not start its fixture round");
            yield return StartGameWaitForActualSettlement();
        }

        private IEnumerator StartGameWaitForActualSettlement()
        {
            // Existing cleanup fixtures need a completed round before editing
            // the table. A win now waits for an explicit offer decision.
            yield return StateEndWaitForIdle(true);
            RequireStartGameStateBet(!spinning && !winRackActive && !ResultDisplayActive &&
                !configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending &&
                sourceRoundPhase == SourceRoundPhase.None && !session.DoubleAvailable &&
                SourcePlayerInputs.Count == 0 && !autoplay && autoRoutine == null &&
                sourceGameplayState == SourceGameplayState.Idle,
                "actual wheel/rack and live module did not finish their Idle entry");
            RefreshPortraitUi();
        }

        private IEnumerator StartGameWaitForBetEntry()
        {
            RequireStartGameStateBet(sourceBetStateEntryPending &&
                sourceGameplayState == SourceGameplayState.Bet,
                "missing deferred StateBet entry before enabling the live adapter");
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 4f;
            do { yield return null; }
            while (sourceBetStateEntryPending && Time.realtimeSinceStartup < deadline);
            sourceInputModule.enabled = false;
            RequireStartGameStateBet(!sourceBetStateEntryPending &&
                sourceGameplayState == SourceGameplayState.Bet && SourcePlayerInputs.Count == 0,
                "live adapter did not automatically service the pending StateBet entry before dispatch");
        }

        private IEnumerator StartGameDrainWithLiveModule()
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 4f;
            do { yield return null; }
            while (SourcePlayerInputs.Count != 0 && Time.realtimeSinceStartup < deadline);
            sourceInputModule.enabled = false;
            RequireStartGameStateBet(SourcePlayerInputs.Count == 0, "live adapter did not drain queued input");
        }

        private IEnumerator StartGameLiveFrames(int count)
        {
            sourceInputModule.enabled = true;
            for (int i = 0; i < count; ++i) yield return null;
            sourceInputModule.enabled = false;
        }

        private void StartGameDispatchKey(KeyCode key)
        {
            RequireStartGameStateBet(SourcePlayerInputs.Count == 0, "source button needs an empty queue");
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(key));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
            RequireStartGameStateBet(SourcePlayerInputs.DispatchOne(), "source button down missing");
            if (SourcePlayerInputs.Count != 0)
                RequireStartGameStateBet(SourcePlayerInputs.DispatchOne(), "source button up missing");
            RequireStartGameStateBet(SourcePlayerInputs.Count == 0, "source button left queued input");
        }

        private IEnumerator StartGameChooseDemo(int option)
        {
            RequireStartGameStateBet(!demoMenuVisible, "Demo selection started in an open menu");
            StartGameDispatchKey(KeyCode.F8);
            RequireStartGameStateBet(demoMenuVisible && demoMenuState == 4,
                "queued F8 did not open enabled Demo");
            yield return DemoCleanupNavigate(option / DemoOptionsPerPage);
            DemoCleanupClick(demoOptionButtons[option % DemoOptionsPerPage]);
            RequireStartGameStateBet(!demoMenuVisible && demoMenuState == 3 &&
                demoOutcome.SelectedOption == option, "real Demo choice did not complete on down");
            // Real input dispatches one event per frame. Let the closed
            // menu's next UI update restore the table's interactable gates
            // before this fixture samples a new gameplay target.
            yield return null;
            RefreshPortraitUi();
        }

        private Vector2 StartGameButtonPoint(Button target)
        {
            RequireStartGameStateBet(target != null && target.IsActive(), "real target is inactive");
            Canvas.ForceUpdateCanvases();
            return RectTransformUtility.WorldToScreenPoint(null,
                target.transform.TransformPoint(((RectTransform)target.transform).rect.center));
        }

        private PointerEventData QueueStartGameButton(Button target, int pointerId, bool touch)
        {
            Vector2 point = StartGameButtonPoint(target);
            sourceInputModule.QueuePointerSample(pointerId, point, true, false, true, touch);
            return sourceInputModule.QueuePointerSample(pointerId, point, false, true, false, touch);
        }

        private string StartGameCreditEventFingerprint()
        {
            PcCreditEvent creditEvent = ReadPcCreditEvent();
            return creditEvent == null ? "null" : JsonUtility.ToJson(creditEvent);
        }

        private string StartGameAccountingFingerprint()
        {
            return DemoCleanupEconomy() + "\n" + DemoCleanupArchive(false) + "\n" +
                StateBetCleanupUndoFingerprint() + "\n" + DemoCleanupRngBytes() + "\n" +
                PlayerPrefs.GetString(PendingSpinKey, "") + "\n" +
                PlayerPrefs.GetString(StagedBetKey, "") + "\n" +
                PlayerPrefs.GetString(SettledTableUndoKey, "") + "\n" +
                StartGameCreditEventFingerprint();
        }

        private string StartGameDurableFingerprint()
        {
            return StartGameAccountingFingerprint() + "\n" + PlayerPrefs.GetString(GameResultsClearedKey, "");
        }

        private string StartGameRestoredEconomy()
        {
            string[] fields = StateBetCleanupRestoredEconomy().Split('|');
            RequireStartGameStateBet(fields.Length == 9, "shared economy fingerprint shape changed");
            // Startup constructs a fresh RouletteSession and restores only
            // the latest playable RoundResult into its transient History.
            // Full older rounds remain in recordedRoundPages, pockets and
            // snapshots, all compared independently and without reduction.
            // Keep same-process AccountingFingerprint's full History intact.
            fields[3] = session.LastResult == null ? "" : DemoCleanupResult(session.LastResult);
            return string.Join("|", fields);
        }

        private void SaveStartGameReceipt(string testCase, string phase, bool ordinary)
        {
            var proof = new StartGameStateBetReceipt {
                testCase = testCase, phase = phase, ordinary = ordinary,
                economy = StartGameRestoredEconomy(), archive = DemoCleanupArchive(false),
                rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint(),
                pending = PlayerPrefs.GetString(PendingSpinKey, ""),
                staged = PlayerPrefs.GetString(StagedBetKey, ""),
                cleared = PlayerPrefs.GetString(GameResultsClearedKey, ""),
                settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, ""),
                creditEvent = StartGameCreditEventFingerprint(), sourceState = (int)sourceGameplayState,
                neighbor = selectedNeighbor, neighborNv = PlayerPrefs.GetInt(NeighborSelectionKey, -2),
                credit = session.Credit, wagered = session.Wagered, returned = session.Returned,
                rounds = session.Rounds
            };
            RequireStartGameStateBet(phase == "pending" ? !string.IsNullOrEmpty(proof.pending) :
                string.IsNullOrEmpty(proof.pending), "receipt does not match pending durability");
            PlayerPrefs.SetString(StartGameStateBetReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
        }

        private void RequireStartGameReceipt(StartGameStateBetReceipt proof)
        {
            var differences = new List<string>();
            string actualEconomy = DemoCleanupEconomy();
            StartGameReceiptDifference(differences, "economy", proof.economy, actualEconomy);
            if (actualEconomy != proof.economy)
            {
                string[] expected = proof.economy.Split('|');
                string[] actual = actualEconomy.Split('|');
                string[] names = { "profile", "money/counters", "lastResult", "sessionHistory",
                    "bets", "chipStacks", "evaluationOrder", "recordedPockets", "recordedSnapshots" };
                if (expected.Length == names.Length && actual.Length == names.Length)
                    for (int i = 0; i < names.Length; ++i)
                        StartGameReceiptDifference(differences, "economy." + names[i], expected[i], actual[i]);
            }
            StartGameReceiptDifference(differences, "archive", proof.archive, DemoCleanupArchive(false));
            StartGameReceiptDifference(differences, "rng", proof.rng, DemoCleanupRngBytes());
            StartGameReceiptDifference(differences, "undo", proof.undo, StateBetCleanupUndoFingerprint());
            StartGameReceiptDifference(differences, "pending", proof.pending, PlayerPrefs.GetString(PendingSpinKey, ""));
            StartGameReceiptDifference(differences, "staged", proof.staged, PlayerPrefs.GetString(StagedBetKey, ""));
            StartGameReceiptDifference(differences, "cleared", proof.cleared, PlayerPrefs.GetString(GameResultsClearedKey, ""));
            StartGameReceiptDifference(differences, "settledUndo", proof.settledUndo,
                PlayerPrefs.GetString(SettledTableUndoKey, ""));
            StartGameReceiptDifference(differences, "creditEvent", proof.creditEvent, StartGameCreditEventFingerprint());
            StartGameReceiptDifference(differences, "neighbor", proof.neighbor.ToString(), selectedNeighbor.ToString());
            StartGameReceiptDifference(differences, "neighborNv", proof.neighborNv.ToString(),
                PlayerPrefs.GetInt(NeighborSelectionKey, -2).ToString());
            RequireStartGameStateBet(differences.Count == 0,
                "fresh process receipt differs: phase=" + proof.phase + " state=" + (int)sourceGameplayState +
                " fields=" + string.Join("; ", differences.ToArray()));
        }

        private static void StartGameReceiptDifference(List<string> differences, string name,
            string expected, string actual)
        {
            if (string.Equals(expected, actual, StringComparison.Ordinal)) return;
            int expectedLength = expected == null ? -1 : expected.Length;
            int actualLength = actual == null ? -1 : actual.Length;
            int first = 0;
            if (expected != null && actual != null)
                while (first < Math.Min(expected.Length, actual.Length) && expected[first] == actual[first])
                    ++first;
            string detail = name + "[expectedLength=" + expectedLength + ",actualLength=" + actualLength +
                ",firstDifference=" + first + "]";
            if (expectedLength >= 0 && expectedLength <= 160 && actualLength >= 0 && actualLength <= 160)
                detail += " expected=" + expected + " actual=" + actual;
            differences.Add(detail);
        }

        private void CaptureStartGameStateBet(string suffix)
        {
            RefreshPortraitUi();
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStartGameStateBetCapturePrefix");
            if (at < 0) return;
            RequireStartGameStateBet(at + 1 < args.Length, "capture prefix missing");
            string path = args[at + 1] + "_" + suffix + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            CaptureActiveScreenOffscreen(path);
        }

        private void RequireStartGameAuPaid(long expectedPaid, string phase)
        {
            if (!auNationalStandardsEnabled) return;
            RequireStartGameStateBet(!spinning && !session.SpinPending && pendingSpin == null &&
                (sourceGameplayState == SourceGameplayState.Idle || sourceGameplayState == SourceGameplayState.Bet),
                "AU Idle/Bet meter assertion reached a pending/playing phase: " + phase);
            RefreshPortraitUi();
            string paid = expectedPaid.ToString();
            string cash = RouletteDenominationFormatter.ForFeatureCurrency(
                checked(expectedPaid * session.Denomination));
            RequireStartGameStateBet(DisplayedPaid == expectedPaid && uiPaid.Value == paid && uiPaid.Visible &&
                uiPaid.GlyphCount == paid.Length && uiCashPaid.Visible && uiCashPaid.Value == cash &&
                uiCashPaidBackground.gameObject.activeSelf && CurrentFooterMovieFrame() == null &&
                !uiFooterMovie.gameObject.activeSelf,
                "AU Idle/Bet paid meters differ at " + phase + ": expected=" + paid + "/" + cash +
                " displayed=" + DisplayedPaid + " paid=" + uiPaid.Value + " paidVisible=" + uiPaid.Visible +
                " cash=" + uiCashPaid.Value + " cashVisible=" + uiCashPaid.Visible +
                " cashBackground=" + uiCashPaidBackground.gameObject.activeSelf +
                " footerActive=" + uiFooterMovie.gameObject.activeSelf);
        }
    }

    // Passive observation on a real table control; it never dispatches,
    // purges, changes a wager or invokes an entry helper.
    internal sealed class RouletteStartGameReleaseProbe : MonoBehaviour, IPointerUpHandler
    {
        internal int UpCount;
        internal Vector2 LastUpPosition;
        internal Action<PointerEventData> ObservedUp;
        public void OnPointerUp(PointerEventData data)
        {
            ++UpCount;
            LastUpPosition = data.position;
            if (ObservedUp != null) ObservedUp(data);
        }
    }
}
