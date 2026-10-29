using System;
namespace RouletteDeluxe {
 // Actual insert/sentry/ios/flush/streambuf owners share the same physical bytes.
 // Native exception state and ctype operations remain explicit required services.
 public sealed class OriginalConfigurationOstreamLibrary {
  public sealed class NativeServices {
   public Func<uint> UncaughtException;public Func<uint,uint[],uint> Facet;
   public Action<uint> IosFailure,BeginCatch,Resume;public Action BadCast,EndCatch,Rethrow;
   public Func<Exception,bool> IsUnwind;public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly OriginalNativeMemory m;readonly NativeServices n;readonly uint scratch;
  readonly OriginalConfigurationOstreamInsert insert;readonly OriginalConfigurationStreambufControl sentry;
  readonly OriginalConfigurationBasicIosControl ios;readonly OriginalConfigurationStreambufLibrary stream;
  public OriginalConfigurationOstreamLibrary(OriginalNativeMemory memory,uint emptySlot,uint threadSlot,uint lengthCString,uint logicCString,uint scratchFrames,Action<uint> lengthError,Action<uint> logicError,NativeServices native){
   if(memory==null||native==null||scratchFrames==0||scratchFrames>0xfffff7ff)throw new ArgumentException("Physical stream memory, separate scratch frames and native services required");
   foreach(var f in typeof(NativeServices).GetFields())if(f.GetValue(native)==null)throw new ArgumentException("Required native ostream service: "+f.Name);
   m=memory;scratch=scratchFrames;n=new NativeServices();foreach(var f in typeof(NativeServices).GetFields())f.SetValue(n,f.GetValue(native));
   stream=new OriginalConfigurationStreambufLibrary(m,emptySlot,threadSlot,lengthCString,logicCString,scratch+0x600,scratch+0x700,lengthError,logicError,n.Resume,n.IsUnwind,n.ExceptionToken,n.ExceptionSelector);
   sentry=new OriginalConfigurationStreambufControl(new OriginalConfigurationStreambufControl.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=n.IsUnwind,ExceptionToken=n.ExceptionToken,ExceptionSelector=n.ExceptionSelector});
   ios=new OriginalConfigurationBasicIosControl(new OriginalConfigurationBasicIosControl.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=n.IsUnwind,ExceptionToken=n.ExceptionToken,ExceptionSelector=n.ExceptionSelector});
   insert=new OriginalConfigurationOstreamInsert(new OriginalConfigurationOstreamInsert.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=n.IsUnwind,ExceptionToken=n.ExceptionToken,ExceptionSelector=n.ExceptionSelector});
  }
  public uint Insert(uint frame,uint owner,uint source,uint count){if(unchecked(frame-scratch)<0x800)throw new ArgumentException("Caller insert frame overlaps nested scratch frames");return insert.Insert(frame,owner,source,count);}
  public uint Flush(uint owner){return ios.Flush(scratch+0x400,owner,0);}
  static void Arity(uint[] a,int size){if(a==null||a.Length!=size)throw new ArgumentException("Original ostream argument count mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x4a690:Arity(a,2);return sentry.ConstructSentry(scratch,a[0],a[1]);
   case 0x487a0:Arity(a,1);return sentry.DestroySentry(scratch+0x100,a[0]);
   case 0x4a2f0:Arity(a,2);return ios.Clear(scratch+0x200,a[0],a[1]);
   case 0x49080:Arity(a,2);return ios.SetState(scratch+0x300,a[0],a[1]);
   case 0x49630:Arity(a,1);return ios.Flush(scratch+0x400,a[0],0);
   case 0x96eb0:Arity(a,3);return stream.Write(scratch+0x500,a[0],a[1],a[2]);
   case 0x92010:Arity(a,2);return stream.Overflow(scratch+0x600,a[0],a[1]);
   case 0x96bc0:Arity(a,1);return stream.Sync(a[0]);
   case 0x48c60:Arity(a,0);return n.UncaughtException();
   case 0x491b0:Arity(a,1);n.IosFailure(a[0]);throw new InvalidOperationException("Native ios failure returned");
   case 0x48a40:Arity(a,0);n.BadCast();throw new InvalidOperationException("Native bad_cast returned");
   case 0x48150:Arity(a,1);n.BeginCatch(a[0]);return 0;
   case 0x497e0:Arity(a,0);n.EndCatch();return 0;
   case 0x4a410:Arity(a,0);n.Rethrow();throw new InvalidOperationException("Native rethrow returned");
   case 0x4a490:Arity(a,1);n.Resume(a[0]);throw new InvalidOperationException("Native Resume returned");
   case 0x49070:Arity(a,1);return n.Facet(method,a);
   default:
    if(a!=null&&a.Length==2&&method==m.ReadWord(unchecked(m.ReadWord(a[0])+0x18)))return n.Facet(method,a);
    throw new ArgumentException("Unbound ostream operation "+method.ToString("x8"));
  }}
 }
}
