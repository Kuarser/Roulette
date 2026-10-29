using System;
namespace RouletteDeluxe
{
 // AddFloat 0940a840 / AddDouble 0940aee0; frame is the post-prologue ESP.
 public sealed class OriginalCfgDictionaryAddFloating
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action LoadZero;
   public Action<uint,bool> StoreFloating;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep,CopyString,Reserve,AppendString,InvokeDestroyConverter;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,AppendCounted,InvokeSetScalar;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> InvokeCreateValue;
   public Func<uint,uint,uint,uint> InvokeCreateConverter;
   public Action<uint,uint,uint> InvokeConvert;
   public Action<uint,uint,uint,uint> InvokeGetAttribute,InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,NameLiteral,ValueLiteral,PrefixLiteral,MiddleLiteral,FloatSuffixLiteral,DoubleSuffixLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAddFloating(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual AddJurisdiction host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("AddJurisdiction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("AddJurisdiction relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(e);throw new InvalidOperationException("Original nonreturning AddJurisdiction Resume returned");}}
  private void Release(uint owner,uint empty,uint allocator){ReleaseData(host.ReadWord(owner),empty,allocator);}
  private void ReleaseData(uint data,uint empty,uint allocator)
  {
   uint rep=unchecked(data-12);if(rep==empty)return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);
  }
  public void AddFloat(uint dictionary,uint node,uint frame){Add(dictionary,node,frame,false);}
  public void AddDouble(uint dictionary,uint node,uint frame){Add(dictionary,node,frame,true);}
  private void Add(uint dictionary,uint node,uint frame,bool wide)
  {
   uint result=unchecked(frame+(wide?32u:48u));if(wide){host.LoadZero();host.StoreFloating(result,true);}else host.WriteWord(result,0);
   uint key=unchecked(frame+(wide?44u:40u)),value=unchecked(frame+(wide?48u:44u)),name=A(frame,52),nameResult=A(frame,56),argument=A(frame,60),valueResult=A(frame,64),p=A(frame,68),m=A(frame,72),k=A(frame,76),message=A(frame,80),empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(value,A(empty,12));
   uint factory=0,holder=0;Protected(()=>factory=host.CreateFactory(),value,key);uint table=host.ReadWord(factory),method=host.ReadWord(A(table,24));Protected(()=>holder=host.InvokeCreateValue(method,factory),value,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(name,layout.NameLiteral,A(frame,84)),value,key);Protected(()=>host.InvokeGetAttribute(method,nameResult,node,name),name,value,key);Protected(()=>host.AssignString(key,nameResult),nameResult,name,value,key);Release(nameResult,empty,A(frame,86));Release(name,empty,A(frame,87));
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(argument,layout.ValueLiteral,A(frame,85)),value,key);Protected(()=>host.InvokeGetAttribute(method,valueResult,node,argument),argument,value,key);Protected(()=>host.AssignString(value,valueResult),valueResult,argument,value,key);Release(valueResult,empty,A(frame,88));Release(argument,empty,A(frame,89));
   table=host.ReadWord(factory);method=host.ReadWord(table);uint converter=0;Protected(()=>converter=host.InvokeCreateConverter(method,factory,value),value,key);table=host.ReadWord(converter);uint convert=host.ReadWord(A(table,wide?8u:12u));host.WriteWord(p,A(empty,12));uint length=host.ReadWord(unchecked(host.ReadWord(value)-12));
   Protected(()=>host.Reserve(p,A(length,25)),p,value,key);Protected(()=>host.AppendCounted(p,layout.PrefixLiteral,25),p,value,key);Protected(()=>host.AppendString(p,value),p,value,key);Protected(()=>host.CopyString(m,p),p,value,key);Protected(()=>host.AppendCounted(m,layout.MiddleLiteral,9),m,p,value,key);Protected(()=>host.CopyString(k,m),m,p,value,key);Protected(()=>host.AppendString(k,key),k,m,p,value,key);Protected(()=>host.CopyString(message,k),k,m,p,value,key);Protected(()=>host.AppendCounted(message,wide?layout.DoubleSuffixLiteral:layout.FloatSuffixLiteral,wide?10u:9u),message,k,m,p,value,key);
   Protected(()=>host.InvokeConvert(convert,converter,message),message,k,m,p,value,key);uint messageData=host.ReadWord(message);host.StoreFloating(result,wide);ReleaseData(messageData,empty,A(frame,90));Release(k,empty,A(frame,91));Release(m,empty,A(frame,92));Release(p,empty,A(frame,93));
   table=host.ReadWord(holder);method=host.ReadWord(A(table,wide?20u:24u));Protected(()=>host.InvokeSetScalar(method,holder,result),value,key);table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,36));Protected(()=>host.InvokeDestroyConverter(method,converter),value,key);Release(value,empty,A(frame,94));Release(key,empty,A(frame,95));
  }
 }
}
