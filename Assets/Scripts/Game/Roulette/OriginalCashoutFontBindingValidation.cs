using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCashoutFontBindingValidation
    {
        private static byte[] B(string value){return Encoding.UTF8.GetBytes(value);}
        public static int Validate(string originalFonts)
        {
            int checks=0;Action<bool> check=condition=>{if(!condition)throw new Exception("CashOut font binding check "+checks);++checks;};
            Action<Action> rejects=action=>{bool rejected=false;try{action();}catch(InvalidOperationException){rejected=true;}check(rejected);};
            string[] yes={"info","info face=Arial\n"," \t\v\f\rinfo\r\n","info\0","info \0","info\nignored"};
            foreach(string text in yes)check(OriginalCashoutFontDescription.IsBitmapFormat(B(text))==(text!="info\0"));
            foreach(string text in new[]{"","\ninfo","INFO","information","unicode_font info","picture_font","BMF\x03","\ufeffinfo","<font>\ninfo","info\0extra","infoX","inf o"})
                check(!OriginalCashoutFontDescription.IsBitmapFormat(B(text)));
            check(!OriginalCashoutFontDescription.IsBitmapFormat(null));
            var defaults=OriginalCashoutFontDescription.ParseTtfnt(new byte[0]);check(defaults.Typeface=="arial"&&defaults.PointSize==20&&defaults.Style==0&&defaults.Color==0xffffff);
            var parsed=OriginalCashoutFontDescription.ParseTtfnt(B("typeface=first\n typeface=ignored\npointsize= -42px\nbold=true\nbold=false\nitalic=true\nitalic=false\ncolor=-1,258,511\ntypeface=last=face\n"));
            check(parsed.Typeface=="last=face"&&parsed.PointSize==-42&&parsed.Style==3&&parsed.Color==0xff02ff);
            parsed=OriginalCashoutFontDescription.ParseTtfnt(B("typeface=Arial\r\npointsize= 25tail\r\nbold=true\r\nitalic=True\ncolor=1,2\n"));
            check(parsed.Typeface=="Arial\r"&&parsed.PointSize==25&&parsed.Style==0&&parsed.Color==0x010002);
            parsed=OriginalCashoutFontDescription.ParseTtfnt(B("pointsize=7\0pointsize=8\ncolor=1,2,3,4\n"));check(parsed.PointSize==7&&parsed.Color==0x010204);
            parsed=OriginalCashoutFontDescription.ParseTtfnt(B("color=1,2,3\ncolor=nocomma\npointsize=nonnumeric\n"));check(parsed.Color==0xffffff&&parsed.PointSize==0);
            foreach(bool newline in new[]{false,true})
            {
                parsed=OriginalCashoutFontDescription.ParseTtfnt(B("typeface="+new string('a',245)+(newline?"\n":"")));check(parsed.Typeface.Length==245);
                rejects(()=>OriginalCashoutFontDescription.ParseTtfnt(B(new string('x',255)+(newline?"\n":""))));
            }
            rejects(()=>OriginalCashoutFontDescription.ParseTtfnt(null));
            for(int n=short.MinValue;n<=short.MaxValue;++n)
            {
                string text=n.ToString(CultureInfo.InvariantCulture);check(OriginalCashoutFontDescription.Strtol32(" \t"+text+"px")==n);
                check(OriginalCashoutFontDescription.ParseTtfnt(B("pointsize="+text)).PointSize==n);
            }
            string[] numbers={"2147483647","2147483648","9999999999999999999999999","-2147483648","-2147483649","-9999999999999999999999999"," +0x20","--5","\u0661","+"," \v-0012tail","23\0ignored"};
            int[] values={int.MaxValue,int.MaxValue,int.MaxValue,int.MinValue,int.MinValue,int.MinValue,0,0,0,0,-12,23};
            for(int i=0;i<numbers.Length;++i)check(OriginalCashoutFontDescription.Strtol32(numbers[i])==values[i]);
            int cases=0;
            for(int slot=0;slot<13;++slot)for(int mode=-2;mode<=4;++mode)for(int kind=0;kind<3;++kind)foreach(int size in new[]{int.MinValue,-1,0,int.MaxValue})for(int seed=0;seed<2;++seed)
            {
                ++cases;var shared=new OriginalCashoutFontBinding.SharedSizes();int[] before=(int[])shared.Values.Clone();
                var trace=new List<string>();string file=kind==0?"agk.fnt":kind==1?"bitmap.any":"descriptor.ttfnt.extra";
                var owner=new OriginalCashoutFontBinding(shared,path=>{trace.Add(path);return B(kind==0?"unicode_font 3":kind==1?"info face=test":"pointsize=-37\n");});
                var second=new OriginalCashoutFontBinding(shared,path=>null);owner.SetUseFontType(mode);check(owner.FontMode==mode);
                if(seed!=0)owner.Colors[slot]=0xabcdef;
                owner.SetCustomFontColor(slot,0x12345678);check(owner.Colors.ContainsKey(slot)==(seed!=0||mode==2));
                if(mode==2)check(owner.Colors[slot]==0x345678);
                owner.SetCustomFont(slot,file,size);check(owner.FileName(slot)==file&&trace.Count==1&&trace[0]==file);
                for(int i=0;i<13;++i)check(shared.Values[i]==(i==slot?(kind==0?before[i]:kind==1?size:-37):before[i]));
                check(second.Sizes.Values[slot]==shared.Values[slot]&&second.FileName(slot)==""&&second.Colors.Count==0);
                check(owner.Colors.ContainsKey(slot)==(kind!=1&&(seed!=0||mode==2)));
            }
            check(cases==2184);
            var sizes=new OriginalCashoutFontBinding.SharedSizes();int readCount=0;
            var binding=new OriginalCashoutFontBinding(sizes,path=>{check(path=="prefix");++readCount;return B("pointsize=71");});
            binding.SetCustomFont(7,"prefix\0.ttfnt",16);check(binding.FileName(7)=="prefix"&&sizes.Values[7]==71&&readCount==1);
            binding.SetCustomFont(7,"prefix",16);byte[] buffer=binding.FileBuffer(7);check(buffer[6]==0);
            var tail=new OriginalCashoutFontBinding(sizes,path=>B("unicode_font"));tail.SetCustomFont(0,"longfilename",4);tail.SetCustomFont(0,"a",5);buffer=tail.FileBuffer(0);
            check(buffer[0]=='a'&&buffer[1]==0&&buffer[2]=='n'&&tail.FileName(0)=="a");buffer[0]=0;check(tail.FileName(0)=="a");
            tail.SetUseFontType(2);tail.SetCustomFontColor(-1,0xffffff);tail.SetCustomFontColor(99,0x112233);check(tail.Colors[-1]==0xffffff&&tail.Colors[99]==0x112233);
            var failure=new OriginalCashoutFontBinding(sizes,path=>null);failure.Colors[1]=7;
            rejects(()=>failure.SetCustomFont(1,"missing.ttfnt",9));check(failure.FileName(1)=="missing.ttfnt"&&failure.Colors[1]==7&&sizes.Values[1]==32);
            failure.SetCustomFont(1,"missing.fnt",9);check(sizes.Values[1]==32&&failure.Colors[1]==7);
            foreach(int invalid in new[]{-1,13}){bool rejected=false;try{tail.SetCustomFont(invalid,"a",0);}catch(ArgumentOutOfRangeException){rejected=true;}check(rejected);}
            bool overflow=false;try{tail.SetCustomFont(0,new string('a',64),0);}catch(ArgumentException){overflow=true;}check(overflow&&tail.FileName(0)=="a");
            tail.SetCustomFont(0,new string('a',63),0);check(tail.FileName(0).Length==63);
            var read=OriginalCashoutFontDescription.LocalReader(path=>Path.Combine(originalFonts,path.Substring("/agk/fonts/".Length)));
            int descriptors=0,agk=0;
            foreach(string file in Directory.GetFiles(originalFonts,"CashOutFont-*.ttfnt"))
            {
                ++descriptors;string name=Path.GetFileNameWithoutExtension(file);string dimensions=name.Substring(name.LastIndexOf('-')+1);
                int expected=name.Contains("-Amt-")?(dimensions=="1360x768"?105:dimensions=="768x1360"?85:150):name.Contains("-Msg-")?(dimensions=="1920x1080"?80:dimensions=="768x1360"?40:55):(dimensions=="1360x768"?25:dimensions=="768x1360"?20:35);
                var description=OriginalCashoutFontDescription.ParseTtfnt(File.ReadAllBytes(file));check(description.Typeface=="Arial Unicode MS"&&description.PointSize==expected&&description.Style==1);
                check(description.Color==(name.Contains("-Amt-")?0xffd400u:0xffffffu));
            }
            check(descriptors==12);
            foreach(string file in Directory.GetFiles(originalFonts,"CashOutFont-*.fnt")){++agk;check(!OriginalCashoutFontDescription.IsBitmapFormat(File.ReadAllBytes(file)));}check(agk==17);
            check(OriginalCashoutFontDescription.IsBitmapFormat(File.ReadAllBytes(Path.Combine(originalFonts,"ArialUnicode-Bold-BM.fnt"))));
            var actual=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),read);
            var setup=new OriginalCashoutTypographySetup();setup.ConstructorFontType(false,2,actual.SetUseFontType);
            setup.ConfigurePrimaryFonts(true,"/agk/fonts/CashOutFont-Msg-1360x768.fnt","/agk/fonts/CashOutFont-Amt-1360x768.fnt","/agk/fonts/CashOutFont-Trans-1360x768.fnt",1360,768,actual.Bind());
            int[] heights={63,115,30};for(int i=0;i<3;++i){check(actual.Sizes.Values[7+i]==new[]{48,96,25}[i]);check(actual.LoadQualifiedAgkFont(7+i).Height==heights[i]);}
            var reference=Resources.Load<TextAsset>("Original/Fonts/CashOut/source_glyph_reference");int qualified=0;
            foreach(string row in reference.text.Split('\n'))if(row.StartsWith("F ",StringComparison.Ordinal))
            {
                string stem=row.Split(' ')[1].Substring("CashOut/".Length);actual.SetCustomFont(0,"/agk/fonts/"+stem+".fnt",16);
                check(actual.LoadQualifiedAgkFont(0)==OriginalBitmapFont.Load("CashOut/"+stem));++qualified;
            }
            check(qualified==13);
            foreach(string stem in new[]{"CashOutFont-Msg-1080x1920","CashOutFont-Msg-1920x1080","CashOutFont-Trans-1080x1920","CashOutFont-Trans-1920x1080"})
            {actual.SetCustomFont(0,"/agk/fonts/"+stem+".fnt",16);rejects(()=>actual.LoadQualifiedAgkFont(0));}
            foreach(int mode in new[]{1,2}){actual.SetUseFontType(mode);rejects(()=>actual.LoadQualifiedAgkFont(7));}
            actual.SetUseFontType(1);setup.ConstructorFontType(true,1,actual.SetUseFontType);
            setup.ConfigurePrimaryFonts(true,"/agk/fonts/CashOutFont-Msg-1360x768.ttfnt","/agk/fonts/CashOutFont-Amt-1360x768.ttfnt","/agk/fonts/CashOutFont-Trans-1360x768.ttfnt",1360,768,actual.Bind());
            check(actual.Sizes.Values[7]==55&&actual.Sizes.Values[8]==105&&actual.Sizes.Values[9]==25);
            actual.SetUseFontType(2);setup.ConstructorFontType(true,2,actual.SetUseFontType);
            setup.ConfigurePrimaryFonts(true,"/agk/fonts/ArialUnicode-Bold-BM.fnt","/agk/fonts/ArialUnicode-Bold-BM.fnt","/agk/fonts/ArialUnicode-Bold-BM.fnt",1360,768,actual.Bind());
            check(actual.Sizes.Values[7]==55&&actual.Sizes.Values[8]==105&&actual.Sizes.Values[9]==25&&actual.Colors.Count==1&&actual.Colors[8]==0xffd400);
            return checks;
        }
    }
}
