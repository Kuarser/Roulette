using System;
namespace RouletteDeluxe
{
 // Full packaged CString C1/C2 and forward-range _S_construct.
 public sealed class OriginalConfigurationLiteralCString
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,ReadByte,StrLen;
   public Action<uint,uint> WriteWord,WriteByte;
   public Func<uint,uint,uint,uint> CreateRep;
   public Action<uint,uint,uint> CopyBytes;
   public Action<uint> ThrowLogicError;
   public uint EmptyRepSlot,ErrorCString;
  }
  private readonly Host host;
  public OriginalConfigurationLiteralCString(Host h)
  {
   if(h==null||h.ReadWord==null||h.ReadByte==null||h.StrLen==null||h.WriteWord==null||h.WriteByte==null||h.CreateRep==null||h.CopyBytes==null||h.ThrowLogicError==null||h.EmptyRepSlot==0||h.ErrorCString==0)throw new ArgumentException("Actual literal string memory, strlen, Rep creation, copy, logic error and relocation owners required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,StrLen=h.StrLen,WriteWord=h.WriteWord,WriteByte=h.WriteByte,CreateRep=h.CreateRep,CopyBytes=h.CopyBytes,ThrowLogicError=h.ThrowLogicError,EmptyRepSlot=h.EmptyRepSlot,ErrorCString=h.ErrorCString};
  }
  public uint ConstructRange(uint begin,uint end,uint allocator)
  {
   if(begin==end)return unchecked(host.ReadWord(host.EmptyRepSlot)+12);
   if(begin==0){host.ThrowLogicError(host.ErrorCString);throw new InvalidOperationException("Native nonreturning literal CString logic error binding returned");}
   uint length=unchecked(end-begin),rep=host.CreateRep(length,0,allocator),data=unchecked(rep+12);
   if(length==1)host.WriteByte(data,host.ReadByte(begin));else host.CopyBytes(data,begin,length);
   if(rep!=host.ReadWord(host.EmptyRepSlot)){host.WriteWord(unchecked(rep+8),0);host.WriteWord(rep,length);host.WriteByte(unchecked(data+length),0);}
   return data;
  }
  public void Construct(uint owner,uint source,uint allocator)
  {uint end=source==0?0xffffffff:unchecked(source+host.StrLen(source));uint data=ConstructRange(source,end,allocator);host.WriteWord(owner,data);}
 }
}
