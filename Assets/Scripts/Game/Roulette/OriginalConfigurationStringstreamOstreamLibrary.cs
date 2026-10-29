using System;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationStringstreamOstreamLibrary {
  readonly OriginalConfigurationStringstreamIoLibrary streams;readonly OriginalConfigurationOstreamLibrary output;readonly uint ostreamScratch,ctypeFrame;
  public OriginalConfigurationStringstreamOstreamLibrary(OriginalNativeMemory m,OriginalConfigurationLocaleRuntimeLibrary locale,OriginalConfigurationFacetLookupLibrary facets,OriginalConfigurationStringstreamLibrary.Layout layout,OriginalConfigurationStringstreamLibrary.NativeServices native,uint overflowFrame,uint reserveFrame,uint nestedOstreamScratch,uint nestedCtypeFrame,OriginalConfigurationOstreamLibrary.NativeServices ostreamNative,Func<uint,uint[],uint> derivedWiden){
   if(m==null||layout==null||native==null||ostreamNative==null||derivedWiden==null||nestedOstreamScratch==0||nestedOstreamScratch>0xfffff7ffu||nestedCtypeFrame==0||nestedCtypeFrame>0xfffffdffu)throw new ArgumentException("Shared original stream/ctype bytes, distinct frames and native services required");ostreamScratch=nestedOstreamScratch;ctypeFrame=nestedCtypeFrame;
   uint[] at={layout.ScratchBase,overflowFrame,reserveFrame,ostreamScratch,ctypeFrame};uint[] size={0x600,256,256,0x800,512};for(int i=0;i<at.Length;i++){m.ReadByte(at[i]);m.ReadByte(unchecked(at[i]+size[i]-1));for(int j=0;j<i;j++)if(Overlap(at[i],size[i],at[j],size[j]))throw new ArgumentException("Overlapping stream/ostream/ctype frames");}
   streams=new OriginalConfigurationStringstreamIoLibrary(m,locale,facets,layout,native,overflowFrame,reserveFrame);var ctype=new OriginalConfigurationCtypeLibrary(m,derivedWiden);
   var services=new OriginalConfigurationOstreamLibrary.NativeServices{UncaughtException=ostreamNative.UncaughtException,Facet=(method,args)=>ctype.InvokeForOstream(ctypeFrame,method,args),IosFailure=ostreamNative.IosFailure,BeginCatch=ostreamNative.BeginCatch,Resume=ostreamNative.Resume,BadCast=ostreamNative.BadCast,EndCatch=ostreamNative.EndCatch,Rethrow=ostreamNative.Rethrow,IsUnwind=ostreamNative.IsUnwind,ExceptionToken=ostreamNative.ExceptionToken,ExceptionSelector=ostreamNative.ExceptionSelector};
   output=new OriginalConfigurationOstreamLibrary(m,layout.Slots[0xe5c80],layout.Slots[0xe5e90],layout.LengthErrorCString,layout.LogicErrorCString,ostreamScratch,native.LengthError,native.LogicError,services);
  }
  static bool Overlap(uint a,uint sa,uint b,uint sb){return a>=b?a-b<sb:b-a<sa;}
  void Check(uint f){if(f>0xfffffeffu||Overlap(f,256,ostreamScratch,0x800)||Overlap(f,256,ctypeFrame,512))throw new ArgumentException("Caller overlaps ostream/ctype frames");}
  public void Construct(uint frame,uint owner,uint source,uint mode,bool hasSource){Check(frame);streams.Construct(frame,owner,source,mode,hasSource,false);}
  public uint Insert(uint frame,uint owner,uint source,uint count){Check(frame);return output.Insert(frame,unchecked(owner+8u),source,count);}
  public uint Flush(uint owner){return output.Flush(unchecked(owner+8u));}
  public void Destroy(uint frame,uint owner){Check(frame);streams.Destroy(frame,owner,false);}
 }
}
