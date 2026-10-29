using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    internal sealed class OriginalBitmapFontPageFault : InvalidOperationException
    {
        public const uint NativeFaultCode = 0x10;
        public readonly int CodePoint;
        public readonly string FontResource;
        public OriginalBitmapFontPageFault(string resource, int codePoint)
            : base("Original threadedvideo font page missing: " + resource + " code=" + codePoint)
        { FontResource = resource; CodePoint = codePoint; }
    }

    internal sealed class OriginalBitmapFont
    {
        [Serializable]
        private sealed class Metrics
        {
            public int[] codes;
            public int[] starts;
            public int[] ends;
            public int width;
            public int height;
            public int measureHeight;
            public bool rendererGeometry;
            public int[] delimiter;
            public Metrics[] pages;
            public string resource;
            public bool nativePacking, unicode;
            public bool threadedVideoMissingPageFault;
            public int page, atlasWidth, atlasHeight;
            public int[] glyphRects, copyRects;
            public string atlasKey;
        }

        private struct Glyph
        {
            public Page Page;
            public Rect Uv;
            public int Width;
        }

        private sealed class Page
        {
            public Metrics Metrics;
            public string Resource;
            public Texture2D Source;
            public Texture2D Rendered;

            public Texture2D GetTexture()
            {
                if (Rendered != null) return Rendered;
                if (prepared.TryGetValue(Metrics.atlasKey, out Rendered)) return Rendered;
                if (Source == null) Source = Resources.Load<Texture2D>("Original/Fonts/" + Resource);
                if (Source == null || Source.width != Metrics.width || Source.height != Metrics.height)
                    throw new InvalidOperationException("Source font page differs: " + Resource);
                Color32[] input = Source.GetPixels32();
                int row = (Metrics.height - 1) * Metrics.width;
                int delimiter = Rgb16(Metrics.delimiter[0], Metrics.delimiter[1], Metrics.delimiter[2]);
                for (int x = 0; x < Metrics.width; ++x)
                {
                    Color32 p = input[row+x];
                    if ((p.a == 0 ? 0 : Rgb16(p.r, p.g, p.b)) == delimiter || x == Metrics.width-1)
                        input[row+x] = new Color32(0, 0, 0, 0);
                }
                // CreateEmpty clears every texel. CopyImage uploads each
                // actual rectangle, excluding the source's terminal column.
                Color32[] output = new Color32[Metrics.atlasWidth * Metrics.atlasHeight];
                int sourceX = 0;
                for (int i = 0; i < Metrics.copyRects.Length; i += 4)
                {
                    int left = Metrics.copyRects[i], top = Metrics.copyRects[i+1];
                    int right = Metrics.copyRects[i+2], bottom = Metrics.copyRects[i+3];
                    int width = right-left, height = bottom-top;
                    // glTexSubImage2D rejects a whole upload outside the texture.
                    if (left >= 0 && top >= 0 && right <= Metrics.atlasWidth && bottom <= Metrics.atlasHeight)
                        for (int y = 0; y < height; ++y)
                            Array.Copy(input, (Metrics.height-1-y)*Metrics.width+sourceX,
                                output, (Metrics.atlasHeight-1-top-y)*Metrics.atlasWidth+left, width);
                    sourceX += width;
                }
                Rendered = new Texture2D(Metrics.atlasWidth, Metrics.atlasHeight, TextureFormat.RGBA32, false) {
                    name = Resource + " renderer font", filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                Rendered.SetPixels32(output);
                Rendered.Apply(false, true);
                prepared.Add(Metrics.atlasKey, Rendered);
                return Rendered;
            }
        }

        private readonly Page firstPage;
        private readonly Texture2D sourceTexture;
        private readonly Metrics metrics;
        private readonly Dictionary<int, Glyph> glyphs = new Dictionary<int, Glyph>();
        private readonly HashSet<int> pages = new HashSet<int>();
        private readonly int defaultPage;
        private static readonly Dictionary<string, Texture2D> prepared = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, OriginalBitmapFont> loaded =
            new Dictionary<string, OriginalBitmapFont>();

        public Texture2D Texture { get { return firstPage.GetTexture(); } }
        public Texture2D SourceTexture { get { return sourceTexture; } }
        public int Height { get { return metrics.measureHeight; } }
        public int PageCount { get { return 1 + (metrics.pages == null ? 0 : metrics.pages.Length); } }
        public int GlyphCount { get { return glyphs.Count; } }
        // FontUTF8TextureImageHandle draws every page into the font's
        // maximum ascender height, mapping that page's complete glyph UV.
        public int GlyphHeight { get { return metrics.measureHeight; } }

        // ScaledLabel::BitMapScaledLabel::Scale selects a native bitmap;
        // the configured width is a strict bound, and ties keep the first font.
        public static OriginalBitmapFont SelectForWidth(OriginalBitmapFont[] fonts,
            string value, float maximumWidth)
        {
            OriginalBitmapFont fitting = null, smallest = null;
            int widest = -1, narrowest = int.MaxValue;
            foreach (OriginalBitmapFont candidate in fonts)
            {
                if (candidate == null) continue;
                int width = candidate.MeasureWidth(value);
                if (width > widest && (maximumWidth == 0 || width < maximumWidth))
                {
                    widest = width;
                    fitting = candidate;
                }
                if (width < narrowest)
                {
                    narrowest = width;
                    smallest = candidate;
                }
            }
            return fitting ?? smallest;
        }

        public int MeasureWidth(string value)
        {
            // SFontData::TextSize stores the accumulated width in a short.
            short width = 0;
            foreach (int c in CharacterCodes(value))
            {
                Glyph glyph;
                if (FindGlyph(c, out glyph)) width = unchecked((short)(width + glyph.Width));
            }
            return width;
        }

        public int DrawWidth(string value)
        {
            int width = 0;
            foreach (int c in CharacterCodes(value))
            {
                Glyph glyph;
                if (FindGlyph(c, out glyph)) width += glyph.Width;
            }
            return width;
        }

        public int[] CharacterCodes(string value)
        {
            if (string.IsNullOrEmpty(value)) return new int[0];
            int nul = value.IndexOf('\0');
            if (nul >= 0) value = value.Substring(0, nul);
            var result = new List<int>(value.Length);
            for (int i = 0; i < value.Length; ++i)
            {
                int c = value[i];
                if (char.IsHighSurrogate(value[i]))
                {
                    if (++i >= value.Length || !char.IsLowSurrogate(value[i])) return new int[0];
                    c = char.ConvertToUtf32(value[i-1], value[i]);
                }
                else if (char.IsLowSurrogate(value[i])) return new int[0];
                result.Add(c);
            }
            if (metrics.unicode) return result.ToArray();
            // Non-Unicode picture fonts index the bytes of the C UTF-8 string.
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            var codes = new int[bytes.Length];
            for (int i = 0; i < bytes.Length; ++i) codes[i] = bytes[i];
            return codes;
        }

        private bool FindGlyph(int value, out Glyph glyph)
        {
            // MultisvrRenderer falls back to its first page. threadedvideo
            // calls bally::Fault(0x10) instead; expose that condition to the
            // managed host without silently selecting another glyph page.
            int page = value >> 8;
            if (metrics.unicode && !pages.Contains(page))
            {
                if (metrics.threadedVideoMissingPageFault)
                    throw new OriginalBitmapFontPageFault(firstPage.Resource, value);
                value = (defaultPage << 8) | (value & 255);
            }
            return glyphs.TryGetValue(value, out glyph);
        }

        public bool TryGetGlyph(char value, out Rect uv, out int width)
        {
            Texture2D ignored;
            return TryGetGlyph(value, out ignored, out uv, out width);
        }

        public bool TryGetGlyph(char value, out Texture2D glyphTexture, out Rect uv, out int width)
        {
            return TryGetGlyph((int)value, out glyphTexture, out uv, out width);
        }

        public bool TryGetGlyph(int value, out Rect uv, out int width)
        {
            Texture2D ignored;
            return TryGetGlyph(value, out ignored, out uv, out width);
        }

        public bool TryGetGlyph(int value, out Texture2D glyphTexture, out Rect uv, out int width)
        {
            Glyph glyph;
            if (!FindGlyph(value, out glyph))
            {
                glyphTexture = null;
                uv = default(Rect);
                width = 0;
                return false;
            }
            glyphTexture = glyph.Page.GetTexture();
            width = glyph.Width;
            uv = glyph.Uv;
            return true;
        }

        private OriginalBitmapFont(Texture2D texture, Metrics metrics)
        {
            sourceTexture = texture;
            this.metrics = metrics;
            firstPage = AddPage(texture, metrics);
            int lowest = metrics.page;
            if (metrics.pages != null)
                foreach (Metrics page in metrics.pages)
                {
                    AddPage(null, page);
                    lowest = Math.Min(lowest, page.page);
                }
            defaultPage = lowest;
        }

        private Page AddPage(Texture2D texture, Metrics metrics)
        {
            var page = new Page { Metrics = metrics, Resource = metrics.resource, Source = texture };
            pages.Add(metrics.page);
            for (int i = 0; i < metrics.codes.Length; ++i)
            {
                int left = metrics.glyphRects[4*i], top = metrics.glyphRects[4*i+1];
                int right = metrics.glyphRects[4*i+2], bottom = metrics.glyphRects[4*i+3];
                int width = right-left;
                glyphs.Add(metrics.codes[i], new Glyph { Page = page, Width = width,
                    Uv = new Rect((float)left / metrics.atlasWidth, 1f-(float)bottom/metrics.atlasHeight,
                        (float)width / metrics.atlasWidth, (float)(bottom-top)/metrics.atlasHeight) });
            }
            return page;
        }

        private static int Rgb16(int r, int g, int b)
        {
            return (r >> 3) << 11 | (g >> 2) << 5 | b >> 3;
        }

        public static OriginalBitmapFont Load(string name)
        {
            OriginalBitmapFont cached;
            if (loaded.TryGetValue(name, out cached)) return cached;
            string path = "Original/Fonts/" + name;
            Texture2D texture = Resources.Load<Texture2D>(path);
            TextAsset metadata = Resources.Load<TextAsset>(path + "_metrics");
            if (texture == null || metadata == null) return null;
            Metrics metrics = JsonUtility.FromJson<Metrics>(metadata.text);
            if (!ValidPage(metrics, metrics == null ? 0 : metrics.measureHeight) ||
                texture.width != metrics.width || texture.height != metrics.height) return null;
            if (metrics.pages != null)
                foreach (Metrics page in metrics.pages)
                    if (page == null || string.IsNullOrEmpty(page.resource) ||
                        !ValidPage(page, metrics.measureHeight)) return null;
            var result = new OriginalBitmapFont(texture, metrics);
            loaded.Add(name, result);
            return result;
        }

        private static bool ValidPage(Metrics metrics, int measureHeight)
        {
            if (metrics == null || metrics.codes == null || metrics.starts == null ||
                metrics.ends == null || metrics.codes.Length != metrics.starts.Length ||
                metrics.codes.Length != metrics.ends.Length || !metrics.rendererGeometry || !metrics.nativePacking ||
                metrics.width <= 0 || metrics.height <= 0 || metrics.atlasWidth <= 0 || metrics.atlasHeight <= 0 ||
                metrics.glyphRects == null || metrics.glyphRects.Length != 4*metrics.codes.Length ||
                metrics.copyRects == null || metrics.copyRects.Length % 4 != 0 || string.IsNullOrEmpty(metrics.atlasKey) ||
                measureHeight < metrics.height || metrics.delimiter == null ||
                metrics.delimiter.Length != 3) return false;
            for (int i = 0; i < metrics.codes.Length; ++i)
                if (metrics.codes[i] < 0 || metrics.codes[i] > 65535 ||
                    metrics.starts[i] < 0 || metrics.ends[i] < metrics.starts[i]) return false;
            return true;
        }

        public void Draw(string value, Rect bounds, int horizontalAlign = 0,
            bool nativeSize = false)
        {
            float naturalWidth = DrawWidth(value);
            if (naturalWidth <= 0) return;
            float naturalHeight = Height;
            float scale = nativeSize ? 1f :
                Mathf.Min(bounds.width / naturalWidth, bounds.height / naturalHeight);
            float x = horizontalAlign < 0 ? bounds.x :
                horizontalAlign > 0 ? bounds.xMax - naturalWidth * scale :
                bounds.x + (bounds.width - naturalWidth * scale) * .5f;
            float y = bounds.y + (bounds.height - naturalHeight * scale) * .5f;
            if (nativeSize)
            {
                if (horizontalAlign == 0)
                    x = bounds.x + Mathf.Floor(bounds.width / 2f) - Mathf.Floor(naturalWidth / 2f);
                y = bounds.y + Mathf.Floor(bounds.height / 2f) - Mathf.Floor(naturalHeight / 2f);
            }
            foreach (int c in CharacterCodes(value))
            {
                Glyph glyph;
                if (!FindGlyph(c, out glyph)) continue;
                GUI.DrawTextureWithTexCoords(new Rect(x, y, glyph.Width * scale, GlyphHeight * scale),
                    glyph.Page.GetTexture(), glyph.Uv);
                x += glyph.Width * scale;
            }
        }
    }
}
