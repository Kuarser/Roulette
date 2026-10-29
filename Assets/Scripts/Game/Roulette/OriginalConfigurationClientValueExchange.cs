using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70 exchange phase: 094b1158 to 094b1287.
 // Caller has built the prefixed query and working vector already.
 public sealed class OriginalConfigurationClientValueExchange
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,Exception> UnwindLandingPad;
   public Action<uint,uint,uint,uint,uint,Exception> UnwindWithRegisters;
  }
  readonly Host h;
  public OriginalConfigurationClientValueExchange(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.UnwindLandingPad==null)throw new ArgumentException("Physical request/reply/COW frame, original XML/heap/transport imports and full-method unwind dispatcher required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,UnwindLandingPad=host.UnwindLandingPad,UnwindWithRegisters=host.UnwindWithRegisters};
  }
  static uint A(uint a,int b){return unchecked(a+(uint)b);}
  uint R(uint at){return h.ReadWord(at);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  uint Guard(uint frame,uint pad,Func<uint> body)
  {try{return body();}catch(Exception e){if(h.IsNativeUnwind(e)){if(h.UnwindWithRegisters!=null)h.UnwindWithRegisters(pad,frame,pad==0x94b29ef?A(frame,-0x8c):0,pad==0x94b29b0?A(frame,-0x90):0,A(frame,-0xb8),e);else h.UnwindLandingPad(pad,frame,e);throw new InvalidOperationException("Native nonreturning value-exchange unwind returned");}throw;}}
  void Release(uint cell,uint allocator)
  {uint data=R(cell),rep=A(data,-12);if(rep!=0x9b482a0&&h.AtomicAdd(A(data,-4),-1)<=0)Call(0x80ab9f4,rep,allocator);}
  public void Exchange(uint frame,uint workingEnd)
  {
   uint xml=A(frame,-0x94),copy=A(frame,-0x90),replyText=A(frame,-0x8c),replies=A(frame,-0xb8);
   h.WriteWord(A(frame,-0xa4),workingEnd);
   Guard(frame,0x94b2ab5,()=>Call(0x949ab70,xml,R(A(frame,-0x190))));
   uint size=A(R(A(R(xml),-12)),9);
   uint packet=Guard(frame,0x94b2aca,()=>Call(0x80aa1b4,size));
   h.WriteWord(A(packet,4),size);h.WriteWord(packet,0x110000);
   Guard(frame,0x94b2aca,()=>Call(0x80ac124,copy,xml));
   uint data=R(copy),count=R(A(data,-12)),available=A(R(A(packet,4)),-8);
   if(count>=available)
   {
    Guard(frame,0x94b29b0,()=>Call(0x80b63d4));
    throw new InvalidOperationException("Native nonreturning MsgXMLString overflow returned");
   }
   Guard(frame,0x94b2aca,()=>Call(0x80ae3e4,A(packet,8),data,A(count,1)));
   Release(copy,A(frame,-0x3c));
   uint response=Guard(frame,0x94b2aca,()=>Call(0x94aef40,R(A(frame,8)),packet,R(A(packet,4))));
   h.WriteWord(A(frame,-0x18c),response);Call(0x80ae7f4,packet);
   h.WriteWord(replies,0x9767ea0);h.WriteWord(A(replies,4),0);h.WriteWord(A(replies,8),0);h.WriteWord(A(replies,12),0);
   Guard(frame,0x94b29d6,()=>Call(0x80afcf4,replyText,A(R(A(frame,-0x18c)),8),A(frame,-0x3f)));
   Guard(frame,0x94b29ef,()=>Call(0x9499960,replies,replyText));
   Release(replyText,A(frame,-0x3b));
  }
 }
}
