using System;
using System.Text;
namespace RouletteDeluxe
{
    public sealed class OriginalNv2Validation
    {
        public sealed class Host
        {
            public Func<bool> HasStorage;public Action AssertStorage;
            public Func<uint> GetStorageSize;
            public Action<uint,OriginalConfigurationNvData.ByteSlot[]> Read;
            // Native Validate uses malloc/free, independently from copy new[]/delete[].
            public Func<uint,OriginalConfigurationNvData.ByteSlot[]> Allocate;
            public Action<OriginalConfigurationNvData.ByteSlot[]> Free;
            public Func<OriginalMemStream> CreateStream;
            public Action<OriginalMemStream> WriteData,DeleteStream;
            public Func<byte[],byte[]> Describe;public Func<byte[]> GetName;
            public Action<byte[],uint,byte[]> NvFault;
            public Func<byte> SoftEnabled;public Action<byte[]> StorageSoftValidate;
        }
        private readonly OriginalNv2Copies.State state;private readonly Host host;
        public OriginalNv2Validation(OriginalNv2Copies.State state,Host h)
        {
            if(state==null||h==null||h.HasStorage==null||h.AssertStorage==null||h.GetStorageSize==null||h.Read==null||h.Allocate==null||h.Free==null||h.CreateStream==null||h.WriteData==null||h.DeleteStream==null||h.Describe==null||h.GetName==null||h.NvFault==null||h.SoftEnabled==null||h.StorageSoftValidate==null)throw new ArgumentException("Actual NV validation/storage/malloc/stream/fault/global bindings required");
            this.state=state;host=new Host{HasStorage=h.HasStorage,AssertStorage=h.AssertStorage,GetStorageSize=h.GetStorageSize,Read=h.Read,Allocate=h.Allocate,Free=h.Free,CreateStream=h.CreateStream,WriteData=h.WriteData,DeleteStream=h.DeleteStream,Describe=h.Describe,GetName=h.GetName,NvFault=h.NvFault,SoftEnabled=h.SoftEnabled,StorageSoftValidate=h.StorageSoftValidate};
        }
        private void Assert(){if(!host.HasStorage()){host.AssertStorage();throw new InvalidOperationException("Native nonreturning storage assertion returned");}}
        private void Fault(string message){var description=host.Describe(Encoding.ASCII.GetBytes(message));var name=host.GetName();host.NvFault(name,2,description);throw new InvalidOperationException("Native nonreturning NVFault binding returned");}
        private static uint Word(byte[] h,int at,int n){uint r=0;for(int i=0;i<n;i++)r|=(uint)h[at+i]<<(8*i);return r;}
        private static OriginalConfigurationNvData.ByteSlot[] Unknown(uint n){var r=new OriginalConfigurationNvData.ByteSlot[checked((int)n)];for(int i=0;i<r.Length;i++)r[i]=new OriginalConfigurationNvData.ByteSlot(null);return r;}
        private static byte[] Known(OriginalConfigurationNvData.ByteSlot[] bytes,uint n)
        {if(n==0)return new byte[0];if(bytes==null||n>(uint)bytes.Length)throw new InvalidOperationException("Native validation memory requires admitted readable storage");var r=new byte[checked((int)n)];for(int i=0;i<r.Length;i++){if(bytes[i]==null||!bytes[i].Value.HasValue)throw new InvalidOperationException("Native validation byte requires caller-known storage");r[i]=bytes[i].Value.Value;}return r;}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
        private static ushort Crc(byte[] b){return OriginalNvCrc16.Calculate(b,0,b.Length);}
        public void Validate()
        {
            Assert();uint size=host.GetStorageSize();var frame=Unknown(20);host.Read(state.HeaderOffset,frame);
            if(state.Status!=1)Fault("NV2::Validate: File state corrupted");
            var h=Known(frame,20);if(!Equal(state.Header,h))Fault("NV2::Validate File Hdr != valHdr");
            uint first=Word(h,4,4),second=Word(h,8,4),allocated=Word(h,12,4);
            if(size!=unchecked(allocated+second+20))Fault("NV2::Validate: File size corrupted");
            var a=host.Allocate(first);var b=host.Allocate(second);
            host.Read(state.DataOffset,a);host.Read(unchecked(state.DataOffset+allocated),b);
            var stream=host.CreateStream();if(stream==null)throw new InvalidOperationException("Actual MemStream constructor owner required");host.WriteData(stream);var serialized=stream.Buffer;
            if(first!=second||first!=stream.Size)Fault("NV2::Validate: File Headers size corrupted");
            var bytesA=Known(a,first);if(Crc(bytesA)!=(ushort)Word(h,16,2))Fault("NV2::Validate: Data1 checksum failed");
            var bytesB=Known(b,second);if(Crc(bytesB)!=(ushort)Word(h,18,2))Fault("NV2::Validate: Data2 checksum failed");
            if(!Equal(bytesA,bytesB))Fault("NV2::Validate: File data1 != data2");
            if(!Equal(bytesA,Known(serialized,first)))Fault("NV2::Validate: File RAM != NVRAM");
            host.Free(a);host.Free(b);host.DeleteStream(stream);
        }
        public void SoftValidate(byte[] path)
        {
            if(host.SoftEnabled()==0)return;
            if(path==null)throw new ArgumentNullException("path");host.StorageSoftValidate((byte[])path.Clone());Assert();uint size=host.GetStorageSize();
            if(state.Status!=1)Fault("NV2::SoftValidate: File state corrupted");
            if(size!=unchecked(state.AllocatedSize+state.Size2+20))Fault("NV2::SoftValidate: File size corrupted");
            var stream=host.CreateStream();if(stream==null)throw new InvalidOperationException("Actual MemStream constructor owner required");host.WriteData(stream);var serialized=stream.Buffer;
            if(state.Size1!=state.Size2||state.Size1!=stream.Size)Fault("NV2::SoftValidate: File Headers size corrupted");
            if(Crc(Known(serialized,state.Size1))!=state.Crc1)Fault("NV2::SoftValidate: Data1 checksum failed");
            host.DeleteStream(stream);
        }
    }
}
