using System;
namespace RouletteDeluxe
{
 public sealed class OriginalJurisdictionReadString
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord;public Action<uint,byte> WriteByte;
   public Func<uint,uint,uint> InvokeUsingEEPROM;
   public Action<uint,uint,uint> ConstructLiteral,CopyBytes,AssignBytes,InsertOffset;
   public Action<uint> DestroyString;public Action<uint,uint,uint,uint,uint> Fault;
   public Func<Exception,bool> IsNativeUnwind;public Action<Exception> Resume;
  }
  public sealed class Layout{public uint EmptyCString,FaultFormat,SourceFile,FunctionCString;}
  private readonly Host host;private readonly Layout layout;
  public OriginalJurisdictionReadString(Host h,Layout l)
  {
   if(h==null||l==null)throw new ArgumentException("Actual ReadString host and layout required");host=new Host();foreach(var f in typeof(Host).GetFields()){object v=f.GetValue(h);if(v==null)throw new ArgumentException("Actual ReadString binding required: "+f.Name);f.SetValue(host,v);}layout=new Layout();foreach(var f in typeof(Layout).GetFields()){uint v=(uint)f.GetValue(l);if(v==0)throw new ArgumentException("Actual ReadString CString required: "+f.Name);f.SetValue(layout,v);}
  }
  private static uint A(uint a,int b){return unchecked(a+(uint)b);}
  private T Call<T>(Func<T> call,uint frame,bool destroy)
  {try{return call();}catch(Exception ex){if(!host.IsNativeUnwind(ex))throw;if(destroy)host.DestroyString(host.ReadWord(A(frame,8)));host.Resume(ex);throw new InvalidOperationException("Original nonreturning ReadString Resume returned");}}
  private void Call(Action call,uint frame,bool destroy){Call(()=>{call();return 0;},frame,destroy);}
  // frame is the original EBP, including live hidden-result/machine/offset/length slots.
  public uint Read(uint frame)
  {
   if(frame==0)throw new ArgumentException("Live ReadString caller frame required");
   uint machine=host.ReadWord(A(frame,12)),length=host.ReadWord(A(frame,20)),vt=host.ReadWord(machine);
   uint usingEeprom=Call(()=>host.InvokeUsingEEPROM(host.ReadWord(A(vt,0x30)),machine),frame,false);
   if((usingEeprom&255)==0)
   {
    uint currentMachine=Call(()=>host.ReadWord(A(frame,12)),frame,false);
    if(Call(()=>host.ReadWord(A(currentMachine,8)),frame,false)==0)
    {
     Call(()=>host.Fault(16,layout.FaultFormat,layout.SourceFile,0x282,layout.FunctionCString),frame,false);
     throw new InvalidOperationException("Original nonreturning ReadString Fault returned");
    }
   }
   uint result=Call(()=>host.ReadWord(A(frame,8)),frame,false);
   Call(()=>host.ConstructLiteral(result,layout.EmptyCString,A(frame,-0x19)),frame,false);
   machine=host.ReadWord(A(frame,12));uint allocation=unchecked(length+0x1f)&0xfffffff0;
   uint buffer=unchecked(frame-0x58-allocation+0x23)&0xfffffff0;
   uint offset=host.ReadWord(A(frame,16)),source=host.ReadWord(A(machine,0x30));host.CopyBytes(buffer,unchecked(source+offset),length);host.WriteByte(unchecked(buffer+length),0);
   uint scan=buffer,word,zero;
   do{word=host.ReadWord(scan);scan=unchecked(scan+4);zero=unchecked(word+0xfefefeff)&~word&0x80808080;}while(zero==0);
   if((zero&0x8080)==0){zero>>=16;scan=unchecked(scan+2);}
   bool carry=((zero&255)*2)>255;uint count=unchecked(scan-3-(carry?1u:0u)-buffer);
   result=host.ReadWord(A(frame,8));Call(()=>host.AssignBytes(result,buffer,count),frame,true);
   if(unchecked((int)length)>0)
   {
    machine=Call(()=>host.ReadWord(A(frame,12)),frame,true);uint tree=A(machine,0x12c);
    for(uint index=0;index!=length;++index)
    {
     uint currentOffset=Call(()=>host.ReadWord(A(frame,16)),frame,true);
     Call(()=>host.WriteWord(A(frame,-0x20),unchecked(currentOffset+index)),frame,true);
     Call(()=>host.InsertOffset(A(frame,-0x30),tree,A(frame,-0x20)),frame,true);
    }
   }
   return host.ReadWord(A(frame,8));
  }
 }
}
