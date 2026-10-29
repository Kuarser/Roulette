using System;
namespace RouletteDeluxe
{
 // Actual regparm3 insert_aux 08dfbe20 and uninitialized_move_a 08dfbd60.
 // Native headers, field owners, heap, catch/rethrow and stack storage required.
 public sealed class OriginalConfigurationInsertAux
 {
  public sealed class Host
  {
   public Func<object,uint,uint> ReadWord;
   public Action<object,uint,uint> WriteWord;
   public Func<uint,uint> Allocate;
   public Func<int,uint> TemporarySimpleAddress;
   public Action<uint,uint> CopyString,AssignString,CopyHash;
   public Action<uint> DestroyString,DestroySimple,Free;
   public Action<Exception> BeginCatch;
   public Action EndCatch,Rethrow;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationInsertAux(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.Allocate==null||h.TemporarySimpleAddress==null||h.CopyString==null||h.AssignString==null||h.CopyHash==null||h.DestroyString==null||h.DestroySimple==null||h.Free==null||h.BeginCatch==null||h.EndCatch==null||h.Rethrow==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual native vector/string/heap/stack/catch/rethrow/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Allocate=h.Allocate,TemporarySimpleAddress=h.TemporarySimpleAddress,CopyString=h.CopyString,AssignString=h.AssignString,CopyHash=h.CopyHash,DestroyString=h.DestroyString,DestroySimple=h.DestroySimple,Free=h.Free,BeginCatch=h.BeginCatch,EndCatch=h.EndCatch,Rethrow=h.Rethrow,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Count(uint begin,uint end){return unchecked((uint)(unchecked((int)(end-begin))>>4));}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private T Protected<T>(Func<T> call,Action<Exception> cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void CatchRethrow(Exception ex,Action cleanup)
  {
   host.BeginCatch(ex);cleanup();Protected(host.Rethrow,next=>host.EndCatch());
   throw new InvalidOperationException("Native nonreturning rethrow binding returned");
  }
  private void DestroyRange(uint begin,uint end){for(uint at=begin;at!=end;at=Add(at,16))host.DestroySimple(at);}
  public uint UninitializedMove(uint begin,uint end,uint destination)
  {
   uint current=destination;
   for(uint source=begin;source!=end;source=Add(source,16),current=Add(current,16))
   {
    if(current==0)continue;uint slot=current;
    Action<Exception> cleanup=ex=>CatchRethrow(ex,()=>DestroyRange(destination,slot));
    Protected(()=>host.CopyString(slot,source),cleanup);
    Protected(()=>host.CopyString(Add(slot,4),Add(source,4)),ex=>{host.DestroyString(slot);cleanup(ex);});
    Protected(()=>host.CopyString(Add(slot,8),Add(source,8)),ex=>{host.DestroyString(Add(slot,4));host.DestroyString(slot);cleanup(ex);});
    host.CopyHash(Add(slot,12),Add(source,12));
   }
   return current;
  }
  public void Insert(object vector,uint position,uint value)
  {
   if(vector==null)throw new ArgumentException("Actual vector owner required");
   uint end=host.ReadWord(vector,4);
   if(end!=host.ReadWord(vector,8))
   {
    uint cloned=end;
    if(end!=0)
    {
     host.CopyString(cloned,Add(cloned,0xfffffff0));
     Protected(()=>host.CopyString(Add(cloned,4),Add(cloned,0xfffffff4)),ex=>host.DestroyString(cloned));
     Protected(()=>host.CopyString(Add(cloned,8),Add(cloned,0xfffffff8)),ex=>{host.DestroyString(Add(cloned,4));host.DestroyString(cloned);});
     host.CopyHash(Add(cloned,12),Add(cloned,0xfffffffc));end=host.ReadWord(vector,4);
    }
    host.WriteWord(vector,4,Add(end,16));
    uint temporary=host.TemporarySimpleAddress(0x20);
    host.CopyString(temporary,value);
    Protected(()=>host.CopyString(Add(temporary,4),Add(value,4)),ex=>host.DestroyString(temporary));
    Protected(()=>host.CopyString(Add(temporary,8),Add(value,8)),ex=>{host.DestroyString(Add(temporary,4));host.DestroyString(temporary);});
    uint newEnd=host.ReadWord(vector,4);host.CopyHash(Add(temporary,12),Add(value,12));
    int count=unchecked((int)(Add(newEnd,0xffffffe0)-position))>>4;uint current=Add(newEnd,0xfffffff0);
    Action<Exception> temporaryCleanup=ex=>host.DestroySimple(temporary);
    for(int i=0;i<count;++i)
    {
     uint target=Add(current,0xfffffff0),source=Add(current,0xffffffe0);
     Protected(()=>host.AssignString(target,source),temporaryCleanup);
     Protected(()=>host.AssignString(Add(target,4),Add(source,4)),temporaryCleanup);
     Protected(()=>host.AssignString(Add(target,8),Add(source,8)),temporaryCleanup);
     host.CopyHash(Add(target,12),Add(source,12));current=target;
    }
    Protected(()=>host.AssignString(position,temporary),temporaryCleanup);
    Protected(()=>host.AssignString(Add(position,4),Add(temporary,4)),temporaryCleanup);
    Protected(()=>host.AssignString(Add(position,8),Add(temporary,8)),temporaryCleanup);
    host.CopyHash(Add(position,12),Add(temporary,12));
    host.DestroyString(Add(temporary,8));host.DestroyString(Add(temporary,4));host.DestroyString(temporary);return;
   }
   uint begin=host.ReadWord(vector,0),size=Count(begin,end),capacity;
   if(size==0)capacity=1;else {uint doubled=unchecked(size*2);capacity=size<=doubled&&doubled<=0x0fffffff?doubled:0x0fffffff;}
   uint index=Count(begin,position),allocated=capacity==0?0:host.Allocate(unchecked(capacity<<4)),inserted=Add(allocated,unchecked(index<<4));
   Action<Exception> insertedCleanup=ex=>CatchRethrow(ex,()=>{if(allocated==0)host.DestroySimple(inserted);else host.Free(allocated);});
   if(inserted!=0)
   {
    Protected(()=>host.CopyString(inserted,value),insertedCleanup);
    Protected(()=>host.CopyString(Add(inserted,4),Add(value,4)),ex=>{host.DestroyString(inserted);insertedCleanup(ex);});
    Protected(()=>host.CopyString(Add(inserted,8),Add(value,8)),ex=>{host.DestroyString(Add(inserted,4));host.DestroyString(inserted);insertedCleanup(ex);});
    host.CopyHash(Add(inserted,12),Add(value,12));
   }
   uint prefixBegin=host.ReadWord(vector,0);
   uint prefixEnd=Protected(()=>UninitializedMove(prefixBegin,position,allocated),ex=>CatchRethrow(ex,()=>{host.DestroySimple(inserted);if(allocated!=0)host.Free(allocated);}));
   uint marker=Add(prefixEnd,16),suffixEnd=host.ReadWord(vector,4);
   uint resultEnd=Protected(()=>UninitializedMove(position,suffixEnd,marker),ex=>CatchRethrow(ex,()=>{if(marker==0)host.DestroySimple(inserted);else DestroyRange(allocated,marker);if(allocated!=0)host.Free(allocated);}));
   uint oldEnd=host.ReadWord(vector,4),oldBegin=host.ReadWord(vector,0);
   if(oldBegin!=oldEnd)
   {
    for(uint at=oldBegin;at!=oldEnd;at=Add(at,16)){host.DestroyString(Add(at,8));host.DestroyString(Add(at,4));host.DestroyString(at);}
    oldBegin=host.ReadWord(vector,0);
   }
   if(oldBegin!=0)host.Free(oldBegin);
   host.WriteWord(vector,0,allocated);host.WriteWord(vector,4,resultEnd);host.WriteWord(vector,8,Add(allocated,unchecked(capacity<<4)));
  }
 }
}
