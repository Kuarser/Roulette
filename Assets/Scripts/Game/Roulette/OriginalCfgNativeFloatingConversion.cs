using System;
namespace RouletteDeluxe
{
 // ToFloat 094001a0 / ToDouble 09400250. Floating ports preserve the x87 ABI boundary.
 public sealed class OriginalCfgNativeFloatingConversion
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,AllocateException;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,CopyString,Strtof,Strtod;
   public Action<uint,bool> StoreFloating,LoadFloating;
   public Action PopFloating;
   public Func<uint> ErrnoLocation;
   public Action<uint> FreeException;
   public Action<uint,uint,uint> ThrowException;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ExceptionDestructorSlot,ExceptionTypeSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgNativeFloatingConversion(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual floating conversion host and exception layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Floating conversion binding required: "+f.Name);f.SetValue(host,v);}if(l.ExceptionDestructorSlot==0||l.ExceptionTypeSlot==0)throw new ArgumentException("Exception relocations required");layout=new Layout{ExceptionDestructorSlot=l.ExceptionDestructorSlot,ExceptionTypeSlot=l.ExceptionTypeSlot};}
  public void ToFloat(uint owner,uint message,uint frame){Convert(owner,message,frame,false);}
  public void ToDouble(uint owner,uint message,uint frame){Convert(owner,message,frame,true);}
  private void Convert(uint owner,uint message,uint frame,bool wide)
  {
   uint data=host.ReadWord(unchecked(owner+4)),length=host.ReadWord(unchecked(data-12)),end=unchecked(frame+44),temporary=unchecked(frame+16);host.WriteWord(end,data);if(wide)host.Strtod(data,end);else host.Strtof(data,end);host.StoreFloating(temporary,wide);uint errno=host.ErrnoLocation();bool reject=host.ReadWord(errno)==34;host.LoadFloating(temporary,wide);
   if(!reject){reject=unchecked((int)length)<=0;if(!reject)reject=host.ReadByte(host.ReadWord(end))!=0;}if(!reject)return;host.PopFloating();uint exception=host.AllocateException(4);
   try{host.CopyString(exception,message);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.FreeException(exception);host.Resume(e);throw new InvalidOperationException("Original nonreturning floating Resume returned");}
   uint destructor=host.ReadWord(layout.ExceptionDestructorSlot),type=host.ReadWord(layout.ExceptionTypeSlot);host.ThrowException(exception,type,destructor);throw new InvalidOperationException("Original nonreturning floating Throw returned");
  }
 }
}
