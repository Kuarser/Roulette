using System;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalHandpayTextScreenRouterValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("Handpay screen router regression "+checks);++checks;};
            string[] languages={"en","","zh","EN","fr"};
            for(int flags=0;flags<64;++flags)for(int mask=0;mask<16;++mask)foreach(string language in languages)
            {
                bool manager=(flags&1)!=0,selected=(flags&2)!=0,translated=(flags&4)!=0,game=(flags&8)!=0,app=(flags&16)!=0,has234=(flags&32)!=0;
                var trace=new List<string>();OriginalHandpayTextScreenRouter router=null;
                Func<string,OriginalHandpayTextScreenRouter.Screen> screen=id=>new OriginalHandpayTextScreenRouter.Screen {
                    Defaults=()=>trace.Add(id+":defaults"),SetLanguage=s=>trace.Add(id+":language:"+s),
                    AddText=(b,slot)=>trace.Add(id+":text:"+slot+":"+Hex(b)),AddTranslatedText=(b,slot)=>trace.Add(id+":translate:"+slot+":"+Hex(b)),
                    SetOrder=o=>{check(o==9100);trace.Add(id+":order");},Init=()=>trace.Add(id+":init"),
                    Display=()=>{check(!router.MessageDisplayed);trace.Add(id+":display");}
                };
                var h=new OriginalHandpayTextScreenRouter.Host { Manager3aa=manager,Ordinary=screen("22c"),Alternate230=screen("230"),
                    Alternate234=has234?screen("234"):null,Alternate238=screen("238"),Selected=()=>{trace.Add("selected");return selected;},
                    Translate=()=>{trace.Add("jur448");return translated;},SelectedLanguage=()=>{trace.Add("language");return "zh";},
                    GameMode=()=>{trace.Add("game");return game;},AppMode=()=>{trace.Add("app");return app;},TwoLetterLanguage=()=>{trace.Add("two");return language;}
                };
                router=new OriginalHandpayTextScreenRouter(h);
                byte[][] data={new byte[]{65,95,66,0,255},new byte[]{0,88},new byte[]{255,95,49},new byte[]{68,95,69}};
                for(int i=0;i<4;++i)if((mask&(1<<i))==0)data[i]=null;
                var expected=new List<string>();if(manager)expected.Add("selected");bool alternate=manager&&selected;
                string id=alternate?(game&&app?"238":has234?"234":"230"):"22c";
                if(alternate){expected.Add("game");if(game)expected.Add("app");}
                expected.Add(id+":defaults");
                string[] hex={(mask&1)!=0?"41-5F-42":"null",(mask&2)!=0?"":"null",(mask&4)!=0?"FF-5F-31":"null",(mask&8)!=0?"44-5F-45":"null"};
                if(!alternate)
                {
                    expected.Add("jur448");if(translated){expected.Add("language");expected.Add(id+":language:zh");}
                    expected.Add(id+(translated?":translate:1:":":text:1:")+hex[0]);expected.Add(id+(translated?":translate:1:":":text:1:")+hex[1]);
                    if(data[2]!=null)expected.Add(id+":text:3:"+hex[2]);
                }
                else
                {
                    for(int i=0;i<4;++i)if(data[i]!=null)expected.Add(id+(i==2?":text:8:":i==3?":translate:9:":":translate:7:")+hex[i]);
                    expected.Add("two");if(language!="en"&&language!="")
                    {
                        var bytes=new List<byte>();if((mask&1)!=0)bytes.AddRange(new byte[]{65,32,66});bytes.Add(32);bytes.Add(32);
                        if((mask&4)!=0)bytes.AddRange(new byte[]{255,32,49});bytes.Add(32);if((mask&8)!=0)bytes.AddRange(new byte[]{68,32,69});
                        expected.Add(id+":text:9:"+Hex(bytes.ToArray()));
                    }
                }
                expected.Add(id+":order");expected.Add(id+":init");expected.Add(id+":display");
                check(router.Display(data[0],data[1],data[2],data[3]));check(router.MessageDisplayed);
                check(string.Join("|",trace)==string.Join("|",expected));check(router.CurrentScreen==(id=="22c"?h.Ordinary:id=="230"?h.Alternate230:id=="234"?h.Alternate234:h.Alternate238));
                int count=trace.Count;check(!router.Display(null,null,null,null)&&trace.Count==count);
            }
            // W2G's second Save must occur before the router reads selected.
            for(int manager=0;manager<2;++manager)
            {
                var nv=new OriginalCreditOutNvPayload(new byte[135]);var calls=new List<string>();
                Func<string,OriginalHandpayTextScreenRouter.Screen> make=id=>new OriginalHandpayTextScreenRouter.Screen {
                    Defaults=()=>calls.Add(id),AddText=(b,s)=>{},AddTranslatedText=(b,s)=>{},SetOrder=o=>{},Init=()=>{},Display=()=>{}};
                var router=new OriginalHandpayTextScreenRouter(new OriginalHandpayTextScreenRouter.Host {Manager3aa=manager!=0,
                    Ordinary=make("ordinary"),Alternate230=make("alternate"),Selected=()=>nv.ReadSelection().HandpaySelected,
                    Translate=()=>false,GameMode=()=>false,TwoLetterLanguage=()=>"en"});
                OriginalW2GHandpay.Display(995,new OriginalW2GHandpay.Host {Nv=nv,Manager3aa=manager!=0,Save=b=>calls.Add("save:"+b[0x5d]),
                    FormatMoney=(a,c,d,s)=>Encoding.ASCII.GetBytes("RM9.95"),GamePrize=()=>false,Voucher=()=>false,Translate=()=>false,
                    ThresholdEnabled=()=>false,DisplayText=(t,b,c,d)=>{check(router.Display(t,b,c,d));}});
                check(string.Join(",",calls)==(manager==0?"save:0,ordinary":"save:0,save:1,alternate"));
            }
            return checks;
        }
        private static string Hex(byte[] b){return b==null?"null":BitConverter.ToString(b);}
    }
}
