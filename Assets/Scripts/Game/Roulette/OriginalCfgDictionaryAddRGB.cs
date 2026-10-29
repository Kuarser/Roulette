using System;
namespace RouletteDeluxe
{
 // AddRGBColor 09407d60; frame is the post-prologue ESP.
 public sealed class OriginalCfgDictionaryAddRGB
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep,CopyString,Reserve,AppendString,InvokeDestroyConverter;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> ConstructCString,AppendCounted,InvokeSetRGB,InvokeSetConverter;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> InvokeCreateValue;
   public Func<uint,uint,uint,uint> InvokeCreateConverter,InvokeToUnsignedChar;
   public Action<uint,uint,uint,uint> InvokeGetAttribute,InvokePutValue;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,NameLiteral,RedLiteral,GreenLiteral,BlueLiteral,RedPrefixLiteral,GreenPrefixLiteral,BluePrefixLiteral,MiddleLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAddRGB(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual AddRGB host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("AddRGB binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("AddRGB relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,params uint[] owners){try{call();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;foreach(uint owner in owners)host.DestroyString(owner);host.Resume(e);throw new InvalidOperationException("Original nonreturning AddRGB Resume returned");}}
  private void Release(uint owner,uint empty,uint allocator)
  {
   uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==empty)return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);
  }
  private uint Message(uint converter,uint method,uint value,uint key,uint frame,uint start,uint allocator,uint prefix,uint count,uint empty,uint save)
  {
   uint p=A(frame,start),m=A(p,4),message=A(p,8);host.WriteWord(p,A(empty,12));uint length=host.ReadWord(unchecked(host.ReadWord(value)-12));
   Protected(()=>host.Reserve(p,A(length,count)),p,value,key);Protected(()=>host.AppendCounted(p,prefix,count),p,value,key);Protected(()=>host.AppendString(p,value),p,value,key);Protected(()=>host.CopyString(m,p),p,value,key);Protected(()=>host.AppendCounted(m,layout.MiddleLiteral,9),m,p,value,key);Protected(()=>host.CopyString(message,m),m,p,value,key);Protected(()=>host.AppendString(message,key),message,m,p,value,key);
   uint parsed=0;Protected(()=>parsed=host.InvokeToUnsignedChar(method,converter,message),message,m,p,value,key);if(save!=0)host.WriteByte(A(frame,save),unchecked((byte)parsed));Release(message,empty,A(frame,allocator));Release(m,empty,A(frame,allocator+1));Release(p,empty,A(frame,allocator+2));return parsed;
  }
  public void AddRGB(uint dictionary,uint node,uint frame)
  {
   uint key=A(frame,60),value=A(frame,64),name=A(frame,68),nameResult=A(frame,72),argument=A(frame,76),valueResult=A(frame,80),greenName=A(frame,96),greenResult=A(frame,100),blueName=A(frame,116),blueResult=A(frame,120);
   uint empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(key,A(empty,12));host.WriteWord(value,A(empty,12));uint factory=0,holder=0;Protected(()=>factory=host.CreateFactory(),value,key);uint table=host.ReadWord(factory),method=host.ReadWord(A(table,24));Protected(()=>holder=host.InvokeCreateValue(method,factory),value,key);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(name,layout.NameLiteral,A(frame,137)),value,key);Protected(()=>host.InvokeGetAttribute(method,nameResult,node,name),name,value,key);Protected(()=>host.AssignString(key,nameResult),nameResult,name,value,key);Release(nameResult,empty,A(frame,141));Release(name,empty,A(frame,142));
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(argument,layout.RedLiteral,A(frame,138)),value,key);Protected(()=>host.InvokeGetAttribute(method,valueResult,node,argument),argument,value,key);Protected(()=>host.AssignString(value,valueResult),valueResult,argument,value,key);Release(valueResult,empty,A(frame,143));Release(argument,empty,A(frame,144));
   table=host.ReadWord(factory);method=host.ReadWord(table);uint converter=0;Protected(()=>converter=host.InvokeCreateConverter(method,factory,value),value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,16));Message(converter,method,value,key,frame,84,145,layout.RedPrefixLiteral,26,empty,44);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(greenName,layout.GreenLiteral,A(frame,139)),value,key);Protected(()=>host.InvokeGetAttribute(method,greenResult,node,greenName),greenName,value,key);Protected(()=>host.AssignString(value,greenResult),greenResult,greenName,value,key);Release(greenResult,empty,A(frame,148));Release(greenName,empty,A(frame,149));
   table=host.ReadWord(converter);method=host.ReadWord(A(table,20));Protected(()=>host.InvokeSetConverter(method,converter,value),value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,16));Message(converter,method,value,key,frame,104,150,layout.GreenPrefixLiteral,28,empty,28);
   table=host.ReadWord(node);method=host.ReadWord(A(table,24));Protected(()=>host.ConstructCString(blueName,layout.BlueLiteral,A(frame,140)),value,key);Protected(()=>host.InvokeGetAttribute(method,blueResult,node,blueName),blueName,value,key);Protected(()=>host.AssignString(value,blueResult),blueResult,blueName,value,key);Release(blueResult,empty,A(frame,153));Release(blueName,empty,A(frame,154));
   table=host.ReadWord(converter);method=host.ReadWord(A(table,20));Protected(()=>host.InvokeSetConverter(method,converter,value),value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,16));uint blue=Message(converter,method,value,key,frame,124,155,layout.BluePrefixLiteral,27,empty,0);
   uint member=0;Protected(()=>member=host.New(3),value,key);byte green=host.ReadByte(A(frame,28)),red=host.ReadByte(A(frame,44));host.WriteByte(A(member,1),green);host.WriteByte(member,red);host.WriteByte(A(member,2),unchecked((byte)blue));table=host.ReadWord(holder);method=host.ReadWord(A(table,32));Protected(()=>host.InvokeSetRGB(method,holder,member),value,key);
   table=host.ReadWord(dictionary);method=host.ReadWord(A(table,64));Protected(()=>host.InvokePutValue(method,dictionary,key,holder),value,key);table=host.ReadWord(converter);method=host.ReadWord(A(table,36));Protected(()=>host.InvokeDestroyConverter(method,converter),value,key);Release(value,empty,A(frame,158));Release(key,empty,A(frame,159));
  }
 }
}
