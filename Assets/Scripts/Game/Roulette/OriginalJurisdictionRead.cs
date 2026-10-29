using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionRead
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,AllocateBuffer,CreateEeprom,CloseFile;
   public Action<uint,uint> WriteWord,EraseOffsets,InvokeFinalize,InvokeDelete;
   public Func<uint> GetFactory;
   public Func<uint,uint,uint> InvokeDictionaryFactory,InvokeUsingEeprom,InvokeVerify,OpenFile;
   public Action<uint,uint,uint> ConstructLiteral,InvokeDictionaryCreate,FaultText;
   public Action<uint> DestroyString,Clear,Validate,ReadPartNumber,ReadJurisdictionName;
   public Action<uint,uint,uint,uint,uint,uint> InvokeEepromRead;
   public Func<uint,uint,uint,uint,uint> ReadFile;
   public Action RemoveFactory,EndCatch;
   public Action<uint,uint,uint,uint,uint> FaultSource;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> UnwindSelector,BeginCatch;
   public Action<Exception> Resume;
  }
  public sealed class Layout
  {
   public uint FileModeCString,SourceFileCString,FunctionCString,AllocationFaultCString,EepromCreateFaultCString,VerifyFaultCString,FileFaultCString,ExceptionCString;
  }
  private readonly Host host;private readonly Layout layout;private readonly OriginalJurisdictionReadRegistration registration;
  public OriginalJurisdictionRead(Host h,Layout l,OriginalJurisdictionReadRegistration.Host registrations,uint[] relocatedCStrings)
  {
   if(h==null||l==null)throw new ArgumentException("Actual Read host and relocated layout required");
   host=new Host();foreach(var field in typeof(Host).GetFields()){object v=field.GetValue(h);if(v==null)throw new ArgumentException("Actual Read binding required: "+field.Name);field.SetValue(host,v);}
   layout=new Layout();foreach(var field in typeof(Layout).GetFields()){uint v=(uint)field.GetValue(l);if(v==0)throw new ArgumentException("Actual Read CString required: "+field.Name);field.SetValue(layout,v);}
   registration=new OriginalJurisdictionReadRegistration(registrations,relocatedCStrings);
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception ex){host.Resume(ex);throw new InvalidOperationException("Original nonreturning Read resume returned");}
  private void Initial(uint nameOwner,bool destroyOnFailure,Action call)
  {
   try{call();}
   catch(Exception ex)
   {
    if(!host.IsNativeUnwind(ex))throw;
    if(destroyOnFailure)host.DestroyString(nameOwner);
    if(host.UnwindSelector(ex)!=1){Resume(ex);return;}
    uint caught=host.BeginCatch(ex),data=host.ReadWord(caught);
    try{host.FaultText(0x10,layout.ExceptionCString,data);}
    catch(Exception fault){if(!host.IsNativeUnwind(fault))throw;host.EndCatch();Resume(fault);}
    throw new InvalidOperationException("Original nonreturning Read exception Fault returned");
   }
  }
  private void FaultSource(uint format,uint line)
  {host.FaultSource(0x10,format,layout.SourceFileCString,line,layout.FunctionCString);throw new InvalidOperationException("Original nonreturning Read source Fault returned");}
  private uint UsingEeprom(uint machine){uint table=host.ReadWord(machine),method=host.ReadWord(A(table,0x30));return host.InvokeUsingEeprom(method,machine)&255;}
  public uint Read(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live Read caller frame required");
   uint machine=host.ReadWord(A(frame,0x50)),nameOwner=A(frame,0x28),binaryPath=host.ReadWord(A(frame,0x58)),factory=host.GetFactory(),table=host.ReadWord(factory);host.WriteWord(A(machine,0xc),factory);
   uint method=host.ReadWord(A(table,0x14)),dictionary=host.InvokeDictionaryFactory(method,factory);host.WriteWord(A(machine,0x10),dictionary);table=host.ReadWord(dictionary);method=host.ReadWord(A(table,0x10));uint configPath=host.ReadWord(A(frame,0x54));
   Initial(nameOwner,false,()=>host.ConstructLiteral(nameOwner,configPath,A(frame,0x2f)));
   dictionary=host.ReadWord(A(machine,0x10));Initial(nameOwner,true,()=>host.InvokeDictionaryCreate(method,dictionary,nameOwner));host.DestroyString(nameOwner);
   uint buffer=host.ReadWord(A(machine,0x30));if(buffer==0){buffer=host.AllocateBuffer(0x800);host.WriteWord(A(machine,0x30),buffer);if(buffer==0)FaultSource(layout.AllocationFaultCString,0x11dd);}
   host.Clear(machine);
   if(UsingEeprom(machine)!=0)
   {
    uint eeprom=host.CreateEeprom(0);host.WriteWord(A(machine,0x2c),eeprom);if(eeprom==0)FaultSource(layout.EepromCreateFaultCString,0x11ee);
    table=host.ReadWord(eeprom);buffer=host.ReadWord(A(machine,0x30));method=host.ReadWord(A(table,0x10));host.InvokeEepromRead(method,eeprom,buffer,0,0x800,0);
   }
   else
   {
    uint file=host.OpenFile(binaryPath,layout.FileModeCString);host.WriteWord(A(machine,8),file);
    if(file==0){host.FaultText(0x10,layout.FileFaultCString,binaryPath);throw new InvalidOperationException("Original nonreturning Read file Fault returned");}
    buffer=host.ReadWord(A(machine,0x30));host.ReadFile(buffer,1,0x800,file);
   }
   table=host.ReadWord(machine);method=host.ReadWord(A(table,0x10));if((host.InvokeVerify(method,machine)&255)==0)FaultSource(layout.VerifyFaultCString,0x120b);
   host.Validate(machine);uint offsets=host.ReadWord(A(machine,0x134));host.EraseOffsets(A(machine,0x12c),offsets);host.WriteWord(A(machine,0x138),A(machine,0x130));host.WriteWord(A(machine,0x13c),A(machine,0x130));host.WriteWord(A(machine,0x134),0);host.WriteWord(A(machine,0x140),0);
   host.ReadPartNumber(machine);host.ReadJurisdictionName(machine);registration.Register(machine);
   table=host.ReadWord(machine);method=host.ReadWord(A(table,0x20));host.InvokeFinalize(method,machine);
   if(UsingEeprom(machine)==0)host.CloseFile(host.ReadWord(A(machine,8)));
   uint current=host.ReadWord(A(machine,0x2c));if(current!=0){table=host.ReadWord(current);method=host.ReadWord(A(table,4));host.InvokeDelete(method,current);host.WriteWord(A(machine,0x2c),0);}
   current=host.ReadWord(A(machine,0x10));if(current!=0){table=host.ReadWord(current);method=host.ReadWord(A(table,4));host.InvokeDelete(method,current);}host.WriteWord(A(machine,0x10),0);host.RemoveFactory();host.WriteWord(A(machine,0xc),0);return 1;
  }
 }
}
