using System;
namespace RouletteDeluxe
{
 // Original CFGConstantsDictionary HasKey, AtKeyPutValue and GetValueHolder.
 public sealed class OriginalCfgDictionaryAccess
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,AllocateException;
   public Action<uint,uint> WriteWord,CopyString,DestroyRep;
   public Func<uint,uint,uint,int> CompareMemory;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint,uint> InsertHint;
   public Func<uint,uint,uint,uint> InvokeHasKey;
   public Action<uint,uint,uint> ConcatException,ThrowException;
   public Action<uint> DestroyString,FreeException;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,PthreadCancelSlot,MissingSuffix,ExceptionTypeSlot,ExceptionDestructorSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgDictionaryAccess(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual dictionary access host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Dictionary access binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Dictionary access relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private int Compare(uint a,uint al,uint b,uint bl){int result=host.CompareMemory(a,b,Math.Min(al,bl));return result!=0?result:unchecked((int)(al-bl));}
  private uint Find(uint owner,uint key,out bool present)
  {
   uint node=host.ReadWord(A(owner,12)),header=A(owner,8),candidate=header;present=false;if(node==0)return header;
   uint data=host.ReadWord(key),length=host.ReadWord(unchecked(data-12));
   while(node!=0){uint entry=host.ReadWord(A(node,16)),entryLength=host.ReadWord(unchecked(entry-12));if(Compare(entry,entryLength,data,length)>=0){candidate=node;node=host.ReadWord(A(node,8));}else node=host.ReadWord(A(node,12));}
   if(candidate!=header){uint entry=host.ReadWord(A(candidate,16)),entryLength=host.ReadWord(unchecked(entry-12));present=Compare(data,length,entry,entryLength)>=0;}
   return candidate;
  }
  public uint HasKey(uint owner,uint key){bool present;Find(owner,key,out present);return present?1u:0;}
  // CFGHybridDictionary::HasKey 0940d0a0 forwards owner/key and returns the complete base EAX.
  public uint HybridHasKey(uint owner,uint key){return HasKey(owner,key);}
  private void Resume(Exception error){host.Resume(error);throw new InvalidOperationException("Original nonreturning dictionary access Resume returned");}
  private void ReleaseTemporary(uint owner,uint allocator)
  {
   uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==host.ReadWord(layout.EmptyRepSlot))return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);
  }
  private uint GetOrInsert(uint owner,uint key,uint frame)
  {
   bool present;uint node=Find(owner,key,out present);if(present)return node;
   uint pair=A(frame,36),result=A(frame,28);host.CopyString(pair,key);host.WriteWord(A(pair,4),0);
   try{host.InsertHint(result,A(owner,4),node,pair);}
   catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.DestroyString(pair);Resume(error);throw;}
   node=host.ReadWord(result);ReleaseTemporary(pair,A(frame,47));return node;
  }
  public void AtKeyPutValue(uint owner,uint key,uint value,uint frame){uint node=GetOrInsert(owner,key,frame);host.WriteWord(A(node,20),value);}
  // Hybrid wrapper 0940d100 calls base with its post-prologue ESP 0x50 bytes lower.
  public void HybridAtKeyPutValue(uint owner,uint key,uint value,uint frame){AtKeyPutValue(owner,key,value,unchecked(frame-80));}
  public uint GetValueHolder(uint owner,uint key,uint frame)
  {
   uint table=host.ReadWord(owner),method=host.ReadWord(A(table,12));uint exists=host.InvokeHasKey(method,owner,key);
   if((exists&255)==0)
   {
    uint exception=host.AllocateException(4);
    try{host.ConcatException(exception,key,layout.MissingSuffix);}
    catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.FreeException(exception);Resume(error);throw;}
    uint destructor=host.ReadWord(layout.ExceptionDestructorSlot),type=host.ReadWord(layout.ExceptionTypeSlot);host.ThrowException(exception,type,destructor);throw new InvalidOperationException("Original nonreturning dictionary access Throw returned");
   }
   uint node=GetOrInsert(owner,key,frame);return host.ReadWord(A(node,20));
  }
 }
}
