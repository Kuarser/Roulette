using System;
namespace RouletteDeluxe
{
 // Original InitHelpText cursor, leak admission, substring and map assignment sequence.
 public sealed class OriginalJurisdictionHelpParsing
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,GetValueSlot;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,AssignString;
   public Action<uint> LeakString,DestroyString,ThrowOutOfRange;
   public Func<uint,byte,uint,uint> Find;
   public Action<uint,uint,uint,uint> ConstructSubstring;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,uint,Exception> Cleanup;
  }
  private readonly Host host;private readonly uint outOfRangeCString;
  public OriginalJurisdictionHelpParsing(Host h,uint actualErrorCString)
  {
   if(h==null||h.ReadWord==null||h.GetValueSlot==null||h.ReadByte==null||h.WriteWord==null||h.AssignString==null||h.LeakString==null||h.DestroyString==null||h.ThrowOutOfRange==null||h.Find==null||h.ConstructSubstring==null||h.IsNativeUnwind==null||h.Cleanup==null||actualErrorCString==0)throw new ArgumentException("Actual help string, map, substring, leak, native cleanup and error CString owners required");
   host=new Host{ReadWord=h.ReadWord,GetValueSlot=h.GetValueSlot,ReadByte=h.ReadByte,WriteWord=h.WriteWord,AssignString=h.AssignString,LeakString=h.LeakString,DestroyString=h.DestroyString,ThrowOutOfRange=h.ThrowOutOfRange,Find=h.Find,ConstructSubstring=h.ConstructSubstring,IsNativeUnwind=h.IsNativeUnwind,Cleanup=h.Cleanup};outOfRangeCString=actualErrorCString;
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private T Protected<T>(uint pad,uint frame,uint ebx,Func<T> call){try{return call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Cleanup(pad,frame,ebx,error);throw new InvalidOperationException("Original nonreturning parser cleanup binding returned");}}
  private void Leak(uint frame,uint cursor){uint owner=host.ReadWord(A(frame,0x24));Protected(0x083686e3,frame,cursor,()=>{host.LeakString(owner);return 0;});}
  private void OutOfRange(uint pad,uint frame,uint cursor){Protected(pad,frame,cursor,()=>{host.ThrowOutOfRange(outOfRangeCString);return 0;});throw new InvalidOperationException("Original nonreturning substring error binding returned");}
  public uint Parse(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live InitHelpText frame required");
   uint cursor=0,data=host.ReadWord(A(frame,0x228)),length=host.ReadWord(A(data,0xfffffff4));
   while(cursor<length)
   {
    uint refs=host.ReadWord(A(data,0xfffffffc));
    if(unchecked((int)refs)>=0){Leak(frame,cursor);data=host.ReadWord(A(frame,0x228));refs=host.ReadWord(A(data,0xfffffffc));}
    byte character=host.ReadByte(A(data,cursor));
    if(unchecked((sbyte)character)>0x60)
    {
     if(unchecked((int)refs)>=0){Leak(frame,cursor);data=host.ReadWord(A(frame,0x228));character=host.ReadByte(A(data,cursor));}
     if(unchecked((sbyte)character)<=0x7a)goto letter;
     refs=host.ReadWord(A(data,0xfffffffc));
    }
    if(unchecked((int)refs)>=0){Leak(frame,cursor);data=host.ReadWord(A(frame,0x228));character=host.ReadByte(A(data,cursor));}
    if(unchecked((sbyte)character)<=0x40)goto skip;
    refs=host.ReadWord(A(data,0xfffffffc));
    if(unchecked((int)refs)>=0){Leak(frame,cursor);data=host.ReadWord(A(frame,0x228));character=host.ReadByte(A(data,cursor));}
    if(unchecked((sbyte)character)>0x5a)goto skip;
   letter:
    if(cursor>=host.ReadWord(A(data,0xfffffff4)))return cursor;
    uint source=host.ReadWord(A(frame,0x24));uint lf=Protected(0x083686e3,frame,cursor,()=>host.Find(source,10,cursor));source=host.ReadWord(A(frame,0x24));host.WriteWord(A(frame,0x1c),lf);
    uint tab=Protected(0x083686e3,frame,cursor,()=>host.Find(source,9,cursor));data=host.ReadWord(A(frame,0x228));uint savedLf=host.ReadWord(A(frame,0x1c));length=host.ReadWord(A(data,0xfffffff4));
    if(savedLf==uint.MaxValue)host.WriteWord(A(frame,0x1c),unchecked(length-1));
    if(host.ReadWord(A(frame,0x1c))<tab||tab==uint.MaxValue){cursor=A(host.ReadWord(A(frame,0x1c)),1);continue;}
    uint position=A(tab,1);if(position>length)OutOfRange(0x083686e3,frame,cursor);
    source=host.ReadWord(A(frame,0x24));uint requested=unchecked(~tab+host.ReadWord(A(frame,0x1c)));
    Protected(0x083686e3,frame,cursor,()=>{host.ConstructSubstring(A(frame,0x234),source,position,requested);return 0;});
    data=host.ReadWord(A(frame,0x228));length=host.ReadWord(A(data,0xfffffff4));if(cursor>length)OutOfRange(0x083686c0,frame,cursor);
    source=host.ReadWord(A(frame,0x24));uint keyLength=unchecked(tab-cursor);Protected(0x083686c0,frame,cursor,()=>{host.ConstructSubstring(A(frame,0x230),source,cursor,keyLength);return 0;});
    uint valueOwner=host.GetValueSlot(frame);Protected(0x0836870f,frame,valueOwner,()=>{host.AssignString(valueOwner,A(frame,0x234));return 0;});
    host.DestroyString(A(frame,0x230));host.DestroyString(A(frame,0x234));cursor=host.ReadWord(A(frame,0x1c));data=host.ReadWord(A(frame,0x228));cursor=A(cursor,1);length=host.ReadWord(A(data,0xfffffff4));continue;
   skip:
    cursor=A(cursor,1);length=host.ReadWord(A(data,0xfffffff4));
   }
   return cursor;
  }
 }
}
