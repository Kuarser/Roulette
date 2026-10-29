using System;
namespace RouletteDeluxe
{
 // Standalone ConfigurationOption copy C1 085963d0, 714 original bytes.
 // Native memory, strings, trees, map destructor and rule-vector C1 remain bound.
 public sealed class OriginalConfigurationCopyConstructor
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString,CopyRuleVector,EraseOrderTree;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint,uint> CopyOrderTree,CopyAllowedTree;
   public Action<uint> DestroyString,DestroyAllowedMap;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationCopyConstructor(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.CopyString==null||h.CopyRuleVector==null||h.EraseOrderTree==null||h.ReadByte==null||h.WriteByte==null||h.CopyOrderTree==null||h.CopyAllowedTree==null||h.DestroyString==null||h.DestroyAllowedMap==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual ConfigurationOption memory/string/tree/map destructor/rule vector C1/native unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,CopyString=h.CopyString,CopyRuleVector=h.CopyRuleVector,EraseOrderTree=h.EraseOrderTree,ReadByte=h.ReadByte,WriteByte=h.WriteByte,CopyOrderTree=h.CopyOrderTree,CopyAllowedTree=h.CopyAllowedTree,DestroyString=h.DestroyString,DestroyAllowedMap=h.DestroyAllowedMap,IsNativeUnwind=h.IsNativeUnwind};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Protected(Action call,Action<Exception> cleanup){try{call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private T Protected<T>(Func<T> call,Action<Exception> cleanup){try{return call();}catch(Exception ex){if(host.IsNativeUnwind(ex))cleanup(ex);throw;}}
  private void Word(uint destination,uint source,uint offset){host.WriteWord(Add(destination,offset),host.ReadWord(Add(source,offset)));}
  private void Byte(uint destination,uint source,uint offset){host.WriteByte(Add(destination,offset),host.ReadByte(Add(source,offset)));}
  private void InitializeTree(uint tree)
  {host.WriteWord(Add(tree,4),0);host.WriteWord(Add(tree,8),0);host.WriteWord(Add(tree,20),0);host.WriteWord(Add(tree,12),Add(tree,4));host.WriteWord(Add(tree,16),Add(tree,4));}
  private void CompleteTree(uint tree,uint source,uint root)
  {
   host.WriteWord(Add(tree,8),root);uint left=root,right=root,next;
   do{next=host.ReadWord(Add(left,8));if(next!=0)left=next;}while(next!=0);
   host.WriteWord(Add(tree,12),left);
   do{next=host.ReadWord(Add(right,12));if(next!=0)right=next;}while(next!=0);
   host.WriteWord(Add(tree,16),right);host.WriteWord(Add(tree,20),host.ReadWord(Add(source,20)));
  }
  public void Construct(uint destination,uint source)
  {
   host.WriteWord(destination,0x097681f8);
   Action<Exception> cleanup=ex=>host.WriteWord(destination,0x09695028);
   foreach(uint offset in new[]{4u,8u,12u,20u,28u,36u})
   {
    uint field=Add(destination,offset),original=Add(source,offset);Action<Exception> prior=cleanup;
    Protected(()=>host.CopyString(field,original),prior);
    cleanup=ex=>{host.DestroyString(field);prior(ex);};
    if(offset==12)Word(destination,source,16);
    if(offset==20)Byte(destination,source,24);
    if(offset==28)Byte(destination,source,32);
    if(offset==36)Byte(destination,source,40);
   }
   uint type=host.ReadWord(Add(source,44));
   uint order=Add(destination,48),allowed=Add(destination,72);
   host.WriteWord(Add(order,4),0);host.WriteWord(Add(order,8),0);host.WriteWord(Add(order,20),0);
   host.WriteWord(Add(destination,44),type);
   host.WriteWord(Add(order,12),Add(order,4));host.WriteWord(Add(order,16),Add(order,4));
   uint root=host.ReadWord(Add(source,56));
   if(root!=0){uint copied=Protected(()=>host.CopyOrderTree(order,root,Add(order,4)),cleanup);CompleteTree(order,Add(source,48),copied);}
   Action<Exception> stringsCleanup=cleanup;
   Action<Exception> orderCleanup=ex=>{host.EraseOrderTree(order,host.ReadWord(Add(order,8)));stringsCleanup(ex);};
   InitializeTree(allowed);root=host.ReadWord(Add(source,80));
   if(root!=0){uint copied=Protected(()=>host.CopyAllowedTree(allowed,root,Add(allowed,4)),orderCleanup);CompleteTree(allowed,Add(source,72),copied);}
   Action<Exception> mapsCleanup=ex=>{host.DestroyAllowedMap(allowed);orderCleanup(ex);};
   Word(destination,source,96);
   Protected(()=>host.CopyRuleVector(Add(destination,100),Add(source,100)),mapsCleanup);
   for(uint offset=112;offset<=126;++offset)Byte(destination,source,offset);
   Word(destination,source,128);
  }
 }
}
