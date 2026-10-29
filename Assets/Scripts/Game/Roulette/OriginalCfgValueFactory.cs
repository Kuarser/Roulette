using System;
namespace RouletteDeluxe
{
 public sealed class OriginalCfgValueFactory
 {
  public sealed class Host{public Func<uint,uint> New;public Action<uint> Construct,Delete;public Func<Exception,bool> IsNativeUnwind;public Action<Exception> Resume;}
  private readonly Host host;
  public OriginalCfgValueFactory(Host h){if(h==null)throw new ArgumentException("Actual value factory host required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Value factory binding required: "+f.Name);f.SetValue(host,v);}}
  public uint Create(){uint owner=host.New(40);try{host.Construct(owner);}catch(Exception error){if(!host.IsNativeUnwind(error))throw;host.Delete(owner);host.Resume(error);throw new InvalidOperationException("Original nonreturning value factory Resume returned");}return owner;}
  public uint CreateHybrid(){return Create();}
 }
}
