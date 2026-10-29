using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RouletteDeluxe
{
    public static class OriginalCashoutDisplayValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut display check "+checks);++checks;};
            int[] dims={1360,768,1920,1080,-32767,-3,32767,32765,1,1};int[] modes={-1,0,1,2,3};
            int scenarios=0;
            for(int flags=0;flags<128;++flags)for(int d=0;d<dims.Length;d+=2)foreach(int mode in modes)for(int bound=0;bound<4;++bound)
            {
                ++scenarios;var trace=new List<string>();var want=new List<string>();
                var state=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=(flags&1)!=0,Paused=(flags&2)!=0},FontMode=mode,Cursor=77};
                state.Layout.FullscreenBackground=(flags&4)!=0;state.Layout.PixelX=65535;state.Layout.PixelY=32768;
                bool centre=(flags&8)!=0;short w=(short)dims[d],h=(short)dims[d+1];
                int count=(flags&16)!=0?2:0;object main=new object(),small=new object();state.Fonts[7]=main;state.FontFiles[10]="small-font\0ignored";
                if((flags&32)!=0)state.Fonts[10]=small;
                state.Left=0;state.Top=11;state.Right=(short)(bound==0?100:bound==1?101:bound==2?0:-1);state.Bottom=(short)(bound==3?10:41);
                bool lookup=(flags&64)!=0;
                Action<string,short,short> move=(id,x,y)=>trace.Add(id+"move:"+x+","+y);
                state.Background=new OriginalCashoutDisplay.Sprite {MoveTo=(x,y)=>move("bg",x,y),Align=(x,y)=>trace.Add("bgalign:"+x+","+y)};
                state.Foreground=new OriginalCashoutDisplay.Sprite {MoveTo=(x,y)=>move("fg",x,y),Align=(x,y)=>trace.Add("fgalign:"+x+","+y)};
                state.Cascade=new OriginalCashoutDisplay.Sprite {MoveTo=(x,y)=>move("cs",x,y),Align=(x,y)=>trace.Add("csalign:"+x+","+y)};
                for(int i=0;i<count;++i)
                {
                    int n=i;state.Lines.Add(new OriginalCashoutDisplay.Line {FontSlot=7,Height=i==0?31:20,Text=new byte[]{(byte)(65+i),0,90}});
                    state.Labels.Add(new OriginalCashoutDisplay.Label {GetBounds=()=>{trace.Add("get:"+n+":"+state.Cursor);return new OriginalSpriteAlignment.Rectangle {Left=-9,Top=100,Right=-8,Bottom=150};},
                        SetBounds=r=>{check(r.Left==state.Left&&r.Right==state.Right&&r.Top==100&&r.Bottom==150);trace.Add("bounds:"+n+":"+state.Cursor);},
                        SetFont=f=>{check(f==small);trace.Add("font:"+n+":"+state.Cursor);}});
                }
                int creates=0;
                var host=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>{trace.Add("size");return new[]{w,h};},Centering=()=>{trace.Add("centre");return centre;},
                    CreateDisplaySprites=offset=>trace.Add("create:"+offset+":"+state.Cursor),
                    LookupTextBounds=key=>{check(key==w+"x"+h);trace.Add("lookup");return lookup?(OriginalSpriteAlignment.Rectangle?)new OriginalSpriteAlignment.Rectangle{Left=0,Top=20,Right=100,Bottom=90}:null;},
                    CalcFontWidth=(font,text)=>{check(font==main&&text.Length==1);int n=text[0]-65;trace.Add("measure:"+n+":"+state.Cursor);return (short)(n==0?101:100);},
                    CreateFont=(path,size,color)=>{check(path=="small-font"&&size==0&&color==0);++creates;trace.Add("newfont");return small;},
                    Emit=(raw,param)=>{check(state.Playback.Visible&&param==0&&raw.Length==44);for(int n=0;n<44;++n)check(raw[n]==(n==0?1:n==2?44:n==25?0x81:n==26?1:0));trace.Add("event");},
                    Show=()=>trace.Add("show")};
                short right=state.Right,top=state.Top,bottom=state.Bottom;int cursor=centre?count:77;
                if((flags&1)!=0)
                {
                    want.Add("size");want.Add("bgmove:"+((flags&4)!=0?w/2:-1)+","+((flags&4)!=0?h/2:-32768));want.Add("bgalign:0,0");
                    want.Add("fgmove:"+w/2+","+h/2);want.Add("fgalign:0,0");want.Add("csmove:800,450");want.Add("csalign:0,0");want.Add("centre");
                    want.Add("create:"+(centre?h/2-(count==2?51:0)/2:0)+":"+cursor);
                    if(right<=0||bottom<=top){want.Add("lookup");if(lookup){right=100;top=20;bottom=90;}}
                    if(mode==1||mode==2)
                    {
                        if(right>0&&bottom>top){cursor=count;for(int n=0;n<count;++n){want.Add("get:"+n+":"+n);want.Add("bounds:"+n+":"+n);}}
                    }
                    else
                    {
                        cursor=count;bool hasSmall=(flags&32)!=0;
                        for(int n=0;n<count;++n){want.Add("measure:"+n+":"+n);if(right<(n==0?101:100)){if(!hasSmall){want.Add("newfont");hasSmall=true;}want.Add("font:"+n+":"+n);}}
                    }
                    want.Add("event");if((flags&2)==0)want.Add("show");
                }
                else cursor=77;
                check(OriginalCashoutDisplay.Display(state,host));check(string.Join("|",trace)==string.Join("|",want));
                check(state.Cursor==cursor&&state.Right==right&&state.Top==top&&state.Bottom==bottom);check(state.Playback.Visible==((flags&1)!=0));check(creates<=1);
            }
            check(scenarios==12800);
            var defaults=new OriginalCashoutDisplayLayout {X=8,Y=9,Width=2,Height=3,PixelX=8,Spacing=12,Border=8};defaults.SetDefaults();
            check(defaults.X==0&&defaults.Y==0&&defaults.Width==1&&defaults.Height==1&&defaults.Order==9100&&defaults.FullscreenBackground);
            check(defaults.PixelX==0&&defaults.PixelY==0&&defaults.PixelWidth==0&&defaults.PixelHeight==0&&defaults.ClipX==0&&defaults.ClipY==0&&defaults.HorizontalAlignment==0&&defaults.VerticalAlignment==0&&defaults.Spacing==0&&defaults.Border==0);
            check(defaults.BackgroundRgb==0x0000ff&&defaults.BorderRgb==0xffff00);
            check(defaults.SetPosition(float.NaN,float.PositiveInfinity)&&defaults.SetSize(-8,float.NegativeInfinity));check(defaults.X==0&&defaults.Y==0&&defaults.Width==1&&defaults.Height==1);
            // Missing optional sprites are skipped. The event callback can change
            // Paused before the final native Show gate is tested.
            var special=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true},FontMode=2,Left=1,Top=1,Right=2,Bottom=2,Cursor=9};
            int shown=0;OriginalCashoutDisplay.Display(special,new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>false,CreateDisplaySprites=o=>{},Emit=(b,p)=>special.Playback.Paused=true,Show=()=>++shown});check(shown==0&&special.Cursor==0);
            for(int i=0;i<3;++i)
            {
                var go=new GameObject("CashOut positioned image",typeof(RectTransform),typeof(CanvasRenderer),typeof(RawImage));var image=go.GetComponent<RawImage>();image.rectTransform.sizeDelta=new Vector2(i==0?1360:i==1?1600:101,i==0?768:i==1?720:51);
                var sprite=OriginalCashoutDisplay.BindImage(image);sprite.MoveTo(i==1?(short)800:(short)680,i==1?(short)450:(short)384);sprite.Align(0,0);
                var position=image.rectTransform.anchoredPosition;check(position.x==(i==0?0:i==1?0:630)&&position.y==(i==0?0:i==1?-90:-359));
                check(image.rectTransform.pivot==new Vector2(0,1));UnityEngine.Object.DestroyImmediate(go);
            }
            var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("configured.cfg");check(table.Entries.Count==0);
            table.Entries.Add("1360x768",new OriginalSpriteAlignment.Rectangle {Left=9,Top=10,Right=11,Bottom=12});table.ConstructorDefaults("\0ignored");check(table.Entries.Count==1);table.ConstructorDefaults("");
            check(table.Entries.Count==4&&table.Lookup("1360x768").Value.Left==9);check(!table.Lookup("800x600").HasValue);
            int[] rows={88,542,679,814,501,345,1418,751,125,768,955,1142};string[] keys={"768x1360","1920x1080","1080x1920"};
            for(int n=0;n<3;++n){var r=table.Lookup(keys[n]).Value;check(r.Left==rows[n*4]&&r.Top==rows[n*4+1]&&r.Right==rows[n*4+2]&&r.Bottom==rows[n*4+3]);}
            table.ConstructorDefaults("");check(table.Entries.Count==4&&table.Lookup("1360x768").Value.Left==9);
            var pristine=new OriginalCashoutTextBoundsTable();pristine.ConstructorDefaults("");var original=pristine.Lookup("1360x768").Value;check(original.Left==356&&original.Top==259&&original.Right==1007&&original.Bottom==557);
            return checks;
        }
    }
}
