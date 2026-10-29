using System;
namespace RouletteDeluxe
{
 // Original IJurisdiction signature member dispatch. Each thunk subtracts four;
 // the primary copies 15 live words and Set then calls NV2::Save(owner +0x3c8).
 public sealed class OriginalMachineDataJurisSignature
 {
  private readonly OriginalNativeMemory memory;private readonly Action<uint> save;
  public OriginalMachineDataJurisSignature(OriginalNativeMemory memory,Action<uint> save)
  {if(memory==null||save==null)throw new ArgumentException("Live MachineData memory and NV Save owner required");this.memory=memory;this.save=save;}
  public uint Get(uint method,uint interfaceOwner,uint destination)
  {
   if(method!=0x838cdd0)throw new ArgumentOutOfRangeException("method");uint source=unchecked(interfaceOwner-4+0x4d9);
   for(uint offset=0;offset<60;offset+=4)memory.WriteWord(unchecked(destination+offset),memory.ReadWord(unchecked(source+offset)));return destination;
  }
  public uint Set(uint method,uint interfaceOwner,uint source)
  {
   if(method!=0x838cd10)throw new ArgumentOutOfRangeException("method");uint owner=unchecked(interfaceOwner-4),destination=unchecked(owner+0x4d9);
   for(uint offset=0;offset<60;offset+=4)memory.WriteWord(unchecked(destination+offset),memory.ReadWord(unchecked(source+offset)));save(unchecked(owner+0x3c8));return source;
  }
 }
}
