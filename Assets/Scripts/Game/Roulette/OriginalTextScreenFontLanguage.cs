using System;
using System.Text;
namespace RouletteDeluxe
{
    // TextScreen::SetLang086c1d00. Byte strings preserve embedded NUL and
    // signed char arguments to C tolower. Font release belongs to the caller.
    public sealed class OriginalTextScreenFontLanguage
    {
        private static readonly string[] English={
            "/agk/fonts/arial-bold-20-white.fnt","/agk/fonts/arial-bold-16-white.fnt","/agk/fonts/arial-bold-14-white.fnt",
            "/agk/fonts/arial-bold-12-white.fnt","/agk/fonts/arial-bold-10-white.fnt","/agk/fonts/arial-bold-8-white.fnt","/agk/fonts/arial-bold-6-white.fnt",
            "custom_1","custom_2","custom_3","custom_1_small","custom_2_small","custom_3_small"};
        private static readonly string[] Chinese={
            "/agk/fonts/simsun-14-white-uni.fnt","/agk/fonts/simsun-14-white-uni.fnt","/agk/fonts/simsun-14-white-uni.fnt",
            "/agk/fonts/simsun-12-white-uni.fnt","/agk/fonts/simsun-10-white-uni.fnt","/agk/fonts/simsun-8-white-uni.fnt","/agk/fonts/simsun-6-white-uni.fnt",
            "custom_1","custom_2","custom_3","custom_1_small","custom_2_small","custom_3_small"};
        private readonly OriginalCashoutDisplay.State state;
        private readonly OriginalCashoutFontBinding binding;
        private readonly Func<int,int> lower;
        private readonly Action<int,object> release;
        private byte[] language;
        public byte[] Language {get{return(byte[])language.Clone();}}
        internal static void InitializeEnglishFiles(OriginalCashoutDisplay.State state,OriginalCashoutFontBinding binding)
        {
            for(int slot=0;slot<13;++slot){binding.ClearNativeFontFile(slot);binding.CopyNativeFontFile(slot,English[slot]);state.FontFiles[slot]=English[slot];}
        }
        public OriginalTextScreenFontLanguage(OriginalCashoutDisplay.State owner,OriginalCashoutFontBinding fonts,
            byte[] initialLanguage,Func<int,int> nativeLowercase,Action<int,object> releaseFont)
        {
            state=owner??throw new ArgumentNullException(nameof(owner));binding=fonts??throw new ArgumentNullException(nameof(fonts));
            language=(byte[])(initialLanguage??throw new ArgumentNullException(nameof(initialLanguage))).Clone();
            lower=nativeLowercase??throw new ArgumentNullException(nameof(nativeLowercase));release=releaseFont??throw new ArgumentNullException(nameof(releaseFont));
        }
        private static bool Equal(byte[] a,byte[] b)
        {if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public bool SetLanguage(byte[] requested)
        {
            if(requested==null)throw new ArgumentNullException(nameof(requested));if(Equal(requested,language))return false;
            byte[] value=(byte[])requested.Clone();
            for(int slot=0;slot<13;++slot)
            {
                if(state.Fonts[slot]!=null){release(slot,state.Fonts[slot]);state.Fonts[slot]=null;}
                binding.ClearNativeFontFile(slot);state.FontFiles[slot]="";
                for(int i=0;i<value.Length;++i)value[i]=unchecked((byte)lower(unchecked((sbyte)value[i])));
                bool chinese=Equal(value,new byte[]{122,104})||Equal(value,new byte[]{99,104,105,110,101,115,101});
                language=chinese?(byte[])value.Clone():new byte[]{101,110};
                string file=(chinese?Chinese:English)[slot];binding.CopyNativeFontFile(slot,file);state.FontFiles[slot]=file;
            }
            return true;
        }
    }
}
