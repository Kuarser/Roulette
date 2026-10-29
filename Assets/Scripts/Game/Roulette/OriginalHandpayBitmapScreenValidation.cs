using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalHandpayBitmapScreenValidation
    {
        [Serializable] public sealed class Line {public string text;public int slot,height,spacing,x,y,horizontal,vertical;}
        [Serializable] public sealed class Capture {public string file,language,fontLanguage,state;public int width,height;public bool translated,voucher;public Line[] lines;}
        [Serializable] public sealed class Report {public int checks,cases;public string engine,backend;public bool financialExecution;public Capture[] captures;}
        public static int Validate(string output)
        {
            int checks=0,cases=0;Action<bool> check=b=>{if(!b)throw new Exception("Ordinary HandPay screen binding check "+checks);++checks;};
            check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);check(!Directory.Exists(output));Directory.CreateDirectory(output);
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));var money=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");var captures=new List<Capture>();
            foreach(var size in new[]{new Vector2Int(1360,768),new Vector2Int(800,600)})foreach(string selected in new[]{"en","zh","es_CL","fr_CA"})foreach(bool translated in new[]{false,true})foreach(bool voucher in new[]{false,true})
            {
                ++cases;var binding=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),path=>null);int displays=0,ends=0,lowers=0,translations=0,releases=0;
                Func<int,byte[],short> measure=(slot,bytes)=>{var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int n=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);return unchecked((short)binding.LoadQualifiedAgkFont(slot).MeasureWidth(new string(chars,0,n)));};
                var owner=new OriginalHandpayBitmapScreen(size.x,size.y,binding,Encoding.UTF8.GetString,measure,key=>{++translations;return Encoding.UTF8.GetBytes(translator.Translate(Encoding.UTF8.GetString(key),selected,false,new string[0],null,null,null));},
                    v=>{++lowers;return v>=65&&v<=90?v+32:v;},(slot,font)=>{check(font is OriginalBitmapFont);++releases;},(raw,p)=>{check(raw.Length==44&&raw[0]==1&&raw[2]==44&&p==0);uint code=(uint)(raw[24]|raw[25]<<8|raw[26]<<16|raw[27]<<24);if(code==0x18100)++displays;else{check(code==0x18200);++ends;}});
                check(owner.FontLanguage=="en"&&lowers==0&&binding.FileName(1)=="/agk/fonts/arial-bold-16-white.fnt"&&owner.Layout.Lines.Count==0&&!owner.Layout.Centering);
                var route=new OriginalHandpayTextScreenRouter(new OriginalHandpayTextScreenRouter.Host {Manager3aa=cases%2==0,Selected=()=>false,Ordinary=owner.RouterScreen,Translate=()=>translated,SelectedLanguage=()=>selected});
                var content=OriginalHandpayBannerText.Create(995,voucher,translated,cases%2==0,false,v=>money.FormatBytes(v,100,false,0));
                check(!content.AlternateScreen&&route.Display(content.Title,content.Body,content.Amount,Encoding.ASCII.GetBytes("IgnoredFourth")));
                check(route.CurrentScreen==owner.RouterScreen&&route.MessageDisplayed&&displays==1&&ends==0&&owner.Order==9100&&owner.DisplayOwner.Lifecycle.Visible);
                int count=translated?3:2;check(owner.Layout.Lines.Count==count&&owner.FontState.Cursor==count&&owner.DisplayOwner.Presentation.LabelCount==count);
                check(owner.FontLanguage==(translated&&selected=="zh"?"zh":"en")&&translations==(translated?2:0)&&lowers==(translated&&selected!="en"?selected.Length*13:0)&&releases==0);
                for(int i=0;i<count;++i){var line=owner.Layout.Lines[i];check(line.FontSlot==(i==2?3:1)&&line.Height==((OriginalBitmapFont)owner.FontState.Fonts[line.FontSlot]).Height&&line.Spacing==line.Height/3);}
                string title=translated?translator.Translate("Call_Attendant",selected,false,new string[0],null,null,null):"Call Attendant";
                string body=translated?translator.Translate(voucher?"VOUCHER_PAY":"HAND_PAY",selected,false,new string[0],null,null,null):(voucher?"VOUCHER PAY ":"HAND PAY ")+"RM9.95";
                check(Encoding.UTF8.GetString(owner.Layout.Lines[0].Text)==title&&Encoding.UTF8.GetString(owner.Layout.Lines[1].Text)==body);
                if(translated)check(Encoding.UTF8.GetString(owner.Layout.Lines[2].Text)=="RM9.95");
                var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();
                Action<string> save=state=>
                {
                    string file="case_"+cases+"_"+state+".png";owner.DisplayOwner.Presentation.DrawTo(target);var old=RenderTexture.active;var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);
                    try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,file),image.EncodeToPNG());}
                    finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(image);}
                    var rows=new List<Line>();foreach(var line in owner.Layout.Lines)rows.Add(new Line {text=Encoding.UTF8.GetString(line.Text),slot=line.FontSlot,height=line.Height,spacing=line.Spacing,x=line.AnchorX,y=line.AnchorY,horizontal=line.HorizontalAlignment,vertical=line.VerticalAlignment});
                    captures.Add(new Capture {file=file,language=selected,fontLanguage=owner.FontLanguage,state=state,width=size.x,height=size.y,translated=translated,voucher=voucher,lines=rows.ToArray()});
                };
                try
                {
                    save("initial");var presentation=owner.DisplayOwner.Presentation;var font=owner.FontState.Fonts[1];owner.Defaults();check(owner.Layout.Lines.Count==count&&owner.FontState.Fonts[1]==font&&owner.DisplayOwner.Presentation==presentation);
                    check(!route.Display(content.Title,content.Body,content.Amount,null)&&displays==1);
                    if(cases==1){check(owner.DisplayOwner.Lifecycle.Hide()&&!owner.DisplayOwner.Lifecycle.Visible&&ends==1);save("hidden");check(owner.DisplayOwner.Lifecycle.Show()&&displays==2);save("shown");}
                    owner.Layout.Centering=true;check(owner.Clear()&&owner.Layout.Lines.Count==0&&!owner.Layout.Centering&&owner.FontState.Lines.Count==0&&owner.FontState.Cursor==count&&owner.FontState.Fonts[1]==font);
                    check(owner.DisplayOwner.Presentation==presentation&&presentation.LabelCount==0&&owner.DisplayOwner.Lifecycle.RootPresent);
                    if(cases==1)save("cleared");
                    check(owner.End()&&owner.DisplayOwner.Presentation==null&&!owner.DisplayOwner.Lifecycle.RootPresent&&!owner.DisplayOwner.Lifecycle.Visible&&ends==(cases==1?2:1)&&owner.FontState.Fonts[1]==font);
                }
                finally{target.Release();UnityEngine.Object.DestroyImmediate(target);}
            }
            check(cases==32&&captures.Count==35);
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(new Report {checks=checks,cases=cases,engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),captures=captures.ToArray()},true)+"\n");return checks;
        }
    }
}
