using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // CreditOutMgr::DisplayTextScreen 081ee100. Screen resources belong to host.
    public sealed class OriginalHandpayTextScreenRouter
    {
        public sealed class Screen
        {
            public Action Defaults, Init, Display;
            public Action<string> SetLanguage;
            public Action<byte[],int> AddText, AddTranslatedText;
            public Action<int> SetOrder;
        }
        public sealed class Host
        {
            public Screen Ordinary, Alternate230, Alternate234, Alternate238;
            public bool Manager3aa;
            public Func<bool> Selected, Translate, GameMode, AppMode;
            public Func<string> SelectedLanguage, TwoLetterLanguage;
        }
        public bool MessageDisplayed;
        public Screen CurrentScreen { get; private set; }
        private readonly Host host;
        public OriginalHandpayTextScreenRouter(Host value){host=value??throw new ArgumentNullException(nameof(value));}
        public bool Display(byte[] title,byte[] body,byte[] third,byte[] fourth)
        {
            if(MessageDisplayed)return false;
            if(!host.Manager3aa||!host.Selected())
            {
                CurrentScreen=host.Ordinary;CurrentScreen.Defaults();
                if(host.Translate())
                {
                    CurrentScreen.SetLanguage(host.SelectedLanguage());
                    CurrentScreen.AddTranslatedText(Cut(title),1);CurrentScreen.AddTranslatedText(Cut(body),1);
                }
                else{CurrentScreen.AddText(Cut(title),1);CurrentScreen.AddText(Cut(body),1);}
                if(third!=null)CurrentScreen.AddText(Cut(third),3);
            }
            else
            {
                CurrentScreen=host.GameMode()&&host.AppMode()?host.Alternate238:host.Alternate234??host.Alternate230;
                CurrentScreen.Defaults();
                if(title!=null)CurrentScreen.AddTranslatedText(Cut(title),7);
                if(body!=null)CurrentScreen.AddTranslatedText(Cut(body),7);
                if(third!=null)CurrentScreen.AddText(Cut(third),8);
                if(fourth!=null)CurrentScreen.AddTranslatedText(Cut(fourth),9);
                string language=host.TwoLetterLanguage();
                if(language!="en"&&language!="")
                {
                    var joined=new List<byte>();byte[][] inputs={title,body,third,fourth};
                    for(int i=0;i<4;++i)
                    {
                        if(i!=0)joined.Add(32);byte[] bytes=Cut(inputs[i]);if(bytes!=null)joined.AddRange(bytes);
                    }
                    for(int i=0;i<joined.Count;++i)if(joined[i]==95)joined[i]=32;
                    CurrentScreen.AddText(joined.ToArray(),9);
                }
            }
            CurrentScreen.SetOrder(9100);CurrentScreen.Init();CurrentScreen.Display();MessageDisplayed=true;return true;
        }
        private static byte[] Cut(byte[] input)
        {
            if(input==null)return null;int n=Array.IndexOf(input,(byte)0);if(n<0)n=input.Length;
            var result=new byte[n];Array.Copy(input,result,n);return result;
        }
    }
}
