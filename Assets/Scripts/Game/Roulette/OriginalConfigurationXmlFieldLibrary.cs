using System;
namespace RouletteDeluxe
{
 // Actual operation bindings for the physical XML field parser. All COW,
 // heap, mutation and byte operations share one persistent memory owner.
 public sealed class OriginalConfigurationXmlFieldLibrary
 {
  readonly OriginalNativeMemory memory;
  readonly OriginalConfigurationMemoryStrings strings;
  readonly OriginalConfigurationCowString cow;
  readonly OriginalConfigurationStringLeak leak;
  readonly uint errnoAddress;
  readonly Action<uint> resume;
  public OriginalConfigurationXmlFieldLibrary(OriginalNativeMemory m,OriginalConfigurationMemoryStrings s,uint emptyRepSlot,uint threadSlot,uint lengthErrorCString,uint errno,Action<uint> lengthError,Action<uint> nativeResume)
  {
   if(m==null||s==null||lengthError==null||nativeResume==null||errno==0)throw new ArgumentException("Shared physical memory, recovered strings, relocated COW slots, errno and nonreturning error/resume owners required");
   memory=m;strings=s;errnoAddress=errno;resume=nativeResume;
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=lengthError},emptyRepSlot,threadSlot,lengthErrorCString);
   var mutation=new OriginalConfigurationCowMutation(new OriginalConfigurationCowMutation.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,CopyBytes=m.CopyBytes,MoveBytes=m.MoveBytes,CreateRep=cow.Create,ReleaseRep=cow.Release},emptyRepSlot);
   OriginalConfigurationStringLeak built=null;
   built=new OriginalConfigurationStringLeak(new OriginalConfigurationStringLeak.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,Mutate=mutation.Mutate,InvokeLeakHard=owner=>built.LeakHard(owner)},emptyRepSlot);leak=built;
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  void Arity(uint[] args,int length){if(args==null||args.Length!=length)throw new ArgumentException("Original normalized library argument count mismatch");}
  uint Find(uint owner,uint pattern,uint position,uint count)
  {
   uint data=memory.ReadWord(owner),length=memory.ReadWord(unchecked(data-12));
   if(position>length||count>length-position)return uint.MaxValue;
   if(count==0)return position;
   uint last=length-count;
   for(uint i=position;i<=last;++i)
   {
    uint j=0;while(j<count&&memory.ReadByte(A(data,A(i,j)))==memory.ReadByte(A(pattern,j)))++j;
    if(j==count)return i;
   }
   return uint.MaxValue;
  }
  uint ReverseFind(uint owner,uint character,uint position)
  {
   uint data=memory.ReadWord(owner),length=memory.ReadWord(unchecked(data-12));if(length==0)return uint.MaxValue;
   uint i=position<length?position:length-1;
   while(true){if(memory.ReadByte(A(data,i))==(byte)character)return i;if(i==0)return uint.MaxValue;--i;}
  }
  uint Decimal(uint[] args)
  {
   Arity(args,3);if(args[1]!=0||args[2]!=10)throw new ArgumentException("XML field parser strtol admission requires null endptr and base ten");
   var state=new OriginalCfgIntegerConverter.State{Errno=unchecked((int)memory.ReadWord(errnoAddress))};
   uint value=OriginalConfigurationXmlReceive.SignedDecimalBits(memory.ReadBytes(args[0],memory.StrLen(args[0])),state);
   if(unchecked((uint)state.Errno)!=memory.ReadWord(errnoAddress))memory.WriteWord(errnoAddress,unchecked((uint)state.Errno));
   return value;
  }
  public uint Invoke(uint method,uint[] args)
  {
   switch(method)
   {
    case 0x80ab384:Arity(args,2);return strings.Assign(args[0],args[1]);
    case 0x80ab9f4:Arity(args,2);cow.DestroyRep(args[0]);return 0;
    case 0x80ac3e4:Arity(args,3);return ReverseFind(args[0],args[1],args[2]);
    case 0x80ad3a4:return Decimal(args);
    case 0x80adb74:Arity(args,3);return strings.AssignBytes(args[0],args[1],args[2]);
    case 0x80add34:Arity(args,1);strings.Destroy(args[0]);return 0;
    case 0x80ae8a4:Arity(args,1);leak.LeakHard(args[0]);return 0;
    case 0x80aed64:Arity(args,1);resume(args[0]);throw new InvalidOperationException("Native nonreturning Resume owner returned");
    case 0x80afb44:Arity(args,4);return Find(args[0],args[1],args[2],args[3]);
    default:throw new ArgumentException("Unbound XML field parser library operation "+method.ToString("x8"));
   }
  }
 }
}
