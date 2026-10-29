using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public static class OriginalVideoInputDispatchValidation
    {
        private static OriginalVideoInputEvent Touch(uint type, int display, short x, short y)
        { return new OriginalVideoInputEvent { Type=type, Display=display, Size=0x1c, Point=new OriginalSpriteInputTarget.Point(x,y) }; }
        public static int Validate()
        {
            int checks=0; Action<bool> check=value=>{if(!value)throw new Exception("Video dispatch regression "+checks);++checks;};
            var dead=new HashSet<OriginalSpriteInputTarget>();var disposed=new List<OriginalVideoInputEvent>();int clockCalls=0;
            var route=new OriginalVideoInputDispatch(node=>!dead.Contains(node),()=>{++clockCalls;return new OriginalVideoInputDispatch.ClockSample{Seconds=123,Microseconds=int.MaxValue};},disposed.Add);
            var root=new OriginalSpriteInputTarget{Flags=1,Rectangle=new OriginalSpriteAlignment.Rectangle{Left=100,Top=200,Right=500,Bottom=600}};
            var yesNode=new OriginalSpriteInputTarget{Flags=0x47,ButtonMask=1,Parent=root,Rectangle=new OriginalSpriteAlignment.Rectangle{Left=10,Top=20,Right=130,Bottom=80}};
            var noNode=new OriginalSpriteInputTarget{Flags=0x47,ButtonMask=2,Parent=root,Rectangle=new OriginalSpriteAlignment.Rectangle{Left=150,Top=20,Right=270,Bottom=80}};
            root.Children.Add(yesNode);root.Children.Add(noNode);route.Windows.Add(root);
            var yes=new OriginalTextButtonPresentationState();var no=new OriginalTextButtonPresentationState();yes.Init();no.Init();
            var input=new OriginalVerifyButtonInput(yes,no);route.AttachVerifyButton(yesNode,input,true,true);route.AttachVerifyButton(noNode,input,false,true);
            var seen=new List<OriginalVideoInputEvent>();var points=new List<OriginalSpriteInputTarget.Point>();var origins=new List<OriginalSpriteInputTarget>();
            route.SetHandler(root,(ev,origin)=>{seen.Add(ev);points.Add(ev.Point);origins.Add(origin);return ev;});
            check(!route.ProcessOne());
            var down=Touch(0x200001,1,115,225);route.InputQueue.Enqueue(down);check(route.ProcessOne());
            int result;check(input.TryGetDisplayResult(out result)&&result==1&&yes.Pressed);
            check(route.TouchCaptured(1)==yesNode&&route.TouchCaptured(2)==null);
            check(points[0].X==15&&points[0].Y==25&&origins[0]==yesNode);
            check(disposed.Count==1&&ReferenceEquals(disposed[0],down)&&down.Point.X==115&&down.Point.Y==225);
            int parentCount=seen.Count;route.Dispatch(Touch(0x200002,1,499,599));
            check(seen.Count==parentCount&&route.TouchCaptured(1)==yesNode&&yes.Pressed);
            bool clearedBeforeUp=false;route.SetHandler(root,(ev,origin)=>{clearedBeforeUp=route.TouchCaptured(1)==null;return ev;});
            route.Dispatch(Touch(0x200004,1,499,599));check(clearedBeforeUp&&!yes.Pressed&&route.TouchCaptured(1)==null);
            route.Dispatch(Touch(0x200001,2,115,225));check(route.TouchCaptured(2)==yesNode&&route.TouchCaptured(1)==null);
            route.Dispatch(Touch(0x200001,1,255,225));check(route.TouchCaptured(1)==noNode&&route.TouchCaptured(2)==yesNode&&no.Pressed);
            check(input.TryGetDisplayResult(out result)&&result==0);
            var events=new List<OriginalVideoInputEvent>();route.SetHandler(root,(ev,origin)=>{events.Add(ev);return ev;});
            route.Dispatch(Touch(0x200001,2,255,225));
            check(clockCalls==1&&!yes.Pressed&&route.TouchCaptured(2)==noNode&&events.Count==2);
            check(events[0].Type==0x200004&&events[0].Display==1&&events[0].Seconds==123&&events[0].Nanoseconds==unchecked(int.MaxValue*1000));
            check(events[0].Point.X==110&&events[0].Point.Y==220); // Synthetic local(0,0) bubbles to screen origin.
            int n=disposed.Count;route.Dispatch(Touch(0x300001,1,0,0));check(disposed.Count==n+1&&route.TouchCaptured(1)==noNode);
            bool fault=false;try{route.Dispatch(Touch(0x200001,0,0,0));}catch(InvalidOperationException){fault=true;}check(fault);
            route.Dispatch(Touch(0x200004,1,255,225));route.Dispatch(Touch(0x200004,2,255,225));
            route.Dispatch(new OriginalVideoInputEvent{Type=0x200010,ButtonMask=1});check(route.ButtonsCaptured==yesNode&&yes.Pressed);
            route.Dispatch(new OriginalVideoInputEvent{Type=0x200020,ButtonMask=2});check(route.ButtonsCaptured==null&&!yes.Pressed);
            route.Dispatch(new OriginalVideoInputEvent{Type=0x200010,ButtonMask=1});route.Dispatch(new OriginalVideoInputEvent{Type=0x200010,ButtonMask=2});
            check(route.ButtonsCaptured==noNode&&!yes.Pressed&&no.Pressed);
            var replacement=new OriginalVideoInputEvent{Type=0x200001,Point=new OriginalSpriteInputTarget.Point(7,8)};
            route.SetHandler(yesNode,(ev,origin)=>replacement);route.SetHandler(root,(ev,origin)=>{check(ReferenceEquals(ev,replacement)&&ev.Point.X==17&&ev.Point.Y==28);return null;});
            var original=Touch(0x200001,1,0,0);n=disposed.Count;route.Bubble(yesNode,original);
            check(disposed.Count==n+2&&ReferenceEquals(disposed[n],original)&&ReferenceEquals(disposed[n+1],replacement));
            yesNode.Flags|=0x20000;route.SetHandler(yesNode,(ev,origin)=>{throw new Exception("No-events node called");});
            route.SetHandler(root,(ev,origin)=>{check(ev.Point.X==3&&ev.Point.Y==4);return ev;});
            route.Bubble(yesNode,Touch(0x200001,1,3,4)); // Skipped node does not add its origin.
            dead.Add(yesNode);n=disposed.Count;route.Bubble(yesNode,Touch(0x200001,1,0,0));check(disposed.Count==n+1);
            route.SetHandler(root,(ev,origin)=>{dead.Add(root);return replacement;});n=disposed.Count;
            route.Bubble(root,original);check(disposed.Count==n+1&&ReferenceEquals(disposed[n],original));
            return checks;
        }
    }
}
