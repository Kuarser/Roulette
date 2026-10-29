using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
    public sealed class OriginalJurisdictionInputFault:Exception
    {
        public readonly int NativeCode=0x10,NativeLine;
        public readonly string NativeFile="JurisdictionMgr.cpp",NativeFunction;
        public const string NativeFormat="\nFile:%s Line:%d\nFunction:%s\nInvalid Binary Input File\n";
        public OriginalJurisdictionInputFault(string function,int line):base("Invalid Binary Input File")
        {NativeFunction=function;NativeLine=line;}
    }
    // Original JurisdictionMgr scalar/string/bit reads from its 2048-byte input.
    // File and EEPROM acquisition, configuration lookup and registration are caller-owned.
    public sealed class OriginalJurisdictionBinaryReader
    {
        public const int InputLength=2048;
        public sealed class State
        {
            public byte[] Input;
            public object FileOwner;
            public Action<int> InsertOffset;
            public readonly SortedSet<int> UsedOffsets=new SortedSet<int>();
        }
        private readonly State state;
        private readonly Func<byte> usingEeprom;
        private readonly OriginalNativeMemory memory;
        private readonly uint owner;
        public OriginalJurisdictionBinaryReader(State state,Func<byte> usingEeprom)
        {this.state=state??throw new ArgumentNullException(nameof(state));this.usingEeprom=usingEeprom??throw new ArgumentNullException(nameof(usingEeprom));}
        // The production owner may allocate or replace its input after construction.
        // Read the live file and input fields on every operation; never clone input.
        public OriginalJurisdictionBinaryReader(State state,OriginalNativeMemory memory,uint owner)
        {
            this.state=state??throw new ArgumentNullException(nameof(state));
            this.memory=memory??throw new ArgumentNullException(nameof(memory));
            if(owner==0)throw new ArgumentException("Live jurisdiction owner required");
            this.owner=owner;usingEeprom=()=>memory.ReadByte(unchecked(owner+0x34));
        }
        private void Ready(string function,int line)
        {if(usingEeprom()==0&&(memory==null?state.FileOwner==null:memory.ReadWord(unchecked(owner+8))==0))throw new OriginalJurisdictionInputFault(function,line);}
        private struct InputSpan
        {
            internal byte[] Bytes;internal OriginalNativeMemory Memory;internal uint Address;
            internal byte Read(int offset){return Memory==null?Bytes[offset]:Memory.ReadByte(unchecked(Address+(uint)offset));}
        }
        private InputSpan Range(int offset,int length)
        {
            if(memory==null&&(state.Input==null||state.Input.Length!=InputLength))throw new ArgumentException("Actual 2048-byte jurisdiction input required");
            if(offset<0||length<0||offset>InputLength-length)throw new ArgumentOutOfRangeException();
            return new InputSpan{Bytes=state.Input,Memory=memory,Address=memory==null?0:memory.ReadWord(unchecked(owner+0x30))};
        }
        private void Mark(int offset,int length)
        {for(int i=0;i<length;++i){int current=offset+i;state.InsertOffset?.Invoke(current);state.UsedOffsets.Add(current);}}
        private ulong Scalar(int offset,int length)
        {InputSpan input=Range(offset,length);ulong value=0;for(int i=0;i<length;++i)value|=(ulong)input.Read(offset+i)<<(8*i);Mark(offset,length);return value;}
        public byte ReadByte(int offset)
        {Ready("ReadByte",0x2f7);return (byte)Scalar(offset,1);}
        public short ReadShort(int offset)
        {Ready("ReadShort",0x2c0);return unchecked((short)Scalar(offset,2));}
        public int ReadInteger(int offset)
        {Ready("ReadInteger",0x2a6);return unchecked((int)Scalar(offset,4));}
        public long ReadInteger64(int offset)
        {Ready("ReadInteger64",0x2d8);return unchecked((long)Scalar(offset,8));}
        public byte[] ReadString(int offset,int length)
        {
            Ready("ReadString",0x282);InputSpan input=Range(offset,length);
            int count=0;while(count<length&&input.Read(offset+count)!=0)++count;
            byte[] value=new byte[count];for(int i=0;i<count;++i)value[i]=input.Read(offset+i);
            Mark(offset,length);return value;
        }
        public byte ReadBit(int offset,int bit)
        {
            Ready("ReadBit",800);if(unchecked((uint)bit)>=8)return 0;
            InputSpan input=Range(offset,1);byte value=input.Read(offset);Mark(offset,1);
            return (byte)((value&(1<<bit))!=0?1:0);
        }
    }
}
