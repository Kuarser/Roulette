using System;
namespace RouletteDeluxe
{
 // Rule string copy helpers082559b0/08255a20, deallocation08255990 and
 // vector<string> copy C1/C2 at082a0990. Imported lower owners are required.
 public sealed class OriginalConfigurationRuleConstruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,CopyString,DestroyRange;
   public Action<uint> DestroyString,Free,DeallocateVector;
   public Action ThrowBadAlloc,EndCatch,Rethrow;
   public Action<Exception> BeginCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationRuleConstruction(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.CopyString==null||h.DestroyRange==null||h.DestroyString==null||h.Free==null||h.DeallocateVector==null||h.ThrowBadAlloc==null||h.EndCatch==null||h.Rethrow==null||h.BeginCatch==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual rule string/vector memory/C1/range destructor/heap/deallocator/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,CopyString=h.CopyString,DestroyRange=h.DestroyRange,DestroyString=h.DestroyString,Free=h.Free,DeallocateVector=h.DeallocateVector,ThrowBadAlloc=h.ThrowBadAlloc,EndCatch=h.EndCatch,Rethrow=h.Rethrow,BeginCatch=h.BeginCatch,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private uint CopyRange(uint begin,uint end,uint destination)
  {
   uint current=destination;
   for(uint source=begin;source!=end;source=Add(source,4),current=Add(current,4))
   {
    if(current==0)continue;uint slot=current,original=source;
    Protected(()=>host.CopyString(slot,original),ex=>{
     host.BeginCatch(ex);for(uint at=destination;at!=slot;at=Add(at,4))host.DestroyString(at);
     Protected(host.Rethrow,next=>host.EndCatch());
     throw new InvalidOperationException("Native nonreturning rule copy rethrow binding returned");
    });
   }
   return current;
  }
  public uint Copy(uint begin,uint end,uint destination){return CopyRange(begin,end,destination);}
  public uint CopyConst(uint begin,uint end,uint destination){return CopyRange(begin,end,destination);}
  public void Deallocate(uint storage){if(storage!=0)host.Free(storage);}
  public void Construct(uint destination,uint source)
  {
   uint oldEnd=host.ReadWord(Add(source,4)),oldBegin=host.ReadWord(source);
   host.WriteWord(destination,0);host.WriteWord(Add(destination,4),0);
   uint count=unchecked((uint)(unchecked((int)(oldEnd-oldBegin))>>2));host.WriteWord(Add(destination,8),0);
   uint bytes=unchecked(count*4),storage=0;
   if(count!=0)
   {
    if(count>0x3fffffff){host.ThrowBadAlloc();throw new InvalidOperationException("Native nonreturning rule constructor bad_alloc binding returned");}
    storage=host.Allocate(bytes);
   }
   host.WriteWord(destination,storage);host.WriteWord(Add(destination,4),storage);host.WriteWord(Add(destination,8),Add(storage,bytes));
   uint end=host.ReadWord(Add(source,4)),begin=host.ReadWord(source),current=storage;
   for(uint at=begin;at!=end;at=Add(at,4),current=Add(current,4))
   {
    if(current==0)continue;uint slot=current,original=at;
    Protected(()=>host.CopyString(slot,original),ex=>{
     host.BeginCatch(ex);host.DestroyRange(storage,slot);
     Protected(host.Rethrow,next=>{host.EndCatch();host.DeallocateVector(host.ReadWord(destination));});
     throw new InvalidOperationException("Native nonreturning rule constructor rethrow binding returned");
    });
   }
   host.WriteWord(Add(destination,4),current);
  }
 }
}
