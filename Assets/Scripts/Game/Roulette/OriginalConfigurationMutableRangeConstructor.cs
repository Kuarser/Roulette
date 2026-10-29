using System;
namespace RouletteDeluxe
{
 // Complete aliased string<char*> C1/C2 at library-relative 0x9af70; no local EH.
 public sealed class OriginalConfigurationMutableRangeConstructor
 {
  public sealed class Host
  {
   public Func<uint,uint,uint,uint> ConstructMutableRange;
   public Action<uint,uint> WriteWord;
  }
  private readonly Host host;
  public OriginalConfigurationMutableRangeConstructor(Host h)
  {
   if(h==null||h.ConstructMutableRange==null||h.WriteWord==null)throw new ArgumentException("Actual dynamic mutable forward-range and destination memory bindings required");
   host=new Host{ConstructMutableRange=h.ConstructMutableRange,WriteWord=h.WriteWord};
  }
  public void Construct(uint destination,uint begin,uint end,uint allocator)
  {uint data=host.ConstructMutableRange(begin,end,allocator);host.WriteWord(destination,data);}
 }
}
