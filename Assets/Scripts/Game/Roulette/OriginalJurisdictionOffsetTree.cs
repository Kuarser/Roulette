using System;
namespace RouletteDeluxe
{
 // Complete specialized long-offset set insertion/erase bodies: 08373130/083731e0/08372e10.
 public sealed class OriginalJurisdictionOffsetTree
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New,Decrement;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Action<uint> Delete;
   public Action<byte,uint,uint,uint> Rebalance;
  }
  private readonly Host host;
  public OriginalJurisdictionOffsetTree(Host h)
  {
   if(h==null)throw new ArgumentException("Actual offset-tree host required");host=new Host();foreach(var field in typeof(Host).GetFields()){object v=field.GetValue(h);if(v==null)throw new ArgumentException("Actual offset-tree binding required: "+field.Name);field.SetValue(host,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private int Key(uint pointer){return unchecked((int)host.ReadWord(pointer));}
  private uint Allocate(uint key)
  {uint node=host.New(20);if(node!=0xfffffff0)host.WriteWord(A(node,16),host.ReadWord(key));return node;}
  private void IncreaseCount(uint tree){uint count=host.ReadWord(A(tree,20));host.WriteWord(A(tree,20),A(count,1));}
  public uint Insert(uint result,uint tree,uint x,uint parent,uint key)
  {
   uint header=A(tree,4);byte left=1;
   if(x==0&&parent!=header){int parentKey=Key(A(parent,16));left=(byte)(Key(key)<parentKey?1:0);}
   uint node=Allocate(key);host.Rebalance(left,node,parent,header);IncreaseCount(tree);host.WriteWord(result,node);return result;
  }
  // scratch admits the original hidden-result temporary at the caller's ESP+0x2c.
  public uint InsertUnique(uint result,uint tree,uint key,uint scratch)
  {
   uint node=host.ReadWord(A(tree,8)),header=A(tree,4),parent=header,candidate=header;int value=0,parentKey=0;bool less=true;
   if(node!=0)
   {
    value=Key(key);
    while(true){parent=node;parentKey=Key(A(node,16));less=parentKey>value;node=host.ReadWord(A(node,less?8u:12u));if(node==0)break;}
    candidate=parent;
   }
   if(less)
   {
    if(host.ReadWord(A(tree,12))==parent)
    {Insert(scratch,tree,0,parent,key);node=host.ReadWord(scratch);host.WriteByte(A(result,4),1);host.WriteWord(result,node);return result;}
    candidate=host.Decrement(parent);value=Key(key);parentKey=Key(A(candidate,16));
   }
   if(value<=parentKey){host.WriteWord(result,candidate);host.WriteByte(A(result,4),0);return result;}
   byte left=1;if(parent!=header)left=(byte)(value<Key(A(parent,16))?1:0);
   node=Allocate(key);host.Rebalance(left,node,parent,header);IncreaseCount(tree);host.WriteByte(A(result,4),1);host.WriteWord(result,node);return result;
  }
  public void Erase(uint tree,uint node)
  {
   // The optimized original unrolls this right-recursion/left-iteration traversal.
   // No node/header/count cleanup is added here; original callers reset the header.
   while(node!=0){uint right=host.ReadWord(A(node,12));Erase(tree,right);uint left=host.ReadWord(A(node,8));host.Delete(node);node=left;}
  }
 }
}
