using System;
namespace RouletteDeluxe
{
 // ConfigurationOption C1/C2 094a3460. Raw32 native object slots and the
 // imported empty-string/vtable stores require actual memory owner bindings.
 public sealed class OriginalConfigurationNativeDefault
 {
  public sealed class Host
  {
   public Action<uint,uint> WriteWord,EraseOrderTree;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint> ReadWord;
   public Action<uint> CleanUpCategory,DestroyRuleVector,DestroyAllowedMap,DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationNativeDefault(Host h)
  {
   if(h==null||h.WriteWord==null||h.WriteByte==null||h.ReadWord==null||h.EraseOrderTree==null||h.CleanUpCategory==null||h.DestroyRuleVector==null||h.DestroyAllowedMap==null||h.DestroyString==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual configuration memory/empty string/vtable/category/rule/map/destructor/native unwind bindings required");
   host=new Host{WriteWord=h.WriteWord,WriteByte=h.WriteByte,ReadWord=h.ReadWord,EraseOrderTree=h.EraseOrderTree,CleanUpCategory=h.CleanUpCategory,DestroyRuleVector=h.DestroyRuleVector,DestroyAllowedMap=h.DestroyAllowedMap,DestroyString=h.DestroyString,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public void Construct(uint address)
  {
   Action<uint,uint> word=(offset,value)=>host.WriteWord(Add(address,offset),value);
   Action<uint,byte> flag=(offset,value)=>host.WriteByte(Add(address,offset),value);
   word(60,Add(address,52));word(64,Add(address,52));word(0,0x097681f8);
   word(4,0x09b482ac);word(8,0x09b482ac);word(12,0x09b482ac);word(16,0);
   word(20,0x09b482ac);flag(24,0);word(28,0x09b482ac);flag(32,0);word(36,0x09b482ac);flag(40,0);
   word(44,0);word(52,0);word(56,0);word(68,0);word(76,0);word(80,0);word(92,0);
   word(84,Add(address,76));word(88,Add(address,76));word(96,0);word(100,0);word(104,0);word(108,0);
   for(uint offset=112;offset<=126;++offset)flag(offset,offset==116||offset==121||offset==122?(byte)1:(byte)0);
   word(128,1);
   try{host.CleanUpCategory(address);}
   catch(Exception ex)
   {
    if(!host.IsNativeUnwind(ex))throw;
    host.DestroyRuleVector(Add(address,100));host.DestroyAllowedMap(Add(address,72));
    host.EraseOrderTree(Add(address,48),host.ReadWord(Add(address,56)));
    foreach(uint offset in new[]{36u,28u,20u,12u,8u,4u})host.DestroyString(Add(address,offset));
    word(0,0x09695028);throw;
   }
  }
 }
}
