using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionScalarSettings
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord;
   public Func<uint,uint,uint> GetOption,ReadInteger,InsertCString;
   public Func<uint,uint,ulong> ReadInteger64;
   public Func<uint,int,uint> InsertInteger;public Action<uint> EndLine;
  }
  public sealed class Layout{public uint MaximumLossName,MaxWinName,MaxWin64Name,MaxPrinterName,PrinterMessageCString,OutputStream;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionScalarSettings(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual scalar settings host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual scalar setting binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual scalar setting CString/stream required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Read(uint originalFunctionVa,uint machine)
  {
   if(originalFunctionVa!=0x08369f20&&originalFunctionVa!=0x0836a260&&originalFunctionVa!=0x0836a650)throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));if(machine==0)throw new ArgumentException("Live jurisdiction owner required");
   uint name=originalFunctionVa==0x08369f20?layout.MaximumLossName:originalFunctionVa==0x0836a260?layout.MaxWinName:layout.MaxPrinterName;
   uint pair=host.GetOption(machine,name);if(pair==0)return;uint offset=host.ReadWord(pair),value=host.ReadInteger(machine,offset);
   if(originalFunctionVa==0x08369f20){host.WriteWord(A(machine,0x214),value);return;}
   if(originalFunctionVa==0x0836a260)
   {
    host.WriteWord(A(machine,0x90),value);pair=host.GetOption(machine,layout.MaxWin64Name);if(pair==0)return;offset=host.ReadWord(pair);ulong wide=host.ReadInteger64(machine,offset);host.WriteWord(A(machine,0x94),(uint)wide);host.WriteWord(A(machine,0x98),(uint)(wide>>32));return;
   }
   host.WriteWord(A(machine,0x70),value);uint stream=host.InsertCString(layout.OutputStream,layout.PrinterMessageCString);stream=host.InsertInteger(stream,unchecked((int)value));host.EndLine(stream);
  }
 }
}
