using System;
namespace RouletteDeluxe
{
 // Physical packaged streambuf and ostream sentry bodies. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Imported string, heap and ios owners remain required.
 public sealed class OriginalConfigurationStreambufControl
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
  public OriginalConfigurationStreambufControl(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Write(uint frame,uint owner,uint source,uint count)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x30u),owner);W(unchecked(frame+0x34u),source);W(unchecked(frame+0x38u),count);goto L96ec4;
   L96ec4: // MOV EBP,dword ptr [ESP + 0x30]
    ebp=R(unchecked(esp+48u));goto L96ec8;
   L96ec8: // CMP dword ptr [ESP + 0x38],EDI
    a=R(unchecked(esp+56u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L96ecc;
   L96ecc: // JLE 0x00096f00
    if(zero||sign!=overflow)goto L96f00;goto L96ece;
   L96ece: // MOV EAX,dword ptr [EBP + 0x14]
    eax=R(unchecked(ebp+20u));goto L96ed1;
   L96ed1: // MOV EDX,dword ptr [EBP + 0x18]
    edx=R(unchecked(ebp+24u));goto L96ed4;
   L96ed4: // SUB EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L96ed6;
   L96ed6: // JNZ 0x00096f10
    if(!zero)goto L96f10;goto L96ed8;
   L96ed8: // MOV ECX,dword ptr [ESP + 0x34]
    ecx=R(unchecked(esp+52u));goto L96edc;
   L96edc: // MOV EAX,dword ptr [EBP]
    eax=R(unchecked(ebp));goto L96edf;
   L96edf: // MOVZX EDX,byte ptr [ECX]
    edx=B(unchecked(ecx));goto L96ee2;
   L96ee2: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L96ee5;
   L96ee5: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L96ee9;
   L96ee9: // CALL dword ptr [EAX + 0x34]
    eax=Call(R(unchecked(eax+52u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L96eec;
   L96eec: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L96eef;
   L96eef: // JZ 0x00096f00
    if(zero)goto L96f00;goto L96ef1;
   L96ef1: // ADD EDI,0x1
    a=edi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L96ef4;
   L96ef4: // ADD dword ptr [ESP + 0x34],0x1
    a=R(unchecked(esp+52u));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(esp+52u),v);goto L96ef9;
   L96ef9: // CMP dword ptr [ESP + 0x38],EDI
    a=R(unchecked(esp+56u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L96efd;
   L96efd: // JG 0x00096ece
    if(!zero&&sign==overflow)goto L96ece;goto L96eff;
   L96eff: // NOP
    goto L96f00;
   L96f00:return edi;
   L96f10: // MOV ESI,dword ptr [ESP + 0x38]
    esi=R(unchecked(esp+56u));goto L96f14;
   L96f14: // SUB ESI,EDI
    a=esi;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L96f16;
   L96f16: // CMP ESI,EDX
    a=esi;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L96f18;
   L96f18: // JLE 0x00096f1c
    if(zero||sign!=overflow)goto L96f1c;goto L96f1a;
   L96f1a: // MOV ESI,EDX
    esi=edx;goto L96f1c;
   L96f1c: // MOV EDX,dword ptr [ESP + 0x34]
    edx=R(unchecked(esp+52u));goto L96f20;
   L96f20: // ADD EDI,ESI
    a=edi;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L96f22;
   L96f22: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L96f26;
   L96f26: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L96f29;
   L96f29: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L96f2d;
   L96f2d: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L96f32;
   L96f32: // ADD dword ptr [EBP + 0x14],ESI
    a=R(unchecked(ebp+20u));b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(ebp+20u),v);goto L96f35;
   L96f35: // CMP dword ptr [ESP + 0x38],EDI
    a=R(unchecked(esp+56u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L96f39;
   L96f39: // JLE 0x00096f00
    if(zero||sign!=overflow)goto L96f00;goto L96f3b;
   L96f3b: // ADD dword ptr [ESP + 0x34],ESI
    a=R(unchecked(esp+52u));b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(esp+52u),v);goto L96f3f;
   L96f3f: // JMP 0x00096ed8
    goto L96ed8;
  }
  public uint Overflow(uint frame,uint owner,uint character)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x50u),owner);W(unchecked(frame+0x54u),character);goto L9202a;
   L9202a: // MOV dword ptr [ESP + 0x48],EBP
    W(unchecked(esp+72u),ebp);goto L9202e;
   L9202e: // MOV EBP,dword ptr [ESP + 0x54]
    ebp=R(unchecked(esp+84u));goto L92032;
   L92032: // TEST byte ptr [ESI + 0x20],0x10
    a=B(unchecked(esi+32u));b=16u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L92036;
   L92036: // MOV dword ptr [ESP + 0x44],EDI
    W(unchecked(esp+68u),edi);goto L9203a;
   L9203a: // JZ 0x000921a8
    if(zero)goto L921a8;goto L92040;
   L92040: // CMP EBP,-0x1
    a=ebp;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L92043;
   L92043: // JZ 0x000921b2
    if(zero)goto L921b2;goto L92049;
   L92049: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L9204c;
   L9204c: // MOV EDX,dword ptr [EAX + -0x8]
    edx=R(unchecked(eax+4294967288u));goto L9204f;
   L9204f: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L92052;
   L92052: // CMP EAX,dword ptr [ESI + 0x18]
    a=eax;b=R(unchecked(esi+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L92055;
   L92055: // JNC 0x00092078
    if(!carry)goto L92078;goto L92057;
   L92057: // MOV ECX,EBP
    ecx=ebp;goto L92059;
   L92059: // MOV byte ptr [EAX],CL
    h.WriteByte(unchecked(eax),unchecked((byte)((ecx&255u))));goto L9205b;
   L9205b: // ADD dword ptr [ESI + 0x14],0x1
    a=R(unchecked(esi+20u));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(esi+20u),v);goto L9205f;
   L9205f: // MOV EAX,EBP
    eax=ebp;goto L92061;
   L92061:return eax;
   L92078: // CMP EDX,0x3ffffffc
    a=edx;b=1073741820u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9207e;
   L9207e: // JZ 0x000921a8
    if(zero)goto L921a8;goto L92084;
   L92084: // ADD EDX,EDX
    a=edx;b=edx;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L92086;
   L92086: // CMP EDX,0x1ff
    a=edx;b=511u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9208c;
   L9208c: // JBE 0x00092198
    if(carry||zero)goto L92198;goto L92092;
   L92092: // CMP EDX,0x3ffffffc
    a=edx;b=1073741820u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L92098;
   L92098: // JBE 0x0009209f
    if(carry||zero)goto L9209f;goto L9209a;
   L9209a: // MOV EDX,0x3ffffffc
    edx=1073741820u;goto L9209f;
   L9209f: // MOV ECX,dword ptr [EBX + 0xfffffc8c]
    ecx=R(unchecked(ebx+4294966412u));goto L920a5;
   L920a5: // LEA EDI,[ESP + 0x28]
    edi=unchecked(esp+40u);goto L920a9;
   L920a9: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L920ad;
   L920ad: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L920b0;
   L920b0: // LEA EAX,[ECX + 0xc]
    eax=unchecked(ecx+12u);goto L920b3;
   L920b3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L920b7;
   L920b7: // CALL 0x00048e50
    try{eax=Call(0x48e50,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L921d0;}goto L920bc;
   L920bc: // MOV EAX,dword ptr [ESI + 0x10]
    eax=R(unchecked(esi+16u));goto L920bf;
   L920bf: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L920c1;
   L920c1: // JZ 0x000920d8
    if(zero)goto L920d8;goto L920c3;
   L920c3: // MOV EDX,dword ptr [ESI + 0x18]
    edx=R(unchecked(esi+24u));goto L920c6;
   L920c6: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L920ca;
   L920ca: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L920cd;
   L920cd: // SUB EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L920cf;
   L920cf: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L920d3;
   L920d3: // CALL 0x0004a6c0
    try{eax=Call(0x4a6c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L921d0;}goto L920d8;
   L920d8: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L920dc;
   L920dc: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L920df;
   L920df: // LEA ECX,[EDX + 0x1]
    ecx=unchecked(edx+1u);goto L920e2;
   L920e2: // CMP ECX,dword ptr [EAX + -0x8]
    a=ecx;b=R(unchecked(eax+4294967288u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L920e5;
   L920e5: // MOV dword ptr [ESP + 0x1c],ECX
    W(unchecked(esp+28u),ecx);goto L920e9;
   L920e9: // JA 0x000920f2
    if(!carry&&!zero)goto L920f2;goto L920eb;
   L920eb: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L920ee;
   L920ee: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L920f0;
   L920f0: // JLE 0x00092109
    if(zero||sign!=overflow)goto L92109;goto L920f2;
   L920f2: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L920f6;
   L920f6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L920f9;
   L920f9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L920fd;
   L920fd: // CALL 0x00048e50
    try{eax=Call(0x48e50,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L921d0;}goto L92102;
   L92102: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L92106;
   L92106: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L92109;
   L92109: // MOV ECX,EBP
    ecx=ebp;goto L9210b;
   L9210b: // MOV byte ptr [EAX + EDX*0x1],CL
    h.WriteByte(unchecked(eax+edx*0x1u),unchecked((byte)((ecx&255u))));goto L9210e;
   L9210e: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L92112;
   L92112: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L92115;
   L92115: // CMP EDX,dword ptr [EBX + 0xfffffc8c]
    a=edx;b=R(unchecked(ebx+4294966412u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9211b;
   L9211b: // JNZ 0x000921b9
    if(!zero)goto L921b9;goto L92121;
   L92121: // LEA EAX,[ESI + 0x24]
    eax=unchecked(esi+36u);goto L92124;
   L92124: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L92128;
   L92128: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9212b;
   L9212b: // CALL 0x0004a190
    try{eax=Call(0x4a190,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L921d0;}goto L92130;
   L92130: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L92133;
   L92133: // SUB EAX,dword ptr [ESI + 0x10]
    a=eax;b=R(unchecked(esi+16u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L92136;
   L92136: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L9213a;
   L9213a: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L9213d;
   L9213d: // SUB EAX,dword ptr [ESI + 0x4]
    a=eax;b=R(unchecked(esi+4u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L92140;
   L92140: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L92144;
   L92144: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L92147;
   L92147: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9214a;
   L9214a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9214e;
   L9214e: // CALL 0x00048480
    try{eax=Call(0x48480,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L921d0;}goto L92153;
   L92153: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L92157;
   L92157: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9215a;
   L9215a: // CMP EDX,dword ptr [EBX + 0xfffffc8c]
    a=edx;b=R(unchecked(ebx+4294966412u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L92160;
   L92160: // JZ 0x0009205b
    if(zero)goto L9205b;goto L92166;
   L92166: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9216d;
   L9216d: // JZ 0x000921e2
    if(zero)goto L921e2;goto L9216f;
   L9216f: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L92172;
   L92172: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L92177;
   L92177: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L92179;
   L92179: // JG 0x0009205b
    if(!zero&&sign==overflow)goto L9205b;goto L9217f;
   L9217f: // LEA EAX,[ESP + 0x2f]
    eax=unchecked(esp+47u);goto L92183;
   L92183: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L92187;
   L92187: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9218a;
   L9218a: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9218f;
   L9218f: // JMP 0x0009205b
    goto L9205b;
   L92198: // MOV EDX,0x200
    edx=512u;goto L9219d;
   L9219d: // JMP 0x0009209f
    goto L9209f;
   L921a8: // MOV EBP,0xffffffff
    ebp=4294967295u;goto L921ad;
   L921ad: // JMP 0x0009205f
    goto L9205f;
   L921b2: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L921b4;
   L921b4: // JMP 0x0009205f
    goto L9205f;
   L921b9: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L921bd;
   L921bd: // MOV dword ptr [EAX + -0x4],0x0
    W(unchecked(eax+4294967292u),0u);goto L921c4;
   L921c4: // MOV dword ptr [EAX + -0xc],EDX
    W(unchecked(eax+4294967284u),edx);goto L921c7;
   L921c7: // MOV byte ptr [EAX + EDX*0x1],0x0
    h.WriteByte(unchecked(eax+edx*0x1u),unchecked((byte)(0u)));goto L921cb;
   L921cb: // JMP 0x00092121
    goto L92121;
   L921d0: // MOV ESI,EAX
    esi=eax;goto L921d2;
   L921d2: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L921d5;
   L921d5: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L921da;
   L921da: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L921dd;
   L921dd: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L921e2: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L921e5;
   L921e5: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L921e8;
   L921e8: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L921eb;
   L921eb: // JMP 0x00092177
    goto L92177;
  }
  public uint ConstructSentry(uint frame,uint guard,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=guard,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L8c6c2;
   L8c6c2: // MOV EDX,dword ptr [ESI]
    edx=R(unchecked(esi));goto L8c6c4;
   L8c6c4: // MOV byte ptr [EDI],0x0
    h.WriteByte(unchecked(edi),unchecked((byte)(0u)));goto L8c6c7;
   L8c6c7: // MOV dword ptr [EDI + 0x4],ESI
    W(unchecked(edi+4u),esi);goto L8c6ca;
   L8c6ca: // MOV EAX,dword ptr [EDX + -0xc]
    eax=R(unchecked(edx+4294967284u));goto L8c6cd;
   L8c6cd: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8c6cf;
   L8c6cf: // MOV ECX,dword ptr [EAX + 0x70]
    ecx=R(unchecked(eax+112u));goto L8c6d2;
   L8c6d2: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c6d4;
   L8c6d4: // JZ 0x0008c6e7
    if(zero)goto L8c6e7;goto L8c6d6;
   L8c6d6: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L8c6d9;
   L8c6d9: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c6db;
   L8c6db: // JNZ 0x0008c708
    if(!zero)goto L8c708;goto L8c6dd;
   L8c6dd: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L8c6e0;
   L8c6e0: // CALL 0x00049630
    eax=Call(0x49630,R(unchecked(esp+0u)));goto L8c6e5;
   L8c6e5: // MOV EDX,dword ptr [ESI]
    edx=R(unchecked(esi));goto L8c6e7;
   L8c6e7: // MOV EAX,dword ptr [EDX + -0xc]
    eax=R(unchecked(edx+4294967284u));goto L8c6ea;
   L8c6ea: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8c6ec;
   L8c6ec: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L8c6ef;
   L8c6ef: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c6f1;
   L8c6f1: // JNZ 0x0008c708
    if(!zero)goto L8c708;goto L8c6f3;
   L8c6f3: // MOV byte ptr [EDI],0x1
    h.WriteByte(unchecked(edi),unchecked((byte)(1u)));goto L8c6f6;
   L8c6f6:return 0;
   L8c708: // OR EDX,0x4
    a=edx;b=4u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L8c70b;
   L8c70b: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L8c70f;
   L8c70f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8c712;
   L8c712: // CALL 0x0004a2f0
    eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L8c717;
   L8c717:return 0;
  }
  public uint DestroySentry(uint frame,uint guard)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=guard,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L8c74e;
   L8c74e: // MOV ESI,dword ptr [EDI + 0x4]
    esi=R(unchecked(edi+4u));goto L8c751;
   L8c751: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8c753;
   L8c753: // ADD ESI,dword ptr [EAX + -0xc]
    a=esi;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L8c756;
   L8c756: // TEST byte ptr [ESI + 0xd],0x20
    a=B(unchecked(esi+13u));b=32u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c75a;
   L8c75a: // JZ 0x0008c779
    if(zero)goto L8c779;goto L8c75c;
   L8c75c: // CALL 0x00048c60
    eax=Call(0x48c60);goto L8c761;
   L8c761: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c763;
   L8c763: // JNZ 0x0008c779
    if(!zero)goto L8c779;goto L8c765;
   L8c765: // MOV EAX,dword ptr [ESI + 0x78]
    eax=R(unchecked(esi+120u));goto L8c768;
   L8c768: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c76a;
   L8c76a: // JZ 0x0008c779
    if(zero)goto L8c779;goto L8c76c;
   L8c76c: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8c76e;
   L8c76e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8c771;
   L8c771: // CALL dword ptr [EDX + 0x18]
    eax=Call(R(unchecked(edx+24u)),R(unchecked(esp+0u)));goto L8c774;
   L8c774: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8c777;
   L8c777: // JZ 0x0008c790
    if(zero)goto L8c790;goto L8c779;
   L8c779:return 0;
   L8c790: // MOV EDX,dword ptr [EDI + 0x4]
    edx=R(unchecked(edi+4u));goto L8c793;
   L8c793: // MOV EAX,dword ptr [EDX]
    eax=R(unchecked(edx));goto L8c795;
   L8c795: // ADD EDX,dword ptr [EAX + -0xc]
    a=edx;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8c798;
   L8c798: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8c79b;
   L8c79b: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8c79e;
   L8c79e: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8c7a1;
   L8c7a1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8c7a5;
   L8c7a5: // CALL 0x0004a2f0
    eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L8c7aa;
   L8c7aa:return 0;
  }
  public uint Sync(uint owner){return 0;}
 }
}
