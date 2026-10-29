using System;
namespace RouletteDeluxe
{
 // Original CFG dictionary D1/D2 and D0 bodies; lower value/node destruction is a required binding.
 public sealed class OriginalCfgDictionaryDestruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Increment;
   public Action<uint,uint> WriteWord,InvokeValueDelete,EraseValueTree;
   public Action<uint> Delete;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint BaseVtableSlot,HybridVtableSlot,InterfaceVtableSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryDestruction(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual dictionary destruction host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Dictionary destruction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Dictionary vtable relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void InterfaceD1(uint owner){uint table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));}
  public void ConcreteInterfaceD1(uint owner){InterfaceD1(owner);}
  private void TreeAndInterface(uint owner){uint root=host.ReadWord(A(owner,12));host.EraseValueTree(A(owner,4),root);InterfaceD1(owner);}
  public void BaseD1(uint owner)
  {
   uint iterator=host.ReadWord(A(owner,16)),header=A(owner,8);uint table=host.ReadWord(layout.BaseVtableSlot);host.WriteWord(owner,A(table,8));
   while(iterator!=header)
   {
    uint value=host.ReadWord(A(iterator,20));
    if(value!=0)
    {
     uint valueTable=host.ReadWord(value),method=host.ReadWord(A(valueTable,4));
     try{host.InvokeValueDelete(method,value);}
     catch(Exception error){if(!host.IsNativeUnwind(error))throw;TreeAndInterface(owner);host.Resume(error);throw new InvalidOperationException("Original nonreturning dictionary Resume returned");}
    }
    iterator=host.Increment(iterator);
   }
   TreeAndInterface(owner);
  }
  public void BaseD0(uint owner){BaseD1(owner);host.Delete(owner);}
  public void HybridD1(uint owner){uint table=host.ReadWord(layout.HybridVtableSlot);host.WriteWord(owner,A(table,8));BaseD1(owner);}
  public void HybridD0(uint owner){HybridD1(owner);host.Delete(owner);}
 }
}
