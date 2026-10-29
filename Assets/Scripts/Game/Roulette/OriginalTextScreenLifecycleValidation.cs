using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public static class OriginalTextScreenLifecycleValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("TextScreen lifecycle regression "+checks);++checks;};
            var calls=new List<string>();var events=new List<byte[]>();OriginalTextScreenLifecycle state=null;
            state=new OriginalTextScreenLifecycle(new OriginalTextScreenLifecycle.Host {
                CreateDisplaySprites=()=>calls.Add("create"),ClearSprites=()=>calls.Add("clear"),ReleaseRoot=()=>calls.Add("release"),SetDefaults=()=>calls.Add("defaults"),
                SetRootVisible=v=>calls.Add(v?"show":"hide"),Emit=(raw,flags)=>{
                    check(flags==0&&raw.Length==44&&raw[0]==1&&raw[2]==44);
                    for(int i=0;i<44;++i)if(i!=0&&i!=2&&i!=24&&i!=25&&i!=26)check(raw[i]==0);
                    check(raw[25]==0x81||raw[25]==0x82);check(raw[26]==1);
                    check(state.Visible==(raw[25]==0x81));events.Add((byte[])raw.Clone());calls.Add(raw[25]==0x81?"open":"close"); } });
            check(!state.Visible&&!state.RootPresent);check(state.Hide());check(calls.Count==0);
            check(state.Show());check(state.Visible&&!state.RootPresent&&events.Count==1);
            state.Show();check(events.Count==1);state.Hide();check(events.Count==2&&!state.Visible);
            calls.Clear();state.Display();check(string.Join(",",calls)=="create,open"&&state.RootPresent);
            calls.Clear();state.Show();check(string.Join(",",calls)=="show");
            calls.Clear();state.Hide();check(string.Join(",",calls)=="hide,close");
            calls.Clear();state.Hide();check(string.Join(",",calls)=="hide");
            calls.Clear();state.Show();check(string.Join(",",calls)=="show,open");
            calls.Clear();state.Clear();check(string.Join(",",calls)=="clear"&&state.Visible&&state.RootPresent);
            calls.Clear();state.End();check(string.Join(",",calls)=="close,clear,release,defaults"&&!state.RootPresent&&!state.Visible);
            calls.Clear();state.End();check(string.Join(",",calls)=="clear,release,defaults");
            calls.Clear();state.Display();state.Display();check(string.Join(",",calls)=="create,open,create,open");
            calls.Clear();state.Pause(true);state.Pause(false);check(calls.Count==0);
            check(OriginalTextScreenLifecycle.UsingBMFonts(2)&&!OriginalTextScreenLifecycle.UsingBMFonts(0)&&!OriginalTextScreenLifecycle.UsingBMFonts(1));
            var layout=new OriginalHandpayBannerLayout(800,600);layout.Centering=true;layout.AddSingleLine(new byte[]{65},1,20,true);
            layout.ClearNativeLines();check(!layout.Centering&&layout.Lines.Count==0&&layout.CurrentFontHeight==20);
            layout.NormalizedWidth=.8f;layout.ClipX=20;layout.HorizontalAlignment=1;layout.SetHandpayDefaults();
            check(layout.Pixels().Width==445&&layout.ClipX==0&&layout.HorizontalAlignment==0);
            return checks;
        }
    }
}
