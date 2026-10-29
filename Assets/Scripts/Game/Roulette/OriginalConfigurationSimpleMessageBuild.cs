using System;
namespace RouletteDeluxe
{
 // XMLMsgVofSimpleOptions::BuildMsg 0949ab70. Frame is original ESP after
 // prologue; output and builder arguments remain at +110 and +114.
 public sealed class OriginalConfigurationSimpleMessageBuild
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
  readonly Host h;
  public OriginalConfigurationSimpleMessageBuild(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, memory, COW atomics, imported stream/XML bodies and native exception token required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint R(uint a){return h.ReadWord(a);}
  void W(uint a,uint v){h.WriteWord(a,v);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Release(uint cell,uint allocator)
  {
   uint data=R(cell),rep=A(data,0xfffffff4);
   if(rep!=0x9b482a0 && h.AtomicAdd(A(data,0xfffffffc),-1)<=0)Call(0x80ab9f4,rep,allocator);
  }
  void ResetStream(uint frame)
  {
   uint stream=A(frame,0x1c),vtt=R(0x9b49594);
   W(stream,vtt);W(A(stream,R(A(vtt,0xfffffff4))),R(0x9b49598));
  }
  void Unwind(uint pad,uint frame,uint savedEbx,Exception error)
  {
   uint token=h.ExceptionToken(error),stream=A(frame,0x1c);
   if(pad==0x949b1a8){Call(0x80add34,savedEbx);}
   else if(pad==0x949b088){W(A(frame,0x48),0x9b49c10);Call(0x80b0fc4,A(frame,0x48));}
   else if(pad==0x949b04a)
   {
    Call(0x81a1a80,savedEbx);ResetStream(frame);W(A(frame,0x48),0x9b49c10);Call(0x80b0fc4,A(frame,0x48));
   }
   else
   {
    if(pad==0x949b008)Call(0x80add34,A(frame,0xd8));
    if(pad==0x949b145||pad==0x949b1c8)Call(0x80add34,savedEbx);
    if(pad==0x949b034||pad==0x949b131||pad==0x949b136||pad==0x949b145||pad==0x949b1c8)Call(0x80add34,R(A(frame,0x110)));
    Call(0x80b0304,stream);
   }
   Call(0x80aed64,token);throw new InvalidOperationException("Native nonreturning Resume returned");
  }
  uint Guard(uint frame,uint pad,uint savedEbx,Func<uint> operation)
  {try{return operation();}catch(Exception e){if(h.IsNativeUnwind(e))Unwind(pad,frame,savedEbx,e);throw;}}
  void Newline(uint frame)
  {
   uint stream=A(frame,0x1c),facet=R(A(A(stream,R(A(R(stream),0xfffffff4))),0x7c));
   if(facet==0){Guard(frame,0x949afb9,0,()=>Call(0x80ae674));throw new InvalidOperationException("Native nonreturning bad_cast returned");}
   uint ch;
   if(h.ReadByte(A(facet,0x1c))!=0)ch=h.ReadByte(A(facet,0x27));
   else
   {
    Guard(frame,0x949afb9,0,()=>Call(0x80aa954,facet));
    ch=Guard(frame,0x949afb9,0,()=>Call(R(A(R(facet),0x18)),facet,10));
   }
   uint widened=unchecked((uint)(int)(sbyte)(byte)ch);
   uint returned=Guard(frame,0x949afb9,0,()=>Call(0x80accc4,stream,widened));
   Guard(frame,0x949afb9,0,()=>Call(0x80acbb4,returned));
  }
  public uint Build(uint frame)
  {
   uint builder=R(A(frame,0x114)),name=A(frame,0xd0),stream=A(frame,0x1c),ios=A(frame,0x48),buffer=A(frame,0x20);
   Call(0x80afcf4,name,0x96c5b87,A(frame,0xe7));
   Guard(frame,0x949b1a8,name,()=>Call(0x94ba610,A(frame,0xd4),name,1));
   Release(A(frame,0xd4),A(frame,0xe8));Release(name,A(frame,0xe9));
   Call(0x80ad8c4,ios);
   uint vtt=R(0x9b49594);h.WriteByte(A(frame,0xbc),0);W(ios,0x9b49c10);W(A(frame,0xb8),0);W(stream,vtt);
   uint virtualIos=A(stream,R(A(vtt,0xfffffff4)));h.WriteByte(A(frame,0xbd),0);W(A(frame,0xc0),0);W(virtualIos,R(0x9b49598));
   W(A(frame,0xc4),0);W(A(frame,0xc8),0);W(A(frame,0xcc),0);
   Guard(frame,0x949b088,0,()=>Call(0x80b06f4,virtualIos,0));
   W(stream,0x9b48dec);W(ios,0x9b48e00);W(buffer,0x9b48a28);
   for(uint i=0x24;i<=0x38;i+=4)W(A(frame,i),0);
   Call(0x80b11f4,A(frame,0x3c));W(buffer,0x9b497c8);W(A(frame,0x40),0x10);W(A(frame,0x44),0x9b482ac);
   Guard(frame,0x949b04a,buffer,()=>Call(0x80b06f4,ios,buffer));
   uint current=R(A(builder,4));
   while(current!=R(A(builder,8)))
   {
    Guard(frame,0x949afb9,0,()=>Call(0x80ad354,stream,0x9767d02,8));Newline(frame);
    uint at=current;Guard(frame,0x949afb9,0,()=>Call(0x94b5390,A(frame,0xd8),at));
    uint data=R(A(frame,0xd8));Guard(frame,0x949b008,0,()=>Call(0x80ad354,stream,data,R(A(data,0xfffffff4))));
    Release(A(frame,0xd8),A(frame,0xea));
    Guard(frame,0x949afb9,0,()=>Call(0x80ad354,stream,0x9767d18,9));Newline(frame);current=A(current,16);
   }
   uint output=R(A(frame,0x110));W(output,0x9b482ac);uint put=R(A(frame,0x34)),getEnd=R(A(frame,0x2c));
   if(put==0)Guard(frame,0x949b034,0,()=>Call(0x80ab384,output,A(frame,0x44)));
   else
   {
    bool extended=put>getEnd;uint temp=A(frame,extended?0xe0u:0xdcu),last=extended?put:getEnd,begin=R(A(frame,0x30));
    Guard(frame,extended?0x949b136u:0x949b131u,temp,()=>Call(0x94987f0,temp,begin,last,A(frame,extended?0xecu:0xebu)));
    Guard(frame,extended?0x949b145u:0x949b1c8u,temp,()=>Call(0x80ab384,output,temp));Release(temp,A(frame,extended?0xedu:0xeeu));
   }
   W(stream,0x9b48dec);W(ios,0x9b48e00);W(buffer,0x9b497c8);Release(A(frame,0x44),A(frame,0xef));
   W(buffer,0x9b48a28);Call(0x80ad934,A(frame,0x3c));ResetStream(frame);W(ios,0x9b49c10);Call(0x80b0fc4,ios);
   return R(A(frame,0x110));
  }
 }
}
