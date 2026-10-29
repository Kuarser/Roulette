using System;
using System.Text;

namespace RouletteDeluxe
{
    // CreditOutMgr::DisplayW2GHandPayScreen, 081eeca0. No payment is performed.
    public static class OriginalW2GHandpay
    {
        public sealed class Host
        {
            public OriginalCreditOutNvPayload Nv;
            public Action<byte[]> Save;
            public Func<long,bool,int,bool,byte[]> FormatMoney;
            public Func<bool> GamePrize, Voucher, Translate, ThresholdEnabled;
            public Func<long> GameAmount, GameThreshold, CreditAmount, CreditThreshold;
            public bool Manager3aa, ResetToCredits;
            public Action<byte[],byte[],byte[],byte[]> DisplayText;
        }
        public static void Display(long amount, Host host)
        {
            if(host==null||host.Nv==null||host.Save==null||host.FormatMoney==null||host.DisplayText==null)
                throw new ArgumentNullException(nameof(host));
            host.Nv.W2GDisplayedByte=1;host.Save(host.Nv.Bytes);
            byte[] formatted=host.FormatMoney(amount,true,100,true);
            if(formatted==null)throw new InvalidOperationException("Native money format missing");
            int n=Array.IndexOf(formatted,(byte)0);if(n<0)n=formatted.Length;
            var money=new byte[n];Array.Copy(formatted,money,n);
            byte[] body;
            if(host.GamePrize()&&amount==0)
                body=Ascii(host.Translate()?"YOU_HAVE_WON_A_PRIZE":"YOU HAVE WON A PRIZE!");
            else
            {
                string prefix=host.Voucher()?"VOUCHER":"HAND";
                if(host.Manager3aa||host.Translate())body=Ascii(prefix+"_PAY");
                else
                {
                    byte[] lead=Ascii(prefix+" PAY ");body=new byte[lead.Length+money.Length];
                    Array.Copy(lead,body,lead.Length);Array.Copy(money,0,body,lead.Length,money.Length);
                }
            }
            string detail="";
            if(host.ThresholdEnabled())
            {
                if(host.ResetToCredits)detail=host.Manager3aa?"Turn_Key_to_Reset_to_Credits":"Turn Key to Reset to Credits";
                else if(host.GameAmount()>=host.GameThreshold()||host.CreditAmount()>=host.CreditThreshold())
                    detail=host.Manager3aa?"Handpay_Required":"Handpay Required";
            }
            if(host.Manager3aa)
            {
                var state=host.Nv.ReadSelection();state.HandpaySelected=true;host.Nv.UpdateSelection(state);
                host.Save(host.Nv.Bytes);
            }
            else if(!host.Translate())
            {
                host.DisplayText(Ascii("Jackpot Call Attendant"),body,Ascii(detail),null);return;
            }
            host.DisplayText(Ascii("Jackpot_Call_Attendant"),body,money,Ascii(detail));
        }
        private static byte[] Ascii(string text){return Encoding.ASCII.GetBytes(text);}
    }
}
