using System;

namespace RouletteDeluxe
{
    // RCGTopbox ELF D52A1F8473E9AFBBAC32B765A24EAB1A08B9A0AB79491EE335EFAF565495156F.
    // Media, input and SafeRam owners are explicit. Money values passed to the
    // media are the original signed/unsigned values; this module owns no wallet.
    public sealed class OriginalResidualCreditTopbox
    {
        public sealed class Host
        {
            public Action LoadRegular, LoadCore, SaveRegular, SaveCore;
            public Action GoOnline, GoOffline, ShowServiceButton;
            public Action DisableInput, CancelHelpTimer, EnableGambleInput, EnableHelpInput;
            public Func<int, short> SelectedInput;
            public Func<string, int> FeatureId;
            public Action<int> Stop;
            // Notify -1 means ordinary playback. Null money means no meters.
            public Action<int, long[], int> Play;
            public Action<byte> SelectionComplete;
            public Action<int> CommandResult;
            public Action<ushort, ushort, ushort> CommandResponse;
        }

        public const int CoreOffset = 0xbbf;
        public const int RegularOffset = 0xbb4;
        private readonly byte[] state;
        private readonly Host host;
        public OriginalResidualCreditTopbox(byte[] state, Host host)
        {
            if(state == null || state.Length < CoreOffset + 20) throw new ArgumentException("Truncated RCG topbox state");
            if(host == null || host.LoadRegular == null || host.LoadCore == null || host.SaveRegular == null ||
                host.SaveCore == null || host.GoOnline == null || host.GoOffline == null || host.ShowServiceButton == null ||
                host.DisableInput == null || host.CancelHelpTimer == null || host.EnableGambleInput == null ||
                host.EnableHelpInput == null || host.SelectedInput == null || host.FeatureId == null || host.Stop == null ||
                host.Play == null || host.SelectionComplete == null || host.CommandResult == null || host.CommandResponse == null)
                throw new ArgumentException("Incomplete RCG topbox host");
            this.state=state;this.host=host;
        }
        public byte LiveState { get { return state[0xb7e]; } }
        public byte Selection { get { return state[0xbb1]; } }
        private uint Word(int offset) {return (uint)state[offset]|(uint)state[offset+1]<<8|(uint)state[offset+2]<<16|(uint)state[offset+3]<<24;}
        private void Word(int offset,uint value) {for(int i=0;i<4;i++)state[offset+i]=unchecked((byte)(value>>(i*8)));}
        private void Load() {host.LoadRegular();host.LoadCore();}
        private void Save() {host.SaveRegular();host.SaveCore();}
        private int Id(string name) {return host.FeatureId(name);}
        private int Variant(string name,int kindOffset) {return unchecked(Id(name)+state[kindOffset]-1);}
        private void Play(string name,long[] money=null,int notify=-1) {host.Play(Id(name),money,notify);}
        private void VariantPlay(string name,int kindOffset,long[] money=null,int notify=-1) {host.Play(Variant(name,kindOffset),money,notify);}
        private bool Won { get {return Word(0xbb6)!=0 || Word(0xbba)!=0;} }
        private long Award { get {return unchecked((long)(Word(0xbb6)|(ulong)Word(0xbba)<<32));} }
        private long[] OfferedMoney() {return new long[]{Word(0xba8),Word(0xbac),Word(0xba8)};}
        private long[] SavedMoney() {return new long[]{unchecked((int)Word(0xbc2)),unchecked((int)Word(0xbc6)),unchecked((int)Word(0xbc2))};}

        public byte[] CaptureCore() {var copy=new byte[20];Buffer.BlockCopy(state,CoreOffset,copy,0,20);return copy;}
        public byte[] CaptureRegular() {var copy=new byte[11];Buffer.BlockCopy(state,RegularOffset,copy,0,11);return copy;}
        public void LoadCore(byte[] record)
        {
            if(record==null||record.Length!=20)throw new ArgumentException("Invalid RCG topbox core record");
            Buffer.BlockCopy(record,0,state,CoreOffset,20);
        }
        public void LoadRegular(byte[] record)
        {
            if(record==null||record.Length!=11)throw new ArgumentException("Invalid RCG topbox regular record");
            Buffer.BlockCopy(record,0,state,RegularOffset,11);
            if(state[RegularOffset]==0)state[RegularOffset]=11;
        }

