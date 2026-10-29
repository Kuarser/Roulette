using System;
namespace RouletteDeluxe
{
 // Complete packaged Sync bodies; IPC mutex creation remains host owned.
 public sealed class OriginalConfigurationSync
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<int,uint,uint> CreateMutex;
   public Action<uint,uint> InvokeDelete;
   public Action<uint> Delete;
  }
  private readonly Host host;
  private readonly uint vtable;
  public OriginalConfigurationSync(Host h,uint syncVtableAddressPoint)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.CreateMutex==null||h.InvokeDelete==null||h.Delete==null||syncVtableAddressPoint==0)throw new ArgumentException("Actual Sync memory, IPC mutex creation, deleting dispatch, heap deletion and relocated vtable bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,CreateMutex=h.CreateMutex,InvokeDelete=h.InvokeDelete,Delete=h.Delete};vtable=syncVtableAddressPoint;
  }
  public void Construct(uint owner,int timeout)
  {
   host.WriteWord(owner,vtable);uint mutex=host.CreateMutex(0,0);host.WriteWord(unchecked(owner+4),mutex);
  }
  private void DestroyMutex(uint mutex)
  {
   if(mutex==0)return;uint table=host.ReadWord(mutex),method=host.ReadWord(unchecked(table+4));host.InvokeDelete(method,mutex);
  }
  public void Destroy(uint owner)
  {
   host.WriteWord(owner,vtable);DestroyMutex(host.ReadWord(unchecked(owner+4)));
  }
  public void Delete(uint owner)
  {
   uint mutex=host.ReadWord(unchecked(owner+4));host.WriteWord(owner,vtable);DestroyMutex(mutex);host.Delete(owner);
  }
 }
}
