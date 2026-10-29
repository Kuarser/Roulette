using System;
namespace RouletteDeluxe
{
 // Complete Init, VerifyData, UsingEeprom and InitDefaultLimits bodies.
 // Read, map insertion, game-byte decoding and native owner lifetimes require bindings.
 public sealed class OriginalJurisdictionStartup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint,int> InvokeGameMode;
   public Func<uint,uint,uint,uint,byte> InvokeRead;
   public Func<uint,uint,int,int,uint> InsertUnique;
   public Action<uint> ReadGameBytes;
   public Func<uint,int> CheckIfSwapped;
   public Func<uint,uint,byte> InvokeByte;
   public Func<uint,uint,ulong> InvokeGet64;
   public Action<uint,uint,ulong> InvokeSet64;
   public Action<uint,uint,uint,int,uint> FaultInit;
  }
  public sealed class Layout
  {
   public uint GameManagerSlot,MachineDataSlot,ConfigPathSlot,NvramPathSlot,SimulatedPathSlot;
   public uint FaultFormat,FaultFile,FaultFunction;
  }
  private readonly Host host;
  private readonly Layout layout;
  public OriginalJurisdictionStartup(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.ReadByte==null||h.WriteWord==null||h.InvokeGameMode==null||h.InvokeRead==null||h.InsertUnique==null||h.ReadGameBytes==null||h.CheckIfSwapped==null||h.InvokeByte==null||h.InvokeGet64==null||h.InvokeSet64==null||h.FaultInit==null||l.GameManagerSlot==0||l.MachineDataSlot==0||l.ConfigPathSlot==0||l.NvramPathSlot==0||l.SimulatedPathSlot==0||l.FaultFormat==0||l.FaultFile==0||l.FaultFunction==0)throw new ArgumentException("Actual jurisdiction memory, dispatch, map, fault and relocated global/path bindings required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,WriteWord=h.WriteWord,InvokeGameMode=h.InvokeGameMode,InvokeRead=h.InvokeRead,InsertUnique=h.InsertUnique,ReadGameBytes=h.ReadGameBytes,CheckIfSwapped=h.CheckIfSwapped,InvokeByte=h.InvokeByte,InvokeGet64=h.InvokeGet64,InvokeSet64=h.InvokeSet64,FaultInit=h.FaultInit};
   layout=new Layout{GameManagerSlot=l.GameManagerSlot,MachineDataSlot=l.MachineDataSlot,ConfigPathSlot=l.ConfigPathSlot,NvramPathSlot=l.NvramPathSlot,SimulatedPathSlot=l.SimulatedPathSlot,FaultFormat=l.FaultFormat,FaultFile=l.FaultFile,FaultFunction=l.FaultFunction};
  }
  private uint Read(uint address){return host.ReadWord(address);}
  private static uint Add(uint address,uint offset){return unchecked(address+offset);}
  public byte UsingEeprom(uint owner){return host.ReadByte(Add(owner,0x34));}
  public byte VerifyData(uint owner)
  {
   uint input=Read(Add(owner,0x30));
   for(uint offset=0;offset<2048;++offset)if(host.ReadByte(Add(input,offset))!=255)return 1;
   return 0;
  }
  public byte Init(uint owner)
  {
   uint method=Read(Add(Read(owner),0x14));
   uint manager=Read(layout.GameManagerSlot),table=Read(manager);
   int mode=host.InvokeGameMode(Read(Add(table,0x108)),manager);
   uint binary=Read(mode!=0||host.ReadByte(Add(owner,0x34))!=0?layout.SimulatedPathSlot:layout.NvramPathSlot);
   uint config=Read(layout.ConfigPathSlot);
   if(host.InvokeRead(method,owner,config,binary)==0)
   {
    host.FaultInit(16,layout.FaultFormat,layout.FaultFile,0x79,layout.FaultFunction);
    throw new InvalidOperationException("Original nonreturning Fault binding returned");
   }
   uint header=Add(owner,0x274),tree=Add(owner,0x270);
   for(int key=0;key<8;++key)
   {
    uint node=Read(Add(owner,0x278)),candidate=header;
    while(node!=0)
    {
     if(key<=unchecked((int)Read(Add(node,0x10)))){candidate=node;node=Read(Add(node,8));}
     else node=Read(Add(node,12));
    }
    if(candidate==header||key<unchecked((int)Read(Add(candidate,0x10))))candidate=host.InsertUnique(tree,candidate,key,0);
    host.WriteWord(Add(candidate,0x14),unchecked((uint)(0x8d-key)));
   }
   host.ReadGameBytes(owner);
   if(host.CheckIfSwapped(owner)!=0)return 0;
   return (byte)(host.InvokeByte(Read(Add(Read(owner),0x2c)),owner)^1);
  }
  public void InitDefaultLimits(uint owner)
  {
   if(Read(Add(owner,0x6c))!=uint.MaxValue&&Read(Add(owner,0x78))!=uint.MaxValue&&Read(Add(owner,0x90))!=uint.MaxValue&&Read(Add(owner,0xb4))!=uint.MaxValue)return;
   SetDefault(0x100,0x32c);SetDefault(0xe4,0x330);SetDefault(0x124,0x338);SetDefault(0x118,0x33c);SetDefault(0x134,0x340);
  }
  private void SetDefault(uint setterOffset,uint getterOffset)
  {
   uint owner=Read(layout.MachineDataSlot),table=Read(owner),setter=Read(Add(table,setterOffset));
   ulong value=host.InvokeGet64(Read(Add(table,getterOffset)),owner);
   host.InvokeSet64(setter,Read(layout.MachineDataSlot),value);
  }
 }
}
