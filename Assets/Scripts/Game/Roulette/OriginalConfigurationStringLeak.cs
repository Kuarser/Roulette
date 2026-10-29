using System;
namespace RouletteDeluxe
{
 // Complete packaged string leak/hard-leak and Rep leaked-state bodies; no local EH.
 public sealed class OriginalConfigurationStringLeak
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,uint,uint,uint> Mutate;
   public Action<uint> InvokeLeakHard;
  }
  private readonly Host host;private readonly uint emptyRepSlot;
  public OriginalConfigurationStringLeak(Host h,uint emptySlot)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.Mutate==null||h.InvokeLeakHard==null||emptySlot==0)throw new ArgumentException("Actual COW memory, mutation, dynamic hard-leak and relocated empty Rep slot required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,Mutate=h.Mutate,InvokeLeakHard=h.InvokeLeakHard};emptyRepSlot=emptySlot;
  }
  public void LeakHard(uint source)
  {
   uint data=host.ReadWord(source);if(unchecked(data-12)==host.ReadWord(emptyRepSlot))return;
   if(unchecked((int)host.ReadWord(unchecked(data-4)))>0){host.Mutate(source,0,0,0);data=host.ReadWord(source);}
   host.WriteWord(unchecked(data-4),uint.MaxValue);
  }
  public void Leak(uint source){uint data=host.ReadWord(source);if(unchecked((int)host.ReadWord(unchecked(data-4)))>=0)host.InvokeLeakHard(source);}
  public bool IsLeaked(uint rep){return (host.ReadWord(unchecked(rep+8))>>31)!=0;}
  public void SetLeaked(uint rep){host.WriteWord(unchecked(rep+8),uint.MaxValue);}
 }
}
