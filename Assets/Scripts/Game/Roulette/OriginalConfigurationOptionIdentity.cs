using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    // ConfigurationOption identity members only: category04, name08, hash2c.
    // Values, flags, allowed values/rules and ConfigServer registration are separate owners.
    public sealed class OriginalConfigurationOptionIdentity
    {
        private byte[] category,name;private uint hash;
        public OriginalConfigurationOptionIdentity():this(new byte[0],new byte[0]){}
        public OriginalConfigurationOptionIdentity(byte[] category,byte[] name)
        {this.category=Copy(category);this.name=Copy(name);CleanUpCategory();}
        private static byte[] Copy(byte[] b){if(b==null)throw new ArgumentNullException();return (byte[])b.Clone();}
        public byte[] Category{get{return Copy(category);}}
        public byte[] Name{get{return Copy(name);}}
        public uint Hash{get{return hash;}}
        private static bool Space(byte b){return b==32||(b>=9&&b<=13);}
        public void SetName(byte[] value){name=Copy(value);ReHash();}
        public void SetCategory(byte[] value){category=Copy(value);CleanUpCategory();}
        public void CleanUpCategory()
        {
            // Explicit ASCII C-locale category admission; names may contain any byte.
            foreach(byte b in category)if(b>127)throw new ArgumentException("ASCII C-locale category required");
            int first=0;while(first<category.Length&&Space(category[first]))++first;
            var clean=new List<byte>();
            for(int i=first;i<category.Length;++i)
            {byte b=category[i]==92?(byte)47:category[i];if(b==47&&clean.Count>0&&clean[clean.Count-1]==47)continue;clean.Add(b);}
            while(clean.Count>0&&(Space(clean[clean.Count-1])||clean[clean.Count-1]==47))clean.RemoveAt(clean.Count-1);
            if(clean.Count==0||clean[0]!=47)clean.Insert(0,47);
            category=clean.ToArray();ReHash();
        }
        public void ReHash()
        {
            hash=0;
            foreach(byte[] raw in new[]{name,category})foreach(byte b in raw)
            {hash=(hash<<3)|(hash>>29);hash^=unchecked((uint)(int)(sbyte)b);}
        }
    }
}
