using System;
namespace RouletteDeluxe
{
 // Thirty startup accessors/setters. SetSerialNumber's event/stream/manager
 // sequence is a separate required binding; it is not silently replaced here.
 public sealed class OriginalMachineDataStartupSetters
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint[],ulong> Invoke;
  }
  private readonly Host h;
  public OriginalMachineDataStartupSetters(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.Invoke==null)throw new ArgumentException("Physical MachineData memory and original imported operations required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,Invoke=host.Invoke};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private ulong Call(uint method,params uint[] args){return h.Invoke(method,args);}
  private ulong Save(uint owner){return Call(0x947e1c0,A(owner,0x3c8));}
  private ulong Read64(uint address){uint low=h.ReadWord(address),high=h.ReadWord(A(address,4));return (ulong)low|((ulong)high<<32);}
  private ulong Amount(uint owner,uint offset,uint low,uint high,uint returnedOffset)
  {h.WriteWord(A(owner,offset),low);h.WriteWord(A(owner,offset+4),high);Save(owner);return Read64(A(owner,returnedOffset));}
  private ulong Flag(uint owner,uint offset,uint value,bool tail=false)
  {h.WriteByte(A(owner,offset),unchecked((byte)value));ulong result=Save(owner);return tail?result:h.ReadByte(A(owner,offset));}
  public static bool Handles(uint method)
  {
   switch(method){case 0x838cd20:case 0x838d810:case 0x838c020:case 0x838d500:case 0x838d580:case 0x838c070:case 0x838bf00:case 0x838bfd0:case 0x838d460:case 0x838cc20:case 0x838cba0:case 0x838beb0:case 0x838bf50:case 0x838c0c0:case 0x838c280:case 0x838bc90:case 0x838bc50:case 0x838bbd0:case 0x838ba50:case 0x838ba10:case 0x838d7b0:case 0x838b920:case 0x838cb70:case 0x838b9a0:case 0x838b970:case 0x838ba90:case 0x838b9e0:case 0x838a920:case 0x838a3a0:case 0x838b770:return true;default:return false;}
  }
  public ulong Invoke(uint method,uint[] args)
  {
   if(!Handles(method))throw new ArgumentOutOfRangeException("method");if(args==null||args.Length<2)throw new ArgumentException("Live original setter arguments required");args=(uint[])args.Clone();uint owner=args[0],value=args[1];
   switch(method)
   {
    case 0x838cd20:for(uint offset=0;offset<60;offset+=4)h.WriteWord(A(owner,0x4d9+offset),h.ReadWord(A(value,offset)));Save(owner);return value;
    case 0x838d810:Call(0x80af674,value,A(owner,0x4a0));return value;
    case 0x838d7b0:Call(0x80af674,A(owner,0x4a0),value);Save(owner);return value;
    case 0x838c020:return Amount(owner,0x51f,value,args[2],0x51f);
    case 0x838c070:return Amount(owner,0x56f,value,args[2],0x51f); // Original returns credit limit, not the new upper-jackpot field.
    case 0x838bf00:return Amount(owner,0x53f,value,args[2],0x53f);
    case 0x838bfd0:return Amount(owner,0x597,value,args[2],0x597);
    case 0x838beb0:return Amount(owner,0x58f,value,args[2],0x58f);
    case 0x838c0c0:return Amount(owner,0x54f,value,args[2],0x54f);
    case 0x838d500:if(unchecked((long)((ulong)value|((ulong)args[2]<<32)))<=unchecked((long)Call(0x87fd560)))return Amount(owner,0x537,value,args[2],0x537);return Read64(A(owner,0x537));
    case 0x838d580:
     bool accept=unchecked((long)((ulong)value|((ulong)args[2]<<32)))<=unchecked((long)Call(0x87fd500));
     if(!accept){uint jurisdiction=h.ReadWord(0x9e874a0),table=h.ReadWord(jurisdiction),getter=h.ReadWord(A(table,0x48));accept=(byte)Call(getter,jurisdiction)==0;}
     return accept?Amount(owner,0x52f,value,args[2],0x52f):Read64(A(owner,0x52f));
    case 0x838d460:case 0x838cc20:case 0x838cba0:
     uint manager=h.ReadWord(0x9e85c48),managerTable=h.ReadWord(manager),mode=h.ReadWord(A(managerTable,0xe0)),high=args[2];if(unchecked((uint)Call(mode,manager))!=0){value=0;high=0;}
     uint field=method==0x838d460?0x527u:method==0x838cc20?0x577u:0x57fu;return Amount(owner,field,value,high,field);
    case 0x838c280:h.WriteWord(A(owner,0x59f),value);if(unchecked((int)value)>16)h.WriteWord(A(owner,0x59f),16);Save(owner);return h.ReadWord(A(owner,0x59f));
    case 0x838b920:h.WriteByte(A(owner,0x5c4),(byte)value);if((byte)value>8)h.WriteByte(A(owner,0x5c4),8);Save(owner);return h.ReadByte(A(owner,0x5c4));
    case 0x838bf50:return Flag(owner,0x5ad,value);
    case 0x838bc90:return Flag(owner,0x5ab,value);
    case 0x838bc50:return Flag(owner,0x5ac,value);
    case 0x838bbd0:return Flag(owner,0x5b1,value);
    case 0x838ba50:return Flag(owner,0x5af,value);
    case 0x838ba10:return Flag(owner,0x5b0,value);
    case 0x838b9a0:return Flag(owner,0x5d1,value);
    case 0x838ba90:return Flag(owner,0x5c1,value);
    case 0x838cb70:return Flag(owner,0x5c3,value,true);
    case 0x838b970:return Flag(owner,0x5f2,value,true);
    case 0x838b9e0:return Flag(owner,0x5b8,value,true);
    case 0x838b770:return Flag(owner,0x5f3,value,true);
    case 0x838a920:h.WriteByte(A(owner,0xa8),(byte)value);return 0; // Native void return registers are outside this adapter's contract.
    case 0x838a3a0:h.WriteWord(A(owner,0x68),value);h.WriteWord(A(owner,0x6c),args[2]);return 0;
    default:throw new InvalidOperationException("Unbound startup setter");
   }
  }
 }
}
