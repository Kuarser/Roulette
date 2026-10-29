using System;
namespace RouletteDeluxe
{
 public sealed class OriginalConfigurationTokenizeLibrary
 {
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationMemoryStrings strings;readonly OriginalConfigurationCowString cow;
  readonly OriginalConfigurationStringVector vector;readonly uint insertionFrame;
  readonly Func<uint,uint[],uint> nativeException;
  public OriginalConfigurationTokenizeLibrary(OriginalNativeMemory m,OriginalConfigurationMemoryStrings s,uint emptySlot,uint threadSlot,uint lengthErrorCString,uint frame,Action<uint> lengthError,Func<Exception,bool> isUnwind,Func<Exception,uint> token,Func<uint,uint[],uint> exceptionOwner)
  {
   if(m==null||s==null||frame==0||lengthError==null||isUnwind==null||token==null||exceptionOwner==null)throw new ArgumentException("Shared physical memory, separate insertion frame and native exception owners required");
   memory=m;strings=s;insertionFrame=frame;nativeException=exceptionOwner;
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=lengthError},emptySlot,threadSlot,lengthErrorCString);
   vector=new OriginalConfigurationStringVector(new OriginalConfigurationStringVector.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=isUnwind,ExceptionToken=token});
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  static void Arity(uint[] args,int n){if(args==null||args.Length!=n)throw new ArgumentException("Original normalized library argument count mismatch");}
  uint FindSet(uint owner,uint set,uint position,uint count,bool member)
  {
   uint data=memory.ReadWord(owner),length=memory.ReadWord(unchecked(data-12));
   for(uint i=position;i<length;++i){byte value=memory.ReadByte(A(data,i));bool found=false;for(uint j=0;j<count;++j)if(value==memory.ReadByte(A(set,j))){found=true;break;}if(found==member)return i;}
   return uint.MaxValue;
  }
  uint Substring(uint[] args)
  {
   Arity(args,4);uint data=memory.ReadWord(args[1]),length=memory.ReadWord(unchecked(data-12)),position=args[2];
   if(position>length){nativeException(0x80aec94,new[]{0x969268cu});throw new InvalidOperationException("Native out_of_range owner returned");}
   uint count=Math.Min(args[3],length-position);strings.ConstructRange(args[0],A(data,position),A(data,A(position,count)),0);return args[0];
  }
  public void Destroy(uint owner){vector.Destroy(owner);}
  public uint Invoke(uint method,uint[] args)
  {
   switch(method)
   {
    case 0x80b1084:Arity(args,4);return FindSet(args[0],args[1],args[2],args[3],false);
    case 0x80aa1a4:Arity(args,4);return FindSet(args[0],args[1],args[2],args[3],true);
    case 0x80aa5c4:return Substring(args);
    case 0x9498ed0:Arity(args,3);return vector.Insert(insertionFrame,args[0],args[1],args[2]);
    case 0x81a1a30:Arity(args,1);vector.Destroy(args[0]);return 0;
    case 0x80ac124:Arity(args,2);strings.Copy(args[0],args[1]);return args[0];
    case 0x80ab384:Arity(args,2);return strings.Assign(args[0],args[1]);
    case 0x80ab9f4:Arity(args,2);cow.DestroyRep(args[0]);return 0;
    case 0x80add34:Arity(args,1);strings.Destroy(args[0]);return 0;
    case 0x80aad84:Arity(args,1);return memory.Allocate(args[0]);
    case 0x80aabe4:case 0x94987d0:Arity(args,1);if(args[0]!=0)memory.Free(args[0]);return 0;
    case 0x80adff4:Arity(args,1);return nativeException(method,args);
    case 0x80b1144:Arity(args,0);return nativeException(method,args);
    case 0x80aed64:case 0x80aec94:Arity(args,1);nativeException(method,args);throw new InvalidOperationException("Native nonreturning exception owner returned");
    case 0x80b1284:Arity(args,0);nativeException(method,args);throw new InvalidOperationException("Native nonreturning rethrow owner returned");
    default:throw new ArgumentException("Unbound tokenizer library operation "+method.ToString("x8"));
   }
  }
 }
}
