using System;
namespace RouletteDeluxe
{
 // Full XMLMsgVofConfigurationOptions.BuildFromMsg 0949fc00. Frame addresses,
 // tokens, strings, vector fill, incorporation and native destruction are owners.
 public sealed class OriginalConfigurationReplyParser
 {
  public sealed class Host
  {
   public Func<int,uint> FrameAddress;
   public Action<uint,uint,int> CreateCString;
   public Action<uint,uint,uint> Tokenize;
   public Action<uint,uint> InitializeEmptyString,WriteWord,CopyString,EraseAllowedTree,EraseOrderTree,InvokeConfigurationDestructor;
   public Func<uint,uint> ReadWord,StringLength;
   public Func<uint,uint,uint,uint,uint,byte> ParseData;
   public Action<uint,uint,uint,uint> Incorporate,FillInsert;
   public Action<uint> DestroyString,DestroyStringVector,DestroyConfiguration,Free;
   public Func<uint,OriginalNvBackendDestructor.Representation> ReadRepresentation;
   public Func<OriginalNvBackendDestructor.Representation,int> AtomicDecrement;
   public Action<OriginalNvBackendDestructor.Representation,int> DestroyRepresentation;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;private readonly OriginalConfigurationNativeDefault defaults;
  public OriginalConfigurationReplyParser(Host h,OriginalConfigurationNativeDefault defaults)
  {
   if(defaults==null||h==null||h.FrameAddress==null||h.CreateCString==null||h.Tokenize==null||h.InitializeEmptyString==null||h.WriteWord==null||h.CopyString==null||h.EraseAllowedTree==null||h.EraseOrderTree==null||h.InvokeConfigurationDestructor==null||h.ReadWord==null||h.StringLength==null||h.ParseData==null||h.Incorporate==null||h.FillInsert==null||h.DestroyString==null||h.DestroyStringVector==null||h.DestroyConfiguration==null||h.Free==null||h.ReadRepresentation==null||h.AtomicDecrement==null||h.DestroyRepresentation==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual reply/parser/frame/string/token/configuration/vector-fill/tree/heap/destructor/native unwind bindings required");
   this.defaults=defaults;host=new Host{FrameAddress=h.FrameAddress,CreateCString=h.CreateCString,Tokenize=h.Tokenize,InitializeEmptyString=h.InitializeEmptyString,WriteWord=h.WriteWord,CopyString=h.CopyString,EraseAllowedTree=h.EraseAllowedTree,EraseOrderTree=h.EraseOrderTree,InvokeConfigurationDestructor=h.InvokeConfigurationDestructor,ReadWord=h.ReadWord,StringLength=h.StringLength,ParseData=h.ParseData,Incorporate=h.Incorporate,FillInsert=h.FillInsert,DestroyString=h.DestroyString,DestroyStringVector=h.DestroyStringVector,DestroyConfiguration=h.DestroyConfiguration,Free=h.Free,ReadRepresentation=h.ReadRepresentation,AtomicDecrement=h.AtomicDecrement,DestroyRepresentation=h.DestroyRepresentation,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Count(uint end,uint begin){return unchecked((uint)(unchecked((int)(end-begin))>>2)*0x3e0f83e1);}
  private uint Frame(int offset){uint at=host.FrameAddress(offset);if(at==0)throw new InvalidOperationException("Actual reply parser frame admission required");return at;}
  private void Protected(Action call,Action cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  private T Protected<T>(Func<T> call,Action cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup();throw;}}
  private void Release(uint slot,int allocator)
  {
   var rep=host.ReadRepresentation(slot);if(rep==null)throw new InvalidOperationException("Actual reply parser representation admission required");
   // This non-PIC body tests the nonzero immediate080b0f04, not a runtime GOT.
   if(!rep.IsSharedEmpty&&host.AtomicDecrement(rep)<=0)host.DestroyRepresentation(rep,allocator);
  }
  private void DestroyInline(uint temp,int firstAllocator,Action callCleanup)
  {
   uint end=host.ReadWord(Add(temp,104)),begin=host.ReadWord(Add(temp,100));host.WriteWord(temp,0x097681f8);
   if(begin!=end){for(uint at=begin;at!=end;at=Add(at,4))Release(at,firstAllocator);begin=host.ReadWord(Add(temp,100));}
   if(begin!=0){if(callCleanup==null)host.Free(begin);else Protected(()=>host.Free(begin),callCleanup);}
   uint allowed=host.ReadWord(Add(temp,80));if(callCleanup==null)host.EraseAllowedTree(Add(temp,72),allowed);else Protected(()=>host.EraseAllowedTree(Add(temp,72),allowed),callCleanup);
   uint order=host.ReadWord(Add(temp,56));if(callCleanup==null)host.EraseOrderTree(Add(temp,48),order);else Protected(()=>host.EraseOrderTree(Add(temp,48),order),callCleanup);
   int allocator=firstAllocator+1;foreach(uint offset in new[]{36u,28u,20u,12u,8u,4u})Release(Add(temp,offset),allocator++);
   host.WriteWord(temp,0x09695028);
  }
  private void Resize(uint builder,uint target,uint temp,Action cleanup)
  {
   uint end=host.ReadWord(Add(builder,8)),begin=host.ReadWord(Add(builder,4)),count=Count(end,begin);
   if(count<target)Protected(()=>host.FillInsert(Add(builder,4),end,unchecked(target-count),temp),cleanup);
   else if(target<count)
   {
    uint next=Add(begin,unchecked(target*132));for(uint at=next;at!=end;at=Add(at,132)){uint owner=at;Protected(()=>host.InvokeConfigurationDestructor(owner,0),cleanup);}host.WriteWord(Add(builder,8),next);
   }
  }
  public bool Build(uint builder,uint messageString)
  {
   if(builder==0||messageString==0)throw new ArgumentException("Actual reply builder and by-value message string required");
   uint delimiter=Frame(0x13c);host.CreateCString(delimiter,0x09767d0b,0x15b);
   uint tokens=Frame(0x130);Protected(()=>host.Tokenize(tokens,messageString,delimiter),()=>host.DestroyString(delimiter));Release(delimiter,0x15c);
   uint name=Frame(0x140),value=Frame(0x144);host.InitializeEmptyString(name,0x09b482ac);host.InitializeEmptyString(value,0x09b482ac);
   Action common=()=>{host.DestroyString(value);host.DestroyString(name);host.DestroyStringVector(tokens);};
   uint first=Frame(0x28);Protected(()=>defaults.Construct(first),common);
   uint tokenEnd=host.ReadWord(Add(tokens,4)),tokenBegin=host.ReadWord(tokens);uint estimate=unchecked((uint)(unchecked((int)(tokenEnd-tokenBegin))>>2))/5;
   Action firstCleanup=()=>{host.DestroyConfiguration(first);common();};Resize(builder,estimate,first,firstCleanup);DestroyInline(first,0x15d,null);
   uint iterator=Frame(0x14c);host.WriteWord(iterator,host.ReadWord(tokens));uint index=0xffffffff,order=Frame(0x148);
   while(host.ReadWord(iterator)!=host.ReadWord(Add(tokens,4)))
   {
    byte fresh=Protected(()=>host.ParseData(tokens,iterator,name,value,order),common);if(fresh!=0)index=Add(index,1);
    if(host.StringLength(name)!=0)
    {
     uint end=host.ReadWord(Add(builder,8)),begin=host.ReadWord(Add(builder,4));if(Count(end,begin)!=0)
     {
      uint savedOrder=host.ReadWord(order),copiedValue=Frame(0x154);Protected(()=>host.CopyString(copiedValue,value),common);
      uint copiedName=Frame(0x150);Action valueCleanup=()=>{host.DestroyString(copiedValue);common();};Protected(()=>host.CopyString(copiedName,name),valueCleanup);
      Action copiedCleanup=()=>{host.DestroyString(copiedName);valueCleanup();};
      uint owner=Add(host.ReadWord(Add(builder,4)),unchecked(index*132));Protected(()=>host.Incorporate(owner,copiedName,copiedValue,savedOrder),copiedCleanup);Release(copiedName,0x164);Release(copiedValue,0x165);
     }
    }
   }
   uint second=Frame(0xac);Protected(()=>defaults.Construct(second),common);Action secondCleanup=()=>{host.DestroyConfiguration(second);common();};
   Resize(builder,Add(index,1),second,secondCleanup);DestroyInline(second,0x166,secondCleanup);Release(value,0x16d);Release(name,0x16e);
   uint finalEnd=host.ReadWord(Add(tokens,4)),finalBegin=host.ReadWord(tokens);
   if(finalBegin!=finalEnd){for(uint at=finalBegin;at!=finalEnd;at=Add(at,4))Release(at,0x16f);finalBegin=host.ReadWord(tokens);}
   if(finalBegin!=0)Protected(()=>host.Free(finalBegin),secondCleanup);return true;
  }
 }
}
