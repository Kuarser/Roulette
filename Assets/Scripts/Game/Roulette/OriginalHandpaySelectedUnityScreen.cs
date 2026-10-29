using System;
using System.Collections;
using UnityEngine;
namespace RouletteDeluxe
{
    // Actual Unity render/input adapter for the recovered081f4130 body. The
    // caller owns shared Video route, fonts/decoder and NV/financial operations.
    public sealed class OriginalHandpaySelectedUnityScreen:MonoBehaviour
    {
        public OriginalHandpaySelected.Screen Binding{get;private set;}
        public OriginalVerifyYesNoPresentation Presentation{get;private set;}
        public OriginalSpriteInputTarget Root{get;private set;}
        public OriginalSpriteInputTarget Yes{get;private set;}
        public OriginalSpriteInputTarget No{get;private set;}
        public bool Visible{get;private set;}
        public bool Deleted{get;private set;}
        public long Amount{get;private set;}
        public string Language{get;private set;}="en";
        private OriginalVideoInputDispatch route;
        private OriginalVerifyTextLayout layout;
        private byte[] yesText,noText;
        private Func<string,string> lowercase;
        private Func<byte[],string> decode;
        private Func<string,int,byte[],short> measure;
        private int width,height,order;
        private Vector3 previousMouse;
        public bool Owns(OriginalSpriteInputTarget target)
        {return target!=null&&!Deleted&&(ReferenceEquals(target,Root)||ReferenceEquals(target,Yes)||ReferenceEquals(target,No));}
        public void Configure(int width,int height,OriginalVideoInputDispatch route,Func<string,string> lowercase,
            Func<byte[],string> decode,Func<string,int,byte[],short> measure)
        {
            if(Binding!=null)throw new InvalidOperationException("One native screen per Unity owner");
            if(route==null||lowercase==null||decode==null||measure==null)throw new ArgumentNullException();
            this.width=width;this.height=height;this.route=route;this.lowercase=lowercase;this.decode=decode;this.measure=measure;
            layout=new OriginalVerifyTextLayout(0,width,0.6f);
            Binding=new OriginalHandpaySelected.Screen{
                SetLanguage=value=>Language=value,AddText=(b,slot)=>{
                    var font=OriginalBitmapFont.Load(OriginalVerifyFontSelection.TextResource(Language,slot,lowercase));
                    if(font==null)throw new InvalidOperationException("Original Verify font missing");layout.AddText(b,slot,font.Height,bytes=>measure(Language,slot,bytes));
                },SetYesText=b=>yesText=OriginalVerifyButtonText.Copy(b),SetNoText=b=>noText=OriginalVerifyButtonText.Copy(b),
                SetAmount=n=>Amount=n,SetOrder=n=>order=n,Init=Init,DisplayResult=ReadResult,Delete=Delete
            };
        }
        private void Init()
        {
            if(Deleted||Presentation!=null)throw new InvalidOperationException("Native screen Init admission");
            Presentation=new OriginalVerifyYesNoPresentation(width,height,layout,layout.Align(5,0,0),Language,lowercase,decode,yesText,noText,true,true,order);
            Root=new OriginalSpriteInputTarget{Flags=1,Rectangle=new OriginalSpriteAlignment.Rectangle{Right=(short)width,Bottom=(short)height}};
            Yes=new OriginalSpriteInputTarget{Flags=0x47,Rectangle=Presentation.Geometry.YesButton,Parent=Root};No=new OriginalSpriteInputTarget{Flags=0x47,Rectangle=Presentation.Geometry.NoButton,Parent=Root};
            Root.Children.Add(Yes);Root.Children.Add(No);
        }
        public void BeginDisplay()
        {
            if(Deleted||Presentation==null||Visible)throw new InvalidOperationException("Prepared native screen required");
            route.Windows.Add(Root);Presentation.AttachNativeInput(route,Yes,No,true);Visible=true;previousMouse=Input.mousePosition;
        }
        public bool TryGetResult(out int result)
        {result=0;return Visible&&Presentation!=null&&Presentation.Input.TryGetDisplayResult(out result);}
        private int ReadResult(){int result;if(!TryGetResult(out result))throw new InvalidOperationException("Unity input must resume native body after actual result");return result;}
        public IEnumerator Display(Action<int> returned)
        {
            if(returned==null)throw new ArgumentNullException(nameof(returned));BeginDisplay();int result;
            while(!TryGetResult(out result)){if(Deleted)throw new InvalidOperationException("Native screen deleted while awaiting input");yield return null;}
            returned(result);
        }
        // Native-format pointer injection is also usable by the genuine device
        // receiver. The PC mouse bridge below uses this same route, not GUI.Button.
        public void DispatchPointer(uint type,short x,short y)
        {
            if(!Visible||Deleted)throw new InvalidOperationException("Displayed screen required for input");
            route.InputQueue.Enqueue(new OriginalVideoInputEvent{Type=type,Size=0x1c,Display=1,Point=new OriginalSpriteInputTarget.Point(x,y)});route.ProcessOne();
        }
        private void Update()
        {
            if(!Visible||Deleted)return;var point=Input.mousePosition;
            short x=unchecked((short)(int)point.x),y=unchecked((short)(height-(int)point.y));
            if(Input.GetMouseButtonDown(0))DispatchPointer(0x200001,x,y);
            else if(Input.GetMouseButtonUp(0))DispatchPointer(0x200004,x,y);
            else if(Input.GetMouseButton(0)&&point!=previousMouse)DispatchPointer(0x200002,x,y);
            previousMouse=point;
        }
        public void Delete()
        {
            if(Deleted)return;Visible=false;Deleted=true;
            if(route!=null&&Root!=null){route.Windows.Remove(Root);route.RemoveHandler(Yes);route.RemoveHandler(No);}
            Presentation=null;
        }
        private void OnGUI(){if(Visible&&Presentation!=null)Presentation.Draw();}
        private void OnDestroy(){Delete();}
    }
}
