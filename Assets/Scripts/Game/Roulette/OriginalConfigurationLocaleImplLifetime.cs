using System;
namespace RouletteDeluxe
{
 // Physical packaged locale Impl D1/D2 000597c0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; ESI captures the original owner. Virtual facet/cache D0, array deletion and native catch/exception ownership remain explicit.
 public sealed class OriginalConfigurationLocaleImplLifetime
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
  public OriginalConfigurationLocaleImplLifetime(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Destroy(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x30u,owner);goto L597d6;
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
 }
}
