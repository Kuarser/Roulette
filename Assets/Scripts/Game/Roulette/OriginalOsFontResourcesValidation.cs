using System;
using System.Globalization;
using UnityEngine;

namespace RouletteDeluxe
{
    // Resource verification and native missing-page condition. Native fatal
    // Fault delivery and LC_CTYPE byte decoding are separate host dependencies.
    public static class OriginalOsFontResourcesValidation
    {
        public static void Validate()
        {
            TextAsset fixture = Resources.Load<TextAsset>("Original/Fonts/OS366J/source_glyph_reference");
            if (fixture == null) throw new Exception("OS source glyph reference missing");
            int fonts = 0, glyphs = 0;
            foreach (string row in fixture.text.Split('\n'))
            {
                if (row.Length == 0) continue;
                string[] fields = row.TrimEnd('\r').Split(' ');
                OriginalBitmapFont font = OriginalBitmapFont.Load(fields[1]);
                if (font == null) throw new Exception("OS font not loadable: " + fields[1]);
                if (fields[0] == "F")
                {
                    if (font.Height != Parse(fields[2]) || font.PageCount != Parse(fields[3]) ||
                        font.GlyphCount != Parse(fields[4])) throw new Exception("OS font dimensions differ: " + fields[1]);
                    ++fonts;
                }
                else if (fields[0] == "G")
                {
                    int code = Parse(fields[2]), expected = Parse(fields[3]), width;
                    Texture2D texture; Rect uv;
                    if (!font.TryGetGlyph(code, out texture, out uv, out width) || texture == null || width != expected)
                        throw new Exception("OS declared glyph differs: " + fields[1] + " " + code);
                    ++glyphs;
                }
                else throw new Exception("Unknown OS font reference row");
            }
            if (fonts != 14 || glyphs != 42843) throw new Exception("Incomplete OS font corpus");
            Debug.Log("OS_FONT_RESOURCES_VALIDATED: fonts=" + fonts + " declaredGlyphs=" + glyphs);
            ValidateMissingPages(fixture.text);
        }
        [Serializable] private sealed class PageMetadata
        { public int page; public bool threadedVideoMissingPageFault; public PageMetadata[] pages; }
        private static void ValidateMissingPages(string reference)
        {
            int faults = 0;
            foreach (string row in reference.Split('\n'))
            {
                if (!row.StartsWith("F ", StringComparison.Ordinal)) continue;
                string name = row.Split(' ')[1];
                TextAsset source = Resources.Load<TextAsset>("Original/Fonts/" + name + "_metrics");
                PageMetadata metadata = JsonUtility.FromJson<PageMetadata>(source.text);
                if (!metadata.threadedVideoMissingPageFault) throw new Exception("OS page fault policy absent");
                var available = new System.Collections.Generic.HashSet<int> { metadata.page };
                if (metadata.pages != null) foreach (PageMetadata page in metadata.pages) available.Add(page.page);
                int missing = 0;
                while (missing < 256 && available.Contains(missing)) ++missing;
                if (missing == 256) throw new Exception("Missing-page test has no absent BMP page");
                OriginalBitmapFont font = OriginalBitmapFont.Load(name);
                int code = (missing << 8) | 0x41;
                bool rejected = false;
                try { font.MeasureWidth(char.ConvertFromUtf32(code)); }
                catch (OriginalBitmapFontPageFault error)
                { rejected = error.CodePoint == code && error.FontResource == name && OriginalBitmapFontPageFault.NativeFaultCode == 0x10; }
                if (!rejected) throw new Exception("OS missing page silently substituted: " + name);
                ++faults;
            }
            OriginalBitmapFont legacy = OriginalBitmapFont.Load("cash_credit");
            Rect a, b; int first, fallback;
            if (legacy == null || !legacy.TryGetGlyph(0x30, out a, out first) ||
                !legacy.TryGetGlyph(0xff30, out b, out fallback) || first != fallback || a != b)
                throw new Exception("MultisvrRenderer missing-page fallback changed");
            Debug.Log("OS_FONT_MISSING_PAGE_VALIDATED: fonts=" + faults + " legacyFallback=1");
        }
        private static int Parse(string value) { return int.Parse(value, CultureInfo.InvariantCulture); }
    }
}
