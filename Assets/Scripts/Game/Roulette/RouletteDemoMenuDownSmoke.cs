using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private IEnumerator DemoMenuDownSmoke()
        {
            yield return null;
            if (demoShowMode != 1 || demoMenuRoot == null || demoOutcome == null ||
                autoplay || autoRoutine != null || spinning || session == null ||
                session.Credit != 1000 || recordedRoundPages.Count != 0 ||
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcPlayerVolumeKey == DefaultPcPlayerVolumeKey || SourcePlayerInputs.Count != 0)
                throw new InvalidOperationException(
                    "Demo down diagnostic needs fresh isolated history/volume and idle demo mode");

            IRouletteSession saved = session;
            string savedFingerprint = PaytableTestLiveFingerprint(saved);
            OriginalAutoPlayData savedConfig = configuredAutoPlay.Clone();
            bool savedPending = configuredAutoPlayCheckPending;
            bool savedInputEnabled = sourceInputModule != null && sourceInputModule.enabled;
            bool savedMuted = pcGameSoundsMuted;
            DemoPocketOutcomeSource savedDemo = demoOutcome;
            int checks = 0;
            try
            {
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayCheckPending = false;
                if (sourceInputModule != null) sourceInputModule.enabled = false;
                demoOutcome = new DemoPocketOutcomeSource(new PaytableTestForbiddenOutcome());
                session = new RouletteSession(saved.Profile, 2000,
                    new PaytableTestForbiddenOutcome());
                string before = PaytableTestLiveFingerprint(session);
                ToggleDemoMenu();
                yield return null;
                CheckDemoNavigationDown(demoNextButton, 1, "12", ref checks);
                yield return null;
                CheckDemoNavigationDown(demoNextButton, 2, "27", ref checks);
                yield return null;
                CheckDemoNavigationDown(demoPreviousButton, 1, "12", ref checks);
                yield return null;
                CheckDemoNavigationDown(demoPreviousButton, 0, "Normal Play", ref checks);
                if (demoPreviousButton.gameObject.activeSelf ||
                    !demoNextButton.gameObject.activeSelf)
                    throw new InvalidOperationException("First demo page navigation bounds differ");
                ++checks;
                CloseDemoMenu();

                int fractionalDenomination = 0;
                foreach (int denomination in session.Profile.Denominations)
                    if (denomination > 1) { fractionalDenomination = denomination; break; }
                if (fractionalDenomination == 0)
                    throw new InvalidOperationException("Demo fractional-credit fixture needs a denomination above one");
                var fixture = (RouletteSession)session;
                fixture.ApplyPlatformMoneyState(0, fractionalDenomination, 1);
                ToggleDemoMenu();
                yield return null;
                PointerEventData fractionalPress = PressThroughUiRaycast(demoNextButton);
                ReleaseUiPointer(demoNextButton, fractionalPress);
                if (fixture.Credit != 0 || fixture.MoneyCents != 1 ||
                    demoMenuPage != 1 || ReadPcCreditEvent() != null)
                    throw new InvalidOperationException("Fractional credits were mistaken for a zero meter");
                ++checks;
                CloseDemoMenu();
                fixture.ApplyPlatformMoneyState(2000, 1, 0);

                // Each choice commits on down without starting a round. Normal
                // and fixed choices preserve configured AutoData and sound.
                for (int option = 0; option < DemoOptionCount; ++option)
                {
                    configuredAutoPlay.SetOptions(true, true, true, true, true, 0);
                    SetPcGameSoundsMuted(true);
                    demoOutcome.SelectFixedPocket(36);
                    ToggleDemoMenu();
                    ShowDemoMenuPage(option / DemoOptionsPerPage);
                    // Allow the newly active page's Graphic to register with
                    // Unity's raycaster before the next real pointer event.
                    yield return null;
                    Button button = demoOptionButtons[option % DemoOptionsPerPage];
                    PointerEventData held = PressThroughUiRaycast(button);
                    int expectedPocket = option >= 3 ? option - 3 : -1;
                    int expectedPath = option >= 3 ? option - 3 : 36;
                    bool expectedTurbo = option != 1;
                    if (demoMenuVisible || demoOutcome.SelectedPocket != expectedPocket ||
                        demoOutcome.SelectedOption != option || demoOutcome.ArmedVisualPath != expectedPath ||
                        autoplay || autoRoutine != null || configuredAutoPlay.AutoPlay != 1 ||
                        !configuredAutoPlay.ReplenishCredits || !configuredAutoPlay.PlayerInput ||
                        !configuredAutoPlay.DemoOnly || configuredAutoPlay.Turbo != expectedTurbo ||
                        pcGameSoundsMuted != expectedTurbo)
                        throw new InvalidOperationException("Demo choice was not applied on down: " + option);
                    ++checks;
                    ExecuteEvents.Execute(button.gameObject, held, ExecuteEvents.dragHandler);
                    ReleaseUiPointer(button, held);
                    if (demoMenuVisible || demoOutcome.SelectedPocket != expectedPocket ||
                        demoOutcome.SelectedOption != option || demoOutcome.ArmedVisualPath != expectedPath || autoplay)
                        throw new InvalidOperationException("Demo move/release repeated a choice: " + option);
                    ++checks;
                }
                if (PaytableTestLiveFingerprint(session) != before || session.Rounds != 0)
                    throw new InvalidOperationException("Demo choices changed funds/history/RNG");
            }
            finally
            {
                if (autoRoutine != null) StopCoroutine(autoRoutine);
                autoRoutine = null; autoplay = false; CloseDemoMenu();
                session = saved; configuredAutoPlay = savedConfig;
                configuredAutoPlayCheckPending = savedPending;
                if (sourceInputModule != null) sourceInputModule.enabled = savedInputEnabled;
                demoOutcome = savedDemo;
                SetPcGameSoundsMuted(savedMuted); RefreshPortraitUi();
            }
            if (PaytableTestLiveFingerprint(saved) != savedFingerprint)
                throw new InvalidOperationException("Demo down diagnostic altered the saved session");

            // This second fixture deliberately performs transactions in its
            // guarded .Test. session. Use the real PC COLLECT/deposit journal,
            // including the collect-armed volume timer and zero-credit state.
            RequestPcCollect();
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame) yield return null;
            if (pcCollectPending || session.Credit != 0 || pcPlayerVolumeIdleDeadlineTimestamp == 0)
                throw new InvalidOperationException("Demo zero-credit COLLECT fixture did not settle");
            ToggleDemoMenu();
            yield return null;
            Button next = demoNextButton;
            var right = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Right };
            ExecuteEvents.Execute(next.gameObject, right, ExecuteEvents.pointerDownHandler);
            if (session.Credit != 0 || demoMenuPage != 0)
                throw new InvalidOperationException("Right button injected demo credits");
            ++checks;
            var move = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(next.gameObject, move, ExecuteEvents.dragHandler);
            if (session.Credit != 0 || demoMenuPage != 0)
                throw new InvalidOperationException("Menu movement injected demo credits");
            ++checks;
            PointerEventData zeroPress = PressThroughUiRaycast(next);
            PcCreditEvent deposit = ReadPcCreditEvent();
            if (session.Credit != 2000 || demoMenuPage != 1 ||
                pcPlayerVolumeIdleDeadlineTimestamp != 0 || deposit == null ||
                deposit.kind != "deposit" || deposit.before != 0 || deposit.after != 2000 ||
                deposit.phase != 3 || session.Rounds != 0)
                throw new InvalidOperationException("Zero-credit menu down missed its source deposit");
            ++checks;
            string depositId = deposit.id;
            ExecuteEvents.Execute(next.gameObject, zeroPress, ExecuteEvents.dragHandler);
            ReleaseUiPointer(next, zeroPress);
            if (session.Credit != 2000 || demoMenuPage != 1 || ReadPcCreditEvent().id != depositId)
                throw new InvalidOperationException("Menu release repeated the demo deposit/page change");
            ++checks;
            yield return null;
            CheckDemoNavigationDown(demoPreviousButton, 0, "Normal Play", ref checks);
            if (session.Credit != 2000 || ReadPcCreditEvent().id != depositId)
                throw new InvalidOperationException("Nonzero-credit menu down added credits");
            ++checks;
            CloseDemoMenu();
            foreach (int option in new[] { 0, 1, 2, 3 })
            {
                RequestPcCollect();
                for (int frame = 0; frame < 6 && pcCollectPending; ++frame) yield return null;
                if (pcCollectPending || session.Credit != 0)
                    throw new InvalidOperationException("Zero-credit demo choice fixture did not collect");
                ToggleDemoMenu();
                yield return null;
                Button button = demoOptionButtons[option];
                PointerEventData held = PressThroughUiRaycast(button);
                deposit = ReadPcCreditEvent();
                if (demoMenuVisible || session.Credit != 2000 ||
                    pcPlayerVolumeIdleDeadlineTimestamp != 0 || deposit == null ||
                    deposit.before != 0 || deposit.after != 2000 || deposit.phase != 3)
                    throw new InvalidOperationException("Demo choice was dispatched before zero-credit deposit: " + option);
                ++checks;
                depositId = deposit.id;
                ReleaseUiPointer(button, held);
                if (session.Credit != 2000 || ReadPcCreditEvent().id != depositId || session.Rounds != 0)
                    throw new InvalidOperationException("Demo choice release repeated the zero-credit deposit");
                ++checks;
            }
            Debug.Log("ROULETTE_DEMO_MENU_DOWN_PASSED: " + checks +
                " down/move/release checks; all 40 choices; first/last page; right/move ignored; " +
                "fractional credits preserved; zero-credit COLLECT then one 2000-credit deposit; " +
                "journal/timer; choice funds/history/RNG isolation");
            Application.Quit();
        }

        private void CheckDemoNavigationDown(Button button, int page, string label, ref int checks)
        {
            PointerEventData held = PressThroughUiRaycast(button);
            if (!demoMenuVisible || demoMenuPage != page || demoOptionLabels[0].Value != label)
                throw new InvalidOperationException("Demo navigation waited for release");
            if (page == 2 && (demoNextButton.gameObject.activeSelf ||
                !demoPreviousButton.gameObject.activeSelf || demoOptionButtons[10].gameObject.activeSelf))
                throw new InvalidOperationException("Last demo page navigation/options bounds differ");
            ++checks;
            ExecuteEvents.Execute(button.gameObject, held, ExecuteEvents.dragHandler);
            ReleaseUiPointer(button, held);
            if (!demoMenuVisible || demoMenuPage != page || demoOptionLabels[0].Value != label)
                throw new InvalidOperationException("Demo navigation repeated on move/release");
            ++checks;
        }
    }
}
