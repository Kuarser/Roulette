using System;
namespace RouletteDeluxe
{
 // _GLOBAL__sub_I_CFGValueHolder.cpp 08187160. Register/PIC and native at-exit transport remain admitted.
 public sealed class OriginalCfgValueTypeInitialization
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint> ConstructIo;
   public Action<uint,uint,uint> ConstructCString;
   public Func<uint,uint,uint,int> RegisterAtExit;
  }
  [Serializable] public sealed class Layout
  {
   public uint IoOwner,DsoHandleSlot,IoDestructorSlot,StringDestructorSlot;
   public uint[] Owners,Literals;
  }
  private readonly Host host;private readonly Layout layout;
  public OriginalCfgValueTypeInitialization(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual type initialization host and relocated layout required");host=new Host();
   foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Type initialization binding required: "+f.Name);f.SetValue(host,v);}
   if(l.IoOwner==0||l.DsoHandleSlot==0||l.IoDestructorSlot==0||l.StringDestructorSlot==0||l.Owners==null||l.Literals==null||l.Owners.Length!=11||l.Literals.Length!=11)throw new ArgumentException("Complete original type initialization layout required");
   layout=new Layout{IoOwner=l.IoOwner,DsoHandleSlot=l.DsoHandleSlot,IoDestructorSlot=l.IoDestructorSlot,StringDestructorSlot=l.StringDestructorSlot,Owners=(uint[])l.Owners.Clone(),Literals=(uint[])l.Literals.Clone()};
   for(int i=0;i<11;++i)if(layout.Owners[i]==0||layout.Literals[i]==0)throw new ArgumentException("Original type owner and literal required");
  }
  public void Initialize(uint frame)
  {
   host.ConstructIo(layout.IoOwner);
   uint dso=host.ReadWord(layout.DsoHandleSlot),ioDestructor=host.ReadWord(layout.IoDestructorSlot);
   host.RegisterAtExit(ioDestructor,layout.IoOwner,dso);
   host.ConstructCString(layout.Owners[0],layout.Literals[0],unchecked(frame+21));
   uint stringDestructor=host.ReadWord(layout.StringDestructorSlot);
   host.RegisterAtExit(stringDestructor,layout.Owners[0],dso);
   for(int i=1;i<11;++i){host.ConstructCString(layout.Owners[i],layout.Literals[i],unchecked(frame+21+(uint)i));host.RegisterAtExit(stringDestructor,layout.Owners[i],dso);}
  }
 }
}
