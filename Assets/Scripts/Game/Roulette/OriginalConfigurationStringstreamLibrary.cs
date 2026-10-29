using System;using System.Collections.Generic;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationStringstreamLibrary {
  public sealed class Layout{public Dictionary<uint,uint> Slots;public uint ScratchBase,LengthErrorCString,LogicErrorCString;}
  public sealed class NativeServices{public Action<uint> LengthError,LogicError,Resume,Unexpected;public Action<uint,uint> DestroyIostream;public Action<uint> DestroyStreambufBase;public Func<uint,uint[],uint> Callback;public Func<uint,uint> BeginCatch;public Action EndCatch;public Func<Exception,bool> IsUnwind;public Func<Exception,uint> ExceptionToken,ExceptionSelector;}
  readonly OriginalNativeMemory m;readonly OriginalConfigurationLocaleRuntimeLibrary locale;readonly OriginalConfigurationFacetLookupLibrary facets;readonly Dictionary<uint,uint> slots;readonly uint scratch;readonly NativeServices native;
  readonly OriginalConfigurationMemoryStrings strings;readonly OriginalConfigurationCowString cow;readonly OriginalConfigurationStreambufPointers pointers;readonly OriginalConfigurationStreamLifetime lifetime;readonly OriginalConfigurationStreamOwners owners;readonly OriginalConfigurationStringstreamConstructors constructors;
  public OriginalConfigurationStringstreamLibrary(OriginalNativeMemory memory,OriginalConfigurationLocaleRuntimeLibrary localeOwner,OriginalConfigurationFacetLookupLibrary facetOwner,Layout layout,NativeServices s){
   if(memory==null||localeOwner==null||facetOwner==null||layout==null||layout.Slots==null||layout.ScratchBase==0||layout.ScratchBase>0xfffff9ffu||s==null||s.LengthError==null||s.LogicError==null||s.Resume==null||s.Unexpected==null||s.DestroyIostream==null||s.DestroyStreambufBase==null||s.Callback==null||s.BeginCatch==null||s.EndCatch==null||s.IsUnwind==null||s.ExceptionToken==null||s.ExceptionSelector==null)throw new ArgumentException("Shared stream memory, actual locale/facet libraries, separate frames and native cleanup/error owners required");
   m=memory;locale=localeOwner;facets=facetOwner;scratch=layout.ScratchBase;slots=new Dictionary<uint,uint>(layout.Slots);native=new NativeServices{LengthError=s.LengthError,LogicError=s.LogicError,Resume=s.Resume,Unexpected=s.Unexpected,DestroyIostream=s.DestroyIostream,DestroyStreambufBase=s.DestroyStreambufBase,Callback=s.Callback,BeginCatch=s.BeginCatch,EndCatch=s.EndCatch,IsUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector};
   foreach(uint at in new[]{0xe5c80u,0xe5e90u,0xe5eccu})if(!slots.ContainsKey(at)||slots[at]==0)throw new ArgumentException("Missing stringstream relocation "+at.ToString("x"));m.ReadByte(scratch);m.ReadByte(scratch+0x5ffu);
   strings=new OriginalConfigurationMemoryStrings(m,slots[0xe5c80],slots[0xe5e90],layout.LengthErrorCString,layout.LogicErrorCString,native.LengthError,native.LogicError);
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=native.LengthError},slots[0xe5c80],slots[0xe5e90],layout.LengthErrorCString);
   pointers=new OriginalConfigurationStreambufPointers(new OriginalConfigurationStreambufPointers.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord});
   lifetime=new OriginalConfigurationStreamLifetime(new OriginalConfigurationStreamLifetime.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke},slots[0xe5ecc],slots[0xe5e90]);
   owners=new OriginalConfigurationStreamOwners(new OriginalConfigurationStreamOwners.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=native.IsUnwind,ExceptionToken=native.ExceptionToken,ExceptionSelector=native.ExceptionSelector});
   constructors=new OriginalConfigurationStringstreamConstructors(new OriginalConfigurationStringstreamConstructors.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=native.IsUnwind,ExceptionToken=native.ExceptionToken,ExceptionSelector=native.ExceptionSelector});
  }
  uint ReadWord(uint at){uint cell;return m.ReadWord(slots.TryGetValue(at,out cell)?cell:at);}
  void Check(uint f){if(f>0xfffffeffu||(f>=scratch?f-scratch<0x600u:scratch-f<0x100u))throw new ArgumentException("Caller overlaps nested stream owner frames");}
  public void Construct(uint frame,uint owner,uint source,uint mode,bool hasSource){Check(frame);if(hasSource)constructors.ConstructStringstreamString(frame,owner,0,source,mode);else constructors.ConstructStringstreamMode(frame,owner,0,0,mode);}
  public void ConstructStringbuf(uint frame,uint owner,uint source,uint mode,bool hasSource){Check(frame);if(hasSource)constructors.ConstructStringbufString(frame,owner,0,source,mode);else constructors.ConstructStringbufMode(frame,owner,0,0,mode);}
  public void Destroy(uint frame,uint owner){Check(frame);owners.DestroyStringstream(frame,owner,0);}
  public void DestroyStringbuf(uint frame,uint owner){Check(frame);owners.DestroyStringbuf(frame,owner,0);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Stringstream import arity mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x48480:Arity(a,4);pointers.Synchronize(a[0],a[1],a[2],a[3]);return 0;
   case 0x48800:Arity(a,1);lifetime.ConstructBase(a[0]);return 0;
   case 0x48bc0:Arity(a,1);locale.Construct(scratch+0x100,a[0]);return 0;
   case 0x496a0:Arity(a,2);return locale.Assign(scratch+0x100,a[0],a[1]);
   case 0x49990:Arity(a,1);locale.Destroy(scratch+0x100,a[0]);return 0;
   case 0x48d30:Arity(a,4);strings.ConstructRange(a[0],a[1],unchecked(a[1]+a[2]),a[3]);return 0;
   case 0x48d10:Arity(a,1);strings.Destroy(a[0]);return 0;
   case 0x481a0:Arity(a,2);cow.DestroyRep(a[0]);return 0;
   case 0x49e90:Arity(a,2);lifetime.Initialize(a[0],a[1]);return 0;
   case 0x49ef0:Arity(a,1);return owners.InitializeBase(scratch,a[0],0);
   case 0x49f90:Arity(a,2);facets.CacheLocale(scratch+0x200,a[0],a[1]);return 0;
   case 0x49b70:Arity(a,1);lifetime.DestroyBase(a[0]);return 0;
   case 0x49380:Arity(a,2);return owners.CallCallbacks(scratch+0x300,a[0],a[1]);
   case 0x480c0:Arity(a,1);return owners.DisposeCallbacks(scratch+0x400,a[0],0);
   case 0x48b20:Arity(a,1);return owners.DestroyStringbuf(scratch+0x500,a[0],0);
   case 0x48270:case 0x48900:Arity(a,1);m.Free(a[0]);return 0;
   case 0x489e0:Arity(a,2);native.DestroyIostream(a[0],a[1]);return 0;
   case 0x49ae0:Arity(a,1);native.DestroyStreambufBase(a[0]);return 0;
   case 0x48150:Arity(a,1);return native.BeginCatch(a[0]);
   case 0x497e0:Arity(a,0);native.EndCatch();return 0;
   case 0x4a490:Arity(a,1);native.Resume(a[0]);break;
   case 0x48ba0:Arity(a,1);native.Unexpected(a[0]);break;
   default:return native.Callback(method,a);
  }throw new InvalidOperationException("Native nonreturning stream owner returned");}
 }
}
