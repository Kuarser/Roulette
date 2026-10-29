using System;
namespace RouletteDeluxe
{
    // ConfigMgmtObj::TestCompleteHandler, 08e7f9e0. All owners and imported
    // operations require actual bindings. This callback ignores both arguments.
    public sealed class OriginalConfigurationManagementBackup
    {
        public sealed class Host
        {
            public Func<uint,byte> ReadFlag;
            public Func<uint,object> CopyPrefix;
            public Action<object,uint> AppendFilename;
            public Func<uint,object> ConstructStream;
            // InsertPath uses the native string's stored length, not strlen.
            public Func<object,object,object> InsertPath;
            public Action<object,uint,uint> InsertSuffix;
            public Func<object,object> GetStreamString;
            public Func<object,uint,int> Access;
            public Func<object,int> Remove;
            public Func<object,object,int> Rename;
            public Action<object> DestroyReturnedString,DestroyPath,DestroyStream;
            public Action<uint,byte> WriteFlag;
            // Normal inline destruction: final vtables 09b48dec/09b48e00/
            // 09b497c8; string +28; streambuf 09b48a28; locale +20;
            // VTT entries 09b49594/98, iosbase 09b49c10; iosbase +2c.
            public Action<object> ResetFinalStreamVtables,DestroyStreamString,
                ResetStreambufBase,DestroyStreamLocale,RestoreStreamBases,DestroyIosBase;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        public OriginalConfigurationManagementBackup(Host h)
        {
            if(h==null||h.ReadFlag==null||h.CopyPrefix==null||h.AppendFilename==null||h.ConstructStream==null||h.InsertPath==null||h.InsertSuffix==null||h.GetStreamString==null||h.Access==null||h.Remove==null||h.Rename==null||h.DestroyReturnedString==null||h.DestroyPath==null||h.DestroyStream==null||h.WriteFlag==null||h.ResetFinalStreamVtables==null||h.DestroyStreamString==null||h.ResetStreambufBase==null||h.DestroyStreamLocale==null||h.RestoreStreamBases==null||h.DestroyIosBase==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual shared flag/filename, string, stream, filesystem, destruction and exception bindings required");
            host=new Host{ReadFlag=h.ReadFlag,CopyPrefix=h.CopyPrefix,AppendFilename=h.AppendFilename,ConstructStream=h.ConstructStream,InsertPath=h.InsertPath,InsertSuffix=h.InsertSuffix,GetStreamString=h.GetStreamString,Access=h.Access,Remove=h.Remove,Rename=h.Rename,DestroyReturnedString=h.DestroyReturnedString,DestroyPath=h.DestroyPath,DestroyStream=h.DestroyStream,WriteFlag=h.WriteFlag,ResetFinalStreamVtables=h.ResetFinalStreamVtables,DestroyStreamString=h.DestroyStreamString,ResetStreambufBase=h.ResetStreambufBase,DestroyStreamLocale=h.DestroyStreamLocale,RestoreStreamBases=h.RestoreStreamBases,DestroyIosBase=h.DestroyIosBase,IsNativeUnwind=h.IsNativeUnwind};
        }
        private static object Known(object owner)
        {return owner??throw new InvalidOperationException("Actual path/stream/string/ostream owner admission required");}
        private void Protected(Action call,Action cleanup)
        {try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private T Protected<T>(Func<T> call,Action cleanup)
        {try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        public void Complete(bool success,object errors)
        {
            if(host.ReadFlag(0xd8)==0)return;
            object path=Known(host.CopyPrefix(0xa122b64));
            Protected(()=>host.AppendFilename(path,0xd4),()=>host.DestroyPath(path));
            object stream=Known(Protected(()=>host.ConstructStream(0x10),()=>host.DestroyPath(path)));
            Action cleanup=()=>{host.DestroyStream(stream);host.DestroyPath(path);};
            object output=Known(Protected(()=>host.InsertPath(stream,path),cleanup));
            Protected(()=>host.InsertSuffix(output,0x972c048,4),cleanup);
            object first=Known(Protected(()=>host.GetStreamString(stream),cleanup));
            int access=Protected(()=>host.Access(first,0),cleanup);
            Protected(()=>host.DestroyReturnedString(first),cleanup);
            if(access==0)
            {
                object second=Known(Protected(()=>host.GetStreamString(stream),cleanup));
                host.Remove(second);
                host.DestroyReturnedString(second);
            }
            object third=Known(Protected(()=>host.GetStreamString(stream),cleanup));
            host.Rename(path,third);
            host.DestroyReturnedString(third);
            host.WriteFlag(0xd8,0);
            host.ResetFinalStreamVtables(stream);
            host.DestroyStreamString(stream);
            host.ResetStreambufBase(stream);
            host.DestroyStreamLocale(stream);
            host.RestoreStreamBases(stream);
            Protected(()=>host.DestroyIosBase(stream),()=>host.DestroyPath(path));
            host.DestroyPath(path);
        }
    }
}
