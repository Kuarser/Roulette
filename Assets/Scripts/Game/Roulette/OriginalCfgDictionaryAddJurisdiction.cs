using System;
namespace RouletteDeluxe
{
 // AddJurisdictionOption 0940a170; frame is the post-prologue ESP.
 public sealed class OriginalCfgDictionaryAddJurisdiction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep,CopyString,Reserve,AppendString,InvokeDestroyConverter;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,AppendCounted,InvokeSetJurisdiction;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> InvokeCreateValue;
   public Func<uint,uint,uint,uint> InvokeCreateConverter,InvokeToLong;
   public Action<uint,uint,uint,uint> InvokeGetAttribute,InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,NameLiteral,OffsetLiteral,PrefixLiteral,MiddleLiteral,SuffixLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAddJurisdiction(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual AddJurisdiction host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("AddJurisdiction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("AddJurisdiction relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(e);throw new InvalidOperationException("Original nonreturning AddJurisdiction Resume returned");}}
  private void Release(uint owner,uint empty,uint allocator)
  {
   uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==empty)return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);
  }
  public void AddJurisdiction(uint dictionary,uint node,uint frame)
  {
   uint key=A(frame,44),offset=A(frame,48),name=A(frame,52),nameResult=A(frame,56),argument=A(frame,60),offsetResult=A(frame,64);
   uint p=A(frame,68),m=A(frame,72),k=A(frame,76),message=A(frame,80),empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(offset,A(empty,12));
   uint factory=0,holder=0;Protected(()=>factory=host.CreateFactory(),offset,key);uint table=host.ReadWord(factory),method=host.ReadWord(A(table,24));Protected(()=>holder=host.InvokeCreateValue(method,factory),offset,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(name,layout.NameLiteral,A(frame,84)),offset,key);Protected(()=>host.InvokeGetAttribute(method,nameResult,node,name),name,offset,key);Protected(()=>host.AssignString(key,nameResult),nameResult,name,offset,key);Release(nameResult,empty,A(frame,86));Release(name,empty,A(frame,87));
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(argument,layout.OffsetLiteral,A(frame,85)),offset,key);Protected(()=>host.InvokeGetAttribute(method,offsetResult,node,argument),argument,offset,key);Protected(()=>host.AssignString(offset,offsetResult),offsetResult,argument,offset,key);Release(offsetResult,empty,A(frame,88));Release(argument,empty,A(frame,89));
   table=host.ReadWord(factory);method=host.ReadWord(table);uint converter=0;Protected(()=>converter=host.InvokeCreateConverter(method,factory,offset),offset,key);
   table=host.ReadWord(converter);uint convert=host.ReadWord(table);host.WriteWord(p,A(empty,12));uint length=host.ReadWord(unchecked(host.ReadWord(offset)-12));
   Protected(()=>host.Reserve(p,A(length,25)),p,offset,key);Protected(()=>host.AppendCounted(p,layout.PrefixLiteral,25),p,offset,key);Protected(()=>host.AppendString(p,offset),p,offset,key);
   Protected(()=>host.CopyString(m,p),p,offset,key);Protected(()=>host.AppendCounted(m,layout.MiddleLiteral,9),m,p,offset,key);Protected(()=>host.CopyString(k,m),m,p,offset,key);Protected(()=>host.AppendString(k,key),k,m,p,offset,key);
   Protected(()=>host.CopyString(message,k),k,m,p,offset,key);Protected(()=>host.AppendCounted(message,layout.SuffixLiteral,8),message,k,m,p,offset,key);uint parsed=0;Protected(()=>parsed=host.InvokeToLong(convert,converter,message),message,k,m,p,offset,key);
   Release(message,empty,A(frame,90));Release(k,empty,A(frame,91));Release(m,empty,A(frame,92));Release(p,empty,A(frame,93));
   uint member=0;Protected(()=>member=host.New(4),offset,key);host.WriteWord(member,parsed);table=host.ReadWord(holder);method=host.ReadWord(A(table,36));Protected(()=>host.InvokeSetJurisdiction(method,holder,member),offset,key);
   table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),offset,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,36));Protected(()=>host.InvokeDestroyConverter(method,converter),offset,key);Release(offset,empty,A(frame,94));Release(key,empty,A(frame,95));
  }
 }
}
