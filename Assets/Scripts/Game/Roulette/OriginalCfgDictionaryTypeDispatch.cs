using System;
namespace RouletteDeluxe
{
 // Complete CFGConstantsDictionary::AddSymbolsFrom 09405e00 and Hybrid AddString 0940d010.
 public sealed class OriginalCfgDictionaryTypeDispatch
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,DestroyRep;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,InvokeBaseAddString;
   public Action<uint,uint,uint,uint,uint,uint> InvokeTraversal;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot;public uint[] SelectorLiterals;}
  private readonly Host host;private readonly Layout layout;
  private static readonly uint[] Members={0x4d,0x51,0x55,0x61,0x5d,0x59,0x65,0x69,0x69,0x6d,0x71};
  public OriginalCfgDictionaryTypeDispatch(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual type dispatch host and relocated selectors required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Type dispatch binding required: "+f.Name);f.SetValue(host,v);}if(l.EmptyRepSlot==0||l.PthreadCancelSlot==0||l.SelectorLiterals==null||l.SelectorLiterals.Length!=11)throw new ArgumentException("Eleven original selectors and string relocations required");foreach(uint v in l.SelectorLiterals)if(v==0)throw new ArgumentException("Original selector relocation required");layout=new Layout{EmptyRepSlot=l.EmptyRepSlot,PthreadCancelSlot=l.PthreadCancelSlot,SelectorLiterals=(uint[])l.SelectorLiterals.Clone()};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Dispatch(uint dictionary,uint node,uint frame)
  {
   uint empty=0;
   for(int index=0;index<11;++index)
   {
    uint table=host.ReadWord(dictionary),method=host.ReadWord(A(table,72)),selector=A(frame,60+(uint)index*4);host.ConstructCString(selector,layout.SelectorLiterals[index],A(frame,106+(uint)index));
    try{host.InvokeTraversal(method,dictionary,node,selector,Members[index],0);}
    catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.DestroyString(selector);host.Resume(error);throw new InvalidOperationException("Original nonreturning type dispatch Resume returned");}
    uint data=host.ReadWord(selector),rep=unchecked(data-12);if(index==0)empty=host.ReadWord(layout.EmptyRepSlot);if(rep==empty)continue;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,A(frame,117+(uint)index));
   }
  }
  public void HybridAddString(uint dictionary,uint node,uint frame){host.InvokeBaseAddString(dictionary,node,unchecked(frame-96));}
 }
}
