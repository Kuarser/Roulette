using System;
namespace RouletteDeluxe
{
 // Physical packaged char/wchar numpunct initialization. Static C# labels
 // preserve original initialization control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Locale query, heap, cache virtual destruction and native exception owners remain required.
 public sealed class OriginalConfigurationNumpunct
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
  public OriginalConfigurationNumpunct(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint InitializeChar(uint frame,uint owner,uint locale)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x40u,owner);W(frame+0x44u,locale);goto L660a6;
   L660a6: // MOV EBP,dword ptr [ESP + 0x44]
    ebp=R(unchecked(esp+68u));goto L660aa;
   L660aa: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L660ad;
   L660ad: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L660af;
   L660af: // JZ 0x00066218
    if(zero)goto L66218;goto L660b5;
   L660b5: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L660b7;
   L660b7: // JZ 0x00066178
    if(zero)goto L66178;goto L660bd;
   L660bd: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L660c1;
   L660c1: // MOV dword ptr [ESP],0x10000
    W(unchecked(esp),65536u);goto L660c8;
   L660c8: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L660cd;
   L660cd: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L660d0;
   L660d0: // MOV byte ptr [EDI + 0x24],AL
    h.WriteByte(unchecked(edi+36u),unchecked((byte)((eax&255u))));goto L660d3;
   L660d3: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L660d6;
   L660d6: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L660da;
   L660da: // MOV dword ptr [ESP],0x10001
    W(unchecked(esp),65537u);goto L660e1;
   L660e1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L660e6;
   L660e6: // MOVZX EAX,byte ptr [EAX]
    eax=B(unchecked(eax));goto L660e9;
   L660e9: // MOV byte ptr [EDI + 0x25],AL
    h.WriteByte(unchecked(edi+37u),unchecked((byte)((eax&255u))));goto L660ec;
   L660ec: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L660ef;
   L660ef: // CMP byte ptr [EAX + 0x25],0x0
    a=B(unchecked(eax+37u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L660f3;
   L660f3: // JNZ 0x00066138
    if(!zero)goto L66138;goto L660f5;
   L660f5: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L660fb;
   L660fb: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L660fe;
   L660fe: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L66105;
   L66105: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L66109;
   L66109: // MOV byte ptr [EAX + 0x25],0x2c
    h.WriteByte(unchecked(eax+37u),unchecked((byte)(44u)));goto L6610d;
   L6610d: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66110;
   L66110: // LEA EDX,[EBX + 0xfffd8e6c]
    edx=unchecked(ebx+4294807148u);goto L66116;
   L66116: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L66119;
   L66119: // LEA EDX,[EBX + 0xfffd7862]
    edx=unchecked(ebx+4294801506u);goto L6611f;
   L6611f: // MOV dword ptr [EAX + 0x18],0x4
    W(unchecked(eax+24u),4u);goto L66126;
   L66126: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L66129;
   L66129: // MOV dword ptr [EAX + 0x20],0x5
    W(unchecked(eax+32u),5u);goto L66130;
   L66130:return 0;
   L66138: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L6613c;
   L6613c: // MOV dword ptr [ESP],0x10002
    W(unchecked(esp),65538u);goto L66143;
   L66143: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66148;
   L66148: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6614b;
   L6614b: // MOV EBP,EAX
    ebp=eax;goto L6614d;
   L6614d: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L66152;
   L66152: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66154;
   L66154: // MOV EDI,EAX
    edi=eax;goto L66156;
   L66156: // JNZ 0x000661e0
    if(!zero)goto L661e0;goto L6615c;
   L6615c: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6615f;
   L6615f: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L66165;
   L66165: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L66168;
   L66168: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L6616c;
   L6616c: // MOV dword ptr [EAX + 0xc],EDI
    W(unchecked(eax+12u),edi);goto L6616f;
   L6616f: // JMP 0x00066110
    goto L66110;
   L66178: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L6617e;
   L6617e: // MOV dword ptr [EDI + 0x8],EAX
    W(unchecked(edi+8u),eax);goto L66181;
   L66181: // MOV byte ptr [EDI + 0x24],0x2e
    h.WriteByte(unchecked(edi+36u),unchecked((byte)(46u)));goto L66185;
   L66185: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66188;
   L66188: // MOV dword ptr [EDI + 0xc],0x0
    W(unchecked(edi+12u),0u);goto L6618f;
   L6618f: // MOV byte ptr [EDI + 0x10],0x0
    h.WriteByte(unchecked(edi+16u),unchecked((byte)(0u)));goto L66193;
   L66193: // MOV byte ptr [EAX + 0x25],0x2c
    h.WriteByte(unchecked(eax+37u),unchecked((byte)(44u)));goto L66197;
   L66197: // MOV EAX,dword ptr [EBX + 0xfffffb44]
    eax=R(unchecked(ebx+4294966084u));goto L6619d;
   L6619d: // MOV EDI,dword ptr [EAX]
    edi=R(unchecked(eax));goto L6619f;
   L6619f: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L661a1;
   L661a1: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L661a8;
   L661a8: // MOVZX ECX,byte ptr [EDI + EAX*0x1]
    ecx=B(unchecked(edi+eax*0x1u));goto L661ac;
   L661ac: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L661af;
   L661af: // MOV byte ptr [EDX + EAX*0x1 + 0x26],CL
    h.WriteByte(unchecked(edx+eax*0x1u+38u),unchecked((byte)((ecx&255u))));goto L661b3;
   L661b3: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L661b6;
   L661b6: // CMP EAX,0x24
    a=eax;b=36u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L661b9;
   L661b9: // JNZ 0x000661a8
    if(!zero)goto L661a8;goto L661bb;
   L661bb: // MOV EAX,dword ptr [EBX + 0xfffffb14]
    eax=R(unchecked(ebx+4294966036u));goto L661c1;
   L661c1: // MOV EDI,dword ptr [EAX]
    edi=R(unchecked(eax));goto L661c3;
   L661c3: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L661c5;
   L661c5: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L661c8;
   L661c8: // MOVZX ECX,byte ptr [EDI + EAX*0x1]
    ecx=B(unchecked(edi+eax*0x1u));goto L661cc;
   L661cc: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L661cf;
   L661cf: // MOV byte ptr [EDX + EAX*0x1 + 0x4a],CL
    h.WriteByte(unchecked(edx+eax*0x1u+74u),unchecked((byte)((ecx&255u))));goto L661d3;
   L661d3: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L661d6;
   L661d6: // CMP EAX,0x1a
    a=eax;b=26u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L661d9;
   L661d9: // JNZ 0x000661c8
    if(!zero)goto L661c8;goto L661db;
   L661db: // JMP 0x0006610d
    goto L6610d;
   L661e0: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L661e3;
   L661e3: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L661e7;
   L661e7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L661ea;
   L661ea: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6627a;}goto L661ef;
   L661ef: // MOV EDX,EAX
    edx=eax;goto L661f1;
   L661f1: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L661f5;
   L661f5: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L661f8;
   L661f8: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L661fc;
   L661fc: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L66200;
   L66200: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L66204;
   L66204: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L66209;
   L66209: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L6620c;
   L6620c: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L66210;
   L66210: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L66213;
   L66213: // JMP 0x0006616c
    goto L6616c;
   L66218: // MOV dword ptr [ESP],0x68
    W(unchecked(esp),104u);goto L6621f;
   L6621f: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L66224;
   L66224: // MOV EDI,EAX
    edi=eax;goto L66226;
   L66226: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L6622d;
   L6622d: // MOV EAX,dword ptr [EBX + 0xfffffde4]
    eax=R(unchecked(ebx+4294966756u));goto L66233;
   L66233: // MOV dword ptr [EDI + 0x8],0x0
    W(unchecked(edi+8u),0u);goto L6623a;
   L6623a: // MOV dword ptr [EDI + 0xc],0x0
    W(unchecked(edi+12u),0u);goto L66241;
   L66241: // MOV byte ptr [EDI + 0x10],0x0
    h.WriteByte(unchecked(edi+16u),unchecked((byte)(0u)));goto L66245;
   L66245: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L66248;
   L66248: // MOV dword ptr [EDI],EAX
    W(unchecked(edi),eax);goto L6624a;
   L6624a: // MOV dword ptr [EDI + 0x14],0x0
    W(unchecked(edi+20u),0u);goto L66251;
   L66251: // MOV dword ptr [EDI + 0x18],0x0
    W(unchecked(edi+24u),0u);goto L66258;
   L66258: // MOV dword ptr [EDI + 0x1c],0x0
    W(unchecked(edi+28u),0u);goto L6625f;
   L6625f: // MOV dword ptr [EDI + 0x20],0x0
    W(unchecked(edi+32u),0u);goto L66266;
   L66266: // MOV byte ptr [EDI + 0x24],0x0
    h.WriteByte(unchecked(edi+36u),unchecked((byte)(0u)));goto L6626a;
   L6626a: // MOV byte ptr [EDI + 0x25],0x0
    h.WriteByte(unchecked(edi+37u),unchecked((byte)(0u)));goto L6626e;
   L6626e: // MOV byte ptr [EDI + 0x64],0x0
    h.WriteByte(unchecked(edi+100u),unchecked((byte)(0u)));goto L66272;
   L66272: // MOV dword ptr [ESI + 0x8],EDI
    W(unchecked(esi+8u),edi);goto L66275;
   L66275: // JMP 0x000660b5
    goto L660b5;
   L6627a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6627d;
   L6627d: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L66282;
   L66282: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66285;
   L66285: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66287;
   L66287: // JZ 0x00066291
    if(zero)goto L66291;goto L66289;
   L66289: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L6628b;
   L6628b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6628e;
   L6628e: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6629d;}goto L66291;
   L66291: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L66298;
   L66298: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6629d;}
   L6629d: // MOV ESI,EAX
    esi=eax;goto L6629f;
   L6629f: // CALL 0x000497e0
    eax=Call(0x497e0);goto L662a4;
   L662a4: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L662a7;
   L662a7: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint InitializeWide(uint frame,uint owner,uint locale)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x40u,owner);W(frame+0x44u,locale);goto L66386;
   L66386: // MOV EDI,dword ptr [ESP + 0x44]
    edi=R(unchecked(esp+68u));goto L6638a;
   L6638a: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L6638d;
   L6638d: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6638f;
   L6638f: // JZ 0x000664f0
    if(zero)goto L664f0;goto L66395;
   L66395: // TEST EDI,EDI
    a=edi;b=edi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66397;
   L66397: // JZ 0x00066450
    if(zero)goto L66450;goto L6639d;
   L6639d: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L663a1;
   L663a1: // MOV dword ptr [ESP],0x10003
    W(unchecked(esp),65539u);goto L663a8;
   L663a8: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L663ad;
   L663ad: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L663b0;
   L663b0: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L663b3;
   L663b3: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L663b7;
   L663b7: // MOV dword ptr [ESP],0x10004
    W(unchecked(esp),65540u);goto L663be;
   L663be: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L663c3;
   L663c3: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L663c6;
   L663c6: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L663c8;
   L663c8: // MOV dword ptr [ECX + 0x28],EAX
    W(unchecked(ecx+40u),eax);goto L663cb;
   L663cb: // JNZ 0x00066410
    if(!zero)goto L66410;goto L663cd;
   L663cd: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L663d3;
   L663d3: // MOV dword ptr [ECX + 0x8],EAX
    W(unchecked(ecx+8u),eax);goto L663d6;
   L663d6: // MOV dword ptr [ECX + 0xc],0x0
    W(unchecked(ecx+12u),0u);goto L663dd;
   L663dd: // MOV byte ptr [ECX + 0x10],0x0
    h.WriteByte(unchecked(ecx+16u),unchecked((byte)(0u)));goto L663e1;
   L663e1: // MOV dword ptr [ECX + 0x28],0x2c
    W(unchecked(ecx+40u),44u);goto L663e8;
   L663e8: // LEA EAX,[EBX + 0xfffd8e74]
    eax=unchecked(ebx+4294807156u);goto L663ee;
   L663ee: // MOV dword ptr [ECX + 0x14],EAX
    W(unchecked(ecx+20u),eax);goto L663f1;
   L663f1: // LEA EAX,[EBX + 0xfffd8e88]
    eax=unchecked(ebx+4294807176u);goto L663f7;
   L663f7: // MOV dword ptr [ECX + 0x18],0x4
    W(unchecked(ecx+24u),4u);goto L663fe;
   L663fe: // MOV dword ptr [ECX + 0x1c],EAX
    W(unchecked(ecx+28u),eax);goto L66401;
   L66401: // MOV dword ptr [ECX + 0x20],0x5
    W(unchecked(ecx+32u),5u);goto L66408;
   L66408:return 0;
   L66410: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L66414;
   L66414: // MOV dword ptr [ESP],0x10002
    W(unchecked(esp),65538u);goto L6641b;
   L6641b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66420;
   L66420: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L66423;
   L66423: // MOV EBP,EAX
    ebp=eax;goto L66425;
   L66425: // CALL 0x00049330
    eax=Call(0x49330,R(unchecked(esp+0u)));goto L6642a;
   L6642a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6642c;
   L6642c: // MOV EDI,EAX
    edi=eax;goto L6642e;
   L6642e: // JNZ 0x000664b8
    if(!zero)goto L664b8;goto L66434;
   L66434: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L66437;
   L66437: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L6643d;
   L6643d: // MOV dword ptr [ECX + 0x8],EAX
    W(unchecked(ecx+8u),eax);goto L66440;
   L66440: // MOV byte ptr [ECX + 0x10],0x0
    h.WriteByte(unchecked(ecx+16u),unchecked((byte)(0u)));goto L66444;
   L66444: // MOV dword ptr [ECX + 0xc],EDI
    W(unchecked(ecx+12u),edi);goto L66447;
   L66447: // JMP 0x000663e8
    goto L663e8;
   L66450: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L66453;
   L66453: // LEA EAX,[EBX + 0xfffdb072]
    eax=unchecked(ebx+4294815858u);goto L66459;
   L66459: // MOV dword ptr [ECX + 0x8],EAX
    W(unchecked(ecx+8u),eax);goto L6645c;
   L6645c: // MOV EAX,dword ptr [EBX + 0xfffffb44]
    eax=R(unchecked(ebx+4294966084u));goto L66462;
   L66462: // MOV dword ptr [ECX + 0xc],0x0
    W(unchecked(ecx+12u),0u);goto L66469;
   L66469: // MOV byte ptr [ECX + 0x10],0x0
    h.WriteByte(unchecked(ecx+16u),unchecked((byte)(0u)));goto L6646d;
   L6646d: // MOV dword ptr [ECX + 0x24],0x2e
    W(unchecked(ecx+36u),46u);goto L66474;
   L66474: // MOV ESI,dword ptr [EAX]
    esi=R(unchecked(eax));goto L66476;
   L66476: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L66478;
   L66478: // MOV dword ptr [ECX + 0x28],0x2c
    W(unchecked(ecx+40u),44u);goto L6647f;
   L6647f: // NOP
    goto L66480;
   L66480: // MOVSX EDX,byte ptr [ESI + EAX*0x1]
    edx=unchecked((uint)(int)(sbyte)(B(unchecked(esi+eax*0x1u))));goto L66484;
   L66484: // MOV dword ptr [ECX + EAX*0x4 + 0x2c],EDX
    W(unchecked(ecx+eax*0x4u+44u),edx);goto L66488;
   L66488: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L6648b;
   L6648b: // CMP EAX,0x24
    a=eax;b=36u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6648e;
   L6648e: // JNZ 0x00066480
    if(!zero)goto L66480;goto L66490;
   L66490: // MOV EAX,dword ptr [EBX + 0xfffffb14]
    eax=R(unchecked(ebx+4294966036u));goto L66496;
   L66496: // MOV ESI,dword ptr [EAX]
    esi=R(unchecked(eax));goto L66498;
   L66498: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L6649a;
   L6649a: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L664a0;
   L664a0: // MOVSX EDX,byte ptr [ESI + EAX*0x1]
    edx=unchecked((uint)(int)(sbyte)(B(unchecked(esi+eax*0x1u))));goto L664a4;
   L664a4: // MOV dword ptr [ECX + EAX*0x4 + 0xbc],EDX
    W(unchecked(ecx+eax*0x4u+188u),edx);goto L664ab;
   L664ab: // ADD EAX,0x1
    a=eax;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L664ae;
   L664ae: // CMP EAX,0x1a
    a=eax;b=26u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L664b1;
   L664b1: // JNZ 0x000664a0
    if(!zero)goto L664a0;goto L664b3;
   L664b3: // JMP 0x000663e8
    goto L663e8;
   L664b8: // LEA EAX,[EAX + 0x1]
    eax=unchecked(eax+1u);goto L664bb;
   L664bb: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L664bf;
   L664bf: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L664c2;
   L664c2: // CALL 0x00048100
    try{eax=Call(0x48100,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L66559;}goto L664c7;
   L664c7: // MOV EDX,EAX
    edx=eax;goto L664c9;
   L664c9: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L664cd;
   L664cd: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L664d0;
   L664d0: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L664d4;
   L664d4: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L664d8;
   L664d8: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L664dc;
   L664dc: // CALL 0x000492f0
    eax=Call(0x492f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L664e1;
   L664e1: // MOV ECX,dword ptr [ESI + 0x8]
    ecx=R(unchecked(esi+8u));goto L664e4;
   L664e4: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L664e8;
   L664e8: // MOV dword ptr [ECX + 0x8],EDX
    W(unchecked(ecx+8u),edx);goto L664eb;
   L664eb: // JMP 0x00066444
    goto L66444;
   L664f0: // MOV dword ptr [ESP],0x128
    W(unchecked(esp),296u);goto L664f7;
   L664f7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L664fc;
   L664fc: // MOV EDX,dword ptr [EBX + 0xffffffd8]
    edx=R(unchecked(ebx+4294967256u));goto L66502;
   L66502: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L66505;
   L66505: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L6650c;
   L6650c: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L6650e;
   L6650e: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L66515;
   L66515: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L6651c;
   L6651c: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L66520;
   L66520: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L66527;
   L66527: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L6652e;
   L6652e: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L66535;
   L66535: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L6653c;
   L6653c: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L66543;
   L66543: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L6654a;
   L6654a: // MOV byte ptr [EAX + 0x124],0x0
    h.WriteByte(unchecked(eax+292u),unchecked((byte)(0u)));goto L66551;
   L66551: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L66554;
   L66554: // JMP 0x00066395
    goto L66395;
   L66559: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6655c;
   L6655c: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L66561;
   L66561: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66564;
   L66564: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66566;
   L66566: // JZ 0x00066570
    if(zero)goto L66570;goto L66568;
   L66568: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L6656a;
   L6656a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6656d;
   L6656d: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6657c;}goto L66570;
   L66570: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L66577;
   L66577: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L6657c;}
   L6657c: // MOV ESI,EAX
    esi=eax;goto L6657e;
   L6657e: // CALL 0x000497e0
    eax=Call(0x497e0);goto L66583;
   L66583: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66586;
   L66586: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
