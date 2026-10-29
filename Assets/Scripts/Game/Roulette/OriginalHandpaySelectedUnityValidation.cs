using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectedUnityValidation
    {
        [Serializable] public sealed class Capture{public string file,language,choice,state;public int width,height;}
        [Serializable] public sealed class Report{public string engine,backend;public int cases,checks;public Capture[] captures;public bool financialExecution;}
        public static void Validate(string output)
        {
            if(Directory.Exists(output))throw new Exception("Fresh Unity input output required");Directory.CreateDirectory(output);
            int checks=0,cases=0;Action<bool> check=b=>{if(!b)throw new Exception("HandPaySelected Unity check="+checks);++checks;};check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));var locale=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");var captures=new List<Capture>();
            foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(1360,768)})foreach(string language in new[]{"en","zh"})foreach(string choice in new[]{"yes","no","exit","disabledno"})
            {
                ++cases;int disposed=0;bool tilt=false;var events=new List<byte[]>();var go=new GameObject("NativeSelectedUnityValidation");var ui=go.AddComponent<OriginalHandpaySelectedUnityScreen>();
                var route=new OriginalVideoInputDispatch(ui.Owns,()=>new OriginalVideoInputDispatch.ClockSample{Seconds=1,Microseconds=2},e=>++disposed);var other=new OriginalSpriteInputTarget();route.Windows.Add(other);
                Func<string,int,byte[],short> measure=(lang,slot,bytes)=>{var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(lang,slot,s=>s.ToLowerInvariant()));var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[bytes.Length];int n=decoder.GetChars(bytes,0,bytes.Length,chars,0,false);return unchecked((short)font.MeasureWidth(new string(chars,0,n)));};
                ui.Configure(size.x,size.y,route,s=>s.ToLowerInvariant(),Encoding.UTF8.GetString,measure);OriginalHandpaySelected.Screen current=null;
                byte[] initial=new byte[135];initial[36]=0xe3;initial[37]=3;initial[99]=2;initial[93]=127;string path=Path.Combine(output,"case_"+cases+".bin");
                using(var owner=OriginalHandpaySelectionPersistence.OpenFile(path,initial,s=>{}))
                {
                    var h=new OriginalHandpaySelected.Host{TiltPending=code=>{check(code==0x6020);return tilt;},Emit=(b,f)=>{check(f==0);events.Add(b);tilt=b[1]==1;},CreateScreen=()=>ui.Binding,ReadManagerScreen2C4=()=>current,AssignManagerScreen2C4=s=>current=s,JurisdictionFlag448=()=>true,MachineFlag188=()=>choice=="disabledno",SelectedLanguage=()=>language,
                        FormatCredit=(n,b,d,c)=>{check(n==995&&b&&d==100&&c);locale.FormatText(n,(uint)d,b,0);},Translate=(key,args,flag)=>Encoding.UTF8.GetBytes(translator.Translate(key,language,flag,args,n=>locale.FormatText(n,100,false,0),null,null))};
                    var session=OriginalHandpaySelected.Begin(owner,h);check(owner.State.InProgress==1&&owner.State.ResidualSubstate==3&&!ui.Visible&&ui.Presentation!=null&&ui.Amount==995&&current==ui.Binding&&route.Windows.Count==1);owner.ValidateStorage();
                    ui.BeginDisplay();check(ui.Visible&&route.Windows.Count==2&&ui.Presentation.NativeOrder==9125);int result;check(!ui.TryGetResult(out result));
                    Action<string> capture=state=>{
                        string file="case_"+cases+"_"+state+".png";var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();var saved=RenderTexture.active;var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);
                        try{if(ui.Presentation!=null)ui.Presentation.DrawTo(target);else{RenderTexture.active=target;GL.Clear(true,true,Color.clear);}RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,file),image.EncodeToPNG());}
                        finally{RenderTexture.active=saved;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);}captures.Add(new Capture{file=file,language=language,choice=choice,state=state,width=size.x,height=size.y});
                    };
                    capture("idle");
                    if(choice=="exit")ui.Presentation.Input.SelectExit();else{var button=choice=="yes"?ui.Yes:ui.No;ui.DispatchPointer(0x200001,(short)(button.Rectangle.Left+5),(short)(button.Rectangle.Top+5));check(route.TouchCaptured(1)==button);}
                    check(ui.TryGetResult(out result)&&result==(choice=="yes"?1:choice=="exit"?-1:0));capture("pressed");
                    if(choice!="exit"){ui.DispatchPointer(0x200002,0,0);ui.DispatchPointer(0x200004,0,0);check(route.TouchCaptured(1)==null&&!ui.Presentation.YesState.Pressed&&!ui.Presentation.NoState.Pressed&&disposed>0);}
                    bool selected=session.Complete(result);check(selected==(choice!="no")&&owner.State.InProgress==0&&owner.State.ResidualSubstate==3&&owner.State.HandpaySelectedByte==127&&ui.Deleted&&!ui.Visible&&ui.Presentation==null&&current==null&&route.Windows.Count==1&&ReferenceEquals(route.Windows[0],other));
                    check(events.Count==2&&events[0].Length==185&&events[0][1]==1&&events[1][1]==2&&BitConverter.ToInt64(events[0],28)==995&&BitConverter.ToInt64(events[1],28)==995&&!tilt);owner.ValidateStorage();capture("closed");
                }
                using(var cold=OriginalHandpaySelectionPersistence.OpenFile(path,new byte[135],s=>{})){check(cold.State.InProgress==0&&cold.State.ResidualSubstate==3&&cold.State.HandpaySelectedByte==127);cold.ValidateStorage();}
                UnityEngine.Object.DestroyImmediate(go);
            }
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(new Report{engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),cases=cases,checks=checks,captures=captures.ToArray(),financialExecution=false},true));Debug.Log("HANDPAY_SELECTED_UNITY_VALIDATED cases="+cases+" checks="+checks);
        }
    }
}
