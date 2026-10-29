using System;
namespace RouletteDeluxe
{
 // Packaged byte find_first_not_of 00098890 / find_last_not_of 00098a00.
 public sealed class OriginalConfigurationNotOf
 {
  public sealed class Host{public Func<uint,uint> ReadWord;public Func<uint,byte> ReadByte;}
  private readonly Host host;
  public OriginalConfigurationNotOf(Host h){if(h==null||h.ReadWord==null||h.ReadByte==null)throw new ArgumentException("Actual string word and byte bindings required");host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte};}
  public uint FindFirst(uint owner,byte character,uint position){uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12));if(position>=length)return uint.MaxValue;byte first=host.ReadByte(unchecked(data+position));if(first!=character)return position;while(++position<length)if(host.ReadByte(unchecked(data+position))!=first)return position;return uint.MaxValue;}
  public uint FindLast(uint owner,byte character,uint position){uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12));if(length==0)return uint.MaxValue;uint index=Math.Min(position,length-1);while(true){if(host.ReadByte(unchecked(data+index))!=character)return index;if(index==0)return uint.MaxValue;--index;}}
 }
}
