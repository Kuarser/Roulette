using System;
namespace RouletteDeluxe
{
 // Complete original Game/Host/Config connection, Host Disconnect, instance
 // getters and TestComplete registration. Caller owns message scratch storage.
 public sealed class OriginalConfigurationClientConnections
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,StrError;
   public Action<uint,uint> WriteWord,CopyCString,InvokeUnary;
   public Action<uint,uint,uint,uint> InvokeTriple,AssertFail;
   public Func<uint,uint,uint,bool,uint> CreateIPC;
   public Func<uint> ErrnoLocation;
   public Action<uint,uint,uint> Fault;
  }
  private readonly Host host;
  public OriginalConfigurationClientConnections(Host h)
  {
   if(h==null||h.ReadWord==null||h.StrError==null||h.WriteWord==null||h.CopyCString==null||h.InvokeUnary==null||h.InvokeTriple==null||h.AssertFail==null||h.CreateIPC==null||h.ErrnoLocation==null||h.Fault==null)throw new ArgumentException("Actual client memory/strcpy/virtual dispatch/IPC/errno/error/fault/assert bindings required");
   host=new Host{ReadWord=h.ReadWord,StrError=h.StrError,WriteWord=h.WriteWord,CopyCString=h.CopyCString,InvokeUnary=h.InvokeUnary,InvokeTriple=h.InvokeTriple,AssertFail=h.AssertFail,CreateIPC=h.CreateIPC,ErrnoLocation=h.ErrnoLocation,Fault=h.Fault};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public uint GetConfigInstance(){return 0x09f5d5cc;}
  public uint GetConfigHostInstance(){return 0x09f5d5c4;}
  private void Connect(uint owner,uint server,uint nameCell)
  {
   host.WriteWord(Add(owner,8),server);
   if(server!=0){uint vtable=host.ReadWord(server),method=host.ReadWord(vtable);host.InvokeTriple(method,server,owner,0xffffffff);host.WriteWord(Add(owner,4),0);return;}
   uint name=host.ReadWord(nameCell),connection=host.CreateIPC(owner,owner,name,true);host.WriteWord(Add(owner,4),connection);if(connection!=0)return;
   uint error=host.ReadWord(host.ErrnoLocation()),description=host.StrError(error);host.Fault(0x10,0x097682a8,description);throw new InvalidOperationException("Native nonreturning client fault binding returned");
  }
  public void InitializeGame(uint owner,uint server){Connect(owner,server,0x0a13cfd4);}
  public void InitializeHost(uint owner,uint server){Connect(owner,server,0x0a13d02c);}
  public void InitializeConfig(uint owner,uint server,uint packet)
  {
   InitializeGame(owner,server);uint password=host.ReadWord(0x09b126a4);host.WriteWord(packet,0x210000);host.WriteWord(Add(packet,4),0x28);uint data=host.ReadWord(password),length=host.ReadWord(Add(data,0xfffffff4));
   if(length>31){host.AssertFail(0x097686fc,0x097686d0,0x38,0x09768760);throw new InvalidOperationException("Native nonreturning password assertion binding returned");}
   host.CopyCString(Add(packet,8),data);uint vtable=host.ReadWord(owner),send=host.ReadWord(Add(vtable,0x18));host.InvokeTriple(send,owner,packet,0x28);
  }
  public void DisconnectHost(uint owner)
  {
   uint server=host.ReadWord(Add(owner,8));
   if(server!=0){uint vt=host.ReadWord(server),disconnect=host.ReadWord(Add(vt,4));host.InvokeTriple(disconnect,server,owner,0xffffffff);host.WriteWord(Add(owner,4),0);host.WriteWord(Add(owner,8),0);return;}
   uint connection=host.ReadWord(Add(owner,4));if(connection==0)return;uint vtable=host.ReadWord(connection),destroy=host.ReadWord(Add(vtable,4));host.InvokeUnary(destroy,connection);
  }
  public uint RegisterTestComplete(uint owner,uint callbackOwner,uint handler,uint packet)
  {
   host.WriteWord(packet,0x80000);host.WriteWord(Add(packet,4),8);uint vt=host.ReadWord(owner),send=host.ReadWord(Add(vt,0x18));host.InvokeTriple(send,owner,packet,8);host.WriteWord(Add(owner,0x44),0);host.WriteWord(Add(owner,0x48),handler);host.WriteWord(Add(owner,0x40),callbackOwner);return 0;
  }
 }
}
