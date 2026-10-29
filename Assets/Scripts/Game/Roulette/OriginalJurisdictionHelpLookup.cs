using System;
namespace RouletteDeluxe
{
 // Both complete GetHelpText bodies share this search/copy algorithm.
 public sealed class OriginalJurisdictionHelpLookup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,uint,uint,int> CompareBytes;
   public Action<uint,uint> CopyString;
   public Action<uint,uint,uint> ConstructLiteral;
  }
  private readonly Host host;private readonly uint emptyCString,allocator;
  public OriginalJurisdictionHelpLookup(Host h,uint emptyCStringAddress,uint allocatorScratch)
  {
   if(h==null||h.ReadWord==null||h.CompareBytes==null||h.CopyString==null||h.ConstructLiteral==null||emptyCStringAddress==0||allocatorScratch==0)throw new ArgumentException("Actual help tree/CString memory, memcmp, string constructors and relocated empty/allocator bindings required");
   host=new Host{ReadWord=h.ReadWord,CompareBytes=h.CompareBytes,CopyString=h.CopyString,ConstructLiteral=h.ConstructLiteral};emptyCString=emptyCStringAddress;allocator=allocatorScratch;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public uint GetHelpText(uint destination,uint jurisdiction,uint name)
  {
   uint node=host.ReadWord(Add(jurisdiction,0x1c));
   if(node!=0)
   {
    uint header=Add(jurisdiction,0x18),candidate=header,query=host.ReadWord(name),queryLength=host.ReadWord(Add(query,0xfffffff4));
    while(node!=0)
    {
     uint key=host.ReadWord(Add(node,0x10)),length=host.ReadWord(Add(key,0xfffffff4));
     int comparison=host.CompareBytes(key,query,Math.Min(length,queryLength));
     if(comparison==0)comparison=unchecked((int)(length-queryLength));
     if(comparison>=0){candidate=node;node=host.ReadWord(Add(node,8));}else node=host.ReadWord(Add(node,12));
    }
    if(candidate!=header)
    {
     uint key=host.ReadWord(Add(candidate,0x10)),length=host.ReadWord(Add(key,0xfffffff4));
     int comparison=host.CompareBytes(query,key,Math.Min(queryLength,length));
     if(comparison==0)comparison=unchecked((int)(queryLength-length));
     if(comparison>=0){host.CopyString(destination,Add(candidate,0x14));return destination;}
    }
   }
   host.ConstructLiteral(destination,emptyCString,allocator);return destination;
  }
 }
}
