using System;
namespace RouletteDeluxe
{
 // InitHelpText entry through its stream-state branch. Caller owns the remaining phases.
 public sealed class OriginalJurisdictionHelpStreamInitialization
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> ConstructLiteral;
   public Action<uint> ConstructIosBase,ConstructFileBuffer;
   public Action<uint,uint> InitIos,Clear,ConstructStringStream;
   public Func<uint,uint,uint,uint> Open;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<uint,uint,uint,Exception> Cleanup;
  }
  public sealed class Layout
  {
   public uint HelpCString,IfstreamVttFirstSlot,IfstreamVttSecondSlot,IosBaseVtable,IfstreamVtable,IfstreamIosVtable,EmptyStringData;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionHelpStreamInitialization(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.ReadByte==null||h.WriteWord==null||h.WriteByte==null||h.ConstructLiteral==null||h.ConstructIosBase==null||h.ConstructFileBuffer==null||h.InitIos==null||h.Clear==null||h.ConstructStringStream==null||h.Open==null||h.IsNativeUnwind==null||h.Cleanup==null||l.HelpCString==0||l.IfstreamVttFirstSlot==0||l.IfstreamVttSecondSlot==0||l.IosBaseVtable==0||l.IfstreamVtable==0||l.IfstreamIosVtable==0||l.EmptyStringData==0)throw new ArgumentException("Actual help path/frame, stream initialization, virtual-base memory, native cleanup and relocated layout bindings required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,WriteWord=h.WriteWord,WriteByte=h.WriteByte,ConstructLiteral=h.ConstructLiteral,ConstructIosBase=h.ConstructIosBase,ConstructFileBuffer=h.ConstructFileBuffer,InitIos=h.InitIos,Clear=h.Clear,ConstructStringStream=h.ConstructStringStream,Open=h.Open,IsNativeUnwind=h.IsNativeUnwind,Cleanup=h.Cleanup};layout=new Layout{HelpCString=l.HelpCString,IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot,IosBaseVtable=l.IosBaseVtable,IfstreamVtable=l.IfstreamVtable,IfstreamIosVtable=l.IfstreamIosVtable,EmptyStringData=l.EmptyStringData};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private T Protected<T>(uint pad,uint frame,uint savedEbx,Func<T> call)
  {try{return call();}catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Cleanup(pad,frame,savedEbx,error);throw new InvalidOperationException("Original nonreturning help cleanup binding returned");}}
  public bool Initialize(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live caller help frame required");
   host.ConstructLiteral(Add(frame,0x224),layout.HelpCString,Add(frame,0x23f));uint pathData=host.ReadWord(Add(frame,0x224));host.ConstructIosBase(Add(frame,0xd8));
   uint first=host.ReadWord(layout.IfstreamVttFirstSlot);host.WriteByte(Add(frame,0x14c),0);uint second=host.ReadWord(layout.IfstreamVttSecondSlot);host.WriteWord(Add(frame,0xd8),layout.IosBaseVtable);host.WriteWord(Add(frame,0x148),0);
   uint offset=host.ReadWord(Add(first,0xfffffff4));host.WriteWord(Add(frame,0x48),first);host.WriteByte(Add(frame,0x14d),0);host.WriteWord(Add(frame,0x150),0);host.WriteWord(Add(Add(frame,0x48),offset),second);host.WriteWord(Add(frame,0x4c),0);
   uint ios=Add(Add(frame,0x48),host.ReadWord(Add(first,0xfffffff4)));host.WriteWord(Add(frame,0x154),0);host.WriteWord(Add(frame,0x158),0);host.WriteWord(Add(frame,0x15c),0);
   Protected(0x083686c7,frame,pathData,()=>{host.InitIos(ios,0);return 0;});host.WriteWord(Add(frame,0x48),layout.IfstreamVtable);host.WriteWord(Add(frame,0xd8),layout.IfstreamIosVtable);
   Protected(0x083686aa,frame,pathData,()=>{host.ConstructFileBuffer(Add(frame,0x50));return 0;});Protected(0x0836865a,frame,pathData,()=>{host.InitIos(Add(frame,0xd8),Add(frame,0x50));return 0;});
   uint file=Protected(0x0836865a,frame,pathData,()=>host.Open(Add(frame,0x50),pathData,8));uint currentVtable=host.ReadWord(Add(frame,0x48));ios=Add(Add(frame,0x48),host.ReadWord(Add(currentVtable,0xfffffff4)));uint state=file==0?host.ReadWord(Add(ios,0x14))|4u:0u;
   Protected(0x0836865a,frame,pathData,()=>{host.Clear(ios,state);return 0;});Protected(0x083686ea,frame,pathData,()=>{host.ConstructStringStream(Add(frame,0x160),0x18);return 0;});
   byte flags=host.ReadByte(Add(frame,0xec));host.WriteWord(Add(frame,0x228),layout.EmptyStringData);return (flags&5)==0;
  }
 }
}
