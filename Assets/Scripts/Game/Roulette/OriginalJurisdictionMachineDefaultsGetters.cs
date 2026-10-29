using System;
namespace RouletteDeluxe
{
 // The eight original IJurisdictionMgr slots consumed by LoadConfiguration.
 // Interface thunks subtract four bytes from their live owner before the getter.
 public sealed class OriginalJurisdictionMachineDefaultsGetters
 {
  private readonly OriginalNativeMemory memory;private readonly Func<uint> interfaceOwner;
  public OriginalJurisdictionMachineDefaultsGetters(OriginalNativeMemory memory,Func<uint> interfaceOwner)
  {if(memory==null||interfaceOwner==null)throw new ArgumentException("Caller memory and live IJurisdictionMgr owner required");this.memory=memory;this.interfaceOwner=interfaceOwner;}
  private uint Owner(uint slot,uint thunk)
  {
   uint live=interfaceOwner(),table=memory.ReadWord(live),method=memory.ReadWord(unchecked(table+slot));
   if(method!=thunk)throw new InvalidOperationException("Original IJurisdictionMgr getter binding required at slot "+slot.ToString("x"));
   return unchecked(live-4);
  }
  public byte ReadByte(int slot)
  {
   uint offset,method;switch(slot){case 0x64:offset=0x56;method=0x8363a90;break;case 0x48:offset=0x4f;method=0x83638f0;break;case 0x1ac:offset=0xf3;method=0x8364640;break;case 0x1b0:offset=0xf4;method=0x8364660;break;default:throw new ArgumentOutOfRangeException("slot");}
   return memory.ReadByte(unchecked(Owner((uint)slot,method)+offset));
  }
  public int ReadInt32(int slot)
  {
   uint offset,method;switch(slot){case 0x9c:offset=0x78;method=0x8363cf0;break;case 0x90:offset=0x6c;method=0x8363bd0;break;default:throw new ArgumentOutOfRangeException("slot");}
   uint raw=memory.ReadWord(unchecked(Owner((uint)slot,method)+offset));return raw==uint.MaxValue?int.MaxValue:unchecked((int)raw);
  }
  public long ReadInt64(int slot)
  {
   if(slot!=0xbc)throw new ArgumentOutOfRangeException("slot");uint owner=Owner(0xbc,0x8363e90);
   uint legacy=memory.ReadWord(unchecked(owner+0x90)),high=memory.ReadWord(unchecked(owner+0x98)),low=memory.ReadWord(unchecked(owner+0x94));
   long wide=unchecked((long)(((ulong)high<<32)|low));if(wide>0)return wide;
   return legacy==0||legacy==uint.MaxValue?999999999999L:unchecked((int)legacy);
  }
  // GetLookAndFeel copies the bytes addressed by the owner +0x44 COW field.
  // The LoadConfiguration managed port returns an independent CString snapshot.
  public byte[] ReadText(int slot)
  {
   if(slot!=0x224)throw new ArgumentOutOfRangeException("slot");uint data=memory.ReadWord(unchecked(Owner(0x224,0x8365e00)+0x44));return memory.ReadBytes(data,memory.StrLen(data));
  }
  public OriginalMachineDataDefaults.Host BindMachineDefaults(OriginalMachineDataDefaults.Host host)
  {
   if(host==null)throw new ArgumentNullException("host");var bound=new OriginalMachineDataDefaults.Host();foreach(var field in typeof(OriginalMachineDataDefaults.Host).GetFields())field.SetValue(bound,field.GetValue(host));
   bound.JurisdictionByte=ReadByte;bound.JurisdictionInt=ReadInt32;bound.JurisdictionAmount=ReadInt64;bound.JurisdictionText=ReadText;return bound;
  }
 }
}
