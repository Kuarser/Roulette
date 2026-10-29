using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionOption
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,CopyString,CreateFromFile,SetName,AssignString,ConstructStringStream,ExtractString,InvokeOptionDestructor;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> ConstructLiteral,GetHelpText,Register,FaultText;
   public Func<uint,uint,uint,uint> DictionaryLookup;
   public Func<uint,uint,uint> ReadInteger,ReadShort,ReadSignedChar;
   public Func<uint,uint,ulong> ReadInteger64;
   public Action<uint,int> InsertInteger,InsertShort;
   public Action<uint,uint> InsertUnsignedLong;
   public Action<uint,long> InsertInteger64;
   public Func<uint> GetConfigInstance;
   public Action<uint> DestroyString,DestroyStringStream,DestroyVector,Delete;
   public Action<uint,uint> Fault;
   public Func<Exception,bool> IsNativeUnwind;public Func<Exception,uint> UnwindSelector,BeginCatch;
   public Action<Exception> Resume;public Action EndCatch;
  }
  public sealed class Layout
  {
   public uint TemplateCString,FaultCString,ExceptionCString,EmptyCString;
   public uint MaxWinLimit64,GameEventWinLimit,MaxProgressiveWinLimit,MaxNonProgressiveWinLimit,GTechProtocolSelection,CountOfGamesToCaptureInDL,CountOfBonusRoundsToCaptureInDL;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionOption(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual GetOption host and relocated layout required");
   // All fields are mandatory and captured; later caller mutations cannot replace bindings.
   host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual GetOption binding required: "+f.Name);f.SetValue(host,v);}
   layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual GetOption CString required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception ex){host.Resume(ex);throw new InvalidOperationException("Original nonreturning GetOption resume returned");}
  private void Typed(uint pad,uint frame,uint ebx,Exception ex)
  {
   if(pad==0x08369d2a)host.DestroyString(ebx);
   if(host.UnwindSelector(ex)!=1){Resume(ex);return;}
   uint caught=host.BeginCatch(ex),data=host.ReadWord(caught);
   try{host.FaultText(0x10,layout.ExceptionCString,data);}
   catch(Exception fault){if(!host.IsNativeUnwind(fault))throw;host.EndCatch();Resume(fault);}
   throw new InvalidOperationException("Original nonreturning GetOption exception Fault returned");
  }
  private void Cleanup(uint pad,uint frame,uint ebx,uint ebp,uint esi,uint edi,Exception ex)
  {
   if(pad==0x08369d47||pad==0x08369d2a){Typed(pad,frame,ebx,ex);return;}
   if(pad==0x08369d66){uint begin=host.ReadWord(A(frame,0xdc));if(begin!=0)host.Delete(begin);Resume(ex);return;}
   if(pad==0x08369d18){host.WriteWord(A(frame,0x14),A(frame,0xdc));host.DestroyVector(A(frame,0xdc));Resume(ex);return;}
   switch(pad)
   {
    case 0x08369c81:case 0x08369df0:case 0x08369d02:case 0x08369deb:host.DestroyString(ebx);break;
    case 0x08369db1:case 0x08369ce6:case 0x08369dce:case 0x08369caf:host.DestroyString(ebx);host.DestroyStringStream(ebp);break;
    case 0x08369dca:case 0x08369cfc:case 0x08369de7:case 0x08369cc5:host.DestroyStringStream(ebp);break;
    case 0x08369dfa:case 0x08369d64:case 0x08369e00:host.DestroyString(esi);break;
    case 0x08369da5:host.DestroyString(edi);host.DestroyString(esi);break;
    case 0x08369d49:host.DestroyString(ebx);host.DestroyString(esi);break;
    case 0x08369cc9:case 0x08369d13:case 0x08369df5:break;
    default:throw new ArgumentOutOfRangeException(nameof(pad));
   }
   host.DestroyString(A(frame,0xec));host.DestroyVector(host.ReadWord(A(frame,0x14)));Resume(ex);
  }
  private T Protected<T>(uint pad,uint frame,uint ebx,uint ebp,uint esi,uint edi,Func<T> call)
  {try{return call();}catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;Cleanup(pad,frame,ebx,ebp,esi,edi,ex);throw new InvalidOperationException("GetOption cleanup returned");}}
  private bool Match(uint frame,uint literal,uint length)
  {
   uint name=host.ReadWord(A(frame,0x154));
   for(uint i=0;i<length;++i){byte source=host.ReadByte(A(name,i)),target=host.ReadByte(A(literal,i));if(source!=target)return false;}
   return true;
  }
  public uint Get(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live GetOption caller frame required");
   uint machine=host.ReadWord(A(frame,0x150)),ebx=A(frame,0xe8),dictionary=host.ReadWord(A(machine,0x10)),table=host.ReadWord(dictionary),esi=host.ReadWord(A(table,0x24)),name=host.ReadWord(A(frame,0x154));
   Protected(0x08369d47,frame,ebx,0,esi,0,()=>{host.ConstructLiteral(ebx,name,A(frame,0x12b));return 0;});
   machine=host.ReadWord(A(frame,0x150));dictionary=host.ReadWord(A(machine,0x10));uint pair=Protected(0x08369d2a,frame,ebx,0,esi,0,()=>host.DictionaryLookup(esi,dictionary,ebx));host.WriteWord(A(frame,0x18),pair);host.DestroyString(ebx);
   host.WriteWord(A(frame,0xdc),0);host.WriteWord(A(frame,0xe0),0);host.WriteWord(A(frame,0xe4),0);
   Protected(0x08369d18,frame,ebx,0,esi,0,()=>{host.ConstructLiteral(A(frame,0xec),layout.TemplateCString,A(frame,0x12c));return 0;});
   ebx=A(frame,0xf0);host.WriteWord(A(frame,0x14),A(frame,0xdc));Protected(0x08369d13,frame,ebx,0,esi,0,()=>{host.CopyString(ebx,A(frame,0xec));return 0;});
   host.WriteWord(A(frame,0x14),A(frame,0xdc));Protected(0x08369d02,frame,ebx,0,esi,0,()=>{host.CreateFromFile(A(frame,0xdc),ebx);return 0;});host.DestroyString(ebx);
   uint end=host.ReadWord(A(frame,0xe0)),begin=host.ReadWord(A(frame,0xdc));uint count=unchecked((uint)(unchecked((int)(end-begin))>>2)*0x3e0f83e1u);
   if(count!=2){Protected(0x08369d13,frame,ebx,0,esi,0,()=>{host.Fault(0x10,layout.FaultCString);return 0;});throw new InvalidOperationException("Original nonreturning GetOption template Fault returned");}
   name=host.ReadWord(A(frame,0x154));ebx=A(frame,0xf4);Protected(0x08369cc9,frame,ebx,0,esi,0,()=>{host.ConstructLiteral(ebx,name,A(frame,0x12d));return 0;});begin=host.ReadWord(A(frame,0xdc));Protected(0x08369c81,frame,ebx,0,esi,0,()=>{host.SetName(A(begin,0x84),ebx);return 0;});host.DestroyString(ebx);
   int kind=0;
   if(Match(frame,layout.MaxWinLimit64,14)||Match(frame,layout.GameEventWinLimit,18)||Match(frame,layout.MaxProgressiveWinLimit,23)||Match(frame,layout.MaxNonProgressiveWinLimit,26))kind=64;
   else if(Match(frame,layout.GTechProtocolSelection,23))kind=8;
   else if(Match(frame,layout.CountOfGamesToCaptureInDL,26)||Match(frame,layout.CountOfBonusRoundsToCaptureInDL,32))kind=16;
   pair=host.ReadWord(A(frame,0x18));uint offset=host.ReadWord(pair);machine=host.ReadWord(A(frame,0x150));uint ebp=A(frame,0x20),edi=0,raw=0;ulong wide=0;
   if(kind==64){wide=Protected(0x08369d13,frame,ebx,ebp,esi,edi,()=>host.ReadInteger64(machine,offset));esi=(uint)wide;edi=(uint)(wide>>32);begin=host.ReadWord(A(frame,0xdc));host.WriteWord(A(frame,0x1c),begin);}
   else{raw=Protected(0x08369d13,frame,ebx,ebp,esi,edi,()=>kind==8?host.ReadSignedChar(machine,offset):kind==16?host.ReadShort(machine,offset):host.ReadInteger(machine,offset));ebx=raw;esi=host.ReadWord(A(frame,0xdc));}
   Protected(0x08369d13,frame,ebx,ebp,esi,edi,()=>{host.ConstructStringStream(ebp,0x18);return 0;});
   uint streamPad=kind==64?0x08369dcau:kind==16?0x08369de7u:kind==8?0x08369cc5u:0x08369cfcu;
   if(kind==64)Protected(streamPad,frame,ebx,ebp,esi,edi,()=>{host.InsertInteger64(A(frame,0x28),unchecked((long)wide));return 0;});
   else{if(kind==16)ebx=unchecked((uint)(int)(short)raw);else if(kind==8)ebx=unchecked((uint)(int)(sbyte)raw);Protected(streamPad,frame,ebx,ebp,esi,edi,()=>{if(kind==16)host.InsertShort(A(frame,0x28),unchecked((int)ebx));else if(kind==8)host.InsertUnsignedLong(A(frame,0x28),ebx);else host.InsertInteger(A(frame,0x28),unchecked((int)ebx));return 0;});}
   ebx=A(frame,kind==64?0x118u:kind==16?0x120u:kind==8?0x11cu:0x124u);Protected(streamPad,frame,ebx,ebp,esi,edi,()=>{host.ExtractString(ebx,A(frame,0x2c));return 0;});
   if(kind==64)esi=host.ReadWord(A(frame,0x1c));esi=A(esi,0x90);uint assignPad=kind==64?0x08369db1u:kind==16?0x08369dceu:kind==8?0x08369cafu:0x08369ce6u;
   Protected(assignPad,frame,ebx,ebp,esi,edi,()=>{host.AssignString(esi,ebx);return 0;});uint temporary=ebx;ebx=A(frame,kind==64?0xf8u:kind==16?0x100u:kind==8?0xfcu:0x104u);host.DestroyString(temporary);
   Protected(streamPad,frame,ebx,ebp,esi,edi,()=>{host.CopyString(ebx,esi);return 0;});Protected(0x08369d13,frame,ebx,ebp,esi,edi,()=>{host.DestroyStringStream(ebp);return 0;});temporary=ebx;ebx=A(frame,0x10c);host.DestroyString(temporary);
   name=host.ReadWord(A(frame,0x154));Protected(0x08369df5,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,name,A(frame,0x12e));return 0;});machine=host.ReadWord(A(frame,0x150));esi=A(frame,0x108);Protected(0x08369deb,frame,ebx,ebp,esi,edi,()=>{host.GetHelpText(esi,machine,ebx);return 0;});host.DestroyString(ebx);
   uint help=host.ReadWord(A(frame,0x108));if(host.ReadWord(A(help,0xfffffff4))!=0){edi=A(frame,0x110);Protected(0x08369dfa,frame,ebx,ebp,esi,edi,()=>{host.CopyString(edi,esi);return 0;});ebx=host.ReadWord(A(frame,0xdc));Protected(0x08369da5,frame,ebx,ebp,esi,edi,()=>{host.AssignString(A(ebx,0xa8),edi);return 0;});host.WriteByte(A(ebx,0xac),1);host.DestroyString(edi);}
   ebx=A(frame,0x114);Protected(0x08369d64,frame,ebx,ebp,esi,edi,()=>{host.ConstructLiteral(ebx,layout.EmptyCString,A(frame,0x12f));return 0;});uint server=Protected(0x08369d49,frame,ebx,ebp,esi,edi,()=>host.GetConfigInstance()),vector=host.ReadWord(A(frame,0x14));Protected(0x08369d49,frame,ebx,ebp,esi,edi,()=>{host.Register(server,vector,ebx);return 0;});
   host.DestroyString(ebx);host.DestroyString(esi);host.DestroyString(A(frame,0xec));end=host.ReadWord(A(frame,0xe0));begin=host.ReadWord(A(frame,0xdc));if(end!=begin){do{table=host.ReadWord(begin);uint method=host.ReadWord(table);Protected(0x08369d66,frame,ebx,ebp,begin,edi,()=>{host.InvokeOptionDestructor(method,begin);return 0;});begin=A(begin,0x84);}while(begin!=end);begin=host.ReadWord(A(frame,0xdc));}if(begin!=0)host.Delete(begin);return host.ReadWord(A(frame,0x18));
  }
 }
}
