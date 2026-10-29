using System;
namespace RouletteDeluxe
{
 // Original Boost count release and Xerces-to-COW-string resource control.
 public sealed class OriginalCfgParserResources
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New,TranscodeWide;
   public Action<uint,uint> WriteWord,InvokeDispose,InvokeDestroy;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint> DeleteArray;
   public Action<uint,uint,uint> ConstructCString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> BeginCatch,Rethrow,Resume;
   public Action EndCatch;
  }
  [Serializable] public sealed class Layout{public uint NarrowCountVtableSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgParserResources(Host h,Layout l)
  {
   if(h==null||l==null||l.NarrowCountVtableSlot==0)throw new ArgumentException("Parser resource bindings and relocated count table required");
   host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Parser resource binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout{NarrowCountVtableSlot=l.NarrowCountVtableSlot};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception e){host.Resume(e);throw new InvalidOperationException("Original nonreturning resource Resume returned");}
  private void ReleasePointer(uint count)
  {
   if(count==0)return;if(host.AtomicAdd(A(count,4),-1)!=1)return;uint table=host.ReadWord(count),method=host.ReadWord(A(table,8));host.InvokeDispose(method,count);
   if(host.AtomicAdd(A(count,8),-1)!=1)return;table=host.ReadWord(count);method=host.ReadWord(A(table,12));host.InvokeDestroy(method,count);
  }
  public void DestroySharedCount(uint owner){ReleasePointer(host.ReadWord(owner));}
  public void DisposeArray(uint count){uint array=host.ReadWord(A(count,12));if(array!=0)host.DeleteArray(array);}
  public uint StringTranscode(uint result,uint wide,uint frame)
  {
   uint buffer=host.TranscodeWide(wide);host.WriteWord(A(frame,24),0);host.WriteWord(A(frame,20),buffer);uint count;
   try{count=host.New(20);}catch(Exception error)
   {
    if(!host.IsNativeUnwind(error))throw;host.BeginCatch(error);if(buffer!=0)host.DeleteArray(buffer);
    try{host.Rethrow(error);throw new InvalidOperationException("Original nonreturning resource Rethrow returned");}
    catch(Exception rethrow){if(!host.IsNativeUnwind(rethrow))throw;host.EndCatch();Resume(rethrow);}throw new InvalidOperationException("Original allocation cleanup returned");
   }
   uint table=host.ReadWord(layout.NarrowCountVtableSlot);host.WriteWord(A(count,4),1);host.WriteWord(A(count,8),1);host.WriteWord(A(count,12),buffer);host.WriteWord(count,A(table,8));host.WriteWord(A(frame,24),count);
   buffer=host.ReadWord(A(frame,20));try{host.ConstructCString(result,buffer,A(frame,31));}catch(Exception error){if(!host.IsNativeUnwind(error))throw;DestroySharedCount(A(frame,24));Resume(error);}
   count=host.ReadWord(A(frame,24));ReleasePointer(count);return result;
  }
 }
}
