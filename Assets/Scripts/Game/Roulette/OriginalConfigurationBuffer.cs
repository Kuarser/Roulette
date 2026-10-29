using System;
namespace RouletteDeluxe
{
    // Complete CreateBufferFromConfig orchestration, 094b66b0 / 094a5c20.
    // Actual string, stream, locale, vector, heap and exception owners are required.
    public sealed class OriginalConfigurationBuffer
    {
        public sealed class Host
        {
            public Func<object,uint> StringLength;
            public Func<object> CreateStreamOwner;
            public Action<object> ConstructIosBase,InitializeInputBase,InitializeOutputBase,
                InitializeIostreamBase,InitializeFinalLayout,ConstructLocale,
                RestoreInputAfterFailedOutputInit,ResetIosBaseVtable,DestroyIosBase,
                ResetStringbufFinalVtable,ResetStreambufBase,DestroyLocale,
                ResetFinalStreamVtables,ClearInputGcount,DestroyStream;
            public Action<object,uint,uint> InitializeStringbuf;
            public Action<object,int> InitializeIos,RestoreBases;
            public Action<object,uint> DestroyIostream;
            public Func<object,object> OwnedString,OutputStream,ReadFacet;
            public Func<int,uint,object> CreateEmptyString;
            public Action<object,uint,uint> AssignLiteral;
            public Func<object,uint,uint> ReadVectorWord,ReadStreamWord;
            public Func<bool,uint,object> GetXml;
            public Func<object,byte> FacetInitialized,CachedWiden10;
            public Action<object> WidenInitialize;
            public Func<object,uint,byte,byte> VirtualWiden;
            public Action ThrowBadCast;
            public Func<object,int,object> Put;
            public Func<object,object> Flush;
            // InsertString uses the native string's stored length, including NUL.
            public Func<object,object,object> InsertString;
            public Func<uint,uint,int,object> ConstructRange;
            public Action<object,object> AssignString,AppendString;
            public Action<object> StringDestructor;
            public Func<object,OriginalNvBackendDestructor.Representation> ReadRepresentation;
            public Func<OriginalNvBackendDestructor.Representation,int> AtomicDecrement;
            public Action<OriginalNvBackendDestructor.Representation,int> DestroyRepresentation;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        public OriginalConfigurationBuffer(Host h)
        {
            if(h==null||h.StringLength==null||h.CreateStreamOwner==null||h.ConstructIosBase==null||h.InitializeInputBase==null||h.InitializeOutputBase==null||h.InitializeIostreamBase==null||h.InitializeFinalLayout==null||h.ConstructLocale==null||h.RestoreInputAfterFailedOutputInit==null||h.ResetIosBaseVtable==null||h.DestroyIosBase==null||h.ResetStringbufFinalVtable==null||h.ResetStreambufBase==null||h.DestroyLocale==null||h.ResetFinalStreamVtables==null||h.ClearInputGcount==null||h.DestroyStream==null||h.InitializeStringbuf==null||h.InitializeIos==null||h.RestoreBases==null||h.DestroyIostream==null||h.OwnedString==null||h.OutputStream==null||h.ReadFacet==null||h.CreateEmptyString==null||h.AssignLiteral==null||h.ReadVectorWord==null||h.ReadStreamWord==null||h.GetXml==null||h.FacetInitialized==null||h.CachedWiden10==null||h.WidenInitialize==null||h.VirtualWiden==null||h.ThrowBadCast==null||h.Put==null||h.Flush==null||h.InsertString==null||h.ConstructRange==null||h.AssignString==null||h.AppendString==null||h.StringDestructor==null||h.ReadRepresentation==null||h.AtomicDecrement==null||h.DestroyRepresentation==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual output/string/stream/locale/vector/XML/range/refcount/destruction/native exception bindings required");
            host=new Host{StringLength=h.StringLength,CreateStreamOwner=h.CreateStreamOwner,ConstructIosBase=h.ConstructIosBase,InitializeInputBase=h.InitializeInputBase,InitializeOutputBase=h.InitializeOutputBase,InitializeIostreamBase=h.InitializeIostreamBase,InitializeFinalLayout=h.InitializeFinalLayout,ConstructLocale=h.ConstructLocale,RestoreInputAfterFailedOutputInit=h.RestoreInputAfterFailedOutputInit,ResetIosBaseVtable=h.ResetIosBaseVtable,DestroyIosBase=h.DestroyIosBase,ResetStringbufFinalVtable=h.ResetStringbufFinalVtable,ResetStreambufBase=h.ResetStreambufBase,DestroyLocale=h.DestroyLocale,ResetFinalStreamVtables=h.ResetFinalStreamVtables,ClearInputGcount=h.ClearInputGcount,DestroyStream=h.DestroyStream,InitializeStringbuf=h.InitializeStringbuf,InitializeIos=h.InitializeIos,RestoreBases=h.RestoreBases,DestroyIostream=h.DestroyIostream,OwnedString=h.OwnedString,OutputStream=h.OutputStream,ReadFacet=h.ReadFacet,CreateEmptyString=h.CreateEmptyString,AssignLiteral=h.AssignLiteral,ReadVectorWord=h.ReadVectorWord,ReadStreamWord=h.ReadStreamWord,GetXml=h.GetXml,FacetInitialized=h.FacetInitialized,CachedWiden10=h.CachedWiden10,WidenInitialize=h.WidenInitialize,VirtualWiden=h.VirtualWiden,ThrowBadCast=h.ThrowBadCast,Put=h.Put,Flush=h.Flush,InsertString=h.InsertString,ConstructRange=h.ConstructRange,AssignString=h.AssignString,AppendString=h.AppendString,StringDestructor=h.StringDestructor,ReadRepresentation=h.ReadRepresentation,AtomicDecrement=h.AtomicDecrement,DestroyRepresentation=h.DestroyRepresentation,IsNativeUnwind=h.IsNativeUnwind};
        }
        private static object Known(object owner){return owner??throw new InvalidOperationException("Actual string/stream/ostream/range owner admission required");}
        private void Protected(Action call,Action cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private T Protected<T>(Func<T> call,Action cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
        private void Release(object value,int allocatorFrame)
        {
            var rep=host.ReadRepresentation(value);if(rep==null)throw new InvalidOperationException("Actual string representation admission required");
            if(!rep.IsSharedEmpty&&host.AtomicDecrement(rep)<=0)host.DestroyRepresentation(rep,allocatorFrame);
        }
        private void Dispose(object value,bool simple,int allocatorFrame)
        {if(simple)Release(value,allocatorFrame);else host.StringDestructor(value);}
        private object EndLine(object output,Action cleanup)
        {
            object facet=host.ReadFacet(output);byte widened;
            if(facet==null)
            {
                Protected(host.ThrowBadCast,cleanup);
                throw new InvalidOperationException("Native nonreturning bad_cast binding returned");
            }
            if(host.FacetInitialized(facet)==0)
            {
                Protected(()=>host.WidenInitialize(facet),cleanup);
                widened=Protected(()=>host.VirtualWiden(facet,0x18,10),cleanup);
            }
            else widened=host.CachedWiden10(facet);
            object result=Known(Protected(()=>host.Put(output,unchecked((sbyte)widened)),cleanup));
            return Known(Protected(()=>host.Flush(result),cleanup));
        }
        public bool CreateSimple(object output,object vector,byte include)
        {return Create(output,vector,include,true);}
        public bool CreateConfiguration(object output,object vector)
        {return Create(output,vector,0,false);}
        private bool Create(object output,object vector,byte include,bool simple)
        {
            if(output==null||vector==null)throw new ArgumentException("Actual output string and vector owners required");
            uint oldLength=host.StringLength(output);object stream=Known(host.CreateStreamOwner());
            host.ConstructIosBase(stream);host.InitializeInputBase(stream);
            Action iosCleanup=()=>{host.ResetIosBaseVtable(stream);host.DestroyIosBase(stream);};
            Protected(()=>host.InitializeIos(stream,0),iosCleanup);
            host.InitializeOutputBase(stream);
            Protected(()=>host.InitializeIos(stream,1),()=>{host.RestoreInputAfterFailedOutputInit(stream);iosCleanup();});
            host.InitializeIostreamBase(stream);host.InitializeFinalLayout(stream);host.ConstructLocale(stream);host.InitializeStringbuf(stream,0x18,0x09b482ac);
            Protected(()=>host.InitializeIos(stream,2),()=>{host.ResetStringbufFinalVtable(stream);host.StringDestructor(Known(host.OwnedString(stream)));host.ResetStreambufBase(stream);host.DestroyLocale(stream);host.DestroyIostream(stream,0x09b49de4);iosCleanup();});
            object prefix=null,suffix=null;
            if(simple){prefix=Known(host.CreateEmptyString(0xdc,0x09b482ac));suffix=Known(host.CreateEmptyString(0xe0,0x09b482ac));}
            Action streamCleanup=()=>{if(simple){host.StringDestructor(suffix);host.StringDestructor(prefix);}host.DestroyStream(stream);};
            uint item=host.ReadVectorWord(vector,0);
            if(item!=host.ReadVectorWord(vector,4))
            {
                do
                {
                    if(simple&&include!=0)
                    {
                        Protected(()=>host.AssignLiteral(prefix,0x0976867a,9),streamCleanup);
                        Protected(()=>host.AssignLiteral(suffix,0x09768684,10),streamCleanup);
                    }
                    object xml=Known(Protected(()=>host.GetXml(simple,item),streamCleanup));
                    Action xmlCleanup=()=>{host.StringDestructor(xml);streamCleanup();};
                    object current=EndLine(Known(host.OutputStream(stream)),xmlCleanup);
                    if(simple)current=Known(Protected(()=>host.InsertString(current,prefix),xmlCleanup));
                    current=Known(Protected(()=>host.InsertString(current,xml),xmlCleanup));
                    if(simple)current=Known(Protected(()=>host.InsertString(current,suffix),xmlCleanup));
                    EndLine(current,xmlCleanup);
                    Dispose(xml,simple,0xf3);
                    item=unchecked(item+(simple?16u:132u));
                }while(item!=host.ReadVectorWord(vector,4));
            }
            object extracted=Known(host.CreateEmptyString(simple?0xe8:0xe0,0x09b482ac));
            Action resultCleanup=()=>{host.StringDestructor(extracted);streamCleanup();};
            uint put=host.ReadStreamWord(stream,0x20);
            if(put==0)
            {
                object owned=Known(host.OwnedString(stream));
                Protected(()=>host.AssignString(extracted,owned),resultCleanup);
            }
            else
            {
                uint getEnd=host.ReadStreamWord(stream,0x18),putBase=host.ReadStreamWord(stream,0x1c);bool above=put>getEnd;
                int frame=simple?(above?0xf0:0xec):(above?0xe8:0xe4);
                object range=Known(Protected(()=>host.ConstructRange(putBase,above?put:getEnd,frame),resultCleanup));
                Protected(()=>host.AssignString(extracted,range),()=>{host.StringDestructor(range);resultCleanup();});
                Dispose(range,simple,above?0xfa:0xfb);
            }
            Protected(()=>host.AppendString(output,extracted),resultCleanup);
            Dispose(extracted,simple,0xfc);
            bool grew=oldLength<host.StringLength(output);
            if(simple){Release(suffix,0xfd);Release(prefix,0xfe);}
            host.ResetFinalStreamVtables(stream);
            Dispose(Known(host.OwnedString(stream)),simple,0xff);
            host.ResetStreambufBase(stream);host.DestroyLocale(stream);
            host.RestoreBases(stream,2);host.RestoreBases(stream,1);host.RestoreBases(stream,0);
            host.ClearInputGcount(stream);host.ResetIosBaseVtable(stream);host.DestroyIosBase(stream);
            return grew;
        }
    }
}
