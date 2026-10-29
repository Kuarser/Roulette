using System;
namespace RouletteDeluxe
{
 // Physical packaged vmi_class_type_info dyncast 000b4d40. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; eight original inputs are at +90..+ac. strcmp, recursive dyncast and public source search remain required. Return exposes the original low byte.
 public sealed class OriginalConfigurationRttiVmiDyncast
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly Host h;
  public OriginalConfigurationRttiVmiDyncast(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Dyncast(uint frame,uint owner,uint hint,uint path,uint target,uint objectPtr,uint sourceType,uint sourcePtr,uint result)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=result;W(frame+0x90u,owner);W(frame+0x94u,hint);W(frame+0x98u,path);W(frame+0x9cu,target);W(frame+0xa0u,objectPtr);W(frame+0xa4u,sourceType);W(frame+0xa8u,sourcePtr);W(frame+0xacu,result);goto Lb4d59;
   Lb4d59: // TEST byte ptr [ESI + 0x10],0x10
    a=B(unchecked(esi+16u));b=16u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4d5d;
   Lb4d5d: // JZ 0x000b4d6c
    if(zero)goto Lb4d6c;goto Lb4d5f;
   Lb4d5f: // MOV EDX,dword ptr [ESP + 0x90]
    edx=R(unchecked(esp+144u));goto Lb4d66;
   Lb4d66: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto Lb4d69;
   Lb4d69: // MOV dword ptr [ESI + 0x10],EAX
    W(unchecked(esi+16u),eax);goto Lb4d6c;
   Lb4d6c: // MOV ECX,dword ptr [ESP + 0xa8]
    ecx=R(unchecked(esp+168u));goto Lb4d73;
   Lb4d73: // CMP dword ptr [ESP + 0xa0],ECX
    a=R(unchecked(esp+160u));b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4d7a;
   Lb4d7a: // JZ 0x000b51d0
    if(zero)goto Lb51d0;goto Lb4d80;
   Lb4d80: // MOV EDX,dword ptr [ESP + 0x90]
    edx=R(unchecked(esp+144u));goto Lb4d87;
   Lb4d87: // MOV EDI,dword ptr [EDX + 0x4]
    edi=R(unchecked(edx+4u));goto Lb4d8a;
   Lb4d8a: // MOV ECX,dword ptr [ESP + 0x9c]
    ecx=R(unchecked(esp+156u));goto Lb4d91;
   Lb4d91: // MOV EAX,dword ptr [ECX + 0x4]
    eax=R(unchecked(ecx+4u));goto Lb4d94;
   Lb4d94: // CMP EAX,EDI
    a=eax;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4d96;
   Lb4d96: // JZ 0x000b5248
    if(zero)goto Lb5248;goto Lb4d9c;
   Lb4d9c: // CMP byte ptr [EDI],0x2a
    a=B(unchecked(edi));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4d9f;
   Lb4d9f: // JZ 0x000b4db5
    if(zero)goto Lb4db5;goto Lb4da1;
   Lb4da1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb4da5;
   Lb4da5: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto Lb4da8;
   Lb4da8: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb4dad;
   Lb4dad: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4daf;
   Lb4daf: // JZ 0x000b5248
    if(zero)goto Lb5248;goto Lb4db5;
   Lb4db5: // MOV EAX,dword ptr [ESP + 0x94]
    eax=R(unchecked(esp+148u));goto Lb4dbc;
   Lb4dbc: // MOV EBP,dword ptr [ESP + 0xa8]
    ebp=R(unchecked(esp+168u));goto Lb4dc3;
   Lb4dc3: // MOV EDX,dword ptr [ESP + 0x98]
    edx=R(unchecked(esp+152u));goto Lb4dca;
   Lb4dca: // SUB EBP,dword ptr [ESP + 0x94]
    a=ebp;b=R(unchecked(esp+148u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebp=v;goto Lb4dd1;
   Lb4dd1: // SAR EAX,0x1f
    a=eax;v=unchecked((uint)((int)a>>31));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto Lb4dd4;
   Lb4dd4: // NOT EAX
    eax=~(eax);goto Lb4dd6;
   Lb4dd6: // AND EBP,EAX
    a=ebp;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto Lb4dd8;
   Lb4dd8: // OR EDX,0x1
    a=edx;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto Lb4ddb;
   Lb4ddb: // MOV dword ptr [ESP + 0x44],EBP
    W(unchecked(esp+68u),ebp);goto Lb4ddf;
   Lb4ddf: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb4de4;
   Lb4de4: // MOV byte ptr [ESP + 0x4f],0x0
    h.WriteByte(unchecked(esp+79u),unchecked((byte)(0u)));goto Lb4de9;
   Lb4de9: // MOV byte ptr [ESP + 0x4e],0x1
    h.WriteByte(unchecked(esp+78u),unchecked((byte)(1u)));goto Lb4dee;
   Lb4dee: // MOV dword ptr [ESP + 0x48],EDX
    W(unchecked(esp+72u),edx);goto Lb4df2;
   Lb4df2: // MOV ECX,dword ptr [ESP + 0x90]
    ecx=R(unchecked(esp+144u));goto Lb4df9;
   Lb4df9: // MOV EDI,dword ptr [ECX + 0xc]
    edi=R(unchecked(ecx+12u));goto Lb4dfc;
   Lb4dfc: // LEA ESI,[ESI]
    esi=unchecked(esi);goto Lb4e00;
   Lb4e00: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4e02;
   Lb4e02: // JZ 0x000b50c5
    if(zero)goto Lb50c5;goto Lb4e08;
   Lb4e08: // MOV EDX,dword ptr [ESP + 0x90]
    edx=R(unchecked(esp+144u));goto Lb4e0f;
   Lb4e0f: // SUB EDI,0x1
    a=edi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edi=v;goto Lb4e12;
   Lb4e12: // MOV EBP,dword ptr [ESI + 0x10]
    ebp=R(unchecked(esi+16u));goto Lb4e15;
   Lb4e15: // MOV dword ptr [ESP + 0x5c],0x0
    W(unchecked(esp+92u),0u);goto Lb4e1d;
   Lb4e1d: // MOV EAX,dword ptr [ESP + 0x98]
    eax=R(unchecked(esp+152u));goto Lb4e24;
   Lb4e24: // MOV dword ptr [ESP + 0x60],0x0
    W(unchecked(esp+96u),0u);goto Lb4e2c;
   Lb4e2c: // MOV ECX,dword ptr [EDX + EDI*0x8 + 0x14]
    ecx=R(unchecked(edx+edi*0x8u+20u));goto Lb4e30;
   Lb4e30: // MOV dword ptr [ESP + 0x2c],EBP
    W(unchecked(esp+44u),ebp);goto Lb4e34;
   Lb4e34: // MOV dword ptr [ESP + 0x6c],EBP
    W(unchecked(esp+108u),ebp);goto Lb4e38;
   Lb4e38: // MOV dword ptr [ESP + 0x64],0x0
    W(unchecked(esp+100u),0u);goto Lb4e40;
   Lb4e40: // MOV EBP,ECX
    ebp=ecx;goto Lb4e42;
   Lb4e42: // SAR EBP,0x8
    a=ebp;v=unchecked((uint)((int)a>>8));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&128u)!=0;overflow=false;ebp=v;goto Lb4e45;
   Lb4e45: // TEST CL,0x1
    a=(ecx&255u);b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4e48;
   Lb4e48: // MOV dword ptr [ESP + 0x68],0x0
    W(unchecked(esp+104u),0u);goto Lb4e50;
   Lb4e50: // MOV dword ptr [ESP + 0x3c],EBP
    W(unchecked(esp+60u),ebp);goto Lb4e54;
   Lb4e54: // JZ 0x000b4e6a
    if(zero)goto Lb4e6a;goto Lb4e56;
   Lb4e56: // MOV EDX,dword ptr [ESP + 0xa0]
    edx=R(unchecked(esp+160u));goto Lb4e5d;
   Lb4e5d: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto Lb4e61;
   Lb4e61: // ADD EBP,dword ptr [EDX]
    a=ebp;b=R(unchecked(edx));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebp=v;goto Lb4e63;
   Lb4e63: // MOV EBP,dword ptr [EBP]
    ebp=R(unchecked(ebp));goto Lb4e66;
   Lb4e66: // MOV dword ptr [ESP + 0x3c],EBP
    W(unchecked(esp+60u),ebp);goto Lb4e6a;
   Lb4e6a: // MOV EBP,dword ptr [ESP + 0x44]
    ebp=R(unchecked(esp+68u));goto Lb4e6e;
   Lb4e6e: // MOV EDX,dword ptr [ESP + 0x3c]
    edx=R(unchecked(esp+60u));goto Lb4e72;
   Lb4e72: // ADD EDX,dword ptr [ESP + 0xa0]
    a=edx;b=R(unchecked(esp+160u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto Lb4e79;
   Lb4e79: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4e7b;
   Lb4e7b: // MOV dword ptr [ESP + 0x40],EDX
    W(unchecked(esp+64u),edx);goto Lb4e7f;
   Lb4e7f: // JZ 0x000b4e94
    if(zero)goto Lb4e94;goto Lb4e81;
   Lb4e81: // MOV EBP,dword ptr [ESP + 0x44]
    ebp=R(unchecked(esp+68u));goto Lb4e85;
   Lb4e85: // CMP EDX,EBP
    a=edx;b=ebp;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4e87;
   Lb4e87: // SETA DL
    edx=(edx&0xffffff00u)|((!carry&&!zero?1u:0u)&255u);goto Lb4e8a;
   Lb4e8a: // CMP DL,byte ptr [ESP + 0x4e]
    a=(edx&255u);b=B(unchecked(esp+78u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4e8e;
   Lb4e8e: // JZ 0x000b50b8
    if(zero)goto Lb50b8;goto Lb4e94;
   Lb4e94: // AND ECX,0x2
    a=ecx;b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb4e97;
   Lb4e97: // JNZ 0x000b4eaa
    if(!zero)goto Lb4eaa;goto Lb4e99;
   Lb4e99: // CMP dword ptr [ESP + 0x94],-0x2
    a=R(unchecked(esp+148u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4ea1;
   Lb4ea1: // JZ 0x000b50e8
    if(zero)goto Lb50e8;goto Lb4ea7;
   Lb4ea7: // AND EAX,0xfffffffd
    a=eax;b=4294967293u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb4eaa;
   Lb4eaa: // MOV EBP,dword ptr [ESP + 0x90]
    ebp=R(unchecked(esp+144u));goto Lb4eb1;
   Lb4eb1: // LEA EDX,[ESP + 0x5c]
    edx=unchecked(esp+92u);goto Lb4eb5;
   Lb4eb5: // MOV ECX,dword ptr [EBP + EDI*0x8 + 0x10]
    ecx=R(unchecked(ebp+edi*0x8u+16u));goto Lb4eb9;
   Lb4eb9: // MOV EBP,dword ptr [ECX]
    ebp=R(unchecked(ecx));goto Lb4ebb;
   Lb4ebb: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto Lb4ebf;
   Lb4ebf: // MOV EDX,dword ptr [ESP + 0xa8]
    edx=R(unchecked(esp+168u));goto Lb4ec6;
   Lb4ec6: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto Lb4eca;
   Lb4eca: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto Lb4ecd;
   Lb4ecd: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto Lb4ed1;
   Lb4ed1: // MOV EDX,dword ptr [ESP + 0xa4]
    edx=R(unchecked(esp+164u));goto Lb4ed8;
   Lb4ed8: // MOV dword ptr [ESP + 0x14],EDX
    W(unchecked(esp+20u),edx);goto Lb4edc;
   Lb4edc: // MOV EDX,dword ptr [ESP + 0x40]
    edx=R(unchecked(esp+64u));goto Lb4ee0;
   Lb4ee0: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto Lb4ee4;
   Lb4ee4: // MOV EDX,dword ptr [ESP + 0x9c]
    edx=R(unchecked(esp+156u));goto Lb4eeb;
   Lb4eeb: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto Lb4eef;
   Lb4eef: // MOV EDX,dword ptr [ESP + 0x94]
    edx=R(unchecked(esp+148u));goto Lb4ef6;
   Lb4ef6: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb4efa;
   Lb4efa: // CALL dword ptr [EBP + 0x1c]
    eax=Call(R(unchecked(ebp+28u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)),R(unchecked(esp+20u)),R(unchecked(esp+24u)),R(unchecked(esp+28u)));goto Lb4efd;
   Lb4efd: // MOV EBP,dword ptr [ESP + 0x68]
    ebp=R(unchecked(esp+104u));goto Lb4f01;
   Lb4f01: // MOV byte ptr [ESP + 0x40],AL
    h.WriteByte(unchecked(esp+64u),unchecked((byte)((eax&255u))));goto Lb4f05;
   Lb4f05: // MOV EAX,dword ptr [ESP + 0x64]
    eax=R(unchecked(esp+100u));goto Lb4f09;
   Lb4f09: // OR EAX,dword ptr [ESI + 0x8]
    a=eax;b=R(unchecked(esi+8u));v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb4f0c;
   Lb4f0c: // CMP EBP,0x2
    a=ebp;b=2u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f0f;
   Lb4f0f: // MOV EDX,EAX
    edx=eax;goto Lb4f11;
   Lb4f11: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto Lb4f14;
   Lb4f14: // JZ 0x000b5220
    if(zero)goto Lb5220;goto Lb4f1a;
   Lb4f1a: // CMP EBP,0x6
    a=ebp;b=6u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f1d;
   Lb4f1d: // JZ 0x000b5220
    if(zero)goto Lb5220;goto Lb4f23;
   Lb4f23: // MOV ECX,dword ptr [ESI]
    ecx=R(unchecked(esi));goto Lb4f25;
   Lb4f25: // CMP byte ptr [ESP + 0x4d],0x0
    a=B(unchecked(esp+77u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f2a;
   Lb4f2a: // MOV dword ptr [ESP + 0x3c],ECX
    W(unchecked(esp+60u),ecx);goto Lb4f2e;
   Lb4f2e: // JNZ 0x000b4fe8
    if(!zero)goto Lb4fe8;goto Lb4f34;
   Lb4f34: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f36;
   Lb4f36: // JZ 0x000b5190
    if(zero)goto Lb5190;goto Lb4f3c;
   Lb4f3c: // MOV ECX,dword ptr [ESP + 0x5c]
    ecx=R(unchecked(esp+92u));goto Lb4f40;
   Lb4f40: // CMP dword ptr [ESP + 0x3c],ECX
    a=R(unchecked(esp+60u));b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f44;
   Lb4f44: // JZ 0x000b5110
    if(zero)goto Lb5110;goto Lb4f4a;
   Lb4f4a: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f4c;
   Lb4f4c: // JZ 0x000b5120
    if(zero)goto Lb5120;goto Lb4f52;
   Lb4f52: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f55;
   Lb4f55: // MOV EDX,dword ptr [ESI + 0xc]
    edx=R(unchecked(esi+12u));goto Lb4f58;
   Lb4f58: // JLE 0x000b5008
    if(zero||sign!=overflow)goto Lb5008;goto Lb4f5e;
   Lb4f5e: // TEST AL,0x1
    a=(eax&255u);b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f60;
   Lb4f60: // JZ 0x000b4f6c
    if(zero)goto Lb4f6c;goto Lb4f62;
   Lb4f62: // TEST byte ptr [ESI + 0x10],0x2
    a=B(unchecked(esi+16u));b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f66;
   Lb4f66: // JNZ 0x000b5008
    if(!zero)goto Lb5008;goto Lb4f6c;
   Lb4f6c: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f6e;
   Lb4f6e: // MOV ECX,EDX
    ecx=edx;goto Lb4f70;
   Lb4f70: // JNZ 0x000b4f7c
    if(!zero)goto Lb4f7c;goto Lb4f72;
   Lb4f72: // MOV ECX,0x1
    ecx=1u;goto Lb4f77;
   Lb4f77: // MOV EDX,0x1
    edx=1u;goto Lb4f7c;
   Lb4f7c: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4f7e;
   Lb4f7e: // JZ 0x000b50f8
    if(zero)goto Lb50f8;goto Lb4f84;
   Lb4f84: // MOV dword ptr [ESP + 0x40],EBP
    W(unchecked(esp+64u),ebp);goto Lb4f88;
   Lb4f88: // MOV EAX,dword ptr [ESP + 0x40]
    eax=R(unchecked(esp+64u));goto Lb4f8c;
   Lb4f8c: // XOR EAX,ECX
    a=eax;b=ecx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb4f8e;
   Lb4f8e: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f91;
   Lb4f91: // JLE 0x000b5090
    if(zero||sign!=overflow)goto Lb5090;goto Lb4f97;
   Lb4f97: // CMP dword ptr [ESP + 0x40],0x3
    a=R(unchecked(esp+64u));b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4f9c;
   Lb4f9c: // JLE 0x000b4fb6
    if(zero||sign!=overflow)goto Lb4fb6;goto Lb4f9e;
   Lb4f9e: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb4fa2;
   Lb4fa2: // MOV EDX,EBP
    edx=ebp;goto Lb4fa4;
   Lb4fa4: // MOV ECX,dword ptr [ESP + 0x40]
    ecx=R(unchecked(esp+64u));goto Lb4fa8;
   Lb4fa8: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb4fad;
   Lb4fad: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto Lb4faf;
   Lb4faf: // MOV EAX,dword ptr [ESP + 0x60]
    eax=R(unchecked(esp+96u));goto Lb4fb3;
   Lb4fb3: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto Lb4fb6;
   Lb4fb6: // TEST CL,0x2
    a=(ecx&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4fb9;
   Lb4fb9: // MOV dword ptr [ESI + 0xc],EDX
    W(unchecked(esi+12u),edx);goto Lb4fbc;
   Lb4fbc: // JNZ 0x000b523e
    if(!zero)goto Lb523e;goto Lb4fc2;
   Lb4fc2: // AND ECX,0x1
    a=ecx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb4fc5;
   Lb4fc5: // JZ 0x000b523e
    if(zero)goto Lb523e;goto Lb4fcb;
   Lb4fcb: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto Lb4fce;
   Lb4fce: // CMP EDX,0x4
    a=edx;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4fd1;
   Lb4fd1: // JNZ 0x000b4e00
    if(!zero)goto Lb4e00;goto Lb4fd7;
   Lb4fd7: // MOVZX EAX,byte ptr [ESP + 0x4d]
    eax=B(unchecked(esp+77u));goto Lb4fdc;
   Lb4fdc:return (eax&255u);
   Lb4fe8: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4fea;
   Lb4fea: // JNZ 0x000b4f3c
    if(!zero)goto Lb4f3c;goto Lb4ff0;
   Lb4ff0: // MOV ECX,dword ptr [ESP + 0x5c]
    ecx=R(unchecked(esp+92u));goto Lb4ff4;
   Lb4ff4: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4ff6;
   Lb4ff6: // JZ 0x000b4fce
    if(zero)goto Lb4fce;goto Lb4ff8;
   Lb4ff8: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4ffb;
   Lb4ffb: // MOV EDX,dword ptr [ESI + 0xc]
    edx=R(unchecked(esi+12u));goto Lb4ffe;
   Lb4ffe: // JG 0x000b4f5e
    if(!zero&&sign==overflow)goto Lb4f5e;goto Lb5004;
   Lb5004: // LEA ESI,[ESI]
    esi=unchecked(esi);goto Lb5008;
   Lb5008: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb500a;
   Lb500a: // MOV ECX,EDX
    ecx=edx;goto Lb500c;
   Lb500c: // MOV dword ptr [ESP + 0x40],EBP
    W(unchecked(esp+64u),ebp);goto Lb5010;
   Lb5010: // JLE 0x000b5130
    if(zero||sign!=overflow)goto Lb5130;goto Lb5016;
   Lb5016: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5018;
   Lb5018: // JG 0x000b4f88
    if(!zero&&sign==overflow)goto Lb4f88;goto Lb501e;
   Lb501e: // CMP ECX,0x3
    a=ecx;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5021;
   Lb5021: // JLE 0x000b504a
    if(zero||sign!=overflow)goto Lb504a;goto Lb5023;
   Lb5023: // TEST CL,0x1
    a=(ecx&255u);b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5026;
   Lb5026: // MOV EBP,0x1
    ebp=1u;goto Lb502b;
   Lb502b: // MOV dword ptr [ESP + 0x40],0x1
    W(unchecked(esp+64u),1u);goto Lb5033;
   Lb5033: // JZ 0x000b4f88
    if(zero)goto Lb4f88;goto Lb5039;
   Lb5039: // MOV EAX,dword ptr [ESP + 0x90]
    eax=R(unchecked(esp+144u));goto Lb5040;
   Lb5040: // TEST byte ptr [EAX + 0x8],0x2
    a=B(unchecked(eax+8u));b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5044;
   Lb5044: // JZ 0x000b4f88
    if(zero)goto Lb4f88;goto Lb504a;
   Lb504a: // MOV EBP,dword ptr [ESP + 0x94]
    ebp=R(unchecked(esp+148u));goto Lb5051;
   Lb5051: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb5055;
   Lb5055: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5057;
   Lb5057: // JS 0x000b5318
    if(sign)goto Lb5318;goto Lb505d;
   Lb505d: // ADD EAX,dword ptr [ESP + 0x94]
    a=eax;b=R(unchecked(esp+148u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5064;
   Lb5064: // CMP dword ptr [ESP + 0xa8],EAX
    a=R(unchecked(esp+168u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb506b;
   Lb506b: // SETZ AL
    eax=(eax&0xffffff00u)|((zero?1u:0u)&255u);goto Lb506e;
   Lb506e: // MOVZX EBP,AL
    ebp=(eax&255u);goto Lb5071;
   Lb5071: // LEA EBP,[EBP + EBP*0x4 + 0x1]
    ebp=unchecked(ebp+ebp*0x4u+1u);goto Lb5075;
   Lb5075: // MOV dword ptr [ESP + 0x40],EBP
    W(unchecked(esp+64u),ebp);goto Lb5079;
   Lb5079: // MOVZX EBP,AL
    ebp=(eax&255u);goto Lb507c;
   Lb507c: // MOV EAX,dword ptr [ESP + 0x40]
    eax=R(unchecked(esp+64u));goto Lb5080;
   Lb5080: // LEA EBP,[EBP + EBP*0x4 + 0x1]
    ebp=unchecked(ebp+ebp*0x4u+1u);goto Lb5084;
   Lb5084: // XOR EAX,ECX
    a=eax;b=ecx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb5086;
   Lb5086: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5089;
   Lb5089: // JG 0x000b4f97
    if(!zero&&sign==overflow)goto Lb4f97;goto Lb508f;
   Lb508f: // NOP
    goto Lb5090;
   Lb5090: // AND ECX,dword ptr [ESP + 0x40]
    a=ecx;b=R(unchecked(esp+64u));v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb5094;
   Lb5094: // MOV dword ptr [ESI],0x0
    W(unchecked(esi),0u);goto Lb509a;
   Lb509a: // CMP ECX,0x3
    a=ecx;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb509d;
   Lb509d: // JG 0x000b5392
    if(!zero&&sign==overflow)goto Lb5392;goto Lb50a3;
   Lb50a3: // MOV dword ptr [ESI + 0xc],0x1
    W(unchecked(esi+12u),1u);goto Lb50aa;
   Lb50aa: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto Lb50ad;
   Lb50ad: // MOV byte ptr [ESP + 0x4d],0x1
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(1u)));goto Lb50b2;
   Lb50b2: // JMP 0x000b4fce
    goto Lb4fce;
   Lb50b8: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb50ba;
   Lb50ba: // MOV byte ptr [ESP + 0x4f],0x1
    h.WriteByte(unchecked(esp+79u),unchecked((byte)(1u)));goto Lb50bf;
   Lb50bf: // JNZ 0x000b4e08
    if(!zero)goto Lb4e08;goto Lb50c5;
   Lb50c5: // CMP byte ptr [ESP + 0x4f],0x0
    a=B(unchecked(esp+79u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb50ca;
   Lb50ca: // JZ 0x000b4fd7
    if(zero)goto Lb4fd7;goto Lb50d0;
   Lb50d0: // CMP byte ptr [ESP + 0x4e],0x0
    a=B(unchecked(esp+78u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb50d5;
   Lb50d5: // JZ 0x000b4fd7
    if(zero)goto Lb4fd7;goto Lb50db;
   Lb50db: // MOV byte ptr [ESP + 0x4e],0x0
    h.WriteByte(unchecked(esp+78u),unchecked((byte)(0u)));goto Lb50e0;
   Lb50e0: // JMP 0x000b4df2
    goto Lb4df2;
   Lb50e8: // TEST byte ptr [ESP + 0x2c],0x3
    a=B(unchecked(esp+44u));b=3u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb50ed;
   Lb50ed: // JZ 0x000b4e00
    if(zero)goto Lb4e00;goto Lb50f3;
   Lb50f3: // JMP 0x000b4ea7
    goto Lb4ea7;
   Lb50f8: // MOV dword ptr [ESP + 0x40],0x1
    W(unchecked(esp+64u),1u);goto Lb5100;
   Lb5100: // MOV EBP,0x1
    ebp=1u;goto Lb5105;
   Lb5105: // JMP 0x000b4f88
    goto Lb4f88;
   Lb5110: // MOV EAX,dword ptr [ESP + 0x60]
    eax=R(unchecked(esp+96u));goto Lb5114;
   Lb5114: // OR dword ptr [ESI + 0x4],EAX
    a=R(unchecked(esi+4u));b=eax;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;W(unchecked(esi+4u),v);goto Lb5117;
   Lb5117: // JMP 0x000b4fce
    goto Lb4fce;
   Lb5120: // CMP byte ptr [ESP + 0x40],0x0
    a=B(unchecked(esp+64u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5125;
   Lb5125: // JNZ 0x000b4f52
    if(!zero)goto Lb4f52;goto Lb512b;
   Lb512b: // JMP 0x000b4fce
    goto Lb4fce;
   Lb5130: // CMP EBP,0x3
    a=ebp;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5133;
   Lb5133: // JLE 0x000b515c
    if(zero||sign!=overflow)goto Lb515c;goto Lb5135;
   Lb5135: // TEST EBP,0x1
    a=ebp;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb513b;
   Lb513b: // MOV ECX,0x1
    ecx=1u;goto Lb5140;
   Lb5140: // MOV EDX,0x1
    edx=1u;goto Lb5145;
   Lb5145: // JZ 0x000b4f88
    if(zero)goto Lb4f88;goto Lb514b;
   Lb514b: // MOV EAX,dword ptr [ESP + 0x90]
    eax=R(unchecked(esp+144u));goto Lb5152;
   Lb5152: // TEST byte ptr [EAX + 0x8],0x2
    a=B(unchecked(eax+8u));b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5156;
   Lb5156: // JZ 0x000b4f88
    if(zero)goto Lb4f88;goto Lb515c;
   Lb515c: // MOV EAX,dword ptr [ESP + 0x94]
    eax=R(unchecked(esp+148u));goto Lb5163;
   Lb5163: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5165;
   Lb5165: // JS 0x000b52b8
    if(sign)goto Lb52b8;goto Lb516b;
   Lb516b: // MOV EAX,dword ptr [ESP + 0x3c]
    eax=R(unchecked(esp+60u));goto Lb516f;
   Lb516f: // XOR ECX,ECX
    a=ecx;b=ecx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb5171;
   Lb5171: // ADD EAX,dword ptr [ESP + 0x94]
    a=eax;b=R(unchecked(esp+148u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5178;
   Lb5178: // CMP dword ptr [ESP + 0xa8],EAX
    a=R(unchecked(esp+168u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb517f;
   Lb517f: // SETZ CL
    ecx=(ecx&0xffffff00u)|((zero?1u:0u)&255u);goto Lb5182;
   Lb5182: // LEA ECX,[ECX + ECX*0x4 + 0x1]
    ecx=unchecked(ecx+ecx*0x4u+1u);goto Lb5186;
   Lb5186: // MOV EDX,ECX
    edx=ecx;goto Lb5188;
   Lb5188: // JMP 0x000b5016
    goto Lb5016;
   Lb5190: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb5194;
   Lb5194: // MOV ECX,dword ptr [ESP + 0x60]
    ecx=R(unchecked(esp+96u));goto Lb5198;
   Lb5198: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto Lb519a;
   Lb519a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb519c;
   Lb519c: // MOVZX EAX,byte ptr [ESP + 0x40]
    eax=B(unchecked(esp+64u));goto Lb51a1;
   Lb51a1: // MOV dword ptr [ESI + 0x4],ECX
    W(unchecked(esi+4u),ecx);goto Lb51a4;
   Lb51a4: // MOV byte ptr [ESP + 0x4d],AL
    h.WriteByte(unchecked(esp+77u),unchecked((byte)((eax&255u))));goto Lb51a8;
   Lb51a8: // JZ 0x000b4fce
    if(zero)goto Lb4fce;goto Lb51ae;
   Lb51ae: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb51b0;
   Lb51b0: // JZ 0x000b4e00
    if(zero)goto Lb4e00;goto Lb51b6;
   Lb51b6: // MOV ECX,dword ptr [ESP + 0x90]
    ecx=R(unchecked(esp+144u));goto Lb51bd;
   Lb51bd: // TEST byte ptr [ECX + 0x8],0x1
    a=B(unchecked(ecx+8u));b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb51c1;
   Lb51c1: // JNZ 0x000b4fce
    if(!zero)goto Lb4fce;goto Lb51c7;
   Lb51c7: // JMP 0x000b4fd7
    goto Lb4fd7;
   Lb51d0: // MOV EBP,dword ptr [ESP + 0x90]
    ebp=R(unchecked(esp+144u));goto Lb51d7;
   Lb51d7: // MOV EDX,dword ptr [ESP + 0xa4]
    edx=R(unchecked(esp+164u));goto Lb51de;
   Lb51de: // MOV EDI,dword ptr [EBP + 0x4]
    edi=R(unchecked(ebp+4u));goto Lb51e1;
   Lb51e1: // MOV EAX,dword ptr [EDX + 0x4]
    eax=R(unchecked(edx+4u));goto Lb51e4;
   Lb51e4: // CMP EDI,EAX
    a=edi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb51e6;
   Lb51e6: // JZ 0x000b5205
    if(zero)goto Lb5205;goto Lb51e8;
   Lb51e8: // CMP byte ptr [EDI],0x2a
    a=B(unchecked(edi));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb51eb;
   Lb51eb: // JZ 0x000b4d8a
    if(zero)goto Lb4d8a;goto Lb51f1;
   Lb51f1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb51f5;
   Lb51f5: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto Lb51f8;
   Lb51f8: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb51fd;
   Lb51fd: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb51ff;
   Lb51ff: // JNZ 0x000b4d8a
    if(!zero)goto Lb4d8a;goto Lb5205;
   Lb5205: // MOV ECX,dword ptr [ESP + 0x98]
    ecx=R(unchecked(esp+152u));goto Lb520c;
   Lb520c: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb5211;
   Lb5211: // MOV dword ptr [ESI + 0x8],ECX
    W(unchecked(esi+8u),ecx);goto Lb5214;
   Lb5214: // JMP 0x000b4fd7
    goto Lb4fd7;
   Lb5220: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb5224;
   Lb5224: // MOV dword ptr [ESI + 0xc],EBP
    W(unchecked(esi+12u),ebp);goto Lb5227;
   Lb5227: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto Lb5229;
   Lb5229: // MOV EAX,dword ptr [ESP + 0x60]
    eax=R(unchecked(esp+96u));goto Lb522d;
   Lb522d: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto Lb5230;
   Lb5230: // MOVZX EAX,byte ptr [ESP + 0x40]
    eax=B(unchecked(esp+64u));goto Lb5235;
   Lb5235: // MOV byte ptr [ESP + 0x4d],AL
    h.WriteByte(unchecked(esp+77u),unchecked((byte)((eax&255u))));goto Lb5239;
   Lb5239: // JMP 0x000b4fd7
    goto Lb4fd7;
   Lb523e: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb5243;
   Lb5243: // JMP 0x000b4fd7
    goto Lb4fd7;
   Lb5248: // MOV EAX,dword ptr [ESP + 0x94]
    eax=R(unchecked(esp+148u));goto Lb524f;
   Lb524f: // MOV EBP,dword ptr [ESP + 0xa0]
    ebp=R(unchecked(esp+160u));goto Lb5256;
   Lb5256: // MOV EDX,dword ptr [ESP + 0x98]
    edx=R(unchecked(esp+152u));goto Lb525d;
   Lb525d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb525f;
   Lb525f: // MOV dword ptr [ESI],EBP
    W(unchecked(esi),ebp);goto Lb5261;
   Lb5261: // MOV dword ptr [ESI + 0x4],EDX
    W(unchecked(esi+4u),edx);goto Lb5264;
   Lb5264: // JS 0x000b5295
    if(sign)goto Lb5295;goto Lb5266;
   Lb5266: // MOV EAX,dword ptr [ESP + 0x94]
    eax=R(unchecked(esp+148u));goto Lb526d;
   Lb526d: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb5272;
   Lb5272: // ADD EAX,EBP
    a=eax;b=ebp;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5274;
   Lb5274: // CMP dword ptr [ESP + 0xa8],EAX
    a=R(unchecked(esp+168u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb527b;
   Lb527b: // SETZ AL
    eax=(eax&0xffffff00u)|((zero?1u:0u)&255u);goto Lb527e;
   Lb527e: // MOVZX EAX,AL
    eax=(eax&255u);goto Lb5281;
   Lb5281: // LEA EAX,[EAX + EAX*0x4 + 0x1]
    eax=unchecked(eax+eax*0x4u+1u);goto Lb5285;
   Lb5285: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto Lb5288;
   Lb5288: // MOVZX EAX,byte ptr [ESP + 0x4d]
    eax=B(unchecked(esp+77u));goto Lb528d;
   Lb528d:return (eax&255u);
   Lb5295: // CMP dword ptr [ESP + 0x94],-0x2
    a=R(unchecked(esp+148u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb529d;
   Lb529d: // MOV byte ptr [ESP + 0x4d],0x0
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(0u)));goto Lb52a2;
   Lb52a2: // JNZ 0x000b4fd7
    if(!zero)goto Lb4fd7;goto Lb52a8;
   Lb52a8: // MOV dword ptr [ESI + 0xc],0x1
    W(unchecked(esi+12u),1u);goto Lb52af;
   Lb52af: // JMP 0x000b4fd7
    goto Lb4fd7;
   Lb52b8: // CMP dword ptr [ESP + 0x94],-0x2
    a=R(unchecked(esp+148u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb52c0;
   Lb52c0: // MOV ECX,0x1
    ecx=1u;goto Lb52c5;
   Lb52c5: // MOV EDX,0x1
    edx=1u;goto Lb52ca;
   Lb52ca: // JZ 0x000b5016
    if(zero)goto Lb5016;goto Lb52d0;
   Lb52d0: // MOV EDX,dword ptr [ESP + 0x9c]
    edx=R(unchecked(esp+156u));goto Lb52d7;
   Lb52d7: // MOV ECX,dword ptr [ESP + 0xa8]
    ecx=R(unchecked(esp+168u));goto Lb52de;
   Lb52de: // MOV EAX,dword ptr [EDX]
    eax=R(unchecked(edx));goto Lb52e0;
   Lb52e0: // MOV EDX,dword ptr [ESP + 0xa4]
    edx=R(unchecked(esp+164u));goto Lb52e7;
   Lb52e7: // MOV dword ptr [ESP + 0x10],ECX
    W(unchecked(esp+16u),ecx);goto Lb52eb;
   Lb52eb: // MOV ECX,dword ptr [ESP + 0x3c]
    ecx=R(unchecked(esp+60u));goto Lb52ef;
   Lb52ef: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto Lb52f3;
   Lb52f3: // MOV EDX,dword ptr [ESP + 0x94]
    edx=R(unchecked(esp+148u));goto Lb52fa;
   Lb52fa: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto Lb52fe;
   Lb52fe: // MOV ECX,dword ptr [ESP + 0x9c]
    ecx=R(unchecked(esp+156u));goto Lb5305;
   Lb5305: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb5309;
   Lb5309: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto Lb530c;
   Lb530c: // CALL dword ptr [EAX + 0x20]
    eax=Call(R(unchecked(eax+32u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)));goto Lb530f;
   Lb530f: // MOV EDX,EAX
    edx=eax;goto Lb5311;
   Lb5311: // MOV ECX,EAX
    ecx=eax;goto Lb5313;
   Lb5313: // JMP 0x000b5016
    goto Lb5016;
   Lb5318: // CMP dword ptr [ESP + 0x94],-0x2
    a=R(unchecked(esp+148u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5320;
   Lb5320: // MOV EBP,0x1
    ebp=1u;goto Lb5325;
   Lb5325: // MOV dword ptr [ESP + 0x40],0x1
    W(unchecked(esp+64u),1u);goto Lb532d;
   Lb532d: // JZ 0x000b4f88
    if(zero)goto Lb4f88;goto Lb5333;
   Lb5333: // MOV EBP,dword ptr [ESP + 0x9c]
    ebp=R(unchecked(esp+156u));goto Lb533a;
   Lb533a: // MOV EBP,dword ptr [EBP]
    ebp=R(unchecked(ebp));goto Lb533d;
   Lb533d: // MOV dword ptr [ESP + 0x34],EDX
    W(unchecked(esp+52u),edx);goto Lb5341;
   Lb5341: // MOV dword ptr [ESP + 0x38],ECX
    W(unchecked(esp+56u),ecx);goto Lb5345;
   Lb5345: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto Lb5349;
   Lb5349: // MOV dword ptr [ESP + 0x30],EBP
    W(unchecked(esp+48u),ebp);goto Lb534d;
   Lb534d: // MOV EBP,dword ptr [ESP + 0xa8]
    ebp=R(unchecked(esp+168u));goto Lb5354;
   Lb5354: // MOV dword ptr [ESP + 0x10],EBP
    W(unchecked(esp+16u),ebp);goto Lb5358;
   Lb5358: // MOV EBP,dword ptr [ESP + 0xa4]
    ebp=R(unchecked(esp+164u));goto Lb535f;
   Lb535f: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb5363;
   Lb5363: // MOV EBP,dword ptr [ESP + 0x94]
    ebp=R(unchecked(esp+148u));goto Lb536a;
   Lb536a: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto Lb536e;
   Lb536e: // MOV EBP,dword ptr [ESP + 0x9c]
    ebp=R(unchecked(esp+156u));goto Lb5375;
   Lb5375: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto Lb5378;
   Lb5378: // MOV EBP,dword ptr [ESP + 0x30]
    ebp=R(unchecked(esp+48u));goto Lb537c;
   Lb537c: // CALL dword ptr [EBP + 0x20]
    eax=Call(R(unchecked(ebp+32u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)));goto Lb537f;
   Lb537f: // MOV EDX,dword ptr [ESP + 0x34]
    edx=R(unchecked(esp+52u));goto Lb5383;
   Lb5383: // MOV ECX,dword ptr [ESP + 0x38]
    ecx=R(unchecked(esp+56u));goto Lb5387;
   Lb5387: // MOV EBP,EAX
    ebp=eax;goto Lb5389;
   Lb5389: // MOV dword ptr [ESP + 0x40],EAX
    W(unchecked(esp+64u),eax);goto Lb538d;
   Lb538d: // JMP 0x000b4f88
    goto Lb4f88;
   Lb5392: // MOV dword ptr [ESI + 0xc],0x2
    W(unchecked(esi+12u),2u);goto Lb5399;
   Lb5399: // MOV byte ptr [ESP + 0x4d],0x1
    h.WriteByte(unchecked(esp+77u),unchecked((byte)(1u)));goto Lb539e;
   Lb539e: // JMP 0x000b4fd7
    goto Lb4fd7;
  }
 }
}
