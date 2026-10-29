using System;
using System.Collections.Generic;
using System.Globalization;

namespace RouletteDeluxe
{
    // C2 resource fallback and availability gate, 086c7a68..086c82b9 plus branches.
    // Image/font/sound objects and the native configuration parser are not created here.
    public static class OriginalCashoutDefaultResources
    {
        // Native cashOutAnimExt is process-wide and initialized once to .bik.
        public sealed class EnvironmentState { public string AnimationExtension=".bik"; }
        public sealed class Owner
        {
            public OriginalCashoutConstructorResources.Selection Selection;
            public int FontType;
            public string SmallTranslationFont="";
            public bool ResourcesAvailable;
        }
        public sealed class Host
        {
            public Func<string,string> GetEnvironment;
            public Func<OriginalCashoutConstructorResources.Size> GetDisplaySize;
            public Func<byte> ConfiguredChannels;
            public Func<string,bool> FileExists;
        }
        public static void Fill(Owner owner,EnvironmentState environment,Host host)
        {
            if(owner==null||environment==null||host==null)throw new ArgumentNullException();
            string video=host.GetEnvironment("AGKVIDEO");
            if(video!=null){int end=video.IndexOf('\0');if(end>=0)video=video.Substring(0,end);}
            if(video!="OGL"&&video!="V2GL")environment.AnimationExtension=".mng";
            var size=host.GetDisplaySize();
            string suffix=size.Width.ToString(CultureInfo.InvariantCulture)+"x"+size.Height.ToString(CultureInfo.InvariantCulture);
            var selected=owner.Selection;var r=selected.Resources;
            bool defaults=r.Background.Length==0||r.Foreground.Length==0||r.Cascade.Length==0||r.MessageFont.Length==0||r.AmountFont.Length==0;
            string fontExtension=owner.FontType==1?".ttfnt":".fnt";
            const string bm="/agk/fonts/ArialUnicode-Bold-BM.fnt";
            if(defaults)
            {
                if(r.Background.Length==0)r.Background=selected.StaticBackground?"/agk/picts/CashOutAnimation-Bottom-Curve.png":"/agk/picts/CashOutAnimation-Bottom-"+suffix+".bik";
                if(r.Foreground.Length==0)r.Foreground=selected.StaticBackground?"/agk/picts/CashOutAnimation-Top-Curve"+environment.AnimationExtension:
                    "/agk/picts/CashOutAnimation-Top-"+suffix+environment.AnimationExtension;
                if(selected.StaticBackground&&r.Cascade.Length==0)r.Cascade="/agk/picts/CashOutAnimation-Cascade-Curve"+environment.AnimationExtension;
                if(r.MessageFont.Length==0)r.MessageFont=owner.FontType==2?bm:"/agk/fonts/CashOutFont-Msg-"+suffix+fontExtension;
                if(r.AmountFont.Length==0)r.AmountFont=owner.FontType==2?bm:"/agk/fonts/CashOutFont-Amt-"+suffix+fontExtension;
                if(owner.FontType==0)
                {
                    if(selected.SmallMessageFont.Length==0)selected.SmallMessageFont="/agk/fonts/CashOutFont-Msg-Small-"+suffix+".fnt";
                    if(selected.SmallAmountFont.Length==0)selected.SmallAmountFont=r.AmountFont;
                }
                // This assignment is unconditional within the defaults branch, for every font mode.
                owner.SmallTranslationFont="/agk/fonts/CashOutFont-Trans-Small-"+suffix+".fnt";
            }
            if(r.TranslationFont.Length==0)r.TranslationFont=owner.FontType==2?bm:"/agk/fonts/CashOutFont-Trans-"+suffix+fontExtension;
            if(r.Sound.Length==0)r.Sound=host.ConfiguredChannels()<3?"/agk/sounds/CashOutSound.wav":"/agk/sounds/CashOutSurroundSound.wav";
            owner.ResourcesAvailable=host.FileExists(r.Background)&&host.FileExists(r.Foreground)&&
                (!selected.StaticBackground||host.FileExists(r.Cascade))&&host.FileExists(r.MessageFont)&&
                host.FileExists(r.AmountFont)&&host.FileExists(r.TranslationFont)&&host.FileExists(r.Sound);
        }
    }

