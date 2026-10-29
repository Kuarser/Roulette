using System;
namespace RouletteDeluxe
{
 // Native 08368c20, 083637e0, 08366140 and 08366080. Tree primitives are explicit bindings.
 public sealed class OriginalJurisdictionUndefinedOptions
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord,EraseTree;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> InsertOffset;
   public Action<uint,uint,uint,uint,uint> Fault;
  }
  public sealed class Layout{public uint FaultFormat,SourceFile,CheckFunction,UnknownValueFunction,BitCodesFunction;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionUndefinedOptions(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual undefined-option memory/tree host and Fault layout required");
   host=new Host();foreach(var f in typeof(Host).GetFields()){var v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual undefined-option binding required: "+f.Name);f.SetValue(host,v);}
   layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual undefined-option Fault literal required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,uint b){return unchecked(a+b);}
  private void Fault(uint line,uint function){host.Fault(16,layout.FaultFormat,layout.SourceFile,line,function);throw new InvalidOperationException("Original nonreturning undefined-option Fault returned");}
  private uint Find(uint root,uint header,int key)
  {
   uint candidate=header,node=root;
   while(node!=0)
   {
    int value=unchecked((int)host.ReadWord(A(node,0x10)));
    if(key<=value){candidate=node;node=host.ReadWord(A(node,8));}
    else node=host.ReadWord(A(node,12));
   }
   if(candidate!=header&&key<unchecked((int)host.ReadWord(A(candidate,0x10))))candidate=header;
   return candidate;
  }
  // frame is the original stable ESP after the five-register/0x4c prologue.
  public void Check(uint machine,uint frame)
  {
   if(frame==0)throw new ArgumentException("Live undefined-option caller frame required");
   if(host.ReadWord(A(machine,0x30))==0)Fault(0x1373,layout.CheckFunction);
   uint root=host.ReadWord(A(machine,0x14c)),tree=A(machine,0x144),usedHeader=A(machine,0x130);
   host.EraseTree(tree,root);uint header=A(machine,0x148);
   host.WriteWord(A(machine,0x150),header);host.WriteWord(A(machine,0x14c),0);host.WriteWord(A(machine,0x154),header);host.WriteWord(A(machine,0x158),0);host.WriteByte(A(machine,0x15c),0);
   for(uint index=0x4e;index<0xce;++index)
   {
    uint input=host.ReadWord(A(machine,0x30));host.WriteWord(A(frame,0x3c),index);
    if(host.ReadByte(A(input,index))==0)continue;
    if(Find(host.ReadWord(A(machine,0x134)),usedHeader,unchecked((int)index))!=usedHeader)continue;
    host.InsertOffset(A(frame,0x28),tree,A(frame,0x3c));host.WriteByte(A(machine,0x15c),1);
   }
  }
  public uint HasUndefinedOptions(uint machine){return host.ReadByte(A(machine,0x15c));}
  public uint GetUnknownByteValue(uint machine,int offset)
  {
   uint input=host.ReadWord(A(machine,0x30));if(input==0)Fault(0x13aa,layout.UnknownValueFunction);
   uint header=A(machine,0x148);if(Find(host.ReadWord(A(machine,0x14c)),header,offset)==header)return 0;
   return host.ReadByte(A(input,unchecked((uint)offset)));
  }
  public uint GetBitCodes(uint machine)
  {
   uint input=host.ReadWord(A(machine,0x30));if(input==0)Fault(0x1359,layout.BitCodesFunction);return A(input,0x4e);
  }
 }
}
