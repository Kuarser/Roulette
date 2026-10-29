using System;
using System.Collections.Generic;
using System.Globalization;

namespace RouletteDeluxe
{
    // Resource preselection in TextScreenCashOut C2, 086c6d50. Later OS fallback,
    // configuration parsing, image/font creation and timer setup are separate stages.
    public static class OriginalCashoutConstructorResources
    {
        public struct Size { public short Width, Height; public Size(short w,short h){Width=w;Height=h;} }
        public struct Bounds { public short Left,Top,Right,Bottom; }
        public sealed class Host
        {
            public Func<uint,Bounds> GetScreenBounds;
            public Func<Size> GetDisplaySize;
            public Func<string,bool> FileExists;
            public Func<byte> ConfiguredChannels;
        }
        public sealed class Selection
        {
            public string Directory;
            public bool StaticBackground, ResolutionSelected, GenericProbed;
            public OriginalCashoutResolutionResources Resources = new OriginalCashoutResolutionResources();
            public string SmallMessageFont="", SmallAmountFont="";
        }
        public static Selection Select(string directory,int preferredWidth,int preferredHeight,bool skipGenericResources,Host host)
        {
            if(directory==null||host==null)throw new ArgumentNullException();
            var selected=new Selection {Directory=directory};
            var bounds=host.GetScreenBounds(2);
            selected.StaticBackground=(int)bounds.Right-bounds.Left==1600&&(int)bounds.Bottom-bounds.Top==900;
            if(directory.Length==0)selected.Directory="/games/";
            else
            {
                var size=host.GetDisplaySize();
                selected.ResolutionSelected=selected.Resources.Select(directory,size.Width,size.Height,host.FileExists,host.ConfiguredChannels);
                if(!selected.ResolutionSelected&&preferredWidth>0&&preferredHeight>0&&
                    (preferredWidth!=size.Width||preferredHeight!=size.Height))
                    selected.ResolutionSelected=selected.Resources.Select(directory,preferredWidth,preferredHeight,host.FileExists,host.ConfiguredChannels);
            }
            if(!skipGenericResources&&!selected.ResolutionSelected)
            {
                selected.GenericProbed=true;
                Func<string,string> pick=name=>{string path=selected.Directory+name;return host.FileExists(path)?path:"";};
                var resource=selected.Resources;
                resource.Background=pick("CashOutAnimation-Bottom.x");
                resource.Foreground=pick("CashOutAnimation-Top.x");
                resource.Cascade=pick("CashOutAnimation-Cascade.x");
                resource.Sound=pick(host.ConfiguredChannels()<3?"CashOutSound.x":"CashOutSurroundSound.x");
                resource.MessageFont=pick("CashOutFont-Msg.x");
                resource.AmountFont=pick("CashOutFont-Amt.x");
                resource.TranslationFont=pick("CashOutFont-Trans.x");
                selected.SmallMessageFont=pick("CashOutFont-Msg-Small.x");
                selected.SmallAmountFont=pick("CashOutFont-Amt-Small.x");
                resource.Configuration=pick("CashOutAnimation.cfg");
            }
            return selected;
        }
    }

