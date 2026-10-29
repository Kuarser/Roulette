using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutResidualScreensValidation
    {
        [Serializable] public sealed class OptionVector{public string initial,payload,durable;public bool pending,selected,mutation;public string[] trace;}
        [Serializable] public sealed class FailedVector{public long amount;public int disabled;public bool translate,result;public string language,currency;public int[] targets;public string[] trace;}
        [Serializable] public sealed class Vectors{public OptionVector[] options;public FailedVector[] failures;}
        private sealed class Device:IOriginalNvDevice
        {
            public byte[] Bytes=new byte[0];public Action<byte[]> Commit;
            public int Size{get{return Bytes.Length;}}public byte[] Read(int o,int n){var b=new byte[n];Array.Copy(Bytes,o,b,0,n);return b;}
            public void Write(int o,byte[] b){Array.Copy(b,0,Bytes,o,b.Length);if(o==0&&b.Length==1&&b[0]==1&&Commit!=null)Commit(Read(20,BitConverter.ToInt32(Bytes,4)));}
            public void SetSize(int n){Array.Resize(ref Bytes,n);}
        }
        public static int Validate(string root)
        {
            var vectors=JsonUtility.FromJson<Vectors>(File.ReadAllText(Path.Combine(root,"vectors_v1.json")));int checks=0,index=0;
            Action<bool> check=b=>{if(!b)throw new Exception("Residual screens case="+index+" check="+checks);++checks;};
            foreach(var v in vectors.options)
            {
                var d=new Device();var t=new List<string>();
                using(var owner=new OriginalHandpaySelectionPersistence(d,Convert.FromBase64String(v.initial),s=>{}))
                {
                    d.Commit=b=>t.Add("save:"+Convert.ToBase64String(b));var reference=owner.State;
                    var h=new OriginalCreditOutResidualScreens.OptionHost{
                        PendingScreen2C4=()=>{t.Add("pending:"+(v.pending?1:0));return v.pending;},GameOperation=o=>{t.Add("game:"+o);if(o==0x208&&v.mutation)owner.CurrentPayload.CashOutAmount=-13;},MachineOperation=(o,b)=>t.Add("machine:"+o+":"+(b?1:0)),
                        HandPaySelected=()=>{t.Add("selected:"+(v.selected?1:0));if(v.mutation)owner.CurrentPayload.ResidualSubstateValue=v.selected?4:9;return v.selected;},
                        ProcessResidualGamble=()=>{t.Add("residual");if(v.mutation){owner.CurrentPayload.CashOutAmount=-11;owner.CurrentPayload.ResidualSubstateValue=0;owner.SaveCurrentPayload();}},
                        ReadManagerEvent18=()=>{t.Add("field18");return 0xfedcba98;},Emit=(b,f)=>{check(f==0);t.Add("event:"+Convert.ToBase64String(b));}};
                    OriginalCreditOutResidualScreens.Option(owner,h);Trace(t,v.trace,index);++checks;check(Convert.ToBase64String(owner.PayloadBytes)==v.payload);
                    if(v.payload==v.durable)owner.ValidateStorage();else{bool fault=false;try{owner.ValidateStorage();}catch(OriginalNvFault){fault=true;}check(fault);}
                    owner.Reload();owner.ValidateStorage();check(Convert.ToBase64String(owner.PayloadBytes)==v.durable&&ReferenceEquals(reference,owner.State));
                }
                using(var cold=new OriginalHandpaySelectionPersistence(d,new byte[135],s=>{})){check(Convert.ToBase64String(cold.PayloadBytes)==v.durable);cold.ValidateStorage();}++index;
            }
            foreach(var v in vectors.failures)
            {
                var t=new List<string>();int at=0;var screens=new OriginalCreditOutResidualScreens.Screen[5];
                for(int i=0;i<5;++i){int id=i;string tag="s"+i+":";screens[i]=new OriginalCreditOutResidualScreens.Screen{
                    SetPosition=(x,y)=>{check(x==0.15f&&y==0.4f&&id==0);t.Add(tag+"position");},SetSize=(w,h)=>{check(w==0.7f&&h==0.2f&&id==0);t.Add(tag+"size");},SetOrder=n=>t.Add(tag+"order:"+n),SetLanguage=s=>t.Add(tag+"lang:"+s),
                    AddText=(b,slot)=>t.Add(tag+"text:"+Convert.ToBase64String(b)+":"+slot),AddTranslatedText=(key,args,slot)=>t.Add(tag+"translate:"+key+":"+string.Join(",",args)+":"+slot),Init=()=>t.Add(tag+"init"),Display=()=>t.Add(tag+"display")};}
                var h=new OriginalCreditOutResidualScreens.FailedHost{
                    FormatCredit=(n,b,div,c)=>{t.Add("format:"+n+":"+(b?1:0)+":"+div+":"+(c?1:0));return Convert.FromBase64String(v.currency);},CreateInfoScreen=()=>{t.Add("create:0");return screens[0];},
                    ReadManagerScreen228=()=>{int id=v.targets[at++];t.Add("read228:"+id);return screens[id];},JurisdictionFlag448=()=>{t.Add("jur:"+(v.translate?1:0));return v.translate;},SelectedLanguage=()=>{t.Add("language:"+v.language);return v.language;},
                    SetInfoTimerHandler=s=>{check(ReferenceEquals(s,screens[0]));t.Add("handler:0");},PublishInfoScreen=s=>{check(ReferenceEquals(s,screens[0]));t.Add("publish:0");},ArmInfoTrigger=(s,n)=>{check(ReferenceEquals(s,screens[0]));t.Add("arm:0:"+n);}};
                check(OriginalCreditOutResidualScreens.Failed(new OriginalCreditOutCollect.Runtime{Field23C=(byte)v.disabled},v.amount,h)==v.result);Trace(t,v.trace,index);++checks;++index;
            }
            // Native early gates admit empty unused binding sets, with no side effect.
            using(var p=new OriginalHandpaySelectionPersistence(new Device(),new byte[135],s=>{})){OriginalCreditOutResidualScreens.Option(p,new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>true});++checks;}
            check(!OriginalCreditOutResidualScreens.Failed(new OriginalCreditOutCollect.Runtime{Field23C=1},37,new OriginalCreditOutResidualScreens.FailedHost()));
            checks+=ValidateInventory();checks+=ValidateBitmap();checks+=ValidateFailures(vectors);
            checks+=OriginalCreditOutStateFlowValidation.ValidateResidualScreenBinding();
            Debug.Log("RESIDUAL_SCREENS_VALIDATED optionCases="+vectors.options.Length+" failedCases="+vectors.failures.Length+" checks="+checks);return checks;
        }
        private static void Trace(List<string> actual,string[] expected,int index)
        {if(string.Join("|",actual)!=string.Join("|",expected))throw new Exception("Residual screens trace case="+index+" actual="+string.Join("|",actual)+" expected="+string.Join("|",expected));}
        private static int ValidateInventory()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("InfoText inventory "+checks);++checks;};
            foreach(int target in new[]{-1,0,1,2})foreach(bool duplicate in new[]{false,true})
            {
                var t=new List<string>();Func<uint> callback=null;var manager=new OriginalInfoTextManager(()=>t.Add("flush"),(s,n)=>t.Add("arm:"+n),(s,h)=>callback=h);
                var screens=new OriginalCreditOutResidualScreens.Screen[4];for(int i=0;i<4;++i){int id=i;screens[i]=new OriginalCreditOutResidualScreens.Screen{Clear=()=>t.Add("clear:"+id),End=()=>t.Add("end:"+id),Delete=()=>t.Add("delete:"+id)};}
                for(int i=0;i<3;++i)manager.Publish(screens[i]);if(duplicate)manager.Publish(screens[1]);var kill=screens[target<0?3:target];manager.SetTimerHandler(kill);check(callback()!=0&&string.Join("|",t)=="clear:"+(target<0?3:target)+"|end:"+(target<0?3:target)+"|flush|arm:4294967295|delete:"+(target<0?3:target));
                check(manager.Screens.Count==3+(duplicate?1:0)-(target<0?0:1));if(duplicate&&target==1)check(ReferenceEquals(manager.Screens[manager.Screens.Count-1],screens[1]));
            }
            // Failed Clear does not end, disarm, erase or delete.
            {
                int next=0;var s=new OriginalCreditOutResidualScreens.Screen{Clear=()=>{throw new InvalidOperationException();},End=()=>++next,Delete=()=>++next};var manager=new OriginalInfoTextManager(()=>++next,(x,n)=>++next,(x,h)=>{});manager.Publish(s);bool fault=false;try{manager.Kill(s);}catch(InvalidOperationException){fault=true;}check(fault&&next==0&&manager.Screens.Count==1);
            }
            return checks;
        }
        private static int ValidateBitmap()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("Residual bitmap "+checks);++checks;};
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));var money=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
            foreach(var size in new[]{new Vector2Int(1360,768),new Vector2Int(800,600),new Vector2Int(540,960)})foreach(string language in new[]{"en","zh","fr_CA"})foreach(bool translated in new[]{false,true})
            {
                int shown=0,ended=0;var owners=new List<OriginalResidualFailedBitmapScreen>();
                Func<OriginalResidualFailedBitmapScreen> create=()=>{
                    var fonts=new OriginalCashoutFontBinding(new OriginalCashoutFontBinding.SharedSizes(),path=>null);
                    Func<int,byte[],short> measure=(slot,b)=>{var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[b.Length];int n=decoder.GetChars(b,0,b.Length,chars,0,false);return unchecked((short)fonts.LoadQualifiedAgkFont(slot).MeasureWidth(new string(chars,0,n)));};
                    var owner=new OriginalResidualFailedBitmapScreen(size.x,size.y,fonts,Encoding.UTF8.GetString,measure,(key,args)=>Encoding.UTF8.GetBytes(translator.Translate(key,language,false,args,n=>Encoding.UTF8.GetString(money.FormatBytes(n,100,false,0)),null,null)),n=>n>=65&&n<=90?n+32:n,(slot,font)=>{},(b,f)=>{check(f==0);uint code=BitConverter.ToUInt32(b,24);if(code==0x18100)++shown;else if(code==0x18200)++ended;else throw new Exception("Unexpected screen event");});owners.Add(owner);return owner;};
                var cached=create();Func<uint> handler=null;uint armed=0;int flushed=0;var manager=new OriginalInfoTextManager(()=>++flushed,(s,n)=>armed=n,(s,h)=>handler=h);
                var h=new OriginalCreditOutResidualScreens.FailedHost{FormatCredit=(n,b,d,c)=>money.FormatBytes(n,(uint)d,b,0),CreateInfoScreen=()=>create().Screen,ReadManagerScreen228=()=>cached.Screen,JurisdictionFlag448=()=>translated,SelectedLanguage=()=>language,SetInfoTimerHandler=manager.SetTimerHandler,PublishInfoScreen=manager.Publish,ArmInfoTrigger=manager.Arm};
                check(OriginalCreditOutResidualScreens.Failed(new OriginalCreditOutCollect.Runtime(),37,h));var info=owners[1];var bitmap=info.Bitmap;
                check(owners.Count==2&&manager.Screens.Count==1&&ReferenceEquals(manager.Screens[0],info.Screen)&&bitmap.Order==9100&&shown==1&&ended==0&&armed==2500);
                check(bitmap.Layout.NormalizedX==0.15f&&bitmap.Layout.NormalizedY==0.4f&&bitmap.Layout.NormalizedWidth==0.7f);
                check(bitmap.DisplayOwner.Lifecycle.Visible&&bitmap.DisplayOwner.Lifecycle.RootPresent&&bitmap.DisplayOwner.Presentation.LabelCount==bitmap.Layout.Lines.Count);
                check(translated?(bitmap.Layout.Lines.Count==0&&cached.Bitmap.Layout.Lines.Count>=3):(bitmap.Layout.Lines.Count>=3&&cached.Bitmap.Layout.Lines.Count==0));
                check(handler()==uint.MaxValue&&info.Deleted&&armed==uint.MaxValue&&flushed==1&&ended==1&&manager.Screens.Count==0);
                check(!bitmap.DisplayOwner.Lifecycle.Visible&&!bitmap.DisplayOwner.Lifecycle.RootPresent&&bitmap.DisplayOwner.Presentation==null&&bitmap.Layout.Lines.Count==0);
                cached.Screen.Clear();cached.Screen.End();cached.Screen.Delete();check(cached.Deleted);
            }
            return checks;
        }
        private static int ValidateFailures(Vectors vectors)
        {
            int checks=0;var d=new Device();using(var owner=new OriginalHandpaySelectionPersistence(d,Convert.FromBase64String(vectors.options[0].initial),s=>{}))
            {
                int saves=0,after=0;d.Commit=b=>++saves;var h=new OriginalCreditOutResidualScreens.OptionHost{PendingScreen2C4=()=>false,GameOperation=o=>{},MachineOperation=(o,b)=>{},HandPaySelected=()=>{throw new InvalidOperationException("selection failure");},ProcessResidualGamble=()=>++after,ReadManagerEvent18=()=>0,Emit=(b,f)=>++after};
                bool fault=false;try{OriginalCreditOutResidualScreens.Option(owner,h);}catch(InvalidOperationException){fault=true;}if(!fault||saves!=0||after!=0)throw new Exception("Residual selection failure fabricated state");++checks;
            }
            return checks;
        }
    }
}
