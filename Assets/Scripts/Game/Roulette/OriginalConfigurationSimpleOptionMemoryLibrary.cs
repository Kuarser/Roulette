using System;
namespace RouletteDeluxe
{
 public sealed class OriginalConfigurationSimpleOptionMemoryLibrary
 {
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationCowString cow;readonly OriginalConfigurationSimpleOptionMemory control;readonly Action<uint> logicError;
  public OriginalConfigurationSimpleOptionMemoryLibrary(OriginalNativeMemory m,uint emptySlot,uint threadSlot,uint lengthErrorCString,Action<uint> lengthError,Action<uint> nativeLogicError,Func<Exception,bool> isUnwind,Func<Exception,uint> exceptionToken)
  {
   if(m==null||lengthError==null||nativeLogicError==null||isUnwind==null||exceptionToken==null)throw new ArgumentException("Shared physical memory, relocated COW slots and native error owners required");
   memory=m;logicError=nativeLogicError;
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=lengthError},emptySlot,threadSlot,lengthErrorCString);
   control=new OriginalConfigurationSimpleOptionMemory(new OriginalConfigurationSimpleOptionMemory.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=isUnwind,ExceptionToken=exceptionToken});
  }
  public void ReHash(uint owner){control.ReHash(owner);}
  public uint ConstructRange(uint frame,uint owner,uint begin,uint end,uint allocator){return control.ConstructRange(frame,owner,begin,end,allocator);}
  static void Arity(uint[] args,int n){if(args==null||args.Length!=n)throw new ArgumentException("Original range import argument count mismatch");}
  uint Invoke(uint method,uint[] args)
  {
   switch(method){
    case 0x80ae694:Arity(args,3);return cow.Create(args[0],args[1]);
    case 0x80ae3e4:Arity(args,3);memory.CopyBytes(args[0],args[1],args[2]);return args[0];
    case 0x80b0064:Arity(args,1);logicError(args[0]);throw new InvalidOperationException("Native nonreturning logic_error owner returned");
    default:throw new ArgumentException("Unbound physical range import "+method.ToString("x8"));
   }
  }
 }
}
