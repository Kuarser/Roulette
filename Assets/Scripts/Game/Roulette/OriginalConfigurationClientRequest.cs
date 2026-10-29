using System;
namespace RouletteDeluxe
{
 // Full SendRequest 094aef40 control. Frames, live IPC/server/client dispatch,
 // split/recursive requests, packet deque operations and native unwind are
 // required ports. Recursive dispatch must allocate a separate original frame.
 public sealed class OriginalConfigurationClientRequest
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,ReadHalf;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,WriteHalf;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  readonly Host h;
  public OriginalConfigurationClientRequest(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadHalf==null||host.ReadByte==null||host.WriteWord==null||host.WriteHalf==null||host.WriteByte==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Physical packet/frame memory, live transports/deques/split/recursive dispatch and unwind required");
   h=new Host{ReadWord=host.ReadWord,ReadHalf=host.ReadHalf,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteHalf=host.WriteHalf,WriteByte=host.WriteByte,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint R(uint a){return h.ReadWord(a);}
  void W(uint a,uint v){h.WriteWord(a,v);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Free(uint owner){Call(0x80aabe4,owner);}
  void DeleteArray(uint owner){Call(0x80ae7f4,owner);}
  void Guard(Action body,Action<Exception> cleanup)
  {try{body();}catch(Exception e){if(h.IsNativeUnwind(e)){cleanup(e);h.Resume(e);throw new InvalidOperationException("Native nonreturning request Resume returned");}throw;}}
  static uint Scaled(uint first,uint last){return unchecked((uint)(unchecked((int)(last-first))>>2)*0x372b7e65);}
  uint DequeCount(uint d)
  {
   uint finish=Scaled(R(A(d,0x1c)),R(A(d,0x18))),nodes=unchecked((uint)(unchecked((int)(R(A(d,0x24))-R(A(d,0x14))))>>2)-1),start=Scaled(R(A(d,8)),R(A(d,0x10)));
   return unchecked(finish+nodes+start);
  }
  void Zero(uint at,uint count){for(uint i=0;i<count;i+=4)W(A(at,i),0);}
  void Copy(uint destination,uint source,uint count)
  {
   uint dst=destination,src=source,left=count;
   if((dst&1)!=0){h.WriteByte(dst,h.ReadByte(src));dst=A(dst,1);src=A(src,1);--left;}
   if((dst&2)!=0){h.WriteHalf(dst,h.ReadHalf(src));dst=A(dst,2);src=A(src,2);left-=2;}
   for(uint i=0;i<(left>>2);++i){W(dst,R(src));dst=A(dst,4);src=A(src,4);}
   uint tail=0;if((left&2)!=0){h.WriteHalf(dst,h.ReadHalf(src));tail=2;}if((left&1)!=0)h.WriteByte(A(dst,tail),h.ReadByte(A(src,tail)));
  }
  void DestroyMap(uint owner)
  {
   uint map=R(owner);if(map==0)return;uint limit=A(R(A(owner,0x24)),4),first=R(A(owner,0x14));
   for(uint at=first;at<limit;at=A(at,4))Free(R(at));if(first<limit)map=R(owner);Free(map);
  }
  void CopyDeque(uint destination,uint source)
  {
   uint dst=R(A(destination,8)),dstEnd=R(A(destination,0x10)),dstNode=R(A(destination,0x14)),src=R(A(source,8)),srcEnd=R(A(source,0x10)),srcNode=R(A(source,0x14)),finish=R(A(source,0x18));
   while(src!=finish)
   {
    if(dst!=0)Copy(dst,src,0xdb4);src=A(src,0xdb4);dst=A(dst,0xdb4);
    if(src==srcEnd){srcNode=A(srcNode,4);src=R(srcNode);srcEnd=A(src,0xdb4);}
    if(dst==dstEnd){dstNode=A(dstNode,4);dst=R(dstNode);dstEnd=A(dst,0xdb4);}
   }
  }
  // MakePackets 094ae9e0. Frame after four saves/sub dec; vector +e04,
  // input bytes +e08, byte count +e0c. Original packet scratch +2c.
  public void MakePackets(uint frame)
  {
   uint vector=R(A(frame,0xe04)),packet=A(frame,0x2c);W(packet,0);W(A(packet,4),0);
   uint begin=R(vector),end=R(A(vector,4));if(Scaled(begin,end)!=0)W(A(vector,4),begin);
   uint total=R(A(frame,0xe0c)),capacity=unchecked(total/0xdac+(total%0xdac!=0?1u:0u));Call(0x94b3de0,vector,capacity);
   uint remaining=R(A(frame,0xe0c));if(remaining==0)return;uint offset=0;
   do
   {
    W(packet,0x10000);uint packetLength=unchecked(remaining+8);if(packetLength>0xdb4)packetLength=0xdb4;W(A(packet,4),packetLength);Zero(A(packet,8),0xdac);
    uint size=remaining>0xdac?0xdacu:remaining;Call(0x80ae3e4,A(packet,8),A(R(A(frame,0xe08)),offset),size);offset=A(offset,size);
    if(offset>R(A(frame,0xe0c))){Call(0x9483bf0,0x97683c7,0x97682c8,0x4db,0x9768600);throw new InvalidOperationException("Native nonreturning split offset assertion returned");}
    vector=R(A(frame,0xe04));end=R(A(vector,4));
    if(end==R(A(vector,8)))Call(0x94b3f40,vector,end,packet);
    else{uint next=0;if(end!=0){Copy(end,packet,0xdb4);next=R(A(vector,4));}W(A(vector,4),A(next,0xdb4));}
    remaining=unchecked(remaining-size);
   }while(remaining!=0);
   if(offset!=R(A(frame,0xe0c))){Call(0x9483bf0,0x97683e3,0x97682c8,0x4e1,0x9768600);throw new InvalidOperationException("Native nonreturning split final offset assertion returned");}
  }
  // Frame is ESP after four saved registers/sub e6c. Client +e80, request
  // +e84, length +e88; temporary packet +48; deques +dfc/+e24; split vector
  // +e4c; recursive eight-byte control request +e58.
  public uint Send(uint frame)
  {
   uint length=R(A(frame,0xe88)),owner=R(A(frame,0xe80)),response=Call(0x80aa1b4,0xdb5);
   if(length<=0xdb4)
   {
    uint connection=R(A(owner,4));
    if(connection!=0){uint table=R(connection),method=R(A(table,8));Call(method,connection,R(A(frame,0xe84)),length,response,0xdb5);}
    else
    {
     uint server=R(A(owner,8));if(server!=0){uint table=R(server),method=R(A(table,0x18));Call(method,server,owner,0xffffffff,R(A(frame,0xe84)),length,response,0xdb5);}
    }
   }
   else
   {
    uint table=R(owner),split=A(frame,0xe4c);Zero(split,12);
    Guard(()=>
    {
     Call(R(A(table,0x1c)),owner,split,R(A(frame,0xe84)),length);
     uint begin=R(split),count=Scaled(begin,R(A(split,4)));
     if(count!=0)
     {
      uint index=0;
      do
      {
       uint packet=A(begin,unchecked(index*0xdb4)),vt=R(owner),packetLength=R(A(packet,4));Call(R(A(vt,0x18)),owner,packet,packetLength);
       begin=R(split);++index;count=Scaled(begin,R(A(split,4)));
      }while(index<count);
     }
     uint control=A(frame,0xe58);W(control,0x10010);W(A(control,4),8);if(response!=0)DeleteArray(response);response=Call(0x94aef40,owner,control,8);
    },e=>{uint allocated=R(split);if(allocated!=0)Free(allocated);});
    uint splitMemory=R(split);if(splitMemory!=0)Free(splitMemory);
   }
   if(R(response)!=0x10000)return response;
   if(R(R(A(frame,0xe84)))==0x10020)return response;
   uint initial=A(frame,0xe24),queue=A(frame,0xdfc);Zero(initial,40);Call(0x8e0a440,initial,0);uint countInitial=DequeCount(initial);Zero(queue,40);
   Guard(()=>Call(0x8e0a440,queue,countInitial),e=>Call(0x834f100,initial));CopyDeque(queue,initial);DestroyMap(initial);
   uint poll=A(frame,0xe58),temporary=A(frame,0x48);W(poll,0x10020);W(A(poll,4),8);
   Action<Exception> queueCleanup=e=>Call(0x834f100,queue);
   while(true)
   {
    if(R(response)!=0x10000){Call(0x9483bf0,0x9768320,0x97682c8,0x4a9,0x97685a0);throw new InvalidOperationException("Native nonreturning packet assertion returned");}
    uint responseLength=R(A(response,4));if(responseLength<=8)break;
    uint packet=response;
    if(responseLength<0xdb4){Zero(temporary,0xdb4);Call(0x80ae3e4,temporary,response,R(A(response,4)));packet=temporary;}
    uint finish=R(A(queue,0x18)),end=R(A(queue,0x20));
    if(finish==A(end,unchecked(0u-0xdb4)))Guard(()=>Call(0x8e0a560,queue,packet),queueCleanup);
    else{if(finish!=0)Copy(finish,packet,0xdb4);W(A(queue,0x18),A(finish,0xdb4));}
    DeleteArray(response);Guard(()=>response=Call(0x94aef40,owner,poll,8),queueCleanup);
   }
   uint size=unchecked(DequeCount(queue)*0xdac),result=0;
   Guard(()=>{DeleteArray(response);result=Call(0x80aa1b4,size);},queueCleanup);
   uint offset=0,capacity=unchecked(size-0xdac);
   while(DequeCount(queue)!=0&&offset<=capacity)
    {
     uint begin=R(A(queue,8));Copy(A(result,offset),A(begin,8),0xdac);offset=A(offset,0xdac);
     if(begin==A(R(A(queue,0x10)),unchecked(0u-0xdb4)))
     {
      Free(R(A(queue,0xc)));uint node=R(A(queue,0x14)),next=R(A(node,4));W(A(queue,8),next);W(A(queue,0x10),A(next,0xdb4));W(A(queue,0xc),next);W(A(queue,0x14),A(node,4));
     }
     else W(A(queue,8),A(begin,0xdb4));
   }
   Guard(()=>DestroyMap(queue),queueCleanup);return result;
  }
 }
}
