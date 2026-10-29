using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class CompleteFontPage
        {
            public int[] codes, glyphRects;
            public int page, atlasWidth, atlasHeight;
            public CompleteFontPage[] pages;
        }

        private static Dictionary<string, string[]> fontSourceReferences;
        private static readonly HashSet<Texture2D> verifiedFontTextures = new HashSet<Texture2D>();
        private static long completeFontPixelCount;
        private static readonly uint[] fontCrcTable = MakeFontCrcTable();

        private static uint[] MakeFontCrcTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < 256; ++i)
            {
                uint value = i;
                for (int j = 0; j < 8; ++j) value = (value >> 1) ^ ((value & 1) == 0 ? 0 : 0xEDB88320u);
                table[i] = value;
            }
            return table;
        }

        private static uint FontCrcByte(uint crc, byte value)
        {
            return (crc >> 8) ^ fontCrcTable[(crc ^ value) & 255];
        }

        private static uint FontCrcInt(uint crc, int value)
        {
            for (int i = 0; i < 4; ++i) crc = FontCrcByte(crc, (byte)(value >> (i*8)));
            return crc;
        }

        private static CompleteFontPage CompleteFontMetadata(string name)
        {
            return JsonUtility.FromJson<CompleteFontPage>(Resources.Load<TextAsset>(
                "Original/Fonts/" + name + "_metrics").text);
        }

        private static void VerifyCompleteFontPage(string name, OriginalBitmapFont font, CompleteFontPage page)
        {
            if (fontSourceReferences == null)
                fontSourceReferences = Resources.Load<TextAsset>("Original/Fonts/complete_font_source_reference")
                    .text.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(row => row.Trim().Split(' ')).ToDictionary(row => row[0]+"/"+row[1]);
            string[] reference = fontSourceReferences[name+"/"+page.page];
            if (page.atlasWidth != int.Parse(reference[2]) || page.atlasHeight != int.Parse(reference[3]) ||
                page.codes.Length != int.Parse(reference[4]))
                throw new InvalidOperationException("Source font page dimensions/count differ: " + name+"/"+page.page);
            uint rectCrc = uint.MaxValue;
            Texture2D rendered = null;
            for (int i = 0; i < page.codes.Length; ++i)
            {
                int left = page.glyphRects[4*i], top = page.glyphRects[4*i+1];
                int right = page.glyphRects[4*i+2], bottom = page.glyphRects[4*i+3];
                Texture2D texture; Rect uv; int width;
                if (!font.TryGetGlyph(page.codes[i], out texture, out uv, out width) || width != right-left ||
                    uv != new Rect((float)left/page.atlasWidth, 1f-(float)bottom/page.atlasHeight,
                        (float)(right-left)/page.atlasWidth, (float)(bottom-top)/page.atlasHeight) ||
                    texture.width != page.atlasWidth || texture.height != page.atlasHeight ||
                    texture.filterMode != FilterMode.Bilinear || texture.wrapMode != TextureWrapMode.Clamp ||
                    (rendered != null && rendered != texture))
                    throw new InvalidOperationException("Source font glyph differs: "+name+"/"+page.codes[i]);
                rendered = texture;
                rectCrc = FontCrcInt(rectCrc, page.codes[i]);
                for (int j = 0; j < 4; ++j) rectCrc = FontCrcInt(rectCrc, page.glyphRects[4*i+j]);
            }
            if (~rectCrc != uint.Parse(reference[6], NumberStyles.HexNumber))
                throw new InvalidOperationException("Source font rectangle checksum differs: " + name+"/"+page.page);
            if (!verifiedFontTextures.Add(rendered)) return;
            Color32[] pixels = ReadFontGpuPixels(rendered);
            uint pixelCrc = uint.MaxValue;
            for (int y = rendered.height-1; y >= 0; --y)
                for (int x = 0; x < rendered.width; ++x)
                {
                    Color32 p = pixels[y*rendered.width+x];
                    pixelCrc = FontCrcByte(pixelCrc,p.r); pixelCrc = FontCrcByte(pixelCrc,p.g);
                    pixelCrc = FontCrcByte(pixelCrc,p.b); pixelCrc = FontCrcByte(pixelCrc,p.a);
                }
            if (~pixelCrc != uint.Parse(reference[5], NumberStyles.HexNumber))
                throw new InvalidOperationException("Source font GPU pixel checksum differs: " + name+"/"+page.page+
                    " actual="+(~pixelCrc).ToString("X8")+" expected="+reference[5]);
            completeFontPixelCount += pixels.Length;
        }

        private static void VerifyCompleteFont(string name)
        {
            OriginalBitmapFont font = OriginalBitmapFont.Load(name);
            CompleteFontPage metadata = CompleteFontMetadata(name);
            foreach (CompleteFontPage page in new[] {metadata}.Concat(metadata.pages))
                VerifyCompleteFontPage(name, font, page);
        }

        private IEnumerator CompleteFontSmoke()
        {
            yield return null;
            string fingerprint = PaytableTestLiveFingerprint(session);
            var names = Resources.Load<TextAsset>("Original/Fonts/complete_font_source_reference").text
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Split(' ')[0]).Distinct();
            int fonts = 0, pages = 0, glyphs = 0;
            foreach (string name in names)
            {
                OriginalBitmapFont font = OriginalBitmapFont.Load(name);
                CompleteFontPage metadata = CompleteFontMetadata(name);
                foreach (CompleteFontPage page in new[] {metadata}.Concat(metadata.pages))
                {
                    VerifyCompleteFontPage(name, font, page);
                    ++pages; glyphs += page.codes.Length;
                    yield return null;
                }
                ++fonts;
                Debug.Log("ROULETTE_COMPLETE_FONT_PROGRESS: " + name + " pages="+pages+" glyphs="+glyphs);
            }
            OriginalBitmapFont unicode = OriginalBitmapFont.Load("operator_9");
            Texture2D aTexture, fallbackTexture; Rect aUv, fallbackUv; int aWidth, fallbackWidth;
            if (!unicode.TryGetGlyph('A', out aTexture, out aUv, out aWidth) ||
                !unicode.TryGetGlyph(0x1F641, out fallbackTexture, out fallbackUv, out fallbackWidth) ||
                fallbackTexture != aTexture || fallbackUv != aUv || fallbackWidth != aWidth ||
                unicode.MeasureWidth("\U0001F641") != aWidth ||
                unicode.MeasureWidth("A\0B") != aWidth || unicode.MeasureWidth("A\uD800") != 0 ||
                unicode.MeasureWidth("\uDC00") != 0 || unicode.CharacterCodes("\U0001F641").Length != 1 ||
                unicode.TryGetGlyph(0x10FF, out fallbackTexture, out fallbackUv, out fallbackWidth))
                throw new InvalidOperationException("Native Unicode page fallback/string conversion differs");
            string longText = new string('A', 4000);
            if (unicode.DrawWidth(longText) != aWidth*4000 ||
                unicode.MeasureWidth(longText) != unchecked((short)(aWidth*4000)))
                throw new InvalidOperationException("Native short measurement/int drawing width differs");
            OriginalBitmapFont legacy = OriginalBitmapFont.Load("neighbor_pick");
            int[] legacyCodes = legacy.CharacterCodes("\u00A2\0A");
            if (legacyCodes.Length != 2 || legacyCodes[0] != 0xC2 || legacyCodes[1] != 0xA2 ||
                legacy.CharacterCodes("\uD800").Length != 0)
                throw new InvalidOperationException("Native non-Unicode UTF-8 byte/NUL conversion differs");
            var label = new PictureLabel(portraitRoot, new Rect(40, 400, 950, 100), unicode,
                "Native Unicode coverage", nativeSize: true);
            label.Set("A\u0401\u10DA\u20AB\u17DB\u0E3F\u0142\u062F\U0001F641");
            yield return null;
            if (label.GlyphCount != 9 || label.Root.GetComponentsInChildren<RawImage>()
                    .Select(glyph => glyph.texture).Distinct().Count() != 8)
                throw new InvalidOperationException("Unicode uGUI pages/supplementary glyph count differ");
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args,"-rouletteCompleteFontCapture");
            if (option >= 0) CaptureActiveScreenOffscreen(args[option+1]);
            label.Root.gameObject.SetActive(false); Destroy(label.Root.gameObject);
            if (PaytableTestLiveFingerprint(session) != fingerprint)
                throw new InvalidOperationException("Font coverage altered live session");
            Debug.Log("ROULETTE_COMPLETE_FONTS_PASSED: "+fonts+" fonts, "+pages+" pages, "+glyphs+
                " glyphs, "+verifiedFontTextures.Count+" unique GPU atlases, "+completeFontPixelCount+
                " source RGBA pixels; Unicode32/fallback/NUL/invalid text/short width/uGUI/session isolation");
            Application.Quit();
        }
    }
}
