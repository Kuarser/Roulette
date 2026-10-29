using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // TextScreenCashOut overrides the base defaults but retains lines, fonts,
    // resource ownership, visibility and centering. Native field offsets in names/comments.
    public sealed class OriginalCashoutDisplayLayout
    {
        public float X,Y,Width,Height;
        public int Order,PixelX,PixelY,PixelWidth,PixelHeight,ClipX,ClipY,HorizontalAlignment,VerticalAlignment,Spacing,Border;
        public uint BackgroundRgb,BorderRgb;
        public bool FullscreenBackground;
        public void SetDefaults()
        {
            X=Y=0;Width=Height=1;Order=9100;
            PixelX=PixelY=PixelWidth=PixelHeight=ClipX=ClipY=HorizontalAlignment=VerticalAlignment=Spacing=Border=0;
            BackgroundRgb=0x0000ff;BorderRgb=0xffff00;FullscreenBackground=true;
        }
        public bool SetPosition(float x,float y){return true;} // Native 086ca4c0 ignores arguments.
        public bool SetSize(float width,float height){return true;} // Native 086ca4d0 ignores arguments.
    }
    // Native process-shared textBounds is initialized empty. Each constructor
    // with an empty configuration filename inserts these defaults uniquely;
    // existing entries win, and configured constructors do not populate them.
    public sealed class OriginalCashoutTextBoundsTable
    {
        public readonly Dictionary<string,OriginalSpriteAlignment.Rectangle> Entries=new Dictionary<string,OriginalSpriteAlignment.Rectangle>(StringComparer.Ordinal);
        public void ConstructorDefaults(string configurationFile)
        {
            if(configurationFile==null)throw new ArgumentNullException(nameof(configurationFile));
            if(configurationFile.Length!=0)return;
            Add("1360x768",356,259,1007,557);Add("768x1360",88,542,679,814);
            Add("1920x1080",501,345,1418,751);Add("1080x1920",125,768,955,1142);
        }
        void Add(string key,short left,short top,short right,short bottom)
        {if(!Entries.ContainsKey(key))Entries.Add(key,new OriginalSpriteAlignment.Rectangle {Left=left,Top=top,Right=right,Bottom=bottom});}
        public OriginalSpriteAlignment.Rectangle? Lookup(string key)
        {OriginalSpriteAlignment.Rectangle result;return Entries.TryGetValue(key,out result)?(OriginalSpriteAlignment.Rectangle?)result:null;}
    }
    public static class OriginalCashoutDisplay
    {
        public sealed class Sprite
        {
            public Action<short,short> MoveTo;
            public Action<int,int> Align;
        }
        public sealed class Label
        {
            public Func<OriginalSpriteAlignment.Rectangle> GetBounds;
            public Action<OriginalSpriteAlignment.Rectangle> SetBounds;
            public Action<object> SetFont;
        }
        public sealed class Line {public byte[] Text;public int FontSlot,Height;}
        public sealed class State
        {
            public OriginalCashoutPlaybackLifecycle.State Playback;
            public readonly OriginalCashoutDisplayLayout Layout=new OriginalCashoutDisplayLayout();
            public Sprite Background,Foreground,Cascade;
            public short Left,Top,Right,Bottom; // Packed Rect at 3378..337f.
            public int FontMode;
            public int Cursor; // Native 32e4 is observable during host callbacks.
            public readonly List<Line> Lines=new List<Line>();
            public readonly object[] Fonts=new object[13];
            public readonly string[] FontFiles=new string[13];
            public readonly List<Label> Labels=new List<Label>();
        }
        public sealed class Host
        {
            public Func<short[]> GetDisplaySize;
            public Func<bool> Centering;
            public Action<int> CreateDisplaySprites;
            public Func<string,OriginalSpriteAlignment.Rectangle?> LookupTextBounds;
            public Func<object,byte[],short> CalcFontWidth;
            public Func<string,int,uint,object> CreateFont;
            public Action<byte[],uint> Emit;
            public Action Show;
        }
        static void Position(Func<Sprite> get,short x,short y)
        {var sprite=get();if(sprite!=null){sprite.MoveTo(x,y);get().Align(0,0);}}
        public static bool Display(State state,Host host)
        {
            if(!state.Playback.ResourcesAvailable)return true;
            short[] size=host.GetDisplaySize();if(size==null||size.Length!=2)throw new ArgumentException("Native signed16 display size required");
            short x=(short)(size[0]/2),y=(short)(size[1]/2);
            Position(()=>state.Background,state.Layout.FullscreenBackground?x:unchecked((short)state.Layout.PixelX),state.Layout.FullscreenBackground?y:unchecked((short)state.Layout.PixelY));
            Position(()=>state.Foreground,x,y);Position(()=>state.Cascade,800,450);
            int offset=0;
            if(host.Centering())
            {
                state.Cursor=0;int sum=0;
                for(;state.Cursor<state.Lines.Count;++state.Cursor)sum=unchecked(sum+state.Lines[state.Cursor].Height);
                offset=unchecked(size[1]/2-sum/2);
            }
            host.CreateDisplaySprites(offset);
            if(state.Right<=state.Left||state.Bottom<=state.Top)
            {
                var bounds=host.LookupTextBounds(size[0].ToString(System.Globalization.CultureInfo.InvariantCulture)+"x"+size[1].ToString(System.Globalization.CultureInfo.InvariantCulture));
                if(bounds.HasValue){state.Left=bounds.Value.Left;state.Top=bounds.Value.Top;state.Right=bounds.Value.Right;state.Bottom=bounds.Value.Bottom;}
            }
            int mode=state.FontMode;
            if(mode==1||mode==2)
            {
                if(state.Left<state.Right&&state.Top<state.Bottom)
                {
                    state.Cursor=0;
                    for(;state.Cursor<state.Lines.Count;++state.Cursor)
                    {
                        Label label=state.Labels[state.Cursor];var bounds=label.GetBounds();bounds.Left=state.Left;bounds.Right=state.Right;label.SetBounds(bounds);
                    }
                }
            }
            else
            {
                state.Cursor=0;
                for(;state.Cursor<state.Lines.Count;++state.Cursor)
                {
                    Line line=state.Lines[state.Cursor];int slot=line.FontSlot;
                    int length=Array.IndexOf(line.Text,(byte)0);if(length<0)length=line.Text.Length;
                    var text=new byte[length];Array.Copy(line.Text,text,length);
                    short width=host.CalcFontWidth(state.Fonts[slot],text);
                    if((int)state.Right-state.Left<width)
                    {
                        int small=slot+3;
                        if(small>=state.Fonts.Length)throw new ArgumentOutOfRangeException("Native small-font slot out of bounds");
                        object font=state.Fonts[small];
                        if(font==null){string path=state.FontFiles[small];int nul=path.IndexOf('\0');if(nul>=0)path=path.Substring(0,nul);font=host.CreateFont(path,0,0);state.Fonts[small]=font;}
                        state.Labels[state.Cursor].SetFont(font);
                    }
                }
            }
            state.Playback.Visible=true;var raw=new byte[44];raw[0]=1;raw[2]=44;raw[24]=0;raw[25]=0x81;raw[26]=1;
            host.Emit(raw,0);if(!state.Playback.Paused)host.Show();return true;
        }
        // The parent canvas uses native screen coordinates. Existing full-size
        // movie textures retain their size; Align(0,0) places their centre here.
        public static Sprite BindImage(RawImage image)
        {
            if(image==null)throw new ArgumentNullException(nameof(image));short px=0,py=0;
            return new Sprite {MoveTo=(x,y)=>{px=x;py=y;},Align=(h,v)=>{
                if(h!=0||v!=0)throw new ArgumentException("CashOut uses centre alignment");
                var r=image.rectTransform;int w=(int)r.rect.width,ht=(int)r.rect.height;
                r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.sizeDelta=new Vector2(w,ht);
                r.anchoredPosition=new Vector2(unchecked((short)(px-w/2)),-unchecked((short)(py-ht/2)));
            }};
        }
    }
}
