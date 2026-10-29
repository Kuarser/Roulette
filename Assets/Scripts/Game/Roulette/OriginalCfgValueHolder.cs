using System;
using System.Text;
namespace RouletteDeluxe
{
    public class OriginalCfgValueHolder
    {
        public const int ScalarLength=24; // Whole-object bytes08..1f; independent scalar slots.
        private readonly byte[] scalars,known;private byte[] type=Encoding.ASCII.GetBytes("unknown type"),text=new byte[0];
        private OriginalCfgJurisdictionOption jurisdiction;private bool jurisdictionKnown;
        public OriginalCfgValueHolder(){scalars=new byte[ScalarLength];known=new byte[ScalarLength];}
        public OriginalCfgValueHolder(byte[] initialScalars)
        {if(initialScalars==null||initialScalars.Length!=ScalarLength)throw new ArgumentException("Exact scalar region required");scalars=(byte[])initialScalars.Clone();known=new byte[ScalarLength];for(int i=0;i<known.Length;++i)known[i]=1;}
        public byte[] ScalarBytes{get{return (byte[])scalars.Clone();}}
        public byte[] KnownBytes{get{return (byte[])known.Clone();}}
        public virtual byte[] GetValueType(){return (byte[])type.Clone();}
        public virtual void SetValueType(byte[] value){if(value==null)throw new ArgumentNullException();type=(byte[])value.Clone();}
        private bool Is(string name){byte[] value=GetValueType(),expected=Encoding.ASCII.GetBytes(name);if(value.Length!=expected.Length)return false;for(int i=0;i<value.Length;++i)if(value[i]!=expected[i])return false;return true;}
        private void Mismatch(string prefix){byte[] first=Encoding.ASCII.GetBytes(prefix),last=GetValueType(),message=new byte[first.Length+last.Length];Array.Copy(first,message,first.Length);Array.Copy(last,0,message,first.Length,last.Length);throw new OriginalCfgDictionaryException(message);}
        private void RequireKnown(int offset,int n){for(int i=0;i<n;++i)if(known[offset+i]==0)throw new InvalidOperationException("Native uninitialized scalar bytes require caller-provided storage");}
        private void Write32(int offset,uint value){for(int i=0;i<4;++i){scalars[offset+i]=(byte)(value>>(8*i));known[offset+i]=1;}}
        private uint Read32(int offset){RequireKnown(offset,4);uint result=0;for(int i=0;i<4;++i)result|=(uint)scalars[offset+i]<<(8*i);return result;}
        public virtual void BeInt32(){SetValueType(Encoding.ASCII.GetBytes("int32"));}
        public virtual void BeUInt32(){SetValueType(Encoding.ASCII.GetBytes("unsignedInt32"));}
        public virtual void BeUInt8(){SetValueType(Encoding.ASCII.GetBytes("unsignedInt8"));}
        public virtual void BeString(){SetValueType(Encoding.ASCII.GetBytes("string"));}
        public virtual bool IsInt32(){return Is("int32");}
        public virtual bool IsUInt32(){return Is("unsignedInt32");}
        public virtual bool IsUInt8(){return Is("unsignedInt8");}
        public virtual bool IsString(){return Is("string");}
        public virtual void SetInt32(int value){Write32(0,unchecked((uint)value));BeInt32();}
        public virtual void SetUInt32(uint value){Write32(4,value);BeUInt32();}
        public virtual void SetUInt8(byte value){scalars[8]=value;known[8]=1;BeUInt8();}
        public virtual void SetString(byte[] value){if(value==null)throw new ArgumentNullException();text=(byte[])value.Clone();BeString();}
        public virtual int GetInt32(){if(!IsInt32())Mismatch("expected long not ");return unchecked((int)Read32(0));}
        public virtual uint GetUInt32(){if(!IsUInt32())Mismatch("expected unsigned long not ");return Read32(4);}
        public virtual byte GetUInt8(){if(!IsUInt8())Mismatch("expected unsigned long not ");RequireKnown(8,1);return scalars[8];}
        public virtual byte[] GetString(){if(!IsString())Mismatch("expected std::string not ");return (byte[])text.Clone();}
        public virtual void BeJurisdictionOption(){SetValueType(Encoding.ASCII.GetBytes("JurisdictionOption"));}
        public virtual void BeJurisdictionOptionStr(){SetValueType(Encoding.ASCII.GetBytes("JurisdictionOptionStr"));}
        public virtual void BeJurisdictionOptionBit(){SetValueType(Encoding.ASCII.GetBytes("JurisdictionOptionBit"));}
        public virtual bool IsJurisdictionOption(){return Is("JurisdictionOption");}
        public virtual bool IsJurisdictionOptionStr(){return Is("JurisdictionOptionStr");}
        public virtual bool IsJurisdictionOptionBit(){return Is("JurisdictionOptionBit");}
        public virtual void SetJurisdictionOption(OriginalCfgJurisdictionOption value){jurisdiction=value;jurisdictionKnown=true;BeJurisdictionOption();}
        public virtual void SetJurisdictionOptionStr(OriginalCfgJurisdictionOption value){jurisdiction=value;jurisdictionKnown=true;BeJurisdictionOptionStr();}
        public virtual void SetJurisdictionOptionBit(OriginalCfgJurisdictionOption value){jurisdiction=value;jurisdictionKnown=true;BeJurisdictionOptionBit();}
        private OriginalCfgJurisdictionOption KnownJurisdiction(){if(!jurisdictionKnown)throw new InvalidOperationException("Native uninitialized jurisdiction pointer requires caller-owned storage");return jurisdiction;}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOption(){if(!IsJurisdictionOption())Mismatch("expected JurisdictionOption not ");return KnownJurisdiction();}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOptionStr(){if(!IsJurisdictionOptionStr())Mismatch("expected JurisdictionOptionStr not ");return KnownJurisdiction();}
        public virtual OriginalCfgJurisdictionOption GetJurisdictionOptionBit(){if(!IsJurisdictionOptionBit())Mismatch("expected JurisdictionOptionBit not ");return KnownJurisdiction();}
    }
}
