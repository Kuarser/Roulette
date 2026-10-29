using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class PaytableTestLayout
        {
            public OriginalRect[] buttons;
            public string[] buttonLabels;
            public OriginalRect messageWindow;
            public OriginalPoint[] messages;
            public string totalBetLabel, payLabel;
            public int[] wheelStops;
            public double wheelRadius, framesPerStop;
            public OriginalPoint wheelCenter;
        }

        private PaytableTestLayout paytableTestLayout;
        private RoulettePaytableTest paytableTest;
        private RectTransform paytableTestOverlay, paytableTestScreen, paytableTestChips;
        private RawImage paytableTestWheel, paytableTestSpindle, paytableTestBall, paytableTestOdds;
        private RawImage paytableTestMarker, paytableTestPays, paytableTestPaysLines;
        private Button[] paytableTestButtons;
        private PictureLabel[] paytableTestButtonLabels, paytableTestMessages;
        private RouletteOriginalButtonFrames[] paytableTestChipStates;
        private Button paytableTestClear;
        private RouletteOriginalButtonFrames paytableTestClearFrames;
        private OriginalBitmapFont paytableTestFont;
        private int paytableTestPaysPage = -1, paytableTestProfileIndex;
        private bool paytableTestTotalWritten;
        private long paytableTestCycleStarted;
        private long paytableTestPaysDeadlineTimestamp;
        private long paytableTestStackSignature = long.MinValue;
        private bool PaytableTestActive { get { return paytableTest != null; } }

        // Explicit PC operator adapter. Source cabinet IPC and screen mappings
        // remain separate; no cabinet configuration is inferred from this key.
        public bool RequestPcPaytableTest(string profileSuffix, int totalLimit,
            int spotLimit, long winLimit, Rect normalizedArea)
        {
            if (!CanEnterPcOperatorPage()) return false;
            int selectedProfile = Array.IndexOf(profiles, profileSuffix);
            if (selectedProfile < 0) throw new ArgumentException("Unknown test paytable");
            if (normalizedArea.width <= 0 || normalizedArea.height <= 0 ||
                normalizedArea.xMin < 0 || normalizedArea.yMin < 0 ||
                normalizedArea.xMax > 1 || normalizedArea.yMax > 1 ||
                float.IsNaN(normalizedArea.x) || float.IsNaN(normalizedArea.y) ||
                float.IsNaN(normalizedArea.width) || float.IsNaN(normalizedArea.height))
                throw new ArgumentException("Invalid PC operator display rectangle");
            if (paytableTestOverlay == null) BuildPaytableTestUi();
            // Load a fresh profile for diagnostics instead of sharing mutable
            // records with the live session or replacing the live session.
            var state = new RoulettePaytableTest(RouletteProfile.Load(profileSuffix),
                paytableTestLayout.wheelStops, totalLimit, spotLimit, winLimit,
                paytableTestLayout.framesPerStop);
            paytableTest = state;
            paytableTestProfileIndex = selectedProfile;
            paytableTestPaysPage = -1;
            paytableTestTotalWritten = false;
            paytableTestCycleStarted = 0;
            paytableTestPaysDeadlineTimestamp = 0;
            paytableTestStackSignature = long.MinValue;
            foreach (PictureLabel label in paytableTestMessages) label.Set("");
            paytableTestMarker.gameObject.SetActive(false);
            paytableTestScreen.anchoredPosition = new Vector2(
                normalizedArea.x * 1080, -normalizedArea.y * 1920);
            paytableTestScreen.localScale = new Vector3(
                normalizedArea.width, normalizedArea.height, 1);
            paytableTestOverlay.SetAsLastSibling();
            paytableTestOverlay.gameObject.SetActive(true);
            RefreshPaytableTestUi();
            Debug.Log("ROULETTE_PC_PAYTABLE_TEST_OPEN: profile=" + profileSuffix +
                " totalLimit=" + totalLimit + " spotLimit=" + spotLimit +
                " winLimit=" + winLimit + " pocket=" + state.Pocket);
            return true;
        }

        public void ClosePcPaytableTest()
        {
            if (!PaytableTestActive) return;
            paytableTest = null;
            paytableTestOverlay.gameObject.SetActive(false);
            RefreshPortraitUi();
            Debug.Log("ROULETTE_PC_PAYTABLE_TEST_CLOSED");
        }

        private void TogglePcPaytableTest()
        {
            if (PaytableTestActive) { ClosePcPaytableTest(); return; }
            if (session == null) return;
            RequestPcPaytableTest(profiles[profileIndex],
                session.Profile.MaximumTotalBet, session.Profile.MaximumSpotBet,
                session.BaseMaximumWin, new Rect(0, 0, 1, 1));
        }

        private RawImage TestRotatingImage(Rect bounds, Texture2D texture, string name)
        {
            RawImage image = UiImage(paytableTestScreen, bounds, texture, name);
            // SSpriteImage::Draw rotates around integer half dimensions in
            // the source's y-down coordinates, including odd-sized images.
            SetSourceRotationPivot(image.rectTransform, bounds);
            return image;
        }

        private static void SetSourceRotationPivot(RectTransform rect, Rect bounds)
        {
            rect.pivot = new Vector2(
                Mathf.Floor(bounds.width*.5f)/bounds.width,
                1f-Mathf.Floor(bounds.height*.5f)/bounds.height);
            rect.anchoredPosition = new Vector2(
                bounds.x + Mathf.Floor(bounds.width*.5f),
                -bounds.y-Mathf.Floor(bounds.height*.5f));
        }

        private void BuildPaytableTestUi()
        {
            TextAsset definition = Resources.Load<TextAsset>("Original/PaytableTest/layout");
            if (definition == null) throw new InvalidOperationException("Missing Paytable Test layout");
            paytableTestLayout = JsonUtility.FromJson<PaytableTestLayout>(definition.text);
            paytableTestFont = OriginalBitmapFont.Load("paytable_test");
            var frames = Enumerable.Range(1, 4).Select(i => Resources.Load<Texture2D>(
                "Original/PaytableTest/Button_" + i)).ToArray();
            if (paytableTestFont == null || frames.Any(f => f == null || f.width != 180 || f.height != 40))
                throw new InvalidOperationException("Missing original Paytable Test artwork");
            paytableTestOverlay = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920),
                "PC operator Paytable Test event ownership");
            Button backdrop = UiButton(paytableTestOverlay, new Rect(0, 0, 1080, 1920), () => { });
            backdrop.targetGraphic.color = Color.black;
            paytableTestScreen = UiRect(paytableTestOverlay, new Rect(0, 0, 1080, 1920),
                "Source Paytable Test Screen 1");
            UiImage(paytableTestScreen, new Rect(0, 0, 1080, 1920),
                originalBackground, "Test background");
            paytableTestWheel = TestRotatingImage(originalLayout.wheel_main.ToRect(), wheel, "Test wheel");
            UiImage(paytableTestScreen, new Rect(200, 112, 544, 543), wheelShine, "Test wheel shine");
            paytableTestBall = UiImage(paytableTestScreen, originalLayout.wheel_ball.ToRect(), ball, "Test ball");
            paytableTestSpindle = TestRotatingImage(originalLayout.wheel_spindle.ToRect(), spindle, "Test spindle");
            UiImage(paytableTestScreen, new Rect(145, 41, 657, 231), wheelShadow, "Test wheel shadow");
            paytableTestOdds = UiImage(paytableTestScreen, new Rect(13, 665, 176, 166),
                originalOddsPanels[0], "Test odds panel");

            for (int i = originalBetHitOrder.Length - 1; i >= 0; --i)
            {
                int spot = originalBetHitOrder[i];
                Button button = UiButton(paytableTestScreen,
                    originalLayout.bet_spots[spot].ToRect(), () => { });
                button.gameObject.name = "Test bet spot " + spot;
                button.gameObject.AddComponent<RouletteTableRelease>().Configure(button, spot,
                    () => PlacePaytableTestBet(100 + spot), HidePaytableTestMarker,
                    () => ShowPaytableTestMarker(spot));
                var hover = button.gameObject.AddComponent<RouletteNeighborHover>();
                hover.OnEnter = () => ShowPaytableTestMarker(spot);
                hover.OnExit = pointer => HidePaytableTestMarker();
                hover.OnDragMove = pointer => MovePaytableTestMarker(pointer);
            }
            paytableTestChips = UiRect(paytableTestScreen, new Rect(0, 0, 1080, 1920), "Test wager/result chips");
            paytableTestMarker = UiImage(paytableTestScreen, new Rect(0, 0, 90, 90),
                originalMainChipButtonFrames[0][0], "Test BetMarker");
            paytableTestChipStates = new RouletteOriginalButtonFrames[4];
            for (int i = 0; i < 4; ++i)
            {
                int value = (int)chips[i];
                Button button = UiPointerDownButton(paytableTestScreen,
                    originalLayout.chip_buttons[i].ToRect(), () => {
                        if (!PaytableTestActive || paytableTestPaysPage >= 0) return;
                        paytableTest.SelectChip(value);
                        RefreshPaytableTestUi();
                    }, originalMainChipButtonFrames[i][0]);
                button.gameObject.name = "Test chip " + value;
                paytableTestChipStates[i] = button.gameObject.AddComponent<RouletteOriginalButtonFrames>();
                paytableTestChipStates[i].Configure(button, originalMainChipButtonFrames[i], false, true, false);
            }
            Button undo = UiButton(paytableTestScreen, originalLayout.undo_button.ToRect(), () => { }, mainUndoFrames[3]);
            undo.gameObject.name = "Test Undo (source callback never enables it)";
            undo.interactable = false;
            paytableTestClear = UiButton(paytableTestScreen, originalLayout.clear_button.ToRect(), () => {
                if (!PaytableTestActive || paytableTestPaysPage >= 0) return;
                paytableTest.Clear();
                paytableTestTotalWritten = true;
                RefreshPaytableTestUi();
            }, mainClearFrames[3]);
            paytableTestClear.gameObject.name = "Test Clear";
            paytableTestClearFrames = paytableTestClear.gameObject.AddComponent<RouletteOriginalButtonFrames>();
            paytableTestClearFrames.Configure(paytableTestClear, mainClearFrames,
                showHoverFrame: false, pressFrameIndex: 1);

            paytableTestPays = UiImage(paytableTestScreen, new Rect(0, 748, 1080, 1054), paysPages[0], "Test Pays page");
            paytableTestPaysLines = UiImage(paytableTestScreen, new Rect(30, 777, 997, 1004), paysLinePages[0], "Test Pays lines");
            RectTransform messages = UiRect(paytableTestScreen, paytableTestLayout.messageWindow.ToRect(), "Test Message Box");
            RawImage background = UiImage(messages, new Rect(0, 0, messages.rect.width, messages.rect.height),
                Texture2D.whiteTexture, "Black test message window");
            background.color = Color.black;
            paytableTestMessages = new PictureLabel[5];
            for (int i = 0; i < paytableTestMessages.Length; ++i)
            {
                OriginalPoint position = paytableTestLayout.messages[i];
                paytableTestMessages[i] = new PictureLabel(messages,
                    new Rect(position.x, position.y, 1080, paytableTestFont.Height), paytableTestFont,
                    "Test message " + i, horizontalAlign: -1, alignToRectEdge: true,
                    fontCandidates: new[] { paytableTestFont }, verticalAlign: -1);
            }
            paytableTestButtons = new Button[6];
            paytableTestButtonLabels = new PictureLabel[6];
            for (int i = 0; i < 6; ++i)
            {
                int index = i;
                Button button = UiButton(paytableTestScreen,
                    paytableTestLayout.buttons[i].ToRect(), () => { }, frames[0]);
                button.gameObject.name = "PaytableTestButton-" + i;
                button.gameObject.AddComponent<CanvasGroup>();
                var state = button.gameObject.AddComponent<RouletteOriginalButtonFrames>();
                state.Configure(button, frames, showHoverFrame: false, pressFrameIndex: 1, selectedFrameIndex: 2);
                var label = new PictureLabel(button.GetComponent<RectTransform>(), new Rect(0, 0, 180, 40),
                    paytableTestFont, "Test button label " + i, fontCandidates: new[] { paytableTestFont });
                label.Set(paytableTestLayout.buttonLabels[i]);
                state.BindDepressedLabel(label.Root);
                button.gameObject.AddComponent<RouletteCapturedMainTouchAction>()
                    .Configure(button, () => PressPaytableTestButton(index));
                paytableTestButtons[i] = button;
                paytableTestButtonLabels[i] = label;
            }
            paytableTestOverlay.gameObject.SetActive(false);
        }

        private void PressPaytableTestButton(int index)
        {
            if (!PaytableTestActive || index < 0 || index >= 6 ||
                !paytableTestButtons[index].IsActive() || !paytableTestButtons[index].interactable) return;
            paytableTest.ClearEvaluation();
            switch (index)
            {
                case 0: paytableTestPaysPage = 0; ResetPaytableTestPaysTimeout(); break;
                case 1: paytableTest.Evaluate(); paytableTestCycleStarted = System.Diagnostics.Stopwatch.GetTimestamp(); break;
                case 2: PlayPaysClick(); paytableTestPaysPage = -1; paytableTestPaysDeadlineTimestamp = 0; break;
                case 3:
                    if (paytableTestPaysPage > 0) { PlayPaysClick(); --paytableTestPaysPage; ResetPaytableTestPaysTimeout(); }
                    break;
                case 4:
                    if (paytableTestPaysPage < 3) { PlayPaysClick(); ++paytableTestPaysPage; ResetPaytableTestPaysTimeout(); }
                    break;
                case 5: paytableTest.Nudge(); break;
            }
            RefreshPaytableTestUi();
        }

        private void ResetPaytableTestPaysTimeout()
        {
            paytableTestPaysDeadlineTimestamp = ResultClockDeadline(
                System.Diagnostics.Stopwatch.GetTimestamp(), PaysTimeoutMilliseconds/1000f,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void PlacePaytableTestBet(int id)
        {
            if (!PaytableTestActive || paytableTestPaysPage >= 0) return;
            try { paytableTest.PlaceBet(id); paytableTestTotalWritten = true; }
            catch (BetRejectedException) { /* Source emits rejection event; it adds no wager. */ }
            RefreshPaytableTestUi();
        }

        private void HidePaytableTestMarker() { if (paytableTestMarker != null) paytableTestMarker.gameObject.SetActive(false); }

        private void ShowPaytableTestMarker(int spot)
        {
            if (!PaytableTestActive || paytableTestPaysPage >= 0 || spot < 0 || spot >= 157) return;
            Rect bounds = originalLayout.bet_spots[spot].ToRect();
            int index = Array.IndexOf(chips, paytableTest.Chip);
            if (index < 0) { HidePaytableTestMarker(); return; }
            paytableTestMarker.texture = originalMainChipButtonFrames[index][0];
            paytableTestMarker.rectTransform.anchoredPosition = new Vector2(
                bounds.x + (bounds.width-90)*.5f, -bounds.y-(bounds.height-90)*.5f);
            paytableTestMarker.gameObject.SetActive(true);
        }

        private void MovePaytableTestMarker(PointerEventData pointer)
        {
            if (pointer == null || EventSystem.current == null) return;
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            RouletteTableRelease target = hits.Count == 0 ? null :
                hits[0].gameObject.GetComponentInParent<RouletteTableRelease>();
            if (target != null && target.transform.IsChildOf(paytableTestScreen))
                ShowPaytableTestMarker(target.SpotIndex);
            else HidePaytableTestMarker();
        }

        private void RefreshPaytableTestUi()
        {
            if (!PaytableTestActive) return;
            if (paytableTestPaysPage >= 0 && paytableTestPaysDeadlineTimestamp > 0 &&
                System.Diagnostics.Stopwatch.GetTimestamp() >= paytableTestPaysDeadlineTimestamp)
            {
                PressPaytableTestButton(2);
                return;
            }
            // The test handler owns input until it exits its nested video loop.
            // Physical PC secondary display canvases must not expose live bets.
            if (ideckCanvas != null) ideckCanvas.enabled = false;
            if (topperCanvas != null) topperCanvas.enabled = false;
            // Original glOrtho has bottom > top; a positive source Rotate
            // therefore turns clockwise on screen. Unity UI uses y-up.
            paytableTestWheel.rectTransform.localEulerAngles = new Vector3(0, 0, -paytableTest.MainRenderAngle);
            paytableTestSpindle.rectTransform.localEulerAngles = new Vector3(0, 0, -paytableTest.SpindleRenderAngle);
            double radians = OriginalWheelArithmetic.NudgeBallRadians(paytableTest.BallAngle);
            Vector2 ballPosition = OriginalWheelArithmetic.BallTopLeft(radians,
                (int)paytableTestLayout.wheelCenter.x,(int)paytableTestLayout.wheelCenter.y,
                paytableTestLayout.wheelRadius,(int)ball.width,(int)ball.height);
            paytableTestBall.rectTransform.anchoredPosition = new Vector2(ballPosition.x,-ballPosition.y);
            paytableTestOdds.texture = originalOddsPanels[paytableTestProfileIndex];
            bool pays = paytableTestPaysPage >= 0;
            int mask = pays ? 0x1c : 0x23;
            for (int i = 0; i < 6; ++i)
            {
                bool shown = (mask & (1 << i)) != 0;
                paytableTestButtons[i].interactable = shown;
                // Source Hide keeps the captured sprite alive for touch-up.
                // Keep the component active so its depressed state restores
                // even when the down action switches the visibility mask.
                CanvasGroup visibility = paytableTestButtons[i].GetComponent<CanvasGroup>();
                visibility.alpha = shown ? 1 : 0;
                visibility.blocksRaycasts = shown;
                paytableTestButtons[i].GetComponent<RouletteOriginalButtonFrames>().Refresh();
            }
            paytableTestPays.gameObject.SetActive(pays);
            paytableTestPaysLines.gameObject.SetActive(pays && paytableTestPaysPage == 3);
            if (pays)
            {
                paytableTestPays.texture = paysPages[paytableTestPaysPage];
                paytableTestPays.rectTransform.sizeDelta = new Vector2(1080,
                    paysPages[paytableTestPaysPage].height);
                paytableTestPaysLines.texture = paysLinePages[paytableTestProfileIndex];
                HidePaytableTestMarker();
            }
            paytableTestClear.interactable = !pays && paytableTest.Bets.Count > 0;
            paytableTestClearFrames.Refresh();
            for (int i = 0; i < 4; ++i)
            {
                paytableTestChipStates[i].GetComponent<Button>().interactable = !pays;
                paytableTestChipStates[i].SetSelected(paytableTest.Chip == chips[i]);
            }
            paytableTestMessages[0].Set(paytableTestTotalWritten ? paytableTestLayout.totalBetLabel + " = " + paytableTest.TotalBet : "");
            RoundResult result = paytableTest.Result;
            paytableTestMessages[2].Set(result == null ? "" : paytableTestLayout.payLabel + " = " + result.Returned);
            int visiblePay = result == null || result.Returned <= 0 || paytableTestCycleStarted == 0 ? -1 :
                WinPayAtTime(result, CycleElapsedSeconds(paytableTestCycleStarted,
                    System.Diagnostics.Stopwatch.GetTimestamp(), System.Diagnostics.Stopwatch.Frequency));
            RefreshPaytableTestStacks(visiblePay);
        }

        private void RefreshPaytableTestStacks(int visiblePay)
        {
            long signature = 17;
            foreach (var bet in paytableTest.Bets.OrderBy(b => b.Key))
                signature = unchecked(signature*31 + bet.Key*10000 + bet.Value);
            signature = unchecked(signature*31 + visiblePay);
            if (signature == paytableTestStackSignature) return;
            paytableTestStackSignature = signature;
            for (int i = paytableTestChips.childCount-1; i >= 0; --i)
            {
                GameObject child = paytableTestChips.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            foreach (var bet in paytableTest.Bets.OrderBy(b => originalLayout.bet_spots[b.Key-100].z).ThenBy(b => b.Key))
            {
                Rect bounds = originalLayout.bet_spots[bet.Key-100].ToRect();
                float cx = bounds.x+bounds.width*.5f, cy = bounds.y+bounds.height*.5f;
                UiImage(paytableTestChips, new Rect(cx-36, cy-36, 72, 72),
                    originalPlacedChips[ClosestBetSelector(bet.Value)], "Test placed chip " + bet.Key);
                new PictureLabel(paytableTestChips, new Rect(cx-15, cy-12, 30, 24),
                    chipStackFont, "Test stack amount", nativeSize: true).Set(bet.Value.ToString());
            }
            if (visiblePay < 0) return;
            Rect spot = originalLayout.bet_spots[visiblePay-100].ToRect();
            Rect highlight = new Rect(spot.x+spot.width*.5f-59.5f, spot.y+spot.height*.5f-54, 119, 108);
            UiImage(paytableTestChips, highlight, betWinHighlight, "Test winning spot");
            WinningPayRecord pay = paytableTest.Result.WinningPays.First(p => p.BetId == visiblePay);
            new PictureLabel(paytableTestChips, highlight, cyclePayFont, "Test winning payout", nativeSize: true)
                .Set(SourceBetSpotPayout(pay.Amount));
        }
    }
}
