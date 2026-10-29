using System;
namespace RouletteDeluxe
{
 // Full CFG factory wrappers, Hybrid singleton and dictionary creation/construction bodies.
 public sealed class OriginalCfgFactory
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,int> GuardAcquire;
   public Action<uint> GuardRelease,Delete;
   public Func<uint,uint,uint,int> RegisterAtExit;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint Guard,Instance,FactoryVtableSlot,BaseDictionaryVtableSlot,HybridDictionaryVtableSlot,DsoHandleSlot,DestructorSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgFactory(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual CFG factory host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual CFG factory binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual CFG factory relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  public uint CreateHybrid()
  {
   uint instance=layout.Instance;
   if(host.ReadByte(layout.Guard)!=0)return instance;
   if(host.GuardAcquire(layout.Guard)==0)return instance;
   uint table=host.ReadWord(layout.FactoryVtableSlot);host.WriteWord(instance,A(table,8));host.GuardRelease(layout.Guard);
   uint dso=host.ReadWord(layout.DsoHandleSlot),destructor=host.ReadWord(layout.DestructorSlot);host.RegisterAtExit(destructor,instance,dso);return instance;
  }
  public uint Create(){return CreateHybrid();}
  public uint HasSingleton(){Create();return 1;}
  public void RemoveSingleton(){}
  public void ConstructDictionary(uint owner)
  {
   host.WriteWord(A(owner,8),0);host.WriteWord(A(owner,12),0);uint table=host.ReadWord(layout.BaseDictionaryVtableSlot);host.WriteWord(A(owner,24),0);host.WriteWord(owner,A(table,8));uint header=A(owner,8);host.WriteWord(A(owner,16),header);host.WriteWord(A(owner,20),header);
  }
  public uint CreateDictionary(bool hybrid)
  {
   uint owner=host.New(28);
   try{ConstructDictionary(owner);}
   catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Delete(owner);host.Resume(error);throw new InvalidOperationException("Original nonreturning CFG factory Resume returned");}
   if(hybrid){uint table=host.ReadWord(layout.HybridDictionaryVtableSlot);host.WriteWord(owner,A(table,8));}
   return owner;
  }
 }
}
