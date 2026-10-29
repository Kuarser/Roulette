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
        [Serializable]
        private sealed class OperatorFontPage
        {
            public int[] codes, starts, ends;
            public int width, height;
            public string resource;
            public OperatorFontPage[] pages;
        }

        private static void RequireOperator(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Operator fallback: " + message);
        }

        private static void VerifyOperatorFontPages()
        {
            OriginalBitmapFont font = OriginalBitmapFont.Load("operator_9");
            RequireOperator(font != null && font.Height == 30 && font.PageCount == 8 &&
                font.GlyphCount == 321, "Complete source Unicode font 9");
            VerifyCompleteFont("operator_9");
            Texture2D baseTexture, fallbackTexture; Rect baseUv, fallbackUv; int baseWidth, fallbackWidth;
            RequireOperator(font.TryGetGlyph('\u00FF', out baseTexture, out baseUv, out baseWidth) &&
                font.TryGetGlyph('\uFFFF', out fallbackTexture, out fallbackUv, out fallbackWidth) &&
                baseTexture == fallbackTexture && baseUv == fallbackUv && baseWidth == fallbackWidth &&
                font.MeasureWidth("\uFFFF") == baseWidth,
                "Missing BMP page falls back to the same low byte in page zero");
            Debug.Log("ROULETTE_OPERATOR_FONT_PASSED: 8 source pages, 321 glyphs, native packed GPU checksums");
        }

        private IEnumerator OperatorFallbackSmoke()
        {
            yield return null;
            IRouletteSession savedSession = session;
            string savedFingerprint = PaytableTestLiveFingerprint(savedSession);
            var forbidden = new PaytableTestForbiddenOutcome();
            session = new RouletteSession(RouletteProfile.Load("97"), 1000, forbidden);
            session.PlaceBet(100, 1);
            RefreshPortraitUi();
            string before = PaytableTestLiveFingerprint(session);
            string[] args = Environment.GetCommandLineArgs();
            int capture = Array.IndexOf(args, "-rouletteOperatorFallbackCaptureDirectory");
            string directory = capture < 0 ? null : args[capture+1];
            if (directory != null) Directory.CreateDirectory(directory);
            Action notification = null;
            Action statisticsDisplay = null, statisticsExit = null;
            int statisticsShown = 0, statisticsClosed = 0;
            try
            {
                VerifyRendererFontAtlases();
                VerifyOperatorFontPages();
                statisticsDisplay = () => {
                    RequireOperator(OperatorFallbackActive && operatorFallbackScreen != null &&
                        operatorFallbackLabel == null, "Statistics completion precedes text display");
                    ++statisticsShown;
                };
                statisticsExit = () => {
                    RequireOperator(!OperatorFallbackActive && operatorFallbackOverlay == null &&
                        operatorFallbackScreen == null && operatorFallbackLabel == null,
                        "Statistics deletion precedes exit completion");
                    ++statisticsClosed;
                };
                PcGameStatisticsDisplayComplete += statisticsDisplay;
                PcGameStatisticsExitComplete += statisticsExit;
                RequireOperator(RequestPcGameStatistics(new Rect(0, 0, 1, 1)), "Idle statistics entry");
                yield return null;
                RequireOperator(operatorFallbackLabel.Value == "No Game Statistics Available" &&
                    operatorFallbackLabel.Root.anchoredPosition == new Vector2(0, -960) &&
                    operatorFallbackLabel.SelectedFont.Height == 30, "Source phrase/font/local top anchor");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "statistics.png"));
                Canvas.ForceUpdateCanvases();
                var pointer = new PointerEventData(EventSystem.current) {
                    position = RectTransformUtility.WorldToScreenPoint(null, uiSpinButton.transform.position),
                    button = PointerEventData.InputButton.Left };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                RequireOperator(hits.Count > 0 && hits[0].gameObject.transform.IsChildOf(operatorFallbackOverlay),
                    "Operator page captures live PLAY hit");
                Bet(131); BeginSpin();
                RequireOperator(!RequestPcPaytableTest("97", 0, 0, 0, new Rect(0, 0, 1, 1)) &&
                    !RequestPcVideoDiagnostics(), "Operator page owns entry/input");
                RequireOperator(PaytableTestLiveFingerprint(session) == before && forbidden.Requests == 0,
                    "Statistics changed live funds, wagers, persistence or RNG");
                RequireOperator(RequestPcGameStatistics(new Rect(.123f, .157f, .743f, .619f)), "Source statistics replacement");
                yield return null;
                RequireOperator(operatorFallbackScreen.anchoredPosition == new Vector2(132, -301) &&
                    operatorFallbackScreen.sizeDelta == new Vector2(802, 1188) &&
                    operatorFallbackLabel.Root.anchoredPosition == new Vector2(0, -594),
                    "Source normalized rectangle truncation and integer local centre");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "statistics_rectangle.png"));
                ClosePcOperatorFallback();
                RequireOperator(statisticsShown == 2 && statisticsClosed == 1,
                    "Replacing statistics sends display completion, without an exit completion");

                // Inspect the transient source page before its synchronous cleanup.
                ShowOperatorFallback(new Rect(0, 0, 1080, 1920), true);
                RequireOperator(operatorFallbackLabel.Value == "NO DIAGNOSTICS AVAILABLE", "Source diagnostics phrase");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "diagnostics_transient_fixture.png"));
                var unicode = new PictureLabel(operatorFallbackScreen, new Rect(0, 1000, 1080, 30),
                    operatorFallbackFont, "Unicode page acceptance fixture", nativeSize: true, verticalAlign: -1);
                string sample = "A\u10DA\u20AB\u17DB\u0401\u0E3F\u0142\u062F";
                unicode.Set(sample);
                RequireOperator(unicode.Root.childCount == 8 &&
                    unicode.Root.GetComponentsInChildren<RawImage>().Select(g => g.texture).Distinct().Count() == 8 &&
                    unicode.Root.GetComponentsInChildren<RawImage>().All(g => g.rectTransform.sizeDelta.y == 30),
                    "uGUI selects each glyph's texture and shared maximum height");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "unicode_pages_fixture.png"));
                ClosePcOperatorFallback();
                int notified = 0;
                notification = () => {
                    RequireOperator(!OperatorFallbackActive && operatorFallbackOverlay == null,
                        "Diagnostics cleanup precedes exit notification");
                    ++notified;
                };
                PcVideoDiagnosticsExitRequested += notification;
                RequireOperator(RequestPcVideoDiagnostics() && notified == 1 && !OperatorFallbackActive,
                    "Synchronous no-diagnostics exit");
                RequireOperator(statisticsShown == 2 && statisticsClosed == 1,
                    "Diagnostics must not send statistics completions");
                RequireOperator(PaytableTestLiveFingerprint(session) == before && forbidden.Requests == 0,
                    "Diagnostics/replacement/exit altered live state or RNG");
                spinning = true;
                RequireOperator(!RequestPcGameStatistics(new Rect(0, 0, 1, 1)) && !RequestPcVideoDiagnostics(),
                    "In-play requests rejected");
                spinning = false;
                RequireOperator(PaytableTestLiveFingerprint(savedSession) == savedFingerprint, "Saved session unchanged");
                Debug.Log("ROULETTE_OPERATOR_FALLBACK_UI_PASSED: source vtables/phrases/font9; replacement/rectangle; raycast/live/RNG/persistence isolation; statistics display/exit callback order; diagnostics cleanup-before-notify; in-play gate");
            }
            finally
            {
                spinning = false;
                if (notification != null) PcVideoDiagnosticsExitRequested -= notification;
                if (statisticsDisplay != null) PcGameStatisticsDisplayComplete -= statisticsDisplay;
                if (statisticsExit != null) PcGameStatisticsExitComplete -= statisticsExit;
                ClosePcOperatorFallback();
                session = savedSession;
                RefreshPortraitUi();
            }
            yield return null;
            Application.Quit();
        }
    }
}
