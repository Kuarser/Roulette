using System;
namespace RouletteDeluxe
{
 // Packaged assign(const char*,unsigned), _M_replace_safe and _M_assign.
 // Actual memory/mutate/copy/move/memset/length-error owners are required.
 public sealed class OriginalConfigurationCStringAssignment
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> CopyBytes,MoveBytes;
   public Action<uint,int,uint> SetBytes;
   public Action<uint,uint,uint,uint> Mutate;
   public Action<uint> ThrowLengthError;
  }
  private readonly Host host;private readonly uint emptySlot,errorCString;
  public OriginalConfigurationCStringAssignment(Host h,uint emptyRepSlot,uint lengthErrorCString)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.ReadByte==null||h.WriteByte==null||h.CopyBytes==null||h.MoveBytes==null||h.SetBytes==null||h.Mutate==null||h.ThrowLengthError==null||emptyRepSlot==0||lengthErrorCString==0)throw new ArgumentException("Actual CString memory/mutation/copy/move/memset/length-error and relocated empty-Rep bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,ReadByte=h.ReadByte,WriteByte=h.WriteByte,CopyBytes=h.CopyBytes,MoveBytes=h.MoveBytes,SetBytes=h.SetBytes,Mutate=h.Mutate,ThrowLengthError=h.ThrowLengthError};emptySlot=emptyRepSlot;errorCString=lengthErrorCString;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Copy(uint destination,uint source,uint size,bool move)
  {
   if(size==1)host.WriteByte(destination,host.ReadByte(source));else if(move)host.MoveBytes(destination,source,size);else host.CopyBytes(destination,source,size);
  }
  public uint Replace(uint owner,uint position,uint removed,uint source,uint inserted)
  {
   host.Mutate(owner,position,removed,inserted);if(inserted!=0)Copy(Add(host.ReadWord(owner),position),source,inserted,false);return owner;
  }
  public uint Assign(uint owner,uint source,uint size)
  {
   uint data=host.ReadWord(owner),length=host.ReadWord(Add(data,0xfffffff4));
   if(size>0x3ffffffc){host.ThrowLengthError(errorCString);throw new InvalidOperationException("Native nonreturning CString length-error binding returned");}
   if(source<data||source>Add(data,length)||unchecked((int)host.ReadWord(Add(data,0xfffffffc)))>0)return Replace(owner,0,length,source,size);
   uint offset=unchecked(source-data);
   if(size<=offset){Copy(data,source,size,false);data=host.ReadWord(owner);}
   else if(offset!=0){Copy(data,source,size,true);data=host.ReadWord(owner);}
   uint rep=Add(data,0xfffffff4);if(rep!=host.ReadWord(emptySlot)){host.WriteWord(Add(data,0xfffffffc),0);host.WriteWord(rep,size);host.WriteByte(Add(data,size),0);}return owner;
  }
  public void AssignBytes(uint destination,uint size,byte value)
  {if(size==1)host.WriteByte(destination,value);else host.SetBytes(destination,unchecked((sbyte)value),size);}
 }
}
