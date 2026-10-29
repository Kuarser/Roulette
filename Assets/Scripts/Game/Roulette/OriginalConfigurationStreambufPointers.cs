using System;
namespace RouletteDeluxe {
 // Packaged stringbuf _M_sync 00091e60, _M_pbump 00091e10 and string swap 000980b0.
 // Public writes preserve their native order; every pointer is a physical address.
 public sealed class OriginalConfigurationStreambufPointers {
  public sealed class Host {public Func<uint,uint> ReadWord;public Action<uint,uint> WriteWord;}
  readonly Host h;
  public OriginalConfigurationStreambufPointers(Host host){if(host==null||host.ReadWord==null||host.WriteWord==null)throw new ArgumentException("Physical streambuf and COW string memory required");h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord};}
  static uint A(uint a,uint b){return unchecked(a+b);}uint R(uint a){return h.ReadWord(a);}void W(uint a,uint v){h.WriteWord(a,v);}
  public void Bump(uint owner,uint begin,uint end,long offset){
   W(A(owner,0x14),begin);W(A(owner,0x10),begin);W(A(owner,0x18),end);
   while(offset>int.MaxValue){begin=A(begin,0x7fffffffu);offset=unchecked(offset-int.MaxValue);}
   W(A(owner,0x14),A(begin,unchecked((uint)offset)));
  }
  public void Synchronize(uint owner,uint buffer,uint inputOffset,uint outputOffset){
   uint mode=R(A(owner,0x20)),data=R(A(owner,0x24));bool input=(mode&8)!=0;uint length=R(unchecked(data-12)),output=mode&0x10,capacity=R(unchecked(data-8));
   uint inputEnd=A(buffer,length),outputEnd;
   if(buffer==data)outputEnd=A(buffer,capacity);else{inputEnd=A(inputEnd,inputOffset);outputEnd=inputEnd;inputOffset=0;}
   if(input){uint current=A(buffer,inputOffset);W(A(owner,4),buffer);W(A(owner,8),current);W(A(owner,12),inputEnd);}
   if(output!=0){Bump(owner,buffer,outputEnd,outputOffset);if(!input){W(A(owner,4),inputEnd);W(A(owner,8),inputEnd);W(A(owner,12),inputEnd);}}
  }
  public void Swap(uint first,uint second){
   uint a=R(first),refs=R(unchecked(a-4)),b;
   if(unchecked((int)refs)<0){b=R(second);W(unchecked(a-4),0);if(unchecked((int)R(unchecked(b-4)))<0)W(unchecked(b-4),0);}
   else{b=R(second);if(unchecked((int)R(unchecked(b-4)))<0)W(unchecked(b-4),0);}
   W(first,b);W(second,a);
  }
 }
}
