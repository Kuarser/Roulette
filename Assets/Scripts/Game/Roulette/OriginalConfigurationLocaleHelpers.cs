using System;
namespace RouletteDeluxe
{
 // Physical packaged wide mask conversion and locale clone. Static C# labels
 // preserve original helper control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Platform wctype_l and duplocale remain required.
 public sealed class OriginalConfigurationLocaleHelpers
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Func<uint,ushort> ReadHalf;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly Host h;
  public OriginalConfigurationLocaleHelpers(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadHalf==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadHalf=host.ReadHalf,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  uint H(uint at){return h.ReadHalf(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ConvertToWideMask(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=argument&65535u;W(frame+0x20u,owner);W(frame+0x24u,argument);goto L63994;
   L63994: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L63998;
   L63998: // CMP AX,0x800
    a=(eax&65535u);b=2048u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6399c;
   L6399c: // JZ 0x00063b40
    if(zero)goto L63b40;goto L639a2;
   L639a2: // JBE 0x000639d0
    if(carry||zero)goto L639d0;goto L639a4;
   L639a4: // CMP AX,0x1000
    a=(eax&65535u);b=4096u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639a8;
   L639a8: // JZ 0x00063ae0
    if(zero)goto L63ae0;goto L639ae;
   L639ae: // NOP
    goto L639b0;
   L639b0: // JBE 0x00063a08
    if(carry||zero)goto L63a08;goto L639b2;
   L639b2: // CMP AX,0x2000
    a=(eax&65535u);b=8192u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639b6;
   L639b6: // JZ 0x00063aa0
    if(zero)goto L63aa0;goto L639bc;
   L639bc: // CMP AX,0x4000
    a=(eax&65535u);b=16384u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639c0;
   L639c0: // JZ 0x00063a60
    if(zero)goto L63a60;goto L639c6;
   L639c6: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L639c8;
   L639c8:return eax;
   L639d0: // CMP AX,0x100
    a=(eax&65535u);b=256u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639d4;
   L639d4: // JZ 0x00063b20
    if(zero)goto L63b20;goto L639da;
   L639da: // JBE 0x00063a30
    if(carry||zero)goto L63a30;goto L639dc;
   L639dc: // CMP AX,0x200
    a=(eax&65535u);b=512u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639e0;
   L639e0: // JZ 0x00063a80
    if(zero)goto L63a80;goto L639e6;
   L639e6: // CMP AX,0x400
    a=(eax&65535u);b=1024u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L639ea;
   L639ea: // JNZ 0x000639c6
    if(!zero)goto L639c6;goto L639ec;
   L639ec: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L639ef;
   L639ef: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L639f3;
   L639f3: // LEA EAX,[EBX + 0xfffd8e13]
    eax=unchecked(ebx+4294807059u);goto L639f9;
   L639f9: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L639fc;
   L639fc: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63a01;
   L63a01: // JMP 0x000639c8
    goto L639c8;
   L63a08: // CMP AX,0xc00
    a=(eax&65535u);b=3072u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L63a0c;
   L63a0c: // JZ 0x00063ac0
    if(zero)goto L63ac0;goto L63a12;
   L63a12: // CMP AX,0xc04
    a=(eax&65535u);b=3076u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L63a16;
   L63a16: // JNZ 0x000639c6
    if(!zero)goto L639c6;goto L63a18;
   L63a18: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63a1b;
   L63a1b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63a1f;
   L63a1f: // LEA EAX,[EBX + 0xfffd8e2c]
    eax=unchecked(ebx+4294807084u);goto L63a25;
   L63a25: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63a28;
   L63a28: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63a2d;
   L63a2d: // JMP 0x000639c8
    goto L639c8;
   L63a30: // CMP AX,0x2
    a=(eax&65535u);b=2u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L63a34;
   L63a34: // JZ 0x00063b00
    if(zero)goto L63b00;goto L63a3a;
   L63a3a: // CMP AX,0x4
    a=(eax&65535u);b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L63a3e;
   L63a3e: // NOP
    goto L63a40;
   L63a40: // JNZ 0x000639c6
    if(!zero)goto L639c6;goto L63a42;
   L63a42: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63a45;
   L63a45: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63a49;
   L63a49: // LEA EAX,[EBX + 0xfffd8e19]
    eax=unchecked(ebx+4294807065u);goto L63a4f;
   L63a4f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63a52;
   L63a52: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63a57;
   L63a57: // JMP 0x000639c8
    goto L639c8;
   L63a60: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63a63;
   L63a63: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63a67;
   L63a67: // LEA EAX,[EBX + 0xfffd8dfb]
    eax=unchecked(ebx+4294807035u);goto L63a6d;
   L63a6d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63a70;
   L63a70: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63a75;
   L63a75: // JMP 0x000639c8
    goto L639c8;
   L63a80: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63a83;
   L63a83: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63a87;
   L63a87: // LEA EAX,[EBX + 0xfffd8e0d]
    eax=unchecked(ebx+4294807053u);goto L63a8d;
   L63a8d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63a90;
   L63a90: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63a95;
   L63a95: // JMP 0x000639c8
    goto L639c8;
   L63aa0: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63aa3;
   L63aa3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63aa7;
   L63aa7: // LEA EAX,[EBX + 0xfffd8df5]
    eax=unchecked(ebx+4294807029u);goto L63aad;
   L63aad: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63ab0;
   L63ab0: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63ab5;
   L63ab5: // JMP 0x000639c8
    goto L639c8;
   L63ac0: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63ac3;
   L63ac3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63ac7;
   L63ac7: // LEA EAX,[EBX + 0xfffd8e26]
    eax=unchecked(ebx+4294807078u);goto L63acd;
   L63acd: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63ad0;
   L63ad0: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63ad5;
   L63ad5: // JMP 0x000639c8
    goto L639c8;
   L63ae0: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63ae3;
   L63ae3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63ae7;
   L63ae7: // LEA EAX,[EBX + 0xfffd8e1f]
    eax=unchecked(ebx+4294807071u);goto L63aed;
   L63aed: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63af0;
   L63af0: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63af5;
   L63af5: // JMP 0x000639c8
    goto L639c8;
   L63b00: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63b03;
   L63b03: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63b07;
   L63b07: // LEA EAX,[EBX + 0xfffd8e01]
    eax=unchecked(ebx+4294807041u);goto L63b0d;
   L63b0d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63b10;
   L63b10: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63b15;
   L63b15: // JMP 0x000639c8
    goto L639c8;
   L63b20: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63b23;
   L63b23: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63b27;
   L63b27: // LEA EAX,[EBX + 0xfffd8e07]
    eax=unchecked(ebx+4294807047u);goto L63b2d;
   L63b2d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63b30;
   L63b30: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63b35;
   L63b35: // JMP 0x000639c8
    goto L639c8;
   L63b40: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto L63b43;
   L63b43: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L63b47;
   L63b47: // LEA EAX,[EBX + 0xfffd8e20]
    eax=unchecked(ebx+4294807072u);goto L63b4d;
   L63b4d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L63b50;
   L63b50: // CALL 0x00049090
    eax=Call(0x49090,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L63b55;
   L63b55: // JMP 0x000639c8
    goto L639c8;
  }
  public uint CloneCLocale(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;W(frame+0x20u,owner);goto L68283;
   L68283: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto L68285;
   L68285: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L68288;
   L68288: // CALL 0x0004a0c0
    eax=Call(0x4a0c0,R(unchecked(esp+0u)));goto L6828d;
   L6828d:return eax;
  }
 }
}
