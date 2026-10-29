using System;
using System.Text;
namespace RouletteDeluxe
{
    // ConfigMgmtObj::WriteConfigOptionsToFile at 08e80e00. Uses the recovered
    // vector algorithms; shared owners, imported helpers and stream ABI are bound.
    public sealed class OriginalConfigurationManagementWrite
    {
        public sealed class Host
        {
            public Func<uint,uint> ReadCategoryWord;
            public Func<object> CreateSimpleVector,CreateStreamOwner;
            public Func<uint,object> CreateEmptyString,CopyCategory,CopyPrefix;
            public Func<uint,object,object> GetConfiguration;
            public Func<object,object> ConvertToSimple,ReadStringData;
            public Func<object,uint,uint> ReadVectorWord;
            public Action<uint,uint> InvokeConfigurationDestructor;
            public Action<uint> DestroySimpleString,Free;
            public Action<object> DestroySimpleVector,DestroyConfigurationVector,DestroyString;
            public Action<object,object,bool> CreateBufferFromConfig;
            public Action<object,uint> AppendFilename;
            public Func<object,uint> ReadStringLength,ReadIosState;
            public Func<object,uint,byte> ReadFixedState;
            public Action<object> ConstructIosBase,InitializeStreamBases,ResetFinalStreamVtables,
                ConstructFilebuf,ResetDestructionVtables,DestroyBasicFile,ResetStreambufBase,
                DestroyLocale,RestoreStreamBases,ResetIosBaseVtable,DestroyIosBase,
                DestroyFilebuf,DestroyStreambufBase,DestroyStream;
            public Action<object,bool> InitializeIos;
            public Func<object,object,uint,bool> Open;
            public Action<object,uint> ClearState;
            public Action<object,object,uint> InsertBuffer;
            public Func<object,bool> Close;
            public Func<object> ErrnoLocation;
            public Func<object,int> ReadErrno;
            public Func<int,object> Strerror;
            public Func<uint,object> ReadFilenameData;
            public Action<uint,byte[],object,object,object> Fault;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        private readonly OriginalConfigurationManagementVectors vectors;
        public OriginalConfigurationManagementWrite(Host h,OriginalConfigurationManagementVectors v)
        {
            if(v==null||h==null||h.ReadCategoryWord==null||h.CreateSimpleVector==null||h.CreateStreamOwner==null||h.CreateEmptyString==null||h.CopyCategory==null||h.CopyPrefix==null||h.GetConfiguration==null||h.ConvertToSimple==null||h.ReadStringData==null||h.ReadVectorWord==null||h.InvokeConfigurationDestructor==null||h.DestroySimpleString==null||h.Free==null||h.DestroySimpleVector==null||h.DestroyConfigurationVector==null||h.DestroyString==null||h.CreateBufferFromConfig==null||h.AppendFilename==null||h.ReadStringLength==null||h.ReadIosState==null||h.ReadFixedState==null||h.ConstructIosBase==null||h.InitializeStreamBases==null||h.ResetFinalStreamVtables==null||h.ConstructFilebuf==null||h.ResetDestructionVtables==null||h.DestroyBasicFile==null||h.ResetStreambufBase==null||h.DestroyLocale==null||h.RestoreStreamBases==null||h.ResetIosBaseVtable==null||h.DestroyIosBase==null||h.DestroyFilebuf==null||h.DestroyStreambufBase==null||h.DestroyStream==null||h.InitializeIos==null||h.Open==null||h.ClearState==null||h.InsertBuffer==null||h.Close==null||h.ErrnoLocation==null||h.ReadErrno==null||h.Strerror==null||h.ReadFilenameData==null||h.Fault==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual shared category/filename, vector algorithms/owners, provider/conversion/serialization, string/stream ABI/filesystem, destruction/fault/exception bindings required");
            vectors=v;
            host=new Host{ReadCategoryWord=h.ReadCategoryWord,CreateSimpleVector=h.CreateSimpleVector,CreateStreamOwner=h.CreateStreamOwner,CreateEmptyString=h.CreateEmptyString,CopyCategory=h.CopyCategory,CopyPrefix=h.CopyPrefix,GetConfiguration=h.GetConfiguration,ConvertToSimple=h.ConvertToSimple,ReadStringData=h.ReadStringData,ReadVectorWord=h.ReadVectorWord,InvokeConfigurationDestructor=h.InvokeConfigurationDestructor,DestroySimpleString=h.DestroySimpleString,Free=h.Free,DestroySimpleVector=h.DestroySimpleVector,DestroyConfigurationVector=h.DestroyConfigurationVector,DestroyString=h.DestroyString,CreateBufferFromConfig=h.CreateBufferFromConfig,AppendFilename=h.AppendFilename,ReadStringLength=h.ReadStringLength,ReadIosState=h.ReadIosState,ReadFixedState=h.ReadFixedState,ConstructIosBase=h.ConstructIosBase,InitializeStreamBases=h.InitializeStreamBases,ResetFinalStreamVtables=h.ResetFinalStreamVtables,ConstructFilebuf=h.ConstructFilebuf,ResetDestructionVtables=h.ResetDestructionVtables,DestroyBasicFile=h.DestroyBasicFile,ResetStreambufBase=h.ResetStreambufBase,DestroyLocale=h.DestroyLocale,RestoreStreamBases=h.RestoreStreamBases,ResetIosBaseVtable=h.ResetIosBaseVtable,DestroyIosBase=h.DestroyIosBase,DestroyFilebuf=h.DestroyFilebuf,DestroyStreambufBase=h.DestroyStreambufBase,DestroyStream=h.DestroyStream,InitializeIos=h.InitializeIos,Open=h.Open,ClearState=h.ClearState,InsertBuffer=h.InsertBuffer,Close=h.Close,ErrnoLocation=h.ErrnoLocation,ReadErrno=h.ReadErrno,Strerror=h.Strerror,ReadFilenameData=h.ReadFilenameData,Fault=h.Fault,IsNativeUnwind=h.IsNativeUnwind};
        }
        private static object Known(object owner){return owner??throw new InvalidOperationException("Actual local string/vector/stream/data owner admission required");}
        private void Protected(Action call,Action cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private T Protected<T>(Func<T> call,Action cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private void DestroySimpleInline(object vector)
        {
            uint end=host.ReadVectorWord(vector,4),begin=host.ReadVectorWord(vector,0);
            if(begin!=end)
            {
                for(uint at=begin;at!=end;at=unchecked(at+16)){host.DestroySimpleString(unchecked(at+8));host.DestroySimpleString(unchecked(at+4));host.DestroySimpleString(at);}
                begin=host.ReadVectorWord(vector,0);
            }
            if(begin!=0)host.Free(begin);
        }
        public void Write()
        {
            uint category=host.ReadCategoryWord(0xc8);if(category==host.ReadCategoryWord(0xcc))return;
            object aggregate=Known(host.CreateSimpleVector()),buffer=Known(host.CreateEmptyString(0x09b482ac));
            Action outer=()=>{host.DestroyString(buffer);host.DestroySimpleVector(aggregate);};
            do
            {
                object local=Known(host.CreateSimpleVector());Action localCleanup=()=>{host.DestroySimpleVector(local);outer();};
                object copy=Known(Protected(()=>host.CopyCategory(category),localCleanup));Action categoryCleanup=()=>{host.DestroyString(copy);localCleanup();};
                object config=Known(Protected(()=>host.GetConfiguration(0x88,copy),categoryCleanup));Action configCleanup=()=>{host.DestroyConfigurationVector(config);categoryCleanup();};
                object converted=Known(Protected(()=>host.ConvertToSimple(config),configCleanup));
                vectors.Assign(local,converted,ex=>{host.DestroySimpleVector(converted);configCleanup();});
                DestroySimpleInline(converted);
                uint configEnd=host.ReadVectorWord(config,4),configBegin=host.ReadVectorWord(config,0);
                if(configBegin!=configEnd)
                {
                    for(uint at=configBegin;at!=configEnd;at=unchecked(at+132)){uint slot=at;Protected(()=>host.InvokeConfigurationDestructor(slot,0),()=>{uint allocated=host.ReadVectorWord(config,0);if(allocated!=0)host.Free(allocated);categoryCleanup();});}
                    configBegin=host.ReadVectorWord(config,0);
                }
                if(configBegin!=0)host.Free(configBegin);
                host.DestroyString(copy);
                vectors.Append(aggregate,local,ex=>localCleanup());
                DestroySimpleInline(local);
                category=unchecked(category+4);
            }while(category!=host.ReadCategoryWord(0xcc));
            Protected(()=>host.CreateBufferFromConfig(buffer,aggregate,true),outer);
            object path=Known(Protected(()=>host.CopyPrefix(0x0a122b64),outer));Action pathCleanup=()=>{host.DestroyString(path);outer();};
            Protected(()=>host.AppendFilename(path,0xd4),pathCleanup);
            object pathData=Known(host.ReadStringData(path)),stream=Known(host.CreateStreamOwner());
            host.ConstructIosBase(stream);host.InitializeStreamBases(stream);
            Protected(()=>host.InitializeIos(stream,false),()=>{host.ResetIosBaseVtable(stream);host.DestroyIosBase(stream);pathCleanup();});
            host.ResetFinalStreamVtables(stream);
            Action baseCleanup=()=>{host.RestoreStreamBases(stream);host.DestroyIosBase(stream);pathCleanup();};
            Protected(()=>host.ConstructFilebuf(stream),baseCleanup);
            Action filebufCleanup=()=>{host.DestroyFilebuf(stream);baseCleanup();};
            Protected(()=>host.InitializeIos(stream,true),filebufCleanup);
            bool opened=Protected(()=>host.Open(stream,pathData,0x30),filebufCleanup);
            uint state=opened?0:host.ReadIosState(stream)|4u;
            Protected(()=>host.ClearState(stream,state),filebufCleanup);
            Action streamCleanup=()=>{host.DestroyStream(stream);pathCleanup();};
            if((host.ReadFixedState(stream,0xa0)&5)!=0)
            {
                object errno=Known(host.ErrnoLocation()),faultPath=Known(host.ReadStringData(path));int error=host.ReadErrno(errno);object message=host.Strerror(error),filename=host.ReadFilenameData(0xd4);
                Protected(()=>host.Fault(16,Encoding.ASCII.GetBytes("%s write error:%s\n%s"),filename,message,faultPath),streamCleanup);
                throw new InvalidOperationException("Native nonreturning Fault binding returned");
            }
            object data=Known(host.ReadStringData(buffer));uint length=host.ReadStringLength(buffer);
            Protected(()=>host.InsertBuffer(stream,data,length),streamCleanup);
            bool closed=Protected(()=>host.Close(stream),streamCleanup);
            if(!closed){state=host.ReadIosState(stream)|4u;Protected(()=>host.ClearState(stream,state),streamCleanup);}
            // Normal inline ofstream destruction. Close is called a second time;
            // its return is ignored, and there is no later Fault state check.
            host.ResetDestructionVtables(stream);
            Action streambufCleanup=()=>{host.DestroyStreambufBase(stream);baseCleanup();};
            Protected(()=>host.Close(stream),()=>{host.DestroyBasicFile(stream);streambufCleanup();});
            Protected(()=>host.DestroyBasicFile(stream),streambufCleanup);
            host.ResetStreambufBase(stream);host.DestroyLocale(stream);host.RestoreStreamBases(stream);
            Protected(()=>host.DestroyIosBase(stream),pathCleanup);
            host.DestroyString(path);host.DestroyString(buffer);DestroySimpleInline(aggregate);
        }
    }
}
