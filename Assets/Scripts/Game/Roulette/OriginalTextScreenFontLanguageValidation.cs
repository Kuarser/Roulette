using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalTextScreenFontLanguageValidation
    {
        [Serializable] private sealed class Reference {public string[] english,chinese;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        private static int Lower(int value){return value>=65&&value<=90?value+32:value;}
        public static int Validate(string nativeReference)
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("TextScreen font language check "+checks);++checks;};
            var reference=JsonUtility.FromJson<Reference>(nativeReference);check(reference.english.Length==13&&reference.chinese.Length==13);
            int cases=0;
            foreach(string initial in new[]{"en","zh","chinese","ZH"})foreach(string requested in new[]{"en","EN","zh","ZH","Chinese","zh_CN","","en\0","zh\0"})
            for(int pattern=0;pattern<17;++pattern)
            {
                ++cases;int reads=0;var sizes=new OriginalCashoutFontBinding.SharedSizes();var binding=new OriginalCashoutFontBinding(sizes,path=>{++reads;return null;});binding.SetUseFontType(2);
                var state=new OriginalCashoutDisplay.State {FontMode=2,Playback=new OriginalCashoutPlaybackLifecycle.State()};var values=new object[13];var before=new byte[13][];
                for(int i=0;i<13;++i){binding.SetCustomFont(i,"/old/custom_font_name_with_retained_tail.fnt",16);before[i]=binding.FileBuffer(i);binding.SetCustomFontColor(i,(uint)(0x100+i));sizes.Values[i]=700+i;
                    bool present=pattern==1||pattern==2&&i%2==0||pattern==3&&i%2!=0||pattern>=4&&i==pattern-4;
                    if(present)state.Fonts[i]=values[i]=new object();state.FontFiles[i]="old";}
                var line=new OriginalCashoutDisplay.Line {Text=new byte[]{65},FontSlot=7,Height=55};state.Lines.Add(line);state.Cursor=7;state.Playback.Visible=true;state.Playback.Paused=true;
                var released=new List<int>();int lowerCalls=0;byte[] input=Encoding.UTF8.GetBytes(requested),original=(byte[])input.Clone();int readBefore=reads;
                var owner=new OriginalTextScreenFontLanguage(state,binding,Encoding.UTF8.GetBytes(initial),value=>
                {
                    int slot=lowerCalls/input.Length;check(state.Fonts[slot]==null&&state.FontFiles[slot]=="");foreach(byte b in binding.FileBuffer(slot))check(b==0);
                    ++lowerCalls;return Lower(value);
                },(slot,value)=>{check(value==values[slot]&&state.Fonts[slot]==value&&binding.FileBuffer(slot)[0]=='/');released.Add(slot);});
                bool changed=!Equal(Encoding.UTF8.GetBytes(initial),input);check(owner.SetLanguage(input)==changed);check(Equal(input,original)&&reads==readBefore);
                byte[] lower=(byte[])input.Clone();for(int i=0;i<lower.Length;++i)lower[i]=(byte)Lower(lower[i]);
                bool chinese=Equal(lower,Encoding.ASCII.GetBytes("zh"))||Equal(lower,Encoding.ASCII.GetBytes("chinese"));
                check(Equal(owner.Language,changed?(chinese?lower:Encoding.ASCII.GetBytes("en")):input));check(lowerCalls==(changed?input.Length*13:0));
                int releaseIndex=0;
                for(int i=0;i<13;++i)
                {
                    check(state.Fonts[i]==(changed?null:values[i]));check(sizes.Values[i]==700+i&&binding.Colors[i]==(uint)(0x100+i)&&binding.FontMode==2&&state.FontMode==2);
                    if(changed){string file=(chinese?reference.chinese:reference.english)[i];check(binding.FileName(i)==file&&state.FontFiles[i]==file);
                        byte[] expected=new byte[64],name=Encoding.ASCII.GetBytes(file);Array.Copy(name,expected,name.Length);check(Equal(binding.FileBuffer(i),expected));if(values[i]!=null)check(released[releaseIndex++]==i);}
                    else check(Equal(binding.FileBuffer(i),before[i])&&state.FontFiles[i]=="old");
                }
                check(released.Count==releaseIndex&&state.Lines.Count==1&&state.Lines[0]==line&&state.Cursor==7&&state.Playback.Visible&&state.Playback.Paused);
                byte[] returned=owner.Language;if(returned.Length>0){byte saved=returned[0];returned[0]^=1;check(owner.Language[0]==saved);}
            }
            check(cases==612);
            // Native applies tolower to signed char, including embedded NUL,
            // and repeats transformation inside all13 iterations.
            var other=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),path=>null);var otherState=new OriginalCashoutDisplay.State();var arguments=new List<int>();
            var custom=new OriginalTextScreenFontLanguage(otherState,other,new byte[]{1},v=>{arguments.Add(v);return v=='z'?'h':v;},(slot,value)=>{throw new Exception("Unexpected release");});
            check(custom.SetLanguage(new byte[]{(byte)'z',(byte)'h',0x80,0xff,0}));check(arguments.Count==65&&arguments[0]=='z'&&arguments[2]==-128&&arguments[3]==-1&&arguments[4]==0&&arguments[5]=='h');check(Equal(custom.Language,Encoding.ASCII.GetBytes("en")));
            var root=new GameObject("Bound TextScreen font language",typeof(RectTransform));OriginalCashoutBitmapScreen screen=null;
            try
            {
                var fonts=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),path=>null);fonts.SetCustomFont(7,"/agk/fonts/CashOutFont-Msg-1360x768.fnt",16);
                var state=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State()};var display=new OriginalCashoutDisplay.Host();var life=new OriginalCashoutPlaybackLifecycle.Host();
                screen=new OriginalCashoutBitmapScreen(root.transform,1360,768,state,fonts,display,life,"en",Encoding.UTF8.GetString,
                    (slot,text)=>unchecked((short)fonts.LoadQualifiedAgkFont(slot).MeasureWidth(Encoding.UTF8.GetString(text))),key=>key);
                screen.AddText(Encoding.ASCII.GetBytes("Call Attendant"),7);object originalFont=state.Fonts[7];int releases=0;
                screen.BindFontLanguage(Lower,(slot,value)=>{check(slot==7&&value==originalFont);++releases;});
                screen.RouterScreen.SetLanguage("en");check(releases==0&&state.Fonts[7]==originalFont);
                screen.RouterScreen.SetLanguage("ZH");check(releases==1&&state.Fonts[7]==null&&screen.FontLanguage=="zh"&&fonts.FileName(0)==reference.chinese[0]&&fonts.FileName(7)=="custom_1");
                check(screen.Layout.Lines.Count==1&&state.Lines.Count==1&&screen.Labels.Root!=null);
                bool rejected=false;try{screen.AddText(Encoding.ASCII.GetBytes("Next"),7);}catch(InvalidOperationException){rejected=true;}check(rejected&&state.Lines.Count==1);
                fonts.SetCustomFont(7,"/agk/fonts/CashOutFont-Msg-1360x768.fnt",16);screen.AddText(Encoding.ASCII.GetBytes("Next"),7);
                check(state.Fonts[7]!=null&&state.Lines.Count==2&&screen.FontLanguage=="zh");
                screen.RouterScreen.SetLanguage("zh_CN");check(screen.FontLanguage=="en"&&fonts.FileName(0)==reference.english[0]&&state.Fonts[7]==null&&releases==2);
            }
            finally{if(screen!=null)screen.Dispose();UnityEngine.Object.DestroyImmediate(root);}
            return checks;
        }
    }
}
