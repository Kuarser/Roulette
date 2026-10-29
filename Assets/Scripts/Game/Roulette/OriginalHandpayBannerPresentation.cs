using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    // Bitmap branch of TextScreen::Display/DisplayTextLines. Input, font decoding,
    // window display selection and event ownership remain caller dependencies.
    public sealed class OriginalHandpayBannerPresentation
    {
        private sealed class Label { public string Text; public OriginalBitmapFont Font; public Rect Rect; public int X,Y; }
        private readonly List<Label> labels = new List<Label>();
        private bool spritesPresent = true;
        public bool RootVisible = true;
        public void ClearNativeSprites() { labels.Clear(); spritesPresent = false; Layout.ClearNativeLines(); }
        public readonly OriginalHandpayBannerLayout Layout;
        public readonly OriginalHandpayBannerLayout.Geometry Geometry;
        public readonly int CenteringOffset;
        public string FontLanguage { get; private set; }
        public int LabelCount { get { return labels.Count; } }
        public string[] DescribeLabels()
        { var rows=new string[labels.Count];for(int i=0;i<labels.Count;++i)rows[i]=labels[i].Text+"|"+labels[i].Rect+"|"+labels[i].Font.Height;return rows; }
        public void Refresh(Func<int,byte[],short> nativeMeasureWidth)
        {
            if(!spritesPresent)throw new InvalidOperationException("Native body required for Refresh");
            var position=OriginalTextScreenRefresh.GetPosition(Layout);
            if(Geometry.ShowBorder&&Layout.BorderWidth>0)
            {
                int width=unchecked((short)(Geometry.Border.Right-Geometry.Border.Left)),height=unchecked((short)(Geometry.Border.Bottom-Geometry.Border.Top));
                Geometry.Border.Left=unchecked((short)position.BorderX);Geometry.Border.Top=unchecked((short)position.BorderY);
                Geometry.Border.Right=unchecked((short)(position.BorderX+width));Geometry.Border.Bottom=unchecked((short)(position.BorderY+height));
            }
            Layout.Align(nativeMeasureWidth);
            int dx=unchecked((short)position.X)-Geometry.Body.Left,dy=unchecked((short)position.Y)-Geometry.Body.Top;
            Geometry.Body.Left=unchecked((short)(Geometry.Body.Left+dx));Geometry.Body.Right=unchecked((short)(Geometry.Body.Right+dx));
            Geometry.Body.Top=unchecked((short)(Geometry.Body.Top+dy));Geometry.Body.Bottom=unchecked((short)(Geometry.Body.Bottom+dy));
            foreach(var label in labels)label.Rect=new Rect(label.Rect.x+dx,label.Rect.y+dy,label.Rect.width,label.Rect.height);
        }
        public OriginalHandpayBannerPresentation(OriginalHandpayBannerLayout layout, string language,
            Func<string, string> lowercase, Func<byte[], string> decode)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            if (decode == null) throw new ArgumentNullException(nameof(decode));
            Geometry = layout.Pixels();
            FontLanguage = language;
            int sum = 0; foreach (var line in layout.Lines) sum = unchecked(sum + line.Height);
            CenteringOffset = layout.Centering ? unchecked(layout.DisplayHeight / 2 - sum / 2) : 0;
            foreach (var line in layout.Lines)
            {
                if (line.FontSlot > 6) throw new InvalidOperationException("Custom native bitmap font resource not qualified");
                var font = OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(language, line.FontSlot, lowercase));
                if (font == null || font.Height != line.Height) throw new InvalidOperationException("Actual bitmap font height differs");
                string value = decode((byte[])line.Text.Clone());
                if (value == null) throw new InvalidOperationException("Native decoder returned null");
                OriginalSpriteAlignment.Rectangle rect;
                OriginalSpriteAlignment.Align(unchecked((short)line.AnchorX), unchecked((short)(line.AnchorY + CenteringOffset)),
                    unchecked((short)font.MeasureWidth(value)), unchecked((short)font.Height), line.HorizontalAlignment,
                    line.VerticalAlignment, 0, 0, out rect);
                labels.Add(new Label { Text = value, Font = font, X=line.AnchorX,Y=unchecked(line.AnchorY+CenteringOffset), Rect = new Rect(
                    Geometry.Window.Left + Geometry.Body.Left + rect.Left, Geometry.Window.Top + Geometry.Body.Top + rect.Top,
                    unchecked((short)(rect.Right - rect.Left)), unchecked((short)(rect.Bottom - rect.Top))) });
            }
        }
        public void UpdateBitmapDisplay(OriginalTextScreenUpdate update, string language,
            Func<string,string> lowercase, Func<byte[],string> decode)
        {
            if(update==null||decode==null)throw new ArgumentNullException();
            if(!spritesPresent)throw new InvalidOperationException("Native body sprite required before UpdateDisplay");
            update.Language=language;FontLanguage=language;
            Func<int,OriginalBitmapFont> font=slot=>{
                if(slot<0||slot>6)throw new InvalidOperationException("Custom bitmap resource not qualified");
                var loaded=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(language,slot,lowercase));
                if(loaded==null)throw new InvalidOperationException("Native bitmap font missing");return loaded;
            };
            update.UpdateDisplay(new OriginalTextScreenUpdate.Host {
                ResizeBody=(w,h)=>{Geometry.Body.Right=unchecked((short)(Geometry.Body.Left+w));Geometry.Body.Bottom=unchecked((short)(Geometry.Body.Top+h));Geometry.Width=w;Geometry.Height=h;},
                ResizeBorder=(w,h)=>{Geometry.Border.Right=unchecked((short)(Geometry.Border.Left+w));Geometry.Border.Bottom=unchecked((short)(Geometry.Border.Top+h));},
                SetText=(i,b)=>{labels[i].Text=decode(b);},SetFont=(i,s)=>{labels[i].Font=font(s);},
                MoveTo=(i,x,y)=>{labels[i].X=x;labels[i].Y=y;},
                CreateLabel=(i,line)=>{
                    var label=new Label {Text=decode(line.Text),Font=font(line.FontSlot),X=unchecked((short)line.AnchorX),Y=unchecked((short)line.AnchorY)};
                    if(i<labels.Count)labels[i]=label;else labels.Add(label);
                },
                Align=(i,h,v)=>{
                    var label=labels[i];OriginalSpriteAlignment.Rectangle r;
                    OriginalSpriteAlignment.Align(unchecked((short)label.X),unchecked((short)label.Y),unchecked((short)label.Font.MeasureWidth(label.Text)),
                        unchecked((short)label.Font.Height),h,v,0,0,out r);
                    label.Rect=new Rect(Geometry.Window.Left+Geometry.Body.Left+r.Left,Geometry.Window.Top+Geometry.Body.Top+r.Top,
                        unchecked((short)(r.Right-r.Left)),unchecked((short)(r.Bottom-r.Top)));
                }
            },Geometry.ShowBorder);
        }
        public static OriginalHandpayBannerPresentation CreateOrdinary(int width, int height,
            OriginalHandpayBannerText.Content content, OriginalLanguageTranslator translator, string language,
            Func<string, string> lowercase, Func<string, byte[]> encode, Func<byte[], string> decode,
            Func<int, byte[], short> nativeMeasureWidth)
        {
            if (content == null || decode == null || encode == null || nativeMeasureWidth == null) throw new ArgumentNullException();
            if (content.AlternateScreen) throw new InvalidOperationException("Original alternate screen requires its own resources");
            var layout = new OriginalHandpayBannerLayout(width, height);
            string fontLanguage = content.TranslateFirstTwoLines ? language : "en";
            Func<int, byte[], short> measure = nativeMeasureWidth;
            Action<byte[], int> add = (bytes, slot) => {
                var font = OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(fontLanguage, slot, lowercase));
                OriginalTextScreenWrapping.AddText(layout, bytes, slot, font.Height, 0, true, measure);
            };
            add(content.TranslateFirstTwoLines ? OriginalHandpayBannerText.Translate(content.Title, translator, language, encode) : content.Title, 1);
            add(content.TranslateFirstTwoLines ? OriginalHandpayBannerText.Translate(content.Body, translator, language, encode) : content.Body, 1);
            if (content.Amount != null) add(content.Amount, 3);
            layout.Align(measure);
            return new OriginalHandpayBannerPresentation(layout, fontLanguage, lowercase, decode);
        }
        private Rect Absolute(OriginalSpriteAlignment.Rectangle r)
        { return new Rect(Geometry.Window.Left + r.Left, Geometry.Window.Top + r.Top, unchecked((short)(r.Right-r.Left)), unchecked((short)(r.Bottom-r.Top))); }
        public void Draw()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint) return;
            Color saved = GUI.color;
            try { Surface((r,t,u,c) => { GUI.color=c;GUI.DrawTextureWithTexCoords(r,t,u,true); }); }
            finally { GUI.color=saved; }
        }
        public void DrawTo(RenderTexture target)
        {
            if (target == null || target.width != Layout.DisplayWidth || target.height != Layout.DisplayHeight) throw new ArgumentException("Native viewport required");
            var shader=Shader.Find("UI/Default");if(shader==null)throw new InvalidOperationException("UI shader missing");
            var material=new Material(shader);var saved=RenderTexture.active;GL.PushMatrix();
            try
            {
                RenderTexture.active=target;GL.Clear(true,true,Color.clear);GL.LoadPixelMatrix(0,target.width,target.height,0);
                Surface((r,t,u,c) => {
                    material.mainTexture=t;material.SetColor("_Color",Color.white);if(!material.SetPass(0))throw new InvalidOperationException("UI shader pass failed");
                    GL.Begin(GL.QUADS);GL.Color(c);
                    GL.TexCoord2(u.xMin,u.yMax);GL.Vertex3(r.xMin,r.yMin,0);GL.TexCoord2(u.xMax,u.yMax);GL.Vertex3(r.xMax,r.yMin,0);
                    GL.TexCoord2(u.xMax,u.yMin);GL.Vertex3(r.xMax,r.yMax,0);GL.TexCoord2(u.xMin,u.yMin);GL.Vertex3(r.xMin,r.yMax,0);GL.End();
                });
            }
            finally { GL.PopMatrix();RenderTexture.active=saved;if(Application.isPlaying)UnityEngine.Object.Destroy(material);else UnityEngine.Object.DestroyImmediate(material); }
        }
        private void Surface(Action<Rect,Texture2D,Rect,Color> draw)
        {
            if (!RootVisible || !spritesPresent) return;
            var full=new Rect(0,0,1,1);
            if(Geometry.ShowBorder)draw(Absolute(Geometry.Border),Texture2D.whiteTexture,full,new Color(1,1,0,1));
            draw(Absolute(Geometry.Body),Texture2D.whiteTexture,full,new Color(0,0,1,1));
            foreach(var label in labels)
            {
                float x=label.Rect.x;
                foreach(int code in label.Font.CharacterCodes(label.Text))
                {
                    Texture2D texture;Rect uv;int width;if(!label.Font.TryGetGlyph(code,out texture,out uv,out width))continue;
                    draw(new Rect(x,label.Rect.y,width,label.Font.GlyphHeight),texture,uv,Color.white);x+=width;
                }
            }
        }
    }
}
