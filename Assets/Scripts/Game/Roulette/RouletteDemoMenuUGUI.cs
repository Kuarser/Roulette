using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private const int DemoOptionsPerPage = 15;
        private const int DemoOptionCount = 40;
        private int demoShowMode;
        private int demoMenuPage;
        private bool demoMenuVisible;
        // DemoOptionsMenu's availability is independent of its selected
        // option and of the game's wager: 0 absent, 2 disabled, 3 ready, 4 open.
        private int demoMenuState;
        private GameObject demoMenuRoot;
        private Button[] demoOptionButtons;
        private PictureLabel[] demoOptionLabels;
        private Button demoPreviousButton, demoNextButton;
        // MenuOnSpriteLabel is independent of the open panel and selected
        // mathematical index. Normal Play has text even though its index is
        // zero; successful Play clears text without resetting that index.
        private string demoSelectedText = "";
        private bool demoSelectedTextShown;
        private PictureLabel demoSelectedLabel;

        private void BuildDemoMenuUi()
        {
            OriginalBitmapFont font = OriginalBitmapFont.Load("demo_menu");
            demoSelectedLabel = new PictureLabel(portraitRoot,
                new Rect(0, 0, 301, 17), font, "MenuOnSpriteLabel",
                nativeSize: true, horizontalAlign: -1, raycastTarget: false,
                alignToRectEdge: true, verticalAlign: -1);
            demoSelectedLabel.Set(demoSelectedText);
            demoSelectedLabel.SetVisible(demoSelectedTextShown);
            RectTransform root = UiRect(portraitRoot,
                new Rect(0, 0, 1080, 1920), "Original attendant demo options");
            demoMenuRoot = root.gameObject;
            UiPointerDownButton(root, new Rect(0, 0, 1080, 1920),
                () => HandleDemoMenuTouchDown(CloseDemoMenu))
                .gameObject.name = "Demo menu outside dismiss";
            UiImage(root, new Rect(18, 1155, 1044, 529),
                Resources.Load<Texture2D>("Original/DemoMenu/Panel"),
                "Original demo options panel");
            demoOptionButtons = new Button[DemoOptionsPerPage];
            demoOptionLabels = new PictureLabel[DemoOptionsPerPage];
            for (int i = 0; i < DemoOptionsPerPage; ++i)
            {
                int slot = i;
                Rect rect = new Rect(i % 3 == 0 ? 40 : i % 3 == 1 ? 374 : 708,
                    1180 + (i / 3) * 83, 332, 83);
                if (i == 12) rect.x = 38;
                if (i == 13) rect.y = 1512;
                if (i == 14) rect.y = 1511;
                string asset = "Original/DemoMenu/DemoOptionsButtonSelect" + (i + 1);
                Texture2D normal = Resources.Load<Texture2D>(asset + "_1");
                Texture2D pressed = Resources.Load<Texture2D>(asset + "_2");
                Button button = UiPointerDownButton(root, rect,
                    () => HandleDemoMenuTouchDown(() =>
                        SelectDemoOption(demoMenuPage * DemoOptionsPerPage + slot)),
                    normal);
                button.gameObject.name = "Demo option slot " + i;
                button.gameObject.AddComponent<RouletteTwoFrameButton>()
                    .Configure(button, normal, pressed);
                demoOptionButtons[i] = button;
                demoOptionLabels[i] = new PictureLabel(
                    button.GetComponent<RectTransform>(),
                    new Rect(8, 32, rect.width - 16, 18),
                    font, "Original demo option label");
            }
            demoPreviousButton = DemoPageButton(root,
                new Rect(286, 1595, 251, 75), "Previous", -1, font);
            demoNextButton = DemoPageButton(root,
                new Rect(540, 1595, 251, 75), "Next", 1, font);
            demoMenuRoot.SetActive(false);
        }

        private Button DemoPageButton(RectTransform root, Rect rect,
            string name, int delta, OriginalBitmapFont font)
        {
            string asset = "Original/DemoMenu/DemoOptionsButton" + name;
            Texture2D normal = Resources.Load<Texture2D>(asset + "_1");
            Texture2D pressed = Resources.Load<Texture2D>(asset + "_2");
            Button button = UiPointerDownButton(root, rect,
                () => HandleDemoMenuTouchDown(() =>
                    ShowDemoMenuPage(demoMenuPage + delta)), normal);
            button.gameObject.name = "Demo " + name;
            button.gameObject.AddComponent<RouletteTwoFrameButton>()
                .Configure(button, normal, pressed);
            new PictureLabel(button.GetComponent<RectTransform>(),
                new Rect(8, 27, rect.width - 16, 18),
                font, "Original demo " + name + " label").Set(name);
            return button;
        }

        private void ShowDemoMenuPage(int page)
        {
            if (!demoMenuVisible || page < 0 || page > 2) return;
            demoMenuPage = page;
            for (int slot = 0; slot < DemoOptionsPerPage; ++slot)
            {
                int option = page * DemoOptionsPerPage + slot;
                demoOptionButtons[slot].gameObject.SetActive(option < DemoOptionCount);
                demoOptionLabels[slot].Set(option < DemoOptionCount ?
                    DemoOptionName(option) : "");
            }
            demoPreviousButton.gameObject.SetActive(page > 0);
            demoNextButton.gameObject.SetActive(page < 2);
        }

        private static string DemoOptionName(int option)
        {
            if (option == 0) return "Normal Play";
            if (option == 1) return "Autoplay";
            if (option == 2) return "Turbo Autoplay";
            return (option - 3).ToString();
        }

        private void ToggleDemoMenu()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (demoMenuVisible) { CloseDemoMenu(); return; }
            if (demoMenuState != 3 || demoShowMode != 1 || demoMenuRoot == null ||
                SourceRoundBlocksOrdinaryInput ||
                session == null || spinning || ResultDisplayActive ||
                session.InDoubleUp || paysPage >= 0 || ideckPreview ||
                topperPreview || histogramPreview || historyView) return;
            demoMenuVisible = true;
            demoMenuState = 4;
            demoMenuRoot.transform.SetAsLastSibling();
            demoMenuRoot.SetActive(true);
            ShowDemoMenuPage(0);
            Debug.Log("ROULETTE_DEMO_MENU_OPEN: options=40 perPage=15");
        }

        private void CloseDemoMenu()
        {
            demoMenuVisible = false;
            if (demoMenuState == 4) demoMenuState = 3;
            if (demoMenuRoot != null) demoMenuRoot.SetActive(false);
        }

        private void EnableSourceDemoMode()
        {
            if (demoShowMode <= 0 || demoMenuState == 0 || demoMenuState == 3) return;
            demoMenuState = 3;
            demoSelectedTextShown = true;
            if (demoSelectedLabel != null) demoSelectedLabel.SetVisible(true);
        }

        private void SetSourceSelectedDemoText(string value)
        {
            if (demoMenuState == 0) return;
            demoSelectedText = value ?? "";
            if (demoSelectedLabel != null) demoSelectedLabel.Set(demoSelectedText);
        }

        private void ClearSourceSelectedDemoText()
        {
            SetSourceSelectedDemoText("");
        }

        private void DisableSourceDemoMode()
        {
            if (demoMenuState == 0 || demoMenuState == 2) return;
            CloseDemoMenu();
            demoMenuState = 2;
            // DisableDemoMode does not reset the mathematical option,
            // selected label, or the wheel's separately armed visual path.
        }

        private void HandleDemoMenuTouchDown(Action action)
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!demoMenuVisible || demoShowMode != 1 || session == null) return;
            // DemoOptionsMenu::HandleDemoEvent accepts only 0x200001. Before
            // dispatching any menu button it adds FileDefs::DemoOptionsCredits
            // when the credit meter is exactly zero (not merely below 2000).
            // The PC uses its existing isolated/recoverable credit adapter.
            // A fractional credit is nonzero in the original double meter.
            // Test the full PC money state so sub-credit remainder is kept.
            RouletteSession local = session as RouletteSession;
            if (local != null && local.MoneyCents == 0)
            {
                AddPcVirtualCredits(2000, true);
                SaveStagedBetsIfChanged();
            }
            action();
        }

        private void SelectDemoOption(int option)
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!demoMenuVisible || option < 0 || option >= DemoOptionCount) return;
            if (demoOutcome == null) return;
            // HandleDemoEvent stores the menu index before the game callback.
            demoOutcome.SelectOption(option, false);
            SetSourceSelectedDemoText(DemoOptionName(option));
            // Roulette::DemoOptionSelected clears the live result before all
            // forty branches. The settled history and its money are separate
            // from the original current-result NV object.
            ApplyClearedGameResultsPresentation(true);
            SaveClearedGameResults();
            if (option == 1 || option == 2)
            {
                configuredAutoPlay.SetOptions(true, true, true, option == 2, true, 0);
                SetPcGameSoundsMuted(option == 2);
                PurgeSourcePlayerInputs();
            }
            // Deluxe arms the wheel only after the base result/configuration
            // callback completes. A failed base action retains the old arm.
            if (option >= 3) demoOutcome.ArmSelectedVisualPath();
            CloseDemoMenu();
            // Normal has no AutoData/sound branch in Roulette::DemoOptionSelected.
            // Every choice replaces the mathematical menu index. Only 3..39
            // arm a visual path; 0..2 retain any previously armed wheel path.
            Debug.Log("ROULETTE_DEMO_OPTION_SELECTED: index=" + option +
                      " name=" + DemoOptionName(option));
        }

        private void ReplenishDemoAutoplayCredits()
        {
            // FileDefs::DemoOptionsCredits is 2000 in both packaged entries.
            // Game::CheckAutoPlay compares credits with this limit, then
            // AddDemoCredits adds another 2000 denomination units.
            const long demoCredits = 2000;
            RouletteSession local = session as RouletteSession;
            if (!ConfiguredAutoPlayEligible || !configuredAutoPlay.ReplenishCredits || local == null ||
                session.Credit >= demoCredits) return;
            AddPcVirtualCredits(demoCredits, true);
            SaveStagedBetsIfChanged();
            Debug.Log("ROULETTE_DEMO_CREDITS: added=" + demoCredits +
                      " credit=" + session.Credit);
        }

        private IEnumerator DemoMenuSmoke()
        {
            yield return null;
            ToggleDemoMenu();
            yield return null;
            if (!demoMenuVisible || demoMenuPage != 0 ||
                demoOptionLabels[0].Value != "Normal Play")
                throw new InvalidOperationException("Original demo menu did not open");
            ClickThroughUiRaycast(demoNextButton);
            yield return null;
            if (demoMenuPage != 1 || demoOptionLabels[0].Value != "12")
                throw new InvalidOperationException("Demo page 2 differs from source");
            ClickThroughUiRaycast(demoNextButton);
            yield return null;
            if (demoMenuPage != 2 || demoOptionLabels[0].Value != "27" ||
                demoOptionButtons[10].gameObject.activeSelf)
                throw new InvalidOperationException("Demo page 3 differs from source");
            ClickThroughUiRaycast(demoOptionButtons[0]);
            if (demoMenuVisible || demoOutcome.SelectedPocket != 27)
                throw new InvalidOperationException("Demo stop selection failed");
            BeginSpin();
            yield return new WaitUntil(() => spinning);
            yield return new WaitUntil(() => !spinning);
            if (session.LastResult == null || session.LastResult.Pocket != 27 ||
                session.LastResult.Returned != 36 || session.Credit != 1035 ||
                demoOutcome.SelectedPocket != -1)
                throw new InvalidOperationException("Demo menu result did not settle");
            Debug.Log("ROULETTE_DEMO_MENU_PASSED: page=3 pocket=27 " +
                      "returned=36 credit=1035 cleared=true");
            Application.Quit();
        }

        private IEnumerator DemoAutoCreditsSmoke()
        {
            yield return null;
            if (session == null || session.Credit != 1000 || session.Rounds != 0 || session.TotalBet != 0 ||
                autoplay || autoRoutine != null || spinning ||
                SourcePlayerInputs.Count != 0 ||
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0)
                throw new InvalidOperationException("Demo credits diagnostic needs a fresh isolated idle session");
            OriginalAutoPlayData savedConfig = configuredAutoPlay.Clone();
            bool savedPending = configuredAutoPlayCheckPending;
            bool savedTableEnabled = sourceTableInputEnabled;
            bool savedInputEnabled = sourceInputModule != null && sourceInputModule.enabled;
            bool savedMuted = pcGameSoundsMuted;
            try
            {
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayCheckPending = false;
                if (sourceInputModule != null) sourceInputModule.enabled = false;
                string before = PaytableTestLiveFingerprint(session);
                ToggleDemoMenu();
                yield return null;
                ClickThroughUiRaycast(demoOptionButtons[1]);
                yield return new WaitForSecondsRealtime(.25f);
                if (autoplay || autoRoutine != null || spinning ||
                    configuredAutoPlay.AutoPlay != 1 || !configuredAutoPlay.ReplenishCredits ||
                    !configuredAutoPlay.PlayerInput || !configuredAutoPlay.DemoOnly ||
                    configuredAutoPlay.Turbo || pcGameSoundsMuted ||
                    PaytableTestLiveFingerprint(session) != before)
                    throw new InvalidOperationException("Menu Auto changed play/funds instead of configuring AutoData");
                ToggleDemoMenu();
                yield return null;
                ClickThroughUiRaycast(demoOptionButtons[0]);
                if (autoplay || configuredAutoPlay.AutoPlay != 1 ||
                    !configuredAutoPlay.ReplenishCredits || !configuredAutoPlay.PlayerInput ||
                    !configuredAutoPlay.DemoOnly || configuredAutoPlay.Turbo ||
                    pcGameSoundsMuted || PaytableTestLiveFingerprint(session) != before)
                    throw new InvalidOperationException("Normal Play changed configured AutoData or funds");
                // Explicit RunLoop case-1 entry fixture. The menu callback
                // does not schedule this boundary; the configured check funds
                // before BeginSpin rejects the fixture's empty wager.
                configuredAutoPlayCheckPending = true;
                ServiceConfiguredAutoPlay();
                PcCreditEvent deposit = ReadPcCreditEvent();
                if (configuredAutoPlayCheckPending || autoplay || autoRoutine != null || spinning ||
                    session.Credit != 3000 || session.Rounds != 0 || deposit == null ||
                    deposit.kind != "deposit" || deposit.before != 1000 || deposit.after != 3000 ||
                    deposit.phase != 3 || configuredAutoPlay.AutoPlay != 1 ||
                    !configuredAutoPlay.ReplenishCredits || sourceTableInputEnabled ||
                    TableTouchInputEnabled())
                    throw new InvalidOperationException("Configured entry replenishment or empty-wager rejection differs");
                RefreshPortraitUi();
                ClickThroughUiRaycast(uiZeroBetButton);
                if (session.TotalBet != 0)
                    throw new InvalidOperationException("Rejected configured DoPlay reopened the table");
                string depositId = deposit.id;
                configuredAutoPlayCheckPending = true;
                SourcePlayerInputs.Enqueue(0x200001);
                ServiceConfiguredAutoPlay();
                if (configuredAutoPlay.AutoPlay != 0 || !sourceTableInputEnabled ||
                    !TableTouchInputEnabled() || session.Credit != 3000 || session.Rounds != 0 ||
                    ReadPcCreditEvent().id != depositId)
                    throw new InvalidOperationException("Queued reset at next state entry did not recover table without funding");
            }
            finally
            {
                CloseDemoMenu();
                configuredAutoPlay = savedConfig;
                configuredAutoPlayCheckPending = savedPending;
                sourceTableInputEnabled = savedTableEnabled;
                if (sourceInputModule != null) sourceInputModule.enabled = savedInputEnabled;
                SetPcGameSoundsMuted(savedMuted);
            }
            Debug.Log("ROULETTE_DEMO_AUTO_CREDITS_PASSED: " +
                      "menuCredit=1000 NormalPreservesConfig=true explicitCase1EntryAdded=2000 credit=3000 rounds=0 activeAuto=false journal=settled rejectedTableLocked=true nextEntryResetUnlocks=true");
            Application.Quit();
        }
    }
}
