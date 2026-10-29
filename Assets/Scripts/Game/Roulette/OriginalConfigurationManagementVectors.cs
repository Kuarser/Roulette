using System;
namespace RouletteDeluxe
{
    // Native 16-byte SimpleConfigOption vector assignment and end insertion
    // in ConfigMgmtObj::WriteConfigOptionsToFile, including actual copy helpers.
    public sealed class OriginalConfigurationManagementVectors
    {
        public sealed class Host
        {
            public Func<object,uint,uint> ReadWord;
            public Action<object,uint,uint> WriteWord;
            public Func<uint,uint> Allocate;
            public Action<uint> Free,DestroyString;
            public Action<uint,uint> CopyConstructString,AssignString,CopyHash,DestroyRange;
            public Action ThrowBadAlloc;
            public Action<uint> ThrowLengthError;
            public Action<Exception> BeginCatch;
            public Action EndCatch,Rethrow;
            public Func<Exception,bool> IsNativeUnwind;
        }
        private readonly Host host;
        public OriginalConfigurationManagementVectors(Host h)
        {
            if(h==null||h.ReadWord==null||h.WriteWord==null||h.Allocate==null||h.Free==null||h.DestroyString==null||h.CopyConstructString==null||h.AssignString==null||h.CopyHash==null||h.DestroyRange==null||h.ThrowBadAlloc==null||h.ThrowLengthError==null||h.BeginCatch==null||h.EndCatch==null||h.Rethrow==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual vector header, native heap, string/hash, range destruction, catch/rethrow and exception bindings required");
            host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Allocate=h.Allocate,Free=h.Free,DestroyString=h.DestroyString,CopyConstructString=h.CopyConstructString,AssignString=h.AssignString,CopyHash=h.CopyHash,DestroyRange=h.DestroyRange,ThrowBadAlloc=h.ThrowBadAlloc,ThrowLengthError=h.ThrowLengthError,BeginCatch=h.BeginCatch,EndCatch=h.EndCatch,Rethrow=h.Rethrow,IsNativeUnwind=h.IsNativeUnwind};
        }
        private static uint Add(uint a,uint b){return unchecked(a+b);}
        private static uint Count(uint begin,uint end){return unchecked((uint)(unchecked((int)(end-begin))>>4));}
        private void Protected(Action call,Action<Exception> cleanup)
        {try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
        private T Protected<T>(Func<T> call,Action<Exception> cleanup)
        {try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
        // Only native rethrow has the end-catch landing pad. Begin-catch and
        // cleanup calls do not receive an invented finally scope.
        private void CatchRethrow(Exception ex,Action cleanup,Action<Exception> after)
        {
            host.BeginCatch(ex);cleanup();
            Protected(host.Rethrow,next=>{host.EndCatch();after(next);});
            throw new InvalidOperationException("Native nonreturning rethrow binding returned");
        }
        private void Construct(uint source,uint destination,Action<Exception,int> failure)
        {
            Protected(()=>host.CopyConstructString(destination,source),ex=>failure(ex,0));
            Protected(()=>host.CopyConstructString(Add(destination,4),Add(source,4)),ex=>failure(ex,1));
            Protected(()=>host.CopyConstructString(Add(destination,8),Add(source,8)),ex=>failure(ex,2));
            host.CopyHash(Add(destination,12),Add(source,12));
        }
        private void Partial(uint destination,int stage)
        {if(stage==2)host.DestroyString(Add(destination,4));if(stage>=1)host.DestroyString(destination);}
        // Both 08e80b00 and 08e80bb0 have this body and native catch-all scope.
        public uint UninitializedCopy(uint begin,uint end,uint destination)
        {
            uint current=destination;
            for(uint source=begin;source!=end;source=Add(source,16),current=Add(current,16))
            {
                if(current==0)continue;
                uint slot=current;
                Construct(source,slot,(ex,stage)=>{Partial(slot,stage);CatchRethrow(ex,()=>host.DestroyRange(destination,slot),next=>{});});
            }
            return current;
        }
        // 08df1f70: signed SAR count, three string assignments, raw hash copy.
        public uint CopyMove(uint begin,uint end,uint destination)
        {
            int count=unchecked((int)(end-begin))>>4;uint source=begin,current=destination;
            for(int i=0;i<count;++i)
            {
                host.AssignString(current,source);host.AssignString(Add(current,4),Add(source,4));host.AssignString(Add(current,8),Add(source,8));host.CopyHash(Add(current,12),Add(source,12));source=Add(source,16);current=Add(current,16);
            }
            return Add(destination,unchecked((uint)(count<<4)));
        }
        private void DestroyInline(uint begin,uint end)
        {for(uint at=begin;at!=end;at=Add(at,16)){host.DestroyString(Add(at,8));host.DestroyString(Add(at,4));host.DestroyString(at);}}
        public void Assign(object local,object converted,Action<Exception> callerUnwind)
        {
            if(local==null||converted==null||callerUnwind==null)throw new ArgumentException("Actual local/converted vector owners and caller unwind required");
            uint sourceEnd=host.ReadWord(converted,4),source=host.ReadWord(converted,0),begin=host.ReadWord(local,0),count=Count(source,sourceEnd),capacity=Count(begin,host.ReadWord(local,8));
            if(count>capacity)
            {
                if(count>0x0fffffff){Protected(host.ThrowBadAlloc,callerUnwind);throw new InvalidOperationException("Native nonreturning bad_alloc binding returned");}
                uint allocated=count==0?0:Protected(()=>host.Allocate(unchecked(count<<4)),callerUnwind),current=allocated;
                for(uint at=source;at!=sourceEnd;at=Add(at,16),current=Add(current,16))
                {
                    if(current==0)continue;uint slot=current;
                    Construct(at,slot,(ex,stage)=>{Partial(slot,stage);CatchRethrow(ex,()=>host.DestroyRange(allocated,slot),next=>CatchRethrow(next,()=>{if(allocated!=0)host.Free(allocated);},callerUnwind));});
                }
                uint oldEnd=host.ReadWord(local,4),oldBegin=host.ReadWord(local,0);host.DestroyRange(oldBegin,oldEnd);oldBegin=host.ReadWord(local,0);if(oldBegin!=0)host.Free(oldBegin);
                host.WriteWord(local,0,allocated);host.WriteWord(local,8,Add(allocated,unchecked(count<<4)));
            }
            else
            {
                uint oldEnd=host.ReadWord(local,4),size=Count(begin,oldEnd);
                if(count>size)
                {
                    uint middle=Add(source,unchecked(size<<4));Protected(()=>CopyMove(source,middle,begin),callerUnwind);
                    uint tail=host.ReadWord(local,4),end=host.ReadWord(converted,4),localBegin=host.ReadWord(local,0),start=Add(host.ReadWord(converted,0),unchecked((tail-localBegin)&0xfffffff0));
                    Protected(()=>UninitializedCopy(start,end,tail),callerUnwind);
                }
                else
                {
                    uint current=begin,at=source;
                    for(int i=unchecked((int)count);i>0;--i)
                    {
                        uint dst=current,src=at;Protected(()=>host.AssignString(dst,src),callerUnwind);Protected(()=>host.AssignString(Add(dst,4),Add(src,4)),callerUnwind);Protected(()=>host.AssignString(Add(dst,8),Add(src,8)),callerUnwind);host.CopyHash(Add(dst,12),Add(src,12));current=Add(current,16);at=Add(at,16);
                    }
                    DestroyInline(current,oldEnd);
                }
            }
            host.WriteWord(local,4,Add(host.ReadWord(local,0),unchecked(count<<4)));
        }
        public void Append(object aggregate,object local,Action<Exception> callerUnwind)
        {
            if(aggregate==null||local==null||callerUnwind==null)throw new ArgumentException("Actual aggregate/local vector owners and caller unwind required");
            uint source=host.ReadWord(local,0),sourceEnd=host.ReadWord(local,4),position=host.ReadWord(aggregate,4);
            if(source==sourceEnd)return;
            uint count=Count(source,sourceEnd),capacity=host.ReadWord(aggregate,8);
            if(Count(position,capacity)>=count)
            {
                Protected(()=>UninitializedCopy(source,sourceEnd,position),callerUnwind);
                host.WriteWord(aggregate,4,Add(host.ReadWord(aggregate,4),unchecked(count<<4)));
                // End insertion still calls the two empty copy helpers.
                Protected(()=>UninitializedCopy(position,position,Add(position,unchecked(count<<4))),callerUnwind);
                Protected(()=>CopyMove(source,source,position),callerUnwind);
                return;
            }
            uint oldBegin=host.ReadWord(aggregate,0),size=Count(oldBegin,position);
            if(unchecked(0x0fffffff-size)<count){Protected(()=>host.ThrowLengthError(0x096cd20a),callerUnwind);throw new InvalidOperationException("Native nonreturning length_error binding returned");}
            uint growth=size>count?size:count;ulong sum=(ulong)size+growth;uint newCapacity=sum>0x0fffffff?0x0fffffff:(uint)sum;
            uint allocated=newCapacity==0?0:Protected(()=>host.Allocate(unchecked(newCapacity<<4)),callerUnwind),copied=allocated;
            Action<Exception> fail=ex=>CatchRethrow(ex,()=>{host.DestroyRange(allocated,copied);if(allocated!=0)host.Free(allocated);},callerUnwind);
            copied=Protected(()=>UninitializedCopy(oldBegin,position,allocated),fail);
            uint prefixEnd=copied;copied=Protected(()=>UninitializedCopy(source,sourceEnd,prefixEnd),fail);
            uint insertedEnd=copied,oldEnd=host.ReadWord(aggregate,4);copied=Protected(()=>UninitializedCopy(position,oldEnd,insertedEnd),fail);
            uint destroyEnd=host.ReadWord(aggregate,4),destroyBegin=host.ReadWord(aggregate,0);host.DestroyRange(destroyBegin,destroyEnd);uint released=host.ReadWord(aggregate,0);if(released!=0)host.Free(released);
            host.WriteWord(aggregate,0,allocated);host.WriteWord(aggregate,4,copied);host.WriteWord(aggregate,8,Add(allocated,unchecked(newCapacity<<4)));
        }
    }
}
