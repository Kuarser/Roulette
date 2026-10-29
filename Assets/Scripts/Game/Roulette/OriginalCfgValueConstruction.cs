using System;
namespace RouletteDeluxe
{
 // Original CFGValueHolder C1/C2, BeUnknown and SetValueType bodies.
 public sealed class OriginalCfgValueConstruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString;
   public Action<uint,uint,uint> InvokeSetValueType;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ValueVtableSlot,EmptyRepSlot,InterfaceVtableSlot,UnknownType;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgValueConstruction(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual CFG value construction host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("CFG value construction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("CFG value construction relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void SetValueType(uint owner,uint source){host.AssignString(A(owner,4),source);}
  public void BeUnknown(uint owner){uint table=host.ReadWord(owner),method=host.ReadWord(A(table,92));host.InvokeSetValueType(method,owner,layout.UnknownType);}
  public void Construct(uint owner)
  {
   host.WriteWord(A(owner,36),0);uint table=host.ReadWord(layout.ValueVtableSlot);host.WriteWord(owner,A(table,8));uint empty=host.ReadWord(layout.EmptyRepSlot),data=A(empty,12);host.WriteWord(A(owner,4),data);host.WriteWord(A(owner,32),data);
   try{BeUnknown(owner);}
   catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.DestroyString(A(owner,32));host.DestroyString(A(owner,4));table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));host.Resume(error);throw new InvalidOperationException("Original nonreturning CFG value construction Resume returned");}
  }
 }
}
