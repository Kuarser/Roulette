using System;
namespace RouletteDeluxe
{
    // Original libsdg profile: addresses are ELF-relative. The host supplies relocation.
    public sealed class OriginalStreamDestructor
    {
        public sealed class Host
        {
            public Action<uint> ResetVtable;
            public Func<object> ReadBasePointer;
            public Action<object> FreeBasePointer;
            public Action DeleteObject;
        }
        private readonly Host host;
        public OriginalStreamDestructor(Host h)
        {
            if(h==null||h.ResetVtable==null||h.ReadBasePointer==null||h.FreeBasePointer==null||h.DeleteObject==null)throw new ArgumentException("Actual Stream vtable, base pointer, free and object deletion bindings required");
            host=new Host{ResetVtable=h.ResetVtable,ReadBasePointer=h.ReadBasePointer,FreeBasePointer=h.FreeBasePointer,DeleteObject=h.DeleteObject};
        }
        public void Destroy()
        {host.ResetVtable(0x8448);object pointer=host.ReadBasePointer();if(pointer!=null)host.FreeBasePointer(pointer);}
        public void Delete()
        {object pointer=host.ReadBasePointer();host.ResetVtable(0x8448);if(pointer!=null)host.FreeBasePointer(pointer);host.DeleteObject();}
    }
    public sealed class OriginalMemStreamDestructor
    {
        private readonly OriginalMemStream stream;private readonly OriginalStreamDestructor baseDestructor;
        private readonly Action<uint> resetVtable;private readonly Action deleteObject;
        public OriginalMemStreamDestructor(OriginalMemStream stream,OriginalStreamDestructor.Host h)
        {
            if(stream==null)throw new ArgumentNullException("stream");baseDestructor=new OriginalStreamDestructor(h);
            this.stream=stream;resetVtable=h.ResetVtable;deleteObject=h.DeleteObject;
        }
        private void Release()
        {
            uint ownership=stream.Ownership;resetVtable(0x8068);
            stream.ReleaseForDestruction(ownership);baseDestructor.Destroy();
        }
        public void Destroy(){Release();}
        public void Delete(){Release();deleteObject();}
    }
}
