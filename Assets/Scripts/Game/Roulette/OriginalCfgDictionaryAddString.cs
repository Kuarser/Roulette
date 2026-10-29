using System;
namespace RouletteDeluxe
{
 // CFGConstantsDictionary::AddString 09406610; frame is its post-prologue ESP.
 public sealed class OriginalCfgDictionaryAddString
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,InvokeSetString;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> InvokeCreateValue;
   public Action<uint,uint,uint,uint> InvokeGetAttribute,InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,NameLiteral,ValueLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAddString(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual AddString host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("AddString binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("AddString relocation required: "+f.Name);f.SetValue(layout,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(error);throw new InvalidOperationException("Original nonreturning AddString Resume returned");}}
  private void Release(uint owner,uint empty,uint allocator){uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==empty)return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);}
  public void AddString(uint dictionary,uint node,uint frame)
  {
   uint key=A(frame,32),value=A(frame,36),name=A(frame,40),nameResult=A(frame,44),argument=A(frame,48),valueResult=A(frame,52);uint empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(value,A(empty,12));
   uint factory=0,holder=0;Protected(()=>factory=host.CreateFactory(),value,key);uint table=host.ReadWord(factory),method=host.ReadWord(A(table,24));Protected(()=>holder=host.InvokeCreateValue(method,factory),value,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(name,layout.NameLiteral,A(frame,56)),value,key);Protected(()=>host.InvokeGetAttribute(method,nameResult,node,name),name,value,key);Protected(()=>host.AssignString(key,nameResult),nameResult,name,value,key);Release(nameResult,empty,A(frame,58));Release(name,empty,A(frame,59));
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(argument,layout.ValueLiteral,A(frame,57)),value,key);Protected(()=>host.InvokeGetAttribute(method,valueResult,node,argument),argument,value,key);Protected(()=>host.AssignString(value,valueResult),valueResult,argument,value,key);Release(valueResult,empty,A(frame,60));Release(argument,empty,A(frame,61));
   table=host.ReadWord(holder);method=host.ReadWord(A(table,28));Protected(()=>host.InvokeSetString(method,holder,value),value,key);table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),value,key);Release(value,empty,A(frame,62));Release(key,empty,A(frame,63));
  }
 }
}
