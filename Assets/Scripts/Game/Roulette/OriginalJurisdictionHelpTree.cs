using System;
namespace RouletteDeluxe
{
 // Complete specialized help-tree insertion and pair destruction bodies.
 public sealed class OriginalJurisdictionHelpTree
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New,Decrement;
   public Action<uint,uint> WriteWord,CopyString;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint,int> CompareBytes;
   public Action<byte,uint,uint,uint> Rebalance;
   public Action<uint> DestroyString,Delete;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> BeginCatch,Rethrow,Resume;
   public Action EndCatch;
  }
  private readonly Host host;
  public OriginalJurisdictionHelpTree(Host h)
  {
   if(h==null||h.ReadWord==null||h.New==null||h.Decrement==null||h.WriteWord==null||h.CopyString==null||h.WriteByte==null||h.CompareBytes==null||h.Rebalance==null||h.DestroyString==null||h.Delete==null||h.IsNativeUnwind==null||h.BeginCatch==null||h.Rethrow==null||h.Resume==null||h.EndCatch==null)throw new ArgumentException("Actual help tree memory, string/new/delete owners, rebalance/decrement and native catch/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,New=h.New,Decrement=h.Decrement,WriteWord=h.WriteWord,CopyString=h.CopyString,WriteByte=h.WriteByte,CompareBytes=h.CompareBytes,Rebalance=h.Rebalance,DestroyString=h.DestroyString,Delete=h.Delete,IsNativeUnwind=h.IsNativeUnwind,BeginCatch=h.BeginCatch,Rethrow=h.Rethrow,Resume=h.Resume,EndCatch=h.EndCatch};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private int Compare(uint a,uint b,uint al,uint bl)
  {int result=host.CompareBytes(a,b,Math.Min(al,bl));return result!=0?result:unchecked((int)(al-bl));}
  private void CopyFailure(uint node,uint key,Exception error,bool keyConstructed)
  {
   if(keyConstructed)host.DestroyString(key);
   host.BeginCatch(error);host.Delete(node);
   try{host.Rethrow(error);}catch(Exception rethrown){if(!host.IsNativeUnwind(rethrown))throw;host.EndCatch();host.Resume(rethrown);throw new InvalidOperationException("Original nonreturning unwind resume binding returned");}
   throw new InvalidOperationException("Original nonreturning rethrow binding returned");
  }
  public uint Insert(uint destination,uint tree,uint x,uint parent,uint pair)
  {
   byte left=1;
   if(x==0&&parent!=Add(tree,4)){uint a=host.ReadWord(pair),b=host.ReadWord(Add(parent,0x10)),al=host.ReadWord(Add(a,0xfffffff4)),bl=host.ReadWord(Add(b,0xfffffff4));left=(byte)(Compare(a,b,al,bl)<0?1:0);}
   uint node=host.New(24),key=Add(node,0x10);
   if(key!=0)
   {
    try{host.CopyString(key,pair);}catch(Exception error){if(!host.IsNativeUnwind(error))throw;CopyFailure(node,key,error,false);throw;}
    try{host.CopyString(Add(node,0x14),Add(pair,4));}catch(Exception error){if(!host.IsNativeUnwind(error))throw;CopyFailure(node,key,error,true);throw;}
   }
   host.Rebalance(left,node,parent,Add(tree,4));host.WriteWord(destination,node);uint count=host.ReadWord(Add(tree,0x14));host.WriteWord(Add(tree,0x14),Add(count,1));return destination;
  }
  public uint InsertUnique(uint destination,uint tree,uint pair,uint resultScratch)
  {
   uint node=host.ReadWord(Add(tree,8)),parent=Add(tree,4),candidate=parent;bool less=true;uint query=0,queryLength=0,key=0,keyLength=0,prefix=0;
   if(node!=0)
   {
    query=host.ReadWord(pair);queryLength=host.ReadWord(Add(query,0xfffffff4));
    while(true){parent=node;key=host.ReadWord(Add(node,0x10));keyLength=host.ReadWord(Add(key,0xfffffff4));prefix=Math.Min(queryLength,keyLength);less=Compare(query,key,queryLength,keyLength)<0;node=host.ReadWord(Add(node,less?8u:12u));if(node==0)break;}
    candidate=parent;
   }
   if(less)
   {
    if(host.ReadWord(Add(tree,12))!=parent){candidate=host.Decrement(parent);query=host.ReadWord(pair);key=host.ReadWord(Add(candidate,0x10));queryLength=host.ReadWord(Add(query,0xfffffff4));keyLength=host.ReadWord(Add(key,0xfffffff4));prefix=Math.Min(queryLength,keyLength);}
    else return InsertResult(destination,tree,parent,pair,resultScratch);
   }
   int comparison=host.CompareBytes(key,query,prefix);if(comparison==0)comparison=unchecked((int)(keyLength-queryLength));
   if(comparison<0)return InsertResult(destination,tree,parent,pair,resultScratch);
   host.WriteWord(destination,candidate);host.WriteByte(Add(destination,4),0);return destination;
  }
  private uint InsertResult(uint destination,uint tree,uint parent,uint pair,uint scratch)
  {Insert(scratch,tree,0,parent,pair);uint node=host.ReadWord(scratch);host.WriteWord(destination,node);host.WriteByte(Add(destination,4),1);return destination;}
  public void DestroyPair(uint pair){host.DestroyString(Add(pair,4));host.DestroyString(pair);}
 }
}
