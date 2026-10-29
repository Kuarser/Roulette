using System;
namespace RouletteDeluxe
{
 // Hybrid AddJurisdictionOption 0940cd50 / Str 0940ccf0 / Bit 0940cc90.
 public sealed class OriginalCfgHybridJurisdictionDispatch
 {
  public sealed class Host{public Action<uint,uint,uint> InvokeBaseJurisdiction,InvokeBaseStr,InvokeBaseBit;}
  private readonly Host host;
  public OriginalCfgHybridJurisdictionDispatch(Host h){if(h==null)throw new ArgumentException("Actual Hybrid jurisdiction host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Hybrid jurisdiction binding required: "+f.Name);f.SetValue(host,v);}}
  public void AddJurisdiction(uint dictionary,uint node,uint frame){host.InvokeBaseJurisdiction(dictionary,node,unchecked(frame-128));}
  public void AddStr(uint dictionary,uint node,uint frame){host.InvokeBaseStr(dictionary,node,unchecked(frame-176));}
  public void AddBit(uint dictionary,uint node,uint frame){host.InvokeBaseBit(dictionary,node,unchecked(frame-176));}
 }
}
