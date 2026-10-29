using System;
namespace RouletteDeluxe
{
 // All sixteen InitHelpText landing-pad paths. Caller supplies its live frame owners.
 public sealed class OriginalJurisdictionHelpCleanup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint> DestroyBasicFile,DestroyStreamBuffer,DestroyIosBase,DestroyString,DestroyPair,DestroyStringStream,DestroyIfstream,DestroyFileBuffer;
   public Action<Exception> Resume;
  }
  public sealed class Layout{public uint IfstreamVttFirstSlot,IfstreamVttSecondSlot,IosBaseVtable;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionHelpCleanup(Host h,Layout l)
  {
   if(h==null||l==null||h.ReadWord==null||h.WriteWord==null||h.DestroyBasicFile==null||h.DestroyStreamBuffer==null||h.DestroyIosBase==null||h.DestroyString==null||h.DestroyPair==null||h.DestroyStringStream==null||h.DestroyIfstream==null||h.DestroyFileBuffer==null||h.Resume==null||l.IfstreamVttFirstSlot==0||l.IfstreamVttSecondSlot==0||l.IosBaseVtable==0)throw new ArgumentException("Actual help frame, stream/string destructors, VTT/vtable and unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,DestroyBasicFile=h.DestroyBasicFile,DestroyStreamBuffer=h.DestroyStreamBuffer,DestroyIosBase=h.DestroyIosBase,DestroyString=h.DestroyString,DestroyPair=h.DestroyPair,DestroyStringStream=h.DestroyStringStream,DestroyIfstream=h.DestroyIfstream,DestroyFileBuffer=h.DestroyFileBuffer,Resume=h.Resume};layout=new Layout{IfstreamVttFirstSlot=l.IfstreamVttFirstSlot,IfstreamVttSecondSlot=l.IfstreamVttSecondSlot,IosBaseVtable=l.IosBaseVtable};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void RestoreBase(uint frame,bool offsetBeforeStore)
  {
   uint first=host.ReadWord(layout.IfstreamVttFirstSlot),second=host.ReadWord(layout.IfstreamVttSecondSlot),offset;
   if(offsetBeforeStore){offset=host.ReadWord(Add(first,0xfffffff4));host.WriteWord(Add(frame,0x48),first);}
   else{host.WriteWord(Add(frame,0x48),first);offset=host.ReadWord(Add(first,0xfffffff4));}
   host.WriteWord(Add(Add(frame,0x48),offset),second);host.WriteWord(Add(frame,0x4c),0);DestroyBase(frame);
  }
  private void DestroyBase(uint frame){host.WriteWord(Add(frame,0xd8),layout.IosBaseVtable);host.DestroyIosBase(Add(frame,0xd8));}
  private void Strings(uint frame){host.DestroyString(Add(frame,0x230));host.DestroyString(Add(frame,0x234));}
  private void Buffer(uint frame){host.DestroyString(host.ReadWord(Add(frame,0x24)));host.DestroyStringStream(Add(frame,0x160));host.DestroyIfstream(Add(frame,0x48));}
  public void Cleanup(uint landingPad,uint frame,uint savedEbxOwner,Exception error)
  {
   if(frame==0||error==null)throw new ArgumentException("Live caller frame and original unwind identity required");
   switch(landingPad)
   {
    case 0x08368589:case 0x08368781:host.DestroyBasicFile(Add(frame,0x88));host.DestroyStreamBuffer(Add(frame,0x50));RestoreBase(frame,true);break;
    case 0x083685f6:host.DestroyPair(Add(frame,0x21c));host.DestroyString(host.ReadWord(Add(frame,0x20)));Strings(frame);Buffer(frame);break;
    case 0x0836865a:host.DestroyFileBuffer(Add(frame,0x50));RestoreBase(frame,false);break;
    case 0x083686aa:RestoreBase(frame,false);break;
    case 0x083686ac:host.WriteWord(Add(frame,0x20),Add(frame,0x238));host.DestroyString(Add(frame,0x238));Strings(frame);Buffer(frame);break;
    case 0x083686c0:host.DestroyString(Add(frame,0x234));Buffer(frame);break;
    case 0x083686c7:DestroyBase(frame);break;
    case 0x083686cb:host.DestroyString(savedEbxOwner);Buffer(frame);break;
    case 0x083686dc:break;
    case 0x083686e3:Buffer(frame);break;
    case 0x083686ea:host.DestroyIfstream(Add(frame,0x48));break;
    case 0x083686f5:host.DestroyString(Add(frame,0x21c));host.DestroyString(host.ReadWord(Add(frame,0x20)));Strings(frame);Buffer(frame);break;
    case 0x0836870f:Strings(frame);Buffer(frame);break;
    case 0x08368716:case 0x0836877a:host.DestroyStreamBuffer(Add(frame,0x50));RestoreBase(frame,true);break;
    default:throw new ArgumentOutOfRangeException(nameof(landingPad));
   }
   host.DestroyString(Add(frame,0x224));host.Resume(error);throw new InvalidOperationException("Original nonreturning help unwind resume binding returned");
  }
 }
}
