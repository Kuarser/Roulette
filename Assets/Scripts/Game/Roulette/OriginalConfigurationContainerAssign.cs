using System;
namespace RouletteDeluxe
{
 // Identical 130-byte tree assignment algorithms at085966a0/08595140.
 // Separate instances bind the actual order/allowed node copy and erase owners.
 public sealed class OriginalConfigurationTreeAssign
 {
  public sealed class Host{public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord,Erase;public Func<uint,uint,uint,uint> Copy;}
  private readonly Host host;
  public OriginalConfigurationTreeAssign(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.Erase==null||h.Copy==null)throw new ArgumentException("Actual tree memory/node erase/node copy bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Erase=h.Erase,Copy=h.Copy};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public uint Assign(uint destination,uint source)
  {
   if(destination==source)return destination;
   host.Erase(destination,host.ReadWord(Add(destination,8)));
   uint header=Add(destination,4);host.WriteWord(Add(destination,12),header);host.WriteWord(Add(destination,8),0);host.WriteWord(Add(destination,16),header);host.WriteWord(Add(destination,20),0);
   uint root=host.ReadWord(Add(source,8));if(root==0)return destination;
   root=host.Copy(destination,root,header);host.WriteWord(Add(destination,8),root);uint left=root,right=root,next;
   do{next=host.ReadWord(Add(left,8));if(next!=0)left=next;}while(next!=0);host.WriteWord(Add(destination,12),left);
   do{next=host.ReadWord(Add(right,12));if(next!=0)right=next;}while(next!=0);host.WriteWord(Add(destination,16),right);
   host.WriteWord(Add(destination,20),host.ReadWord(Add(source,20)));return destination;
  }
 }
 // Complete vector<string> assignment08267ca0/426bytes. Helper constructors
 // own their partial copies; only replacement-copy failure has local catch.
 public sealed class OriginalConfigurationRuleAssign
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,AssignString;
   public Action<uint> DestroyString,Free,Deallocate;
   public Action<uint,uint,uint> CopyRange,CopyConstRange;
   public Action ThrowBadAlloc,EndCatch,Rethrow;
   public Action<Exception> BeginCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationRuleAssign(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.AssignString==null||h.DestroyString==null||h.Free==null||h.Deallocate==null||h.CopyRange==null||h.CopyConstRange==null||h.ThrowBadAlloc==null||h.EndCatch==null||h.Rethrow==null||h.BeginCatch==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual rule vector memory/string assignment/copy/range/heap/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,AssignString=h.AssignString,DestroyString=h.DestroyString,Free=h.Free,Deallocate=h.Deallocate,CopyRange=h.CopyRange,CopyConstRange=h.CopyConstRange,ThrowBadAlloc=h.ThrowBadAlloc,EndCatch=h.EndCatch,Rethrow=h.Rethrow,BeginCatch=h.BeginCatch,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Count(uint end,uint begin){return unchecked((uint)(unchecked((int)(end-begin))>>2));}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  public uint Assign(uint destination,uint source)
  {
   if(destination==source)return destination;
   uint begin=host.ReadWord(destination),sourceBegin=host.ReadWord(source),sourceEnd=host.ReadWord(Add(source,4)),count=Count(sourceEnd,sourceBegin);
   uint capacity=Count(host.ReadWord(Add(destination,8)),begin),next;
   if(count>capacity)
   {
    uint allocated=0;
    if(count!=0)
    {
     if(count>0x3fffffff){host.ThrowBadAlloc();throw new InvalidOperationException("Native nonreturning rule assignment bad_alloc binding returned");}
     allocated=host.Allocate(unchecked(count*4));
    }
    Protected(()=>host.CopyConstRange(sourceBegin,sourceEnd,allocated),ex=>{
     host.BeginCatch(ex);host.Deallocate(allocated);
     Protected(host.Rethrow,second=>host.EndCatch());
     throw new InvalidOperationException("Native nonreturning rule assignment rethrow binding returned");
    });
    uint end=host.ReadWord(Add(destination,4));begin=host.ReadWord(destination);
    if(begin!=end){for(uint at=begin;at!=end;at=Add(at,4))host.DestroyString(at);begin=host.ReadWord(destination);}
    if(begin!=0)host.Free(begin);
    host.WriteWord(destination,allocated);next=Add(allocated,unchecked(count*4));host.WriteWord(Add(destination,8),next);
   }
   else
   {
    uint end=host.ReadWord(Add(destination,4)),length=Count(end,begin);
    if(count>length)
    {
     uint tailSource=Add(sourceBegin,unchecked(length*4)),src=sourceBegin,dst=begin;
     if(unchecked((int)length)>0)
     {
      for(uint remaining=length;remaining!=0;--remaining,src=Add(src,4),dst=Add(dst,4))host.AssignString(dst,src);
      end=host.ReadWord(Add(destination,4));uint currentBegin=host.ReadWord(destination);
      sourceEnd=host.ReadWord(Add(source,4));sourceBegin=host.ReadWord(source);
      tailSource=Add(sourceBegin,unchecked((end-currentBegin)&0xfffffffc));
     }
     host.CopyRange(tailSource,sourceEnd,end);
    }
    else
    {
     uint dst=begin,src=sourceBegin;
     if(unchecked((int)count)>0){for(uint remaining=count;remaining!=0;--remaining,dst=Add(dst,4),src=Add(src,4))host.AssignString(dst,src);begin=Add(begin,unchecked(count*4));}
     for(uint at=begin;at!=end;at=Add(at,4))host.DestroyString(at);
    }
    next=Add(host.ReadWord(destination),unchecked(count*4));
   }
   host.WriteWord(Add(destination,4),next);return destination;
  }
 }
}
