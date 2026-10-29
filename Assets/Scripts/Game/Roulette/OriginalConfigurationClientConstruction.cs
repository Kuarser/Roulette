using System;
namespace RouletteDeluxe
{
 // Full Game/Host C1. Caller supplies the original-layout 40-byte temporary deque.
 public sealed class OriginalConfigurationClientConstruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,ReadHalf,ReadByte;
   public Action<uint,uint> WriteWord,WriteHalf,WriteByte,InitializeDeque;
   public Action<uint> DestroyDeque,Free;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host host;
  public OriginalConfigurationClientConstruction(Host h)
  {
   if(h==null||h.ReadWord==null||h.ReadHalf==null||h.ReadByte==null||h.WriteWord==null||h.WriteHalf==null||h.WriteByte==null||h.InitializeDeque==null||h.DestroyDeque==null||h.Free==null||h.IsNativeUnwind==null||h.Resume==null)throw new ArgumentException("Actual client memory/packet/deque/heap/native unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,ReadHalf=h.ReadHalf,ReadByte=h.ReadByte,WriteWord=h.WriteWord,WriteHalf=h.WriteHalf,WriteByte=h.WriteByte,InitializeDeque=h.InitializeDeque,DestroyDeque=h.DestroyDeque,Free=h.Free,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private static uint Scaled(uint a,uint b){return unchecked((uint)((int)unchecked(a-b)>>2)*0x372b7e65);}
  private void Base(uint owner,bool hostClient){host.WriteWord(owner,hostClient?Add(host.ReadWord(0x09b120e0),8):0x096a93e0);}
  private void Initialize(uint owner,uint target,uint count,uint temp,bool hostClient,bool destroyTemp)
  {
   try{host.InitializeDeque(target,count);}catch(Exception ex){if(host.IsNativeUnwind(ex)){if(destroyTemp)host.DestroyDeque(temp);Base(owner,hostClient);host.Resume(ex);throw new InvalidOperationException("Native nonreturning client construction resume binding returned");}throw;}
  }
  private void CopyPacket(uint destination,uint source)
  {
   if(destination==0)return;uint dst=destination,src=source,left=0xdb4;
   if((dst&1)!=0){uint b=host.ReadByte(src);host.WriteByte(dst,b);dst=Add(dst,1);src=Add(src,1);left--;}
   if((dst&2)!=0){uint w=host.ReadHalf(src);host.WriteHalf(dst,w);dst=Add(dst,2);src=Add(src,2);left-=2;}
   uint words=left>>2;for(uint i=0;i<words;++i){uint w=host.ReadWord(src);host.WriteWord(dst,w);src=Add(src,4);dst=Add(dst,4);}
   uint tail=0;if((left&2)!=0){uint w=host.ReadHalf(src);host.WriteHalf(dst,w);tail=2;}
   if((left&1)!=0){uint b=host.ReadByte(Add(src,tail));host.WriteByte(Add(dst,tail),b);}
  }
  private void CopyAndRelease(uint member,uint temp,bool hostClient)
  {
   uint dstNode,dst,dstEnd,end,src,srcEnd,srcNode;
   if(hostClient){dstEnd=host.ReadWord(Add(member,0x10));end=host.ReadWord(Add(temp,0x18));dst=host.ReadWord(Add(member,8));dstNode=host.ReadWord(Add(member,0x14));src=host.ReadWord(Add(temp,8));srcEnd=host.ReadWord(Add(temp,0x10));srcNode=host.ReadWord(Add(temp,0x14));}
   else{dstNode=host.ReadWord(Add(member,0x14));dst=host.ReadWord(Add(member,8));dstEnd=host.ReadWord(Add(member,0x10));end=host.ReadWord(Add(temp,0x18));src=host.ReadWord(Add(temp,8));srcEnd=host.ReadWord(Add(temp,0x10));srcNode=host.ReadWord(Add(temp,0x14));}
   uint originalNode=srcNode;
   while(src!=end)
   {
    CopyPacket(dst,src);src=Add(src,0xdb4);dst=Add(dst,0xdb4);
    if(src==srcEnd){src=host.ReadWord(Add(srcNode,4));srcEnd=Add(src,0xdb4);srcNode=Add(srcNode,4);}
    if(dst==dstEnd){dst=host.ReadWord(Add(dstNode,4));dstNode=Add(dstNode,4);dstEnd=Add(dst,0xdb4);}
   }
   uint map=host.ReadWord(temp);if(map==0)return;uint limit=Add(host.ReadWord(Add(temp,0x24)),4),at=originalNode;
   for(;at<limit;at=Add(at,4))host.Free(host.ReadWord(at));if(originalNode<limit)map=host.ReadWord(temp);host.Free(map);
  }
  public void Construct(uint owner,uint temp,bool hostClient)
  {
   for(uint o=0;o<12;o+=4)host.WriteWord(Add(temp,o),0);
   if(hostClient){host.WriteWord(Add(owner,4),0);uint vt=host.ReadWord(0x09b122f8);host.WriteWord(Add(owner,8),0);host.WriteWord(Add(owner,0xc),0);host.WriteWord(Add(owner,0x10),0);host.WriteWord(owner,Add(vt,8));host.WriteWord(Add(owner,0x14),0);}
   else{host.WriteWord(owner,0x09768488);host.WriteWord(Add(owner,4),0);host.WriteWord(Add(owner,8),0);}
   for(uint o=12;o<40;o+=4)host.WriteWord(Add(temp,o),0);
   Initialize(owner,temp,0,temp,hostClient,false);
   uint a=host.ReadWord(Add(temp,0x18)),b=host.ReadWord(Add(temp,0x1c)),c=host.ReadWord(Add(temp,0x24)),d=host.ReadWord(Add(temp,0x14));
   uint count=unchecked(Scaled(a,b)+(uint)((int)unchecked(c-d)>>2)-1);
   uint member=Add(owner,hostClient?0x18u:0xcu);
   if(hostClient){host.WriteWord(member,0);host.WriteWord(Add(member,4),0);}
   uint e=host.ReadWord(Add(temp,0x10)),f=host.ReadWord(Add(temp,8));count=Add(count,Scaled(e,f));
   for(uint o=hostClient?8u:0u;o<40;o+=4)host.WriteWord(Add(member,o),0);
   Initialize(owner,member,count,temp,hostClient,true);CopyAndRelease(member,temp,hostClient);
   if(!hostClient)foreach(uint o in new uint[]{0x34,0x38,0x3c,0x40,0x44,0x48,0x4c,0x50,0x54,0x64,0x74})host.WriteWord(Add(owner,o),0);
  }
 }
}
