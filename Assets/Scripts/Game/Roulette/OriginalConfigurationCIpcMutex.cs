using System;
namespace RouletteDeluxe
{
 // Original C IPC wrappers preserve the actual vararg syscall signatures.
 public sealed class OriginalConfigurationCIpcMutex
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,uint,int,int,int,int,int,int> SyscallMisc;
   public Func<uint,uint,int,int> SyscallRegisterName;
  }
  private readonly Host host;
  private readonly uint miscNumberSlot,registerNumberSlot;
  public OriginalConfigurationCIpcMutex(Host h,uint miscSyscallNumberSlot,uint registerNameSyscallNumberSlot)
  {
   if(h==null||h.ReadWord==null||h.SyscallMisc==null||h.SyscallRegisterName==null||miscSyscallNumberSlot==0||registerNameSyscallNumberSlot==0)throw new ArgumentException("Actual current IPC syscall number memory and both syscall signatures required");
   host=new Host{ReadWord=h.ReadWord,SyscallMisc=h.SyscallMisc,SyscallRegisterName=h.SyscallRegisterName};miscNumberSlot=miscSyscallNumberSlot;registerNumberSlot=registerNameSyscallNumberSlot;
  }
  public int Misc(uint command,int a,int b,int c,int d,int e)
  {uint number=host.ReadWord(miscNumberSlot);return host.SyscallMisc(number,command,a,b,c,d,e);}
  public int Create(){return Misc(15,0,0,0,0,0);}
  public int Delete(int id){return Misc(16,id,0,0,0,0);}
  public int Lock(int id,int blocking){return Misc(17,id,blocking,0,0,0);}
  public int Unlock(int id){return Misc(18,id,0,0,0,0);}
  public int SetPermissions(int id,int permissions){return Misc(23,id,permissions,0,0,0);}
  public int RegisterName(uint name,int id)
  {uint number=host.ReadWord(registerNumberSlot);return host.SyscallRegisterName(number,name,id);}
 }
}
