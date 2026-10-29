using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70 reply-selection/merge phase, 094b1287 onward.
 // Returns the original physical vector to feed to the subsequent native
 // stringstream value-writeback phase; it does not perform transport/writeback.
 public sealed class OriginalConfigurationClientReplyMerge
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   // Mandatory dispatch to the original full-method cleanup landing pad. This
   // phase owns no outer request/XML/vector lifetime and cannot replace it.
   public Action<uint,uint,Exception> UnwindLandingPad;
   public Action<uint,uint,uint,uint,uint,Exception> UnwindWithRegisters;
  }
  readonly Host h;
  public OriginalConfigurationClientReplyMerge(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.UnwindLandingPad==null)throw new ArgumentException("Physical reply/COW/vector memory, original imports and full-method unwind dispatcher required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,UnwindLandingPad=host.UnwindLandingPad,UnwindWithRegisters=host.UnwindWithRegisters};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint R(uint at){return h.ReadWord(at);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Guard(uint frame,uint landing,Action body)
  {try{body();}catch(Exception e){if(h.IsNativeUnwind(e)){if(h.UnwindWithRegisters!=null)h.UnwindWithRegisters(landing,frame,0,0,A(frame,0xffffff48),e);else h.UnwindLandingPad(landing,frame,e);throw new InvalidOperationException("Native nonreturning reply-merge unwind returned");}throw;}}
  void Release(uint cell,uint allocator)
  {
   uint data=R(cell),rep=A(data,0xfffffff4);if(rep!=0x9b482a0&&h.AtomicAdd(A(data,0xfffffffc),-1)<=0)Call(0x80ab9f4,rep,allocator);
  }
  bool Compare(uint frame,uint left,uint right,bool nested)
  {
   uint nameLeft=A(frame,nested?0xffffff94u:0xffffff84u),nameRight=A(frame,nested?0xffffff90u:0xffffff80u);
   uint categoryLeft=A(frame,nested?0xffffff9cu:0xffffff8cu),categoryRight=A(frame,nested?0xffffff98u:0xffffff88u);
   // Category scratch cells are -64/-68 in the nested path, -74/-78 in
   // the first path; keep them distinct from the already-live name cells.
   Guard(frame,nested?0x94b2902u:0x94b2b50u,()=>Call(0x94b5330,nameLeft,left));
   Guard(frame,nested?0x94b2920u:0x94b2b57u,()=>Call(0x94b5330,nameRight,right));
   uint dataRight=R(nameRight),dataLeft=R(nameLeft),size=R(A(dataRight,0xfffffff4));bool match=false;
   if(size==R(A(dataLeft,0xfffffff4))&&Call(0x80aafa4,dataRight,dataLeft,size)==0)
   {
    Guard(frame,nested?0x94b292fu:0x94b2b93u,()=>Call(0x94b5360,categoryLeft,left));
    Guard(frame,nested?0x94b2959u:0x94b2ba2u,()=>Call(0x94b5360,categoryRight,right));
    dataRight=R(categoryRight);dataLeft=R(categoryLeft);size=R(A(dataRight,0xfffffff4));match=size==R(A(dataLeft,0xfffffff4))&&Call(0x80aafa4,dataRight,dataLeft,size)==0;
    Release(categoryRight,A(frame,nested?0xffffffd2u:0xffffffceu));Release(categoryLeft,A(frame,nested?0xffffffd3u:0xffffffcfu));
   }
   Release(nameRight,A(frame,nested?0xffffffd4u:0xffffffd0u));Release(nameLeft,A(frame,nested?0xffffffd5u:0xffffffd1u));return match;
  }
  void Assign(uint frame,uint target,uint source)
  {
   Guard(frame,0x94b2b91,()=>{Call(0x80ab384,target,source);Call(0x80ab384,A(target,4),A(source,4));Call(0x80ab384,A(target,8),A(source,8));});
   h.WriteWord(A(target,12),R(A(source,12)));
  }
  // EBP frame: caller vector argument +c; working vector -a8, replies -b4.
  public uint SelectAndMerge(uint frame)
  {
   uint caller=R(A(frame,0xc)),reply=A(frame,0xffffff4c),work=A(frame,0xffffff58),replyEnd=R(A(reply,4)),replyAt=R(reply);
   uint callerCount=unchecked((uint)(unchecked((int)(R(A(caller,4))-R(caller)))>>4)),replyCount=unchecked((uint)(unchecked((int)(replyEnd-replyAt))>>4));
   if(callerCount==replyCount)return reply;
   uint at=R(work),end=R(A(work,4));if(at!=end)h.WriteWord(A(frame,0xfffffe4c),work);
   if(at!=end&&replyAt!=replyEnd)
   {
    do
    {
     if(Compare(frame,at,replyAt,false)){Assign(frame,at,replyAt);replyAt=A(replyAt,16);}
     at=A(at,16);end=R(A(work,4));
    }while(at!=end&&replyAt!=R(A(reply,4)));
    if(at==end)replyEnd=R(A(reply,4));
   }
   if(replyAt!=replyEnd)
   {
    at=R(work);end=R(A(work,4));
    if(at!=end)
    {
     do
     {
      uint pending=R(reply);replyEnd=R(A(reply,4));
      // Native nested pass restarts at the beginning of the reply vector,
      // including replies already consumed by the sequential pass.
      while(pending!=replyEnd){if(Compare(frame,at,pending,true))Assign(frame,at,pending);pending=A(pending,16);replyEnd=R(A(reply,4));}
      at=A(at,16);end=R(A(work,4));
     }while(at!=end);
    }
   }
   return work;
  }
 }
}
