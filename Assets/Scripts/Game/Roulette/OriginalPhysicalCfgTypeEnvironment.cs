using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace RouletteDeluxe
{
 // Caller-owned module type lifetime. ios and at-exit dispatch are diagnostic ports.
 public sealed class OriginalPhysicalCfgTypeEnvironment:IDisposable
 {
  private static readonly ConditionalWeakTable<OriginalNativeMemory,OriginalPhysicalCfgTypeEnvironment> active=new ConditionalWeakTable<OriginalNativeMemory,OriginalPhysicalCfgTypeEnvironment>();
  private static readonly uint[] owners={0xa13c398u,0xa13c39cu,0xa13c3a0u,0xa13c3a4u,0xa13c3a8u,0xa13c3acu,0xa13c3b0u,0xa13c3b4u,0xa13c3b8u,0xa13c3bcu,0xa13c3c0u};private static readonly uint[] literals={0x975d4f6u,0x96c7d18u,0x975d503u,0x975d511u,0x975d6b9u,0x975d6afu,0x96c7d22u,0x975d51eu,0x975d527u,0x975d53au,0x96acab5u};private static readonly byte[][] literalBytes={new byte[]{117,110,107,110,111,119,110,32,116,121,112,101,0},new byte[]{105,110,116,51,50,0},new byte[]{117,110,115,105,103,110,101,100,73,110,116,51,50,0},new byte[]{117,110,115,105,103,110,101,100,73,110,116,56,0},new byte[]{100,111,117,98,108,101,0},new byte[]{102,108,111,97,116,0},new byte[]{115,116,114,105,110,103,0},new byte[]{82,71,66,67,111,108,111,114,0},new byte[]{74,117,114,105,115,100,105,99,116,105,111,110,79,112,116,105,111,110,0},new byte[]{74,117,114,105,115,100,105,99,116,105,111,110,79,112,116,105,111,110,83,116,114,0},new byte[]{74,117,114,105,115,100,105,99,116,105,111,110,79,112,116,105,111,110,66,105,116,0}};
  private readonly OriginalNativeMemory memory;private readonly OriginalConfigurationMemoryStrings strings;private readonly bool atomic;private bool disposed;
  private readonly List<uint[]> exits=new List<uint[]>();private readonly List<uint> constructed=new List<uint>();
  public int ActiveDictionaries{get;private set;}public int InitializationCalls{get;private set;}public int StringConstructors{get;private set;}public int StringDestructors{get;private set;}public int ExitRegistrations{get{return exits.Count;}}
  public OriginalNativeMemory Memory{get{return memory;}}public bool Atomic{get{return atomic;}}
  public uint[] TypeOwners{get{return (uint[])owners.Clone();}}
  public void Validate(OriginalNativeMemory candidate,bool threadAtomic){Alive();if(!ReferenceEquals(memory,candidate)||atomic!=threadAtomic)throw new ArgumentException("Type environment requires the same caller memory and reference-count mode");}
  private void Alive(){if(disposed)throw new ObjectDisposedException("OriginalPhysicalCfgTypeEnvironment");}
  internal void Acquire(){lock(memory){Alive();++ActiveDictionaries;}}
  internal void Release(){lock(memory){Alive();if(ActiveDictionaries<=0)throw new InvalidOperationException("No live dictionary lease");--ActiveDictionaries;}}
  public byte[] ReadType(int index){Alive();if(index<0||index>=11)throw new ArgumentOutOfRangeException("index");return strings.Read(owners[index]);}
  private void Ensure(uint address,byte[] bytes,bool immutable)
  {
   int index=0;while(index<bytes.Length){bool mapped;byte value=0;try{value=memory.ReadByte(unchecked(address+(uint)index));mapped=true;}catch(InvalidOperationException){mapped=false;}
    if(mapped){if(immutable&&value!=bytes[index])throw new InvalidOperationException("Conflicting original type literal binding");++index;continue;}
    int first=index;while(++index<bytes.Length){try{memory.ReadByte(unchecked(address+(uint)index));break;}catch(InvalidOperationException){}}
    var span=new byte[index-first];Array.Copy(bytes,first,span,0,span.Length);memory.Map(unchecked(address+(uint)first),span);
   }
  }
  public OriginalPhysicalCfgTypeEnvironment(OriginalNativeMemory memory,bool atomic)
  {
   if(memory==null)throw new ArgumentNullException("memory");this.memory=memory;this.atomic=atomic;
   lock(memory)
   {
    OriginalPhysicalCfgTypeEnvironment existing;if(active.TryGetValue(memory,out existing))throw new InvalidOperationException("One module type environment per caller memory required");
    Ensure(0x9b11e00,new byte[0x1200],false);Ensure(0x9b482a0,new byte[16],false);Ensure(0xa13c398,new byte[45],false);Ensure(0x80000,new byte[64],false);for(int i=0;i<11;++i)Ensure(literals[i],literalBytes[i],true);
    memory.WriteWord(0x9b11f28,0x9b482a0);memory.WriteWord(0x9b12dbc,atomic?1u:0);memory.WriteWord(0x9b12204,0x9b14f64);memory.WriteWord(0x9b12db4,0x11223344);memory.WriteWord(0x9b12728,0x80add34);
    strings=new OriginalConfigurationMemoryStrings(memory,0x9b11f28,0x9b12dbc,1,2,a=>{throw new Exception("length");},a=>{throw new Exception("logic");});active.Add(memory,this);
    try
    {
     var initializer=new OriginalCfgValueTypeInitialization(new OriginalCfgValueTypeInitialization.Host{ReadWord=memory.ReadWord,ConstructIo=a=>memory.WriteByte(a,1),ConstructCString=(a,b,c)=>{strings.Construct(a,b,c);constructed.Add(a);++StringConstructors;},RegisterAtExit=(a,b,c)=>{exits.Add(new[]{a,b,c});return 0;}},new OriginalCfgValueTypeInitialization.Layout{IoOwner=0xa13c3c4,DsoHandleSlot=0x9b12204,IoDestructorSlot=0x9b12db4,StringDestructorSlot=0x9b12728,Owners=owners,Literals=literals});initializer.Initialize(0x80000);++InitializationCalls;
    }
    catch
    {
     // Diagnostic owner cleanup, outside the recovered initializer's no-rollback control.
     for(int i=constructed.Count-1;i>=0;--i)strings.Destroy(constructed[i]);memory.WriteByte(0xa13c3c4,0);active.Remove(memory);throw;
    }
   }
  }
  public void Dispose()
  {
   lock(memory)
   {
    if(disposed)return;if(ActiveDictionaries!=0)throw new InvalidOperationException("Release dictionary owners before their module type environment");
    for(int i=exits.Count-1;i>=0;--i){uint[] e=exits[i];if(e[0]==0x80add34){strings.Destroy(e[1]);++StringDestructors;}else if(e[0]==0x11223344&&e[1]==0xa13c3c4)memory.WriteByte(e[1],0);else throw new InvalidOperationException("Unsupported diagnostic exit callback");}
    disposed=true;active.Remove(memory);
   }
  }
 }
}