    public static class OriginalCashoutConstructorResourcesValidation
    {
        static string[] Generic(string root,byte channels)
        {
            return new[]{root+"CashOutAnimation-Bottom.x",root+"CashOutAnimation-Top.x",root+"CashOutAnimation-Cascade.x",
                root+(channels<3?"CashOutSound.x":"CashOutSurroundSound.x"),root+"CashOutFont-Msg.x",root+"CashOutFont-Amt.x",
                root+"CashOutFont-Trans.x",root+"CashOutFont-Msg-Small.x",root+"CashOutFont-Amt-Small.x",root+"CashOutAnimation.cfg"};
        }
        static string[] Resolution(string root,int width,int height,byte channels)
        {
            string suffix="-"+width.ToString(CultureInfo.InvariantCulture)+"x"+height.ToString(CultureInfo.InvariantCulture);
            return new[]{root+"CashOutAnimation-Bottom"+suffix+".x",root+"CashOutAnimation-Top"+suffix+".x",root+"CashOutAnimation-Cascade"+suffix+".x",
                root+(channels<3?"CashOutSound.x":"CashOutSurroundSound.x"),root+"CashOutFont-Msg"+suffix+".x",root+"CashOutFont-Amt"+suffix+".x",
                root+"CashOutFont-Trans"+suffix+".x",root+"CashOutAnimation"+suffix+".cfg"};
        }
        static string[] Fields(OriginalCashoutConstructorResources.Selection s)
        {
            var r=s.Resources;return new[]{r.Background,r.Foreground,r.Cascade,r.Sound,r.MessageFont,r.AmountFont,r.TranslationFont,
                s.SmallMessageFont,s.SmallAmountFont,r.Configuration};
        }
        static void TraceProbe(List<string> trace,string[] paths)
        {for(int i=0;i<paths.Length;++i){if(i==3)trace.Add("channels");trace.Add(paths[i]);}}

        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut constructor resources regression "+checks);++checks;};
            // All generic presence masks, including the two small fonts absent from resolution selection.
            foreach(string root in new[]{"","/games/","/noSlash"})foreach(byte channels in new byte[]{0,2,3,255})for(int mask=0;mask<1024;++mask)
            {
                string effective=root.Length==0?"/games/":root;var paths=Generic(effective,channels);var trace=new List<string>();
                var present=new HashSet<string>();for(int i=0;i<10;++i)if((mask&(1<<i))!=0)present.Add(paths[i]);
                // Empty root bypasses resolution. For other roots, generic sound may itself select a resolution set.
                var host=new OriginalCashoutConstructorResources.Host{
                    GetScreenBounds=id=>{check(id==2);trace.Add("bounds");return new OriginalCashoutConstructorResources.Bounds();},
                    GetDisplaySize=()=>{trace.Add("display");return new OriginalCashoutConstructorResources.Size(800,600);},
                    ConfiguredChannels=()=>{trace.Add("channels");return channels;},FileExists=p=>{trace.Add(p);return present.Contains(p);}};
                var selected=OriginalCashoutConstructorResources.Select(root,0,0,false,host);
                bool soundSelect=root.Length!=0&&(mask&8)!=0;
                var expected=new List<string>{"bounds"};if(root.Length!=0){expected.Add("display");TraceProbe(expected,Resolution(root,800,600,channels));}
                if(!soundSelect)TraceProbe(expected,paths);
                check(string.Join("|",trace)==string.Join("|",expected));check(selected.Directory==effective);
                check(selected.ResolutionSelected==soundSelect&&selected.GenericProbed==!soundSelect&&!selected.StaticBackground);
                string[] fields=Fields(selected);for(int i=0;i<10;++i)check(fields[i]==(soundSelect?(i==3?paths[3]:""):((mask&(1<<i))!=0?paths[i]:"")));
            }
            // Control-flow cross-product: dimensions are signed native shorts, preferred sizes signed ints.
            var sizes=new[]{new OriginalCashoutConstructorResources.Size(800,600),new OriginalCashoutConstructorResources.Size(1600,900),
                new OriginalCashoutConstructorResources.Size(-1,600),new OriginalCashoutConstructorResources.Size(32767,-32768)};
            foreach(string root in new[]{"","/custom/","/noSlash"})foreach(bool skip in new[]{false,true})foreach(var size in sizes)
            for(int preferred=0;preferred<7;++preferred)for(int mode=0;mode<4;++mode)foreach(int genericMask in new[]{0,1,512,1023})
            {
                int w=preferred==0?0:preferred==1?-1:preferred==2?size.Width:preferred==3?801:preferred==4?size.Width:preferred==5?int.MaxValue:540;
                int h=preferred==0?0:preferred==1?900:preferred==2?size.Height:preferred==3?size.Height:preferred==4?601:preferred==5?1:960;
                bool canFallback=w>0&&h>0&&(w!=size.Width||h!=size.Height);
                string effective=root.Length==0?"/games/":root;
                byte channels=(byte)(preferred%2==0?2:3);
                var primary=Resolution(root,size.Width,size.Height,channels);var fallback=Resolution(root,w,h,channels);var generic=Generic(effective,channels);
                var present=new HashSet<string>();
                if(mode==1)present.Add(primary[0]);if(mode==2)present.Add(primary[3]);if(mode==3&&canFallback)present.Add(fallback[1]);
                for(int i=0;i<10;++i)if((genericMask&(1<<i))!=0)present.Add(generic[i]);
                // Offset screen bounds prove selection uses extents rather than absolute right/bottom coordinates.
                var bounds=new OriginalCashoutConstructorResources.Bounds {Left=10,Top=-20,Right=(short)(10+(preferred%2==0?1600:1599)),Bottom=880};
                var trace=new List<string>();var host=new OriginalCashoutConstructorResources.Host{
                    GetScreenBounds=id=>{check(id==2);trace.Add("bounds");return bounds;},
                    GetDisplaySize=()=>{trace.Add("display");return size;},ConfiguredChannels=()=>{trace.Add("channels");return channels;},
                    FileExists=p=>{trace.Add(p);return present.Contains(p);}};
                var selected=OriginalCashoutConstructorResources.Select(root,w,h,skip,host);
                var expectedTrace=new List<string>{"bounds"};var expectedFields=new string[10];for(int i=0;i<10;++i)expectedFields[i]="";
                bool found=false;
                if(root.Length!=0)
                {
                    expectedTrace.Add("display");TraceProbe(expectedTrace,primary);
                    for(int i=0;i<8;++i)if(present.Contains(primary[i])){found=true;expectedFields[i==7?9:i]=primary[i];}
                    if(!found&&canFallback)
                    {
                        TraceProbe(expectedTrace,fallback);
                        for(int i=0;i<8;++i)if(present.Contains(fallback[i])){found=true;expectedFields[i==7?9:i]=fallback[i];}
                    }
                }
                bool genericProbed=!skip&&!found;
                if(genericProbed){TraceProbe(expectedTrace,generic);for(int i=0;i<10;++i)if(present.Contains(generic[i]))expectedFields[i]=generic[i];}
                check(string.Join("|",trace)==string.Join("|",expectedTrace));
                check(selected.StaticBackground==(preferred%2==0));check(selected.Directory==effective);
                check(selected.ResolutionSelected==found&&selected.GenericProbed==genericProbed);
                var actual=Fields(selected);for(int i=0;i<10;++i)check(actual[i]==expectedFields[i]);
                check(root==""||root=="/custom/"||root=="/noSlash");
            }
            return checks;
        }
    }
}
