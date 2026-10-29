using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70 input phase: 094b0f7c to 094b1158.
 public sealed class OriginalConfigurationClientValueInput
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   // 094ac970 regparm normalized as destination vector,source vector.
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   // pad,EBP,EBX,ESI,EDI,error. Partial option construction cleanup needs
   // the original preserved register state as well as the physical frame.
   public Action<uint,uint,uint,uint,uint,Exception> UnwindLandingPad;
  }
  readonly Host h;
  public OriginalConfigurationClientValueInput(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.UnwindLandingPad==null)throw new ArgumentException("Physical query/working/COW frame, original imports and full-method register-aware unwind required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,UnwindLandingPad=host.UnwindLandingPad};
  }
  static uint A(uint a,int b){return unchecked(a+(uint)b);}
  uint R(uint a){return h.ReadWord(a);}
  void W(uint a,uint value){h.WriteWord(a,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  uint Guard(uint frame,uint pad,uint ebx,uint esi,uint edi,Func<uint> body)
  {try{return body();}catch(Exception e){if(h.IsNativeUnwind(e)){h.UnwindLandingPad(pad,frame,ebx,esi,edi,e);throw new InvalidOperationException("Native nonreturning value-input unwind returned");}throw;}}
  void Release(uint cell,uint allocator)
  {uint data=R(cell),rep=A(data,-12);if(rep!=0x9b482a0&&h.AtomicAdd(A(data,-4),-1)<=0)Call(0x80ab9f4,rep,allocator);}
  uint Count(uint begin,uint end){return unchecked((uint)(unchecked((int)(end-begin))>>4));}
  public uint Prepare(uint frame,uint ebx,uint esi,uint edi)
  {
   uint builder=A(frame,-0xc8),query=A(frame,-0xc4),work=A(frame,-0xa8);
   W(A(frame,-0x190),builder);W(builder,0x9767ea0);W(query,0);W(A(query,4),0);W(A(query,8),0);
   Guard(frame,0x94b293e,ebx,esi,edi,()=>Call(0x94ac970,query,R(A(frame,12))));
   if(R(A(R(R(A(frame,16))),-12))!=0)
   {
    uint caller=R(A(frame,12)),begin=R(caller),end=R(A(caller,4));
    if(Count(begin,end)!=0)
    {
     esi=0;W(A(frame,-0x190),builder);ebx=A(frame,-0x98);
     do
     {
      edi=unchecked(esi<<4);uint category=A(frame,-0x9c);
      Guard(frame,0x94b293e,ebx,esi,edi,()=>Call(0x94b5360,category,A(begin,unchecked((int)edi))));
      Guard(frame,0x94b2bbf,ebx,esi,edi,()=>Call(0x80ac124,ebx,R(A(frame,16))));
      Guard(frame,0x94b2bd4,ebx,esi,edi,()=>Call(0x80af324,ebx,category));
      edi=A(edi,unchecked((int)R(A(R(A(frame,-0x190)),4))));
      Guard(frame,0x94b2be2,ebx,esi,edi,()=>Call(0x94b4970,edi,ebx));
      Release(A(frame,-0x98),A(frame,-0x3e));Release(category,A(frame,-0x3d));
      caller=R(A(frame,12));esi=A(esi,1);begin=R(caller);end=R(A(caller,4));
     }while(esi<Count(begin,end));
    }
    else W(A(frame,-0x190),builder);
   }
   else W(A(frame,-0x190),builder);
   uint sourceEnd=R(A(query,4));ebx=R(query);W(work,0);W(A(work,4),0);W(A(work,8),0);esi=Count(ebx,sourceEnd);
   uint allocation;
   if(esi==0){W(A(frame,-0x1a8),0);esi=0;allocation=R(A(frame,-0x1a8));}
   else
   {
    if(esi>0xfffffff)
    {
     Guard(frame,0x94b293e,ebx,esi,edi,()=>Call(0x80b00c4));
     throw new InvalidOperationException("Native nonreturning bad_alloc returned");
    }
    esi=unchecked(esi<<4);allocation=Guard(frame,0x94b293e,ebx,esi,edi,()=>Call(0x80aad84,esi));
    sourceEnd=R(A(query,4));ebx=R(query);W(A(frame,-0x1a8),allocation);
   }
   W(A(work,8),A(esi,unchecked((int)allocation)));esi=allocation;W(work,allocation);W(A(work,4),allocation);
   if(sourceEnd!=ebx)
   {
    W(A(frame,-0x1a4),sourceEnd);
    do
    {
     if(esi!=0)
     {
      Guard(frame,0x94b2af9,ebx,esi,edi,()=>Call(0x80ac124,esi,ebx));
      edi=A(esi,4);
      Guard(frame,0x94b2a73,ebx,esi,edi,()=>Call(0x80ac124,edi,A(ebx,4)));
      Guard(frame,0x94b2aa9,ebx,esi,edi,()=>Call(0x80ac124,A(esi,8),A(ebx,8)));
      W(A(esi,12),R(A(ebx,12)));
     }
     ebx=A(ebx,16);esi=A(esi,16);
    }while(ebx!=R(A(frame,-0x1a4)));
   }
   // Exchange stores this cursor into working end at 094b1164.
   return esi;
  }
 }
}
