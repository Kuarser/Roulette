using System;
namespace RouletteDeluxe
{
 // CFGStringConverter::Trim 094003c0; only byte 0x20 is searched.
 public sealed class OriginalCfgStringConverterTrim
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep;
   public Action<uint,uint,uint> AssignCounted;
   public Func<uint,uint,uint,uint> FindFirstNotOf,FindLastNotOf;
   public Action<uint,uint,uint,uint> ConstructSubstring;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint> ThrowOutOfRange,DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,ZeroLiteral,OutOfRangeLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgStringConverterTrim(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual Trim host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Trim binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Trim relocation required: "+f.Name);f.SetValue(layout,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint Trim(uint converter,uint output,uint frame)
  {
   host.AssignCounted(output,layout.ZeroLiteral,0);uint input=A(converter,4),first=host.FindFirstNotOf(input,32,0);if(first==0xffffffff)return output;uint last=host.FindLastNotOf(input,32,0xffffffff),data=host.ReadWord(A(converter,4)),length=host.ReadWord(unchecked(data-12));if(first>length){host.ThrowOutOfRange(layout.OutOfRangeLiteral);throw new InvalidOperationException("Original nonreturning out_of_range returned");}
   uint temporary=A(frame,24);host.ConstructSubstring(temporary,input,first,unchecked(last-first+1));try{host.AssignString(output,temporary);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.DestroyString(temporary);host.Resume(e);throw new InvalidOperationException("Original nonreturning Trim Resume returned");}
   data=host.ReadWord(temporary);uint rep=unchecked(data-12);if(rep!=host.ReadWord(layout.EmptyRepSlot)){int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,A(frame,31));}return output;
  }
 }
}
