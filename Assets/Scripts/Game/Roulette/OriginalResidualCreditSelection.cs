using System;

namespace RouletteDeluxe
{
    // Game::StateResidualCredit 08115320. The host owns the original
    // substate and frame pump; only an explicit request initializes amounts.
    public static class OriginalResidualCreditSelection
    {
        public sealed class Host
        {
            public Func<int> Substate;
            public Action<int> AssignSubstate;
            public Func<bool> ResumeInMainState,Available,LastChanceSupported;
            public Action PlatformStartComplete,LastChanceHook,BeforeSelection,AfterSelection;
            public Action<int> SelectionInput,ProcessFrame,PlatformComplete,SoftValidateNv,SaveNv;
            public Action<byte> ScheduleEventOnGameOver;
            public Func<uint,uint,uint,bool,bool> SendRequest;
        }
        public static bool Run(byte[] game,Host h)
        {
            if(game==null||game.Length<0x859c)throw new ArgumentException("Truncated original RCG selection state");
            if(h==null||h.Substate==null||h.AssignSubstate==null||h.ResumeInMainState==null||h.Available==null||
                h.LastChanceSupported==null||h.PlatformStartComplete==null||h.LastChanceHook==null||h.BeforeSelection==null||
                h.AfterSelection==null||h.SelectionInput==null||h.ProcessFrame==null||h.PlatformComplete==null||h.SoftValidateNv==null||
                h.SaveNv==null||h.ScheduleEventOnGameOver==null||h.SendRequest==null)
                throw new ArgumentException("Complete original RCG selection owners required");
            if(h.Substate()==1&&game[0x90e]==0&&!h.ResumeInMainState())h.AssignSubstate(0);
            int state=h.Substate();
            if(state!=1)
            {
                if(state==2){Clear(game,h);return true;}
                if(state!=0)return true;
                h.PlatformStartComplete();
                if(!h.Available())
                {
                    h.PlatformComplete(1);h.AssignSubstate(2);Write(game,0x48,0);return true;
                }
                if(h.LastChanceSupported()){game[0x859b]=1;h.LastChanceHook();}
                h.BeforeSelection();h.SelectionInput(1);
                if(!Request(game,h,true))
                {
                    h.AssignSubstate(2);Write(game,0x48,0);
                    if(h.LastChanceSupported())game[0x859b]=0;
                    return true;
                }
                h.AssignSubstate(1);Write(game,0x48,1);
            }
            while(Read(game,0x48)==1)
            {
                if(game[0x90e]!=0)Write(game,0x48,3);
                h.ProcessFrame(1);
            }
            if(game[0x90e]!=0)Request(game,h,false);
            bool result;
            if(Read(game,0x48)==4)
            {
                h.AfterSelection();h.PlatformComplete(1);
                if(h.LastChanceSupported())game[0x859b]=0;
                result=true;
            }
            else if(Read(game,0x48)==2)
            {
                h.ScheduleEventOnGameOver(4);h.SoftValidateNv(0x20c);game[0x901]=1;h.SaveNv(0x20c);
                result=false;
            }
            else
            {
                h.PlatformComplete(0);if(h.LastChanceSupported())game[0x859b]=0;
                h.AfterSelection();result=false;
            }
            h.AssignSubstate(2);Clear(game,h);return result;
        }
        private static bool Request(byte[] game,Host h,bool initial)
        {return h.SendRequest(Read(game,0x902),Read(game,0x906),Read(game,0x90a),initial);}
        private static void Clear(byte[] game,Host h)
        {Write(game,0x48,0);h.SoftValidateNv(0x20c);game[0x90e]=0;h.SaveNv(0x20c);}
        private static uint Read(byte[] b,int o)
        {return (uint)b[o]|(uint)b[o+1]<<8|(uint)b[o+2]<<16|(uint)b[o+3]<<24;}
        private static void Write(byte[] b,int o,uint value)
        {for(int i=0;i<4;i++)b[o+i]=unchecked((byte)(value>>(i*8)));}
    }
}
