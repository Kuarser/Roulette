using System;
namespace RouletteDeluxe
{
 // Complete packaged Mutex bodies; Sync and thread primitives remain host owned.
 public sealed class OriginalConfigurationMutex
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Action<uint,ushort> WriteHalf;
   public Action<uint,int> ConstructSync;
   public Action<uint> DestroySync;
   public Action<uint,uint,uint> InvokeLock;
   public Action<uint,uint> InvokeUnlock;
   public Action<uint> Delete;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host host;
  private readonly uint vtable;
  public OriginalConfigurationMutex(Host h,uint mutexVtableAddressPoint)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.WriteHalf==null||h.ConstructSync==null||h.DestroySync==null||h.InvokeLock==null||h.InvokeUnlock==null||h.Delete==null||h.IsNativeUnwind==null||h.Resume==null||mutexVtableAddressPoint==0)throw new ArgumentException("Actual Mutex memory, Sync, dispatch, deletion, unwind and relocated vtable bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,WriteHalf=h.WriteHalf,ConstructSync=h.ConstructSync,DestroySync=h.DestroySync,InvokeLock=h.InvokeLock,InvokeUnlock=h.InvokeUnlock,Delete=h.Delete,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};vtable=mutexVtableAddressPoint;
  }
  public void Construct(uint owner,bool initiallyLocked,int timeout)
  {
   host.ConstructSync(owner,0);
   host.WriteWord(owner,vtable);host.WriteWord(unchecked(owner+8),0);host.WriteHalf(unchecked(owner+12),0);
   if(!initiallyLocked)return;
   uint sync=host.ReadWord(unchecked(owner+4)),table=host.ReadWord(sync),method=host.ReadWord(unchecked(table+8));
   try{host.InvokeLock(method,sync,1);}
   catch(Exception ex){if(host.IsNativeUnwind(ex)){host.DestroySync(owner);host.Resume(ex);throw new InvalidOperationException("Native nonreturning Mutex unwind resume binding returned");}throw;}
  }
  public uint Lock(uint owner,int timeout)
  {
   uint sync=host.ReadWord(unchecked(owner+4)),table=host.ReadWord(sync),method=host.ReadWord(unchecked(table+8));host.InvokeLock(method,sync,1);return 1;
  }
  public void Unlock(uint owner)
  {
   uint sync=host.ReadWord(unchecked(owner+4)),table=host.ReadWord(sync),method=host.ReadWord(unchecked(table+12));host.InvokeUnlock(method,sync);
  }
  public void Destroy(uint owner){host.WriteWord(owner,vtable);host.DestroySync(owner);}
  public void Delete(uint owner){Destroy(owner);host.Delete(owner);}
 }
}
