using System;
namespace RouletteDeluxe
{
 public sealed class OriginalCfgStringConverterLifetime
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,New;
   public Action<uint,uint> WriteWord,CopyString,AssignString,DestroyRep;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Action<uint,uint,uint> AppendCounted,InvokeConstruct,InvokeSetString;
   public Action<uint> LeakHard,DestroyString,Delete;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  [Serializable] public sealed class Layout{public uint ConverterVtableSlot,InterfaceVtableSlot,EmptyRepSlot,PthreadCancelSlot,ZeroAppendLiteral;}
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgStringConverterLifetime(Host h,Layout l){if(h==null||l==null)throw new ArgumentException("Actual converter lifecycle host and relocated layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Converter lifecycle binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Converter lifecycle relocation required: "+f.Name);f.SetValue(layout,v);}}
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Resume(Exception e){host.Resume(e);throw new InvalidOperationException("Original nonreturning converter Resume returned");}
  public void ConstructEmpty(uint owner){uint table=host.ReadWord(layout.ConverterVtableSlot);host.WriteWord(owner,A(table,8));uint empty=host.ReadWord(layout.EmptyRepSlot);host.WriteWord(A(owner,4),A(empty,12));}
  public void Construct(uint owner,uint source,uint frame){ConstructEmpty(owner);try{host.InvokeSetString(owner,source,unchecked(frame-64));}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.DestroyString(A(owner,4));uint table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));Resume(e);}}
  public uint Create(uint source,uint frame){uint owner=host.New(8);try{host.InvokeConstruct(owner,source,unchecked(frame-48));}catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.Delete(owner);Resume(e);}return owner;}
  public uint CreateHybrid(uint source,uint frame){return Create(source,unchecked(frame-48));}
  private void Release(uint owner,uint allocator){uint data=host.ReadWord(owner),rep=unchecked(data-12);if(rep==host.ReadWord(layout.EmptyRepSlot))return;int old;if(host.ReadWord(layout.PthreadCancelSlot)!=0)old=host.AtomicAdd(unchecked(data-4),-1);else{uint count=host.ReadWord(unchecked(data-4));old=unchecked((int)count);host.WriteWord(unchecked(data-4),unchecked(count-1));}if(old<=0)host.DestroyRep(rep,allocator);}
  private void Destroy(uint owner,uint frame,bool deleting){uint table=host.ReadWord(layout.ConverterVtableSlot);host.WriteWord(owner,A(table,8));Release(A(owner,4),A(frame,31));table=host.ReadWord(layout.InterfaceVtableSlot);host.WriteWord(owner,A(table,8));if(deleting)host.Delete(owner);}
  public void D1(uint owner,uint frame){Destroy(owner,frame,false);}
  public void D0(uint owner,uint frame){Destroy(owner,frame,true);}
  public void SetString(uint owner,uint source,uint frame)
  {
   uint temporary=A(frame,24),storage=A(owner,4);host.CopyString(temporary,source);
   try{host.AppendCounted(temporary,layout.ZeroAppendLiteral,0);host.AssignString(storage,temporary);}
   catch(Exception e){if(!host.IsNativeUnwind(e))throw;host.DestroyString(temporary);Resume(e);}
   Release(temporary,A(frame,31));uint data=host.ReadWord(storage);if(unchecked((int)host.ReadWord(unchecked(data-4)))>=0){host.LeakHard(storage);data=host.ReadWord(storage);}uint current;if(unchecked((int)host.ReadWord(unchecked(data-4)))>=0){host.LeakHard(storage);current=host.ReadWord(storage);}else current=data;byte first=host.ReadByte(current);host.WriteByte(data,first);
  }
 }
}
