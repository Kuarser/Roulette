using System;
namespace RouletteDeluxe
{
 // Local callback registration 094ab710 and ConfigModel<string>::changeNotify
 // 08342510. Caller owns native stack scratch; imported COW/memcmp/send and
 // function-manager operations are mandatory live bindings.
 public sealed class OriginalConfigurationModelNotifications
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  readonly Host h;
  public OriginalConfigurationModelNotifications(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.Invoke==null||host.IsNativeUnwind==null||host.Resume==null)throw new ArgumentException("Physical notification memory, imported operations and native unwind required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,Resume=host.Resume};
  }
  static uint A(uint a,uint b){return unchecked(a+b);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  void Destroy(uint owner){Call(0x80add34,owner);}
  void Guard(Action body,Action<Exception> cleanup)
  {try{body();}catch(Exception e){if(h.IsNativeUnwind(e)){cleanup(e);h.Resume(e);throw new InvalidOperationException("Native nonreturning notification Resume returned");}throw;}}
  // Frame is ESP after push EBX/sub 28: owner +30, callback owner +34,
  // callback method +38, and the 8-byte request at +18.
  public uint RegisterLocal(uint frame)
  {
   uint owner=h.ReadWord(A(frame,0x30)),packet=A(frame,0x18);
   h.WriteWord(packet,0x30000);h.WriteWord(A(packet,4),8);
   uint table=h.ReadWord(owner),send=h.ReadWord(A(table,0x18));Call(send,owner,packet,8);
   uint callback=h.ReadWord(A(frame,0x38));h.WriteWord(A(owner,0x34),0);h.WriteWord(A(owner,0x38),callback);
   uint manager=h.ReadWord(A(owner,0x54));
   if(manager!=0){uint release=h.ReadWord(manager);if(release!=0){uint storage=A(owner,0x58);Call(release,storage,storage,2);}h.WriteWord(A(owner,0x54),0);}
   h.WriteWord(A(owner,0x3c),h.ReadWord(A(frame,0x34)));return 0;
  }
  // Frame is ESP after four saved registers/sub 2c: model +40,
  // option vector +44, temporary name/category/value copies +14/+10/+18/+1c.
  public void ChangeNotify(uint frame)
  {
   uint vector=h.ReadWord(A(frame,0x44)),model=h.ReadWord(A(frame,0x40)),current=h.ReadWord(vector);
   if(current==h.ReadWord(A(vector,4)))return;
   uint name=A(frame,0x14),category=A(frame,0x10),value=A(frame,0x18),copy=A(frame,0x1c);
   do
   {
    Call(0x94b5330,name,current);
    uint modelName=h.ReadWord(model),optionName=h.ReadWord(name),length=h.ReadWord(A(optionName,0xfffffff4));
    bool nameMatches=length==h.ReadWord(A(modelName,0xfffffff4));
    if(nameMatches)nameMatches=Call(0x80aafa4,optionName,modelName,length)==0;
    if(!nameMatches)Destroy(name);
    else
    {
     Guard(()=>Call(0x94b5360,category,current),e=>Destroy(name));
     uint modelCategory=h.ReadWord(A(model,4)),optionCategory=h.ReadWord(category),categoryLength=h.ReadWord(A(optionCategory,0xfffffff4));
     bool categoryMatches=categoryLength==h.ReadWord(A(modelCategory,0xfffffff4));
     if(categoryMatches)categoryMatches=Call(0x80aafa4,optionCategory,modelCategory,categoryLength)==0;
     Destroy(category);Destroy(name);
     if(categoryMatches)
     {
      h.WriteWord(value,0x9b482ac);
      Guard(()=>{Call(0x80ab384,value,A(current,4));Call(0x80ac124,copy,value);},e=>Destroy(value));
      Guard(()=>Call(0x80ab384,A(model,8),copy),e=>{Destroy(copy);Destroy(value);});
      Destroy(copy);Destroy(value);
     }
    }
    current=A(current,16);vector=h.ReadWord(A(frame,0x44));
   }while(current!=h.ReadWord(A(vector,4)));
  }
 }
}
