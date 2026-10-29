using System;
namespace RouletteDeluxe
{
 // MachineData::SetSerialNumber 08393280. The frame is the post-prolog ESP.
 // Imported strings, streams, NV, event and manager operations remain live ports.
 public sealed class OriginalMachineDataSetSerialNumber
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint[],ulong> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host h;
  public OriginalMachineDataSetSerialNumber(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Live serial memory, imports and unwind bindings required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private uint R(uint a){return h.ReadWord(a);}
  private void W(uint a,uint v){h.WriteWord(a,v);}
  private void Half(uint a,uint v){h.WriteByte(a,(byte)v);h.WriteByte(A(a,1),(byte)(v>>8));}
  private ulong Call(uint method,params uint[] args){return h.Invoke(method,args);}
  private ulong Protected(uint method,Action<Exception> cleanup,params uint[] args)
  {
   try{return Call(method,args);}catch(Exception error){if(!h.IsNativeUnwind(error))throw;cleanup(error);h.Resume(error);throw new InvalidOperationException("Nonreturning native serial Resume returned");}
  }
  public uint Set(uint frame)
  {
   uint owner=R(A(frame,0x1d0)),packet=A(frame,0x2f),stream=A(frame,0xe8),ios=A(frame,0x114),buffer=A(frame,0xec),locale=A(frame,0x108),bufferString=A(frame,0x110),asset=A(frame,0x19c),serial=A(frame,0x1a0);
   for(uint i=0x33;i<=0x36;++i)h.WriteByte(A(frame,i),0);
   Half(A(frame,0x37),0);W(A(frame,0x39),0);W(A(frame,0x3d),0);Half(A(frame,0x41),0);
   for(uint i=0x43;i<=0x63;i+=4)W(A(frame,i),0);h.WriteByte(A(frame,0x67),0);
   uint at=A(frame,0x68),remaining=128;
   if((at&2)!=0){Half(at,0);at=A(at,2);remaining-=2;}
   for(uint i=0;i<(remaining>>2);++i){W(at,0);at=A(at,4);}
   if((remaining&2)!=0){Half(at,0);at=A(at,2);}if((remaining&1)!=0)h.WriteByte(at,0);
   Half(A(frame,0x31),0xb9);Half(packet,0x200);
   Call(0x80ae3f4,A(owner,0x4c0),R(A(frame,0x1d4)),R(A(owner,0x59f)));Call(0x947e1c0,A(owner,0x3c8));
   uint eventOwner=R(0x9b4d4a8);W(A(frame,0x47),0x13700);Half(A(frame,0x41),0x10);uint eventMethod=R(A(R(eventOwner),0x24));Call(eventMethod,eventOwner,packet,0);
   Call(0x80ad8c4,ios);uint vtt=R(0x9b49594),second=R(0x9b49598),baseAddress=A(stream,R(unchecked(vtt-12)));
   h.WriteByte(A(frame,0x188),0);W(ios,0x9b49c10);W(stream,vtt);W(A(frame,0x184),0);W(baseAddress,second);h.WriteByte(A(frame,0x189),0);
   for(uint i=0x18c;i<=0x198;i+=4)W(A(frame,i),0);
   Protected(0x80b06f4,error=>{W(ios,0x9b49c10);Call(0x80b0fc4,ios);},baseAddress,0);
   W(stream,0x9b48dec);W(ios,0x9b48e00);W(buffer,0x9b48a28);for(uint i=0xf0;i<=0x104;i+=4)W(A(frame,i),0);
   Call(0x80b11f4,locale);W(buffer,0x9b497c8);W(A(frame,0x10c),0x10);W(bufferString,0x9b482ac);
   Action restore=()=>{uint offset=R(unchecked(vtt-12)),entry=R(0x9b49598);W(stream,vtt);W(A(stream,offset),entry);W(ios,0x9b49c10);};
   Protected(0x80b06f4,error=>{Call(0x81a1a80,buffer);restore();Call(0x80b0fc4,ios);},ios,buffer);
   Action<Exception> streamCleanup=error=>{Call(0x80b0304,stream);};
   uint getter=R(A(R(owner),0x284));uint number=(uint)Protected(getter,streamCleanup,owner);Protected(0x80aea04,streamCleanup,stream,number);
   uint managerMethod=R(A(R(R(0x9e85c48)),0x2f4));W(asset,0x9b482ac);
   Action<Exception> assetCleanup=error=>{Call(0x80add34,asset);Call(0x80b0304,stream);};
   uint position=R(A(frame,0x100));
   if(position==0)Protected(0x80ab384,assetCleanup,asset,bufferString);
   else
   {
    uint end=R(A(frame,0xf8)),temporary,allocator;
    if(position>end){end=position;temporary=A(frame,0x1a8);allocator=A(frame,0x1af);}else{temporary=A(frame,0x1a4);allocator=A(frame,0x1ae);}
    // Original range constructor uses EAX/EDX/ECX plus a stack allocator.
    Protected(0x838e1f0,assetCleanup,temporary,R(A(frame,0xfc)),end,allocator);
    Protected(0x80ab384,error=>{Call(0x80add34,temporary);assetCleanup(error);},asset,temporary);Call(0x80add34,temporary);
   }
   Protected(0x80afcf4,assetCleanup,serial,R(A(frame,0x1d4)),A(frame,0x1ad));
   Protected(managerMethod,error=>{Call(0x80add34,serial);assetCleanup(error);},R(0x9e85c48),serial,asset);
   Call(0x80add34,serial);Call(0x80add34,asset);
   W(stream,0x9b48dec);W(ios,0x9b48e00);W(buffer,0x9b497c8);Call(0x80add34,bufferString);W(buffer,0x9b48a28);Call(0x80ad934,locale);restore();Call(0x80b0fc4,ios);
   return R(A(frame,0x1d4));
  }
 }
}
