using System;
namespace RouletteDeluxe
{
    // Admitted inherited NV2/NVConfigOptionData lifecycle slots.
    public sealed class OriginalNv2Lifecycle
    {
        public sealed class StorageOwner
        {
            public readonly Action<byte[],byte[]> Open;
            public readonly Func<byte[],byte> OpenWithFault;
            public readonly Action<uint,OriginalConfigurationNvData.ByteSlot[],uint> Read;
            public readonly Action Delete;
            public StorageOwner(Action<byte[],byte[]> open,Func<byte[],byte> openWithFault,Action<uint,OriginalConfigurationNvData.ByteSlot[],uint> read,Action delete)
            {if(open==null||openWithFault==null||read==null||delete==null)throw new ArgumentException("Actual NV owner/lifetime bindings required");Open=open;OpenWithFault=openWithFault;Read=read;Delete=delete;}
        }
        public sealed class State
        {
            public readonly OriginalNv2Save.State Nv;
            public StorageOwner Storage;
            public State(OriginalNv2Save.State nv,StorageOwner storage){if(nv==null)throw new ArgumentNullException("nv");Nv=nv;Storage=storage;}
        }
        public sealed class Host
        {
            public Func<StorageOwner> CreateStorage;
            public Action<int> AssertStorage;
            public Action<OriginalNv2Save.Cache> DeleteCached;
            public Action Reload;
            public Func<uint,OriginalMemStream> CreateStream;
            public Action<OriginalMemStream> ReadData;
        }
        private readonly State state;private readonly Host host;
        public OriginalNv2Lifecycle(State state,Host h)
        {
            if(state==null||h==null||h.CreateStorage==null||h.AssertStorage==null||h.DeleteCached==null||h.Reload==null||h.CreateStream==null||h.ReadData==null)throw new ArgumentException("Actual NV lifecycle/cache/storage/reload/stream bindings required");
            this.state=state;host=new Host{CreateStorage=h.CreateStorage,AssertStorage=h.AssertStorage,DeleteCached=h.DeleteCached,Reload=h.Reload,CreateStream=h.CreateStream,ReadData=h.ReadData};
        }
        public void Close()
        {
            if(state.Storage!=null)state.Storage.Delete();state.Storage=null;
            if(state.Nv.Cached!=null){host.DeleteCached(state.Nv.Cached);state.Nv.Cached=null;}
        }
        private void Create(int line)
        {state.Storage=host.CreateStorage();if(state.Storage==null){host.AssertStorage(line);throw new InvalidOperationException("Native nonreturning storage assertion returned");}}
        public void Open(byte[] path){OpenWithDesc(path,new byte[0]);}
        public void OpenWithDesc(byte[] path,byte[] description)
        {
            if(path==null||description==null)throw new ArgumentException("Admitted raw stdstring bytes required");
            Close();if(path.Length==0)return;Create(0x97);state.Storage.Open((byte[])path.Clone(),(byte[])description.Clone());host.Reload();
        }
        private void ReadHeader()
        {
            var h=state.Nv.Header;var raw=h.Header;var frame=new OriginalConfigurationNvData.ByteSlot[20];for(int i=0;i<20;++i)frame[i]=new OriginalConfigurationNvData.ByteSlot(raw[i]);
            try{state.Storage.Read(h.HeaderOffset,frame,20);}
            finally{for(int i=0;i<20;++i){if(frame[i]==null||!frame[i].Value.HasValue)throw new InvalidOperationException("Native open header byte requires caller-known storage");h.Word(i,1,frame[i].Value.Value);}}
        }
        public byte OpenWithFault(byte[] path)
        {
            if(path==null)throw new ArgumentException("Admitted raw stdstring bytes required");Close();if(path.Length==0)return 0;Create(0xb9);
            byte result=state.Storage.OpenWithFault((byte[])path.Clone());if(result!=0)
            {
                ReadHeader();var stream=host.CreateStream(state.Nv.Header.Size1);if(stream==null)throw new InvalidOperationException("Actual MemStream owner required");
                state.Storage.Read(state.Nv.Header.DataOffset,stream.Buffer,state.Nv.Header.Size1);host.ReadData(stream);
                // Original returns without deleting this temporary stream or adopting cache.
            }
            return result;
        }
        public void OpenConfiguration(OriginalConfigurationNvData owner,byte[] path,byte flag)
        {if(owner==null)throw new ArgumentNullException("owner");owner.Flag=flag;if(path==null)throw new ArgumentException("Admitted C-string path required");int length=Array.IndexOf(path,(byte)0);if(length<0)length=path.Length;var text=new byte[length];Array.Copy(path,text,length);Open(text);}
    }
}
