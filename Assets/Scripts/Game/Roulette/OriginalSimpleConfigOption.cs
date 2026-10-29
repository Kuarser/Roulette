using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // Native logical 16-byte SimpleConfigOption fields: name, value, category, hash.
    // Categories are raw here; ConfigurationOption's category cleanup does not apply.
    public sealed class OriginalSimpleConfigOption
    {
        private byte[] name,value,category;private uint hash;
        private static byte[] Copy(byte[] raw){if(raw==null)throw new ArgumentNullException(nameof(raw));return (byte[])raw.Clone();}
        public OriginalSimpleConfigOption(byte[] name,byte[] category,byte[] value)
        {this.name=Copy(name);this.value=Copy(value);this.category=Copy(category);ReHash();}
        public byte[] Name{get{return Copy(name);}}public byte[] Value{get{return Copy(value);}}public byte[] Category{get{return Copy(category);}}public uint Hash{get{return hash;}}
        private void ReHash()
        {hash=0;foreach(var part in new[]{name,category})foreach(byte b in part)hash=unchecked(((hash<<3)|(hash>>29))^(uint)(int)(sbyte)b);}
        public void SetName(byte[] raw){name=Copy(raw);ReHash();}
        public void SetCategory(byte[] raw){category=Copy(raw);ReHash();}
        public void SetValue(byte[] raw){value=Copy(raw);}
        public OriginalSimpleConfigOption DeepCopy(){var result=new OriginalSimpleConfigOption(name,category,value);result.hash=hash;return result;}
        public static OriginalSimpleConfigOption FromConfiguration(OriginalConfigurationOption option)
        {if(option==null)throw new ArgumentNullException(nameof(option));return new OriginalSimpleConfigOption(option.Name,option.Category,option.Value);}
        private static bool Equal(byte[] a,byte[] b)
        {if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        public bool SameIdentity(OriginalSimpleConfigOption other)
        {if(other==null)throw new ArgumentNullException(nameof(other));return hash==other.hash&&Equal(name,other.name)&&Equal(category,other.category);}
        public sealed class NativeComparer:IComparer<OriginalSimpleConfigOption>
        {
            public int Compare(OriginalSimpleConfigOption a,OriginalSimpleConfigOption b)
            {
                if(a==null||b==null)throw new ArgumentNullException();
                if(a.hash!=b.hash)return a.hash<b.hash?-1:1;
                // Original operator< uses raw category+name concatenations, with no separator.
                int al=checked(a.category.Length+a.name.Length),bl=checked(b.category.Length+b.name.Length);
                for(int i=0;i<Math.Min(al,bl);++i)
                {byte av=i<a.category.Length?a.category[i]:a.name[i-a.category.Length];byte bv=i<b.category.Length?b.category[i]:b.name[i-b.category.Length];if(av!=bv)return av<bv?-1:1;}
                return al.CompareTo(bl);
            }
        }
    }
}
