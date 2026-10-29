using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class DoubleHistoryLayout
        {
            public DoubleHistoryItem[] items;
        }

        [Serializable]
        private sealed class DoubleHistoryItem
        {
            public string name;
            public string asset;
            public string font;
            public int x, y, w, h, z;
            public Rect Bounds { get { return new Rect(x, y, w, h); } }
        }

        private readonly Dictionary<string, DoubleHistoryItem> doubleHistoryLayout =
            new Dictionary<string, DoubleHistoryItem>();
        private RectTransform doubleHistoryRoot, doubleHistoryRows;
        private Button historyDoubleButton, doubleHistoryExitButton;
        private bool doubleHistoryView;
        private long doubleHistoryExitTimestamp;
        private OriginalBitmapFont doubleHistoryFont, doubleHistoryFinalFont,
            doubleHistoryFinalSmallFont;

        private void CreateDoubleHistoryUi()
        {
            TextAsset metadata = Resources.Load<TextAsset>("Original/DoubleHistory/layout");
            if (metadata == null) throw new InvalidOperationException("Original Double Up history layout missing");
            DoubleHistoryLayout layout = JsonUtility.FromJson<DoubleHistoryLayout>(metadata.text);
            if (layout == null || layout.items == null || layout.items.Length < 250)
                throw new InvalidOperationException("Original Double Up history layout incomplete");
            foreach (DoubleHistoryItem item in layout.items)
                doubleHistoryLayout[item.name] = item;
            doubleHistoryFont = OriginalBitmapFont.Load("double_history");
            doubleHistoryFinalFont = OriginalBitmapFont.Load("double_history_final");
            doubleHistoryFinalSmallFont = OriginalBitmapFont.Load("double_history_final_small");
            if (doubleHistoryFont == null || doubleHistoryFinalFont == null ||
                doubleHistoryFinalSmallFont == null)
                throw new InvalidOperationException("Original Double Up history font missing");
            doubleHistoryRoot = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920),
                "Original Double Up recall");
            DoubleHistoryItem background = doubleHistoryLayout["fHistBackground"];
            RawImage backdrop = UiImage(doubleHistoryRoot, background.Bounds,
                DoubleHistoryTexture(background.asset), "Original blue recall background");
            backdrop.raycastTarget = true;
            doubleHistoryRows = UiRect(doubleHistoryRoot, new Rect(0, 0, 1080, 1920),
                "Original Double Up recall rows");
            DoubleHistoryItem exit = doubleHistoryLayout["fHistoryExitButton"];
            doubleHistoryExitButton = UiButton(doubleHistoryRoot, exit.Bounds,
                () => { }, DoubleHistoryTexture(exit.asset));
            doubleHistoryExitButton.gameObject.name = "Original Double Up history exit";
            doubleHistoryExitButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(doubleHistoryExitButton, () => SetDoubleHistoryView(false));
            doubleHistoryRoot.gameObject.SetActive(false);

            historyDoubleButton = UiButton(historyRoot, new Rect(482, 60, 116, 38),
                () => { },
                Resources.Load<Texture2D>("Original/History/DSDoubleUp_1"));
            historyDoubleButton.gameObject.name = "Original history Double Up";
            BindHistoryButtonFrames(historyDoubleButton, "DSDoubleUp");
            historyDoubleButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(historyDoubleButton, () => SetDoubleHistoryView(true));
            BindHistoryButtonLabel(historyDoubleButton, "DoubleUp");
            historyDoubleButton.gameObject.SetActive(false);
        }

        private static Texture2D DoubleHistoryTexture(string name)
        {
            Texture2D value = Resources.Load<Texture2D>("Original/DoubleHistory/" + name);
            if (value == null)
                throw new InvalidOperationException("Original Double Up history bitmap missing: " + name);
            return value;
        }

        private void SetDoubleHistoryView(bool enabled)
        {
            int sourceIndex = HistoryRecordIndex(historyPage);
            RecordedRound round = null;
            byte[] snapshot;
            doubleHistoryView = enabled && historyView &&
                TryGetHistoryRecord(sourceIndex, out round, out snapshot) &&
                round != null && round.present && round.doubleUp != null && round.doubleUp.complete;
            if (doubleHistoryRoot == null) return;
            doubleHistoryRoot.gameObject.SetActive(doubleHistoryView);
            if (!doubleHistoryView) return;
            doubleHistoryExitTimestamp = OriginalRackClock.AfterMilliseconds(
                System.Diagnostics.Stopwatch.GetTimestamp(), 30000,
                System.Diagnostics.Stopwatch.Frequency);
            RenderDoubleHistory(round.doubleUp);
        }

        private void RefreshDoubleHistoryUi()
        {
            if (doubleHistoryView && DoubleHistoryRecallExpired(
                doubleHistoryExitTimestamp,
                System.Diagnostics.Stopwatch.GetTimestamp()))
                SetDoubleHistoryView(false);
            if (doubleHistoryRoot != null)
                doubleHistoryRoot.gameObject.SetActive(doubleHistoryView && historyView);
        }

        private static bool DoubleHistoryRecallExpired(long deadline, long now)
        {
            return now >= deadline;
        }

        private void RenderDoubleHistory(RecordedDouble feature)
        {
            ClearHistoryLayer(doubleHistoryRows);
            for (int i = 0; i < feature.steps.Length; ++i)
            {
                RecordedDoubleStep step = feature.steps[i];
                int row = i + 1;
                ShowDoubleHistoryFeature("fGambleNumber" + row, null);
                string dealer = step.dealer == 0 ? "Black" :
                    step.dealer == 1 ? "Black" : "Red";
                if (feature.suitMode)
                    dealer = step.dealer == 0 ? "Club" : step.dealer == 1 ? "Spade" :
                        step.dealer == 2 ? "Diamond" : "Heart";
                ShowDoubleHistoryFeature("fHistDealer" + dealer + row, null);
                string player = step.choice == 5 ? "Red" : step.choice == 4 ? "Black" :
                    step.choice == 3 ? "Heart" : step.choice == 2 ? "Diamond" :
                    step.choice == 1 ? "Spade" : "Club";
                ShowDoubleHistoryFeature("fHistPlayer" + player + row, null);
                ShowDoubleHistoryFeature("fCreditsWagered" + row, step.wager.ToString());
                ShowDoubleHistoryFeature((step.won > 0 ? "fCreditsWon" : "fFinalWin") + row,
                    step.won.ToString());
            }
            // FeatureRecall also emits a Take Win row when the player
            // collects the initial stake without making any guess.
            if (feature.finalWin > 0 && feature.steps.Length < 17)
            {
                int row = feature.steps.Length + 1;
                ShowDoubleHistoryFeature("fGambleNumber" + row, null);
                ShowDoubleHistoryFeature("fHistTakeWin" + row, null);
                ShowDoubleHistoryFeature("fFinalWin" + row, feature.finalWin.ToString());
            }
        }

        private void ShowDoubleHistoryFeature(string name, string value)
        {
            DoubleHistoryItem item;
            if (!doubleHistoryLayout.TryGetValue(name, out item))
                throw new InvalidOperationException("Original Double Up recall feature missing: " + name);
            if (!string.IsNullOrEmpty(item.asset))
                UiImage(doubleHistoryRows, item.Bounds, DoubleHistoryTexture(item.asset), name);
            else
                new PictureLabel(doubleHistoryRows, item.Bounds,
                    DoubleHistoryMeterFont(item, value ?? ""), name, true)
                    .Set(value ?? "");
        }

        private OriginalBitmapFont DoubleHistoryMeterFont(DoubleHistoryItem item, string value)
        {
            if (item.font == "FUT_BLD_18_WHT") return doubleHistoryFont;
            if (item.font == "FUT_BLD_24_RED")
                return doubleHistoryFinalFont.MeasureWidth(value) < item.w ?
                    doubleHistoryFinalFont : doubleHistoryFinalSmallFont;
            throw new InvalidOperationException("Unknown Double Up history font: " + item.font);
        }

        private IEnumerator DoubleHistorySmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test", StringComparison.Ordinal) ||
                recordedRoundPages.Count != 1 ||
                recordedRoundPages[0].doubleUp == null)
                throw new InvalidOperationException("Double Up history smoke needs saved test round");
            SetHistoryView(true);
            RefreshHistoryPageUi();
            yield return null;
            // The final Paid meter stays at zero after the feature loss,
            // while the base-table win cycle can show a constituent pay.
            if (!historyDoubleButton.gameObject.activeSelf ||
                historyPaid.Value != "0" || recordedRoundPages[0].returned != 0 ||
                historyDoubleButton.GetComponent<RectTransform>().anchoredPosition !=
                    new Vector2(482, -60))
                throw new InvalidOperationException(
                    "Source Double Up history button unavailable: active=" +
                    historyDoubleButton.gameObject.activeSelf + " paid=" +
                    historyPaid.Value + " gamePays=" + historyGamePays.Value +
                    " position=" + historyDoubleButton.GetComponent<RectTransform>()
                        .anchoredPosition);
            RectTransform buttonLabel = historyDoubleButton.transform.Find(
                "Bitmap text") as RectTransform;
            RawImage buttonGlyph = buttonLabel == null ? null :
                buttonLabel.GetComponentInChildren<RawImage>();
            if (buttonGlyph == null ||
                buttonLabel.anchoredPosition != Vector2.zero ||
                buttonLabel.rect.size != historyDoubleButton.GetComponent<RectTransform>().rect.size ||
                buttonGlyph.texture != historyMeterFont.Texture ||
                Mathf.Abs(buttonGlyph.rectTransform.rect.height -
                    historyMeterFont.Height) > .01f)
                throw new InvalidOperationException(
                    "Original Double Up history button glyph size differs");
            PointerEventData press = PressThroughUiRaycast(historyDoubleButton);
            if (((RawImage)historyDoubleButton.targetGraphic).texture !=
                Resources.Load<Texture2D>("Original/History/DSDoubleUp_2") ||
                buttonLabel.anchoredPosition != new Vector2(1, -1))
                throw new InvalidOperationException(
                    "Double Up history Button did not select the source depressed frame");
            if (!doubleHistoryView || !doubleHistoryRoot.gameObject.activeSelf ||
                doubleHistoryRows.childCount != 10)
                throw new InvalidOperationException("Source Double Up recall rows did not render");
            ReleaseUiPointer(historyDoubleButton, press);
            if (buttonLabel.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("Double Up history label did not return on release");
            if (!doubleHistoryView || doubleHistoryRows.childCount != 10)
                throw new InvalidOperationException(
                    "History Double Up repeated on pointer release");
            DoubleHistoryItem finalDefinition;
            RectTransform finalRow = doubleHistoryRows.Find("fFinalWin2") as RectTransform;
            RawImage finalGlyph = finalRow == null ? null :
                finalRow.GetComponentInChildren<RawImage>();
            if (!doubleHistoryLayout.TryGetValue("fFinalWin1", out finalDefinition) ||
                finalDefinition.font != "FUT_BLD_24_RED" ||
                DoubleHistoryMeterFont(finalDefinition, "1234567890") !=
                    doubleHistoryFinalSmallFont ||
                finalGlyph == null || finalGlyph.texture != doubleHistoryFinalFont.Texture ||
                Mathf.Abs(finalGlyph.rectTransform.rect.height - doubleHistoryFinalFont.Height) > .01f)
                throw new InvalidOperationException(
                    "Original native-size Final Win font was not used in recall");
            string[] args = Environment.GetCommandLineArgs();
            int capture = Array.IndexOf(args, "-rouletteDoubleHistoryCapture");
            if (capture >= 0 && capture + 1 < args.Length)
                CaptureActiveScreenOffscreen(args[capture + 1]);
            var suitRecall = new RecordedDouble {
                startingStake = 4, suitMode = true, tookWin = true,
                complete = true, finalWin = 16,
                steps = new[] { new RecordedDoubleStep {
                    choice = 3, dealer = 3, wager = 4, won = 16
                } }
            };
            if (!IsValidRecordedDouble(suitRecall))
                throw new InvalidOperationException("Source suit recall validation failed");
            RenderDoubleHistory(suitRecall);
            int activeRows = 0;
            bool dealerHeart = false, playerHeart = false, takeWin = false;
            for (int i = 0; i < doubleHistoryRows.childCount; ++i)
            {
                GameObject row = doubleHistoryRows.GetChild(i).gameObject;
                if (!row.activeSelf) continue;
                ++activeRows;
                dealerHeart |= row.name == "fHistDealerHeart1";
                playerHeart |= row.name == "fHistPlayerHeart1";
                takeWin |= row.name == "fHistTakeWin2";
            }
            if (activeRows != 8 || !dealerHeart || !playerHeart || !takeWin)
                throw new InvalidOperationException("Source suit and Take Win recall rows differ");
            var immediateRecall = new RecordedDouble {
                startingStake = 4, suitMode = false, tookWin = true,
                complete = true, finalWin = 4,
                steps = new RecordedDoubleStep[0]
            };
            if (!IsValidRecordedDouble(immediateRecall))
                throw new InvalidOperationException("Immediate Take Win archive is invalid");
            RenderDoubleHistory(immediateRecall);
            int immediateRows = 0;
            bool numberRow = false, takeWinRow = false, finalWinRow = false;
            for (int i = 0; i < doubleHistoryRows.childCount; ++i)
            {
                GameObject row = doubleHistoryRows.GetChild(i).gameObject;
                if (!row.activeSelf) continue;
                ++immediateRows;
                numberRow |= row.name == "fGambleNumber1";
                takeWinRow |= row.name == "fHistTakeWin1";
                finalWinRow |= row.name == "fFinalWin1";
            }
            if (immediateRows != 3 || !numberRow || !takeWinRow || !finalWinRow)
                throw new InvalidOperationException(
                    "Immediate Take Win row is missing from original recall");
            yield return null;
            PointerEventData exitPress =
                PressThroughUiRaycast(doubleHistoryExitButton);
            if (doubleHistoryView || doubleHistoryRoot.gameObject.activeSelf ||
                !historyView)
                throw new InvalidOperationException(
                    "Double Up history EXIT did not act on pointer down");
            ReleaseUiPointer(doubleHistoryExitButton, exitPress);
            if (doubleHistoryView)
                throw new InvalidOperationException(
                    "Double Up history EXIT repeated on pointer release");
            SetDoubleHistoryView(true);
            if (!doubleHistoryView || !doubleHistoryRoot.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Double Up history did not reopen for timeout check");
            long longUptime = OriginalRackClock.AfterMilliseconds(
                1000000000L * System.Diagnostics.Stopwatch.Frequency,
                30000, System.Diagnostics.Stopwatch.Frequency);
            if (DoubleHistoryRecallExpired(longUptime, longUptime - 1) ||
                !DoubleHistoryRecallExpired(longUptime, longUptime) ||
                longUptime <= 1000000000L * System.Diagnostics.Stopwatch.Frequency)
                throw new InvalidOperationException(
                    "Double Up recall monotonic deadline is invalid");
            doubleHistoryExitTimestamp = System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            RefreshDoubleHistoryUi();
            if (doubleHistoryView || doubleHistoryRoot.gameObject.activeSelf || !historyView)
                throw new InvalidOperationException("Source 30-second recall exit did not return to history");
            SetHistoryView(false);
            Debug.Log("ROULETTE_DOUBLE_HISTORY_UI_PASSED: source red/black and suit rows, immediate Take Win, DS button, press EXIT, 30s exit");
        }
    }
}
