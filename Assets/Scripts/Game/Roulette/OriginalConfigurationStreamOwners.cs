using System;
namespace RouletteDeluxe
{
 // Physical packaged streambuf/stringstream/locale/ios lifetime owners. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Required imported lifetime and native exception owners remain explicit.
 public sealed class OriginalConfigurationStreamOwners
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
  public OriginalConfigurationStreamOwners(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint CallCallbacks(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=owner,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x34u),argument);goto L580f6;
   L580f6: // MOV EBP,dword ptr [ESP + 0x34]
    ebp=R(unchecked(esp+52u));goto L580fa;
   L580fa: // MOV ESI,dword ptr [EDI + 0x18]
    esi=R(unchecked(edi+24u));goto L580fd;
   L580fd: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L580ff;
   L580ff: // JZ 0x0005811f
    if(zero)goto L5811f;goto L58101;
   L58101: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L58108;
   L58108: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L5810b;
   L5810b: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L5810f;
   L5810f: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L58112;
   L58112: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L58116;
   L58116: // CALL dword ptr [ESI + 0x4]
    try{eax=Call(R(unchecked(esi+4u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L58127;}goto L58119;
   L58119: // MOV ESI,dword ptr [ESI]
    esi=R(unchecked(esi));goto L5811b;
   L5811b: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5811d;
   L5811d: // JNZ 0x00058108
    if(!zero)goto L58108;goto L5811f;
   L5811f:return 0;
   L58127: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5812a;
   L5812a: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L5812f;
   L5812f: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L58136;}goto L58134;
   L58134: // JMP 0x00058119
    goto L58119;
   L58136: // CMP EDX,-0x1
    a=edx;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L58139;
   L58139: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5813c;
   L5813c: // JZ 0x00058145
    if(zero)goto L58145;goto L5813e;
   L5813e: // NOP
    goto L58140;
   L58140: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L58145: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DestroyStringbuf(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5252e;
   L5252e: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L52534;
   L52534: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L52537;
   L52537: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L52539;
   L52539: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L5253c;
   L5253c: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L5253f;
   L5253f: // CMP EDX,dword ptr [EBX + 0xfffffc8c]
    a=edx;b=R(unchecked(ebx+4294966412u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L52545;
   L52545: // JNZ 0x0005256d
    if(!zero)goto L5256d;goto L52547;
   L52547: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L5254d;
   L5254d: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L52550;
   L52550: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L52552;
   L52552: // ADD ESI,0x1c
    a=esi;b=28u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L52555;
   L52555: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L52558;
   L52558: // CALL 0x00049990
    eax=Call(0x49990,R(unchecked(esp+0u)));goto L5255d;
   L5255d:return 0;
   L5256d: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L52574;
   L52574: // JZ 0x00052594
    if(zero)goto L52594;goto L52576;
   L52576: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L52579;
   L52579: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L5257e;
   L5257e: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L52580;
   L52580: // JG 0x00052547
    if(!zero&&sign==overflow)goto L52547;goto L52582;
   L52582: // LEA EAX,[ESP + 0x1f]
    eax=unchecked(esp+31u);goto L52586;
   L52586: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5258a;
   L5258a: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L5258d;
   L5258d: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L52592;
   L52592: // JMP 0x00052547
    goto L52547;
   L52594: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L52597;
   L52597: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L5259a;
   L5259a: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L5259d;
   L5259d: // JMP 0x0005257e
    goto L5257e;
  }
  public uint InitializeBase(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x30u),owner);goto L58fa2;
   L58fa2: // MOV dword ptr [ESP + 0x28],EDI
    W(unchecked(esp+40u),edi);goto L58fa6;
   L58fa6: // MOV EDI,dword ptr [ESP + 0x30]
    edi=R(unchecked(esp+48u));goto L58faa;
   L58faa: // MOV dword ptr [ESP + 0x24],ESI
    W(unchecked(esp+36u),esi);goto L58fae;
   L58fae: // LEA ESI,[ESP + 0x1c]
    esi=unchecked(esp+28u);goto L58fb2;
   L58fb2: // MOV dword ptr [EDI + 0x4],0x6
    W(unchecked(edi+4u),6u);goto L58fb9;
   L58fb9: // MOV dword ptr [EDI + 0x8],0x0
    W(unchecked(edi+8u),0u);goto L58fc0;
   L58fc0: // MOV dword ptr [EDI + 0xc],0x1002
    W(unchecked(edi+12u),4098u);goto L58fc7;
   L58fc7: // ADD EDI,0x6c
    a=edi;b=108u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L58fca;
   L58fca: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L58fcd;
   L58fcd: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L58fd2;
   L58fd2: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L58fd6;
   L58fd6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L58fd9;
   L58fd9: // CALL 0x000496a0
    eax=Call(0x496a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L58fde;
   L58fde: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L58fe1;
   L58fe1: // CALL 0x00049990
    eax=Call(0x49990,R(unchecked(esp+0u)));goto L58fe6;
   L58fe6:return 0;
  }
  public uint DestroyLocaleImpl(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L597d6;
   L597d6: // MOV EDX,dword ptr [ESI + 0x4]
    edx=R(unchecked(esi+4u));goto L597d9;
   L597d9: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L597db;
   L597db: // JZ 0x00059835
    if(zero)goto L59835;goto L597dd;
   L597dd: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L597e0;
   L597e0: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L597e2;
   L597e2: // JZ 0x0005982c
    if(zero)goto L5982c;goto L597e4;
   L597e4: // XOR EDI,EDI
    a=edi;b=edi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edi=v;goto L597e6;
   L597e6: // MOV EBP,dword ptr [EBX + 0xfffffe9c]
    ebp=R(unchecked(ebx+4294966940u));goto L597ec;
   L597ec: // JMP 0x000597fb
    goto L597fb;
   L597f0: // MOV EDX,dword ptr [ESI + 0x4]
    edx=R(unchecked(esi+4u));goto L597f3;
   L597f3: // ADD EDI,0x1
    a=edi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L597f6;
   L597f6: // CMP dword ptr [ESI + 0x8],EDI
    a=R(unchecked(esi+8u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L597f9;
   L597f9: // JBE 0x00059828
    if(carry||zero)goto L59828;goto L597fb;
   L597fb: // MOV EAX,dword ptr [EDX + EDI*0x4]
    eax=R(unchecked(edx+edi*0x4u));goto L597fe;
   L597fe: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59800;
   L59800: // JZ 0x000597f3
    if(zero)goto L597f3;goto L59802;
   L59802: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59804;
   L59804: // JZ 0x000598c6
    if(zero)goto L598c6;goto L5980a;
   L5980a: // MOV EDX,0xffffffff
    edx=4294967295u;goto L5980f;
   L5980f: // XADD.LOCK dword ptr [EAX + 0x4],EDX
    a=R(unchecked(eax+4u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59814;
   L59814: // CMP EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59817;
   L59817: // JNZ 0x000597f0
    if(!zero)goto L597f0;goto L59819;
   L59819: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L5981b;
   L5981b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5981e;
   L5981e: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L598f1;}goto L59821;
   L59821: // JMP 0x000597f0
    goto L597f0;
   L59828: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5982a;
   L5982a: // JZ 0x00059835
    if(zero)goto L59835;goto L5982c;
   L5982c: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L5982f;
   L5982f: // NOP
    goto L59830;
   L59830: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L59835;
   L59835: // MOV EDX,dword ptr [ESI + 0xc]
    edx=R(unchecked(esi+12u));goto L59838;
   L59838: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5983a;
   L5983a: // JZ 0x0005988c
    if(zero)goto L5988c;goto L5983c;
   L5983c: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L5983f;
   L5983f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59841;
   L59841: // JZ 0x00059884
    if(zero)goto L59884;goto L59843;
   L59843: // XOR EDI,EDI
    a=edi;b=edi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edi=v;goto L59845;
   L59845: // MOV EBP,dword ptr [EBX + 0xfffffe9c]
    ebp=R(unchecked(ebx+4294966940u));goto L5984b;
   L5984b: // JMP 0x0005985b
    goto L5985b;
   L59850: // MOV EDX,dword ptr [ESI + 0xc]
    edx=R(unchecked(esi+12u));goto L59853;
   L59853: // ADD EDI,0x1
    a=edi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L59856;
   L59856: // CMP dword ptr [ESI + 0x8],EDI
    a=R(unchecked(esi+8u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59859;
   L59859: // JBE 0x00059880
    if(carry||zero)goto L59880;goto L5985b;
   L5985b: // MOV EAX,dword ptr [EDX + EDI*0x4]
    eax=R(unchecked(edx+edi*0x4u));goto L5985e;
   L5985e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59860;
   L59860: // JZ 0x00059853
    if(zero)goto L59853;goto L59862;
   L59862: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59864;
   L59864: // JZ 0x000598d4
    if(zero)goto L598d4;goto L59866;
   L59866: // MOV EDX,0xffffffff
    edx=4294967295u;goto L5986b;
   L5986b: // XADD.LOCK dword ptr [EAX + 0x4],EDX
    a=R(unchecked(eax+4u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59870;
   L59870: // CMP EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59873;
   L59873: // JNZ 0x00059850
    if(!zero)goto L59850;goto L59875;
   L59875: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L59877;
   L59877: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5987a;
   L5987a: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L598df;}goto L5987d;
   L5987d: // JMP 0x00059850
    goto L59850;
   L59880: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59882;
   L59882: // JZ 0x0005988c
    if(zero)goto L5988c;goto L59884;
   L59884: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L59887;
   L59887: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L5988c;
   L5988c: // MOV EDX,dword ptr [ESI + 0x10]
    edx=R(unchecked(esi+16u));goto L5988f;
   L5988f: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59891;
   L59891: // JZ 0x000598be
    if(zero)goto L598be;goto L59893;
   L59893: // XOR EDI,EDI
    a=edi;b=edi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edi=v;goto L59895;
   L59895: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L59898;
   L59898: // MOV EAX,dword ptr [EDX + EDI*0x1]
    eax=R(unchecked(edx+edi*0x1u));goto L5989b;
   L5989b: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5989d;
   L5989d: // JZ 0x000598aa
    if(zero)goto L598aa;goto L5989f;
   L5989f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L598a2;
   L598a2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L598a7;
   L598a7: // MOV EDX,dword ptr [ESI + 0x10]
    edx=R(unchecked(esi+16u));goto L598aa;
   L598aa: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L598ad;
   L598ad: // CMP EDI,0x30
    a=edi;b=48u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L598b0;
   L598b0: // JNZ 0x00059898
    if(!zero)goto L59898;goto L598b2;
   L598b2: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L598b4;
   L598b4: // JZ 0x000598be
    if(zero)goto L598be;goto L598b6;
   L598b6: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L598b9;
   L598b9: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L598be;
   L598be:return 0;
   L598c6: // MOV EDX,dword ptr [EAX + 0x4]
    edx=R(unchecked(eax+4u));goto L598c9;
   L598c9: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L598cc;
   L598cc: // MOV dword ptr [EAX + 0x4],ECX
    W(unchecked(eax+4u),ecx);goto L598cf;
   L598cf: // JMP 0x00059814
    goto L59814;
   L598d4: // MOV EDX,dword ptr [EAX + 0x4]
    edx=R(unchecked(eax+4u));goto L598d7;
   L598d7: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L598da;
   L598da: // MOV dword ptr [EAX + 0x4],ECX
    W(unchecked(eax+4u),ecx);goto L598dd;
   L598dd: // JMP 0x00059870
    goto L59870;
   L598df: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L598e2;
   L598e2: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L598e7;
   L598e7: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L59905;}goto L598ec;
   L598ec: // JMP 0x00059850
    goto L59850;
   L598f1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L598f4;
   L598f4: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L598f9;
   L598f9: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L59915;}goto L598fe;
   L598fe: // NOP
    goto L59900;
   L59900: // JMP 0x000597f0
    goto L597f0;
   L59905: // CMP EDX,-0x1
    a=edx;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59908;
   L59908: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5990b;
   L5990b: // JZ 0x00059917
    if(zero)goto L59917;goto L5990d;
   L5990d: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L59910;
   L59910: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L59915: // JMP 0x00059905
    goto L59905;
   L59917: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DisposeCallbacks(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ebp=owner;goto L58166;
   L58166: // MOV EAX,dword ptr [EBP + 0x18]
    eax=R(unchecked(ebp+24u));goto L58169;
   L58169: // MOV EDI,dword ptr [EBX + 0xfffffe9c]
    edi=R(unchecked(ebx+4294966940u));goto L5816f;
   L5816f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L58171;
   L58171: // JZ 0x000581a8
    if(zero)goto L581a8;goto L58173;
   L58173: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L58175;
   L58175: // JZ 0x00058199
    if(zero)goto L58199;goto L58177;
   L58177: // MOV EDX,0xffffffff
    edx=4294967295u;goto L5817c;
   L5817c: // XADD.LOCK dword ptr [EAX + 0xc],EDX
    a=R(unchecked(eax+12u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+12u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L58181;
   L58181: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L58183;
   L58183: // JNZ 0x000581a8
    if(!zero)goto L581a8;goto L58185;
   L58185: // MOV ESI,dword ptr [EAX]
    esi=R(unchecked(eax));goto L58187;
   L58187: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5818a;
   L5818a: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L5818f;
   L5818f: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L58191;
   L58191: // JZ 0x000581a8
    if(zero)goto L581a8;goto L58193;
   L58193: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L58195;
   L58195: // MOV EAX,ESI
    eax=esi;goto L58197;
   L58197: // JNZ 0x00058177
    if(!zero)goto L58177;goto L58199;
   L58199: // MOV EDX,dword ptr [EAX + 0xc]
    edx=R(unchecked(eax+12u));goto L5819c;
   L5819c: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L5819f;
   L5819f: // MOV dword ptr [EAX + 0xc],ECX
    W(unchecked(eax+12u),ecx);goto L581a2;
   L581a2: // JMP 0x00058181
    goto L58181;
   L581a8: // MOV dword ptr [EBP + 0x18],0x0
    W(unchecked(ebp+24u),0u);goto L581af;
   L581af:return 0;
  }
  public uint DestroyStringstream(uint frame,uint owner,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L912ce;
   L912ce: // MOV EAX,dword ptr [EBX + 0xfffffa60]
    eax=R(unchecked(ebx+4294965856u));goto L912d4;
   L912d4: // LEA EDX,[EAX + 0xc]
    edx=unchecked(eax+12u);goto L912d7;
   L912d7: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L912d9;
   L912d9: // LEA EDX,[EAX + 0x34]
    edx=unchecked(eax+52u);goto L912dc;
   L912dc: // ADD EAX,0x20
    a=eax;b=32u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L912df;
   L912df: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L912e2;
   L912e2: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L912e8;
   L912e8: // MOV dword ptr [ESI + 0x34],EDX
    W(unchecked(esi+52u),edx);goto L912eb;
   L912eb: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L912ee;
   L912ee: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L912f1;
   L912f1: // MOV EAX,dword ptr [ESI + 0x30]
    eax=R(unchecked(esi+48u));goto L912f4;
   L912f4: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L912f7;
   L912f7: // CMP EDX,dword ptr [EBX + 0xfffffc8c]
    a=edx;b=R(unchecked(ebx+4294966412u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L912fd;
   L912fd: // JNZ 0x0009135b
    if(!zero)goto L9135b;goto L912ff;
   L912ff: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L91305;
   L91305: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L91308;
   L91308: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L9130b;
   L9130b: // LEA EAX,[ESI + 0x28]
    eax=unchecked(esi+40u);goto L9130e;
   L9130e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L91311;
   L91311: // CALL 0x00049990
    eax=Call(0x49990,R(unchecked(esp+0u)));goto L91316;
   L91316: // MOV EDX,dword ptr [EBX + 0xfffffdc4]
    edx=R(unchecked(ebx+4294966724u));goto L9131c;
   L9131c: // LEA EAX,[ESI + 0x34]
    eax=unchecked(esi+52u);goto L9131f;
   L9131f: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L91326;
   L91326: // ADD EDX,0xc
    a=edx;b=12u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L91329;
   L91329: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L9132c;
   L9132c: // MOV EDX,dword ptr [EBX + 0xfffffe80]
    edx=R(unchecked(ebx+4294966912u));goto L91332;
   L91332: // ADD EDX,0xc
    a=edx;b=12u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L91335;
   L91335: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L91337;
   L91337: // MOV EDX,dword ptr [EBX + 0xfffffc40]
    edx=R(unchecked(ebx+4294966336u));goto L9133d;
   L9133d: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L91340;
   L91340: // MOV dword ptr [ESI + 0x34],EDX
    W(unchecked(esi+52u),edx);goto L91343;
   L91343: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L91346;
   L91346: // CALL 0x00049b70
    eax=Call(0x49b70,R(unchecked(esp+0u)));goto L9134b;
   L9134b:return 0;
   L9135b: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91362;
   L91362: // JZ 0x00091385
    if(zero)goto L91385;goto L91364;
   L91364: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L91367;
   L91367: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9136c;
   L9136c: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9136e;
   L9136e: // JG 0x000912ff
    if(!zero&&sign==overflow)goto L912ff;goto L91370;
   L91370: // LEA EAX,[ESP + 0x1f]
    eax=unchecked(esp+31u);goto L91374;
   L91374: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91378;
   L91378: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L9137b;
   L9137b: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L91380;
   L91380: // JMP 0x000912ff
    goto L912ff;
   L91385: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L91388;
   L91388: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L9138b;
   L9138b: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L9138e;
   L9138e: // JMP 0x0009136c
    goto L9136c;
  }
 }
}
