using System;
using System.Text;
namespace RouletteDeluxe
{
    // Native SetState and Copy1to2/Copy2to1. Actual allocation, storage and
    // NVFault remain mandatory bindings; no file or financial owner is invented.
    public sealed class OriginalNv2Copies
    {
        public sealed class State
        {
            private readonly byte[] header;
            public readonly uint HeaderOffset,StateOffset,DataOffset;
            public State(byte[] header,uint headerOffset,uint stateOffset,uint dataOffset)
            {if(header==null||header.Length!=20)throw new ArgumentException("Actual admitted twenty-byte NV header required");this.header=(byte[])header.Clone();HeaderOffset=headerOffset;StateOffset=stateOffset;DataOffset=dataOffset;}
            public byte[] Header{get{return (byte[])header.Clone();}}
            public byte Status{get{return header[0];}internal set{header[0]=value;}}
            internal uint Word(int at,int count){uint result=0;for(int i=0;i<count;++i)result|=(uint)header[at+i]<<(8*i);return result;}
            internal void Word(int at,int count,uint value){for(int i=0;i<count;++i)header[at+i]=(byte)(value>>(8*i));}
            public uint Size1{get{return Word(4,4);}}public uint Size2{get{return Word(8,4);}}public uint AllocatedSize{get{return Word(12,4);}}
            public ushort Crc1{get{return (ushort)Word(16,2);}}public ushort Crc2{get{return (ushort)Word(18,2);}}
        }
        public sealed class Host
        {
            public Func<uint,OriginalConfigurationNvData.ByteSlot[]> Allocate;
            public Action<OriginalConfigurationNvData.ByteSlot[]> Free;
            public Action<uint,OriginalConfigurationNvData.ByteSlot[]> Read;
            public Action<uint,byte[]> Write;
            public Action<uint> Resize;
            public Func<byte[],byte[]> Describe;
            public Func<byte[]> GetName;
            public Action<byte[],uint,byte[]> NvFault;
        }
        private readonly State state;private readonly Host host;
        public OriginalNv2Copies(State state,Host host)
        {
            if(state==null||host==null||host.Allocate==null||host.Free==null||host.Read==null||host.Write==null||host.Resize==null||host.Describe==null||host.GetName==null||host.NvFault==null)throw new ArgumentException("Genuine NV state, allocation, storage and fault bindings required");
            this.state=state;this.host=new Host{Allocate=host.Allocate,Free=host.Free,Read=host.Read,Write=host.Write,Resize=host.Resize,Describe=host.Describe,GetName=host.GetName,NvFault=host.NvFault};
        }
        public void SetState(int value){state.Status=unchecked((byte)value);host.Write(state.StateOffset,new[]{state.Status});}
        private static byte[] Prefix(byte[] bytes,uint length)
        {if(bytes==null||length>(uint)bytes.Length)throw new InvalidOperationException("Native data pointer requires admitted readable bytes");var result=new byte[checked((int)length)];Array.Copy(bytes,result,result.Length);return result;}
        private byte[] Read(uint offset,uint length,ushort expected,string message,out OriginalConfigurationNvData.ByteSlot[] allocated)
        {
            allocated=host.Allocate(length);if(allocated==null||allocated.Length!=length)throw new InvalidOperationException("Actual native allocation and byte-frame admission required");host.Read(offset,allocated);
            var bytes=new byte[allocated.Length];for(int i=0;i<bytes.Length;++i){if(allocated[i]==null||!allocated[i].Value.HasValue)throw new InvalidOperationException("Native unread allocation byte requires caller-known storage");bytes[i]=allocated[i].Value.Value;}
            if(OriginalNvCrc16.Calculate(bytes,0,bytes.Length)!=expected)
            {var description=host.Describe(Encoding.ASCII.GetBytes(message));var name=host.GetName();host.NvFault(name,2,description);throw new InvalidOperationException("Native nonreturning NVFault binding returned");}
            return bytes;
        }
        public void Copy1To2(byte[] provided)
        {
            if(state.Status!=6)SetState(6);OriginalConfigurationNvData.ByteSlot[] allocated=null;
            byte[] bytes=provided==null?Read(state.DataOffset,state.Size1,state.Crc1,"NV2::Copy1to2: Data1 checksums failed",out allocated):Prefix(provided,state.Size1);
            state.Word(18,2,state.Crc1);uint size=state.Size1;state.Word(8,4,size);state.Word(12,4,size);
            host.Write(unchecked(state.DataOffset+size),bytes);host.Write(state.HeaderOffset,state.Header);
            if(provided==null)host.Free(allocated);host.Resize(unchecked(size*2+20));SetState(1);
        }
        public void Copy2To1(byte[] provided)
        {
            if(state.Status!=7)SetState(7);OriginalConfigurationNvData.ByteSlot[] allocated=null;
            byte[] bytes=provided==null?Read(unchecked(state.DataOffset+state.AllocatedSize),state.Size2,state.Crc2,"NV2::Copy2to1: Data2 checksums failed",out allocated):Prefix(provided,state.Size2);
            state.Word(16,2,state.Crc2);state.Word(4,4,state.Size2);
            host.Write(state.DataOffset,bytes);host.Write(state.HeaderOffset,state.Header);
            if(provided==null)host.Free(allocated);SetState(1);
        }
    }
}
