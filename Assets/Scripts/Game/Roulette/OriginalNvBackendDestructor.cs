using System;
namespace RouletteDeluxe
{
    // NV D1/D2 and D0. Native representations, atomic refcounts, stdstring D1,
    // exception translation and operator delete remain genuine owner bindings.
    public sealed class OriginalNvBackendDestructor
    {
        public sealed class Representation
        {
            public readonly object Owner;
            // Admitted identity with native shared empty rep, not byte length.
            public readonly bool IsSharedEmpty;
            public Representation(object owner,bool isSharedEmpty)
            {if(owner==null)throw new ArgumentNullException("owner");Owner=owner;IsSharedEmpty=isSharedEmpty;}
        }
        public sealed class Host
        {
            public Action<uint> ResetVtable;
            public Action BaseClose;
            public Func<int,Representation> ReadRepresentation;
            // Return the OLD signed refcount, after the atomic decrement effect.
            public Func<Representation,int> AtomicDecrement;
            public Action<Representation,int> DestroyRepresentation;
            public Action<int> StringDestructor;
            public Action DeleteObject;
            // Bridge distinguishes actual native unwinding from diagnostic stops
            // and managed frame-admission errors; this is not a native callback.
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        public OriginalNvBackendDestructor(Host h)
        {
            if(h==null||h.ResetVtable==null||h.BaseClose==null||h.ReadRepresentation==null||h.AtomicDecrement==null||h.DestroyRepresentation==null||h.StringDestructor==null||h.DeleteObject==null||h.IsNativeUnwind==null)
                throw new ArgumentException("Actual NV object/string/refcount/destructor and native unwind bindings required");
            host=new Host{ResetVtable=h.ResetVtable,BaseClose=h.BaseClose,ReadRepresentation=h.ReadRepresentation,AtomicDecrement=h.AtomicDecrement,DestroyRepresentation=h.DestroyRepresentation,StringDestructor=h.StringDestructor,DeleteObject=h.DeleteObject,IsNativeUnwind=h.IsNativeUnwind};
        }
        private void Release(int field)
        {
            var representation=host.ReadRepresentation(field);
            if(representation==null)throw new InvalidOperationException("Actual native string representation frame required");
            if(representation.IsSharedEmpty)return;
            int old=host.AtomicDecrement(representation);
            if(old<=0)host.DestroyRepresentation(representation,field);
        }
        private void Destruct(bool deleting)
        {
            host.ResetVtable(0x09764e08);
            try{host.BaseClose();}
            catch(Exception failure)
            {
                if(!host.IsNativeUnwind(failure))throw;
                host.StringDestructor(0x18);host.StringDestructor(0x14);host.StringDestructor(0x10);host.StringDestructor(8);
                throw; // Native _Unwind_Resume: same exception; D0 does not delete.
            }
            Release(0x18);Release(0x14);Release(0x10);Release(8);
            if(deleting)host.DeleteObject();
        }
        public void Destroy(){Destruct(false);}
        public void Delete(){Destruct(true);}
    }
}
