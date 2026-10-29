using System;using System.Collections.Generic;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationLocaleRuntimeLibrary {
  public sealed class Layout{public Dictionary<uint,uint> Slots;public uint ScratchBase;}
  public sealed class NativeServices{public Func<uint,uint,Action,uint> Once;public Func<uint,uint> GuardAcquire,MutexLock,MutexUnlock,AllocateException,BeginCatch;public Action<uint> GuardRelease,Resume,Unexpected;public Action<uint,uint,uint> Throw;public Action EndCatch;public Func<uint,uint[],uint> VirtualD0;public Func<Exception,bool> IsUnwind;public Func<Exception,uint> ExceptionToken,ExceptionSelector;}
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationClassicLocaleLibrary classic;readonly OriginalConfigurationLocaleSupport support;readonly OriginalConfigurationLocaleConstructor constructor;readonly OriginalConfigurationLocaleImplLifetime implLifetime;readonly OriginalConfigurationStreamLifetime localeLifetime;readonly Dictionary<uint,uint> slots;readonly NativeServices services;readonly uint scratch;
  public OriginalConfigurationLocaleRuntimeLibrary(OriginalNativeMemory m,OriginalConfigurationClassicLocaleLibrary c,Layout l,NativeServices s){
   if(m==null||c==null||l==null||l.Slots==null||l.ScratchBase==0||l.ScratchBase>0xfffff9ffu||s==null||s.Once==null||s.GuardAcquire==null||s.GuardRelease==null||s.MutexLock==null||s.MutexUnlock==null||s.AllocateException==null||s.BeginCatch==null||s.EndCatch==null||s.Throw==null||s.Resume==null||s.Unexpected==null||s.VirtualD0==null||s.IsUnwind==null||s.ExceptionToken==null||s.ExceptionSelector==null)throw new ArgumentException("Shared original locale memory, actual classic construction, separate frames and platform/exception/virtual owners required");
   memory=m;classic=c;scratch=l.ScratchBase;slots=new Dictionary<uint,uint>(l.Slots);foreach(uint at in new uint[]{940872u,941056u,941068u,941308u,941384u,941432u,941712u,941772u,941812u,941908u,941940u,942024u})if(!slots.ContainsKey(at)||slots[at]==0)throw new ArgumentException("Missing runtime relocation "+at.ToString("x"));
   if(ReadWord(0xe5ef4)!=0x5b310)throw new ArgumentException("Exact original once function required");
   services=new NativeServices{Once=s.Once,GuardAcquire=s.GuardAcquire,GuardRelease=s.GuardRelease,MutexLock=s.MutexLock,MutexUnlock=s.MutexUnlock,AllocateException=s.AllocateException,BeginCatch=s.BeginCatch,EndCatch=s.EndCatch,Throw=s.Throw,Resume=s.Resume,Unexpected=s.Unexpected,VirtualD0=s.VirtualD0,IsUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector};
   support=new OriginalConfigurationLocaleSupport(new OriginalConfigurationLocaleSupport.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector});
   constructor=new OriginalConfigurationLocaleConstructor(new OriginalConfigurationLocaleConstructor.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector});
   implLifetime=new OriginalConfigurationLocaleImplLifetime(new OriginalConfigurationLocaleImplLifetime.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector});
   localeLifetime=new OriginalConfigurationStreamLifetime(new OriginalConfigurationStreamLifetime.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke},slots[941772u],slots[0xe5e90]);
  }
  uint ReadWord(uint at){uint cell;return memory.ReadWord(slots.TryGetValue(at,out cell)?cell:at);}
  void CheckFrame(uint f){for(uint i=0;i<6;++i){uint n=scratch+i*0x100u;if(f>=n?f-n<0x100u:n-f<0x100u)throw new ArgumentException("Caller overlaps locale runtime nested frame");}}
  public void Initialize(uint frame){CheckFrame(frame);support.Initialize(frame,0,0);}
  public uint GetMutex(uint frame){CheckFrame(frame);return support.GetMutex(frame,0,0);}
  public void Construct(uint frame,uint owner){CheckFrame(frame);constructor.Construct(frame,owner);}
  public uint Assign(uint frame,uint owner,uint source){CheckFrame(frame);return support.Assign(frame,owner,source);}
  public void Destroy(uint frame,uint owner){CheckFrame(frame);localeLifetime.DestroyLocale(owner);}
  public void DestroyImpl(uint frame,uint owner){CheckFrame(frame);implLifetime.Destroy(frame,owner);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Locale runtime import arity mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x5b360:Arity(a,4);return support.Initialize(scratch,0,0);
   case 0x5a150:Arity(a,4);return support.GetMutex(scratch+0x100u,0,0);
   case 0x5a200:Arity(a,4);return support.ThrowMutexError(scratch+0x200u,0,0);
   case 0x5b310:Arity(a,0);return support.InitializeOnce(scratch+0x300u,0,0);
   case 0x48cd0:Arity(a,2);classic.Construct(scratch+0x500u,a[0],a[1]);return 0;
   case 0x485b0:Arity(a,1);return implLifetime.Destroy(scratch+0x400u,a[0]);
   case 0x48900:case 0x48270:Arity(a,1);memory.Free(a[0]);return 0;
   case 0x48bf0:Arity(a,2);if(a[1]!=0x5b310)throw new ArgumentException("Unknown original once function");return services.Once(a[0],a[1],()=>support.InitializeOnce(scratch+0x300u,0,0));
   case 0x49800:Arity(a,1);return services.GuardAcquire(a[0]);
   case 0x48850:Arity(a,1);services.GuardRelease(a[0]);return 0;
   case 0x49a80:Arity(a,1);return services.MutexLock(a[0]);
   case 0x49030:Arity(a,1);return services.MutexUnlock(a[0]);
   case 0x48ec0:Arity(a,1);return services.AllocateException(a[0]);
   case 0x49370:Arity(a,3);services.Throw(a[0],a[1],a[2]);break;
   case 0x48150:Arity(a,1);return services.BeginCatch(a[0]);
   case 0x497e0:Arity(a,0);services.EndCatch();return 0;
   case 0x4a490:Arity(a,1);services.Resume(a[0]);break;
   case 0x48ba0:Arity(a,1);services.Unexpected(a[0]);break;
   default:Arity(a,1);return services.VirtualD0(method,a);
  }throw new InvalidOperationException("Native nonreturning locale runtime operation returned");}
 }
}
