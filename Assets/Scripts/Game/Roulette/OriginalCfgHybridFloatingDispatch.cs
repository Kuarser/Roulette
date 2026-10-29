using System;
namespace RouletteDeluxe
{
 // Hybrid floating wrappers 0940cf80 / 0940ce60.
 public sealed class OriginalCfgHybridFloatingDispatch
 {
  public sealed class Host{public Action<uint,uint,uint> InvokeBaseFloat,InvokeBaseDouble;}
  private readonly Host host;
  public OriginalCfgHybridFloatingDispatch(Host h){if(h==null)throw new ArgumentException("Actual Hybrid floating host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid floating binding required: "+f.Name);f.SetValue(host,v);}}
  public void AddFloat(uint dictionary,uint node,uint frame){host.InvokeBaseFloat(dictionary,node,unchecked(frame-128));}
  public void AddDouble(uint dictionary,uint node,uint frame){host.InvokeBaseDouble(dictionary,node,unchecked(frame-128));}
 }
}
