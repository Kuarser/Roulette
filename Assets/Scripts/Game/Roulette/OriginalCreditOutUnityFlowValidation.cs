using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteDeluxe
{
    public static class OriginalCreditOutUnityFlowValidation
    {
        [Serializable] public sealed class Capture{public string file,language,choice,state;public int width,height;}
        [Serializable] public sealed class Report{public string engine,backend;public int cases,checks;public Capture[] captures;public bool financialExecution;}
        public static void Validate(string output)
        {
            if(Directory.Exists(output))throw new Exception("Fresh Unity flow output required");Directory.CreateDirectory(output);int checks=0,cases=0;var captures=new List<Capture>();
            Action<bool> check=b=>{if(!b)throw new Exception("Unity flow check="+checks);++checks;};check(Application.unityVersion=="2020.3.17f1"&&SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Direct3D11);
            var translator=OriginalLanguageTranslator.Load(name=>File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath,"Original/definitions/BAL_OS366J/LanguageTranslation/"+name)));var locale=OriginalMonetaryLocales.LoadPlatform("ms_MY.utf8");
            foreach(var size in new[]{new Vector2Int(800,600),new Vector2Int(1360,768)})foreach(string language in new[]{"en","zh"})foreach(string choice in new[]{"yes","no","exit","disabledno","interrupt","mismatch"})
            {
                ++cases;var go=new GameObject("NativeCreditOutUnityFlowValidation");var ui=go.AddComponent<OriginalHandpaySelectedUnityScreen>();int disposed=0;bool tilt=false;OriginalHandpaySelected.Screen current=null;
                var route=new OriginalVideoInputDispatch(ui.Owns,()=>new OriginalVideoInputDispatch.ClockSample{Seconds=1,Microseconds=2},e=>++disposed);var unrelated=new OriginalSpriteInputTarget();route.Windows.Add(unrelated);
                Func<string,int,byte[],short> measure=(lang,slot,b)=>{var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(lang,slot,s=>s.ToLowerInvariant()));var decoder=new UTF8Encoding(false,true).GetDecoder();var chars=new char[b.Length];int n=decoder.GetChars(b,0,b.Length,chars,0,false);return unchecked((short)font.MeasureWidth(new string(chars,0,n)));};
                ui.Configure(size.x,size.y,route,s=>s.ToLowerInvariant(),Encoding.UTF8.GetString,measure);
                var body=new OriginalHandpaySelected.Host{TiltPending=code=>{check(code==0x6020);return tilt;},Emit=(b,f)=>{check(f==0&&b.Length==185&&BitConverter.ToInt64(b,28)==995);tilt=b[1]==1;},CreateScreen=()=>ui.Binding,ReadManagerScreen2C4=()=>current,AssignManagerScreen2C4=s=>current=s,JurisdictionFlag448=()=>true,MachineFlag188=()=>choice=="disabledno",SelectedLanguage=()=>language,
                    FormatCredit=(n,b,d,c)=>{check(n==995&&b&&d==100&&c);locale.FormatText(n,(uint)d,b,0);},Translate=(key,args,flag)=>Encoding.UTF8.GetBytes(translator.Translate(key,language,flag,args,n=>locale.FormatText(n,100,false,0),null,null))};
                Action<string> capture=state=>{
                    string file="case_"+cases+"_"+state+".png";var target=new RenderTexture(size.x,size.y,0,RenderTextureFormat.ARGB32);target.Create();var old=RenderTexture.active;var image=new Texture2D(size.x,size.y,TextureFormat.RGBA32,false);
                    try{if(ui.Presentation!=null)ui.Presentation.DrawTo(target);else{RenderTexture.active=target;GL.Clear(true,true,Color.clear);}RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,file),image.EncodeToPNG());}
                    finally{RenderTexture.active=old;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);}captures.Add(new Capture{file=file,language=language,choice=choice,state=state,width=size.x,height=size.y});
                };
                try
                {
                    checks+=OriginalCreditOutStateFlowValidation.ValidateHandpayUnityFlow(body,ui,choice,capture,Path.Combine(output,"case_"+cases));
                    if(choice!="interrupt"&&choice!="mismatch")check(!tilt&&current==null&&route.Windows.Count==1&&route.Windows[0]==unrelated);
                    else check(tilt&&current==ui.Binding&&ui.Presentation!=null); // Caller teardown only; no fabricated native cleanup.
                }
                finally{ui.Delete();UnityEngine.Object.DestroyImmediate(go);}
            }
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(new Report{engine=Application.unityVersion,backend=SystemInfo.graphicsDeviceType.ToString(),cases=cases,checks=checks,captures=captures.ToArray(),financialExecution=false},true));Debug.Log("CREDITOUT_UNITY_FLOW_VALIDATED cases="+cases+" checks="+checks);
        }
    }
}
