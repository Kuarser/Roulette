using System;
using System.Text;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Logical normal ConfigurationOption storage and native IncorporateXMLTag behavior.
    // Managed owners do not invent native pointers, allocator padding or EH behavior.
    public sealed class OriginalConfigurationOption:IDisposable
    {
        private sealed class BytesComparer:IComparer<byte[]>{public int Compare(byte[] a,byte[] b){for(int i=0;i<Math.Min(a.Length,b.Length);++i)if(a[i]!=b[i])return a[i].CompareTo(b[i]);return a.Length.CompareTo(b.Length);}}
        public sealed class OrderEntry{public byte[] Key;public uint Order;}
        public sealed class AllowedEntry{public byte[] Key;public byte[][] Rules;}
        private readonly OriginalConfigurationOptionIdentity identity=new OriginalConfigurationOptionIdentity();
        private byte[] value=new byte[0],minimum=new byte[0],maximum=new byte[0],help=new byte[0];
        private readonly byte[] flags=new byte[15];private uint type,control,maximumValues=1;private bool hasMinimum,hasMaximum,hasHelp,disposed;
        private readonly SortedDictionary<byte[],uint> orders=new SortedDictionary<byte[],uint>(new BytesComparer());
        private readonly SortedDictionary<byte[],byte[][]> allowed=new SortedDictionary<byte[],byte[][]>(new BytesComparer());private readonly List<byte[]> rules=new List<byte[]>();
        public OriginalConfigurationOption(){flags[4]=1;flags[9]=1;flags[10]=1;}
        private void Check(){if(disposed)throw new ObjectDisposedException(nameof(OriginalConfigurationOption));}
        private static byte[] Copy(byte[] b){if(b==null)throw new ArgumentNullException();return (byte[])b.Clone();}
        private static byte[] A(string s){return Encoding.ASCII.GetBytes(s);}
        private static bool Eq(byte[] b,string s){if(b.Length!=s.Length)return false;for(int i=0;i<b.Length;++i)if(b[i]!=(byte)s[i])return false;return true;}
        private static byte[][] Copies(byte[][] values){var result=new byte[values.Length][];for(int i=0;i<result.Length;++i)result[i]=Copy(values[i]);return result;}
        public byte[] Category{get{Check();return identity.Category;}}public byte[] Name{get{Check();return identity.Name;}}public uint Hash{get{Check();return identity.Hash;}}
        public byte[] Value{get{Check();return Copy(value);}}public byte[] Minimum{get{Check();return Copy(minimum);}}public byte[] Maximum{get{Check();return Copy(maximum);}}public byte[] Help{get{Check();return Copy(help);}}
        public bool HasMinimum{get{Check();return hasMinimum;}}public bool HasMaximum{get{Check();return hasMaximum;}}public bool HasHelp{get{Check();return hasHelp;}}
        public uint Type{get{Check();return type;}}public uint Control{get{Check();return control;}}public uint MaximumValues{get{Check();return maximumValues;}}
        public byte[] Flags{get{Check();return Copy(flags);}}
        public byte[][] Rules{get{Check();var result=new byte[rules.Count][];for(int i=0;i<result.Length;++i)result[i]=Copy(rules[i]);return result;}}
        public OrderEntry[] Orders{get{Check();var result=new OrderEntry[orders.Count];int i=0;foreach(var pair in orders)result[i++]=new OrderEntry{Key=Copy(pair.Key),Order=pair.Value};return result;}}
        public AllowedEntry[] Allowed{get{Check();var result=new AllowedEntry[allowed.Count];int i=0;foreach(var pair in allowed)result[i++]=new AllowedEntry{Key=Copy(pair.Key),Rules=Copies(pair.Value)};return result;}}
        public void SetName(byte[] raw){Check();identity.SetName(raw);}public void SetCategory(byte[] raw){Check();identity.SetCategory(raw);}
        public void SetValue(byte[] raw){Check();value=Copy(raw);}public void SetType(uint raw){Check();type=raw;}public void SetControl(uint raw){Check();control=raw;}
        public void SetDisplayOrder(byte[] key,uint order){Check();if(order==0)order=unchecked((uint)allowed.Count+1);orders[Copy(key)]=order;}
        public void AddAllowedValue(byte[] key,byte[][] valueRules){Check();if(!allowed.ContainsKey(key))allowed.Add(Copy(key),Copies(valueRules));}
        public void AddRule(byte[] raw){Check();rules.Add(Copy(raw));}
        public void SetIsSet(bool raw){Check();flags[2]=raw?(byte)1:(byte)0;}
        public bool RemoveAllowedValue(byte[] key){Check();if(key==null)throw new ArgumentNullException(nameof(key));return allowed.Remove(key);}
        public byte[][] GetAllowedValues()
        {
            Check();bool ordered=false;uint position;
            foreach(var pair in allowed)if(orders.TryGetValue(pair.Key,out position)&&position!=0){ordered=true;break;}
            var result=new List<byte[]>();
            if(!ordered){foreach(var pair in allowed)result.Add(Copy(pair.Key));}
            else
            {
                // Native scans positions 1..current allowed count. Missing, zero and
                // out-of-range positions disappear; equal positions retain raw-key order.
                for(uint at=1;at<=(uint)allowed.Count;++at)
                    foreach(var pair in allowed)if(orders.TryGetValue(pair.Key,out position)&&position==at)result.Add(Copy(pair.Key));
            }
            return result.ToArray();
        }
        public void Incorporate(byte[] key,byte[] raw,uint order,Func<byte[],uint,uint> extractUInt32=null)
        {
            Check();if(key==null||raw==null)throw new ArgumentNullException();
            if(Eq(key,"Category")){identity.SetCategory(raw);return;}if(Eq(key,"Name")){identity.SetName(raw);return;}
            if(Eq(key,"Value")){value=Copy(raw);return;}
            if(Eq(key,"Type")){type=Eq(raw,"Boolean")?40U:Eq(raw,"Decimal")?20U:Eq(raw,"Integer")?10U:Eq(raw,"String")?30U:0U;return;}
            if(Eq(key,"Minimum")){minimum=Copy(raw);hasMinimum=true;return;}if(Eq(key,"Maximum")){maximum=Copy(raw);hasMaximum=true;return;}if(Eq(key,"OptionHelp")){help=Copy(raw);hasHelp=true;return;}
            if(Eq(key,"Allowed_Value")||Eq(key,"Allowed Value")){SetDisplayOrder(raw,order);AddAllowedValue(raw,new byte[0][]);return;}
            if(Eq(key,"Allowed_Value_Rule")||Eq(key,"Allowed Value Rule")){AddAllowedValue(new byte[0],new[]{Copy(raw)});return;}
            if(Eq(key,"Control_Type")||Eq(key,"Control Type"))
            {string[] names={"Category","Single_Line_Edit_Box","Multi_Line_Edit_Box","Slider","CheckBox","CheckBoxArray","ListBox","ComboBox","RadioButton"};control=0;for(int i=0;i<names.Length;++i)if(Eq(raw,names[i]))control=(uint)((i+1)*10);return;}
            if(Eq(key,"Rule")){AddRule(raw);return;}
            string[] namesForFlags={"ReadOnly","OneTimeSettable","IsSet","ReadOnlyWithCredits","Visible","RestrictToAllowedValues","UniquePerMachine",null,null,"RemotelySettable","LocallySettable","Volatile","SaveSpaceIfEmpty"};
            for(int i=0;i<namesForFlags.Length;++i)if((namesForFlags[i]!=null&&Eq(key,namesForFlags[i]))||(i==11&&Eq(key,"Volitile"))){flags[i]=(byte)(Eq(raw,"true")?1:0);return;}
            if(Eq(key,"MaximumValues")){if(extractUInt32==null)throw new InvalidOperationException("Genuine native uint32 stream extraction required");maximumValues=extractUInt32(Copy(raw),1);}
            // Native unknown tags do nothing, including differently cased or NUL-suffixed keys.
        }
        public OriginalConfigurationOption DeepCopy()
        {
            Check();var copy=new OriginalConfigurationOption();copy.identity.SetCategory(identity.Category);copy.identity.SetName(identity.Name);copy.value=Copy(value);copy.minimum=Copy(minimum);copy.maximum=Copy(maximum);copy.help=Copy(help);copy.type=type;copy.control=control;copy.maximumValues=maximumValues;copy.hasMinimum=hasMinimum;copy.hasMaximum=hasMaximum;copy.hasHelp=hasHelp;Array.Copy(flags,copy.flags,flags.Length);
            foreach(var pair in orders)copy.orders.Add(Copy(pair.Key),pair.Value);foreach(var pair in allowed)copy.allowed.Add(Copy(pair.Key),Copies(pair.Value));foreach(var rule in rules)copy.rules.Add(Copy(rule));return copy;
        }
        public void Dispose(){Check();disposed=true;}
        public sealed class Vector
        {
            private readonly List<OriginalConfigurationOption> values=new List<OriginalConfigurationOption>();
            public OriginalConfigurationOption[] Snapshot(){var result=new OriginalConfigurationOption[values.Count];for(int i=0;i<result.Length;++i)result[i]=values[i].DeepCopy();return result;}
            public OriginalConfigurationTemplateScanner BindScanner(OriginalCfgIntegerConverter.State errno,Func<byte[],uint,uint> extractUInt32=null)
            {return new OriginalConfigurationTemplateScanner(new OriginalConfigurationTemplateScanner.Host{Count=()=>values.Count,Create=()=>new OriginalConfigurationOption(),AppendCopy=o=>values.Add(((OriginalConfigurationOption)o).DeepCopy()),Delete=o=>((OriginalConfigurationOption)o).Dispose(),Incorporate=(o,k,v,n)=>((OriginalConfigurationOption)o).Incorporate(k,v,n,extractUInt32)},errno);}
        }
    }
}
