using System;
namespace RouletteDeluxe
{
 // Physical packaged __dynamic_cast 000b1bb0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; four original inputs are at +50..+5c. RTTI virtual traversal remains required.
 public sealed class OriginalConfigurationDynamicCast
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
  public OriginalConfigurationDynamicCast(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Cast(uint frame,uint owner,uint sourceType,uint targetType,uint hint)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x50u,owner);W(frame+0x54u,sourceType);W(frame+0x58u,targetType);W(frame+0x5cu,hint);goto Lb1bb6;
   Lb1bb6: // MOV EDI,dword ptr [ESP + 0x50]
    edi=R(unchecked(esp+80u));goto Lb1bba;
   Lb1bba: // LEA ESI,[ESP + 0x2c]
    esi=unchecked(esp+44u);goto Lb1bbe;
   Lb1bbe: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto Lb1bc0;
   Lb1bc0: // MOV ECX,dword ptr [EAX + -0x8]
    ecx=R(unchecked(eax+4294967288u));goto Lb1bc3;
   Lb1bc3: // MOV EAX,dword ptr [EAX + -0x4]
    eax=R(unchecked(eax+4294967292u));goto Lb1bc6;
   Lb1bc6: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto Lb1bce;
   Lb1bce: // MOV dword ptr [ESP + 0x30],0x0
    W(unchecked(esp+48u),0u);goto Lb1bd6;
   Lb1bd6: // ADD ECX,EDI
    a=ecx;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto Lb1bd8;
   Lb1bd8: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto Lb1bda;
   Lb1bda: // MOV dword ptr [ESP + 0x1c],ESI
    W(unchecked(esp+28u),esi);goto Lb1bde;
   Lb1bde: // MOV ESI,dword ptr [ESP + 0x54]
    esi=R(unchecked(esp+84u));goto Lb1be2;
   Lb1be2: // MOV dword ptr [ESP + 0x10],ECX
    W(unchecked(esp+16u),ecx);goto Lb1be6;
   Lb1be6: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto Lb1bea;
   Lb1bea: // MOV dword ptr [ESP + 0x34],0x0
    W(unchecked(esp+52u),0u);goto Lb1bf2;
   Lb1bf2: // MOV dword ptr [ESP + 0x38],0x0
    W(unchecked(esp+56u),0u);goto Lb1bfa;
   Lb1bfa: // MOV dword ptr [ESP + 0x14],ESI
    W(unchecked(esp+20u),esi);goto Lb1bfe;
   Lb1bfe: // MOV ESI,dword ptr [ESP + 0x5c]
    esi=R(unchecked(esp+92u));goto Lb1c02;
   Lb1c02: // MOV dword ptr [ESP + 0x3c],0x10
    W(unchecked(esp+60u),16u);goto Lb1c0a;
   Lb1c0a: // MOV dword ptr [ESP + 0x18],EDI
    W(unchecked(esp+24u),edi);goto Lb1c0e;
   Lb1c0e: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto Lb1c12;
   Lb1c12: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto Lb1c16;
   Lb1c16: // MOV dword ptr [ESP + 0x8],0x6
    W(unchecked(esp+8u),6u);goto Lb1c1e;
   Lb1c1e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb1c21;
   Lb1c21: // CALL dword ptr [EDX + 0x1c]
    eax=Call(R(unchecked(edx+28u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)),R(unchecked(esp+20u)),R(unchecked(esp+24u)),R(unchecked(esp+28u)));goto Lb1c24;
   Lb1c24: // MOV ESI,dword ptr [ESP + 0x2c]
    esi=R(unchecked(esp+44u));goto Lb1c28;
   Lb1c28: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1c2a;
   Lb1c2a: // MOV EBP,ESI
    ebp=esi;goto Lb1c2c;
   Lb1c2c: // JZ 0x000b1c6e
    if(zero)goto Lb1c6e;goto Lb1c2e;
   Lb1c2e: // MOV EDX,dword ptr [ESP + 0x38]
    edx=R(unchecked(esp+56u));goto Lb1c32;
   Lb1c32: // MOV EAX,EDX
    eax=edx;goto Lb1c34;
   Lb1c34: // AND EAX,0x6
    a=eax;b=6u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1c37;
   Lb1c37: // CMP EAX,0x6
    a=eax;b=6u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1c3a;
   Lb1c3a: // JZ 0x000b1c6e
    if(zero)goto Lb1c6e;goto Lb1c3c;
   Lb1c3c: // MOV ECX,dword ptr [ESP + 0x34]
    ecx=R(unchecked(esp+52u));goto Lb1c40;
   Lb1c40: // MOV EAX,dword ptr [ESP + 0x30]
    eax=R(unchecked(esp+48u));goto Lb1c44;
   Lb1c44: // AND EAX,ECX
    a=eax;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1c46;
   Lb1c46: // AND EAX,0x6
    a=eax;b=6u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1c49;
   Lb1c49: // CMP EAX,0x6
    a=eax;b=6u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1c4c;
   Lb1c4c: // JZ 0x000b1c6e
    if(zero)goto Lb1c6e;goto Lb1c4e;
   Lb1c4e: // AND ECX,0x5
    a=ecx;b=5u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb1c51;
   Lb1c51: // XOR ESI,ESI
    a=esi;b=esi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;esi=v;goto Lb1c53;
   Lb1c53: // CMP ECX,0x4
    a=ecx;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1c56;
   Lb1c56: // JZ 0x000b1c6e
    if(zero)goto Lb1c6e;goto Lb1c58;
   Lb1c58: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1c5a;
   Lb1c5a: // JNZ 0x000b1c6e
    if(!zero)goto Lb1c6e;goto Lb1c5c;
   Lb1c5c: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb1c60;
   Lb1c60: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1c62;
   Lb1c62: // JS 0x000b1c78
    if(sign)goto Lb1c78;goto Lb1c64;
   Lb1c64: // MOV EAX,dword ptr [ESP + 0x5c]
    eax=R(unchecked(esp+92u));goto Lb1c68;
   Lb1c68: // ADD EAX,EBP
    a=eax;b=ebp;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb1c6a;
   Lb1c6a: // CMP EDI,EAX
    a=edi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1c6c;
   Lb1c6c: // JZ 0x000b1cb5
    if(zero)goto Lb1cb5;goto Lb1c6e;
   Lb1c6e:return esi;
   Lb1c78: // CMP dword ptr [ESP + 0x5c],-0x2
    a=R(unchecked(esp+92u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1c7d;
   Lb1c7d: // MOV ESI,EDX
    esi=edx;goto Lb1c7f;
   Lb1c7f: // JZ 0x000b1c6e
    if(zero)goto Lb1c6e;goto Lb1c81;
   Lb1c81: // MOV EDX,dword ptr [ESP + 0x58]
    edx=R(unchecked(esp+88u));goto Lb1c85;
   Lb1c85: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto Lb1c89;
   Lb1c89: // MOV EAX,dword ptr [EDX]
    eax=R(unchecked(edx));goto Lb1c8b;
   Lb1c8b: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto Lb1c8f;
   Lb1c8f: // MOV EDX,dword ptr [ESP + 0x5c]
    edx=R(unchecked(esp+92u));goto Lb1c93;
   Lb1c93: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto Lb1c97;
   Lb1c97: // MOV dword ptr [ESP + 0x10],EDI
    W(unchecked(esp+16u),edi);goto Lb1c9b;
   Lb1c9b: // MOV dword ptr [ESP + 0x8],EBP
    W(unchecked(esp+8u),ebp);goto Lb1c9f;
   Lb1c9f: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb1ca3;
   Lb1ca3: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto Lb1ca6;
   Lb1ca6: // CALL dword ptr [EAX + 0x20]
    eax=Call(R(unchecked(eax+32u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)));goto Lb1ca9;
   Lb1ca9: // AND EAX,0x6
    a=eax;b=6u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1cac;
   Lb1cac: // CMP EAX,0x6
    a=eax;b=6u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1caf;
   Lb1caf: // JNZ 0x000b1c6e
    if(!zero)goto Lb1c6e;goto Lb1cb1;
   Lb1cb1: // MOV EBP,dword ptr [ESP + 0x2c]
    ebp=R(unchecked(esp+44u));goto Lb1cb5;
   Lb1cb5: // MOV ESI,EBP
    esi=ebp;goto Lb1cb7;
   Lb1cb7:return esi;
  }
 }
}
