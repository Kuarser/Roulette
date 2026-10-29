using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionOptionString
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString,CreateFromFile,SetName,AssignString,ConstructStringStream,ExtractString,InvokeOptionDestructor;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> ConstructLiteral,GetHelpText,Register,FaultText,InsertBytes;
   public Action<uint,uint,uint,uint> ReadString;
   public Func<uint,uint,uint,uint> DictionaryLookup;
   public Func<uint> GetConfigInstance;
   public Action<uint> DestroyString,DestroyStringStream,DestroyVector,Delete;
   public Action<uint,uint> Fault;
   public Func<Exception,bool> IsNativeUnwind;public Func<Exception,uint> UnwindSelector,BeginCatch;
   public Action<Exception> Resume;public Action EndCatch;
  }
  public sealed class Layout{public uint TemplateCString,FaultCString,ExceptionCString,EmptyCString;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionOptionString(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual GetOptionStr host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual GetOptionStr binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual GetOptionStr CString required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception ex){host.Resume(ex);throw new InvalidOperationException("Original nonreturning GetOptionStr Resume returned");}
  private void Cleanup(uint pad,uint frame,uint ebx,uint ebp,uint esi,uint edi,Exception ex)
  {
   if(pad==0x836ab83||pad==0x836ac5d)
   {
    if(pad==0x836ac5d)host.DestroyString(ebx);
    if(host.UnwindSelector(ex)!=1){Resume(ex);return;}
    uint caught=host.BeginCatch(ex),data=host.ReadWord(caught);
    try{host.FaultText(16,layout.ExceptionCString,data);}catch(Exception fault){if(!host.IsNativeUnwind(fault))throw;host.EndCatch();Resume(fault);}
    throw new InvalidOperationException("Original nonreturning GetOptionStr exception Fault returned");
   }
   if(pad==0x836ab90){uint begin=host.ReadWord(A(frame,0xe0));if(begin!=0)host.Delete(begin);Resume(ex);return;}
   if(pad==0x836ac4e){host.WriteWord(A(frame,0x18),A(frame,0xe0));host.DestroyVector(A(frame,0xe0));Resume(ex);return;}
   switch(pad)
   {
    case 0x836ac28:case 0x836ac4c:case 0x836ac74:break;
    case 0x836ac3e:case 0x836ac18:case 0x836ac72:host.DestroyString(ebx);break;
    case 0x836ac05:host.DestroyString(A(frame,0xfc));break;
    case 0x836ac8f:host.DestroyStringStream(ebp);host.DestroyString(A(frame,0xfc));break;
    case 0x836ac76:host.DestroyString(ebx);host.DestroyStringStream(ebp);host.DestroyString(A(frame,0xfc));break;
    case 0x836ac93:host.DestroyString(esi);host.DestroyString(edi);break;
    case 0x836aca2:case 0x836aca7:host.DestroyString(edi);break;
    case 0x836abcf:host.DestroyString(ebx);host.DestroyString(edi);break;
    default:throw new ArgumentOutOfRangeException(nameof(pad));
   }
   host.DestroyString(A(frame,0xf0));host.DestroyVector(host.ReadWord(A(frame,0x18)));Resume(ex);
  }
  private T Protected<T>(uint pad,uint frame,uint ebx,uint ebp,uint esi,uint edi,Func<T> call)
  {try{return call();}catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;Cleanup(pad,frame,ebx,ebp,esi,edi,ex);throw new InvalidOperationException("GetOptionStr cleanup returned");}}
  public uint Get(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live GetOptionStr frame required");
   uint machine=host.ReadWord(A(frame,0x140)),ebx=A(frame,0xec),dictionary=host.ReadWord(A(machine,0x10)),table=host.ReadWord(dictionary),edi=host.ReadWord(A(table,0x28)),name=host.ReadWord(A(frame,0x144)),ebp=0,esi=machine;
   Protected(0x836ab83,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11b));return 0;});
   dictionary=host.ReadWord(A(machine,0x10));uint pair=Protected(0x836ac5d,frame,ebx,ebp,esi,edi,()=>host.DictionaryLookup(edi,dictionary,ebx));host.WriteWord(A(frame,0x1c),pair);host.DestroyString(ebx);
   host.WriteWord(A(frame,0xe0),0);host.WriteWord(A(frame,0xe4),0);host.WriteWord(A(frame,0xe8),0);
   Protected(0x836ac4e,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(A(frame,0xf0),layout.TemplateCString,A(frame,0x11c));return 0;});
   ebx=A(frame,0xf4);host.WriteWord(A(frame,0x18),A(frame,0xe0));Protected(0x836ac4c,frame,ebx,ebp,esi,edi,()=>{host.CopyString(ebx,A(frame,0xf0));return 0;});host.WriteWord(A(frame,0x18),A(frame,0xe0));Protected(0x836ac3e,frame,ebx,ebp,esi,edi,()=>{host.CreateFromFile(A(frame,0xe0),ebx);return 0;});host.DestroyString(ebx);
   uint end=host.ReadWord(A(frame,0xe4)),begin=host.ReadWord(A(frame,0xe0)),count=unchecked((uint)(unchecked((int)(end-begin))>>2)*0x3e0f83e1u);
   if(count!=2){Protected(0x836ac4c,frame,ebx,ebp,esi,edi,()=>{host.Fault(16,layout.FaultCString);return 0;});throw new InvalidOperationException("Original nonreturning GetOptionStr template Fault returned");}
   name=host.ReadWord(A(frame,0x144));ebx=A(frame,0xf8);Protected(0x836ac28,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11d));return 0;});begin=host.ReadWord(A(frame,0xe0));Protected(0x836ac18,frame,ebx,ebp,esi,edi,()=>{host.SetName(A(begin,0x84),ebx);return 0;});host.DestroyString(ebx);
   pair=host.ReadWord(A(frame,0x1c));uint length=host.ReadWord(A(pair,4)),offset=host.ReadWord(pair);
   Protected(0x836ac4c,frame,ebx,ebp,esi,edi,()=>{host.ReadString(A(frame,0xfc),machine,offset,length);return 0;});
   ebp=A(frame,0x24);edi=host.ReadWord(A(frame,0xe0));Protected(0x836ac05,frame,ebx,ebp,esi,edi,()=>{host.ConstructStringStream(ebp,0x18);return 0;});
   uint bytes=host.ReadWord(A(frame,0xfc)),byteCount=host.ReadWord(A(bytes,0xfffffff4));Protected(0x836ac8f,frame,ebx,ebp,esi,edi,()=>{host.InsertBytes(A(frame,0x2c),bytes,byteCount);return 0;});
   ebx=A(frame,0x114);Protected(0x836ac8f,frame,ebx,ebp,esi,edi,()=>{host.ExtractString(ebx,A(frame,0x30));return 0;});edi=A(edi,0x90);Protected(0x836ac76,frame,ebx,ebp,esi,edi,()=>{host.AssignString(edi,ebx);return 0;});uint temporary=ebx;ebx=A(frame,0x100);host.DestroyString(temporary);
   Protected(0x836ac8f,frame,ebx,ebp,esi,edi,()=>{host.CopyString(ebx,edi);return 0;});Protected(0x836ac05,frame,ebx,ebp,esi,edi,()=>{host.DestroyStringStream(ebp);return 0;});temporary=ebx;ebx=A(frame,0x108);host.DestroyString(temporary);host.DestroyString(A(frame,0xfc));
   name=host.ReadWord(A(frame,0x144));Protected(0x836ac74,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,name,A(frame,0x11e));return 0;});edi=A(frame,0x104);Protected(0x836ac72,frame,ebx,ebp,esi,edi,()=>{host.GetHelpText(edi,machine,ebx);return 0;});host.DestroyString(ebx);
   uint help=host.ReadWord(A(frame,0x104));ebx=host.ReadWord(A(help,0xfffffff4));
   if(ebx!=0){esi=A(frame,0x10c);Protected(0x836aca2,frame,ebx,ebp,esi,edi,()=>{host.CopyString(esi,edi);return 0;});ebx=host.ReadWord(A(frame,0xe0));Protected(0x836ac93,frame,ebx,ebp,esi,edi,()=>{host.AssignString(A(ebx,0xa8),esi);return 0;});host.WriteByte(A(ebx,0xac),1);host.DestroyString(esi);}
   ebx=A(frame,0x110);Protected(0x836aca7,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,layout.EmptyCString,A(frame,0x11f));return 0;});uint server=Protected(0x836abcf,frame,ebx,ebp,esi,edi,()=>host.GetConfigInstance()),vector=host.ReadWord(A(frame,0x18));Protected(0x836abcf,frame,ebx,ebp,esi,edi,()=>{host.Register(server,vector,ebx);return 0;});host.DestroyString(ebx);host.DestroyString(edi);host.DestroyString(A(frame,0xf0));
   end=host.ReadWord(A(frame,0xe4));begin=host.ReadWord(A(frame,0xe0));if(end!=begin){do{table=host.ReadWord(begin);uint method=host.ReadWord(table);Protected(0x836ab90,frame,ebx,ebp,esi,edi,()=>{host.InvokeOptionDestructor(method,begin);return 0;});begin=A(begin,0x84);}while(begin!=end);begin=host.ReadWord(A(frame,0xe0));}if(begin!=0)host.Delete(begin);return host.ReadWord(A(frame,0x1c));
  }
 }
}
