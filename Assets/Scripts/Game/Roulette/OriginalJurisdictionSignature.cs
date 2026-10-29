using System;
namespace RouletteDeluxe
{
 // Complete CheckIfSwapped body; actual signature, memory and native lifetime owners required.
 public sealed class OriginalJurisdictionSignature
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint,uint> MoveBytes;
   public Func<uint,byte> GetSignature;
   public Action<uint,uint,uint> InvokeGetSignature,InvokeSetSignature;
   public Func<uint,uint> GetEnvironment;
   public Action<uint> Delete;
   public Action<uint,uint> Fault;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  public sealed class Layout
  {
   public uint MachineDataSlot,EnvironmentCString,ReadFaultCString,MismatchFaultCString;
   public uint VectorOwner,ReadSignatureBuffer,MachineSignatureBuffer;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionSignature(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.WriteWord==null||h.ReadByte==null||h.MoveBytes==null||h.GetSignature==null||h.InvokeGetSignature==null||h.InvokeSetSignature==null||h.GetEnvironment==null||h.Delete==null||h.Fault==null||h.IsNativeUnwind==null||h.Resume==null||l.MachineDataSlot==0||l.EnvironmentCString==0||l.ReadFaultCString==0||l.MismatchFaultCString==0||l.VectorOwner==0||l.ReadSignatureBuffer==0||l.MachineSignatureBuffer==0)throw new ArgumentException("Actual signature memory, media, MachineData dispatch, environment, deletion/unwind and caller scratch bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,ReadByte=h.ReadByte,MoveBytes=h.MoveBytes,GetSignature=h.GetSignature,InvokeGetSignature=h.InvokeGetSignature,InvokeSetSignature=h.InvokeSetSignature,GetEnvironment=h.GetEnvironment,Delete=h.Delete,Fault=h.Fault,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};layout=new Layout{MachineDataSlot=l.MachineDataSlot,EnvironmentCString=l.EnvironmentCString,ReadFaultCString=l.ReadFaultCString,MismatchFaultCString=l.MismatchFaultCString,VectorOwner=l.VectorOwner,ReadSignatureBuffer=l.ReadSignatureBuffer,MachineSignatureBuffer=l.MachineSignatureBuffer};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Release(){uint begin=host.ReadWord(layout.VectorOwner);if(begin!=0)host.Delete(begin);}
  private T Protected<T>(Func<T> call)
  {try{return call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;Release();host.Resume(error);throw new InvalidOperationException("Original nonreturning unwind resume binding returned");}}
  private void Fault(uint format)
  {Protected<int>(()=>{host.Fault(16,format);throw new InvalidOperationException("Original nonreturning Fault binding returned");});}
  public int CheckIfSwapped()
  {
   for(uint i=0;i<15;++i)host.WriteWord(Add(layout.ReadSignatureBuffer,i*4),0);
   host.WriteWord(layout.VectorOwner,0);host.WriteWord(Add(layout.VectorOwner,4),0);host.WriteWord(Add(layout.VectorOwner,8),0);
   if(Protected(()=>host.GetSignature(layout.VectorOwner))!=0)Fault(layout.ReadFaultCString);
   uint begin=host.ReadWord(layout.VectorOwner),length=unchecked(host.ReadWord(Add(layout.VectorOwner,4))-begin);
   if(length!=0)Protected(()=>{host.MoveBytes(layout.ReadSignatureBuffer,begin,length);return 0;});
   uint machine=host.ReadWord(layout.MachineDataSlot),table=host.ReadWord(machine),method=host.ReadWord(Add(table,0x44));
   Protected(()=>{host.InvokeGetSignature(method,machine,layout.MachineSignatureBuffer);return 0;});
   uint offset=0;while(offset<60&&host.ReadByte(Add(layout.MachineSignatureBuffer,offset))==0)++offset;
   if(offset==60)
   {
    machine=host.ReadWord(layout.MachineDataSlot);table=host.ReadWord(machine);method=host.ReadWord(Add(table,0x48));
    Protected(()=>{host.InvokeSetSignature(method,machine,layout.ReadSignatureBuffer);return 0;});
   }
   else if(Protected(()=>host.GetEnvironment(layout.EnvironmentCString))==0)
   {
    for(uint i=0;i<60;++i){byte original=host.ReadByte(Add(layout.ReadSignatureBuffer,i));if(host.ReadByte(Add(layout.MachineSignatureBuffer,i))!=original)Fault(layout.MismatchFaultCString);}
   }
   begin=host.ReadWord(layout.VectorOwner);if(begin!=0)Protected(()=>{host.Delete(begin);return 0;});
   return 1;
  }
 }
}
