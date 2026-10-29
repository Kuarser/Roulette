using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70 composed physical control. Imported operation
 // bodies and original frame/ABI memory are supplied by the caller.
 public sealed class OriginalConfigurationClientGetValue
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken;
  }
  readonly OriginalConfigurationClientValueInput input;
  readonly OriginalConfigurationClientValueExchange exchange;
  readonly OriginalConfigurationClientReplyMerge merge;
  readonly OriginalConfigurationClientValueWriteback writeback;
  readonly OriginalConfigurationClientValueCleanup cleanup;
  public OriginalConfigurationClientGetValue(Host h)
  {
   if(h==null||h.ReadWord==null||h.ReadByte==null||h.WriteWord==null||h.WriteByte==null||h.AtomicAdd==null||h.Invoke==null||h.IsNativeUnwind==null||h.ExceptionToken==null)throw new ArgumentException("Original physical memory, COW atomics, imports and native exception tokens required");
   var unwind=new OriginalConfigurationClientValueUnwind(new OriginalConfigurationClientValueUnwind.Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Invoke=h.Invoke,IsNativeUnwind=h.IsNativeUnwind,ExceptionToken=h.ExceptionToken});
   Action<uint,uint,uint,uint,uint,Exception> dispatch=(pad,frame,ebx,esi,edi,e)=>unwind.Dispatch(pad,frame,h.ExceptionToken(e),ebx,esi,edi);
   Action<uint,uint,Exception> legacy=(pad,frame,e)=>{throw new InvalidOperationException("Composed GetOptionValue requires preserved-register dispatch");};
   input=new OriginalConfigurationClientValueInput(new OriginalConfigurationClientValueInput.Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,AtomicAdd=h.AtomicAdd,Invoke=h.Invoke,IsNativeUnwind=h.IsNativeUnwind,UnwindLandingPad=dispatch});
   exchange=new OriginalConfigurationClientValueExchange(new OriginalConfigurationClientValueExchange.Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,AtomicAdd=h.AtomicAdd,Invoke=h.Invoke,IsNativeUnwind=h.IsNativeUnwind,UnwindLandingPad=legacy,UnwindWithRegisters=dispatch});
   merge=new OriginalConfigurationClientReplyMerge(new OriginalConfigurationClientReplyMerge.Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,AtomicAdd=h.AtomicAdd,Invoke=h.Invoke,IsNativeUnwind=h.IsNativeUnwind,UnwindLandingPad=legacy,UnwindWithRegisters=dispatch});
   writeback=new OriginalConfigurationClientValueWriteback(new OriginalConfigurationClientValueWriteback.Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,WriteByte=h.WriteByte,AtomicAdd=h.AtomicAdd,Invoke=h.Invoke,IsNativeUnwind=h.IsNativeUnwind,UnwindLandingPad=legacy,UnwindWithRegisters=dispatch});
   cleanup=new OriginalConfigurationClientValueCleanup(new OriginalConfigurationClientValueCleanup.Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,WriteWord=h.WriteWord,WriteByte=h.WriteByte,AtomicAdd=h.AtomicAdd,Invoke=h.Invoke});
  }
  public uint Get(uint frame,uint incomingEbx,uint incomingEsi,uint incomingEdi)
  {
   uint end=input.Prepare(frame,incomingEbx,incomingEsi,incomingEdi);
   exchange.Exchange(frame,end);
   uint selected=merge.SelectAndMerge(frame);
   writeback.Write(frame,selected==unchecked(frame-0xb4u));
   return cleanup.Finish(frame);
  }
 }
}
