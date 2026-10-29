using System;
namespace RouletteDeluxe
{
 // Physical regparm tokenise 094997e0. Frame is ESP after prologue.
 public sealed class OriginalConfigurationTokenize
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationTokenize(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, string/vector memory, COW atomics, search/substring/vector imports and native exception token required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint R(uint a){return h.ReadWord(a);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  uint Guard(uint frame,uint vector,bool temporary,Func<uint> action)
  {
   try{return action();}catch(Exception e)
   {
    if(!h.IsNativeUnwind(e))throw;uint token=h.ExceptionToken(e);
    if(temporary)Call(0x80add34,A(frame,0x28));Call(0x81a1a30,vector);Call(0x80aed64,token);
    throw new InvalidOperationException("Native nonreturning Resume returned");
   }
  }
  public uint Tokenize(uint frame,uint vector,uint message,uint delimiters)
  {
   h.WriteWord(vector,0);h.WriteWord(A(vector,4),0);h.WriteWord(A(vector,8),0);
   uint chars=R(delimiters);uint first=Guard(frame,vector,false,()=>Call(0x80b1084,message,chars,0,R(A(chars,0xfffffff4))));
   while(first!=0xffffffff)
   {
    chars=R(delimiters);uint at=first;
    uint last=Guard(frame,vector,false,()=>Call(0x80aa1a4,message,chars,at,R(A(chars,0xfffffff4))));
    uint length=R(A(R(message),0xfffffff4));if(last==0xffffffff)last=length;
    if(first>length){Guard(frame,vector,false,()=>Call(0x80aec94,0x969268c));throw new InvalidOperationException("Native nonreturning out_of_range returned");}
    uint size=unchecked(last-first);Guard(frame,vector,false,()=>Call(0x80aa5c4,A(frame,0x28),message,at,size));
    uint end=R(A(vector,4));
    if(end==R(A(vector,8)))Guard(frame,vector,true,()=>Call(0x9498ed0,vector,end,A(frame,0x28)));
    else
    {
     uint next=0;
     if(end!=0){Guard(frame,vector,true,()=>Call(0x80ac124,end,A(frame,0x28)));next=R(A(vector,4));}
     h.WriteWord(A(vector,4),A(next,4));
    }
    uint data=R(A(frame,0x28)),rep=A(data,0xfffffff4);
    if(rep!=0x9b482a0&&h.AtomicAdd(A(data,0xfffffffc),-1)<=0)Call(0x80ab9f4,rep,A(frame,0x2f));
    first=A(last,1);if(first>=R(A(R(message),0xfffffff4)))break;
   }
   return vector;
  }
 }
}
