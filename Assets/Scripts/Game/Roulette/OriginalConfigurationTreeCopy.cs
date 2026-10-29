using System;
namespace RouletteDeluxe
{
 // Complete _M_copy helpers08596260/08594cb0. Node payload construction
 // and erasure remain mandatory actual owner bindings.
 public sealed class OriginalConfigurationTreeCopy
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,CopyString,CopyRuleVector,EraseOrderTree,EraseAllowedTree;
   public Action<uint> DestroyString,Free;
   public Action<Exception> BeginCatch;
   public Action EndCatch,Rethrow;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationTreeCopy(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.CopyString==null||h.CopyRuleVector==null||h.EraseOrderTree==null||h.EraseAllowedTree==null||h.DestroyString==null||h.Free==null||h.BeginCatch==null||h.EndCatch==null||h.Rethrow==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual tree memory/node heap/string/vector construction/erase/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,CopyString=h.CopyString,CopyRuleVector=h.CopyRuleVector,EraseOrderTree=h.EraseOrderTree,EraseAllowedTree=h.EraseAllowedTree,DestroyString=h.DestroyString,Free=h.Free,BeginCatch=h.BeginCatch,EndCatch=h.EndCatch,Rethrow=h.Rethrow,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private T Guard<T>(Func<T> call,Action<Exception> cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void Guard(Action call,Action<Exception> cleanup){Guard(()=>{call();return 0;},cleanup);}
  private void Rethrow(Action<Exception> cleanup)
  {
   Guard(host.Rethrow,cleanup);throw new InvalidOperationException("Native nonreturning tree copy rethrow binding returned");
  }
  private void EraseCatch(uint tree,uint root,bool allowed,Exception ex)
  {
   host.BeginCatch(ex);if(allowed)host.EraseAllowedTree(tree,root);else host.EraseOrderTree(tree,root);Rethrow(next=>host.EndCatch());
  }
  private void NodeCatch(uint tree,uint root,uint node,bool allowed,bool later,bool keyConstructed,Exception ex)
  {
   if(keyConstructed)host.DestroyString(Add(node,16));host.BeginCatch(ex);host.Free(node);
   Rethrow(next=>{host.EndCatch();if(later)EraseCatch(tree,root,allowed,next);});
  }
  private void Payload(uint tree,uint root,uint node,uint source,bool allowed,bool later)
  {
   if(Add(node,16)==0)return;
   Guard(()=>host.CopyString(Add(node,16),Add(source,16)),ex=>NodeCatch(tree,root,node,allowed,later,false,ex));
   if(allowed)Guard(()=>host.CopyRuleVector(Add(node,20),Add(source,20)),ex=>NodeCatch(tree,root,node,true,later,true,ex));
   else host.WriteWord(Add(node,20),host.ReadWord(Add(source,20)));
  }
  public uint CopyOrderTree(uint tree,uint source,uint parent){return Copy(tree,source,parent,false);}
  public uint CopyAllowedTree(uint tree,uint source,uint parent){return Copy(tree,source,parent,true);}
  private uint Copy(uint tree,uint source,uint parent,bool allowed)
  {
   uint size=allowed?32u:24u,root=host.Allocate(size);
   Payload(tree,root,root,source,allowed,false);
   uint color=host.ReadWord(source);host.WriteWord(Add(root,12),0);host.WriteWord(root,color);host.WriteWord(Add(root,8),0);host.WriteWord(Add(root,4),parent);
   uint right=host.ReadWord(Add(source,12));
   Action<Exception> erase=ex=>EraseCatch(tree,root,allowed,ex);
   if(right!=0)
   {
    uint copied=Guard(()=>Copy(tree,right,root,allowed),erase);Guard(()=>host.WriteWord(Add(root,12),copied),erase);
   }
   uint left=Guard(()=>host.ReadWord(Add(source,8)),erase),previous=root;
   while(left!=0)
   {
    uint node=Guard(()=>host.Allocate(size),erase);Payload(tree,root,node,left,allowed,true);
    color=host.ReadWord(left);host.WriteWord(Add(node,12),0);host.WriteWord(Add(node,8),0);host.WriteWord(Add(node,4),previous);host.WriteWord(node,color);
    right=host.ReadWord(Add(left,12));host.WriteWord(Add(previous,8),node);
    if(right!=0){uint copied=Guard(()=>Copy(tree,right,node,allowed),erase);host.WriteWord(Add(node,12),copied);}
    left=host.ReadWord(Add(left,8));previous=node;
   }
   return root;
  }
 }
}
