using System;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private struct FontRendererFixture
        {
            public string Name;
            public int AtlasHeight, MeasureHeight, DigitsWidth, Delimiter;
            public FontRendererFixture(string name, int glyphHeight, int measureHeight,
                int digitsWidth, int delimiter)
            {
                Name = name; AtlasHeight = glyphHeight; MeasureHeight = measureHeight;
                DigitsWidth = digitsWidth; Delimiter = delimiter;
            }
        }

        private static Color32[] ReadFontGpuPixels(Texture2D texture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture rendered = RenderTexture.GetTemporary(texture.width, texture.height,
                0, RenderTextureFormat.ARGB32);
            Texture2D readback = new Texture2D(texture.width, texture.height,
                TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(texture, rendered);
                RenderTexture.active = rendered;
                readback.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rendered);
                Destroy(readback);
            }
        }

        private static void VerifyRendererFontAtlases()
        {
            // Literal dimensions/advances from original FNT+PNG recovery in
            // outputs/renderer_font_geometry_before.json, not PC rectangle sizes.
            FontRendererFixture[] fixtures = {
                new FontRendererFixture("cash_credit", 18, 19, 80, 63488),
                new FontRendererFixture("cash_meter", 36, 36, 280, 63488),
                new FontRendererFixture("cash_meter_small", 36, 36, 220, 63488),
                new FontRendererFixture("cash_paid", 19, 19, 90, 63488),
                new FontRendererFixture("cashbet", 19, 21, 100, 63488),
                new FontRendererFixture("cashbet_narrow", 16, 21, 80, 63488),
                new FontRendererFixture("chip_value", 19, 19, 139, 63519),
                new FontRendererFixture("cycle_pay", 51, 51, 348, 63519),
                new FontRendererFixture("demo_menu", 17, 17, 90, 63488),
                new FontRendererFixture("denom", 68, 68, 323, 2016),
                new FontRendererFixture("denom_small", 68, 68, 190, 2016),
                new FontRendererFixture("double_history_final", 30, 30, 169, 63513),
                new FontRendererFixture("double_history_final_small", 26, 26, 117, 63545),
                new FontRendererFixture("double_history", 26, 26, 117, 63545),
                new FontRendererFixture("double_meter", 31, 31, 134, 63488),
                new FontRendererFixture("double_win_limit", 25, 25, 158, 63509),
                new FontRendererFixture("history_meter", 17, 17, 90, 63488),
                new FontRendererFixture("ideck_autoplay", 27, 27, 71, 63488),
                new FontRendererFixture("ideck_cashout", 27, 27, 80, 63488),
                new FontRendererFixture("ideck_confirm_notify", 41, 41, 105, 63488),
                new FontRendererFixture("ideck_confirm_query", 107, 107, 0, 63488),
                new FontRendererFixture("ideck_confirm_yes_no", 40, 41, 176, 63488),
                new FontRendererFixture("ideck_double_decision", 53, 53, 133, 63488),
                new FontRendererFixture("ideck_service", 27, 27, 80, 63488),
                new FontRendererFixture("ideck_title", 23, 23, 139, 63488),
                new FontRendererFixture("malfunction", 13, 13, 70, 63488),
                new FontRendererFixture("message", 21, 21, 90, 63488),
                new FontRendererFixture("neighbor_pick", 50, 50, 280, 2016),
                new FontRendererFixture("onecredit_alt", 16, 21, 90, 63488),
                new FontRendererFixture("onecredit", 16, 21, 85, 63488),
                new FontRendererFixture("paid_meter", 48, 48, 340, 63488),
                new FontRendererFixture("suit_cash_meter", 26, 28, 129, 63519),
                new FontRendererFixture("suit_meter", 26, 28, 129, 63519),
                new FontRendererFixture("topper_result", 276, 276, 2084, 63519),
                new FontRendererFixture("tote_green", 26, 26, 203, 63519),
                new FontRendererFixture("tote_red", 26, 26, 203, 63519),
                new FontRendererFixture("tote_white", 57, 57, 440, 63519),
                new FontRendererFixture("tote_yellow", 26, 26, 203, 63519),
                new FontRendererFixture("touch_to_change", 11, 16, 70, 63488),
                new FontRendererFixture("win_total", 139, 139, 714, 63519),
                new FontRendererFixture("wincollect", 25, 25, 120, 63488),
                new FontRendererFixture("wincollect_narrow", 19, 21, 120, 63488),
                new FontRendererFixture("winpaid", 48, 48, 340, 63488),
                new FontRendererFixture("winpaid_small", 48, 48, 260, 63488),
            };
            foreach (FontRendererFixture fixture in fixtures)
            {
                OriginalBitmapFont font = OriginalBitmapFont.Load(fixture.Name);
                if (font == null || font.SourceTexture.height != fixture.AtlasHeight ||
                    font.GlyphHeight != fixture.MeasureHeight || font.Height != fixture.MeasureHeight ||
                    font.MeasureWidth("1234567890") != fixture.DigitsWidth)
                    throw new InvalidOperationException("Renderer font measurement differs: " + fixture.Name);
                VerifyCompleteFontPage(fixture.Name, font, CompleteFontMetadata(fixture.Name));
            }
            Debug.Log("ROULETTE_RENDERER_FONT_PIXELS_PASSED: " + fixtures.Length +
                " source first pages; native packing/GPU pixel checksums/advances/max ascender heights");
        }
    }
}
