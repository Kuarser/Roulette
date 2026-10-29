using System;

namespace RouletteDeluxe
{
    // RCG's MeterType 0/1 path: CMeterMgr::SetMeterVal (08164710),
    // CMeterObj::AdvanceMeter (08162c50). Currency text belongs to the host.
    public sealed class OriginalResidualCreditMeter
    {
        private OriginalFeatureFloat80 current,increment;
        private long target;
        private Action completed;
        public int PeriodMilliseconds {get;private set;}
        public bool ZeroDisplay {get;private set;}
        public OriginalFeatureFloat80 Current {get{return current;}}
        public OriginalFeatureFloat80 Increment {get{return increment;}}
        public long CurrencyCents {get{return current.Add(OriginalFeatureFloat80.FromDouble(.01)).TruncateInt64();}}
        public bool LabelVisible
        {
            // FRNDINT with RC=down is used only for the zero-display decision.
            get{return ZeroDisplay||current.CompareTo(OriginalFeatureFloat80.FromInt64(0))<0||
                current.CompareTo(OriginalFeatureFloat80.FromInt64(1))>=0;}
        }
        public void Set(long value,int type,int delay,bool zeroDisplay,Action completion)
        {
            if(type!=0&&type!=1)throw new ArgumentOutOfRangeException("type","RCG graph only uses meter types 0 and 1");
            completed=completion;ZeroDisplay=zeroDisplay;
            // The original rejects negative awards before changing the value.
            if(value<0)return;
            if(value==0&&type==0)
            {
                current=OriginalFeatureFloat80.FromInt64(0);increment=current;target=0;
                Finish();return;
            }
            OriginalFeatureFloat80 desired=OriginalFeatureFloat80.FromInt64(value);
            if(type==0){current=desired;increment=OriginalFeatureFloat80.FromInt64(0);target=value;Finish();return;}
            OriginalFeatureFloat80 difference=desired.Subtract(current);
            if(difference.Sign<0)difference=difference.Negate();
            target=value;
            if(difference.Sign==0){current=desired;increment=OriginalFeatureFloat80.FromInt64(0);Finish();return;}
            // DAT_082d5688 is the binary64 value 16.666666666666668,
            // whereas the fast rack timer is the integer 16 milliseconds.
            OriginalFeatureFloat80 ticks=OriginalFeatureFloat80.FromInt64(delay).Divide(
                OriginalFeatureFloat80.FromDouble(BitConverter.Int64BitsToDouble(0x4030aaaaaaaaaaabL)));
            int direction=desired.CompareTo(current)>0?1:-1;
            if(difference.CompareTo(ticks)>0)
            {
                increment=difference.Divide(ticks);if(direction<0)increment=increment.Negate();
                PeriodMilliseconds=16;
            }
            else
            {
                increment=OriginalFeatureFloat80.FromInt64(direction);
                int divisor=unchecked((int)difference.TruncateInt64());
                PeriodMilliseconds=delay/divisor;
            }
        }
        public void Tick(bool suspended=false)
        {
            if(suspended||PeriodMilliseconds==0)return;
            // Native stores current as binary64, adds in x87, then stores the
            // sum as binary64 again before writing the long-double state.
            OriginalFeatureFloat80 next=OriginalFeatureFloat80.FromDouble(
                OriginalFeatureFloat80.FromDouble(current.ToDouble()).Add(increment).ToDouble());
            OriginalFeatureFloat80 desired=OriginalFeatureFloat80.FromInt64(target);
            bool done=increment.Sign>0?next.CompareTo(desired)>=0:
                increment.Sign<0&&next.CompareTo(desired)<=0;
            if(done){next=desired;PeriodMilliseconds=0;}
            current=next;
            if(done)Finish();
        }
        private void Finish()
        {
            // Detach before invoking: callbacks may immediately start another rack.
            Action handler=completed;completed=null;if(handler!=null)handler();
        }
    }
}
