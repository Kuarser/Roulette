using System;
namespace RouletteDeluxe
{
 // Physical SimpleConfigOption incorporation 094b5d40. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after allocation; option/name/value are at +130/+134/+138.
 public sealed class OriginalConfigurationSimpleOptionIncorporate
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
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationSimpleOptionIncorporate(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Incorporate(uint frame,uint incomingEbx,uint incomingEsi,uint incomingEdi,uint incomingEbp)
  {
   uint eax=0,ebx=incomingEbx,esi=incomingEsi,edi=incomingEdi,ecx=0,edx=0,ebp=incomingEbp,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L94b5d46;
   L94b5d46: // CMP byte ptr [0x0a13cfe0],0x0
    a=B(unchecked(169070560u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5d4d;
   L94b5d4d: // MOV dword ptr [ESP + 0x11c],EBX
    W(unchecked(esp+284u),ebx);goto L94b5d54;
   L94b5d54: // MOV EBX,dword ptr [ESP + 0x130]
    ebx=R(unchecked(esp+304u));goto L94b5d5b;
   L94b5d5b: // MOV dword ptr [ESP + 0x120],ESI
    W(unchecked(esp+288u),esi);goto L94b5d62;
   L94b5d62: // MOV ESI,dword ptr [ESP + 0x138]
    esi=R(unchecked(esp+312u));goto L94b5d69;
   L94b5d69: // MOV dword ptr [ESP + 0x124],EDI
    W(unchecked(esp+292u),edi);goto L94b5d70;
   L94b5d70: // MOV dword ptr [ESP + 0x128],EBP
    W(unchecked(esp+296u),ebp);goto L94b5d77;
   L94b5d77: // JZ 0x094b5eb0
    if(zero)goto L94b5eb0;goto L94b5d7d;
   L94b5d7d: // CMP byte ptr [0x0a13cff0],0x0
    a=B(unchecked(169070576u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5d84;
   L94b5d84: // JZ 0x094b5e50
    if(zero)goto L94b5e50;goto L94b5d8a;
   L94b5d8a: // CMP byte ptr [0x0a13d000],0x0
    a=B(unchecked(169070592u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5d91;
   L94b5d91: // JZ 0x094b5df0
    if(zero)goto L94b5df0;goto L94b5d93;
   L94b5d93: // MOV EAX,dword ptr [ESP + 0x134]
    eax=R(unchecked(esp+308u));goto L94b5d9a;
   L94b5d9a: // MOV EBP,dword ptr [EAX]
    ebp=R(unchecked(eax));goto L94b5d9c;
   L94b5d9c: // MOV EAX,[0x0a13cfe8]
    eax=R(unchecked(169070568u));goto L94b5da1;
   L94b5da1: // MOV EDI,dword ptr [EBP + -0xc]
    edi=R(unchecked(ebp+4294967284u));goto L94b5da4;
   L94b5da4: // CMP EDI,dword ptr [EAX + -0xc]
    a=edi;b=R(unchecked(eax+4294967284u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5da7;
   L94b5da7: // JZ 0x094b5f10
    if(zero)goto L94b5f10;goto L94b5dad;
   L94b5dad: // MOV EAX,[0x0a13cff8]
    eax=R(unchecked(169070584u));goto L94b5db2;
   L94b5db2: // CMP EDI,dword ptr [EAX + -0xc]
    a=edi;b=R(unchecked(eax+4294967284u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5db5;
   L94b5db5: // JZ 0x094b5fa8
    if(zero)goto L94b5fa8;goto L94b5dbb;
   L94b5dbb: // MOV EAX,[0x0a13d008]
    eax=R(unchecked(169070600u));goto L94b5dc0;
   L94b5dc0: // CMP EDI,dword ptr [EAX + -0xc]
    a=edi;b=R(unchecked(eax+4294967284u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5dc3;
   L94b5dc3: // JZ 0x094b62e0
    if(zero)goto L94b62e0;goto L94b5dc9;
   L94b5dc9: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L94b5dcb;
   L94b5dcb:return eax&255u;
   L94b5df0: // MOV dword ptr [ESP],0xa13d000
    W(unchecked(esp),169070592u);goto L94b5df7;
   L94b5df7: // CALL 0x080ac8f4
    eax=Call(0x80ac8f4,R(unchecked(esp+0u)));goto L94b5dfc;
   L94b5dfc: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5dfe;
   L94b5dfe: // JZ 0x094b5d93
    if(zero)goto L94b5d93;goto L94b5e00;
   L94b5e00: // LEA EAX,[ESP + 0x105]
    eax=unchecked(esp+261u);goto L94b5e07;
   L94b5e07: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L94b5e0b;
   L94b5e0b: // MOV dword ptr [ESP + 0x4],0x96e483e
    W(unchecked(esp+4u),158222398u);goto L94b5e13;
   L94b5e13: // MOV dword ptr [ESP],0xa13d008
    W(unchecked(esp),169070600u);goto L94b5e1a;
   L94b5e1a: // CALL 0x080afcf4
    try{eax=Call(0x80afcf4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b65aa;}goto L94b5e1f;
   L94b5e1f: // MOV dword ptr [ESP],0xa13d000
    W(unchecked(esp),169070592u);goto L94b5e26;
   L94b5e26: // CALL 0x080b0be4
    eax=Call(0x80b0be4,R(unchecked(esp+0u)));goto L94b5e2b;
   L94b5e2b: // MOV dword ptr [ESP + 0x8],0x9b14f64
    W(unchecked(esp+8u),162615140u);goto L94b5e33;
   L94b5e33: // MOV dword ptr [ESP + 0x4],0xa13d008
    W(unchecked(esp+4u),169070600u);goto L94b5e3b;
   L94b5e3b: // MOV dword ptr [ESP],0x80add34
    W(unchecked(esp),134929716u);goto L94b5e42;
   L94b5e42: // CALL 0x080ab434
    eax=Call(0x80ab434,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b5e47;
   L94b5e47: // JMP 0x094b5d93
    goto L94b5d93;
   L94b5e50: // MOV dword ptr [ESP],0xa13cff0
    W(unchecked(esp),169070576u);goto L94b5e57;
   L94b5e57: // CALL 0x080ac8f4
    eax=Call(0x80ac8f4,R(unchecked(esp+0u)));goto L94b5e5c;
   L94b5e5c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5e5e;
   L94b5e5e: // JZ 0x094b5d8a
    if(zero)goto L94b5d8a;goto L94b5e64;
   L94b5e64: // LEA EAX,[ESP + 0x104]
    eax=unchecked(esp+260u);goto L94b5e6b;
   L94b5e6b: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L94b5e6f;
   L94b5e6f: // MOV dword ptr [ESP + 0x4],0x9707d84
    W(unchecked(esp+4u),158367108u);goto L94b5e77;
   L94b5e77: // MOV dword ptr [ESP],0xa13cff8
    W(unchecked(esp),169070584u);goto L94b5e7e;
   L94b5e7e: // CALL 0x080afcf4
    try{eax=Call(0x80afcf4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b65ce;}goto L94b5e83;
   L94b5e83: // MOV dword ptr [ESP],0xa13cff0
    W(unchecked(esp),169070576u);goto L94b5e8a;
   L94b5e8a: // CALL 0x080b0be4
    eax=Call(0x80b0be4,R(unchecked(esp+0u)));goto L94b5e8f;
   L94b5e8f: // MOV dword ptr [ESP + 0x8],0x9b14f64
    W(unchecked(esp+8u),162615140u);goto L94b5e97;
   L94b5e97: // MOV dword ptr [ESP + 0x4],0xa13cff8
    W(unchecked(esp+4u),169070584u);goto L94b5e9f;
   L94b5e9f: // MOV dword ptr [ESP],0x80add34
    W(unchecked(esp),134929716u);goto L94b5ea6;
   L94b5ea6: // CALL 0x080ab434
    eax=Call(0x80ab434,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b5eab;
   L94b5eab: // JMP 0x094b5d8a
    goto L94b5d8a;
   L94b5eb0: // MOV dword ptr [ESP],0xa13cfe0
    W(unchecked(esp),169070560u);goto L94b5eb7;
   L94b5eb7: // CALL 0x080ac8f4
    eax=Call(0x80ac8f4,R(unchecked(esp+0u)));goto L94b5ebc;
   L94b5ebc: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5ebe;
   L94b5ebe: // JZ 0x094b5d7d
    if(zero)goto L94b5d7d;goto L94b5ec4;
   L94b5ec4: // LEA EAX,[ESP + 0x103]
    eax=unchecked(esp+259u);goto L94b5ecb;
   L94b5ecb: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L94b5ecf;
   L94b5ecf: // MOV dword ptr [ESP + 0x4],0x9732e14
    W(unchecked(esp+4u),158543380u);goto L94b5ed7;
   L94b5ed7: // MOV dword ptr [ESP],0xa13cfe8
    W(unchecked(esp),169070568u);goto L94b5ede;
   L94b5ede: // CALL 0x080afcf4
    try{eax=Call(0x80afcf4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b65e4;}goto L94b5ee3;
   L94b5ee3: // MOV dword ptr [ESP],0xa13cfe0
    W(unchecked(esp),169070560u);goto L94b5eea;
   L94b5eea: // CALL 0x080b0be4
    eax=Call(0x80b0be4,R(unchecked(esp+0u)));goto L94b5eef;
   L94b5eef: // MOV dword ptr [ESP + 0x8],0x9b14f64
    W(unchecked(esp+8u),162615140u);goto L94b5ef7;
   L94b5ef7: // MOV dword ptr [ESP + 0x4],0xa13cfe8
    W(unchecked(esp+4u),169070568u);goto L94b5eff;
   L94b5eff: // MOV dword ptr [ESP],0x80add34
    W(unchecked(esp),134929716u);goto L94b5f06;
   L94b5f06: // CALL 0x080ab434
    eax=Call(0x80ab434,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b5f0b;
   L94b5f0b: // JMP 0x094b5d7d
    goto L94b5d7d;
   L94b5f10: // MOV dword ptr [ESP + 0x8],EDI
    W(unchecked(esp+8u),edi);goto L94b5f14;
   L94b5f14: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L94b5f18;
   L94b5f18: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b5f1b;
   L94b5f1b: // CALL 0x080aafa4
    eax=Call(0x80aafa4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b5f20;
   L94b5f20: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5f22;
   L94b5f22: // JNZ 0x094b5dad
    if(!zero)goto L94b5dad;goto L94b5f28;
   L94b5f28: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b5f2c;
   L94b5f2c: // LEA ESI,[ESP + 0xe4]
    esi=unchecked(esp+228u);goto L94b5f33;
   L94b5f33: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b5f36;
   L94b5f36: // CALL 0x080ac124
    eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b5f3b;
   L94b5f3b: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b5f3f;
   L94b5f3f: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b5f42;
   L94b5f42: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6598;}goto L94b5f47;
   L94b5f47: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b5f4a;
   L94b5f4a: // CALL 0x094b4900
    eax=Call(0x94b4900,R(unchecked(esp+0u)));goto L94b5f4f;
   L94b5f4f: // MOV EAX,dword ptr [ESP + 0xe4]
    eax=R(unchecked(esp+228u));goto L94b5f56;
   L94b5f56: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b5f59;
   L94b5f59: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b5f5f;
   L94b5f5f: // JZ 0x094b633c
    if(zero)goto L94b633c;goto L94b5f65;
   L94b5f65: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b5f6a;
   L94b5f6a: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5f6c;
   L94b5f6c: // JZ 0x094b658a
    if(zero)goto L94b658a;goto L94b5f72;
   L94b5f72: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b5f75;
   L94b5f75: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b5f7a;
   L94b5f7a: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5f7c;
   L94b5f7c: // MOV EAX,0x1
    eax=1u;goto L94b5f81;
   L94b5f81: // JG 0x094b5dcb
    if(!zero&&sign==overflow)goto L94b5dcb;goto L94b5f87;
   L94b5f87: // LEA ECX,[ESP + 0x106]
    ecx=unchecked(esp+262u);goto L94b5f8e;
   L94b5f8e: // MOV byte ptr [ESP + 0x1c],AL
    h.WriteByte(unchecked(esp+28u),unchecked((byte)((eax&255u))));goto L94b5f92;
   L94b5f92: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L94b5f96;
   L94b5f96: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b5f99;
   L94b5f99: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b5f9e;
   L94b5f9e: // MOVZX EAX,byte ptr [ESP + 0x1c]
    eax=B(unchecked(esp+28u));goto L94b5fa3;
   L94b5fa3: // JMP 0x094b5dcb
    goto L94b5dcb;
   L94b5fa8: // MOV dword ptr [ESP + 0x8],EDI
    W(unchecked(esp+8u),edi);goto L94b5fac;
   L94b5fac: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L94b5fb0;
   L94b5fb0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b5fb3;
   L94b5fb3: // CALL 0x080aafa4
    eax=Call(0x80aafa4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b5fb8;
   L94b5fb8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b5fba;
   L94b5fba: // JNZ 0x094b5dbb
    if(!zero)goto L94b5dbb;goto L94b5fc0;
   L94b5fc0: // LEA EAX,[ESP + 0xe8]
    eax=unchecked(esp+232u);goto L94b5fc7;
   L94b5fc7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b5fca;
   L94b5fca: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b5fce;
   L94b5fce: // CALL 0x080ac124
    eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b5fd3;
   L94b5fd3: // LEA EDX,[ESP + 0x5c]
    edx=unchecked(esp+92u);goto L94b5fd7;
   L94b5fd7: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b5fda;
   L94b5fda: // CALL 0x080ad8c4
    eax=Call(0x80ad8c4,R(unchecked(esp+0u)));goto L94b5fdf;
   L94b5fdf: // MOV EDI,dword ptr [0x09b49de8]
    edi=R(unchecked(162831848u));goto L94b5fe5;
   L94b5fe5: // MOV byte ptr [ESP + 0xd0],0x0
    h.WriteByte(unchecked(esp+208u),unchecked((byte)(0u)));goto L94b5fed;
   L94b5fed: // MOV EBP,dword ptr [0x09b49dec]
    ebp=R(unchecked(162831852u));goto L94b5ff3;
   L94b5ff3: // MOV dword ptr [ESP + 0x5c],0x9b49c10
    W(unchecked(esp+92u),162831376u);goto L94b5ffb;
   L94b5ffb: // MOV dword ptr [ESP + 0xcc],0x0
    W(unchecked(esp+204u),0u);goto L94b6006;
   L94b6006: // MOV EAX,dword ptr [EDI + -0xc]
    eax=R(unchecked(edi+4294967284u));goto L94b6009;
   L94b6009: // MOV dword ptr [ESP + 0x28],EDI
    W(unchecked(esp+40u),edi);goto L94b600d;
   L94b600d: // MOV byte ptr [ESP + 0xd1],0x0
    h.WriteByte(unchecked(esp+209u),unchecked((byte)(0u)));goto L94b6015;
   L94b6015: // MOV dword ptr [ESP + 0xd4],0x0
    W(unchecked(esp+212u),0u);goto L94b6020;
   L94b6020: // MOV dword ptr [ESP + EAX*0x1 + 0x28],EBP
    W(unchecked(esp+eax*0x1u+40u),ebp);goto L94b6024;
   L94b6024: // LEA EAX,[ESP + 0x28]
    eax=unchecked(esp+40u);goto L94b6028;
   L94b6028: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L94b6030;
   L94b6030: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L94b6038;
   L94b6038: // ADD EAX,dword ptr [EDI + -0xc]
    a=eax;b=R(unchecked(edi+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L94b603b;
   L94b603b: // MOV dword ptr [ESP + 0xd8],0x0
    W(unchecked(esp+216u),0u);goto L94b6046;
   L94b6046: // MOV dword ptr [ESP + 0xdc],0x0
    W(unchecked(esp+220u),0u);goto L94b6051;
   L94b6051: // MOV dword ptr [ESP + 0xe0],0x0
    W(unchecked(esp+224u),0u);goto L94b605c;
   L94b605c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b605f;
   L94b605f: // CALL 0x080b06f4
    try{eax=Call(0x80b06f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6586;}goto L94b6064;
   L94b6064: // MOV EAX,[0x09b49df0]
    eax=R(unchecked(162831856u));goto L94b6069;
   L94b6069: // LEA ESI,[ESP + 0x30]
    esi=unchecked(esp+48u);goto L94b606d;
   L94b606d: // MOV EDX,dword ptr [0x09b49df4]
    edx=R(unchecked(162831860u));goto L94b6073;
   L94b6073: // MOV dword ptr [ESP + 0x30],EAX
    W(unchecked(esp+48u),eax);goto L94b6077;
   L94b6077: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L94b607a;
   L94b607a: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L94b607c;
   L94b607c: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L94b607e;
   L94b607e: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L94b6086;
   L94b6086: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6089;
   L94b6089: // CALL 0x080b06f4
    try{eax=Call(0x80b06f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b656f;}goto L94b608e;
   L94b608e: // MOV EBP,dword ptr [0x09b49de4]
    ebp=R(unchecked(162831844u));goto L94b6094;
   L94b6094: // LEA EDI,[ESP + 0x50]
    edi=unchecked(esp+80u);goto L94b6098;
   L94b6098: // MOV EDX,dword ptr [0x09b49df8]
    edx=R(unchecked(162831864u));goto L94b609e;
   L94b609e: // MOV EAX,dword ptr [EBP + -0xc]
    eax=R(unchecked(ebp+4294967284u));goto L94b60a1;
   L94b60a1: // MOV dword ptr [ESP + EAX*0x1 + 0x28],EDX
    W(unchecked(esp+eax*0x1u+40u),edx);goto L94b60a5;
   L94b60a5: // MOV dword ptr [ESP + 0x28],0x9b494cc
    W(unchecked(esp+40u),162829516u);goto L94b60ad;
   L94b60ad: // MOV dword ptr [ESP + 0x5c],0x9b494f4
    W(unchecked(esp+92u),162829556u);goto L94b60b5;
   L94b60b5: // MOV dword ptr [ESP + 0x30],0x9b494e0
    W(unchecked(esp+48u),162829536u);goto L94b60bd;
   L94b60bd: // MOV dword ptr [ESP + 0x34],0x9b48a28
    W(unchecked(esp+52u),162826792u);goto L94b60c5;
   L94b60c5: // MOV dword ptr [ESP + 0x38],0x0
    W(unchecked(esp+56u),0u);goto L94b60cd;
   L94b60cd: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L94b60d5;
   L94b60d5: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L94b60dd;
   L94b60dd: // MOV dword ptr [ESP + 0x44],0x0
    W(unchecked(esp+68u),0u);goto L94b60e5;
   L94b60e5: // MOV dword ptr [ESP + 0x48],0x0
    W(unchecked(esp+72u),0u);goto L94b60ed;
   L94b60ed: // MOV dword ptr [ESP + 0x4c],0x0
    W(unchecked(esp+76u),0u);goto L94b60f5;
   L94b60f5: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b60f8;
   L94b60f8: // CALL 0x080b11f4
    eax=Call(0x80b11f4,R(unchecked(esp+0u)));goto L94b60fd;
   L94b60fd: // LEA EAX,[ESP + 0x34]
    eax=unchecked(esp+52u);goto L94b6101;
   L94b6101: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6105;
   L94b6105: // LEA EAX,[ESP + 0x5c]
    eax=unchecked(esp+92u);goto L94b6109;
   L94b6109: // MOV dword ptr [ESP + 0x34],0x9b497c8
    W(unchecked(esp+52u),162830280u);goto L94b6111;
   L94b6111: // MOV dword ptr [ESP + 0x54],0x18
    W(unchecked(esp+84u),24u);goto L94b6119;
   L94b6119: // MOV dword ptr [ESP + 0x58],0x9b482ac
    W(unchecked(esp+88u),162824876u);goto L94b6121;
   L94b6121: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6124;
   L94b6124: // CALL 0x080b06f4
    try{eax=Call(0x80b06f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b651f;}goto L94b6129;
   L94b6129: // MOV EAX,dword ptr [ESP + 0xe8]
    eax=R(unchecked(esp+232u));goto L94b6130;
   L94b6130: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L94b6133;
   L94b6133: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6137;
   L94b6137: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b613a;
   L94b613a: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L94b613e;
   L94b613e: // CALL 0x080ad354
    try{eax=Call(0x80ad354,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b651b;}goto L94b6143;
   L94b6143: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L94b6147;
   L94b6147: // MOV dword ptr [ESP + 0xf4],0x9b482ac
    W(unchecked(esp+244u),162824876u);goto L94b6152;
   L94b6152: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6154;
   L94b6154: // JZ 0x094b63d0
    if(zero)goto L94b63d0;goto L94b615a;
   L94b615a: // MOV ECX,dword ptr [ESP + 0x40]
    ecx=R(unchecked(esp+64u));goto L94b615e;
   L94b615e: // CMP EAX,ECX
    a=eax;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b6160;
   L94b6160: // JBE 0x094b6348
    if(carry||zero)goto L94b6348;goto L94b6166;
   L94b6166: // MOV ESI,dword ptr [ESP + 0x44]
    esi=R(unchecked(esp+68u));goto L94b616a;
   L94b616a: // LEA EDX,[ESP + 0x108]
    edx=unchecked(esp+264u);goto L94b6171;
   L94b6171: // MOV ECX,EAX
    ecx=eax;goto L94b6173;
   L94b6173: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b6176;
   L94b6176: // LEA EAX,[ESP + 0xfc]
    eax=unchecked(esp+252u);goto L94b617d;
   L94b617d: // MOV EDX,ESI
    edx=esi;goto L94b617f;
   L94b617f: // CALL 0x094b44a0
    try{eax=Call(0x94b44a0,eax,edx,ecx,R(esp));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6669;}goto L94b6184;
   L94b6184: // LEA EDX,[ESP + 0xfc]
    edx=unchecked(esp+252u);goto L94b618b;
   L94b618b: // LEA ESI,[ESP + 0xf4]
    esi=unchecked(esp+244u);goto L94b6192;
   L94b6192: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L94b6196;
   L94b6196: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b6199;
   L94b6199: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6651;}goto L94b619e;
   L94b619e: // MOV EAX,dword ptr [ESP + 0xfc]
    eax=R(unchecked(esp+252u));goto L94b61a5;
   L94b61a5: // LEA ECX,[EAX + -0xc]
    ecx=unchecked(eax+4294967284u);goto L94b61a8;
   L94b61a8: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b61ae;
   L94b61ae: // JNZ 0x094b6615
    if(!zero)goto L94b6615;goto L94b61b4;
   L94b61b4: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L94b61b7;
   L94b61b7: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b61bb;
   L94b61bb: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b61be;
   L94b61be: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6605;}goto L94b61c3;
   L94b61c3: // MOV EAX,dword ptr [ESP + 0xf4]
    eax=R(unchecked(esp+244u));goto L94b61ca;
   L94b61ca: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b61cd;
   L94b61cd: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b61d3;
   L94b61d3: // JNZ 0x094b6484
    if(!zero)goto L94b6484;goto L94b61d9;
   L94b61d9: // LEA EAX,[ESP + 0xec]
    eax=unchecked(esp+236u);goto L94b61e0;
   L94b61e0: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L94b61e4;
   L94b61e4: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b61e7;
   L94b61e7: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b651b;}goto L94b61ec;
   L94b61ec: // MOV EAX,dword ptr [ESP + 0x58]
    eax=R(unchecked(esp+88u));goto L94b61f0;
   L94b61f0: // MOV dword ptr [ESP + 0x28],0x9b494cc
    W(unchecked(esp+40u),162829516u);goto L94b61f8;
   L94b61f8: // MOV dword ptr [ESP + 0x5c],0x9b494f4
    W(unchecked(esp+92u),162829556u);goto L94b6200;
   L94b6200: // MOV dword ptr [ESP + 0x30],0x9b494e0
    W(unchecked(esp+48u),162829536u);goto L94b6208;
   L94b6208: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b620b;
   L94b620b: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b6211;
   L94b6211: // MOV dword ptr [ESP + 0x34],0x9b497c8
    W(unchecked(esp+52u),162830280u);goto L94b6219;
   L94b6219: // JNZ 0x094b644f
    if(!zero)goto L94b644f;goto L94b621f;
   L94b621f: // MOV dword ptr [ESP + 0x34],0x9b48a28
    W(unchecked(esp+52u),162826792u);goto L94b6227;
   L94b6227: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b622a;
   L94b622a: // CALL 0x080ad934
    eax=Call(0x80ad934,R(unchecked(esp+0u)));goto L94b622f;
   L94b622f: // MOV EAX,dword ptr [EBP + -0xc]
    eax=R(unchecked(ebp+4294967284u));goto L94b6232;
   L94b6232: // MOV EDX,dword ptr [0x09b49df8]
    edx=R(unchecked(162831864u));goto L94b6238;
   L94b6238: // MOV dword ptr [ESP + EAX*0x1 + 0x28],EDX
    W(unchecked(esp+eax*0x1u+40u),edx);goto L94b623c;
   L94b623c: // MOV EAX,[0x09b49df0]
    eax=R(unchecked(162831856u));goto L94b6241;
   L94b6241: // MOV EDX,dword ptr [0x09b49df4]
    edx=R(unchecked(162831860u));goto L94b6247;
   L94b6247: // MOV dword ptr [ESP + 0x30],EAX
    W(unchecked(esp+48u),eax);goto L94b624b;
   L94b624b: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L94b624e;
   L94b624e: // MOV dword ptr [ESP + EAX*0x1 + 0x30],EDX
    W(unchecked(esp+eax*0x1u+48u),edx);goto L94b6252;
   L94b6252: // MOV EAX,[0x09b49de8]
    eax=R(unchecked(162831848u));goto L94b6257;
   L94b6257: // MOV EDX,dword ptr [0x09b49dec]
    edx=R(unchecked(162831852u));goto L94b625d;
   L94b625d: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L94b6261;
   L94b6261: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L94b6264;
   L94b6264: // MOV dword ptr [ESP + EAX*0x1 + 0x28],EDX
    W(unchecked(esp+eax*0x1u+40u),edx);goto L94b6268;
   L94b6268: // LEA EAX,[ESP + 0x5c]
    eax=unchecked(esp+92u);goto L94b626c;
   L94b626c: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L94b6274;
   L94b6274: // MOV dword ptr [ESP + 0x5c],0x9b49c10
    W(unchecked(esp+92u),162831376u);goto L94b627c;
   L94b627c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b627f;
   L94b627f: // CALL 0x080b0fc4
    try{eax=Call(0x80b0fc4,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b667d;}goto L94b6284;
   L94b6284: // MOV EAX,dword ptr [ESP + 0xec]
    eax=R(unchecked(esp+236u));goto L94b628b;
   L94b628b: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b628e;
   L94b628e: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b6294;
   L94b6294: // JNZ 0x094b641a
    if(!zero)goto L94b641a;goto L94b629a;
   L94b629a: // MOV EAX,dword ptr [ESP + 0xe8]
    eax=R(unchecked(esp+232u));goto L94b62a1;
   L94b62a1: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b62a4;
   L94b62a4: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b62aa;
   L94b62aa: // JZ 0x094b633c
    if(zero)goto L94b633c;goto L94b62b0;
   L94b62b0: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b62b5;
   L94b62b5: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b62b7;
   L94b62b7: // JZ 0x094b65c0
    if(zero)goto L94b65c0;goto L94b62bd;
   L94b62bd: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b62c0;
   L94b62c0: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b62c5;
   L94b62c5: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b62c7;
   L94b62c7: // MOV EAX,0x1
    eax=1u;goto L94b62cc;
   L94b62cc: // JG 0x094b5dcb
    if(!zero&&sign==overflow)goto L94b5dcb;goto L94b62d2;
   L94b62d2: // LEA ECX,[ESP + 0x10e]
    ecx=unchecked(esp+270u);goto L94b62d9;
   L94b62d9: // JMP 0x094b5f8e
    goto L94b5f8e;
   L94b62e0: // MOV dword ptr [ESP + 0x8],EDI
    W(unchecked(esp+8u),edi);goto L94b62e4;
   L94b62e4: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L94b62e8;
   L94b62e8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b62eb;
   L94b62eb: // CALL 0x080aafa4
    eax=Call(0x80aafa4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b62f0;
   L94b62f0: // MOV EDX,EAX
    edx=eax;goto L94b62f2;
   L94b62f2: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L94b62f4;
   L94b62f4: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b62f6;
   L94b62f6: // JNZ 0x094b5dcb
    if(!zero)goto L94b5dcb;goto L94b62fc;
   L94b62fc: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b6300;
   L94b6300: // LEA ESI,[ESP + 0xf0]
    esi=unchecked(esp+240u);goto L94b6307;
   L94b6307: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b630a;
   L94b630a: // CALL 0x080ac124
    eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b630f;
   L94b630f: // LEA EAX,[EBX + 0x8]
    eax=unchecked(ebx+8u);goto L94b6312;
   L94b6312: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b6316;
   L94b6316: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6319;
   L94b6319: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b65fa;}goto L94b631e;
   L94b631e: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b6321;
   L94b6321: // CALL 0x094b4900
    eax=Call(0x94b4900,R(unchecked(esp+0u)));goto L94b6326;
   L94b6326: // MOV EAX,dword ptr [ESP + 0xf0]
    eax=R(unchecked(esp+240u));goto L94b632d;
   L94b632d: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94b6330;
   L94b6330: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b6336;
   L94b6336: // JNZ 0x094b63ec
    if(!zero)goto L94b63ec;goto L94b633c;
   L94b633c: // MOV EAX,0x1
    eax=1u;goto L94b6341;
   L94b6341: // JMP 0x094b5dcb
    goto L94b5dcb;
   L94b6348: // MOV EDX,dword ptr [ESP + 0x44]
    edx=R(unchecked(esp+68u));goto L94b634c;
   L94b634c: // LEA EAX,[ESP + 0x107]
    eax=unchecked(esp+263u);goto L94b6353;
   L94b6353: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6356;
   L94b6356: // LEA EAX,[ESP + 0xf8]
    eax=unchecked(esp+248u);goto L94b635d;
   L94b635d: // CALL 0x094b44a0
    try{eax=Call(0x94b44a0,eax,edx,ecx,R(esp));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b6512;}goto L94b6362;
   L94b6362: // LEA EAX,[ESP + 0xf8]
    eax=unchecked(esp+248u);goto L94b6369;
   L94b6369: // LEA ESI,[ESP + 0xf4]
    esi=unchecked(esp+244u);goto L94b6370;
   L94b6370: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6374;
   L94b6374: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b6377;
   L94b6377: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b64d2;}goto L94b637c;
   L94b637c: // MOV EAX,dword ptr [ESP + 0xf8]
    eax=R(unchecked(esp+248u));goto L94b6383;
   L94b6383: // LEA ECX,[EAX + -0xc]
    ecx=unchecked(eax+4294967284u);goto L94b6386;
   L94b6386: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b638c;
   L94b638c: // JZ 0x094b61b4
    if(zero)goto L94b61b4;goto L94b6392;
   L94b6392: // MOV EDX,0x80b0f04
    edx=134942468u;goto L94b6397;
   L94b6397: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6399;
   L94b6399: // JZ 0x094b64b5
    if(zero)goto L94b64b5;goto L94b639f;
   L94b639f: // OR EDX,0xffffffff
    a=edx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L94b63a2;
   L94b63a2: // XADD.LOCK dword ptr [EAX + -0x4],EDX
    a=R(unchecked(eax+4294967292u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b63a7;
   L94b63a7: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L94b63ab;
   L94b63ab: // CMP dword ptr [ESP + 0x18],0x0
    a=R(unchecked(esp+24u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b63b0;
   L94b63b0: // JG 0x094b61b4
    if(!zero&&sign==overflow)goto L94b61b4;goto L94b63b6;
   L94b63b6: // LEA EAX,[ESP + 0x10a]
    eax=unchecked(esp+266u);goto L94b63bd;
   L94b63bd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b63c1;
   L94b63c1: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b63c4;
   L94b63c4: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b63c9;
   L94b63c9: // JMP 0x094b61b4
    goto L94b61b4;
   L94b63d0: // LEA EAX,[ESP + 0x58]
    eax=unchecked(esp+88u);goto L94b63d4;
   L94b63d4: // LEA ESI,[ESP + 0xf4]
    esi=unchecked(esp+244u);goto L94b63db;
   L94b63db: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b63df;
   L94b63df: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b63e2;
   L94b63e2: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b65fc;}goto L94b63e7;
   L94b63e7: // JMP 0x094b61b4
    goto L94b61b4;
   L94b63ec: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b63f1;
   L94b63f1: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b63f3;
   L94b63f3: // JZ 0x094b6684
    if(zero)goto L94b6684;goto L94b63f9;
   L94b63f9: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b63fc;
   L94b63fc: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b6401;
   L94b6401: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6403;
   L94b6403: // MOV EAX,0x1
    eax=1u;goto L94b6408;
   L94b6408: // JG 0x094b5dcb
    if(!zero&&sign==overflow)goto L94b5dcb;goto L94b640e;
   L94b640e: // LEA ECX,[ESP + 0x10f]
    ecx=unchecked(esp+271u);goto L94b6415;
   L94b6415: // JMP 0x094b5f8e
    goto L94b5f8e;
   L94b641a: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b641f;
   L94b641f: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6421;
   L94b6421: // JZ 0x094b6692
    if(zero)goto L94b6692;goto L94b6427;
   L94b6427: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b642a;
   L94b642a: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b642f;
   L94b642f: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6431;
   L94b6431: // JG 0x094b629a
    if(!zero&&sign==overflow)goto L94b629a;goto L94b6437;
   L94b6437: // LEA EAX,[ESP + 0x10d]
    eax=unchecked(esp+269u);goto L94b643e;
   L94b643e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6442;
   L94b6442: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b6445;
   L94b6445: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b644a;
   L94b644a: // JMP 0x094b629a
    goto L94b629a;
   L94b644f: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b6454;
   L94b6454: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6456;
   L94b6456: // JZ 0x094b66a0
    if(zero)goto L94b66a0;goto L94b645c;
   L94b645c: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b645f;
   L94b645f: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b6464;
   L94b6464: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6466;
   L94b6466: // JG 0x094b621f
    if(!zero&&sign==overflow)goto L94b621f;goto L94b646c;
   L94b646c: // LEA EAX,[ESP + 0x10c]
    eax=unchecked(esp+268u);goto L94b6473;
   L94b6473: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6477;
   L94b6477: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b647a;
   L94b647a: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b647f;
   L94b647f: // JMP 0x094b621f
    goto L94b621f;
   L94b6484: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L94b6489;
   L94b6489: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b648b;
   L94b648b: // JZ 0x094b64c7
    if(zero)goto L94b64c7;goto L94b648d;
   L94b648d: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94b6490;
   L94b6490: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b6495;
   L94b6495: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b6497;
   L94b6497: // JG 0x094b61d9
    if(!zero&&sign==overflow)goto L94b61d9;goto L94b649d;
   L94b649d: // LEA EAX,[ESP + 0x10b]
    eax=unchecked(esp+267u);goto L94b64a4;
   L94b64a4: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b64a8;
   L94b64a8: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b64ab;
   L94b64ab: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b64b0;
   L94b64b0: // JMP 0x094b61d9
    goto L94b61d9;
   L94b64b5: // MOV EDX,dword ptr [EAX + -0x4]
    edx=R(unchecked(eax+4294967292u));goto L94b64b8;
   L94b64b8: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L94b64bc;
   L94b64bc: // SUB EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L94b64bf;
   L94b64bf: // MOV dword ptr [EAX + -0x4],EDX
    W(unchecked(eax+4294967292u),edx);goto L94b64c2;
   L94b64c2: // JMP 0x094b63ab
    goto L94b63ab;
   L94b64c7: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b64ca;
   L94b64ca: // LEA ESI,[ECX + -0x1]
    esi=unchecked(ecx+4294967295u);goto L94b64cd;
   L94b64cd: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L94b64d0;
   L94b64d0: // JMP 0x094b6495
    goto L94b6495;
   L94b64d2: // MOV EBX,EAX
    ebx=eax;goto L94b64d4;
   L94b64d4: // LEA EDX,[ESP + 0xf8]
    edx=unchecked(esp+248u);goto L94b64db;
   L94b64db: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b64de;
   L94b64de: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b64e3;
   L94b64e3: // MOV EAX,EBX
    eax=ebx;goto L94b64e5;
   L94b64e5: // MOV EBX,EAX
    ebx=eax;goto L94b64e7;
   L94b64e7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b64ea;
   L94b64ea: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b64ef;
   L94b64ef: // LEA EDX,[ESP + 0x28]
    edx=unchecked(esp+40u);goto L94b64f3;
   L94b64f3: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b64f6;
   L94b64f6: // CALL 0x080ac134
    eax=Call(0x80ac134,R(unchecked(esp+0u)));goto L94b64fb;
   L94b64fb: // LEA EAX,[ESP + 0xe8]
    eax=unchecked(esp+232u);goto L94b6502;
   L94b6502: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6505;
   L94b6505: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b650a;
   L94b650a: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b650d;
   L94b650d: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b6512: // LEA ESI,[ESP + 0xf4]
    esi=unchecked(esp+244u);goto L94b6519;
   L94b6519: // JMP 0x094b64e5
    goto L94b64e5;
   L94b651b: // MOV EBX,EAX
    ebx=eax;goto L94b651d;
   L94b651d: // JMP 0x094b64ef
    goto L94b64ef;
   L94b651f: // MOV EBX,EAX
    ebx=eax;goto L94b6521;
   L94b6521: // LEA EAX,[ESP + 0x58]
    eax=unchecked(esp+88u);goto L94b6525;
   L94b6525: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6528;
   L94b6528: // MOV dword ptr [ESP + 0x34],0x9b497c8
    W(unchecked(esp+52u),162830280u);goto L94b6530;
   L94b6530: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b6535;
   L94b6535: // MOV dword ptr [ESP + 0x34],0x9b48a28
    W(unchecked(esp+52u),162826792u);goto L94b653d;
   L94b653d: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b6540;
   L94b6540: // CALL 0x080ad934
    eax=Call(0x80ad934,R(unchecked(esp+0u)));goto L94b6545;
   L94b6545: // LEA EDX,[ESP + 0x28]
    edx=unchecked(esp+40u);goto L94b6549;
   L94b6549: // MOV dword ptr [ESP + 0x4],0x9b49de4
    W(unchecked(esp+4u),162831844u);goto L94b6551;
   L94b6551: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b6554;
   L94b6554: // CALL 0x080b0dd4
    eax=Call(0x80b0dd4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b6559;
   L94b6559: // LEA EAX,[ESP + 0x5c]
    eax=unchecked(esp+92u);goto L94b655d;
   L94b655d: // MOV dword ptr [ESP + 0x5c],0x9b49c10
    W(unchecked(esp+92u),162831376u);goto L94b6565;
   L94b6565: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b6568;
   L94b6568: // CALL 0x080b0fc4
    eax=Call(0x80b0fc4,R(unchecked(esp+0u)));goto L94b656d;
   L94b656d: // JMP 0x094b64fb
    goto L94b64fb;
   L94b656f: // MOV EDX,dword ptr [EDI + -0xc]
    edx=R(unchecked(edi+4294967284u));goto L94b6572;
   L94b6572: // MOV EBX,EAX
    ebx=eax;goto L94b6574;
   L94b6574: // MOV dword ptr [ESP + 0x28],EDI
    W(unchecked(esp+40u),edi);goto L94b6578;
   L94b6578: // MOV dword ptr [ESP + EDX*0x1 + 0x28],EBP
    W(unchecked(esp+edx*0x1u+40u),ebp);goto L94b657c;
   L94b657c: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L94b6584;
   L94b6584: // JMP 0x094b6559
    goto L94b6559;
   L94b6586: // MOV EBX,EAX
    ebx=eax;goto L94b6588;
   L94b6588: // JMP 0x094b6559
    goto L94b6559;
   L94b658a: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b658d;
   L94b658d: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L94b6590;
   L94b6590: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L94b6593;
   L94b6593: // JMP 0x094b5f7a
    goto L94b5f7a;
   L94b6598: // MOV EBX,EAX
    ebx=eax;goto L94b659a;
   L94b659a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b659d;
   L94b659d: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b65a2;
   L94b65a2: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b65a5;
   L94b65a5: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b65aa: // MOV EBX,EAX
    ebx=eax;goto L94b65ac;
   L94b65ac: // MOV dword ptr [ESP],0xa13d000
    W(unchecked(esp),169070592u);goto L94b65b3;
   L94b65b3: // CALL 0x080aa8f4
    eax=Call(0x80aa8f4,R(unchecked(esp+0u)));goto L94b65b8;
   L94b65b8: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b65bb;
   L94b65bb: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b65c0: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b65c3;
   L94b65c3: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L94b65c6;
   L94b65c6: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L94b65c9;
   L94b65c9: // JMP 0x094b62c5
    goto L94b62c5;
   L94b65ce: // MOV EBX,EAX
    ebx=eax;goto L94b65d0;
   L94b65d0: // MOV dword ptr [ESP],0xa13cff0
    W(unchecked(esp),169070576u);goto L94b65d7;
   L94b65d7: // CALL 0x080aa8f4
    eax=Call(0x80aa8f4,R(unchecked(esp+0u)));goto L94b65dc;
   L94b65dc: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b65df;
   L94b65df: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b65e4: // MOV EBX,EAX
    ebx=eax;goto L94b65e6;
   L94b65e6: // MOV dword ptr [ESP],0xa13cfe0
    W(unchecked(esp),169070560u);goto L94b65ed;
   L94b65ed: // CALL 0x080aa8f4
    eax=Call(0x80aa8f4,R(unchecked(esp+0u)));goto L94b65f2;
   L94b65f2: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b65f5;
   L94b65f5: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b65fa: // JMP 0x094b6598
    goto L94b6598;
   L94b65fc: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L94b6600;
   L94b6600: // JMP 0x094b64e5
    goto L94b64e5;
   L94b6605: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L94b6609;
   L94b6609: // LEA EDI,[EDI]
    edi=unchecked(edi);goto L94b6610;
   L94b6610: // JMP 0x094b64e5
    goto L94b64e5;
   L94b6615: // MOV EDX,0x80b0f04
    edx=134942468u;goto L94b661a;
   L94b661a: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b661c;
   L94b661c: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L94b6620;
   L94b6620: // JZ 0x094b666e
    if(zero)goto L94b666e;goto L94b6622;
   L94b6622: // OR EDX,0xffffffff
    a=edx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L94b6625;
   L94b6625: // XADD.LOCK dword ptr [EAX + -0x4],EDX
    a=R(unchecked(eax+4294967292u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94b662a;
   L94b662a: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L94b662e;
   L94b662e: // CMP dword ptr [ESP + 0x18],0x0
    a=R(unchecked(esp+24u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b6633;
   L94b6633: // JG 0x094b61b4
    if(!zero&&sign==overflow)goto L94b61b4;goto L94b6639;
   L94b6639: // LEA EAX,[ESP + 0x109]
    eax=unchecked(esp+265u);goto L94b6640;
   L94b6640: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94b6644;
   L94b6644: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b6647;
   L94b6647: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b664c;
   L94b664c: // JMP 0x094b61b4
    goto L94b61b4;
   L94b6651: // MOV EBX,EAX
    ebx=eax;goto L94b6653;
   L94b6653: // LEA EAX,[ESP + 0xfc]
    eax=unchecked(esp+252u);goto L94b665a;
   L94b665a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b665d;
   L94b665d: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b6662;
   L94b6662: // MOV EAX,EBX
    eax=ebx;goto L94b6664;
   L94b6664: // JMP 0x094b64e5
    goto L94b64e5;
   L94b6669: // JMP 0x094b6512
    goto L94b6512;
   L94b666e: // MOV EDX,dword ptr [EAX + -0x4]
    edx=R(unchecked(eax+4294967292u));goto L94b6671;
   L94b6671: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L94b6675;
   L94b6675: // SUB EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L94b6678;
   L94b6678: // MOV dword ptr [EAX + -0x4],EDX
    W(unchecked(eax+4294967292u),edx);goto L94b667b;
   L94b667b: // JMP 0x094b662e
    goto L94b662e;
   L94b667d: // MOV EBX,EAX
    ebx=eax;goto L94b667f;
   L94b667f: // JMP 0x094b64fb
    goto L94b64fb;
   L94b6684: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b6687;
   L94b6687: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L94b668a;
   L94b668a: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L94b668d;
   L94b668d: // JMP 0x094b6401
    goto L94b6401;
   L94b6692: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b6695;
   L94b6695: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L94b6698;
   L94b6698: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L94b669b;
   L94b669b: // JMP 0x094b642f
    goto L94b642f;
   L94b66a0: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L94b66a3;
   L94b66a3: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L94b66a6;
   L94b66a6: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L94b66a9;
   L94b66a9: // JMP 0x094b6464
    goto L94b6464;
  }
 }
}
