using System;
namespace RouletteDeluxe
{
 // Complete inline-copy loops: move 0949d130 and fill_n 0949d5d0.
 // Native memory/string/tree/rule allocator/destructor/catch bindings required.
 public sealed class OriginalConfigurationUninitialized
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Allocate;
   public Action<uint,uint> WriteWord,CopyString;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint,uint> CopyOrderTree,CopyAllowedTree;
   public Action<uint,uint> EraseOrderTree,EraseAllowedTree;
   public Action<uint> DestroyString,DeallocateRules,InvokeConfigurationDestructor;
   public Action ThrowBadAlloc,EndCatch,Rethrow;
   public Action<Exception> BeginCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationUninitialized(Host h)
  {
   if(h==null||h.ReadWord==null||h.Allocate==null||h.WriteWord==null||h.CopyString==null||h.ReadByte==null||h.WriteByte==null||h.CopyOrderTree==null||h.CopyAllowedTree==null||h.EraseOrderTree==null||h.EraseAllowedTree==null||h.DestroyString==null||h.DeallocateRules==null||h.InvokeConfigurationDestructor==null||h.ThrowBadAlloc==null||h.EndCatch==null||h.Rethrow==null||h.BeginCatch==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual ConfigurationOption/string/RB-tree/rule heap/destructor/catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,Allocate=h.Allocate,WriteWord=h.WriteWord,CopyString=h.CopyString,ReadByte=h.ReadByte,WriteByte=h.WriteByte,CopyOrderTree=h.CopyOrderTree,CopyAllowedTree=h.CopyAllowedTree,EraseOrderTree=h.EraseOrderTree,EraseAllowedTree=h.EraseAllowedTree,DestroyString=h.DestroyString,DeallocateRules=h.DeallocateRules,InvokeConfigurationDestructor=h.InvokeConfigurationDestructor,ThrowBadAlloc=h.ThrowBadAlloc,EndCatch=h.EndCatch,Rethrow=h.Rethrow,BeginCatch=h.BeginCatch,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private T Protected<T>(Func<T> call,Action<Exception> cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void Word(uint destination,uint source,uint offset){host.WriteWord(Add(destination,offset),host.ReadWord(Add(source,offset)));}
  private void Byte(uint destination,uint source,uint offset){host.WriteByte(Add(destination,offset),host.ReadByte(Add(source,offset)));}
  private void InitializeTree(uint tree)
  {host.WriteWord(Add(tree,4),0);host.WriteWord(Add(tree,8),0);host.WriteWord(Add(tree,20),0);host.WriteWord(Add(tree,12),Add(tree,4));host.WriteWord(Add(tree,16),Add(tree,4));}
  private void CompleteTree(uint tree,uint source,uint root)
  {
   host.WriteWord(Add(tree,8),root);uint left=root,right=root,next;
   do{next=host.ReadWord(Add(left,8));if(next!=0)left=next;}while(next!=0);
   host.WriteWord(Add(tree,12),left);
   do{next=host.ReadWord(Add(right,12));if(next!=0)right=next;}while(next!=0);
   host.WriteWord(Add(tree,16),right);host.WriteWord(Add(tree,20),host.ReadWord(Add(source,20)));
  }
  // The completed prefix is destroyed through the current native virtual slot0.
  // Only prefix virtual calls and rethrow have the enclosing end-catch scope.
  private void FailedElement(uint first,uint current,Exception ex)
  {
   host.WriteWord(current,0x09695028);host.BeginCatch(ex);
   for(uint at=first;at!=current;at=Add(at,132))
   {uint option=at;Protected(()=>host.InvokeConfigurationDestructor(option),next=>host.EndCatch());}
   Protected(host.Rethrow,next=>host.EndCatch());
   throw new InvalidOperationException("Native nonreturning uninitialized rethrow binding returned");
  }
  public uint Move(uint sourceBegin,uint sourceEnd,uint destination)
  {
   uint first=destination,current=destination;
   for(uint source=sourceBegin;source!=sourceEnd;source=Add(source,132),current=Add(current,132))
   {uint option=current;Copy(option,source,ex=>FailedElement(first,option,ex));}
   return current;
  }
  public void Fill(uint destination,uint count,uint source)
  {
   uint first=destination,current=destination;
   for(uint remaining=count;remaining!=0;--remaining,current=Add(current,132))
   {uint option=current;Copy(option,source,ex=>FailedElement(first,option,ex));}
  }
  private void Copy(uint destination,uint source,Action<Exception> callerUnwind)
  {
   if(callerUnwind==null)throw new ArgumentException("Actual surrounding completed-option catch cleanup required");
   if(destination==0)return;
   host.WriteWord(destination,0x097681f8);
   Action<Exception> cleanup=callerUnwind;
   foreach(uint offset in new[]{4u,8u,12u,20u,28u,36u})
   {
    uint field=Add(destination,offset),original=Add(source,offset);Action<Exception> prior=cleanup;
    Protected(()=>host.CopyString(field,original),prior);
    cleanup=ex=>{host.DestroyString(field);prior(ex);};
    if(offset==12)Word(destination,source,16);
    if(offset==20)Byte(destination,source,24);
    if(offset==28)Byte(destination,source,32);
    if(offset==36)Byte(destination,source,40);
   }
   Word(destination,source,44);
   uint order=Add(destination,48),allowed=Add(destination,72);InitializeTree(order);
   uint root=host.ReadWord(Add(source,56));
   if(root!=0){uint copied=Protected(()=>host.CopyOrderTree(order,root,Add(order,4)),cleanup);CompleteTree(order,Add(source,48),copied);}
   Action<Exception> stringsCleanup=cleanup;
   Action<Exception> orderCleanup=ex=>{host.EraseOrderTree(order,host.ReadWord(Add(order,8)));stringsCleanup(ex);};
   InitializeTree(allowed);root=host.ReadWord(Add(source,80));
   if(root!=0){uint copied=Protected(()=>host.CopyAllowedTree(allowed,root,Add(allowed,4)),orderCleanup);CompleteTree(allowed,Add(source,72),copied);}
   Action<Exception> mapsCleanup=ex=>{host.EraseAllowedTree(allowed,host.ReadWord(Add(allowed,8)));orderCleanup(ex);};
   Word(destination,source,96);
   uint rules=Add(destination,100),oldEnd=host.ReadWord(Add(source,104)),oldBegin=host.ReadWord(Add(source,100));
   uint count=unchecked((uint)(unchecked((int)(oldEnd-oldBegin))>>2));
   host.WriteWord(rules,0);host.WriteWord(Add(rules,4),0);host.WriteWord(Add(rules,8),0);
   uint bytes=unchecked(count<<2),allocated;
   if(count==0)allocated=0;
   else
   {
    if(count>0x3fffffff){Protected(host.ThrowBadAlloc,mapsCleanup);throw new InvalidOperationException("Native nonreturning bad_alloc binding returned");}
    allocated=Protected(()=>host.Allocate(bytes),mapsCleanup);
   }
   host.WriteWord(Add(rules,8),Add(allocated,bytes));host.WriteWord(rules,allocated);host.WriteWord(Add(rules,4),allocated);
   uint sourceEnd=host.ReadWord(Add(source,104)),currentSource=host.ReadWord(Add(source,100)),current=allocated;
   for(;currentSource!=sourceEnd;currentSource=Add(currentSource,4),current=Add(current,4))
   {
    if(current==0)continue;uint slot=current,original=currentSource;
    Protected(()=>host.CopyString(slot,original),ex=>{
     host.BeginCatch(ex);
     for(uint at=allocated;at!=slot;at=Add(at,4))host.DestroyString(at);
     Protected(host.Rethrow,next=>{host.EndCatch();uint storage=host.ReadWord(rules);host.DeallocateRules(storage);mapsCleanup(next);});
     throw new InvalidOperationException("Native nonreturning rethrow binding returned");
    });
   }
   host.WriteWord(Add(rules,4),current);
   for(uint offset=112;offset<=126;++offset)Byte(destination,source,offset);
   Word(destination,source,128);
  }
 }
}
