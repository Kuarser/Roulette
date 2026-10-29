using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    // Native SpriteLabelAdapter 086bd390 / 086bdef0, classic C byte semantics.
    public static class OriginalCashoutFontDescription
    {
        public sealed class Description
        {
            public string Typeface="arial";
            public int PointSize=20, Style;
            public uint Color=0xffffff;
        }
        internal static string CString(string value)
        {
            if(value==null)throw new ArgumentNullException(nameof(value));
            int n=value.IndexOf('\0');return n<0?value:value.Substring(0,n);
        }
        private static bool Space(int c){return c==32||c>=9&&c<=13;}
        private static string Bytes(byte[] bytes,int start,int count)
        {
            var chars=new char[count];for(int i=0;i<count;++i)chars[i]=(char)bytes[start+i];return new string(chars);
        }
        public static bool IsBitmapFormat(byte[] bytes)
        {
            if(bytes==null)return false;int end=Array.IndexOf(bytes,(byte)10);if(end<0)end=bytes.Length;
            int begin=0;while(begin<end&&Space(bytes[begin]))++begin;
            int at=begin;while(at<end&&!Space(bytes[at]))++at;
            return at-begin==4&&bytes[begin]=='i'&&bytes[begin+1]=='n'&&bytes[begin+2]=='f'&&bytes[begin+3]=='o';
        }
        public static int Strtol32(string text)
        {
            int i=0;while(i<text.Length&&Space(text[i]))++i;
            bool negative=i<text.Length&&text[i]=='-';if(i<text.Length&&(text[i]=='+'||text[i]=='-'))++i;
            ulong limit=negative?2147483648UL:2147483647UL,value=0;
            while(i<text.Length&&text[i]>='0'&&text[i]<='9')
            {
                uint digit=(uint)(text[i++]-'0');value=value>(limit-digit)/10?limit:value*10+digit;
            }
            return negative?unchecked((int)-(long)value):(int)value;
        }
        public static Description ParseTtfnt(byte[] bytes)
        {
            if(bytes==null)throw new InvalidOperationException("Native TTFNT open failure would not progress");
            var result=new Description();int start=0;
            while(start<bytes.Length)
            {
                int end=Array.IndexOf(bytes,(byte)10,start);if(end<0)end=bytes.Length;
                // getline(buffer,255) allows 254 payload bytes, then delimiter/EOF.
                if(end-start>254)throw new InvalidOperationException("Native TTFNT getline failbit would not progress");
                int nul=Array.IndexOf(bytes,(byte)0,start,end-start);int count=(nul<0?end:nul)-start;
                string line=Bytes(bytes,start,count);start=end==bytes.Length?end:end+1;
                int equals=line.IndexOf('=');if(equals<0)continue;
                string key=line.Substring(0,equals),value=line.Substring(equals+1);
                switch(key)
                {
                    case "typeface":result.Typeface=value;break;
                    case "pointsize":result.PointSize=Strtol32(value);break;
                    case "bold":if(value=="true")result.Style|=1;break;
                    case "italic":if(value=="true")result.Style|=2;break;
                    case "color":
                        int first=value.IndexOf(','),last=value.LastIndexOf(',');
                        if(first<0){result.Color=0xffffff;break;}
                        uint r=unchecked((byte)Strtol32(value.Substring(0,first)));
                        uint g=unchecked((byte)Strtol32(value.Substring(first+1,last-first)));
                        uint b=unchecked((byte)Strtol32(value.Substring(last+1)));
                        result.Color=(r<<16)|(g<<8)|b;break;
                }
            }
            return result;
        }
        // Explicit path mapping; callers choose the original IMG mount.
        public static Func<string,byte[]> LocalReader(Func<string,string> resolve)
        {
            if(resolve==null)throw new ArgumentNullException(nameof(resolve));
            return path=>
            {
                try{return File.ReadAllBytes(resolve(CString(path)));}
                catch(IOException){return null;}
                catch(UnauthorizedAccessException){return null;}
                catch(ArgumentException){return null;}
            };
        }
    }

    // TextScreen 086bf220 / 086c1c30 / 086c3540. One SharedSizes instance
    // represents the process global array; file buffers and colours belong to owners.
    public sealed class OriginalCashoutFontBinding
    {
        public sealed class SharedSizes
        {
            public readonly int[] Values={40,32,28,24,20,16,12,48,96,25,24,48,12};
        }
        public readonly SharedSizes Sizes;
        public readonly Dictionary<int,uint> Colors=new Dictionary<int,uint>();
        private readonly byte[][] files=new byte[13][];
        private readonly Func<string,byte[]> read;
        public int FontMode {get;private set;}
        public OriginalCashoutFontBinding(SharedSizes sizes,Func<string,byte[]> reader)
        {
            Sizes=sizes??throw new ArgumentNullException(nameof(sizes));read=reader??throw new ArgumentNullException(nameof(reader));
            for(int i=0;i<files.Length;++i)files[i]=new byte[64];
        }
        private static void Slot(int slot){if(slot<0||slot>=13)throw new ArgumentOutOfRangeException(nameof(slot),"Native out-of-array font writes are unsupported");}
        public byte[] FileBuffer(int slot){Slot(slot);return(byte[])files[slot].Clone();}
        public string FileName(int slot){Slot(slot);return Encoding.UTF8.GetString(files[slot],0,Array.IndexOf(files[slot],(byte)0));}
        internal void ClearNativeFontFile(int slot){Slot(slot);Array.Clear(files[slot],0,64);}
        internal void CopyNativeFontFile(int slot,string path)
        {
            Slot(slot);byte[] name=Encoding.ASCII.GetBytes(path);if(name.Length>=64)throw new ArgumentException("Native font filename exceeds its buffer");
            Buffer.BlockCopy(name,0,files[slot],0,name.Length);files[slot][name.Length]=0;
        }
        public void SetUseFontType(int mode){FontMode=mode;}
        public void SetCustomFontColor(int slot,uint rgb){if(FontMode==2)Colors[slot]=rgb&0xffffff;}
        public bool IsBitmapFontFormat(string path){return OriginalCashoutFontDescription.IsBitmapFormat(read(OriginalCashoutFontDescription.CString(path)));}
        public void SetCustomFont(int slot,string path,int suppliedSize)
        {
            Slot(slot);byte[] name=Encoding.UTF8.GetBytes(OriginalCashoutFontDescription.CString(path));
            if(name.Length>=64)throw new ArgumentException("Native 64-byte strcpy overflow is unsupported",nameof(path));
            Buffer.BlockCopy(name,0,files[slot],0,name.Length);files[slot][name.Length]=0; // Retain strcpy tail.
            if(path.IndexOf(".ttfnt",StringComparison.Ordinal)>=0)
                Sizes.Values[slot]=OriginalCashoutFontDescription.ParseTtfnt(read(OriginalCashoutFontDescription.CString(path))).PointSize;
            else if(IsBitmapFontFormat(path)){Sizes.Values[slot]=suppliedSize;Colors.Remove(slot);}
        }
        public OriginalCashoutTypographySetup.FontHost Bind()
        {
            return new OriginalCashoutTypographySetup.FontHost {IsBitmapFontFormat=IsBitmapFontFormat,SetCustomFont=SetCustomFont,SetCustomFontColor=SetCustomFontColor};
        }
        // Only the previously validated original AGK corpus is accepted. TTF and
        // text BMFont need their own rasterizers, and malformed metadata stays rejected.
        internal OriginalBitmapFont LoadQualifiedAgkFont(int slot)
        {
            string file=FileName(slot);const string prefix="/agk/fonts/";
            if(FontMode==1||FontMode==2||!file.StartsWith(prefix,StringComparison.Ordinal)||!file.EndsWith(".fnt",StringComparison.Ordinal))
                throw new InvalidOperationException("Qualified original AGK font binding required");
            string stem=file.Substring(prefix.Length,file.Length-prefix.Length-4);
            string[] qualified={"CashOutFont-Amt-1080x1920","CashOutFont-Amt-1360x768","CashOutFont-Amt-1600x1200","CashOutFont-Amt-1920x1080","CashOutFont-Amt-768x1360","CashOutFont-Msg-1360x768","CashOutFont-Msg-768x1360","CashOutFont-Msg-Small-1360x768","CashOutFont-Msg-Small-768x1360","CashOutFont-Trans-1360x768","CashOutFont-Trans-768x1360","CashOutFont-Trans-Small-1360x768","CashOutFont-Trans-Small-768x1360"};
            string[] ordinary={"arial-bold-20-white","arial-bold-16-white","arial-bold-14-white","arial-bold-12-white","arial-bold-10-white","arial-bold-8-white","arial-bold-6-white",
                "simsun-14-white-uni","simsun-12-white-uni","simsun-10-white-uni","simsun-8-white-uni","simsun-6-white-uni"};
            string resource=Array.IndexOf(qualified,stem)>=0?"CashOut/"+stem:Array.IndexOf(ordinary,stem)>=0?"OS366J/"+stem:null;
            if(resource==null)throw new InvalidOperationException("Unqualified original AGK font: "+file);
            var font=OriginalBitmapFont.Load(resource);if(font==null)throw new InvalidOperationException("Qualified font resource absent: "+file);return font;
        }
    }
}
