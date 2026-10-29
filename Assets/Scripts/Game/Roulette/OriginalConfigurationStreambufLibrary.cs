using System;
namespace RouletteDeluxe {
 // Actual normal streambuf imports share one memory, heap and COW owner.
 // Sentry/ios/locale/exception ownership remains outside this streambuf binding.
 public sealed class OriginalConfigurationStreambufLibrary {
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationCowString cow;
  readonly OriginalConfigurationMemoryStrings strings;readonly OriginalConfigurationCountedAppend append;
  readonly OriginalConfigurationStreambufPointers pointers;readonly OriginalConfigurationStreambufControl control;
  readonly uint emptySlot,threadSlot,overflowFrame,reserveFrame;readonly Action<uint> resume;
  public OriginalConfigurationStreambufLibrary(OriginalNativeMemory m,uint emptyRepSlot,uint pthreadSlot,uint lengthErrorCString,uint logicErrorCString,uint nestedOverflowFrame,uint nestedReserveFrame,Action<uint> lengthError,Action<uint> logicError,Action<uint> nativeResume,Func<Exception,bool> isUnwind,Func<Exception,uint> exceptionToken,Func<Exception,uint> exceptionSelector){
   if(m==null||emptyRepSlot==0||pthreadSlot==0||nestedOverflowFrame==0||nestedReserveFrame==0||nestedOverflowFrame==nestedReserveFrame||lengthError==null||logicError==null||nativeResume==null||isUnwind==null||exceptionToken==null||exceptionSelector==null)throw new ArgumentException("Shared physical stream/string memory, separate nested frames and actual error owners required");
   memory=m;emptySlot=emptyRepSlot;threadSlot=pthreadSlot;overflowFrame=nestedOverflowFrame;reserveFrame=nestedReserveFrame;resume=nativeResume;
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=lengthError},emptySlot,threadSlot,lengthErrorCString);
   strings=new OriginalConfigurationMemoryStrings(m,emptySlot,threadSlot,lengthErrorCString,logicErrorCString,lengthError,logicError);
   append=new OriginalConfigurationCountedAppend(new OriginalConfigurationCountedAppend.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,CopyBytes=m.CopyBytes,CloneRep=(rep,allocator,extra)=>cow.Clone(rep,extra),ReleaseRep=(rep,allocator)=>cow.Release(rep),InvokeReserve=(owner,count,frame)=>append.Reserve(owner,count,frame),ThrowLengthError=lengthError},new OriginalConfigurationCountedAppend.Layout{EmptyRepSlot=emptySlot,LengthErrorCString=lengthErrorCString});
   pointers=new OriginalConfigurationStreambufPointers(new OriginalConfigurationStreambufPointers.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord});
   control=new OriginalConfigurationStreambufControl(new OriginalConfigurationStreambufControl.Host{ReadWord=ReadControlWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=isUnwind,ExceptionToken=exceptionToken,ExceptionSelector=exceptionSelector});
  }
  uint ReadControlWord(uint address){if(address==0xe5c80)return memory.ReadWord(emptySlot);if(address==0xe5e90)return memory.ReadWord(threadSlot);return memory.ReadWord(address);}
  public uint Write(uint frame,uint owner,uint source,uint count){return control.Write(frame,owner,source,count);}
  public uint Overflow(uint frame,uint owner,uint character){return control.Overflow(frame,owner,character);}
  public uint Sync(uint owner){return control.Sync(owner);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Packaged streambuf import argument count mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x492f0:Arity(a,3);memory.CopyBytes(a[0],a[1],a[2]);return a[0];
   case 0x92010:Arity(a,2);return control.Overflow(overflowFrame,a[0],a[1]);
   case 0x96eb0:Arity(a,3);return control.Write(overflowFrame,a[0],a[1],a[2]);
   case 0x96bc0:Arity(a,1);return control.Sync(a[0]);
   case 0x48e50:Arity(a,2);append.Reserve(a[0],a[1],reserveFrame);return 0;
   case 0x4a6c0:Arity(a,3);return strings.AssignBytes(a[0],a[1],a[2]);
   case 0x4a190:Arity(a,2);pointers.Swap(a[0],a[1]);return 0;
   case 0x48480:Arity(a,4);pointers.Synchronize(a[0],a[1],a[2],a[3]);return 0;
   case 0x481a0:Arity(a,2);cow.DestroyRep(a[0]);return 0;
   case 0x48d10:Arity(a,1);strings.Destroy(a[0]);return 0;
   case 0x4a490:Arity(a,1);resume(a[0]);throw new InvalidOperationException("Native nonreturning streambuf resume owner returned");
   default:throw new ArgumentException("Unbound streambuf import "+method.ToString("x8"));
  }}
 }
}
