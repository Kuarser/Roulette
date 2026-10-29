using System;
namespace RouletteDeluxe
{
 // Hybrid integer wrappers 0940d040 / 0940cfe0 / 0940cfb0.
 public sealed class OriginalCfgHybridScalarDispatch
 {
  public sealed class Host{public Action<uint,uint,uint> InvokeBaseInt32,InvokeBaseUnsignedInt32,InvokeBaseUnsignedInt8;}
  private readonly Host host;
  public OriginalCfgHybridScalarDispatch(Host h){if(h==null)throw new ArgumentException("Actual Hybrid scalar host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid scalar binding required: "+f.Name);f.SetValue(host,v);}}
  public void AddInt32(uint dictionary,uint node,uint frame){host.InvokeBaseInt32(dictionary,node,unchecked(frame-128));}
  public void AddUnsignedInt32(uint dictionary,uint node,uint frame){host.InvokeBaseUnsignedInt32(dictionary,node,unchecked(frame-128));}
  public void AddUnsignedInt8(uint dictionary,uint node,uint frame){host.InvokeBaseUnsignedInt8(dictionary,node,unchecked(frame-128));}
 }
}
