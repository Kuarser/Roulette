using System;
namespace RouletteDeluxe
{
 // Exact CFGValueHolder setters 09404120..09404240 and BeTyped 09403f40..094040f0.
 public sealed class OriginalCfgValueSetters
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,AssignString,InvokeBeType;
   public Action<uint,byte> WriteByte;
   public Func<uint,double> ReadDouble;
   public Action<uint,double> WriteDouble;
   public Action<uint,uint,uint> InvokeSetValueType;
  }
  [Serializable] public sealed class Layout{public uint Int32Type,UInt32Type,UInt8Type,DoubleType,FloatType,StringType,RGBType,JurisdictionType,StrType,BitType;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgValueSetters(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual CFG setter host and relocated type constants required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("CFG setter binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("CFG setter relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Invoke(uint owner,uint slot){uint table=host.ReadWord(owner),method=host.ReadWord(A(table,slot));host.InvokeBeType(method,owner);}
  private void Word(uint owner,uint source,uint offset,uint slot){uint value=host.ReadWord(source);host.WriteWord(A(owner,offset),value);Invoke(owner,slot);}
  private void Pointer(uint owner,uint value,uint slot){host.WriteWord(A(owner,36),value);Invoke(owner,slot);}
  public void SetInt32(uint owner,uint source){Word(owner,source,8,100);}
  public void SetUnsignedInt32(uint owner,uint source){Word(owner,source,12,104);}
  public void SetUnsignedInt8(uint owner,uint source){byte value=host.ReadByte(source);host.WriteByte(A(owner,16),value);Invoke(owner,108);}
  public void SetDouble(uint owner,uint source){double value=host.ReadDouble(source);uint table=host.ReadWord(owner);host.WriteDouble(A(owner,20),value);uint method=host.ReadWord(A(table,112));host.InvokeBeType(method,owner);}
  public void SetFloat(uint owner,uint source){Word(owner,source,28,116);}
  public void SetString(uint owner,uint source){host.AssignString(A(owner,32),source);Invoke(owner,120);}
  public void SetRGBColor(uint owner,uint value){Pointer(owner,value,124);}
  public void SetJurisdictionOption(uint owner,uint value){Pointer(owner,value,128);}
  public void SetJurisdictionOptionStr(uint owner,uint value){Pointer(owner,value,132);}
  public void SetJurisdictionOptionBit(uint owner,uint value){Pointer(owner,value,136);}
  private void Be(uint owner,uint type){uint table=host.ReadWord(owner),method=host.ReadWord(A(table,92));host.InvokeSetValueType(method,owner,type);}
  public void BeInt32(uint owner){Be(owner,layout.Int32Type);}
  public void BeUnsignedInt32(uint owner){Be(owner,layout.UInt32Type);}
  public void BeUnsignedInt8(uint owner){Be(owner,layout.UInt8Type);}
  public void BeDouble(uint owner){Be(owner,layout.DoubleType);}
  public void BeFloat(uint owner){Be(owner,layout.FloatType);}
  public void BeString(uint owner){Be(owner,layout.StringType);}
  public void BeRGBColor(uint owner){Be(owner,layout.RGBType);}
  public void BeJurisdictionOption(uint owner){Be(owner,layout.JurisdictionType);}
  public void BeJurisdictionOptionStr(uint owner){Be(owner,layout.StrType);}
  public void BeJurisdictionOptionBit(uint owner){Be(owner,layout.BitType);}
 }
}
