using System;
namespace RouletteDeluxe
{
 // Complete original GetOptionBit registration and typed/untyped cleanup paths.
 public sealed class OriginalJurisdictionOptionBit
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString,CreateFromFile,SetName,AssignString,ConstructStringStream;
   public Action<uint,byte> WriteByte,InsertBool;
   public Action<uint,uint,uint> ConstructLiteral,GetHelpText,Register;
   public Action<uint,uint> ExtractString,InvokeOptionDestructor;
   public Func<uint,uint,uint,uint> DictionaryLookup,ReadBit;
   public Func<uint> GetConfigInstance;
   public Action<uint> DestroyString,DestroyStringStream,DestroyVector,Delete;
   public Action<uint,uint> Fault;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> UnwindSelector;
   public Action<Exception> BeginCatch,Resume;
   public Action EndCatch;
  }
  public sealed class Layout{public uint TemplateCString,FaultCString,EmptyCString;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionOptionBit(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.WriteWord==null||h.CopyString==null||h.CreateFromFile==null||h.SetName==null||h.AssignString==null||h.ConstructStringStream==null||h.WriteByte==null||h.InsertBool==null||h.ConstructLiteral==null||h.GetHelpText==null||h.Register==null||h.ExtractString==null||h.InvokeOptionDestructor==null||h.DictionaryLookup==null||h.ReadBit==null||h.GetConfigInstance==null||h.DestroyString==null||h.DestroyStringStream==null||h.DestroyVector==null||h.Delete==null||h.Fault==null||h.IsNativeUnwind==null||h.UnwindSelector==null||h.BeginCatch==null||h.Resume==null||h.EndCatch==null||l.TemplateCString==0||l.FaultCString==0||l.EmptyCString==0)throw new ArgumentException("Actual option dictionary/XML/bit/stream/help/register/destruction, std::string unwind selection and relocated CString bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,CopyString=h.CopyString,CreateFromFile=h.CreateFromFile,SetName=h.SetName,AssignString=h.AssignString,ConstructStringStream=h.ConstructStringStream,WriteByte=h.WriteByte,InsertBool=h.InsertBool,ConstructLiteral=h.ConstructLiteral,GetHelpText=h.GetHelpText,Register=h.Register,ExtractString=h.ExtractString,InvokeOptionDestructor=h.InvokeOptionDestructor,DictionaryLookup=h.DictionaryLookup,ReadBit=h.ReadBit,GetConfigInstance=h.GetConfigInstance,DestroyString=h.DestroyString,DestroyStringStream=h.DestroyStringStream,DestroyVector=h.DestroyVector,Delete=h.Delete,Fault=h.Fault,IsNativeUnwind=h.IsNativeUnwind,UnwindSelector=h.UnwindSelector,BeginCatch=h.BeginCatch,Resume=h.Resume,EndCatch=h.EndCatch};layout=new Layout{TemplateCString=l.TemplateCString,FaultCString=l.FaultCString,EmptyCString=l.EmptyCString};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception ex){host.Resume(ex);throw new InvalidOperationException("Original nonreturning option unwind resume returned");}
  private bool Cleanup(uint pad,uint frame,uint ebx,uint ebp,uint edi,uint esi,Exception ex)
  {
   if(pad==0x0836ccef||pad==0x0836cc5b)
   {
    if(pad==0x0836ccef)host.DestroyString(ebx);
    if(host.UnwindSelector(ex)==1){host.BeginCatch(ex);host.EndCatch();host.WriteWord(A(frame,0x1c),0);return true;}
    Resume(ex);
   }
   if(pad==0x0836cc6c){uint begin=host.ReadWord(A(frame,0xe4));if(begin!=0)host.Delete(begin);Resume(ex);}
   if(pad==0x0836cce0){host.WriteWord(A(frame,0x18),A(frame,0xe4));host.DestroyVector(A(frame,0xe4));Resume(ex);}
   switch(pad)
   {
    case 0x0836ccd0:case 0x0836ccaa:case 0x0836cd10:host.DestroyString(ebx);break;
    case 0x0836cc86:host.DestroyStringStream(ebp);break;
    case 0x0836cd14:host.DestroyString(ebx);host.DestroyStringStream(ebp);break;
    case 0x0836cd04:case 0x0836cd25:host.DestroyString(edi);break;
    case 0x0836cd27:host.DestroyString(esi);host.DestroyString(edi);break;
    case 0x0836cd37:host.DestroyString(ebx);host.DestroyString(edi);break;
    case 0x0836ccde:case 0x0836ccba:case 0x0836cd12:break;
    default:throw new ArgumentOutOfRangeException(nameof(pad));
   }
   host.DestroyString(A(frame,0xf4));host.DestroyVector(host.ReadWord(A(frame,0x18)));Resume(ex);return false;
  }
  private T Protected<T>(uint pad,uint frame,uint ebx,uint ebp,uint edi,uint esi,Func<T> call)
  {try{return call();}catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;Cleanup(pad,frame,ebx,ebp,edi,esi,ex);throw new InvalidOperationException("Untyped option cleanup returned");}}
  private bool Initial(uint pad,uint frame,uint ebx,Func<uint> call,out uint value)
  {try{value=call();return true;}catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;if(Cleanup(pad,frame,ebx,0,0,0,ex)){value=0;return false;}throw new InvalidOperationException("Initial option cleanup returned");}}
  public uint Get(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live GetOptionBit caller frame required");
   uint machine=host.ReadWord(A(frame,0x140)),ebx=A(frame,0xf0),dictionary=host.ReadWord(A(machine,0x10)),vtable=host.ReadWord(dictionary),method=host.ReadWord(A(vtable,0x2c)),name=host.ReadWord(A(frame,0x144)),value;
   if(!Initial(0x0836cc5b,frame,ebx,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11b));return 0;},out value))return host.ReadWord(A(frame,0x1c));
   dictionary=host.ReadWord(A(machine,0x10));if(!Initial(0x0836ccef,frame,ebx,()=>host.DictionaryLookup(method,dictionary,ebx),out value))return host.ReadWord(A(frame,0x1c));
   host.WriteWord(A(frame,0x1c),value);host.DestroyString(ebx);
   host.WriteWord(A(frame,0xe4),0);host.WriteWord(A(frame,0xe8),0);host.WriteWord(A(frame,0xec),0);
   Protected(0x0836cce0,frame,ebx,0,method,machine,()=>{host.ConstructLiteral(A(frame,0xf4),layout.TemplateCString,A(frame,0x11c));return 0;});
   ebx=A(frame,0xf8);host.WriteWord(A(frame,0x18),A(frame,0xe4));Protected(0x0836ccde,frame,ebx,0,method,machine,()=>{host.CopyString(ebx,A(frame,0xf4));return 0;});
   host.WriteWord(A(frame,0x18),A(frame,0xe4));Protected(0x0836ccd0,frame,ebx,0,method,machine,()=>{host.CreateFromFile(A(frame,0xe4),ebx);return 0;});host.DestroyString(ebx);
   uint end=host.ReadWord(A(frame,0xe8)),begin=host.ReadWord(A(frame,0xe4));uint count=unchecked((uint)(unchecked((int)(end-begin))>>2)*0x3e0f83e1u);
   if(count!=2){Protected(0x0836ccde,frame,ebx,0,method,machine,()=>{host.Fault(0x10,layout.FaultCString);return 0;});throw new InvalidOperationException("Original nonreturning option template Fault returned");}
   name=host.ReadWord(A(frame,0x144));ebx=A(frame,0xfc);Protected(0x0836ccba,frame,ebx,0,method,machine,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11d));return 0;});
   begin=host.ReadWord(A(frame,0xe4));Protected(0x0836ccaa,frame,ebx,0,method,machine,()=>{host.SetName(A(begin,0x84),ebx);return 0;});host.DestroyString(ebx);
   uint pair=host.ReadWord(A(frame,0x1c)),bit=host.ReadWord(A(pair,4)),offset=host.ReadWord(pair);uint raw=Protected(0x0836ccde,frame,ebx,0,method,machine,()=>host.ReadBit(machine,offset,bit));
   uint ebp=A(frame,0x28),edi=host.ReadWord(A(frame,0xe4));ebx=raw;Protected(0x0836ccde,frame,ebx,ebp,edi,machine,()=>{host.ConstructStringStream(ebp,0x18);return 0;});
   ebx=raw&255;Protected(0x0836cc86,frame,ebx,ebp,edi,machine,()=>{host.InsertBool(A(frame,0x30),(byte)ebx);return 0;});
   ebx=A(frame,0x114);Protected(0x0836cc86,frame,ebx,ebp,edi,machine,()=>{host.ExtractString(ebx,A(frame,0x34));return 0;});edi=A(edi,0x90);
   Protected(0x0836cd14,frame,ebx,ebp,edi,machine,()=>{host.AssignString(edi,ebx);return 0;});uint temporary=ebx;ebx=A(frame,0x100);host.DestroyString(temporary);
   Protected(0x0836cc86,frame,ebx,ebp,edi,machine,()=>{host.CopyString(ebx,edi);return 0;});Protected(0x0836ccde,frame,ebx,ebp,edi,machine,()=>{host.DestroyStringStream(ebp);return 0;});temporary=ebx;ebx=A(frame,0x108);host.DestroyString(temporary);
   name=host.ReadWord(A(frame,0x144));Protected(0x0836cd12,frame,ebx,ebp,edi,machine,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11e));return 0;});edi=A(frame,0x104);
   Protected(0x0836cd10,frame,ebx,ebp,edi,machine,()=>{host.GetHelpText(edi,machine,ebx);return 0;});host.DestroyString(ebx);
   uint help=host.ReadWord(A(frame,0x104));if(host.ReadWord(A(help,0xfffffff4))!=0)
   {
    uint esi=A(frame,0x10c);Protected(0x0836cd04,frame,ebx,ebp,edi,esi,()=>{host.CopyString(esi,edi);return 0;});ebx=host.ReadWord(A(frame,0xe4));
    Protected(0x0836cd27,frame,ebx,ebp,edi,esi,()=>{host.AssignString(A(ebx,0xa8),esi);return 0;});host.WriteByte(A(ebx,0xac),1);host.DestroyString(esi);
   }
   ebx=A(frame,0x110);Protected(0x0836cd25,frame,ebx,ebp,edi,machine,()=>{host.ConstructLiteral(ebx,layout.EmptyCString,A(frame,0x11f));return 0;});
   uint server=Protected(0x0836cd37,frame,ebx,ebp,edi,machine,()=>host.GetConfigInstance()),vector=host.ReadWord(A(frame,0x18));Protected(0x0836cd37,frame,ebx,ebp,edi,machine,()=>{host.Register(server,vector,ebx);return 0;});
   host.DestroyString(ebx);host.DestroyString(edi);host.DestroyString(A(frame,0xf4));end=host.ReadWord(A(frame,0xe8));begin=host.ReadWord(A(frame,0xe4));
   if(end!=begin){do{uint table=host.ReadWord(begin),destructor=host.ReadWord(table);Protected(0x0836cc6c,frame,begin,ebp,edi,end,()=>{host.InvokeOptionDestructor(destructor,begin);return 0;});begin=A(begin,0x84);}while(begin!=end);begin=host.ReadWord(A(frame,0xe4));}
   if(begin!=0)host.Delete(begin);return host.ReadWord(A(frame,0x1c));
  }
 }
}
