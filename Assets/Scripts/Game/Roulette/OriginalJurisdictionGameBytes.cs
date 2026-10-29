using System;
namespace RouletteDeluxe
{
 // Complete ReadGameBytes body. Set insertion and byte-vector growth remain required owners.
 public sealed class OriginalJurisdictionGameBytes
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,byte> InvokeUsingEeprom;
   public Action<uint,int> InsertOffset;
   public Action<uint,uint,byte> InsertByte;
   public Action ReadByteFault;
  }
  private readonly Host host;
  public OriginalJurisdictionGameBytes(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.ReadByte==null||h.WriteByte==null||h.InvokeUsingEeprom==null||h.InsertOffset==null||h.InsertByte==null||h.ReadByteFault==null)throw new ArgumentException("Actual jurisdiction memory, EEPROM dispatch, offset-set insertion, byte-vector growth and ReadByte fault bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,ReadByte=h.ReadByte,WriteByte=h.WriteByte,InvokeUsingEeprom=h.InvokeUsingEeprom,InsertOffset=h.InsertOffset,InsertByte=h.InsertByte,ReadByteFault=h.ReadByteFault};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public void ReadGameBytes(uint owner)
  {
   host.WriteWord(Add(owner,0x244),host.ReadWord(Add(owner,0x240)));
   for(uint offset=0x8d;offset>=0x86;--offset)
   {
    uint table=host.ReadWord(owner),method=host.ReadWord(Add(table,0x30));
    if(host.InvokeUsingEeprom(method,owner)==0&&host.ReadWord(Add(owner,8))==0)
    {host.ReadByteFault();throw new InvalidOperationException("Original nonreturning ReadByte fault binding returned");}
    byte value=host.ReadByte(Add(host.ReadWord(Add(owner,0x30)),offset));
    host.InsertOffset(Add(owner,0x12c),unchecked((int)offset));
    uint end=host.ReadWord(Add(owner,0x244)),capacity=host.ReadWord(Add(owner,0x248));
    if(end==capacity)host.InsertByte(Add(owner,0x240),end,value);
    else
    {
     if(end!=0){host.WriteByte(end,value);end=host.ReadWord(Add(owner,0x244));}
     host.WriteWord(Add(owner,0x244),Add(end,1));
    }
   }
  }
 }
}
