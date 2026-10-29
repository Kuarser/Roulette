using System;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    // TextScreenCashOut::HasCustomAnimationFiles,086c67f0. Directory is passed by value.
    public static class OriginalCashoutCustomResources
    {
        private static readonly string[] Names={"CashOutAnimation-Bottom","CashOutAnimation","CashOutAnimation-Top","CashOutAnimation-Cascade",
            "CashOutSound.x","CashOutSurroundSound.x","CashOutFont-Msg","CashOutFont-Amt","CashOutFont-Trans","CashOutFont-Msg-Small.x","CashOutFont-Amt-Small.x"};
        public static bool HasCustomAnimationFiles(string directory,Func<string,bool> exists)
        {
            if(directory==null||exists==null)throw new ArgumentNullException();
            if(directory=="")directory="/games/";
            foreach(string name in Names)if(exists(directory+name))return true;
            return false;
        }
    }
    public static class OriginalCashoutCustomResourcesValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut custom resource regression "+checks);++checks;};
            string[] names={"CashOutAnimation-Bottom","CashOutAnimation","CashOutAnimation-Top","CashOutAnimation-Cascade","CashOutSound.x","CashOutSurroundSound.x",
                "CashOutFont-Msg","CashOutFont-Amt","CashOutFont-Trans","CashOutFont-Msg-Small.x","CashOutFont-Amt-Small.x"};
            foreach(string input in new[]{"","/games/","/custom/","/noSlash"})for(int mask=0;mask<2048;++mask)
            {
                string root=input==""?"/games/":input;var trace=new List<string>();int index=0;
                bool result=OriginalCashoutCustomResources.HasCustomAnimationFiles(input,path=>{
                    check(path==root+names[index]);trace.Add(path);return (mask&(1<<index++))!=0;});
                int first=0;while(first<11&&(mask&(1<<first))==0)++first;
                check(result==(mask!=0));check(index==(first==11?11:first+1));
                check(trace[0]==root+"CashOutAnimation-Bottom"&&trace.Count==index);
                // Caller supplied string retains its exact bytes and slash state.
                check(input==""||root==input);
            }
            int queries=0;check(!OriginalCashoutCustomResources.HasCustomAnimationFiles("/custom/",p=>{++queries;return p.EndsWith(".bik",StringComparison.Ordinal)||p.EndsWith(".fnt",StringComparison.Ordinal)||p.EndsWith(".cfg",StringComparison.Ordinal);}));
            check(queries==11);return checks;
        }
    }
}
