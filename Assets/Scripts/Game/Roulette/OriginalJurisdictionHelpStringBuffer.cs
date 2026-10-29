using System;
namespace RouletteDeluxe
{
 // Complete library-relative 0x91c50 stringbuf::str body. Owners remain caller admitted.
 public sealed class OriginalJurisdictionHelpStringBuffer
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString,DestroyRep;
   public Func<uint,int,uint> AtomicAdd;
   public Action<uint,uint,uint,uint> ConstructRange;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host host;private readonly uint emptyRepSlot,pthreadCancelSlot;
  public OriginalJurisdictionHelpStringBuffer(Host h,uint emptySlot,uint pthreadSlot)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.AssignString==null||h.DestroyRep==null||h.AtomicAdd==null||h.ConstructRange==null||h.DestroyString==null||h.IsNativeUnwind==null||h.Resume==null||emptySlot==0||pthreadSlot==0)throw new ArgumentException("Actual string range, assignment, reference release, unwind and relocated slots required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,AssignString=h.AssignString,DestroyRep=h.DestroyRep,AtomicAdd=h.AtomicAdd,ConstructRange=h.ConstructRange,DestroyString=h.DestroyString,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};emptyRepSlot=emptySlot;pthreadCancelSlot=pthreadSlot;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(uint destination,uint temporary,Action call)
  {
   try{call();}catch(Exception error)
   {
    if(!host.IsNativeUnwind(error))throw;
    if(temporary!=0)host.DestroyString(temporary);
    host.DestroyString(destination);host.Resume(error);
    throw new InvalidOperationException("Original nonreturning stringbuf unwind binding returned");
   }
  }
  public uint Extract(uint destination,uint owner,uint scratchFrame)
  {
   if(scratchFrame==0)throw new ArgumentException("Live stringbuf caller scratch frame required");
   uint emptyRep=host.ReadWord(emptyRepSlot);host.WriteWord(destination,Add(emptyRep,12));
   uint putEnd=host.ReadWord(Add(owner,0x14));
   if(putEnd==0){Protected(destination,0,()=>host.AssignString(destination,Add(owner,0x24)));return destination;}
   uint getEnd=host.ReadWord(Add(owner,0xc));bool greater=putEnd>getEnd;
   uint temporary=Add(scratchFrame,greater?0x14u:0x18u),allocator=Add(scratchFrame,greater?0x1cu:0x1du),begin=host.ReadWord(Add(owner,0x10)),end=greater?putEnd:getEnd;
   Protected(destination,0,()=>host.ConstructRange(temporary,begin,end,allocator));
   Protected(destination,temporary,()=>host.AssignString(destination,temporary));
   uint data=host.ReadWord(temporary),rep=Add(data,0xfffffff4);
   if(rep!=emptyRep)
   {
    uint reference=Add(data,0xfffffffc),prior;
    if(host.ReadWord(pthreadCancelSlot)!=0)prior=host.AtomicAdd(reference,-1);
    else{prior=host.ReadWord(reference);host.WriteWord(reference,unchecked(prior-1));}
    if(unchecked((int)prior)<=0)host.DestroyRep(rep,Add(scratchFrame,greater?0x1eu:0x1fu));
   }
   return destination;
  }
 }
}
