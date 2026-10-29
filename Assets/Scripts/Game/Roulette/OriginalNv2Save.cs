using System;
using System.Text;
namespace RouletteDeluxe
{
    // NVConfigOptionData's admitted inherited NV2 Save/Save1/Save2 slots.
    // MemStream and storage operations must be bound to their actual owners.
    public sealed class OriginalNv2Save
    {
        public sealed class Cache
        {
            public readonly Func<uint> GetMode;
            public readonly Action ClearPosition,ClearLength;
            public readonly Func<uint> GetSize;
            public readonly Func<byte[]> GetData;
            public Cache(Func<uint> getMode,Action clearPosition,Action clearLength,Func<uint> getSize,Func<byte[]> getData)
            {if(getMode==null||clearPosition==null||clearLength==null||getSize==null||getData==null)throw new ArgumentException("Actual MemStream owner bindings required");GetMode=getMode;ClearPosition=clearPosition;ClearLength=clearLength;GetSize=getSize;GetData=getData;}
        }
        public sealed class State
        {
            public readonly OriginalNv2Copies.State Header;
            public byte Busy,ReloadRequested;
            public Cache Cached;
            public State(OriginalNv2Copies.State header,byte busy,byte reloadRequested,Cache cached)
            {if(header==null)throw new ArgumentNullException("header");Header=header;Busy=busy;ReloadRequested=reloadRequested;Cached=cached;}
        }
        public sealed class Host
        {
            public OriginalNv2Copies.Host Storage;
            public Func<bool> HasStorage;
            public Action AssertStorage;
            // Must construct the native MemStream(NULL,0,0,1) profile.
            public Func<Cache> CreateCache;
            public Action<Cache> WriteData;
            public Action Reload;
        }
        private readonly State state;private readonly Host host;private readonly OriginalNv2Copies copies;
        public OriginalNv2Save(State state,Host host)
        {
            if(state==null||host==null||host.Storage==null||host.HasStorage==null||host.AssertStorage==null||host.CreateCache==null||host.WriteData==null||host.Reload==null)throw new ArgumentException("Genuine NV save, storage, assertion, MemStream, WriteData and Reload bindings required");
            var s=host.Storage;
            var storage=new OriginalNv2Copies.Host{Allocate=s.Allocate,Free=s.Free,Read=s.Read,Write=s.Write,Resize=s.Resize,Describe=s.Describe,GetName=s.GetName,NvFault=s.NvFault};
            copies=new OriginalNv2Copies(state.Header,storage);
            this.state=state;this.host=new Host{Storage=storage,HasStorage=host.HasStorage,AssertStorage=host.AssertStorage,CreateCache=host.CreateCache,WriteData=host.WriteData,Reload=host.Reload};
        }
        public void Save(){state.Busy=1;Save1();Save2();state.Busy=0;}
        public void Save1()
        {
            if(!host.HasStorage()){host.AssertStorage();throw new InvalidOperationException("Native nonreturning storage assertion returned");}
            state.ReloadRequested=0;
            if(state.Cached==null){var cache=host.CreateCache();if(cache==null)throw new InvalidOperationException("Actual MemStream constructor returned no owner");state.Cached=cache;}
            else if(state.Cached.GetMode()!=0){state.Cached.ClearLength();state.Cached.ClearPosition();}
            host.WriteData(state.Cached);
            uint size=state.Cached.GetSize();byte[] data=state.Cached.GetData();
            if(size>int.MaxValue||(size!=0&&(data==null||size>(uint)data.Length)))throw new InvalidOperationException("Serialized native data pointer requires admitted readable bytes");
            ushort crc=OriginalNvCrc16.Calculate(data??new byte[0],0,(int)size);
            var h=state.Header;byte[] payload=new byte[(int)size];if(size!=0)Array.Copy(data,payload,payload.Length);
            if(h.Size1<size)
            {
                copies.SetState(3);host.Storage.Resize(unchecked(size*2+20));h.Word(12,4,size);h.Word(8,4,size);h.Word(18,2,crc);
                host.Storage.Write(h.HeaderOffset,h.Header);host.Storage.Write(unchecked(h.DataOffset+h.AllocatedSize),payload);
                if(state.Busy!=0)h.Status=5;else copies.SetState(5);
            }
            else
            {
                copies.SetState(2);h.Word(4,4,size);h.Word(16,2,crc);
                host.Storage.Write(h.HeaderOffset,h.Header);host.Storage.Write(h.DataOffset,payload);
                if(state.Busy!=0)h.Status=4;else copies.SetState(4);
            }
        }
        public void Save2()
        {
            byte status=state.Header.Status;
            if(status!=1)
            {
                if(status!=4&&status!=5)
                {var description=host.Storage.Describe(Encoding.ASCII.GetBytes("NV2::Save2: Invalid state"));var name=host.Storage.GetName();host.Storage.NvFault(name,2,description);throw new InvalidOperationException("Native nonreturning NVFault binding returned");}
                byte[] data=state.Cached==null?null:state.Cached.GetData();
                if(status==5)copies.Copy2To1(data);else copies.Copy1To2(data);
            }
            if(state.ReloadRequested!=0)host.Reload();
        }
    }
}
