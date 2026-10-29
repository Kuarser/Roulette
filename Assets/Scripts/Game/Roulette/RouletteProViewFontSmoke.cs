using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireTotalWinNative(PictureLabel label, string value,
            int firstX)
        {
            label.Set(value);
            if (label.Root.anchoredPosition != new Vector2(0, -656) ||
                label.Root.sizeDelta != new Vector2(941, 139))
                throw new InvalidOperationException("Total-win source bounds differ");
            int x = firstX;
            int active = 0;
            foreach (Transform child in label.Root)
            {
                if (!child.gameObject.activeSelf) continue;
                RawImage glyph = child.GetComponent<RawImage>();
                Rect uv;
                int width;
                if (!label.SelectedFont.TryGetGlyph(value[active], out uv, out width) ||
                    glyph == null || glyph.texture != label.SelectedFont.Texture ||
                    glyph.uvRect != uv || glyph.raycastTarget ||
                    glyph.rectTransform.sizeDelta != new Vector2(width, 139) ||
                    glyph.rectTransform.anchoredPosition != new Vector2(x, 0))
                    throw new InvalidOperationException("Total-win glyph size/top anchor differs");
                x += width;
                ++active;
            }
            if (active != value.Length || label.SelectedFont.Height != 139 ||
                label.SelectedFont.MeasureWidth(value) != (470 - firstX) * 2 +
                    (value == "(123456789012)" ? 0 : 1))
                throw new InvalidOperationException("Total-win native glyphs missing");
        }

        private static void RequireProViewFont(PictureLabel label, string value,
            OriginalBitmapFont expected, int firstWidth, int height,
            int horizontalAlign = 0, int measureHeight = 0)
        {
            label.Set(value);
            if (measureHeight == 0) measureHeight = height;
            if (label.SelectedFont != expected || expected.Height != measureHeight ||
                expected.SourceTexture.height != height || expected.GlyphHeight != measureHeight)
                throw new InvalidOperationException("Original ProView font choice differs: " + value);
            RawImage first = null;
            foreach (Transform child in label.Root)
                if (child.gameObject.activeSelf) { first = child.GetComponent<RawImage>(); break; }
            if (first == null || first.texture != expected.Texture ||
                first.rectTransform.sizeDelta != new Vector2(firstWidth, measureHeight) ||
                first.raycastTarget)
                throw new InvalidOperationException("ProView bitmap was scaled or covers touch: " + value);
            int width = expected.MeasureWidth(value);
            float x = horizontalAlign < 0 ? 0 :
                horizontalAlign > 0 ? label.Root.rect.width - width :
                Mathf.Floor(label.Root.rect.width / 2) - width / 2;
            Vector2 position = new Vector2(x,
                -(Mathf.Floor(label.Root.rect.height / 2) - measureHeight / 2));
            if (first.rectTransform.anchoredPosition != position)
                throw new InvalidOperationException("ProView native integer centre differs: " + value);
        }

        private IEnumerator ProViewFontSmoke()
        {
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-rouletteProViewFontSmoke");
            if (option < 0 || option + 1 >= args.Length || args[option + 1].StartsWith("-"))
                throw new ArgumentException("ProView font capture directory is missing");
            string directory = args[option + 1];
            System.IO.Directory.CreateDirectory(directory);
            yield return null;
            VerifyRendererFontAtlases();
            temporaryHistory = true;
            recordedPockets.Clear(); recordedSnapshots.Clear(); recordedRoundPages.Clear();
            ++recordedHistoryVersion;
            var source = new RouletteSession(session.Profile, 1234567,
                new FixedPocketSmokeOutcome(0));
            source.PlaceBet(100, 1);
            RoundResult result = source.Spin();
            RecordPocket(0, result, source);
            session = source;
            RefreshPortraitUi();
            yield return null;
            RequireTotalWinNative(uiTotalWinAmount, "(36)", 326);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_total_native.png"));
            RequireTotalWinNative(uiTotalWinAmount, "(12345678)", 115);
            RequireTotalWinNative(uiTotalWinAmount, "(123456789012)", -23);
            RefreshPortraitUi();
            RequireProViewFont(uiCredit, "123456", footerCreditMeterFonts[0], 28, 36);
            RequireProViewFont(uiBet, "12345", footerCreditMeterFonts[0], 28, 36);
            RequireProViewFont(uiPaid, "1234567", footerPaidMeterFonts[0], 34, 48);
            uiFooterMovie.gameObject.SetActive(false);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_large.png"));
            RequireProViewFont(uiCredit, "1234567", footerCreditMeterFonts[1], 22, 36);
            RequireProViewFont(uiBet, "123456", footerCreditMeterFonts[1], 22, 36);
            RequireProViewFont(uiPaid, "12345678", footerPaidMeterFonts[1], 26, 48);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_narrow.png"));
            RequireProViewFont(uiCredit, "1234567890", footerCreditMeterFonts[1], 22, 36);
            RequireProViewFont(uiCreditTitle, "CREDITS", footerCreditTitleFonts[0], 10, 19, -1, measureHeight: 21);
            RequireProViewFont(uiBetTitle, "BET", footerCreditTitleFonts[0], 9, 19, -1, measureHeight: 21);
            RequireProViewFont(uiWinTitle, "WIN", footerWinTitleFonts[0], 17, 25, -1);
            RequireProViewFont(uiOneCredit, "1 CREDIT", footerOneCreditFonts[1], 9, 16, measureHeight: 21);
            RequireProViewFont(uiCreditTitle, "CREDITS LONG", footerCreditTitleFonts[1], 7, 16, -1, measureHeight: 21);
            RequireProViewFont(uiWinTitle, "WINWINWIN", footerWinTitleFonts[1], 16, 19, -1, measureHeight: 21);
            RequireProViewFont(uiDenom, "1\u00a2", footerDenomFonts[0], 32, 68);
            RequireProViewFont(uiDenom, "25\u00a2", footerDenomFonts[1], 19, 68);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_title_fallback.png"));
            uiCreditTitle.Set("CREDITS");
            uiWinTitle.Set("WIN");
            var boundary = ProViewLabel(portraitRoot, new Rect(0, 0, 196, 45),
                footerCreditMeterFonts, "Source strict width boundary");
            RequireProViewFont(boundary, "1234567", footerCreditMeterFonts[1], 22, 36);
            boundary.Root.sizeDelta = new Vector2(197, 45);
            boundary.Set("");
            RequireProViewFont(boundary, "1234567", footerCreditMeterFonts[0], 28, 36);
            boundary.Root.gameObject.SetActive(false);
            Destroy(boundary.Root.gameObject);
            RefreshPortraitUi();
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_live.png"));
            RequireProViewFont(uiCashCredit, "RM10.00", footerCashCreditFont, 7, 18, measureHeight: 19);
            RequireProViewFont(uiCashBet, "RM10.00", footerCashCreditFont, 7, 18, measureHeight: 19);
            RequireProViewFont(uiCashPaid, "RM0.36", footerCashPaidFont, 10, 19);
            RequireProViewFont(uiGameOver, "GAME OVER", footerMessageFont, 10, 21);
            RequireProViewFont(uiLinePays, "Straight Up Pays 36", footerMessageFont, 9, 21, -1);
            RequireProViewFont(uiGamePays, "Game Pays 36", footerMessageFont, 10, 21, 1);
            RequireProViewFont(uiTouch, "TOUCH TO CHANGE", footerTouchFont, 6, 11, measureHeight: 16);
            RequireProViewFont(uiDisclaimer, "MALFUNCTION VOIDS ALL PAYS AND PLAYS",
                footerDisclaimerFont, 10, 13);
            uiFooterMovie.gameObject.SetActive(false);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "main_native_labels.png"));
            RefreshPortraitUi();
            SetHistoryView(true);
            yield return null;
            RequireTotalWinNative(historyTotalWinAmount, "(36)", 326);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "history_total_native.png"));
            RequireTotalWinNative(historyTotalWinAmount, "(12345678)", 115);
            RequireTotalWinNative(historyTotalWinAmount, "(123456789012)", -23);
            RefreshHistoryWinCycle();
            RequireProViewFont(historyCredit, "123456", footerCreditMeterFonts[0], 28, 36);
            RequireProViewFont(historyBet, "12345", footerCreditMeterFonts[0], 28, 36);
            RequireProViewFont(historyPaid, "1234567", footerPaidMeterFonts[0], 34, 48);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "history_large.png"));
            RequireProViewFont(historyCredit, "1234567", footerCreditMeterFonts[1], 22, 36);
            RequireProViewFont(historyBet, "123456", footerCreditMeterFonts[1], 22, 36);
            RequireProViewFont(historyPaid, "12345678", footerPaidMeterFonts[1], 26, 48);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "history_narrow.png"));
            RequireProViewFont(historyCreditTitle, "CREDITS", footerCreditTitleFonts[0], 10, 19, -1, measureHeight: 21);
            RequireProViewFont(historyBetTitle, "BET", footerCreditTitleFonts[0], 9, 19, -1, measureHeight: 21);
            RequireProViewFont(historyWinTitle, "WIN", footerWinTitleFonts[0], 17, 25, -1);
            RequireProViewFont(historyCashCredit, "RM10.00", footerCashCreditFont, 7, 18, measureHeight: 19);
            RequireProViewFont(historyCashBet, "RM10.00", footerCashCreditFont, 7, 18, measureHeight: 19);
            RequireProViewFont(historyCashPaid, "RM0.36", footerCashPaidFont, 10, 19);
            RequireProViewFont(historyLinePays, "Straight Up Pays 36", footerMessageFont, 9, 21, -1);
            RequireProViewFont(historyGamePays, "Game Pays 36", footerMessageFont, 10, 21, 1);
            if (historyGamePays.Root.sizeDelta != new Vector2(200, 28))
                throw new InvalidOperationException("History Game Pays has the wrong source box height");
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory, "history_native_labels.png"));
            SetHistoryView(false);
            yield return null;
            if (uiCredit.Value != DisplayedCredit.ToString() || uiBet.Value != "1")
                throw new InvalidOperationException("Font test replaced live meter values");
            Debug.Log("ROULETTE_PROVIEW_FONT_CHOICE_PASSED: main/history, large/narrow, strict width, native centres, overflow fallback");
            Debug.Log("ROULETTE_PROVIEW_NATIVE_LABELS_PASSED: left titles, cash, flashing phrase, left/right pays, touch and disclaimer");
            Debug.Log("ROULETTE_TOTAL_WIN_NATIVE_PASSED: main/history, native 139px, integer centre, top anchor, long-value overflow");
            Application.Quit();
        }
    }
}
