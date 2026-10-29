using System;
namespace RouletteDeluxe
{
 // Remaining stream phases of InitHelpText; parsing/map population is caller-owned.
 public sealed class OriginalJurisdictionHelpStreamConsumption
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,Close;
   public Action<uint,uint> WriteWord,TransferBuffer,Clear,ExtractString,AssignString;
   public Func<uint,uint,uint,uint> InsertBytes;
   public Func<uint,uint,uint> InsertCString;
   public Action<uint> EndLine,DestroyString,DestroyStringStream,DestroyBasicFile,DestroyLocale,DestroyIosBase;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,uint,Exception> Cleanup;
  }
  public sealed class Layout
  {
   public uint ErrorStream,WarningCString,IfstreamVtable,IfstreamIosVtable,FileBufferVtable,StreamBufferVtable,IosBaseVtable,IfstreamVttFirstSlot,IfstreamVttSecondSlot;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionHelpStreamConsumption(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.Close==null||h.WriteWord==null||h.TransferBuffer==null||h.Clear==null||h.ExtractString==null||h.AssignString==null||h.InsertBytes==null||h.InsertCString==null||h.EndLine==null||h.DestroyString==null||h.DestroyStringStream==null||h.DestroyBasicFile==null||h.DestroyLocale==null||h.DestroyIosBase==null||h.IsNativeUnwind==null||h.Cleanup==null||l.ErrorStream==0||l.WarningCString==0||l.IfstreamVtable==0||l.IfstreamIosVtable==0||l.FileBufferVtable==0||l.StreamBufferVtable==0||l.IosBaseVtable==0||l.IfstreamVttFirstSlot==0||l.IfstreamVttSecondSlot==0)throw new ArgumentException("Actual help stream transfer/extraction, diagnostics, destruction, native cleanup and relocated owners required");
   host=new Host{ReadWord=h.ReadWord,Close=h.Close,WriteWord=h.WriteWord,TransferBuffer=h.TransferBuffer,Clear=h.Clear,ExtractString=h.ExtractString,AssignString=h.AssignString,InsertBytes=h.InsertBytes,InsertCString=h.InsertCString,EndLine=h.EndLine,DestroyString=h.DestroyString,DestroyStringStream=h.DestroyStringStream,DestroyBasicFile=h.DestroyBasicFile,DestroyLocale=h.DestroyLocale,DestroyIosBase=h.DestroyIosBase,IsNativeUnwind=h.IsNativeUnwind,Cleanup=h.Cleanup};layout=new Layout{ErrorStream=l.ErrorStream,WarningCString=l.WarningCString,IfstreamVtable=l.IfstreamVtable,IfstreamIosVtable=l.IfstreamIosVtable,FileBufferVtable=l.FileBufferVtable,StreamBufferVtable=l.StreamBufferVtable,IosBaseVtable=l.IosBaseVtable,IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private T Protected<T>(uint pad,uint frame,uint savedEbx,Func<T> call)
  {try{return call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Cleanup(pad,frame,savedEbx,error);throw new InvalidOperationException("Original nonreturning help cleanup binding returned");}}
  private static void Admit(uint frame){if(frame==0)throw new ArgumentException("Live caller help frame required");}
  public uint Acquire(uint frame,uint savedPathData)
  {
   Admit(frame);uint source=Add(frame,0x228);host.WriteWord(Add(frame,0x24),source);Protected(0x083686e3,frame,savedPathData,()=>{host.TransferBuffer(Add(frame,0x168),Add(frame,0x50));return 0;});
   host.WriteWord(Add(frame,0x24),source);uint closed=Protected(0x083686e3,frame,savedPathData,()=>host.Close(Add(frame,0x50)));
   if(closed==0){uint vtable=host.ReadWord(Add(frame,0x48));host.WriteWord(Add(frame,0x24),source);uint ios=Add(Add(frame,0x48),host.ReadWord(Add(vtable,0xfffffff4)));uint state=host.ReadWord(Add(ios,0x14))|4u;Protected(0x083686e3,frame,savedPathData,()=>{host.Clear(ios,state);return 0;});}
   uint temporary=Add(frame,0x22c);host.WriteWord(Add(frame,0x24),source);Protected(0x083686e3,frame,temporary,()=>{host.ExtractString(temporary,Add(frame,0x16c));return 0;});
   host.WriteWord(Add(frame,0x24),source);Protected(0x083686cb,frame,temporary,()=>{host.AssignString(source,temporary);return 0;});host.DestroyString(temporary);return source;
  }
  public void WarnAndFinish(uint frame,uint savedPathData)
  {
   Admit(frame);uint data=host.ReadWord(Add(frame,0x224));host.WriteWord(Add(frame,0x24),Add(frame,0x228));uint length=host.ReadWord(Add(data,0xfffffff4));
   uint output=Protected(0x083686e3,frame,savedPathData,()=>host.InsertBytes(layout.ErrorStream,data,length));host.WriteWord(Add(frame,0x24),Add(frame,0x228));output=Protected(0x083686e3,frame,savedPathData,()=>host.InsertCString(output,layout.WarningCString));host.WriteWord(Add(frame,0x24),Add(frame,0x228));Protected(0x083686e3,frame,savedPathData,()=>{host.EndLine(output);return 0;});host.DestroyString(Add(frame,0x228));Teardown(frame,savedPathData,true);
  }
  public void Finish(uint frame,uint savedCursor)
  {Admit(frame);host.DestroyString(host.ReadWord(Add(frame,0x24)));Teardown(frame,savedCursor,false);}
  private void Teardown(uint frame,uint savedEbx,bool warning)
  {
   Protected(0x083686ea,frame,savedEbx,()=>{host.DestroyStringStream(Add(frame,0x160));return 0;});host.WriteWord(Add(frame,0x48),layout.IfstreamVtable);host.WriteWord(Add(frame,0xd8),layout.IfstreamIosVtable);host.WriteWord(Add(frame,0x50),layout.FileBufferVtable);
   Protected(warning?0x08368781u:0x08368589u,frame,savedEbx,()=>host.Close(Add(frame,0x50)));Protected(warning?0x08368716u:0x0836877au,frame,savedEbx,()=>{host.DestroyBasicFile(Add(frame,0x88));return 0;});host.WriteWord(Add(frame,0x50),layout.StreamBufferVtable);host.DestroyLocale(Add(frame,0x6c));
   uint first=host.ReadWord(layout.IfstreamVttFirstSlot),second=host.ReadWord(layout.IfstreamVttSecondSlot),offset=host.ReadWord(Add(first,0xfffffff4));host.WriteWord(Add(frame,0x48),first);host.WriteWord(Add(Add(frame,0x48),offset),second);host.WriteWord(Add(frame,0x4c),0);host.WriteWord(Add(frame,0xd8),layout.IosBaseVtable);Protected(0x083686dc,frame,savedEbx,()=>{host.DestroyIosBase(Add(frame,0xd8));return 0;});host.DestroyString(Add(frame,0x224));
  }
 }
}
