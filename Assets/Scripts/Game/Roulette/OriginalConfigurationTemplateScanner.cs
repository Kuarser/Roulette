using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // CreateFromFile's scanner after admitted successful native stream acquisition.
    // Full ConfigurationOption lifecycle and IncorporateXMLTag are required owner bindings.
    public sealed class OriginalConfigurationTemplateScanner
    {
        public sealed class Host
        {
            public Func<object> Create;public Action<object> AppendCopy,Delete;
            public Action<object,byte[],byte[],uint> Incorporate;
            public Func<int> Count;
        }
        private readonly Host host;private readonly OriginalMachineDataNumeric numeric;
        public OriginalConfigurationTemplateScanner(Host supplied,OriginalCfgIntegerConverter.State errno)
        {
            if(supplied==null||supplied.Create==null||supplied.AppendCopy==null||supplied.Delete==null||supplied.Incorporate==null||supplied.Count==null)throw new ArgumentException("Actual configuration object, vector and tag owners required");
            host=new Host{Create=supplied.Create,AppendCopy=supplied.AppendCopy,Delete=supplied.Delete,Incorporate=supplied.Incorporate,Count=supplied.Count};numeric=new OriginalMachineDataNumeric(errno);
        }
        private static bool Has(byte[] raw,string value)
        {for(int i=0;i<=raw.Length-value.Length;++i){int j=0;while(j<value.Length&&raw[i+j]==(byte)value[j])++j;if(j==value.Length)return true;}return false;}
        private static List<byte[]> Tokens(byte[] raw)
        {
            var result=new List<byte[]>();int at=0;
            while(at<raw.Length){while(at<raw.Length&&(raw[at]==34||raw[at]==10))++at;if(at==raw.Length)break;int start=at;while(at<raw.Length&&raw[at]!=34&&raw[at]!=10)++at;byte[] token=new byte[at-start];Array.Copy(raw,start,token,0,token.Length);result.Add(token);}return result;
        }
        public bool AddAdmittedContent(byte[] content)
        {
            if(content==null)throw new ArgumentNullException();var tokens=Tokens(content);int prior=host.Count();bool inside=false,pending=false;int mode=0;object owner=null;byte[] field=new byte[0];uint order=0;
            foreach(byte[] token in tokens)
            {
                if(!pending&&Has(token,"="))
                {pending=true;if(Has(token,"name"))mode=3;else if(Has(token,"value"))mode=4;else if(Has(token,"order"))mode=5;}
                else if(!inside&&!pending&&Has(token,"<struct>"))
                {if(owner==null)owner=host.Create();inside=true;pending=false;mode=1;}
                else if(inside&&!pending&&Has(token,"</struct>"))
                {inside=false;mode=2;if(owner!=null){host.AppendCopy(owner);host.Delete(owner);owner=null;}}
                else if(mode==3){field=token;pending=false;mode=0;}
                else if(mode==4)
                {if(owner!=null){host.Incorporate(owner,(byte[])field.Clone(),(byte[])token.Clone(),order);order=0;}pending=false;mode=0;}
                else if(mode==5){order=unchecked((uint)numeric.ScanInt32(token,0));pending=false;mode=0;}
                else{pending=false;mode=0;}
            }
            if(owner!=null)host.Delete(owner);
            return unchecked((uint)prior)<unchecked((uint)host.Count());
        }
    }
}
