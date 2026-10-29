using System;
namespace RouletteDeluxe {
 // All calls execute recovered static bodies against one original-layout byte owner.
 public sealed class OriginalConfigurationRttiLibrary {
  readonly OriginalNativeMemory m;readonly uint[] frames;int depth;
  readonly OriginalConfigurationDynamicCast entry;
  readonly OriginalConfigurationRttiClass classes;
  readonly OriginalConfigurationRttiPublicSource source;
  readonly OriginalConfigurationRttiVmiUpcast up;
  readonly OriginalConfigurationRttiVmiDyncast down;
  public OriginalConfigurationRttiLibrary(OriginalNativeMemory memory,uint[] nestedFrames){
   if(memory==null||nestedFrames==null||nestedFrames.Length==0)throw new ArgumentException("Shared bytes and separate recursive frames required");m=memory;frames=(uint[])nestedFrames.Clone();
   for(int i=0;i<frames.Length;i++){if(frames[i]==0||frames[i]>0xfffffeffu)throw new ArgumentException("Representable nonempty RTTI frame required");m.ReadByte(frames[i]);m.ReadByte(frames[i]+255u);for(int j=0;j<i;j++)if(Overlap(frames[i],frames[j]))throw new ArgumentException("Overlapping recursive RTTI frames");}
   entry=new OriginalConfigurationDynamicCast(new OriginalConfigurationDynamicCast.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   classes=new OriginalConfigurationRttiClass(new OriginalConfigurationRttiClass.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   source=new OriginalConfigurationRttiPublicSource(new OriginalConfigurationRttiPublicSource.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   up=new OriginalConfigurationRttiVmiUpcast(new OriginalConfigurationRttiVmiUpcast.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
   down=new OriginalConfigurationRttiVmiDyncast(new OriginalConfigurationRttiVmiDyncast.Host{ReadWord=m.ReadWord,ReadByte=m.ReadByte,WriteWord=m.WriteWord,WriteByte=m.WriteByte,AtomicAdd=m.AtomicAdd,Invoke=Invoke,IsNativeUnwind=e=>false,ExceptionToken=e=>0,ExceptionSelector=e=>0});
  }
  static bool Overlap(uint a,uint b){return a>=b?a-b<256u:b-a<256u;}
  void Check(uint frame){if(frame>0xfffffeffu)throw new ArgumentException("Representable RTTI caller frame required");foreach(uint f in frames)if(Overlap(frame,f))throw new ArgumentException("Caller overlaps nested RTTI frames");}
  public uint Cast(uint frame,uint objectPtr,uint sourceType,uint targetType,uint hint){Check(frame);return entry.Cast(frame,objectPtr,sourceType,targetType,hint);}
  public bool Upcast(uint frame,uint type,uint target,uint objectPointerSlot){Check(frame);return classes.PublicUpcast(frame,type,target,objectPointerSlot,0,0,0,0,0)!=0;}
  public uint FindPublicSource(uint frame,uint type,uint hint,uint objectPtr,uint sourceType,uint sourcePtr){Check(frame);uint method=m.ReadWord(m.ReadWord(type)+0x20u);return Dispatch(frame,method,new[]{type,hint,objectPtr,sourceType,sourcePtr});}
  static void Arity(uint[] a,int n){if(a==null||a.Length!=n)throw new ArgumentException("RTTI import arity mismatch");}
  uint Invoke(uint method,uint[] a){
   if(method==0x4a500){Arity(a,2);uint j=0;for(;;j++){byte x=m.ReadByte(unchecked(a[0]+j)),y=m.ReadByte(unchecked(a[1]+j));if(x!=y)return unchecked((uint)(x-y));if(x==0)return 0;}}
   if(depth==frames.Length)throw new InvalidOperationException("Original RTTI recursion requires additional separate frames");uint frame=frames[depth++];try{return Dispatch(frame,method,a);}finally{--depth;}
  }
  uint Dispatch(uint f,uint method,uint[] a){switch(method){
   case 0x48e00:case 0xb1940:Arity(a,4);return classes.ClassUpcast(f,a[0],a[1],a[2],a[3],0,0,0,0);
   case 0xb42e0:Arity(a,4);return classes.SingleUpcast(f,a[0],a[1],a[2],a[3],0,0,0,0);
   case 0xb53b0:Arity(a,4);return up.Upcast(f,a[0],a[1],a[2],a[3]);
   case 0xb1a50:Arity(a,8);return classes.ClassDyncast(f,a[0],a[3],a[4],a[7],a[1],a[2],a[5],a[6]);
   case 0xb4360:Arity(a,8);return classes.SingleDyncast(f,a[0],a[3],a[4],a[7],a[1],a[2],a[5],a[6]);
   case 0xb4d40:Arity(a,8);return down.Dyncast(f,a[0],a[1],a[2],a[3],a[4],a[5],a[6],a[7]);
   case 0xb18b0:Arity(a,5);return source.ClassFind(f,a[0],a[1],a[2],a[3],a[4]);
   case 0xb44b0:Arity(a,5);return source.SingleFind(f,a[0],a[1],a[2],a[3],a[4]);
   case 0xb55e0:Arity(a,5);return source.MultipleFind(f,a[0],a[1],a[2],a[3],a[4]);
   default:throw new ArgumentException("Unbound original RTTI method "+method.ToString("x"));
  }}
 }
}
