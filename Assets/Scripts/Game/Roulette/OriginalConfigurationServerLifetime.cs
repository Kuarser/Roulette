using System;
namespace RouletteDeluxe
{
 // Complete HostServer/ConfigServer C1/D1, including CreateServer unwind pads.
 public sealed class OriginalConfigurationServerLifetime
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint,uint,uint,uint> CreateServer;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host host;
  public OriginalConfigurationServerLifetime(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.CreateServer==null||h.IsNativeUnwind==null||h.Resume==null)throw new ArgumentException("Actual server word memory, IPC creation and native unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,CreateServer=h.CreateServer,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};
  }
  public void Construct(uint owner,bool hostServer)
  {
   uint name=host.ReadWord(hostServer?0x0a120214u:0x0a1201f8u);host.WriteWord(owner,hostServer?0x09729de8u:0x09729a48u);uint handle;
   try{handle=host.CreateServer(owner,owner,0,name);}
   catch(Exception ex){if(host.IsNativeUnwind(ex)){host.WriteWord(owner,0x096fd8c8);host.Resume(ex);throw new InvalidOperationException("Native nonreturning server unwind resume binding returned");}throw;}
   host.WriteWord(unchecked(owner+4),handle);
  }
  public void Destroy(uint owner){host.WriteWord(owner,0x096fd8c8);}
 }
}
