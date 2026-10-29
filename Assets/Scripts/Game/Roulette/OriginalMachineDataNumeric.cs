using System;
namespace RouletteDeluxe
{
    // MachineData uses sscanf %ld/%lld and default-decimal istringstream >> int64.
    // Explicit ASCII C/classic locale profile; caller owns the shared errno state.
    public sealed class OriginalMachineDataNumeric
    {
        private readonly OriginalCfgIntegerConverter.State errno;
        public OriginalMachineDataNumeric(OriginalCfgIntegerConverter.State state){errno=state??throw new ArgumentNullException();}
        private static bool Space(byte b){return b==32||(b>=9&&b<=13);}
        private sealed class Parsed{public long Value;public bool Digits,Overflow;public int First,End,Length;}
        private static Parsed Decimal(byte[] raw,bool wide)
        {
            if(raw==null)throw new ArgumentNullException();int length=Array.IndexOf(raw,(byte)0);if(length<0)length=raw.Length;
            for(int i=0;i<length;++i)if(raw[i]>127)throw new ArgumentException("ASCII C/classic locale numeric input required");
            int at=0;while(at<length&&Space(raw[at]))++at;int first=at;bool negative=false;
            if(at<length&&(raw[at]==43||raw[at]==45)){negative=raw[at]==45;++at;}
            int start=at;ulong value=0,limit=wide?(negative?0x8000000000000000UL:0x7fffffffffffffffUL):(negative?2147483648UL:2147483647UL);bool overflow=false;
            while(at<length&&raw[at]>=48&&raw[at]<=57)
            {ulong digit=(ulong)(raw[at]-48);if(value>(limit-digit)/10)overflow=true;if(!overflow)value=value*10+digit;++at;}
            if(overflow)value=limit;long result=negative?unchecked((long)(0UL-value)):unchecked((long)value);
            return new Parsed{Value=result,Digits=at!=start,Overflow=overflow,First=first,End=at,Length=length};
        }
        public long ScanInt64(byte[] raw,long previous)
        {var p=Decimal(raw,true);if(!p.Digits)return previous;if(p.Overflow)errno.Errno=34;return p.Value;}
        public int ScanInt32(byte[] raw,int previous)
        {var p=Decimal(raw,false);if(!p.Digits)return previous;if(p.Overflow)errno.Errno=34;return (int)p.Value;}
        public long ExtractStreamInt64(byte[] raw,long previous)
        {
            var p=Decimal(raw,true);
            if(p.First==p.Length)return previous; // Sentry fails at EOF before num_get, leaving target unchanged.
            return p.Digits?p.Value:0; // num_get writes zero on no digits, saturates on overflow; no errno mutation.
        }
        public OriginalMachineDataDefaults.Host BindMachineDefaults(OriginalMachineDataDefaults.Host h)
        {
            if(h==null)throw new ArgumentNullException();return new OriginalMachineDataDefaults.Host{PrimaryOperation=h.PrimaryOperation,JurisdictionByte=h.JurisdictionByte,JurisdictionInt=h.JurisdictionInt,JurisdictionAmount=h.JurisdictionAmount,JurisdictionText=h.JurisdictionText,ParseFile=h.ParseFile,ParserString=h.ParserString,ClearParser=h.ClearParser,SystemString=h.SystemString,SystemBool=h.SystemBool,ScanInt64=ScanInt64,ScanInt32=ScanInt32,ExtractStreamInt64=ExtractStreamInt64};
        }
    }
}
