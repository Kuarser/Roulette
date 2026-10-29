using System;
namespace RouletteDeluxe
{
 // Complete InitHelpText composition with original call guards and cleanup paths.
 public sealed class OriginalJurisdictionInitHelpText
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Close,Decrement,Increment;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,AssignString,CopyString,TransferBuffer,Clear,InitIos,ConstructStringStream,ExtractString;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> ConstructLiteral,InsertUnique;
   public Func<uint,uint,uint,uint> Open,InsertBytes;
   public Func<uint,uint,uint> InsertCString;
   public Func<uint,byte,uint,uint> Find;
   public Action<uint,uint,uint,uint> ConstructSubstring;
   public Func<uint,uint,uint,int> CompareBytes;
   public Action<uint,uint,uint,uint,uint> Insert;
   public Action<uint> ConstructIosBase,ConstructFileBuffer,LeakString,DestroyString,DestroyStringStream,DestroyBasicFile,DestroyLocale,DestroyIosBase,DestroyStreamBuffer,DestroyIfstream,DestroyFileBuffer,EndLine,ThrowOutOfRange,DestroyPair;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  public sealed class Layout
  {
   public uint HelpCString,WarningCString,ErrorStream,IfstreamVttFirstSlot,IfstreamVttSecondSlot,IosBaseVtable,IfstreamVtable,IfstreamIosVtable,FileBufferVtable,StreamBufferVtable,EmptyStringData,OutOfRangeCString;
  }
  private readonly Host host;private readonly OriginalJurisdictionHelpStreamInitialization.Layout initializationLayout;
  private readonly OriginalJurisdictionHelpStreamConsumption consumption;private readonly OriginalJurisdictionHelpParsing parsing;private readonly OriginalJurisdictionHelpCleanup cleanup;
  public OriginalJurisdictionInitHelpText(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual InitHelpText host and relocated layout required");
   host=new Host{ReadWord=h.ReadWord,Close=h.Close,Decrement=h.Decrement,Increment=h.Increment,ReadByte=h.ReadByte,WriteWord=h.WriteWord,AssignString=h.AssignString,CopyString=h.CopyString,TransferBuffer=h.TransferBuffer,Clear=h.Clear,InitIos=h.InitIos,ConstructStringStream=h.ConstructStringStream,ExtractString=h.ExtractString,WriteByte=h.WriteByte,ConstructLiteral=h.ConstructLiteral,InsertUnique=h.InsertUnique,Open=h.Open,InsertBytes=h.InsertBytes,InsertCString=h.InsertCString,Find=h.Find,ConstructSubstring=h.ConstructSubstring,CompareBytes=h.CompareBytes,Insert=h.Insert,ConstructIosBase=h.ConstructIosBase,ConstructFileBuffer=h.ConstructFileBuffer,LeakString=h.LeakString,DestroyString=h.DestroyString,DestroyStringStream=h.DestroyStringStream,DestroyBasicFile=h.DestroyBasicFile,DestroyLocale=h.DestroyLocale,DestroyIosBase=h.DestroyIosBase,DestroyStreamBuffer=h.DestroyStreamBuffer,DestroyIfstream=h.DestroyIfstream,DestroyFileBuffer=h.DestroyFileBuffer,EndLine=h.EndLine,ThrowOutOfRange=h.ThrowOutOfRange,DestroyPair=h.DestroyPair,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};
   initializationLayout=new OriginalJurisdictionHelpStreamInitialization.Layout{HelpCString=l.HelpCString,IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot,IosBaseVtable=l.IosBaseVtable,IfstreamVtable=l.IfstreamVtable,IfstreamIosVtable=l.IfstreamIosVtable,EmptyStringData=l.EmptyStringData};
   cleanup=new OriginalJurisdictionHelpCleanup(new OriginalJurisdictionHelpCleanup.Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,DestroyBasicFile=host.DestroyBasicFile,DestroyStreamBuffer=host.DestroyStreamBuffer,DestroyIosBase=host.DestroyIosBase,DestroyString=host.DestroyString,DestroyPair=host.DestroyPair,DestroyStringStream=host.DestroyStringStream,DestroyIfstream=host.DestroyIfstream,DestroyFileBuffer=host.DestroyFileBuffer,Resume=host.Resume},new OriginalJurisdictionHelpCleanup.Layout{IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot,IosBaseVtable=l.IosBaseVtable});
   new OriginalJurisdictionHelpStreamInitialization(InitializationHost(host.ReadWord),initializationLayout);
   consumption=new OriginalJurisdictionHelpStreamConsumption(new OriginalJurisdictionHelpStreamConsumption.Host{ReadWord=host.ReadWord,Close=host.Close,WriteWord=host.WriteWord,TransferBuffer=host.TransferBuffer,Clear=host.Clear,ExtractString=host.ExtractString,AssignString=host.AssignString,InsertBytes=host.InsertBytes,InsertCString=host.InsertCString,EndLine=host.EndLine,DestroyString=host.DestroyString,DestroyStringStream=host.DestroyStringStream,DestroyBasicFile=host.DestroyBasicFile,DestroyLocale=host.DestroyLocale,DestroyIosBase=host.DestroyIosBase,IsNativeUnwind=host.IsNativeUnwind,Cleanup=cleanup.Cleanup},new OriginalJurisdictionHelpStreamConsumption.Layout{ErrorStream=l.ErrorStream,WarningCString=l.WarningCString,IfstreamVtable=l.IfstreamVtable,IfstreamIosVtable=l.IfstreamIosVtable,FileBufferVtable=l.FileBufferVtable,StreamBufferVtable=l.StreamBufferVtable,IosBaseVtable=l.IosBaseVtable,IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot});
   var map=new OriginalJurisdictionHelpMapSlot(new OriginalJurisdictionHelpMapSlot.Host{ReadWord=host.ReadWord,Decrement=host.Decrement,Increment=host.Increment,WriteWord=host.WriteWord,CopyString=host.CopyString,CompareBytes=host.CompareBytes,Insert=host.Insert,InsertUnique=host.InsertUnique,DestroyString=host.DestroyString,IsNativeUnwind=host.IsNativeUnwind,Cleanup=cleanup.Cleanup},l.EmptyStringData);
   parsing=new OriginalJurisdictionHelpParsing(new OriginalJurisdictionHelpParsing.Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AssignString=host.AssignString,LeakString=host.LeakString,DestroyString=host.DestroyString,ThrowOutOfRange=host.ThrowOutOfRange,Find=host.Find,ConstructSubstring=host.ConstructSubstring,GetValueSlot=map.GetValueSlot,IsNativeUnwind=host.IsNativeUnwind,Cleanup=cleanup.Cleanup},l.OutOfRangeCString);
  }
  private OriginalJurisdictionHelpStreamInitialization.Host InitializationHost(Func<uint,uint> read)
  {return new OriginalJurisdictionHelpStreamInitialization.Host{ReadWord=read,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,ConstructLiteral=host.ConstructLiteral,ConstructIosBase=host.ConstructIosBase,ConstructFileBuffer=host.ConstructFileBuffer,InitIos=host.InitIos,Clear=host.Clear,ConstructStringStream=host.ConstructStringStream,Open=host.Open,IsNativeUnwind=host.IsNativeUnwind,Cleanup=cleanup.Cleanup};}
  public void Initialize(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live InitHelpText caller frame required");
   uint pathData=0;bool captured=false;
   var initialization=new OriginalJurisdictionHelpStreamInitialization(InitializationHost(address=>{uint value=host.ReadWord(address);if(!captured&&address==unchecked(frame+0x224)){pathData=value;captured=true;}return value;}),initializationLayout);
   bool readable=initialization.Initialize(frame);
   if(!captured)throw new InvalidOperationException("InitHelpText original path capture was not observed");
   if(!readable){consumption.WarnAndFinish(frame,pathData);return;}
   consumption.Acquire(frame,pathData);uint cursor=parsing.Parse(frame);consumption.Finish(frame,cursor);
  }
 }
}
