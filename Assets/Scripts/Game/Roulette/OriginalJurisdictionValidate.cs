using System;
namespace RouletteDeluxe
{
 // Complete Validate body. Allocation, memory and MachineData flag owners are required.
 public sealed class OriginalJurisdictionValidate
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint,byte> InvokeByte;
   public Func<uint,uint> New;
   public Action<uint> Delete;
  }
  private readonly Host host;private readonly uint machineSlot;
  public OriginalJurisdictionValidate(Host h,uint machineDataSlot)
  {
   if(h==null||h.ReadWord==null||h.ReadByte==null||h.WriteWord==null||h.InvokeByte==null||h.New==null||h.Delete==null||machineDataSlot==0)throw new ArgumentException("Actual jurisdiction memory, dispatch, allocation/deletion and relocated MachineData slot required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,WriteWord=h.WriteWord,InvokeByte=h.InvokeByte,New=h.New,Delete=h.Delete};machineSlot=machineDataSlot;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public void Validate(uint owner)
  {
   uint table=host.ReadWord(owner),method=host.ReadWord(Add(table,0x30));
   if(host.InvokeByte(method,owner)==0||host.ReadByte(Add(owner,0x34))==0)return;
   uint machine=host.ReadWord(machineSlot);table=host.ReadWord(machine);method=host.ReadWord(Add(table,0x290));
   if(host.InvokeByte(method,machine)!=0)return;
   uint temporary=host.New(44),input=host.ReadWord(Add(owner,0x30)),source=Add(input,0x1d8);
   uint first=host.ReadWord(source);host.WriteWord(Add(temporary,4),first);
   for(uint offset=4;offset<40;offset+=4)host.WriteWord(Add(temporary,4+offset),host.ReadWord(Add(source,offset)));
   input=host.ReadWord(Add(owner,0x30));uint destination=Add(input,0x1d8);
   host.WriteWord(destination,first);
   for(uint offset=4;offset<40;offset+=4)host.WriteWord(Add(destination,offset),host.ReadWord(Add(temporary,4+offset)));
   host.Delete(temporary);
  }
 }
}
