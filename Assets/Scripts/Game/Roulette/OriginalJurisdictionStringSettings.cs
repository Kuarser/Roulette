using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionStringSettings
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,uint,uint> GetOptionString;
   public Action<uint,uint,uint,uint> ReadString;
   public Action<uint,uint> AssignString;
   public Action<uint> DestroyString;
   public Func<Exception,bool> IsNativeUnwind;public Action<Exception> Resume;
  }
  public sealed class Layout{public uint AutoConfigFileName,CustomLocaleSuffix,LookAndFeel,Locale;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionStringSettings(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual string settings host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual string setting binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual string setting CString required: "+f.Name);f.SetValue(layout,v);}
  }
  // temporaryString admits the original frame's hidden result owner at ESP+1c.
  public void Read(uint originalFunctionVa,uint machine,uint temporaryString)
  {
   uint name,offset;
   switch(originalFunctionVa)
   {
    case 0x0836acd0:name=layout.AutoConfigFileName;offset=0x1f0;break;
    case 0x0836ad50:name=layout.CustomLocaleSuffix;offset=0x1c0;break;
    case 0x0836add0:name=layout.LookAndFeel;offset=0x44;break;
    case 0x0836ae50:name=layout.Locale;offset=0x40;break;
    default:throw new ArgumentOutOfRangeException(nameof(originalFunctionVa));
   }
   if(machine==0||temporaryString==0)throw new ArgumentException("Live jurisdiction and temporary string owners required");
   uint pair=host.GetOptionString(machine,name);if(pair==0)return;
   uint length=host.ReadWord(unchecked(pair+4)),sourceOffset=host.ReadWord(pair);
   host.ReadString(temporaryString,machine,sourceOffset,length);
   try{host.AssignString(unchecked(machine+offset),temporaryString);}
   catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;host.DestroyString(temporaryString);host.Resume(ex);throw new InvalidOperationException("Original nonreturning string setting unwind resume returned");}
   host.DestroyString(temporaryString);
  }
 }
}
