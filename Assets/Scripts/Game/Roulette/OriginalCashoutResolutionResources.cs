using System;
using System.Globalization;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreenCashOut::SetResolutionSpecificFileNames, 086c5a20.
    public sealed class OriginalCashoutResolutionResources
    {
        public string Background="",Foreground="",Cascade="",Sound="",MessageFont="",AmountFont="",TranslationFont="",Configuration="";
        public bool Select(string directory,int width,int height,Func<string,bool> exists,Func<byte> configuredChannels)
        {
            if(directory==null||exists==null||configuredChannels==null)throw new ArgumentNullException();
            string suffix="-"+width.ToString(CultureInfo.InvariantCulture)+"x"+height.ToString(CultureInfo.InvariantCulture);
            Background=Foreground=Cascade=Sound=MessageFont=AmountFont=TranslationFont=Configuration="";
            bool found=false;
            Func<string,string,string> pick=(name,ext)=>{string path=directory+name+suffix+ext;if(!exists(path))return "";found=true;return path;};
            Background=pick("CashOutAnimation-Bottom",".x");Foreground=pick("CashOutAnimation-Top",".x");Cascade=pick("CashOutAnimation-Cascade",".x");
            string sound=directory+(configuredChannels()<3?"CashOutSound.x":"CashOutSurroundSound.x");
            if(exists(sound)){Sound=sound;found=true;}
            MessageFont=pick("CashOutFont-Msg",".x");AmountFont=pick("CashOutFont-Amt",".x");TranslationFont=pick("CashOutFont-Trans",".x");
            Configuration=pick("CashOutAnimation",".cfg");return found;
        }
    }
    public static class OriginalCashoutResolutionResourcesValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut resolution regression "+checks);++checks;};
            int[,] sizes={{800,600},{540,960},{1360,768},{-1,int.MinValue}};
            string[] suffixes={"-800x600","-540x960","-1360x768","--1x-2147483648"};
            foreach(string root in new[]{"/g/","","/without/slash"})foreach(byte channels in new byte[]{0,2,3,255})
            for(int size=0;size<4;++size)for(int mask=0;mask<256;++mask)
            {
                string suffix=suffixes[size];string[] paths={root+"CashOutAnimation-Bottom"+suffix+".x",root+"CashOutAnimation-Top"+suffix+".x",
                    root+"CashOutAnimation-Cascade"+suffix+".x",root+(channels<3?"CashOutSound.x":"CashOutSurroundSound.x"),
                    root+"CashOutFont-Msg"+suffix+".x",root+"CashOutFont-Amt"+suffix+".x",root+"CashOutFont-Trans"+suffix+".x",root+"CashOutAnimation"+suffix+".cfg"};
                var resource=new OriginalCashoutResolutionResources {Background="old",Foreground="old",Cascade="old",Sound="old",MessageFont="old",AmountFont="old",TranslationFont="old",Configuration="old"};
                var trace=new List<string>();int seen=0;
                Func<string,bool> exists=path=>{
                    if(seen==0)check(resource.Background==""&&resource.Foreground==""&&resource.Cascade==""&&resource.Sound==""&&resource.MessageFont==""&&resource.AmountFont==""&&resource.TranslationFont==""&&resource.Configuration=="");
                    check(path==paths[seen]);trace.Add(path);return (mask&(1<<seen++))!=0;
                };
                check(resource.Select(root,sizes[size,0],sizes[size,1],exists,()=>{check(seen==3);trace.Add("channels");return channels;})==(mask!=0));
                string[] actual={resource.Background,resource.Foreground,resource.Cascade,resource.Sound,resource.MessageFont,resource.AmountFont,resource.TranslationFont,resource.Configuration};
                for(int i=0;i<8;++i)check(actual[i]==((mask&(1<<i))!=0?paths[i]:""));
                check(seen==8&&trace.Count==9&&trace[3]=="channels");
                // A later empty probe must clear earlier selections and return false.
                check(!resource.Select(root,1,2,path=>false,()=>channels)&&resource.Background==""&&resource.Sound==""&&resource.Configuration=="");
            }
            return checks;
        }
    }
}
