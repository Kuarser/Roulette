using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // Bitmap path of ResetforUpdate/UpdateDisplay; oldCount is a saved line
    // count, not the allocated label count. Empty Reset leaves it unchanged.
    public sealed class OriginalTextScreenUpdate
    {
        public sealed class Host
        {
            public Action<int,int> ResizeBody, ResizeBorder;
            public Action<int,byte[]> SetText;
            public Action<int,int> SetFont;
            public Action<int,int,int> MoveTo, Align;
            public Action<int,OriginalHandpayBannerLayout.Line> CreateLabel;
        }
        private readonly OriginalHandpayBannerLayout layout;
        private readonly List<OriginalHandpayBannerLayout.Line> old = new List<OriginalHandpayBannerLayout.Line>();
        private string oldLanguage = "";
        public string Language = "";
        public int OldCount { get { return old.Count; } }
        public OriginalTextScreenUpdate(OriginalHandpayBannerLayout target) { layout = target ?? throw new ArgumentNullException(nameof(target)); }
        private static OriginalHandpayBannerLayout.Line Copy(OriginalHandpayBannerLayout.Line line)
        {
            return new OriginalHandpayBannerLayout.Line { Text=(byte[])line.Text.Clone(),FontSlot=line.FontSlot,Height=line.Height,
                Spacing=line.Spacing,AnchorX=line.AnchorX,AnchorY=line.AnchorY,HorizontalAlignment=line.HorizontalAlignment,
                VerticalAlignment=line.VerticalAlignment,BitmapFontPresent=line.BitmapFontPresent };
        }
        public bool ResetForUpdate()
        {
            if (layout.Lines.Count == 0) return true;
            old.Clear();
            foreach (var line in layout.Lines) { var copied=Copy(line); if(oldLanguage!=Language)copied.FontSlot=13; old.Add(copied); }
            oldLanguage=Language;layout.ClearLinesForUpdate();layout.SetHandpayDefaults();return true;
        }
        private static bool Equal(byte[] a,byte[] b)
        {if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public bool UpdateDisplay(Host host, bool borderPresent)
        {
            if(host==null||host.ResizeBody==null||host.ResizeBorder==null||host.SetText==null||host.SetFont==null||
                host.MoveTo==null||host.Align==null||host.CreateLabel==null)throw new ArgumentException("Native bitmap update operations required");
            var pixels=layout.Pixels();
            if(borderPresent)host.ResizeBorder(unchecked(pixels.Width+2*layout.BorderWidth),unchecked(pixels.Height+2*layout.BorderWidth));
            host.ResizeBody(pixels.Width,pixels.Height);
            for(int i=0;i<layout.Lines.Count;++i)
            {
                var line=layout.Lines[i];
                if(i<old.Count)
                {
                    if(!Equal(line.Text,old[i].Text))host.SetText(i,(byte[])line.Text.Clone());
                    if(line.FontSlot!=old[i].FontSlot||Language!=oldLanguage)host.SetFont(i,line.FontSlot);
                    host.MoveTo(i,unchecked((short)line.AnchorX),unchecked((short)line.AnchorY));
                }
                else host.CreateLabel(i,Copy(line));
                host.Align(i,line.HorizontalAlignment,line.VerticalAlignment);
            }
            for(int i=layout.Lines.Count;i<old.Count;++i)host.SetText(i,new byte[0]);
            return true;
        }
    }
}
