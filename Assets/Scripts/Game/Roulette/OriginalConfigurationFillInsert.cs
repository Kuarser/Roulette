using System;
namespace RouletteDeluxe
{
 // Complete vector<ConfigurationOption>::fill_insert 0949f000, 3062 bytes.
 // Every helper is an actual owner binding; FrameAddress resolves native locals.
 public sealed class OriginalConfigurationFillInsert
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,CopyConstructor,Assign,EraseAllowedTree,EraseOrderTree;
   public Action<uint,uint,uint> UninitializedFill;
   public Func<uint,uint,uint,uint> UninitializedMove;
   public Action<uint> InvokeConfigurationDestructor,DestroyConfiguration,Free,ThrowLengthError;
   public Func<int,uint> FrameAddress;
   public Func<uint,OriginalNvBackendDestructor.Representation> ReadRepresentation;
   public Func<OriginalNvBackendDestructor.Representation,int> AtomicDecrement;
   public Action<OriginalNvBackendDestructor.Representation,uint> DestroyRepresentation;
   public Action<Exception> BeginCatch;
   public Action EndCatch,Rethrow;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationFillInsert(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.CopyConstructor==null||h.Assign==null||h.EraseAllowedTree==null||h.EraseOrderTree==null||h.UninitializedFill==null||h.UninitializedMove==null||h.InvokeConfigurationDestructor==null||h.DestroyConfiguration==null||h.Free==null||h.ThrowLengthError==null||h.FrameAddress==null||h.ReadRepresentation==null||h.AtomicDecrement==null||h.DestroyRepresentation==null||h.BeginCatch==null||h.EndCatch==null||h.Rethrow==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual vector/frame/C1/assignment/uninitialized copy/heap/tree/string representation/destructor/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,CopyConstructor=h.CopyConstructor,Assign=h.Assign,EraseAllowedTree=h.EraseAllowedTree,EraseOrderTree=h.EraseOrderTree,UninitializedFill=h.UninitializedFill,UninitializedMove=h.UninitializedMove,InvokeConfigurationDestructor=h.InvokeConfigurationDestructor,DestroyConfiguration=h.DestroyConfiguration,Free=h.Free,ThrowLengthError=h.ThrowLengthError,FrameAddress=h.FrameAddress,ReadRepresentation=h.ReadRepresentation,AtomicDecrement=h.AtomicDecrement,DestroyRepresentation=h.DestroyRepresentation,BeginCatch=h.BeginCatch,EndCatch=h.EndCatch,Rethrow=h.Rethrow,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Bytes(uint n){return unchecked(n*132);}
  private static uint Count(uint end,uint begin){return unchecked((uint)(unchecked((int)(end-begin))>>2)*0x3e0f83e1);}
  private void Protected(Action call,Action cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  private T Protected<T>(Func<T> call,Action cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  private uint Frame(int offset){uint at=host.FrameAddress(offset);if(at==0)throw new InvalidOperationException("Actual fill_insert frame admission required");return at;}
  private void Release(uint slot,int allocator)
  {
   var rep=host.ReadRepresentation(slot);if(rep==null)throw new InvalidOperationException("Actual fill_insert representation admission required");
   // Non-PIC original tests immediate080b0f04, so only the atomic path is live.
   if(!rep.IsSharedEmpty&&host.AtomicDecrement(rep)<=0)host.DestroyRepresentation(rep,Frame(allocator));
  }
  private void DestroyInline(uint temp)
  {
   uint end=host.ReadWord(Add(temp,104)),begin=host.ReadWord(Add(temp,100));host.WriteWord(temp,0x097681f8);
   for(uint at=begin;at!=end;at=Add(at,4))Release(at,0xd9);
   if(begin!=end)begin=host.ReadWord(Add(temp,100));
   Action cleanup=()=>host.DestroyConfiguration(temp);
   if(begin!=0)Protected(()=>host.Free(begin),cleanup);
   uint root=host.ReadWord(Add(temp,80));Protected(()=>host.EraseAllowedTree(Add(temp,72),root),cleanup);
   root=host.ReadWord(Add(temp,56));Protected(()=>host.EraseOrderTree(Add(temp,48),root),cleanup);
   int allocator=0xda;foreach(uint off in new[]{36u,28u,20u,12u,8u,4u})Release(Add(temp,off),allocator++);
  }
  private void GrowFailure(Exception ex,uint storage,uint marker,uint inserted,uint index,uint count)
  {
   host.BeginCatch(ex);
   if(marker!=0)
   {
    for(uint at=storage;at!=marker;at=Add(at,132)){uint option=at;Protected(()=>host.InvokeConfigurationDestructor(option),host.EndCatch);}
   }
   else
   {
    uint end=Add(storage,Bytes(Add(count,index)));
    for(uint at=inserted;at!=end;at=Add(at,132)){uint option=at;Protected(()=>host.InvokeConfigurationDestructor(option),host.EndCatch);}
   }
   if(storage!=0)host.Free(storage);
   Protected(host.Rethrow,host.EndCatch);
   throw new InvalidOperationException("Native nonreturning fill_insert rethrow binding returned");
  }
  private void GrowCall(Action call,Action<Exception> cleanup)
  {try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private T GrowCall<T>(Func<T> call,Action<Exception> cleanup)
  {try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  public void Insert(uint vector,uint position,uint count,uint source)
  {
   if(count==0)return;
   uint end=host.ReadWord(Add(vector,4)),capacity=host.ReadWord(Add(vector,8));
   if(count<=Count(capacity,end))
   {
    uint temp=Frame(0x54);host.CopyConstructor(temp,source);
    uint oldEnd=host.ReadWord(Add(vector,4)),tail=Count(oldEnd,position);Action cleanup=()=>host.DestroyConfiguration(temp);
    if(count<tail)
    {
     uint bytes=Bytes(count),split=unchecked(oldEnd-bytes);
     Protected(()=>host.UninitializedMove(split,oldEnd,oldEnd),cleanup);
     host.WriteWord(Add(vector,4),Add(host.ReadWord(Add(vector,4)),bytes));
     int backward=unchecked((int)Count(split,position));
     uint dst=unchecked(oldEnd-132),src=unchecked(split-132);
     for(;backward>0;--backward,dst=unchecked(dst-132),src=unchecked(src-132)){uint d=dst,s=src;Protected(()=>host.Assign(d,s),cleanup);}
     uint filledEnd=Add(position,bytes);
     for(uint at=position;at!=filledEnd;at=Add(at,132)){uint option=at;Protected(()=>host.Assign(option,temp),cleanup);}
    }
    else
    {
     uint extra=unchecked(count-tail);Protected(()=>host.UninitializedFill(oldEnd,extra,temp),cleanup);
     uint movedTo=Add(host.ReadWord(Add(vector,4)),Bytes(extra));host.WriteWord(Add(vector,4),movedTo);
     Protected(()=>host.UninitializedMove(position,oldEnd,movedTo),cleanup);
     host.WriteWord(Add(vector,4),Add(host.ReadWord(Add(vector,4)),Bytes(tail)));
     for(uint at=position;at!=oldEnd;at=Add(at,132)){uint option=at;Protected(()=>host.Assign(option,temp),cleanup);}
    }
    DestroyInline(temp);return;
   }
   uint begin=host.ReadWord(vector),current=Count(end,begin);
   if(unchecked(0x01f07c1f-current)<count){host.ThrowLengthError(0x096a29d3);throw new InvalidOperationException("Native nonreturning fill_insert length_error binding returned");}
   uint increment=current<count?count:current,newCapacity=Add(current,increment);
   bool saturated=newCapacity<current||newCapacity>0x01f07c1f;
   if(saturated)newCapacity=0x01f07c1f;
   uint index=Count(position,begin),storage=newCapacity==0?0:host.Allocate(saturated?0xfffffffc:Bytes(newCapacity));
   uint inserted=Add(storage,Bytes(index));
   GrowCall(()=>host.UninitializedFill(inserted,count,source),ex=>GrowFailure(ex,storage,storage,inserted,index,count));
   uint prefixSource=host.ReadWord(vector);
   uint prefix=GrowCall(()=>host.UninitializedMove(prefixSource,position,storage),ex=>GrowFailure(ex,storage,0,inserted,index,count));
   uint suffixAt=Add(prefix,Bytes(count));
   uint sourceEnd=host.ReadWord(Add(vector,4));
   uint suffixEnd=GrowCall(()=>host.UninitializedMove(position,sourceEnd,suffixAt),ex=>GrowFailure(ex,storage,suffixAt,inserted,index,count));
   end=host.ReadWord(Add(vector,4));begin=host.ReadWord(vector);
   if(begin!=end){for(uint at=begin;at!=end;at=Add(at,132))host.InvokeConfigurationDestructor(at);begin=host.ReadWord(vector);}
   if(begin!=0)host.Free(begin);
   host.WriteWord(vector,storage);host.WriteWord(Add(vector,4),suffixEnd);host.WriteWord(Add(vector,8),Add(storage,Bytes(newCapacity)));
  }
 }
}
