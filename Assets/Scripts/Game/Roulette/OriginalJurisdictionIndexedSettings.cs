using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionIndexedSettings
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord;
   public Func<uint,double> ReadDouble;public Action<uint,double> WriteDouble;
   public Func<uint,uint,int,int> FormatName;
   public Func<uint,uint,uint> GetOptionBit;
   public Func<uint,uint,uint,uint> ReadBit;
   public Action<uint,uint,uint,uint,uint,uint,uint,uint> Fault;
  }
  public sealed class Layout{public uint KenoFormat,SlotsFormat,PokerFormat,FaultFormat,SourceFile,KenoFunction,SlotsFunction,PokerFunction;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionIndexedSettings(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual indexed settings host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual indexed setting binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual indexed setting CString required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public void Read(uint originalFunctionVa,uint machine,ushort percent,uint frame)
  {
   uint format,mask,min,max,function,line;
   switch(originalFunctionVa)
   {
    case 0x0836ed90:format=layout.KenoFormat;mask=0xec;min=0x180;max=0x188;function=layout.KenoFunction;line=0xb0b;break;
    case 0x0836ef00:format=layout.SlotsFormat;mask=0xe8;min=0x170;max=0x178;function=layout.SlotsFunction;line=0xadf;break;
    case 0x0836f070:format=layout.PokerFormat;mask=0xe4;min=0x160;max=0x168;function=layout.PokerFunction;line=0xab3;break;
    default:throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));
   }
   if(machine==0||frame==0)throw new ArgumentException("Live jurisdiction owner and indexed frame required");
   if(unchecked((ushort)(percent-75))>31)
   {
    host.Fault(16,layout.FaultFormat,layout.SourceFile,line,function,percent,75,106);throw new InvalidOperationException("Original nonreturning indexed Fault binding returned");
   }
   host.FormatName(A(frame,0x30),format,percent);uint pair=host.GetOptionBit(machine,A(frame,0x30));if(pair==0)return;
   uint bit=host.ReadWord(A(pair,4)),offset=host.ReadWord(pair),value=host.ReadBit(machine,offset,bit);int shift=percent-75;
   if((value&255)==0){uint clear=~(1u<<shift);host.WriteWord(A(machine,mask),host.ReadWord(A(machine,mask))&clear);return;}
   double minimum=host.ReadDouble(A(machine,min));if(minimum==0||minimum>percent)host.WriteDouble(A(machine,min),percent);
   double maximum=host.ReadDouble(A(machine,max));if(maximum==0||percent>maximum)host.WriteDouble(A(machine,max),percent);
   host.WriteWord(A(machine,mask),host.ReadWord(A(machine,mask))|(1u<<shift));
  }
 }
}
