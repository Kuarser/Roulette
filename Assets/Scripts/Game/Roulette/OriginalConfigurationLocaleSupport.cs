using System;
namespace RouletteDeluxe
{
 // Physical packaged locale assignment and initialization support. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Native guard, pthread, locale-Impl and exception owners remain required.
 public sealed class OriginalConfigurationLocaleSupport
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
  public OriginalConfigurationLocaleSupport(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Initialize(uint frame,uint owner,uint source)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5b36f;
   L5b36f: // MOV EAX,dword ptr [EBX + 0xfffffe9c]
    eax=R(unchecked(ebx+4294966940u));goto L5b375;
   L5b375: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b377;
   L5b377: // JZ 0x0005b391
    if(zero)goto L5b391;goto L5b379;
   L5b379: // MOV EAX,dword ptr [EBX + 0xffffff00]
    eax=R(unchecked(ebx+4294967040u));goto L5b37f;
   L5b37f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5b383;
   L5b383: // MOV EAX,dword ptr [EBX + 0xfffffd08]
    eax=R(unchecked(ebx+4294966536u));goto L5b389;
   L5b389: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b38c;
   L5b38c: // CALL 0x00048bf0
    eax=Call(0x48bf0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L5b391;
   L5b391: // MOV EAX,dword ptr [EBX + 0xfffffc0c]
    eax=R(unchecked(ebx+4294966284u));goto L5b397;
   L5b397: // MOV ECX,dword ptr [EAX]
    ecx=R(unchecked(eax));goto L5b399;
   L5b399: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b39b;
   L5b39b: // JZ 0x0005b3a8
    if(zero)goto L5b3a8;goto L5b39d;
   L5b39d:return 0;
   L5b3a8: // CALL 0x0005b310
    eax=Call(0x5b310);goto L5b3ad;
   L5b3ad:return 0;
  }
  public uint GetMutex(uint frame,uint owner,uint source)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5a162;
   L5a162: // MOV dword ptr [ESP + 0x38],ESI
    W(unchecked(esp+56u),esi);goto L5a166;
   L5a166: // CMP byte ptr [EBX + 0x1a5c],0x0
    a=B(unchecked(ebx+6748u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5a16d;
   L5a16d: // JZ 0x0005a188
    if(zero)goto L5a188;goto L5a16f;
   L5a16f: // LEA EAX,[EBX + 0x1a64]
    eax=unchecked(ebx+6756u);goto L5a175;
   L5a175:return eax;
   L5a188: // LEA ESI,[EBX + 0x1a5c]
    esi=unchecked(ebx+6748u);goto L5a18e;
   L5a18e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a191;
   L5a191: // CALL 0x00049800
    eax=Call(0x49800,R(unchecked(esp+0u)));goto L5a196;
   L5a196: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5a198;
   L5a198: // JZ 0x0005a16f
    if(zero)goto L5a16f;goto L5a19a;
   L5a19a: // MOV EAX,dword ptr [EBX + 0xfffffe9c]
    eax=R(unchecked(ebx+4294966940u));goto L5a1a0;
   L5a1a0: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5a1a2;
   L5a1a2: // JZ 0x0005a1f2
    if(zero)goto L5a1f2;goto L5a1a4;
   L5a1a4: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L5a1a6;
   L5a1a6: // MOV dword ptr [ESP + EAX*0x1 + 0x18],0x0
    W(unchecked(esp+eax*0x1u+24u),0u);goto L5a1ae;
   L5a1ae: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a1b1;
   L5a1b1: // CMP EAX,0x18
    a=eax;b=24u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5a1b4;
   L5a1b4: // JC 0x0005a1a6
    if(carry)goto L5a1a6;goto L5a1b6;
   L5a1b6: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L5a1ba;
   L5a1ba: // MOV dword ptr [EBX + 0x1a64],EAX
    W(unchecked(ebx+6756u),eax);goto L5a1c0;
   L5a1c0: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L5a1c4;
   L5a1c4: // MOV dword ptr [EBX + 0x1a68],EAX
    W(unchecked(ebx+6760u),eax);goto L5a1ca;
   L5a1ca: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L5a1ce;
   L5a1ce: // MOV dword ptr [EBX + 0x1a6c],EAX
    W(unchecked(ebx+6764u),eax);goto L5a1d4;
   L5a1d4: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L5a1d8;
   L5a1d8: // MOV dword ptr [EBX + 0x1a70],EAX
    W(unchecked(ebx+6768u),eax);goto L5a1de;
   L5a1de: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L5a1e2;
   L5a1e2: // MOV dword ptr [EBX + 0x1a74],EAX
    W(unchecked(ebx+6772u),eax);goto L5a1e8;
   L5a1e8: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L5a1ec;
   L5a1ec: // MOV dword ptr [EBX + 0x1a78],EAX
    W(unchecked(ebx+6776u),eax);goto L5a1f2;
   L5a1f2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a1f5;
   L5a1f5: // CALL 0x00048850
    eax=Call(0x48850,R(unchecked(esp+0u)));goto L5a1fa;
   L5a1fa: // JMP 0x0005a16f
    goto L5a16f;
  }
  public uint ThrowMutexError(uint frame,uint owner,uint source)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5a20f;
   L5a20f: // MOV dword ptr [ESP],0x4
    W(unchecked(esp),4u);goto L5a216;
   L5a216: // CALL 0x00048ec0
    eax=Call(0x48ec0,R(unchecked(esp+0u)));goto L5a21b;
   L5a21b: // MOV EDX,dword ptr [EBX + 0xffffff60]
    edx=R(unchecked(ebx+4294967136u));goto L5a221;
   L5a221: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L5a224;
   L5a224: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L5a226;
   L5a226: // MOV EDX,dword ptr [EBX + 0xfffffd54]
    edx=R(unchecked(ebx+4294966612u));goto L5a22c;
   L5a22c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5a22f;
   L5a22f: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L5a233;
   L5a233: // MOV EDX,dword ptr [EBX + 0xffffff80]
    edx=R(unchecked(ebx+4294967168u));goto L5a239;
   L5a239: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5a23d;
   L5a23d: // CALL 0x00049370
    Call(0x49370,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint Assign(uint frame,uint owner,uint source)
  {
   uint eax=owner,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x34u),source);goto L599a6;
   L599a6: // MOV dword ptr [ESP + 0x28],EDI
    W(unchecked(esp+40u),edi);goto L599aa;
   L599aa: // MOV EDI,dword ptr [ESP + 0x34]
    edi=R(unchecked(esp+52u));goto L599ae;
   L599ae: // MOV dword ptr [ESP + 0x24],ESI
    W(unchecked(esp+36u),esi);goto L599b2;
   L599b2: // MOV EDX,dword ptr [EDI]
    edx=R(unchecked(edi));goto L599b4;
   L599b4: // MOV ECX,dword ptr [EBX + 0xfffffe9c]
    ecx=R(unchecked(ebx+4294966940u));goto L599ba;
   L599ba: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L599bc;
   L599bc: // JZ 0x00059a06
    if(zero)goto L59a06;goto L599be;
   L599be: // ADD.LOCK dword ptr [EDX],0x1
    b=1u;a=unchecked((uint)h.AtomicAdd(unchecked(edx),unchecked((int)b)));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L599c2;
   L599c2: // MOV EDX,0xffffffff
    edx=4294967295u;goto L599c7;
   L599c7: // MOV ESI,dword ptr [EAX]
    esi=R(unchecked(eax));goto L599c9;
   L599c9: // XADD.LOCK dword ptr [ESI],EDX
    a=R(unchecked(esi));b=edx;v=unchecked((uint)h.AtomicAdd(unchecked(esi),unchecked((int)b)));edx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L599cd;
   L599cd: // CMP EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L599d0;
   L599d0: // JZ 0x000599e8
    if(zero)goto L599e8;goto L599d2;
   L599d2: // MOV EDX,dword ptr [EDI]
    edx=R(unchecked(edi));goto L599d4;
   L599d4: // MOV EBX,dword ptr [ESP + 0x20]
    ebx=R(unchecked(esp+32u));goto L599d8;
   L599d8: // MOV ESI,dword ptr [ESP + 0x24]
    esi=R(unchecked(esp+36u));goto L599dc;
   L599dc: // MOV EDI,dword ptr [ESP + 0x28]
    edi=R(unchecked(esp+40u));goto L599e0;
   L599e0: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L599e2;
   L599e2:return eax;
   L599e8: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L599ea;
   L599ea: // JZ 0x000599d2
    if(zero)goto L599d2;goto L599ec;
   L599ec: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L599f0;
   L599f0: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L599f3;
   L599f3: // CALL 0x000485b0
    eax=Call(0x485b0,R(unchecked(esp+0u)));goto L599f8;
   L599f8: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L599fb;
   L599fb: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L59a00;
   L59a00: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L59a04;
   L59a04: // JMP 0x000599d2
    goto L599d2;
   L59a06: // MOV ESI,dword ptr [EAX]
    esi=R(unchecked(eax));goto L59a08;
   L59a08: // ADD dword ptr [EDX],0x1
    a=R(unchecked(edx));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(edx),v);goto L59a0b;
   L59a0b: // MOV EDX,dword ptr [ESI]
    edx=R(unchecked(esi));goto L59a0d;
   L59a0d: // LEA ECX,[EDX + -0x1]
    ecx=unchecked(edx+4294967295u);goto L59a10;
   L59a10: // MOV dword ptr [ESI],ECX
    W(unchecked(esi),ecx);goto L59a12;
   L59a12: // JMP 0x000599cd
    goto L599cd;
  }
  public uint InitializeOnce(uint frame,uint owner,uint source)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5b322;
   L5b322: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L5b326;
   L5b326: // MOV dword ptr [ESP + 0x4],0x2
    W(unchecked(esp+4u),2u);goto L5b32e;
   L5b32e: // LEA ESI,[EBX + 0x1a80]
    esi=unchecked(ebx+6784u);goto L5b334;
   L5b334: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b337;
   L5b337: // CALL 0x00048cd0
    eax=Call(0x48cd0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L5b33c;
   L5b33c: // MOV EAX,dword ptr [EBX + 0xfffffc0c]
    eax=R(unchecked(ebx+4294966284u));goto L5b342;
   L5b342: // MOV dword ptr [EAX],ESI
    W(unchecked(eax),esi);goto L5b344;
   L5b344: // MOV EAX,dword ptr [EBX + 0xfffffd84]
    eax=R(unchecked(ebx+4294966660u));goto L5b34a;
   L5b34a: // MOV EBX,dword ptr [ESP + 0x14]
    ebx=R(unchecked(esp+20u));goto L5b34e;
   L5b34e: // MOV dword ptr [EAX],ESI
    W(unchecked(eax),esi);goto L5b350;
   L5b350:return 0;
  }
 }
}
