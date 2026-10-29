using System;
namespace RouletteDeluxe
{
 // InitHelpText's inlined map operator[] and hinted insertion, before value assignment.
 public sealed class OriginalJurisdictionHelpMapSlot
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Decrement,Increment;
   public Action<uint,uint> WriteWord,CopyString;
   public Func<uint,uint,uint,int> CompareBytes;
   public Action<uint,uint,uint,uint,uint> Insert;
   public Action<uint,uint,uint> InsertUnique;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,uint,Exception> Cleanup;
  }
  private readonly Host host;private readonly uint emptyData;
  public OriginalJurisdictionHelpMapSlot(Host h,uint actualEmptyData)
  {
   if(h==null||h.ReadWord==null||h.Decrement==null||h.Increment==null||h.WriteWord==null||h.CopyString==null||h.CompareBytes==null||h.Insert==null||h.InsertUnique==null||h.DestroyString==null||h.IsNativeUnwind==null||h.Cleanup==null||actualEmptyData==0)throw new ArgumentException("Actual help map memory, strings, iterator, insertion, cleanup and empty data owners required");
   host=new Host{ReadWord=h.ReadWord,Decrement=h.Decrement,Increment=h.Increment,WriteWord=h.WriteWord,CopyString=h.CopyString,CompareBytes=h.CompareBytes,Insert=h.Insert,InsertUnique=h.InsertUnique,DestroyString=h.DestroyString,IsNativeUnwind=h.IsNativeUnwind,Cleanup=h.Cleanup};emptyData=actualEmptyData;
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Protected(uint pad,uint frame,uint ebx,Action call){try{call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Cleanup(pad,frame,ebx,error);throw new InvalidOperationException("Original nonreturning map cleanup binding returned");}}
  private int Compare(uint a,uint b,uint al,uint bl){int result=host.CompareBytes(a,b,Math.Min(al,bl));return result!=0?result:unchecked((int)(al-bl));}
  public uint GetValueSlot(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live InitHelpText frame required");
   uint first=host.ReadWord(A(frame,0x260)),second=host.ReadWord(A(frame,0x260)),node=host.ReadWord(A(first,0x1c)),header=A(second,0x18),ebx=header;
   host.WriteWord(A(frame,0x2c),header);uint query=0,queryLength=0;
   if(node!=0)
   {
    query=host.ReadWord(A(frame,0x230));host.WriteWord(A(frame,0x20),ebx);host.WriteWord(A(frame,0x28),query);queryLength=host.ReadWord(A(query,0xfffffff4));
    while(true){uint data=host.ReadWord(A(node,0x10)),length=host.ReadWord(A(data,0xfffffff4));if(Compare(data,query,length,queryLength)>=0){host.WriteWord(A(frame,0x20),node);node=host.ReadWord(A(node,8));}else node=host.ReadWord(A(node,12));if(node==0)break;}
    ebx=host.ReadWord(A(frame,0x20));
    if(ebx!=host.ReadWord(A(frame,0x2c))){uint data=host.ReadWord(A(ebx,0x10)),length=host.ReadWord(A(data,0xfffffff4)),currentQuery=host.ReadWord(A(frame,0x28));if(Compare(currentQuery,data,queryLength,length)>=0)return A(ebx,0x14);}
   }
   host.WriteWord(A(frame,0x238),emptyData);Protected(0x083686ac,frame,ebx,()=>host.CopyString(A(frame,0x21c),A(frame,0x230)));
   host.WriteWord(A(frame,0x20),A(frame,0x238));Protected(0x083686f5,frame,ebx,()=>host.CopyString(A(frame,0x220),A(frame,0x238)));
   uint machine=host.ReadWord(A(frame,0x260)),currentHeader=host.ReadWord(A(frame,0x2c));host.WriteWord(A(frame,0x28),A(machine,0x14));
   if(currentHeader==ebx)
   {
    machine=host.ReadWord(A(frame,0x260));uint count=host.ReadWord(A(machine,0x28));
    if(count!=0)
    {
     uint rightmost=host.ReadWord(A(machine,0x24)),pairData=host.ReadWord(A(frame,0x21c)),lastData=host.ReadWord(A(rightmost,0x10));ebx=host.ReadWord(A(pairData,0xfffffff4));uint lastLength=host.ReadWord(A(lastData,0xfffffff4));int result=0;
     Protected(0x083685f6,frame,ebx,()=>result=Compare(lastData,pairData,lastLength,ebx));
     if(result<0){ebx=Insert(frame,ebx,0,rightmost);goto inserted;}
    }
    ebx=Unique(frame,ebx);goto inserted;
   }
   {
    uint pairData=host.ReadWord(A(frame,0x21c)),hintData=host.ReadWord(A(ebx,0x10)),pairLength=host.ReadWord(A(pairData,0xfffffff4)),hintLength=host.ReadWord(A(hintData,0xfffffff4)),prefix=Math.Min(hintLength,pairLength);
    host.WriteWord(A(frame,0x14),hintLength);host.WriteWord(A(frame,0x18),prefix);int result=host.CompareBytes(pairData,hintData,prefix);hintLength=host.ReadWord(A(frame,0x14));prefix=host.ReadWord(A(frame,0x18));if(result==0)result=unchecked((int)(pairLength-hintLength));
    if(result<0)
    {
     machine=host.ReadWord(A(frame,0x260));if(host.ReadWord(A(machine,0x20))==ebx){ebx=Insert(frame,ebx,ebx,ebx);goto inserted;}
     uint previous=host.Decrement(ebx),data=host.ReadWord(A(previous,0x10)),length=host.ReadWord(A(data,0xfffffff4));host.WriteWord(A(frame,0x14),previous);result=Compare(data,pairData,length,pairLength);previous=host.ReadWord(A(frame,0x14));
     if(result>=0){ebx=Unique(frame,ebx);goto inserted;}
     if(host.ReadWord(A(previous,12))==0)ebx=Insert(frame,ebx,0,previous);else ebx=Insert(frame,ebx,ebx,ebx);goto inserted;
    }
    host.WriteWord(A(frame,0x14),hintLength);result=host.CompareBytes(hintData,pairData,prefix);hintLength=host.ReadWord(A(frame,0x14));if(result==0)result=unchecked((int)(hintLength-pairLength));
    if(result>=0)goto inserted;
    machine=host.ReadWord(A(frame,0x260));if(host.ReadWord(A(machine,0x24))==ebx){ebx=Insert(frame,ebx,0,ebx);goto inserted;}
    uint next=host.Increment(ebx),nextData=host.ReadWord(A(next,0x10)),nextLength=host.ReadWord(A(nextData,0xfffffff4));host.WriteWord(A(frame,0x14),nextLength);result=Compare(pairData,nextData,pairLength,nextLength);nextLength=host.ReadWord(A(frame,0x14));
    if(result>=0){ebx=Unique(frame,ebx);goto inserted;}
    if(host.ReadWord(A(ebx,12))==0)ebx=Insert(frame,ebx,0,ebx);else ebx=Insert(frame,ebx,next,next);
   }
  inserted:
   host.DestroyString(A(frame,0x220));host.DestroyString(A(frame,0x21c));host.DestroyString(host.ReadWord(A(frame,0x20)));return A(ebx,0x14);
  }
  private uint Insert(uint frame,uint ebx,uint x,uint parent){uint tree=host.ReadWord(A(frame,0x28));Protected(0x083685f6,frame,ebx,()=>host.Insert(A(frame,0x3c),tree,x,parent,A(frame,0x21c)));return host.ReadWord(A(frame,0x3c));}
  private uint Unique(uint frame,uint ebx){uint tree=host.ReadWord(A(frame,0x28));Protected(0x083685f6,frame,ebx,()=>host.InsertUnique(A(frame,0x30),tree,A(frame,0x21c)));return host.ReadWord(A(frame,0x30));}
 }
}