    public static class OriginalCashoutDefaultResourcesValidation
    {
        public static int ValidateExtractedResources()
        {
            int checks=0;
            int[,] cases={{1360,768,0,0,0,1},{768,1360,0,0,0,1},{1920,1080,0,0,0,1},{1080,1920,0,0,0,1},
                {1360,768,1,0,0,1},{1360,768,0,1,0,1},{768,1360,0,1,0,0},{1600,900,2,0,1,1},{1600,900,2,1,1,0},{800,600,0,0,0,0}};
            string root=System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../../bal/BAL_OS366J"));
            for(int index=0;index<cases.GetLength(0);++index)
            {
                int width=cases[index,0],height=cases[index,1],mode=cases[index,2];bool still=cases[index,4]!=0;
                var size=new OriginalCashoutConstructorResources.Size((short)width,(short)height);
                var selection=OriginalCashoutConstructorResources.Select("",0,0,true,new OriginalCashoutConstructorResources.Host {
                    GetScreenBounds=id=>new OriginalCashoutConstructorResources.Bounds {Right=(short)(still?1600:800),Bottom=(short)(still?900:600)},
                    GetDisplaySize=()=>{throw new Exception("Empty directory must bypass display preselection");},
                    FileExists=p=>{throw new Exception("Skip generic must bypass generic preselection");},ConfiguredChannels=()=>2});
                var owner=new OriginalCashoutDefaultResources.Owner {Selection=selection,FontType=mode};
                var probes=new List<string>();
                OriginalCashoutDefaultResources.Fill(owner,new OriginalCashoutDefaultResources.EnvironmentState(),new OriginalCashoutDefaultResources.Host {
                    GetEnvironment=n=>cases[index,3]==0?"OGL":null,GetDisplaySize=()=>size,ConfiguredChannels=()=>2,
                    FileExists=p=>{
                        if(!p.StartsWith("/agk/",StringComparison.Ordinal))throw new Exception("Unexpected fixture mount path");
                        bool exists=OriginalCashoutFileExists.ProbeLocalRegularFile(System.IO.Path.Combine(root,p.Substring(1).Replace('/',System.IO.Path.DirectorySeparatorChar)));
                        probes.Add((exists?"1:":"0:")+p);return exists;}});
                if(owner.ResourcesAvailable!=(cases[index,5]!=0))throw new Exception("Extracted CashOut availability regression "+index);
                ++checks;
                UnityEngine.Debug.Log("CASHOUT_DEFAULT_EXTRACTED_CASE: index="+index+" size="+width+"x"+height+" mode="+mode+" static="+still+
                    " video="+(cases[index,3]==0?"OGL":"absent")+" ready="+owner.ResourcesAvailable+" probes="+string.Join("|",probes));
            }
            return checks;
        }
        static string[] Fields(OriginalCashoutDefaultResources.Owner owner)
        {
            var s=owner.Selection;var r=s.Resources;return new[]{r.Background,r.Foreground,r.Cascade,r.MessageFont,r.AmountFont,
                r.TranslationFont,r.Sound,s.SmallMessageFont,s.SmallAmountFont,owner.SmallTranslationFont,r.Configuration};
        }
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut default resources regression "+checks);++checks;};
            // Input presence covers the outer five-field gate and all optional font fields independently.
            foreach(bool still in new[]{false,true})foreach(int fontType in new[]{0,1,2,7})foreach(string video in new[]{null,"OGL","V2GL","ogl","OGLx","OGL\0ignored"})
            for(int mask=0;mask<1024;++mask)
            {
                var s=new OriginalCashoutConstructorResources.Selection {StaticBackground=still,Directory="/custom/"};var r=s.Resources;
                string[] before=new string[11];for(int i=0;i<10;++i)before[i]=(mask&(1<<i))!=0?"custom"+i:"";before[10]="customConfig";
                r.Background=before[0];r.Foreground=before[1];r.Cascade=before[2];r.MessageFont=before[3];r.AmountFont=before[4];
                r.TranslationFont=before[5];r.Sound=before[6];s.SmallMessageFont=before[7];s.SmallAmountFont=before[8];r.Configuration=before[10];
                var owner=new OriginalCashoutDefaultResources.Owner {Selection=s,FontType=fontType,SmallTranslationFont=before[9],ResourcesAvailable=true};
                var environment=new OriginalCashoutDefaultResources.EnvironmentState();var trace=new List<string>();
                byte channels=(byte)((mask&1)==0?2:3);
                var host=new OriginalCashoutDefaultResources.Host {
                    GetEnvironment=name=>{check(name=="AGKVIDEO");trace.Add("env");return video;},
                    GetDisplaySize=()=>{trace.Add("display");return new OriginalCashoutConstructorResources.Size(1360,768);},
                    ConfiguredChannels=()=>{trace.Add("channels");return channels;},FileExists=p=>{trace.Add(p);return true;}};
                OriginalCashoutDefaultResources.Fill(owner,environment,host);
                string ext=video=="OGL"||video=="V2GL"||video=="OGL\0ignored"?".bik":".mng";
                string fontExt=fontType==1?".ttfnt":".fnt";const string bm="/agk/fonts/ArialUnicode-Bold-BM.fnt";
                var expected=(string[])before.Clone();bool defaults=(mask&31)!=31;
                if(defaults)
                {
                    if(before[0]=="")expected[0]=still?"/agk/picts/CashOutAnimation-Bottom-Curve.png":"/agk/picts/CashOutAnimation-Bottom-1360x768.bik";
                    if(before[1]=="")expected[1]="/agk/picts/CashOutAnimation-Top-"+(still?"Curve":"1360x768")+ext;
                    if(still&&before[2]=="")expected[2]="/agk/picts/CashOutAnimation-Cascade-Curve"+ext;
                    if(before[3]=="")expected[3]=fontType==2?bm:"/agk/fonts/CashOutFont-Msg-1360x768"+fontExt;
                    if(before[4]=="")expected[4]=fontType==2?bm:"/agk/fonts/CashOutFont-Amt-1360x768"+fontExt;
                    if(fontType==0){if(before[7]=="")expected[7]="/agk/fonts/CashOutFont-Msg-Small-1360x768.fnt";if(before[8]=="")expected[8]=expected[4];}
                    expected[9]="/agk/fonts/CashOutFont-Trans-Small-1360x768.fnt";
                }
                if(before[5]=="")expected[5]=fontType==2?bm:"/agk/fonts/CashOutFont-Trans-1360x768"+fontExt;
                if(before[6]=="")expected[6]=channels<3?"/agk/sounds/CashOutSound.wav":"/agk/sounds/CashOutSurroundSound.wav";
                string[] actual=Fields(owner);for(int i=0;i<11;++i)check(actual[i]==expected[i]);
                check(environment.AnimationExtension==ext&&owner.ResourcesAvailable);
                var expectedTrace=new List<string>{"env","display"};if(before[6]=="")expectedTrace.Add("channels");
                expectedTrace.Add(expected[0]);expectedTrace.Add(expected[1]);if(still)expectedTrace.Add(expected[2]);
                foreach(int index in new[]{3,4,5,6})expectedTrace.Add(expected[index]);
                check(string.Join("|",trace)==string.Join("|",expectedTrace));
            }
            // All availability masks prove short circuit and static-only cascade requirements.
            foreach(bool still in new[]{false,true})for(int mask=0;mask<128;++mask)
            {
                var s=new OriginalCashoutConstructorResources.Selection {StaticBackground=still};var r=s.Resources;
                r.Background="bg";r.Foreground="fg";r.Cascade="cascade";r.MessageFont="msg";r.AmountFont="amt";r.TranslationFont="trans";r.Sound="sound";
                var owner=new OriginalCashoutDefaultResources.Owner {Selection=s,FontType=0,SmallTranslationFont="keepSmall",ResourcesAvailable=true};
                var paths=new List<string>{"bg","fg"};if(still)paths.Add("cascade");paths.AddRange(new[]{"msg","amt","trans","sound"});
                int seen=0;var trace=new List<string>();
                OriginalCashoutDefaultResources.Fill(owner,new OriginalCashoutDefaultResources.EnvironmentState(),new OriginalCashoutDefaultResources.Host {
                    GetEnvironment=name=>"OGL",GetDisplaySize=()=>new OriginalCashoutConstructorResources.Size(800,600),
                    ConfiguredChannels=()=>{throw new Exception("Unexpected sound override");},FileExists=p=>{check(p==paths[seen]);trace.Add(p);return (mask&(1<<seen++))!=0;}});
                int needed=paths.Count;int expectedQueries=needed;bool ready=true;
                for(int i=0;i<needed;++i)if((mask&(1<<i))==0){expectedQueries=i+1;ready=false;break;}
                check(owner.ResourcesAvailable==ready&&seen==expectedQueries);check(owner.SmallTranslationFont=="keepSmall");
            }
            // Process-wide extension is sticky after a non-GL constructor, including embedded NUL.
            var shared=new OriginalCashoutDefaultResources.EnvironmentState();
            foreach(string video in new[]{"V2GL",null,"OGL","V2GL\0ignored"})
            {
                var owner=new OriginalCashoutDefaultResources.Owner {Selection=new OriginalCashoutConstructorResources.Selection(),FontType=0};
                OriginalCashoutDefaultResources.Fill(owner,shared,new OriginalCashoutDefaultResources.Host {GetEnvironment=n=>video,
                    GetDisplaySize=()=>new OriginalCashoutConstructorResources.Size(-1,-32768),ConfiguredChannels=()=>255,FileExists=p=>true});
                string ext=video=="V2GL"?".bik":".mng";
                check(shared.AnimationExtension==ext);check(owner.Selection.Resources.Foreground=="/agk/picts/CashOutAnimation-Top--1x-32768"+ext);
                check(owner.Selection.Resources.Background=="/agk/picts/CashOutAnimation-Bottom--1x-32768.bik");
            }
            return checks;
        }
    }
}
