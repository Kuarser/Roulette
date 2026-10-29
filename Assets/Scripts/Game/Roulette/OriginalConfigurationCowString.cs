using System;
namespace RouletteDeluxe
{
 // Packaged libstdc++ COW C1/D1/assignment/Rep create/clone/release/delete.
 // Relocated GOT slots and heap/atomic/byte-memory owners are mandatory.
 public sealed class OriginalConfigurationCowString
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> CopyBytes;
   public Action<uint> Free,ThrowLengthError;
  }
  private readonly Host host;
  private readonly uint emptyRepSlot,pthreadCancelSlot,lengthErrorCString;
  public OriginalConfigurationCowString(Host h,uint emptySlot,uint threadSlot,uint errorCString)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.ReadByte==null||h.WriteByte==null||h.AtomicAdd==null||h.CopyBytes==null||h.Free==null||h.ThrowLengthError==null||emptySlot==0||threadSlot==0||errorCString==0)throw new ArgumentException("Actual COW memory/atomic/heap/copy/length-error and relocated GOT/CString bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,ReadByte=h.ReadByte,WriteByte=h.WriteByte,AtomicAdd=h.AtomicAdd,CopyBytes=h.CopyBytes,Free=h.Free,ThrowLengthError=h.ThrowLengthError};emptyRepSlot=emptySlot;pthreadCancelSlot=threadSlot;lengthErrorCString=errorCString;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Rep(uint data){return unchecked(data-12);}
  public void Delete(uint storage){if(storage!=0)host.Free(storage);}
  public void DestroyRep(uint rep){Delete(rep);}
  public void Release(uint rep)
  {
   int old;
   if(host.ReadWord(pthreadCancelSlot)!=0)old=host.AtomicAdd(Add(rep,8),-1);
   else{old=unchecked((int)host.ReadWord(Add(rep,8)));host.WriteWord(Add(rep,8),unchecked((uint)(old-1)));}
   if(old<=0)DestroyRep(rep);
  }
  public void Destroy(uint source)
  {
   uint rep=Rep(host.ReadWord(source));if(rep!=host.ReadWord(emptyRepSlot))Release(rep);
  }
  private bool Increment(uint data)
  {
   bool atomic=host.ReadWord(pthreadCancelSlot)!=0;uint count=unchecked(data-4);
   if(atomic)host.AtomicAdd(count,1);else host.WriteWord(count,Add(host.ReadWord(count),1));return atomic;
  }
  public void Construct(uint destination,uint source)
  {
   uint data=host.ReadWord(source);int refs=unchecked((int)host.ReadWord(unchecked(data-4)));
   if(refs<0)data=Clone(Rep(data),0);
   else if(Rep(data)!=host.ReadWord(emptyRepSlot))Increment(data);
   host.WriteWord(destination,data);
  }
  public uint Assign(uint destination,uint source)
  {
   uint data=host.ReadWord(source),oldRep=Rep(host.ReadWord(destination)),sourceRep=Rep(data);
   if(sourceRep==oldRep)return destination;
   int refs=unchecked((int)host.ReadWord(unchecked(data-4)));uint empty;
   if(refs<0){data=Clone(sourceRep,0);empty=host.ReadWord(emptyRepSlot);oldRep=Rep(host.ReadWord(destination));}
   else{empty=host.ReadWord(emptyRepSlot);if(sourceRep!=empty&&Increment(data))oldRep=Rep(host.ReadWord(destination));}
   if(oldRep!=empty)Release(oldRep);host.WriteWord(destination,data);return destination;
  }
  public uint OperatorAssign(uint destination,uint source){return Assign(destination,source);}
  public uint Create(uint requested,uint previous)
  {
   if(requested>0x3ffffffc){host.ThrowLengthError(lengthErrorCString);throw new InvalidOperationException("Native nonreturning COW length-error binding returned");}
   uint capacity=requested;
   if(capacity>previous)
   {
    uint twice=unchecked(previous*2);if(capacity<twice)capacity=twice;
    uint accounted=Add(capacity,29);
    if(accounted>4096&&previous<capacity){capacity=unchecked(capacity+4096-(accounted&4095));if(capacity>0x3ffffffc)capacity=0x3ffffffc;}
   }
   uint rep=host.Allocate(Add(capacity,13));host.WriteWord(Add(rep,4),capacity);host.WriteWord(Add(rep,8),0);return rep;
  }
  public uint Clone(uint sourceRep,uint extra)
  {
   uint previous=host.ReadWord(Add(sourceRep,4)),requested=Add(extra,host.ReadWord(sourceRep));uint rep=Create(requested,previous),length=host.ReadWord(sourceRep),data=Add(rep,12),retainedLength=0;
   if(length!=0)
   {
    if(length==1)host.WriteByte(data,host.ReadByte(Add(sourceRep,12)));else host.CopyBytes(data,Add(sourceRep,12),length);
    retainedLength=host.ReadWord(sourceRep);
   }
   if(rep!=host.ReadWord(emptyRepSlot)){host.WriteWord(Add(rep,8),0);host.WriteWord(rep,retainedLength);host.WriteByte(Add(data,retainedLength),0);}
   return data;
  }
 }
}
