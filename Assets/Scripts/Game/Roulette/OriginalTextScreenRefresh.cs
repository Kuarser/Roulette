using System;

namespace RouletteDeluxe
{
    public static class OriginalTextScreenRefresh
    {
        public struct Position { public int X,Y,BorderX,BorderY; }
        private static int Percentage(float ratio)
        {
            double value=(double)ratio*100.0;
            if(double.IsNaN(value)||double.IsInfinity(value)||value<int.MinValue||value>int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ratio));
            return (int)value;
        }
        // Refresh 086bf540 uses integer percentage first; Display uses the
        // float ratio times (display-clip-border). Keep those distinct.
        public static Position GetPosition(OriginalHandpayBannerLayout layout)
        {
            if(layout==null)throw new ArgumentNullException(nameof(layout));
            int x=unchecked((layout.DisplayWidth-layout.ClipX)*Percentage(layout.NormalizedX))/100;
            int y=unchecked((layout.DisplayHeight-layout.ClipY)*Percentage(layout.NormalizedY))/100;
            return new Position {X=x,Y=y,BorderX=Math.Max(0,unchecked(x-layout.BorderWidth)),BorderY=Math.Max(0,unchecked(y-layout.BorderWidth))};
        }
    }
    public static class OriginalTextScreenRefreshValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("TextScreen Refresh regression "+checks);++checks;};
            var p=new OriginalHandpayBannerLayout(800,600);var r=OriginalTextScreenRefresh.GetPosition(p);
            check(r.X==168&&r.Y==240&&r.BorderX==163&&r.BorderY==235);
            check(p.Pixels().X==174&&p.Pixels().Y==243);
            p=new OriginalHandpayBannerLayout(540,960);r=OriginalTextScreenRefresh.GetPosition(p);check(r.X==113&&r.Y==384);
            p=new OriginalHandpayBannerLayout(800,600);p.ClipX=100;p.ClipY=50;r=OriginalTextScreenRefresh.GetPosition(p);check(r.X==147&&r.Y==220);
            p.NormalizedX=-.1f;p.NormalizedY=-.1f;r=OriginalTextScreenRefresh.GetPosition(p);check(r.X==-70&&r.Y==-55&&r.BorderX==0&&r.BorderY==0);
            p.NormalizedX=.5f;p.NormalizedY=.5f;p.BorderWidth=0;r=OriginalTextScreenRefresh.GetPosition(p);check(r.X==350&&r.Y==275&&r.BorderX==350&&r.BorderY==275);
            bool failed=false;p.NormalizedX=float.NaN;try{OriginalTextScreenRefresh.GetPosition(p);}catch(ArgumentOutOfRangeException){failed=true;}check(failed);
            return checks;
        }
    }
}
