using System;
namespace RouletteDeluxe
{
 // Original syscall-number discovery and both initializer bodies.
 public sealed class OriginalConfigurationCIpcStartup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> Open;
   public Func<int,uint,uint,int> Read;
   public Func<int,int> Close;
   public Func<uint,uint,int,int> StrToLong;
   public Action<uint,uint,uint> PrintOpenError;
   public Action<uint,uint,uint,int> PrintValueError;
   public Func<uint,int> Flush;
   public Action Abort;
  }
  public sealed class Layout
  {
   public uint StartingNumberSlot,ProcFile,OpenError,ReadError,NumberError,StderrSlot;
   public uint[] SyscallNumberSlots;
  }
  private readonly Host host;
  private readonly Layout layout;
  private readonly uint scratch;
  public OriginalConfigurationCIpcStartup(Host h,Layout l,uint scratchBuffer)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.Open==null||h.Read==null||h.Close==null||h.StrToLong==null||h.PrintOpenError==null||h.PrintValueError==null||h.Flush==null||h.Abort==null||l==null||l.StartingNumberSlot==0||l.ProcFile==0||l.OpenError==0||l.ReadError==0||l.NumberError==0||l.StderrSlot==0||l.SyscallNumberSlots==null||l.SyscallNumberSlots.Length!=9||scratchBuffer==0)throw new ArgumentException("Actual IPC startup memory, libc, diagnostics, abort, relocated layout and 128-byte scratch owners required");
   foreach(uint slot in l.SyscallNumberSlots)if(slot==0)throw new ArgumentException("Actual nine IPC syscall number slots required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Open=h.Open,Read=h.Read,Close=h.Close,StrToLong=h.StrToLong,PrintOpenError=h.PrintOpenError,PrintValueError=h.PrintValueError,Flush=h.Flush,Abort=h.Abort};
   layout=new Layout{StartingNumberSlot=l.StartingNumberSlot,ProcFile=l.ProcFile,OpenError=l.OpenError,ReadError=l.ReadError,NumberError=l.NumberError,StderrSlot=l.StderrSlot,SyscallNumberSlots=(uint[])l.SyscallNumberSlots.Clone()};scratch=scratchBuffer;
  }
  private void Stop()
  {
   host.Flush(host.ReadWord(layout.StderrSlot));host.Abort();throw new InvalidOperationException("Native nonreturning IPC startup abort binding returned");
  }
  public int GetStartingNumber()
  {
   int fd=host.Open(layout.ProcFile,0);
   if(fd<0){host.PrintOpenError(host.ReadWord(layout.StderrSlot),layout.OpenError,layout.ProcFile);Stop();}
   int count=host.Read(fd,scratch,128);host.Close(fd);
   if(count<3){host.PrintValueError(host.ReadWord(layout.StderrSlot),layout.ReadError,layout.ProcFile,count);Stop();}
   int number=host.StrToLong(scratch,0,0);
   if(number<239){host.PrintValueError(host.ReadWord(layout.StderrSlot),layout.NumberError,layout.ProcFile,number);Stop();}
   return number;
  }
  public void Initialize(int phase,int priority)
  {
   if(phase!=1||priority!=65535)return;
   host.WriteWord(layout.StartingNumberSlot,unchecked((uint)GetStartingNumber()));
   for(uint i=0;i<9;++i){uint number=host.ReadWord(layout.StartingNumberSlot);host.WriteWord(layout.SyscallNumberSlots[i],unchecked(number+i));}
  }
  public void InitializeGlobal(){Initialize(1,65535);}
 }
}
