using System;
namespace RouletteDeluxe
{
 // Complete original C++ IPC Mutex factory and object bodies.
 public sealed class OriginalConfigurationIpcMutex
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint> New;
   public Action<uint> Delete;
   public Func<int> CreateKernelMutex;
   public Func<int,int> DeleteKernelMutex,UnlockKernelMutex;
   public Func<int,int,int> SetPermissions;
   public Func<uint,int,int> RegisterName;
   public Func<int,byte,int> LockKernelMutex;
   public Func<uint> ErrnoLocation;
   public Func<uint,uint> AllocateException;
   public Action<uint,uint,uint> Throw;
   public Action<Exception> BeginCatch,Resume;
   public Action EndCatch;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  private readonly uint baseVtable,objectVtable,intTypeInfo;
  public OriginalConfigurationIpcMutex(Host h,uint baseVtableAddressPoint,uint objectVtableAddressPoint,uint intTypeInfoAddress)
  {
   if(h==null||h.ReadWord==null||h.ReadByte==null||h.WriteWord==null||h.WriteByte==null||h.New==null||h.Delete==null||h.CreateKernelMutex==null||h.DeleteKernelMutex==null||h.UnlockKernelMutex==null||h.SetPermissions==null||h.RegisterName==null||h.LockKernelMutex==null||h.ErrnoLocation==null||h.AllocateException==null||h.Throw==null||h.BeginCatch==null||h.Resume==null||h.EndCatch==null||h.IsNativeUnwind==null||baseVtableAddressPoint==0||objectVtableAddressPoint==0||intTypeInfoAddress==0)throw new ArgumentException("Actual C++ IPC memory, kernel, heap, exception ABI and relocated type/vtable bindings required");
   host=new Host{ReadWord=h.ReadWord,ReadByte=h.ReadByte,WriteWord=h.WriteWord,WriteByte=h.WriteByte,New=h.New,Delete=h.Delete,CreateKernelMutex=h.CreateKernelMutex,DeleteKernelMutex=h.DeleteKernelMutex,UnlockKernelMutex=h.UnlockKernelMutex,SetPermissions=h.SetPermissions,RegisterName=h.RegisterName,LockKernelMutex=h.LockKernelMutex,ErrnoLocation=h.ErrnoLocation,AllocateException=h.AllocateException,Throw=h.Throw,BeginCatch=h.BeginCatch,Resume=h.Resume,EndCatch=h.EndCatch,IsNativeUnwind=h.IsNativeUnwind};baseVtable=baseVtableAddressPoint;objectVtable=objectVtableAddressPoint;intTypeInfo=intTypeInfoAddress;
  }
  private T Protected<T>(uint owner,Func<T> call)
  {
   try{return call();}
   catch(Exception ex){if(host.IsNativeUnwind(ex)){DestroyBase(owner);host.Resume(ex);throw new InvalidOperationException("Native nonreturning IPC Mutex unwind resume binding returned");}throw;}
  }
  private void ThrowFailure(uint owner)
  {
   uint exception=Protected(owner,()=>host.AllocateException(4));host.WriteWord(exception,0xffffffff);
   Protected(owner,()=>{host.Throw(exception,intTypeInfo,0);return 0;});throw new InvalidOperationException("Native nonreturning IPC integer throw binding returned");
  }
  private void ReleaseFailedHandle(uint owner)
  {
   uint errno=host.ReadWord(Protected(owner,host.ErrnoLocation));int id=unchecked((int)host.ReadWord(unchecked(owner+4)));Protected(owner,()=>host.DeleteKernelMutex(id));host.WriteWord(unchecked(owner+4),0);uint location=Protected(owner,host.ErrnoLocation);host.WriteWord(location,errno);ThrowFailure(owner);
  }
  public void Construct(uint owner,int permissions,uint name)
  {
   ConstructBase(owner);host.WriteWord(owner,objectVtable);host.WriteByte(unchecked(owner+8),1);
   int id=Protected(owner,host.CreateKernelMutex);host.WriteWord(unchecked(owner+4),unchecked((uint)id));
   if(host.ReadWord(unchecked(owner+4))==0xffffffff){host.WriteWord(unchecked(owner+4),0);ThrowFailure(owner);}
   if(permissions!=0){int current=unchecked((int)host.ReadWord(unchecked(owner+4)));if(Protected(owner,()=>host.SetPermissions(current,permissions))==-1)ReleaseFailedHandle(owner);}
   if(name!=0){int current=unchecked((int)host.ReadWord(unchecked(owner+4)));if(Protected(owner,()=>host.RegisterName(name,current))==-1)ReleaseFailedHandle(owner);}
  }
  public void ConstructBorrowed(uint owner,int id)
  {ConstructBase(owner);host.WriteWord(owner,objectVtable);host.WriteWord(unchecked(owner+4),unchecked((uint)id));host.WriteByte(unchecked(owner+8),0);}
  public void ConstructBase(uint owner){host.WriteWord(owner,baseVtable);}
  public void DestroyBase(uint owner){host.WriteWord(owner,baseVtable);}
  public void DeleteBase(uint owner){DestroyBase(owner);host.Delete(owner);}
  public void Destroy(uint owner)
  {
   host.WriteWord(owner,objectVtable);
   if(host.ReadByte(unchecked(owner+8))!=0&&host.ReadWord(unchecked(owner+4))!=0){int id=unchecked((int)host.ReadWord(unchecked(owner+4)));Protected(owner,()=>host.DeleteKernelMutex(id));}
   DestroyBase(owner);
  }
  public void Delete(uint owner){Destroy(owner);host.Delete(owner);}
  public int Lock(uint owner,byte blocking)
  {
   while(true){int id=unchecked((int)host.ReadWord(unchecked(owner+4)));int result=host.LockKernelMutex(id,blocking);if(result!=-1||host.ReadWord(host.ErrnoLocation())!=4)return result;}
  }
  public void Unlock(uint owner){host.UnlockKernelMutex(unchecked((int)host.ReadWord(unchecked(owner+4))));}
  public uint GetId(uint owner){return host.ReadWord(unchecked(owner+4));}
  public uint Create(int permissions,uint name)
  {
   uint allocated;
   try{allocated=host.New(12);}catch(Exception ex){if(host.IsNativeUnwind(ex))return CatchCreation(ex);throw;}
   try{Construct(allocated,permissions,name);}catch(Exception ex){if(host.IsNativeUnwind(ex)){host.Delete(allocated);return CatchCreation(ex);}throw;}
   return allocated;
  }
  private uint CatchCreation(Exception ex)
  {
   host.BeginCatch(ex);uint errno=host.ReadWord(host.ErrnoLocation());uint location=host.ErrnoLocation();host.WriteWord(location,errno);host.EndCatch();return 0;
  }
 }
}
