using System;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalW2GHandpayValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=v=>{if(!v)throw new Exception("W2G caller regression "+checks);++checks;};
            // Signed threshold edges: equality, below both, credit equality, negative, Int64 extremes.
            long[,] values={{10,10,0,1},{9,10,0,1},{9,10,1,1},{-2,-1,-3,-2},{long.MinValue,long.MaxValue,long.MaxValue,long.MaxValue}};
            bool[] required={true,false,true,false,true};
            long[] amounts={995,0,long.MinValue,long.MaxValue};
            for(int flags=0;flags<64;++flags)for(int variant=0;variant<amounts.Length;++variant)for(int edge=0;edge<5;++edge)for(int old=0;old<2;++old)
            {
                bool voucher=(flags&1)!=0,translate=(flags&2)!=0,manager=(flags&4)!=0,
                    enabled=(flags&8)!=0,reset=(flags&16)!=0,prize=(flags&32)!=0;
                long amount=amounts[variant];
                var original=new byte[135];for(int i=0;i<135;++i)original[i]=(byte)(i*19+7);
                original[0x5d]=(byte)(old==0?0:0x7f);
                var nv=new OriginalCreditOutNvPayload(original);var saved=new List<byte[]>();var trace=new List<string>();
                byte[][] output=null;byte[] money={82,77,57,46,57,53,0,255};
                var host=new OriginalW2GHandpay.Host {
                    Nv=nv,Manager3aa=manager,ResetToCredits=reset,
                    Save=b=>{trace.Add("save");saved.Add((byte[])b.Clone());b[0]=0;},
                    FormatMoney=(a,c,d,s)=>{trace.Add("money");check(a==amount&&c&&d==100&&s);return money;},
                    GamePrize=()=>{trace.Add("prize");return prize;},Voucher=()=>{trace.Add("voucher");return voucher;},
                    Translate=()=>{trace.Add("translate");return translate;},ThresholdEnabled=()=>{trace.Add("enabled");return enabled;},
                    GameAmount=()=>{trace.Add("game");return values[edge,0];},GameThreshold=()=>{trace.Add("gameLimit");return values[edge,1];},
                    CreditAmount=()=>{trace.Add("credit");return values[edge,2];},CreditThreshold=()=>{trace.Add("creditLimit");return values[edge,3];},
                    DisplayText=(t,b,c,d)=>{trace.Add("display");output=new[]{t,b,c,d};}
                };
                OriginalW2GHandpay.Display(amount,host);money[0]=0;
                string expectedBody=prize&&amount==0?(translate?"YOU_HAVE_WON_A_PRIZE":"YOU HAVE WON A PRIZE!"):
                    (voucher?"VOUCHER":"HAND")+((manager||translate)?"_PAY":" PAY RM9.95");
                string detail=!enabled?"":reset?(manager?"Turn_Key_to_Reset_to_Credits":"Turn Key to Reset to Credits"):
                    required[edge]?(manager?"Handpay_Required":"Handpay Required"):"";
                bool keyed=manager||translate;
                check(Text(output[0])==(keyed?"Jackpot_Call_Attendant":"Jackpot Call Attendant"));
                check(Text(output[1])==expectedBody&&Text(output[2])==(keyed?"RM9.95":detail));
                check(keyed?Text(output[3])==detail:output[3]==null);
                check(saved.Count==(manager?2:1)&&saved[0][0x51]==1&&saved[0][0x5d]==original[0x5d]);
                check(nv.Bytes[0x5d]==(manager?1:original[0x5d]));
                for(int i=0;i<135;++i)if(i!=0x51&&i!=0x5d)check(nv.Bytes[i]==original[i]);
                var expected=new List<string>{"save","money","prize"};
                if(prize&&amount==0)expected.Add("translate");else{expected.Add("voucher");if(!manager)expected.Add("translate");}
                expected.Add("enabled");
                if(enabled&&!reset){expected.Add("game");expected.Add("gameLimit");if(edge!=0){expected.Add("credit");expected.Add("creditLimit");}}
                if(manager)expected.Add("save");else expected.Add("translate");expected.Add("display");
                check(string.Join(",",trace)==string.Join(",",expected));
            }
            return checks;
        }
        private static string Text(byte[] bytes){return bytes==null?null:Encoding.ASCII.GetString(bytes);}
    }
}
