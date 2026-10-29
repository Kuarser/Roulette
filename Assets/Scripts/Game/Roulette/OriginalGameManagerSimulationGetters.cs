using System;
namespace RouletteDeluxe
{
 // The captured GameMgrServer secondary interface adjusts this by 40 bytes.
 public sealed class OriginalGameManagerSimulationGetters
 {
  private readonly OriginalNativeMemory memory;
  public OriginalGameManagerSimulationGetters(OriginalNativeMemory memory)
  {if(memory==null)throw new ArgumentNullException("memory");this.memory=memory;}
  public uint Invoke(uint method,uint interfaceOwner)
  {
   uint offset;
   switch(method){case 0x82c2bd0:offset=0x108;break;case 0x82c2bf0:offset=0x10c;break;default:throw new ArgumentOutOfRangeException("method");}
   return memory.ReadWord(unchecked(interfaceOwner-40+offset));
  }
 }
}
