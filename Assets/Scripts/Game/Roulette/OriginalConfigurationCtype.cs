using System;
namespace RouletteDeluxe {
 // Packaged ctype<char> _M_widen_init 000553c0 and default do_widen bodies.
 public sealed class OriginalConfigurationCtype {
  public sealed class Host {public Func<uint,uint> ReadWord;public Action<uint,byte> WriteByte;public Action<uint,uint,uint> CopyBytes;public Func<uint,uint,uint,int> CompareBytes;public Func<uint,uint[],uint> Invoke;}
  readonly Host h;public OriginalConfigurationCtype(Host host){if(host==null||host.ReadWord==null||host.WriteByte==null||host.CopyBytes==null||host.CompareBytes==null||host.Invoke==null)throw new ArgumentException("Physical facet, counted bytes and actual virtual widen owners required");h=new Host{ReadWord=host.ReadWord,WriteByte=host.WriteByte,CopyBytes=host.CopyBytes,CompareBytes=host.CompareBytes,Invoke=host.Invoke};}
  public void Initialize(uint frame,uint owner){uint source=unchecked(frame+0x10);for(uint i=0;i<256;++i)h.WriteByte(unchecked(source+i),(byte)i);uint table=h.ReadWord(owner),end=unchecked(frame+0x110),cache=unchecked(owner+0x1d);h.Invoke(h.ReadWord(unchecked(table+0x1c)),new[]{owner,source,end,cache});h.WriteByte(unchecked(owner+0x1c),1);if(h.CompareBytes(source,cache,256)!=0)h.WriteByte(unchecked(owner+0x1c),2);}
  public uint Widen(uint owner,uint character){return character&255;}
  public uint WidenRange(uint owner,uint first,uint end,uint destination){h.CopyBytes(destination,first,unchecked(end-first));return end;}
 }
 public sealed class OriginalConfigurationCtypeLibrary {
  readonly OriginalNativeMemory m;readonly OriginalConfigurationCtype c;readonly Func<uint,uint[],uint> derived;
  public OriginalConfigurationCtypeLibrary(OriginalNativeMemory memory,Func<uint,uint[],uint> derivedWiden){if(memory==null||derivedWiden==null)throw new ArgumentException("Shared facet memory and explicit derived-facet owner required");m=memory;derived=derivedWiden;c=new OriginalConfigurationCtype(new OriginalConfigurationCtype.Host{ReadWord=m.ReadWord,WriteByte=m.WriteByte,CopyBytes=m.CopyBytes,CompareBytes=Compare,Invoke=Invoke});}
  int Compare(uint first,uint second,uint count){for(uint i=0;i<count;++i){int a=m.ReadByte(unchecked(first+i)),b=m.ReadByte(unchecked(second+i));if(a!=b)return a-b;}return 0;}
  uint Invoke(uint method,uint[] args){if(method==0x54f40){if(args==null||args.Length!=4)throw new ArgumentException("Original widen-range arity");return c.WidenRange(args[0],args[1],args[2],args[3]);}return derived(method,args);}
  public void Initialize(uint frame,uint facet){c.Initialize(frame,facet);}
  public uint Widen(uint facet,uint character){return c.Widen(facet,character);}
  public uint WidenRange(uint facet,uint first,uint end,uint destination){return c.WidenRange(facet,first,end,destination);}
  public uint InvokeForOstream(uint frame,uint method,uint[] args){if(method==0x49070){if(args==null||args.Length!=1)throw new ArgumentException("Original widen-init arity");c.Initialize(frame,args[0]);return 0;}if(method==0x54c60){if(args==null||args.Length!=2)throw new ArgumentException("Original character-widen arity");return c.Widen(args[0],args[1]);}return derived(method,args);}
 }
}
