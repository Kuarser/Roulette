using System;
namespace RouletteDeluxe
{
 // Physical packaged locale facet core. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. STOSD uses the ABI-required clear direction flag. Local initializer receives ESP/EAX/EDX/ECX snapshots; heap, facet and platform owners remain required.
 public sealed class OriginalConfigurationLocaleFacetCore
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;public Func<uint,ushort> ReadHalf;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Action<uint,byte> WriteByte;public Action<uint,ushort> WriteHalf;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly Host h;
  public OriginalConfigurationLocaleFacetCore(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadHalf==null||host.WriteHalf==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadHalf=host.ReadHalf,WriteHalf=host.WriteHalf,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  uint H(uint at){return h.ReadHalf(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint GetId(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=owner;goto L59db0;
   L59db0: // MOV EAX,dword ptr [EDX]
    eax=R(unchecked(edx));goto L59db2;
   L59db2: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59db4;
   L59db4: // JZ 0x00059dc0
    if(zero)goto L59dc0;goto L59db6;
   L59db6: // SUB EAX,0x1
    a=eax;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L59db9;
   L59db9:return eax;
   L59dc0: // MOV ECX,dword ptr [EBX + 0xfffffe9c]
    ecx=R(unchecked(ebx+4294966940u));goto L59dc6;
   L59dc6: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59dc8;
   L59dc8: // JZ 0x00059de3
    if(zero)goto L59de3;goto L59dca;
   L59dca: // MOV EAX,dword ptr [EBX + 0xfffffe34]
    eax=R(unchecked(ebx+4294966836u));goto L59dd0;
   L59dd0: // MOV ECX,0x1
    ecx=1u;goto L59dd5;
   L59dd5: // XADD.LOCK dword ptr [EAX],ECX
    a=R(unchecked(eax));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59dd9;
   L59dd9: // LEA EAX,[ECX + 0x1]
    eax=unchecked(ecx+1u);goto L59ddc;
   L59ddc: // MOV dword ptr [EDX],EAX
    W(unchecked(edx),eax);goto L59dde;
   L59dde: // SUB EAX,0x1
    a=eax;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L59de1;
   L59de1:return eax;
   L59de3: // MOV ECX,dword ptr [EBX + 0xfffffe34]
    ecx=R(unchecked(ebx+4294966836u));goto L59de9;
   L59de9: // MOV EAX,dword ptr [ECX]
    eax=R(unchecked(ecx));goto L59deb;
   L59deb: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L59dee;
   L59dee: // MOV dword ptr [ECX],EAX
    W(unchecked(ecx),eax);goto L59df0;
   L59df0: // MOV dword ptr [EDX],EAX
    W(unchecked(edx),eax);goto L59df2;
   L59df2: // JMP 0x00059dde
    goto L59dde;
  }
  public uint Install(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=argument,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x40u,owner);W(frame+0x44u,source);W(frame+0x48u,argument);goto L59e16;
   L59e16: // MOV ESI,dword ptr [ESP + 0x40]
    esi=R(unchecked(esp+64u));goto L59e1a;
   L59e1a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59e1c;
   L59e1c: // JZ 0x00059ef0
    if(zero)goto L59ef0;goto L59e22;
   L59e22: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L59e26;
   L59e26: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L59e29;
   L59e29: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L59e2e;
   L59e2e: // MOV dword ptr [ESP + 0x10],EAX
    W(unchecked(esp+16u),eax);goto L59e32;
   L59e32: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L59e35;
   L59e35: // SUB EAX,0x1
    a=eax;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;eax=v;goto L59e38;
   L59e38: // CMP dword ptr [ESP + 0x10],EAX
    a=R(unchecked(esp+16u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59e3c;
   L59e3c: // JA 0x00059ef8
    if(!carry&&!zero)goto L59ef8;goto L59e42;
   L59e42: // MOV EAX,dword ptr [EBX + 0xfffffe9c]
    eax=R(unchecked(ebx+4294966940u));goto L59e48;
   L59e48: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59e4a;
   L59e4a: // JZ 0x0005a074
    if(zero)goto L5a074;goto L59e50;
   L59e50: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L59e54;
   L59e54: // ADD.LOCK dword ptr [EAX + 0x4],0x1
    b=1u;a=unchecked((uint)h.AtomicAdd(unchecked(eax+4u),unchecked((int)b)));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59e59;
   L59e59: // MOV EAX,dword ptr [ESI + 0x4]
    eax=R(unchecked(esi+4u));goto L59e5c;
   L59e5c: // MOV EDI,dword ptr [ESP + 0x10]
    edi=R(unchecked(esp+16u));goto L59e60;
   L59e60: // LEA EDI,[EAX + EDI*0x4]
    edi=unchecked(eax+edi*0x4u);goto L59e63;
   L59e63: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L59e65;
   L59e65: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59e67;
   L59e67: // JZ 0x00059fe0
    if(zero)goto L59fe0;goto L59e6d;
   L59e6d: // MOV EBP,dword ptr [EBX + 0xfffffe9c]
    ebp=R(unchecked(ebx+4294966940u));goto L59e73;
   L59e73: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59e75;
   L59e75: // JZ 0x0005a01d
    if(zero)goto L5a01d;goto L59e7b;
   L59e7b: // MOV EDX,0xffffffff
    edx=4294967295u;goto L59e80;
   L59e80: // XADD.LOCK dword ptr [EAX + 0x4],EDX
    a=R(unchecked(eax+4u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59e85;
   L59e85: // CMP EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59e88;
   L59e88: // JZ 0x00059ff0
    if(zero)goto L59ff0;goto L59e8e;
   L59e8e: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L59e92;
   L59e92: // MOV dword ptr [EDI],EAX
    W(unchecked(edi),eax);goto L59e94;
   L59e94: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L59e97;
   L59e97: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59e99;
   L59e99: // JZ 0x00059ef0
    if(zero)goto L59ef0;goto L59e9b;
   L59e9b: // XOR EDI,EDI
    a=edi;b=edi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edi=v;goto L59e9d;
   L59e9d: // JMP 0x00059eb2
    goto L59eb2;
   L59ea0: // MOV EAX,dword ptr [ESI + 0xc]
    eax=R(unchecked(esi+12u));goto L59ea3;
   L59ea3: // MOV dword ptr [EAX + EBP*0x1],0x0
    W(unchecked(eax+ebp*0x1u),0u);goto L59eaa;
   L59eaa: // ADD EDI,0x1
    a=edi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L59ead;
   L59ead: // CMP dword ptr [ESI + 0x8],EDI
    a=R(unchecked(esi+8u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59eb0;
   L59eb0: // JBE 0x00059ef0
    if(carry||zero)goto L59ef0;goto L59eb2;
   L59eb2: // MOV EAX,dword ptr [ESI + 0xc]
    eax=R(unchecked(esi+12u));goto L59eb5;
   L59eb5: // LEA EBP,[EDI*0x4 + 0x0]
    ebp=unchecked(edi*0x4u+0u);goto L59ebc;
   L59ebc: // MOV EAX,dword ptr [EAX + EDI*0x4]
    eax=R(unchecked(eax+edi*0x4u));goto L59ebf;
   L59ebf: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59ec1;
   L59ec1: // JZ 0x00059eaa
    if(zero)goto L59eaa;goto L59ec3;
   L59ec3: // MOV EDX,dword ptr [EBX + 0xfffffe9c]
    edx=R(unchecked(ebx+4294966940u));goto L59ec9;
   L59ec9: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59ecb;
   L59ecb: // JZ 0x00059ffd
    if(zero)goto L59ffd;goto L59ed1;
   L59ed1: // MOV EDX,0xffffffff
    edx=4294967295u;goto L59ed6;
   L59ed6: // XADD.LOCK dword ptr [EAX + 0x4],EDX
    a=R(unchecked(eax+4u));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4u),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L59edb;
   L59edb: // CMP EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59ede;
   L59ede: // JNZ 0x00059ea0
    if(!zero)goto L59ea0;goto L59ee0;
   L59ee0: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L59ee2;
   L59ee2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L59ee5;
   L59ee5: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a02b;}goto L59ee8;
   L59ee8: // JMP 0x00059ea0
    goto L59ea0;
   L59ef0:return 0;
   L59ef8: // MOV EAX,dword ptr [ESP + 0x10]
    eax=R(unchecked(esp+16u));goto L59efc;
   L59efc: // MOV EDX,dword ptr [ESI + 0x4]
    edx=R(unchecked(esi+4u));goto L59eff;
   L59eff: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L59f02;
   L59f02: // MOV EBP,EAX
    ebp=eax;goto L59f04;
   L59f04: // SHL EBP,0x2
    a=ebp;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;ebp=v;goto L59f07;
   L59f07: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L59f0b;
   L59f0b: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L59f0f;
   L59f0f: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L59f12;
   L59f12: // CALL 0x00048100
    eax=Call(0x48100,R(unchecked(esp+0u)));goto L59f17;
   L59f17: // MOV EDI,EAX
    edi=eax;goto L59f19;
   L59f19: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L59f1c;
   L59f1c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59f1e;
   L59f1e: // JZ 0x00059f38
    if(zero)goto L59f38;goto L59f20;
   L59f20: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L59f22;
   L59f22: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L59f28;
   L59f28: // MOV ECX,dword ptr [ESI + 0x4]
    ecx=R(unchecked(esi+4u));goto L59f2b;
   L59f2b: // MOV ECX,dword ptr [ECX + EDX*0x4]
    ecx=R(unchecked(ecx+edx*0x4u));goto L59f2e;
   L59f2e: // MOV dword ptr [EDI + EDX*0x4],ECX
    W(unchecked(edi+edx*0x4u),ecx);goto L59f31;
   L59f31: // ADD EDX,0x1
    a=edx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L59f34;
   L59f34: // CMP EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f36;
   L59f36: // JNZ 0x00059f28
    if(!zero)goto L59f28;goto L59f38;
   L59f38: // CMP dword ptr [ESP + 0x14],EAX
    a=R(unchecked(esp+20u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f3c;
   L59f3c: // JBE 0x00059f56
    if(carry||zero)goto L59f56;goto L59f3e;
   L59f3e: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L59f42;
   L59f42: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L59f48;
   L59f48: // MOV dword ptr [EDI + EAX*0x4],0x0
    W(unchecked(edi+eax*0x4u),0u);goto L59f4f;
   L59f4f: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L59f52;
   L59f52: // CMP EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f54;
   L59f54: // JA 0x00059f48
    if(!carry&&!zero)goto L59f48;goto L59f56;
   L59f56: // MOV EAX,dword ptr [ESI + 0xc]
    eax=R(unchecked(esi+12u));goto L59f59;
   L59f59: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L59f5c;
   L59f5c: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L59f60;
   L59f60: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a03d;}goto L59f65;
   L59f65: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L59f68;
   L59f68: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59f6a;
   L59f6a: // JZ 0x00059f81
    if(zero)goto L59f81;goto L59f6c;
   L59f6c: // XOR ECX,ECX
    a=ecx;b=ecx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L59f6e;
   L59f6e: // NOP
    goto L59f70;
   L59f70: // MOV EBP,dword ptr [ESI + 0xc]
    ebp=R(unchecked(esi+12u));goto L59f73;
   L59f73: // MOV EBP,dword ptr [EBP + ECX*0x4]
    ebp=R(unchecked(ebp+ecx*0x4u));goto L59f77;
   L59f77: // MOV dword ptr [EAX + ECX*0x4],EBP
    W(unchecked(eax+ecx*0x4u),ebp);goto L59f7a;
   L59f7a: // ADD ECX,0x1
    a=ecx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L59f7d;
   L59f7d: // CMP ECX,EDX
    a=ecx;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f7f;
   L59f7f: // JNZ 0x00059f70
    if(!zero)goto L59f70;goto L59f81;
   L59f81: // CMP dword ptr [ESP + 0x14],EDX
    a=R(unchecked(esp+20u));b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f85;
   L59f85: // JBE 0x00059f9e
    if(carry||zero)goto L59f9e;goto L59f87;
   L59f87: // MOV ECX,dword ptr [ESP + 0x14]
    ecx=R(unchecked(esp+20u));goto L59f8b;
   L59f8b: // NOP
    goto L59f8c;
   L59f8c: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L59f90;
   L59f90: // MOV dword ptr [EAX + EDX*0x4],0x0
    W(unchecked(eax+edx*0x4u),0u);goto L59f97;
   L59f97: // ADD EDX,0x1
    a=edx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L59f9a;
   L59f9a: // CMP ECX,EDX
    a=ecx;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L59f9c;
   L59f9c: // JA 0x00059f90
    if(!carry&&!zero)goto L59f90;goto L59f9e;
   L59f9e: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L59fa1;
   L59fa1: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L59fa5;
   L59fa5: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L59fa9;
   L59fa9: // MOV dword ptr [ESI + 0x4],EDI
    W(unchecked(esi+4u),edi);goto L59fac;
   L59fac: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59fae;
   L59fae: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L59fb1;
   L59fb1: // JZ 0x00059fbf
    if(zero)goto L59fbf;goto L59fb3;
   L59fb3: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L59fb7;
   L59fb7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L59fba;
   L59fba: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L59fbf;
   L59fbf: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L59fc3;
   L59fc3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59fc5;
   L59fc5: // JZ 0x00059e42
    if(zero)goto L59e42;goto L59fcb;
   L59fcb: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L59fcf;
   L59fcf: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L59fd2;
   L59fd2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L59fd7;
   L59fd7: // JMP 0x00059e42
    goto L59e42;
   L59fe0: // MOV EDX,dword ptr [ESP + 0x48]
    edx=R(unchecked(esp+72u));goto L59fe4;
   L59fe4: // MOV dword ptr [EDI],EDX
    W(unchecked(edi),edx);goto L59fe6;
   L59fe6: // JMP 0x00059e94
    goto L59e94;
   L59ff0: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L59ff2;
   L59ff2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L59ff5;
   L59ff5: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a00b;}goto L59ff8;
   L59ff8: // JMP 0x00059e8e
    goto L59e8e;
   L59ffd: // MOV EDX,dword ptr [EAX + 0x4]
    edx=R(unchecked(eax+4u));goto L5a000;
   L5a000: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L5a003;
   L5a003: // MOV dword ptr [EAX + 0x4],ECX
    W(unchecked(eax+4u),ecx);goto L5a006;
   L5a006: // JMP 0x00059edb
    goto L59edb;
   L5a00b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5a00e;
   L5a00e: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L5a013;
   L5a013: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a081;}goto L5a018;
   L5a018: // JMP 0x00059e8e
    goto L59e8e;
   L5a01d: // MOV EDX,dword ptr [EAX + 0x4]
    edx=R(unchecked(eax+4u));goto L5a020;
   L5a020: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L5a023;
   L5a023: // MOV dword ptr [EAX + 0x4],ECX
    W(unchecked(eax+4u),ecx);goto L5a026;
   L5a026: // JMP 0x00059e85
    goto L59e85;
   L5a02b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5a02e;
   L5a02e: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L5a033;
   L5a033: // CALL 0x000497e0
    try{eax=Call(0x497e0);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a056;}goto L5a038;
   L5a038: // JMP 0x00059ea0
    goto L59ea0;
   L5a03d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5a040;
   L5a040: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L5a045;
   L5a045: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5a047;
   L5a047: // JZ 0x0005a051
    if(zero)goto L5a051;goto L5a049;
   L5a049: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a04c;
   L5a04c: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L5a051;
   L5a051: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5a063;}
   L5a056: // ADD EDX,0x1
    a=edx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L5a059;
   L5a059: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5a05c;
   L5a05c: // JZ 0x0005a06f
    if(zero)goto L5a06f;goto L5a05e;
   L5a05e: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L5a063: // MOV ESI,EAX
    esi=eax;goto L5a065;
   L5a065: // CALL 0x000497e0
    eax=Call(0x497e0);goto L5a06a;
   L5a06a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a06d;
   L5a06d: // JMP 0x0005a05e
    goto L5a05e;
   L5a06f: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L5a074: // MOV EDX,dword ptr [ESP + 0x48]
    edx=R(unchecked(esp+72u));goto L5a078;
   L5a078: // ADD dword ptr [EDX + 0x4],0x1
    a=R(unchecked(edx+4u));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(edx+4u),v);goto L5a07c;
   L5a07c: // JMP 0x00059e59
    goto L59e59;
   L5a081: // JMP 0x0005a056
    goto L5a056;
  }
  public uint GetClassicName(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4;goto L597ab;
   L597ab: // MOV EAX,dword ptr [ECX + 0xfffffd74]
    eax=R(unchecked(ecx+4294966644u));goto L597b1;
   L597b1:return eax;
  }
  public uint GetClassicLocale(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L59762;
   L59762: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L59766;
   L59766: // MOV ESI,dword ptr [EBX + 0xfffffbdc]
    esi=R(unchecked(ebx+4294966236u));goto L5976c;
   L5976c: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L5976e;
   L5976e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L59770;
   L59770: // JZ 0x00059780
    if(zero)goto L59780;goto L59772;
   L59772:return eax;
   L59780: // CALL 0x00059710
    eax=Call(0x59710,esp,eax,edx,ecx);goto L59785;
   L59785: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L59787;
   L59787:return eax;
  }
  public uint DestroyFacetBase(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4;W(frame+4u,owner);goto L5919b;
   L5919b: // MOV EAX,dword ptr [ECX + 0xfffffb8c]
    eax=R(unchecked(ecx+4294966156u));goto L591a1;
   L591a1: // LEA EDX,[EAX + 0x8]
    edx=unchecked(eax+8u);goto L591a4;
   L591a4: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L591a8;
   L591a8: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L591aa;
   L591aa:return 0;
  }
  public uint ConstructCtype(uint frame,uint owner,uint source,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,source);W(frame+0x38u,argument);W(frame+0x3cu,references);goto L55168;
   L55168: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto L5516c;
   L5516c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L55170;
   L55170: // MOV dword ptr [ESP + 0x24],EDI
    W(unchecked(esp+36u),edi);goto L55174;
   L55174: // MOV EDI,dword ptr [ESP + 0x34]
    edi=R(unchecked(esp+52u));goto L55178;
   L55178: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5517a;
   L5517a: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L5517d;
   L5517d: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto L55181;
   L55181: // MOVZX EBP,byte ptr [ESP + 0x38]
    ebp=B(unchecked(esp+56u));goto L55186;
   L55186: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L55189;
   L55189: // MOV EAX,dword ptr [EBX + 0xfffffe90]
    eax=R(unchecked(ebx+4294966928u));goto L5518f;
   L5518f: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L55192;
   L55192: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L55194;
   L55194: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L552d0;}goto L55199;
   L55199: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5519b;
   L5519b: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L5519e;
   L5519e: // JZ 0x00055270
    if(zero)goto L55270;goto L551a4;
   L551a4: // MOV EDX,EBP
    edx=ebp;goto L551a6;
   L551a6: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L551a8;
   L551a8: // JNZ 0x00055258
    if(!zero)goto L55258;goto L551ae;
   L551ae: // MOV EDX,dword ptr [EAX + 0x3c]
    edx=R(unchecked(eax+60u));goto L551b1;
   L551b1: // MOV EAX,dword ptr [EAX + 0x38]
    eax=R(unchecked(eax+56u));goto L551b4;
   L551b4: // MOV byte ptr [ESI + 0xc],0x0
    h.WriteByte(unchecked(esi+12u),unchecked((byte)(0u)));goto L551b8;
   L551b8: // MOV dword ptr [ESI + 0x10],EDX
    W(unchecked(esi+16u),edx);goto L551bb;
   L551bb: // MOV dword ptr [ESI + 0x14],EAX
    W(unchecked(esi+20u),eax);goto L551be;
   L551be: // MOV dword ptr [ESI + 0x18],EDI
    W(unchecked(esi+24u),edi);goto L551c1;
   L551c1: // LEA EDI,[ESI + 0x1d]
    edi=unchecked(esi+29u);goto L551c4;
   L551c4: // MOV EDX,0x100
    edx=256u;goto L551c9;
   L551c9: // TEST EDI,0x1
    a=edi;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L551cf;
   L551cf: // MOV byte ptr [ESI + 0x1c],0x0
    h.WriteByte(unchecked(esi+28u),unchecked((byte)(0u)));goto L551d3;
   L551d3: // MOV byte ptr [ESI + 0x21d],0x0
    h.WriteByte(unchecked(esi+541u),unchecked((byte)(0u)));goto L551da;
   L551da: // JNZ 0x000552c0
    if(!zero)goto L552c0;goto L551e0;
   L551e0: // TEST EDI,0x2
    a=edi;b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L551e6;
   L551e6: // JNZ 0x000552b0
    if(!zero)goto L552b0;goto L551ec;
   L551ec: // MOV ECX,EDX
    ecx=edx;goto L551ee;
   L551ee: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L551f0;
   L551f0: // SHR ECX,0x2
    a=ecx;v=unchecked(a>>2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;ecx=v;goto L551f3;
   L551f3: // TEST DL,0x2
    a=(edx&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L551f6;
   L551f6: // STOSD.REP ES:EDI
    while(ecx!=0){W(edi,eax);edi=unchecked(edi+4u);ecx=unchecked(ecx-1u);}goto L551f8;
   L551f8: // JZ 0x00055202
    if(zero)goto L55202;goto L551fa;
   L551fa: // MOV word ptr [EDI],0x0
    h.WriteHalf(unchecked(edi),unchecked((ushort)(0u)));goto L551ff;
   L551ff: // ADD EDI,0x2
    a=edi;b=2u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L55202;
   L55202: // AND EDX,0x1
    a=edx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L55205;
   L55205: // JZ 0x0005520a
    if(zero)goto L5520a;goto L55207;
   L55207: // MOV byte ptr [EDI],0x0
    h.WriteByte(unchecked(edi),unchecked((byte)(0u)));goto L5520a;
   L5520a: // LEA EDI,[ESI + 0x11d]
    edi=unchecked(esi+285u);goto L55210;
   L55210: // MOV EDX,0x100
    edx=256u;goto L55215;
   L55215: // TEST EDI,0x1
    a=edi;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5521b;
   L5521b: // JNZ 0x00055298
    if(!zero)goto L55298;goto L5521d;
   L5521d: // TEST EDI,0x2
    a=edi;b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L55223;
   L55223: // JNZ 0x00055288
    if(!zero)goto L55288;goto L55225;
   L55225: // MOV ECX,EDX
    ecx=edx;goto L55227;
   L55227: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L55229;
   L55229: // SHR ECX,0x2
    a=ecx;v=unchecked(a>>2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&2u)!=0;overflow=false;ecx=v;goto L5522c;
   L5522c: // TEST DL,0x2
    a=(edx&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5522f;
   L5522f: // STOSD.REP ES:EDI
    while(ecx!=0){W(edi,eax);edi=unchecked(edi+4u);ecx=unchecked(ecx-1u);}goto L55231;
   L55231: // JZ 0x0005523b
    if(zero)goto L5523b;goto L55233;
   L55233: // MOV word ptr [EDI],0x0
    h.WriteHalf(unchecked(edi),unchecked((ushort)(0u)));goto L55238;
   L55238: // ADD EDI,0x2
    a=edi;b=2u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L5523b;
   L5523b: // AND EDX,0x1
    a=edx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L5523e;
   L5523e: // JZ 0x00055243
    if(zero)goto L55243;goto L55240;
   L55240: // MOV byte ptr [EDI],0x0
    h.WriteByte(unchecked(edi),unchecked((byte)(0u)));goto L55243;
   L55243:return 0;
   L55258: // MOV EDX,dword ptr [EAX + 0x3c]
    edx=R(unchecked(eax+60u));goto L5525b;
   L5525b: // MOV EAX,dword ptr [EAX + 0x38]
    eax=R(unchecked(eax+56u));goto L5525e;
   L5525e: // MOV byte ptr [ESI + 0xc],0x1
    h.WriteByte(unchecked(esi+12u),unchecked((byte)(1u)));goto L55262;
   L55262: // MOV dword ptr [ESI + 0x10],EDX
    W(unchecked(esi+16u),edx);goto L55265;
   L55265: // MOV dword ptr [ESI + 0x14],EAX
    W(unchecked(esi+20u),eax);goto L55268;
   L55268: // JMP 0x000551be
    goto L551be;
   L55270: // MOV EDX,dword ptr [EAX + 0x3c]
    edx=R(unchecked(eax+60u));goto L55273;
   L55273: // MOV byte ptr [ESI + 0xc],0x0
    h.WriteByte(unchecked(esi+12u),unchecked((byte)(0u)));goto L55277;
   L55277: // MOV EDI,dword ptr [EAX + 0x34]
    edi=R(unchecked(eax+52u));goto L5527a;
   L5527a: // MOV dword ptr [ESI + 0x10],EDX
    W(unchecked(esi+16u),edx);goto L5527d;
   L5527d: // MOV EDX,dword ptr [EAX + 0x38]
    edx=R(unchecked(eax+56u));goto L55280;
   L55280: // MOV dword ptr [ESI + 0x14],EDX
    W(unchecked(esi+20u),edx);goto L55283;
   L55283: // JMP 0x000551be
    goto L551be;
   L55288: // MOV word ptr [EDI],0x0
    h.WriteHalf(unchecked(edi),unchecked((ushort)(0u)));goto L5528d;
   L5528d: // SUB EDX,0x2
    a=edx;b=2u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L55290;
   L55290: // ADD EDI,0x2
    a=edi;b=2u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L55293;
   L55293: // JMP 0x00055225
    goto L55225;
   L55298: // MOV byte ptr [ESI + 0x11d],0x0
    h.WriteByte(unchecked(esi+285u),unchecked((byte)(0u)));goto L5529f;
   L5529f: // LEA EDI,[ESI + 0x11e]
    edi=unchecked(esi+286u);goto L552a5;
   L552a5: // MOV DX,0xff
    edx=(edx&0xffff0000u)|((255u)&65535u);goto L552a9;
   L552a9: // JMP 0x0005521d
    goto L5521d;
   L552b0: // MOV word ptr [EDI],0x0
    h.WriteHalf(unchecked(edi),unchecked((ushort)(0u)));goto L552b5;
   L552b5: // SUB EDX,0x2
    a=edx;b=2u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L552b8;
   L552b8: // ADD EDI,0x2
    a=edi;b=2u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L552bb;
   L552bb: // JMP 0x000551ec
    goto L551ec;
   L552c0: // MOV byte ptr [ESI + 0x1d],0x0
    h.WriteByte(unchecked(esi+29u),unchecked((byte)(0u)));goto L552c4;
   L552c4: // LEA EDI,[ESI + 0x1e]
    edi=unchecked(esi+30u);goto L552c7;
   L552c7: // MOV DX,0xff
    edx=(edx&0xffff0000u)|((255u)&65535u);goto L552cb;
   L552cb: // JMP 0x000551e0
    goto L551e0;
   L552d0: // MOV EDI,EAX
    edi=eax;goto L552d2;
   L552d2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L552d5;
   L552d5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L552da;
   L552da: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L552dd;
   L552dd: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
