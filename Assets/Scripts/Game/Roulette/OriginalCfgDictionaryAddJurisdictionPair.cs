using System;
namespace RouletteDeluxe
{
 // AddJurisdictionOptionStr 09409630 / Bit 09408af0; frame is the post-prologue ESP.
 public sealed class OriginalCfgDictionaryAddJurisdictionPair
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep,CopyString,Reserve,AppendString,InvokeDestroyConverter;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,AppendCounted,InvokeSetJurisdiction,InvokeSetConverter;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> InvokeCreateValue;
   public Func<uint,uint,uint,uint> InvokeCreateConverter,InvokeToLong;
   public Action<uint,uint,uint,uint> InvokeGetAttribute,InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,NameLiteral,OffsetLiteral,OffsetPrefixLiteral,MiddleLiteral,SuffixLiteral,SizeLiteral,BitLiteral,SizePrefixLiteral,BitPrefixLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAddJurisdictionPair(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual AddJurisdiction host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("AddJurisdiction binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("AddJurisdiction relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(e);throw new InvalidOperationException("Original nonreturning AddJurisdiction Resume returned");}}
  private void Release(uint owner,uint empty,uint allocator)
  {
   uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==empty)return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);
  }
  private uint Message(uint converter,uint method,uint value,uint key,uint frame,uint start,uint allocator,uint prefix,uint count,uint empty)
  {
   uint p=A(frame,start),m=A(p,4),k=A(p,8),message=A(p,12);host.WriteWord(p,A(empty,12));uint length=host.ReadWord(unchecked(host.ReadWord(value)-12));
   Protected(()=>host.Reserve(p,A(length,count)),p,value,key);Protected(()=>host.AppendCounted(p,prefix,count),p,value,key);Protected(()=>host.AppendString(p,value),p,value,key);
   Protected(()=>host.CopyString(m,p),p,value,key);Protected(()=>host.AppendCounted(m,layout.MiddleLiteral,9),m,p,value,key);Protected(()=>host.CopyString(k,m),m,p,value,key);Protected(()=>host.AppendString(k,key),k,m,p,value,key);
   Protected(()=>host.CopyString(message,k),k,m,p,value,key);Protected(()=>host.AppendCounted(message,layout.SuffixLiteral,8),message,k,m,p,value,key);uint parsed=0;Protected(()=>parsed=host.InvokeToLong(method,converter,message),message,k,m,p,value,key);
   Release(message,empty,A(frame,allocator));Release(k,empty,A(frame,allocator+1));Release(m,empty,A(frame,allocator+2));Release(p,empty,A(frame,allocator+3));return parsed;
  }
  public void AddStr(uint dictionary,uint node,uint frame){Add(dictionary,node,frame,false);}
  public void AddBit(uint dictionary,uint node,uint frame){Add(dictionary,node,frame,true);}
  private void Add(uint dictionary,uint node,uint frame,bool bit)
  {
   uint key=A(frame,60),offset=A(frame,64),name=A(frame,68),nameResult=A(frame,72),argument=A(frame,76),offsetResult=A(frame,80),secondName=A(frame,100),secondResult=A(frame,104);
   uint empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(offset,A(empty,12));uint factory=0,holder=0;Protected(()=>factory=host.CreateFactory(),offset,key);uint table=host.ReadWord(factory),method=host.ReadWord(A(table,24));Protected(()=>holder=host.InvokeCreateValue(method,factory),offset,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(name,layout.NameLiteral,A(frame,125)),offset,key);Protected(()=>host.InvokeGetAttribute(method,nameResult,node,name),name,offset,key);Protected(()=>host.AssignString(key,nameResult),nameResult,name,offset,key);Release(nameResult,empty,A(frame,128));Release(name,empty,A(frame,129));
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(argument,layout.OffsetLiteral,A(frame,126)),offset,key);Protected(()=>host.InvokeGetAttribute(method,offsetResult,node,argument),argument,offset,key);Protected(()=>host.AssignString(offset,offsetResult),offsetResult,argument,offset,key);Release(offsetResult,empty,A(frame,130));Release(argument,empty,A(frame,131));
   table=host.ReadWord(factory);method=host.ReadWord(table);uint converter=0;Protected(()=>converter=host.InvokeCreateConverter(method,factory,offset),offset,key);table=host.ReadWord(converter);method=host.ReadWord(table);uint first=Message(converter,method,offset,key,frame,84,132,layout.OffsetPrefixLiteral,26,empty);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(secondName,bit?layout.BitLiteral:layout.SizeLiteral,A(frame,127)),offset,key);Protected(()=>host.InvokeGetAttribute(method,secondResult,node,secondName),secondName,offset,key);Protected(()=>host.AssignString(offset,secondResult),secondResult,secondName,offset,key);Release(secondResult,empty,A(frame,136));Release(secondName,empty,A(frame,137));
   table=host.ReadWord(converter);method=host.ReadWord(A(table,20));Protected(()=>host.InvokeSetConverter(method,converter,offset),offset,key);table=host.ReadWord(converter);method=host.ReadWord(table);uint second=Message(converter,method,offset,key,frame,108,138,bit?layout.BitPrefixLiteral:layout.SizePrefixLiteral,bit?23u:24u,empty);
   uint member=0;Protected(()=>member=host.New(8),offset,key);host.WriteWord(A(member,4),second);host.WriteWord(member,first);table=host.ReadWord(holder);method=host.ReadWord(A(table,bit?44u:40u));Protected(()=>host.InvokeSetJurisdiction(method,holder,member),offset,key);
   table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),offset,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,36));Protected(()=>host.InvokeDestroyConverter(method,converter),offset,key);Release(offset,empty,A(frame,142));Release(key,empty,A(frame,143));
  }
 }
}
