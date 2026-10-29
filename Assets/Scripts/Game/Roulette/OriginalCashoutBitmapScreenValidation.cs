using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCashoutBitmapScreenValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut bitmap screen binding check "+checks);++checks;};
            string fontRoot=Path.Combine(Application.streamingAssetsPath,"Original/CashOutFontFiles");
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));
            var money=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");int cases=0;
            foreach(string language in new[]{"en","zh","es_CL","fr_CA"})foreach(bool voucher in new[]{false,true})foreach(bool detail in new[]{false,true})foreach(long amount in new long[]{0,995,999999})
            {
                ++cases;var root=new GameObject("CashOut routed owner test",typeof(RectTransform));OriginalCashoutBitmapScreen owner=null;
                try
                {
                    var binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),OriginalCashoutFontDescription.LocalReader(path=>Path.Combine(fontRoot,path.Substring("/agk/fonts/".Length))));
                    foreach(int slot in new[]{7,8,9,10,11,12})binding.SetCustomFont(slot,"/agk/fonts/CashOutFont-"+(slot==8||slot==11?"Amt":slot==9||slot==12?"Trans":"Msg")+(slot==10||slot==12?"-Small":"")+"-1360x768.fnt",16);
                    var state=new OriginalCashoutDisplay.State {Playback=new OriginalCashoutPlaybackLifecycle.State {ResourcesAvailable=true,ThreadedVideo=true}};
                    int displayEvents=0,endEvents=0,translations=0;
                    var life=new OriginalCashoutPlaybackLifecycle.Host {GameStatusSecondByte=()=>0,IsTiltActive=m=>false,IsMachineSuspended=()=>false,
                        Emit=(raw,p)=>{check(raw[25]==0x82&&!state.Playback.Visible);++endEvents;},SetDefaults=()=>owner.Defaults()};
                    var table=new OriginalCashoutTextBoundsTable();table.ConstructorDefaults("");
                    var display=new OriginalCashoutDisplay.Host {GetDisplaySize=()=>new short[]{1360,768},Centering=()=>owner.Layout.Centering,LookupTextBounds=table.Lookup,
                        Emit=(raw,p)=>{check(raw[25]==0x81&&state.Playback.Visible);++displayEvents;},Show=()=>OriginalCashoutPlaybackLifecycle.Show(state.Playback,life)};
                    // Explicit UTF8 prefix fixture. Production native LC_CTYPE is a
                    // separate host; no CLR replacement glyph added for incomplete input.
                    Func<int,byte[],short> measure=(slot,bytes)=>
                    {
                        var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int used=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);
                        return unchecked((short)binding.LoadQualifiedAgkFont(slot).MeasureWidth(new string(chars,0,used)));
                    };
                    owner=new OriginalCashoutBitmapScreen(root.transform,1360,768,state,binding,display,life,"en",Encoding.UTF8.GetString,measure,key=>
                    {++translations;return Encoding.UTF8.GetBytes(translator.Translate(Encoding.UTF8.GetString(key),language,false,new string[0],null,null,null));});
                    var route=new OriginalHandpayTextScreenRouter(new OriginalHandpayTextScreenRouter.Host {Manager3aa=true,Selected=()=>true,GameMode=()=>false,
                        Alternate230=owner.RouterScreen,TwoLetterLanguage=()=>language=="en"?"en":language.Substring(0,2)});
                    var content=OriginalHandpayBannerText.Create(amount,voucher,false,true,true,v=>money.FormatBytes(v,100,false,0));byte[] fourth=detail?Encoding.ASCII.GetBytes("Handpay_Required"):null;
                    check(route.Display(content.Title,content.Body,content.Amount,fourth));check(route.MessageDisplayed&&route.CurrentScreen==owner.RouterScreen&&displayEvents==1);
                    var labels=owner.Labels.Describe();int count=3+(detail?1:0)+(language!="en"?1:0);check(labels.Length==count&&state.Cursor==count&&state.Layout.Order==9100&&owner.Layout.Centering);
                    check(translations==2+(detail?1:0));
                    string title=translator.Translate("Call_Attendant",language,false,new string[0],null,null,null),body=translator.Translate(voucher?"VOUCHER_PAY":"HAND_PAY",language,false,new string[0],null,null,null);
                    check(labels[0].Text==title&&labels[0].FontSlot==7&&labels[0].Height==63);check(labels[1].Text==body&&labels[1].FontSlot==7&&labels[1].Height==63);
                    check(labels[2].Text==Encoding.UTF8.GetString(content.Amount)&&labels[2].FontSlot==8&&labels[2].Height==115);
                    if(detail)check(labels[3].FontSlot==9&&labels[3].Text==translator.Translate("Handpay_Required",language,false,new string[0],null,null,null));
                    if(language!="en")check(labels[count-1].Text=="Call Attendant "+(voucher?"VOUCHER PAY":"HAND PAY")+" "+Encoding.UTF8.GetString(content.Amount)+" "+(detail?"Handpay Required":"")&&labels[count-1].FontSlot==9);
                    int sum=241+(count-3)*30,top=384-sum/2;
                    for(int i=0;i<count;++i){check(labels[i].Top==top&&labels[i].Left==680-labels[i].Width/2&&labels[i].Bottom==top+labels[i].Height);top+=labels[i].Height;}
                    check(!route.Display(content.Title,content.Body,content.Amount,fourth)&&displayEvents==1&&owner.Labels.Describe().Length==count);
                    var languageFont=state.Fonts[7];owner.RouterScreen.SetLanguage("en");check(owner.FontLanguage=="en"&&state.Fonts[7]==languageFont);
                    bool rejected=false;try{owner.RouterScreen.SetLanguage("zh");}catch(InvalidOperationException){rejected=true;}check(rejected&&owner.FontLanguage=="en"&&state.Fonts[7]==languageFont);
                    owner.Defaults();check(owner.Layout.Lines.Count==count&&state.Labels.Count==count&&state.Playback.Visible); // Native Defaults does not Clear.
                    state.Playback.Paused=true;check(owner.End()&&!state.Playback.Visible&&!state.Playback.Paused&&endEvents==1&&state.Labels.Count==count&&state.Lines.Count==count&&!owner.Labels.Root.activeSelf);
                    var font=state.Fonts[7];state.Playback.ResourcesAvailable=false;check(owner.Clear()&&state.Labels.Count==count&&state.Lines.Count==count);
                    state.Playback.ResourcesAvailable=true;check(owner.Clear()&&state.Labels.Count==0&&state.Lines.Count==0&&owner.Layout.Lines.Count==0&&state.Cursor==count&&state.Fonts[7]==font&&owner.Layout.Centering&&owner.Labels.Root!=null);
                    check(owner.Clear()&&state.Cursor==0); // Repeated clear starts cursor at zero.
                }
                finally{if(owner!=null)owner.Dispose();UnityEngine.Object.DestroyImmediate(root);}
            }
            check(cases==48);
            return checks;
        }
    }
}
