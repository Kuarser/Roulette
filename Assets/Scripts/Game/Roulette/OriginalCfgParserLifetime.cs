using System;
namespace RouletteDeluxe
{
 // Original parser ownership controls. BuildDocument and Xerces runtime remain required ports.
 public sealed class OriginalCfgParserLifetime
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Action<uint,uint> WriteWord,BuildFile,ReleaseDocument;
   public Action<uint,uint,uint> BuildMemory,SetRoot;
   public Action<uint> Delete;
   public Action Terminate;
   public Func<uint> CreateFactory;
   public Func<uint,uint,uint> CreateParsedData,GetDocumentRoot;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ParserVtableSlot,InterfaceVtableSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgParserLifetime(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Parser lifetime host and relocated layout required");
   host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Parser lifetime binding required: "+f.Name);f.SetValue(host,v);}
   if(l.ParserVtableSlot==0||l.InterfaceVtableSlot==0)throw new ArgumentException("Parser vtable relocation required");layout=new Layout{ParserVtableSlot=l.ParserVtableSlot,InterfaceVtableSlot=l.InterfaceVtableSlot};
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception e){host.Resume(e);throw new InvalidOperationException("Original nonreturning parser Resume returned");}
  public void InterfaceD1(uint owner){uint table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));}
  public void ConcreteInterfaceD1(uint owner){InterfaceD1(owner);}
  public void InterfaceD0(uint owner){InterfaceD1(owner);host.Delete(owner);}
  public void ConcreteInterfaceD0(uint owner){InterfaceD1(owner);host.Delete(owner);}
  private void Initialize(uint owner)
  {
   host.WriteWord(A(owner,4),0);uint table=host.ReadWord(layout.ParserVtableSlot);host.WriteWord(A(owner,8),0);host.WriteWord(owner,A(table,8));
  }
  public void ConstructFile(uint owner,uint path)
  {
   Initialize(owner);try{host.BuildFile(owner,path);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;InterfaceD1(owner);Resume(e);}
  }
  public void ConstructMemory(uint owner,uint text,uint name)
  {
   Initialize(owner);try{host.BuildMemory(owner,text,name);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;InterfaceD1(owner);Resume(e);}
  }
  public uint CreateFile(uint factory,uint path)
  {
   uint owner=host.New(12);try{ConstructFile(owner,path);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.Delete(owner);Resume(e);}return owner;
  }
  public uint CreateMemory(uint factory,uint text,uint name)
  {
   uint owner=host.New(12);try{ConstructMemory(owner,text,name);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.Delete(owner);Resume(e);}return owner;
  }
  public uint CreateHybridFile(uint factory,uint path){return CreateFile(factory,path);}
  public uint GetParsedData(uint owner)
  {
   uint factory=host.CreateFactory(),table=host.ReadWord(factory),method=host.ReadWord(A(table,4));uint parsed=host.CreateParsedData(method,factory);
   table=host.ReadWord(parsed);uint setRoot=host.ReadWord(A(table,64));uint document=host.ReadWord(A(owner,8));table=host.ReadWord(document);method=host.ReadWord(A(table,52));uint root=host.GetDocumentRoot(method,document);host.SetRoot(setRoot,parsed,root);return parsed;
  }
  public void ParserD1(uint owner)
  {
   uint table=host.ReadWord(layout.ParserVtableSlot);host.WriteWord(owner,A(table,8));uint document=host.ReadWord(A(owner,4));table=host.ReadWord(document);
   try{uint method=host.ReadWord(A(table,80));host.ReleaseDocument(method,document);host.Terminate();}catch(Exception e){if(!host.IsNativeUnwind(e))throw;InterfaceD1(owner);Resume(e);}InterfaceD1(owner);
  }
  public void ParserD0(uint owner){ParserD1(owner);host.Delete(owner);}
 }
}
