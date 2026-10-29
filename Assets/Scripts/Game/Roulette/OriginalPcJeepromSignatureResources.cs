using System;
using System.Collections.Generic;
namespace RouletteDeluxe
{
 // PC transport for the original sdg.juriseeprom.1 read contract. Its file image
 // supplies the device's 2048 bytes; the recovered signature algorithm owns parsing.
 // IPC/device construction and std::vector byte assignment are admitted ports.
 public sealed class OriginalPcJeepromSignatureResources:IDisposable
 {
  private readonly OriginalNativeMemory memory;private readonly OriginalJurisdictionFileResources files;
  private readonly uint path,table,frame;private readonly OriginalStorageMediaJeepromSignature control;
  private readonly HashSet<uint> arrays=new HashSet<uint>(),devices=new HashSet<uint>(),vectors=new HashSet<uint>();
  private bool disposed;
  public int LiveArrays{get{return arrays.Count;}}public int LiveDevices{get{return devices.Count;}}public int LiveVectors{get{return vectors.Count;}}
  public OriginalPcJeepromSignatureResources(OriginalNativeMemory memory,Func<string,string> resolveImage,uint imagePathCString,uint deviceInterfaceTable,uint markerCString,uint readModeCString,uint frame)
  {
   if(memory==null||resolveImage==null||imagePathCString==0||deviceInterfaceTable==0||markerCString==0||readModeCString==0||frame==0)throw new ArgumentException("Physical PC signature memory, device image and original bindings required");
   if(memory.ReadWord(deviceInterfaceTable+16)!=0x93f0cc0||memory.ReadWord(deviceInterfaceTable+4)!=0x93f0e80)throw new ArgumentException("Original JEep interface table required");
   this.memory=memory;path=imagePathCString;table=deviceInterfaceTable;this.frame=frame;files=new OriginalJurisdictionFileResources(memory,resolveImage);
   control=new OriginalStorageMediaJeepromSignature(new OriginalStorageMediaJeepromSignature.Host{
    ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,WriteByte=memory.WriteByte,
    NewArray=n=>{uint owner=memory.Allocate(n);arrays.Add(owner);return owner;},
    CreateEeprom=callback=>{if(callback!=0)throw new ArgumentException("Original null JEep allocator callback required");uint owner=memory.Allocate(48),primaryTable=table-52;memory.WriteWord(owner,primaryTable);memory.WriteWord(owner+40,table);devices.Add(owner+40);return owner+40;},
    InvokeRead=(method,owner,destination,offset,count,kind)=>{
     if(!devices.Contains(owner)||method!=0x93f0cc0||offset!=0||count!=2048||kind!=0)throw new InvalidOperationException("Captured original JEep read dispatch required");
     uint file=files.Open(path,readModeCString);if(file==0)throw new OriginalNvFault("PC jurisdiction device image could not be opened");
     try{if(files.Read(destination,1,count,file)!=count)throw new OriginalNvFault("PC jurisdiction device image is shorter than 2048 bytes");}finally{files.Close(file);}
    },
    Compare=(a,b,n)=>{for(uint i=0;i<n;++i){int difference=memory.ReadByte(a+i)-memory.ReadByte(b+i);if(difference!=0)return difference;}return 0;},CopyBytes=memory.CopyBytes,AssignVector=Assign,
    InvokeDelete=(method,owner)=>{if(method!=0x93f0e80||!devices.Remove(owner))throw new InvalidOperationException("Captured original JEep deletion required");memory.Free(owner-40);},
    DeleteArray=owner=>{if(!arrays.Remove(owner))throw new InvalidOperationException("Owned JEep signature array required");memory.Free(owner);}
   },markerCString);
  }
  private void Alive(){if(disposed)throw new ObjectDisposedException("OriginalPcJeepromSignatureResources");}
  private void Assign(uint vector,uint begin,uint end,byte iteratorTag)
  {
   if(iteratorTag!=0)throw new ArgumentException("Original forward iterator tag required");uint length=unchecked(end-begin),destination=memory.ReadWord(vector),capacity=memory.ReadWord(vector+8);
   if(unchecked(capacity-destination)<length){uint replacement=memory.Allocate(length);vectors.Add(replacement);if(destination!=0)DeleteVectorBuffer(destination);destination=replacement;memory.WriteWord(vector,destination);memory.WriteWord(vector+8,destination+length);}
   memory.CopyBytes(destination,begin,length);memory.WriteWord(vector+4,destination+length);
  }
  public uint Get(uint vector){Alive();return control.Get(vector,frame);}
  public void DeleteVectorBuffer(uint owner){Alive();if(!vectors.Remove(owner))throw new InvalidOperationException("Owned PC signature vector buffer required");memory.Free(owner);}
  public void Dispose()
  {
   if(disposed)return;files.Dispose();foreach(uint owner in vectors)memory.Free(owner);vectors.Clear();foreach(uint owner in devices)memory.Free(owner-40);devices.Clear();foreach(uint owner in arrays)memory.Free(owner);arrays.Clear();disposed=true;
  }
 }
}
