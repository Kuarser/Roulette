using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace RouletteDeluxe
{
 // Recovered parser controls on caller memory; Xerces DOM, streams and imported runtime are ports.
 public sealed class OriginalPhysicalCfgParser:IDisposable
 {
  private sealed class NativeError:Exception{internal uint Address,Wide;internal int Selector;internal NativeError(string message):base(message){Selector=1;}}
  public sealed class ParsedRoot
  {
   private readonly OriginalPhysicalCfgParser parser;private readonly uint owner;private readonly OriginalCfgDocument.Node node;private bool deleted;
   internal ParsedRoot(OriginalPhysicalCfgParser p,uint a,OriginalCfgDocument.Node n){parser=p;owner=a;node=n;}
   public uint Owner{get{return owner;}}public uint RootPointer{get{Alive();return parser.m.ReadWord(owner+4);}}
   public OriginalCfgDocument.Node Node{get{Alive();return node;}}
   private void Alive(){if(deleted||parser.disposed)throw new ObjectDisposedException("Physical CFG parsed root");}
   public void Delete(){Alive();parser.parsedOwners.D0(owner);node.Delete();deleted=true;--parser.ActiveRoots;++parser.ParsedDeletes;}
  }
  private readonly OriginalNativeMemory m;private readonly bool atomic;private bool disposed;private readonly byte[] memoryBytes;private readonly string filePath;
  private OriginalConfigurationMemoryStrings strings;private OriginalCfgParserLifetime lifetime;private OriginalCfgParserResources resources;private OriginalCfgParserBuildDocument build;private OriginalCfgParsedDataOwner parsedOwners;private OriginalCfgFactory factory;
  private OriginalCfgDocument document;private uint domParser,domDocument,domRoot;private readonly HashSet<uint> inputSources=new HashSet<uint>();private readonly Dictionary<uint,NativeError> errors=new Dictionary<uint,NativeError>();private readonly Stack<NativeError> catches=new Stack<NativeError>();private NativeError lastError;
  private readonly List<string> output=new List<string>();private string pending="";
  public uint CountedInputLength{get;private set;}public int CountedInputNuls{get;private set;}public uint Owner{get;private set;}public int ActiveRoots{get;private set;}public int Initializations{get;private set;}public int Terminations{get;private set;}public int BuildCalls{get;private set;}public int WrapperCreates{get;private set;}public int WrapperDeletes{get;private set;}public int ParsedCreates{get;private set;}public int ParsedDeletes{get;private set;}public int NativeParserDeletes{get;private set;}public int DiagnosticInputDeletes{get;private set;}public int ErrorCreates{get;private set;}public int ErrorDeletes{get;private set;}public int FactoryGuardAcquires{get;private set;}public int FactoryExitRegistrations{get;private set;}
  public string[] Messages{get{return output.ToArray();}}public bool HasDocument{get{Alive();return m.ReadWord(Owner+8)!=0;}}
  private void Alive(){if(disposed)throw new ObjectDisposedException("OriginalPhysicalCfgParser");}
  public OriginalPhysicalCfgParser(OriginalNativeMemory memory,byte[] bytes,bool atomic,OriginalPhysicalCfgTypeEnvironment environment=null):this(memory,bytes,null,atomic,environment){}
  public static OriginalPhysicalCfgParser OpenFile(OriginalNativeMemory memory,string path,bool atomic,OriginalPhysicalCfgTypeEnvironment environment=null){if(path==null)throw new ArgumentNullException("path");return new OriginalPhysicalCfgParser(memory,null,path,atomic,environment);}
  private OriginalPhysicalCfgParser(OriginalNativeMemory memory,byte[] bytes,string path,bool atomic,OriginalPhysicalCfgTypeEnvironment environment)
  {
   if(memory==null||(bytes==null&&path==null))throw new ArgumentException("Caller memory and original configuration input required");if(environment!=null)environment.Validate(memory,atomic);m=memory;this.atomic=atomic;memoryBytes=bytes==null?null:(byte[])bytes.Clone();filePath=path;Initialize();
   uint text=ConstructBytes(path==null?memoryBytes:Encoding.ASCII.GetBytes(path)),name=0;
   try{if(path==null){name=ConstructBytes(Encoding.ASCII.GetBytes("jurisdiction.cfg"));Owner=lifetime.CreateMemory(0,text,name);}else Owner=lifetime.CreateHybridFile(0,text);}
   finally{if(name!=0){strings.Destroy(name);m.Free(name);}strings.Destroy(text);m.Free(text);}
  }
  private void Ensure(uint address,byte[] bytes,bool immutable=false)
  {
   int index=0;while(index<bytes.Length){bool mapped;byte value=0;try{value=m.ReadByte(unchecked(address+(uint)index));mapped=true;}catch(InvalidOperationException){mapped=false;}
    if(mapped){if(immutable&&value!=bytes[index])throw new InvalidOperationException("Conflicting physical parser immutable binding");++index;continue;}
    int first=index;while(++index<bytes.Length){try{m.ReadByte(unchecked(address+(uint)index));break;}catch(InvalidOperationException){}}
    var span=new byte[index-first];Array.Copy(bytes,first,span,0,span.Length);m.Map(unchecked(address+(uint)first),span);
   }
  }
  private uint ConstructBytes(byte[] bytes)
  {
   uint source=m.Allocate((uint)Math.Max(bytes.Length,1)),owner=m.Allocate(4);m.WriteBytes(source,bytes);try{strings.ConstructRange(owner,source,source+(uint)bytes.Length,0x89f00);return owner;}finally{m.Free(source);}
  }
  private void Initialize()
  {
   uint threadSelector=atomic?1u:0u;try{threadSelector=m.ReadWord(0x9b12dbc);if((threadSelector!=0)!=atomic)throw new ArgumentException("Parser requires the caller's reference-count mode");}catch(InvalidOperationException){}
   Ensure(0x9b11e00,new byte[0x1200]);Ensure(0x9b482a0,new byte[16]);Ensure(0xa13c378,new byte[24]);Ensure(0x89f00,new byte[0x1100]);
   Ensure(0x9b0ab40u,new byte[]{0,0,0,0,28,171,176,9,96,255,63,9,0,0,64,9,176,255,63,9,208,255,63,9,128,250,63,9,0,254,63,9,144,255,63,9,16,255,63,9,48,255,63,9},true);
   Ensure(0x9b0ac78u,new byte[]{0,0,0,0,68,172,176,9,0,11,64,9,96,11,64,9,160,11,64,9},true);
   Ensure(0x9b0ace8u,new byte[]{0,0,0,0,184,172,176,9,128,25,64,9,48,27,64,9,116,184,10,8},true);
   Ensure(0x9b0ad18u,new byte[]{0,0,0,0,52,173,176,9,32,26,64,9,64,26,64,9,0,28,64,9,176,172,69,8,96,27,64,9},true);
   Ensure(0x9b0ad60u,new byte[]{0,0,0,0,64,173,176,9,160,58,64,9,16,59,64,9,176,41,64,9,144,52,64,9,112,40,64,9,160,30,64,9,224,43,64,9,208,50,64,9,240,33,64,9,64,30,64,9,0,54,64,9,64,46,64,9,240,29,64,9,96,31,64,9,144,29,64,9,176,29,64,9,112,29,64,9,128,29,64,9,144,32,64,9},true);
   Ensure(0x9b0aa60u,new byte[]{0,0,0,0,20,171,176,9,32,252,63,9,208,253,63,9,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8,116,184,10,8},true);
   Ensure(0x9b0acc0u,new byte[]{0,0,0,0,220,172,176,9,0,26,64,9,112,26,64,9,48,28,64,9,176,172,69,8,176,27,64,9},true);
   Ensure(0x970f9c5u,new byte[]{76,83,0},true);
   Ensure(0x975cf38u,new byte[]{85,110,101,120,112,101,99,116,101,100,32,69,120,99,101,112,116,105,111,110,32,10,0},true);
   Ensure(0x975cf50u,new byte[]{69,114,114,111,114,32,100,117,114,105,110,103,32,105,110,105,116,105,97,108,105,122,97,116,105,111,110,33,32,58,10,0},true);
   Ensure(0x96d5861u,new byte[]{10,0},true);
   Ensure(0x975cf02u,new byte[]{10,68,79,77,32,69,114,114,111,114,32,100,117,114,105,110,103,32,112,97,114,115,105,110,103,58,32,39,0},true);
   Ensure(0x96b90d6u,new byte[]{39,10,0},true);
   Ensure(0x975cf1fu,new byte[]{68,79,77,69,120,99,101,112,116,105,111,110,32,99,111,100,101,32,105,115,58,32,32,10,0},true);
   Ensure(0x975ceeau,new byte[]{69,120,99,101,112,116,105,111,110,32,109,101,115,115,97,103,101,32,105,115,58,32,10,0},true);
   m.WriteWord(0x9b11fa8u,0x9b0b020u);
   m.WriteWord(0x9b11fecu,0x9b0ad18u);
   m.WriteWord(0x9b12074u,0x9b0b180u);
   m.WriteWord(0x9b12204u,0x9b14f64u);
   m.WriteWord(0x9b12500u,0x9b0ac78u);
   m.WriteWord(0x9b125ecu,0x9b0acc0u);
   m.WriteWord(0x9b12790u,0x9b0ace8u);
   m.WriteWord(0x9b127e0u,0x93fff10u);
   m.WriteWord(0x9b12bc0u,0x9b0ab40u);
   m.WriteWord(0x9b12c90u,0x9b0aa60u);
   m.WriteWord(0x9b12e78u,0x9b0ad60u);
   m.WriteWord(0x9b11f28,0x9b482a0);m.WriteWord(0x9b12dbc,threadSelector);
   // Diagnostic imported Xerces objects and virtual methods, outside native-runtime qualification.
   Ensure(0xf2000000,new byte[0x800]);m.WriteWord(0x9b120d8,0xf2000700);m.WriteWord(0x9b122f4,0xf2000300);m.WriteWord(0xf2000300,0xf2000400);m.WriteWord(0x9b12eb8,0xf2000710);m.WriteWord(0x9b12b8c,0x9b0ac60);m.WriteWord(0x9b1251c,0x9b0ac90);m.WriteWord(0x9b128ac,0xf2000720);m.WriteWord(0x9b12af0,0xf2000730);
   m.WriteWord(0xf2000200,0xf2000100);m.WriteWord(0xf2000108,0xf0010001);m.WriteWord(0xf2000520,0xf0010002);m.WriteWord(0xf200052c,0xf0010003);m.WriteWord(0xf2000538,0xf0010004);m.WriteWord(0xf2000540,0xf0010005);m.WriteWord(0xf2000550,0xf0010006);m.WriteWord(0xf2000634,0xf0010007);
   strings=new OriginalConfigurationMemoryStrings(m,0x9b11f28,0x9b12dbc,1,2,a=>{throw new Exception("length");},a=>{throw new Exception("logic");});
   parsedOwners=new OriginalCfgParsedDataOwner(new OriginalCfgParsedDataOwner.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,New=m.Allocate,Delete=m.Free},new OriginalCfgParsedDataOwner.Layout{ConcreteVtableSlot=0x9b12e78,InterfaceVtableSlot=0x9b12c90});
   resources=new OriginalCfgParserResources(new OriginalCfgParserResources.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,New=m.Allocate,AtomicAdd=m.AtomicAdd,TranscodeWide=wide=>{var chars=new List<char>();for(uint a=wide;m.ReadHalf(a)!=0;a+=2)chars.Add((char)m.ReadHalf(a));byte[] b=Encoding.ASCII.GetBytes(new string(chars.ToArray())+"\0");uint p=m.Allocate((uint)b.Length);m.WriteBytes(p,b);return p;},DeleteArray=m.Free,ConstructCString=strings.Construct,InvokeDispose=(method,a)=>{if(method!=0x9401c00&&method!=0x9401c30)throw new InvalidOperationException("Original count dispose method required");resources.DisposeArray(a);},InvokeDestroy=(method,a)=>{if(method!=0x845acb0)throw new InvalidOperationException("Original count destroy method required");m.Free(a);},IsNativeUnwind=e=>e is NativeError,BeginCatch=e=>catches.Push((NativeError)e),Rethrow=e=>{throw e;},EndCatch=EndCatch,Resume=e=>{throw e;}},new OriginalCfgParserResources.Layout{NarrowCountVtableSlot=0x9b125ec});
   factory=new OriginalCfgFactory(new OriginalCfgFactory.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,New=m.Allocate,Delete=m.Free,GuardAcquire=a=>{++FactoryGuardAcquires;return m.ReadByte(a)==0?1:0;},GuardRelease=a=>m.WriteByte(a,1),RegisterAtExit=(a,b,c)=>{++FactoryExitRegistrations;return 0;},IsNativeUnwind=e=>e is NativeError,Resume=e=>{throw e;}},new OriginalCfgFactory.Layout{Guard=0xa13c378,Instance=0xa13c380,FactoryVtableSlot=0x9b12bc0,BaseDictionaryVtableSlot=0x9b11fa8,HybridDictionaryVtableSlot=0x9b12074,DsoHandleSlot=0x9b12204,DestructorSlot=0x9b127e0});
   build=new OriginalCfgParserBuildDocument(new OriginalCfgParserBuildDocument.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>e is NativeError,ExceptionAddress=e=>((NativeError)e).Address,ExceptionSelector=e=>(uint)((NativeError)e).Selector},new OriginalCfgParserBuildDocument.Layout{LocaleSlot=0x9b120d8,WideCountVtableSlot=0x9b11fec,MemoryManagerSlot=0x9b122f4,ValidateIfSchemaSlot=0x9b12eb8,ErrorHandlerVtableSlot=0x9b12b8c,BaseErrorHandlerVtableSlot=0x9b1251c,CoutSlot=0x9b128ac,CerrSlot=0x9b12af0});
   lifetime=new OriginalCfgParserLifetime(new OriginalCfgParserLifetime.Host{ReadWord=m.ReadWord,WriteWord=m.WriteWord,New=m.Allocate,Delete=a=>{m.Free(a);++NativeParserDeletes;},BuildFile=(a,b)=>{++BuildCalls;build.BuildFile(a,b,0x8a000);},BuildMemory=(a,b,c)=>{++BuildCalls;build.BuildMemory(a,b,c,0x8a000);},ReleaseDocument=(method,a)=>{if(method!=0xf0010006||a!=domParser)throw new InvalidOperationException("Diagnostic Xerces release port required");if(document!=null){document.Dispose();document=null;m.Free(domRoot);m.Free(domDocument);}m.Free(domParser);},Terminate=()=>++Terminations,CreateFactory=factory.Create,CreateParsedData=(method,a)=>{if(method!=0x9400000)throw new InvalidOperationException("Native Hybrid parsed-data factory required");++ParsedCreates;return parsedOwners.CreateHybrid(a);},GetDocumentRoot=(method,a)=>{if(method!=0xf0010007||a!=domDocument)throw new InvalidOperationException("Diagnostic DOM root port required");return domRoot;},SetRoot=(method,a,b)=>{if(method!=0x9401d70)throw new InvalidOperationException("Native SetRoot method required");parsedOwners.SetRoot(a,b);},IsNativeUnwind=e=>e is NativeError,Resume=e=>{throw e;}},new OriginalCfgParserLifetime.Layout{ParserVtableSlot=0x9b12500,InterfaceVtableSlot=0x9b12790});
  }
  private void EndCatch()
  {
   NativeError error=catches.Pop();m.Free(error.Wide);m.Free(error.Address);errors.Remove(error.Address);++ErrorDeletes;output.Add(pending);pending="";
  }
  private NativeError Error(Exception exception)
  {
   byte[] wide=Encoding.Unicode.GetBytes(exception.Message+"\0");var error=new NativeError(exception.Message){Address=m.Allocate(20),Wide=m.Allocate((uint)wide.Length)};m.WriteBytes(error.Wide,wide);m.WriteWord(error.Address+16,error.Wide);errors.Add(error.Address,error);++ErrorCreates;lastError=error;return error;
  }
  private uint Invoke(uint method,uint[] a)
  {
   switch(method)
   {
    case 0x80ac464:++Initializations;return 0;
    case 0x80aa3d4:if(Encoding.ASCII.GetString(m.ReadBytes(a[0],m.StrLen(a[0])))!="LS")throw new InvalidOperationException("Original LS feature required");uint feature=m.Allocate(6);m.WriteBytes(feature,Encoding.Unicode.GetBytes("LS\0"));return feature;
    case 0x80aad84:return m.Allocate(a[0]);case 0x80b1344:return 0xf2000200;
    case 0xf0010001:domParser=m.Allocate(4);m.WriteWord(domParser,0xf2000500);return domParser;
    case 0xf0010002:case 0xf0010003:return 0;
    case 0x80acca4:uint input=m.Allocate(a[0]);inputSources.Add(input);return input;
    case 0x80b1ad4:m.WriteWord(a[0]+4,a[1]);m.WriteWord(a[0]+8,a[2]);return 0;
    case 0x80aa8d4:m.WriteWord(a[0]+4,a[1]);++WrapperCreates;return 0;
    case 0x80ab044:++WrapperDeletes;return 0;
    case 0x80abc14:m.Free(a[0]);inputSources.Remove(a[0]);return 0;
    case 0xf0010004:case 0xf0010005:
     try{if(method==0xf0010005)document=OriginalCfgDocument.OpenFile(filePath,Encoding.ASCII);else{uint source=m.ReadWord(a[1]+4);CountedInputLength=m.ReadWord(source+8);byte[] bytes=m.ReadBytes(m.ReadWord(source+4),CountedInputLength);foreach(byte b in bytes)if(b==0)++CountedInputNuls;document=OriginalCfgDocument.FromBytes(bytes,Encoding.ASCII);}}
     catch(System.Xml.XmlException e){throw Error(e);}catch(IOException e){throw Error(e);}
     domDocument=m.Allocate(4);domRoot=m.Allocate(4);m.WriteWord(domDocument,0xf2000600);m.WriteWord(domRoot,0x524f4f54);return domDocument;
    case 0x9400c00:resources.DestroySharedCount(a[0]);return 0;
    case 0x9401c00:case 0x9401c30:resources.DisposeArray(a[0]);return 0;
    case 0x845acb0:m.Free(a[0]);return 0;
    case 0x80adff4:lastError=errors[a[0]];catches.Push(lastError);return a[0];
    case 0x80b1144:EndCatch();return 0;case 0x80b1284:case 0x80aed64:throw lastError;
    case 0x80ae7f4:m.Free(a[0]);return 0;
    case 0x9401c60:return resources.StringTranscode(a[0],a[1],0x89fc0);
    case 0x80add34:strings.Destroy(a[0]);return 0;
    case 0x80b0c04:pending+=Encoding.ASCII.GetString(m.ReadBytes(a[1],m.StrLen(a[1])));return a[0];
    case 0x80adde4:pending+=Encoding.ASCII.GetString(strings.Read(a[1]));return a[0];
    case 0x80ac834:pending+=unchecked((int)a[1]).ToString(System.Globalization.CultureInfo.InvariantCulture);return a[0];
    case 0x80ab984:pending+="\n";return a[0];default:throw new InvalidOperationException("Unbound physical parser port "+method.ToString("x8"));
   }
  }
  public ParsedRoot GetParsedRoot()
  {
   Alive();if(!HasDocument)throw new OriginalCfgPathException("Diagnostic facade rejects GetParsedData after a swallowed parse error");uint owner=lifetime.GetParsedData(Owner);++ActiveRoots;return new ParsedRoot(this,owner,document.GetParsedData());
  }
  public void Dispose()
  {
   if(disposed)return;if(ActiveRoots!=0)throw new InvalidOperationException("Delete parsed-data owners before their parser");lifetime.ParserD0(Owner);
   // Separate diagnostic ownership of imported MemBuf allocations; original Wrapper D1 does not free them.
   foreach(uint input in inputSources){m.Free(input);++DiagnosticInputDeletes;}inputSources.Clear();if(errors.Count!=0||catches.Count!=0)throw new InvalidOperationException("Unconsumed diagnostic exception owner");disposed=true;
  }
 }
}
