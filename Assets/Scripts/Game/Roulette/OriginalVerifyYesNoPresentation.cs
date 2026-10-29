using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    // Explicit viewport and LC_CTYPE decoder: no machine/display/financial
    // ownership is inferred here. The caller owns event capture and results.
    public sealed class OriginalVerifyYesNoPresentation
    {
        private sealed class Label
        { public string Text; public OriginalBitmapFont Font; public Rect Rectangle; }
        private readonly List<Label> labels = new List<Label>();
        private readonly List<Label> yesLabels = new List<Label>(), noLabels = new List<Label>();
        private readonly Texture2D[] frames = new Texture2D[4];
        public readonly OriginalVerifyYesNoGeometry Geometry;
        public readonly int Width, Height;
        public readonly int NativeOrder;
        public readonly OriginalTextButtonPresentationState YesState = new OriginalTextButtonPresentationState();
        public readonly OriginalTextButtonPresentationState NoState = new OriginalTextButtonPresentationState();
        public readonly OriginalVerifyButtonInput Input;
        public void AttachNativeInput(OriginalVideoInputDispatch route, OriginalSpriteInputTarget yesTarget,
            OriginalSpriteInputTarget noTarget, bool ignoreDrags)
        {
            if (route == null) throw new ArgumentNullException("route");
            if (Geometry.ShowYes) { if (yesTarget == null) throw new ArgumentNullException("yesTarget"); route.AttachVerifyButton(yesTarget, Input, true, ignoreDrags); }
            if (Geometry.ShowNo) { if (noTarget == null) throw new ArgumentNullException("noTarget"); route.AttachVerifyButton(noTarget, Input, false, ignoreDrags); }
        }

        public OriginalVerifyYesNoPresentation(int width, int height, OriginalVerifyTextLayout text,
            OriginalVerifyTextLayout.Placement[] placement, string language,
            Func<string, string> nativeLowercase, Func<byte[], string> nativeDecode,
            byte[] yesText, byte[] noText, bool yes, bool no, int nativeOrder=12000)
        {
            if (text == null || placement == null || nativeDecode == null) throw new ArgumentNullException();
            if (placement.Length != text.Count) throw new ArgumentException("One placement per native line required");
            Width = width; Height = height; NativeOrder=nativeOrder;
            Input = new OriginalVerifyButtonInput(YesState, NoState);
            Geometry = new OriginalVerifyYesNoGeometry(width, height, text.Count, yes, no);
            for (int i = 0; i < text.Count; ++i)
            {
                var font = OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(language, text[i].FontSlot, nativeLowercase));
                if (font == null || font.Height != text[i].FontHeight) throw new InvalidOperationException("Native font missing or height differs from layout");
                var p = placement[i];
                labels.Add(MakeLabel(nativeDecode(text[i].Bytes), font, unchecked((short)p.X), unchecked((short)p.Y),
                    p.HorizontalAlignment, p.VerticalAlignment, Geometry.Panel.Left, Geometry.Panel.Top));
            }
            if (Geometry.ReturnImmediately) return;
            for (int i = 0; i < frames.Length; ++i)
            {
                frames[i] = Resources.Load<Texture2D>("Original/HandpayButtons/gold_120x60/frame_" + i);
                if (frames[i] == null || frames[i].width != 120 || frames[i].height != 60)
                    throw new InvalidOperationException("Native gold button frame missing/resized");
            }
            var buttonFont = OriginalBitmapFont.Load(OriginalVerifyFontSelection.ButtonResource(language, nativeLowercase));
            if (buttonFont == null) throw new InvalidOperationException("Native button font missing");
            if (Geometry.ShowYes) { AddButtonLabels(yesLabels, OriginalVerifyButtonText.Copy(yesText), buttonFont, nativeDecode, Geometry.YesButton); YesState.Init(); }
            if (Geometry.ShowNo) { AddButtonLabels(noLabels, OriginalVerifyButtonText.Copy(noText), buttonFont, nativeDecode, Geometry.NoButton); NoState.Init(); }
        }

        private static Label MakeLabel(string value, OriginalBitmapFont font, short x, short y,
            int horizontal, int vertical, int parentX, int parentY)
        {
            if (value == null) throw new InvalidOperationException("Native decoder returned null");
            OriginalSpriteAlignment.Rectangle rect;
            OriginalSpriteAlignment.Align(x, y, unchecked((short)font.MeasureWidth(value)), unchecked((short)font.Height),
                horizontal, vertical, 0, 0, out rect);
            return new Label { Text = value, Font = font, Rectangle = new Rect(parentX + rect.Left, parentY + rect.Top,
                unchecked((short)(rect.Right - rect.Left)), unchecked((short)(rect.Bottom - rect.Top))) };
        }

        private static void AddButtonLabels(List<Label> target, byte[] input, OriginalBitmapFont font,
            Func<byte[], string> decode, OriginalSpriteAlignment.Rectangle button)
        {
            if (input == null) throw new ArgumentNullException("input");
            var anchors = OriginalTextButtonLabelGeometry.Layout(input, 120, 60);
            int end = Array.IndexOf(input, (byte)0); if (end < 0) end = input.Length;
            int start = 0, index = 0;
            for (int i = 0; i <= end; ++i)
                if (i == end || input[i] == 10)
                {
                    var bytes = new byte[i - start]; Array.Copy(input, start, bytes, 0, bytes.Length);
                    var a = anchors[index++];
                    target.Add(MakeLabel(decode(bytes), font, a.X, a.Y, a.HorizontalAlignment, a.VerticalAlignment, button.Left, button.Top));
                    start = i + 1;
                }
        }

        // Call within an IMGUI Repaint. Coordinates are native physical pixels.
        public void Draw()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            Color saved = GUI.color;
            try
            {
                DrawSurface((rect, texture, uv, color) => {
                    GUI.color = color; GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
                });
            }
            finally { GUI.color = saved; }
        }
        public void DrawTo(RenderTexture target)
        {
            if (target == null || target.width != Width || target.height != Height)
                throw new ArgumentException("Native viewport-sized render target required");
            var shader = Shader.Find("UI/Default");
            if (shader == null) throw new InvalidOperationException("UI texture shader unavailable");
            var material = new Material(shader);
            RenderTexture saved = RenderTexture.active;
            GL.PushMatrix();
            try
            {
                RenderTexture.active = target;
                GL.Clear(true, true, Color.clear);
                GL.LoadPixelMatrix(0, Width, Height, 0);
                DrawSurface((rect, texture, uv, color) => {
                    material.mainTexture = texture; material.SetColor("_Color", Color.white);
                    if (!material.SetPass(0)) throw new InvalidOperationException("UI texture shader pass failed");
                    GL.Begin(GL.QUADS); GL.Color(color);
                    GL.TexCoord2(uv.xMin, uv.yMax); GL.Vertex3(rect.xMin, rect.yMin, 0);
                    GL.TexCoord2(uv.xMax, uv.yMax); GL.Vertex3(rect.xMax, rect.yMin, 0);
                    GL.TexCoord2(uv.xMax, uv.yMin); GL.Vertex3(rect.xMax, rect.yMax, 0);
                    GL.TexCoord2(uv.xMin, uv.yMin); GL.Vertex3(rect.xMin, rect.yMax, 0);
                    GL.End();
                });
            }
            finally
            {
                GL.PopMatrix(); RenderTexture.active = saved;
                if (Application.isPlaying) UnityEngine.Object.Destroy(material); else UnityEngine.Object.DestroyImmediate(material);
            }
        }
        private void DrawSurface(Action<Rect, Texture2D, Rect, Color> draw)
        {
            var full = new Rect(0, 0, 1, 1);
            draw(Rectangle(Geometry.Border), Texture2D.whiteTexture, full, new Color(1, 1, 0, 1));
            draw(Rectangle(Geometry.Panel), Texture2D.whiteTexture, full, Color.black);
            foreach (var label in labels) DrawLabel(label, 0, draw);
            if (Geometry.ShowYes) DrawButton(Geometry.YesButton, YesState, yesLabels, draw);
            if (Geometry.ShowNo) DrawButton(Geometry.NoButton, NoState, noLabels, draw);
        }
        private void DrawButton(OriginalSpriteAlignment.Rectangle rect, OriginalTextButtonPresentationState state, List<Label> text,
            Action<Rect, Texture2D, Rect, Color> draw)
        {
            draw(Rectangle(rect), frames[state.State], new Rect(0, 0, 1, 1), Color.white);
            foreach (var label in text) DrawLabel(label, state.Pressed ? 2 : 0, draw);
        }
        private static Rect Rectangle(OriginalSpriteAlignment.Rectangle r)
        { return new Rect(r.Left, r.Top, unchecked((short)(r.Right - r.Left)), unchecked((short)(r.Bottom - r.Top))); }
        private static void DrawLabel(Label label, int offset, Action<Rect, Texture2D, Rect, Color> draw)
        {
            float x = label.Rectangle.x + offset, y = label.Rectangle.y + offset;
            foreach (int code in label.Font.CharacterCodes(label.Text))
            {
                Texture2D texture; Rect uv; int width;
                if (!label.Font.TryGetGlyph(code, out texture, out uv, out width)) continue;
                draw(new Rect(x, y, width, label.Font.GlyphHeight), texture, uv, Color.white);
                x += width;
            }
        }
    }
}
