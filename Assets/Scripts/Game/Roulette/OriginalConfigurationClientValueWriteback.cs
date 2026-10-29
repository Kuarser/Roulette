using System;
namespace RouletteDeluxe
{
 // GetOptionValue 094b0f70: both inlined stringstream value-writeback loops.
 // Stops at 094b14d8. Outer response/XML/vector cleanup is a separate phase.
 public sealed class OriginalConfigurationClientValueWriteback
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   // 094abaf0 regparm is normalized as destination,begin,end,allocator.
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,Exception> UnwindLandingPad;
   public Action<uint,uint,uint,uint,uint,Exception> UnwindWithRegisters;
  }
  readonly Host h;
  public OriginalConfigurationClientValueWriteback(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.UnwindLandingPad==null)throw new ArgumentException("Physical stream/COW/vector memory, original imports, VTT memory and full-method unwind dispatcher required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,UnwindLandingPad=host.UnwindLandingPad,UnwindWithRegisters=host.UnwindWithRegisters};
  }
  static uint A(uint a,int b){return unchecked(a+(uint)b);}
  uint R(uint at){return h.ReadWord(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Guard(uint frame,uint landing,Action body,uint savedEbx=0,uint savedEsi=0)
  {try{body();}catch(Exception e){if(h.IsNativeUnwind(e)){if(h.UnwindWithRegisters!=null)h.UnwindWithRegisters(landing,frame,savedEbx,savedEsi,A(frame,-0xb8),e);else h.UnwindLandingPad(landing,frame,e);throw new InvalidOperationException("Native nonreturning value-writeback unwind returned");}throw;}}
  void Release(uint cell,uint allocator)
  {uint data=R(cell),rep=A(data,-12);if(rep!=0x9b482a0&&h.AtomicAdd(A(data,-4),-1)<=0)Call(0x80ab9f4,rep,allocator);}
  uint Count(uint vector){return unchecked((uint)(unchecked((int)(R(A(vector,4))-R(vector)))>>4));}
  public void Write(uint frame,bool positionalReplies)
  {
   uint caller=R(A(frame,12));if(Count(caller)==0)return;
   uint selected=A(frame,positionalReplies?-0xb4:-0xa8),replyBegin=R(selected);
   W(A(frame,-0x19c),0);if(!positionalReplies)W(A(frame,-0x1b4),selected);
   do
   {
    uint index=R(A(frame,-0x19c)),offset=unchecked(index*16);
    uint sourceCopy=A(frame,positionalReplies?-0x88:-0x60),callerCopy=A(frame,positionalReplies?-0x84:-0x5c);
    uint extracted=A(frame,positionalReplies?-0x58:-0x4c);
    uint baseStream=A(frame,-0x184),ios=A(frame,-0x150),output=A(frame,-0x17c),buffer=A(frame,-0x178),locale=A(frame,-0x15c),owned=A(frame,-0x154);
    W(A(frame,-0x1b4),A(frame,-0xa8));W(A(frame,-0x1a0),offset);
    uint begin=positionalReplies?replyBegin:R(R(A(frame,-0x1b4)));
    Guard(frame,0x94b2b91,()=>Call(0x80ac124,sourceCopy,A(begin,unchecked((int)offset+4))));
    uint callerBegin=R(R(A(frame,12)));W(A(frame,-0x194),callerBegin);
    Call(0x80ad8c4,ios);
    uint first=R(0x9b49de8),firstIos=R(0x9b49dec);
    h.WriteByte(A(frame,-0xdc),0);W(ios,0x9b49c10);W(A(frame,-0xe0),0);W(baseStream,first);h.WriteByte(A(frame,-0xdb),0);
    W(A(baseStream,unchecked((int)R(A(first,-12)))),firstIos);W(A(frame,-0x180),0);
    W(A(frame,-0xd8),0);W(A(frame,-0xd4),0);W(A(frame,-0xd0),0);W(A(frame,-0xcc),0);
    Guard(frame,positionalReplies?0x94b2388u:0x94b29fdu,()=>Call(0x80b06f4,A(baseStream,unchecked((int)R(A(first,-12)))),0));
    uint second=R(0x9b49df0),secondIos=R(0x9b49df4);W(A(frame,-0x1a8),output);W(output,second);
    uint outputIos=A(output,unchecked((int)R(A(second,-12))));W(outputIos,secondIos);
    Guard(frame,positionalReplies?0x94b2e7bu:0x94b2a04u,()=>Call(0x80b06f4,outputIos,0),first,firstIos);
    uint final=R(0x9b49de4),finalIos=R(0x9b49df8);W(A(frame,-0x198),finalIos);W(A(baseStream,unchecked((int)R(A(final,-12)))),finalIos);
    W(baseStream,0x9b494cc);W(ios,0x9b494f4);W(output,0x9b494e0);W(buffer,0x9b48a28);
    for(int i=-0x174;i<=-0x160;i+=4)W(A(frame,i),0);
    Call(0x80b11f4,locale);W(buffer,0x9b497c8);W(A(frame,-0x158),0x18);W(owned,0x9b482ac);
    Guard(frame,positionalReplies?0x94b2e34u:0x94b23f6u,()=>Call(0x80b06f4,ios,buffer),final,locale);
    uint data=R(sourceCopy);
    Guard(frame,positionalReplies?0x94b2e9cu:0x94b2460u,()=>Call(0x80ad354,R(A(frame,-0x1a8)),data,R(A(data,-12))));
    uint putCurrent=R(A(frame,-0x164));W(extracted,0x9b482ac);
    if(putCurrent==0)
    {
     W(A(frame,positionalReplies?-0x1a4:-0x1a8),extracted);
     Guard(frame,positionalReplies?0x94b2c5au:0x94b2647u,()=>Call(0x80ab384,extracted,owned));
    }
    else
    {
     uint getEnd=R(A(frame,-0x16c)),rangeCopy=A(frame,positionalReplies?(putCurrent>getEnd?-0x50:-0x54):(putCurrent>getEnd?-0x44:-0x48));
     uint allocator=A(frame,positionalReplies?(putCurrent>getEnd?-0x39:-0x3a):(putCurrent>getEnd?-0x29:-0x2a));
     Guard(frame,positionalReplies?(putCurrent>getEnd?0x94b2da6u:0x94b2c8eu):(putCurrent>getEnd?0x94b2472u:0x94b2607u),()=>Call(0x94abaf0,rangeCopy,R(A(frame,-0x168)),putCurrent>getEnd?putCurrent:getEnd,allocator));
     W(A(frame,positionalReplies?-0x1a4:-0x1a8),extracted);
     Guard(frame,positionalReplies?(putCurrent>getEnd?0x94b2de6u:0x94b2c7du):(putCurrent>getEnd?0x94b2487u:0x94b262fu),()=>Call(0x80ab384,extracted,rangeCopy));
     Release(rangeCopy,A(frame,positionalReplies?(putCurrent>getEnd?-0x38:-0x37):(putCurrent>getEnd?-0x28:-0x27)));
    }
    uint target=A(R(A(frame,-0x194)),unchecked((int)R(A(frame,-0x1a0))+4));
    W(A(frame,positionalReplies?-0x1a8:-0x1a0),target);
    Guard(frame,positionalReplies?0x94b2e0bu:0x94b24d7u,()=>Call(0x80ab384,target,R(A(frame,positionalReplies?-0x1a4:-0x1a8))));
    Release(extracted,A(frame,positionalReplies?-0x36:-0x26));
    Guard(frame,positionalReplies?0x94b2e9cu:0x94b2460u,()=>Call(0x80ac124,callerCopy,R(A(frame,positionalReplies?-0x1a8:-0x1a0))));
    W(baseStream,0x9b494cc);W(ios,0x9b494f4);W(output,0x9b494e0);W(buffer,0x9b497c8);
    Release(owned,A(frame,positionalReplies?-0x35:-0x25));W(buffer,0x9b48a28);Call(0x80ad934,locale);
    W(A(baseStream,unchecked((int)R(A(final,-12)))),R(A(frame,-0x198)));
    second=R(0x9b49df0);secondIos=R(0x9b49df4);W(output,second);W(A(output,unchecked((int)R(A(second,-12)))),secondIos);
    first=R(0x9b49de8);firstIos=R(0x9b49dec);W(baseStream,first);W(A(baseStream,unchecked((int)R(A(first,-12)))),firstIos);W(A(frame,-0x180),0);W(ios,0x9b49c10);
    Guard(frame,positionalReplies?0x94b2d21u:0x94b2595u,()=>Call(0x80b0fc4,ios));
    Release(callerCopy,A(frame,positionalReplies?-0x34:-0x24));Release(sourceCopy,A(frame,positionalReplies?-0x33:-0x23));
    W(A(frame,-0x19c),A(R(A(frame,-0x19c)),1));
    if(R(A(frame,-0x19c))>=Count(R(A(frame,12))))break;
    if(positionalReplies)replyBegin=R(A(frame,-0xb4));
   }while(true);
  }
 }
}
