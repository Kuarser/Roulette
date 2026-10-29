using System;
namespace RouletteDeluxe {
 public sealed class OriginalConfigurationStringstreamResultLibrary {
  public sealed class NativeServices {
   public Action<uint> LengthError,LogicError,Resume;
   public Func<Exception,bool> IsUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly OriginalNativeMemory memory;readonly OriginalConfigurationStringstreamResult control;readonly OriginalConfigurationMemoryStrings strings;readonly OriginalConfigurationCowString cow;readonly NativeServices native;
  public OriginalConfigurationStringstreamResultLibrary(OriginalNativeMemory m,uint emptyRepSlot,uint threadSlot,uint lengthErrorCString,uint logicErrorCString,NativeServices services){
   if(m==null||services==null||services.LengthError==null||services.LogicError==null||services.Resume==null||services.IsUnwind==null||services.ExceptionToken==null||services.ExceptionSelector==null)throw new ArgumentException("Original shared string bytes, COW slots and native errors required");memory=m;
   native=new NativeServices{LengthError=services.LengthError,LogicError=services.LogicError,Resume=services.Resume,IsUnwind=services.IsUnwind,ExceptionToken=services.ExceptionToken,ExceptionSelector=services.ExceptionSelector};
   strings=new OriginalConfigurationMemoryStrings(m,emptyRepSlot,threadSlot,lengthErrorCString,logicErrorCString,native.LengthError,native.LogicError);
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=native.LengthError},emptyRepSlot,threadSlot,lengthErrorCString);
   control=new OriginalConfigurationStringstreamResult(new OriginalConfigurationStringstreamResult.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=native.IsUnwind,ExceptionToken=native.ExceptionToken,ExceptionSelector=native.ExceptionSelector});
  }
  uint Invoke(uint method,uint[] a){switch(method){
   case 0x49840:strings.ConstructRange(a[0],a[1],a[2],a[3]);return 0;
   case 0x49f60:return strings.Assign(a[0],a[1]);
   case 0x481a0:cow.DestroyRep(a[0]);return 0;
   case 0x48d10:strings.Destroy(a[0]);return 0;
   case 0x4a490:native.Resume(a[0]);throw new InvalidOperationException("Original resume returned");
   default:throw new InvalidOperationException("Unbound original string getter import "+method.ToString("x"));
  }}
  static bool Overlap(uint a,uint size,uint b,uint other){return a>=b?a-b<other:b-a<size;}
  void Check(uint frame,uint owner,uint result,uint extent){if(frame==0||frame>0xfffffeffu||owner>uint.MaxValue-extent||result>0xfffffffbu||Overlap(frame,256,owner,extent)||Overlap(frame,256,result,4)||Overlap(owner,extent,result,4))throw new ArgumentException("Distinct original getter frame, stream and hidden return owner required");memory.ReadByte(frame);memory.ReadByte(frame+255);memory.ReadByte(owner);memory.ReadByte(owner+extent-1);memory.ReadWord(result);}
  public uint GetStringbuf(uint frame,uint owner,uint result){Check(frame,owner,result,0x28);return control.GetStringbuf(frame,owner,result);}
  public uint GetStringstream(uint frame,uint owner,uint result){Check(frame,owner,result,0x34);return control.GetStringstream(frame,owner,result);}
  public uint GetOstringstream(uint frame,uint owner,uint result){Check(frame,owner,result,0x2c);return control.GetOstringstream(frame,owner,result);}
  public void DestroyResult(uint owner){strings.Destroy(owner);}
 }
}