        public void Start(uint amount,uint target,int kind,bool initial)
        {
            Load();host.ShowServiceButton();
            if(initial)
            {
                state[0xbb0]=state[0xbc1]=unchecked((byte)kind);
                Word(0xba8,amount);Word(0xbac,target);Word(0xbc2,amount);Word(0xbc6,target);
                state[0xbb1]=0;state[0xba5]=1;Save();
                if(state[0xba6]!=0)return;
                host.GoOnline();
                int saved=state[CoreOffset]>>4;
                if(saved==13){WaitButton();return;}
                if(saved!=12){state[0xb7e]=0;if(state[0xbb0]!=0)ScrollUpGamble();return;}
            }
            state[0xbb1]=3;state[0xbb2]=1;WaitButton();state[0xbb2]=0;Save();
        }
        public void Custom(int type,uint amount,uint target,int kind)
        {
            if(type==2)
            {
                Load();Word(0xba8,amount);Word(0xbac,target);Word(0xbc2,amount);Word(0xbc6,target);
                state[0xbb0]=state[0xbc1]=unchecked((byte)kind);Save();
            }
            host.CommandResponse(0,0,0);
        }
        public void InitTopbox() {state[0xba7]=0;host.GoOnline();}
        public void FeatureStart()
        {
            if(state[0xba7]!=0){host.GoOnline();state[0xba7]=0;}
            state[0xba5]=0;
        }
        public void GoOffline()
        {
            if(state[0xba7]!=0)return;
            state[0xb7e]=0;host.GoOffline();state[0xba7]=1;
        }
        public void ScrollUpGamble()
        {
            state[0xb7e]=0;
            if(state[0xbb0]!=0)VariantPlay("f_ScrollUpBkgd_HandpayCancel",0xbb0,OfferedMoney(),13);
        }
        public void ShowGamble()
        {
            if(state[0xbb0]!=0)
            {
                VariantPlay("f_Background_HandpayCancel",0xbb0);
                Play("f_Display_Meters",OfferedMoney());Play("f_sndFadeIn");
            }
            EnableGambleButtons();
        }
        public void EnableGambleButtons() {state[0xb7e]=4;host.EnableGambleInput();}
        public void EnableHelpButton()
        {
            if(state[0xbb0]!=0)VariantPlay("f_Help_HandpayCancel",0xbb0);
            host.EnableHelpInput();
        }
        public void ScrollDownHelp()
        {
            host.DisableInput();host.CancelHelpTimer();
            if(state[0xbb0]!=0)VariantPlay("f_ScrollDownHelp_HandpayCancel",0xbb0,null,16);
        }
        public void WaitButton()
        {
            state[0xb7e]=5;
            short pick=3;bool redraw=true;
            if(state[0xbb2]==0)
            {
                pick=2;
                if(state[CoreOffset]>>4!=13){pick=host.SelectedInput(unchecked((int)Word(0xb84)));redraw=pick!=1;}
            }
            host.DisableInput();
            long[] offered=OfferedMoney();
            if(redraw&&state[0xbb0]!=0)
            {
                host.Stop(Variant("f_ScrollDownHelp_HandpayCancel",0xbb0));
                host.Stop(Variant("f_Background_HandpayCancel",0xbb0));
                VariantPlay("f_DisabledBkgd_HandpayCancel",0xbb0);Play("f_Display_Reel");Play("f_Display_Meters",offered);
            }
            if(pick==0||pick==7)
            {
                Load();Word(0xbc2,Word(0xba8));Word(0xbc6,Word(0xbac));state[0xbc1]=state[0xbb0];
                Save();state[0xbb1]=4;SendGamblePick();return;
            }
            if(pick==1)
            {
                state[0xb7e]=6;
                if(state[0xbb0]!=0)VariantPlay("f_ScrollUpHelp_HandpayCancel",0xbb0,null,16);
                return;
            }
            if(pick!=2&&pick!=3)return;
            state[CoreOffset]=unchecked((byte)((state[CoreOffset]&15)|(pick==2?0xd0:0xc0)));
            Save();state[0xbb1]=unchecked((byte)pick);
            // Native code uses uninitialized money only for malformed kind=0.
            // Reject that unsupported call rather than create a payment/display.
            if(state[0xbb0]==0)throw new InvalidOperationException("RCG exit has no initialized gamble type");
            VariantPlay("f_ScrollDownBkgd_HandpayCancel",0xbb0,offered,15);
        }
        public void SendGamblePick()
        {
            if(state[0xbb1]!=4)
            {
                int background=Variant("f_ScrollDownBkgd_HandpayCancel",0xbb0);
                Play("f_sndFadeOut");host.Stop(Id("f_sndFadeIn"));host.Stop(Id("f_sndLoop"));host.Stop(background);
                Word(0xba8,0);Word(0xbac,0);state[0xbb0]=0;Word(0xbc2,0);Word(0xbc6,0);state[0xbc1]=0;Save();
            }
            state[0xb7e]=0;host.SelectionComplete(state[0xbb1]);
            state[CoreOffset]&=15;Save();
        }
        public void SpinReel()
        {
            state[0xb7e]=8;Load();host.GoOnline();
            if(state[0xbc1]!=0)
            {
                VariantPlay("f_DisabledBkgd_HandpayCancel",0xbc1);Play("f_Display_Reel");Play("f_Display_Meters",SavedMoney());
            }
            Play(Won?"f_Spin_Win":"f_Spin_NoWin",null,19);
        }
        public void SpinComplete()
        {
            if(state[0xbc1]!=0)VariantPlay(Won?"f_DisabledWin_HandpayCancel":"f_DisabledLose_HandpayCancel",0xbc1,SavedMoney());
            host.CommandResult(0);
        }
        public void RackWin()
        {
            state[0xb7e]=9;Load();
            if(state[0xbc1]==0)return;
            VariantPlay(Won?"f_DisabledWin_HandpayCancel":"f_DisabledLose_HandpayCancel",0xbc1,SavedMoney());
            Play(Won?"f_Rack_Meters":"f_Rack_Lose_Meters",new long[]{Award,unchecked((int)Word(0xbc2)),unchecked((int)Word(0xbc6))},21);
        }
        public void RackScroll()
        {
            Play("f_sndFadeOut");
            if(state[0xbc1]!=0)VariantPlay(Won?"f_ScrollDownWin_HandpayCancel":"f_ScrollDownLose_HandpayCancel",0xbc1,
                new long[]{unchecked((int)Word(0xbc2)),unchecked((int)Word(0xbc6)),Award},22);
        }
        public void RackComplete()
        {
            if(state[0xbc1]!=0)host.Stop(Variant(Won?"f_ScrollDownWin_HandpayCancel":"f_ScrollDownLose_HandpayCancel",0xbc1));
            Word(0xba8,0);Word(0xbac,0);state[0xbb0]=0;host.CommandResult(0);
        }
        public bool ProcessEvent(uint eventId,int argument1,int argument2)
        {
            if(state[0x68]==0)return false;
            Word(0xb84,unchecked((uint)argument1));Word(0xb88,unchecked((uint)argument2));
            byte current=state[0xb7e];Word(0xb80,eventId);
            if(Word(0xb90)!=0)return false;
            if(eventId==24){state[0xb7e]=0;GoOffline();return true;}
            if(current==0)
            {
                if(eventId==11){state[0xb7e]=0;FeatureStart();return true;}
                if(eventId==13){state[0xb7e]=4;ShowGamble();return true;}
                if(eventId==18){state[0xb7e]=8;SpinReel();return true;}
                if(eventId==20){state[0xb7e]=9;RackWin();return true;}
                if(eventId==0)throw new InvalidOperationException("Original RCG topbox state-0/event-0 dispatch has no target");
            }
            else if(current==4&&eventId==14){state[0xb7e]=5;WaitButton();return true;}
            else if(current==5)
            {
                if(eventId==15){state[0xb7e]=0;SendGamblePick();return true;}
                if(eventId==16){state[0xb7e]=4;EnableGambleButtons();return true;}
            }
            else if(current==6&&eventId==16){state[0xb7e]=7;EnableHelpButton();return true;}
            else if(current==7&&eventId==14){state[0xb7e]=5;ScrollDownHelp();return true;}
            else if(current==8&&eventId==19){state[0xb7e]=0;SpinComplete();return true;}
            else if(current==9&&eventId==21){state[0xb7e]=10;RackScroll();return true;}
            else if(current==10&&eventId==22){state[0xb7e]=0;RackComplete();return true;}
            else if(current==2&&eventId==12){state[0xb7e]=0;host.CommandResponse(0,0,0);return true;}
            return false;
        }
    }
}
