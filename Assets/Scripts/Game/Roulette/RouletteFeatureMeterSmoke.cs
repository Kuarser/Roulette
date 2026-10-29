using System;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireFeatureMeterNative(PictureLabel label,
            string value, string fontName, Rect sourceBounds, int firstWidth, int height,
            int measureHeight = 0)
        {
            if (measureHeight == 0) measureHeight = height;
            if (label.Value != value || label.Root.sizeDelta != sourceBounds.size ||
                label.Root.anchoredPosition != new Vector2(sourceBounds.x, -sourceBounds.y) ||
                label.SelectedFont == null || label.SelectedFont.SourceTexture.height != height ||
                label.SelectedFont.GlyphHeight != measureHeight ||
                label.SelectedFont.Height != measureHeight ||
                label.SelectedFont.SourceTexture != Resources.Load<Texture2D>("Original/Fonts/" + fontName))
                throw new InvalidOperationException("Feature meter value, font or source bounds differ");
            int x = (int)sourceBounds.width / 2 - label.SelectedFont.MeasureWidth(value) / 2;
            int y = (int)sourceBounds.height / 2 - label.SelectedFont.Height / 2;
            int index = 0;
            int first = -1;
            int expectedGlyphs = 0, activeGlyphs = 0;
            Rect countedUv;
            int countedWidth;
            foreach (char c in value)
                if (label.SelectedFont.TryGetGlyph(c, out countedUv, out countedWidth)) ++expectedGlyphs;
            foreach (Transform child in label.Root)
            {
                if (!child.gameObject.activeSelf) continue;
                Rect uv = default(Rect);
                int width = 0;
                while (index < value.Length &&
                    !label.SelectedFont.TryGetGlyph(value[index], out uv, out width)) ++index;
                if (index >= value.Length)
                    throw new InvalidOperationException("Unexpected feature meter glyph");
                RawImage glyph = child.GetComponent<RawImage>();
                if (first < 0) first = width;
                if (glyph == null || glyph.raycastTarget ||
                    glyph.texture != label.SelectedFont.Texture || glyph.uvRect != uv ||
                    glyph.rectTransform.sizeDelta != new Vector2(width, measureHeight) ||
                    glyph.rectTransform.anchoredPosition != new Vector2(x, -y))
                    throw new InvalidOperationException("Feature meter glyph scaled or misplaced");
                x += width;
                ++index;
                ++activeGlyphs;
            }
            if (first != firstWidth || activeGlyphs != expectedGlyphs)
                throw new InvalidOperationException("Feature meter first glyph width differs from source fixture");
        }

        private void CaptureFeatureMeterFrame(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-rouletteFeatureMeterCaptureDirectory");
            if (option < 0 || option + 1 >= args.Length) return;
            System.IO.Directory.CreateDirectory(args[option + 1]);
            CaptureActiveScreenOffscreen(System.IO.Path.Combine(args[option + 1], name + ".png"));
        }
    }
}
