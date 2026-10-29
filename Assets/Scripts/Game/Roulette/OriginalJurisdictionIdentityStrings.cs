using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionIdentityStrings
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,uint,uint> GetOptionString,InvokeGameMode;
   public Action<uint,uint,uint,uint> ReadString,ConstructSubstring;
   public Action<uint,uint> AssignString;
   public Action<uint> DestroyString;
   public Func<uint,uint,uint,uint,uint> ReverseFind;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  public sealed class Layout{public uint JurisdictionName,PartNumber,DotCString,GameMgrSlot;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionIdentityStrings(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual identity-string host and layout required");
   host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual identity-string binding required: "+f.Name);f.SetValue(host,v);}
   layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual identity-string relocation required: "+f.Name);f.SetValue(layout,v);}
  }
  private void Assign(uint destination,uint temporary)
  {
   try{host.AssignString(destination,temporary);}
   catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;host.DestroyString(temporary);host.Resume(ex);throw new InvalidOperationException("Original nonreturning identity-string resume returned");}
   host.DestroyString(temporary);
  }
  // frame admits the original post-prologue ESP and its two distinct local owners.
  public void Read(uint originalFunctionVa,uint machine,uint frame)
  {
   bool part;
   switch(originalFunctionVa){case 0x0836aed0:part=false;break;case 0x0836af50:part=true;break;default:throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));}
   if(machine==0||frame==0)throw new ArgumentException("Live jurisdiction and caller frame required");
   uint pair=host.GetOptionString(machine,part?layout.PartNumber:layout.JurisdictionName);if(pair==0)return;
   uint length=host.ReadWord(unchecked(pair+4)),offset=host.ReadWord(pair),destination=unchecked(machine+(part?0x38u:0x3cu)),temporary=unchecked(frame+(part?0x18u:0x1cu));
   host.ReadString(temporary,machine,offset,length);Assign(destination,temporary);
   if(!part)return;
   uint game=host.ReadWord(layout.GameMgrSlot),table=host.ReadWord(game),method=host.ReadWord(unchecked(table+0x10c));
   if(host.InvokeGameMode(method,game)!=0)return;
   uint position=host.ReverseFind(destination,layout.DotCString,uint.MaxValue,1);if(position==uint.MaxValue)return;
   temporary=unchecked(frame+0x1c);host.ConstructSubstring(temporary,destination,0,position);Assign(destination,temporary);
  }
 }
}
