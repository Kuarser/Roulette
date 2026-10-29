using System;
namespace RouletteDeluxe
{
 // Packaged basic_string::append(const basic_string&) at 0009a8c0.
 public sealed class OriginalConfigurationStringAppend
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> CopyBytes,InvokeReserve;
  }
  [Serializable] public sealed class Layout{public uint EmptyRepSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalConfigurationStringAppend(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual String append host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("String append binding required: "+f.Name);f.SetValue(host,v);}if(l.EmptyRepSlot==0)throw new ArgumentException("Empty representation slot required");layout=new Layout{EmptyRepSlot=l.EmptyRepSlot};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint Append(uint owner,uint source,uint frame)
  {
   uint text=host.ReadWord(source),count=host.ReadWord(unchecked(text-12));if(count==0)return owner;
   uint data=host.ReadWord(owner),length=host.ReadWord(unchecked(data-12)),nextLength=A(length,count),capacity=host.ReadWord(unchecked(data-8));
   if(nextLength>capacity||unchecked((int)host.ReadWord(unchecked(data-4)))>0)
   {
    host.InvokeReserve(owner,nextLength,unchecked(frame-64));data=host.ReadWord(owner);text=host.ReadWord(source);length=host.ReadWord(unchecked(data-12));
   }
   uint destination=A(data,length);if(count==1){byte value=host.ReadByte(text);host.WriteByte(destination,value);}else host.CopyBytes(destination,text,count);
   data=host.ReadWord(owner);if(unchecked(data-12)!=host.ReadWord(layout.EmptyRepSlot)){host.WriteWord(unchecked(data-4),0);host.WriteWord(unchecked(data-12),nextLength);host.WriteByte(A(data,nextLength),0);}return owner;
  }
 }
}
