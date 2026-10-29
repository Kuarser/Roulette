using System;
namespace RouletteDeluxe
{
 // Physical ParseXMLData constprop.183 at 094988c0. Static C# labels
 // preserve original field control and COW writes and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; value/order pointers are at +60/+64.
 public sealed class OriginalConfigurationXmlFieldParse
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
  public OriginalConfigurationXmlFieldParse(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  void WB(uint at,byte value){h.WriteByte(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Parse(uint frame,uint tokens,uint iterator,uint name)
  {
   uint eax=tokens,ebx=0,esi=iterator,edi=0,ecx=name,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L94988c9;
   L94988c9: // MOV dword ptr [ESP + 0x2c],ECX
    W(unchecked(esp+44u),ecx);goto L94988cd;
   L94988cd: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94988d0;
   L94988d0: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L94988d4;
   L94988d4: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L94988dc;
   L94988dc: // MOV dword ptr [ESP + 0x4],0x96c5b87
    W(unchecked(esp+4u),158096263u);goto L94988e4;
   L94988e4: // CALL 0x080adb74
    eax=Call(0x80adb74,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94988e9;
   L94988e9: // MOV EDX,dword ptr [ESP + 0x60]
    edx=R(unchecked(esp+96u));goto L94988ed;
   L94988ed: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L94988f5;
   L94988f5: // MOV dword ptr [ESP + 0x4],0x96c5b87
    W(unchecked(esp+4u),158096263u);goto L94988fd;
   L94988fd: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498900;
   L9498900: // CALL 0x080adb74
    eax=Call(0x80adb74,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L9498905;
   L9498905: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L9498909;
   L9498909: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L949890b;
   L949890b: // MOV dword ptr [ESP + 0x30],0x9b482ac
    W(unchecked(esp+48u),162824876u);goto L9498913;
   L9498913: // MOV dword ptr [ESP + 0x34],0x9b482ac
    W(unchecked(esp+52u),162824876u);goto L949891b;
   L949891b: // CMP dword ptr [EDX + 0x4],EAX
    a=R(unchecked(edx+4u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949891e;
   L949891e: // MOV dword ptr [ESP + 0x38],0x9b482ac
    W(unchecked(esp+56u),162824876u);goto L9498926;
   L9498926: // JZ 0x09498c9f
    if(zero)goto L9498c9f;goto L949892c;
   L949892c: // MOV byte ptr [ESP + 0x27],0x0
    WB(unchecked(esp+39u),unchecked((byte)(0u)));goto L9498931;
   L9498931: // MOV dword ptr [ESP + 0x28],0x0
    W(unchecked(esp+40u),0u);goto L9498939;
   L9498939: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L9498941;
   L9498941: // MOV byte ptr [ESP + 0x26],0x1
    WB(unchecked(esp+38u),unchecked((byte)(1u)));goto L9498946;
   L9498946: // JMP 0x09498997
    goto L9498997;
   L9498948: // MOV dword ptr [ESP + 0xc],0x4
    W(unchecked(esp+12u),4u);goto L9498950;
   L9498950: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498954;
   L9498954: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L949895c;
   L949895c: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498960;
   L9498960: // MOV dword ptr [ESP + 0x4],0x96def3b
    W(unchecked(esp+4u),158199611u);goto L9498968;
   L9498968: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L949896a;
   L949896a: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L949896e;
   L949896e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498971;
   L9498971: // CALL 0x080afb44
    try{eax=Call(0x80afb44,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498976;
   L9498976: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498979;
   L9498979: // JZ 0x09498aa0
    if(zero)goto L9498aa0;goto L949897f;
   L949897f: // MOV dword ptr [ESP + 0x1c],0x3
    W(unchecked(esp+28u),3u);goto L9498987;
   L9498987: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498989;
   L9498989: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L949898d;
   L949898d: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9498990;
   L9498990: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9498992;
   L9498992: // CMP EAX,dword ptr [EDX + 0x4]
    a=eax;b=R(unchecked(edx+4u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498995;
   L9498995: // JZ 0x09498a04
    if(zero)goto L9498a04;goto L9498997;
   L9498997: // MOV dword ptr [ESP + 0x8],0xffffffff
    W(unchecked(esp+8u),4294967295u);goto L949899f;
   L949899f: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L94989a3;
   L94989a3: // MOV dword ptr [ESP + 0x4],0x3d
    W(unchecked(esp+4u),61u);goto L94989ab;
   L94989ab: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L94989af;
   L94989af: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94989b2;
   L94989b2: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L94989b6;
   L94989b6: // CALL 0x080ac3e4
    try{eax=Call(0x80ac3e4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L94989bb;
   L94989bb: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94989be;
   L94989be: // JNZ 0x09498948
    if(!zero)goto L9498948;goto L94989c0;
   L94989c0: // CMP byte ptr [ESP + 0x26],0x0
    a=B(unchecked(esp+38u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94989c5;
   L94989c5: // JNZ 0x09498a50
    if(!zero)goto L9498a50;goto L94989cb;
   L94989cb: // CMP dword ptr [ESP + 0x1c],0x3
    a=R(unchecked(esp+28u));b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94989d0;
   L94989d0: // JZ 0x09498ae0
    if(zero)goto L9498ae0;goto L94989d6;
   L94989d6: // CMP dword ptr [ESP + 0x1c],0x4
    a=R(unchecked(esp+28u));b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94989db;
   L94989db: // JZ 0x09498b8b
    if(zero)goto L9498b8b;goto L94989e1;
   L94989e1: // CMP dword ptr [ESP + 0x1c],0x5
    a=R(unchecked(esp+28u));b=5u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94989e6;
   L94989e6: // JZ 0x09498b00
    if(zero)goto L9498b00;goto L94989ec;
   L94989ec: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L94989ee;
   L94989ee: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L94989f2;
   L94989f2: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L94989fa;
   L94989fa: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L94989fd;
   L94989fd: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L94989ff;
   L94989ff: // CMP EAX,dword ptr [EDX + 0x4]
    a=eax;b=R(unchecked(edx+4u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498a02;
   L9498a02: // JNZ 0x09498997
    if(!zero)goto L9498997;goto L9498a04;
   L9498a04: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9498a08;
   L9498a08: // MOV byte ptr [ESP + 0x27],0x0
    WB(unchecked(esp+39u),unchecked((byte)(0u)));goto L9498a0d;
   L9498a0d: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9498a10;
   L9498a10: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498a16;
   L9498a16: // JNZ 0x09498d46
    if(!zero)goto L9498d46;goto L9498a1c;
   L9498a1c: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9498a20;
   L9498a20: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9498a23;
   L9498a23: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498a29;
   L9498a29: // JNZ 0x09498cde
    if(!zero)goto L9498cde;goto L9498a2f;
   L9498a2f: // MOV EAX,dword ptr [ESP + 0x30]
    eax=R(unchecked(esp+48u));goto L9498a33;
   L9498a33: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9498a36;
   L9498a36: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498a3c;
   L9498a3c: // JNZ 0x09498cae
    if(!zero)goto L9498cae;goto L9498a42;
   L9498a42: // MOVZX EAX,byte ptr [ESP + 0x27]
    eax=B(unchecked(esp+39u));goto L9498a47;
   L9498a47:return eax;
   L9498a50: // MOV dword ptr [ESP + 0xc],0x8
    W(unchecked(esp+12u),8u);goto L9498a58;
   L9498a58: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498a5c;
   L9498a5c: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9498a64;
   L9498a64: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498a68;
   L9498a68: // MOV dword ptr [ESP + 0x4],0x9767d02
    W(unchecked(esp+4u),158760194u);goto L9498a70;
   L9498a70: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498a72;
   L9498a72: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498a76;
   L9498a76: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498a79;
   L9498a79: // CALL 0x080afb44
    try{eax=Call(0x80afb44,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498a7e;
   L9498a7e: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498a81;
   L9498a81: // JZ 0x094989cb
    if(zero)goto L94989cb;goto L9498a87;
   L9498a87: // MOV byte ptr [ESP + 0x26],0x0
    WB(unchecked(esp+38u),unchecked((byte)(0u)));goto L9498a8c;
   L9498a8c: // MOV dword ptr [ESP + 0x1c],0x1
    W(unchecked(esp+28u),1u);goto L9498a94;
   L9498a94: // MOV byte ptr [ESP + 0x27],0x1
    WB(unchecked(esp+39u),unchecked((byte)(1u)));goto L9498a99;
   L9498a99: // JMP 0x09498987
    goto L9498987;
   L9498aa0: // MOV dword ptr [ESP + 0xc],0x5
    W(unchecked(esp+12u),5u);goto L9498aa8;
   L9498aa8: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498aac;
   L9498aac: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9498ab4;
   L9498ab4: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498ab8;
   L9498ab8: // MOV dword ptr [ESP + 0x4],0x9713005
    W(unchecked(esp+4u),158412805u);goto L9498ac0;
   L9498ac0: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498ac2;
   L9498ac2: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498ac6;
   L9498ac6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498ac9;
   L9498ac9: // CALL 0x080afb44
    try{eax=Call(0x80afb44,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498ace;
   L9498ace: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498ad1;
   L9498ad1: // JZ 0x09498b47
    if(zero)goto L9498b47;goto L9498ad3;
   L9498ad3: // MOV dword ptr [ESP + 0x1c],0x4
    W(unchecked(esp+28u),4u);goto L9498adb;
   L9498adb: // JMP 0x09498987
    goto L9498987;
   L9498ae0: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498ae2;
   L9498ae2: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498ae6;
   L9498ae6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9498ae9;
   L9498ae9: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498aed;
   L9498aed: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498af1;
   L9498af1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498af5;
   L9498af5: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498afa;
   L9498afa: // JMP 0x094989ec
    goto L94989ec;
   L9498b00: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498b02;
   L9498b02: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498b06;
   L9498b06: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9498b09;
   L9498b09: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498b0d;
   L9498b0d: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498b11;
   L9498b11: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498b15;
   L9498b15: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498b1a;
   L9498b1a: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9498b1e;
   L9498b1e: // MOV dword ptr [ESP + 0x8],0xa
    W(unchecked(esp+8u),10u);goto L9498b26;
   L9498b26: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9498b2e;
   L9498b2e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498b31;
   L9498b31: // CALL 0x080ad3a4
    try{eax=Call(0x80ad3a4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498b36;
   L9498b36: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L9498b3e;
   L9498b3e: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L9498b42;
   L9498b42: // JMP 0x09498987
    goto L9498987;
   L9498b47: // MOV dword ptr [ESP + 0xc],0x5
    W(unchecked(esp+12u),5u);goto L9498b4f;
   L9498b4f: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498b53;
   L9498b53: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9498b5b;
   L9498b5b: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498b5f;
   L9498b5f: // MOV dword ptr [ESP + 0x4],0x96d6380
    W(unchecked(esp+4u),158163840u);goto L9498b67;
   L9498b67: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498b69;
   L9498b69: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498b6d;
   L9498b6d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498b70;
   L9498b70: // CALL 0x080afb44
    try{eax=Call(0x80afb44,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498b75;
   L9498b75: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498b78;
   L9498b78: // JZ 0x09498987
    if(zero)goto L9498987;goto L9498b7e;
   L9498b7e: // MOV dword ptr [ESP + 0x1c],0x5
    W(unchecked(esp+28u),5u);goto L9498b86;
   L9498b86: // JMP 0x09498987
    goto L9498987;
   L9498b8b: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L9498b8d;
   L9498b8d: // LEA EBP,[ESP + 0x34]
    ebp=unchecked(esp+52u);goto L9498b91;
   L9498b91: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9498b94;
   L9498b94: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498b98;
   L9498b98: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498b9c;
   L9498b9c: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498ba0;
   L9498ba0: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498ba5;
   L9498ba5: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9498ba9;
   L9498ba9: // MOV EBX,dword ptr [EAX + -0xc]
    ebx=R(unchecked(eax+4294967284u));goto L9498bac;
   L9498bac: // TEST EBX,EBX
    a=ebx;b=ebx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498bae;
   L9498bae: // JZ 0x09498c2f
    if(zero)goto L9498c2f;goto L9498bb0;
   L9498bb0: // XOR ESI,ESI
    a=esi;b=esi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;esi=v;goto L9498bb2;
   L9498bb2: // JMP 0x09498bcc
    goto L9498bcc;
   L9498bb8: // CMP EDX,dword ptr [0x097686c8]
    a=edx;b=R(unchecked(158762696u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498bbe;
   L9498bbe: // JZ 0x09498c70
    if(zero)goto L9498c70;goto L9498bc4;
   L9498bc4: // ADD ESI,0x1
    a=esi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9498bc7;
   L9498bc7: // CMP ESI,dword ptr [EAX + -0xc]
    a=esi;b=R(unchecked(eax+4294967284u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498bca;
   L9498bca: // JZ 0x09498c2f
    if(zero)goto L9498c2f;goto L9498bcc;
   L9498bcc: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L9498bcf;
   L9498bcf: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498bd1;
   L9498bd1: // JS 0x09498be7
    if(sign)goto L9498be7;goto L9498bd3;
   L9498bd3: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9498bd6;
   L9498bd6: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498bda;
   L9498bda: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498bde;
   L9498bde: // CALL 0x080ae8a4
    try{eax=Call(0x80ae8a4,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498be3;
   L9498be3: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9498be7;
   L9498be7: // LEA ECX,[EAX + ESI*0x1]
    ecx=unchecked(eax+esi*0x1u);goto L9498bea;
   L9498bea: // MOVZX EDX,byte ptr [ECX]
    edx=B(unchecked(ecx));goto L9498bed;
   L9498bed: // CMP DL,0xfc
    a=(edx&255u);b=252u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498bf0;
   L9498bf0: // JBE 0x09498bc4
    if(carry||zero)goto L9498bc4;goto L9498bf2;
   L9498bf2: // MOVZX EDX,DL
    edx=(edx&255u);goto L9498bf5;
   L9498bf5: // CMP EDX,dword ptr [0x097686c0]
    a=edx;b=R(unchecked(158762688u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498bfb;
   L9498bfb: // JNZ 0x09498bb8
    if(!zero)goto L9498bb8;goto L9498bfd;
   L9498bfd: // MOV EDX,dword ptr [EAX + -0x4]
    edx=R(unchecked(eax+4294967292u));goto L9498c00;
   L9498c00: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498c02;
   L9498c02: // JS 0x09498c1a
    if(sign)goto L9498c1a;goto L9498c04;
   L9498c04: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9498c07;
   L9498c07: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498c0b;
   L9498c0b: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498c0f;
   L9498c0f: // CALL 0x080ae8a4
    try{eax=Call(0x80ae8a4,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498c14;
   L9498c14: // MOV ECX,dword ptr [ESP + 0x34]
    ecx=R(unchecked(esp+52u));goto L9498c18;
   L9498c18: // ADD ECX,ESI
    a=ecx;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L9498c1a;
   L9498c1a: // MOVZX EAX,byte ptr [0x097686c4]
    eax=B(unchecked(158762692u));goto L9498c21;
   L9498c21: // ADD ESI,0x1
    a=esi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9498c24;
   L9498c24: // MOV byte ptr [ECX],AL
    WB(unchecked(ecx),unchecked((byte)((eax&255u))));goto L9498c26;
   L9498c26: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9498c2a;
   L9498c2a: // CMP ESI,dword ptr [EAX + -0xc]
    a=esi;b=R(unchecked(eax+4294967284u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498c2d;
   L9498c2d: // JNZ 0x09498bcc
    if(!zero)goto L9498bcc;goto L9498c2f;
   L9498c2f: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L9498c33;
   L9498c33: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498c37;
   L9498c37: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L9498c3b;
   L9498c3b: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498c3f;
   L9498c3f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9498c42;
   L9498c42: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498c47;
   L9498c47: // MOV EDX,dword ptr [ESP + 0x60]
    edx=R(unchecked(esp+96u));goto L9498c4b;
   L9498c4b: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498c4f;
   L9498c4f: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L9498c53;
   L9498c53: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498c56;
   L9498c56: // CALL 0x080ab384
    try{eax=Call(0x80ab384,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498c5b;
   L9498c5b: // MOV EAX,dword ptr [ESP + 0x64]
    eax=R(unchecked(esp+100u));goto L9498c5f;
   L9498c5f: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto L9498c63;
   L9498c63: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L9498c65;
   L9498c65: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9498c69;
   L9498c69: // JMP 0x09498a0d
    goto L9498a0d;
   L9498c70: // MOV EAX,dword ptr [EAX + -0x4]
    eax=R(unchecked(eax+4294967292u));goto L9498c73;
   L9498c73: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498c75;
   L9498c75: // JS 0x09498c8d
    if(sign)goto L9498c8d;goto L9498c77;
   L9498c77: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9498c7a;
   L9498c7a: // LEA EDI,[ESP + 0x30]
    edi=unchecked(esp+48u);goto L9498c7e;
   L9498c7e: // LEA EBX,[ESP + 0x38]
    ebx=unchecked(esp+56u);goto L9498c82;
   L9498c82: // CALL 0x080ae8a4
    try{eax=Call(0x80ae8a4,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9498d24;}goto L9498c87;
   L9498c87: // MOV ECX,dword ptr [ESP + 0x34]
    ecx=R(unchecked(esp+52u));goto L9498c8b;
   L9498c8b: // ADD ECX,ESI
    a=ecx;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L9498c8d;
   L9498c8d: // MOVZX EDX,byte ptr [0x097686cc]
    edx=B(unchecked(158762700u));goto L9498c94;
   L9498c94: // MOV byte ptr [ECX],DL
    WB(unchecked(ecx),unchecked((byte)((edx&255u))));goto L9498c96;
   L9498c96: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9498c9a;
   L9498c9a: // JMP 0x09498bc4
    goto L9498bc4;
   L9498c9f: // MOV EAX,0x9b482ac
    eax=162824876u;goto L9498ca4;
   L9498ca4: // MOV byte ptr [ESP + 0x27],0x0
    WB(unchecked(esp+39u),unchecked((byte)(0u)));goto L9498ca9;
   L9498ca9: // JMP 0x09498a0d
    goto L9498a0d;
   L9498cae: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L9498cb3;
   L9498cb3: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498cb5;
   L9498cb5: // JZ 0x09498d0e
    if(zero)goto L9498d0e;goto L9498cb7;
   L9498cb7: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L9498cbc;
   L9498cbc: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498cc1;
   L9498cc1: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498cc3;
   L9498cc3: // JG 0x09498a42
    if(!zero&&sign==overflow)goto L9498a42;goto L9498cc9;
   L9498cc9: // LEA EAX,[ESP + 0x3f]
    eax=unchecked(esp+63u);goto L9498ccd;
   L9498ccd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498cd1;
   L9498cd1: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498cd4;
   L9498cd4: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498cd9;
   L9498cd9: // JMP 0x09498a42
    goto L9498a42;
   L9498cde: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L9498ce3;
   L9498ce3: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498ce5;
   L9498ce5: // JZ 0x09498d19
    if(zero)goto L9498d19;goto L9498ce7;
   L9498ce7: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L9498cec;
   L9498cec: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498cf1;
   L9498cf1: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498cf3;
   L9498cf3: // JG 0x09498a2f
    if(!zero&&sign==overflow)goto L9498a2f;goto L9498cf9;
   L9498cf9: // LEA EAX,[ESP + 0x3e]
    eax=unchecked(esp+62u);goto L9498cfd;
   L9498cfd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498d01;
   L9498d01: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498d04;
   L9498d04: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498d09;
   L9498d09: // JMP 0x09498a2f
    goto L9498a2f;
   L9498d0e: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L9498d11;
   L9498d11: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L9498d14;
   L9498d14: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L9498d17;
   L9498d17: // JMP 0x09498cc1
    goto L9498cc1;
   L9498d19: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L9498d1c;
   L9498d1c: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L9498d1f;
   L9498d1f: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L9498d22;
   L9498d22: // JMP 0x09498cf1
    goto L9498cf1;
   L9498d24: // MOV ESI,EAX
    esi=eax;goto L9498d26;
   L9498d26: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L9498d29;
   L9498d29: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L9498d2e;
   L9498d2e: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9498d31;
   L9498d31: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L9498d36;
   L9498d36: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9498d39;
   L9498d39: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L9498d3e;
   L9498d3e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9498d41;
   L9498d41: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L9498d46: // MOV ECX,0x80b0f04
    ecx=134942468u;goto L9498d4b;
   L9498d4b: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498d4d;
   L9498d4d: // JZ 0x09498d76
    if(zero)goto L9498d76;goto L9498d4f;
   L9498d4f: // MOV ECX,0xffffffff
    ecx=4294967295u;goto L9498d54;
   L9498d54: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498d59;
   L9498d59: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498d5b;
   L9498d5b: // JG 0x09498a1c
    if(!zero&&sign==overflow)goto L9498a1c;goto L9498d61;
   L9498d61: // LEA EAX,[ESP + 0x3d]
    eax=unchecked(esp+61u);goto L9498d65;
   L9498d65: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9498d69;
   L9498d69: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9498d6c;
   L9498d6c: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498d71;
   L9498d71: // JMP 0x09498a1c
    goto L9498a1c;
   L9498d76: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L9498d79;
   L9498d79: // LEA EBX,[ECX + -0x1]
    ebx=unchecked(ecx+4294967295u);goto L9498d7c;
   L9498d7c: // MOV dword ptr [EAX + -0x4],EBX
    W(unchecked(eax+4294967292u),ebx);goto L9498d7f;
   L9498d7f: // JMP 0x09498d59
    goto L9498d59;
  }
 }
}
