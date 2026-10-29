using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    // Dedicated diagnostic scene. No RouletteGame, finance store or RNG owner.
    public sealed class OriginalVerifyYesNoPcProbe : MonoBehaviour
    {
        [Serializable] private sealed class Report
        { public string engine, backend, mode, inputScope; public int width, height, result, disposedEvents, checks; public bool financialExecution; }
        private OriginalVerifyYesNoPresentation screen;
        private OriginalVideoInputDispatch route;
        private OriginalSpriteInputTarget yes, no;
        private int disposed;
        private bool scripted;
        private static string Arg(string key, string fallback)
        { var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,key);return index>=0&&index+1<args.Length?args[index+1]:fallback; }
        private static string DecodeAscii(byte[] bytes)
        { foreach(byte value in bytes)if(value>127)throw new Exception("Probe ASCII input only");return Encoding.ASCII.GetString(bytes); }
        private IEnumerator Start()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor)
                throw new Exception("PC-only diagnostic");
            int width=int.Parse(Arg("-nativeVerifyWidth","800")),height=int.Parse(Arg("-nativeVerifyHeight","600"));
            string mode=Arg("-nativeVerifyChoice","interactive");scripted=mode!="interactive";
            for(int frame=0;frame<120&&(Screen.width!=width||Screen.height!=height);++frame)yield return null;
            if(Screen.width!=width||Screen.height!=height){Debug.LogError("VERIFY_PC_VIEWPORT_FAILED actual="+Screen.width+"x"+Screen.height);Application.Quit(8);yield break;}
            var font=OriginalBitmapFont.Load("OS366J/arial-bold-12-white");
            var text=new OriginalVerifyTextLayout(0,width,0.6f);
            Func<byte[],short> measure=bytes=>(short)font.MeasureWidth(DecodeAscii(bytes));
            text.AddText(Encoding.ASCII.GetBytes("10.00 cannot be paid from the hopper."),3,font.Height,measure);
            text.AddText(Encoding.ASCII.GetBytes("Please touch an option."),3,font.Height,measure);
            screen=new OriginalVerifyYesNoPresentation(width,height,text,text.Align(5,0,0),"en",value=>value,DecodeAscii,
                Encoding.ASCII.GetBytes("Hand\nPay"),Encoding.ASCII.GetBytes("Continue\nGame Play"),true,true);
            route=new OriginalVideoInputDispatch(node=>node!=null,()=>{
                long micro=(DateTime.UtcNow.Ticks-621355968000000000L)/10;
                return new OriginalVideoInputDispatch.ClockSample{Seconds=unchecked((int)(micro/1000000)),Microseconds=(int)(micro%1000000)};
            },value=>++disposed);
            var root=new OriginalSpriteInputTarget{Flags=1,Rectangle=new OriginalSpriteAlignment.Rectangle{Right=(short)width,Bottom=(short)height}};
            yes=new OriginalSpriteInputTarget{Flags=0x47,Rectangle=screen.Geometry.YesButton,Parent=root};
            no=new OriginalSpriteInputTarget{Flags=0x47,Rectangle=screen.Geometry.NoButton,Parent=root};
            root.Children.Add(yes);root.Children.Add(no);route.Windows.Add(root);screen.AttachNativeInput(route,yes,no,true);
            Debug.Log("VERIFY_PC_READY viewport="+width+"x"+height+" mode="+mode);
            if(!scripted)yield break;
            string output=Path.GetFullPath(Arg("-nativeVerifyProbeOutput",""));
            if(mode!="yes"&&mode!="no")throw new Exception("Probe choice must be yes/no/interactive");
            if(Directory.Exists(output))throw new Exception("Fresh probe output required");Directory.CreateDirectory(output);
            yield return Capture(Path.Combine(output,"idle.png"));
            var target=mode=="yes"?yes:no;
            Send(0x200001,(short)(target.Rectangle.Left+5),(short)(target.Rectangle.Top+5));
            int result;
            if(!screen.Input.TryGetDisplayResult(out result)||result!=(mode=="yes"?1:0)||route.TouchCaptured(1)!=target)
                throw new Exception("PC probe routed down/result/capture failed");
            yield return Capture(Path.Combine(output,"pressed.png"));
            Send(0x200002,0,0);Send(0x200004,0,0);
            if(route.TouchCaptured(1)!=null||screen.YesState.Pressed||screen.NoState.Pressed)
                throw new Exception("PC probe outside release/capture cleanup failed");
            yield return Capture(Path.Combine(output,"released.png"));
            File.WriteAllText(Path.Combine(output,"report.json"),JsonUtility.ToJson(new Report{engine=Application.unityVersion,
                backend=SystemInfo.graphicsDeviceType.ToString(),mode=mode,width=width,height=height,result=result,disposedEvents=disposed,checks=5,
                inputScope="Actual Player native-format event fixture; Unity mouse adapter compiled but OS input not exercised. Explicit nodes/ASCII/amount/newlines/managed clock.",financialExecution=false},true));
            Debug.Log("VERIFY_PC_PROBE_PASSED choice="+mode+" result="+result+" disposed="+disposed+" output="+output);Application.Quit(0);
        }
        private IEnumerator Capture(string path)
        {
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(path);
            for(int frame=0;frame<120&&!File.Exists(path);++frame)yield return null;
            if(!File.Exists(path))throw new Exception("Player screenshot not produced");
        }
        private void Send(uint type,short x,short y)
        { route.InputQueue.Enqueue(new OriginalVideoInputEvent{Type=type,Size=0x1c,Display=1,Point=new OriginalSpriteInputTarget.Point(x,y)});route.ProcessOne(); }
        private void Update()
        {
            if(route==null||scripted)return;
            short x=unchecked((short)(int)Input.mousePosition.x),y=unchecked((short)(Screen.height-1-(int)Input.mousePosition.y));
            if(Input.GetMouseButtonDown(0))Send(0x200001,x,y);
            else if(Input.GetMouseButtonUp(0))Send(0x200004,x,y);
            else if(Input.GetMouseButton(0))Send(0x200002,x,y);
        }
        private void OnGUI(){if(screen!=null)screen.Draw();}
    }
}
