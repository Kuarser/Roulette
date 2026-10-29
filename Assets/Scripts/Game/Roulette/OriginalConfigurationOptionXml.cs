using System;
using System.IO;
using System.Text;
using System.Globalization;
namespace RouletteDeluxe
{
    public static class OriginalConfigurationOptionXml
    {
        private static byte[] A(string text){return Encoding.ASCII.GetBytes(text);}
        private static void Raw(MemoryStream stream,byte[] bytes){stream.Write(bytes,0,bytes.Length);}
        private static void Field(MemoryStream stream,string name,byte[] value){Raw(stream,OriginalConfigurationXml.Field(A(name),value));}
        public static byte[] OrderedField(byte[] name,byte[] value,uint order)
        {
            if(name==null||value==null)throw new ArgumentNullException();
            using(var stream=new MemoryStream())
            {
                Raw(stream,A("  <field name = \""));Raw(stream,name);Raw(stream,A("\"  order = \""));
                Raw(stream,A(unchecked((int)order).ToString(CultureInfo.InvariantCulture)));Raw(stream,A("\"  value = \""));
                foreach(byte b in value)stream.WriteByte(b==34?(byte)254:b==0?(byte)253:b);
                Raw(stream,A("\" />\n"));return stream.ToArray();
            }
        }
        public static byte[] Option(OriginalConfigurationOption option)
        {
            if(option==null)throw new ArgumentNullException();
            using(var stream=new MemoryStream())
            {
                Field(stream,"Category",option.Category);Field(stream,"Name",option.Name);
                if(option.Control==10){Field(stream,"Control Type",A("Category"));return stream.ToArray();}
                Field(stream,"Value",option.Value);
                string type=option.Type==10?"Integer":option.Type==20?"Decimal":option.Type==30?"String":option.Type==40?"Boolean":"Unknown";
                Field(stream,"Type",A(type));
                if(option.HasMinimum)Field(stream,"Minimum",option.Minimum);
                if(option.HasMaximum)Field(stream,"Maximum",option.Maximum);
                if(option.HasHelp)Field(stream,"OptionHelp",option.Help);
                var orders=option.Orders;
                foreach(var allowed in option.Allowed)
                {
                    uint order=0;
                    foreach(var item in orders){if(Equal(item.Key,allowed.Key)){order=item.Order;break;}}
                    if(order==0)Field(stream,"Allowed Value",allowed.Key);
                    else Raw(stream,OrderedField(A("Allowed Value"),allowed.Key,order));
                    foreach(var rule in allowed.Rules)Field(stream,"Allowed Value Rule",rule);
                }
                string[] controls={"Unknown","Category","Single_Line_Edit_Box","Multi_Line_Edit_Box","Slider","CheckBox","CheckBoxArray","ListBox","ComboBox","RadioButton"};
                uint control=option.Control;string controlName=control%10==0&&control<=90?controls[control/10]:"Unknown";
                Field(stream,"Control Type",A(controlName));
                foreach(var rule in option.Rules)Field(stream,"Rule",rule);
                var flags=option.Flags;
                int[] indices={0,10,11,12,9,1,3,4,5,6};
                string[] labels={"ReadOnly","LocallySettable","Volatile","SaveSpaceIfEmpty","RemotelySettable","OneTimeSettable","ReadOnlyWithCredits","Visible","RestrictToAllowedValues","UniquePerMachine"};
                for(int i=0;i<indices.Length;++i)
                {
                    int index=indices[i];byte initial=(index==4||index==9||index==10)?(byte)1:(byte)0;
                    if(flags[index]==initial)continue;
                    Raw(stream,OriginalConfigurationXml.BooleanField(A(labels[i]),flags[index]!=0));
                    if(index==1)Raw(stream,OriginalConfigurationXml.BooleanField(A("IsSet"),flags[2]!=0));
                }
                if(option.MaximumValues!=1)Field(stream,"MaximumValues",A(option.MaximumValues.ToString(CultureInfo.InvariantCulture)));
                return stream.ToArray();
            }
        }
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
    }
}
