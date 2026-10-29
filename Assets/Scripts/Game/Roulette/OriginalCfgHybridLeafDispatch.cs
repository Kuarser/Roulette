using System;
namespace RouletteDeluxe
{
 // CFGHybridDictionary::AddSymbolsFrom 0940c810; post-prologue frame.
 public sealed class OriginalCfgHybridLeafDispatch
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep,InvokeDeleteIterator,InvokeDestroyConverter;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint,uint> InvokeCreateConverter,InvokeTrim;
   public Action<uint,uint,uint> InvokeBaseDispatch,InvokeSetConverter,InvokeSetString;
   public Func<uint,uint,uint> InvokeGetChildren,InvokeHasNext,InvokeNext,InvokeHasChildren,InvokeHasAttributes,InvokeCreateValue;
   public Action<uint,uint,uint> InvokeGetName,InvokeGetText;
   public Action<uint,uint,uint,uint> InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgHybridLeafDispatch(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual Hybrid leaf host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid leaf binding required: "+f.Name);f.SetValue(host,v);}if(l.EmptyRepSlot==0||l.PthreadCancelSlot==0)throw new ArgumentException("Original string relocations required");layout=new Layout{EmptyRepSlot=l.EmptyRepSlot,PthreadCancelSlot=l.PthreadCancelSlot};}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(e);throw new InvalidOperationException("Original nonreturning leaf Resume returned");}}
  private void Release(uint owner,uint allocator){uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==host.ReadWord(layout.EmptyRepSlot))return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);}
  public void Dispatch(uint dictionary,uint node,uint frame)
  {
   uint key=A(frame,36),value=A(frame,40),trimmed=A(frame,44),name=A(frame,48),text=A(frame,52),empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(value,A(empty,12));host.WriteWord(trimmed,A(empty,12));uint factory=0,converter=0;
   Protected(()=>factory=host.CreateFactory(),trimmed,value,key);uint table=host.ReadWord(factory),method=host.ReadWord(table);Protected(()=>converter=host.InvokeCreateConverter(method,factory,value),trimmed,value,key);Protected(()=>host.InvokeBaseDispatch(dictionary,node,unchecked(frame-160)),trimmed,value,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,36));uint iterator=0;Protected(()=>iterator=host.InvokeGetChildren(method,node),trimmed,value,key);
   while(true)
   {
    table=host.ReadWord(iterator);method=host.ReadWord(A(table,12));uint has=0;Protected(()=>has=host.InvokeHasNext(method,iterator),trimmed,value,key);table=host.ReadWord(iterator);
    if((has&255)==0){method=host.ReadWord(A(table,4));Protected(()=>host.InvokeDeleteIterator(method,iterator),trimmed,value,key);break;}
    method=host.ReadWord(A(table,8));uint leaf=0;Protected(()=>leaf=host.InvokeNext(method,iterator),trimmed,value,key);table=host.ReadWord(leaf);method=host.ReadWord(A(table,60));uint skip=0;Protected(()=>skip=host.InvokeHasChildren(method,leaf),trimmed,value,key);if((skip&255)!=0)continue;
    table=host.ReadWord(leaf);method=host.ReadWord(A(table,56));Protected(()=>skip=host.InvokeHasAttributes(method,leaf),trimmed,value,key);if((skip&255)!=0)continue;
    table=host.ReadWord(leaf);method=host.ReadWord(A(table,44));Protected(()=>host.InvokeGetName(method,name,leaf),trimmed,value,key);Protected(()=>host.AssignString(key,name),name,trimmed,value,key);Release(name,A(frame,59));
    table=host.ReadWord(leaf);method=host.ReadWord(A(table,8));Protected(()=>host.InvokeGetText(method,text,leaf),trimmed,value,key);Protected(()=>host.AssignString(value,text),text,trimmed,value,key);Release(text,A(frame,60));
    table=host.ReadWord(factory);method=host.ReadWord(A(table,24));uint holder=0;Protected(()=>holder=host.InvokeCreateValue(method,factory),trimmed,value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,20));Protected(()=>host.InvokeSetConverter(method,converter,value),trimmed,value,key);
    table=host.ReadWord(converter);method=host.ReadWord(A(table,28));uint result=0;Protected(()=>result=host.InvokeTrim(method,converter,trimmed),trimmed,value,key);Protected(()=>host.AssignString(trimmed,result),trimmed,value,key);
    table=host.ReadWord(holder);method=host.ReadWord(A(table,28));Protected(()=>host.InvokeSetString(method,holder,trimmed),trimmed,value,key);table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),trimmed,value,key);
   }
   if(converter!=0){uint vt=host.ReadWord(converter),d0=host.ReadWord(A(vt,36));Protected(()=>host.InvokeDestroyConverter(d0,converter),trimmed,value,key);}Release(trimmed,A(frame,61));Release(value,A(frame,62));Release(key,A(frame,63));
  }
 }
}
