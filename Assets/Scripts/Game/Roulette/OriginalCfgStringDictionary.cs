using System;
using System.Text;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public sealed class OriginalCfgDictionaryException:Exception
    {
        public readonly byte[] NativeMessage;
        public OriginalCfgDictionaryException(byte[] message):base("Native CFG dictionary exception"){NativeMessage=(byte[])message.Clone();}
    }
    // CFGConstantsDictionary string/integer portion + Hybrid root-leaf extension.
    // Remaining selected native typed values require genuine caller handlers.
    public class OriginalCfgStringDictionary
    {
        public sealed class Entry{public byte[] Key,Value;}
        private sealed class Comparer:IComparer<byte[]>
        {
            public int Compare(byte[] a,byte[] b){for(int i=0;i<Math.Min(a.Length,b.Length);++i)if(a[i]!=b[i])return a[i].CompareTo(b[i]);return a.Length.CompareTo(b.Length);}
        }
        private readonly SortedDictionary<byte[],OriginalCfgValueHolder> values=new SortedDictionary<byte[],OriginalCfgValueHolder>(new Comparer());
        private static byte[] Copy(byte[] b){if(b==null)throw new ArgumentNullException();return (byte[])b.Clone();}
        private static byte[] A(string s){return Encoding.ASCII.GetBytes(s);}
        public int Count{get{return values.Count;}}
        public virtual bool HasKey(byte[] key){if(key==null)throw new ArgumentNullException();return values.ContainsKey(key);}
        public virtual void PutValueHolder(byte[] key,OriginalCfgValueHolder value){values[Copy(key)]=value;}
        public void PutString(byte[] key,byte[] value){var holder=new OriginalCfgValueHolder();holder.SetString(value);PutValueHolder(key,holder);}
        public virtual OriginalCfgValueHolder GetValueHolder(byte[] key)
        {
            if(!HasKey(key))
            {byte[] suffix=A(" is not in the dictionary"),message=new byte[key.Length+suffix.Length];Array.Copy(key,message,key.Length);Array.Copy(suffix,0,message,key.Length,suffix.Length);throw new OriginalCfgDictionaryException(message);}
            OriginalCfgValueHolder value;if(!values.TryGetValue(key,out value))values.Add(Copy(key),null);return value;
        }
        public virtual byte[] GetString(byte[] key){return GetValueHolder(key).GetString();}
        private T ReadInteger<T>(byte[] key,Func<OriginalCfgValueHolder,T> read)
        {
            OriginalCfgValueHolder holder=GetValueHolder(key); // Native lookup is outside the getter catch region.
            try{return read(holder);}catch(OriginalCfgDictionaryException error)
            {byte[] middle=A(" at key "),message=new byte[error.NativeMessage.Length+middle.Length+key.Length];int at=0;foreach(byte[] part in new[]{error.NativeMessage,middle,key}){Array.Copy(part,0,message,at,part.Length);at+=part.Length;}throw new OriginalCfgDictionaryException(message);}
        }
        public int GetInt32(byte[] key){return ReadInteger(key,h=>h.GetInt32());}
        public uint GetUInt32(byte[] key){return ReadInteger(key,h=>h.GetUInt32());}
        public byte GetUInt8(byte[] key){return ReadInteger(key,h=>h.GetUInt8());}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOption(byte[] key){return ReadInteger(key,h=>h.GetJurisdictionOption());}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOptionStr(byte[] key){return ReadInteger(key,h=>h.GetJurisdictionOptionStr());}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOptionBit(byte[] key){return ReadInteger(key,h=>h.GetJurisdictionOptionBit());}
        public byte[][] Keys(){var result=new byte[Count][];int i=0;foreach(var key in values.Keys)result[i++]=Copy(key);return result;}
        public Entry[] Snapshot(){var result=new Entry[Count];int i=0;foreach(var pair in values)result[i++]=new Entry{Key=Copy(pair.Key),Value=pair.Value.GetString()};return result;}
        private static byte[] TrimSpaces(byte[] b){int first=0,last=b.Length;while(first<last&&b[first]==32)++first;while(last>first&&b[last-1]==32)--last;var result=new byte[last-first];Array.Copy(b,first,result,0,result.Length);return result;}
        public void AddString(OriginalCfgDocument.Node node){byte[] key=node.Attribute(A("name"));byte[] value=node.Attribute(A("value"));PutString(key,value);}
        public void AddInteger(string type,OriginalCfgDocument.Node node,OriginalCfgIntegerConverter.State state)
        {
            if(state==null)throw new ArgumentNullException();var holder=new OriginalCfgValueHolder();byte[] key=node.Attribute(A("name")),value=node.Attribute(A("value"));
            string suffix=type=="int32"?" to long":type=="unsignedInt32"?" to unsigned long":type=="unsignedInt8"?" to unsigned char":null;if(suffix==null)throw new ArgumentException("Native integer type required");
            byte[] prefix=A("Problem converting value "),middle=A(" for key "),tail=A(suffix),message=new byte[prefix.Length+value.Length+middle.Length+key.Length+tail.Length];int at=0;
            foreach(byte[] part in new[]{prefix,value,middle,key,tail}){Array.Copy(part,0,message,at,part.Length);at+=part.Length;}
            if(type=="int32")holder.SetInt32(OriginalCfgIntegerConverter.ToInt32(value,message,state));else if(type=="unsignedInt32")holder.SetUInt32(OriginalCfgIntegerConverter.ToUInt32(value,message,state));else holder.SetUInt8(OriginalCfgIntegerConverter.ToUInt8(value,message,state));
            PutValueHolder(key,holder);
        }
        public void AddSymbolsFrom(OriginalCfgDocument.Node root,Action<string,OriginalCfgDocument.Node,OriginalCfgStringDictionary> addOtherType)
        {AddSymbolsFrom(root,null,addOtherType);}
        public void AddSymbolsFrom(OriginalCfgDocument.Node root,OriginalCfgIntegerConverter.State integerState,Action<string,OriginalCfgDocument.Node,OriginalCfgStringDictionary> addOtherType)
        {
            if(root==null)throw new ArgumentNullException();
            // Native base dictionary dispatches these descendant selectors in order.
            foreach(string type in new[]{"int32","unsignedInt32","unsignedInt8","string","double","float","RGBColor","JurisdictionOptionInt","JurisdictionOptionInt64","JurisdictionOptionStr","JurisdictionOptionBit"})
            {
                var iterator=root.SelectByName(A(type));
                while(iterator.HasNext())
                {
                    var node=iterator.Next();if(type=="string")AddString(node);
                    else if(integerState!=null&&(type=="int32"||type=="unsignedInt32"||type=="unsignedInt8"))AddInteger(type,node,integerState);
                    else if(integerState!=null&&type.StartsWith("JurisdictionOption",StringComparison.Ordinal))AddJurisdiction(type,node,integerState);
                    else{if(addOtherType==null)throw new InvalidOperationException("Native typed CFG handler required: "+type);addOtherType(type,node,this);}
                }
                iterator.Delete(); // Native success path only; callback faults preserve partial values.
            }
            var children=root.GetChildren();
            while(children.HasNext())
            {
                var node=children.Next();if(node.HasChildren()||node.HasAttributes())continue;
                byte[] key=node.TagName(),value=node.Text();PutString(key,TrimSpaces(value));
            }
            children.Delete();
        }
        private static byte[] ConversionMessage(string prefix,byte[] value,byte[] key)
        {byte[][] parts={A(prefix),value,A(" for key "),key,A(" to long")};int length=0;foreach(var p in parts)length+=p.Length;var message=new byte[length];int at=0;foreach(var p in parts){Array.Copy(p,0,message,at,p.Length);at+=p.Length;}return message;}
        public void AddJurisdiction(string type,OriginalCfgDocument.Node node,OriginalCfgIntegerConverter.State state)
        {
            if(state==null||node==null)throw new ArgumentNullException();
            bool integer=type=="JurisdictionOptionInt"||type=="JurisdictionOptionInt64",str=type=="JurisdictionOptionStr",bit=type=="JurisdictionOptionBit";
            if(!integer&&!str&&!bit)throw new ArgumentException("Native jurisdiction type required");
            var holder=new OriginalCfgValueHolder();byte[] key=node.Attribute(A("name")),offset=node.Attribute(A("offset"));
            int converted=OriginalCfgIntegerConverter.ToInt32(offset,ConversionMessage(integer?"Problem converting value ":"Problem converting offset ",offset,key),state);
            if(integer)holder.SetJurisdictionOption(new OriginalCfgJurisdictionOption(converted));
            else
            {
                byte[] argument=node.Attribute(A(str?"size":"bit"));int second=OriginalCfgIntegerConverter.ToInt32(argument,ConversionMessage(str?"Problem converting size ":"Problem converting bit ",argument,key),state);
                var option=new OriginalCfgJurisdictionOption(converted,second);if(str)holder.SetJurisdictionOptionStr(option);else holder.SetJurisdictionOptionBit(option);
            }
            PutValueHolder(key,holder);
        }
    }
}
