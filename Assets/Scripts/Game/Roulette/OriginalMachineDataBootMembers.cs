using System;
namespace RouletteDeluxe
{
 // Original NV2 base constructor and MachineData boot members. Imported
 // string, NV persistence, factory and stream operations are mandatory ports.
 public sealed class OriginalMachineDataBootMembers
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint[],uint> Invoke;
  }
  private readonly Host h;
  public OriginalMachineDataBootMembers(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null)throw new ArgumentException("Physical memory and original imported boot operations required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public static bool Handles(uint method)
  {
   switch(method){case 0x9480680:case 0x838c490:case 0x838da30:case 0x838d990:case 0x838d320:case 0x838b420:case 0x838b430:case 0x83b8630:case 0x8396f90:case 0x8396fc0:return true;default:return false;}
  }
  public uint Invoke(uint method,params uint[] args)
  {
   if(!Handles(method))throw new ArgumentOutOfRangeException("method");
   int required=method==0x838c490||method==0x838da30||method==0x838d990||method==0x8396f90||method==0x8396fc0?2:1;
   if(args==null||args.Length!=required)throw new ArgumentException("Original member arguments required");
   args=(uint[])args.Clone();uint owner=args[0];
   switch(method)
   {
    case 0x9480680:
     h.WriteWord(owner,0x9765908);
     for(uint i=4;i<=0x18;i+=4)h.WriteWord(A(owner,i),0);
     h.WriteByte(A(owner,0x1c),0);h.WriteByte(A(owner,0x1d),0);
     h.WriteWord(A(owner,0x24),0);h.WriteWord(A(owner,0x28),0);
     h.WriteWord(A(owner,0x2c),4);h.WriteWord(A(owner,0x30),8);h.WriteWord(A(owner,0x34),16);h.WriteWord(A(owner,0x38),18);h.WriteWord(A(owner,0x3c),20);
     h.WriteWord(A(owner,0x44),0);h.WriteWord(A(owner,0x40),0);h.WriteWord(A(owner,0x20),0);
     return 0; // Native void return register is outside the adapter contract.
    case 0x838c490:
     h.WriteByte(A(owner,0x5c0),unchecked((byte)args[1]));Call(0x947e1c0,A(owner,0x3c8));return h.ReadByte(A(owner,0x5c0));
    case 0x838da30:case 0x838d990:
     Call(0x80af674,A(owner,method==0x838da30?0x410u:0x430u),args[1]);Call(0x947e1c0,A(owner,0x3c8));return args[1];
    case 0x838d320:return h.ReadWord(A(owner,0x5a3));
    case 0x838b420:return h.ReadByte(A(unchecked(owner-4),0x631));
    case 0x838b430:return h.ReadByte(A(owner,0x631));
    case 0x83b8630:
     h.WriteWord(owner,0x96b3fa0);uint factory=Call(0x93ffef0);uint table=h.ReadWord(factory),getter=h.ReadWord(A(table,0x14));uint dictionary=Call(getter,factory);h.WriteWord(A(owner,4),dictionary);return 0;
    case 0x8396f90:case 0x8396fc0:
     uint stream=args[1],vtable=h.ReadWord(stream),operation=h.ReadWord(A(vtable,method==0x8396f90?12u:8u));Call(operation,stream,A(owner,0x48),40);return 0;
    default:throw new InvalidOperationException("Unbound MachineData boot member");
   }
  }
 }
}
