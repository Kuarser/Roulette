using System;
using System.IO;
using System.Text;
using System.Globalization;
namespace RouletteDeluxe
{
    // Native raw-byte XML fragments. This intentionally preserves the original
    // FE/FD substitution and does not impose an XML parser/Unicode normalization.
    public static class OriginalConfigurationXml
    {
        private static byte[] A(string text){return Encoding.ASCII.GetBytes(text);}
        private static void Raw(MemoryStream output,byte[] bytes){if(bytes==null)throw new ArgumentNullException();output.Write(bytes,0,bytes.Length);}
        public static byte[] Field(byte[] name,byte[] value)
        {
            if(name==null||value==null)throw new ArgumentNullException();using(var output=new MemoryStream())
            {Raw(output,A("  <field name = \""));Raw(output,name);Raw(output,A("\"  value = \""));foreach(byte b in value)output.WriteByte(b==34?(byte)254:b==0?(byte)253:b);Raw(output,A("\" />\n"));return output.ToArray();}
        }
        public static byte[] BooleanField(byte[] name,bool value)
        {if(name==null)throw new ArgumentNullException();using(var output=new MemoryStream()){Raw(output,A("  <field name = \""));Raw(output,name);Raw(output,A("\" value = \""));Raw(output,A(value?"true":"false"));Raw(output,A("\" />\n"));return output.ToArray();}}
        public static byte[] Option(OriginalSimpleConfigOption option)
        {if(option==null)throw new ArgumentNullException();using(var output=new MemoryStream()){Raw(output,Field(A("Name"),option.Name));Raw(output,Field(A("Value"),option.Value));Raw(output,Field(A("Category"),option.Category));return output.ToArray();}}
        public static byte[] Changes(OriginalSimpleConfigOption[] options)
        {if(options==null)throw new ArgumentNullException();using(var output=new MemoryStream()){foreach(var option in options){Raw(output,A("<struct>\n"));Raw(output,Option(option));Raw(output,A("</struct>\n"));}return output.ToArray();}}
        public static byte[] TestRequest(uint transactionId,OriginalConfigurationTransactionRecord.ErrorEntry[] errors)
        {
            var ordered=OriginalConfigurationTransactionRecord.Request.FromErrors(errors,transactionId,null,new byte?[4]).ErrorSnapshot();using(var output=new MemoryStream())
            {foreach(var entry in ordered){Raw(output,A("<struct>\n"));Raw(output,Option(entry.Key));foreach(var error in entry.Errors)Raw(output,Field(A("ErrorString"),error));Raw(output,A("</struct>\n"));}Raw(output,A("<struct>\n"));Raw(output,Field(A("TransactionId"),A(transactionId.ToString(CultureInfo.InvariantCulture))));Raw(output,A("</struct>\n"));return output.ToArray();}
        }
        public static void BindTransactionHost(OriginalConfigurationTransactionManager.Host host)
        {if(host==null)throw new ArgumentNullException();host.BuildChangesXml=Changes;host.BuildSummaryXml=(success,errors)=>TestRequest(success?1u:0u,errors);}
    }
}
