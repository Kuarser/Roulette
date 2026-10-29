using System;
namespace RouletteDeluxe
{
 // Original order/allowed _M_erase082169d0/08216bf0, string _Destroy0824d920
 // and vector deallocator20508283780. Lower string and heap owners are required.
 public sealed class OriginalConfigurationContainerDestruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint> DestroyString,Free;
  }
  private readonly Host host;
  public OriginalConfigurationContainerDestruction(Host h)
  {
   if(h==null||h.ReadWord==null||h.DestroyString==null||h.Free==null)throw new ArgumentException("Actual container memory/string destructor/heap bindings required");
   host=new Host{ReadWord=h.ReadWord,DestroyString=h.DestroyString,Free=h.Free};
  }
  private static uint Add(uint at,uint offset){return unchecked(at+offset);}
  public void DestroyRange(uint begin,uint end)
  {
   while(begin!=end){uint slot=begin;begin=Add(begin,4);host.DestroyString(slot);}
  }
  public void DeallocateVector(uint storage){if(storage!=0)host.Free(storage);}
  public void EraseOrderTree(uint tree,uint root){Erase(root,false,0);}
  public void EraseAllowedTree(uint tree,uint root){Erase(root,true,0);}
  private void Erase(uint node,bool allowed,int level)
  {
   while(node!=0)
   {
    uint right=host.ReadWord(Add(node,12));
    // Nine inlined node levels precede the original recursive call. A zero
    // right child at the last level still enters the no-op recursive helper.
    Erase(right,allowed,level==8?0:level+1);
    uint left;
    if(allowed)
    {
     uint end=host.ReadWord(Add(node,24)),begin=host.ReadWord(Add(node,20));
     left=host.ReadWord(Add(node,8));
     if(begin!=end){DestroyRange(begin,end);begin=host.ReadWord(Add(node,20));}
     if(begin!=0)host.Free(begin);
    }
    else left=host.ReadWord(Add(node,8));
    host.DestroyString(Add(node,16));host.Free(node);node=left;
   }
  }
 }
}
