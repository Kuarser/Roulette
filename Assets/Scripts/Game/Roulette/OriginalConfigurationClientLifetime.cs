using System;
namespace RouletteDeluxe
{
 // Config C1/D1, Host D1 and GameConfigClient D1, including original LSDA tails.
 public sealed class OriginalConfigurationClientLifetime
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,InvokeUnary;
   public Action<uint,uint,uint,uint> InvokeTriple;
   public Action<uint> Free,DisconnectHost,DestroyDeque,ConstructGame;
   public Func<Exception,bool> IsNativeUnwind;
   public Action<Exception> Resume;
  }
  private readonly Host host;
  public OriginalConfigurationClientLifetime(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.InvokeUnary==null||h.InvokeTriple==null||h.Free==null||h.DisconnectHost==null||h.DestroyDeque==null||h.ConstructGame==null||h.IsNativeUnwind==null||h.Resume==null)throw new ArgumentException("Actual client word-memory/virtual dispatch/heap/connection/deque/construction/native unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,InvokeUnary=h.InvokeUnary,InvokeTriple=h.InvokeTriple,Free=h.Free,DisconnectHost=h.DisconnectHost,DestroyDeque=h.DestroyDeque,ConstructGame=h.ConstructGame,IsNativeUnwind=h.IsNativeUnwind,Resume=h.Resume};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,Action cleanup)
  {
   try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex)){cleanup();host.Resume(ex);throw new InvalidOperationException("Native nonreturning client unwind resume binding returned");}throw;}
  }
  public void ConstructConfig(uint owner){host.ConstructGame(owner);host.WriteWord(owner,Add(host.ReadWord(0x09b11fe0),8));}
  public void DestroyConfig(uint owner){host.WriteWord(owner,Add(host.ReadWord(0x09b11fe0),8));DestroyGame(owner);}
  private void DestroyMap(uint owner)
  {
   uint map=host.ReadWord(owner);if(map==0)return;uint limit=Add(host.ReadWord(Add(owner,0x24)),4),first=host.ReadWord(Add(owner,0x14));
   for(uint at=first;at<limit;at=Add(at,4))host.Free(host.ReadWord(at));if(first<limit)map=host.ReadWord(owner);host.Free(map);
  }
  private void SetHostBase(uint owner){host.WriteWord(owner,Add(host.ReadWord(0x09b120e0),8));}
  public void DestroyHost(uint owner)
  {
   host.WriteWord(owner,Add(host.ReadWord(0x09b122f8),8));
   Protected(()=>host.DisconnectHost(owner),()=>{host.DestroyDeque(Add(owner,0x18));SetHostBase(owner);});
   DestroyMap(Add(owner,0x18));SetHostBase(owner);
  }
  private void Manager(uint owner,uint offset,bool protect,int remaining)
  {
   uint manager=host.ReadWord(Add(owner,offset));if(manager==0)return;uint method=host.ReadWord(manager);
   if(method!=0){uint storage=Add(owner,offset+4);Action call=()=>host.InvokeTriple(method,storage,storage,2);if(protect)Protected(call,()=>CleanupGame(owner,remaining));else call();}
   host.WriteWord(Add(owner,offset),0);
  }
  private void CleanupGame(uint owner,int remaining)
  {
   if(remaining>=3)Manager(owner,0x74,false,0);if(remaining>=2)Manager(owner,0x64,false,0);if(remaining>=1)Manager(owner,0x54,false,0);
   host.DestroyDeque(Add(owner,0xc));host.WriteWord(owner,0x096a93e0);
  }
  public void DestroyGame(uint owner)
  {
   uint server=host.ReadWord(Add(owner,8));host.WriteWord(owner,0x09768488);
   if(server!=0){uint vt=host.ReadWord(server),method=host.ReadWord(Add(vt,4));Protected(()=>host.InvokeTriple(method,server,owner,0xffffffff),()=>CleanupGame(owner,3));host.WriteWord(Add(owner,4),0);host.WriteWord(Add(owner,8),0);}
   else{uint connection=host.ReadWord(Add(owner,4));if(connection!=0){uint vt=host.ReadWord(connection),method=host.ReadWord(Add(vt,4));Protected(()=>host.InvokeUnary(method,connection),()=>CleanupGame(owner,3));}}
   Manager(owner,0x74,true,2);Manager(owner,0x64,true,1);Manager(owner,0x54,true,0);
   DestroyMap(Add(owner,0xc));host.WriteWord(owner,0x096a93e0);
  }
 }
}
