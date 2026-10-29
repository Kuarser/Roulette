using System;
namespace RouletteDeluxe
{
 // Original CFGValueHolder/CCFGValueHolder/ICFGValueHolder D1/D2 and D0 bodies.
 public sealed class OriginalCfgValueDestruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,IsRGBColor,IsJurisdictionOption,IsJurisdictionOptionStr,IsJurisdictionOptionBit;
   public Action<uint,uint> WriteWord,DestroyRep;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint> Delete,DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ValueVtableSlot,InterfaceVtableSlot,EmptyRepSlot,PthreadCancelSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgValueDestruction(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual value destruction host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("CFG value destruction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("CFG value relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void InterfaceD1(uint owner){uint table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));}
  public void ConcreteD1(uint owner){InterfaceD1(owner);}
  public void InterfaceD0(uint owner){InterfaceD1(owner);host.Delete(owner);}
  public void ConcreteD0(uint owner){ConcreteD1(owner);host.Delete(owner);}
  private void Cleanup(uint owner,Exception error)
  {
   if(!host.IsNativeUnwind(error))throw error;
   host.DestroyString(A(owner,32));host.DestroyString(A(owner,4));InterfaceD1(owner);host.Resume(error);throw new InvalidOperationException("Original nonreturning CFG value Resume returned");
  }
  private bool Predicate(uint owner,Func<uint,uint> call)
  {uint result;try{result=call(owner);}catch(Exception error){Cleanup(owner,error);throw;}return (result&255)!=0;}
  private void DeleteMember(uint owner,uint member)
  {try{host.Delete(member);}catch(Exception error){Cleanup(owner,error);throw;}}
  private void Release(uint data,uint empty,uint allocator)
  {
   uint rep=unchecked(data-12);if(rep==empty)return;int old;
   if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);
   else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}
   if(old<=0)host.DestroyRep(rep,allocator);
  }
  public void ValueD1(uint owner,uint frame)
  {
   uint table=host.ReadWord(layout.ValueVtableSlot);host.WriteWord(owner,A(table,8));
   if(Predicate(owner,host.IsRGBColor)||Predicate(owner,host.IsJurisdictionOption)||Predicate(owner,host.IsJurisdictionOptionStr)||Predicate(owner,host.IsJurisdictionOptionBit)){uint member=host.ReadWord(A(owner,36));DeleteMember(owner,member);}
   uint data=host.ReadWord(A(owner,32)),empty=host.ReadWord(layout.EmptyRepSlot);Release(data,empty,A(frame,30));
   data=host.ReadWord(A(owner,4));Release(data,empty,A(frame,31));InterfaceD1(owner);
  }
  public void ValueD0(uint owner,uint frame){ValueD1(owner,unchecked(frame-64));host.Delete(owner);}
 }
}
