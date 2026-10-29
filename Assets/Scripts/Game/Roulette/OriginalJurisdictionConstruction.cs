using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionConstruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Action<uint,ulong> Write64;
   public Action<uint> InitHelpText;
   public Func<uint,uint,uint> Open;
   public Action<uint> Close;
   public Func<uint,uint,int> InvokeGameMode;
   public Action<uint,uint> EraseGameMap,EraseOffsetSet,EraseHelpMap;
   public Action<uint> DeleteArray,DestroyString;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  public sealed class Layout
  {
   public uint DerivedVtable,DerivedInterfaceVtable,BaseVtable,BaseInterfaceVtable,EmptyStringData;
   public uint NvramPathSlot,ReadModeCString,GameManagerSlot,SingletonSlot;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionConstruction(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.WriteWord==null||h.WriteByte==null||h.Write64==null||h.InitHelpText==null||h.Open==null||h.Close==null||h.InvokeGameMode==null||h.EraseGameMap==null||h.EraseOffsetSet==null||h.EraseHelpMap==null||h.DeleteArray==null||h.DestroyString==null||h.IsNativeUnwind==null||h.Resume==null||l.DerivedVtable==0||l.DerivedInterfaceVtable==0||l.BaseVtable==0||l.BaseInterfaceVtable==0||l.EmptyStringData==0||l.NvramPathSlot==0||l.ReadModeCString==0||l.GameManagerSlot==0||l.SingletonSlot==0)throw new ArgumentException("Actual jurisdiction memory, help, file, tree/string/array lifetime, native unwind and relocated owner bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,WriteByte=h.WriteByte,Write64=h.Write64,InitHelpText=h.InitHelpText,Open=h.Open,Close=h.Close,InvokeGameMode=h.InvokeGameMode,EraseGameMap=h.EraseGameMap,EraseOffsetSet=h.EraseOffsetSet,EraseHelpMap=h.EraseHelpMap,DeleteArray=h.DeleteArray,DestroyString=h.DestroyString,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};
   layout=new Layout{DerivedVtable=l.DerivedVtable,DerivedInterfaceVtable=l.DerivedInterfaceVtable,BaseVtable=l.BaseVtable,BaseInterfaceVtable=l.BaseInterfaceVtable,EmptyStringData=l.EmptyStringData,NvramPathSlot=l.NvramPathSlot,ReadModeCString=l.ReadModeCString,GameManagerSlot=l.GameManagerSlot,SingletonSlot=l.SingletonSlot};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Cleanup(uint owner,Exception error)
  {
   host.EraseGameMap(Add(owner,0x270),host.ReadWord(Add(owner,0x278)));
   uint array=host.ReadWord(Add(owner,0x240));if(array!=0)host.DeleteArray(array);
   host.DestroyString(Add(owner,0x1f0));host.DestroyString(Add(owner,0x1c0));
   host.EraseOffsetSet(Add(owner,0x144),host.ReadWord(Add(owner,0x14c)));
   host.EraseOffsetSet(Add(owner,0x12c),host.ReadWord(Add(owner,0x134)));
   host.DestroyString(Add(owner,0x44));host.DestroyString(Add(owner,0x40));host.DestroyString(Add(owner,0x3c));host.DestroyString(Add(owner,0x38));
   host.EraseHelpMap(Add(owner,0x14),host.ReadWord(Add(owner,0x1c)));
   host.WriteWord(Add(owner,4),layout.BaseInterfaceVtable);host.WriteWord(owner,layout.BaseVtable);
   host.Resume(error);throw new InvalidOperationException("Original nonreturning unwind resume binding returned");
  }
  private T Protected<T>(uint owner,Func<T> call)
  {try{return call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;Cleanup(owner,error);throw;}}
  public void Construct(uint owner)
  {
   host.WriteWord(Add(owner,0x20),Add(owner,0x18));
   host.WriteWord(Add(owner,0x24),Add(owner,0x18));
   host.WriteWord(Add(owner,0x138),Add(owner,0x130));
   host.WriteWord(Add(owner,0x13c),Add(owner,0x130));
   host.WriteWord(Add(owner,0x150),Add(owner,0x148));
   host.WriteWord(Add(owner,0x154),Add(owner,0x148));
   host.WriteWord(Add(owner,0),layout.DerivedVtable);
   host.WriteWord(Add(owner,0x4),layout.DerivedInterfaceVtable);
   host.WriteWord(Add(owner,0x18),0x0);
   host.WriteWord(Add(owner,0x1c),0x0);
   host.WriteWord(Add(owner,0x28),0x0);
   host.WriteWord(Add(owner,0x38),layout.EmptyStringData);
   host.WriteWord(Add(owner,0x3c),layout.EmptyStringData);
   host.WriteWord(Add(owner,0x40),layout.EmptyStringData);
   host.WriteWord(Add(owner,0x44),layout.EmptyStringData);
   host.WriteWord(Add(owner,0xe4),0x0);
   host.WriteWord(Add(owner,0xe8),0x0);
   host.WriteWord(Add(owner,0xec),0x0);
   host.WriteWord(Add(owner,0x130),0x0);
   host.WriteWord(Add(owner,0x134),0x0);
   host.WriteWord(Add(owner,0x140),0x0);
   host.WriteWord(Add(owner,0x148),0x0);
   host.WriteWord(Add(owner,0x14c),0x0);
   host.WriteWord(Add(owner,0x158),0x0);
   host.WriteWord(Add(owner,0x1c0),layout.EmptyStringData);
   host.WriteWord(Add(owner,0x1f0),layout.EmptyStringData);
   host.WriteWord(Add(owner,0x240),0x0);
   host.WriteWord(Add(owner,0x244),0x0);
   host.WriteWord(Add(owner,0x248),0x0);
   host.WriteWord(Add(owner,0x274),0x0);
   host.Write64(Add(owner,0x160),0);
   host.WriteWord(Add(owner,0x278),0x0);
   host.Write64(Add(owner,0x168),0);
   host.WriteWord(Add(owner,0x284),0x0);
   host.Write64(Add(owner,0x170),0);
   host.WriteWord(Add(owner,0x27c),Add(owner,0x274));
   host.Write64(Add(owner,0x178),0);
   host.WriteWord(Add(owner,0x280),Add(owner,0x274));
   host.Write64(Add(owner,0x180),0);
   host.WriteWord(Add(owner,0x8),0x0);
   host.Write64(Add(owner,0x188),0);
   host.WriteWord(Add(owner,0xc),0x0);
   host.WriteWord(Add(owner,0x10),0x0);
   host.WriteWord(Add(owner,0x2c),0x0);
   host.WriteWord(Add(owner,0x30),0x0);
   host.WriteByte(Add(owner,0x15c),0x0);
   host.WriteByte(Add(owner,0x34),0x0);
   host.WriteByte(Add(owner,0x252),0x0);
   Protected(owner,()=>{host.InitHelpText(owner);return 0;});
   uint path=host.ReadWord(layout.NvramPathSlot);
   uint file=Protected(owner,()=>host.Open(path,layout.ReadModeCString));
   if(file!=0){host.WriteByte(Add(owner,0x34),0);Protected(owner,()=>{host.Close(file);return 0;});}
   else
   {
    uint manager=host.ReadWord(layout.GameManagerSlot),table=host.ReadWord(manager),method=host.ReadWord(Add(table,0x108));
    int mode=Protected(owner,()=>host.InvokeGameMode(method,manager));host.WriteByte(Add(owner,0x34),(byte)(mode==0?1:0));
   }
   host.WriteWord(layout.SingletonSlot,owner);
  }
 }
}
