using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionSelectionSettings
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint,uint> GetOption,InvokeUsingEEPROM;
   public Action<uint,uint,uint> InsertOffset;
   public Action ReadByteFault;
   public Action<uint,uint,uint,uint,uint> Fault;
  }
  public sealed class Layout{public uint PlayerName,RngName,PlayerFault,RngFault,SourceFile,PlayerFunction,RngFunction;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionSelectionSettings(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual selection settings host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual selection setting binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual selection setting CString required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Read(uint originalFunctionVa,uint machine,uint frame)
  {
   if(originalFunctionVa!=0x083704f0&&originalFunctionVa!=0x083705b0)throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));if(machine==0||frame==0)throw new ArgumentException("Live jurisdiction owner and selection frame required");
   bool player=originalFunctionVa==0x083704f0;
   uint pair=host.GetOption(machine,player?layout.PlayerName:layout.RngName);if(pair==0)return;
   uint offset=host.ReadWord(pair),vt=host.ReadWord(machine),method=host.ReadWord(A(vt,0x30));
   uint usingEeprom=host.InvokeUsingEEPROM(method,machine);
   if((usingEeprom&255)==0&&host.ReadWord(A(machine,8))==0)
   {
    host.ReadByteFault();
   }
   else
   {
    uint buffer=host.ReadWord(A(machine,0x30));int selection=unchecked((sbyte)host.ReadByte(A(buffer,offset)));
    host.WriteWord(A(frame,0x3c),offset);host.InsertOffset(A(frame,0x28),A(machine,0x12c),A(frame,0x3c));
    host.WriteWord(A(machine,player?0x1d8u:0x1d4u),unchecked((uint)selection));
    if(selection<=(player?5:1))return;
   }
   host.Fault(16,player?layout.PlayerFault:layout.RngFault,layout.SourceFile,player?0xf17u:0xee4u,player?layout.PlayerFunction:layout.RngFunction);
   throw new InvalidOperationException("Original nonreturning selection Fault binding returned");
  }
 }
}
