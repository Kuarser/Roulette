using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70 normal tail: 094b14d8 through native return.
 // Cleanup imports in this tail are outside native protected ranges.
 public sealed class OriginalConfigurationClientValueCleanup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
  }
  readonly Host h;
  public OriginalConfigurationClientValueCleanup(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null)throw new ArgumentException("Physical reply/working/prefixed vectors, COW memory and original cleanup imports required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke};
  }
  static uint A(uint at,int delta){return unchecked(at+(uint)delta);}
  uint R(uint at){return h.ReadWord(at);}
  void Call(uint method,params uint[] args){h.Invoke(method,args);}
  void Release(uint cell,uint allocator)
  {uint data=R(cell),rep=A(data,-12);if(rep!=0x9b482a0&&h.AtomicAdd(A(data,-4),-1)<=0)Call(0x80ab9f4,rep,allocator);}
  void DestroyVector(uint header,uint frame,int categoryAllocator,int valueAllocator,int nameAllocator)
  {
   uint end=R(A(header,4)),begin=R(header),at=begin;
   if(at!=end)
   {
    do{Release(A(at,8),A(frame,categoryAllocator));Release(A(at,4),A(frame,valueAllocator));Release(at,A(frame,nameAllocator));at=A(at,16);}while(at!=end);
    // Native destruction captures its end but reloads begin before delete.
    begin=R(header);
   }
   if(begin!=0)Call(0x80aabe4,begin);
  }
  public uint Finish(uint frame)
  {
   uint response=R(A(frame,-0x18c));if(response!=0)Call(0x80ae7f4,response);
   uint replies=A(frame,-0xb4),end=R(A(replies,4)),begin=R(replies);
   h.WriteWord(A(frame,-0xb8),0x9767ea0);
   h.WriteByte(A(frame,-0x198),unchecked((int)(end-begin))>>4!=0?(byte)1:(byte)0);
   // Preserve native timing: the reply range above is read before its vtable
   // write. DestroyVector uses the same physical header without another import.
   DestroyVector(replies,frame,-0x22,-0x21,-0x20);
   h.WriteWord(A(frame,-0xb8),0x9729ab8);Release(A(frame,-0x94),A(frame,-0x1f));
   DestroyVector(A(frame,-0xa8),frame,-0x1e,-0x1d,-0x1c);
   h.WriteWord(A(frame,-0xc8),0x9767ea0);
   DestroyVector(A(frame,-0xc4),frame,-0x1b,-0x1a,-0x19);
   return h.ReadByte(A(frame,-0x198));
  }
 }
}
