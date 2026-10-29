using System;using System.Collections.Generic;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationFacetLookupLibrary {
  public sealed class Layout{public Dictionary<uint,uint> Slots;public uint IdFrame,CastFrame,QueryFrame;public uint[] RttiFrames;}
  public sealed class NativeServices{public Action ThrowMissing,ThrowMismatch;}
  readonly OriginalNativeMemory m;readonly Dictionary<uint,uint> slots;readonly uint idFrame,castFrame,queryFrame;readonly uint[] privateFrames;readonly NativeServices native;
  readonly OriginalConfigurationLocaleFacetCore core;readonly OriginalConfigurationFacetLookup lookup;readonly OriginalConfigurationBasicIosControl ios;readonly OriginalConfigurationRttiLibrary rtti;
  public OriginalConfigurationFacetLookupLibrary(OriginalNativeMemory memory,Layout layout,NativeServices services){
   if(memory==null||layout==null||layout.Slots==null||layout.RttiFrames==null||services==null||services.ThrowMissing==null||services.ThrowMismatch==null)throw new ArgumentException("Shared original bytes, relocation slots, independent frames and bad-cast owners required");
   m=memory;slots=new Dictionary<uint,uint>(layout.Slots);idFrame=layout.IdFrame;castFrame=layout.CastFrame;queryFrame=layout.QueryFrame;native=new NativeServices{ThrowMissing=services.ThrowMissing,ThrowMismatch=services.ThrowMismatch};privateFrames=new uint[layout.RttiFrames.Length+3];privateFrames[0]=idFrame;privateFrames[1]=castFrame;privateFrames[2]=queryFrame;Array.Copy(layout.RttiFrames,0,privateFrames,3,layout.RttiFrames.Length);
   for(int i=0;i<privateFrames.Length;i++){uint f=privateFrames[i];if(f==0||f>0xfffffeffu)throw new ArgumentException("Representable nonempty facet frame required");m.ReadByte(f);m.ReadByte(f+255u);for(int j=0;j<i;j++)if(Overlap(f,privateFrames[j]))throw new ArgumentException("Overlapping facet recursive frames");}
   foreach(uint at in new uint[]{941160,941188,941248,941328,941608,941640,941648,941712,941996})if(!slots.ContainsKey(at)||slots[at]==0)throw new ArgumentException("Missing facet lookup relocation "+at.ToString("x"));
   rtti=new OriginalConfigurationRttiLibrary(m,layout.RttiFrames);
   core=new OriginalConfigurationLocaleFacetCore(new OriginalConfigurationLocaleFacetCore.Host{ReadWord=ReadWord,ReadHalf=m.ReadHalf,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteHalf=m.WriteHalf,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   lookup=new OriginalConfigurationFacetLookup(new OriginalConfigurationFacetLookup.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   ios=new OriginalConfigurationBasicIosControl(new OriginalConfigurationBasicIosControl.Host{ReadWord=ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
  }
  uint ReadWord(uint at){uint cell;return m.ReadWord(slots.TryGetValue(at,out cell)?cell:at);}
  static bool Overlap(uint a,uint b){return a>=b?a-b<256u:b-a<256u;}
  void Check(uint f){if(f>0xfffffeffu)throw new ArgumentException("Representable facet caller frame required");foreach(uint nested in privateFrames)if(Overlap(f,nested))throw new ArgumentException("Caller overlaps facet nested frame");}
  uint Query(uint f,int kind,uint locale,bool use){switch(kind){case 0:return use?lookup.UseCtype(f,locale):lookup.HasCtype(f,locale);case 1:return use?lookup.UseNumPut(f,locale):lookup.HasNumPut(f,locale);case 2:return use?lookup.UseNumGet(f,locale):lookup.HasNumGet(f,locale);default:throw new ArgumentException("Char facet kind required");}}
  public bool Has(uint frame,int kind,uint locale){Check(frame);return Query(frame,kind,locale,false)!=0;}
  public uint Use(uint frame,int kind,uint locale){Check(frame);return Query(frame,kind,locale,true);}
  public void CacheLocale(uint frame,uint owner,uint locale){Check(frame);ios.CacheLocale(frame,owner,locale);}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("Facet lookup import arity mismatch");}
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x48b50:Arity(a,1);return core.GetId(idFrame,a[0],0,0,0);
   case 0x49ca0:Arity(a,4);return rtti.Cast(castFrame,a[0],a[1],a[2],a[3]);
   case 0x4a6b0:Arity(a,1);return Query(queryFrame,0,a[0],false);
   case 0x47fa0:Arity(a,1);return Query(queryFrame,0,a[0],true);
   case 0x49c50:Arity(a,1);return Query(queryFrame,1,a[0],false);
   case 0x48200:Arity(a,1);return Query(queryFrame,1,a[0],true);
   case 0x47ea0:Arity(a,1);return Query(queryFrame,2,a[0],false);
   case 0x48ca0:Arity(a,1);return Query(queryFrame,2,a[0],true);
   case 0x48a40:Arity(a,0);native.ThrowMissing();break;
   case 0x48cb0:Arity(a,0);native.ThrowMismatch();break;
   default:throw new ArgumentException("Unbound facet lookup import "+method.ToString("x"));
  }throw new InvalidOperationException("Native nonreturning bad-cast operation returned");}
 }
}
