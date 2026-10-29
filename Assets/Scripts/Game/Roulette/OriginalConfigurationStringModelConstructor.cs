using System;
namespace RouletteDeluxe
{
 // ConfigModel<string> C2 0834b460. Frame is the original 0x6c-byte ESP
 // scratch with owner/name/module arguments at +80/+84/+88. All imported
 // COW, client, option/vector and native unwind operations remain required.
 public sealed class OriginalConfigurationStringModelConstructor
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   // Regparm imports use normalized logical arguments: insert(vector,end,
   // simple), destroy-vector(vector). They must bind the original bodies.
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  readonly Host h;
  public OriginalConfigurationStringModelConstructor(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Original model frame, COW/client/vector operations and native unwind required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Destroy(uint owner){Call(0x80add34,owner);}
  void Tail(uint frame,uint owner,int level,Exception error)
  {
   if(level>=4)Call(0x82d1690,A(frame,0x3c));
   if(level>=3)Call(0x94ba6f0,A(owner,0xc));
   if(level>=2){Destroy(A(owner,8));Destroy(A(owner,4));}
   if(level>=1)Destroy(owner);
   h.Resume(error);throw new InvalidOperationException("Native nonreturning model Resume returned");
  }
  void Guard(Action call,Action<Exception> cleanup)
  {try{call();}catch(Exception error){if(h.IsNativeUnwind(error))cleanup(error);throw;}}
  public void Construct(uint frame)
  {
   uint owner=h.ReadWord(A(frame,0x80)),name=h.ReadWord(A(frame,0x84)),module=h.ReadWord(A(frame,0x88)),client=A(owner,0xc);
   Call(0x80ac124,owner,name);
   Guard(()=>Call(0x80ac124,A(owner,4),module),e=>Tail(frame,owner,1,e));
   h.WriteWord(A(owner,8),0x9b482ac);
   Guard(()=>Call(0x94baa50,client),e=>Tail(frame,owner,2,e));
   Guard(()=>{uint service=Call(0x89c5590);Call(0x94ba7a0,client,service);Call(0x94ab710,client,owner,0x8342510);},e=>Tail(frame,owner,3,e));
   h.WriteWord(A(frame,0x3c),0);h.WriteWord(A(frame,0x40),0);h.WriteWord(A(frame,0x44),0);
   Guard(()=>Call(0x80afcf4,A(frame,0x50),0x96c5b87,A(frame,0x5d)),e=>Tail(frame,owner,4,e));
   Guard(()=>Call(0x80ac124,A(frame,0x4c),module),e=>{Destroy(A(frame,0x50));Tail(frame,owner,4,e);});
   Guard(()=>Call(0x80ac124,A(frame,0x48),name),e=>{Destroy(A(frame,0x4c));Destroy(A(frame,0x50));Tail(frame,owner,4,e);});
   Action<Exception> afterTemps=e=>{Destroy(A(frame,0x48));Destroy(A(frame,0x4c));Destroy(A(frame,0x50));Tail(frame,owner,4,e);};
   Guard(()=>Call(0x80ac124,A(frame,0x2c),A(frame,0x48)),afterTemps);
   Guard(()=>Call(0x80ac124,A(frame,0x30),A(frame,0x50)),e=>{Destroy(A(frame,0x2c));afterTemps(e);});
   Guard(()=>Call(0x80ac124,A(frame,0x34),A(frame,0x4c)),e=>{Destroy(A(frame,0x30));Destroy(A(frame,0x2c));afterTemps(e);});
   Guard(()=>Call(0x94b4900,A(frame,0x2c)),e=>{Destroy(A(frame,0x34));Destroy(A(frame,0x30));Destroy(A(frame,0x2c));afterTemps(e);});
   uint end=h.ReadWord(A(frame,0x40)),capacity=h.ReadWord(A(frame,0x44));
   Action<Exception> afterSimple=e=>{Call(0x81d7ac0,A(frame,0x2c));afterTemps(e);};
   if(end==capacity)Guard(()=>Call(0x82da490,A(frame,0x3c),end,A(frame,0x2c)),afterSimple);
   else
   {
    uint next=0;
    if(end!=0)
    {
     Guard(()=>Call(0x80ac124,end,A(frame,0x2c)),afterSimple);
     Guard(()=>Call(0x80ac124,A(end,4),A(frame,0x30)),e=>{Destroy(end);afterSimple(e);});
     Guard(()=>Call(0x80ac124,A(end,8),A(frame,0x34)),e=>{Destroy(A(end,4));Destroy(end);afterSimple(e);});
     h.WriteWord(A(end,0xc),h.ReadWord(A(frame,0x38)));next=h.ReadWord(A(frame,0x40));
    }
    h.WriteWord(A(frame,0x40),A(next,0x10));
   }
   Destroy(A(frame,0x34));Destroy(A(frame,0x30));Destroy(A(frame,0x2c));Destroy(A(frame,0x48));Destroy(A(frame,0x4c));Destroy(A(frame,0x50));
   Guard(()=>Call(0x80afcf4,A(frame,0x54),0x96c5b87,A(frame,0x5e)),e=>Tail(frame,owner,4,e));
   Guard(()=>Call(0x94b0340,client,A(frame,0x3c),A(frame,0x54)),e=>{Destroy(A(frame,0x54));Tail(frame,owner,4,e);});Destroy(A(frame,0x54));
   Guard(()=>Call(0x80afcf4,A(frame,0x58),0x96c5b87,A(frame,0x5f)),e=>Tail(frame,owner,4,e));
   Guard(()=>Call(0x94b0f70,client,A(frame,0x3c),A(frame,0x58)),e=>{Destroy(A(frame,0x58));Tail(frame,owner,4,e);});Destroy(A(frame,0x58));
   Guard(()=>Call(0x80ab384,A(owner,8),A(h.ReadWord(A(frame,0x3c)),4)),e=>Tail(frame,owner,4,e));
   uint limit=h.ReadWord(A(frame,0x40)),begin=h.ReadWord(A(frame,0x3c)),current=begin;
   while(current!=limit){Destroy(A(current,8));Destroy(A(current,4));Destroy(current);current=A(current,16);}
   if(begin!=limit)begin=h.ReadWord(A(frame,0x3c));if(begin!=0)Call(0x80aabe4,begin);
  }
 }
}
