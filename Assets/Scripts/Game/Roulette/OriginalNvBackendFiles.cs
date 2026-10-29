using System;
using System.Text;
namespace RouletteDeluxe
{
    // Native NV construction content and static stat/remove helper boundaries.
    public sealed class OriginalNvBackendFiles
    {
        public sealed class ConstructorHost
        {
            // NULL is absent; a present empty value still enables log display.
            public Func<byte[],byte[]> GetEnvironment;
            public Action<byte> SetLogDisplayEnabled;
        }
        public sealed class Host
        {
            public Func<int,byte[],OriginalConfigurationNvData.ByteSlot[],int> Stat;
            public Func<byte[],int> Remove;
            public Func<int> Errno;
            public Func<int,byte[]> Strerror;
            public Action<byte[],uint,byte[],int,byte[]> NvFault;
        }
        private readonly Host host;
        public OriginalNvBackendFiles(Host h)
        {
            if(h==null||h.Stat==null||h.Remove==null||h.Errno==null||h.Strerror==null||h.NvFault==null)
                throw new ArgumentException("Actual POSIX stat/remove/errno and direct NVFault bindings required");
            host=new Host{Stat=h.Stat,Remove=h.Remove,Errno=h.Errno,Strerror=h.Strerror,NvFault=h.NvFault};
        }
        public static OriginalNvBackendDisplay.State Construct(ConstructorHost h)
        {
            if(h==null||h.GetEnvironment==null||h.SetLogDisplayEnabled==null)throw new ArgumentException("Actual environment and native global-byte owner required");
            var environment=h.GetEnvironment;var setFlag=h.SetLogDisplayEnabled;
            var result=new OriginalNvBackendDisplay.State(new OriginalNvBackendSize.State(-1,0,new byte[0]),new byte[0],new byte[0],new byte[0]);
            if(environment(Encoding.ASCII.GetBytes("AGKSIM"))!=null||
                environment(Encoding.ASCII.GetBytes("AGK_DEMO_MODE"))!=null||
                environment(Encoding.ASCII.GetBytes("AGKBIOSENABLE"))!=null)setFlag(1);
            return result;
        }
        private static OriginalConfigurationNvData.ByteSlot[] StatFrame()
        {
            var frame=new OriginalConfigurationNvData.ByteSlot[100];
            for(int i=0;i<frame.Length;++i)frame[i]=new OriginalConfigurationNvData.ByteSlot(null);
            return frame;
        }
        private static byte[] Path(byte[] path)
        {if(path==null)throw new ArgumentException("Admitted native stdstring path required");return (byte[])path.Clone();}
        public bool FileExists(byte[] path){return host.Stat(3,Path(path),StatFrame())!=-1;}
        public uint FileSize(byte[] path)
        {
            var frame=StatFrame();int result=host.Stat(3,Path(path),frame);
            if(result==-1)return 0;
            uint size=0;
            for(int i=0;i<4;++i)
            {
                var slot=frame[44+i];
                if(slot==null||!slot.Value.HasValue)throw new InvalidOperationException("Native stat size word requires caller-known bytes");
                size|=(uint)slot.Value.Value<<(8*i);
            }
            return size;
        }
        public void DeleteFile(byte[] path)
        {
            byte[] raw=Path(path);
            if(host.Remove(raw)!=-1)return;
            int error=host.Errno();byte[] text=host.Strerror(error);error=host.Errno();
            host.NvFault(Path(path),2,Encoding.ASCII.GetBytes("NV::DeleteFile: error %d\n%s"),error,text);
            throw new InvalidOperationException("Native nonreturning NVFault binding returned");
        }
    }
}
