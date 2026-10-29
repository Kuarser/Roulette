using System;
namespace RouletteDeluxe
{
 // Complete specialized dictionary node _M_insert_ at 094058e0.
 public sealed class OriginalCfgDictionaryNodeInsert
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Action<uint,uint> WriteWord,CopyString;
   public Func<uint,uint,uint,int> CompareMemory;
   public Action<byte,uint,uint,uint> Rebalance;
   public Action<uint> Delete;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> BeginCatch,Rethrow,Resume;
   public Action EndCatch;
  }
  private readonly Host host;
  public OriginalCfgDictionaryNodeInsert(Host h)
  {
   if(h==null)throw new ArgumentException("Actual dictionary node insertion host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Dictionary node insertion binding required: "+f.Name);f.SetValue(host,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint Insert(uint result,uint tree,uint x,uint parent,uint pair)
  {
   uint header=A(tree,4);byte left=1;
   if(x==0&&parent!=header){uint data=host.ReadWord(pair),length=host.ReadWord(unchecked(data-12)),parentData=host.ReadWord(A(parent,16)),parentLength=host.ReadWord(unchecked(parentData-12));int cmp=host.CompareMemory(data,parentData,Math.Min(length,parentLength));if(cmp==0)cmp=unchecked((int)(length-parentLength));left=(byte)(cmp<0?1:0);}
   uint node=host.New(24);
   if(node!=0xfffffff0)
   {
    try{host.CopyString(A(node,16),pair);}
    catch(Exception error)
    {
     if(!host.IsNativeUnwind(error))throw;host.BeginCatch(error);host.Delete(node);
     try{host.Rethrow(error);}
     catch(Exception rethrow){if(!host.IsNativeUnwind(rethrow))throw;host.EndCatch();host.Resume(rethrow);throw new InvalidOperationException("Original nonreturning dictionary node Resume returned");}
     throw new InvalidOperationException("Original nonreturning dictionary node Rethrow returned");
    }
    uint value=host.ReadWord(A(pair,4));host.WriteWord(A(node,20),value);
   }
   host.Rebalance(left,node,parent,header);uint count=host.ReadWord(A(tree,20));host.WriteWord(A(tree,20),A(count,1));host.WriteWord(result,node);return result;
  }
 }
}
