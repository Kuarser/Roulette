using System;
namespace RouletteDeluxe
{
 // Complete original insertion balancing, rotations and iterator traversal.
 public sealed class OriginalConfigurationRbTree
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord,InvokeMutableDecrement,InvokeMutableIncrement;
   public Action<uint,uint> WriteWord;
  }
  private readonly Host host;
  public OriginalConfigurationRbTree(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.InvokeMutableDecrement==null||h.InvokeMutableIncrement==null)throw new ArgumentException("Actual RB tree memory and dynamic mutable traversal bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,InvokeMutableDecrement=h.InvokeMutableDecrement,InvokeMutableIncrement=h.InvokeMutableIncrement};
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public void RotateLeft(uint node,uint rootCell)
  {
   uint child=host.ReadWord(Add(node,12)),middle=host.ReadWord(Add(child,8));host.WriteWord(Add(node,12),middle);if(middle!=0)host.WriteWord(Add(middle,4),node);
   uint parent=host.ReadWord(Add(node,4));host.WriteWord(Add(child,4),parent);
   if(host.ReadWord(rootCell)==node)host.WriteWord(rootCell,child);
   else{parent=host.ReadWord(Add(node,4));if(host.ReadWord(Add(parent,8))==node)host.WriteWord(Add(parent,8),child);else host.WriteWord(Add(parent,12),child);}
   host.WriteWord(Add(child,8),node);host.WriteWord(Add(node,4),child);
  }
  public void RotateRight(uint node,uint rootCell)
  {
   uint child=host.ReadWord(Add(node,8)),middle=host.ReadWord(Add(child,12));host.WriteWord(Add(node,8),middle);if(middle!=0)host.WriteWord(Add(middle,4),node);
   uint parent=host.ReadWord(Add(node,4));host.WriteWord(Add(child,4),parent);
   if(host.ReadWord(rootCell)==node)host.WriteWord(rootCell,child);
   else{parent=host.ReadWord(Add(node,4));if(host.ReadWord(Add(parent,12))==node)host.WriteWord(Add(parent,12),child);else host.WriteWord(Add(parent,8),child);}
   host.WriteWord(Add(child,12),node);host.WriteWord(Add(node,4),child);
  }
  public void InsertAndRebalance(byte left,uint node,uint parent,uint header)
  {
   uint rootCell=Add(header,4);host.WriteWord(Add(node,4),parent);host.WriteWord(Add(node,8),0);host.WriteWord(Add(node,12),0);host.WriteWord(node,0);
   if(left!=0){host.WriteWord(Add(parent,8),node);if(parent==header){host.WriteWord(rootCell,node);host.WriteWord(Add(header,12),node);}else if(host.ReadWord(Add(header,8))==parent)host.WriteWord(Add(header,8),node);}
   else{host.WriteWord(Add(parent,12),node);if(host.ReadWord(Add(header,12))==parent)host.WriteWord(Add(header,12),node);}
   uint root=host.ReadWord(rootCell);
   while(node!=root)
   {
    parent=host.ReadWord(Add(node,4));if(host.ReadWord(parent)!=0)break;
    uint grand=host.ReadWord(Add(parent,4)),uncle=host.ReadWord(Add(grand,8));
    if(parent==uncle)
    {
     uncle=host.ReadWord(Add(grand,12));
     if(uncle!=0&&host.ReadWord(uncle)==0){host.WriteWord(parent,1);host.WriteWord(uncle,1);host.WriteWord(grand,0);node=grand;continue;}
     if(host.ReadWord(Add(parent,12))==node){uint previous=parent;RotateLeft(parent,rootCell);node=previous;parent=host.ReadWord(Add(previous,4));}
     host.WriteWord(parent,1);host.WriteWord(grand,0);RotateRight(grand,rootCell);root=host.ReadWord(rootCell);
    }
    else
    {
     if(uncle!=0&&host.ReadWord(uncle)==0){host.WriteWord(parent,1);host.WriteWord(uncle,1);host.WriteWord(grand,0);node=grand;continue;}
     if(host.ReadWord(Add(parent,8))==node){uint previous=parent;RotateRight(parent,rootCell);node=previous;parent=host.ReadWord(Add(previous,4));}
     host.WriteWord(parent,1);host.WriteWord(grand,0);RotateLeft(grand,rootCell);root=host.ReadWord(rootCell);
    }
   }
   host.WriteWord(root,1);
  }
  public uint Decrement(uint node)
  {
   if(host.ReadWord(node)==0){uint parent=host.ReadWord(Add(node,4)),grand=host.ReadWord(Add(parent,4));if(grand==node)return host.ReadWord(Add(grand,12));}
   uint child=host.ReadWord(Add(node,8));if(child!=0){uint right;while((right=host.ReadWord(Add(child,12)))!=0)child=right;return child;}
   uint ancestor=host.ReadWord(Add(node,4));if(node!=host.ReadWord(Add(ancestor,8)))return ancestor;
   while(true){uint previous=ancestor;ancestor=host.ReadWord(Add(previous,4));if(host.ReadWord(Add(ancestor,8))!=previous)return ancestor;}
  }
  public uint Increment(uint node)
  {
   uint child=host.ReadWord(Add(node,12));if(child!=0){uint left;while((left=host.ReadWord(Add(child,8)))!=0)child=left;return child;}
   uint ancestor=host.ReadWord(Add(node,4)),right=0;
   if(node==host.ReadWord(Add(ancestor,12)))
   {
    while(true){uint previous=ancestor;ancestor=host.ReadWord(Add(previous,4));if(host.ReadWord(Add(ancestor,12))!=previous){right=host.ReadWord(Add(previous,12));node=previous;break;}}
   }
   return ancestor!=right?ancestor:node;
  }
  public uint DecrementConst(uint node){return host.InvokeMutableDecrement(node);}
  public uint IncrementConst(uint node){return host.InvokeMutableIncrement(node);}
 }
}
