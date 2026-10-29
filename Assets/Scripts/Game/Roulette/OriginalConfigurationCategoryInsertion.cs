using System;
namespace RouletteDeluxe
{
 // Complete category vector insert_aux08215fb0 and copy.isra46408205540.
 // The in-place temporary is actual caller-owned scratch string storage.
 public sealed class OriginalConfigurationCategoryInsertion
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,CopyString,AssignString;
   public Action<uint> DestroyString,Free;
   public Action<Exception> BeginCatch;
   public Action Rethrow,EndCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationCategoryInsertion(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.CopyString==null||h.AssignString==null||h.DestroyString==null||h.Free==null||h.BeginCatch==null||h.Rethrow==null||h.EndCatch==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual category memory/heap/string/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,CopyString=h.CopyString,AssignString=h.AssignString,DestroyString=h.DestroyString,Free=h.Free,BeginCatch=h.BeginCatch,Rethrow=h.Rethrow,EndCatch=h.EndCatch,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Distance(uint a,uint b){return unchecked((uint)(unchecked((int)(a-b))>>2));}
  private void Protected(Action action,Action<Exception> cleanup){try{action();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void ResumeCatch()
  {
   Protected(host.Rethrow,ex=>host.EndCatch());
   throw new InvalidOperationException("Native nonreturning category rethrow binding returned");
  }
  private void Destroy(uint begin,uint end){for(uint at=begin;at!=end;at=Add(at,4))host.DestroyString(at);}
  public uint Copy(uint begin,uint end,uint destination)
  {
   uint current=destination;
   for(uint at=begin;at!=end;at=Add(at,4),current=Add(current,4))
   {
    if(current==0)continue;uint slot=current,source=at;
    Protected(()=>host.CopyString(slot,source),ex=>{host.BeginCatch(ex);Destroy(destination,slot);ResumeCatch();});
   }
   return current;
  }
  private void Cleanup(Exception ex,uint storage,uint inserted,uint finish,bool prefixFailed)
  {
   host.BeginCatch(ex);
   if(prefixFailed||finish==0)host.DestroyString(inserted);else Destroy(storage,finish);
   if(storage!=0)host.Free(storage);ResumeCatch();
  }
  public void Insert(uint vector,uint position,uint source,uint temporaryString)
  {
   uint end=host.ReadWord(Add(vector,4)),capacity=host.ReadWord(Add(vector,8));
   if(end!=capacity)
   {
    uint next=0;if(end!=0){host.CopyString(end,Add(end,0xfffffffc));next=host.ReadWord(Add(vector,4));}
    host.WriteWord(Add(vector,4),Add(next,4));host.CopyString(temporaryString,source);
    uint currentEnd=host.ReadWord(Add(vector,4));int count=unchecked((int)Distance(Add(currentEnd,0xfffffff8),position));
    if(count>0)
    {
     uint current=Add(currentEnd,0xfffffffc);
     do{current=Add(current,0xfffffffc);uint target=current;Protected(()=>host.AssignString(target,Add(target,0xfffffffc)),ex=>host.DestroyString(temporaryString));}while(--count!=0);
    }
    Protected(()=>host.AssignString(position,temporaryString),ex=>host.DestroyString(temporaryString));host.DestroyString(temporaryString);return;
   }
   uint begin=host.ReadWord(vector),size=Distance(end,begin),offset=Distance(position,begin),newCapacity;
   if(size==0)newCapacity=1;
   else{uint doubled=unchecked(size*2);newCapacity=size>doubled||doubled>0x3fffffff?0x3fffffffu:doubled;}
   uint storage=newCapacity==0?0:host.Allocate(unchecked(newCapacity*4)),inserted=Add(storage,unchecked(offset*4));
   if(inserted!=0)Protected(()=>host.CopyString(inserted,source),ex=>Cleanup(ex,storage,inserted,storage,false));
   uint prefixBegin=host.ReadWord(vector),prefixFinish=0;
   Protected(()=>prefixFinish=Copy(prefixBegin,position,storage),ex=>Cleanup(ex,storage,inserted,0,true));
   uint suffixEnd=host.ReadWord(Add(vector,4)),finish=Add(prefixFinish,4),suffixFinish=0;
   Protected(()=>suffixFinish=Copy(position,suffixEnd,finish),ex=>Cleanup(ex,storage,inserted,finish,false));
   uint oldEnd=host.ReadWord(Add(vector,4)),oldBegin=host.ReadWord(vector);Destroy(oldBegin,oldEnd);
   if(oldBegin!=oldEnd)oldBegin=host.ReadWord(vector);if(oldBegin!=0)host.Free(oldBegin);
   host.WriteWord(vector,storage);host.WriteWord(Add(vector,4),suffixFinish);host.WriteWord(Add(vector,8),Add(storage,unchecked(newCapacity*4)));
  }
 }
}
