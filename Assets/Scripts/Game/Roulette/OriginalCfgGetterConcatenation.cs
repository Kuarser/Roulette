using System;
namespace RouletteDeluxe
{
 // Packaged CString+string, string+CString and regparm2 string+string helpers.
 public sealed class OriginalCfgGetterConcatenation
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,CStringLength;
   public Action<uint,uint> WriteWord,Reserve,CopyString,AppendString;
   public Action<uint,uint,uint> AppendCounted;
   public Action<uint> DestroyString,Resume;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionAddress;
  }
  [Serializable] public sealed class Layout{public uint EmptyData;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgGetterConcatenation(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual concatenation host and relocated empty data required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Concatenation binding required: "+f.Name);f.SetValue(host,v);}if(l.EmptyData==0)throw new ArgumentException("Concatenation empty data required");layout=new Layout{EmptyData=l.EmptyData};
  }
  private void Resume(uint address){host.Resume(address);throw new InvalidOperationException("Original nonreturning concatenation Resume returned");}
  public uint Prefix(uint result,uint leftCString,uint rightString,uint frame)
  {
   uint leftLength=host.CStringLength(leftCString);host.WriteWord(result,layout.EmptyData);uint data=host.ReadWord(rightString),rightLength=host.ReadWord(unchecked(data-12));
   try{host.Reserve(result,unchecked(leftLength+rightLength));host.AppendCounted(result,leftCString,leftLength);host.AppendString(result,rightString);}
   catch(Exception e){if(!host.IsNativeUnwind(e))throw;uint error=host.ExceptionAddress(e);host.DestroyString(result);Resume(error);throw;}return result;
  }
  public uint Suffix(uint result,uint leftString,uint rightCString,uint frame)
  {
   host.CopyString(result,leftString);uint length=host.CStringLength(rightCString);
   try{host.AppendCounted(result,rightCString,length);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;uint error=host.ExceptionAddress(e);host.DestroyString(result);Resume(error);throw;}return result;
  }
  public uint Strings(uint result,uint leftString,uint rightString,uint frame)
  {
   host.CopyString(result,leftString);
   try{host.AppendString(result,rightString);}catch(Exception e){if(!host.IsNativeUnwind(e))throw;uint temporary=unchecked(frame+28);host.WriteWord(temporary,host.ExceptionAddress(e));host.DestroyString(result);uint error=host.ReadWord(temporary);Resume(error);throw;}return result;
  }
 }
}
