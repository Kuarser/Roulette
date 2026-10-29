using System;
namespace RouletteDeluxe
{
 // Original BuildDocument control. Xerces, streams and lower resource helpers are required ports.
 public sealed class OriginalCfgParserBuildDocument
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionAddress,ExceptionSelector;
  }
  [Serializable] public sealed class Layout
  {
   public uint LocaleSlot,WideCountVtableSlot,MemoryManagerSlot,ValidateIfSchemaSlot,ErrorHandlerVtableSlot,BaseErrorHandlerVtableSlot,CoutSlot,CerrSlot;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgParserBuildDocument(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("BuildDocument bindings and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("BuildDocument binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("BuildDocument relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private uint Call(uint method,params uint[] args){return host.Invoke(method,args);}
  private bool Protected(uint method,uint[] args,Action<Exception> unwind,out uint result)
  {
   try{result=Call(method,args);return true;}catch(Exception e){if(!host.IsNativeUnwind(e))throw;unwind(e);result=0;return false;}
  }
  private void Required(uint method,uint[] args,Action<Exception> unwind){uint ignored;Protected(method,args,unwind,out ignored);}
  private void Resume(Exception e){Call(0x80aed64,host.ExceptionAddress(e));throw new InvalidOperationException("Original nonreturning BuildDocument Resume returned");}
  private void Release(uint count)
  {
   if(count==0||host.AtomicAdd(A(count,4),-1)!=1)return;uint table=host.ReadWord(count);Call(host.ReadWord(A(table,8)),count);if(host.AtomicAdd(A(count,8),-1)!=1)return;table=host.ReadWord(count);Call(host.ReadWord(A(table,12)),count);
  }
  private void EndCatch(Action<Exception> unwind){Required(0x80b1144,new uint[0],unwind);}
  private void LogXml(Exception error,uint temporary,bool initialization,Action<Exception> unwind)
  {
   uint caught=Call(0x80adff4,host.ExceptionAddress(error));uint message=host.ReadWord(A(caught,16));uint ignored;
   if(!Protected(0x9401c60,new[]{temporary,message},e=>{Call(0x80b1144);unwind(e);},out ignored))return;
   Action<Exception> logUnwind=e=>{Call(0x80add34,temporary);Call(0x80b1144);unwind(e);};uint stream=host.ReadWord(initialization?layout.CoutSlot:layout.CerrSlot);
   uint prefix=initialization?0x975cf50u:0x975ceeau;
   if(!Protected(0x80b0c04,new[]{stream,prefix},logUnwind,out stream))return;
   if(!Protected(0x80adde4,new[]{stream,temporary},logUnwind,out stream))return;
   if(!Protected(0x80b0c04,new[]{stream,0x96d5861u},logUnwind,out stream))return;
   Call(0x80add34,temporary);if(initialization)Call(0x80b1144);else EndCatch(unwind);
  }
  private void LogParse(Exception error,uint name,uint temporary,Action<Exception> unwind)
  {
   uint selector=host.ExceptionSelector(error);
   if(selector==1){LogXml(error,temporary,false,unwind);return;}
   uint caught=Call(0x80adff4,host.ExceptionAddress(error));Action<Exception> logUnwind=e=>{Call(0x80b1144);unwind(e);};uint code=selector==2?host.ReadWord(A(caught,4)):0;uint stream=host.ReadWord(selector==2?layout.CerrSlot:layout.CoutSlot);
   uint ignored;
   if(selector!=2){Required(0x80b0c04,new[]{stream,0x975cf38u},logUnwind);EndCatch(unwind);return;}
   if(!Protected(0x80b0c04,new[]{stream,0x975cf02u},logUnwind,out stream))return;
   if(!Protected(0x80adde4,new[]{stream,name},logUnwind,out stream))return;
   if(!Protected(0x80b0c04,new[]{stream,0x96b90d6u},logUnwind,out stream))return;
   if(!Protected(0x80b0c04,new[]{stream,0x975cf1fu},logUnwind,out stream))return;
   if(!Protected(0x80ac834,new[]{stream,code},logUnwind,out stream))return;
   if(!Protected(0x80b0c04,new[]{stream,0x96d5861u},logUnwind,out stream))return;
   if(!Protected(0x80ab984,new[]{stream},logUnwind,out ignored))return;EndCatch(unwind);
  }
  private void Build(uint owner,uint text,uint name,uint frame,bool memoryInput)
  {
   uint countSlot=A(frame,memoryInput?64u:48u),featureSlot=A(frame,memoryInput?60u:44u),handler=A(frame,memoryInput?72u:56u),wrapper=A(frame,48);
   Action<Exception> initUnwind=e=>{if(host.ExceptionSelector(e)==1)LogXml(e,A(frame,memoryInput?68u:52u),true,Resume);else Resume(e);};uint ignored;
   uint locale=host.ReadWord(layout.LocaleSlot);if(!Protected(0x80ac464,new[]{locale,0u,0u,0u,0u},initUnwind,out ignored))return;
   uint feature=Call(0x80aa3d4,0x970f9c5);host.WriteWord(countSlot,0);host.WriteWord(featureSlot,feature);uint count;
   Action<Exception> allocationUnwind=e=>
   {
    Call(0x80adff4,host.ExceptionAddress(e));if(feature!=0)Call(0x80ae7f4,feature);
    Required(0x80b1284,new uint[0],rethrow=>{Call(0x80b1144);Resume(rethrow);});throw new InvalidOperationException("Original nonreturning BuildDocument Rethrow returned");
   };
   if(!Protected(0x80aad84,new[]{20u},allocationUnwind,out count))return;
   uint table=host.ReadWord(layout.WideCountVtableSlot);host.WriteWord(A(count,4),1);host.WriteWord(A(count,8),1);host.WriteWord(A(count,12),feature);host.WriteWord(count,A(table,8));host.WriteWord(countSlot,count);
   Action<Exception> countUnwind=e=>{Call(0x9400c00,countSlot);Resume(e);};uint implementation;
   feature=host.ReadWord(featureSlot);if(!Protected(0x80b1344,new[]{feature},countUnwind,out implementation))return;
   uint managerAddress=host.ReadWord(layout.MemoryManagerSlot);table=host.ReadWord(implementation);uint manager=host.ReadWord(managerAddress),parser;
   if(!Protected(host.ReadWord(A(table,8)),new[]{implementation,1u,0u,manager,0u},countUnwind,out parser))return;
   table=host.ReadWord(parser);uint validate=host.ReadWord(layout.ValidateIfSchemaSlot);host.WriteWord(A(owner,4),parser);
   Required(host.ReadWord(A(table,44)),new[]{parser,validate,1u},countUnwind);
   if(memoryInput)
   {
    uint bytes=host.ReadWord(text),systemId=host.ReadWord(name),length=host.ReadWord(A(bytes,0xfffffff4)),input;
    if(!Protected(0x80acca4,new[]{36u},countUnwind,out input))return;
    manager=host.ReadWord(managerAddress);
    Required(0x80b1ad4,new[]{input,bytes,length,systemId,0u,manager},e=>{Call(0x80abc14,input);countUnwind(e);});
    manager=host.ReadWord(managerAddress);Required(0x80aa8d4,new[]{wrapper,input,0u,manager},countUnwind);
   }
   table=host.ReadWord(layout.ErrorHandlerVtableSlot);host.WriteWord(handler,A(table,8));
   Action<Exception> handlerUnwind=e=>
   {
    uint baseTable=host.ReadWord(layout.BaseErrorHandlerVtableSlot);host.WriteWord(handler,A(baseTable,8));if(memoryInput)Call(0x80ab044,wrapper);countUnwind(e);
   };
   parser=host.ReadWord(A(owner,4));table=host.ReadWord(parser);Required(host.ReadWord(A(table,32)),new[]{parser,handler},handlerUnwind);
   parser=host.ReadWord(A(owner,4));uint inputArgument=memoryInput?wrapper:host.ReadWord(text);table=host.ReadWord(parser);uint document;
   bool parsed=Protected(host.ReadWord(A(table,memoryInput?56u:64u)),new[]{parser,inputArgument},e=>LogParse(e,name,A(frame,memoryInput?76u:60u),handlerUnwind),out document);
   if(parsed)host.WriteWord(A(owner,8),document);
   table=host.ReadWord(layout.BaseErrorHandlerVtableSlot);host.WriteWord(handler,A(table,8));if(memoryInput)Required(0x80ab044,new[]{wrapper},countUnwind);
   count=host.ReadWord(countSlot);Release(count);
  }
  public void BuildFile(uint owner,uint path,uint frame){Build(owner,path,path,frame,false);}
  public void BuildMemory(uint owner,uint text,uint name,uint frame){Build(owner,text,name,frame,true);}
 }
}
