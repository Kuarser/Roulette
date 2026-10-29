using System;
namespace RouletteDeluxe
{
 // CFGHybridDictionary::GetValueHolder 0940d0d0, original outgoing argument order.
 // Frame is this wrapper's post-prologue ESP. PIC thunk/register transport is admitted.
 public sealed class OriginalCfgHybridValueLookup
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,InvokeBaseLookup;
   public Action<uint,uint> WriteWord;
  }
  private readonly Host host;
  public OriginalCfgHybridValueLookup(Host h)
  {
   if(h==null)throw new ArgumentException("Actual Hybrid lookup host required");host=new Host();
   foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid lookup binding required: "+f.Name);f.SetValue(host,v);}
  }
  public uint GetValueHolder(uint frame)
  {
   uint key=host.ReadWord(unchecked(frame+36));host.WriteWord(unchecked(frame+4),key);
   uint owner=host.ReadWord(unchecked(frame+32));host.WriteWord(frame,owner);
   return host.InvokeBaseLookup(unchecked(frame-80));
  }
 }
}
