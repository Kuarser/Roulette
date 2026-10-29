using System;
using System.Text;
namespace RouletteDeluxe
{
    // ConfigMgmtObj::LoadSavedOptionsFromFile at 08e80c60.
    // Paths, local vectors, XML acquisition/conversion, SetOptions and Fault
    // remain actual bindings; this orchestrates their recovered ownership scopes.
    public sealed class OriginalConfigurationManagementLoad
    {
        public sealed class Host
        {
            public Func<uint,object> CopyPrefix;
            public Action<object,uint> AppendFilename;
            public Func<uint,uint> ReadFilenameLength;
            public Func<object,object> CopyPath;
            public Action<object> DestroyPath;
            public Func<object,uint,int> Access;
            public Func<object> CreateConfigurationVector;
            public Action<object,object> CreateFromFile;
            public Func<object,uint> ConfigurationCount;
            public Func<object,object> ConvertToSimple;
            public Action<uint,byte> WriteFlag;
            public Action<uint,object> SetOptions;
            public Action<object> DestroySimpleElements,DestroySimpleVector,DestroyConfigurationVector,FreeSimple;
            public Func<object,object> ReadSimpleBegin;
            public Action<uint,byte[],object> Fault;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        public OriginalConfigurationManagementLoad(Host h)
        {
            if(h==null||h.CopyPrefix==null||h.AppendFilename==null||h.ReadFilenameLength==null||h.CopyPath==null||h.DestroyPath==null||h.Access==null||h.CreateConfigurationVector==null||h.CreateFromFile==null||h.ConfigurationCount==null||h.ConvertToSimple==null||h.WriteFlag==null||h.SetOptions==null||h.DestroySimpleElements==null||h.DestroySimpleVector==null||h.DestroyConfigurationVector==null||h.FreeSimple==null||h.ReadSimpleBegin==null||h.Fault==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual shared filename/flag/client, path, vector, XML, conversion, allocation, fault and exception bindings required");
            host=new Host{CopyPrefix=h.CopyPrefix,AppendFilename=h.AppendFilename,ReadFilenameLength=h.ReadFilenameLength,CopyPath=h.CopyPath,DestroyPath=h.DestroyPath,Access=h.Access,CreateConfigurationVector=h.CreateConfigurationVector,CreateFromFile=h.CreateFromFile,ConfigurationCount=h.ConfigurationCount,ConvertToSimple=h.ConvertToSimple,WriteFlag=h.WriteFlag,SetOptions=h.SetOptions,DestroySimpleElements=h.DestroySimpleElements,DestroySimpleVector=h.DestroySimpleVector,DestroyConfigurationVector=h.DestroyConfigurationVector,FreeSimple=h.FreeSimple,ReadSimpleBegin=h.ReadSimpleBegin,Fault=h.Fault,IsNativeUnwind=h.IsNativeUnwind};
        }
        private static object Known(object owner)
        {return owner??throw new InvalidOperationException("Actual local path/vector owner admission required");}
        private void Protected(Action call,Action cleanup)
        {try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private T Protected<T>(Func<T> call,Action cleanup)
        {try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        public void Load()
        {
            object path=Known(host.CopyPrefix(0x0a122b64));
            Protected(()=>host.AppendFilename(path,0xd4),()=>host.DestroyPath(path));
            if(host.ReadFilenameLength(0xd4)!=0&&host.Access(path,0)==0)
            {
                object options=Known(host.CreateConfigurationVector());
                Action cleanup=()=>{host.DestroyConfigurationVector(options);host.DestroyPath(path);};
                object copy=Known(Protected(()=>host.CopyPath(path),cleanup));
                Protected(()=>host.CreateFromFile(options,copy),()=>{host.DestroyPath(copy);cleanup();});
                host.DestroyPath(copy);
                if(host.ConfigurationCount(options)==0)
                {
                    Protected(()=>host.Fault(16,Encoding.ASCII.GetBytes("LoadSavedOptionsFromFile Failure.  Zero options in:\n%s"),path),cleanup);
                    throw new InvalidOperationException("Native nonreturning Fault binding returned");
                }
                object simple=Known(Protected(()=>host.ConvertToSimple(options),cleanup));
                host.WriteFlag(0xd8,1);
                Protected(()=>host.SetOptions(4,simple),()=>{host.DestroySimpleVector(simple);cleanup();});
                host.DestroySimpleElements(simple);
                object begin=host.ReadSimpleBegin(simple);if(begin!=null)host.FreeSimple(begin);
                Protected(()=>host.DestroyConfigurationVector(options),()=>host.DestroyPath(path));
            }
            host.DestroyPath(path);
        }
    }
}
