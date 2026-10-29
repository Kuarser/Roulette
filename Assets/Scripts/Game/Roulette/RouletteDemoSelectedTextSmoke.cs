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
        private sealed class DemoSelectedTextOutcome : IRouletteOutcomeSource, IDisposable
        {
            private readonly RouletteGame game;
            private readonly IRouletteOutcomeSource platform;
            internal int Draws, ExpectedOption, ExpectedArm;
            internal DemoSelectedTextOutcome(RouletteGame owner, IRouletteOutcomeSource inner)
            {
                game = owner;
                platform = inner;
            }
            public int DrawPocket()
            {
                game.RequireDemoSelectedText("", true, "DrawPocket entry");
                RequireDemoSelected(game.demoOutcome.SelectedOption == ExpectedOption &&
                    game.demoOutcome.ArmedVisualPath == ExpectedArm,
                    "clear-text reset the mathematical option or wheel arm before draw");
                int actual = platform.DrawPocket();
                ++Draws;
                Debug.Log("ROULETTE_DEMO_SELECTED_TEXT_DRAW: count=" + Draws +
                    " actual=" + actual + " fixture=16 index=" + ExpectedOption + " textEmpty=true");
                return 16;
            }
            public int DrawDoubleSuit()
            {
                throw new InvalidOperationException("Demo selected text diagnostic reached feature RNG");
            }
            public void Dispose()
            {
                var disposable = platform as IDisposable;
                if (disposable != null) disposable.Dispose();
            }
        }

        [Serializable]
        private sealed class DemoSelectedTextReceipt
        {
            public string testCase, economy, archive, rng, undo, pending, creditEvent;
            public long credit, wagered, returned, rounds, wins, bet;
            public int archiveCount;
        }

        private string DemoSelectedTextReceiptKey
        {
            get { return recordedHistoryKey + ".DemoSelectedTextFixture.v1"; }
        }

        private static void RequireDemoSelected(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Demo selected text: " + message);
        }

        private IEnumerator DemoSelectedTextSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteDemoSelectedTextCase");
            RequireDemoSelected(at >= 0 && at + 1 < args.Length, "missing case");
            string testCase = args[at + 1];
            bool ordinary = testCase == "ordinary";
            bool load = Array.IndexOf(args, "-rouletteDemoSelectedTextLoadSmoke") >= 0;
            RequireDemoSelected(new[] { "auto", "turbo", "ordinary" }.Contains(testCase) &&
                !(ordinary && load) && demoShowMode == (ordinary ? 0 : 1) &&
                session != null && sourceInputModule != null && demoOutcome != null &&
                !temporaryHistory && recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                !autoplay && autoRoutine == null && !session.InDoubleUp && SourcePlayerInputs.Count == 0,
                "requires isolated keys, ordinary or Demo mode, and the real input module");
            bool moduleEnabled = sourceInputModule.enabled;
            try
            {
                sourceInputModule.enabled = false;
                RequireDemoSelectedText("", !ordinary, "fresh menu object");
                if (load)
                {
                    yield return ResumeDemoSelectedText(testCase);
                }
                else
                {
                    RequireDemoSelected(!spinning && !session.SpinPending && pendingSpin == null &&
                        session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                        ReadPcCreditEvent() == null && !PlayerPrefs.HasKey(DemoSelectedTextReceiptKey) &&
                        sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                        configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlayCheckPending &&
                        ReferenceEquals(outcome, demoOutcome) && demoOutcome.SelectedOption == 0 &&
                        demoOutcome.ArmedVisualPath == -1,
                        "save requires unused keys and fresh Normal Idle state");
                    if (ordinary)
                    {
                        string before = StartGameDurableFingerprint();
                        yield return StartGameLiveFrames(2);
                        RequireDemoSelectedText("", false, "ordinary live frames");
                        RequireDemoSelected(demoMenuState == 0 && StartGameDurableFingerprint() == before,
                            "ordinary hidden label changed idle state or financial storage");
                        CaptureDemoSelectedText("ordinary_hidden");
                    }
                    else
                    {
                        var counted = new DemoSelectedTextOutcome(this, outcome);
                        demoOutcome = new DemoPocketOutcomeSource(counted);
                        outcome = demoOutcome;
                        session = new RouletteSession(session.Profile, 1000, outcome) { DoubleEnabled = true };
                        if (testCase == "auto") yield return DemoSelectedTextLifecycle(counted);
                        else
                        {
                            RefreshPortraitUi();
                            DemoCleanupClick(uiBetButtons[27]);
                            yield return StartGameWaitForBetEntry();
                        }
                        yield return ChooseDemoSelectedText(testCase == "auto" ? 1 : 2);
                        yield return CommitDemoSelectedText(testCase, counted);
                    }
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            Debug.Log((load ? "ROULETTE_DEMO_SELECTED_TEXT_LOAD_PASSED: " :
                "ROULETTE_DEMO_SELECTED_TEXT_SAVE_PASSED: ") + "case=" + testCase +
                " credit=" + session.Credit + " rounds=" + session.Rounds +
                " textEmpty=" + string.IsNullOrEmpty(demoSelectedText));
            Application.Quit();
        }

        private IEnumerator ChooseDemoSelectedText(int option)
        {
            RequireDemoSelected(!demoMenuVisible && demoMenuState == 3, "choice requires ready Demo");
            string before = StartGameAccountingFingerprint();
            int oldArm = demoOutcome.ArmedVisualPath;
            StartGameDispatchKey(KeyCode.F8);
            yield return DemoCleanupNavigate(option / DemoOptionsPerPage);
            PointerEventData held = DemoCleanupQueue(demoOptionButtons[option % DemoOptionsPerPage]);
            RequireDemoSelected(SourcePlayerInputs.DispatchOne(), "choice Down missing");
            RequireDemoSelectedText(DemoOptionName(option), true, "choice Down " + option);
            RequireDemoSelected(!demoMenuVisible && demoMenuState == 3 && demoOutcome.SelectedOption == option &&
                demoOutcome.ArmedVisualPath == (option >= 3 ? option - 3 : oldArm) &&
                StartGameAccountingFingerprint() == before,
                "choice Down changed money/history/RNG or mixed text/index/arm");
            DemoCleanupDrainRelease(held);
            RequireDemoSelectedText(DemoOptionName(option), true, "choice Up " + option);
            RequireDemoSelected(StartGameAccountingFingerprint() == before &&
                demoOutcome.SelectedOption == option &&
                demoOutcome.ArmedVisualPath == (option >= 3 ? option - 3 : oldArm),
                "choice Up repeated selection or changed its accounting");
            yield return null;
            RefreshPortraitUi();
            Debug.Log("ROULETTE_DEMO_SELECTED_TEXT_CHOICE: index=" + option + " text=" + demoSelectedText +
                " arm=" + demoOutcome.ArmedVisualPath + " onDown=true");
        }

        private IEnumerator DemoSelectedTextLifecycle(DemoSelectedTextOutcome counted)
        {
            foreach (int option in new[] { 0, 1, 2, 3, 30, 39 })
            {
                yield return ChooseDemoSelectedText(option);
                if (option == 2)
                {
                    RequireDemoSelectedGeometry();
                    CaptureDemoSelectedText("turbo_selected");
                }
            }
            yield return ChooseDemoSelectedText(30);
            string navigationProof = StartGameDurableFingerprint();
            StartGameDispatchKey(KeyCode.F8);
            yield return null;
            foreach (Button button in new[] { demoNextButton, demoNextButton, demoPreviousButton })
            {
                DemoCleanupClick(button);
                RequireDemoSelectedText("27", true, "page navigation");
                yield return null;
            }
            DemoCleanupClick(DemoCleanupDismissButton(), new Vector2(1065f, 100f));
            RequireDemoSelectedText("27", true, "outside close");
            RequireDemoSelected(StartGameDurableFingerprint() == navigationProof &&
                demoOutcome.SelectedOption == 30 && demoOutcome.ArmedVisualPath == 27,
                "navigation changed financial state or independent selection");
            yield return null;
            RefreshPortraitUi();

            // The first released wager requests StateBet; the next captured
            // move/up is purged by its actual later entry, cancelling AutoData.
            Vector2 firstPoint = StartGameButtonPoint(uiBetButtons[27]);
            Vector2 secondPoint = StartGameButtonPoint(uiBetButtons[1]);
            sourceInputModule.QueuePointerSample(111, firstPoint, true, false, true, true);
            RequireDemoSelected(SourcePlayerInputs.DispatchOne(), "first held Down missing");
            sourceInputModule.QueuePointerSample(112, secondPoint, true, false, true, true);
            RequireDemoSelected(SourcePlayerInputs.DispatchOne(), "second held Down missing");
            PointerEventData first = sourceInputModule.QueuePointerSample(111, firstPoint, false, true, false, true);
            sourceInputModule.QueuePointerSample(112, secondPoint + Vector2.right, false, false, true, true);
            PointerEventData second = sourceInputModule.QueuePointerSample(112, secondPoint, false, true, false, true);
            string entryRng = DemoCleanupRngBytes();
            RequireDemoSelected(SourcePlayerInputs.DispatchOne() && session.TotalBet == 1 &&
                sourceBetStateEntryPending && configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted,
                "released first wager did not defer StateBet with configured Turbo");
            RequireDemoSelectedText("27", true, "first wager callback");
            RequireSourcePointerReleased(first);
            yield return StartGameWaitForBetEntry();
            RequireSourcePointerReleased(second);
            RequireDemoSelectedText("27", true, "StateBet purge and Enable");
            RequireDemoSelected(session.TotalBet == 1 && session.Bets[127] == 1 &&
                configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo && !pcGameSoundsMuted &&
                demoOutcome.SelectedOption == 30 && demoOutcome.ArmedVisualPath == 27 &&
                DemoCleanupRngBytes() == entryRng && counted.Draws == 0,
                "StateBet purge failed to cancel AutoData independently of text/index/arm");
            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[1]);
            RequireDemoSelected(session.TotalBet == 2 && !sourceBetStateEntryPending,
                "same-StateBet append requested another entry");
            RequireDemoSelectedText("27", true, "same-state append");
            RefreshPortraitUi();
            DemoCleanupClick(uiUndoButton);
            RequireDemoSelected(session.TotalBet == 1 && session.Bets[127] == 1, "Undo did not remove added wager");
            RequireDemoSelectedText("27", true, "Undo");
            RefreshPortraitUi();
            DemoCleanupClick(uiClearButton);
            RequireDemoSelected(session.TotalBet == 0 && sourceGameplayState == SourceGameplayState.Bet,
                "Clear changed empty Bet state");
            RequireDemoSelectedText("27", true, "Clear");
            yield return DemoSelectedTextPays("27", 3);
            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[1]);
            RequireDemoSelected(session.TotalBet == 1 && session.Bets[101] == 1,
                "fixed-result precursor did not place its losing straight wager");
            counted.ExpectedOption = 30;
            counted.ExpectedArm = 27;
            yield return StartGamePlayAndSettle();
            RequireDemoSelectedText("", true, "fixed consumption and actual Idle Enable");
            RequireDemoSelected(counted.Draws == 1 && demoOutcome.SelectedOption == 0 &&
                demoOutcome.ArmedVisualPath == -1 && session.LastResult.Pocket == 27 &&
                session.Credit == 999 && session.Rounds == 1 && session.Wagered == 1 && session.Returned == 0,
                "actual fixed draw did not consume index/arm and settle exactly one losing round");
            CaptureDemoSelectedText("fixed_success_empty");

            yield return ChooseDemoSelectedText(30);
            RefreshPortraitUi();
            DemoCleanupClick(uiClearButton);
            RequireDemoSelected(session.TotalBet == 0 && sourceGameplayState == SourceGameplayState.Idle &&
                gameOverMessageLatched, "zero rejection requires a completed empty Idle table");
            string rejectedProof = StartGameAccountingFingerprint();
            Debug.Log("ROULETTE_DEMO_SELECTED_TEXT_CALL_LEVEL_ZERO: ordinaryUserInput=false");
            InvokeStartGameCallLevelFixture();
            RequireDemoSelectedText("27", true, "call-level zero rejection after Disable");
            RequireDemoSelected(!spinning && !session.SpinPending && pendingSpin == null &&
                demoMenuState == 2 && !sourceTableInputEnabled && !sourceBetStateEntryPending &&
                sourceGameplayState == SourceGameplayState.Idle && currentGameResultsCleared &&
                demoOutcome.SelectedOption == 30 && demoOutcome.ArmedVisualPath == 27 &&
                counted.Draws == 1 && StartGameAccountingFingerprint() == rejectedProof,
                "zero rejection cleared selection or changed money/history/RNG");
            yield return StartGameLiveFrames(2);
            RequireDemoSelectedText("27", true, "rejected live frames");
            RequireDemoSelected(demoMenuState == 2 && StartGameAccountingFingerprint() == rejectedProof,
                "rejected state silently reentered or changed accounting");
            CaptureDemoSelectedText("zero_rejected_retained");
            // This is the real completed-game Pays return path. It restores
            // the table only; a subsequent wager supplies the real Bet entry.
            yield return DemoSelectedTextPays("27", 2);
            RequireDemoSelected(sourceTableInputEnabled && demoMenuState == 2,
                "Pays return failed to restore only the completed table");
            RefreshPortraitUi();
            DemoCleanupClick(uiBetButtons[27]);
            RequireDemoSelected(session.TotalBet == 1 && sourceBetStateEntryPending,
                "post-rejection real wager did not request Bet entry");
            yield return StartGameWaitForBetEntry();
            RequireDemoSelectedText("27", true, "post-rejection Bet Enable");
            RequireDemoSelected(demoMenuState == 3 && demoOutcome.SelectedOption == 30 &&
                demoOutcome.ArmedVisualPath == 27, "Bet Enable replaced or consumed selection");
        }

        private IEnumerator DemoSelectedTextPays(string text, int menuState)
        {
            string proof = StartGameAccountingFingerprint();
            int option = demoOutcome.SelectedOption, arm = demoOutcome.ArmedVisualPath;
            yield return ConfiguredOrderUiClick(uiInfoButton);
            RequireDemoSelected(gameInfoOptionsOpen, "real INFO did not open");
            yield return ConfiguredOrderUiClick(uiGameInfoSeePays);
            RequireDemoSelected(paysPage == 0, "real SEE PAYS did not open");
            RequireDemoSelectedText(text, true, "Pays open");
            yield return ConfiguredOrderUiClick(uiPaysExit);
            RequireDemoSelectedText(text, true, "Pays exit");
            RequireDemoSelected(paysPage < 0 && demoMenuState == menuState &&
                demoOutcome.SelectedOption == option && demoOutcome.ArmedVisualPath == arm &&
                StartGameAccountingFingerprint() == proof, "Pays altered independent label or accounting");
        }

        private IEnumerator CommitDemoSelectedText(string testCase, DemoSelectedTextOutcome counted)
        {
            int option = testCase == "auto" ? 1 : 2;
            string selected = DemoOptionName(option);
            RequireDemoSelectedText(selected, true, "before actual PLAY");
            RequireDemoSelected(session.TotalBet == 1 && session.Bets.Count == 1 && session.Bets[127] == 1,
                "pending test needs one straight 27 wager");
            counted.ExpectedOption = option;
            counted.ExpectedArm = demoOutcome.ArmedVisualPath;
            int beforeDraws = counted.Draws;
            long beforeCredit = session.Credit, beforeRounds = session.Rounds;
            string rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint();
            OriginalAutoPlayData configuration = configuredAutoPlay.Clone();
            bool muted = pcGameSoundsMuted;
            RefreshPortraitUi();
            RequireDemoSelected(SourcePlayInputAllowed && uiSpinButton.interactable,
                "actual successful PLAY gate is disabled");
            var probe = uiBetButtons[1].gameObject.AddComponent<RouletteStartGameReleaseProbe>();
            bool sawEmptyAtPurge = false;
            int optionAtPurge = -1, drawsAtPurge = -1;
            probe.ObservedUp = data => {
                sawEmptyAtPurge = demoSelectedText == "" && demoSelectedTextShown &&
                    demoSelectedLabel.Value == "" && ActiveDemoSelectedGlyphs().Length == 0;
                optionAtPurge = demoOutcome.SelectedOption;
                drawsAtPurge = counted.Draws;
            };
            try
            {
                Vector2 heldPoint = StartGameButtonPoint(uiBetButtons[1]);
                sourceInputModule.QueuePointerSample(113, heldPoint, true, false, true, true);
                RequireDemoSelected(SourcePlayerInputs.DispatchOne(), "purge observer capture missing");
                PointerEventData play = QueueStartGameButton(uiSpinButton, 114, true);
                PointerEventData held = sourceInputModule.QueuePointerSample(113, heldPoint, false, true, false, true);
                RequireDemoSelected(SourcePlayerInputs.Count == 3 && SourcePlayerInputs.DispatchOne(),
                    "actual PLAY Down missing");
                RequireDemoSelectedText("", true, "successful PLAY Down");
                RequireDemoSelected(probe.UpCount == 1 && probe.LastUpPosition == Vector2.zero &&
                    sawEmptyAtPurge && optionAtPurge == option && drawsAtPurge == beforeDraws,
                    "text was not empty before the first Commit purge release and subsequent draw");
                RequireSourcePointerReleased(play);
                RequireSourcePointerReleased(held);
                RequireDemoSelected(SourcePlayerInputs.Count == 0 && spinning && session.SpinPending &&
                    pendingSpin != null && session.PendingPocket == 16 && session.Credit == beforeCredit - 1 &&
                    session.Rounds == beforeRounds && session.TotalBet == 1 &&
                    counted.Draws == beforeDraws + 1 && demoOutcome.SelectedOption == option &&
                    demoOutcome.ArmedVisualPath == -1 && StateBetCleanupUndoFingerprint() == undo &&
                    SameConfiguredSmokeData(configuredAutoPlay, configuration) && pcGameSoundsMuted == muted &&
                    DemoCleanupRngBytes() != rng && demoMenuState == 2 &&
                    sourceGameplayState == SourceGameplayState.Playing,
                    "actual PLAY reset Auto/Turbo index, lost config/Undo, or committed the wrong draw");
                string committed = StartGameAccountingFingerprint();
                PointerEventData physicalUp = sourceInputModule.QueuePointerSample(114,
                    StartGameButtonPoint(uiSpinButton), false, true, false, true);
                RequireDemoSelected(SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 0 &&
                    StartGameAccountingFingerprint() == committed && counted.Draws == beforeDraws + 1,
                    "physical Up repeated the commitment");
                RequireSourcePointerReleased(physicalUp);
                SaveDemoSelectedTextReceipt(testCase);
                CaptureDemoSelectedText("pending_empty");
            }
            finally { Destroy(probe); }
            yield break;
        }

        private void SaveDemoSelectedTextReceipt(string testCase)
        {
            var proof = new DemoSelectedTextReceipt {
                testCase = testCase, economy = StartGameRestoredEconomy(), archive = DemoCleanupArchive(false),
                rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint(),
                pending = PlayerPrefs.GetString(PendingSpinKey, ""), creditEvent = StartGameCreditEventFingerprint(),
                credit = session.Credit, wagered = session.Wagered, returned = session.Returned,
                rounds = session.Rounds, wins = session.Wins, bet = session.TotalBet,
                archiveCount = recordedRoundPages.Count
            };
            RequireDemoSelected(!string.IsNullOrEmpty(proof.pending), "receipt is not pending");
            PlayerPrefs.SetString(DemoSelectedTextReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
        }

        private IEnumerator ResumeDemoSelectedText(string testCase)
        {
            var proof = JsonUtility.FromJson<DemoSelectedTextReceipt>(
                PlayerPrefs.GetString(DemoSelectedTextReceiptKey, ""));
            RequireDemoSelected(proof != null && proof.testCase == testCase, "missing pending receipt");
            var differences = new List<string>();
            StartGameReceiptDifference(differences, "economy", proof.economy, DemoCleanupEconomy());
            StartGameReceiptDifference(differences, "archive", proof.archive, DemoCleanupArchive(false));
            StartGameReceiptDifference(differences, "rng", proof.rng, DemoCleanupRngBytes());
            StartGameReceiptDifference(differences, "undo", proof.undo, StateBetCleanupUndoFingerprint());
            StartGameReceiptDifference(differences, "creditEvent", proof.creditEvent, StartGameCreditEventFingerprint());
            StartGameReceiptDifference(differences, "pending", StateBetPendingCommitFingerprint(proof.pending),
                StateBetPendingCommitFingerprint(PlayerPrefs.GetString(PendingSpinKey, "")));
            RequireDemoSelected(differences.Count == 0 && spinning && session.SpinPending && pendingSpin != null &&
                session.PendingPocket == 16 && sourceGameplayState == SourceGameplayState.Playing,
                "fresh pending restore differs: " + string.Join("; ", differences.ToArray()));
            RequireDemoSelectedText("", true, "fresh pending process");
            yield return StartGameWaitForActualSettlement();
            RequireDemoSelectedText("", true, "resumed settlement and actual Idle Enable");
            RequireDemoSelected(session.Credit == proof.credit && session.Rounds == proof.rounds + 1 &&
                session.Wagered == proof.wagered + proof.bet && session.Returned == proof.returned &&
                session.Wins == proof.wins && session.TotalBet == proof.bet && session.LastResult.Pocket == 16 &&
                session.LastResult.Returned == 0 && recordedRoundPages.Count == proof.archiveCount + 1 &&
                recordedPockets.Count == proof.archiveCount + 1 && pendingSpin == null && !session.SpinPending &&
                sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                StateBetCleanupUndoFingerprint() == proof.undo && DemoCleanupRngBytes() == proof.rng &&
                ReadPcCreditEvent() == null && demoMenuState == 3,
                "resumed settlement redrew, changed prior money/Undo or failed to enter Idle");
            // The full older archive remains strict after the new result is prepended.
            string olderArchive = JsonUtility.ToJson(new RecordedRoundArchive {
                rounds = recordedRoundPages.Skip(1).ToArray()
            });
            RequireDemoSelected(olderArchive == proof.archive, "resumed result rewrote the older full archive");
            string settled = StartGameDurableFingerprint();
            yield return StartGameLiveFrames(2);
            RequireDemoSelectedText("", true, "later enabled idle frames");
            RequireDemoSelected(StartGameDurableFingerprint() == settled, "later idle repeated settlement");
            CaptureDemoSelectedText("resumed_empty");
        }

        private RawImage[] ActiveDemoSelectedGlyphs()
        {
            return demoSelectedLabel.Root.GetComponentsInChildren<RawImage>(true)
                .Where(glyph => glyph.gameObject.activeSelf).ToArray();
        }

        private void RequireDemoSelectedText(string expected, bool shown, string phase)
        {
            RequireDemoSelected(demoSelectedLabel != null && demoSelectedText == expected &&
                demoSelectedTextShown == shown && demoSelectedLabel.Value == expected &&
                demoSelectedLabel.Visible == shown && demoSelectedLabel.Root.parent == portraitRoot,
                "label mismatch at " + phase + ": expected='" + expected + "' text='" + demoSelectedText +
                "' shown=" + demoSelectedTextShown + " index=" + demoOutcome.SelectedOption +
                " arm=" + demoOutcome.ArmedVisualPath + " menu=" + demoMenuState);
            RawImage[] active = ActiveDemoSelectedGlyphs();
            RequireDemoSelected(active.Length == expected.Length && active.All(glyph => !glyph.raycastTarget &&
                glyph.color == Color.white), "active glyph count/tint/raycast differs at " + phase);
        }

        private void RequireDemoSelectedGeometry()
        {
            RequireDemoSelectedText("Turbo Autoplay", true, "native Turbo geometry");
            RectTransform root = demoSelectedLabel.Root;
            OriginalBitmapFont font = OriginalBitmapFont.Load("demo_menu");
            RequireDemoSelected(root.parent == portraitRoot && root.anchoredPosition == Vector2.zero &&
                root.rect.width == 301 && root.rect.height == 17 && demoSelectedLabel.SelectedFont == font &&
                font.GlyphHeight == 17 && font.MeasureWidth("Turbo Autoplay") == 131 &&
                root.GetSiblingIndex() < demoMenuRoot.transform.GetSiblingIndex(),
                "selected label anchor, font, native rectangle or menu layer differs");
            float width = 0;
            foreach (RawImage glyph in ActiveDemoSelectedGlyphs())
            {
                RequireDemoSelected(glyph.rectTransform.anchoredPosition == new Vector2(width, 0) &&
                    glyph.rectTransform.rect.height == 17 && glyph.rectTransform.localScale == Vector3.one,
                    "Turbo glyph was shifted, scaled or clipped");
                width += glyph.rectTransform.rect.width;
            }
            RequireDemoSelected(width == 131, "Turbo active glyph run does not measure 131 native pixels");
            Debug.Log("ROULETTE_DEMO_SELECTED_TEXT_GEOMETRY: x=0 y=0 width=131 height=17 white=true raycast=false");
        }

        private void CaptureDemoSelectedText(string suffix)
        {
            RefreshPortraitUi();
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteDemoSelectedTextCapturePrefix");
            RequireDemoSelected(at >= 0 && at + 1 < args.Length, "capture prefix missing");
            string path = args[at + 1] + "_" + suffix + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            CaptureActiveScreenOffscreen(path);
        }
    }
}
