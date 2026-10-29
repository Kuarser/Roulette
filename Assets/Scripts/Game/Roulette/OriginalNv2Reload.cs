using System;
using System.Text;
namespace RouletteDeluxe
{
    public sealed class OriginalNv2Reload
    {
        public sealed class Host
        {
            public OriginalNv2Copies.Host Storage;
            public Func<bool> HasStorage;public Action AssertStorage;
            public Func<uint> GetStorageSize;
            public Action<uint,OriginalConfigurationNvData.ByteSlot[],uint> Read;
            // Must construct MemStream(NULL,size,0,1), including NULL buffer for size0.
            public Func<uint,OriginalMemStream> CreateStream;
            public Action<OriginalMemStream> WriteData,ReadData,DeleteStream;
            public Action<OriginalNv2Save.Cache> DeleteCached;
            public Action<byte[]> LogRecovery;
        }
        private readonly OriginalNv2Save.State state;private readonly Host host;private readonly OriginalNv2Copies copies;
        public OriginalNv2Reload(OriginalNv2Save.State state,Host h)
        {
            if(state==null||h==null||h.Storage==null||h.HasStorage==null||h.AssertStorage==null||h.GetStorageSize==null||h.Read==null||h.CreateStream==null||h.WriteData==null||h.ReadData==null||h.DeleteStream==null||h.DeleteCached==null||h.LogRecovery==null)throw new ArgumentException("Actual NV reload/storage/stream/lifetime/recovery-log bindings required");
            var s=h.Storage;var storage=new OriginalNv2Copies.Host{Allocate=s.Allocate,Free=s.Free,Read=s.Read,Write=s.Write,Resize=s.Resize,Describe=s.Describe,GetName=s.GetName,NvFault=s.NvFault};copies=new OriginalNv2Copies(state.Header,storage);
            this.state=state;host=new Host{Storage=storage,HasStorage=h.HasStorage,AssertStorage=h.AssertStorage,GetStorageSize=h.GetStorageSize,Read=h.Read,CreateStream=h.CreateStream,WriteData=h.WriteData,ReadData=h.ReadData,DeleteStream=h.DeleteStream,DeleteCached=h.DeleteCached,LogRecovery=h.LogRecovery};
        }
        private void Fault(string message){var description=host.Storage.Describe(Encoding.ASCII.GetBytes(message));var name=host.Storage.GetName();host.Storage.NvFault(name,2,description);throw new InvalidOperationException("Native nonreturning NVFault binding returned");}
        private static byte[] Known(OriginalConfigurationNvData.ByteSlot[] frame,uint size)
        {if(size==0)return new byte[0];if(frame==null||size>(uint)frame.Length)throw new InvalidOperationException("Native reload memory requires admitted readable storage");var bytes=new byte[checked((int)size)];for(int i=0;i<bytes.Length;++i){if(frame[i]==null||!frame[i].Value.HasValue)throw new InvalidOperationException("Native reload byte requires caller-known storage");bytes[i]=frame[i].Value.Value;}return bytes;}
        private static ushort Crc(byte[] bytes){return OriginalNvCrc16.Calculate(bytes,0,bytes.Length);}
        private static byte[] Provided(OriginalConfigurationNvData.ByteSlot[] frame,uint size){return frame==null?null:Known(frame,size);}
        private static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;++i)if(a[i]!=b[i])return false;return true;}
        private void ReadHeader()
        {
            var raw=state.Header.Header;var frame=new OriginalConfigurationNvData.ByteSlot[20];for(int i=0;i<20;++i)frame[i]=new OriginalConfigurationNvData.ByteSlot(raw[i]);
            try{host.Read(state.Header.HeaderOffset,frame,20);}
            finally{for(int i=0;i<20;++i){if(frame[i]==null||!frame[i].Value.HasValue)throw new InvalidOperationException("Native reload header byte requires caller-known storage");state.Header.Word(i,1,frame[i].Value.Value);}}
        }
        private void Pair(out OriginalMemStream a,out OriginalMemStream b)
        {
            a=host.CreateStream(state.Header.Size1);b=host.CreateStream(state.Header.Size2);
            if(a==null||b==null)Fault("NV2::Open: Data1&2 checksums failed");
            host.Read(state.Header.DataOffset,a.Buffer,state.Header.Size1);
            host.Read(unchecked(state.Header.DataOffset+state.Header.AllocatedSize),b.Buffer,state.Header.Size2);
        }
        private void Initialize()
        {
            var stream=host.CreateStream(0);if(stream==null)throw new InvalidOperationException("Actual MemStream owner required");host.WriteData(stream);uint size=stream.Size;var h=state.Header;
            h.Word(12,4,size);h.Word(8,4,size);h.Word(4,4,size);var frame=stream.Buffer;h.Word(1,1,165);byte[] bytes=Known(frame,size);ushort crc=Crc(bytes);h.Word(18,2,crc);h.Word(16,2,crc);
            host.Storage.Resize(unchecked(size*2+20));host.Storage.Write(h.DataOffset,bytes);host.Storage.Write(unchecked(h.DataOffset+h.AllocatedSize),bytes);host.Storage.Write(h.HeaderOffset,h.Header);copies.SetState(1);host.DeleteStream(stream);
        }
        public void Reload()
        {
            if(!host.HasStorage()){host.AssertStorage();throw new InvalidOperationException("Native nonreturning storage assertion returned");}
            uint fileSize=host.GetStorageSize();var h=state.Header;
            if(fileSize<20){fileSize=20;for(int i=0;i<20;i+=4)h.Word(i,4,0);}else ReadHeader();
            if(fileSize<unchecked(h.Size1+h.Size2+20))Fault("NV2::Open: File size corrupted");
            if(h.Status>7)Fault("NV2::Open: File state corrupted");
            if(h.Status!=0&&h.Header[1]!=165)Fault("NV2::Open: File reserved byte corrupted");
            state.ReloadRequested=1;if(state.Cached!=null){host.DeleteCached(state.Cached);state.Cached=null;}
            while(true)
            {
                byte status=h.Status;
                if(status==2||status==7){copies.Copy2To1(null);continue;}
                if(status==3||status==6){copies.Copy1To2(null);continue;}
                if(status!=1&&status!=4&&status!=5){Initialize();return;}
                OriginalMemStream a,b;Pair(out a,out b);var frameA=a.Buffer;var frameB=b.Buffer;
                byte[] bytesA=Known(frameA,h.Size1);bool validA=Crc(bytesA)==h.Crc1;
                if(status==4||status==5)
                {
                    if(!validA)Fault("NV2::Open: Data1 checksums failed for new1/2 state");
                    if(Crc(Known(frameB,h.Size2))!=h.Crc2)Fault("NV2::Open: Data2 checksums failed for new1/2 state");
                    host.ReadData(h.Status==4?b:a);host.DeleteStream(a);host.DeleteStream(b);return;
                }
                bool validB=Crc(Known(frameB,h.Size2))==h.Crc2;
                if(validA&&validB)
                {
                    if(!Equal(bytesA,Known(frameB,h.Size1)))Fault("NV2::Open: Data mismatch in idle state");
                    host.DeleteStream(b);host.ReadData(a);state.Cached=a.SaveCache();return;
                }
                if(!validA&&!validB)Fault("NV2::Open: Data1&2 checksums failed idle state");
                if(validA){host.LogRecovery(Encoding.ASCII.GetBytes("NVCP2"));copies.Copy1To2(Provided(frameA,h.Size1));}
                else{host.LogRecovery(Encoding.ASCII.GetBytes("NVCP1"));copies.Copy2To1(Provided(frameB,h.Size2));}
                host.DeleteStream(a);host.DeleteStream(b);
            }
        }
    }
}
