using System;
namespace RouletteDeluxe
{
 // MachineData::InitConfiguration 08396630. Imported runtime, stream/string,
 // signature acquisition, setters, loader and native unwind remain required ports.
 public sealed class OriginalMachineDataInitConfiguration
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
  public OriginalMachineDataInitConfiguration(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Complete live MachineData initialization bindings required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private uint R(uint a){return h.ReadWord(a);}
  private ulong Call(uint method,params uint[] args){return h.Invoke(method,args);}
  private uint Method(uint owner,uint slot){return R(A(R(owner),slot));}
  private ulong Member(uint owner,uint slot,params uint[] supplied)
  {uint method=Method(owner,slot);var args=new uint[supplied.Length+1];args[0]=owner;Array.Copy(supplied,0,args,1,supplied.Length);return Call(method,args);}
  private byte Jurisdiction(uint slot){return (byte)Member(R(0x9e874a0),slot);}
  private void Resume(Exception error){h.Resume(error);throw new InvalidOperationException("Nonreturning native initialization Resume returned");}
  private void Fault(uint text){Call(0x9483f60,16,text);throw new InvalidOperationException("Nonreturning native initialization Fault returned");}
  private void Amount(uint owner,uint slot,uint field,int order=0)
  {
   uint method=0,low,high;
   if(order==1)method=Method(owner,slot);
   low=R(A(owner,field));if(order==2)method=Method(owner,slot);high=R(A(owner,field+4));
   if(order==0)method=Method(owner,slot);Call(method,owner,low,high);
  }
  private void VectorFailure(Exception error,uint pointer)
  {if(!h.IsNativeUnwind(error))throw error;if(pointer!=0)Call(0x80aabe4,pointer);Resume(error);}
  public uint Initialize(uint frame)
  {
   uint owner=R(A(frame,0x120));Call(0x83912d0,owner);
   if(unchecked((uint)Member(R(0x9e85c48),0x108))==0)
   {
    for(uint i=0;i<60;i+=4)h.WriteWord(A(frame,0x80+i),0);
    h.WriteWord(A(frame,0xdc),0);h.WriteWord(A(frame,0xe0),0);h.WriteWord(A(frame,0xe4),0);
    for(uint i=0;i<60;i+=4)h.WriteWord(A(frame,0x44+i),0);
    byte failure;
    try{failure=(byte)Call(0x94e3e30,A(frame,0xdc));}
    catch(Exception error){VectorFailure(error,R(A(frame,0xdc)));throw;}
    if(failure!=0){try{Fault(0x96adb58);}catch(Exception error){VectorFailure(error,R(A(frame,0xdc)));throw;}}
    uint vector=R(A(frame,0xdc)),end=R(A(frame,0xe0));int size=unchecked((int)(end-vector));
    for(int i=0;i<size;++i)h.WriteByte(A(frame,0x44+(uint)i),h.ReadByte(A(vector,(uint)i)));
    int empty=unchecked((int)Call(0x80aafa4,A(owner,0x4d9),A(frame,0x80),60));
    if(empty==0)
    {
     try{Member(owner,0x60,A(frame,0x44));}catch(Exception error){VectorFailure(error,R(A(frame,0xdc)));throw;}
     vector=R(A(frame,0xdc));
    }
    else if(h.ReadByte(A(owner,0xa9))==0&&unchecked((int)Call(0x80aafa4,A(frame,0x44),A(owner,0x4d9),60))!=0)
    {try{Fault(0x96b0e28);}catch(Exception error){VectorFailure(error,vector);throw;}}
    if(vector!=0)Call(0x80aabe4,vector);
   }
   Member(owner,0xa0,A(frame,0xbc));
   if(h.ReadByte(A(frame,0xbc))==0)
   {
    if(Jurisdiction(0x2d0)!=0){Amount(owner,0x140,0x18);Amount(owner,0x12c,0x78);Amount(owner,0x160,0x28);Amount(owner,0x158,0x40);Amount(owner,0x178,0x30,1);}
    Amount(owner,0x148,0x88);Amount(owner,0x150,0x20);Amount(owner,0x190,0x48);Amount(owner,0x198,0x50);Amount(owner,0x1a0,0x60,1);
    uint any=R(A(owner,0x3c))|R(A(owner,0x38));Member(owner,0x188,any==0?0u:1u);Amount(owner,0x138,0x80,2);
    Member(owner,0xb8,16);Member(owner,0xac,A(owner,0x90));uint flag=h.ReadByte(A(owner,0xa0));Member(owner,0x1f8,flag);
    flag=h.ReadByte(A(owner,0xa2));uint method=Method(owner,0x204);h.WriteByte(A(owner,0xa1),0);Call(method,owner,flag);
    h.WriteByte(A(owner,0x5b4),h.ReadByte(A(owner,0xa3)));Call(0x947e1c0,A(owner,0x3c8));flag=h.ReadByte(A(owner,0xa4));Member(owner,0x214,flag);
    flag=h.ReadByte(A(owner,0xa6));method=Method(owner,0x260);h.WriteByte(A(owner,0xa5),0);Call(method,owner,flag);
    flag=h.ReadByte(A(owner,0xa7));Member(owner,0x268,flag);Member(owner,0xa4,A(owner,0x208));Member(owner,0x2a4,0);Member(owner,0x2b0,0);Member(owner,0x37c,0);
    if(Jurisdiction(0x2ec)!=0)Member(owner,0x384,1);Member(owner,0x24c,3);Member(owner,0x278,0);
   }
   uint trailing=Method(owner,0x270);h.WriteByte(A(owner,0xc0),0);Call(trailing,owner,0);
   bool useBank=false;if(Jurisdiction(0x16c)!=0)useBank=unchecked((long)Member(R(0x9e874a0),0xbc))>0;
   if(useBank){uint setter=Method(owner,0x16c);ulong value=Member(R(0x9e874a0),0xbc);Call(setter,owner,(uint)value,(uint)(value>>32));}
   else Amount(owner,0x16c,0x70,1);
   Member(owner,0x428,0);uint module=A(frame,0xf0),temporary=0;bool protectedCall=true;h.WriteWord(module,0x9b482ac);
   try
   {
    if(Jurisdiction(0x424)!=0&&(byte)Call(0x8395f10,module)!=0)
    {
     protectedCall=false;uint crc=uint.MaxValue;Call(0x8397e50);uint data=R(module),end=A(data,R(unchecked(data-12)));
     while(data<end){byte low=(byte)(crc^h.ReadByte(data));data=A(data,1);crc=R(A(0x9e87bc0,(uint)low*4))^(crc>>8);}
     Call(0x80b0cf4,A(frame,0xdc),0x96b0c5d,~crc);uint value=A(frame,0xf4);protectedCall=true;Call(0x80afcf4,value,A(frame,0xdc),A(frame,0xfe));temporary=value;
     Call(0x80ab384,A(owner,0x2000),value);temporary=0;protectedCall=false;Call(0x80add34,value);
    }
    protectedCall=true;if(Jurisdiction(0x424)!=0)
    {uint path=A(frame,0xf8);Call(0x80afcf4,path,0x96b0e68,A(frame,0xff));temporary=path;Call(0x947ef10,A(owner,0xabc),path);temporary=0;protectedCall=false;Call(0x80add34,path);}
    protectedCall=true;
    if((byte)Member(R(0x9e87a40),0x424)!=0){uint control=(uint)Call(0x81be880);Member(control,0x128,0);}
    Member(R(0x9b4d4a8),12,0x8390960,0x100,0xffff,0);Member(R(0x9b4d4a8),12,0x8390960,0x200,0xffff,0);
    Call(0x80add34,module);
   }
   catch(Exception error)
   {
    if(!protectedCall||!h.IsNativeUnwind(error))throw;
    if(temporary!=0)Call(0x80add34,temporary);Call(0x80add34,module);Resume(error);throw;
   }
   return 1;
  }
 }
}
