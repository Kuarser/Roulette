using System;
namespace RouletteDeluxe
{
 // Six original provider constructors. Addresses refer to the audited i386 image;
 // actual memory, base/client, string, vector and unwind owners must be bound.
 public sealed class OriginalConfigurationProviderConstruction
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord,CopyString;
   public Action<uint> ConstructBase,DestroyBase;
   public Action<uint,uint,uint> AssignCString,InsertAux;
   public Action<uint,uint,uint,uint> AssertFail;
   public Func<Exception,bool> IsNativeUnwind;
  }
  private readonly Host host;
  public OriginalConfigurationProviderConstruction(Host h)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.CopyString==null||h.ConstructBase==null||h.DestroyBase==null||h.AssignCString==null||h.InsertAux==null||h.AssertFail==null||h.IsNativeUnwind==null)throw new ArgumentException("Actual provider memory/base/string/vector/assertion/unwind bindings required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,CopyString=h.CopyString,ConstructBase=h.ConstructBase,DestroyBase=h.DestroyBase,AssignCString=h.AssignCString,InsertAux=h.InsertAux,AssertFail=h.AssertFail,IsNativeUnwind=h.IsNativeUnwind};
  }
  public void Construct(int factoryIndex,uint owner)
  {
   uint singleton,vtable,expression,file,line,pretty,label=0,length=0;uint[] categories=null;
   switch(factoryIndex)
   {
    case 18:singleton=0x0a124564;vtable=0x0972e0c8;expression=0x0972c7a1;file=0x0972e068;line=0x19;pretty=0x0972e0e8;break;
    case 7:singleton=0x0a124484;vtable=0x0972dea8;expression=0x0972c7a1;file=0x0972de2e;line=0x4d;pretty=0x0972df1d;break;
    case 5:singleton=0x0a123dc4;vtable=0x0972dc48;expression=0x0972c7a1;file=0x0972dae8;line=0x2e;pretty=0x0972dc68;label=0x0972db05;length=17;categories=new uint[]{0x0a123dc0};break;
    case 6:singleton=0x0a123d20;vtable=0x0972d968;expression=0x0972a9f8;file=0x0972d8af;line=0x8c;pretty=0x0972d9a0;break;
    case 1:singleton=0x0a122ef4;vtable=0x0972c688;expression=0x0972c608;file=0x0972c5a0;line=0x28;pretty=0x0972c6c0;break;
    case 0:singleton=0x0a122e30;vtable=0x0972c488;expression=0x0972a9f8;file=0x0972c1ff;line=0x88;pretty=0x0972c4c0;label=0x0972c223;length=16;categories=new uint[]{0x0a122e0c,0x0a122e08,0x0a122dec,0x0a122ddc};break;
    default:throw new ArgumentOutOfRangeException("factoryIndex","Provider constructor has not been recovered");
   }
   host.ConstructBase(owner);
   uint existing=host.ReadWord(singleton);host.WriteWord(owner,vtable);
   if(existing!=0){host.AssertFail(expression,file,line,pretty);throw new InvalidOperationException("Native nonreturning provider assertion binding returned");}
   host.WriteWord(singleton,owner);if(categories==null)return;
   try
   {
    host.AssignCString(unchecked(owner+0xd4),label,length);
    uint end=host.ReadWord(unchecked(owner+0xcc)),capacity=host.ReadWord(unchecked(owner+0xd0));
    for(int i=0;i<categories.Length;++i)
    {
     bool last=i==categories.Length-1;
     if(end==capacity)
     {
      host.InsertAux(unchecked(owner+0xc8),end,categories[i]);if(last)return;
      end=host.ReadWord(unchecked(owner+0xcc));capacity=host.ReadWord(unchecked(owner+0xd0));
     }
     else
     {
      if(end!=0){host.CopyString(end,categories[i]);end=host.ReadWord(unchecked(owner+0xcc));if(!last)capacity=host.ReadWord(unchecked(owner+0xd0));}
      end=unchecked(end+4);host.WriteWord(unchecked(owner+0xcc),end);
     }
    }
   }
   catch(Exception ex){if(host.IsNativeUnwind(ex))host.DestroyBase(owner);throw;}
  }
 }
}
