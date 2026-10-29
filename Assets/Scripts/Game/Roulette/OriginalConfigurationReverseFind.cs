using System;
namespace RouletteDeluxe
{
 // Complete packaged rfind(const char*,pos,count), 00098370 / 140 bytes.
 public sealed class OriginalConfigurationReverseFind
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,uint,uint,int> CompareBytes;
  }
  private readonly Host host;
  public OriginalConfigurationReverseFind(Host h)
  {
   if(h==null||h.ReadWord==null||h.CompareBytes==null)throw new ArgumentException("Actual string memory and counted memcmp bindings required");
   host=new Host{ReadWord=h.ReadWord,CompareBytes=h.CompareBytes};
  }
  public uint Find(uint owner,uint needle,uint position,uint count)
  {
   uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12));
   if(count>length)return uint.MaxValue;
   uint index=Math.Min(position,length-count);
   while(true)
   {
    if(host.CompareBytes(unchecked(data+index),needle,count)==0)return index;
    if(index==0)return uint.MaxValue;
    --index;
   }
  }
 }
}
