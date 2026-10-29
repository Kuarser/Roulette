using System;
namespace RouletteDeluxe
{
 // StorageMediaInfo::GetJeepromSignature 094e3e30, with a post-prolog ESP frame.
 // JEep device creation/read, vector assignment and allocation remain caller ports.
 public sealed class OriginalStorageMediaJeepromSignature
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,NewArray,CreateEeprom;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint,uint,uint,uint> InvokeRead;
   public Func<uint,uint,uint,int> Compare;
   public Action<uint,uint,uint> CopyBytes;
   public Action<uint,uint,uint,byte> AssignVector;
   public Action<uint,uint> InvokeDelete;
   public Action<uint> DeleteArray;
  }
  private readonly Host h;private readonly uint marker;
  public OriginalStorageMediaJeepromSignature(Host host,uint markerCString)
  {
   if(host==null||markerCString==0)throw new ArgumentException("Live JEep signature resource ports and original marker required");
   h=new Host();foreach(var field in typeof(Host).GetFields()){object value=field.GetValue(host);if(value==null)throw new ArgumentException("Signature port required: "+field.Name);field.SetValue(h,value);}marker=markerCString;
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint Get(uint vector,uint frame)
  {
   if(vector==0||frame==0)throw new ArgumentException("Live vector owner and caller frame required");
   h.WriteWord(A(vector,4),h.ReadWord(vector));
   uint local=A(frame,0x34);h.WriteWord(A(frame,0x28),local);
   for(uint offset=0;offset<60;offset+=4)h.WriteWord(A(local,offset),0);
   uint allocation=h.NewArray(0x1000);h.WriteWord(A(frame,0x2c),allocation);
   uint eeprom=h.CreateEeprom(0);if(eeprom==0)return uint.MaxValue;
   uint input=A(allocation,0x800)&0xfffffffcu;
   uint method=h.ReadWord(A(h.ReadWord(eeprom),0x10));h.InvokeRead(method,eeprom,input,0,0x800,0);
   if(h.Compare(A(input,0x7b4),marker,6)==0)
    h.CopyBytes(local,A(input,0x7c0),h.ReadWord(A(input,0x7bc)));
   else for(uint offset=0;offset<40;offset+=4)h.WriteWord(A(local,offset),h.ReadWord(A(input,0x1d8+offset)));
   h.WriteByte(A(frame,0xc),0);h.AssignVector(vector,local,A(frame,0x70),0);
   method=h.ReadWord(A(h.ReadWord(eeprom),4));h.InvokeDelete(method,eeprom);
   allocation=h.ReadWord(A(frame,0x2c));if(allocation!=0)h.DeleteArray(allocation);
   return 0;
  }
 }
}
