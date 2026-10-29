using System;
using System.Text;

namespace RouletteDeluxe
{
    // Native NV Read/Write share the actual descriptor and stored size with
    // OriginalNvBackendSize. Pointer frames and POSIX IO must be supplied.
    public sealed class OriginalNvBackendIo
    {
        public sealed class Host
        {
            public Func<int,uint,int,int> Seek;
            public Func<int,OriginalConfigurationNvData.ByteSlot[],uint,int> Read;
            public Func<int,byte[],uint,int> Write;
            public Func<int> Errno;
            public Func<int,byte[]> Strerror;
            // Last argument is raw uint32: stored size for length/seek errors,
            // ssize_t return bits for short IO. Formatter owns native varargs.
            public Func<byte[],byte[],uint,uint,uint,byte[]> GetDisplayInfo;
            public Func<byte[]> GetDisplayName;
            public Action<byte[],uint,byte[]> NvFault;
        }
        private readonly OriginalNvBackendSize.State state;
        private readonly Host host;
        public OriginalNvBackendIo(OriginalNvBackendSize.State state,Host h)
        {
            if(state==null||h==null||h.Seek==null||h.Read==null||h.Write==null||
                h.Errno==null||h.Strerror==null||h.GetDisplayInfo==null||h.GetDisplayName==null||h.NvFault==null)
                throw new ArgumentException("Actual NV descriptor, byte frames, POSIX IO and fault bindings required");
            this.state=state;
            host=new Host{Seek=h.Seek,Read=h.Read,Write=h.Write,Errno=h.Errno,Strerror=h.Strerror,
                GetDisplayInfo=h.GetDisplayInfo,GetDisplayName=h.GetDisplayName,NvFault=h.NvFault};
        }
        private void Fault(string format,byte[] explanation,uint offset,uint count,uint last)
        {
            byte[] description=host.GetDisplayInfo(Encoding.ASCII.GetBytes(format),explanation,offset,count,last);
            byte[] name=host.GetDisplayName();
            host.NvFault(name,2,description);
            throw new InvalidOperationException("Native nonreturning NVFault binding returned");
        }
        private bool Begin(bool write,uint offset,uint count)
        {
            string name=write?"Write":"Read";
            uint size=state.StoredSize;
            if(size<unchecked(offset+count))
                Fault("NV::"+name+": length error\noff=%ld,len=%ld,size=%ld",null,offset,count,size);
            if(host.Seek(state.Descriptor,offset,0)!=-1)return true;
            byte[] text=host.Strerror(host.Errno());
            Fault("NV::"+name+": seek error\n%s\noff=%ld,len=%ld,size=%ld",text,offset,count,state.StoredSize);
            return false;
        }
        public void Read(uint offset,OriginalConfigurationNvData.ByteSlot[] destination,uint count)
        {
            if(!Begin(false,offset,count))return;
            int actual=host.Read(state.Descriptor,destination,count);
            if(unchecked((uint)actual)==count)return;
            byte[] text=host.Strerror(host.Errno());
            Fault("NV::Read: read error\n%s\noff=%ld,len=%ld,read=%d",text,offset,count,unchecked((uint)actual));
        }
        public void Write(uint offset,byte[] source,uint count)
        {
            if(!Begin(true,offset,count))return;
            int actual=host.Write(state.Descriptor,source,count);
            if(unchecked((uint)actual)==count)return;
            byte[] text=host.Strerror(host.Errno());
            Fault("NV::Write: write error\n%s\noff=%ld,len=%ld,written=%d",text,offset,count,unchecked((uint)actual));
        }
    }
}
