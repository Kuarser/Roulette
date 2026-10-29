using System;
namespace RouletteDeluxe
{
 // Physical packaged locale C1 0005b410. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Local calls receive frame/EAX/EDX/ECX snapshots, not an inferred C++ argument list; exception selector is preserved.
 public sealed class OriginalConfigurationLocaleConstructor
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken,ExceptionSelector;
  }
  readonly Host h;
  public OriginalConfigurationLocaleConstructor(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Construct(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0x1111,edi=0x2222,ecx=0,edx=0,ebp=0x3333,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x30u),owner);goto L5b422;
   L5b422: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto L5b426;
   L5b426: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L5b42a;
   L5b42a: // MOV dword ptr [ESP + 0x24],EDI
    W(unchecked(esp+36u),edi);goto L5b42e;
   L5b42e: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto L5b432;
   L5b432: // MOV dword ptr [ESI],0x0
    W(unchecked(esi),0u);goto L5b438;
   L5b438: // CALL 0x0005b360
    try{eax=Call(0x5b360,esp,eax,edx,ecx);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b4fe;}goto L5b43d;
   L5b43d: // MOV EDI,dword ptr [EBX + 0xfffffd84]
    edi=R(unchecked(ebx+4294966660u));goto L5b443;
   L5b443: // MOV EDX,dword ptr [EBX + 0xfffffc0c]
    edx=R(unchecked(ebx+4294966284u));goto L5b449;
   L5b449: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L5b44b;
   L5b44b: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L5b44d;
   L5b44d: // CMP EAX,dword ptr [EDX]
    a=eax;b=R(unchecked(edx));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5b44f;
   L5b44f: // JZ 0x0005b4a0
    if(zero)goto L5b4a0;goto L5b451;
   L5b451: // CALL 0x0005a150
    eax=Call(0x5a150,esp,eax,edx,ecx);goto L5b456;
   L5b456: // MOV EBP,EAX
    ebp=eax;goto L5b458;
   L5b458: // MOV EAX,dword ptr [EBX + 0xfffffe9c]
    eax=R(unchecked(ebx+4294966940u));goto L5b45e;
   L5b45e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b460;
   L5b460: // JZ 0x0005b4b0
    if(zero)goto L5b4b0;goto L5b462;
   L5b462: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L5b465;
   L5b465: // CALL 0x00049a80
    eax=Call(0x49a80,R(unchecked(esp+0u)));goto L5b46a;
   L5b46a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b46c;
   L5b46c: // JNZ 0x0005b4f9
    if(!zero)goto L5b4f9;goto L5b472;
   L5b472: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L5b474;
   L5b474: // ADD.LOCK dword ptr [EAX],0x1
    a=unchecked((uint)h.AtomicAdd(unchecked(eax),unchecked((int)1u)));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L5b478;
   L5b478: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L5b47a;
   L5b47a: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L5b47c;
   L5b47c: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L5b47f;
   L5b47f: // CALL 0x00049030
    eax=Call(0x49030,R(unchecked(esp+0u)));goto L5b484;
   L5b484: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b486;
   L5b486: // JNZ 0x0005b4b9
    if(!zero)goto L5b4b9;goto L5b488;
   L5b488:return 0;
   L5b4a0: // MOV EDX,dword ptr [EBX + 0xfffffe9c]
    edx=R(unchecked(ebx+4294966940u));goto L5b4a6;
   L5b4a6: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L5b4a8;
   L5b4a8: // JZ 0x0005b502
    if(zero)goto L5b502;goto L5b4aa;
   L5b4aa: // ADD.LOCK dword ptr [EAX],0x1
    a=unchecked((uint)h.AtomicAdd(unchecked(eax),unchecked((int)1u)));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L5b4ae;
   L5b4ae: // JMP 0x0005b488
    goto L5b488;
   L5b4b0: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L5b4b2;
   L5b4b2: // ADD dword ptr [EAX],0x1
    a=R(unchecked(eax));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(eax),v);goto L5b4b5;
   L5b4b5: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L5b4b7;
   L5b4b7: // JMP 0x0005b488
    goto L5b488;
   L5b4b9: // MOV dword ptr [ESP],0x4
    W(unchecked(esp),4u);goto L5b4c0;
   L5b4c0: // CALL 0x00048ec0
    eax=Call(0x48ec0,R(unchecked(esp+0u)));goto L5b4c5;
   L5b4c5: // MOV EDX,dword ptr [EBX + 0xfffffc18]
    edx=R(unchecked(ebx+4294966296u));goto L5b4cb;
   L5b4cb: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L5b4ce;
   L5b4ce: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L5b4d0;
   L5b4d0: // MOV EDX,dword ptr [EBX + 0xffffffd4]
    edx=R(unchecked(ebx+4294967252u));goto L5b4d6;
   L5b4d6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b4d9;
   L5b4d9: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L5b4dd;
   L5b4dd: // MOV EDX,dword ptr [EBX + 0xfffffb54]
    edx=R(unchecked(ebx+4294966100u));goto L5b4e3;
   L5b4e3: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5b4e7;
   L5b4e7: // CALL 0x00049370
    try{Call(0x49370,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b4ec;}
   L5b4ec: // ADD EDX,0x1
    a=edx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L5b4ef;
   L5b4ef: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b4f2;
   L5b4f2: // JZ 0x0005b507
    if(zero)goto L5b507;goto L5b4f4;
   L5b4f4: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L5b4f9: // CALL 0x0005a200
    try{Call(0x5a200,esp,eax,edx,ecx);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b4fe;}
   L5b4fe: // NOP
    goto L5b500;
   L5b500: // JMP 0x0005b4ec
    goto L5b4ec;
   L5b502: // ADD dword ptr [EAX],0x1
    a=R(unchecked(eax));b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(eax),v);goto L5b505;
   L5b505: // JMP 0x0005b488
    goto L5b488;
   L5b507: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
