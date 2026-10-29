using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalVerifyYesNoRenderValidation
    {
        public static void Validate()
        {
            string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../outputs/native_handpay_layout_v1/yesno_render_v5"));
            if (Directory.Exists(output)) throw new Exception("Frozen render output exists");
            Directory.CreateDirectory(output);
            int checks = 0;
            foreach (var size in new[] { new Vector2Int(800, 600), new Vector2Int(540, 960) })
            {
                var font = OriginalBitmapFont.Load("OS366J/arial-bold-12-white");
                Func<byte[], string> ascii = bytes => {
                    foreach (byte b in bytes) if (b > 127) throw new Exception("ASCII fixture decoder only");
                    return Encoding.ASCII.GetString(bytes);
                };
                var translator = OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,
                    "Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));
                var locale=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
                var content=OriginalHandpaySelectionText.Build(translator,995,"en",true,
                    amount=>locale.FormatText(amount,100,false,0),Encoding.ASCII.GetBytes,null);
                var screen=OriginalHandpaySelectionPresentation.Create(size.x,size.y,content,value=>value,ascii,
                    (slot,bytes)=>(short)font.MeasureWidth(ascii(bytes)));
                if(screen.NativeOrder!=9125)throw new Exception("Native selection screen order differs");
                var target = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32);
                target.Create();
                try
                {
                    for (int state = 0; state < 4; ++state)
                    {
                        screen.YesState.Init(); screen.NoState.Init();
                        if (state == 1) { screen.YesState.Flash(true); screen.NoState.Flash(true); }
                        if (state == 2)
                        {
                            var root = new OriginalSpriteInputTarget { Flags = 1,
                                Rectangle = new OriginalSpriteAlignment.Rectangle { Right = (short)size.x, Bottom = (short)size.y } };
                            var yes = new OriginalSpriteInputTarget { Flags = 0x47, Rectangle = screen.Geometry.YesButton, Parent = root };
                            var no = new OriginalSpriteInputTarget { Flags = 0x47, Rectangle = screen.Geometry.NoButton, Parent = root };
                            root.Children.Add(yes); root.Children.Add(no);
                            var route = new OriginalVideoInputDispatch(node => true,
                                () => new OriginalVideoInputDispatch.ClockSample { Seconds = 1, Microseconds = 2 }, value => { });
                            route.Windows.Add(root); screen.AttachNativeInput(route, yes, no, true);
                            route.Dispatch(new OriginalVideoInputEvent { Type = 0x200001, Display = 1,
                                Point = new OriginalSpriteInputTarget.Point((short)(yes.Rectangle.Left + 5), (short)(yes.Rectangle.Top + 5)) });
                            route.Dispatch(new OriginalVideoInputEvent { Type = 0x200001, Display = 2,
                                Point = new OriginalSpriteInputTarget.Point((short)(no.Rectangle.Left + 5), (short)(no.Rectangle.Top + 5)) });
                            int result;
                            if (!screen.YesState.Pressed || !screen.NoState.Pressed || route.TouchCaptured(1) != yes ||
                                route.TouchCaptured(2) != no || !screen.Input.TryGetDisplayResult(out result) || result != 0)
                                throw new Exception("Native route did not drive presentation/result/capture");
                            Debug.Log("VERIFY_YESNO_ROUTED_GPU_INPUT_VALIDATED: viewport=" + size.x + "x" + size.y + " captures=2 result=0");
                        }
                        if (state == 3) { screen.YesState.Enabling(false); screen.NoState.Enabling(false); }
                        screen.DrawTo(target);
                        RenderTexture saved = RenderTexture.active;
                        var read = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                        try
                        {
                            RenderTexture.active = target; read.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0); read.Apply();
                            var panel = screen.Geometry.Panel;
                            Color border = read.GetPixel(panel.Left - 2, size.y - 1 - panel.Top);
                            Color background = read.GetPixel(panel.Left + 2, size.y - 1 - panel.Top);
                            File.WriteAllBytes(Path.Combine(output, size.x + "x" + size.y + "_state" + state + ".png"), read.EncodeToPNG());
                            Debug.Log("YESNO_RENDER_PIXELS border=" + border + " background=" + background);
                            if (border.r < 0.99f || border.g < 0.99f || border.b > 0.01f || background.maxColorComponent > 0.01f)
                                throw new Exception("Native panel/border GPU pixel mismatch");
                            ++checks;
                            Color outside = read.GetPixel(0, 0);
                            if (outside.a > 0.01f) throw new Exception("Unexpected drawing outside screen sprites");
                            ++checks;
                            int bright = 0;
                            for (int y = panel.Top + 2; y < panel.Top + 60; ++y)
                                for (int x = panel.Left + 5; x < panel.Right - 5; ++x)
                                {
                                    Color c = read.GetPixel(x, size.y - 1 - y);
                                    if (c.r > 0.5f && c.g > 0.5f && c.b > 0.5f) ++bright;
                                }
                            if (bright < 100) throw new Exception("Bitmap text absent from assembled GPU screen");
                            ++checks;
                            File.WriteAllBytes(Path.Combine(output, size.x + "x" + size.y + "_state" + state + ".png"), read.EncodeToPNG());
                        }
                        finally { RenderTexture.active = saved; UnityEngine.Object.DestroyImmediate(read); }
                    }
                }
                finally { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
            }
            File.WriteAllText(Path.Combine(output, "scope.txt"),
                "Unity2020.3.17f1 Editor GPU render; explicit ASCII/identity lowercase diagnostic fixtures.\n" +
                "Original font slot3, gold frames0..3, panel/root parenting and pressed text offset; translation keys and first-space newline now use original selection factory.\n" +
                "Selection amount995 and ms_MY.utf8 are explicit host fixtures; native money format gives RM9.95; no financial ownership.\n" +
                "Pressed state now uses routed native touch fixtures on explicit displays1/2; captures/result checked. Device/display binding remains unqualified.\n" +
                "No Player or production display binding; no formalv10 update.\n");
            Debug.Log("VERIFY_YESNO_GPU_RENDER_VALIDATED: captures=8 pixelChecks=" + checks + " output=" + output);
        }
    }
}
