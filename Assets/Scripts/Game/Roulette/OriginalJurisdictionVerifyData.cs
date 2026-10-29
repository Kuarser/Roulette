using System;
namespace RouletteDeluxe
{
 // Full 083637a0 VerifyData; the caller tests AL, while SETNZ retains EAX high bits.
 public sealed class OriginalJurisdictionVerifyData
 {
  public sealed class Host{public Func<uint,uint> ReadWord;public Func<uint,byte> ReadByte;}
  private readonly Host host;
  public OriginalJurisdictionVerifyData(Host h)
  {
   if(h==null||h.ReadWord==null||h.ReadByte==null)throw new ArgumentException("Actual jurisdiction input-pointer and byte memory required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte};
  }
  public uint Verify(uint machine)
  {
   uint input=host.ReadWord(unchecked(machine+0x30));
   for(uint index=0;index<2048;++index)if(host.ReadByte(unchecked(input+index))!=255)return (index&0xffffff00u)|1u;
   return 0x800;
  }
 }
}
