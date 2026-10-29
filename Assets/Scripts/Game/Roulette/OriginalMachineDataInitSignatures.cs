using System;
using System.Text;
namespace RouletteDeluxe
{
 // MachineData::InitSignatures 0838f730, with a post-prolog ESP frame.
 // Device signatures, strings, NV, libc and native unwind remain required ports.
 public sealed class OriginalMachineDataInitSignatures
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
  public OriginalMachineDataInitSignatures(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Live signature memory, imports and unwind bindings required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private uint R(uint a){return h.ReadWord(a);}
  private void W(uint a,uint v){h.WriteWord(a,v);}
  private ulong Call(uint method,params uint[] args){return h.Invoke(method,args);}
  private ulong Protected(uint method,Action<Exception> cleanup,params uint[] args)
  {
   try{return Call(method,args);}catch(Exception error){if(!h.IsNativeUnwind(error))throw;cleanup(error);h.Resume(error);throw new InvalidOperationException("Nonreturning signature Resume returned");}
  }
  private void Literal(uint address,string text)
  {
   byte[] bytes=Encoding.ASCII.GetBytes(text+"\0");int i=0;
   for(;i+4<=bytes.Length;i+=4)W(A(address,(uint)i),(uint)(bytes[i]|bytes[i+1]<<8|bytes[i+2]<<16|bytes[i+3]<<24));
   for(;i<bytes.Length;++i)h.WriteByte(A(address,(uint)i),bytes[i]);
  }
  private void Zero40(uint address){for(uint i=0;i<40;i+=4)W(A(address,i),0);}
  private uint MemberMethod(uint owner,uint slot){return R(A(R(owner),slot));}
  private ulong Manager(uint slot,Action<Exception> cleanup=null)
  {uint owner=R(0x9e85c48),method=MemberMethod(owner,slot);return cleanup==null?Call(method,owner):Protected(method,cleanup,owner);}
  private void CommonCleanup(uint frame)
  {Call(0x80add34,A(frame,0x114));uint saved=R(A(frame,0x20));Call(0x80add34,saved);uint pointer=R(A(frame,0xf0));if(pointer!=0)Call(0x80aabe4,pointer);}
  private void Open(uint frame,uint owner,uint path,uint nv,uint temporary,uint allocator)
  {
   uint text=A(frame,temporary);Call(0x80afcf4,text,A(owner,path),A(frame,allocator));Protected(0x947ef10,error=>{if(temporary==0xfc)Call(0x80add34,text);else Protected(0x80add34,second=>{CommonCleanup(frame);},text);},A(owner,nv),text);Call(0x80add34,text);
  }
  private void CopyVector(uint frame,uint owner,uint destination,bool scratch)
  {
   uint source=R(A(frame,0xf0)),end=R(A(frame,0xf4)),length=unchecked(end-source);if(unchecked((int)length)<=0)return;
   uint packed=(length>>2)<<2;if(scratch)W(A(frame,0x20),packed);
   if(packed!=0&&length>6&&(source&3)==0&&(destination>A(source,4)||source>A(destination,4)))
   {
    for(uint i=0;i<packed;i+=4)W(A(destination,i),R(A(source,i)));source=A(source,packed);destination=A(destination,packed);length-=packed;if(length==0)return;
   }
   if(scratch)W(A(frame,0x20),owner);
   for(uint i=0;i<length;++i)h.WriteByte(A(destination,i),h.ReadByte(A(source,i)));
  }
  private void Fault(uint text,Action<Exception> cleanup)
  {Protected(0x9483f60,cleanup,16,text);throw new InvalidOperationException("Nonreturning signature Fault returned");}
  public uint Initialize(uint frame)
  {
   uint owner=R(A(frame,0x150)),os=A(frame,0x30),game=A(frame,0x58),bios=A(frame,0x80),zero=A(frame,0xa8),vector=A(frame,0xf0),version=A(frame,0x110),errorString=A(frame,0x114);
   Literal(A(owner,0xc8),"/nvram_os/critical/machineData");Literal(A(owner,0x108),"/nvram_os/critical/machineDataExtended");Literal(A(owner,0x148),"/nvram_os/critical/OSMediaSignature");Literal(A(owner,0x188),"/nvram_os/critical/BiosSignature");Literal(A(owner,0x1c8),"/nvram_os/critical/GameMediaSignature");
   W(A(frame,0x20),A(owner,0x188));int existed=unchecked((int)Call(0x80aa544,A(owner,0x148),0));
   Open(frame,owner,0xc8,0x3c8,0xfc,0x129);Open(frame,owner,0x108,0x640,0x100,0x12a);Open(frame,owner,0x148,0x710,0x104,0x12b);Open(frame,owner,0x1c8,0x7f0,0x108,0x12c);Open(frame,owner,0x188,0x780,0x10c,0x12d);
   if((uint)Manager(0x108)!=0)
   {
    Literal(A(owner,0x298),"Dev OS ");Call(0x80ab2a4,A(owner,0x298),R(0x9b15014));Literal(A(owner,0x2d8),"Game Version ");Call(0x80ab2a4,A(owner,0x2d8),R(0x9b15014));Literal(A(owner,0x318),"Dev Bios Version ");Call(0x80ab2a4,A(owner,0x318),R(0x9b15014));Literal(A(owner,0x348),"S9000C Personality #");
   }
   else
   {
    Zero40(zero);h.WriteByte(A(owner,0xa9),0);W(vector,0);W(A(vector,4),0);W(A(vector,8),0);W(version,0x9b482ac);W(errorString,0x9b482ac);
    if((uint)Call(0x80aef14,0x96ac794)!=0)h.WriteByte(A(owner,0xa9),1);Zero40(os);W(A(frame,0x20),version);
    Action<Exception> cleanup=error=>{CommonCleanup(frame);};
    if((byte)Protected(0x94e3ca0,cleanup,vector,errorString)!=0)Fault(R(errorString),cleanup);CopyVector(frame,owner,os,true);
    if(h.ReadByte(A(owner,0xa9))==0&&existed==0&&unchecked((int)Protected(0x80aafa4,cleanup,os,A(owner,0x758),40))!=0){W(A(frame,0x20),version);Fault(0x96b0d18,cleanup);}
    if(unchecked((int)Protected(0x80aafa4,cleanup,A(owner,0x758),zero,40))==0){W(A(frame,0x20),version);Protected(MemberMethod(owner,0x40),cleanup,owner,os);}
    Zero40(game);W(A(frame,0x20),version);if((byte)Protected(0x94e3fc0,cleanup,vector,errorString)!=0){W(A(frame,0x20),version);Fault(R(errorString),cleanup);}CopyVector(frame,owner,game,true);
    if(unchecked((int)Call(0x80aafa4,A(owner,0x838),zero,40))==0){W(A(frame,0x20),version);Protected(MemberMethod(owner,0x48),cleanup,owner,game);}
    else if(h.ReadByte(A(owner,0xa9))==0&&unchecked((int)Protected(0x80aafa4,cleanup,game,A(owner,0x838),40))!=0){W(A(frame,0x20),version);Fault(0x96b0d4c,cleanup);}
    uint temporary=A(frame,0x118);
    Protected(0x80afcf4,error=>{W(A(frame,0x20),version);cleanup(error);},temporary,0x96c5b87,A(frame,0x12e));W(A(frame,0x20),version);
    Protected(0x94e1ff0,error=>{Protected(0x80add34,cleanup,temporary);cleanup(error);},version,temporary);Call(0x80add34,temporary);Call(0x80af674,A(owner,0x2d8),R(version));
    Protected(0x94e2410,cleanup,version);Protected(0x80af674,cleanup,A(owner,0x2f8),R(version));Protected(0x94e24c0,cleanup,version);Protected(0x80af674,cleanup,A(owner,0x298),R(version));
    if((uint)Manager(0x10c,cleanup)==0)
    {
     uint trim=A(frame,0x11c),substring=A(frame,0x120);Protected(0x80afcf4,cleanup,trim,A(owner,0x298),A(frame,0x12f));
     Action<Exception> trimCleanup=error=>{Call(0x80add34,trim);cleanup(error);};
     uint position=(uint)Protected(0x80b05a4,trimCleanup,trim,0x9732c83,uint.MaxValue,1);
     if(position!=uint.MaxValue)
     {
      Protected(0x80aa5c4,trimCleanup,substring,trim,0,position);Protected(0x80ab384,error=>{Call(0x80add34,substring);trimCleanup(error);},trim,substring);Call(0x80add34,substring);Call(0x80af674,A(owner,0x298),R(trim));
     }
     uint length=(uint)Call(0x80b0a54,A(owner,0x2d8));Protected(0x80adb74,trimCleanup,trim,A(owner,0x2d8),length);position=(uint)Protected(0x80b05a4,trimCleanup,trim,0x9732c83,uint.MaxValue,1);
     if(position!=uint.MaxValue)
     {
      substring=A(frame,0x124);Protected(0x80aa5c4,trimCleanup,substring,trim,0,position);Protected(0x80ab384,error=>{Call(0x80add34,substring);trimCleanup(error);},trim,substring);Call(0x80add34,substring);Call(0x80af674,A(owner,0x2d8),R(trim));
     }
     Call(0x80add34,trim);
    }
    if((byte)Protected(0x94e4350,cleanup,vector,errorString,0)!=0){Protected(0x80aeb44,cleanup,0x96b0ad8);Fault(R(errorString),cleanup);}CopyVector(frame,owner,bios,false);
    if(unchecked((int)Protected(0x80aafa4,cleanup,A(owner,0x7c8),zero,40))==0)Protected(MemberMethod(owner,0x58),cleanup,owner,bios);
    else if(h.ReadByte(A(owner,0xa9))==0&&unchecked((int)Protected(0x80aafa4,cleanup,bios,A(owner,0x7c8),40))!=0)Fault(0x96b0d84,cleanup);
    Protected(0x94e4a70,cleanup,version,0);
    // This strcpy and the final string/vector destructors have no own landing pad.
    Call(0x80af674,A(owner,0x318),R(version));Call(0x80add34,errorString);Call(0x80add34,version);uint data=R(vector);if(data!=0)Call(0x80aabe4,data);
   }
   Literal(A(owner,0x2b8),"Game Manager Version ");Call(0x80ab2a4,A(owner,0x2b8),R(0x9b15014));Literal(A(owner,0x278),"Compact Flash Version ");Call(0x80ab2a4,A(owner,0x278),R(0x9b15014));Call(MemberMethod(owner,0x7c),owner,A(owner,0x2b8));
   Literal(A(frame,0xd0),"Game Version ");Call(0x80ab2a4,A(frame,0xd0),R(0x9b15014));Call(MemberMethod(owner,0x84),owner,A(frame,0xd0));return 1;
  }
 }
}
