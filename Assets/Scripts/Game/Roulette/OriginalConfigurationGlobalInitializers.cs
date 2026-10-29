using System;
namespace RouletteDeluxe
{
 // Six complete translation-unit initializers. Allocator bytes are caller owners.
 public sealed class OriginalConfigurationGlobalInitializers
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint> InitializeIo,ConstructHostServer,ConstructConfigServer;
   public Action<uint,uint,uint> ConstructCString,ConstructMutex;
   public Func<uint,uint,uint,int> RegisterAtExit;
  }
  private readonly Host host;
  public OriginalConfigurationGlobalInitializers(Host h)
  {
   if(h==null||h.ReadWord==null||h.InitializeIo==null||h.ConstructHostServer==null||h.ConstructConfigServer==null||h.ConstructCString==null||h.ConstructMutex==null||h.RegisterAtExit==null)throw new ArgumentException("Actual global memory, ios, string, mutex, server and atexit owners required");
   host=new Host{ReadWord=h.ReadWord,InitializeIo=h.InitializeIo,ConstructHostServer=h.ConstructHostServer,ConstructConfigServer=h.ConstructConfigServer,ConstructCString=h.ConstructCString,ConstructMutex=h.ConstructMutex,RegisterAtExit=h.RegisterAtExit};
  }
  private void PlainIo(uint owner){host.InitializeIo(owner);host.RegisterAtExit(0x080b0ea4,owner,0x09b14f64);}
  private void PlainString(uint owner,uint literal,uint allocator){host.ConstructCString(owner,literal,allocator);host.RegisterAtExit(0x080add34,owner,0x09b14f64);}
  public void InitializeGetterObjects(uint configAllocator,uint hostAllocator)
  {
   PlainIo(0x09f5d5d4);PlainString(0x09f5d5d8,0x09692867,configAllocator);PlainString(0x09f5d5dc,0x09692863,hostAllocator);
   host.ConstructHostServer(0x09f5d5c4);host.RegisterAtExit(0x089cde00,0x09f5d5c4,0x09b14f64);host.ConstructConfigServer(0x09f5d5cc);host.RegisterAtExit(0x089cde10,0x09f5d5cc,0x09b14f64);
  }
  public void InitializeGameClientNames(uint configAllocator,uint hostAllocator){PlainIo(0x0a13cfd8);PlainString(0x0a13cfd4,0x09692867,configAllocator);PlainString(0x0a13cfdc,0x09692863,hostAllocator);}
  private void PicClientNames(bool configClient,uint configAllocator,uint hostAllocator,uint passwordAllocator)
  {
   uint io=configClient?0x0a13d020u:0x0a13d030u,config=configClient?0x0a13d024u:0x0a13d034u,hostName=configClient?0x0a13d028u:0x0a13d02cu;
   host.InitializeIo(io);uint dso=host.ReadWord(0x09b12204),ioDestructor=host.ReadWord(0x09b12db4);host.RegisterAtExit(ioDestructor,io,dso);
   host.ConstructCString(config,0x09692867,configAllocator);uint stringDestructor=host.ReadWord(0x09b12728);host.RegisterAtExit(stringDestructor,config,dso);
   host.ConstructCString(hostName,0x09692863,hostAllocator);host.RegisterAtExit(stringDestructor,hostName,dso);
   if(configClient){uint password=host.ReadWord(0x09b126a4);host.ConstructCString(password,0x097686e1,passwordAllocator);host.RegisterAtExit(stringDestructor,password,dso);}
  }
  public void InitializeConfigClientNames(uint configAllocator,uint hostAllocator,uint passwordAllocator){PicClientNames(true,configAllocator,hostAllocator,passwordAllocator);}
  public void InitializeHostClientNames(uint configAllocator,uint hostAllocator){PicClientNames(false,configAllocator,hostAllocator,0);}
  public void InitializeConfigServerNames(uint configAllocator,uint hostAllocator)
  {PlainIo(0x0a12020c);PlainString(0x0a1201f8,0x09692867,configAllocator);PlainString(0x0a120210,0x09692863,hostAllocator);host.ConstructMutex(0x0a1201fc,0,0);host.RegisterAtExit(0x080ac7a4,0x0a1201fc,0x09b14f64);}
  public void InitializeHostServerNames(uint configAllocator,uint hostAllocator)
  {PlainIo(0x0a120228);PlainString(0x0a12022c,0x09692867,configAllocator);PlainString(0x0a120214,0x09692863,hostAllocator);host.ConstructMutex(0x0a120218,0,0);host.RegisterAtExit(0x080ac7a4,0x0a120218,0x09b14f64);}
 }
}
