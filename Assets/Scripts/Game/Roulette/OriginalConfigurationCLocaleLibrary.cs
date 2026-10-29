using System;
namespace RouletteDeluxe {
 // Actual C++ facet/ctype lifecycle over the same physical memory and heap.
 // Platform newlocale/freelocale and non-ctype virtual owners remain required.
 public sealed class OriginalConfigurationCLocaleLibrary {
  public sealed class Layout {public OriginalConfigurationLocaleFacetLibrary.Layout Facet;public uint ScratchBase,RuntimeErrorCString;}
  public sealed class NativeServices {
   public Func<uint,uint,uint,uint> NewLocale;
   public Action<uint> FreeLocale,RuntimeError,BeginCatch,Resume,Unexpected;
   public Action EndCatch,Rethrow;
   public Func<uint,uint[],uint> OtherFacet;
   public Func<Exception,bool> IsUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationLocaleFacetLifetime lifetime;
  readonly OriginalConfigurationLocaleFacetLibrary facets;readonly NativeServices services;
  readonly uint scratch,errorCString,nameSlot,classicSlot,ctypeSlot;readonly uint[] facetFrames;
  public OriginalConfigurationCLocaleLibrary(OriginalNativeMemory m,Layout l,NativeServices s){
   if(m==null||l==null||l.Facet==null||l.ScratchBase==0||l.RuntimeErrorCString==0||l.ScratchBase>0xfffff8ffu||s==null||s.NewLocale==null||s.FreeLocale==null||s.RuntimeError==null||s.OtherFacet==null||s.BeginCatch==null||s.EndCatch==null||s.Rethrow==null||s.Resume==null||s.Unexpected==null||s.IsUnwind==null||s.ExceptionToken==null||s.ExceptionSelector==null)throw new ArgumentException("Shared locale memory, explicit platform/error owners and separate scratch frames required");
   foreach(uint nested in new[]{l.Facet.IdFrame,l.Facet.ClassicLocaleFrame,l.Facet.FacetBaseFrame})for(uint i=0;i<7;++i)if(!Separate(nested,l.ScratchBase+i*0x100u))throw new ArgumentException("C-locale and facet nested frames overlap");
   memory=m;scratch=l.ScratchBase;errorCString=l.RuntimeErrorCString;nameSlot=l.Facet.ClassicNameSlot;classicSlot=l.Facet.ClassicLocaleSlot;ctypeSlot=l.Facet.CtypeVTableSlot;facetFrames=new[]{l.Facet.IdFrame,l.Facet.ClassicLocaleFrame,l.Facet.FacetBaseFrame};
   services=new NativeServices{NewLocale=s.NewLocale,FreeLocale=s.FreeLocale,RuntimeError=s.RuntimeError,OtherFacet=s.OtherFacet,BeginCatch=s.BeginCatch,EndCatch=s.EndCatch,Rethrow=s.Rethrow,Resume=s.Resume,Unexpected=s.Unexpected,IsUnwind=s.IsUnwind,ExceptionToken=s.ExceptionToken,ExceptionSelector=s.ExceptionSelector};
   lifetime=new OriginalConfigurationLocaleFacetLifetime(new OriginalConfigurationLocaleFacetLifetime.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=services.IsUnwind,ExceptionToken=services.ExceptionToken,ExceptionSelector=services.ExceptionSelector});
   facets=new OriginalConfigurationLocaleFacetLibrary(m,l.Facet,new OriginalConfigurationLocaleFacetLibrary.NativeServices{DestroyFacet=DestroyVirtualFacet,InitializeClassic=(f,a,d,c)=>lifetime.InitializeClassic(scratch+0x400u,0,0,0),BeginCatch=services.BeginCatch,EndCatch=services.EndCatch,Rethrow=services.Rethrow,Resume=services.Resume,Unexpected=services.Unexpected,IsUnwind=services.IsUnwind,ExceptionToken=services.ExceptionToken,ExceptionSelector=services.ExceptionSelector});
  }
  static bool Separate(uint a,uint b){return a>=b?a-b>=0x100u:b-a>=0x100u;}
  void CheckFrame(uint frame){for(uint i=0;i<7;++i)if(!Separate(frame,scratch+i*0x100u))throw new ArgumentException("Caller and C-locale nested frames overlap");foreach(uint nested in facetFrames)if(!Separate(frame,nested))throw new ArgumentException("Caller and facet nested frames overlap");}
  uint ReadWord(uint at){switch(at){case 0xe5e84:return memory.ReadWord(ctypeSlot);case 0xe5d68:return memory.ReadWord(nameSlot);case 0xe5bd0:return memory.ReadWord(classicSlot);default:return memory.ReadWord(at);}}
  public uint GetId(uint frame,uint owner){CheckFrame(frame);return facets.GetId(frame,owner);}
  public void Install(uint frame,uint impl,uint id,uint facet){CheckFrame(frame);facets.Install(frame,impl,id,facet);}
  public void ConstructCtype(uint frame,uint owner,uint table,uint deleteTable,uint references){CheckFrame(frame);facets.ConstructCtype(frame,owner,table,deleteTable,references);}
  public uint GetClassicLocale(uint frame){CheckFrame(frame);return facets.GetClassicLocale(frame);}
  public void CreateCLocale(uint frame,uint owner,uint name,uint basis){CheckFrame(frame);lifetime.CreateCLocale(frame,owner,name,basis);}
  public void DestroyCLocale(uint frame,uint owner){CheckFrame(frame);lifetime.DestroyCLocale(frame,owner,0,0);}
  public void DestroyCtype(uint frame,uint owner){CheckFrame(frame);lifetime.DestroyCtype(frame,owner,0,0);}
  public void DeleteCtype(uint frame,uint owner){CheckFrame(frame);lifetime.DeleteCtype(frame,owner,0,0);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Packaged C-locale import argument count mismatch");}
  uint DestroyVirtualFacet(uint method,uint[] a){Arity(a,1);if(method==0x54eb0)return lifetime.DeleteCtype(scratch+0x100u,a[0],0,0);return services.OtherFacet(method,a);}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x47f60:Arity(a,1);return lifetime.DestroyCtype(scratch,a[0],0,0);
   case 0x48270:case 0x48900:Arity(a,1);memory.Free(a[0]);return 0;
   case 0x489c0:Arity(a,3);return lifetime.CreateCLocale(scratch+0x200u,a[0],a[1],a[2]);
   case 0x48d70:Arity(a,1);return lifetime.DestroyCLocale(scratch+0x300u,a[0],0,0);
   case 0x49d10:Arity(a,0);return facets.GetClassicLocale(scratch+0x500u);
   case 0x4a150:Arity(a,1);facets.DestroyFacetBase(scratch+0x600u,a[0]);return 0;
   case 0x4a1a0:Arity(a,3);return services.NewLocale(a[0],a[1],a[2]);
   case 0x49bd0:Arity(a,1);services.FreeLocale(a[0]);return 0;
   case 0x494b0:Arity(a,1);if(a[0]!=0xbf454u)throw new ArgumentException("Unexpected C-locale runtime error text address");services.RuntimeError(errorCString);break;
   case 0x4a490:Arity(a,1);services.Resume(a[0]);break;
   default:throw new ArgumentException("Unbound C-locale import "+method.ToString("x8"));
  }throw new InvalidOperationException("Native nonreturning C-locale operation returned");}
 }
}
