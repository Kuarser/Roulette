using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public static class OriginalCashoutTypographyValidation
    {
        public static int Validate()
        {
            int checks=0;Action<bool> check=b=>{if(!b)throw new Exception("CashOut typography check "+checks);++checks;};
            string[] keys={"CashOutFont-Amt-1920x1080","CashOutFont-Amt-1080x1920","CashOutFont-Amt-1360x768","CashOutFont-Amt-768x1360","CashOutFont-Msg-1920x1080","CashOutFont-Msg-1080x1920","CashOutFont-Msg-1360x768","CashOutFont-Msg-768x1360","CashOutFont-Trans-1920x1080","CashOutFont-Trans-1080x1920","CashOutFont-Trans-1360x768","CashOutFont-Trans-768x1360"};
            int[] expected={150,150,105,85,80,55,55,40,35,35,25,20};
            for(int enabled=0;enabled<2;++enabled)for(int mode=-2;mode<=4;++mode)for(int seed=0;seed<2;++seed)
            {
                var setup=new OriginalCashoutTypographySetup();setup.Heights.Add("custom",707);if(seed!=0)foreach(string key in keys)setup.Heights[key]=-9;
                var calls=new List<int>();setup.ConstructorFontType(enabled!=0,mode,n=>calls.Add(n));
                check(calls.Count==(enabled==0||mode==1||mode==2?1:0));if(calls.Count>0)check(calls[0]==(enabled==0?0:mode));
                check(setup.Heights["custom"]==707);check(setup.Heights.Count==(enabled!=0||seed!=0?13:1));
                for(int i=0;i<12;++i)check(enabled!=0?setup.Heights[keys[i]]==expected[i]:seed!=0?setup.Heights[keys[i]]==-9:!setup.Heights.ContainsKey(keys[i]));
            }
            int[] dims={1920,1080,1080,1920,1360,768,768,1360,800,600,-3,-32768};int cases=0;
            for(int tableState=0;tableState<3;++tableState)for(int ready=0;ready<2;++ready)for(int mask=0;mask<8;++mask)for(int d=0;d<dims.Length;d+=2)
            {
                ++cases;var setup=new OriginalCashoutTypographySetup();if(tableState>0)setup.ConstructorFontType(true,2,n=>{});
                if(tableState==2)foreach(string key in keys)setup.Heights[key]=999;
                var trace=new List<string>();var expectedTrace=new List<string>();int at=0;
                string[] files={"custom-message\0suffix","custom-amount","custom-translation"};
                var host=new OriginalCashoutTypographySetup.FontHost {IsBitmapFontFormat=path=>{check(path==files[at]);trace.Add("format:"+at);return(mask&(1<<at))!=0;},
                    SetCustomFont=(slot,path,size)=>{check(slot==7+at&&path==files[at]);trace.Add("font:"+at+":"+size);++at;},
                    SetCustomFontColor=(slot,rgb)=>{check(slot==8&&rgb==0xffd400&&at==2);trace.Add("gold");}};
                if(ready!=0)for(int i=0;i<3;++i)
                {
                    bool bitmap=(mask&(1<<i))!=0;int size=16;
                    if(bitmap)size=tableState==0||d>=8?0:tableState==2?999:i==0?expected[4+d/2]:i==1?expected[d/2]:expected[8+d/2];
                    expectedTrace.Add("format:"+i);expectedTrace.Add("font:"+i+":"+size);if(bitmap&&i==1)expectedTrace.Add("gold");
                }
                int countBefore=setup.Heights.Count;
                setup.ConfigurePrimaryFonts(ready!=0,files[0],files[1],files[2],(short)dims[d],(short)dims[d+1],host);
                check(string.Join("|",trace)==string.Join("|",expectedTrace));check(at==(ready!=0?3:0));
                int inserted=ready!=0&&(tableState==0||d>=8)?((mask&1)!=0?1:0)+((mask&2)!=0?1:0)+((mask&4)!=0?1:0):0;
                check(setup.Heights.Count==countBefore+inserted);
                setup.ConfigurePrimaryFonts(ready!=0,files[0],files[1],files[2],(short)dims[d],(short)dims[d+1],new OriginalCashoutTypographySetup.FontHost {IsBitmapFontFormat=p=>true,SetCustomFont=(s,p,h)=>{},SetCustomFontColor=(s,c)=>{}});
            }
            check(cases==288);
            for(int state=0;state<256;++state)for(int variant=0;variant<4;++variant)
            {
                var owner=new OriginalCashoutDisplay.State {Left=8,Top=9,Right=10,Bottom=11};var table=new OriginalCashoutTextBoundsTable();var trace=new List<string>();
                short[] values={short.MinValue,-1,0,short.MaxValue};int n=0;string file=variant==0?"":variant==1?"configuration.cfg":variant==2?"configuration.cfg\0ignored":"\0notempty";
                OriginalCashoutConfiguration.Read(owner,table,file,new OriginalCashoutConfiguration.Host {Open=(path,mode)=>{
                    check(path==(variant==2?"configuration.cfg":variant==3?"":file)&&mode==8);trace.Add("open");return new OriginalCashoutConfiguration.Input {
                        Extract=(ref short value)=>{check(value==0);trace.Add("extract:"+n);value=values[n++];},
                        State=()=>{trace.Add("state");return state;},CloseAndDestroy=()=>{check(state==0||owner.Left==0&&owner.Top==0&&owner.Right==0&&owner.Bottom==0);trace.Add("close");}};}});
                check(variant==0?trace.Count==0:string.Join("|",trace)=="open|extract:0|extract:1|extract:2|extract:3|state|close");
                check(table.Entries.Count==(variant==0?4:0));check(owner.Left==(variant!=0&&state==0?values[0]:0)&&owner.Top==(variant!=0&&state==0?values[1]:0)&&owner.Right==0&&owner.Bottom==(variant!=0&&state==0?values[3]:0));
            }
            return checks;
        }
    }
}
