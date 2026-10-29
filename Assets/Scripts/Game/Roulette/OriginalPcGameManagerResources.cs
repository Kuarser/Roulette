using System;
using System.IO;
using UnityEngine;
namespace RouletteDeluxe
{
 // PC ownership and allocation are admitted ports. Getter dispatch and fields retain
 // the recovered GameMgrServer interface layout; no original program is executed.
 public sealed class OriginalPcGameManagerResources:IDisposable
 {
  [Serializable] private sealed class Table{public uint address;public byte[] bytes;}
  [Serializable] private sealed class Bindings{public Table gameManagerTable;public uint gameManagerPrimaryVtable,gameManagerInterfaceVtable,gameManagerInterfaceAdjustment,simulationThunk,simulationOffset,debugThunk,debugOffset;}
  public const uint GlobalInterfaceSlot=0x9e85c48;
  private readonly OriginalNativeMemory memory;private readonly OriginalGameManagerSimulationGetters getters;private bool disposed;
  public uint PrimaryOwner{get;private set;}
  public uint InterfaceOwner{get{return unchecked(PrimaryOwner+40);}}
  public OriginalPcGameManagerResources(OriginalNativeMemory memory,OriginalPcBootResourceCatalog catalog)
  {
   if(memory==null||catalog==null)throw new ArgumentException("Shared native memory and verified PC boot catalog required");this.memory=memory;
   var b=JsonUtility.FromJson<Bindings>(File.ReadAllText(catalog.Binding("jurisdiction_read_bindings_v1.json")));
   if(b==null||b.gameManagerTable==null||b.gameManagerTable.address!=0x96a8360||b.gameManagerTable.bytes==null||b.gameManagerTable.bytes.Length!=2504||b.gameManagerPrimaryVtable!=0x96a8368||b.gameManagerInterfaceVtable!=0x96a8878||b.gameManagerInterfaceAdjustment!=40||b.simulationThunk!=0x82c2bd0||b.simulationOffset!=0x108||b.debugThunk!=0x82c2bf0||b.debugOffset!=0x10c)throw new InvalidDataException("Original GameMgrServer layout mismatch");
   // The caller maps the global pointer slot, shared with the remaining boot owners.
   if(memory.ReadWord(GlobalInterfaceSlot)!=0)throw new InvalidOperationException("GameMgrServer interface is already owned");
   bool mapped=true;try{memory.ReadByte(b.gameManagerTable.address);}catch(InvalidOperationException){mapped=false;}
   if(!mapped)memory.Map(b.gameManagerTable.address,b.gameManagerTable.bytes);
   else{byte[] live=memory.ReadBytes(b.gameManagerTable.address,(uint)b.gameManagerTable.bytes.Length);for(int i=0;i<live.Length;++i)if(live[i]!=b.gameManagerTable.bytes[i])throw new InvalidDataException("Shared GameMgrServer table differs from original binding");}
   if(memory.ReadWord(b.gameManagerInterfaceVtable+0x108)!=b.simulationThunk||memory.ReadWord(b.gameManagerInterfaceVtable+0x10c)!=b.debugThunk)throw new InvalidDataException("Original GameMgrServer dispatch mismatch");
   getters=new OriginalGameManagerSimulationGetters(memory);PrimaryOwner=memory.Allocate(0x110);
   memory.WriteWord(PrimaryOwner,b.gameManagerPrimaryVtable);memory.WriteWord(InterfaceOwner,b.gameManagerInterfaceVtable);
   memory.WriteWord(PrimaryOwner+0x108,1);memory.WriteWord(PrimaryOwner+0x10c,0);memory.WriteWord(GlobalInterfaceSlot,InterfaceOwner);
  }
  public uint Invoke(uint method,uint capturedInterfaceOwner){if(disposed)throw new ObjectDisposedException("OriginalPcGameManagerResources");return getters.Invoke(method,capturedInterfaceOwner);}
  public uint GetSimulated(){return Dispatch(0x108);}
  public uint GetGdbDebug(){return Dispatch(0x10c);}
  private uint Dispatch(uint slot){if(disposed)throw new ObjectDisposedException("OriginalPcGameManagerResources");uint owner=memory.ReadWord(GlobalInterfaceSlot),table=memory.ReadWord(owner),method=memory.ReadWord(table+slot);return getters.Invoke(method,owner);}
  public void Dispose(){if(disposed)return;if(memory.ReadWord(GlobalInterfaceSlot)==InterfaceOwner)memory.WriteWord(GlobalInterfaceSlot,0);memory.Free(PrimaryOwner);disposed=true;}
 }
}
