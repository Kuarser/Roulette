using System;
namespace RouletteDeluxe
{
 // Physical XMLMsgVofSimpleOptions::BuildFromMsg 09499960. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; builder/message are at +b0/+b4.
 public sealed class OriginalConfigurationSimpleMessageParse
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
  public OriginalConfigurationSimpleMessageParse(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Build(uint frame,uint incomingEbx,uint incomingEsi,uint incomingEdi)
  {
   uint eax=0,ebx=incomingEbx,esi=incomingEsi,edi=incomingEdi,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L949996a;
   L949996a: // LEA EAX,[ESP + 0x7d]
    eax=unchecked(esp+125u);goto L949996e;
   L949996e: // MOV EBP,dword ptr [ESP + 0xb0]
    ebp=R(unchecked(esp+176u));goto L9499975;
   L9499975: // LEA EBX,[ESP + 0x60]
    ebx=unchecked(esp+96u);goto L9499979;
   L9499979: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L949997d;
   L949997d: // MOV dword ptr [ESP + 0x4],0x9767d0b
    W(unchecked(esp+4u),158760203u);goto L9499985;
   L9499985: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499988;
   L9499988: // CALL 0x080afcf4
    eax=Call(0x80afcf4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L949998d;
   L949998d: // MOV EDX,dword ptr [ESP + 0xb4]
    edx=R(unchecked(esp+180u));goto L9499994;
   L9499994: // MOV ECX,EBX
    ecx=ebx;goto L9499996;
   L9499996: // LEA EAX,[ESP + 0x54]
    eax=unchecked(esp+84u);goto L949999a;
   L949999a: // CALL 0x094997e0
    try{eax=Call(0x94997e0,eax,edx,ecx);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a5e5;}goto L949999f;
   L949999f: // MOV EAX,dword ptr [ESP + 0x60]
    eax=R(unchecked(esp+96u));goto L94999a3;
   L94999a3: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94999a6;
   L94999a6: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94999ac;
   L94999ac: // JNZ 0x0949a62d
    if(!zero)goto L949a62d;goto L94999b2;
   L94999b2: // MOV EAX,dword ptr [ESP + 0x54]
    eax=R(unchecked(esp+84u));goto L94999b6;
   L94999b6: // MOV dword ptr [ESP + 0x64],0x9b482ac
    W(unchecked(esp+100u),162824876u);goto L94999be;
   L94999be: // MOV dword ptr [ESP + 0x68],0x9b482ac
    W(unchecked(esp+104u),162824876u);goto L94999c6;
   L94999c6: // MOV dword ptr [ESP + 0x70],EAX
    W(unchecked(esp+112u),eax);goto L94999ca;
   L94999ca: // JMP 0x09499aac
    goto L9499aac;
   L94999d0: // MOV EDX,0x9b482ac
    edx=162824876u;goto L94999d5;
   L94999d5: // JC 0x09499d10
    if(carry)goto L9499d10;goto L94999db;
   L94999db: // LEA EAX,[EDX + -0xc]
    eax=unchecked(edx+4294967284u);goto L94999de;
   L94999de: // CMP EAX,0x9b482a0
    a=eax;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94999e3;
   L94999e3: // JNZ 0x0949a238
    if(!zero)goto L949a238;goto L94999e9;
   L94999e9: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L94999ed;
   L94999ed: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L94999f0;
   L94999f0: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94999f6;
   L94999f6: // JNZ 0x0949a2d8
    if(!zero)goto L949a2d8;goto L94999fc;
   L94999fc: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9499a00;
   L9499a00: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499a03;
   L9499a03: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499a09;
   L9499a09: // JNZ 0x0949a2a0
    if(!zero)goto L949a2a0;goto L9499a0f;
   L9499a0f: // MOV EAX,dword ptr [ESP + 0x64]
    eax=R(unchecked(esp+100u));goto L9499a13;
   L9499a13: // MOV ESI,dword ptr [EAX + -0xc]
    esi=R(unchecked(eax+4294967284u));goto L9499a16;
   L9499a16: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499a18;
   L9499a18: // JZ 0x09499aa8
    if(zero)goto L9499aa8;goto L9499a1e;
   L9499a1e: // MOV EAX,dword ptr [ESP + 0x68]
    eax=R(unchecked(esp+104u));goto L9499a22;
   L9499a22: // MOV EBX,dword ptr [EAX + -0xc]
    ebx=R(unchecked(eax+4294967284u));goto L9499a25;
   L9499a25: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499a27;
   L9499a27: // JZ 0x09499aa8
    if(zero)goto L9499aa8;goto L9499a29;
   L9499a29: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L9499a2c;
   L9499a2c: // SUB EAX,dword ptr [EBP + 0x4]
    a=eax;b=R(unchecked(ebp+4u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499a2f;
   L9499a2f: // SAR EAX,0x4
    a=eax;v=unchecked((uint)((int)a>>4));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&8u)!=0;overflow=false;eax=v;goto L9499a32;
   L9499a32: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499a34;
   L9499a34: // JZ 0x09499aa8
    if(zero)goto L9499aa8;goto L9499a36;
   L9499a36: // LEA EDX,[ESP + 0x68]
    edx=unchecked(esp+104u);goto L9499a3a;
   L9499a3a: // LEA ESI,[ESP + 0x78]
    esi=unchecked(esp+120u);goto L9499a3e;
   L9499a3e: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L9499a42;
   L9499a42: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9499a45;
   L9499a45: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a626;}goto L9499a4a;
   L9499a4a: // LEA ECX,[ESP + 0x64]
    ecx=unchecked(esp+100u);goto L9499a4e;
   L9499a4e: // LEA EBX,[ESP + 0x74]
    ebx=unchecked(esp+116u);goto L9499a52;
   L9499a52: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L9499a56;
   L9499a56: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499a59;
   L9499a59: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a5f7;}goto L9499a5e;
   L9499a5e: // MOV EDX,dword ptr [EBP + 0x4]
    edx=R(unchecked(ebp+4u));goto L9499a61;
   L9499a61: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L9499a65;
   L9499a65: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9499a69;
   L9499a69: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L9499a6c;
   L9499a6c: // SUB EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499a6e;
   L9499a6e: // AND EAX,0xfffffff0
    a=eax;b=4294967280u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L9499a71;
   L9499a71: // LEA EAX,[EDX + EAX*0x1 + -0x10]
    eax=unchecked(edx+eax*0x1u+4294967280u);goto L9499a75;
   L9499a75: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499a78;
   L9499a78: // CALL 0x094b5d40
    try{eax=Call(0x94b5d40,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a683;}goto L9499a7d;
   L9499a7d: // MOV EAX,dword ptr [ESP + 0x74]
    eax=R(unchecked(esp+116u));goto L9499a81;
   L9499a81: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499a84;
   L9499a84: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499a8a;
   L9499a8a: // JNZ 0x0949a694
    if(!zero)goto L949a694;goto L9499a90;
   L9499a90: // MOV EAX,dword ptr [ESP + 0x78]
    eax=R(unchecked(esp+120u));goto L9499a94;
   L9499a94: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499a97;
   L9499a97: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499a9d;
   L9499a9d: // JNZ 0x0949a5aa
    if(!zero)goto L949a5aa;goto L9499aa3;
   L9499aa3: // NOP
    goto L9499aa4;
   L9499aa4: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9499aa8;
   L9499aa8: // MOV EAX,dword ptr [ESP + 0x70]
    eax=R(unchecked(esp+112u));goto L9499aac;
   L9499aac: // CMP dword ptr [ESP + 0x58],EAX
    a=R(unchecked(esp+88u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499ab0;
   L9499ab0: // JZ 0x09499f68
    if(zero)goto L9499f68;goto L9499ab6;
   L9499ab6: // LEA EAX,[ESP + 0x6c]
    eax=unchecked(esp+108u);goto L9499aba;
   L9499aba: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499abe;
   L9499abe: // LEA EAX,[ESP + 0x68]
    eax=unchecked(esp+104u);goto L9499ac2;
   L9499ac2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499ac5;
   L9499ac5: // LEA ECX,[ESP + 0x64]
    ecx=unchecked(esp+100u);goto L9499ac9;
   L9499ac9: // LEA EDX,[ESP + 0x70]
    edx=unchecked(esp+112u);goto L9499acd;
   L9499acd: // LEA EAX,[ESP + 0x54]
    eax=unchecked(esp+84u);goto L9499ad1;
   L9499ad1: // CALL 0x094988c0
    try{eax=Call(0x94988c0,eax,edx,ecx,R(esp),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a626;}goto L9499ad6;
   L9499ad6: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499ad8;
   L9499ad8: // JZ 0x09499a0f
    if(zero)goto L9499a0f;goto L9499ade;
   L9499ade: // MOV EDI,dword ptr [EBP + 0x8]
    edi=R(unchecked(ebp+8u));goto L9499ae1;
   L9499ae1: // MOV EAX,dword ptr [EBP + 0x4]
    eax=R(unchecked(ebp+4u));goto L9499ae4;
   L9499ae4: // MOV dword ptr [ESP + 0x34],0x9b482ac
    W(unchecked(esp+52u),162824876u);goto L9499aec;
   L9499aec: // MOV dword ptr [ESP + 0x38],0x9b482ac
    W(unchecked(esp+56u),162824876u);goto L9499af4;
   L9499af4: // MOV EBX,EDI
    ebx=edi;goto L9499af6;
   L9499af6: // SUB EBX,EAX
    a=ebx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebx=v;goto L9499af8;
   L9499af8: // SAR EBX,0x4
    a=ebx;v=unchecked((uint)((int)a>>4));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&8u)!=0;overflow=false;ebx=v;goto L9499afb;
   L9499afb: // LEA EDX,[EBX + 0x1]
    edx=unchecked(ebx+1u);goto L9499afe;
   L9499afe: // CMP EDX,EBX
    a=edx;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499b00;
   L9499b00: // MOV dword ptr [ESP + 0x3c],0x9b482ac
    W(unchecked(esp+60u),162824876u);goto L9499b08;
   L9499b08: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L9499b10;
   L9499b10: // MOV dword ptr [ESP + 0x20],EDX
    W(unchecked(esp+32u),edx);goto L9499b14;
   L9499b14: // JBE 0x094999d0
    if(carry||zero)goto L94999d0;goto L9499b1a;
   L9499b1a: // MOV EAX,dword ptr [EBP + 0xc]
    eax=R(unchecked(ebp+12u));goto L9499b1d;
   L9499b1d: // SUB EAX,EDI
    a=eax;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499b1f;
   L9499b1f: // SAR EAX,0x4
    a=eax;v=unchecked((uint)((int)a>>4));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&8u)!=0;overflow=false;eax=v;goto L9499b22;
   L9499b22: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499b24;
   L9499b24: // JNZ 0x09499db8
    if(!zero)goto L9499db8;goto L9499b2a;
   L9499b2a: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499b2c;
   L9499b2c: // MOV EAX,0x1
    eax=1u;goto L9499b31;
   L9499b31: // JNZ 0x09499d98
    if(!zero)goto L9499d98;goto L9499b37;
   L9499b37: // MOV EDX,EAX
    edx=eax;goto L9499b39;
   L9499b39: // SHL EDX,0x4
    a=edx;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;edx=v;goto L9499b3c;
   L9499b3c: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L9499b40;
   L9499b40: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9499b43;
   L9499b43: // LEA ESI,[ESP + 0x34]
    esi=unchecked(esp+52u);goto L9499b47;
   L9499b47: // CALL 0x080aad84
    try{eax=Call(0x80aad84,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a5a3;}goto L9499b4c;
   L9499b4c: // SHL EBX,0x4
    a=ebx;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;ebx=v;goto L9499b4f;
   L9499b4f: // ADD EBX,EAX
    a=ebx;b=eax;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499b51;
   L9499b51: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9499b55;
   L9499b55: // MOV dword ptr [ESP + 0x14],EBX
    W(unchecked(esp+20u),ebx);goto L9499b59;
   L9499b59: // JZ 0x09499ba4
    if(zero)goto L9499ba4;goto L9499b5b;
   L9499b5b: // LEA ESI,[ESP + 0x34]
    esi=unchecked(esp+52u);goto L9499b5f;
   L9499b5f: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L9499b63;
   L9499b63: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499b66;
   L9499b66: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a5a1;}goto L9499b6b;
   L9499b6b: // MOV EBX,dword ptr [ESP + 0x14]
    ebx=R(unchecked(esp+20u));goto L9499b6f;
   L9499b6f: // LEA EAX,[ESP + 0x38]
    eax=unchecked(esp+56u);goto L9499b73;
   L9499b73: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499b77;
   L9499b77: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499b7a;
   L9499b7a: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499b7d;
   L9499b7d: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a59d;}goto L9499b82;
   L9499b82: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L9499b86;
   L9499b86: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499b8a;
   L9499b8a: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9499b8e;
   L9499b8e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9499b91;
   L9499b91: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499b94;
   L9499b94: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a562;}goto L9499b99;
   L9499b99: // MOV EAX,dword ptr [ESP + 0x40]
    eax=R(unchecked(esp+64u));goto L9499b9d;
   L9499b9d: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L9499ba1;
   L9499ba1: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L9499ba4;
   L9499ba4: // MOV EBX,dword ptr [EBP + 0x4]
    ebx=R(unchecked(ebp+4u));goto L9499ba7;
   L9499ba7: // MOV ESI,dword ptr [ESP + 0x1c]
    esi=R(unchecked(esp+28u));goto L9499bab;
   L9499bab: // CMP EDI,EBX
    a=edi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499bad;
   L9499bad: // JZ 0x09499bf7
    if(zero)goto L9499bf7;goto L9499baf;
   L9499baf: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499bb1;
   L9499bb1: // JZ 0x09499bed
    if(zero)goto L9499bed;goto L9499bb3;
   L9499bb3: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9499bb7;
   L9499bb7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9499bba;
   L9499bba: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a55e;}goto L9499bbf;
   L9499bbf: // LEA ECX,[ESI + 0x4]
    ecx=unchecked(esi+4u);goto L9499bc2;
   L9499bc2: // LEA EAX,[EBX + 0x4]
    eax=unchecked(ebx+4u);goto L9499bc5;
   L9499bc5: // MOV dword ptr [ESP + 0x18],ECX
    W(unchecked(esp+24u),ecx);goto L9499bc9;
   L9499bc9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499bcd;
   L9499bcd: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L9499bd0;
   L9499bd0: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a52c;}goto L9499bd5;
   L9499bd5: // LEA EAX,[EBX + 0x8]
    eax=unchecked(ebx+8u);goto L9499bd8;
   L9499bd8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499bdc;
   L9499bdc: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L9499bdf;
   L9499bdf: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499be2;
   L9499be2: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a721;}goto L9499be7;
   L9499be7: // MOV EAX,dword ptr [EBX + 0xc]
    eax=R(unchecked(ebx+12u));goto L9499bea;
   L9499bea: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L9499bed;
   L9499bed: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499bf0;
   L9499bf0: // ADD ESI,0x10
    a=esi;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9499bf3;
   L9499bf3: // CMP EDI,EBX
    a=edi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499bf5;
   L9499bf5: // JNZ 0x09499baf
    if(!zero)goto L9499baf;goto L9499bf7;
   L9499bf7: // MOV EDX,dword ptr [EBP + 0x8]
    edx=R(unchecked(ebp+8u));goto L9499bfa;
   L9499bfa: // ADD ESI,0x10
    a=esi;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9499bfd;
   L9499bfd: // MOV EBX,EDI
    ebx=edi;goto L9499bff;
   L9499bff: // MOV dword ptr [ESP + 0x24],ESI
    W(unchecked(esp+36u),esi);goto L9499c03;
   L9499c03: // CMP EDI,EDX
    a=edi;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499c05;
   L9499c05: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L9499c09;
   L9499c09: // JZ 0x0949a0c3
    if(zero)goto L949a0c3;goto L9499c0f;
   L9499c0f: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto L9499c13;
   L9499c13: // MOV EBP,dword ptr [ESP + 0x18]
    ebp=R(unchecked(esp+24u));goto L9499c17;
   L9499c17: // NOP
    goto L9499c18;
   L9499c18: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499c1a;
   L9499c1a: // JZ 0x09499c52
    if(zero)goto L9499c52;goto L9499c1c;
   L9499c1c: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9499c20;
   L9499c20: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9499c23;
   L9499c23: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a424;}goto L9499c28;
   L9499c28: // LEA EDI,[ESI + 0x4]
    edi=unchecked(esi+4u);goto L9499c2b;
   L9499c2b: // LEA EAX,[EBX + 0x4]
    eax=unchecked(ebx+4u);goto L9499c2e;
   L9499c2e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499c32;
   L9499c32: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9499c35;
   L9499c35: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a420;}goto L9499c3a;
   L9499c3a: // LEA EAX,[EBX + 0x8]
    eax=unchecked(ebx+8u);goto L9499c3d;
   L9499c3d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499c41;
   L9499c41: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L9499c44;
   L9499c44: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499c47;
   L9499c47: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a3e6;}goto L9499c4c;
   L9499c4c: // MOV EAX,dword ptr [EBX + 0xc]
    eax=R(unchecked(ebx+12u));goto L9499c4f;
   L9499c4f: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L9499c52;
   L9499c52: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499c55;
   L9499c55: // ADD ESI,0x10
    a=esi;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9499c58;
   L9499c58: // CMP EBP,EBX
    a=ebp;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499c5a;
   L9499c5a: // JNZ 0x09499c18
    if(!zero)goto L9499c18;goto L9499c5c;
   L9499c5c: // MOV EBP,dword ptr [ESP + 0x28]
    ebp=R(unchecked(esp+40u));goto L9499c60;
   L9499c60: // MOV ECX,dword ptr [EBP + 0x8]
    ecx=R(unchecked(ebp+8u));goto L9499c63;
   L9499c63: // MOV dword ptr [ESP + 0x18],ECX
    W(unchecked(esp+24u),ecx);goto L9499c67;
   L9499c67: // MOV EDX,ECX
    edx=ecx;goto L9499c69;
   L9499c69: // MOV EAX,dword ptr [EBP + 0x4]
    eax=R(unchecked(ebp+4u));goto L9499c6c;
   L9499c6c: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499c6e;
   L9499c6e: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L9499c72;
   L9499c72: // JZ 0x09499cdb
    if(zero)goto L9499cdb;goto L9499c74;
   L9499c74: // MOV EBX,EAX
    ebx=eax;goto L9499c76;
   L9499c76: // LEA EAX,[EAX + 0x8]
    eax=unchecked(eax+8u);goto L9499c79;
   L9499c79: // MOV dword ptr [ESP + 0x24],EBP
    W(unchecked(esp+36u),ebp);goto L9499c7d;
   L9499c7d: // LEA EDI,[EBX + 0x4]
    edi=unchecked(ebx+4u);goto L9499c80;
   L9499c80: // MOV EBP,EAX
    ebp=eax;goto L9499c82;
   L9499c82: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto L9499c86;
   L9499c86: // NOP
    goto L9499c88;
   L9499c88: // MOV ESI,EBX
    esi=ebx;goto L9499c8a;
   L9499c8a: // SUB ESI,dword ptr [ESP + 0x14]
    a=esi;b=R(unchecked(esp+20u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9499c8e;
   L9499c8e: // MOV EAX,dword ptr [ESI + EBP*0x1]
    eax=R(unchecked(esi+ebp*0x1u));goto L9499c91;
   L9499c91: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499c94;
   L9499c94: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499c9a;
   L9499c9a: // JNZ 0x0949a184
    if(!zero)goto L949a184;goto L9499ca0;
   L9499ca0: // MOV EAX,dword ptr [ESI + EDI*0x1]
    eax=R(unchecked(esi+edi*0x1u));goto L9499ca3;
   L9499ca3: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499ca6;
   L9499ca6: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499cac;
   L9499cac: // JNZ 0x0949a200
    if(!zero)goto L949a200;goto L9499cb2;
   L9499cb2: // MOV EAX,dword ptr [EBX]
    eax=R(unchecked(ebx));goto L9499cb4;
   L9499cb4: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499cb7;
   L9499cb7: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499cbd;
   L9499cbd: // JNZ 0x0949a1c8
    if(!zero)goto L949a1c8;goto L9499cc3;
   L9499cc3: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499cc6;
   L9499cc6: // CMP EBX,dword ptr [ESP + 0x18]
    a=ebx;b=R(unchecked(esp+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499cca;
   L9499cca: // JNZ 0x09499c88
    if(!zero)goto L9499c88;goto L9499ccc;
   L9499ccc: // MOV EBP,dword ptr [ESP + 0x24]
    ebp=R(unchecked(esp+36u));goto L9499cd0;
   L9499cd0: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9499cd4;
   L9499cd4: // MOV EAX,dword ptr [EBP + 0x4]
    eax=R(unchecked(ebp+4u));goto L9499cd7;
   L9499cd7: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L9499cdb;
   L9499cdb: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9499cdf;
   L9499cdf: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499ce1;
   L9499ce1: // JZ 0x09499cef
    if(zero)goto L9499cef;goto L9499ce3;
   L9499ce3: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L9499ce7;
   L9499ce7: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9499cea;
   L9499cea: // CALL 0x080aabe4
    eax=Call(0x80aabe4,R(unchecked(esp+0u)));goto L9499cef;
   L9499cef: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9499cf3;
   L9499cf3: // MOV dword ptr [EBP + 0x8],ESI
    W(unchecked(ebp+8u),esi);goto L9499cf6;
   L9499cf6: // MOV dword ptr [EBP + 0x4],EAX
    W(unchecked(ebp+4u),eax);goto L9499cf9;
   L9499cf9: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L9499cfd;
   L9499cfd: // SHL EAX,0x4
    a=eax;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;eax=v;goto L9499d00;
   L9499d00: // ADD EAX,dword ptr [ESP + 0x1c]
    a=eax;b=R(unchecked(esp+28u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9499d04;
   L9499d04: // MOV dword ptr [EBP + 0xc],EAX
    W(unchecked(ebp+12u),eax);goto L9499d07;
   L9499d07: // MOV EDX,dword ptr [ESP + 0x3c]
    edx=R(unchecked(esp+60u));goto L9499d0b;
   L9499d0b: // JMP 0x094999db
    goto L94999db;
   L9499d10: // MOV ECX,dword ptr [ESP + 0x20]
    ecx=R(unchecked(esp+32u));goto L9499d14;
   L9499d14: // SHL ECX,0x4
    a=ecx;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;ecx=v;goto L9499d17;
   L9499d17: // ADD ECX,EAX
    a=ecx;b=eax;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L9499d19;
   L9499d19: // CMP EDI,ECX
    a=edi;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d1b;
   L9499d1b: // MOV dword ptr [ESP + 0x14],ECX
    W(unchecked(esp+20u),ecx);goto L9499d1f;
   L9499d1f: // JZ 0x09499d86
    if(zero)goto L9499d86;goto L9499d21;
   L9499d21: // LEA EAX,[ECX + 0x8]
    eax=unchecked(ecx+8u);goto L9499d24;
   L9499d24: // MOV EBX,ECX
    ebx=ecx;goto L9499d26;
   L9499d26: // LEA EDX,[ECX + 0x4]
    edx=unchecked(ecx+4u);goto L9499d29;
   L9499d29: // MOV dword ptr [ESP + 0x1c],EBP
    W(unchecked(esp+28u),ebp);goto L9499d2d;
   L9499d2d: // MOV EBP,EAX
    ebp=eax;goto L9499d2f;
   L9499d2f: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L9499d33;
   L9499d33: // NOP
    goto L9499d34;
   L9499d34: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9499d38;
   L9499d38: // MOV ESI,EBX
    esi=ebx;goto L9499d3a;
   L9499d3a: // SUB ESI,dword ptr [ESP + 0x14]
    a=esi;b=R(unchecked(esp+20u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9499d3e;
   L9499d3e: // MOV EAX,dword ptr [ESI + EBP*0x1]
    eax=R(unchecked(esi+ebp*0x1u));goto L9499d41;
   L9499d41: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499d44;
   L9499d44: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d4a;
   L9499d4a: // JNZ 0x0949a10e
    if(!zero)goto L949a10e;goto L9499d50;
   L9499d50: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L9499d54;
   L9499d54: // MOV EAX,dword ptr [ESI + EDX*0x1]
    eax=R(unchecked(esi+edx*0x1u));goto L9499d57;
   L9499d57: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499d5a;
   L9499d5a: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d60;
   L9499d60: // JNZ 0x0949a0d7
    if(!zero)goto L949a0d7;goto L9499d66;
   L9499d66: // MOV EAX,dword ptr [EBX]
    eax=R(unchecked(ebx));goto L9499d68;
   L9499d68: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499d6b;
   L9499d6b: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d71;
   L9499d71: // JNZ 0x0949a14d
    if(!zero)goto L949a14d;goto L9499d77;
   L9499d77: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499d7a;
   L9499d7a: // CMP EDI,EBX
    a=edi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d7c;
   L9499d7c: // JNZ 0x09499d38
    if(!zero)goto L9499d38;goto L9499d7e;
   L9499d7e: // MOV EBP,dword ptr [ESP + 0x1c]
    ebp=R(unchecked(esp+28u));goto L9499d82;
   L9499d82: // MOV EDX,dword ptr [ESP + 0x3c]
    edx=R(unchecked(esp+60u));goto L9499d86;
   L9499d86: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9499d8a;
   L9499d8a: // MOV dword ptr [EBP + 0x8],EAX
    W(unchecked(ebp+8u),eax);goto L9499d8d;
   L9499d8d: // JMP 0x094999db
    goto L94999db;
   L9499d98: // LEA EAX,[EBX + EBX*0x1]
    eax=unchecked(ebx+ebx*0x1u);goto L9499d9b;
   L9499d9b: // CMP EBX,EAX
    a=ebx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499d9d;
   L9499d9d: // JBE 0x0949a38f
    if(carry||zero)goto L949a38f;goto L9499da3;
   L9499da3: // MOV EDX,0xfffffff0
    edx=4294967280u;goto L9499da8;
   L9499da8: // MOV dword ptr [ESP + 0x2c],0xfffffff
    W(unchecked(esp+44u),268435455u);goto L9499db0;
   L9499db0: // JMP 0x09499b40
    goto L9499b40;
   L9499db8: // LEA ESI,[ESP + 0x34]
    esi=unchecked(esp+52u);goto L9499dbc;
   L9499dbc: // LEA ECX,[ESP + 0x44]
    ecx=unchecked(esp+68u);goto L9499dc0;
   L9499dc0: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L9499dc4;
   L9499dc4: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L9499dc7;
   L9499dc7: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a5a3;}goto L9499dcc;
   L9499dcc: // LEA EAX,[ESP + 0x38]
    eax=unchecked(esp+56u);goto L9499dd0;
   L9499dd0: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499dd4;
   L9499dd4: // LEA EAX,[ESP + 0x48]
    eax=unchecked(esp+72u);goto L9499dd8;
   L9499dd8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499ddb;
   L9499ddb: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a756;}goto L9499de0;
   L9499de0: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L9499de4;
   L9499de4: // LEA EDX,[ESP + 0x4c]
    edx=unchecked(esp+76u);goto L9499de8;
   L9499de8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499dec;
   L9499dec: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9499def;
   L9499def: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a79c;}goto L9499df4;
   L9499df4: // MOV ECX,dword ptr [EBP + 0x8]
    ecx=R(unchecked(ebp+8u));goto L9499df7;
   L9499df7: // MOV EAX,dword ptr [ESP + 0x40]
    eax=R(unchecked(esp+64u));goto L9499dfb;
   L9499dfb: // MOV dword ptr [ESP + 0x14],ECX
    W(unchecked(esp+20u),ecx);goto L9499dff;
   L9499dff: // SUB ECX,EDI
    a=ecx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ecx=v;goto L9499e01;
   L9499e01: // SAR ECX,0x4
    a=ecx;v=unchecked((uint)((int)a>>4));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&8u)!=0;overflow=false;ecx=v;goto L9499e04;
   L9499e04: // CMP ECX,0x1
    a=ecx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499e07;
   L9499e07: // MOV dword ptr [ESP + 0x50],EAX
    W(unchecked(esp+80u),eax);goto L9499e0b;
   L9499e0b: // MOV dword ptr [ESP + 0x18],ECX
    W(unchecked(esp+24u),ecx);goto L9499e0f;
   L9499e0f: // JBE 0x09499fdd
    if(carry||zero)goto L9499fdd;goto L9499e15;
   L9499e15: // MOV ECX,dword ptr [ESP + 0x14]
    ecx=R(unchecked(esp+20u));goto L9499e19;
   L9499e19: // SUB ECX,0x10
    a=ecx;b=16u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ecx=v;goto L9499e1c;
   L9499e1c: // MOV dword ptr [ESP + 0x20],ECX
    W(unchecked(esp+32u),ecx);goto L9499e20;
   L9499e20: // MOV ECX,dword ptr [ESP + 0x14]
    ecx=R(unchecked(esp+20u));goto L9499e24;
   L9499e24: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L9499e28;
   L9499e28: // MOV EDX,ECX
    edx=ecx;goto L9499e2a;
   L9499e2a: // CALL 0x09499720
    try{eax=Call(0x9499720,eax,edx,ecx);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499e2f;
   L9499e2f: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L9499e33;
   L9499e33: // ADD dword ptr [EBP + 0x8],0x10
    a=R(unchecked(ebp+8u));b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(ebp+8u),v);goto L9499e37;
   L9499e37: // SUB EAX,EDI
    a=eax;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499e39;
   L9499e39: // SAR EAX,0x4
    a=eax;v=unchecked((uint)((int)a>>4));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&8u)!=0;overflow=false;eax=v;goto L9499e3c;
   L9499e3c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499e3e;
   L9499e3e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9499e42;
   L9499e42: // JLE 0x09499eb5
    if(zero||sign!=overflow)goto L9499eb5;goto L9499e44;
   L9499e44: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L9499e48;
   L9499e48: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L9499e4c;
   L9499e4c: // MOV EBX,EDX
    ebx=edx;goto L9499e4e;
   L9499e4e: // MOV EAX,EDX
    eax=edx;goto L9499e50;
   L9499e50: // JMP 0x09499e5e
    goto L9499e5e;
   L9499e58: // MOV dword ptr [ESP + 0x18],EBX
    W(unchecked(esp+24u),ebx);goto L9499e5c;
   L9499e5c: // MOV EAX,EBX
    eax=ebx;goto L9499e5e;
   L9499e5e: // SUB EBX,0x10
    a=ebx;b=16u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebx=v;goto L9499e61;
   L9499e61: // SUB EAX,0x20
    a=eax;b=32u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499e64;
   L9499e64: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499e68;
   L9499e68: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499e6b;
   L9499e6b: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499e70;
   L9499e70: // LEA EAX,[EBX + -0xc]
    eax=unchecked(ebx+4294967284u);goto L9499e73;
   L9499e73: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499e77;
   L9499e77: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L9499e7b;
   L9499e7b: // SUB EAX,0xc
    a=eax;b=12u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499e7e;
   L9499e7e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499e81;
   L9499e81: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499e86;
   L9499e86: // LEA EAX,[EBX + -0x8]
    eax=unchecked(ebx+4294967288u);goto L9499e89;
   L9499e89: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499e8d;
   L9499e8d: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L9499e91;
   L9499e91: // SUB EAX,0x8
    a=eax;b=8u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499e94;
   L9499e94: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499e97;
   L9499e97: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499e9c;
   L9499e9c: // MOV ECX,dword ptr [ESP + 0x20]
    ecx=R(unchecked(esp+32u));goto L9499ea0;
   L9499ea0: // MOV EAX,EBX
    eax=ebx;goto L9499ea2;
   L9499ea2: // SUB EAX,dword ptr [ESP + 0x14]
    a=eax;b=R(unchecked(esp+20u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499ea6;
   L9499ea6: // SUB dword ptr [ESP + 0x1c],0x1
    a=R(unchecked(esp+28u));b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;W(unchecked(esp+28u),v);goto L9499eab;
   L9499eab: // MOV EDX,dword ptr [EAX + ECX*0x1 + 0xc]
    edx=R(unchecked(eax+ecx*0x1u+12u));goto L9499eaf;
   L9499eaf: // MOV dword ptr [EAX + ECX*0x1 + 0x1c],EDX
    W(unchecked(eax+ecx*0x1u+28u),edx);goto L9499eb3;
   L9499eb3: // JNZ 0x09499e58
    if(!zero)goto L9499e58;goto L9499eb5;
   L9499eb5: // LEA EAX,[ESP + 0x44]
    eax=unchecked(esp+68u);goto L9499eb9;
   L9499eb9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499ebd;
   L9499ebd: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9499ec0;
   L9499ec0: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499ec5;
   L9499ec5: // LEA EDX,[ESP + 0x48]
    edx=unchecked(esp+72u);goto L9499ec9;
   L9499ec9: // LEA EAX,[EDI + 0x4]
    eax=unchecked(edi+4u);goto L9499ecc;
   L9499ecc: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L9499ed0;
   L9499ed0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499ed3;
   L9499ed3: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499ed8;
   L9499ed8: // LEA ECX,[ESP + 0x4c]
    ecx=unchecked(esp+76u);goto L9499edc;
   L9499edc: // LEA EAX,[EDI + 0x8]
    eax=unchecked(edi+8u);goto L9499edf;
   L9499edf: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L9499ee3;
   L9499ee3: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499ee6;
   L9499ee6: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L9499eeb;
   L9499eeb: // MOV EAX,dword ptr [ESP + 0x50]
    eax=R(unchecked(esp+80u));goto L9499eef;
   L9499eef: // MOV dword ptr [EDI + 0xc],EAX
    W(unchecked(edi+12u),eax);goto L9499ef2;
   L9499ef2: // MOV EAX,dword ptr [ESP + 0x4c]
    eax=R(unchecked(esp+76u));goto L9499ef6;
   L9499ef6: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499ef9;
   L9499ef9: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499eff;
   L9499eff: // JNZ 0x0949a459
    if(!zero)goto L949a459;goto L9499f05;
   L9499f05: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L9499f09;
   L9499f09: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499f0c;
   L9499f0c: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499f12;
   L9499f12: // JNZ 0x0949a426
    if(!zero)goto L949a426;goto L9499f18;
   L9499f18: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L9499f1c;
   L9499f1c: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499f1f;
   L9499f1f: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499f25;
   L9499f25: // JZ 0x09499d07
    if(zero)goto L9499d07;goto L9499f2b;
   L9499f2b: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L9499f30;
   L9499f30: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499f32;
   L9499f32: // JZ 0x0949a50b
    if(zero)goto L949a50b;goto L9499f38;
   L9499f38: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L9499f3d;
   L9499f3d: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9499f42;
   L9499f42: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499f44;
   L9499f44: // JG 0x09499d07
    if(!zero&&sign==overflow)goto L9499d07;goto L9499f4a;
   L9499f4a: // LEA EAX,[ESP + 0x81]
    eax=unchecked(esp+129u);goto L9499f51;
   L9499f51: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9499f54;
   L9499f54: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499f58;
   L9499f58: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9499f5d;
   L9499f5d: // MOV EDX,dword ptr [ESP + 0x3c]
    edx=R(unchecked(esp+60u));goto L9499f61;
   L9499f61: // JMP 0x094999db
    goto L94999db;
   L9499f68: // MOV EAX,dword ptr [ESP + 0x68]
    eax=R(unchecked(esp+104u));goto L9499f6c;
   L9499f6c: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499f6f;
   L9499f6f: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499f75;
   L9499f75: // JNZ 0x0949a6ee
    if(!zero)goto L949a6ee;goto L9499f7b;
   L9499f7b: // MOV EAX,dword ptr [ESP + 0x64]
    eax=R(unchecked(esp+100u));goto L9499f7f;
   L9499f7f: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499f82;
   L9499f82: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499f88;
   L9499f88: // JNZ 0x0949a769
    if(!zero)goto L949a769;goto L9499f8e;
   L9499f8e: // MOV ESI,dword ptr [ESP + 0x58]
    esi=R(unchecked(esp+88u));goto L9499f92;
   L9499f92: // MOV EBX,dword ptr [ESP + 0x54]
    ebx=R(unchecked(esp+84u));goto L9499f96;
   L9499f96: // CMP ESI,EBX
    a=esi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499f98;
   L9499f98: // JZ 0x09499fc4
    if(zero)goto L9499fc4;goto L9499f9a;
   L9499f9a: // MOV EAX,0x80b0f04
    eax=134942468u;goto L9499f9f;
   L9499f9f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499fa1;
   L9499fa1: // JZ 0x0949a606
    if(zero)goto L949a606;goto L9499fa7;
   L9499fa7: // NOP
    goto L9499fa8;
   L9499fa8: // MOV EAX,dword ptr [EBX]
    eax=R(unchecked(ebx));goto L9499faa;
   L9499faa: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9499fad;
   L9499fad: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499fb3;
   L9499fb3: // JNZ 0x0949a270
    if(!zero)goto L949a270;goto L9499fb9;
   L9499fb9: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499fbc;
   L9499fbc: // CMP ESI,EBX
    a=esi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499fbe;
   L9499fbe: // JNZ 0x09499fa8
    if(!zero)goto L9499fa8;goto L9499fc0;
   L9499fc0: // MOV EBX,dword ptr [ESP + 0x54]
    ebx=R(unchecked(esp+84u));goto L9499fc4;
   L9499fc4: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499fc6;
   L9499fc6: // JZ 0x09499fd0
    if(zero)goto L9499fd0;goto L9499fc8;
   L9499fc8: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9499fcb;
   L9499fcb: // CALL 0x080aabe4
    eax=Call(0x80aabe4,R(unchecked(esp+0u)));goto L9499fd0;
   L9499fd0:return 0;
   L9499fdd: // MOV EAX,0x1
    eax=1u;goto L9499fe2;
   L9499fe2: // SUB EAX,dword ptr [ESP + 0x18]
    a=eax;b=R(unchecked(esp+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L9499fe6;
   L9499fe6: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9499fea;
   L9499fea: // JZ 0x0949a0cc
    if(zero)goto L949a0cc;goto L9499ff0;
   L9499ff0: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9499ff4;
   L9499ff4: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9499ff6;
   L9499ff6: // JZ 0x0949a045
    if(zero)goto L949a045;goto L9499ff8;
   L9499ff8: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L9499ffc;
   L9499ffc: // LEA EAX,[ESP + 0x44]
    eax=unchecked(esp+68u);goto L949a000;
   L949a000: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a004;
   L949a004: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a007;
   L949a007: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a73f;}goto L949a00c;
   L949a00c: // MOV EBX,dword ptr [ESP + 0x14]
    ebx=R(unchecked(esp+20u));goto L949a010;
   L949a010: // LEA ECX,[ESP + 0x48]
    ecx=unchecked(esp+72u);goto L949a014;
   L949a014: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L949a018;
   L949a018: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L949a01b;
   L949a01b: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a01e;
   L949a01e: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a372;}goto L949a023;
   L949a023: // LEA EAX,[ESP + 0x4c]
    eax=unchecked(esp+76u);goto L949a027;
   L949a027: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a02b;
   L949a02b: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L949a02f;
   L949a02f: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L949a032;
   L949a032: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a035;
   L949a035: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a745;}goto L949a03a;
   L949a03a: // MOV EAX,dword ptr [ESP + 0x50]
    eax=R(unchecked(esp+80u));goto L949a03e;
   L949a03e: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L949a042;
   L949a042: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L949a045;
   L949a045: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L949a048;
   L949a048: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L949a04c;
   L949a04c: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L949a050;
   L949a050: // SHL ECX,0x4
    a=ecx;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;ecx=v;goto L949a053;
   L949a053: // ADD ECX,EAX
    a=ecx;b=eax;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L949a055;
   L949a055: // MOV EAX,EDI
    eax=edi;goto L949a057;
   L949a057: // MOV dword ptr [EBP + 0x8],ECX
    W(unchecked(ebp+8u),ecx);goto L949a05a;
   L949a05a: // CALL 0x09499720
    try{eax=Call(0x9499720,eax,edx,ecx);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L949a05f;
   L949a05f: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L949a063;
   L949a063: // SHL EAX,0x4
    a=eax;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;eax=v;goto L949a066;
   L949a066: // ADD dword ptr [EBP + 0x8],EAX
    a=R(unchecked(ebp+8u));b=eax;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(ebp+8u),v);goto L949a069;
   L949a069: // CMP EDI,dword ptr [ESP + 0x14]
    a=edi;b=R(unchecked(esp+20u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a06d;
   L949a06d: // JZ 0x09499ef2
    if(zero)goto L9499ef2;goto L949a073;
   L949a073: // NOP
    goto L949a074;
   L949a074: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L949a078;
   L949a078: // LEA EAX,[ESP + 0x44]
    eax=unchecked(esp+68u);goto L949a07c;
   L949a07c: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a080;
   L949a080: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L949a083;
   L949a083: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L949a088;
   L949a088: // LEA EDX,[ESP + 0x48]
    edx=unchecked(esp+72u);goto L949a08c;
   L949a08c: // LEA EAX,[EDI + 0x4]
    eax=unchecked(edi+4u);goto L949a08f;
   L949a08f: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L949a093;
   L949a093: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a096;
   L949a096: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L949a09b;
   L949a09b: // LEA ECX,[ESP + 0x4c]
    ecx=unchecked(esp+76u);goto L949a09f;
   L949a09f: // LEA EAX,[EDI + 0x8]
    eax=unchecked(edi+8u);goto L949a0a2;
   L949a0a2: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L949a0a6;
   L949a0a6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a0a9;
   L949a0a9: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a525;}goto L949a0ae;
   L949a0ae: // MOV EAX,dword ptr [ESP + 0x50]
    eax=R(unchecked(esp+80u));goto L949a0b2;
   L949a0b2: // MOV dword ptr [EDI + 0xc],EAX
    W(unchecked(edi+12u),eax);goto L949a0b5;
   L949a0b5: // ADD EDI,0x10
    a=edi;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L949a0b8;
   L949a0b8: // CMP dword ptr [ESP + 0x14],EDI
    a=R(unchecked(esp+20u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a0bc;
   L949a0bc: // JNZ 0x0949a078
    if(!zero)goto L949a078;goto L949a0be;
   L949a0be: // JMP 0x09499ef2
    goto L9499ef2;
   L949a0c3: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L949a0c7;
   L949a0c7: // JMP 0x09499c69
    goto L9499c69;
   L949a0cc: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L949a0d0;
   L949a0d0: // MOV EDX,EAX
    edx=eax;goto L949a0d2;
   L949a0d2: // JMP 0x0949a04c
    goto L949a04c;
   L949a0d7: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a0dc;
   L949a0dc: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a0de;
   L949a0de: // JZ 0x0949a30b
    if(zero)goto L949a30b;goto L949a0e4;
   L949a0e4: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a0e9;
   L949a0e9: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a0ee;
   L949a0ee: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a0f0;
   L949a0f0: // JG 0x09499d66
    if(!zero&&sign==overflow)goto L9499d66;goto L949a0f6;
   L949a0f6: // LEA ECX,[ESP + 0x86]
    ecx=unchecked(esp+134u);goto L949a0fd;
   L949a0fd: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L949a101;
   L949a101: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a104;
   L949a104: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a109;
   L949a109: // JMP 0x09499d66
    goto L9499d66;
   L949a10e: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a113;
   L949a113: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a115;
   L949a115: // JZ 0x0949a319
    if(zero)goto L949a319;goto L949a11b;
   L949a11b: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a120;
   L949a120: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a125;
   L949a125: // MOV dword ptr [ESP + 0x20],ECX
    W(unchecked(esp+32u),ecx);goto L949a129;
   L949a129: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L949a12d;
   L949a12d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a12f;
   L949a12f: // JG 0x09499d50
    if(!zero&&sign==overflow)goto L9499d50;goto L949a135;
   L949a135: // LEA EAX,[ESP + 0x85]
    eax=unchecked(esp+133u);goto L949a13c;
   L949a13c: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a140;
   L949a140: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a143;
   L949a143: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a148;
   L949a148: // JMP 0x09499d50
    goto L9499d50;
   L949a14d: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a152;
   L949a152: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a154;
   L949a154: // JZ 0x0949a6c7
    if(zero)goto L949a6c7;goto L949a15a;
   L949a15a: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a15f;
   L949a15f: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a164;
   L949a164: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a166;
   L949a166: // JG 0x09499d77
    if(!zero&&sign==overflow)goto L9499d77;goto L949a16c;
   L949a16c: // LEA EAX,[ESP + 0x87]
    eax=unchecked(esp+135u);goto L949a173;
   L949a173: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a177;
   L949a177: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a17a;
   L949a17a: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a17f;
   L949a17f: // JMP 0x09499d77
    goto L9499d77;
   L949a184: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a189;
   L949a189: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a18b;
   L949a18b: // JZ 0x0949a336
    if(zero)goto L949a336;goto L949a191;
   L949a191: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a196;
   L949a196: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a19b;
   L949a19b: // MOV dword ptr [ESP + 0x28],ECX
    W(unchecked(esp+40u),ecx);goto L949a19f;
   L949a19f: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L949a1a3;
   L949a1a3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a1a5;
   L949a1a5: // JG 0x09499ca0
    if(!zero&&sign==overflow)goto L9499ca0;goto L949a1ab;
   L949a1ab: // LEA EAX,[ESP + 0x82]
    eax=unchecked(esp+130u);goto L949a1b2;
   L949a1b2: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a1b6;
   L949a1b6: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a1b9;
   L949a1b9: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a1be;
   L949a1be: // JMP 0x09499ca0
    goto L9499ca0;
   L949a1c8: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a1cd;
   L949a1cd: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a1cf;
   L949a1cf: // JZ 0x0949a348
    if(zero)goto L949a348;goto L949a1d5;
   L949a1d5: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a1da;
   L949a1da: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a1df;
   L949a1df: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a1e1;
   L949a1e1: // JG 0x09499cc3
    if(!zero&&sign==overflow)goto L9499cc3;goto L949a1e7;
   L949a1e7: // LEA EAX,[ESP + 0x84]
    eax=unchecked(esp+132u);goto L949a1ee;
   L949a1ee: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a1f2;
   L949a1f2: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a1f5;
   L949a1f5: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a1fa;
   L949a1fa: // JMP 0x09499cc3
    goto L9499cc3;
   L949a200: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a205;
   L949a205: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a207;
   L949a207: // JZ 0x0949a356
    if(zero)goto L949a356;goto L949a20d;
   L949a20d: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a212;
   L949a212: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a217;
   L949a217: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a219;
   L949a219: // JG 0x09499cb2
    if(!zero&&sign==overflow)goto L9499cb2;goto L949a21f;
   L949a21f: // LEA ECX,[ESP + 0x83]
    ecx=unchecked(esp+131u);goto L949a226;
   L949a226: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L949a22a;
   L949a22a: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a22d;
   L949a22d: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a232;
   L949a232: // JMP 0x09499cb2
    goto L9499cb2;
   L949a238: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a23d;
   L949a23d: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a23f;
   L949a23f: // JZ 0x0949a364
    if(zero)goto L949a364;goto L949a245;
   L949a245: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a24a;
   L949a24a: // XADD.LOCK dword ptr [EDX + -0x4],ECX
    a=R(unchecked(edx+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(edx+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a24f;
   L949a24f: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a251;
   L949a251: // JG 0x094999e9
    if(!zero&&sign==overflow)goto L94999e9;goto L949a257;
   L949a257: // LEA EDX,[ESP + 0x88]
    edx=unchecked(esp+136u);goto L949a25e;
   L949a25e: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L949a262;
   L949a262: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a265;
   L949a265: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a26a;
   L949a26a: // JMP 0x094999e9
    goto L94999e9;
   L949a270: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a275;
   L949a275: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a27a;
   L949a27a: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a27c;
   L949a27c: // JG 0x09499fb9
    if(!zero&&sign==overflow)goto L9499fb9;goto L949a282;
   L949a282: // LEA EAX,[ESP + 0x8f]
    eax=unchecked(esp+143u);goto L949a289;
   L949a289: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a28d;
   L949a28d: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a290;
   L949a290: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a295;
   L949a295: // JMP 0x09499fb9
    goto L9499fb9;
   L949a2a0: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a2a5;
   L949a2a5: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a2a7;
   L949a2a7: // JZ 0x0949a6d5
    if(zero)goto L949a6d5;goto L949a2ad;
   L949a2ad: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a2b2;
   L949a2b2: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a2b7;
   L949a2b7: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a2b9;
   L949a2b9: // JG 0x09499a0f
    if(!zero&&sign==overflow)goto L9499a0f;goto L949a2bf;
   L949a2bf: // LEA EAX,[ESP + 0x8a]
    eax=unchecked(esp+138u);goto L949a2c6;
   L949a2c6: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a2ca;
   L949a2ca: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a2cd;
   L949a2cd: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a2d2;
   L949a2d2: // JMP 0x09499a0f
    goto L9499a0f;
   L949a2d8: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a2dd;
   L949a2dd: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a2df;
   L949a2df: // JZ 0x0949a32b
    if(zero)goto L949a32b;goto L949a2e1;
   L949a2e1: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a2e6;
   L949a2e6: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a2eb;
   L949a2eb: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a2ed;
   L949a2ed: // JG 0x094999fc
    if(!zero&&sign==overflow)goto L94999fc;goto L949a2f3;
   L949a2f3: // LEA EAX,[ESP + 0x89]
    eax=unchecked(esp+137u);goto L949a2fa;
   L949a2fa: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a2fe;
   L949a2fe: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a301;
   L949a301: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a306;
   L949a306: // JMP 0x094999fc
    goto L94999fc;
   L949a30b: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a30e;
   L949a30e: // LEA ESI,[ECX + -0x1]
    esi=unchecked(ecx+4294967295u);goto L949a311;
   L949a311: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L949a314;
   L949a314: // JMP 0x0949a0ee
    goto L949a0ee;
   L949a319: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a31c;
   L949a31c: // MOV dword ptr [ESP + 0x20],ECX
    W(unchecked(esp+32u),ecx);goto L949a320;
   L949a320: // SUB ECX,0x1
    a=ecx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ecx=v;goto L949a323;
   L949a323: // MOV dword ptr [EAX + -0x4],ECX
    W(unchecked(eax+4294967292u),ecx);goto L949a326;
   L949a326: // JMP 0x0949a129
    goto L949a129;
   L949a32b: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a32e;
   L949a32e: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a331;
   L949a331: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a334;
   L949a334: // JMP 0x0949a2eb
    goto L949a2eb;
   L949a336: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a339;
   L949a339: // MOV dword ptr [ESP + 0x28],ECX
    W(unchecked(esp+40u),ecx);goto L949a33d;
   L949a33d: // SUB ECX,0x1
    a=ecx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ecx=v;goto L949a340;
   L949a340: // MOV dword ptr [EAX + -0x4],ECX
    W(unchecked(eax+4294967292u),ecx);goto L949a343;
   L949a343: // JMP 0x0949a19f
    goto L949a19f;
   L949a348: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a34b;
   L949a34b: // LEA ESI,[ECX + -0x1]
    esi=unchecked(ecx+4294967295u);goto L949a34e;
   L949a34e: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L949a351;
   L949a351: // JMP 0x0949a1df
    goto L949a1df;
   L949a356: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a359;
   L949a359: // LEA ESI,[ECX + -0x1]
    esi=unchecked(ecx+4294967295u);goto L949a35c;
   L949a35c: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L949a35f;
   L949a35f: // JMP 0x0949a217
    goto L949a217;
   L949a364: // MOV ECX,dword ptr [EDX + -0x4]
    ecx=R(unchecked(edx+4294967292u));goto L949a367;
   L949a367: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a36a;
   L949a36a: // MOV dword ptr [EDX + -0x4],EBX
    W(unchecked(edx+4294967292u),ebx);goto L949a36d;
   L949a36d: // JMP 0x0949a24f
    goto L949a24f;
   L949a372: // MOV EBX,EAX
    ebx=eax;goto L949a374;
   L949a374: // MOV ECX,dword ptr [ESP + 0x14]
    ecx=R(unchecked(esp+20u));goto L949a378;
   L949a378: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L949a37b;
   L949a37b: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a380;
   L949a380: // MOV EAX,EBX
    eax=ebx;goto L949a382;
   L949a382: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a385;
   L949a385: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a38a;
   L949a38a: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a39f;}
   L949a38f: // CMP EAX,0xfffffff
    a=eax;b=268435455u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a394;
   L949a394: // JA 0x09499da3
    if(!carry&&!zero)goto L9499da3;goto L949a39a;
   L949a39a: // JMP 0x09499b37
    goto L9499b37;
   L949a39f: // MOV EBX,EAX
    ebx=eax;goto L949a3a1;
   L949a3a1: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L949a3a6;
   L949a3a6: // LEA EDX,[ESP + 0x44]
    edx=unchecked(esp+68u);goto L949a3aa;
   L949a3aa: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a3ad;
   L949a3ad: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L949a3b2;
   L949a3b2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949a3b5;
   L949a3b5: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L949a3ba;
   L949a3ba: // LEA EDX,[ESP + 0x68]
    edx=unchecked(esp+104u);goto L949a3be;
   L949a3be: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a3c1;
   L949a3c1: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a3c6;
   L949a3c6: // LEA ECX,[ESP + 0x64]
    ecx=unchecked(esp+100u);goto L949a3ca;
   L949a3ca: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L949a3cd;
   L949a3cd: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a3d2;
   L949a3d2: // LEA EAX,[ESP + 0x54]
    eax=unchecked(esp+84u);goto L949a3d6;
   L949a3d6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a3d9;
   L949a3d9: // CALL 0x081a1a30
    eax=Call(0x81a1a30,R(unchecked(esp+0u)));goto L949a3de;
   L949a3de: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a3e1;
   L949a3e1: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L949a3e6: // MOV EBX,EAX
    ebx=eax;goto L949a3e8;
   L949a3e8: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L949a3eb;
   L949a3eb: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a3f0;
   L949a3f0: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949a3f3;
   L949a3f3: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a3f8;
   L949a3f8: // MOV EAX,EBX
    eax=ebx;goto L949a3fa;
   L949a3fa: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a3fd;
   L949a3fd: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a402;
   L949a402: // CMP dword ptr [ESP + 0x24],ESI
    a=R(unchecked(esp+36u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a406;
   L949a406: // JZ 0x0949a41b
    if(zero)goto L949a41b;goto L949a408;
   L949a408: // MOV EBX,dword ptr [ESP + 0x24]
    ebx=R(unchecked(esp+36u));goto L949a40c;
   L949a40c: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a40f;
   L949a40f: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L949a412;
   L949a412: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L949a417;
   L949a417: // CMP EBX,ESI
    a=ebx;b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a419;
   L949a419: // JNZ 0x0949a40c
    if(!zero)goto L949a40c;goto L949a41b;
   L949a41b: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a49f;}
   L949a420: // MOV EBX,EAX
    ebx=eax;goto L949a422;
   L949a422: // JMP 0x0949a3f0
    goto L949a3f0;
   L949a424: // JMP 0x0949a3fa
    goto L949a3fa;
   L949a426: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a42b;
   L949a42b: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a42d;
   L949a42d: // JZ 0x0949a489
    if(zero)goto L949a489;goto L949a42f;
   L949a42f: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a434;
   L949a434: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a439;
   L949a439: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a43b;
   L949a43b: // JG 0x09499f18
    if(!zero&&sign==overflow)goto L9499f18;goto L949a441;
   L949a441: // LEA EAX,[ESP + 0x80]
    eax=unchecked(esp+128u);goto L949a448;
   L949a448: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a44c;
   L949a44c: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a44f;
   L949a44f: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a454;
   L949a454: // JMP 0x09499f18
    goto L9499f18;
   L949a459: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a45e;
   L949a45e: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a460;
   L949a460: // JZ 0x0949a494
    if(zero)goto L949a494;goto L949a462;
   L949a462: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a467;
   L949a467: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a46c;
   L949a46c: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a46e;
   L949a46e: // JG 0x09499f05
    if(!zero&&sign==overflow)goto L9499f05;goto L949a474;
   L949a474: // LEA EAX,[ESP + 0x7f]
    eax=unchecked(esp+127u);goto L949a478;
   L949a478: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a47c;
   L949a47c: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a47f;
   L949a47f: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a484;
   L949a484: // JMP 0x09499f05
    goto L9499f05;
   L949a489: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a48c;
   L949a48c: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a48f;
   L949a48f: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a492;
   L949a492: // JMP 0x0949a439
    goto L949a439;
   L949a494: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a497;
   L949a497: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a49a;
   L949a49a: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a49d;
   L949a49d: // JMP 0x0949a46c
    goto L949a46c;
   L949a49f: // MOV EBX,EAX
    ebx=eax;goto L949a4a1;
   L949a4a1: // LEA ESI,[ESP + 0x34]
    esi=unchecked(esp+52u);goto L949a4a5;
   L949a4a5: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L949a4aa;
   L949a4aa: // MOV EAX,EBX
    eax=ebx;goto L949a4ac;
   L949a4ac: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a4af;
   L949a4af: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a4b4;
   L949a4b4: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L949a4b8;
   L949a4b8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a4ba;
   L949a4ba: // JZ 0x0949a4f5
    if(zero)goto L949a4f5;goto L949a4bc;
   L949a4bc: // MOV EDX,dword ptr [ESP + 0x24]
    edx=R(unchecked(esp+36u));goto L949a4c0;
   L949a4c0: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L949a4c4;
   L949a4c4: // CALL 0x09498d90
    eax=Call(0x9498d90,eax,edx);goto L949a4c9;
   L949a4c9: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L949a4cd;
   L949a4cd: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a4cf;
   L949a4cf: // JZ 0x0949a4dd
    if(zero)goto L949a4dd;goto L949a4d1;
   L949a4d1: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L949a4d5;
   L949a4d5: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a4d8;
   L949a4d8: // CALL 0x080aabe4
    eax=Call(0x80aabe4,R(unchecked(esp+0u)));goto L949a4dd;
   L949a4dd: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a519;}
   L949a4e2: // MOV EBX,EAX
    ebx=eax;goto L949a4e4;
   L949a4e4: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L949a4e9;
   L949a4e9: // LEA ESI,[ESP + 0x34]
    esi=unchecked(esp+52u);goto L949a4ed;
   L949a4ed: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a4f0;
   L949a4f0: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a4f5;
   L949a4f5: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L949a4f9;
   L949a4f9: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L949a4fd;
   L949a4fd: // SHL EDX,0x4
    a=edx;v=unchecked(a<<4);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&268435456u)!=0;overflow=false;edx=v;goto L949a500;
   L949a500: // ADD EDX,dword ptr [ESP + 0x1c]
    a=edx;b=R(unchecked(esp+28u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L949a504;
   L949a504: // CALL 0x09498d90
    eax=Call(0x9498d90,eax,edx);goto L949a509;
   L949a509: // JMP 0x0949a4c9
    goto L949a4c9;
   L949a50b: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a50e;
   L949a50e: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a511;
   L949a511: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a514;
   L949a514: // JMP 0x09499f42
    goto L9499f42;
   L949a519: // MOV EBX,EAX
    ebx=eax;goto L949a51b;
   L949a51b: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L949a520;
   L949a520: // JMP 0x0949a3b2
    goto L949a3b2;
   L949a525: // MOV EBX,EAX
    ebx=eax;goto L949a527;
   L949a527: // JMP 0x0949a3a6
    goto L949a3a6;
   L949a52c: // MOV EBX,EAX
    ebx=eax;goto L949a52e;
   L949a52e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949a531;
   L949a531: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a536;
   L949a536: // MOV EAX,EBX
    eax=ebx;goto L949a538;
   L949a538: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a53b;
   L949a53b: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a540;
   L949a540: // CMP dword ptr [ESP + 0x1c],ESI
    a=R(unchecked(esp+28u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a544;
   L949a544: // JZ 0x0949a559
    if(zero)goto L949a559;goto L949a546;
   L949a546: // MOV EBX,dword ptr [ESP + 0x1c]
    ebx=R(unchecked(esp+28u));goto L949a54a;
   L949a54a: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a54d;
   L949a54d: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L949a550;
   L949a550: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L949a555;
   L949a555: // CMP EBX,ESI
    a=ebx;b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a557;
   L949a557: // JNZ 0x0949a54a
    if(!zero)goto L949a54a;goto L949a559;
   L949a559: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a4e2;}
   L949a55e: // NOP
    goto L949a560;
   L949a560: // JMP 0x0949a538
    goto L949a538;
   L949a562: // MOV EDI,EAX
    edi=eax;goto L949a564;
   L949a564: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a567;
   L949a567: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a56c;
   L949a56c: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L949a570;
   L949a570: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a573;
   L949a573: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a578;
   L949a578: // MOV EAX,EDI
    eax=edi;goto L949a57a;
   L949a57a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a57d;
   L949a57d: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949a582;
   L949a582: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L949a587;}
   L949a587: // MOV EBX,EAX
    ebx=eax;goto L949a589;
   L949a589: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L949a58e;
   L949a58e: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L949a592;
   L949a592: // MOV EAX,EBX
    eax=ebx;goto L949a594;
   L949a594: // MOV dword ptr [ESP + 0x24],EDX
    W(unchecked(esp+36u),edx);goto L949a598;
   L949a598: // JMP 0x0949a4ac
    goto L949a4ac;
   L949a59d: // MOV EDI,EAX
    edi=eax;goto L949a59f;
   L949a59f: // JMP 0x0949a56c
    goto L949a56c;
   L949a5a1: // JMP 0x0949a57a
    goto L949a57a;
   L949a5a3: // MOV EBX,EAX
    ebx=eax;goto L949a5a5;
   L949a5a5: // JMP 0x0949a3b2
    goto L949a3b2;
   L949a5aa: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a5af;
   L949a5af: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a5b1;
   L949a5b1: // JZ 0x0949a7ac
    if(zero)goto L949a7ac;goto L949a5b7;
   L949a5b7: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a5bc;
   L949a5bc: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a5c1;
   L949a5c1: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a5c3;
   L949a5c3: // JG 0x09499aa8
    if(!zero&&sign==overflow)goto L9499aa8;goto L949a5c9;
   L949a5c9: // LEA EAX,[ESP + 0x8c]
    eax=unchecked(esp+140u);goto L949a5d0;
   L949a5d0: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a5d4;
   L949a5d4: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a5d7;
   L949a5d7: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a5dc;
   L949a5dc: // MOV EAX,dword ptr [ESP + 0x70]
    eax=R(unchecked(esp+112u));goto L949a5e0;
   L949a5e0: // JMP 0x09499aac
    goto L9499aac;
   L949a5e5: // MOV ESI,EAX
    esi=eax;goto L949a5e7;
   L949a5e7: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a5ea;
   L949a5ea: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a5ef;
   L949a5ef: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949a5f2;
   L949a5f2: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L949a5f7: // MOV EBX,EAX
    ebx=eax;goto L949a5f9;
   L949a5f9: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L949a5fc;
   L949a5fc: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a601;
   L949a601: // JMP 0x0949a3ba
    goto L949a3ba;
   L949a606: // LEA EDI,[ESP + 0x8f]
    edi=unchecked(esp+143u);goto L949a60d;
   L949a60d: // MOV EAX,dword ptr [EBX]
    eax=R(unchecked(ebx));goto L949a60f;
   L949a60f: // LEA ECX,[EAX + -0xc]
    ecx=unchecked(eax+4294967284u);goto L949a612;
   L949a612: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a618;
   L949a618: // JNZ 0x0949a65d
    if(!zero)goto L949a65d;goto L949a61a;
   L949a61a: // ADD EBX,0x4
    a=ebx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L949a61d;
   L949a61d: // CMP ESI,EBX
    a=esi;b=ebx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949a61f;
   L949a61f: // JNZ 0x0949a60d
    if(!zero)goto L949a60d;goto L949a621;
   L949a621: // JMP 0x09499fc0
    goto L9499fc0;
   L949a626: // MOV EBX,EAX
    ebx=eax;goto L949a628;
   L949a628: // JMP 0x0949a3ba
    goto L949a3ba;
   L949a62d: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a632;
   L949a632: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a634;
   L949a634: // JZ 0x0949a678
    if(zero)goto L949a678;goto L949a636;
   L949a636: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a63b;
   L949a63b: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a640;
   L949a640: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a642;
   L949a642: // JG 0x094999b2
    if(!zero&&sign==overflow)goto L94999b2;goto L949a648;
   L949a648: // LEA EAX,[ESP + 0x7e]
    eax=unchecked(esp+126u);goto L949a64c;
   L949a64c: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a650;
   L949a650: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a653;
   L949a653: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a658;
   L949a658: // JMP 0x094999b2
    goto L94999b2;
   L949a65d: // MOV EDX,dword ptr [EAX + -0x4]
    edx=R(unchecked(eax+4294967292u));goto L949a660;
   L949a660: // LEA EBP,[EDX + -0x1]
    ebp=unchecked(edx+4294967295u);goto L949a663;
   L949a663: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a665;
   L949a665: // MOV dword ptr [EAX + -0x4],EBP
    W(unchecked(eax+4294967292u),ebp);goto L949a668;
   L949a668: // JG 0x0949a61a
    if(!zero&&sign==overflow)goto L949a61a;goto L949a66a;
   L949a66a: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L949a66e;
   L949a66e: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L949a671;
   L949a671: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a676;
   L949a676: // JMP 0x0949a61a
    goto L949a61a;
   L949a678: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a67b;
   L949a67b: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a67e;
   L949a67e: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a681;
   L949a681: // JMP 0x0949a640
    goto L949a640;
   L949a683: // MOV EDI,EAX
    edi=eax;goto L949a685;
   L949a685: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a688;
   L949a688: // MOV EBX,EDI
    ebx=edi;goto L949a68a;
   L949a68a: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a68f;
   L949a68f: // JMP 0x0949a5f9
    goto L949a5f9;
   L949a694: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a699;
   L949a699: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a69b;
   L949a69b: // JZ 0x0949a6e3
    if(zero)goto L949a6e3;goto L949a69d;
   L949a69d: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a6a2;
   L949a6a2: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a6a7;
   L949a6a7: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a6a9;
   L949a6a9: // JG 0x09499a90
    if(!zero&&sign==overflow)goto L9499a90;goto L949a6af;
   L949a6af: // LEA EAX,[ESP + 0x8b]
    eax=unchecked(esp+139u);goto L949a6b6;
   L949a6b6: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a6ba;
   L949a6ba: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a6bd;
   L949a6bd: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a6c2;
   L949a6c2: // JMP 0x09499a90
    goto L9499a90;
   L949a6c7: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a6ca;
   L949a6ca: // LEA ESI,[ECX + -0x1]
    esi=unchecked(ecx+4294967295u);goto L949a6cd;
   L949a6cd: // MOV dword ptr [EAX + -0x4],ESI
    W(unchecked(eax+4294967292u),esi);goto L949a6d0;
   L949a6d0: // JMP 0x0949a164
    goto L949a164;
   L949a6d5: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a6d8;
   L949a6d8: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a6db;
   L949a6db: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a6de;
   L949a6de: // JMP 0x0949a2b7
    goto L949a2b7;
   L949a6e3: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a6e6;
   L949a6e6: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a6e9;
   L949a6e9: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a6ec;
   L949a6ec: // JMP 0x0949a6a7
    goto L949a6a7;
   L949a6ee: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a6f3;
   L949a6f3: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a6f5;
   L949a6f5: // JZ 0x0949a734
    if(zero)goto L949a734;goto L949a6f7;
   L949a6f7: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a6fc;
   L949a6fc: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a701;
   L949a701: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a703;
   L949a703: // JG 0x09499f7b
    if(!zero&&sign==overflow)goto L9499f7b;goto L949a709;
   L949a709: // LEA EAX,[ESP + 0x8d]
    eax=unchecked(esp+141u);goto L949a710;
   L949a710: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a714;
   L949a714: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a717;
   L949a717: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a71c;
   L949a71c: // JMP 0x09499f7b
    goto L9499f7b;
   L949a721: // MOV EBX,EAX
    ebx=eax;goto L949a723;
   L949a723: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L949a727;
   L949a727: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a72a;
   L949a72a: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a72f;
   L949a72f: // JMP 0x0949a52e
    goto L949a52e;
   L949a734: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a737;
   L949a737: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a73a;
   L949a73a: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a73d;
   L949a73d: // JMP 0x0949a701
    goto L949a701;
   L949a73f: // NOP
    goto L949a740;
   L949a740: // JMP 0x0949a382
    goto L949a382;
   L949a745: // MOV EDI,EAX
    edi=eax;goto L949a747;
   L949a747: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L949a74a;
   L949a74a: // MOV EBX,EDI
    ebx=edi;goto L949a74c;
   L949a74c: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a751;
   L949a751: // JMP 0x0949a374
    goto L949a374;
   L949a756: // MOV EBX,EAX
    ebx=eax;goto L949a758;
   L949a758: // LEA EDX,[ESP + 0x44]
    edx=unchecked(esp+68u);goto L949a75c;
   L949a75c: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a75f;
   L949a75f: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a764;
   L949a764: // JMP 0x0949a3b2
    goto L949a3b2;
   L949a769: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L949a76e;
   L949a76e: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a770;
   L949a770: // JZ 0x0949a7ba
    if(zero)goto L949a7ba;goto L949a772;
   L949a772: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L949a777;
   L949a777: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L949a77c;
   L949a77c: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949a77e;
   L949a77e: // JG 0x09499f8e
    if(!zero&&sign==overflow)goto L9499f8e;goto L949a784;
   L949a784: // LEA EAX,[ESP + 0x8e]
    eax=unchecked(esp+142u);goto L949a78b;
   L949a78b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L949a78f;
   L949a78f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L949a792;
   L949a792: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L949a797;
   L949a797: // JMP 0x09499f8e
    goto L9499f8e;
   L949a79c: // MOV EBX,EAX
    ebx=eax;goto L949a79e;
   L949a79e: // LEA EAX,[ESP + 0x48]
    eax=unchecked(esp+72u);goto L949a7a2;
   L949a7a2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949a7a5;
   L949a7a5: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L949a7aa;
   L949a7aa: // JMP 0x0949a758
    goto L949a758;
   L949a7ac: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a7af;
   L949a7af: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a7b2;
   L949a7b2: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a7b5;
   L949a7b5: // JMP 0x0949a5c1
    goto L949a5c1;
   L949a7ba: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L949a7bd;
   L949a7bd: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L949a7c0;
   L949a7c0: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L949a7c3;
   L949a7c3: // JMP 0x0949a77c
    goto L949a77c;
  }
 }
}
