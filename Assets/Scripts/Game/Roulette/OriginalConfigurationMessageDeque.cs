using System;
namespace RouletteDeluxe
{
 // Full MsgPacket deque InitializeMap08e0a440 and D10834f100.
 public sealed class OriginalConfigurationMessageDeque
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord;
   public Action<uint> Free;
   public Action ThrowBadAlloc,Rethrow,EndCatch;
   public Action<Exception> BeginCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationMessageDeque(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.Free==null||h.ThrowBadAlloc==null||h.Rethrow==null||h.EndCatch==null||h.BeginCatch==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual message deque memory/heap/bad_alloc/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,Free=h.Free,ThrowBadAlloc=h.ThrowBadAlloc,Rethrow=h.Rethrow,EndCatch=h.EndCatch,BeginCatch=h.BeginCatch,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void Cleanup(Exception ex,uint owner,uint first,uint current)
  {
   host.BeginCatch(ex);for(uint at=first;at<current;at=Add(at,4))host.Free(host.ReadWord(at));
   Protected(host.Rethrow,next=>{
    host.EndCatch();host.BeginCatch(next);host.Free(host.ReadWord(owner));host.WriteWord(owner,0);host.WriteWord(Add(owner,4),0);
    Protected(host.Rethrow,last=>host.EndCatch());throw new InvalidOperationException("Native nonreturning deque outer rethrow binding returned");
   });
   throw new InvalidOperationException("Native nonreturning deque node rethrow binding returned");
  }
  public void Initialize(uint owner,uint count)
  {
   uint mapSize=Add(count,3),bytes;
   if(mapSize<=8){host.WriteWord(Add(owner,4),8);bytes=32;}
   else{host.WriteWord(Add(owner,4),mapSize);if(mapSize>0x3fffffff){host.ThrowBadAlloc();throw new InvalidOperationException("Native nonreturning deque bad_alloc binding returned");}bytes=unchecked(mapSize*4);}
   uint nodes=Add(count,1),map=host.Allocate(bytes),actualSize=host.ReadWord(Add(owner,4)),first=Add(map,unchecked((unchecked(actualSize-nodes)>>1)*4)),end=Add(first,unchecked(nodes*4)),current=first;
   host.WriteWord(owner,map);
   while(current<end)
   {
    uint slot=current,node=0;Protected(()=>node=host.Allocate(0xdb4),ex=>Cleanup(ex,owner,first,slot));host.WriteWord(slot,node);current=Add(current,4);
   }
   host.WriteWord(Add(owner,0x14),first);uint beginNode=host.ReadWord(first);host.WriteWord(Add(owner,0x10),Add(beginNode,0xdb4));host.WriteWord(Add(owner,0xc),beginNode);host.WriteWord(Add(owner,0x24),Add(end,0xfffffffc));uint endNode=host.ReadWord(Add(end,0xfffffffc));host.WriteWord(Add(owner,8),beginNode);host.WriteWord(Add(owner,0x1c),endNode);host.WriteWord(Add(owner,0x20),Add(endNode,0xdb4));host.WriteWord(Add(owner,0x18),endNode);
  }
  public void Destroy(uint owner)
  {
   uint map=host.ReadWord(owner);if(map==0)return;uint limit=Add(host.ReadWord(Add(owner,0x24)),4),first=host.ReadWord(Add(owner,0x14));
   for(uint at=first;at<limit;at=Add(at,4))host.Free(host.ReadWord(at));if(first<limit)map=host.ReadWord(owner);host.Free(map);
  }
 }
}
