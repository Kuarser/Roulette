using System;
using System.Text;
namespace RouletteDeluxe
{
    // Native NV Open/Close/OpenWithFault on the actual shared backend owner.
    public sealed class OriginalNvBackendLifecycle
    {
        public sealed class Host
        {
            public Action VirtualClose;
            public Func<int,int> Close;
            public Func<byte[],uint,uint,int> Open;
            public Func<byte[],int,int> Access;
            public Func<byte[],uint,int> Mkdir;
            public Func<int,uint,int,int> Seek;
            public Func<int> Errno;
            public Func<int,byte[]> Strerror;
            public Func<byte[],int,byte[],byte[]> GetDisplayInfo;
            public Func<byte[]> GetDisplayName;
            public Action<byte[],uint,byte[]> NvFault;
        }
        private readonly OriginalNvBackendDisplay.State strings;
        private readonly OriginalNvBackendSize.State state;
        private readonly Host host;
        public OriginalNvBackendLifecycle(OriginalNvBackendDisplay.State strings,Host h)
        {
            if(strings==null||h==null||h.VirtualClose==null||h.Close==null||h.Open==null||h.Access==null||h.Mkdir==null||h.Seek==null||h.Errno==null||h.Strerror==null||h.GetDisplayInfo==null||h.GetDisplayName==null||h.NvFault==null)
                throw new ArgumentException("Actual NV owner, virtual Close, POSIX and fault bindings required");
            this.strings=strings;state=strings.Storage;
            host=new Host{VirtualClose=h.VirtualClose,Close=h.Close,Open=h.Open,Access=h.Access,Mkdir=h.Mkdir,Seek=h.Seek,Errno=h.Errno,Strerror=h.Strerror,GetDisplayInfo=h.GetDisplayInfo,GetDisplayName=h.GetDisplayName,NvFault=h.NvFault};
        }
        private void Error(string format,int error)
        {
            byte[] text=host.Strerror(error);error=host.Errno();
            byte[] description=host.GetDisplayInfo(Encoding.ASCII.GetBytes(format),error,text);
            byte[] name=host.GetDisplayName();host.NvFault(name,2,description);
            throw new InvalidOperationException("Native nonreturning NVFault binding returned");
        }
        public void Close()
        {
            if(state.Descriptor==-1)return;
            if(host.Close(state.Descriptor)==-1)Error("NV::Close: close error=%d\n%s",host.Errno());
            state.Descriptor=-1;state.SetPath(new byte[0]);state.StoredSize=0;
        }
        private static byte[] Prefix(byte[] bytes,int length)
        {var result=new byte[length];Array.Copy(bytes,result,length);return result;}
        public void Open(byte[] path,byte[] description)
        {
            if(path==null||description==null)throw new ArgumentException("Actual raw stdstring path and description required");
            host.VirtualClose();state.SetPath(path);
            if(description.Length==0)
            {
                byte[] raw=state.Path;int start=Array.LastIndexOf(raw,(byte)'/')+1;
                var basename=new byte[raw.Length-start];Array.Copy(raw,start,basename,0,basename.Length);strings.SetDescription(basename);
            }
            else strings.SetDescription(description);
            int remaining=10;
            while(state.Descriptor==-1)
            {
                int fd=host.Open(state.Path,0x101042,0x1b6);state.Descriptor=fd;
                if(fd>=0)break;
                int error=host.Errno();
                if(error!=2)Error("NV::open: error=%d\n%s",error);
                byte[] raw=state.Path;int slash=Array.LastIndexOf(raw,(byte)'/');
                if(slash<0)Error("NV::open: error=%d\n%s",host.Errno());
                while(true)
                {
                    int result=host.Mkdir(Prefix(raw,slash),0x1ff);
                    if(result==0)break;
                    error=host.Errno();
                    if(error==17)break;
                    if(error!=2)Error("NV::mkdir: error=%d\n%s",error);
                    slash=Array.LastIndexOf(Prefix(raw,slash),(byte)'/');
                    if(slash<0)Error("NV::mkdir: error=%d\n%s",host.Errno());
                }
                if(--remaining==0)Error("NV::mkdir: error=%d\n%s",host.Errno());
            }
            int size=host.Seek(state.Descriptor,0,2);state.StoredSize=unchecked((uint)size);
            if(size==-1)Error("NV::FileSize: error=%d\n%s",host.Errno());
        }
        public byte OpenWithFault(byte[] path)
        {
            if(path==null)throw new ArgumentException("Actual raw stdstring path required");
            host.VirtualClose();state.SetPath(path);
            if(host.Access(state.Path,0)!=0)return 0;
            int fd=host.Open(state.Path,0x101042,0x1b6);state.Descriptor=fd;
            if(fd<0)return 0;
            int size=host.Seek(fd,0,2);state.StoredSize=unchecked((uint)size);
            return state.Descriptor>=0?(byte)(size!=0?1:0):(byte)0;
        }
    }
}
