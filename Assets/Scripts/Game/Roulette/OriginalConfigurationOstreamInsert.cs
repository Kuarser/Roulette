using System;
namespace RouletteDeluxe
{
 // Physical packaged ostream counted insert 0008ce40. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; owner/source/count are caller stack inputs. Virtual calls preserve their slot-specific original argument lists.
 public sealed class OriginalConfigurationOstreamInsert
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
  public OriginalConfigurationOstreamInsert(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Insert(uint frame,uint owner,uint source,uint count)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x50u),owner);W(unchecked(frame+0x54u),source);W(unchecked(frame+0x58u),count);goto L8ce52;
   L8ce52: // MOV EDI,dword ptr [ESP + 0x50]
    edi=R(unchecked(esp+80u));goto L8ce56;
   L8ce56: // LEA EAX,[ESP + 0x28]
    eax=unchecked(esp+40u);goto L8ce5a;
   L8ce5a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8ce5d;
   L8ce5d: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L8ce61;
   L8ce61: // CALL 0x0004a690
    eax=Call(0x4a690,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L8ce66;
   L8ce66: // CMP byte ptr [ESP + 0x28],0x0
    a=B(unchecked(esp+40u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8ce6b;
   L8ce6b: // JZ 0x0008ceb8
    if(zero)goto L8ceb8;goto L8ce6d;
   L8ce6d: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8ce6f;
   L8ce6f: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto L8ce73;
   L8ce73: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8ce76;
   L8ce76: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8ce79;
   L8ce79: // MOV EDX,dword ptr [EBP + 0x8]
    edx=R(unchecked(ebp+8u));goto L8ce7c;
   L8ce7c: // CMP EDX,ECX
    a=edx;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8ce7e;
   L8ce7e: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L8ce82;
   L8ce82: // JLE 0x0008cf80
    if(zero||sign!=overflow)goto L8cf80;goto L8ce88;
   L8ce88: // MOV EDX,dword ptr [EBP + 0xc]
    edx=R(unchecked(ebp+12u));goto L8ce8b;
   L8ce8b: // AND EDX,0xb0
    a=edx;b=176u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L8ce91;
   L8ce91: // CMP EDX,0x20
    a=edx;b=32u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8ce94;
   L8ce94: // SETZ byte ptr [ESP + 0x1f]
    h.WriteByte(unchecked(esp+31u),unchecked((byte)(zero?1u:0u)));goto L8ce99;
   L8ce99: // JNZ 0x0008d000
    if(!zero)goto L8d000;goto L8ce9f;
   L8ce9f: // MOV ECX,dword ptr [EBP + 0x14]
    ecx=R(unchecked(ebp+20u));goto L8cea2;
   L8cea2: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8cea4;
   L8cea4: // JZ 0x0008cfa8
    if(zero)goto L8cfa8;goto L8ceaa;
   L8ceaa: // CMP byte ptr [ESP + 0x1f],0x0
    a=B(unchecked(esp+31u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8ceaf;
   L8ceaf: // JNZ 0x0008cef8
    if(!zero)goto L8cef8;goto L8ceb1;
   L8ceb1: // MOV dword ptr [EBP + 0x8],0x0
    W(unchecked(ebp+8u),0u);goto L8ceb8;
   L8ceb8: // MOV ESI,dword ptr [ESP + 0x2c]
    esi=R(unchecked(esp+44u));goto L8cebc;
   L8cebc: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8cebe;
   L8cebe: // ADD ESI,dword ptr [EAX + -0xc]
    a=esi;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L8cec1;
   L8cec1: // TEST byte ptr [ESI + 0xd],0x20
    a=B(unchecked(esi+13u));b=32u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8cec5;
   L8cec5: // JZ 0x0008cee8
    if(zero)goto L8cee8;goto L8cec7;
   L8cec7: // CALL 0x00048c60
    eax=Call(0x48c60);goto L8cecc;
   L8cecc: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8cece;
   L8cece: // JNZ 0x0008cee8
    if(!zero)goto L8cee8;goto L8ced0;
   L8ced0: // MOV EAX,dword ptr [ESI + 0x78]
    eax=R(unchecked(esi+120u));goto L8ced3;
   L8ced3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8ced5;
   L8ced5: // JZ 0x0008cee8
    if(zero)goto L8cee8;goto L8ced7;
   L8ced7: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8ced9;
   L8ced9: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8cedc;
   L8cedc: // CALL dword ptr [EDX + 0x18]
    eax=Call(R(unchecked(edx+24u)),R(unchecked(esp+0u)));goto L8cedf;
   L8cedf: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cee2;
   L8cee2: // JZ 0x0008d058
    if(zero)goto L8d058;goto L8cee8;
   L8cee8:return edi;
   L8cef8: // MOV EDX,dword ptr [EBP + 0x14]
    edx=R(unchecked(ebp+20u));goto L8cefb;
   L8cefb: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8cefd;
   L8cefd: // JNZ 0x0008ceb1
    if(!zero)goto L8ceb1;goto L8ceff;
   L8ceff: // CMP byte ptr [EBP + 0x75],0x0
    a=B(unchecked(ebp+117u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cf03;
   L8cf03: // JZ 0x0008d0b8
    if(zero)goto L8d0b8;goto L8cf09;
   L8cf09: // MOVZX EDX,byte ptr [EBP + 0x74]
    edx=B(unchecked(ebp+116u));goto L8cf0d;
   L8cf0d: // MOV byte ptr [ESP + 0x1e],DL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((edx&255u))));goto L8cf11;
   L8cf11: // MOV ESI,dword ptr [ESP + 0x18]
    esi=R(unchecked(esp+24u));goto L8cf15;
   L8cf15: // SUB ESI,dword ptr [ESP + 0x58]
    a=esi;b=R(unchecked(esp+88u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L8cf19;
   L8cf19: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8cf1b;
   L8cf1b: // JLE 0x0008ceb1
    if(zero||sign!=overflow)goto L8ceb1;goto L8cf1d;
   L8cf1d: // MOVZX EBP,byte ptr [ESP + 0x1e]
    ebp=B(unchecked(esp+30u));goto L8cf22;
   L8cf22: // JMP 0x0008cf3d
    goto L8cf3d;
   L8cf28: // MOVZX ECX,byte ptr [ESP + 0x1e]
    ecx=B(unchecked(esp+30u));goto L8cf2d;
   L8cf2d: // MOV byte ptr [EDX],CL
    h.WriteByte(unchecked(edx),unchecked((byte)((ecx&255u))));goto L8cf2f;
   L8cf2f: // ADD dword ptr [EAX + 0x14],0x1
    a=R(unchecked(eax+20u));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(eax+20u),v);goto L8cf33;
   L8cf33: // SUB ESI,0x1
    a=esi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L8cf36;
   L8cf36: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cf38;
   L8cf38: // JZ 0x0008cf75
    if(zero)goto L8cf75;goto L8cf3a;
   L8cf3a: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8cf3d;
   L8cf3d: // MOV EAX,dword ptr [EDI + EAX*0x1 + 0x78]
    eax=R(unchecked(edi+eax*0x1u+120u));goto L8cf41;
   L8cf41: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L8cf44;
   L8cf44: // CMP EDX,dword ptr [EAX + 0x18]
    a=edx;b=R(unchecked(eax+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cf47;
   L8cf47: // JC 0x0008cf28
    if(carry)goto L8cf28;goto L8cf49;
   L8cf49: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8cf4b;
   L8cf4b: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L8cf4f;
   L8cf4f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8cf52;
   L8cf52: // CALL dword ptr [EDX + 0x34]
    try{eax=Call(R(unchecked(edx+52u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8cf55;
   L8cf55: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8cf58;
   L8cf58: // JNZ 0x0008cf33
    if(!zero)goto L8cf33;goto L8cf5a;
   L8cf5a: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cf5c;
   L8cf5c: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L8cf5f;
   L8cf5f: // ADD EDX,EDI
    a=edx;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8cf61;
   L8cf61: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8cf64;
   L8cf64: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8cf67;
   L8cf67: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8cf6a;
   L8cf6a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8cf6e;
   L8cf6e: // CALL 0x0004a2f0
    try{eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8cf73;
   L8cf73: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cf75;
   L8cf75: // MOV EBP,dword ptr [EAX + -0xc]
    ebp=R(unchecked(eax+4294967284u));goto L8cf78;
   L8cf78: // ADD EBP,EDI
    a=ebp;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebp=v;goto L8cf7a;
   L8cf7a: // JMP 0x0008ceb1
    goto L8ceb1;
   L8cf80: // MOV EAX,dword ptr [EBP + 0x78]
    eax=R(unchecked(ebp+120u));goto L8cf83;
   L8cf83: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto L8cf87;
   L8cf87: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8cf89;
   L8cf89: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto L8cf8d;
   L8cf8d: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto L8cf91;
   L8cf91: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8cf94;
   L8cf94: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L8cf98;
   L8cf98: // CALL dword ptr [EDX + 0x30]
    try{eax=Call(R(unchecked(edx+48u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8cf9b;
   L8cf9b: // CMP dword ptr [ESP + 0x58],EAX
    a=R(unchecked(esp+88u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cf9f;
   L8cf9f: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cfa1;
   L8cfa1: // JNZ 0x0008cf5c
    if(!zero)goto L8cf5c;goto L8cfa3;
   L8cfa3: // JMP 0x0008cf75
    goto L8cf75;
   L8cfa8: // MOV EAX,dword ptr [EBP + 0x78]
    eax=R(unchecked(ebp+120u));goto L8cfab;
   L8cfab: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto L8cfaf;
   L8cfaf: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8cfb1;
   L8cfb1: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto L8cfb5;
   L8cfb5: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto L8cfb9;
   L8cfb9: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8cfbc;
   L8cfbc: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto L8cfc0;
   L8cfc0: // CALL dword ptr [EDX + 0x30]
    try{eax=Call(R(unchecked(edx+48u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8cfc3;
   L8cfc3: // CMP dword ptr [ESP + 0x58],EAX
    a=R(unchecked(esp+88u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cfc7;
   L8cfc7: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cfc9;
   L8cfc9: // JZ 0x0008cfe4
    if(zero)goto L8cfe4;goto L8cfcb;
   L8cfcb: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L8cfce;
   L8cfce: // ADD EDX,EDI
    a=edx;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8cfd0;
   L8cfd0: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8cfd3;
   L8cfd3: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8cfd6;
   L8cfd6: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8cfd9;
   L8cfd9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8cfdd;
   L8cfdd: // CALL 0x0004a2f0
    try{eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8cfe2;
   L8cfe2: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8cfe4;
   L8cfe4: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8cfe7;
   L8cfe7: // CMP byte ptr [ESP + 0x1f],0x0
    a=B(unchecked(esp+31u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8cfec;
   L8cfec: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8cfef;
   L8cfef: // JZ 0x0008ceb1
    if(zero)goto L8ceb1;goto L8cff5;
   L8cff5: // JMP 0x0008cef8
    goto L8cef8;
   L8d000: // CMP byte ptr [EBP + 0x75],0x0
    a=B(unchecked(ebp+117u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8d004;
   L8d004: // JZ 0x0008d080
    if(zero)goto L8d080;goto L8d006;
   L8d006: // MOVZX EDX,byte ptr [EBP + 0x74]
    edx=B(unchecked(ebp+116u));goto L8d00a;
   L8d00a: // MOV byte ptr [ESP + 0x1e],DL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((edx&255u))));goto L8d00e;
   L8d00e: // MOV ESI,dword ptr [ESP + 0x18]
    esi=R(unchecked(esp+24u));goto L8d012;
   L8d012: // SUB ESI,dword ptr [ESP + 0x58]
    a=esi;b=R(unchecked(esp+88u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L8d016;
   L8d016: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8d018;
   L8d018: // JLE 0x0008ce9f
    if(zero||sign!=overflow)goto L8ce9f;goto L8d01e;
   L8d01e: // MOVZX EBP,byte ptr [ESP + 0x1e]
    ebp=B(unchecked(esp+30u));goto L8d023;
   L8d023: // NOP
    goto L8d024;
   L8d024: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L8d028;
   L8d028: // MOV EAX,dword ptr [EDI + EAX*0x1 + 0x78]
    eax=R(unchecked(edi+eax*0x1u+120u));goto L8d02c;
   L8d02c: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L8d02f;
   L8d02f: // CMP EDX,dword ptr [EAX + 0x18]
    a=edx;b=R(unchecked(eax+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8d032;
   L8d032: // JNC 0x0008d12e
    if(!carry)goto L8d12e;goto L8d038;
   L8d038: // MOVZX ECX,byte ptr [ESP + 0x1e]
    ecx=B(unchecked(esp+30u));goto L8d03d;
   L8d03d: // MOV byte ptr [EDX],CL
    h.WriteByte(unchecked(edx),unchecked((byte)((ecx&255u))));goto L8d03f;
   L8d03f: // ADD dword ptr [EAX + 0x14],0x1
    a=R(unchecked(eax+20u));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(eax+20u),v);goto L8d043;
   L8d043: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d045;
   L8d045: // SUB ESI,0x1
    a=esi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L8d048;
   L8d048: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8d04b;
   L8d04b: // JNZ 0x0008d028
    if(!zero)goto L8d028;goto L8d04d;
   L8d04d: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8d050;
   L8d050: // JMP 0x0008ce9f
    goto L8ce9f;
   L8d058: // MOV EDX,dword ptr [ESP + 0x2c]
    edx=R(unchecked(esp+44u));goto L8d05c;
   L8d05c: // MOV EAX,dword ptr [EDX]
    eax=R(unchecked(edx));goto L8d05e;
   L8d05e: // ADD EDX,dword ptr [EAX + -0xc]
    a=edx;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8d061;
   L8d061: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8d064;
   L8d064: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8d067;
   L8d067: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8d06a;
   L8d06a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8d06e;
   L8d06e: // CALL 0x0004a2f0
    eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L8d073;
   L8d073:return edi;
   L8d080: // MOV ESI,dword ptr [EBP + 0x7c]
    esi=R(unchecked(ebp+124u));goto L8d083;
   L8d083: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8d085;
   L8d085: // JZ 0x0008d197
    if(zero)goto L8d197;goto L8d08b;
   L8d08b: // CMP byte ptr [ESI + 0x1c],0x0
    a=B(unchecked(esi+28u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8d08f;
   L8d08f: // JZ 0x0008d0f0
    if(zero)goto L8d0f0;goto L8d091;
   L8d091: // MOVZX EAX,byte ptr [ESI + 0x3d]
    eax=B(unchecked(esi+61u));goto L8d095;
   L8d095: // MOV byte ptr [ESP + 0x1e],AL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((eax&255u))));goto L8d099;
   L8d099: // MOVZX EAX,byte ptr [ESP + 0x1e]
    eax=B(unchecked(esp+30u));goto L8d09e;
   L8d09e: // MOV byte ptr [EBP + 0x75],0x1
    h.WriteByte(unchecked(ebp+117u),unchecked((byte)(1u)));goto L8d0a2;
   L8d0a2: // MOV byte ptr [EBP + 0x74],AL
    h.WriteByte(unchecked(ebp+116u),unchecked((byte)((eax&255u))));goto L8d0a5;
   L8d0a5: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d0a7;
   L8d0a7: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8d0aa;
   L8d0aa: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8d0ad;
   L8d0ad: // JMP 0x0008d00e
    goto L8d00e;
   L8d0b8: // MOV ESI,dword ptr [EBP + 0x7c]
    esi=R(unchecked(ebp+124u));goto L8d0bb;
   L8d0bb: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8d0bd;
   L8d0bd: // JZ 0x0008d19c
    if(zero)goto L8d19c;goto L8d0c3;
   L8d0c3: // CMP byte ptr [ESI + 0x1c],0x0
    a=B(unchecked(esi+28u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8d0c7;
   L8d0c7: // JZ 0x0008d110
    if(zero)goto L8d110;goto L8d0c9;
   L8d0c9: // MOVZX EAX,byte ptr [ESI + 0x3d]
    eax=B(unchecked(esi+61u));goto L8d0cd;
   L8d0cd: // MOV byte ptr [ESP + 0x1e],AL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((eax&255u))));goto L8d0d1;
   L8d0d1: // MOVZX EAX,byte ptr [ESP + 0x1e]
    eax=B(unchecked(esp+30u));goto L8d0d6;
   L8d0d6: // MOV byte ptr [EBP + 0x75],0x1
    h.WriteByte(unchecked(ebp+117u),unchecked((byte)(1u)));goto L8d0da;
   L8d0da: // MOV byte ptr [EBP + 0x74],AL
    h.WriteByte(unchecked(ebp+116u),unchecked((byte)((eax&255u))));goto L8d0dd;
   L8d0dd: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d0df;
   L8d0df: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8d0e2;
   L8d0e2: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8d0e5;
   L8d0e5: // JMP 0x0008cf11
    goto L8cf11;
   L8d0f0: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8d0f3;
   L8d0f3: // CALL 0x00049070
    try{eax=Call(0x49070,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d0f8;
   L8d0f8: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8d0fa;
   L8d0fa: // MOV dword ptr [ESP + 0x4],0x20
    W(unchecked(esp+4u),32u);goto L8d102;
   L8d102: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8d105;
   L8d105: // CALL dword ptr [EAX + 0x18]
    try{eax=Call(R(unchecked(eax+24u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d108;
   L8d108: // MOV byte ptr [ESP + 0x1e],AL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((eax&255u))));goto L8d10c;
   L8d10c: // JMP 0x0008d099
    goto L8d099;
   L8d110: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8d113;
   L8d113: // CALL 0x00049070
    try{eax=Call(0x49070,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d118;
   L8d118: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8d11a;
   L8d11a: // MOV dword ptr [ESP + 0x4],0x20
    W(unchecked(esp+4u),32u);goto L8d122;
   L8d122: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8d125;
   L8d125: // CALL dword ptr [EAX + 0x18]
    try{eax=Call(R(unchecked(eax+24u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d128;
   L8d128: // MOV byte ptr [ESP + 0x1e],AL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((eax&255u))));goto L8d12c;
   L8d12c: // JMP 0x0008d0d1
    goto L8d0d1;
   L8d12e: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8d130;
   L8d130: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L8d134;
   L8d134: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8d137;
   L8d137: // CALL dword ptr [EDX + 0x34]
    try{eax=Call(R(unchecked(edx+52u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d13a;
   L8d13a: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8d13d;
   L8d13d: // JNZ 0x0008d043
    if(!zero)goto L8d043;goto L8d143;
   L8d143: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d145;
   L8d145: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L8d148;
   L8d148: // ADD EDX,EDI
    a=edx;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8d14a;
   L8d14a: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8d14d;
   L8d14d: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8d150;
   L8d150: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8d153;
   L8d153: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8d157;
   L8d157: // CALL 0x0004a2f0
    try{eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}goto L8d15c;
   L8d15c: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d15e;
   L8d15e: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8d161;
   L8d161: // LEA EBP,[EDI + EAX*0x1]
    ebp=unchecked(edi+eax*0x1u);goto L8d164;
   L8d164: // JMP 0x0008ce9f
    goto L8ce9f;
   L8d169: // SUB EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L8d16c;
   L8d16c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8d16f;
   L8d16f: // JZ 0x0008d1c4
    if(zero)goto L8d1c4;goto L8d171;
   L8d171: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L8d176;
   L8d176: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L8d17e;
   L8d17e: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d180;
   L8d180: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8d183;
   L8d183: // ADD EAX,EDI
    a=eax;b=edi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8d185;
   L8d185: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8d188;
   L8d188: // CALL 0x00049080
    try{eax=Call(0x49080,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d1bb;}goto L8d18d;
   L8d18d: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d1a5;}goto L8d192;
   L8d192: // JMP 0x0008ceb8
    goto L8ceb8;
   L8d197: // CALL 0x00048a40
    try{Call(0x48a40);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}
   L8d19c: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L8d1a0;
   L8d1a0: // CALL 0x00048a40
    try{Call(0x48a40);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d169;}
   L8d1a5: // MOV ESI,EAX
    esi=eax;goto L8d1a7;
   L8d1a7: // LEA EAX,[ESP + 0x28]
    eax=unchecked(esp+40u);goto L8d1ab;
   L8d1ab: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8d1ae;
   L8d1ae: // CALL 0x000487a0
    eax=Call(0x487a0,R(unchecked(esp+0u)));goto L8d1b3;
   L8d1b3: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8d1b6;
   L8d1b6: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L8d1bb: // MOV ESI,EAX
    esi=eax;goto L8d1bd;
   L8d1bd: // CALL 0x000497e0
    eax=Call(0x497e0);goto L8d1c2;
   L8d1c2: // JMP 0x0008d1a7
    goto L8d1a7;
   L8d1c4: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L8d1c9;
   L8d1c9: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L8d1d1;
   L8d1d1: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L8d1d3;
   L8d1d3: // ADD EDI,dword ptr [EAX + -0xc]
    a=edi;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L8d1d6;
   L8d1d6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L8d1d9;
   L8d1d9: // CALL 0x00049080
    try{eax=Call(0x49080,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d1e3;}goto L8d1de;
   L8d1de: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8d1e3;}
   L8d1e3: // MOV ESI,EAX
    esi=eax;goto L8d1e5;
   L8d1e5: // CALL 0x000497e0
    eax=Call(0x497e0);goto L8d1ea;
   L8d1ea: // JMP 0x0008d1a7
    goto L8d1a7;
  }
 }
}
