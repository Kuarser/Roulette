using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RouletteDeluxe
{
    // TextScreen::DisplayTextLines AGK branch, SpriteLabel::Set(Font) and
    // Sprite::Align. The caller supplies the native text root and decoder.
    public sealed class OriginalCashoutBitmapLabels:IDisposable
    {
        [Serializable] public sealed class Snapshot
        {
            public string Text;
            public int FontSlot,RenderFontSlot,Width,Height,GlyphCount,Left,Top,Right,Bottom;
            public OriginalSpriteAlignment.Rectangle Bounds;
        }
        private sealed class Label
        {
            public string Text;
            public OriginalBitmapFont Font;
            public int Slot,RenderSlot,Horizontal,Vertical;
            public short X,Y;
            public OriginalSpriteAlignment.Rectangle Bounds;
            public readonly List<GameObject> Glyphs=new List<GameObject>();
        }
        private readonly List<Label> labels=new List<Label>();
        private readonly OriginalCashoutDisplay.State state;
        private readonly OriginalCashoutFontBinding binding;
        private readonly IReadOnlyList<OriginalHandpayBannerLayout.Line> lines;
        private readonly Func<byte[],string> decode;
        private readonly GameObject root;
        private bool disposed;
        public GameObject Root {get{return root;}}
        public OriginalCashoutBitmapLabels(Transform nativeRoot,OriginalCashoutDisplay.State owner,
            OriginalCashoutFontBinding fonts,IReadOnlyList<OriginalHandpayBannerLayout.Line> nativeLines,Func<byte[],string> nativeDecode)
        {
            if(nativeRoot==null||owner==null||fonts==null||nativeLines==null||nativeDecode==null)throw new ArgumentNullException();
            state=owner;binding=fonts;lines=nativeLines;decode=nativeDecode;
            if(state.FontMode==1||state.FontMode==2||binding.FontMode==1||binding.FontMode==2)throw new InvalidOperationException("AGK label owner required; TTF/BMFont rasterizers pending");
            root=new GameObject("CashOut AGK labels",typeof(RectTransform));root.transform.SetParent(nativeRoot,false);
            var rect=root.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=Vector2.zero;
            for(int slot=0;slot<13;++slot)state.FontFiles[slot]=binding.FileName(slot);
        }
        internal OriginalBitmapFont Font(int slot)
        {
            var existing=state.Fonts[slot] as OriginalBitmapFont;
            if(existing!=null)return existing;
            var font=binding.LoadQualifiedAgkFont(slot);state.Fonts[slot]=font;return font;
        }
        public short Measure(int slot,byte[] text){return unchecked((short)Font(slot).MeasureWidth(Decode(text)));}
        private string Decode(byte[] text)
        {
            if(text==null)throw new ArgumentNullException(nameof(text));int end=Array.IndexOf(text,(byte)0);if(end<0)end=text.Length;
            byte[] value=new byte[end];Array.Copy(text,value,end);return decode(value)??throw new InvalidOperationException("Native text decoder returned null");
        }
        private static void Release(GameObject go){if(Application.isPlaying)UnityEngine.Object.Destroy(go);else UnityEngine.Object.DestroyImmediate(go);}
        private void ClearLabels()
        {
            foreach(var label in labels)foreach(var go in label.Glyphs)Release(go);labels.Clear();state.Labels.Clear();
        }
        public void ClearNativeLabels()
        {
            if(disposed)throw new ObjectDisposedException(nameof(OriginalCashoutBitmapLabels));ClearLabels();
        }
        private void Rebuild(Label label)
        {
            foreach(var go in label.Glyphs)Release(go);label.Glyphs.Clear();
            OriginalSpriteAlignment.Align(label.X,label.Y,unchecked((short)label.Font.MeasureWidth(label.Text)),unchecked((short)label.Font.Height),
                label.Horizontal,label.Vertical,label.Bounds.Left,label.Bounds.Top,out label.Bounds);
            int x=label.Bounds.Left;
            foreach(int code in label.Font.CharacterCodes(label.Text))
            {
                Texture2D texture;Rect uv;int width;if(!label.Font.TryGetGlyph(code,out texture,out uv,out width))continue;
                var go=new GameObject("Glyph "+code,typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));go.transform.SetParent(root.transform,false);
                var image=go.GetComponent<RawImage>();image.texture=texture;image.uvRect=uv;image.color=Color.white;image.raycastTarget=false;
                var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-label.Bounds.Top);r.sizeDelta=new Vector2(width,label.Font.GlyphHeight);
                label.Glyphs.Add(go);x+=width;
            }
        }
        public void CreateDisplayTextLines(int offset)
        {
            if(disposed)throw new ObjectDisposedException(nameof(OriginalCashoutBitmapLabels));
            if(state.FontMode==1||state.FontMode==2)throw new InvalidOperationException("AGK label owner required");
            if(lines.Count!=state.Lines.Count)throw new InvalidOperationException("Native line table and display owner differ");
            ClearLabels();state.Cursor=0;
            for(;state.Cursor<lines.Count;++state.Cursor)
            {
                var line=lines[state.Cursor];var label=new Label {Text=Decode(line.Text),Font=Font(line.FontSlot),Slot=line.FontSlot,RenderSlot=line.FontSlot,
                    X=unchecked((short)line.AnchorX),Y=unchecked((short)(line.AnchorY+offset)),Horizontal=line.HorizontalAlignment,Vertical=line.VerticalAlignment};
                labels.Add(label);Rebuild(label);
                state.Labels.Add(new OriginalCashoutDisplay.Label {GetBounds=()=>label.Bounds,SetBounds=b=>{}, // Adapter SetBounds does not affect unscaled AGK SpriteLabel.
                    SetFont=value=>{var font=value as OriginalBitmapFont;if(font==null)throw new InvalidOperationException("Qualified AGK font object required");label.Font=font;
                        label.RenderSlot=label.Slot<10&&state.Fonts[label.Slot+3]==font?label.Slot+3:Array.IndexOf(state.Fonts,font);Rebuild(label);}});
            }
        }
        public void Bind(OriginalCashoutDisplay.Host host)
        {
            if(host==null)throw new ArgumentNullException(nameof(host));
            host.CreateDisplaySprites=CreateDisplayTextLines;
            host.CalcFontWidth=(font,text)=>{var value=font as OriginalBitmapFont;if(value==null)throw new InvalidOperationException("Actual AGK font object required");return unchecked((short)value.MeasureWidth(Decode(text)));};
            host.CreateFont=(path,mode,color)=>
            {
                if(mode!=0||color!=0)throw new InvalidOperationException("Native lazy small font arguments differ");
                for(int slot=0;slot<13;++slot)if(binding.FileName(slot)==path)return Font(slot);
                throw new InvalidOperationException("Original AGK font path not bound: "+path);
            };
            state.Playback.TextRoot=new OriginalCashoutPlaybackLifecycle.Sprite {Hide=()=>root.SetActive(false),Show=visible=>root.SetActive(visible)};
        }
        public Snapshot[] Describe()
        {
            var result=new Snapshot[labels.Count];for(int i=0;i<labels.Count;++i){var label=labels[i];result[i]=new Snapshot {Text=label.Text,FontSlot=label.Slot,
                RenderFontSlot=label.RenderSlot,Width=unchecked((short)label.Font.MeasureWidth(label.Text)),Height=label.Font.Height,GlyphCount=label.Glyphs.Count,
                Bounds=label.Bounds,Left=label.Bounds.Left,Top=label.Bounds.Top,Right=label.Bounds.Right,Bottom=label.Bounds.Bottom};}return result;
        }
        public void Dispose(){if(disposed)return;disposed=true;ClearLabels();Release(root);}
    }
}
