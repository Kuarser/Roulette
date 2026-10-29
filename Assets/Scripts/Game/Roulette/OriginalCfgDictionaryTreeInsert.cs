using System;
namespace RouletteDeluxe
{
 // Full specialized dictionary hint/unique insertion bodies 09405b50/094059f0.
 public sealed class OriginalCfgDictionaryTreeInsert
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,DecrementMutable,DecrementConst,IncrementConst;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint,int> CompareMemory;
   public Action<uint,uint,uint,uint,uint> InsertNode;
  }
  private readonly Host host;
  public OriginalCfgDictionaryTreeInsert(Host h){if(h==null)throw new ArgumentException("Actual dictionary tree insertion host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Dictionary tree insertion binding required: "+f.Name);f.SetValue(host,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private int Compare(uint a,uint al,uint b,uint bl,uint minimum){int cmp=host.CompareMemory(a,b,minimum);return cmp!=0?cmp:unchecked((int)(al-bl));}
  private uint FinishUnique(uint result,uint tree,uint parent,uint pair,uint frame){uint temporary=A(frame,44);host.InsertNode(temporary,tree,0,parent,pair);uint node=host.ReadWord(temporary);host.WriteWord(result,node);host.WriteByte(A(result,4),1);return result;}
  public uint InsertUnique(uint result,uint tree,uint pair,uint frame)
  {
   uint node=host.ReadWord(A(tree,8)),parent=A(tree,4),candidate=parent,data=0,length=0,entry=0,entryLength=0,minimum=0;bool less=true;
   if(node!=0)
   {
    data=host.ReadWord(pair);length=host.ReadWord(unchecked(data-12));
    while(true){parent=node;entry=host.ReadWord(A(node,16));entryLength=host.ReadWord(unchecked(entry-12));minimum=Math.Min(length,entryLength);less=Compare(data,length,entry,entryLength,minimum)<0;node=host.ReadWord(A(node,less?8u:12u));if(node==0)break;}
    candidate=parent;
   }
   if(less)
   {
    if(host.ReadWord(A(tree,12))==parent)return FinishUnique(result,tree,parent,pair,frame);
    candidate=host.DecrementMutable(parent);data=host.ReadWord(pair);entry=host.ReadWord(A(candidate,16));length=host.ReadWord(unchecked(data-12));entryLength=host.ReadWord(unchecked(entry-12));minimum=Math.Min(length,entryLength);
   }
   if(Compare(entry,entryLength,data,length,minimum)<0)return FinishUnique(result,tree,parent,pair,frame);
   host.WriteWord(result,candidate);host.WriteByte(A(result,4),0);return result;
  }
  private uint Fallback(uint result,uint tree,uint pair,uint frame){uint temporary=A(frame,40);InsertUnique(temporary,tree,pair,unchecked(frame-80));host.WriteWord(result,host.ReadWord(temporary));return result;}
  private uint Insert(uint result,uint tree,uint x,uint parent,uint pair){host.InsertNode(result,tree,x,parent,pair);return result;}
  public uint InsertHint(uint result,uint tree,uint hint,uint pair,uint frame)
  {
   if(hint==A(tree,4))
   {
    if(host.ReadWord(A(tree,20))!=0){uint last=host.ReadWord(A(tree,16)),entry=host.ReadWord(A(last,16)),data=host.ReadWord(pair),entryLength=host.ReadWord(unchecked(entry-12)),length=host.ReadWord(unchecked(data-12));if(Compare(entry,entryLength,data,length,Math.Min(length,entryLength))<0)return Insert(result,tree,0,last,pair);}
    return Fallback(result,tree,pair,frame);
   }
   uint query=host.ReadWord(pair),ql=host.ReadWord(unchecked(query-12)),key=host.ReadWord(A(hint,16)),kl=host.ReadWord(unchecked(key-12)),min=Math.Min(ql,kl);
   if(Compare(query,ql,key,kl,min)<0)
   {
    if(hint==host.ReadWord(A(tree,12)))return Insert(result,tree,hint,hint,pair);
    uint before=host.DecrementConst(hint),beforeKey=host.ReadWord(A(before,16)),beforeLength=host.ReadWord(unchecked(beforeKey-12));
    if(Compare(beforeKey,beforeLength,query,ql,Math.Min(beforeLength,ql))>=0)return Fallback(result,tree,pair,frame);
    if(host.ReadWord(A(before,12))==0)return Insert(result,tree,0,before,pair);
    return Insert(result,tree,hint,hint,pair);
   }
   if(Compare(key,kl,query,ql,min)>=0){host.WriteWord(result,hint);return result;}
   if(hint==host.ReadWord(A(tree,16)))return Insert(result,tree,0,hint,pair);
   uint after=host.IncrementConst(hint),afterKey=host.ReadWord(A(after,16)),afterLength=host.ReadWord(unchecked(afterKey-12));
   if(Compare(query,ql,afterKey,afterLength,Math.Min(ql,afterLength))>=0)return Fallback(result,tree,pair,frame);
   if(host.ReadWord(A(hint,12))==0)return Insert(result,tree,0,hint,pair);
   return Insert(result,tree,after,after,pair);
  }
 }
}
