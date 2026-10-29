using System;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalTextScreenUpdateValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("TextScreen update regression "+checks);++checks;};
            var p=new OriginalHandpayBannerLayout(800,600);var u=new OriginalTextScreenUpdate(p);var calls=new List<string>();
            var h=new OriginalTextScreenUpdate.Host {
                ResizeBody=(w,t)=>calls.Add("body:"+w+","+t),ResizeBorder=(w,t)=>calls.Add("border:"+w+","+t),
                SetText=(i,b)=>calls.Add("text:"+i+":"+Encoding.ASCII.GetString(b)),SetFont=(i,f)=>calls.Add("font:"+i+":"+f),
                MoveTo=(i,x,y)=>calls.Add("move:"+i+":"+x+","+y),Align=(i,x,y)=>calls.Add("align:"+i+":"+x+","+y),
                CreateLabel=(i,line)=>calls.Add("create:"+i+":"+Encoding.ASCII.GetString(line.Text)) };
            Action<string,int> add=(text,slot)=>p.AddSingleLine(Encoding.ASCII.GetBytes(text),slot,20,true);
            add("old",1);add("extra",3);p.Align((s,b)=>100);u.Language="en";check(u.ResetForUpdate());
            check(u.OldCount==2&&p.Lines.Count==0&&p.Pixels().Width==445);
            add("old",1);p.Align((s,b)=>100);u.UpdateDisplay(h,true);
            check(string.Join("|",calls)=="border:455,102|body:445,92|font:0:1|move:0:222,21|align:0:0,0|text:1:");
            calls.Clear();u.ResetForUpdate();add("changed",1);p.Align((s,b)=>100);u.UpdateDisplay(h,false);
            check(string.Join("|",calls)=="body:445,92|text:0:changed|move:0:222,21|align:0:0,0");
            calls.Clear();u.ResetForUpdate();add("changed",2);add("new",3);p.Align((s,b)=>100);u.UpdateDisplay(h,true);
            check(calls.Contains("font:0:2")&&calls.Contains("create:1:new")&&calls.Contains("align:1:0,0"));
            u.ResetForUpdate();check(u.OldCount==2);check(u.ResetForUpdate()&&u.OldCount==2);
            calls.Clear();u.UpdateDisplay(h,false);check(calls.Count==3&&calls[1]=="text:0:"&&calls[2]=="text:1:");
            p.Centering=true;add("center",1);u.ResetForUpdate();check(p.Centering&&p.Lines.Count==0);
            add("center",1);p.Align((s,b)=>100);calls.Clear();u.Language="zh";u.UpdateDisplay(h,false);
            check(calls.Contains("font:0:1")&&calls.Contains("move:0:222,5")&&calls.Contains("align:0:0,-1"));
            p.Lines[0].AnchorX=65535;p.Lines[0].AnchorY=32768;calls.Clear();u.UpdateDisplay(h,false);
            check(calls.Contains("move:0:-1,-32768"));
            return checks;
        }
    }
}
