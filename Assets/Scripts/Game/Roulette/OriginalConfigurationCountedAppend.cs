using System;
namespace RouletteDeluxe
{
 // Packaged append(const char*,unsigned) 0009a5b0 and reserve(unsigned) 0009a2b0.
 public sealed class OriginalConfigurationCountedAppend
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,ReleaseRep;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> CopyBytes,InvokeReserve;
   public Func<uint,uint,uint,uint> CloneRep;
   public Action<uint> ThrowLengthError;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot,LengthErrorCString;}
  private readonly Host host;private readonly Layout layout;
  public OriginalConfigurationCountedAppend(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual counted append host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Counted append binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Counted append relocation required: "+f.Name);f.SetValue(layout,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Reserve(uint owner,uint requested,uint frame)
  {
   uint data=host.ReadWord(owner),capacity=host.ReadWord(unchecked(data-8));if(requested==capacity&&unchecked((int)host.ReadWord(unchecked(data-4)))<=0)return;uint length=host.ReadWord(unchecked(data-12));if(requested<length)requested=length;uint allocator=A(frame,31),next=host.CloneRep(unchecked(data-12),allocator,unchecked(requested-length));uint old=unchecked(host.ReadWord(owner)-12);if(old!=host.ReadWord(layout.EmptyRepSlot))host.ReleaseRep(old,allocator);host.WriteWord(owner,next);
  }
  public uint Append(uint owner,uint text,uint count,uint frame)
  {
   if(count==0)return owner;uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12));if(count>unchecked(0x3ffffffcu-length)){host.ThrowLengthError(layout.LengthErrorCString);throw new InvalidOperationException("Original nonreturning append length error returned");}uint nextLength=A(length,count),capacity=host.ReadWord(unchecked(data-8));
   if(nextLength>capacity||unchecked((int)host.ReadWord(unchecked(data-4)))>0)
   {
    bool alias=text>=data&&text<=A(data,length);uint offset=unchecked(text-data);host.InvokeReserve(owner,nextLength,unchecked(frame-64));data=host.ReadWord(owner);length=host.ReadWord(unchecked(data-12));if(alias)text=A(data,offset);
   }
   uint destination=A(data,length);if(count==1){byte value=host.ReadByte(text);host.WriteByte(destination,value);}else host.CopyBytes(destination,text,count);
   data=host.ReadWord(owner);if(unchecked(data-12)!=host.ReadWord(layout.EmptyRepSlot)){host.WriteWord(unchecked(data-4),0);host.WriteWord(unchecked(data-12),nextLength);host.WriteByte(A(data,nextLength),0);}return owner;
  }
 }
}
