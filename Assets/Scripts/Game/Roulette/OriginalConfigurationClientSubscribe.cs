using System;
namespace RouletteDeluxe
{
 // GameConfigClient::RegisterForChanges 094b0340, including inline atomic
 // COW releases and original native cleanup tails. Caller owns original frame.
 public sealed class OriginalConfigurationClientSubscribe
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  readonly Host h;
  public OriginalConfigurationClientSubscribe(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Physical request frame, atomic COW memory, vector/XML/transport and unwind required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint R(uint a){return h.ReadWord(a);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Destroy(uint owner){Call(0x80add34,owner);}
  void Release(uint owner,uint allocator,uint oldCell=0)
  {
   uint data=R(owner),rep=A(data,0xfffffff4);if(rep==0x9b482a0)return;
   int old=h.AtomicAdd(A(data,0xfffffffc),-1);if(oldCell!=0)h.WriteWord(oldCell,unchecked((uint)old));
   if(old<=0)Call(0x80ab9f4,rep,allocator);
  }
  void Guard(Action body,Action<Exception> cleanup)
  {try{body();}catch(Exception e){if(h.IsNativeUnwind(e)){cleanup(e);h.Resume(e);throw new InvalidOperationException("Native nonreturning subscribe Resume returned");}throw;}}
  void DestroyMessage(uint frame){Call(0x8df1640,R(A(frame,0x14)));}
  // Frame is ESP after four saved registers/sub 5c. Arguments: client +70,
  // input vector +74, COW category prefix +78. Builder +28, strings +38..44.
  public uint Register(uint frame)
  {
   uint source=R(A(frame,0x74)),builder=A(frame,0x28),vector=A(frame,0x2c);
   h.WriteWord(A(frame,0x14),builder);h.WriteWord(builder,0x9767ea0);
   h.WriteWord(vector,0);h.WriteWord(A(vector,4),0);h.WriteWord(A(vector,8),0);
   Guard(()=>Call(0x94ac970,vector,source),e=>DestroyMessage(frame));
   uint prefix=R(A(frame,0x78)),prefixData=R(prefix);
   if(R(A(prefixData,0xfffffff4))!=0)
   {
    uint begin=R(source),end=R(A(source,4));
    if(unchecked((int)(end-begin))>>4!=0)
    {
     h.WriteWord(A(frame,0x14),builder);uint index=0;
     do
     {
      uint scaled=unchecked(index<<4),category=A(frame,0x38),combined=A(frame,0x3c);
      Guard(()=>Call(0x94b5360,category,A(begin,scaled)),e=>DestroyMessage(frame));
      Guard(()=>Call(0x80ac124,combined,R(A(frame,0x78))),e=>{Destroy(category);DestroyMessage(frame);});
      Guard(()=>Call(0x80af324,combined,category),e=>{Destroy(combined);Destroy(category);DestroyMessage(frame);});
      Guard(()=>Call(0x94b4970,A(R(A(R(A(frame,0x14)),4)),scaled),combined),e=>{Destroy(combined);Destroy(category);DestroyMessage(frame);});
      Release(combined,A(frame,0x49));Release(category,A(frame,0x4a));
      begin=R(source);index=unchecked(index+1);end=R(A(source,4));
      if(index>=unchecked((uint)(unchecked((int)(end-begin))>>4)))break;
     }while(true);
    }
    else h.WriteWord(A(frame,0x14),builder);
   }
   else h.WriteWord(A(frame,0x14),builder);
   uint xml=A(frame,0x40),copy=A(frame,0x44);
   Guard(()=>Call(0x949ab70,xml,R(A(frame,0x14))),e=>DestroyMessage(frame));
   Action<Exception> sharedCleanup=e=>{Destroy(xml);DestroyMessage(frame);};
   uint length=unchecked(R(A(R(xml),0xfffffff4))+9),request=0;
   Guard(()=>request=Call(0x80aa1b4,length),sharedCleanup);
   h.WriteWord(A(request,4),length);h.WriteWord(request,0x50000);
   Guard(()=>Call(0x80ac124,copy,xml),sharedCleanup);
   uint data=R(copy),size=R(A(data,0xfffffff4)),capacity=unchecked(R(A(request,4))-8);
   if(size>=capacity)
   {
    Guard(()=>Call(0x80b63d4),e=>{Destroy(copy);sharedCleanup(e);});
    throw new InvalidOperationException("Native nonreturning XML capacity fault returned");
   }
   Guard(()=>Call(0x80ae3e4,A(request,8),data,unchecked(size+1)),sharedCleanup);
   Release(copy,A(frame,0x4b));uint response=0;
   Guard(()=>response=Call(0x94aef40,R(A(frame,0x70)),request,R(A(request,4))),sharedCleanup);
   Call(0x80ae7f4,request);uint result=R(A(response,8));h.WriteWord(A(frame,0x1c),result);Call(0x80ae7f4,response);
   Release(xml,A(frame,0x4c));
   uint limit=R(A(frame,0x30)),first=R(A(frame,0x2c));h.WriteWord(builder,0x9767ea0);h.WriteWord(A(frame,0x14),limit);
   if(limit!=first)
   {
    uint at=first,category=A(first,8);h.WriteWord(A(frame,0x18),A(first,4));
    do
    {
     uint offset=unchecked(at-first);Release(A(offset,category),A(frame,0x4d),A(frame,0x10));Release(A(offset,R(A(frame,0x18))),A(frame,0x4e));Release(at,A(frame,0x4f));at=A(at,16);
    }while(R(A(frame,0x14))!=at);
    first=R(A(frame,0x2c));
   }
   if(first!=0)Call(0x80aabe4,first);return R(A(frame,0x1c));
  }
 }
}
