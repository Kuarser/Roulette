using System;
namespace RouletteDeluxe
{
 // Physical packaged monetary facet/cache lifetime, all four kinds. Static C# labels
 // preserve original lifetime control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. REP CMPSB uses the native clear-direction ABI. Initializer/destructor, heap, wide compare and exception owners remain required.
 public sealed class OriginalConfigurationMoneypunctLifetime
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
  public OriginalConfigurationMoneypunctLifetime(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint DestroyFacetInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+64u,owner);goto L64fa2;
   L64fa2: // MOV dword ptr [ESP + 0x38],EBP
    W(unchecked(esp+56u),ebp);goto L64fa6;
   L64fa6: // MOV EBP,dword ptr [ESP + 0x40]
    ebp=R(unchecked(esp+64u));goto L64faa;
   L64faa: // MOV dword ptr [ESP + 0x30],ESI
    W(unchecked(esp+48u),esi);goto L64fae;
   L64fae: // MOV dword ptr [ESP + 0x34],EDI
    W(unchecked(esp+52u),edi);goto L64fb2;
   L64fb2: // MOV EAX,dword ptr [EBX + 0xffffff0c]
    eax=R(unchecked(ebx+4294967052u));goto L64fb8;
   L64fb8: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L64fbb;
   L64fbb: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto L64fbe;
   L64fbe: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L64fc1;
   L64fc1: // MOV EDX,dword ptr [EAX + 0xc]
    edx=R(unchecked(eax+12u));goto L64fc4;
   L64fc4: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64fc6;
   L64fc6: // JZ 0x00064fda
    if(zero)goto L64fda;goto L64fc8;
   L64fc8: // MOV EDX,dword ptr [EAX + 0x8]
    edx=R(unchecked(eax+8u));goto L64fcb;
   L64fcb: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64fcd;
   L64fcd: // JZ 0x00064fda
    if(zero)goto L64fda;goto L64fcf;
   L64fcf: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L64fd2;
   L64fd2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64fd7;
   L64fd7: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L64fda;
   L64fda: // MOV EDI,dword ptr [EAX + 0x20]
    edi=R(unchecked(eax+32u));goto L64fdd;
   L64fdd: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64fdf;
   L64fdf: // JZ 0x00064ff3
    if(zero)goto L64ff3;goto L64fe1;
   L64fe1: // MOV EDX,dword ptr [EAX + 0x1c]
    edx=R(unchecked(eax+28u));goto L64fe4;
   L64fe4: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64fe6;
   L64fe6: // JZ 0x00064ff3
    if(zero)goto L64ff3;goto L64fe8;
   L64fe8: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L64feb;
   L64feb: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L64ff0;
   L64ff0: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L64ff3;
   L64ff3: // MOV ESI,dword ptr [EAX + 0x28]
    esi=R(unchecked(eax+40u));goto L64ff6;
   L64ff6: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64ff8;
   L64ff8: // JZ 0x00065012
    if(zero)goto L65012;goto L64ffa;
   L64ffa: // MOV EDX,dword ptr [EAX + 0x24]
    edx=R(unchecked(eax+36u));goto L64ffd;
   L64ffd: // MOV ECX,0x3
    ecx=3u;goto L65002;
   L65002: // LEA EDI,[EBX + 0xfffd8e5c]
    edi=unchecked(ebx+4294807132u);goto L65008;
   L65008: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L6500c;
   L6500c: // MOV ESI,EDX
    esi=edx;goto L6500e;
   L6500e: // CMPSB.REPE ES:EDI,ESI
    while(ecx!=0){a=B(esi);b=B(edi);v=unchecked(a-b);zero=(v&255u)==0;sign=(v&128u)!=0;carry=a<b;overflow=((a^b)&(a^v)&128u)!=0;esi=unchecked(esi+1u);edi=unchecked(edi+1u);ecx=unchecked(ecx-1u);if(!zero)break;}goto L65010;
   L65010: // JNZ 0x00065058
    if(!zero)goto L65058;goto L65012;
   L65012: // MOV EDX,dword ptr [EAX + 0x18]
    edx=R(unchecked(eax+24u));goto L65015;
   L65015: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65017;
   L65017: // JZ 0x0006502b
    if(zero)goto L6502b;goto L65019;
   L65019: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L6501c;
   L6501c: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6501e;
   L6501e: // JZ 0x0006502b
    if(zero)goto L6502b;goto L65020;
   L65020: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65023;
   L65023: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65028;
   L65028: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L6502b;
   L6502b: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6502d;
   L6502d: // JZ 0x00065037
    if(zero)goto L65037;goto L6502f;
   L6502f: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L65031;
   L65031: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65034;
   L65034: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65071;}goto L65037;
   L65037: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L6503a;
   L6503a: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L6503f;
   L6503f:return 0;
   L65058: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L6505c;
   L6505c: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6505e;
   L6505e: // JZ 0x00065012
    if(zero)goto L65012;goto L65060;
   L65060: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L65064;
   L65064: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L65067;
   L65067: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L6506c;
   L6506c: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L6506f;
   L6506f: // JMP 0x00065012
    goto L65012;
   L65071: // MOV ESI,EAX
    esi=eax;goto L65073;
   L65073: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65076;
   L65076: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L6507b;
   L6507b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6507e;
   L6507e: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteFacetInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L650a2;
   L650a2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L650a6;
   L650a6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L650aa;
   L650aa: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L650ad;
   L650ad: // CALL 0x00048d80
    eax=Call(0x48d80,R(unchecked(esp+0u)));goto L650b2;
   L650b2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L650b5;
   L650b5: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L650ba;
   L650ba:return 0;
  }
  public uint DestroyFacetDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+64u,owner);goto L650e2;
   L650e2: // MOV dword ptr [ESP + 0x38],EBP
    W(unchecked(esp+56u),ebp);goto L650e6;
   L650e6: // MOV EBP,dword ptr [ESP + 0x40]
    ebp=R(unchecked(esp+64u));goto L650ea;
   L650ea: // MOV dword ptr [ESP + 0x30],ESI
    W(unchecked(esp+48u),esi);goto L650ee;
   L650ee: // MOV dword ptr [ESP + 0x34],EDI
    W(unchecked(esp+52u),edi);goto L650f2;
   L650f2: // MOV EAX,dword ptr [EBX + 0xfffffbd0]
    eax=R(unchecked(ebx+4294966224u));goto L650f8;
   L650f8: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L650fb;
   L650fb: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto L650fe;
   L650fe: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L65101;
   L65101: // MOV ECX,dword ptr [EAX + 0xc]
    ecx=R(unchecked(eax+12u));goto L65104;
   L65104: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65106;
   L65106: // JZ 0x0006511a
    if(zero)goto L6511a;goto L65108;
   L65108: // MOV EDX,dword ptr [EAX + 0x8]
    edx=R(unchecked(eax+8u));goto L6510b;
   L6510b: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6510d;
   L6510d: // JZ 0x0006511a
    if(zero)goto L6511a;goto L6510f;
   L6510f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65112;
   L65112: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65117;
   L65117: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L6511a;
   L6511a: // MOV EDX,dword ptr [EAX + 0x20]
    edx=R(unchecked(eax+32u));goto L6511d;
   L6511d: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6511f;
   L6511f: // JZ 0x00065133
    if(zero)goto L65133;goto L65121;
   L65121: // MOV EDX,dword ptr [EAX + 0x1c]
    edx=R(unchecked(eax+28u));goto L65124;
   L65124: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65126;
   L65126: // JZ 0x00065133
    if(zero)goto L65133;goto L65128;
   L65128: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L6512b;
   L6512b: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65130;
   L65130: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L65133;
   L65133: // MOV EDI,dword ptr [EAX + 0x28]
    edi=R(unchecked(eax+40u));goto L65136;
   L65136: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65138;
   L65138: // JZ 0x00065152
    if(zero)goto L65152;goto L6513a;
   L6513a: // MOV EDX,dword ptr [EAX + 0x24]
    edx=R(unchecked(eax+36u));goto L6513d;
   L6513d: // MOV ECX,0x3
    ecx=3u;goto L65142;
   L65142: // LEA EDI,[EBX + 0xfffd8e5c]
    edi=unchecked(ebx+4294807132u);goto L65148;
   L65148: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L6514c;
   L6514c: // MOV ESI,EDX
    esi=edx;goto L6514e;
   L6514e: // CMPSB.REPE ES:EDI,ESI
    while(ecx!=0){a=B(esi);b=B(edi);v=unchecked(a-b);zero=(v&255u)==0;sign=(v&128u)!=0;carry=a<b;overflow=((a^b)&(a^v)&128u)!=0;esi=unchecked(esi+1u);edi=unchecked(edi+1u);ecx=unchecked(ecx-1u);if(!zero)break;}goto L65150;
   L65150: // JNZ 0x00065198
    if(!zero)goto L65198;goto L65152;
   L65152: // MOV ECX,dword ptr [EAX + 0x18]
    ecx=R(unchecked(eax+24u));goto L65155;
   L65155: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65157;
   L65157: // JZ 0x0006516b
    if(zero)goto L6516b;goto L65159;
   L65159: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L6515c;
   L6515c: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6515e;
   L6515e: // JZ 0x0006516b
    if(zero)goto L6516b;goto L65160;
   L65160: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L65163;
   L65163: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65168;
   L65168: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L6516b;
   L6516b: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6516d;
   L6516d: // JZ 0x00065177
    if(zero)goto L65177;goto L6516f;
   L6516f: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L65171;
   L65171: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65174;
   L65174: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L651b1;}goto L65177;
   L65177: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L6517a;
   L6517a: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L6517f;
   L6517f:return 0;
   L65198: // MOV ESI,dword ptr [ESP + 0x1c]
    esi=R(unchecked(esp+28u));goto L6519c;
   L6519c: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6519e;
   L6519e: // JZ 0x00065152
    if(zero)goto L65152;goto L651a0;
   L651a0: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L651a4;
   L651a4: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L651a7;
   L651a7: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L651ac;
   L651ac: // MOV EAX,dword ptr [EBP + 0x8]
    eax=R(unchecked(ebp+8u));goto L651af;
   L651af: // JMP 0x00065152
    goto L65152;
   L651b1: // MOV ESI,EAX
    esi=eax;goto L651b3;
   L651b3: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L651b6;
   L651b6: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L651bb;
   L651bb: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L651be;
   L651be: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteFacetDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L651e2;
   L651e2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L651e6;
   L651e6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L651ea;
   L651ea: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L651ed;
   L651ed: // CALL 0x00048ed0
    eax=Call(0x48ed0,R(unchecked(esp+0u)));goto L651f2;
   L651f2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L651f5;
   L651f5: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L651fa;
   L651fa:return 0;
  }
  public uint DestroyFacetInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+48u,owner);goto L65e42;
   L65e42: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto L65e46;
   L65e46: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L65e4a;
   L65e4a: // MOV dword ptr [ESP + 0x24],EDI
    W(unchecked(esp+36u),edi);goto L65e4e;
   L65e4e: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto L65e52;
   L65e52: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65e55;
   L65e55: // MOV EAX,dword ptr [EBX + 0xfffffd00]
    eax=R(unchecked(ebx+4294966528u));goto L65e5b;
   L65e5b: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65e5e;
   L65e5e: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L65e60;
   L65e60: // MOV EAX,dword ptr [EDI + 0xc]
    eax=R(unchecked(edi+12u));goto L65e63;
   L65e63: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65e65;
   L65e65: // JZ 0x00065e79
    if(zero)goto L65e79;goto L65e67;
   L65e67: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65e6a;
   L65e6a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65e6c;
   L65e6c: // JZ 0x00065e79
    if(zero)goto L65e79;goto L65e6e;
   L65e6e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65e71;
   L65e71: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65e76;
   L65e76: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65e79;
   L65e79: // MOV EBP,dword ptr [EDI + 0x28]
    ebp=R(unchecked(edi+40u));goto L65e7c;
   L65e7c: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65e7e;
   L65e7e: // JZ 0x00065e92
    if(zero)goto L65e92;goto L65e80;
   L65e80: // MOV EAX,dword ptr [EDI + 0x24]
    eax=R(unchecked(edi+36u));goto L65e83;
   L65e83: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65e85;
   L65e85: // JZ 0x00065e92
    if(zero)goto L65e92;goto L65e87;
   L65e87: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65e8a;
   L65e8a: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65e8f;
   L65e8f: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65e92;
   L65e92: // MOV ECX,dword ptr [EDI + 0x30]
    ecx=R(unchecked(edi+48u));goto L65e95;
   L65e95: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65e97;
   L65e97: // JZ 0x00065eb2
    if(zero)goto L65eb2;goto L65e99;
   L65e99: // MOV EBP,dword ptr [EDI + 0x2c]
    ebp=R(unchecked(edi+44u));goto L65e9c;
   L65e9c: // LEA EAX,[EBX + 0xfffd8e60]
    eax=unchecked(ebx+4294807136u);goto L65ea2;
   L65ea2: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L65ea6;
   L65ea6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65ea9;
   L65ea9: // CALL 0x00049a30
    eax=Call(0x49a30,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65eae;
   L65eae: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65eb0;
   L65eb0: // JNZ 0x00065ef8
    if(!zero)goto L65ef8;goto L65eb2;
   L65eb2: // MOV EDX,dword ptr [EDI + 0x20]
    edx=R(unchecked(edi+32u));goto L65eb5;
   L65eb5: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65eb7;
   L65eb7: // JZ 0x00065ecb
    if(zero)goto L65ecb;goto L65eb9;
   L65eb9: // MOV EAX,dword ptr [EDI + 0x1c]
    eax=R(unchecked(edi+28u));goto L65ebc;
   L65ebc: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65ebe;
   L65ebe: // JZ 0x00065ecb
    if(zero)goto L65ecb;goto L65ec0;
   L65ec0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65ec3;
   L65ec3: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65ec8;
   L65ec8: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65ecb;
   L65ecb: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65ecd;
   L65ecd: // JZ 0x00065ed7
    if(zero)goto L65ed7;goto L65ecf;
   L65ecf: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L65ed1;
   L65ed1: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L65ed4;
   L65ed4: // CALL dword ptr [EAX + 0x4]
    try{eax=Call(R(unchecked(eax+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L65f09;}goto L65ed7;
   L65ed7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65eda;
   L65eda: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L65edf;
   L65edf:return 0;
   L65ef8: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65efa;
   L65efa: // JZ 0x00065eb2
    if(zero)goto L65eb2;goto L65efc;
   L65efc: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65eff;
   L65eff: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65f04;
   L65f04: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65f07;
   L65f07: // JMP 0x00065eb2
    goto L65eb2;
   L65f09: // MOV EDI,EAX
    edi=eax;goto L65f0b;
   L65f0b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65f0e;
   L65f0e: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L65f13;
   L65f13: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L65f16;
   L65f16: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteFacetInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L65f32;
   L65f32: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L65f36;
   L65f36: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L65f3a;
   L65f3a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65f3d;
   L65f3d: // CALL 0x00048180
    eax=Call(0x48180,R(unchecked(esp+0u)));goto L65f42;
   L65f42: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L65f45;
   L65f45: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L65f4a;
   L65f4a:return 0;
  }
  public uint DestroyFacetDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+48u,owner);goto L65f72;
   L65f72: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto L65f76;
   L65f76: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L65f7a;
   L65f7a: // MOV dword ptr [ESP + 0x24],EDI
    W(unchecked(esp+36u),edi);goto L65f7e;
   L65f7e: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto L65f82;
   L65f82: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65f85;
   L65f85: // MOV EAX,dword ptr [EBX + 0xfffffad4]
    eax=R(unchecked(ebx+4294965972u));goto L65f8b;
   L65f8b: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L65f8e;
   L65f8e: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L65f90;
   L65f90: // MOV EAX,dword ptr [EDI + 0xc]
    eax=R(unchecked(edi+12u));goto L65f93;
   L65f93: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65f95;
   L65f95: // JZ 0x00065fa9
    if(zero)goto L65fa9;goto L65f97;
   L65f97: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L65f9a;
   L65f9a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65f9c;
   L65f9c: // JZ 0x00065fa9
    if(zero)goto L65fa9;goto L65f9e;
   L65f9e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65fa1;
   L65fa1: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65fa6;
   L65fa6: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65fa9;
   L65fa9: // MOV EAX,dword ptr [EDI + 0x28]
    eax=R(unchecked(edi+40u));goto L65fac;
   L65fac: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fae;
   L65fae: // JZ 0x00065fc2
    if(zero)goto L65fc2;goto L65fb0;
   L65fb0: // MOV EAX,dword ptr [EDI + 0x24]
    eax=R(unchecked(edi+36u));goto L65fb3;
   L65fb3: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fb5;
   L65fb5: // JZ 0x00065fc2
    if(zero)goto L65fc2;goto L65fb7;
   L65fb7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65fba;
   L65fba: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65fbf;
   L65fbf: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65fc2;
   L65fc2: // MOV EAX,dword ptr [EDI + 0x30]
    eax=R(unchecked(edi+48u));goto L65fc5;
   L65fc5: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fc7;
   L65fc7: // JZ 0x00065fe2
    if(zero)goto L65fe2;goto L65fc9;
   L65fc9: // MOV EBP,dword ptr [EDI + 0x2c]
    ebp=R(unchecked(edi+44u));goto L65fcc;
   L65fcc: // LEA EAX,[EBX + 0xfffd8e60]
    eax=unchecked(ebx+4294807136u);goto L65fd2;
   L65fd2: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L65fd6;
   L65fd6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L65fd9;
   L65fd9: // CALL 0x00049a30
    eax=Call(0x49a30,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L65fde;
   L65fde: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fe0;
   L65fe0: // JNZ 0x00066028
    if(!zero)goto L66028;goto L65fe2;
   L65fe2: // MOV EAX,dword ptr [EDI + 0x20]
    eax=R(unchecked(edi+32u));goto L65fe5;
   L65fe5: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fe7;
   L65fe7: // JZ 0x00065ffb
    if(zero)goto L65ffb;goto L65fe9;
   L65fe9: // MOV EAX,dword ptr [EDI + 0x1c]
    eax=R(unchecked(edi+28u));goto L65fec;
   L65fec: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65fee;
   L65fee: // JZ 0x00065ffb
    if(zero)goto L65ffb;goto L65ff0;
   L65ff0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L65ff3;
   L65ff3: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L65ff8;
   L65ff8: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L65ffb;
   L65ffb: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L65ffd;
   L65ffd: // JZ 0x00066007
    if(zero)goto L66007;goto L65fff;
   L65fff: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L66001;
   L66001: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L66004;
   L66004: // CALL dword ptr [EAX + 0x4]
    try{eax=Call(R(unchecked(eax+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L66039;}goto L66007;
   L66007: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6600a;
   L6600a: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L6600f;
   L6600f:return 0;
   L66028: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6602a;
   L6602a: // JZ 0x00065fe2
    if(zero)goto L65fe2;goto L6602c;
   L6602c: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L6602f;
   L6602f: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L66034;
   L66034: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66037;
   L66037: // JMP 0x00065fe2
    goto L65fe2;
   L66039: // MOV EDI,EAX
    edi=eax;goto L6603b;
   L6603b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6603e;
   L6603e: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L66043;
   L66043: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L66046;
   L66046: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteFacetDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L66062;
   L66062: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L66066;
   L66066: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L6606a;
   L6606a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6606d;
   L6606d: // CALL 0x00049ec0
    eax=Call(0x49ec0,R(unchecked(esp+0u)));goto L66072;
   L66072: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66075;
   L66075: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L6607a;
   L6607a:return 0;
  }
  public uint DestroyCacheInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L79cb2;
   L79cb2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79cb6;
   L79cb6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79cba;
   L79cba: // MOV EAX,dword ptr [EBX + 0xffffff90]
    eax=R(unchecked(ebx+4294967184u));goto L79cc0;
   L79cc0: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L79cc3;
   L79cc3: // CMP byte ptr [ESI + 0x43],0x0
    a=B(unchecked(esi+67u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L79cc7;
   L79cc7: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L79cc9;
   L79cc9: // JZ 0x00079d07
    if(zero)goto L79d07;goto L79ccb;
   L79ccb: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L79cce;
   L79cce: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79cd0;
   L79cd0: // JZ 0x00079cda
    if(zero)goto L79cda;goto L79cd2;
   L79cd2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79cd5;
   L79cd5: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79cda;
   L79cda: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L79cdd;
   L79cdd: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79cdf;
   L79cdf: // JZ 0x00079ce9
    if(zero)goto L79ce9;goto L79ce1;
   L79ce1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79ce4;
   L79ce4: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79ce9;
   L79ce9: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L79cec;
   L79cec: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79cee;
   L79cee: // JZ 0x00079cf8
    if(zero)goto L79cf8;goto L79cf0;
   L79cf0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79cf3;
   L79cf3: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79cf8;
   L79cf8: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L79cfb;
   L79cfb: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79cfd;
   L79cfd: // JZ 0x00079d07
    if(zero)goto L79d07;goto L79cff;
   L79cff: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79d02;
   L79d02: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79d07;
   L79d07: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79d0a;
   L79d0a: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L79d0f;
   L79d0f:return 0;
  }
  public uint DeleteCacheInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L79d32;
   L79d32: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79d36;
   L79d36: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79d3a;
   L79d3a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79d3d;
   L79d3d: // CALL 0x000485a0
    eax=Call(0x485a0,R(unchecked(esp+0u)));goto L79d42;
   L79d42: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79d45;
   L79d45: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L79d4a;
   L79d4a:return 0;
  }
  public uint DestroyCacheDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L79d72;
   L79d72: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79d76;
   L79d76: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79d7a;
   L79d7a: // MOV EAX,dword ptr [EBX + 0xfffffaac]
    eax=R(unchecked(ebx+4294965932u));goto L79d80;
   L79d80: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L79d83;
   L79d83: // CMP byte ptr [ESI + 0x43],0x0
    a=B(unchecked(esi+67u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L79d87;
   L79d87: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L79d89;
   L79d89: // JZ 0x00079dc7
    if(zero)goto L79dc7;goto L79d8b;
   L79d8b: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L79d8e;
   L79d8e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79d90;
   L79d90: // JZ 0x00079d9a
    if(zero)goto L79d9a;goto L79d92;
   L79d92: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79d95;
   L79d95: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79d9a;
   L79d9a: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L79d9d;
   L79d9d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79d9f;
   L79d9f: // JZ 0x00079da9
    if(zero)goto L79da9;goto L79da1;
   L79da1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79da4;
   L79da4: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79da9;
   L79da9: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L79dac;
   L79dac: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79dae;
   L79dae: // JZ 0x00079db8
    if(zero)goto L79db8;goto L79db0;
   L79db0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79db3;
   L79db3: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79db8;
   L79db8: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L79dbb;
   L79dbb: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79dbd;
   L79dbd: // JZ 0x00079dc7
    if(zero)goto L79dc7;goto L79dbf;
   L79dbf: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79dc2;
   L79dc2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79dc7;
   L79dc7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79dca;
   L79dca: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L79dcf;
   L79dcf:return 0;
  }
  public uint DeleteCacheDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L79df2;
   L79df2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79df6;
   L79df6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79dfa;
   L79dfa: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79dfd;
   L79dfd: // CALL 0x00048c30
    eax=Call(0x48c30,R(unchecked(esp+0u)));goto L79e02;
   L79e02: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79e05;
   L79e05: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L79e0a;
   L79e0a:return 0;
  }
  public uint ConstructDefaultDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);W(frame+0x3cu,references);goto L7ab88;
   L7ab88: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7ab8c;
   L7ab8c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7ab90;
   L7ab90: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ab92;
   L7ab92: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ab95;
   L7ab95: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7ab98;
   L7ab98: // MOV EAX,dword ptr [EBX + 0xfffffbd0]
    eax=R(unchecked(ebx+4294966224u));goto L7ab9e;
   L7ab9e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7aba5;
   L7aba5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7aba8;
   L7aba8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7abaa;
   L7abaa: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L7abb2;
   L7abb2: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7abba;
   L7abba: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7abbd;
   L7abbd: // CALL 0x000481c0
    try{eax=Call(0x481c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7abce;}goto L7abc2;
   L7abc2:return 0;
   L7abce: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7abd2;
   L7abd2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7abd5;
   L7abd5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7abda;
   L7abda: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7abde;
   L7abde: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7abe1;
   L7abe1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructSuppliedCacheDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);W(frame+0x3cu,references);goto L7ac08;
   L7ac08: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7ac0c;
   L7ac0c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7ac10;
   L7ac10: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ac12;
   L7ac12: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ac15;
   L7ac15: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7ac18;
   L7ac18: // MOV EAX,dword ptr [EBX + 0xfffffbd0]
    eax=R(unchecked(ebx+4294966224u));goto L7ac1e;
   L7ac1e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7ac21;
   L7ac21: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7ac23;
   L7ac23: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7ac27;
   L7ac27: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L7ac2a;
   L7ac2a: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L7ac32;
   L7ac32: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7ac3a;
   L7ac3a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7ac3d;
   L7ac3d: // CALL 0x000481c0
    try{eax=Call(0x481c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7ac4e;}goto L7ac42;
   L7ac42:return 0;
   L7ac4e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7ac52;
   L7ac52: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7ac55;
   L7ac55: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7ac5a;
   L7ac5a: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7ac5e;
   L7ac5e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7ac61;
   L7ac61: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructLocaleDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,localeName);W(frame+0x3cu,references);goto L7ac88;
   L7ac88: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7ac8c;
   L7ac8c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7ac90;
   L7ac90: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ac92;
   L7ac92: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ac95;
   L7ac95: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7ac98;
   L7ac98: // MOV EAX,dword ptr [EBX + 0xfffffbd0]
    eax=R(unchecked(ebx+4294966224u));goto L7ac9e;
   L7ac9e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7aca5;
   L7aca5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7aca8;
   L7aca8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7acaa;
   L7acaa: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L7acae;
   L7acae: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7acb1;
   L7acb1: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L7acb5;
   L7acb5: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7acb9;
   L7acb9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7acbd;
   L7acbd: // CALL 0x000481c0
    try{eax=Call(0x481c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7acce;}goto L7acc2;
   L7acc2:return 0;
   L7acce: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7acd2;
   L7acd2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7acd5;
   L7acd5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7acda;
   L7acda: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7acde;
   L7acde: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7ace1;
   L7ace1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructDefaultInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);W(frame+0x3cu,references);goto L7ae88;
   L7ae88: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7ae8c;
   L7ae8c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7ae90;
   L7ae90: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ae92;
   L7ae92: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ae95;
   L7ae95: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7ae98;
   L7ae98: // MOV EAX,dword ptr [EBX + 0xffffff0c]
    eax=R(unchecked(ebx+4294967052u));goto L7ae9e;
   L7ae9e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7aea5;
   L7aea5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7aea8;
   L7aea8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7aeaa;
   L7aeaa: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L7aeb2;
   L7aeb2: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7aeba;
   L7aeba: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7aebd;
   L7aebd: // CALL 0x0004a430
    try{eax=Call(0x4a430,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7aece;}goto L7aec2;
   L7aec2:return 0;
   L7aece: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7aed2;
   L7aed2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7aed5;
   L7aed5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7aeda;
   L7aeda: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7aede;
   L7aede: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7aee1;
   L7aee1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructSuppliedCacheInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);W(frame+0x3cu,references);goto L7af08;
   L7af08: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7af0c;
   L7af0c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7af10;
   L7af10: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7af12;
   L7af12: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7af15;
   L7af15: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7af18;
   L7af18: // MOV EAX,dword ptr [EBX + 0xffffff0c]
    eax=R(unchecked(ebx+4294967052u));goto L7af1e;
   L7af1e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7af21;
   L7af21: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7af23;
   L7af23: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7af27;
   L7af27: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L7af2a;
   L7af2a: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L7af32;
   L7af32: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7af3a;
   L7af3a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7af3d;
   L7af3d: // CALL 0x0004a430
    try{eax=Call(0x4a430,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7af4e;}goto L7af42;
   L7af42:return 0;
   L7af4e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7af52;
   L7af52: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7af55;
   L7af55: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7af5a;
   L7af5a: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7af5e;
   L7af5e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7af61;
   L7af61: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructLocaleInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,localeName);W(frame+0x3cu,references);goto L7af88;
   L7af88: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7af8c;
   L7af8c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7af90;
   L7af90: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7af92;
   L7af92: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7af95;
   L7af95: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7af98;
   L7af98: // MOV EAX,dword ptr [EBX + 0xffffff0c]
    eax=R(unchecked(ebx+4294967052u));goto L7af9e;
   L7af9e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7afa5;
   L7afa5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7afa8;
   L7afa8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7afaa;
   L7afaa: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L7afae;
   L7afae: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7afb1;
   L7afb1: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L7afb5;
   L7afb5: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7afb9;
   L7afb9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7afbd;
   L7afbd: // CALL 0x0004a430
    try{eax=Call(0x4a430,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7afce;}goto L7afc2;
   L7afc2:return 0;
   L7afce: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7afd2;
   L7afd2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7afd5;
   L7afd5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7afda;
   L7afda: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7afde;
   L7afde: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7afe1;
   L7afe1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCacheDomesticChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L7b17b;
   L7b17b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L7b17f;
   L7b17f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L7b181;
   L7b181: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7b186;
   L7b186: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L7b189;
   L7b189: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L7b18c;
   L7b18c: // MOV EDX,dword ptr [ECX + 0xfffffaac]
    edx=R(unchecked(ecx+4294965932u));goto L7b192;
   L7b192: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L7b199;
   L7b199: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L7b1a0;
   L7b1a0: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L7b1a4;
   L7b1a4: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L7b1a7;
   L7b1a7: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L7b1a9;
   L7b1a9: // MOV byte ptr [EAX + 0x11],0x0
    h.WriteByte(unchecked(eax+17u),unchecked((byte)(0u)));goto L7b1ad;
   L7b1ad: // MOV byte ptr [EAX + 0x12],0x0
    h.WriteByte(unchecked(eax+18u),unchecked((byte)(0u)));goto L7b1b1;
   L7b1b1: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L7b1b8;
   L7b1b8: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L7b1bf;
   L7b1bf: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L7b1c6;
   L7b1c6: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L7b1cd;
   L7b1cd: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L7b1d4;
   L7b1d4: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L7b1db;
   L7b1db: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L7b1e2;
   L7b1e2: // MOV byte ptr [EAX + 0x30],0x0
    h.WriteByte(unchecked(eax+48u),unchecked((byte)(0u)));goto L7b1e6;
   L7b1e6: // MOV byte ptr [EAX + 0x31],0x0
    h.WriteByte(unchecked(eax+49u),unchecked((byte)(0u)));goto L7b1ea;
   L7b1ea: // MOV byte ptr [EAX + 0x32],0x0
    h.WriteByte(unchecked(eax+50u),unchecked((byte)(0u)));goto L7b1ee;
   L7b1ee: // MOV byte ptr [EAX + 0x33],0x0
    h.WriteByte(unchecked(eax+51u),unchecked((byte)(0u)));goto L7b1f2;
   L7b1f2: // MOV byte ptr [EAX + 0x34],0x0
    h.WriteByte(unchecked(eax+52u),unchecked((byte)(0u)));goto L7b1f6;
   L7b1f6: // MOV byte ptr [EAX + 0x35],0x0
    h.WriteByte(unchecked(eax+53u),unchecked((byte)(0u)));goto L7b1fa;
   L7b1fa: // MOV byte ptr [EAX + 0x36],0x0
    h.WriteByte(unchecked(eax+54u),unchecked((byte)(0u)));goto L7b1fe;
   L7b1fe: // MOV byte ptr [EAX + 0x37],0x0
    h.WriteByte(unchecked(eax+55u),unchecked((byte)(0u)));goto L7b202;
   L7b202: // MOV byte ptr [EAX + 0x43],0x0
    h.WriteByte(unchecked(eax+67u),unchecked((byte)(0u)));goto L7b206;
   L7b206:return 0;
  }
  public uint ConstructCacheInternationalChar(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L7b21b;
   L7b21b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L7b21f;
   L7b21f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L7b221;
   L7b221: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7b226;
   L7b226: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L7b229;
   L7b229: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L7b22c;
   L7b22c: // MOV EDX,dword ptr [ECX + 0xffffff90]
    edx=R(unchecked(ecx+4294967184u));goto L7b232;
   L7b232: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L7b239;
   L7b239: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L7b240;
   L7b240: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L7b244;
   L7b244: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L7b247;
   L7b247: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L7b249;
   L7b249: // MOV byte ptr [EAX + 0x11],0x0
    h.WriteByte(unchecked(eax+17u),unchecked((byte)(0u)));goto L7b24d;
   L7b24d: // MOV byte ptr [EAX + 0x12],0x0
    h.WriteByte(unchecked(eax+18u),unchecked((byte)(0u)));goto L7b251;
   L7b251: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L7b258;
   L7b258: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L7b25f;
   L7b25f: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L7b266;
   L7b266: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L7b26d;
   L7b26d: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L7b274;
   L7b274: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L7b27b;
   L7b27b: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L7b282;
   L7b282: // MOV byte ptr [EAX + 0x30],0x0
    h.WriteByte(unchecked(eax+48u),unchecked((byte)(0u)));goto L7b286;
   L7b286: // MOV byte ptr [EAX + 0x31],0x0
    h.WriteByte(unchecked(eax+49u),unchecked((byte)(0u)));goto L7b28a;
   L7b28a: // MOV byte ptr [EAX + 0x32],0x0
    h.WriteByte(unchecked(eax+50u),unchecked((byte)(0u)));goto L7b28e;
   L7b28e: // MOV byte ptr [EAX + 0x33],0x0
    h.WriteByte(unchecked(eax+51u),unchecked((byte)(0u)));goto L7b292;
   L7b292: // MOV byte ptr [EAX + 0x34],0x0
    h.WriteByte(unchecked(eax+52u),unchecked((byte)(0u)));goto L7b296;
   L7b296: // MOV byte ptr [EAX + 0x35],0x0
    h.WriteByte(unchecked(eax+53u),unchecked((byte)(0u)));goto L7b29a;
   L7b29a: // MOV byte ptr [EAX + 0x36],0x0
    h.WriteByte(unchecked(eax+54u),unchecked((byte)(0u)));goto L7b29e;
   L7b29e: // MOV byte ptr [EAX + 0x37],0x0
    h.WriteByte(unchecked(eax+55u),unchecked((byte)(0u)));goto L7b2a2;
   L7b2a2: // MOV byte ptr [EAX + 0x43],0x0
    h.WriteByte(unchecked(eax+67u),unchecked((byte)(0u)));goto L7b2a6;
   L7b2a6:return 0;
  }
  public uint DestroyCacheInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L9c092;
   L9c092: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9c096;
   L9c096: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9c09a;
   L9c09a: // MOV EAX,dword ptr [EBX + 0xfffffc04]
    eax=R(unchecked(ebx+4294966276u));goto L9c0a0;
   L9c0a0: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9c0a3;
   L9c0a3: // CMP byte ptr [ESI + 0x6c],0x0
    a=B(unchecked(esi+108u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9c0a7;
   L9c0a7: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9c0a9;
   L9c0a9: // JZ 0x0009c0e7
    if(zero)goto L9c0e7;goto L9c0ab;
   L9c0ab: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L9c0ae;
   L9c0ae: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c0b0;
   L9c0b0: // JZ 0x0009c0ba
    if(zero)goto L9c0ba;goto L9c0b2;
   L9c0b2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c0b5;
   L9c0b5: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c0ba;
   L9c0ba: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L9c0bd;
   L9c0bd: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c0bf;
   L9c0bf: // JZ 0x0009c0c9
    if(zero)goto L9c0c9;goto L9c0c1;
   L9c0c1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c0c4;
   L9c0c4: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c0c9;
   L9c0c9: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L9c0cc;
   L9c0cc: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c0ce;
   L9c0ce: // JZ 0x0009c0d8
    if(zero)goto L9c0d8;goto L9c0d0;
   L9c0d0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c0d3;
   L9c0d3: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c0d8;
   L9c0d8: // MOV EAX,dword ptr [ESI + 0x2c]
    eax=R(unchecked(esi+44u));goto L9c0db;
   L9c0db: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c0dd;
   L9c0dd: // JZ 0x0009c0e7
    if(zero)goto L9c0e7;goto L9c0df;
   L9c0df: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c0e2;
   L9c0e2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c0e7;
   L9c0e7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c0ea;
   L9c0ea: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9c0ef;
   L9c0ef:return 0;
  }
  public uint DeleteCacheInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L9c112;
   L9c112: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9c116;
   L9c116: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9c11a;
   L9c11a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c11d;
   L9c11d: // CALL 0x0004a1e0
    eax=Call(0x4a1e0,R(unchecked(esp+0u)));goto L9c122;
   L9c122: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c125;
   L9c125: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L9c12a;
   L9c12a:return 0;
  }
  public uint DestroyCacheDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L9c152;
   L9c152: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9c156;
   L9c156: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9c15a;
   L9c15a: // MOV EAX,dword ptr [EBX + 0xfffffa74]
    eax=R(unchecked(ebx+4294965876u));goto L9c160;
   L9c160: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9c163;
   L9c163: // CMP byte ptr [ESI + 0x6c],0x0
    a=B(unchecked(esi+108u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9c167;
   L9c167: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9c169;
   L9c169: // JZ 0x0009c1a7
    if(zero)goto L9c1a7;goto L9c16b;
   L9c16b: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L9c16e;
   L9c16e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c170;
   L9c170: // JZ 0x0009c17a
    if(zero)goto L9c17a;goto L9c172;
   L9c172: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c175;
   L9c175: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c17a;
   L9c17a: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L9c17d;
   L9c17d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c17f;
   L9c17f: // JZ 0x0009c189
    if(zero)goto L9c189;goto L9c181;
   L9c181: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c184;
   L9c184: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c189;
   L9c189: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L9c18c;
   L9c18c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c18e;
   L9c18e: // JZ 0x0009c198
    if(zero)goto L9c198;goto L9c190;
   L9c190: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c193;
   L9c193: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c198;
   L9c198: // MOV EAX,dword ptr [ESI + 0x2c]
    eax=R(unchecked(esi+44u));goto L9c19b;
   L9c19b: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c19d;
   L9c19d: // JZ 0x0009c1a7
    if(zero)goto L9c1a7;goto L9c19f;
   L9c19f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c1a2;
   L9c1a2: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c1a7;
   L9c1a7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c1aa;
   L9c1aa: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9c1af;
   L9c1af:return 0;
  }
  public uint DeleteCacheDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+32u,owner);goto L9c1d2;
   L9c1d2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9c1d6;
   L9c1d6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9c1da;
   L9c1da: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c1dd;
   L9c1dd: // CALL 0x00048b90
    eax=Call(0x48b90,R(unchecked(esp+0u)));goto L9c1e2;
   L9c1e2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c1e5;
   L9c1e5: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L9c1ea;
   L9c1ea:return 0;
  }
  public uint ConstructDefaultDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);W(frame+0x3cu,references);goto L9cf98;
   L9cf98: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9cf9c;
   L9cf9c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9cfa0;
   L9cfa0: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9cfa2;
   L9cfa2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9cfa5;
   L9cfa5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9cfa8;
   L9cfa8: // MOV EAX,dword ptr [EBX + 0xfffffad4]
    eax=R(unchecked(ebx+4294965972u));goto L9cfae;
   L9cfae: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9cfb5;
   L9cfb5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9cfb8;
   L9cfb8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9cfba;
   L9cfba: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9cfc2;
   L9cfc2: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9cfca;
   L9cfca: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9cfcd;
   L9cfcd: // CALL 0x00049500
    try{eax=Call(0x49500,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9cfde;}goto L9cfd2;
   L9cfd2:return 0;
   L9cfde: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9cfe2;
   L9cfe2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9cfe5;
   L9cfe5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9cfea;
   L9cfea: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9cfee;
   L9cfee: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9cff1;
   L9cff1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructSuppliedCacheDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);W(frame+0x3cu,references);goto L9d018;
   L9d018: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9d01c;
   L9d01c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9d020;
   L9d020: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9d022;
   L9d022: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9d025;
   L9d025: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9d028;
   L9d028: // MOV EAX,dword ptr [EBX + 0xfffffad4]
    eax=R(unchecked(ebx+4294965972u));goto L9d02e;
   L9d02e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9d031;
   L9d031: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9d033;
   L9d033: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9d037;
   L9d037: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L9d03a;
   L9d03a: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9d042;
   L9d042: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9d04a;
   L9d04a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d04d;
   L9d04d: // CALL 0x00049500
    try{eax=Call(0x49500,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9d05e;}goto L9d052;
   L9d052:return 0;
   L9d05e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9d062;
   L9d062: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d065;
   L9d065: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9d06a;
   L9d06a: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9d06e;
   L9d06e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9d071;
   L9d071: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructLocaleDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,localeName);W(frame+0x3cu,references);goto L9d098;
   L9d098: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9d09c;
   L9d09c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9d0a0;
   L9d0a0: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9d0a2;
   L9d0a2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9d0a5;
   L9d0a5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9d0a8;
   L9d0a8: // MOV EAX,dword ptr [EBX + 0xfffffad4]
    eax=R(unchecked(ebx+4294965972u));goto L9d0ae;
   L9d0ae: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9d0b5;
   L9d0b5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9d0b8;
   L9d0b8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9d0ba;
   L9d0ba: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9d0be;
   L9d0be: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d0c1;
   L9d0c1: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L9d0c5;
   L9d0c5: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9d0c9;
   L9d0c9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9d0cd;
   L9d0cd: // CALL 0x00049500
    try{eax=Call(0x49500,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9d0de;}goto L9d0d2;
   L9d0d2:return 0;
   L9d0de: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9d0e2;
   L9d0e2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d0e5;
   L9d0e5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9d0ea;
   L9d0ea: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9d0ee;
   L9d0ee: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9d0f1;
   L9d0f1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructDefaultInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);W(frame+0x3cu,references);goto L9d298;
   L9d298: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9d29c;
   L9d29c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9d2a0;
   L9d2a0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9d2a2;
   L9d2a2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9d2a5;
   L9d2a5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9d2a8;
   L9d2a8: // MOV EAX,dword ptr [EBX + 0xfffffd00]
    eax=R(unchecked(ebx+4294966528u));goto L9d2ae;
   L9d2ae: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9d2b5;
   L9d2b5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9d2b8;
   L9d2b8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9d2ba;
   L9d2ba: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9d2c2;
   L9d2c2: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9d2ca;
   L9d2ca: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d2cd;
   L9d2cd: // CALL 0x00049c40
    try{eax=Call(0x49c40,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9d2de;}goto L9d2d2;
   L9d2d2:return 0;
   L9d2de: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9d2e2;
   L9d2e2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d2e5;
   L9d2e5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9d2ea;
   L9d2ea: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9d2ee;
   L9d2ee: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9d2f1;
   L9d2f1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructSuppliedCacheInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);W(frame+0x3cu,references);goto L9d318;
   L9d318: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9d31c;
   L9d31c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9d320;
   L9d320: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9d322;
   L9d322: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9d325;
   L9d325: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9d328;
   L9d328: // MOV EAX,dword ptr [EBX + 0xfffffd00]
    eax=R(unchecked(ebx+4294966528u));goto L9d32e;
   L9d32e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9d331;
   L9d331: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9d333;
   L9d333: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9d337;
   L9d337: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L9d33a;
   L9d33a: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L9d342;
   L9d342: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9d34a;
   L9d34a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d34d;
   L9d34d: // CALL 0x00049c40
    try{eax=Call(0x49c40,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9d35e;}goto L9d352;
   L9d352:return 0;
   L9d35e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9d362;
   L9d362: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d365;
   L9d365: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9d36a;
   L9d36a: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9d36e;
   L9d36e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9d371;
   L9d371: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructLocaleInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,localeName);W(frame+0x3cu,references);goto L9d398;
   L9d398: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9d39c;
   L9d39c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9d3a0;
   L9d3a0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9d3a2;
   L9d3a2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9d3a5;
   L9d3a5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9d3a8;
   L9d3a8: // MOV EAX,dword ptr [EBX + 0xfffffd00]
    eax=R(unchecked(ebx+4294966528u));goto L9d3ae;
   L9d3ae: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9d3b5;
   L9d3b5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9d3b8;
   L9d3b8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9d3ba;
   L9d3ba: // MOV EAX,dword ptr [ESP + 0x38]
    eax=R(unchecked(esp+56u));goto L9d3be;
   L9d3be: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d3c1;
   L9d3c1: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L9d3c5;
   L9d3c5: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9d3c9;
   L9d3c9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9d3cd;
   L9d3cd: // CALL 0x00049c40
    try{eax=Call(0x49c40,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9d3de;}goto L9d3d2;
   L9d3d2:return 0;
   L9d3de: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9d3e2;
   L9d3e2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9d3e5;
   L9d3e5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9d3ea;
   L9d3ea: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9d3ee;
   L9d3ee: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9d3f1;
   L9d3f1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCacheDomesticWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L9d58b;
   L9d58b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L9d58f;
   L9d58f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L9d591;
   L9d591: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9d596;
   L9d596: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L9d599;
   L9d599: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L9d59c;
   L9d59c: // MOV EDX,dword ptr [ECX + 0xfffffa74]
    edx=R(unchecked(ecx+4294965876u));goto L9d5a2;
   L9d5a2: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L9d5a9;
   L9d5a9: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L9d5b0;
   L9d5b0: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L9d5b4;
   L9d5b4: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L9d5b7;
   L9d5b7: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L9d5b9;
   L9d5b9: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L9d5c0;
   L9d5c0: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L9d5c7;
   L9d5c7: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L9d5ce;
   L9d5ce: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L9d5d5;
   L9d5d5: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L9d5dc;
   L9d5dc: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L9d5e3;
   L9d5e3: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L9d5ea;
   L9d5ea: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L9d5f1;
   L9d5f1: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L9d5f8;
   L9d5f8: // MOV byte ptr [EAX + 0x38],0x0
    h.WriteByte(unchecked(eax+56u),unchecked((byte)(0u)));goto L9d5fc;
   L9d5fc: // MOV byte ptr [EAX + 0x39],0x0
    h.WriteByte(unchecked(eax+57u),unchecked((byte)(0u)));goto L9d600;
   L9d600: // MOV byte ptr [EAX + 0x3a],0x0
    h.WriteByte(unchecked(eax+58u),unchecked((byte)(0u)));goto L9d604;
   L9d604: // MOV byte ptr [EAX + 0x3b],0x0
    h.WriteByte(unchecked(eax+59u),unchecked((byte)(0u)));goto L9d608;
   L9d608: // MOV byte ptr [EAX + 0x3c],0x0
    h.WriteByte(unchecked(eax+60u),unchecked((byte)(0u)));goto L9d60c;
   L9d60c: // MOV byte ptr [EAX + 0x3d],0x0
    h.WriteByte(unchecked(eax+61u),unchecked((byte)(0u)));goto L9d610;
   L9d610: // MOV byte ptr [EAX + 0x3e],0x0
    h.WriteByte(unchecked(eax+62u),unchecked((byte)(0u)));goto L9d614;
   L9d614: // MOV byte ptr [EAX + 0x3f],0x0
    h.WriteByte(unchecked(eax+63u),unchecked((byte)(0u)));goto L9d618;
   L9d618: // MOV byte ptr [EAX + 0x6c],0x0
    h.WriteByte(unchecked(eax+108u),unchecked((byte)(0u)));goto L9d61c;
   L9d61c:return 0;
  }
  public uint ConstructCacheInternationalWide(uint frame,uint owner,uint argument,uint localeName,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L9d62b;
   L9d62b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L9d62f;
   L9d62f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L9d631;
   L9d631: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9d636;
   L9d636: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L9d639;
   L9d639: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L9d63c;
   L9d63c: // MOV EDX,dword ptr [ECX + 0xfffffc04]
    edx=R(unchecked(ecx+4294966276u));goto L9d642;
   L9d642: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L9d649;
   L9d649: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L9d650;
   L9d650: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L9d654;
   L9d654: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L9d657;
   L9d657: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L9d659;
   L9d659: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L9d660;
   L9d660: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L9d667;
   L9d667: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L9d66e;
   L9d66e: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L9d675;
   L9d675: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L9d67c;
   L9d67c: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L9d683;
   L9d683: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L9d68a;
   L9d68a: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L9d691;
   L9d691: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L9d698;
   L9d698: // MOV byte ptr [EAX + 0x38],0x0
    h.WriteByte(unchecked(eax+56u),unchecked((byte)(0u)));goto L9d69c;
   L9d69c: // MOV byte ptr [EAX + 0x39],0x0
    h.WriteByte(unchecked(eax+57u),unchecked((byte)(0u)));goto L9d6a0;
   L9d6a0: // MOV byte ptr [EAX + 0x3a],0x0
    h.WriteByte(unchecked(eax+58u),unchecked((byte)(0u)));goto L9d6a4;
   L9d6a4: // MOV byte ptr [EAX + 0x3b],0x0
    h.WriteByte(unchecked(eax+59u),unchecked((byte)(0u)));goto L9d6a8;
   L9d6a8: // MOV byte ptr [EAX + 0x3c],0x0
    h.WriteByte(unchecked(eax+60u),unchecked((byte)(0u)));goto L9d6ac;
   L9d6ac: // MOV byte ptr [EAX + 0x3d],0x0
    h.WriteByte(unchecked(eax+61u),unchecked((byte)(0u)));goto L9d6b0;
   L9d6b0: // MOV byte ptr [EAX + 0x3e],0x0
    h.WriteByte(unchecked(eax+62u),unchecked((byte)(0u)));goto L9d6b4;
   L9d6b4: // MOV byte ptr [EAX + 0x3f],0x0
    h.WriteByte(unchecked(eax+63u),unchecked((byte)(0u)));goto L9d6b8;
   L9d6b8: // MOV byte ptr [EAX + 0x6c],0x0
    h.WriteByte(unchecked(eax+108u),unchecked((byte)(0u)));goto L9d6bc;
   L9d6bc:return 0;
  }
 }
}
