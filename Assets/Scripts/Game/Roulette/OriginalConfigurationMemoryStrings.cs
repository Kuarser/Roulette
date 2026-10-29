using System;
namespace RouletteDeluxe
{
 // Persistent byte-memory composition of already recovered packaged COW string bodies.
 public sealed class OriginalConfigurationMemoryStrings
 {
  private readonly OriginalNativeMemory memory;private readonly OriginalConfigurationCowString cow;
  private readonly OriginalConfigurationCStringAssignment counted;private readonly OriginalConfigurationLiteralCString literal;
  public OriginalConfigurationMemoryStrings(OriginalNativeMemory m,uint emptyRepSlot,uint threadSlot,uint lengthErrorCString,uint logicErrorCString,Action<uint> lengthError,Action<uint> logicError)
  {
   if(m==null||lengthError==null||logicError==null)throw new ArgumentException("Persistent memory and original error bindings required");memory=m;
   cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,Allocate=m.Allocate,Free=m.Free,AtomicAdd=m.AtomicAdd,CopyBytes=m.CopyBytes,ThrowLengthError=lengthError},emptyRepSlot,threadSlot,lengthErrorCString);
   var mutation=new OriginalConfigurationCowMutation(new OriginalConfigurationCowMutation.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,CopyBytes=m.CopyBytes,MoveBytes=m.MoveBytes,CreateRep=cow.Create,ReleaseRep=cow.Release},emptyRepSlot);
   counted=new OriginalConfigurationCStringAssignment(new OriginalConfigurationCStringAssignment.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=m.ReadByte,WriteByte=m.WriteByte,CopyBytes=m.CopyBytes,MoveBytes=m.MoveBytes,SetBytes=m.SetBytes,Mutate=mutation.Mutate,ThrowLengthError=lengthError},emptyRepSlot,lengthErrorCString);
   literal=new OriginalConfigurationLiteralCString(new OriginalConfigurationLiteralCString.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,ReadByte=a=>m.ReadByte(a),WriteByte=(a,v)=>m.WriteByte(a,(byte)v),StrLen=m.StrLen,CreateRep=(n,p,a)=>cow.Create(n,p),CopyBytes=m.CopyBytes,ThrowLogicError=logicError,EmptyRepSlot=emptyRepSlot,ErrorCString=logicErrorCString});
  }
  public void Construct(uint owner,uint source,uint allocator){literal.Construct(owner,source,allocator);}
  public void ConstructRange(uint owner,uint begin,uint end,uint allocator){memory.WriteWord(owner,literal.ConstructRange(begin,end,allocator));}
  public uint ConstructRange(uint begin,uint end,uint allocator){return literal.ConstructRange(begin,end,allocator);}
  public void Copy(uint destination,uint source){cow.Construct(destination,source);}
  public uint Assign(uint destination,uint source){return cow.Assign(destination,source);}
  public uint AssignBytes(uint owner,uint source,uint size){return counted.Assign(owner,source,size);}
  public void Destroy(uint owner){cow.Destroy(owner);}
  public byte[] Read(uint owner){uint data=memory.ReadWord(owner);return memory.ReadBytes(data,memory.ReadWord(unchecked(data-12)));}
 }
}
