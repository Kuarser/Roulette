using System;
namespace RouletteDeluxe
{
 // Complete native IsRGBColor/IsJurisdictionOption/Str/Bit and GetValueType bodies.
 public sealed class OriginalCfgValuePredicates
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString,DestroyRep;
   public Action<uint,uint,uint> InvokeGetValueType;
   public Func<uint,uint,uint,int> CompareMemory;
   public Func<uint,int,int> AtomicAdd;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,RGBType,JurisdictionType,StrType,BitType;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgValuePredicates(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual type-predicate host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Type-predicate binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Type-predicate relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint GetValueType(uint result,uint owner){host.CopyString(result,A(owner,4));return result;}
  private uint Test(uint owner,uint frame,uint constant)
  {
   uint table=host.ReadWord(owner),method=host.ReadWord(A(table,88));host.InvokeGetValueType(method,A(frame,40),owner);
   uint expected=host.ReadWord(constant),data=host.ReadWord(A(frame,40)),length=host.ReadWord(unchecked(data-12));uint result=0;
   if(length==host.ReadWord(unchecked(expected-12))){int compared=host.CompareMemory(data,expected,length);result=(unchecked((uint)compared)&0xffffff00u)|(compared==0?1u:0);}
   uint rep=unchecked(data-12);if(rep!=host.ReadWord(layout.EmptyRepSlot))
   {
    int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}
    if(old<=0){host.DestroyRep(rep,A(frame,47));result&=255;}
   }
   return result;
  }
  public uint IsRGBColor(uint owner,uint frame){return Test(owner,frame,layout.RGBType);}
  public uint IsJurisdictionOption(uint owner,uint frame){return Test(owner,frame,layout.JurisdictionType);}
  public uint IsJurisdictionOptionStr(uint owner,uint frame){return Test(owner,frame,layout.StrType);}
  public uint IsJurisdictionOptionBit(uint owner,uint frame){return Test(owner,frame,layout.BitType);}
 }
}
