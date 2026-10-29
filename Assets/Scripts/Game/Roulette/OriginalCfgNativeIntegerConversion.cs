using System;
namespace RouletteDeluxe
{
 // Original CFGStringConverter ToLong/ToUnsignedLong/ToUnsignedChar; libc stays explicit.
 public sealed class OriginalCfgNativeIntegerConversion
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,AllocateException;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,CopyString;
   public Func<uint,uint,uint,uint> Strtol,Strtoul;
   public Func<uint> ErrnoLocation;
   public Action<uint> FreeException;
   public Action<uint,uint,uint> ThrowException;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ExceptionDestructorSlot,ExceptionTypeSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgNativeIntegerConversion(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual integer conversion host and exception relocations required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Integer conversion binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Integer conversion relocation required: "+f.Name);f.SetValue(layout,v);}}
  private uint Convert(uint owner,uint message,uint frame,bool signed,bool narrow)
  {
   uint data=host.ReadWord(unchecked(owner+4)),length=host.ReadWord(unchecked(data-12)),end=unchecked(frame+28);host.WriteWord(end,data);uint value=signed?host.Strtol(data,end,0):host.Strtoul(data,end,0);uint errno=host.ErrnoLocation();
   bool reject=host.ReadWord(errno)==34||unchecked((int)length)<=0;
   if(!reject){uint last=host.ReadWord(end);reject=host.ReadByte(last)!=0;if(!reject&&narrow)reject=value>255;}
   if(!reject)return value;
   uint exception=host.AllocateException(4);
   try{host.CopyString(exception,message);}
   catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.FreeException(exception);host.Resume(error);throw new InvalidOperationException("Original nonreturning conversion Resume returned");}
   uint destructor=host.ReadWord(layout.ExceptionDestructorSlot),type=host.ReadWord(layout.ExceptionTypeSlot);host.ThrowException(exception,type,destructor);throw new InvalidOperationException("Original nonreturning conversion Throw returned");
  }
  public uint ToLong(uint owner,uint message,uint frame){return Convert(owner,message,frame,true,false);}
  public uint ToUnsignedLong(uint owner,uint message,uint frame){return Convert(owner,message,frame,false,false);}
  public uint ToUnsignedChar(uint owner,uint message,uint frame){return Convert(owner,message,frame,false,true);}
 }
}
