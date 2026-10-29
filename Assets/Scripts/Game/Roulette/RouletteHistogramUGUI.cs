using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private const int HistogramCapacity = 200;
        private const int HistogramBaseY = 632;
        private bool histogramPreview;
        private int histogramPage;
        private GameObject histogramRoot;
        private readonly RawImage[] histogramBars = new RawImage[37];
        private readonly int[] histogramCounts = new int[37];
        private int histogramShownVersion = -1;
        private int histogramShownPage = -1;
        private bool histogramShownHistoryView;

        private void SetHistogramPreview(bool enabled)
        {
            if (enabled) historyView = false;
            histogramPreview = enabled;
            if (enabled)
            {
                histogramPage = 0;
                ideckPreview = false;
                topperPreview = false;
            }
            RefreshTopperTarget();
        }

        private void CreateHistogramUi(RectTransform topper)
        {
            RectTransform root = UiRect(topper, new Rect(0, 0, 1360, 768),
                "Original 200 spin histogram");
            histogramRoot = root.gameObject;
            UiImage(root, new Rect(0, 0, 1360, 768),
                Resources.Load<Texture2D>("Original/Histogram/Background"),
                "Original histogram background");
            for (int pocket = 0; pocket < histogramBars.Length; ++pocket)
                histogramBars[pocket] = UiImage(root,
                    new Rect(34 + 35 * pocket, 505, 32, 127),
                    Resources.Load<Texture2D>("Original/Histogram/Tick" + pocket),
                    "Original histogram tick " + pocket);
            UiImage(root, new Rect(358, 685, 644, 69),
                Resources.Load<Texture2D>("Original/Histogram/Title"),
                "Original histogram title");
            histogramRoot.SetActive(false);
        }

        private void RefreshHistogramUi()
        {
            if (histogramRoot == null) return;
            histogramRoot.SetActive(histogramPreview);
            if (historyView)
                histogramPage = HistoryRecordIndex(Mathf.Clamp(historyPage, 0,
                    Mathf.Max(0, CommittedHistoryCount() - 1)));
            if (!histogramPreview ||
                (histogramShownVersion == recordedHistoryVersion &&
                 histogramShownPage == histogramPage &&
                 histogramShownHistoryView == historyView)) return;
            histogramShownVersion = recordedHistoryVersion;
            histogramShownPage = histogramPage;
            histogramShownHistoryView = historyView;
            Array.Clear(histogramCounts, 0, histogramCounts.Length);
            byte[] page;
            if (historyView)
            {
                RecordedRound round;
                TryGetHistoryRecord(histogramPage, out round, out page);
            }
            else page = histogramPage >= 0 && histogramPage < recordedSnapshots.Count ?
                recordedSnapshots[histogramPage] : null;
            // Empty committed history must not expose the unpublished live
            // result. The standalone histogram keeps its live fallback.
            int count = page == null ? (historyView ? 0 :
                Math.Min(recordedPockets.Count, HistogramCapacity)) : HistogramCapacity;
            for (int i = 0; i < count; ++i)
            {
                int pocket = page == null ? recordedPockets[i] : page[i];
                if (pocket >= 0 && pocket <= 36) ++histogramCounts[pocket];
            }
            int maximum = 0;
            for (int i = 0; i < histogramCounts.Length; ++i)
                maximum = Math.Max(maximum, histogramCounts[i]);
            for (int pocket = 0; pocket < histogramBars.Length; ++pocket)
            {
                RawImage bar = histogramBars[pocket];
                int height = maximum == 0 ? 0 :
                    (int)Math.Floor(127.0 * histogramCounts[pocket] / maximum + .5);
                bar.gameObject.SetActive(height > 0);
                bar.rectTransform.anchoredPosition =
                    new Vector2(34 + 35 * pocket, -(HistogramBaseY - height));
                bar.rectTransform.sizeDelta = new Vector2(32, height);
            }
        }

        private IEnumerator HistogramSmoke()
        {
            yield return null;
            temporaryHistory = true;
            recordedPockets.Clear();
            for (int i = 0; i < HistogramCapacity; ++i)
                recordedPockets.Add(i < 100 ? 0 : i < 150 ? 1 : 2);
            ++recordedHistoryVersion;
            SetHistogramPreview(true);
            yield return null;
            if (recordedPockets.Count != HistogramCapacity ||
                histogramRoot == null || !histogramRoot.activeSelf ||
                histogramCounts[0] != 100 || histogramCounts[1] != 50 ||
                histogramCounts[2] != 50 || histogramBars[0].rectTransform.sizeDelta.y != 127 ||
                histogramBars[1].rectTransform.sizeDelta.y != 64 ||
                histogramBars[2].rectTransform.anchoredPosition.y != -(HistogramBaseY - 64) ||
                histogramBars[3].gameObject.activeSelf)
                throw new InvalidOperationException("Original histogram counts or uGUI geometry differ");
            RecordPocket(3);
            for (int i = 0; i <= 50; ++i) RecordPocket(i % 37);
            if (recordedPockets.Count != HistogramCapacity ||
                recordedSnapshots.Count != 50 ||
                recordedSnapshots[0][0] != 50 % 37 ||
                recordedSnapshots[49][0] != 1)
                throw new InvalidOperationException("Original 50-page histogram ring differs");
            Debug.Log("ROULETTE_HISTOGRAM_PASSED: 200 outcomes, 37 bars, 127/64 pixel scaling");
        }

        private IEnumerator HistogramSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test", StringComparison.Ordinal))
                throw new InvalidOperationException("Histogram persistence smoke needs isolated test key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            for (int i = 0; i < HistogramCapacity; ++i)
                recordedPockets.Add(i % 37);
            for (int i = 0; i <= 50; ++i) RecordPocket(i % 37);
            if (recordedPockets.Count != HistogramCapacity ||
                recordedSnapshots.Count != 50 ||
                recordedSnapshots[0][0] != 50 % 37 ||
                recordedSnapshots[49][0] != 1 ||
                string.IsNullOrEmpty(PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "")))
                throw new InvalidOperationException("Histogram page ring was not saved");
            Debug.Log("ROULETTE_HISTOGRAM_SAVE_PASSED: 50 pages of 200 outcomes");
        }

        private IEnumerator HistogramLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test", StringComparison.Ordinal) ||
                recordedPockets.Count != HistogramCapacity ||
                recordedSnapshots.Count != 50 ||
                recordedSnapshots[0][0] != 50 % 37 ||
                recordedSnapshots[49][0] != 1)
                throw new InvalidOperationException("Histogram page ring did not reload");
            SetHistogramPreview(true);
            histogramPage = 49;
            yield return null;
            if (!histogramRoot.activeSelf || histogramShownPage != 49 ||
                histogramCounts[1] <= 0)
                throw new InvalidOperationException("Older histogram page did not render");
            Debug.Log("ROULETTE_HISTOGRAM_LOAD_PASSED: 50 pages reloaded and navigated");
        }
    }
}
