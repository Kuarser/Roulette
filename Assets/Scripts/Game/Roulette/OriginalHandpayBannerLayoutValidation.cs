using System;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalHandpayBannerLayoutValidation
    {
        public static int Validate()
        {
            int checks = 0;
            Action<bool> check = value => { if (!value) throw new Exception("Handpay banner layout regression " + checks); ++checks; };
            var layout = new OriginalHandpayBannerLayout(800, 600); var pixels = layout.Pixels();
            check(pixels.X == 174 && pixels.Y == 243 && pixels.Width == 445 && pixels.Height == 92);
            check(pixels.Body.Left == 174 && pixels.Body.Top == 243 && pixels.Body.Right == 619 && pixels.Body.Bottom == 335);
            check(pixels.Border.Left == 169 && pixels.Border.Top == 238 && pixels.Border.Right == 624 && pixels.Border.Bottom == 340);
            check(pixels.Window.Left == 0 && pixels.Window.Top == 0 && pixels.Window.Right == 800 && pixels.Window.Bottom == 600);
            check(OriginalHandpayBannerLayout.NativeOrder == 9100 && OriginalHandpayBannerLayout.BackgroundRgb == 255 && OriginalHandpayBannerLayout.BorderRgb == 0xffff00);
            var tall = new OriginalHandpayBannerLayout(540, 960); pixels = tall.Pixels();
            check(pixels.X == 117 && pixels.Y == 391 && pixels.Width == 299 && pixels.Height == 148);
            check(pixels.Border.Left == 112 && pixels.Border.Top == 386 && pixels.Border.Right == 421 && pixels.Border.Bottom == 544);
            var small = new OriginalHandpayBannerLayout(7, 7); pixels = small.Pixels();
            check(pixels.X == 0 && pixels.Y == 0 && pixels.Width == 1 && pixels.Height == 0);
            layout.ClipX = 100; layout.ClipY = 50; pixels = layout.Pixels();
            check(pixels.X == 152 && pixels.Y == 223 && pixels.Width == 389 && pixels.Height == 85);
            check(pixels.Window.Left == 100 && pixels.Window.Top == 50);
            layout = new OriginalHandpayBannerLayout(800, 600); layout.NormalizedX = -0.01f; layout.NormalizedY = -0.01f; pixels = layout.Pixels();
            check(pixels.X == -7 && pixels.Y == -5 && pixels.Border.Left == 0 && pixels.Border.Top == 0);
            layout.BorderWidth = 0; check(!layout.Pixels().ShowBorder);
            layout = new OriginalHandpayBannerLayout(32767, 32767); layout.NormalizedX = 1; layout.NormalizedWidth = 1; pixels = layout.Pixels();
            check(pixels.Body.Right == -12 && pixels.Border.Right == 32767);
            layout = new OriginalHandpayBannerLayout(800, 600);
            byte[] text = { 65, 0, 66 }; check(layout.AddSingleLine(text, 3, 20, true)); text[0] = 90;
            check(layout.Lines[0].Text.Length == 1 && layout.Lines[0].Text[0] == 65 && layout.CurrentSpacing == 6);
            check(layout.AddSingleLine(Encoding.ASCII.GetBytes("second"), 4, 20, true));
            pixels = layout.Align((slot, bytes) => { bytes[0] = 88; return 100; });
            check(layout.Lines[0].Text[0] == 65 && layout.Lines[0].AnchorX == 222 && layout.Lines[0].AnchorY == 21);
            check(layout.Lines[1].AnchorX == 222 && layout.Lines[1].AnchorY == 47 && pixels.Width == 445 && pixels.Height == 92);
            check(layout.Lines[0].HorizontalAlignment == 0 && layout.Lines[0].VerticalAlignment == 0);
            layout = new OriginalHandpayBannerLayout(800, 600); layout.Centering = true;
            layout.AddSingleLine(new byte[0], 0, 20, false); layout.AddSingleLine(new byte[0], 0, 20, false);
            pixels = layout.Align((slot, bytes) => { throw new Exception("Null native bitmap font must not be measured"); });
            check(layout.Lines[0].AnchorY == 5 && layout.Lines[1].AnchorY == 25 && layout.Lines[0].VerticalAlignment == -1);
            layout = new OriginalHandpayBannerLayout(800, 600); layout.AddSingleLine(new byte[0], 0, 20, true); layout.Centering = true;
            layout.Align((slot, bytes) => 0); check(layout.Lines[0].AnchorY == 21 && layout.Lines[0].Spacing == 6 && layout.Lines[0].VerticalAlignment == -1);
            layout = new OriginalHandpayBannerLayout(800, 600); layout.AddSingleLine(new byte[200], 12, 99, true);
            // Embedded NUL yields empty native string, including in long byte arrays.
            check(layout.Lines[0].Text.Length == 0);
            byte[] longText = new byte[200]; for (int i = 0; i < longText.Length; ++i) longText[i] = 65;
            layout.AddSingleLine(longText, 12, 99, true); check(layout.Lines[1].Text.Length == 127);
            pixels = layout.Align((slot, bytes) => 700);
            check(pixels.Height == 269 && pixels.Width == 699);
            check(layout.Lines[0].AnchorX == 222 && layout.Lines[0].AnchorY == 87 && layout.Lines[1].AnchorY == 219);
            check(layout.NormalizedWidth > 0.88f && layout.NormalizedHeight > 0.452f);
            layout = new OriginalHandpayBannerLayout(800, 600);
            for (int i = 0; i < 32; ++i) check(layout.AddSingleLine(new byte[0], 0, 20, false));
            check(!layout.AddSingleLine(new byte[0], 1, 72, false) && layout.Lines.Count == 32 && layout.CurrentFontHeight == 72 && layout.CurrentSpacing == 24);
            pixels = layout.Align((slot, bytes) => 0);
            // Required837 is stored as a float32 ratio; native truncation yields836.
            check(layout.CurrentFontHeight == 20 && pixels.Height == 836 && layout.Lines[31].AnchorY == 827);
            return checks;
        }
    }
}
