using System;
namespace RouletteDeluxe
{
 // Physical packaged char/wchar domestic/international moneypunct initialization. Static C# labels
 // preserve original initialization control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Pattern returns use the native hidden destination and callee-pop ABI. Locale, conversion, heap, cache D0 and native exception owners remain required.
 public sealed class OriginalConfigurationMoneypunct
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
  public OriginalConfigurationMoneypunct(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint InitializeInternationalChar(uint frame,uint owner,uint locale,uint name)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x50u,owner);W(frame+0x54u,locale);W(frame+0x58u,name);goto L64466;
   L64466: // MOV EDI,dword ptr [ESP + 0x54]
    edi=R(unchecked(esp+84u));goto L6446a;
   L6446a: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L6446d;
   L6446d: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6446f;
   L6446f: // JZ 0x000648a0
    if(zero)goto L648a0;goto L64475;
   L64475: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64477;
   L64477: // JZ 0x00064718
    if(zero)goto L64718;goto L6447d;
   L6447d: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64481;
   L64481: // MOV dword ptr [ESP],0x40002
    W(unchecked(esp),262146u);goto L64488;
   L64488: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6448d;
   L6448d: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L64490;
   L64490: // MOV byte ptr [EBP + 0x11],AL
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)((eax&255u))));goto L64493;
   L64493: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L64496;
   L64496: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L6449a;
   L6449a: // MOV dword ptr [ESP],0x40003
    W(unchecked(esp),262147u);goto L644a1;
   L644a1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L644a6;
   L644a6: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L644a9;
   L644a9: // MOV byte ptr [EBP + 0x12],AL
    h.WriteByte(unchecked(ebp+18u),unchecked((byte)((eax&255u))));goto L644ac;
   L644ac: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L644af;
   L644af: // CMP byte ptr [EBP + 0x11],0x0
    a=B(unchecked(ebp+17u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L644b3;
   L644b3: // JNZ 0x00064680
    if(!zero)goto L64680;goto L644b9;
   L644b9: // MOV dword ptr [EBP + 0x2c],0x0
    W(unchecked(ebp+44u),0u);goto L644c0;
   L644c0: // MOV byte ptr [EBP + 0x11],0x2e
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(46u)));goto L644c4;
   L644c4: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L644c8;
   L644c8: // MOV dword ptr [ESP],0x40004
    W(unchecked(esp),262148u);goto L644cf;
   L644cf: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L644d4;
   L644d4: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L644d8;
   L644d8: // MOV dword ptr [ESP],0x40005
    W(unchecked(esp),262149u);goto L644df;
   L644df: // MOV EBP,EAX
    ebp=eax;goto L644e1;
   L644e1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L644e6;
   L644e6: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L644ea;
   L644ea: // MOV dword ptr [ESP],0x40006
    W(unchecked(esp),262150u);goto L644f1;
   L644f1: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L644f5;
   L644f5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L644fa;
   L644fa: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L644fe;
   L644fe: // MOV dword ptr [ESP],0x40000
    W(unchecked(esp),262144u);goto L64505;
   L64505: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L64509;
   L64509: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6450e;
   L6450e: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64512;
   L64512: // MOV dword ptr [ESP],0x40015
    W(unchecked(esp),262165u);goto L64519;
   L64519: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto L6451d;
   L6451d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64522;
   L64522: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64525;
   L64525: // CMP byte ptr [EDX + 0x12],0x0
    a=B(unchecked(edx+18u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64529;
   L64529: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L6452c;
   L6452c: // MOV byte ptr [ESP + 0x1f],AL
    h.WriteByte(unchecked(esp+31u),unchecked((byte)((eax&255u))));goto L64530;
   L64530: // JNZ 0x000646d8
    if(!zero)goto L646d8;goto L64536;
   L64536: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L6453c;
   L6453c: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L6453f;
   L6453f: // MOV dword ptr [EDX + 0xc],0x0
    W(unchecked(edx+12u),0u);goto L64546;
   L64546: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L6454a;
   L6454a: // MOV byte ptr [EDX + 0x12],0x2c
    h.WriteByte(unchecked(edx+18u),unchecked((byte)(44u)));goto L6454e;
   L6454e: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L64556;
   L64556: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L6455a;
   L6455a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6455d;
   L6455d: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L64562;
   L64562: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64564;
   L64564: // MOV EBP,EAX
    ebp=eax;goto L64566;
   L64566: // JNZ 0x000647e0
    if(!zero)goto L647e0;goto L6456c;
   L6456c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6456f;
   L6456f: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64575;
   L64575: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L6457d;
   L6457d: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L64580;
   L64580: // CMP byte ptr [ESP + 0x1f],0x0
    a=B(unchecked(esp+31u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64585;
   L64585: // MOV dword ptr [EDX + 0x20],EBP
    W(unchecked(edx+32u),ebp);goto L64588;
   L64588: // JNZ 0x000646a0
    if(!zero)goto L646a0;goto L6458e;
   L6458e: // LEA EAX,[EBX + 0xfffd8e5c]
    eax=unchecked(ebx+4294807132u);goto L64594;
   L64594: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L64597;
   L64597: // MOV dword ptr [EDX + 0x28],0x2
    W(unchecked(edx+40u),2u);goto L6459e;
   L6459e: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L645a6;
   L645a6: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L645aa;
   L645aa: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L645ae;
   L645ae: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L645b1;
   L645b1: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L645b6;
   L645b6: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L645ba;
   L645ba: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L645bc;
   L645bc: // MOV EBP,EAX
    ebp=eax;goto L645be;
   L645be: // JNZ 0x000647a0
    if(!zero)goto L647a0;goto L645c4;
   L645c4: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L645ca;
   L645ca: // MOV dword ptr [EDX + 0x14],EAX
    W(unchecked(edx+20u),eax);goto L645cd;
   L645cd: // MOV dword ptr [EDX + 0x18],EBP
    W(unchecked(edx+24u),ebp);goto L645d0;
   L645d0: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L645d4;
   L645d4: // MOV dword ptr [ESP],0x40010
    W(unchecked(esp),262160u);goto L645db;
   L645db: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L645e0;
   L645e0: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L645e3;
   L645e3: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L645e7;
   L645e7: // MOV dword ptr [ESP],0x40011
    W(unchecked(esp),262161u);goto L645ee;
   L645ee: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L645f3;
   L645f3: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L645f6;
   L645f6: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L645fa;
   L645fa: // MOV dword ptr [ESP],0x40014
    W(unchecked(esp),262164u);goto L64601;
   L64601: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L64605;
   L64605: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6460a;
   L6460a: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6460d;
   L6460d: // ADD EDX,0x30
    a=edx;b=48u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L64610;
   L64610: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64613;
   L64613: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L64616;
   L64616: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L6461a;
   L6461a: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L6461e;
   L6461e: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64622;
   L64622: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64626;
   L64626: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L6462b;
   L6462b: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L6462e;
   L6462e: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64632;
   L64632: // MOV dword ptr [ESP],0x40012
    W(unchecked(esp),262162u);goto L64639;
   L64639: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6463e;
   L6463e: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64641;
   L64641: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64645;
   L64645: // MOV dword ptr [ESP],0x40013
    W(unchecked(esp),262163u);goto L6464c;
   L6464c: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64651;
   L64651: // MOVSX ECX,byte ptr [ESP + 0x1f]
    ecx=unchecked((uint)(int)(sbyte)(B(unchecked(esp+31u))));goto L64656;
   L64656: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64659;
   L64659: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L6465d;
   L6465d: // ADD EDX,0x34
    a=edx;b=52u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L64660;
   L64660: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64663;
   L64663: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L64667;
   L64667: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L6466a;
   L6466a: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L6466e;
   L6466e: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L64673;
   L64673: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L64676;
   L64676:return 0;
   L64680: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64684;
   L64684: // MOV dword ptr [ESP],0x40007
    W(unchecked(esp),262151u);goto L6468b;
   L6468b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64690;
   L64690: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64693;
   L64693: // MOV dword ptr [EBP + 0x2c],EAX
    W(unchecked(ebp+44u),eax);goto L64696;
   L64696: // JMP 0x000644c4
    goto L644c4;
   L646a0: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L646a4;
   L646a4: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L646a8;
   L646a8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L646ab;
   L646ab: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L646b0;
   L646b0: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L646b4;
   L646b4: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L646b6;
   L646b6: // MOV EBP,EAX
    ebp=eax;goto L646b8;
   L646b8: // JNZ 0x00064820
    if(!zero)goto L64820;goto L646be;
   L646be: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L646c4;
   L646c4: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L646c7;
   L646c7: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L646cf;
   L646cf: // MOV dword ptr [EDX + 0x28],EBP
    W(unchecked(edx+40u),ebp);goto L646d2;
   L646d2: // JMP 0x000645a6
    goto L645a6;
   L646d8: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L646db;
   L646db: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L646df;
   L646df: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L646e4;
   L646e4: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L646e8;
   L646e8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L646ea;
   L646ea: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L646ee;
   L646ee: // JNZ 0x00064860
    if(!zero)goto L64860;goto L646f4;
   L646f4: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L646fa;
   L646fa: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L646fd;
   L646fd: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L64701;
   L64701: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L64709;
   L64709: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L6470d;
   L6470d: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L64710;
   L64710: // JMP 0x00064556
    goto L64556;
   L64718: // MOV byte ptr [EBP + 0x11],0x2e
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(46u)));goto L6471c;
   L6471c: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6471f;
   L6471f: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L64725;
   L64725: // MOV byte ptr [EAX + 0x12],0x2c
    h.WriteByte(unchecked(eax+18u),unchecked((byte)(44u)));goto L64729;
   L64729: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6472c;
   L6472c: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L6472f;
   L6472f: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L64732;
   L64732: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L64735;
   L64735: // MOV dword ptr [EAX + 0x24],EDX
    W(unchecked(eax+36u),edx);goto L64738;
   L64738: // MOV EDX,dword ptr [EBX + 0xfffffcd4]
    edx=R(unchecked(ebx+4294966484u));goto L6473e;
   L6473e: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L64745;
   L64745: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L64749;
   L64749: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L64750;
   L64750: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L64752;
   L64752: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L64759;
   L64759: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L64760;
   L64760: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L64767;
   L64767: // MOV dword ptr [EAX + 0x30],EDX
    W(unchecked(eax+48u),edx);goto L6476a;
   L6476a: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6476d;
   L6476d: // MOV dword ptr [EAX + 0x34],EDX
    W(unchecked(eax+52u),edx);goto L64770;
   L64770: // MOV EAX,dword ptr [EBX + 0xfffffecc]
    eax=R(unchecked(ebx+4294966988u));goto L64776;
   L64776: // MOV EDI,dword ptr [EAX]
    edi=R(unchecked(eax));goto L64778;
   L64778: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L6477a;
   L6477a: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L64780;
   L64780: // MOVZX ECX,byte ptr [EDI + EAX*0x1]
    ecx=B(unchecked(edi+eax*0x1u));goto L64784;
   L64784: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64787;
   L64787: // MOV byte ptr [EDX + EAX*0x1 + 0x38],CL
    h.WriteByte(unchecked(edx+eax*0x1u+56u),unchecked((byte)((ecx&255u))));goto L6478b;
   L6478b: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L6478e;
   L6478e: // CMP EAX,0xb
    a=eax;b=11u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64791;
   L64791: // JNZ 0x00064780
    if(!zero)goto L64780;goto L64793;
   L64793:return 0;
   L647a0: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L647a3;
   L647a3: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L647a7;
   L647a7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L647aa;
   L647aa: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L649a1;}goto L647af;
   L647af: // MOV ECX,EAX
    ecx=eax;goto L647b1;
   L647b1: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L647b5;
   L647b5: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L647b8;
   L647b8: // MOV dword ptr [ESP + 0x10],ECX
    W(unchecked(esp+16u),ecx);goto L647bc;
   L647bc: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L647c0;
   L647c0: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L647c4;
   L647c4: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L647c8;
   L647c8: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L647cd;
   L647cd: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L647d0;
   L647d0: // MOV ECX,dword ptr [ESP + 0x10]
    ecx=R(unchecked(esp+16u));goto L647d4;
   L647d4: // MOV dword ptr [EDX + 0x14],ECX
    W(unchecked(edx+20u),ecx);goto L647d7;
   L647d7: // JMP 0x000645cd
    goto L645cd;
   L647e0: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L647e3;
   L647e3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L647e7;
   L647e7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L647ea;
   L647ea: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L649bd;}goto L647ef;
   L647ef: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L647f3;
   L647f3: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L647f7;
   L647f7: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L647fb;
   L647fb: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L647ff;
   L647ff: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64803;
   L64803: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64807;
   L64807: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6480a;
   L6480a: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L6480f;
   L6480f: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64812;
   L64812: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64816;
   L64816: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L64819;
   L64819: // JMP 0x00064580
    goto L64580;
   L64820: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L64823;
   L64823: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L64827;
   L64827: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6482a;
   L6482a: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64938;}goto L6482f;
   L6482f: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L64833;
   L64833: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L64837;
   L64837: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L6483b;
   L6483b: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L6483f;
   L6483f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64843;
   L64843: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64847;
   L64847: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6484a;
   L6484a: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L6484f;
   L6484f: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64852;
   L64852: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64856;
   L64856: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L64859;
   L64859: // JMP 0x000646cf
    goto L646cf;
   L64860: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L64863;
   L64863: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L64867;
   L64867: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6486a;
   L6486a: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L649a3;}goto L6486f;
   L6486f: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L64873;
   L64873: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L64877;
   L64877: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L6487b;
   L6487b: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L6487f;
   L6487f: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64883;
   L64883: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64886;
   L64886: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L6488b;
   L6488b: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6488e;
   L6488e: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64892;
   L64892: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L64895;
   L64895: // JMP 0x00064709
    goto L64709;
   L648a0: // MOV dword ptr [ESP],0x44
    W(unchecked(esp),68u);goto L648a7;
   L648a7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L648ac;
   L648ac: // MOV EBP,EAX
    ebp=eax;goto L648ae;
   L648ae: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L648b5;
   L648b5: // MOV EAX,dword ptr [EBX + 0xffffff90]
    eax=R(unchecked(ebx+4294967184u));goto L648bb;
   L648bb: // MOV dword ptr [EBP + 0x8],0x0
    W(unchecked(ebp+8u),0u);goto L648c2;
   L648c2: // MOV dword ptr [EBP + 0xc],0x0
    W(unchecked(ebp+12u),0u);goto L648c9;
   L648c9: // MOV byte ptr [EBP + 0x10],0x0
    h.WriteByte(unchecked(ebp+16u),unchecked((byte)(0u)));goto L648cd;
   L648cd: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L648d0;
   L648d0: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto L648d3;
   L648d3: // MOV byte ptr [EBP + 0x11],0x0
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(0u)));goto L648d7;
   L648d7: // MOV byte ptr [EBP + 0x12],0x0
    h.WriteByte(unchecked(ebp+18u),unchecked((byte)(0u)));goto L648db;
   L648db: // MOV dword ptr [EBP + 0x14],0x0
    W(unchecked(ebp+20u),0u);goto L648e2;
   L648e2: // MOV dword ptr [EBP + 0x18],0x0
    W(unchecked(ebp+24u),0u);goto L648e9;
   L648e9: // MOV dword ptr [EBP + 0x1c],0x0
    W(unchecked(ebp+28u),0u);goto L648f0;
   L648f0: // MOV dword ptr [EBP + 0x20],0x0
    W(unchecked(ebp+32u),0u);goto L648f7;
   L648f7: // MOV dword ptr [EBP + 0x24],0x0
    W(unchecked(ebp+36u),0u);goto L648fe;
   L648fe: // MOV dword ptr [EBP + 0x28],0x0
    W(unchecked(ebp+40u),0u);goto L64905;
   L64905: // MOV dword ptr [EBP + 0x2c],0x0
    W(unchecked(ebp+44u),0u);goto L6490c;
   L6490c: // MOV byte ptr [EBP + 0x30],0x0
    h.WriteByte(unchecked(ebp+48u),unchecked((byte)(0u)));goto L64910;
   L64910: // MOV byte ptr [EBP + 0x31],0x0
    h.WriteByte(unchecked(ebp+49u),unchecked((byte)(0u)));goto L64914;
   L64914: // MOV byte ptr [EBP + 0x32],0x0
    h.WriteByte(unchecked(ebp+50u),unchecked((byte)(0u)));goto L64918;
   L64918: // MOV byte ptr [EBP + 0x33],0x0
    h.WriteByte(unchecked(ebp+51u),unchecked((byte)(0u)));goto L6491c;
   L6491c: // MOV byte ptr [EBP + 0x34],0x0
    h.WriteByte(unchecked(ebp+52u),unchecked((byte)(0u)));goto L64920;
   L64920: // MOV byte ptr [EBP + 0x35],0x0
    h.WriteByte(unchecked(ebp+53u),unchecked((byte)(0u)));goto L64924;
   L64924: // MOV byte ptr [EBP + 0x36],0x0
    h.WriteByte(unchecked(ebp+54u),unchecked((byte)(0u)));goto L64928;
   L64928: // MOV byte ptr [EBP + 0x37],0x0
    h.WriteByte(unchecked(ebp+55u),unchecked((byte)(0u)));goto L6492c;
   L6492c: // MOV byte ptr [EBP + 0x43],0x0
    h.WriteByte(unchecked(ebp+67u),unchecked((byte)(0u)));goto L64930;
   L64930: // MOV dword ptr [ESI + 0x8],EBP
    W(unchecked(esi+8u),ebp);goto L64933;
   L64933: // JMP 0x00064475
    goto L64475;
   L64938: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64940;
   L64940: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64943;
   L64943: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L64948;
   L64948: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6494b;
   L6494b: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6494d;
   L6494d: // JZ 0x00064957
    if(zero)goto L64957;goto L6494f;
   L6494f: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L64951;
   L64951: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64954;
   L64954: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L649d2;}goto L64957;
   L64957: // CMP dword ptr [ESP + 0x2c],0x0
    a=R(unchecked(esp+44u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6495c;
   L6495c: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L64963;
   L64963: // JZ 0x00064971
    if(zero)goto L64971;goto L64965;
   L64965: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64969;
   L64969: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6496c;
   L6496c: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64971;
   L64971: // CMP dword ptr [ESP + 0x18],0x0
    a=R(unchecked(esp+24u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64976;
   L64976: // JZ 0x00064984
    if(zero)goto L64984;goto L64978;
   L64978: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L6497c;
   L6497c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6497f;
   L6497f: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64984;
   L64984: // CMP dword ptr [ESP + 0x14],0x0
    a=R(unchecked(esp+20u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64989;
   L64989: // JZ 0x00064997
    if(zero)goto L64997;goto L6498b;
   L6498b: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L6498f;
   L6498f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64992;
   L64992: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64997;
   L64997: // CALL 0x000497e0
    eax=Call(0x497e0);goto L6499c;
   L6499c: // JMP 0x000645d0
    goto L645d0;
   L649a1: // JMP 0x00064940
    goto L64940;
   L649a3: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L649ab;
   L649ab: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L649b3;
   L649b3: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L649bb;
   L649bb: // JMP 0x00064940
    goto L64940;
   L649bd: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L649c5;
   L649c5: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L649cd;
   L649cd: // JMP 0x00064940
    goto L64940;
   L649d2: // MOV ESI,EAX
    esi=eax;goto L649d4;
   L649d4: // CALL 0x000497e0
    eax=Call(0x497e0);goto L649d9;
   L649d9: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L649dc;
   L649dc: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint InitializeDomesticChar(uint frame,uint owner,uint locale,uint name)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x50u,owner);W(frame+0x54u,locale);W(frame+0x58u,name);goto L64a06;
   L64a06: // MOV EDI,dword ptr [ESP + 0x54]
    edi=R(unchecked(esp+84u));goto L64a0a;
   L64a0a: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L64a0d;
   L64a0d: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64a0f;
   L64a0f: // JZ 0x00064e40
    if(zero)goto L64e40;goto L64a15;
   L64a15: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64a17;
   L64a17: // JZ 0x00064cb8
    if(zero)goto L64cb8;goto L64a1d;
   L64a1d: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a21;
   L64a21: // MOV dword ptr [ESP],0x40002
    W(unchecked(esp),262146u);goto L64a28;
   L64a28: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64a2d;
   L64a2d: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L64a30;
   L64a30: // MOV byte ptr [EBP + 0x11],AL
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)((eax&255u))));goto L64a33;
   L64a33: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L64a36;
   L64a36: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a3a;
   L64a3a: // MOV dword ptr [ESP],0x40003
    W(unchecked(esp),262147u);goto L64a41;
   L64a41: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64a46;
   L64a46: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L64a49;
   L64a49: // MOV byte ptr [EBP + 0x12],AL
    h.WriteByte(unchecked(ebp+18u),unchecked((byte)((eax&255u))));goto L64a4c;
   L64a4c: // MOV EBP,dword ptr [ESI + 0x8]
    ebp=R(unchecked(esi+8u));goto L64a4f;
   L64a4f: // CMP byte ptr [EBP + 0x11],0x0
    a=B(unchecked(ebp+17u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64a53;
   L64a53: // JNZ 0x00064c20
    if(!zero)goto L64c20;goto L64a59;
   L64a59: // MOV dword ptr [EBP + 0x2c],0x0
    W(unchecked(ebp+44u),0u);goto L64a60;
   L64a60: // MOV byte ptr [EBP + 0x11],0x2e
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(46u)));goto L64a64;
   L64a64: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a68;
   L64a68: // MOV dword ptr [ESP],0x40004
    W(unchecked(esp),262148u);goto L64a6f;
   L64a6f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64a74;
   L64a74: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a78;
   L64a78: // MOV dword ptr [ESP],0x40005
    W(unchecked(esp),262149u);goto L64a7f;
   L64a7f: // MOV EBP,EAX
    ebp=eax;goto L64a81;
   L64a81: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64a86;
   L64a86: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a8a;
   L64a8a: // MOV dword ptr [ESP],0x40006
    W(unchecked(esp),262150u);goto L64a91;
   L64a91: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L64a95;
   L64a95: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64a9a;
   L64a9a: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64a9e;
   L64a9e: // MOV dword ptr [ESP],0x40001
    W(unchecked(esp),262145u);goto L64aa5;
   L64aa5: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L64aa9;
   L64aa9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64aae;
   L64aae: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64ab2;
   L64ab2: // MOV dword ptr [ESP],0x4000e
    W(unchecked(esp),262158u);goto L64ab9;
   L64ab9: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto L64abd;
   L64abd: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64ac2;
   L64ac2: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64ac5;
   L64ac5: // CMP byte ptr [EDX + 0x12],0x0
    a=B(unchecked(edx+18u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64ac9;
   L64ac9: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L64acc;
   L64acc: // MOV byte ptr [ESP + 0x1f],AL
    h.WriteByte(unchecked(esp+31u),unchecked((byte)((eax&255u))));goto L64ad0;
   L64ad0: // JNZ 0x00064c78
    if(!zero)goto L64c78;goto L64ad6;
   L64ad6: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64adc;
   L64adc: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L64adf;
   L64adf: // MOV dword ptr [EDX + 0xc],0x0
    W(unchecked(edx+12u),0u);goto L64ae6;
   L64ae6: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L64aea;
   L64aea: // MOV byte ptr [EDX + 0x12],0x2c
    h.WriteByte(unchecked(edx+18u),unchecked((byte)(44u)));goto L64aee;
   L64aee: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L64af6;
   L64af6: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64afa;
   L64afa: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64afd;
   L64afd: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L64b02;
   L64b02: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64b04;
   L64b04: // MOV EBP,EAX
    ebp=eax;goto L64b06;
   L64b06: // JNZ 0x00064d80
    if(!zero)goto L64d80;goto L64b0c;
   L64b0c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64b0f;
   L64b0f: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64b15;
   L64b15: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L64b1d;
   L64b1d: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L64b20;
   L64b20: // CMP byte ptr [ESP + 0x1f],0x0
    a=B(unchecked(esp+31u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64b25;
   L64b25: // MOV dword ptr [EDX + 0x20],EBP
    W(unchecked(edx+32u),ebp);goto L64b28;
   L64b28: // JNZ 0x00064c40
    if(!zero)goto L64c40;goto L64b2e;
   L64b2e: // LEA EAX,[EBX + 0xfffd8e5c]
    eax=unchecked(ebx+4294807132u);goto L64b34;
   L64b34: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L64b37;
   L64b37: // MOV dword ptr [EDX + 0x28],0x2
    W(unchecked(edx+40u),2u);goto L64b3e;
   L64b3e: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64b46;
   L64b46: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L64b4a;
   L64b4a: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L64b4e;
   L64b4e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64b51;
   L64b51: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L64b56;
   L64b56: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L64b5a;
   L64b5a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64b5c;
   L64b5c: // MOV EBP,EAX
    ebp=eax;goto L64b5e;
   L64b5e: // JNZ 0x00064d40
    if(!zero)goto L64d40;goto L64b64;
   L64b64: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64b6a;
   L64b6a: // MOV dword ptr [EDX + 0x14],EAX
    W(unchecked(edx+20u),eax);goto L64b6d;
   L64b6d: // MOV dword ptr [EDX + 0x18],EBP
    W(unchecked(edx+24u),ebp);goto L64b70;
   L64b70: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64b74;
   L64b74: // MOV dword ptr [ESP],0x40009
    W(unchecked(esp),262153u);goto L64b7b;
   L64b7b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64b80;
   L64b80: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64b83;
   L64b83: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64b87;
   L64b87: // MOV dword ptr [ESP],0x4000a
    W(unchecked(esp),262154u);goto L64b8e;
   L64b8e: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64b93;
   L64b93: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64b96;
   L64b96: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64b9a;
   L64b9a: // MOV dword ptr [ESP],0x4000d
    W(unchecked(esp),262157u);goto L64ba1;
   L64ba1: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L64ba5;
   L64ba5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64baa;
   L64baa: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64bad;
   L64bad: // ADD EDX,0x30
    a=edx;b=48u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L64bb0;
   L64bb0: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64bb3;
   L64bb3: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L64bb6;
   L64bb6: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L64bba;
   L64bba: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L64bbe;
   L64bbe: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64bc2;
   L64bc2: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64bc6;
   L64bc6: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L64bcb;
   L64bcb: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L64bce;
   L64bce: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64bd2;
   L64bd2: // MOV dword ptr [ESP],0x4000b
    W(unchecked(esp),262155u);goto L64bd9;
   L64bd9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64bde;
   L64bde: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64be1;
   L64be1: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64be5;
   L64be5: // MOV dword ptr [ESP],0x4000c
    W(unchecked(esp),262156u);goto L64bec;
   L64bec: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64bf1;
   L64bf1: // MOVSX ECX,byte ptr [ESP + 0x1f]
    ecx=unchecked((uint)(int)(sbyte)(B(unchecked(esp+31u))));goto L64bf6;
   L64bf6: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64bf9;
   L64bf9: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L64bfd;
   L64bfd: // ADD EDX,0x34
    a=edx;b=52u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L64c00;
   L64c00: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64c03;
   L64c03: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L64c07;
   L64c07: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L64c0a;
   L64c0a: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64c0e;
   L64c0e: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L64c13;
   L64c13: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L64c16;
   L64c16:return 0;
   L64c20: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L64c24;
   L64c24: // MOV dword ptr [ESP],0x40008
    W(unchecked(esp),262152u);goto L64c2b;
   L64c2b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L64c30;
   L64c30: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L64c33;
   L64c33: // MOV dword ptr [EBP + 0x2c],EAX
    W(unchecked(ebp+44u),eax);goto L64c36;
   L64c36: // JMP 0x00064a64
    goto L64a64;
   L64c40: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L64c44;
   L64c44: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L64c48;
   L64c48: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64c4b;
   L64c4b: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L64c50;
   L64c50: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L64c54;
   L64c54: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64c56;
   L64c56: // MOV EBP,EAX
    ebp=eax;goto L64c58;
   L64c58: // JNZ 0x00064dc0
    if(!zero)goto L64dc0;goto L64c5e;
   L64c5e: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64c64;
   L64c64: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L64c67;
   L64c67: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64c6f;
   L64c6f: // MOV dword ptr [EDX + 0x28],EBP
    W(unchecked(edx+40u),ebp);goto L64c72;
   L64c72: // JMP 0x00064b46
    goto L64b46;
   L64c78: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L64c7b;
   L64c7b: // MOV dword ptr [ESP + 0x10],EDX
    W(unchecked(esp+16u),edx);goto L64c7f;
   L64c7f: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L64c84;
   L64c84: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L64c88;
   L64c88: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64c8a;
   L64c8a: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L64c8e;
   L64c8e: // JNZ 0x00064e00
    if(!zero)goto L64e00;goto L64c94;
   L64c94: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L64c9a;
   L64c9a: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L64c9d;
   L64c9d: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L64ca1;
   L64ca1: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L64ca9;
   L64ca9: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64cad;
   L64cad: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L64cb0;
   L64cb0: // JMP 0x00064af6
    goto L64af6;
   L64cb8: // MOV byte ptr [EBP + 0x11],0x2e
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(46u)));goto L64cbc;
   L64cbc: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L64cbf;
   L64cbf: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L64cc5;
   L64cc5: // MOV byte ptr [EAX + 0x12],0x2c
    h.WriteByte(unchecked(eax+18u),unchecked((byte)(44u)));goto L64cc9;
   L64cc9: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L64ccc;
   L64ccc: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L64ccf;
   L64ccf: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L64cd2;
   L64cd2: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L64cd5;
   L64cd5: // MOV dword ptr [EAX + 0x24],EDX
    W(unchecked(eax+36u),edx);goto L64cd8;
   L64cd8: // MOV EDX,dword ptr [EBX + 0xfffffcd4]
    edx=R(unchecked(ebx+4294966484u));goto L64cde;
   L64cde: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L64ce5;
   L64ce5: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L64ce9;
   L64ce9: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L64cf0;
   L64cf0: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L64cf2;
   L64cf2: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L64cf9;
   L64cf9: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L64d00;
   L64d00: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L64d07;
   L64d07: // MOV dword ptr [EAX + 0x30],EDX
    W(unchecked(eax+48u),edx);goto L64d0a;
   L64d0a: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L64d0d;
   L64d0d: // MOV dword ptr [EAX + 0x34],EDX
    W(unchecked(eax+52u),edx);goto L64d10;
   L64d10: // MOV EAX,dword ptr [EBX + 0xfffffecc]
    eax=R(unchecked(ebx+4294966988u));goto L64d16;
   L64d16: // MOV EDI,dword ptr [EAX]
    edi=R(unchecked(eax));goto L64d18;
   L64d18: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L64d1a;
   L64d1a: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L64d20;
   L64d20: // MOVZX ECX,byte ptr [EDI + EAX*0x1]
    ecx=B(unchecked(edi+eax*0x1u));goto L64d24;
   L64d24: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64d27;
   L64d27: // MOV byte ptr [EDX + EAX*0x1 + 0x38],CL
    h.WriteByte(unchecked(edx+eax*0x1u+56u),unchecked((byte)((ecx&255u))));goto L64d2b;
   L64d2b: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L64d2e;
   L64d2e: // CMP EAX,0xb
    a=eax;b=11u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64d31;
   L64d31: // JNZ 0x00064d20
    if(!zero)goto L64d20;goto L64d33;
   L64d33:return 0;
   L64d40: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L64d43;
   L64d43: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L64d47;
   L64d47: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64d4a;
   L64d4a: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64f41;}goto L64d4f;
   L64d4f: // MOV ECX,EAX
    ecx=eax;goto L64d51;
   L64d51: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L64d55;
   L64d55: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L64d58;
   L64d58: // MOV dword ptr [ESP + 0x10],ECX
    W(unchecked(esp+16u),ecx);goto L64d5c;
   L64d5c: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64d60;
   L64d60: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L64d64;
   L64d64: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64d68;
   L64d68: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L64d6d;
   L64d6d: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64d70;
   L64d70: // MOV ECX,dword ptr [ESP + 0x10]
    ecx=R(unchecked(esp+16u));goto L64d74;
   L64d74: // MOV dword ptr [EDX + 0x14],ECX
    W(unchecked(edx+20u),ecx);goto L64d77;
   L64d77: // JMP 0x00064b6d
    goto L64b6d;
   L64d80: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L64d83;
   L64d83: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L64d87;
   L64d87: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64d8a;
   L64d8a: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64f5d;}goto L64d8f;
   L64d8f: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L64d93;
   L64d93: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L64d97;
   L64d97: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64d9b;
   L64d9b: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64d9f;
   L64d9f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64da3;
   L64da3: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64da7;
   L64da7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64daa;
   L64daa: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L64daf;
   L64daf: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64db2;
   L64db2: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64db6;
   L64db6: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L64db9;
   L64db9: // JMP 0x00064b20
    goto L64b20;
   L64dc0: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L64dc3;
   L64dc3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L64dc7;
   L64dc7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64dca;
   L64dca: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64ed8;}goto L64dcf;
   L64dcf: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L64dd3;
   L64dd3: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L64dd7;
   L64dd7: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64ddb;
   L64ddb: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L64ddf;
   L64ddf: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64de3;
   L64de3: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64de7;
   L64de7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64dea;
   L64dea: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L64def;
   L64def: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64df2;
   L64df2: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64df6;
   L64df6: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L64df9;
   L64df9: // JMP 0x00064c6f
    goto L64c6f;
   L64e00: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L64e03;
   L64e03: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L64e07;
   L64e07: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64e0a;
   L64e0a: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64f43;}goto L64e0f;
   L64e0f: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L64e13;
   L64e13: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L64e17;
   L64e17: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L64e1b;
   L64e1b: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L64e1f;
   L64e1f: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64e23;
   L64e23: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64e26;
   L64e26: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L64e2b;
   L64e2b: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L64e2e;
   L64e2e: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64e32;
   L64e32: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L64e35;
   L64e35: // JMP 0x00064ca9
    goto L64ca9;
   L64e40: // MOV dword ptr [ESP],0x44
    W(unchecked(esp),68u);goto L64e47;
   L64e47: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L64e4c;
   L64e4c: // MOV EBP,EAX
    ebp=eax;goto L64e4e;
   L64e4e: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L64e55;
   L64e55: // MOV EAX,dword ptr [EBX + 0xfffffaac]
    eax=R(unchecked(ebx+4294965932u));goto L64e5b;
   L64e5b: // MOV dword ptr [EBP + 0x8],0x0
    W(unchecked(ebp+8u),0u);goto L64e62;
   L64e62: // MOV dword ptr [EBP + 0xc],0x0
    W(unchecked(ebp+12u),0u);goto L64e69;
   L64e69: // MOV byte ptr [EBP + 0x10],0x0
    h.WriteByte(unchecked(ebp+16u),unchecked((byte)(0u)));goto L64e6d;
   L64e6d: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L64e70;
   L64e70: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto L64e73;
   L64e73: // MOV byte ptr [EBP + 0x11],0x0
    h.WriteByte(unchecked(ebp+17u),unchecked((byte)(0u)));goto L64e77;
   L64e77: // MOV byte ptr [EBP + 0x12],0x0
    h.WriteByte(unchecked(ebp+18u),unchecked((byte)(0u)));goto L64e7b;
   L64e7b: // MOV dword ptr [EBP + 0x14],0x0
    W(unchecked(ebp+20u),0u);goto L64e82;
   L64e82: // MOV dword ptr [EBP + 0x18],0x0
    W(unchecked(ebp+24u),0u);goto L64e89;
   L64e89: // MOV dword ptr [EBP + 0x1c],0x0
    W(unchecked(ebp+28u),0u);goto L64e90;
   L64e90: // MOV dword ptr [EBP + 0x20],0x0
    W(unchecked(ebp+32u),0u);goto L64e97;
   L64e97: // MOV dword ptr [EBP + 0x24],0x0
    W(unchecked(ebp+36u),0u);goto L64e9e;
   L64e9e: // MOV dword ptr [EBP + 0x28],0x0
    W(unchecked(ebp+40u),0u);goto L64ea5;
   L64ea5: // MOV dword ptr [EBP + 0x2c],0x0
    W(unchecked(ebp+44u),0u);goto L64eac;
   L64eac: // MOV byte ptr [EBP + 0x30],0x0
    h.WriteByte(unchecked(ebp+48u),unchecked((byte)(0u)));goto L64eb0;
   L64eb0: // MOV byte ptr [EBP + 0x31],0x0
    h.WriteByte(unchecked(ebp+49u),unchecked((byte)(0u)));goto L64eb4;
   L64eb4: // MOV byte ptr [EBP + 0x32],0x0
    h.WriteByte(unchecked(ebp+50u),unchecked((byte)(0u)));goto L64eb8;
   L64eb8: // MOV byte ptr [EBP + 0x33],0x0
    h.WriteByte(unchecked(ebp+51u),unchecked((byte)(0u)));goto L64ebc;
   L64ebc: // MOV byte ptr [EBP + 0x34],0x0
    h.WriteByte(unchecked(ebp+52u),unchecked((byte)(0u)));goto L64ec0;
   L64ec0: // MOV byte ptr [EBP + 0x35],0x0
    h.WriteByte(unchecked(ebp+53u),unchecked((byte)(0u)));goto L64ec4;
   L64ec4: // MOV byte ptr [EBP + 0x36],0x0
    h.WriteByte(unchecked(ebp+54u),unchecked((byte)(0u)));goto L64ec8;
   L64ec8: // MOV byte ptr [EBP + 0x37],0x0
    h.WriteByte(unchecked(ebp+55u),unchecked((byte)(0u)));goto L64ecc;
   L64ecc: // MOV byte ptr [EBP + 0x43],0x0
    h.WriteByte(unchecked(ebp+67u),unchecked((byte)(0u)));goto L64ed0;
   L64ed0: // MOV dword ptr [ESI + 0x8],EBP
    W(unchecked(esi+8u),ebp);goto L64ed3;
   L64ed3: // JMP 0x00064a15
    goto L64a15;
   L64ed8: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64ee0;
   L64ee0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64ee3;
   L64ee3: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L64ee8;
   L64ee8: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L64eeb;
   L64eeb: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64eed;
   L64eed: // JZ 0x00064ef7
    if(zero)goto L64ef7;goto L64eef;
   L64eef: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L64ef1;
   L64ef1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64ef4;
   L64ef4: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64f72;}goto L64ef7;
   L64ef7: // CMP dword ptr [ESP + 0x2c],0x0
    a=R(unchecked(esp+44u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64efc;
   L64efc: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L64f03;
   L64f03: // JZ 0x00064f11
    if(zero)goto L64f11;goto L64f05;
   L64f05: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L64f09;
   L64f09: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64f0c;
   L64f0c: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64f11;
   L64f11: // CMP dword ptr [ESP + 0x18],0x0
    a=R(unchecked(esp+24u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64f16;
   L64f16: // JZ 0x00064f24
    if(zero)goto L64f24;goto L64f18;
   L64f18: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L64f1c;
   L64f1c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64f1f;
   L64f1f: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64f24;
   L64f24: // CMP dword ptr [ESP + 0x14],0x0
    a=R(unchecked(esp+20u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L64f29;
   L64f29: // JZ 0x00064f37
    if(zero)goto L64f37;goto L64f2b;
   L64f2b: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L64f2f;
   L64f2f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L64f32;
   L64f32: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64f37;
   L64f37: // CALL 0x000497e0
    eax=Call(0x497e0);goto L64f3c;
   L64f3c: // JMP 0x00064b70
    goto L64b70;
   L64f41: // JMP 0x00064ee0
    goto L64ee0;
   L64f43: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64f4b;
   L64f4b: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L64f53;
   L64f53: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L64f5b;
   L64f5b: // JMP 0x00064ee0
    goto L64ee0;
   L64f5d: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto L64f65;
   L64f65: // MOV dword ptr [ESP + 0x18],0x0
    W(unchecked(esp+24u),0u);goto L64f6d;
   L64f6d: // JMP 0x00064ee0
    goto L64ee0;
   L64f72: // MOV ESI,EAX
    esi=eax;goto L64f74;
   L64f74: // CALL 0x000497e0
    eax=Call(0x497e0);goto L64f79;
   L64f79: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L64f7c;
   L64f7c: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint InitializeInternationalWide(uint frame,uint owner,uint locale,uint name)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edi=owner;W(frame+0x70u,owner);W(frame+0x74u,locale);W(frame+0x78u,name);goto L65226;
   L65226: // MOV ESI,dword ptr [ESP + 0x74]
    esi=R(unchecked(esp+116u));goto L6522a;
   L6522a: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L6522d;
   L6522d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6522f;
   L6522f: // JZ 0x000656e0
    if(zero)goto L656e0;goto L65235;
   L65235: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65237;
   L65237: // JZ 0x00065518
    if(zero)goto L65518;goto L6523d;
   L6523d: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65240;
   L65240: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L65245;
   L65245: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65249;
   L65249: // MOV dword ptr [ESP],0x4002b
    W(unchecked(esp),262187u);goto L65250;
   L65250: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto L65254;
   L65254: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65259;
   L65259: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L6525c;
   L6525c: // MOV dword ptr [EDX + 0x14],EAX
    W(unchecked(edx+20u),eax);goto L6525f;
   L6525f: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65263;
   L65263: // MOV dword ptr [ESP],0x4002c
    W(unchecked(esp),262188u);goto L6526a;
   L6526a: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6526f;
   L6526f: // MOV EBP,dword ptr [EDI + 0x8]
    ebp=R(unchecked(edi+8u));goto L65272;
   L65272: // MOV dword ptr [EBP + 0x18],EAX
    W(unchecked(ebp+24u),eax);goto L65275;
   L65275: // MOV EAX,dword ptr [EBP + 0x14]
    eax=R(unchecked(ebp+20u));goto L65278;
   L65278: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6527a;
   L6527a: // JNZ 0x00065498
    if(!zero)goto L65498;goto L65280;
   L65280: // MOV dword ptr [EBP + 0x34],0x0
    W(unchecked(ebp+52u),0u);goto L65287;
   L65287: // MOV dword ptr [EBP + 0x14],0x2e
    W(unchecked(ebp+20u),46u);goto L6528e;
   L6528e: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65292;
   L65292: // MOV dword ptr [ESP],0x40004
    W(unchecked(esp),262148u);goto L65299;
   L65299: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6529e;
   L6529e: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L652a2;
   L652a2: // MOV dword ptr [ESP],0x40005
    W(unchecked(esp),262149u);goto L652a9;
   L652a9: // MOV EBP,EAX
    ebp=eax;goto L652ab;
   L652ab: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L652b0;
   L652b0: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L652b4;
   L652b4: // MOV dword ptr [ESP],0x40006
    W(unchecked(esp),262150u);goto L652bb;
   L652bb: // MOV dword ptr [ESP + 0x44],EAX
    W(unchecked(esp+68u),eax);goto L652bf;
   L652bf: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L652c4;
   L652c4: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L652c8;
   L652c8: // MOV dword ptr [ESP],0x40000
    W(unchecked(esp),262144u);goto L652cf;
   L652cf: // MOV dword ptr [ESP + 0x48],EAX
    W(unchecked(esp+72u),eax);goto L652d3;
   L652d3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L652d8;
   L652d8: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L652dc;
   L652dc: // MOV dword ptr [ESP],0x40015
    W(unchecked(esp),262165u);goto L652e3;
   L652e3: // MOV dword ptr [ESP + 0x4c],EAX
    W(unchecked(esp+76u),eax);goto L652e7;
   L652e7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L652ec;
   L652ec: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L652ef;
   L652ef: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L652f2;
   L652f2: // MOV byte ptr [ESP + 0x23],AL
    h.WriteByte(unchecked(esp+35u),unchecked((byte)((eax&255u))));goto L652f6;
   L652f6: // MOV EAX,dword ptr [EDX + 0x18]
    eax=R(unchecked(edx+24u));goto L652f9;
   L652f9: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L652fb;
   L652fb: // JNZ 0x000654d8
    if(!zero)goto L654d8;goto L65301;
   L65301: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L65307;
   L65307: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L6530a;
   L6530a: // MOV dword ptr [EDX + 0xc],0x0
    W(unchecked(edx+12u),0u);goto L65311;
   L65311: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L65315;
   L65315: // MOV dword ptr [EDX + 0x18],0x2c
    W(unchecked(edx+24u),44u);goto L6531c;
   L6531c: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L65324;
   L65324: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L65328;
   L65328: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L6532c;
   L6532c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6532f;
   L6532f: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L65334;
   L65334: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L65338;
   L65338: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6533a;
   L6533a: // JNZ 0x000655f0
    if(!zero)goto L655f0;goto L65340;
   L65340: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L65346;
   L65346: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L65348;
   L65348: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L6534b;
   L6534b: // MOV EAX,dword ptr [EDX + 0x24]
    eax=R(unchecked(edx+36u));goto L6534e;
   L6534e: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L65352;
   L65352: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65355;
   L65355: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L6535a;
   L6535a: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L6535e;
   L6535e: // MOV dword ptr [EDX + 0x28],EAX
    W(unchecked(edx+40u),eax);goto L65361;
   L65361: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L65365;
   L65365: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65368;
   L65368: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L6536d;
   L6536d: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L65371;
   L65371: // CMP byte ptr [ESP + 0x23],0x0
    a=B(unchecked(esp+35u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65376;
   L65376: // JNZ 0x000654b8
    if(!zero)goto L654b8;goto L6537c;
   L6537c: // LEA EAX,[EBX + 0xfffd8e60]
    eax=unchecked(ebx+4294807136u);goto L65382;
   L65382: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L65385;
   L65385: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L6538d;
   L6538d: // MOV EAX,dword ptr [EDX + 0x2c]
    eax=R(unchecked(edx+44u));goto L65390;
   L65390: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L65394;
   L65394: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65397;
   L65397: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L6539c;
   L6539c: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L653a0;
   L653a0: // MOV dword ptr [EDX + 0x30],EAX
    W(unchecked(edx+48u),eax);goto L653a3;
   L653a3: // MOV EAX,dword ptr [ESP + 0x4c]
    eax=R(unchecked(esp+76u));goto L653a7;
   L653a7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L653aa;
   L653aa: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L653af;
   L653af: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L653b3;
   L653b3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L653b5;
   L653b5: // JNZ 0x000655a0
    if(!zero)goto L655a0;goto L653bb;
   L653bb: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L653c1;
   L653c1: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L653c4;
   L653c4: // MOV EAX,dword ptr [EDX + 0x1c]
    eax=R(unchecked(edx+28u));goto L653c7;
   L653c7: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L653cb;
   L653cb: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L653ce;
   L653ce: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L653d3;
   L653d3: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L653d7;
   L653d7: // MOV dword ptr [EDX + 0x20],EAX
    W(unchecked(edx+32u),eax);goto L653da;
   L653da: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L653de;
   L653de: // MOV dword ptr [ESP],0x40010
    W(unchecked(esp),262160u);goto L653e5;
   L653e5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L653ea;
   L653ea: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L653ed;
   L653ed: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L653f1;
   L653f1: // MOV dword ptr [ESP],0x40011
    W(unchecked(esp),262161u);goto L653f8;
   L653f8: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L653fd;
   L653fd: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65400;
   L65400: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65404;
   L65404: // MOV dword ptr [ESP],0x40014
    W(unchecked(esp),262164u);goto L6540b;
   L6540b: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L6540f;
   L6540f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65414;
   L65414: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65417;
   L65417: // ADD EDX,0x38
    a=edx;b=56u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L6541a;
   L6541a: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L6541d;
   L6541d: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65420;
   L65420: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L65424;
   L65424: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L65428;
   L65428: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L6542c;
   L6542c: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65430;
   L65430: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L65435;
   L65435: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L65438;
   L65438: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L6543c;
   L6543c: // MOV dword ptr [ESP],0x40012
    W(unchecked(esp),262162u);goto L65443;
   L65443: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65448;
   L65448: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L6544b;
   L6544b: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L6544f;
   L6544f: // MOV dword ptr [ESP],0x40013
    W(unchecked(esp),262163u);goto L65456;
   L65456: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6545b;
   L6545b: // MOVSX ECX,byte ptr [ESP + 0x23]
    ecx=unchecked((uint)(int)(sbyte)(B(unchecked(esp+35u))));goto L65460;
   L65460: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65463;
   L65463: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L65467;
   L65467: // ADD EDX,0x3c
    a=edx;b=60u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L6546a;
   L6546a: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L6546d;
   L6546d: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L65471;
   L65471: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65474;
   L65474: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65478;
   L65478: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L6547d;
   L6547d: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L65480;
   L65480: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L65484;
   L65484: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65487;
   L65487: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L6548c;
   L6548c:return 0;
   L65498: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L6549c;
   L6549c: // MOV dword ptr [ESP],0x40007
    W(unchecked(esp),262151u);goto L654a3;
   L654a3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L654a8;
   L654a8: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L654ab;
   L654ab: // MOV dword ptr [EBP + 0x34],EAX
    W(unchecked(ebp+52u),eax);goto L654ae;
   L654ae: // JMP 0x0006528e
    goto L6528e;
   L654b8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L654ba;
   L654ba: // JNZ 0x00065640
    if(!zero)goto L65640;goto L654c0;
   L654c0: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L654c6;
   L654c6: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L654c9;
   L654c9: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L654d1;
   L654d1: // JMP 0x0006538d
    goto L6538d;
   L654d8: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L654db;
   L654db: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L654df;
   L654df: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L654e4;
   L654e4: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L654e8;
   L654e8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L654ea;
   L654ea: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L654ee;
   L654ee: // JNZ 0x000656a0
    if(!zero)goto L656a0;goto L654f4;
   L654f4: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L654fa;
   L654fa: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L654fd;
   L654fd: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L65501;
   L65501: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L65509;
   L65509: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L6550d;
   L6550d: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L65510;
   L65510: // JMP 0x00065324
    goto L65324;
   L65518: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L6551b;
   L6551b: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L65521;
   L65521: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L65524;
   L65524: // LEA EDX,[EBX + 0xfffd8e68]
    edx=unchecked(ebx+4294807144u);goto L6552a;
   L6552a: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L6552d;
   L6552d: // MOV dword ptr [EAX + 0x24],EDX
    W(unchecked(eax+36u),edx);goto L65530;
   L65530: // MOV dword ptr [EAX + 0x2c],EDX
    W(unchecked(eax+44u),edx);goto L65533;
   L65533: // MOV EDX,dword ptr [EBX + 0xfffffcd4]
    edx=R(unchecked(ebx+4294966484u));goto L65539;
   L65539: // MOV dword ptr [EAX + 0x14],0x2e
    W(unchecked(eax+20u),46u);goto L65540;
   L65540: // MOV dword ptr [EAX + 0x18],0x2c
    W(unchecked(eax+24u),44u);goto L65547;
   L65547: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L6554e;
   L6554e: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L65550;
   L65550: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L65554;
   L65554: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L6555b;
   L6555b: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L65562;
   L65562: // MOV dword ptr [EAX + 0x38],EDX
    W(unchecked(eax+56u),edx);goto L65565;
   L65565: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L6556c;
   L6556c: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L65573;
   L65573: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65576;
   L65576: // MOV dword ptr [EAX + 0x3c],EDX
    W(unchecked(eax+60u),edx);goto L65579;
   L65579: // MOV EAX,dword ptr [EBX + 0xfffffecc]
    eax=R(unchecked(ebx+4294966988u));goto L6557f;
   L6557f: // MOV ESI,dword ptr [EDI + 0x8]
    esi=R(unchecked(edi+8u));goto L65582;
   L65582: // MOV ECX,dword ptr [EAX]
    ecx=R(unchecked(eax));goto L65584;
   L65584: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L65586;
   L65586: // NOP
    goto L65588;
   L65588: // MOVSX EDX,byte ptr [ECX + EAX*0x1]
    edx=unchecked((uint)(int)(sbyte)(B(unchecked(ecx+eax*0x1u))));goto L6558c;
   L6558c: // MOV dword ptr [ESI + EAX*0x4 + 0x40],EDX
    W(unchecked(esi+eax*0x4u+64u),edx);goto L65590;
   L65590: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65593;
   L65593: // CMP EAX,0xb
    a=eax;b=11u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65596;
   L65596: // JNZ 0x00065588
    if(!zero)goto L65588;goto L65598;
   L65598:return 0;
   L655a0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L655a3;
   L655a3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L655a7;
   L655a7: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L655aa;
   L655aa: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L655b2;
   L655b2: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L655ba;
   L655ba: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L655bd;
   L655bd: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L657e4;}goto L655c2;
   L655c2: // MOV EBP,EAX
    ebp=eax;goto L655c4;
   L655c4: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L655c8;
   L655c8: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L655cc;
   L655cc: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L655d0;
   L655d0: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L655d3;
   L655d3: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L655d7;
   L655d7: // LEA EAX,[ESP + 0x4c]
    eax=unchecked(esp+76u);goto L655db;
   L655db: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L655df;
   L655df: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L655e4;
   L655e4: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L655e7;
   L655e7: // MOV dword ptr [EDX + 0x1c],EBP
    W(unchecked(edx+28u),ebp);goto L655ea;
   L655ea: // JMP 0x000653c4
    goto L653c4;
   L655f0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L655f3;
   L655f3: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L655f7;
   L655f7: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L655fa;
   L655fa: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L65602;
   L65602: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L6560a;
   L6560a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6560d;
   L6560d: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L657fa;}goto L65612;
   L65612: // MOV EBP,EAX
    ebp=eax;goto L65614;
   L65614: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L65618;
   L65618: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L6561c;
   L6561c: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65620;
   L65620: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65623;
   L65623: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65627;
   L65627: // LEA EAX,[ESP + 0x44]
    eax=unchecked(esp+68u);goto L6562b;
   L6562b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6562f;
   L6562f: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L65634;
   L65634: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65637;
   L65637: // MOV dword ptr [EDX + 0x24],EBP
    W(unchecked(edx+36u),ebp);goto L6563a;
   L6563a: // JMP 0x0006534b
    goto L6534b;
   L65640: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65643;
   L65643: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L65647;
   L65647: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L6564a;
   L6564a: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L65652;
   L65652: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L6565a;
   L6565a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6565d;
   L6565d: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6577b;}goto L65662;
   L65662: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L65666;
   L65666: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L6566a;
   L6566a: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L6566e;
   L6566e: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L65672;
   L65672: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65676;
   L65676: // LEA EAX,[ESP + 0x48]
    eax=unchecked(esp+72u);goto L6567a;
   L6567a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6567e;
   L6567e: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65682;
   L65682: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65685;
   L65685: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L6568a;
   L6568a: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L6568d;
   L6568d: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65691;
   L65691: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L65694;
   L65694: // JMP 0x0006538d
    goto L6538d;
   L656a0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L656a3;
   L656a3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L656a7;
   L656a7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L656aa;
   L656aa: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L657e6;}goto L656af;
   L656af: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L656b3;
   L656b3: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L656b7;
   L656b7: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L656bb;
   L656bb: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L656bf;
   L656bf: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L656c3;
   L656c3: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L656c6;
   L656c6: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L656cb;
   L656cb: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L656ce;
   L656ce: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L656d2;
   L656d2: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L656d5;
   L656d5: // JMP 0x00065509
    goto L65509;
   L656e0: // MOV dword ptr [ESP],0x70
    W(unchecked(esp),112u);goto L656e7;
   L656e7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L656ec;
   L656ec: // MOV EDX,dword ptr [EBX + 0xfffffc04]
    edx=R(unchecked(ebx+4294966276u));goto L656f2;
   L656f2: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L656f5;
   L656f5: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L656fc;
   L656fc: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L656fe;
   L656fe: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L65705;
   L65705: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L6570c;
   L6570c: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L65710;
   L65710: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L65717;
   L65717: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L6571e;
   L6571e: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L65725;
   L65725: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L6572c;
   L6572c: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L65733;
   L65733: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L6573a;
   L6573a: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L65741;
   L65741: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L65748;
   L65748: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L6574f;
   L6574f: // MOV byte ptr [EAX + 0x38],0x0
    h.WriteByte(unchecked(eax+56u),unchecked((byte)(0u)));goto L65753;
   L65753: // MOV byte ptr [EAX + 0x39],0x0
    h.WriteByte(unchecked(eax+57u),unchecked((byte)(0u)));goto L65757;
   L65757: // MOV byte ptr [EAX + 0x3a],0x0
    h.WriteByte(unchecked(eax+58u),unchecked((byte)(0u)));goto L6575b;
   L6575b: // MOV byte ptr [EAX + 0x3b],0x0
    h.WriteByte(unchecked(eax+59u),unchecked((byte)(0u)));goto L6575f;
   L6575f: // MOV byte ptr [EAX + 0x3c],0x0
    h.WriteByte(unchecked(eax+60u),unchecked((byte)(0u)));goto L65763;
   L65763: // MOV byte ptr [EAX + 0x3d],0x0
    h.WriteByte(unchecked(eax+61u),unchecked((byte)(0u)));goto L65767;
   L65767: // MOV byte ptr [EAX + 0x3e],0x0
    h.WriteByte(unchecked(eax+62u),unchecked((byte)(0u)));goto L6576b;
   L6576b: // MOV byte ptr [EAX + 0x3f],0x0
    h.WriteByte(unchecked(eax+63u),unchecked((byte)(0u)));goto L6576f;
   L6576f: // MOV byte ptr [EAX + 0x6c],0x0
    h.WriteByte(unchecked(eax+108u),unchecked((byte)(0u)));goto L65773;
   L65773: // MOV dword ptr [EDI + 0x8],EAX
    W(unchecked(edi+8u),eax);goto L65776;
   L65776: // JMP 0x00065235
    goto L65235;
   L6577b: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65783;
   L65783: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65786;
   L65786: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L6578b;
   L6578b: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L6578e;
   L6578e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65790;
   L65790: // JZ 0x0006579a
    if(zero)goto L6579a;goto L65792;
   L65792: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L65794;
   L65794: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65797;
   L65797: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}goto L6579a;
   L6579a: // CMP dword ptr [ESP + 0x2c],0x0
    a=R(unchecked(esp+44u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6579f;
   L6579f: // MOV dword ptr [EDI + 0x8],0x0
    W(unchecked(edi+8u),0u);goto L657a6;
   L657a6: // JZ 0x000657b4
    if(zero)goto L657b4;goto L657a8;
   L657a8: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L657ac;
   L657ac: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L657af;
   L657af: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}goto L657b4;
   L657b4: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L657b6;
   L657b6: // JZ 0x000657c0
    if(zero)goto L657c0;goto L657b8;
   L657b8: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L657bb;
   L657bb: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}goto L657c0;
   L657c0: // CMP dword ptr [ESP + 0x1c],0x0
    a=R(unchecked(esp+28u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L657c5;
   L657c5: // JZ 0x000657d3
    if(zero)goto L657d3;goto L657c7;
   L657c7: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L657cb;
   L657cb: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L657ce;
   L657ce: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}goto L657d3;
   L657d3: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L657d7;
   L657d7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L657da;
   L657da: // CALL 0x00049aa0
    try{eax=Call(0x49aa0,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}goto L657df;
   L657df: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65809;}
   L657e4: // JMP 0x00065783
    goto L65783;
   L657e6: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L657ee;
   L657ee: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L657f0;
   L657f0: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L657f8;
   L657f8: // JMP 0x00065783
    goto L65783;
   L657fa: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65802;
   L65802: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L65804;
   L65804: // JMP 0x00065783
    goto L65783;
   L65809: // MOV ESI,EAX
    esi=eax;goto L6580b;
   L6580b: // CALL 0x000497e0
    eax=Call(0x497e0);goto L65810;
   L65810: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65813;
   L65813: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint InitializeDomesticWide(uint frame,uint owner,uint locale,uint name)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edi=owner;W(frame+0x70u,owner);W(frame+0x74u,locale);W(frame+0x78u,name);goto L65836;
   L65836: // MOV ESI,dword ptr [ESP + 0x74]
    esi=R(unchecked(esp+116u));goto L6583a;
   L6583a: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L6583d;
   L6583d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6583f;
   L6583f: // JZ 0x00065cf0
    if(zero)goto L65cf0;goto L65845;
   L65845: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65847;
   L65847: // JZ 0x00065b28
    if(zero)goto L65b28;goto L6584d;
   L6584d: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65850;
   L65850: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L65855;
   L65855: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65859;
   L65859: // MOV dword ptr [ESP],0x4002b
    W(unchecked(esp),262187u);goto L65860;
   L65860: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto L65864;
   L65864: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65869;
   L65869: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L6586c;
   L6586c: // MOV dword ptr [EDX + 0x14],EAX
    W(unchecked(edx+20u),eax);goto L6586f;
   L6586f: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65873;
   L65873: // MOV dword ptr [ESP],0x4002c
    W(unchecked(esp),262188u);goto L6587a;
   L6587a: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6587f;
   L6587f: // MOV EBP,dword ptr [EDI + 0x8]
    ebp=R(unchecked(edi+8u));goto L65882;
   L65882: // MOV dword ptr [EBP + 0x18],EAX
    W(unchecked(ebp+24u),eax);goto L65885;
   L65885: // MOV EAX,dword ptr [EBP + 0x14]
    eax=R(unchecked(ebp+20u));goto L65888;
   L65888: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6588a;
   L6588a: // JNZ 0x00065aa8
    if(!zero)goto L65aa8;goto L65890;
   L65890: // MOV dword ptr [EBP + 0x34],0x0
    W(unchecked(ebp+52u),0u);goto L65897;
   L65897: // MOV dword ptr [EBP + 0x14],0x2e
    W(unchecked(ebp+20u),46u);goto L6589e;
   L6589e: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L658a2;
   L658a2: // MOV dword ptr [ESP],0x40004
    W(unchecked(esp),262148u);goto L658a9;
   L658a9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L658ae;
   L658ae: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L658b2;
   L658b2: // MOV dword ptr [ESP],0x40005
    W(unchecked(esp),262149u);goto L658b9;
   L658b9: // MOV EBP,EAX
    ebp=eax;goto L658bb;
   L658bb: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L658c0;
   L658c0: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L658c4;
   L658c4: // MOV dword ptr [ESP],0x40006
    W(unchecked(esp),262150u);goto L658cb;
   L658cb: // MOV dword ptr [ESP + 0x44],EAX
    W(unchecked(esp+68u),eax);goto L658cf;
   L658cf: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L658d4;
   L658d4: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L658d8;
   L658d8: // MOV dword ptr [ESP],0x40001
    W(unchecked(esp),262145u);goto L658df;
   L658df: // MOV dword ptr [ESP + 0x48],EAX
    W(unchecked(esp+72u),eax);goto L658e3;
   L658e3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L658e8;
   L658e8: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L658ec;
   L658ec: // MOV dword ptr [ESP],0x4000e
    W(unchecked(esp),262158u);goto L658f3;
   L658f3: // MOV dword ptr [ESP + 0x4c],EAX
    W(unchecked(esp+76u),eax);goto L658f7;
   L658f7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L658fc;
   L658fc: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L658ff;
   L658ff: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L65902;
   L65902: // MOV byte ptr [ESP + 0x23],AL
    h.WriteByte(unchecked(esp+35u),unchecked((byte)((eax&255u))));goto L65906;
   L65906: // MOV EAX,dword ptr [EDX + 0x18]
    eax=R(unchecked(edx+24u));goto L65909;
   L65909: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6590b;
   L6590b: // JNZ 0x00065ae8
    if(!zero)goto L65ae8;goto L65911;
   L65911: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L65917;
   L65917: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L6591a;
   L6591a: // MOV dword ptr [EDX + 0xc],0x0
    W(unchecked(edx+12u),0u);goto L65921;
   L65921: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L65925;
   L65925: // MOV dword ptr [EDX + 0x18],0x2c
    W(unchecked(edx+24u),44u);goto L6592c;
   L6592c: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L65934;
   L65934: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L65938;
   L65938: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L6593c;
   L6593c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6593f;
   L6593f: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L65944;
   L65944: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L65948;
   L65948: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6594a;
   L6594a: // JNZ 0x00065c00
    if(!zero)goto L65c00;goto L65950;
   L65950: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L65956;
   L65956: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L65958;
   L65958: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L6595b;
   L6595b: // MOV EAX,dword ptr [EDX + 0x24]
    eax=R(unchecked(edx+36u));goto L6595e;
   L6595e: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L65962;
   L65962: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65965;
   L65965: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L6596a;
   L6596a: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L6596e;
   L6596e: // MOV dword ptr [EDX + 0x28],EAX
    W(unchecked(edx+40u),eax);goto L65971;
   L65971: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L65975;
   L65975: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65978;
   L65978: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L6597d;
   L6597d: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L65981;
   L65981: // CMP byte ptr [ESP + 0x23],0x0
    a=B(unchecked(esp+35u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65986;
   L65986: // JNZ 0x00065ac8
    if(!zero)goto L65ac8;goto L6598c;
   L6598c: // LEA EAX,[EBX + 0xfffd8e60]
    eax=unchecked(ebx+4294807136u);goto L65992;
   L65992: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L65995;
   L65995: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L6599d;
   L6599d: // MOV EAX,dword ptr [EDX + 0x2c]
    eax=R(unchecked(edx+44u));goto L659a0;
   L659a0: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L659a4;
   L659a4: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L659a7;
   L659a7: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L659ac;
   L659ac: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L659b0;
   L659b0: // MOV dword ptr [EDX + 0x30],EAX
    W(unchecked(edx+48u),eax);goto L659b3;
   L659b3: // MOV EAX,dword ptr [ESP + 0x4c]
    eax=R(unchecked(esp+76u));goto L659b7;
   L659b7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L659ba;
   L659ba: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L659bf;
   L659bf: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L659c3;
   L659c3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L659c5;
   L659c5: // JNZ 0x00065bb0
    if(!zero)goto L65bb0;goto L659cb;
   L659cb: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L659d1;
   L659d1: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L659d4;
   L659d4: // MOV EAX,dword ptr [EDX + 0x1c]
    eax=R(unchecked(edx+28u));goto L659d7;
   L659d7: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L659db;
   L659db: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L659de;
   L659de: // CALL 0x00048630
    eax=Call(0x48630,R(unchecked(esp+0u)));goto L659e3;
   L659e3: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L659e7;
   L659e7: // MOV dword ptr [EDX + 0x20],EAX
    W(unchecked(edx+32u),eax);goto L659ea;
   L659ea: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L659ee;
   L659ee: // MOV dword ptr [ESP],0x40009
    W(unchecked(esp),262153u);goto L659f5;
   L659f5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L659fa;
   L659fa: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L659fd;
   L659fd: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65a01;
   L65a01: // MOV dword ptr [ESP],0x4000a
    W(unchecked(esp),262154u);goto L65a08;
   L65a08: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65a0d;
   L65a0d: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65a10;
   L65a10: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65a14;
   L65a14: // MOV dword ptr [ESP],0x4000d
    W(unchecked(esp),262157u);goto L65a1b;
   L65a1b: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L65a1f;
   L65a1f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65a24;
   L65a24: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65a27;
   L65a27: // ADD EDX,0x38
    a=edx;b=56u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L65a2a;
   L65a2a: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65a2d;
   L65a2d: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65a30;
   L65a30: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L65a34;
   L65a34: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L65a38;
   L65a38: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65a3c;
   L65a3c: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65a40;
   L65a40: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L65a45;
   L65a45: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L65a48;
   L65a48: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65a4c;
   L65a4c: // MOV dword ptr [ESP],0x4000b
    W(unchecked(esp),262155u);goto L65a53;
   L65a53: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65a58;
   L65a58: // MOVSX EBP,byte ptr [EAX]
    ebp=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65a5b;
   L65a5b: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65a5f;
   L65a5f: // MOV dword ptr [ESP],0x4000c
    W(unchecked(esp),262156u);goto L65a66;
   L65a66: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65a6b;
   L65a6b: // MOVSX ECX,byte ptr [ESP + 0x23]
    ecx=unchecked((uint)(int)(sbyte)(B(unchecked(esp+35u))));goto L65a70;
   L65a70: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65a73;
   L65a73: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L65a77;
   L65a77: // ADD EDX,0x3c
    a=edx;b=60u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L65a7a;
   L65a7a: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65a7d;
   L65a7d: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L65a81;
   L65a81: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65a84;
   L65a84: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65a88;
   L65a88: // CALL 0x00048970
    eax=Call(0x48970,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));esp=unchecked(esp+4u);goto L65a8d;
   L65a8d: // SUB ESP,0x4
    a=esp;b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esp=v;goto L65a90;
   L65a90: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L65a94;
   L65a94: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65a97;
   L65a97: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L65a9c;
   L65a9c:return 0;
   L65aa8: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L65aac;
   L65aac: // MOV dword ptr [ESP],0x40008
    W(unchecked(esp),262152u);goto L65ab3;
   L65ab3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65ab8;
   L65ab8: // MOVSX EAX,byte ptr [EAX]
    eax=unchecked((uint)(int)(sbyte)(B(unchecked(eax))));goto L65abb;
   L65abb: // MOV dword ptr [EBP + 0x34],EAX
    W(unchecked(ebp+52u),eax);goto L65abe;
   L65abe: // JMP 0x0006589e
    goto L6589e;
   L65ac8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65aca;
   L65aca: // JNZ 0x00065c50
    if(!zero)goto L65c50;goto L65ad0;
   L65ad0: // LEA EAX,[EBX + 0xfffd8e68]
    eax=unchecked(ebx+4294807144u);goto L65ad6;
   L65ad6: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L65ad9;
   L65ad9: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65ae1;
   L65ae1: // JMP 0x0006599d
    goto L6599d;
   L65ae8: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65aeb;
   L65aeb: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L65aef;
   L65aef: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L65af4;
   L65af4: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L65af8;
   L65af8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65afa;
   L65afa: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L65afe;
   L65afe: // JNZ 0x00065cb0
    if(!zero)goto L65cb0;goto L65b04;
   L65b04: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L65b0a;
   L65b0a: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L65b0d;
   L65b0d: // MOV byte ptr [EDX + 0x10],0x0
    h.WriteByte(unchecked(edx+16u),unchecked((byte)(0u)));goto L65b11;
   L65b11: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L65b19;
   L65b19: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65b1d;
   L65b1d: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L65b20;
   L65b20: // JMP 0x00065934
    goto L65934;
   L65b28: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65b2b;
   L65b2b: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L65b31;
   L65b31: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L65b34;
   L65b34: // LEA EDX,[EBX + 0xfffd8e68]
    edx=unchecked(ebx+4294807144u);goto L65b3a;
   L65b3a: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L65b3d;
   L65b3d: // MOV dword ptr [EAX + 0x24],EDX
    W(unchecked(eax+36u),edx);goto L65b40;
   L65b40: // MOV dword ptr [EAX + 0x2c],EDX
    W(unchecked(eax+44u),edx);goto L65b43;
   L65b43: // MOV EDX,dword ptr [EBX + 0xfffffcd4]
    edx=R(unchecked(ebx+4294966484u));goto L65b49;
   L65b49: // MOV dword ptr [EAX + 0x14],0x2e
    W(unchecked(eax+20u),46u);goto L65b50;
   L65b50: // MOV dword ptr [EAX + 0x18],0x2c
    W(unchecked(eax+24u),44u);goto L65b57;
   L65b57: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L65b5e;
   L65b5e: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L65b60;
   L65b60: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L65b64;
   L65b64: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L65b6b;
   L65b6b: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L65b72;
   L65b72: // MOV dword ptr [EAX + 0x38],EDX
    W(unchecked(eax+56u),edx);goto L65b75;
   L65b75: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L65b7c;
   L65b7c: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L65b83;
   L65b83: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65b86;
   L65b86: // MOV dword ptr [EAX + 0x3c],EDX
    W(unchecked(eax+60u),edx);goto L65b89;
   L65b89: // MOV EAX,dword ptr [EBX + 0xfffffecc]
    eax=R(unchecked(ebx+4294966988u));goto L65b8f;
   L65b8f: // MOV ESI,dword ptr [EDI + 0x8]
    esi=R(unchecked(edi+8u));goto L65b92;
   L65b92: // MOV ECX,dword ptr [EAX]
    ecx=R(unchecked(eax));goto L65b94;
   L65b94: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L65b96;
   L65b96: // NOP
    goto L65b98;
   L65b98: // MOVSX EDX,byte ptr [ECX + EAX*0x1]
    edx=unchecked((uint)(int)(sbyte)(B(unchecked(ecx+eax*0x1u))));goto L65b9c;
   L65b9c: // MOV dword ptr [ESI + EAX*0x4 + 0x40],EDX
    W(unchecked(esi+eax*0x4u+64u),edx);goto L65ba0;
   L65ba0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65ba3;
   L65ba3: // CMP EAX,0xb
    a=eax;b=11u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65ba6;
   L65ba6: // JNZ 0x00065b98
    if(!zero)goto L65b98;goto L65ba8;
   L65ba8:return 0;
   L65bb0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65bb3;
   L65bb3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L65bb7;
   L65bb7: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L65bba;
   L65bba: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L65bc2;
   L65bc2: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L65bca;
   L65bca: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65bcd;
   L65bcd: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65df4;}goto L65bd2;
   L65bd2: // MOV EBP,EAX
    ebp=eax;goto L65bd4;
   L65bd4: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L65bd8;
   L65bd8: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L65bdc;
   L65bdc: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L65be0;
   L65be0: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65be3;
   L65be3: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65be7;
   L65be7: // LEA EAX,[ESP + 0x4c]
    eax=unchecked(esp+76u);goto L65beb;
   L65beb: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L65bef;
   L65bef: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L65bf4;
   L65bf4: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65bf7;
   L65bf7: // MOV dword ptr [EDX + 0x1c],EBP
    W(unchecked(edx+28u),ebp);goto L65bfa;
   L65bfa: // JMP 0x000659d4
    goto L659d4;
   L65c00: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65c03;
   L65c03: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L65c07;
   L65c07: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L65c0a;
   L65c0a: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L65c12;
   L65c12: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L65c1a;
   L65c1a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65c1d;
   L65c1d: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e0a;}goto L65c22;
   L65c22: // MOV EBP,EAX
    ebp=eax;goto L65c24;
   L65c24: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L65c28;
   L65c28: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L65c2c;
   L65c2c: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65c30;
   L65c30: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65c33;
   L65c33: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65c37;
   L65c37: // LEA EAX,[ESP + 0x44]
    eax=unchecked(esp+68u);goto L65c3b;
   L65c3b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L65c3f;
   L65c3f: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L65c44;
   L65c44: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65c47;
   L65c47: // MOV dword ptr [EDX + 0x24],EBP
    W(unchecked(edx+36u),ebp);goto L65c4a;
   L65c4a: // JMP 0x0006595b
    goto L6595b;
   L65c50: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65c53;
   L65c53: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L65c57;
   L65c57: // SHL EAX,0x2
    a=eax;v=unchecked(a<<2);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&1073741824u)!=0;overflow=false;eax=v;goto L65c5a;
   L65c5a: // MOV dword ptr [ESP + 0x3c],0x0
    W(unchecked(esp+60u),0u);goto L65c62;
   L65c62: // MOV dword ptr [ESP + 0x40],0x0
    W(unchecked(esp+64u),0u);goto L65c6a;
   L65c6a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65c6d;
   L65c6d: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65d8b;}goto L65c72;
   L65c72: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L65c76;
   L65c76: // LEA EAX,[ESP + 0x3c]
    eax=unchecked(esp+60u);goto L65c7a;
   L65c7a: // MOV dword ptr [ESP + 0xc],EAX
    W(unchecked(esp+12u),eax);goto L65c7e;
   L65c7e: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L65c82;
   L65c82: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65c86;
   L65c86: // LEA EAX,[ESP + 0x48]
    eax=unchecked(esp+72u);goto L65c8a;
   L65c8a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L65c8e;
   L65c8e: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65c92;
   L65c92: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65c95;
   L65c95: // CALL 0x000486b0
    eax=Call(0x486b0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L65c9a;
   L65c9a: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65c9d;
   L65c9d: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65ca1;
   L65ca1: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L65ca4;
   L65ca4: // JMP 0x0006599d
    goto L6599d;
   L65cb0: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65cb3;
   L65cb3: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L65cb7;
   L65cb7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65cba;
   L65cba: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65df6;}goto L65cbf;
   L65cbf: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L65cc3;
   L65cc3: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto L65cc7;
   L65cc7: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L65ccb;
   L65ccb: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L65ccf;
   L65ccf: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L65cd3;
   L65cd3: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65cd6;
   L65cd6: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L65cdb;
   L65cdb: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L65cde;
   L65cde: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L65ce2;
   L65ce2: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L65ce5;
   L65ce5: // JMP 0x00065b19
    goto L65b19;
   L65cf0: // MOV dword ptr [ESP],0x70
    W(unchecked(esp),112u);goto L65cf7;
   L65cf7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L65cfc;
   L65cfc: // MOV EDX,dword ptr [EBX + 0xfffffa74]
    edx=R(unchecked(ebx+4294965876u));goto L65d02;
   L65d02: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L65d05;
   L65d05: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L65d0c;
   L65d0c: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L65d0e;
   L65d0e: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L65d15;
   L65d15: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L65d1c;
   L65d1c: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L65d20;
   L65d20: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L65d27;
   L65d27: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L65d2e;
   L65d2e: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L65d35;
   L65d35: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L65d3c;
   L65d3c: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L65d43;
   L65d43: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L65d4a;
   L65d4a: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L65d51;
   L65d51: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L65d58;
   L65d58: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L65d5f;
   L65d5f: // MOV byte ptr [EAX + 0x38],0x0
    h.WriteByte(unchecked(eax+56u),unchecked((byte)(0u)));goto L65d63;
   L65d63: // MOV byte ptr [EAX + 0x39],0x0
    h.WriteByte(unchecked(eax+57u),unchecked((byte)(0u)));goto L65d67;
   L65d67: // MOV byte ptr [EAX + 0x3a],0x0
    h.WriteByte(unchecked(eax+58u),unchecked((byte)(0u)));goto L65d6b;
   L65d6b: // MOV byte ptr [EAX + 0x3b],0x0
    h.WriteByte(unchecked(eax+59u),unchecked((byte)(0u)));goto L65d6f;
   L65d6f: // MOV byte ptr [EAX + 0x3c],0x0
    h.WriteByte(unchecked(eax+60u),unchecked((byte)(0u)));goto L65d73;
   L65d73: // MOV byte ptr [EAX + 0x3d],0x0
    h.WriteByte(unchecked(eax+61u),unchecked((byte)(0u)));goto L65d77;
   L65d77: // MOV byte ptr [EAX + 0x3e],0x0
    h.WriteByte(unchecked(eax+62u),unchecked((byte)(0u)));goto L65d7b;
   L65d7b: // MOV byte ptr [EAX + 0x3f],0x0
    h.WriteByte(unchecked(eax+63u),unchecked((byte)(0u)));goto L65d7f;
   L65d7f: // MOV byte ptr [EAX + 0x6c],0x0
    h.WriteByte(unchecked(eax+108u),unchecked((byte)(0u)));goto L65d83;
   L65d83: // MOV dword ptr [EDI + 0x8],EAX
    W(unchecked(edi+8u),eax);goto L65d86;
   L65d86: // JMP 0x00065845
    goto L65845;
   L65d8b: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65d93;
   L65d93: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65d96;
   L65d96: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L65d9b;
   L65d9b: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65d9e;
   L65d9e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65da0;
   L65da0: // JZ 0x00065daa
    if(zero)goto L65daa;goto L65da2;
   L65da2: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L65da4;
   L65da4: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65da7;
   L65da7: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}goto L65daa;
   L65daa: // CMP dword ptr [ESP + 0x2c],0x0
    a=R(unchecked(esp+44u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65daf;
   L65daf: // MOV dword ptr [EDI + 0x8],0x0
    W(unchecked(edi+8u),0u);goto L65db6;
   L65db6: // JZ 0x00065dc4
    if(zero)goto L65dc4;goto L65db8;
   L65db8: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto L65dbc;
   L65dbc: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65dbf;
   L65dbf: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}goto L65dc4;
   L65dc4: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65dc6;
   L65dc6: // JZ 0x00065dd0
    if(zero)goto L65dd0;goto L65dc8;
   L65dc8: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65dcb;
   L65dcb: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}goto L65dd0;
   L65dd0: // CMP dword ptr [ESP + 0x1c],0x0
    a=R(unchecked(esp+28u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L65dd5;
   L65dd5: // JZ 0x00065de3
    if(zero)goto L65de3;goto L65dd7;
   L65dd7: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L65ddb;
   L65ddb: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65dde;
   L65dde: // CALL 0x00048270
    try{eax=Call(0x48270,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}goto L65de3;
   L65de3: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L65de7;
   L65de7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65dea;
   L65dea: // CALL 0x00049aa0
    try{eax=Call(0x49aa0,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}goto L65def;
   L65def: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65e19;}
   L65df4: // JMP 0x00065d93
    goto L65d93;
   L65df6: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65dfe;
   L65dfe: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L65e00;
   L65e00: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto L65e08;
   L65e08: // JMP 0x00065d93
    goto L65d93;
   L65e0a: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto L65e12;
   L65e12: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L65e14;
   L65e14: // JMP 0x00065d93
    goto L65d93;
   L65e19: // MOV ESI,EAX
    esi=eax;goto L65e1b;
   L65e1b: // CALL 0x000497e0
    eax=Call(0x497e0);goto L65e20;
   L65e20: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65e23;
   L65e23: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
