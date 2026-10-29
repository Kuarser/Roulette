using System;
namespace RouletteDeluxe {
 // Shared-memory binding for recovered facet ID, array installation and ctype C1.
 // Virtual facet D0, platform classic initialization and native exception owners
 // remain explicit. No fixture event replay supplies normal imported effects.
 public sealed class OriginalConfigurationLocaleFacetLibrary {
  public sealed class Layout {
   public uint PthreadSlot,IdCounterSlot,ClassicNameSlot,ClassicLocaleSlot,FacetVTableSlot,CtypeVTableSlot;
   public uint IdFrame,ClassicLocaleFrame,FacetBaseFrame;
  }
  public sealed class NativeServices {
   public Func<uint,uint[],uint> DestroyFacet;
   public Action<uint,uint,uint,uint> InitializeClassic;
   public Action<uint> BeginCatch,Resume,Unexpected;
   public Action EndCatch,Rethrow;
   public Func<Exception,bool> IsUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationLocaleFacetCore core;
  readonly Layout layout;readonly NativeServices services;
  public OriginalConfigurationLocaleFacetLibrary(OriginalNativeMemory m,Layout l,NativeServices s) {
   if(m==null||l==null||s==null||l.PthreadSlot==0||l.IdCounterSlot==0||l.ClassicNameSlot==0||l.ClassicLocaleSlot==0||l.FacetVTableSlot==0||l.CtypeVTableSlot==0||l.IdFrame==0||l.ClassicLocaleFrame==0||l.FacetBaseFrame==0||l.IdFrame==l.ClassicLocaleFrame||l.IdFrame==l.FacetBaseFrame||l.ClassicLocaleFrame==l.FacetBaseFrame||s.DestroyFacet==null||s.InitializeClassic==null||s.BeginCatch==null||s.EndCatch==null||s.Rethrow==null||s.Resume==null||s.Unexpected==null||s.IsUnwind==null||s.ExceptionToken==null||s.ExceptionSelector==null)throw new ArgumentException("Shared locale memory, relocation slots, separate nested frames and native ownership services required");
   if(!Separate(l.IdFrame,l.ClassicLocaleFrame)||!Separate(l.IdFrame,l.FacetBaseFrame)||!Separate(l.ClassicLocaleFrame,l.FacetBaseFrame))throw new ArgumentException("Nested facet frames must have separate 256-byte windows");
   memory=m;layout=new Layout{PthreadSlot=l.PthreadSlot,IdCounterSlot=l.IdCounterSlot,ClassicNameSlot=l.ClassicNameSlot,ClassicLocaleSlot=l.ClassicLocaleSlot,FacetVTableSlot=l.FacetVTableSlot,CtypeVTableSlot=l.CtypeVTableSlot,IdFrame=l.IdFrame,ClassicLocaleFrame=l.ClassicLocaleFrame,FacetBaseFrame=l.FacetBaseFrame};
   services=new NativeServices{DestroyFacet=s.DestroyFacet,InitializeClassic=s.InitializeClassic,BeginCatch=s.BeginCatch,EndCatch=s.EndCatch,Rethrow=s.Rethrow,Resume=s.Resume,Unexpected=s.Unexpected,IsUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector};
   core=new OriginalConfigurationLocaleFacetCore(new OriginalConfigurationLocaleFacetCore.Host{ReadWord=ReadWord,ReadHalf=m.ReadHalf,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteHalf=m.WriteHalf,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=services.IsUnwind,ExceptionToken=services.ExceptionToken,ExceptionSelector=services.ExceptionSelector});
  }
  uint ReadWord(uint at) {switch(at){case 0xe5e90:return memory.ReadWord(layout.PthreadSlot);case 0xe5e28:return memory.ReadWord(layout.IdCounterSlot);case 0xe5d68:return memory.ReadWord(layout.ClassicNameSlot);case 0xe5bd0:return memory.ReadWord(layout.ClassicLocaleSlot);case 0xe5b80:return memory.ReadWord(layout.FacetVTableSlot);case 0xe5e84:return memory.ReadWord(layout.CtypeVTableSlot);default:return memory.ReadWord(at);}}
  static bool Separate(uint a,uint b){return a>=b?a-b>=0x100u:b-a>=0x100u;}
  void CheckFrame(uint frame){foreach(uint nested in new[]{layout.IdFrame,layout.ClassicLocaleFrame,layout.FacetBaseFrame})if(!Separate(frame,nested))throw new ArgumentException("Caller and nested facet frames must have separate 256-byte windows");}
  public uint GetId(uint frame,uint owner){CheckFrame(frame);return core.GetId(frame,owner,0,0,0);}
  public void Install(uint frame,uint impl,uint id,uint facet){CheckFrame(frame);core.Install(frame,impl,id,facet,0);}
  public uint GetClassicName(uint frame){CheckFrame(frame);return core.GetClassicName(frame,0,0,0,0);}
  public uint GetClassicLocale(uint frame){CheckFrame(frame);return core.GetClassicLocale(frame,0,0,0,0);}
  public void DestroyFacetBase(uint frame,uint owner){CheckFrame(frame);core.DestroyFacetBase(frame,owner,0,0,0);}
  public void ConstructCtype(uint frame,uint owner,uint table,uint deleteTable,uint references){CheckFrame(frame);core.ConstructCtype(frame,owner,table,deleteTable,references);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Packaged facet import argument count mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x48100:Arity(a,1);return memory.Allocate(a[0]);
   case 0x48270:Arity(a,1);memory.Free(a[0]);return 0;
   case 0x48b50:Arity(a,1);return core.GetId(layout.IdFrame,a[0],0,0,0);
   case 0x49d10:Arity(a,0);return core.GetClassicLocale(layout.ClassicLocaleFrame,0,0,0,0);
   case 0x4a150:Arity(a,1);return core.DestroyFacetBase(layout.FacetBaseFrame,a[0],0,0,0);
   case 0x59710:Arity(a,4);services.InitializeClassic(a[0],a[1],a[2],a[3]);return 0;
   case 0x48150:Arity(a,1);services.BeginCatch(a[0]);return 0;
   case 0x497e0:Arity(a,0);services.EndCatch();return 0;
   case 0x4a410:Arity(a,0);services.Rethrow();break;
   case 0x4a490:Arity(a,1);services.Resume(a[0]);break;
   case 0x48ba0:Arity(a,1);services.Unexpected(a[0]);break;
   default:Arity(a,1);return services.DestroyFacet(method,a);
  }throw new InvalidOperationException("Native nonreturning facet operation returned");}
 }
}
