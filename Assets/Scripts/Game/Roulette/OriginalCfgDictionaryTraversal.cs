using System;
namespace RouletteDeluxe
{
 // Complete member-function traversal 09405540 and Hybrid forwarding body 0940d130.
 public sealed class OriginalCfgDictionaryTraversal
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString,DestroyRep;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint,uint,uint> InvokeSelect;
   public Func<uint,uint,uint> InvokeNext,InvokeHasNext;
   public Action<uint,uint> InvokeDelete;
   public Action<uint,uint,uint> InvokeMember;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryTraversal(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual traversal host and relocated string layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Traversal binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Traversal relocation required: "+f.Name);f.SetValue(layout,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Traverse(uint dictionary,uint node,uint selector,uint member,uint adjustment)
  {
   uint table=host.ReadWord(node),method=host.ReadWord(A(table,16)),iterator=host.InvokeSelect(method,node,selector),owner=A(dictionary,adjustment);bool virtualMember=(member&1)!=0;
   while(true){table=host.ReadWord(iterator);method=host.ReadWord(A(table,12));if((host.InvokeHasNext(method,iterator)&255)==0)break;table=host.ReadWord(iterator);method=host.ReadWord(A(table,8));uint child=host.InvokeNext(method,iterator);if(virtualMember){table=host.ReadWord(owner);method=host.ReadWord(unchecked(table+member-1));}else method=member;host.InvokeMember(method,owner,child);}
   table=host.ReadWord(iterator);method=host.ReadWord(A(table,4));host.InvokeDelete(method,iterator);
  }
  public void TraverseHybrid(uint dictionary,uint node,uint selector,uint member,uint adjustment,uint frame)
  {
   uint copy=A(frame,56);host.CopyString(copy,selector);
   try{Traverse(dictionary,node,copy,member,adjustment);}
   catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.DestroyString(copy);host.Resume(error);throw new InvalidOperationException("Original nonreturning Hybrid traversal Resume returned");}
   uint data=host.ReadWord(copy),rep=unchecked(data-12);if(rep==host.ReadWord(layout.EmptyRepSlot))return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,A(frame,63));
  }
 }
}
