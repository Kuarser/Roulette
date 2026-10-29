using System;
namespace RouletteDeluxe
{
 // Captured IJurisdictionMgr member dispatch used by MachineData::InitConfiguration.
 public sealed class OriginalJurisdictionMachineInitializationGetters
 {
  private readonly OriginalNativeMemory memory;
  public OriginalJurisdictionMachineInitializationGetters(OriginalNativeMemory memory)
  {if(memory==null)throw new ArgumentNullException("memory");this.memory=memory;}
  public uint Invoke(uint method,uint interfaceOwner)
  {
   uint owner=unchecked(interfaceOwner-4),offset;bool boolean=false;
   switch(method)
   {
    case 0x8364e30:offset=0x1dd;boolean=true;break;
    case 0x8364f10:offset=0x1e0;break;
    case 0x83644a0:offset=0xdf;break;
    case 0x8365860:offset=0x25b;break;
    default:throw new ArgumentOutOfRangeException("method");
   }
   byte value=memory.ReadByte(unchecked(owner+offset));
   // SETNE AL retains EAX's upper owner bits. The recovered caller consumes AL.
   return boolean?(owner&0xffffff00u)|(value!=0?1u:0u):value;
  }
 }
}
