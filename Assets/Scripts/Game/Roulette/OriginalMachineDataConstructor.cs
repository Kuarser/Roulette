using System;
namespace RouletteDeluxe
{
 // MachineData C2 at 08394c90. Imported member constructors/destructors remain
 // required ports; this control initializes the original physical object layout.
 public sealed class OriginalMachineDataConstructor
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint[],ulong> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host h;
  public OriginalMachineDataConstructor(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Live MachineData constructor memory, members and unwind bindings required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void W(uint a,uint v){h.WriteWord(a,v);}
  private void Half(uint a){h.WriteByte(a,0);h.WriteByte(A(a,1),0);}
  private void Call(uint method,params uint[] args){h.Invoke(method,args);}
  private void Zero(uint at,uint count)
  {
   if((at&1)!=0){h.WriteByte(at,0);at=A(at,1);--count;}
   if((at&2)!=0){Half(at);at=A(at,2);count-=2;}
   for(uint i=0;i<(count>>2);++i){W(at,0);at=A(at,4);}
   if((count&2)!=0){Half(at);at=A(at,2);}if((count&1)!=0)h.WriteByte(at,0);
  }
  private void NvDestroy(uint owner,uint offset,uint table){W(A(owner,offset),table);Call(0x947ee80,A(owner,offset));}
  private void Cleanup(uint owner,int completed)
  {
   if(completed>=12){Call(0x80add34,A(owner,0x2000));NvDestroy(owner,0xabc,0x96b2208);}
   if(completed>=11)Call(0x8348a70,A(owner,0xa2c));
   if(completed>=10)Call(0x8348a70,A(owner,0x99c));
   if(completed>=9)Call(0x8348a70,A(owner,0x90c));
   if(completed>=8)Call(0x8348a70,A(owner,0x87c));
   if(completed>=7){uint root=h.ReadWord(A(owner,0x86c));Call(0x8397aa0,A(owner,0x864),root);Call(0x80add34,A(owner,0x860));NvDestroy(owner,0x7f0,0x96b21a8);}
   if(completed>=6)NvDestroy(owner,0x780,0x96b2148);
   if(completed>=5)NvDestroy(owner,0x710,0x96b20e8);
   if(completed>=4)NvDestroy(owner,0x640,0x96b2088);
   if(completed>=3)NvDestroy(owner,0x3c8,0x96b2028);
   if(completed>=2)Call(0x83b85d0,A(owner,0x3bc));
   if(completed>=1){Call(0x834a190,A(owner,0x3b0));Call(0x834a190,A(owner,0x3a4));Call(0x80ac7a4,A(owner,0xac));}
   W(A(owner,4),0x96b1788);W(owner,0x96b1760);
  }
  private void Construct(uint owner,int completed,uint method,params uint[] args)
  {
   try{Call(method,args);}catch(Exception error){if(!h.IsNativeUnwind(error))throw;Cleanup(owner,completed);h.Resume(error);throw new InvalidOperationException("Nonreturning native constructor Resume returned");}
  }
  private void Nv(uint owner,int completed,uint offset,uint table,uint length)
  {Construct(owner,completed,0x9480680,A(owner,offset));W(A(owner,offset),table);Zero(A(owner,offset+0x48),length);}
  public void Create(uint owner)
  {
   W(owner,0x96b0ec8);W(A(owner,4),0x96b1320);Construct(owner,0,0x80ad6b4,A(owner,0xac),0,0);
   for(uint i=0x3a4;i<=0x3b8;i+=4)W(A(owner,i),0);Construct(owner,1,0x83b8630,A(owner,0x3bc));Construct(owner,2,0x9480680,A(owner,0x3c8));
   // Adjacent packed controls precede the main payload's zeroing in the ELF.
   Half(A(owner,0x5f5));for(uint i=0x5f7;i<=0x5fb;++i)h.WriteByte(A(owner,i),0);Half(A(owner,0x5fc));W(A(owner,0x5fe),0);Half(A(owner,0x602));
   for(uint i=0x604;i<=0x608;++i)h.WriteByte(A(owner,i),0);Half(A(owner,0x609));W(A(owner,0x60b),0);Half(A(owner,0x60f));
   for(uint i=0x611;i<=0x615;++i)h.WriteByte(A(owner,i),0);Half(A(owner,0x616));W(A(owner,0x618),0);Half(A(owner,0x61c));
   for(uint i=0x61e;i<=0x622;++i)h.WriteByte(A(owner,i),0);Half(A(owner,0x623));W(A(owner,0x625),0);
   W(A(owner,0x3c8),0x96b2028);Zero(A(owner,0x410),0x22d);
   Nv(owner,3,0x640,0x96b2088,0x87);Nv(owner,4,0x710,0x96b20e8,0x28);Nv(owner,5,0x780,0x96b2148,0x28);Nv(owner,6,0x7f0,0x96b21a8,0x28);
   W(A(owner,0x870),A(owner,0x868));W(A(owner,0x874),A(owner,0x868));W(A(owner,0x860),0x9b482ac);W(A(owner,0x868),0);W(A(owner,0x86c),0);W(A(owner,0x878),0);
   Construct(owner,7,0x834b460,A(owner,0x87c),0xa12353c,0xa123598);Construct(owner,8,0x834b460,A(owner,0x90c),0xa1251dc,0xa1251e8);Construct(owner,9,0x834b460,A(owner,0x99c),0xa1251d8,0xa1251e8);Construct(owner,10,0x834b460,A(owner,0xa2c),0xa1251d4,0xa1251e8);
   Nv(owner,11,0xabc,0x96b2208,0x14fa);W(A(owner,0x2000),0x9b482ac);W(A(owner,0xbc),0);W(0x9e87a44,owner);Construct(owner,12,0x80adb74,A(owner,0x2000),0x9701b4c,13);
  }
 }
}
