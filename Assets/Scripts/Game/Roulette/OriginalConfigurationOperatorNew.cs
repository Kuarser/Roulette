using System;
namespace RouletteDeluxe
{
 // Complete packaged operator new000b3ce0/153 bytes, including its
 // bad_alloc exception specification. Actual heap/unwind owners are required.
 public sealed class OriginalConfigurationOperatorNew
 {
  public sealed class Host
  {
   public Func<uint,uint> Malloc,ReadWord,AllocateException;
   public Action<uint,uint> WriteWord;
   public Action<uint> InvokeNewHandler;
   public Action<uint,uint,uint> ThrowException;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,int> ExceptionSelector;
   public Action<Exception> CallUnexpected;
  }
  private readonly Host host;private readonly uint handlerSlot,vtableSlot,typeSlot,destructorSlot;
  public OriginalConfigurationOperatorNew(Host h,uint newHandlerSlot,uint badAllocVtableSlot,uint badAllocTypeSlot,uint badAllocDestructorSlot)
  {
   if(h==null||h.Malloc==null||h.ReadWord==null||h.AllocateException==null||h.WriteWord==null||h.InvokeNewHandler==null||h.ThrowException==null||h.IsNativeUnwind==null||h.ExceptionSelector==null||h.CallUnexpected==null||newHandlerSlot==0||badAllocVtableSlot==0||badAllocTypeSlot==0||badAllocDestructorSlot==0)throw new ArgumentException("Actual operator new heap/handler/exception/unwind/specification and relocated GOT bindings required");
   host=new Host{Malloc=h.Malloc,ReadWord=h.ReadWord,AllocateException=h.AllocateException,WriteWord=h.WriteWord,InvokeNewHandler=h.InvokeNewHandler,ThrowException=h.ThrowException,IsNativeUnwind=h.IsNativeUnwind,ExceptionSelector=h.ExceptionSelector,CallUnexpected=h.CallUnexpected};handlerSlot=newHandlerSlot;vtableSlot=badAllocVtableSlot;typeSlot=badAllocTypeSlot;destructorSlot=badAllocDestructorSlot;
  }
  private T Protected<T>(Func<T> call)
  {
   try{return call();}catch(Exception ex)
   {
    if(host.IsNativeUnwind(ex)&&host.ExceptionSelector(ex)==-1){host.CallUnexpected(ex);throw new InvalidOperationException("Native nonreturning operator new unexpected binding returned");}
    throw;
   }
  }
  public uint Allocate(uint size)
  {
   if(size==0)size=1;uint storage=host.Malloc(size);if(storage!=0)return storage;
   uint handlerCell=host.ReadWord(handlerSlot),handler=host.ReadWord(handlerCell);
   while(handler!=0)
   {
    uint retained=handler;Protected(()=>{host.InvokeNewHandler(retained);return 0;});
    storage=Protected(()=>host.Malloc(size));if(storage!=0)return storage;
    handler=Protected(()=>host.ReadWord(handlerCell));
   }
   Protected(()=>{
    uint exception=host.AllocateException(4),vtable=unchecked(host.ReadWord(vtableSlot)+8);host.WriteWord(exception,vtable);
    uint destructor=host.ReadWord(destructorSlot),type=host.ReadWord(typeSlot);host.ThrowException(exception,type,destructor);return 0;
   });
   throw new InvalidOperationException("Native nonreturning operator new throw binding returned");
  }
 }
}
