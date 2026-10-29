using System;
namespace RouletteDeluxe
{
 // Physical string-vector insert_aux 09498ed0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; vector/position/value are regparm inputs.
 public sealed class OriginalConfigurationStringVector
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationStringVector(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Insert(uint frame,uint vector,uint position,uint value)
  {
   uint eax=vector,ebx=value,esi=0,edi=position,ecx=value,edx=position,ebp=vector,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L9498edd;
   L9498edd: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto L9498ee0;
   L9498ee0: // CMP EAX,dword ptr [EBP + 0x8]
    a=eax;b=R(unchecked(ebp+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498ee3;
   L9498ee3: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L9498ee7;
   L9498ee7: // JZ 0x09498f72
    if(zero)goto L9498f72;goto L9498eed;
   L9498eed: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L9498eef;
   L9498eef: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498ef1;
   L9498ef1: // JZ 0x09498f05
    if(zero)goto L9498f05;goto L9498ef3;
   L9498ef3: // LEA EDX,[EAX + -0x4]
    edx=unchecked(eax+4294967292u);goto L9498ef6;
   L9498ef6: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L9498efa;
   L9498efa: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498efd;
   L9498efd: // CALL 0x080ac124
    eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498f02;
   L9498f02: // MOV EDX,dword ptr [EBP + 0x4]
    edx=R(unchecked(ebp+4u));goto L9498f05;
   L9498f05: // ADD EDX,0x4
    a=edx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L9498f08;
   L9498f08: // MOV dword ptr [EBP + 0x4],EDX
    W(unchecked(ebp+4u),edx);goto L9498f0b;
   L9498f0b: // LEA EDI,[ESP + 0x38]
    edi=unchecked(esp+56u);goto L9498f0f;
   L9498f0f: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9498f13;
   L9498f13: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9498f16;
   L9498f16: // CALL 0x080ac124
    eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498f1b;
   L9498f1b: // MOV EBX,dword ptr [EBP + 0x4]
    ebx=R(unchecked(ebp+4u));goto L9498f1e;
   L9498f1e: // LEA ESI,[EBX + -0x8]
    esi=unchecked(ebx+4294967288u);goto L9498f21;
   L9498f21: // SUB ESI,dword ptr [ESP + 0x1c]
    a=esi;b=R(unchecked(esp+28u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9498f25;
   L9498f25: // SAR ESI,0x2
    a=esi;v=unchecked((uint)((int)a>>2));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;esi=v;goto L9498f28;
   L9498f28: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498f2a;
   L9498f2a: // JLE 0x09498f47
    if(zero||sign!=overflow)goto L9498f47;goto L9498f2c;
   L9498f2c: // SUB EBX,0x4
    a=ebx;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebx=v;goto L9498f2f;
   L9498f2f: // NOP
    goto L9498f30;
   L9498f30: // LEA EAX,[EBX + -0x8]
    eax=unchecked(ebx+4294967288u);goto L9498f33;
   L9498f33: // SUB EBX,0x4
    a=ebx;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebx=v;goto L9498f36;
   L9498f36: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498f3a;
   L9498f3a: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9498f3d;
   L9498f3d: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499102;}goto L9498f42;
   L9498f42: // SUB ESI,0x1
    a=esi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9498f45;
   L9498f45: // JNZ 0x09498f30
    if(!zero)goto L9498f30;goto L9498f47;
   L9498f47: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L9498f4b;
   L9498f4b: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L9498f4f;
   L9498f4f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498f52;
   L9498f52: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499102;}goto L9498f57;
   L9498f57: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9498f5b;
   L9498f5b: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9498f5e;
   L9498f5e: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498f64;
   L9498f64: // JNZ 0x094990d2
    if(!zero)goto L94990d2;goto L9498f6a;
   L9498f6a:return 0;
   L9498f72: // MOV ECX,dword ptr [EBP]
    ecx=R(unchecked(ebp));goto L9498f75;
   L9498f75: // SUB EAX,ECX
    a=eax;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9498f77;
   L9498f77: // SAR EAX,0x2
    a=eax;v=unchecked((uint)((int)a>>2));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;eax=v;goto L9498f7a;
   L9498f7a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498f7c;
   L9498f7c: // JZ 0x0949908b
    if(zero)goto L949908b;goto L9498f82;
   L9498f82: // MOV ESI,dword ptr [ESP + 0x1c]
    esi=R(unchecked(esp+28u));goto L9498f86;
   L9498f86: // LEA EDX,[EAX + EAX*0x1]
    edx=unchecked(eax+eax*0x1u);goto L9498f89;
   L9498f89: // SUB ESI,ECX
    a=esi;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9498f8b;
   L9498f8b: // SAR ESI,0x2
    a=esi;v=unchecked((uint)((int)a>>2));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;esi=v;goto L9498f8e;
   L9498f8e: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498f90;
   L9498f90: // JBE 0x094990a9
    if(carry||zero)goto L94990a9;goto L9498f96;
   L9498f96: // MOV EAX,0xfffffffc
    eax=4294967292u;goto L9498f9b;
   L9498f9b: // MOV dword ptr [ESP + 0x2c],0x3fffffff
    W(unchecked(esp+44u),1073741823u);goto L9498fa3;
   L9498fa3: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498fa6;
   L9498fa6: // CALL 0x080aad84
    eax=Call(0x80aad84,R(unchecked(esp+0u)));goto L9498fab;
   L9498fab: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L9498faf;
   L9498faf: // LEA ESI,[EAX + ESI*0x4]
    esi=unchecked(eax+esi*0x4u);goto L9498fb2;
   L9498fb2: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498fb4;
   L9498fb4: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9498fb8;
   L9498fb8: // JZ 0x09498fc6
    if(zero)goto L9498fc6;goto L9498fba;
   L9498fba: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9498fbe;
   L9498fbe: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9498fc1;
   L9498fc1: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94991d0;}goto L9498fc6;
   L9498fc6: // MOV ESI,dword ptr [EBP]
    esi=R(unchecked(ebp));goto L9498fc9;
   L9498fc9: // CMP dword ptr [ESP + 0x1c],ESI
    a=R(unchecked(esp+28u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498fcd;
   L9498fcd: // MOV EBX,dword ptr [ESP + 0x20]
    ebx=R(unchecked(esp+32u));goto L9498fd1;
   L9498fd1: // JZ 0x09498ffe
    if(zero)goto L9498ffe;goto L9498fd3;
   L9498fd3: // MOV dword ptr [ESP + 0x24],EDI
    W(unchecked(esp+36u),edi);goto L9498fd7;
   L9498fd7: // MOV EDI,dword ptr [ESP + 0x1c]
    edi=R(unchecked(esp+28u));goto L9498fdb;
   L9498fdb: // NOP
    goto L9498fdc;
   L9498fdc: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9498fe0;
   L9498fe0: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498fe2;
   L9498fe2: // JZ 0x09498ff0
    if(zero)goto L9498ff0;goto L9498fe4;
   L9498fe4: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L9498fe8;
   L9498fe8: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9498feb;
   L9498feb: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94991aa;}goto L9498ff0;
   L9498ff0: // ADD ESI,0x4
    a=esi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9498ff3;
   L9498ff3: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9498ff6;
   L9498ff6: // CMP EDI,ESI
    a=edi;b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498ff8;
   L9498ff8: // JNZ 0x09498fe0
    if(!zero)goto L9498fe0;goto L9498ffa;
   L9498ffa: // MOV EDI,dword ptr [ESP + 0x24]
    edi=R(unchecked(esp+36u));goto L9498ffe;
   L9498ffe: // MOV ESI,dword ptr [EBP + 0x4]
    esi=R(unchecked(ebp+4u));goto L9499001;
   L9499001: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499004;
   L9499004: // CMP dword ptr [ESP + 0x1c],ESI
    a=R(unchecked(esp+28u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499008;
   L9499008: // MOV dword ptr [ESP + 0x24],EBX
    W(unchecked(esp+36u),ebx);goto L949900c;
   L949900c: // JZ 0x0949902d
    if(zero)goto L949902d;goto L949900e;
   L949900e: // NOP
    goto L9499010;
   L9499010: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499012;
   L9499012: // JZ 0x09499020
    if(zero)goto L9499020;goto L9499014;
   L9499014: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L9499018;
   L9499018: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949901b;
   L949901b: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499165;}goto L9499020;
   L9499020: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L9499023;
   L9499023: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499026;
   L9499026: // CMP ESI,EDI
    a=esi;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499028;
   L9499028: // JNZ 0x09499010
    if(!zero)goto L9499010;goto L949902a;
   L949902a: // MOV ESI,dword ptr [EBP + 0x4]
    esi=R(unchecked(ebp+4u));goto L949902d;
   L949902d: // MOV EDI,dword ptr [EBP]
    edi=R(unchecked(ebp));goto L9499030;
   L9499030: // CMP ESI,EDI
    a=esi;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499032;
   L9499032: // JZ 0x09499063
    if(zero)goto L9499063;goto L9499034;
   L9499034: // MOV EAX,0x80b0f04
    eax=134942468u;goto L9499039;
   L9499039: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949903b;
   L949903b: // JZ 0x09499146
    if(zero)goto L9499146;goto L9499041;
   L9499041: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9499048;
   L9499048: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L949904a;
   L949904a: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L949904d;
   L949904d: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499053;
   L9499053: // JNZ 0x09499114
    if(!zero)goto L9499114;goto L9499059;
   L9499059: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L949905c;
   L949905c: // CMP ESI,EDI
    a=esi;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949905e;
   L949905e: // JNZ 0x09499048
    if(!zero)goto L9499048;goto L9499060;
   L9499060: // MOV EDI,dword ptr [EBP]
    edi=R(unchecked(ebp));goto L9499063;
   L9499063: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499065;
   L9499065: // JZ 0x0949906f
    if(zero)goto L949906f;goto L9499067;
   L9499067: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L949906a;
   L949906a: // CALL 0x080aabe4
    eax=Call(0x80aabe4,R(unchecked(esp+0u)));goto L949906f;
   L949906f: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L9499073;
   L9499073: // MOV ESI,dword ptr [ESP + 0x2c]
    esi=R(unchecked(esp+44u));goto L9499077;
   L9499077: // MOV dword ptr [EBP + 0x4],EBX
    W(unchecked(ebp+4u),ebx);goto L949907a;
   L949907a: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto L949907d;
   L949907d: // LEA EAX,[EAX + ESI*0x4]
    eax=unchecked(eax+esi*0x4u);goto L9499080;
   L9499080: // MOV dword ptr [EBP + 0x8],EAX
    W(unchecked(ebp+8u),eax);goto L9499083;
   L9499083:return 0;
   L949908b: // MOV ESI,dword ptr [ESP + 0x1c]
    esi=R(unchecked(esp+28u));goto L949908f;
   L949908f: // MOV EDX,0x1
    edx=1u;goto L9499094;
   L9499094: // SUB ESI,ECX
    a=esi;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9499096;
   L9499096: // SAR ESI,0x2
    a=esi;v=unchecked((uint)((int)a>>2));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;esi=v;goto L9499099;
   L9499099: // LEA EAX,[EDX*0x4 + 0x0]
    eax=unchecked(edx*0x4u+0u);goto L94990a0;
   L94990a0: // MOV dword ptr [ESP + 0x2c],EDX
    W(unchecked(esp+44u),edx);goto L94990a4;
   L94990a4: // JMP 0x09498fa3
    goto L9498fa3;
   L94990a9: // CMP EDX,0x3fffffff
    a=edx;b=1073741823u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94990af;
   L94990af: // JA 0x09498f96
    if(!carry&&!zero)goto L9498f96;goto L94990b5;
   L94990b5: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94990b7;
   L94990b7: // JNZ 0x09499099
    if(!zero)goto L9499099;goto L94990b9;
   L94990b9: // MOV dword ptr [ESP + 0x20],0x0
    W(unchecked(esp+32u),0u);goto L94990c1;
   L94990c1: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L94990c5;
   L94990c5: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L94990cd;
   L94990cd: // JMP 0x09498faf
    goto L9498faf;
   L94990d2: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94990d7;
   L94990d7: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94990d9;
   L94990d9: // JZ 0x0949913b
    if(zero)goto L949913b;goto L94990db;
   L94990db: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L94990e0;
   L94990e0: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94990e5;
   L94990e5: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94990e7;
   L94990e7: // JG 0x09498f6a
    if(!zero&&sign==overflow)goto L9498f6a;goto L94990ed;
   L94990ed: // LEA EAX,[ESP + 0x3e]
    eax=unchecked(esp+62u);goto L94990f1;
   L94990f1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94990f5;
   L94990f5: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94990f8;
   L94990f8: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94990fd;
   L94990fd: // JMP 0x09498f6a
    goto L9498f6a;
   L9499102: // MOV EBX,EAX
    ebx=eax;goto L9499104;
   L9499104: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9499107;
   L9499107: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949910c;
   L949910c: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949910f;
   L949910f: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L9499114: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L9499119;
   L9499119: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949911e;
   L949911e: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499120;
   L9499120: // JG 0x09499059
    if(!zero&&sign==overflow)goto L9499059;goto L9499126;
   L9499126: // LEA EAX,[ESP + 0x3f]
    eax=unchecked(esp+63u);goto L949912a;
   L949912a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949912e;
   L949912e: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9499131;
   L9499131: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9499136;
   L9499136: // JMP 0x09499059
    goto L9499059;
   L949913b: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949913e;
   L949913e: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L9499141;
   L9499141: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L9499144;
   L9499144: // JMP 0x094990e5
    goto L94990e5;
   L9499146: // MOV dword ptr [ESP + 0x1c],ESI
    W(unchecked(esp+28u),esi);goto L949914a;
   L949914a: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L949914c;
   L949914c: // LEA ECX,[EAX + -0xc]
    ecx=unchecked(eax+4294967284u);goto L949914f;
   L949914f: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499155;
   L9499155: // JNZ 0x0949918b
    if(!zero)goto L949918b;goto L9499157;
   L9499157: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L949915a;
   L949915a: // CMP dword ptr [ESP + 0x1c],EDI
    a=R(unchecked(esp+28u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949915e;
   L949915e: // JNZ 0x0949914a
    if(!zero)goto L949914a;goto L9499160;
   L9499160: // JMP 0x09499060
    goto L9499060;
   L9499165: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499168;
   L9499168: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949916d;
   L949916d: // CMP dword ptr [ESP + 0x24],EBX
    a=R(unchecked(esp+36u));b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499171;
   L9499171: // JZ 0x09499186
    if(zero)goto L9499186;goto L9499173;
   L9499173: // MOV ESI,dword ptr [ESP + 0x24]
    esi=R(unchecked(esp+36u));goto L9499177;
   L9499177: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949917a;
   L949917a: // ADD ESI,0x4
    a=esi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L949917d;
   L949917d: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L9499182;
   L9499182: // CMP ESI,EBX
    a=esi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499184;
   L9499184: // JNZ 0x09499177
    if(!zero)goto L9499177;goto L9499186;
   L9499186: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499211;}
   L949918b: // MOV EDX,dword ptr [EAX + -0x4]
    edx=R(unchecked(eax+4294967292u));goto L949918e;
   L949918e: // LEA ESI,[EDX + -0x1]
    esi=unchecked(edx+4294967295u);goto L9499191;
   L9499191: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499193;
   L9499193: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L9499196;
   L9499196: // JG 0x09499157
    if(!zero&&sign==overflow)goto L9499157;goto L9499198;
   L9499198: // LEA EDX,[ESP + 0x3f]
    edx=unchecked(esp+63u);goto L949919c;
   L949919c: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L94991a0;
   L94991a0: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94991a3;
   L94991a3: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94991a8;
   L94991a8: // JMP 0x09499157
    goto L9499157;
   L94991aa: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94991ad;
   L94991ad: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L94991b2;
   L94991b2: // CMP dword ptr [ESP + 0x20],EBX
    a=R(unchecked(esp+32u));b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94991b6;
   L94991b6: // JZ 0x094991cb
    if(zero)goto L94991cb;goto L94991b8;
   L94991b8: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L94991bc;
   L94991bc: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94991bf;
   L94991bf: // ADD ESI,0x4
    a=esi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L94991c2;
   L94991c2: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94991c7;
   L94991c7: // CMP EBX,ESI
    a=ebx;b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94991c9;
   L94991c9: // JNZ 0x094991bc
    if(!zero)goto L94991bc;goto L94991cb;
   L94991cb: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949921c;}
   L94991d0: // MOV ECX,dword ptr [ESP + 0x20]
    ecx=R(unchecked(esp+32u));goto L94991d4;
   L94991d4: // MOV dword ptr [ESP + 0x24],ECX
    W(unchecked(esp+36u),ecx);goto L94991d8;
   L94991d8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94991db;
   L94991db: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L94991e0;
   L94991e0: // MOV ECX,dword ptr [ESP + 0x24]
    ecx=R(unchecked(esp+36u));goto L94991e4;
   L94991e4: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94991e6;
   L94991e6: // JZ 0x0949922b
    if(zero)goto L949922b;goto L94991e8;
   L94991e8: // MOV EBX,dword ptr [ESP + 0x20]
    ebx=R(unchecked(esp+32u));goto L94991ec;
   L94991ec: // CMP dword ptr [ESP + 0x24],EBX
    a=R(unchecked(esp+36u));b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94991f0;
   L94991f0: // JZ 0x09499203
    if(zero)goto L9499203;goto L94991f2;
   L94991f2: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94991f5;
   L94991f5: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L94991f8;
   L94991f8: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94991fd;
   L94991fd: // CMP dword ptr [ESP + 0x24],EBX
    a=R(unchecked(esp+36u));b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499201;
   L9499201: // JNZ 0x094991f2
    if(!zero)goto L94991f2;goto L9499203;
   L9499203: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L9499207;
   L9499207: // CALL 0x094987d0
    eax=Call(0x94987d0,eax);goto L949920c;
   L949920c: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499239;}
   L9499211: // MOV EBX,EAX
    ebx=eax;goto L9499213;
   L9499213: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L9499218;
   L9499218: // MOV EAX,EBX
    eax=ebx;goto L949921a;
   L949921a: // JMP 0x094991d8
    goto L94991d8;
   L949921c: // MOV EBX,EAX
    ebx=eax;goto L949921e;
   L949921e: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L9499223;
   L9499223: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499226;
   L9499226: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949922b;
   L949922b: // MOV ESI,dword ptr [ESP + 0x28]
    esi=R(unchecked(esp+40u));goto L949922f;
   L949922f: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9499232;
   L9499232: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L9499237;
   L9499237: // JMP 0x09499203
    goto L9499203;
   L9499239: // MOV EBX,EAX
    ebx=eax;goto L949923b;
   L949923b: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L9499240;
   L9499240: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499243;
   L9499243: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  // vector<string>::D1 081a1a30, preserving its captured end and begin reload.
  public void Destroy(uint vector)
  {
   uint end=R(unchecked(vector+4u)),begin=R(vector),current=begin;
   while(current!=end){Call(0x80add34,current);current=unchecked(current+4u);}
   if(begin!=end)begin=R(vector);
   if(begin!=0)Call(0x80aabe4,begin);
  }
 }
}
