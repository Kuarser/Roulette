using System;
namespace RouletteDeluxe
{
 // NV2Type<MachineDataNvram/ExtendedNvram> ReadData and WriteData.
 // The caller owns the stream, table, data region and native/PC transport.
 public sealed class OriginalMachineNvPayload
 {
  public const uint DataOffset=0x48,MainLength=0x22d,ExtendedLength=0x87;
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint,uint,uint> InvokeStream;
  }
  private readonly Host host;
  private readonly uint length;
  public OriginalMachineNvPayload(Host h,bool extended)
  {
   if(h==null||h.ReadWord==null||h.InvokeStream==null)throw new ArgumentException("Actual stream table and data transport required");
   host=new Host{ReadWord=h.ReadWord,InvokeStream=h.InvokeStream};length=extended?ExtendedLength:MainLength;
  }
  public void ReadData(uint owner,uint stream)
  {uint table=host.ReadWord(stream),method=host.ReadWord(unchecked(table+12));host.InvokeStream(method,stream,unchecked(owner+DataOffset),length);}
  public void WriteData(uint owner,uint stream)
  {uint table=host.ReadWord(stream),method=host.ReadWord(unchecked(table+8));host.InvokeStream(method,stream,unchecked(owner+DataOffset),length);}
  // PC memory-stream admission composed with the existing original NV2 storage.
  // No default-region-to-NV projection is invented here. The caller must initialize
  // the actual payload through recovered MachineData setters/startup controls.
  public static OriginalNv2Storage BindStorage(OriginalNativeMemory memory,uint owner,bool extended,uint stream,IOriginalNvDevice device,Action<string> recovery)
  {
   if(memory==null||device==null||recovery==null)throw new ArgumentNullException();
   byte[] transport=null;bool reading=false;
   var payload=new OriginalMachineNvPayload(new Host{ReadWord=memory.ReadWord,InvokeStream=(method,current,data,count)=>{
    uint expected=memory.ReadWord(memory.ReadWord(stream)+(reading?12u:8u));
    if(current!=stream||method!=expected||method==0||data!=unchecked(owner+DataOffset)||count!=(extended?ExtendedLength:MainLength))throw new InvalidOperationException("MachineData stream binding changed");
    if(reading){if(transport==null||transport.Length!=count)throw new OriginalNvFault("MachineData NV payload length mismatch");memory.WriteBytes(data,transport);}
    else transport=memory.ReadBytes(data,count);
   }},extended);
   return new OriginalNv2Storage(device,()=>{reading=false;transport=null;payload.WriteData(owner,stream);return transport;},bytes=>{reading=true;transport=bytes;payload.ReadData(owner,stream);transport=null;},recovery);
  }
 }
}
