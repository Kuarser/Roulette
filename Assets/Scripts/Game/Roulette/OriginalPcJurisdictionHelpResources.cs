using System;
using System.Collections.Generic;
using System.IO;
namespace RouletteDeluxe
{
 // PC file materialization and failure disposal are resource ports. Successful
 // parsing, COW operations, map insertion, rebalance and lookup use recovered bodies.
 public sealed class OriginalPcJurisdictionHelpResources:IDisposable
 {
  private readonly OriginalNativeMemory memory;private readonly OriginalConfigurationMemoryStrings strings;
  private readonly uint owner,frame;private readonly OriginalJurisdictionHelpParsing parser;private readonly OriginalJurisdictionHelpLookup lookup;
  private readonly HashSet<uint> nodes=new HashSet<uint>(),liveStrings=new HashSet<uint>();private bool busy,disposed;
  public int LiveNodes{get{return nodes.Count;}}public int LiveStrings{get{return liveStrings.Count;}}
  public uint Count{get{return memory.ReadWord(owner+0x28);}}
  public OriginalPcJurisdictionHelpResources(OriginalNativeMemory memory,OriginalConfigurationMemoryStrings strings,uint jurisdictionOwner,uint scratchFrame,uint emptyRepSlot,uint threadSlot,uint emptyCString,uint errorCString,Action<uint> error)
  {
   if(memory==null||strings==null||jurisdictionOwner==0||scratchFrame==0||error==null)throw new ArgumentException("Shared memory, COW owner, jurisdiction, 0x300-byte frame and error port required");
   this.memory=memory;this.strings=strings;owner=jurisdictionOwner;frame=scratchFrame;
   if(memory.ReadWord(owner+0x1c)!=0||memory.ReadWord(owner+0x28)!=0||memory.ReadWord(owner+0x20)!=owner+0x18||memory.ReadWord(owner+0x24)!=owner+0x18)throw new InvalidOperationException("Constructed empty jurisdiction help tree required");
   var cow=new OriginalConfigurationCowString(new OriginalConfigurationCowString.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,ReadByte=memory.ReadByte,WriteByte=memory.WriteByte,Allocate=memory.Allocate,Free=memory.Free,AtomicAdd=memory.AtomicAdd,CopyBytes=memory.CopyBytes,ThrowLengthError=error},emptyRepSlot,threadSlot,errorCString);
   var mutation=new OriginalConfigurationCowMutation(new OriginalConfigurationCowMutation.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,ReadByte=memory.ReadByte,WriteByte=memory.WriteByte,CopyBytes=memory.CopyBytes,MoveBytes=memory.MoveBytes,CreateRep=cow.Create,ReleaseRep=cow.Release},emptyRepSlot);
   OriginalConfigurationStringLeak leak=null;leak=new OriginalConfigurationStringLeak(new OriginalConfigurationStringLeak.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,Mutate=mutation.Mutate,InvokeLeakHard=a=>leak.LeakHard(a)},emptyRepSlot);
   OriginalConfigurationRbTree rb=null;rb=new OriginalConfigurationRbTree(new OriginalConfigurationRbTree.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,InvokeMutableDecrement=a=>rb.Decrement(a),InvokeMutableIncrement=a=>rb.Increment(a)});
   var tree=new OriginalJurisdictionHelpTree(new OriginalJurisdictionHelpTree.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,WriteByte=memory.WriteByte,New=n=>{uint node=memory.Allocate(n);nodes.Add(node);return node;},Delete=a=>{memory.Free(a);nodes.Remove(a);},CopyString=Copy,DestroyString=Destroy,Decrement=rb.Decrement,CompareBytes=Compare,Rebalance=rb.InsertAndRebalance,IsNativeUnwind=e=>false,BeginCatch=e=>{throw e;},Rethrow=e=>{throw e;},Resume=e=>{throw e;},EndCatch=()=>{throw new InvalidOperationException("No native exception transport admitted");}});
   var slot=new OriginalJurisdictionHelpMapSlot(new OriginalJurisdictionHelpMapSlot.Host{ReadWord=memory.ReadWord,WriteWord=(a,v)=>{memory.WriteWord(a,v);if(a==frame+0x238)liveStrings.Add(a);},CopyString=Copy,DestroyString=Destroy,Decrement=rb.Decrement,Increment=rb.Increment,CompareBytes=Compare,Insert=(a,b,c,d,e)=>{tree.Insert(a,b,c,d,e);},InsertUnique=(a,b,c)=>{tree.InsertUnique(a,b,c,frame+0x270);},IsNativeUnwind=e=>false,Cleanup=(a,b,c,e)=>{throw e;}},memory.ReadWord(emptyRepSlot)+12);
   var substring=new OriginalConfigurationSubstring(new OriginalConfigurationSubstring.Host{ReadWord=memory.ReadWord,WriteWord=(a,v)=>{memory.WriteWord(a,v);liveStrings.Add(a);},MemChr=(a,ch,n)=>{for(uint i=0;i<n;++i)if(memory.ReadByte(a+i)==unchecked((byte)ch))return a+i;return 0;},ConstructMutableRange=strings.ConstructRange,ThrowOutOfRange=error},errorCString,frame+0x278);
   parser=new OriginalJurisdictionHelpParsing(new OriginalJurisdictionHelpParsing.Host{ReadWord=memory.ReadWord,WriteWord=memory.WriteWord,ReadByte=memory.ReadByte,LeakString=leak.Leak,Find=substring.Find,ConstructSubstring=substring.Construct,GetValueSlot=slot.GetValueSlot,AssignString=(a,b)=>{strings.Assign(a,b);},DestroyString=Destroy,ThrowOutOfRange=error,IsNativeUnwind=e=>false,Cleanup=(a,b,c,e)=>{throw e;}},errorCString);
   lookup=new OriginalJurisdictionHelpLookup(new OriginalJurisdictionHelpLookup.Host{ReadWord=memory.ReadWord,CompareBytes=Compare,CopyString=Copy,ConstructLiteral=(a,b,c)=>{strings.Construct(a,b,c);liveStrings.Add(a);}},emptyCString,frame+0x278);
  }
  private int Compare(uint a,uint b,uint size){for(uint i=0;i<size;++i){int delta=memory.ReadByte(a+i)-memory.ReadByte(b+i);if(delta!=0)return delta;}return 0;}
  private void Copy(uint a,uint b){strings.Copy(a,b);liveStrings.Add(a);}
  private void Destroy(uint a){strings.Destroy(a);liveStrings.Remove(a);}
  private void Enter(){if(disposed)throw new ObjectDisposedException("OriginalPcJurisdictionHelpResources");if(busy)throw new InvalidOperationException("Shared help frame is already in use");busy=true;}
  private void ClearFrameStrings(){var pending=new List<uint>();foreach(uint a in liveStrings)if(unchecked(a-frame)<0x300)pending.Add(a);foreach(uint a in pending)Destroy(a);}
  public uint Load(OriginalPcBootResourceCatalog catalog){if(catalog==null)throw new ArgumentNullException("catalog");return Parse(File.ReadAllBytes(catalog.Resolve("/agk/cfg/SuperConfig/JurisdictionHelp")));}
  public uint Parse(byte[] input)
  {
   if(input==null)throw new ArgumentNullException("input");Enter();uint bytes=0;
   try{bytes=memory.Allocate((uint)Math.Max(1,input.Length));memory.WriteBytes(bytes,input);strings.ConstructRange(frame+0x228,bytes,bytes+(uint)input.Length,frame+0x278);liveStrings.Add(frame+0x228);memory.WriteWord(frame+0x24,frame+0x228);memory.WriteWord(frame+0x260,owner);return parser.Parse(frame);}
   catch{ClearNodes();throw;}
   finally{ClearFrameStrings();if(bytes!=0)memory.Free(bytes);busy=false;}
  }
  public byte[] GetHelpText(byte[] key)
  {
   if(key==null)throw new ArgumentNullException("key");Enter();uint bytes=0;
   try{bytes=memory.Allocate((uint)Math.Max(1,key.Length));memory.WriteBytes(bytes,key);strings.ConstructRange(frame+0x280,bytes,bytes+(uint)key.Length,frame+0x278);liveStrings.Add(frame+0x280);lookup.GetHelpText(frame+0x284,owner,frame+0x280);return strings.Read(frame+0x284);}
   finally{ClearFrameStrings();if(bytes!=0)memory.Free(bytes);busy=false;}
  }
  private void ClearNodes(){foreach(uint node in nodes){if(liveStrings.Contains(node+0x14))Destroy(node+0x14);if(liveStrings.Contains(node+0x10))Destroy(node+0x10);memory.Free(node);}nodes.Clear();memory.WriteWord(owner+0x18,0);memory.WriteWord(owner+0x1c,0);memory.WriteWord(owner+0x20,owner+0x18);memory.WriteWord(owner+0x24,owner+0x18);memory.WriteWord(owner+0x28,0);}
  public void Dispose(){if(disposed)return;if(busy)throw new InvalidOperationException("Shared help frame is already in use");ClearNodes();ClearFrameStrings();disposed=true;}
 }
}
