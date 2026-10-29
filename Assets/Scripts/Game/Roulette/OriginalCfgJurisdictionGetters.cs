using System;
namespace RouletteDeluxe
{
 // GetJurisdictionOption/Str/Bit: holder, dictionary and Hybrid entry points.
 // Every frame argument is the ESP after that function's prologue.
 public sealed class OriginalCfgJurisdictionGetters
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,AllocateException;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint,uint> InvokePredicate,InvokeValueGetter;
   public Func<uint,uint,uint,uint> InvokeValueLookup;
   public Action<uint,uint,uint> InvokeGetValueType,ConcatPrefix,ConcatCStringSuffix,ConcatStrings,ThrowException;
   public Action<uint> DestroyString,FreeException;
   public Func<Exception,uint> BeginCatch;
   public Action EndCatch;
   public Func<Exception,bool> IsNativeUnwind,IsStringException;
   public Action<Exception> Resume;
   public Func<int,uint,uint,uint,uint> InvokeBaseGetter;
  }
  [Serializable] public sealed class Layout{public uint BitPrefix,StrPrefix,OptionPrefix,AtKeyLiteral,ExceptionDestructorSlot,ExceptionTypeSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgJurisdictionGetters(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual getter host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Getter binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Getter relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception e){host.Resume(e);throw new InvalidOperationException("Original nonreturning getter Resume returned");}
  private uint Holder(uint owner,uint frame,int kind)
  {
   uint table=host.ReadWord(owner),method=host.ReadWord(A(table,kind==0?176u:kind==1?172u:168u));
   if((host.InvokePredicate(method,owner)&255)!=0)return host.ReadWord(A(owner,36));
   uint exception=host.AllocateException(4),temporary=A(frame,28);table=host.ReadWord(owner);method=host.ReadWord(A(table,88));
   try{host.InvokeGetValueType(method,temporary,owner);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.FreeException(exception);Resume(e);throw;}
   uint prefix=kind==0?layout.BitPrefix:kind==1?layout.StrPrefix:layout.OptionPrefix;
   try{host.ConcatPrefix(exception,prefix,temporary);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.DestroyString(temporary);host.FreeException(exception);Resume(e);throw;}
   host.DestroyString(temporary);uint destructor=host.ReadWord(layout.ExceptionDestructorSlot),type=host.ReadWord(layout.ExceptionTypeSlot);host.ThrowException(exception,type,destructor);throw new InvalidOperationException("Original nonreturning holder Throw returned");
  }
  private uint Dictionary(uint owner,uint key,uint frame,int kind)
  {
   uint table=host.ReadWord(owner),method=host.ReadWord(A(table,68)),value=host.InvokeValueLookup(method,owner,key);
   table=host.ReadWord(value);method=host.ReadWord(A(table,kind==0?84u:kind==1?80u:76u));
   try{return host.InvokeValueGetter(method,value);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;if(!host.IsStringException(e)){Resume(e);throw;}return Mismatch(e,key,frame);}
  }
  private uint Mismatch(Exception caught,uint key,uint frame)
  {
   uint error=host.BeginCatch(caught);host.WriteWord(A(frame,28),error);uint exception=host.AllocateException(4);error=host.ReadWord(A(frame,28));uint temporary=A(frame,44);
   try{host.ConcatCStringSuffix(temporary,error,layout.AtKeyLiteral);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.FreeException(exception);host.EndCatch();Resume(e);throw;}
   try{host.ConcatStrings(exception,temporary,key);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.DestroyString(temporary);host.FreeException(exception);host.EndCatch();Resume(e);throw;}
   host.DestroyString(temporary);uint destructor=host.ReadWord(layout.ExceptionDestructorSlot),type=host.ReadWord(layout.ExceptionTypeSlot);
   try{host.ThrowException(exception,type,destructor);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.EndCatch();Resume(e);throw;}
   throw new InvalidOperationException("Original nonreturning dictionary Throw returned");
  }
  public uint HolderBit(uint owner,uint frame){return Holder(owner,frame,0);}
  public uint HolderStr(uint owner,uint frame){return Holder(owner,frame,1);}
  public uint HolderOption(uint owner,uint frame){return Holder(owner,frame,2);}
  public uint DictionaryBit(uint owner,uint key,uint frame){return Dictionary(owner,key,frame,0);}
  public uint DictionaryStr(uint owner,uint key,uint frame){return Dictionary(owner,key,frame,1);}
  public uint DictionaryOption(uint owner,uint key,uint frame){return Dictionary(owner,key,frame,2);}
  public uint HybridBit(uint owner,uint key,uint frame){return host.InvokeBaseGetter(0,owner,key,unchecked(frame-80));}
  public uint HybridStr(uint owner,uint key,uint frame){return host.InvokeBaseGetter(1,owner,key,unchecked(frame-80));}
  public uint HybridOption(uint owner,uint key,uint frame){return host.InvokeBaseGetter(2,owner,key,unchecked(frame-80));}
 }
}
