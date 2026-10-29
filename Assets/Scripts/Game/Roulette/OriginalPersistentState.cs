using System;
namespace RouletteDeluxe
{
    // ELF bally::State is a byte slot plus a shared NVState. The fixed26-byte
    // NV payload is count+25 slots; cursor and reference count are not persisted.
    public sealed class OriginalPersistentState:IDisposable
    {
        private sealed class Owner
        {
            internal byte[] Data=new byte[26];internal int Cursor,References;
            internal readonly OriginalNv2Storage Storage;internal readonly bool SoftEnabled;
            internal readonly Action<string> ContextLog;private readonly IDisposable ownedDevice;
            internal Owner(IOriginalNvDevice device,bool ownsDevice,Action<string> recovery,bool soft,Action<string> context)
            {
                Data[0]=1;SoftEnabled=soft;ContextLog=context;ownedDevice=ownsDevice?device as IDisposable:null;
                if(ownsDevice&&ownedDevice==null)throw new ArgumentException("Owned NV device must be disposable");
                Storage=new OriginalNv2Storage(device,()=>Data,bytes=>{if(bytes.Length!=26)throw new OriginalNvFault("Exact26-byte State NV payload required");Data=(byte[])bytes.Clone();},recovery);
                try{Storage.Reload();}catch{if(ownedDevice!=null)ownedDevice.Dispose();throw;}
            }
            internal void Soft(){Storage.SoftValidate(SoftEnabled,"",ContextLog);}
            internal void Release(){--References;if(References==0&&ownedDevice!=null)ownedDevice.Dispose();}
        }
        private Owner owner;private byte slot;private bool disposed;
        public OriginalPersistentState(){}
        private OriginalPersistentState(Owner shared,byte index){owner=shared;slot=index;if(owner!=null)++owner.References;}
        private Owner Bound()
        {if(disposed)throw new ObjectDisposedException(nameof(OriginalPersistentState));if(owner==null)throw new InvalidOperationException("Native State requires bound NVState");return owner;}
        public void Open(Func<IOriginalNvDevice> openDevice,bool ownsDevice,Action<string> recovery,bool softEnabled,Action<string> contextLog)
        {
            if(disposed)throw new ObjectDisposedException(nameof(OriginalPersistentState));if(openDevice==null)throw new ArgumentNullException(nameof(openDevice));
            if(recovery==null)throw new ArgumentNullException(nameof(recovery));if(softEnabled&&contextLog==null)throw new ArgumentNullException(nameof(contextLog));
            if(owner!=null){owner.Release();owner=null;}slot=0;
            owner=new Owner(openDevice(),ownsDevice,recovery,softEnabled,contextLog);++owner.References;
        }
        public static OriginalPersistentState OpenFile(string path,Action<string> recovery,bool softEnabled,Action<string> contextLog)
        {var result=new OriginalPersistentState();result.Open(()=>new OriginalNvFileDevice(path),true,recovery,softEnabled,contextLog);return result;}
        public OriginalPersistentState Copy()
        {if(disposed)throw new ObjectDisposedException(nameof(OriginalPersistentState));return new OriginalPersistentState(owner,slot);}
        public void AssignReference(OriginalPersistentState source)
        {
            if(source==null)throw new ArgumentNullException(nameof(source));if(disposed||source.disposed)throw new ObjectDisposedException(nameof(OriginalPersistentState));
            // Native releases before acquiring; sole-reference self assignment has a
            // dangling pointer hazard. Managed self assignment is explicitly rejected.
            if(ReferenceEquals(this,source))throw new InvalidOperationException("Native self assignment lifetime is not qualified");
            if(owner!=null)owner.Release();owner=source.owner;slot=source.slot;if(owner!=null)++owner.References;
        }
        public int Value
        {get{var shared=Bound();if(slot>=shared.Data[0])throw new OriginalNvFault("State::int(), stack overflow");return shared.Data[1+slot];}}
        public int Assign(int value)
        {
            var shared=Bound();if(value>=256||slot>=shared.Data[0])throw new OriginalNvFault("State::=(), stack overflow");
            // Original signed long32 admits negatives and compares full int to byte,
            // so assigning-1 twice writes255 and performs two saves.
            if(shared.Data[1+slot]!=value)
            {shared.Soft();shared.Data[1+slot]=unchecked((byte)value);shared.Cursor=slot;shared.Data[0]=unchecked((byte)(slot+1));shared.Storage.Save();}
            return value;
        }
        public OriginalPersistentState NewSubState()
        {
            var shared=Bound();shared.Soft();shared.Cursor=unchecked(shared.Cursor+1);
            if(shared.Data[0]<=shared.Cursor)
            {
                shared.Data[0]=unchecked((byte)(shared.Data[0]+1));
                if(shared.Data[0]>25)throw new OriginalNvFault("State::NewSubState, stack overflow");
                shared.Data[1+shared.Cursor]=0;
            }
            shared.Storage.Save();return new OriginalPersistentState(shared,unchecked((byte)shared.Cursor));
        }
        public void DeleteSubState()
        {
            var shared=Bound();shared.Soft();shared.Cursor=Math.Max(unchecked(shared.Cursor-1),0);
            shared.Data[0]=unchecked((byte)(slot+1));shared.Storage.Save();
        }
        public byte[] PayloadBytes{get{return (byte[])Bound().Data.Clone();}}
        public byte[] DeviceImage{get{return Bound().Storage.ReadDeviceImage();}}
        public void ValidateStorage(){Bound().Storage.Validate();}
        public void Reload(){Bound().Storage.Reload();}
        internal int ReferenceCount{get{return Bound().References;}}
        internal int Cursor{get{return Bound().Cursor;}}
        internal byte Slot{get{return slot;}}
        public void Dispose(){if(disposed)return;disposed=true;if(owner!=null){owner.Release();owner=null;}}
    }
}
