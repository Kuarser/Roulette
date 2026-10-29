using System;
using System.Text;
namespace RouletteDeluxe
{
 // Complete HostClient.GetConfiguration 094bd090, composed with its recovered
 // full ConfigurationOption copy. All native owners and runtime PIC bindings
 // must be supplied; the original arrays are not managed transport buffers.
 public sealed class OriginalConfigurationHostGet
 {
  public sealed class Host
  {
   public Func<int,object> CreateBuilder;
   public Action<object,uint> ResetBuilderVtable;
   public Func<object,uint,object> InitializeRequestString;
   public Action<object,object> AssignString;
   public Func<int,object,object> CopyString;
   public Func<int,object,uint,int,object> CopyCString;
   public Func<object,uint> StringLength;
   public Func<object,object> StringData;
   public Func<uint,object> AllocateArray;
   public Func<object,uint,uint> ReadArrayWord,ReadBuilderWord,ReadOutputWord;
   public Action<object,uint,uint> WriteArrayWord,WriteBuilderWord,WriteOutputWord;
   public Func<object,uint,byte> ReadArrayByte;
   public Action<object,uint,object,uint> CopyBytes;
   public Func<object,object,uint,object> SendRequest;
   public Action<object,object> BuildFromMsg;
   public Action<object> DeleteArray,DestroyString,DestroyResponseBuilder;
   public Func<object,OriginalNvBackendDestructor.Representation> ReadRepresentation;
   public Func<bool> ThreadActive;
   public Func<OriginalNvBackendDestructor.Representation,int> AtomicDecrement,Decrement;
   public Action<OriginalNvBackendDestructor.Representation,int> DestroyRepresentation;
   public Func<uint,uint> Allocate;
   public Action<uint> Free;
   public Action ThrowBadAlloc,EndCatch,Rethrow;
   public Action<uint,byte[]> Fault;
   public Action<Exception> BeginCatch;
   public Action<uint,uint> ResetConfigurationVtable,InvokeConfigurationDestructor;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;private readonly OriginalConfigurationNativeCopy copy;
  public OriginalConfigurationHostGet(Host h,OriginalConfigurationNativeCopy copy)
  {
   if(copy==null||h==null||h.CreateBuilder==null||h.ResetBuilderVtable==null||h.InitializeRequestString==null||h.AssignString==null||h.CopyString==null||h.CopyCString==null||h.StringLength==null||h.StringData==null||h.AllocateArray==null||h.ReadArrayWord==null||h.ReadBuilderWord==null||h.ReadOutputWord==null||h.WriteArrayWord==null||h.WriteBuilderWord==null||h.WriteOutputWord==null||h.ReadArrayByte==null||h.CopyBytes==null||h.SendRequest==null||h.BuildFromMsg==null||h.DeleteArray==null||h.DestroyString==null||h.DestroyResponseBuilder==null||h.ReadRepresentation==null||h.ThreadActive==null||h.AtomicDecrement==null||h.Decrement==null||h.DestroyRepresentation==null||h.Allocate==null||h.Free==null||h.ThrowBadAlloc==null||h.EndCatch==null||h.Rethrow==null||h.Fault==null||h.BeginCatch==null||h.ResetConfigurationVtable==null||h.InvokeConfigurationDestructor==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual client/return/string/array/parser/PIC/thread/heap/copy/destruction/catch/fault/native unwind bindings required");
   this.copy=copy;host=new Host{CreateBuilder=h.CreateBuilder,ResetBuilderVtable=h.ResetBuilderVtable,InitializeRequestString=h.InitializeRequestString,AssignString=h.AssignString,CopyString=h.CopyString,CopyCString=h.CopyCString,StringLength=h.StringLength,StringData=h.StringData,AllocateArray=h.AllocateArray,ReadArrayWord=h.ReadArrayWord,ReadBuilderWord=h.ReadBuilderWord,ReadOutputWord=h.ReadOutputWord,WriteArrayWord=h.WriteArrayWord,WriteBuilderWord=h.WriteBuilderWord,WriteOutputWord=h.WriteOutputWord,ReadArrayByte=h.ReadArrayByte,CopyBytes=h.CopyBytes,SendRequest=h.SendRequest,BuildFromMsg=h.BuildFromMsg,DeleteArray=h.DeleteArray,DestroyString=h.DestroyString,DestroyResponseBuilder=h.DestroyResponseBuilder,ReadRepresentation=h.ReadRepresentation,ThreadActive=h.ThreadActive,AtomicDecrement=h.AtomicDecrement,Decrement=h.Decrement,DestroyRepresentation=h.DestroyRepresentation,Allocate=h.Allocate,Free=h.Free,ThrowBadAlloc=h.ThrowBadAlloc,EndCatch=h.EndCatch,Rethrow=h.Rethrow,Fault=h.Fault,BeginCatch=h.BeginCatch,ResetConfigurationVtable=h.ResetConfigurationVtable,InvokeConfigurationDestructor=h.InvokeConfigurationDestructor,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static object Known(object value){return value??throw new InvalidOperationException("Actual query builder/string/data/request/reply owner admission required");}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private T Protected<T>(Func<T> call,Action<Exception> cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void Release(object value,int frame)
  {
   var rep=host.ReadRepresentation(value);if(rep==null)throw new InvalidOperationException("Actual query representation admission required");
   if(rep.IsSharedEmpty)return;int old=host.ThreadActive()?host.AtomicDecrement(rep):host.Decrement(rep);if(old<=0)host.DestroyRepresentation(rep,frame);
  }
  private void Deallocate(uint value){if(value!=0)host.Free(value);}
  public object GetConfiguration(object output,object client,object category)
  {
   if(output==null||client==null||category==null)throw new ArgumentException("Actual query return/client/by-value category owners required");
   object requestBuilder=Known(host.CreateBuilder(0x64));host.ResetBuilderVtable(requestBuilder,0x09b12ab0);
   object requestString=Known(host.InitializeRequestString(requestBuilder,0x09b11f28));
   Action<Exception> requestCleanup=ex=>{host.ResetBuilderVtable(requestBuilder,0x09b12ab0);host.DestroyString(requestString);};
   Protected(()=>host.AssignString(requestString,category),requestCleanup);
   // The actual 36-byte BuildMsg body is a copy of builder+4, without XML tags.
   object built=Known(Protected(()=>host.CopyString(0x6c,requestString),requestCleanup));
   Action<Exception> sharedCleanup=ex=>{host.DestroyString(built);requestCleanup(ex);};
   uint total=Add(host.StringLength(built),9);object request=Known(Protected(()=>host.AllocateArray(total),sharedCleanup));
   host.WriteArrayWord(request,4,total);host.WriteArrayWord(request,0,0x00130000);
   object payload=Known(Protected(()=>host.CopyString(0x70,built),sharedCleanup));
   object data=Known(host.StringData(payload));uint room=unchecked(host.ReadArrayWord(request,4)-8),length=host.StringLength(payload);
   if(length>=room)
   {
    Protected(()=>host.Fault(16,Encoding.ASCII.GetBytes(" Insufficient memory for MsgXMLString ")),ex=>{host.DestroyString(payload);sharedCleanup(ex);});
    throw new InvalidOperationException("Native nonreturning query Fault binding returned");
   }
   Protected(()=>host.CopyBytes(request,8,data,Add(length,1)),sharedCleanup);
   Release(payload,0x7c);
   uint requestLength=host.ReadArrayWord(request,4);
   object reply=Known(Protected(()=>host.SendRequest(client,request,requestLength),sharedCleanup));
   object response=Known(host.CreateBuilder(0x54));host.WriteBuilderWord(response,8,0);host.WriteBuilderWord(response,12,0);
   byte first=host.ReadArrayByte(reply,8);host.ResetBuilderVtable(response,0x09b128d0);host.WriteBuilderWord(response,4,0);
   Action<Exception> responseCleanup=ex=>{host.DestroyResponseBuilder(response);sharedCleanup(ex);};
   if(first!=0)
   {
    object text=Known(Protected(()=>host.CopyCString(0x74,reply,8,0x7b),responseCleanup));
    Protected(()=>host.BuildFromMsg(response,text),ex=>{host.DestroyString(text);responseCleanup(ex);});Release(text,0x7d);
   }
   host.DeleteArray(reply);host.DeleteArray(request);
   uint sourceEnd=host.ReadBuilderWord(response,8),source=host.ReadBuilderWord(response,4);
   int quarters=unchecked((int)(sourceEnd-source))>>2;uint count=unchecked((uint)quarters*0x3e0f83e1),bytes=unchecked((uint)(quarters<<2));
   host.WriteOutputWord(output,0,0);host.WriteOutputWord(output,4,0);host.WriteOutputWord(output,8,0);
   uint allocated;
   if(count==0){allocated=0;bytes=0;}
   else
   {
    if(count>0x01f07c1f){Protected(host.ThrowBadAlloc,responseCleanup);throw new InvalidOperationException("Native nonreturning query bad_alloc binding returned");}
    allocated=Protected(()=>host.Allocate(bytes),responseCleanup);sourceEnd=host.ReadBuilderWord(response,8);source=host.ReadBuilderWord(response,4);
   }
   host.WriteOutputWord(output,0,allocated);host.WriteOutputWord(output,4,allocated);host.WriteOutputWord(output,8,Add(allocated,bytes));
   uint current=allocated;
   for(;source!=sourceEnd;source=Add(source,132),current=Add(current,132))
   {
    uint slot=current;
    copy.Copy(slot,source,ex=>{
     host.ResetConfigurationVtable(slot,0x09b122a8);host.BeginCatch(ex);
     Action<Exception> caughtCleanup=next=>{host.EndCatch();Deallocate(host.ReadOutputWord(output,0));responseCleanup(next);};
     for(uint completed=allocated;completed!=slot;completed=Add(completed,132)){uint owner=completed;Protected(()=>host.InvokeConfigurationDestructor(owner,0),caughtCleanup);}
     Protected(host.Rethrow,caughtCleanup);throw new InvalidOperationException("Native nonreturning query rethrow binding returned");
    });
   }
   host.WriteOutputWord(output,4,current);host.ResetBuilderVtable(response,0x09b128d0);
   uint oldEnd=host.ReadBuilderWord(response,8),oldBegin=host.ReadBuilderWord(response,4);
   Action<Exception> sourceDestructorCleanup=ex=>{Deallocate(host.ReadBuilderWord(response,4));host.ResetBuilderVtable(response,0x09b12354);sharedCleanup(ex);};
   if(oldBegin!=oldEnd)
   {
    for(uint at=oldBegin;at!=oldEnd;at=Add(at,132)){uint owner=at;Protected(()=>host.InvokeConfigurationDestructor(owner,0),sourceDestructorCleanup);}
    oldBegin=host.ReadBuilderWord(response,4);
   }
   Deallocate(oldBegin);host.ResetBuilderVtable(response,0x09b12354);Release(built,0x7e);
   host.ResetBuilderVtable(requestBuilder,0x09b12ab0);Release(requestString,0x7f);return output;
  }
 }
}
