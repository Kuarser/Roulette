using System;
namespace RouletteDeluxe
{
 // Exact public-memory operation order of packaged ios/locale lifetime bodies.
 // Imported locale, callback, cache and heap owners remain required.
 public sealed class OriginalConfigurationStreamLifetime
 {
  public sealed class Host {
   public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord;public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;public Func<uint,uint[],uint> Invoke;
  }
  readonly Host h;readonly uint iosVtableSlot,threadSlot;
  public OriginalConfigurationStreamLifetime(Host host,uint vtableSlot,uint pthreadSlot) {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||vtableSlot==0||pthreadSlot==0)throw new ArgumentException("Physical memory, relocated ios vtable/thread slots and actual imported lifetime owners required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke};iosVtableSlot=vtableSlot;threadSlot=pthreadSlot;
  }
  static uint A(uint a,uint b){return unchecked(a+b);}uint R(uint a){return h.ReadWord(a);}void W(uint a,uint v){h.WriteWord(a,v);}uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  // ios_base C2 00057e10: inline word array and locale are initialized in order.
  public void ConstructBase(uint owner) {
   uint words=A(owner,0x24),vtable=R(iosVtableSlot),end=A(owner,0x64);
   W(A(owner,4),0);W(A(owner,8),0);W(A(owner,12),0);vtable=A(vtable,8);W(owner,vtable);
   W(A(owner,16),0);W(A(owner,20),0);W(A(owner,24),0);W(A(owner,28),0);W(A(owner,32),0);
   for(uint at=words;at!=end;at=A(at,8)){W(at,0);W(A(at,4),0);}
   W(A(owner,0x68),words);W(A(owner,0x64),8);Call(0x48bc0,A(owner,0x6c));
  }
  // basic_ios::init 00071b80 retains the supplied buffer through callbacks.
  public void Initialize(uint owner,uint buffer) {
   Call(0x49ef0,owner);Call(0x49f90,owner,A(owner,0x6c));uint state=buffer==0?1u:0u;
   h.WriteByte(A(owner,0x74),0);W(A(owner,0x78),buffer);h.WriteByte(A(owner,0x75),0);W(A(owner,0x70),0);W(A(owner,0x10),0);W(A(owner,0x14),state);
  }
  // ios_base D2 000581c0 reloads callback-modified word storage.
  public void DestroyBase(uint owner) {
   W(owner,A(R(iosVtableSlot),8));Call(0x49380,owner,0);Call(0x480c0,owner);
   uint words=R(A(owner,0x68));if(words!=A(owner,0x24)){if(words!=0)Call(0x48270,words);W(A(owner,0x68),0);}Call(0x49990,A(owner,0x6c));
  }
  // locale D1 00059920 destroys Impl only when the previous count equals one.
  public void DestroyLocale(uint owner) {
   uint impl=R(owner);int previous;
   if(R(threadSlot)!=0)previous=h.AtomicAdd(impl,-1);else{uint old=R(impl);W(impl,unchecked(old-1));previous=unchecked((int)old);}
   if(previous==1&&impl!=0){Call(0x485b0,impl);Call(0x48900,impl);}
  }
  // basic_istream D2 00072c50 includes the two observable writes to owner+8.
  public void DestroyIstream(uint owner,uint vtt) {
   uint table=R(vtt);W(owner,table);uint offset=R(unchecked(table-12)),baseTable=R(A(vtt,0x14));W(A(owner,offset),baseTable);
   table=R(A(vtt,0x18));W(A(owner,8),table);table=R(A(vtt,0xc));W(A(owner,8),table);
   offset=R(unchecked(table-12));baseTable=R(A(vtt,0x10));W(A(A(owner,offset),8),baseTable);
   table=R(A(vtt,4));W(owner,table);baseTable=R(A(vtt,8));offset=R(unchecked(table-12));W(A(owner,offset),baseTable);W(A(owner,4),0);
  }
 }
}
