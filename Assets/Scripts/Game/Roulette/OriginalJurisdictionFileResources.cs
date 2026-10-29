using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace RouletteDeluxe
{
 // PC fopen/fread/fclose ports for recovered Jurisdiction::Read. Caller owns
 // memory and path mapping; this owner keeps actual streams until fclose.
 public sealed class OriginalJurisdictionFileResources : IDisposable
 {
  private readonly OriginalNativeMemory memory;
  private readonly Func<string,string> resolvePath;
  private readonly Dictionary<uint,FileStream> files=new Dictionary<uint,FileStream>();
  private uint nextHandle=1;private bool disposed;
  public int OpenFiles{get{return files.Count;}}
  public OriginalJurisdictionFileResources(OriginalNativeMemory memory,Func<string,string> resolvePath)
  {if(memory==null||resolvePath==null)throw new ArgumentException("Caller memory and PC path resolver required");this.memory=memory;this.resolvePath=resolvePath;}
  private void Alive(){if(disposed)throw new ObjectDisposedException("OriginalJurisdictionFileResources");}
  private string CString(uint address){return Encoding.ASCII.GetString(memory.ReadBytes(address,memory.StrLen(address)));}
  public void Bind(OriginalJurisdictionRead.Host host)
  {Alive();if(host==null)throw new ArgumentNullException("host");host.OpenFile=Open;host.ReadFile=Read;host.CloseFile=Close;}
  public uint Open(uint path,uint mode)
  {
   Alive();string access=CString(mode);if(access!="r"&&access!="rb")throw new ArgumentException("Jurisdiction file port requires original read mode");
   string target=resolvePath(CString(path));if(nextHandle==0)throw new InvalidOperationException("PC file handle space exhausted");
   FileStream file;try{file=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read);}catch(IOException){return 0;}catch(UnauthorizedAccessException){return 0;}
   uint handle=nextHandle++;files.Add(handle,file);return handle;
  }
  public uint Read(uint destination,uint size,uint count,uint handle)
  {
   Alive();if(size==0||count==0)return 0;FileStream file;if(!files.TryGetValue(handle,out file))throw new InvalidOperationException("Live PC file handle required");
   ulong requested=(ulong)size*count,received=0;var buffer=new byte[8192];
   while(received<requested)
   {
    int read;try{read=file.Read(buffer,0,(int)Math.Min((ulong)buffer.Length,requested-received));}catch(IOException){break;}
    if(read==0)break;for(int i=0;i<read;++i)memory.WriteByte(unchecked(destination+(uint)(received+(ulong)i)),buffer[i]);received+=(uint)read;
   }
   return (uint)(received/size);
  }
  public uint Close(uint handle)
  {
   Alive();FileStream file;if(!files.TryGetValue(handle,out file))throw new InvalidOperationException("Live PC file handle required");files.Remove(handle);
   try{file.Dispose();return 0;}catch(IOException){return uint.MaxValue;}
  }
  public void Dispose()
  {
   if(disposed)return;disposed=true;Exception failure=null;
   foreach(var file in files.Values)try{file.Dispose();}catch(Exception ex){if(failure==null)failure=ex;}
   files.Clear();if(failure!=null)throw failure;
  }
 }
}
