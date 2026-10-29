using System;
namespace RouletteDeluxe
{
    // ConfigMgmtObj C1/C2 and D1/D2/D0. Subobjects and native storage are
    // actual caller owners; no managed GC or default client substitutes them.
    public sealed class OriginalConfigurationManagementLifecycle
    {
        public sealed class Categories
        {
            public readonly object Begin;
            public readonly uint Count;
            private readonly Func<uint,object> at;
            public Categories(object begin,uint count,Func<uint,object> at)
            {if(at==null||count!=0&&begin==null)throw new ArgumentException("Actual category vector extent required");Begin=begin;Count=count;this.at=at;}
            internal object At(uint index){return at(index);}
        }
        public sealed class Host
        {
            public Action<uint,uint> WriteWord;
            public Action<uint,byte> WriteByte;
            public Action<uint> ConstructConfigClient,ConstructHostClient,DestroyConfigClient,DestroyHostClient,DestroyString,DestroyCategoryVector;
            public Func<object> GetConfigInstance,GetConfigHostInstance;
            public Action<uint,object> InitializeConfigConnection,InitializeHostConnection;
            public Action<uint,object,uint> RegisterTestCompleteHandler;
            public Func<Categories> ReadCategories;
            public Func<object> ReadCategoryBegin;
            public Action<object> DestroyCategoryString,FreeCategories,DeleteObject;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly object owner;
        private readonly Host host;
        public OriginalConfigurationManagementLifecycle(object owner,Host h)
        {
            if(owner==null||h==null||h.WriteWord==null||h.WriteByte==null||h.ConstructConfigClient==null||h.ConstructHostClient==null||h.DestroyConfigClient==null||h.DestroyHostClient==null||h.DestroyString==null||h.DestroyCategoryVector==null||h.GetConfigInstance==null||h.GetConfigHostInstance==null||h.InitializeConfigConnection==null||h.InitializeHostConnection==null||h.RegisterTestCompleteHandler==null||h.ReadCategories==null||h.ReadCategoryBegin==null||h.DestroyCategoryString==null||h.FreeCategories==null||h.DeleteObject==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual management owner, fields, clients, vector, string, heap, connection, callback and exception bindings required");
            this.owner=owner;host=new Host{WriteWord=h.WriteWord,WriteByte=h.WriteByte,ConstructConfigClient=h.ConstructConfigClient,ConstructHostClient=h.ConstructHostClient,DestroyConfigClient=h.DestroyConfigClient,DestroyHostClient=h.DestroyHostClient,DestroyString=h.DestroyString,DestroyCategoryVector=h.DestroyCategoryVector,GetConfigInstance=h.GetConfigInstance,GetConfigHostInstance=h.GetConfigHostInstance,InitializeConfigConnection=h.InitializeConfigConnection,InitializeHostConnection=h.InitializeHostConnection,RegisterTestCompleteHandler=h.RegisterTestCompleteHandler,ReadCategories=h.ReadCategories,ReadCategoryBegin=h.ReadCategoryBegin,DestroyCategoryString=h.DestroyCategoryString,FreeCategories=h.FreeCategories,DeleteObject=h.DeleteObject,IsNativeUnwind=h.IsNativeUnwind};
        }
        public void Construct()
        {
            host.WriteWord(0,0x0972c0e8);host.ConstructConfigClient(4);
            try{host.ConstructHostClient(0x88);}
            catch(Exception ex){if(host.IsNativeUnwind(ex))host.DestroyConfigClient(4);throw;}
            host.WriteWord(0xc8,0);host.WriteWord(0xcc,0);host.WriteWord(0xd0,0);host.WriteWord(0xd4,0x09b482ac);host.WriteByte(0xd8,0);
            try
            {
                object config=host.GetConfigInstance();host.InitializeConfigConnection(4,config);
                object configHost=host.GetConfigHostInstance();host.InitializeHostConnection(0x88,configHost);
                host.RegisterTestCompleteHandler(4,owner,0x08e7f9e0);
            }
            catch(Exception ex)
            {
                if(host.IsNativeUnwind(ex)){host.DestroyString(0xd4);host.DestroyCategoryVector(0xc8);host.DestroyHostClient(0x88);host.DestroyConfigClient(4);}
                throw;
            }
        }
        public void Destroy()
        {
            host.WriteWord(0,0x0972c0e8);host.DestroyString(0xd4);
            var frame=host.ReadCategories();if(frame==null)throw new InvalidOperationException("Actual category frame admission required");object begin=frame.Begin;
            if(frame.Count!=0)
            {
                for(uint index=0;index<frame.Count;index=unchecked(index+1))
                {object slot=frame.At(index);if(slot==null)throw new InvalidOperationException("Actual category string slot admission required");host.DestroyCategoryString(slot);}
                begin=host.ReadCategoryBegin();
            }
            if(begin!=null)host.FreeCategories(begin);
            try{host.DestroyHostClient(0x88);}
            catch(Exception ex){if(host.IsNativeUnwind(ex))host.DestroyConfigClient(4);throw;}
            host.DestroyConfigClient(4);
        }
        public void Delete(){Destroy();host.DeleteObject(owner);}
    }
}
