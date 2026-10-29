using System;
namespace RouletteDeluxe
{
 // Complete original find(char,pos) and aliased substring C1/C2 bodies.
 public sealed class OriginalConfigurationSubstring
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,uint,uint> MemChr;
   public Func<uint,uint,uint,uint> ConstructMutableRange;
   public Action<uint> ThrowOutOfRange;
  }
  private readonly Host host;private readonly uint errorCString,allocator;
  public OriginalConfigurationSubstring(Host h,uint outOfRangeCString,uint allocatorScratch)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.MemChr==null||h.ConstructMutableRange==null||h.ThrowOutOfRange==null||outOfRangeCString==0||allocatorScratch==0)throw new ArgumentException("Actual string memory, memchr, mutable-range constructor, error CString and local allocator owners required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,MemChr=h.MemChr,ConstructMutableRange=h.ConstructMutableRange,ThrowOutOfRange=h.ThrowOutOfRange};errorCString=outOfRangeCString;allocator=allocatorScratch;
  }
  public uint Find(uint owner,byte character,uint position)
  {
   uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12));if(position>=length)return uint.MaxValue;
   uint found=host.MemChr(unchecked(data+position),unchecked((sbyte)character),length-position);return found==0?uint.MaxValue:unchecked(found-data);
  }
  public void Construct(uint destination,uint source,uint position,uint requested)
  {
   uint data=host.ReadWord(source),length=host.ReadWord(unchecked(data-12));if(position>length){host.ThrowOutOfRange(errorCString);throw new InvalidOperationException("Original nonreturning substring out-of-range binding returned");}
   uint count=Math.Min(length-position,requested),begin=unchecked(data+position),end=unchecked(begin+count);uint result=host.ConstructMutableRange(begin,end,allocator);host.WriteWord(destination,result);
  }
 }
}
