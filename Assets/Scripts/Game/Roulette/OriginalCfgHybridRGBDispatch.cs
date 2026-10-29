using System;
namespace RouletteDeluxe
{
 // Hybrid RGB wrapper 0940cdb0.
 public sealed class OriginalCfgHybridRGBDispatch
 {
  public sealed class Host{public Action<uint,uint,uint> InvokeBaseRGB;}
  private readonly Host host;
  public OriginalCfgHybridRGBDispatch(Host h){if(h==null)throw new ArgumentException("Actual Hybrid RGB host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid RGB binding required: "+f.Name);f.SetValue(host,v);}}
  public void AddRGB(uint dictionary,uint node,uint frame){host.InvokeBaseRGB(dictionary,node,unchecked(frame-192));}
 }
}
