using System;
namespace RouletteDeluxe
{
 // Full 0940c330 specialized dictionary node erase, including the optimized inline key releases.
 public sealed class OriginalCfgDictionaryTreeErase
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,DestroyRep;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint> Delete;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryTreeErase(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual dictionary node erase host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Dictionary node erase binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Dictionary key relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Release(uint data,uint allocator,bool fixedMode,bool atomic)
  {
   uint rep=unchecked(data-12);if(rep==host.ReadWord(layout.EmptyRepSlot))return;
   if(!fixedMode)atomic=host.ReadWord(layout.PthreadCancelSlot)!=0;
   int old;
   if(atomic)old=host.AtomicAdd(unchecked(data-4),-1);
   else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}
   if(old<=0)host.DestroyRep(rep,allocator);
  }
  // frame is the original post-prologue ESP; recursive invocation subtracts 0x60.
  public void Erase(uint tree,uint node,uint frame){Level(tree,node,frame,0);}
  private void Level(uint tree,uint node,uint frame,int depth)
  {
   while(node!=0)
   {
    uint right=host.ReadWord(A(node,12));
    if(depth==7){if(right!=0){bool atomic=host.ReadWord(layout.PthreadCancelSlot)!=0;Deep(tree,right,frame,atomic);}}
    else Level(tree,right,frame,depth+1);
    uint left,data;
    if(depth==7){data=host.ReadWord(A(node,16));left=host.ReadWord(A(node,8));}
    else{left=host.ReadWord(A(node,8));data=host.ReadWord(A(node,16));}
    Release(data,A(frame,(uint)(0x3f-depth)),false,false);host.Delete(node);node=left;
   }
  }
  private void Deep(uint tree,uint node,uint frame,bool atomic)
  {
   while(node!=0)
   {
    uint right=host.ReadWord(A(node,12));Erase(tree,right,unchecked(frame-0x60));
    uint data=host.ReadWord(A(node,16)),left=host.ReadWord(A(node,8));
    Release(data,A(frame,0x37),true,atomic);host.Delete(node);node=left;
   }
  }
 }
}
