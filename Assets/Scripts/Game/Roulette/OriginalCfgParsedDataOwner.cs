using System;
namespace RouletteDeluxe
{
 // Original factory, SetRoot and D1/D0 of the 8-byte parsed-data owner; DOM is separately owned.
 public sealed class OriginalCfgParsedDataOwner
 {
  public sealed class Host{public Func<uint,uint> ReadWord,New;public Action<uint,uint> WriteWord;public Action<uint> Delete;}
  [Serializable] public sealed class Layout{public uint ConcreteVtableSlot,InterfaceVtableSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgParsedDataOwner(Host h,Layout l)
  {
   if(h==null||l==null||l.ConcreteVtableSlot==0||l.InterfaceVtableSlot==0)throw new ArgumentException("Parsed-data owner bindings required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Parsed-data owner binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout{ConcreteVtableSlot=l.ConcreteVtableSlot,InterfaceVtableSlot=l.InterfaceVtableSlot};
  }
  public uint Create(uint factory){uint owner=host.New(8),table=host.ReadWord(layout.ConcreteVtableSlot);host.WriteWord(owner,unchecked(table+8));host.WriteWord(unchecked(owner+4),0);return owner;}
  public uint CreateHybrid(uint factory){return Create(factory);}
  public void SetRoot(uint owner,uint root){host.WriteWord(unchecked(owner+4),root);}
  public void D1(uint owner){uint table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,unchecked(table+8));}
  public void D0(uint owner){D1(owner);host.Delete(owner);}
 }
}
