using System;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectionValidation
    {
        public static int Validate(Func<string,byte[]> readTranslation,OriginalPlatformMonetaryLocale locale)
        {
            int checks=0;Action<bool> check=value=>{if(!value)throw new Exception("Handpay selection regression "+checks);++checks;};
            var translator=OriginalLanguageTranslator.Load(readTranslation);
            foreach(var language in new[]{"en","zh","fr_CA"})
            {
                var content=OriginalHandpaySelectionText.Build(translator,995,language,true,
                    amount=>locale.FormatText(amount,100,false,0),Encoding.UTF8.GetBytes,null);
                check(Encoding.UTF8.GetString(content.AmountLine)=="RM9.95 cannot be paid from the hopper.");
                check(Encoding.UTF8.GetString(content.Yes)=="Hand\nPay\0");
                check(Encoding.UTF8.GetString(content.No)=="Continue\nGame Play\0");
                check(content.FontLanguage==language);
            }
            var spanish=OriginalHandpaySelectionText.Build(translator,995,"es_CL",true,
                amount=>locale.FormatText(amount,100,false,0),Encoding.UTF8.GetBytes,null);
            check(Encoding.UTF8.GetString(spanish.AmountLine)=="RM9.95 no se puede pagar desde la tolva.");
            check(Encoding.UTF8.GetString(spanish.Yes)=="Pago\nde Mano\0");
            check(Encoding.UTF8.GetString(spanish.No)=="Continúe\nLa Jugada\0");
            check(Encoding.UTF8.GetString(spanish.OptionLine)=="Por favor toque una opción.");
            check(spanish.FontLanguage=="es_CL");
            var forced=OriginalHandpaySelectionText.Build(translator,0,"zh",false,amount=>"zero",Encoding.UTF8.GetBytes,null);
            check(forced.FontLanguage=="en"&&Encoding.UTF8.GetString(forced.AmountLine)=="zero cannot be paid from the hopper.");
            var raw=OriginalHandpaySelectionText.FirstSpaceToNewline(new byte[]{65,32,66,32,67,0,32});
            check(raw[1]==10&&raw[3]==32&&raw[6]==32);
            check(OriginalHandpaySelectionText.FirstSpaceToNewline(new byte[]{65,0,32})[2]==32);
            foreach(bool disabled in new[]{false,true})foreach(int display in new[]{-1,0,1})
            {
                var calls=new List<string>();var saves=new List<OriginalHandpaySelectionNvState>();
                var state=new OriginalHandpaySelectionNvState{ResidualSubstate=8};
                var host=new OriginalHandpaySelectionCoordinator.Host{
                    BeforeResidualSelection=()=>calls.Add("before"),SetTransactionInProgress=value=>calls.Add("transaction:"+value),
                    PrepareScreen=()=>calls.Add("prepare"),Save=value=>{saves.Add(value);calls.Add("save:"+value.InProgress+":"+value.ResidualSubstate+":"+value.HandpaySelected);},
                    ReleaseScreenAndTiltEvents=()=>calls.Add("release"),MachineDisabled=()=>{calls.Add("disabled");return disabled;},
                    ResidualGamble=()=>calls.Add("gamble"),AfterResidualGamble=()=>calls.Add("after"),EmitResidualEvent=()=>calls.Add("event")};
                var coordinator=new OriginalHandpaySelectionCoordinator(state,host);
                check(coordinator.Begin()&&coordinator.ScreenPresent&&state.InProgress==1&&state.ResidualSubstate==3);
                check(calls[0]=="before"&&calls[1]=="transaction:True"&&calls[2]=="prepare"&&saves.Count==2&&saves[0].ResidualSubstate==8);
                int count=calls.Count;check(!coordinator.Begin()&&calls.Count==count);
                bool selected=disabled||display!=0;check(coordinator.Complete(display)==selected&&!coordinator.ScreenPresent&&state.InProgress==0);
                check(calls[count]=="save:0:3:False"&&calls[count+1]=="release"&&calls[count+2]=="disabled");
                check(state.ResidualSubstate==(selected?4:9)&&state.HandpaySelected==selected);
                check(saves.Count==(selected?5:4));
                check(selected?!calls.Contains("gamble"):calls[calls.Count-3]=="gamble"&&calls[calls.Count-2]=="after"&&calls[calls.Count-1]=="event");
                bool rejected=false;try{coordinator.Complete(display);}catch(InvalidOperationException){rejected=true;}check(rejected);
            }
            return checks;
        }
    }
}
