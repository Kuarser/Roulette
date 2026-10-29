using System;
namespace RouletteDeluxe
{
 // ConfigurationOption assignments in native fill_insert0949f000. These
 // existing owners use string/tree/rule assignment, not construction or copy.
 public sealed class OriginalConfigurationNativeAssign
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,AssignString,AssignOrderTree,AssignAllowedTree,AssignRules;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
  }
  private readonly Host host;
  public OriginalConfigurationNativeAssign(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.AssignString==null||h.AssignOrderTree==null||h.AssignAllowedTree==null||h.AssignRules==null||h.ReadByte==null||h.WriteByte==null)throw new ArgumentException("Actual existing configuration memory/string/tree/rule assignment bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,AssignString=h.AssignString,AssignOrderTree=h.AssignOrderTree,AssignAllowedTree=h.AssignAllowedTree,AssignRules=h.AssignRules,ReadByte=h.ReadByte,WriteByte=h.WriteByte};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  private void Word(uint dst,uint src,uint off){host.WriteWord(Add(dst,off),host.ReadWord(Add(src,off)));}
  private void Byte(uint dst,uint src,uint off){host.WriteByte(Add(dst,off),host.ReadByte(Add(src,off)));}
  public void Assign(uint destination,uint source)
  {
   host.AssignString(Add(destination,4),Add(source,4));host.AssignString(Add(destination,8),Add(source,8));host.AssignString(Add(destination,12),Add(source,12));Word(destination,source,16);
   host.AssignString(Add(destination,20),Add(source,20));Byte(destination,source,24);
   host.AssignString(Add(destination,28),Add(source,28));Byte(destination,source,32);
   host.AssignString(Add(destination,36),Add(source,36));Byte(destination,source,40);Word(destination,source,44);
   host.AssignOrderTree(Add(destination,48),Add(source,48));host.AssignAllowedTree(Add(destination,72),Add(source,72));Word(destination,source,96);host.AssignRules(Add(destination,100),Add(source,100));
   for(uint offset=112;offset<=126;++offset)Byte(destination,source,offset);Word(destination,source,128);
   // Exception cleanup belongs to the enclosing fill_insert scope. There is
   // no assignment-local destructor, catch translation or vtable reset.
  }
 }
}
