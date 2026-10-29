using System;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationStringstreamIoLibrary {
  readonly OriginalConfigurationStringstreamLibrary streams;readonly OriginalConfigurationStreambufLibrary buffers;readonly uint overflowFrame,reserveFrame;
  public OriginalConfigurationStringstreamIoLibrary(OriginalNativeMemory m,OriginalConfigurationLocaleRuntimeLibrary locale,OriginalConfigurationFacetLookupLibrary facets,OriginalConfigurationStringstreamLibrary.Layout layout,OriginalConfigurationStringstreamLibrary.NativeServices native,uint nestedOverflowFrame,uint nestedReserveFrame){
   if(m==null||layout==null||native==null||nestedOverflowFrame==0||nestedReserveFrame==0||nestedOverflowFrame>0xfffffeffu||nestedReserveFrame>0xfffffeffu||Overlap(nestedOverflowFrame,nestedReserveFrame))throw new ArgumentException("Shared stream owner and independent write frames required");
   foreach(uint f in new[]{nestedOverflowFrame,nestedReserveFrame}){if(f>=layout.ScratchBase?f-layout.ScratchBase<0x600u:layout.ScratchBase-f<256u)throw new ArgumentException("Write frames overlap nested stream ownership");m.ReadByte(f);m.ReadByte(f+255u);}
   overflowFrame=nestedOverflowFrame;reserveFrame=nestedReserveFrame;streams=new OriginalConfigurationStringstreamLibrary(m,locale,facets,layout,native);
   buffers=new OriginalConfigurationStreambufLibrary(m,layout.Slots[0xe5c80],layout.Slots[0xe5e90],layout.LengthErrorCString,layout.LogicErrorCString,overflowFrame,reserveFrame,native.LengthError,native.LogicError,native.Resume,native.IsUnwind,native.ExceptionToken,native.ExceptionSelector);
  }
  static bool Overlap(uint a,uint b){return a>=b?a-b<256u:b-a<256u;}
  void Check(uint f){if(f>0xfffffeffu||Overlap(f,overflowFrame)||Overlap(f,reserveFrame))throw new ArgumentException("Caller overlaps nested stream write frames");}
  static uint Buffer(uint owner,bool bufferOnly){return bufferOnly?owner:unchecked(owner+12u);}
  public void Construct(uint frame,uint owner,uint source,uint mode,bool hasSource,bool bufferOnly){Check(frame);if(bufferOnly)streams.ConstructStringbuf(frame,owner,source,mode,hasSource);else streams.Construct(frame,owner,source,mode,hasSource);}
  public uint Write(uint frame,uint owner,uint source,uint count,bool bufferOnly){Check(frame);return buffers.Write(frame,Buffer(owner,bufferOnly),source,count);}
  public uint Overflow(uint frame,uint owner,uint character,bool bufferOnly){Check(frame);return buffers.Overflow(frame,Buffer(owner,bufferOnly),character);}
  public uint Sync(uint owner,bool bufferOnly){return buffers.Sync(Buffer(owner,bufferOnly));}
  public void Destroy(uint frame,uint owner,bool bufferOnly){Check(frame);if(bufferOnly)streams.DestroyStringbuf(frame,owner);else streams.Destroy(frame,owner);}
 }
}
