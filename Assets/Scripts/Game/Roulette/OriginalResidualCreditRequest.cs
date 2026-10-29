using System;

namespace RouletteDeluxe
{
    // Explicit original request boundary. No denomination, wallet balance,
    // automatic cashout trigger, or RNG provider is inferred here.
    public static class OriginalResidualCreditRequest
    {
        public sealed class Host
        {
            public Action<int> SoftValidateNv,SaveNv;
            public Func<int> MainState;
            public Action<int> AssignMainState,PlatformComplete,UnknownOption;
            public Action PreResidualCreditGamble;
        }

        // Game::ResidualCreditGamble(long long,long long,Option), 080f3590.
        // The original deliberately serializes the LOW 32 bits of each
        // 64-bit argument. Preserve both saves and the invalid-option path.
        public static void ResidualCreditGamble(byte[] game,long balance,long target,int originalOption,Host host)
        {
            NeedGame(game);
            if(host==null||host.SoftValidateNv==null||host.SaveNv==null||host.MainState==null||
                host.AssignMainState==null||host.PlatformComplete==null||host.UnknownOption==null||host.PreResidualCreditGamble==null)
                throw new ArgumentException("Complete original RCG request owners required");
            host.SoftValidateNv(0x20c);
            Write32(game,0x90a,0);Write32(game,0x902,unchecked((uint)balance));
            Write32(game,0x906,unchecked((uint)target));host.SaveNv(0x20c);
            host.SoftValidateNv(0x20c);
            uint kind;
            switch(originalOption)
            {
                case 4:kind=4;break;
                case 5:kind=3;break;
                case 6:kind=2;break;
                case 7:kind=1;break;
                default:host.PlatformComplete(0);host.UnknownOption(originalOption);return;
            }
            Write32(game,0x90a,kind);host.SaveNv(0x20c);
            if(host.MainState()!=1){host.PlatformComplete(0);return;}
            host.PreResidualCreditGamble();host.AssignMainState(0xd);
        }

        // FeatGameAgent::ResidualCreditRequest, 08294f20. These are Agent
        // 56-byte entries, distinct from the 42-byte COMMON_FEATURES table.
        public static bool Send(byte[] agentFeatures,uint balance,uint target,uint kind,bool initial,
            Action<byte[],bool> send)
        {
            if(agentFeatures==null||agentFeatures.Length%0x38!=0||send==null)
                throw new ArgumentException("Complete original RCG Agent table and transport required");
            byte[] name={0x52,0x43,0x47,0x54,0x4f,0x50,0x30,0};
            for(int offset=0;offset<agentFeatures.Length;offset+=0x38)
            {
                if(Read32(agentFeatures,offset+0x24)!=5)continue;
                bool match=true;for(int i=0;i<8;i++)if(agentFeatures[offset+i]!=name[i]){match=false;break;}
                if(!match)continue;
                var message=new byte[0x15];Write32(message,0,0x310);Write32(message,4,0x15);
                Write32(message,8,balance);Write32(message,12,target);Write32(message,16,kind);
                message[20]=(byte)(initial?1:0);send(message,true);return true;
            }
            return false;
        }

        // FeatGameAgent::HandleRCGResponse, 08293430. Dispatch/packet
        // authentication belongs to the supplied IPC owner, as in the source.
        public static void HandleResponse(byte[] game,byte[] message,Action<int> softValidateNv,Action<int> saveNv)
        {
            NeedGame(game);
            if(message==null||message.Length<12||softValidateNv==null||saveNv==null)
                throw new ArgumentException("Complete original RCG response required");
            uint response=Read32(message,8);
            Write32(game,0x48,response==2?4u:response==4?2u:3u);
            if(response!=4)return;
            softValidateNv(0x20c);game[0x90f]=1;saveNv(0x20c);
        }
        private static void NeedGame(byte[] game)
        {if(game==null||game.Length<0x910)throw new ArgumentException("Truncated original RCG request state");}
        private static uint Read32(byte[] bytes,int offset)
        {return (uint)bytes[offset]|(uint)bytes[offset+1]<<8|(uint)bytes[offset+2]<<16|(uint)bytes[offset+3]<<24;}
        private static void Write32(byte[] bytes,int offset,uint value)
        {for(int i=0;i<4;i++)bytes[offset+i]=unchecked((byte)(value>>(i*8)));}
    }
}
