using System;
namespace RouletteDeluxe
{
 // Physical packaged class and single-inheritance RTTI traversal. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; original RTTI/object/path inputs are supplied explicitly. Imported strcmp and recursive virtual traversal remain required.
 public sealed class OriginalConfigurationRttiClass
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
  public OriginalConfigurationRttiClass(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint PublicUpcast(uint frame,uint owner,uint target,uint objectPtr,uint result,uint hint,uint path,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x30u,owner);W(frame+0x34u,target);W(frame+0x38u,objectPtr);goto Lb1844;
   Lb1844: // MOV EAX,dword ptr [ESP + 0x30]
    eax=R(unchecked(esp+48u));goto Lb1848;
   Lb1848: // LEA ECX,[ESP + 0x10]
    ecx=unchecked(esp+16u);goto Lb184c;
   Lb184c: // MOV ESI,dword ptr [ESP + 0x38]
    esi=R(unchecked(esp+56u));goto Lb1850;
   Lb1850: // MOV dword ptr [ESP + 0x10],0x0
    W(unchecked(esp+16u),0u);goto Lb1858;
   Lb1858: // MOV dword ptr [ESP + 0x14],0x0
    W(unchecked(esp+20u),0u);goto Lb1860;
   Lb1860: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto Lb1862;
   Lb1862: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto Lb1866;
   Lb1866: // MOV ECX,dword ptr [ESI]
    ecx=R(unchecked(esi));goto Lb1868;
   Lb1868: // MOV dword ptr [ESP + 0x18],0x10
    W(unchecked(esp+24u),16u);goto Lb1870;
   Lb1870: // MOV dword ptr [ESP + 0x1c],0x0
    W(unchecked(esp+28u),0u);goto Lb1878;
   Lb1878: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb187b;
   Lb187b: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto Lb187f;
   Lb187f: // MOV ECX,dword ptr [ESP + 0x34]
    ecx=R(unchecked(esp+52u));goto Lb1883;
   Lb1883: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto Lb1887;
   Lb1887: // CALL dword ptr [EDX + 0x18]
    eax=Call(R(unchecked(edx+24u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto Lb188a;
   Lb188a: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto Lb188e;
   Lb188e: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1890;
   Lb1890: // AND EDX,0x6
    a=edx;b=6u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto Lb1893;
   Lb1893: // CMP EDX,0x6
    a=edx;b=6u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1896;
   Lb1896: // JNZ 0x000b18a3
    if(!zero)goto Lb18a3;goto Lb1898;
   Lb1898: // MOV EAX,dword ptr [ESP + 0x10]
    eax=R(unchecked(esp+16u));goto Lb189c;
   Lb189c: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto Lb189e;
   Lb189e: // MOV EAX,0x1
    eax=1u;goto Lb18a3;
   Lb18a3:return eax;
  }
  public uint ClassUpcast(uint frame,uint owner,uint target,uint objectPtr,uint result,uint hint,uint path,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;edx=target;W(frame+0x20u,owner);W(frame+0x24u,target);W(frame+0x28u,objectPtr);W(frame+0x2cu,result);goto Lb195a;
   Lb195a: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto Lb195d;
   Lb195d: // MOV EDX,dword ptr [EDX + 0x4]
    edx=R(unchecked(edx+4u));goto Lb1960;
   Lb1960: // MOV dword ptr [ESP + 0x18],EDI
    W(unchecked(esp+24u),edi);goto Lb1964;
   Lb1964: // MOV EDI,dword ptr [ESP + 0x2c]
    edi=R(unchecked(esp+44u));goto Lb1968;
   Lb1968: // MOV dword ptr [ESP + 0x14],ESI
    W(unchecked(esp+20u),esi);goto Lb196c;
   Lb196c: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb196e;
   Lb196e: // JZ 0x000b19a0
    if(zero)goto Lb19a0;goto Lb1970;
   Lb1970: // XOR ESI,ESI
    a=esi;b=esi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;esi=v;goto Lb1972;
   Lb1972: // CMP byte ptr [EAX],0x2a
    a=B(unchecked(eax));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1975;
   Lb1975: // JZ 0x000b1987
    if(zero)goto Lb1987;goto Lb1977;
   Lb1977: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb197b;
   Lb197b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb197e;
   Lb197e: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb1983;
   Lb1983: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1985;
   Lb1985: // JZ 0x000b19a0
    if(zero)goto Lb19a0;goto Lb1987;
   Lb1987:return esi;
   Lb19a0: // MOV EAX,dword ptr [ESP + 0x28]
    eax=R(unchecked(esp+40u));goto Lb19a4;
   Lb19a4: // MOV ESI,0x1
    esi=1u;goto Lb19a9;
   Lb19a9: // MOV dword ptr [EDI + 0xc],0x8
    W(unchecked(edi+12u),8u);goto Lb19b0;
   Lb19b0: // MOV dword ptr [EDI + 0x4],0x6
    W(unchecked(edi+4u),6u);goto Lb19b7;
   Lb19b7: // MOV dword ptr [EDI],EAX
    W(unchecked(edi),eax);goto Lb19b9;
   Lb19b9: // JMP 0x000b1987
    goto Lb1987;
  }
  public uint ClassDyncast(uint frame,uint owner,uint target,uint objectPtr,uint result,uint hint,uint path,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;edi=objectPtr;W(frame+0x30u,owner);W(frame+0x34u,hint);W(frame+0x38u,path);W(frame+0x3cu,target);W(frame+0x40u,objectPtr);W(frame+0x44u,sourceType);W(frame+0x48u,sourcePtr);W(frame+0x4cu,result);goto Lb1a6e;
   Lb1a6e: // CMP EDI,dword ptr [ESP + 0x48]
    a=edi;b=R(unchecked(esp+72u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1a72;
   Lb1a72: // MOV dword ptr [ESP + 0x20],ESI
    W(unchecked(esp+32u),esi);goto Lb1a76;
   Lb1a76: // MOV ESI,dword ptr [EAX + 0x4]
    esi=R(unchecked(eax+4u));goto Lb1a79;
   Lb1a79: // MOV dword ptr [ESP + 0x28],EBP
    W(unchecked(esp+40u),ebp);goto Lb1a7d;
   Lb1a7d: // MOV EBP,dword ptr [ESP + 0x4c]
    ebp=R(unchecked(esp+76u));goto Lb1a81;
   Lb1a81: // JZ 0x000b1ad8
    if(zero)goto Lb1ad8;goto Lb1a83;
   Lb1a83: // MOV EAX,dword ptr [ESP + 0x3c]
    eax=R(unchecked(esp+60u));goto Lb1a87;
   Lb1a87: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto Lb1a8a;
   Lb1a8a: // CMP ESI,EAX
    a=esi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1a8c;
   Lb1a8c: // JZ 0x000b1ac0
    if(zero)goto Lb1ac0;goto Lb1a8e;
   Lb1a8e: // CMP byte ptr [ESI],0x2a
    a=B(unchecked(esi));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1a91;
   Lb1a91: // JZ 0x000b1aa3
    if(zero)goto Lb1aa3;goto Lb1a93;
   Lb1a93: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb1a97;
   Lb1a97: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto Lb1a9a;
   Lb1a9a: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb1a9f;
   Lb1a9f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1aa1;
   Lb1aa1: // JZ 0x000b1ac0
    if(zero)goto Lb1ac0;goto Lb1aa3;
   Lb1aa3: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb1aa5;
   Lb1aa5:return eax;
   Lb1ac0: // MOV EDX,dword ptr [ESP + 0x38]
    edx=R(unchecked(esp+56u));goto Lb1ac4;
   Lb1ac4: // MOV dword ptr [EBP],EDI
    W(unchecked(ebp),edi);goto Lb1ac7;
   Lb1ac7: // MOV dword ptr [EBP + 0xc],0x1
    W(unchecked(ebp+12u),1u);goto Lb1ace;
   Lb1ace: // MOV dword ptr [EBP + 0x4],EDX
    W(unchecked(ebp+4u),edx);goto Lb1ad1;
   Lb1ad1: // JMP 0x000b1aa3
    goto Lb1aa3;
   Lb1ad8: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto Lb1adc;
   Lb1adc: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto Lb1adf;
   Lb1adf: // CMP ESI,EAX
    a=esi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1ae1;
   Lb1ae1: // JZ 0x000b1af8
    if(zero)goto Lb1af8;goto Lb1ae3;
   Lb1ae3: // CMP byte ptr [ESI],0x2a
    a=B(unchecked(esi));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb1ae6;
   Lb1ae6: // JZ 0x000b1a83
    if(zero)goto Lb1a83;goto Lb1ae8;
   Lb1ae8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb1aec;
   Lb1aec: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto Lb1aef;
   Lb1aef: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb1af4;
   Lb1af4: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb1af6;
   Lb1af6: // JNZ 0x000b1a83
    if(!zero)goto Lb1a83;goto Lb1af8;
   Lb1af8: // MOV EDX,dword ptr [ESP + 0x38]
    edx=R(unchecked(esp+56u));goto Lb1afc;
   Lb1afc: // MOV dword ptr [EBP + 0x8],EDX
    W(unchecked(ebp+8u),edx);goto Lb1aff;
   Lb1aff: // JMP 0x000b1aa3
    goto Lb1aa3;
  }
  public uint SingleUpcast(uint frame,uint owner,uint target,uint objectPtr,uint result,uint hint,uint path,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=owner;esi=target;edi=objectPtr;ebp=result;W(frame+0x30u,owner);W(frame+0x34u,target);W(frame+0x38u,objectPtr);W(frame+0x3cu,result);goto Lb430e;
   Lb430e: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto Lb4311;
   Lb4311: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto Lb4315;
   Lb4315: // MOV dword ptr [ESP + 0x8],EDI
    W(unchecked(esp+8u),edi);goto Lb4319;
   Lb4319: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb431d;
   Lb431d: // CALL 0x00048e00
    eax=Call(0x48e00,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto Lb4322;
   Lb4322: // MOV EDX,0x1
    edx=1u;goto Lb4327;
   Lb4327: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4329;
   Lb4329: // JNZ 0x000b4348
    if(!zero)goto Lb4348;goto Lb432b;
   Lb432b: // MOV EDX,dword ptr [ESP + 0x30]
    edx=R(unchecked(esp+48u));goto Lb432f;
   Lb432f: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto Lb4332;
   Lb4332: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto Lb4334;
   Lb4334: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb4338;
   Lb4338: // MOV dword ptr [ESP + 0x8],EDI
    W(unchecked(esp+8u),edi);goto Lb433c;
   Lb433c: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto Lb4340;
   Lb4340: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb4343;
   Lb4343: // CALL dword ptr [EDX + 0x18]
    eax=Call(R(unchecked(edx+24u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto Lb4346;
   Lb4346: // MOV EDX,EAX
    edx=eax;goto Lb4348;
   Lb4348: // MOV EAX,EDX
    eax=edx;goto Lb434a;
   Lb434a:return eax;
  }
  public uint SingleDyncast(uint frame,uint owner,uint target,uint objectPtr,uint result,uint hint,uint path,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=owner;ebp=target;esi=R(owner+4u);eax=R(target+4u);W(frame+0x50u,owner);W(frame+0x54u,hint);W(frame+0x58u,path);W(frame+0x5cu,target);W(frame+0x60u,objectPtr);W(frame+0x64u,sourceType);W(frame+0x68u,sourcePtr);W(frame+0x6cu,result);goto Lb438c;
   Lb438c: // MOV EDI,dword ptr [ESP + 0x60]
    edi=R(unchecked(esp+96u));goto Lb4390;
   Lb4390: // CMP ESI,EAX
    a=esi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4392;
   Lb4392: // JZ 0x000b4420
    if(zero)goto Lb4420;goto Lb4398;
   Lb4398: // MOVZX ECX,byte ptr [ESI]
    ecx=B(unchecked(esi));goto Lb439b;
   Lb439b: // CMP CL,0x2a
    a=(ecx&255u);b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb439e;
   Lb439e: // JZ 0x000b43c1
    if(zero)goto Lb43c1;goto Lb43a0;
   Lb43a0: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb43a4;
   Lb43a4: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto Lb43a7;
   Lb43a7: // MOV dword ptr [ESP + 0x28],EDX
    W(unchecked(esp+40u),edx);goto Lb43ab;
   Lb43ab: // MOV byte ptr [ESP + 0x2c],CL
    h.WriteByte(unchecked(esp+44u),unchecked((byte)((ecx&255u))));goto Lb43af;
   Lb43af: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb43b4;
   Lb43b4: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto Lb43b8;
   Lb43b8: // MOVZX ECX,byte ptr [ESP + 0x2c]
    ecx=B(unchecked(esp+44u));goto Lb43bd;
   Lb43bd: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb43bf;
   Lb43bf: // JZ 0x000b4420
    if(zero)goto Lb4420;goto Lb43c1;
   Lb43c1: // CMP EDI,dword ptr [ESP + 0x68]
    a=edi;b=R(unchecked(esp+104u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb43c5;
   Lb43c5: // JZ 0x000b4468
    if(zero)goto Lb4468;goto Lb43cb;
   Lb43cb: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto Lb43ce;
   Lb43ce: // MOV ECX,dword ptr [ESP + 0x6c]
    ecx=R(unchecked(esp+108u));goto Lb43d2;
   Lb43d2: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto Lb43d4;
   Lb43d4: // MOV dword ptr [ESP + 0x1c],ECX
    W(unchecked(esp+28u),ecx);goto Lb43d8;
   Lb43d8: // MOV ECX,dword ptr [ESP + 0x68]
    ecx=R(unchecked(esp+104u));goto Lb43dc;
   Lb43dc: // MOV dword ptr [ESP + 0x10],EDI
    W(unchecked(esp+16u),edi);goto Lb43e0;
   Lb43e0: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb43e4;
   Lb43e4: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb43e7;
   Lb43e7: // MOV dword ptr [ESP + 0x18],ECX
    W(unchecked(esp+24u),ecx);goto Lb43eb;
   Lb43eb: // MOV ECX,dword ptr [ESP + 0x64]
    ecx=R(unchecked(esp+100u));goto Lb43ef;
   Lb43ef: // MOV dword ptr [ESP + 0x14],ECX
    W(unchecked(esp+20u),ecx);goto Lb43f3;
   Lb43f3: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto Lb43f7;
   Lb43f7: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto Lb43fb;
   Lb43fb: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto Lb43ff;
   Lb43ff: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto Lb4403;
   Lb4403: // CALL dword ptr [EDX + 0x1c]
    eax=Call(R(unchecked(edx+28u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)),R(unchecked(esp+20u)),R(unchecked(esp+24u)),R(unchecked(esp+28u)));goto Lb4406;
   Lb4406:return eax;
   Lb4420: // MOV EAX,dword ptr [ESP + 0x54]
    eax=R(unchecked(esp+84u));goto Lb4424;
   Lb4424: // MOV EDX,dword ptr [ESP + 0x6c]
    edx=R(unchecked(esp+108u));goto Lb4428;
   Lb4428: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto Lb442c;
   Lb442c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb442e;
   Lb442e: // MOV dword ptr [EDX],EDI
    W(unchecked(edx),edi);goto Lb4430;
   Lb4430: // MOV dword ptr [EDX + 0x4],ECX
    W(unchecked(edx+4u),ecx);goto Lb4433;
   Lb4433: // JS 0x000b4450
    if(sign)goto Lb4450;goto Lb4435;
   Lb4435: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb4437;
   Lb4437: // ADD EDI,dword ptr [ESP + 0x54]
    a=edi;b=R(unchecked(esp+84u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto Lb443b;
   Lb443b: // CMP dword ptr [ESP + 0x68],EDI
    a=R(unchecked(esp+104u));b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb443f;
   Lb443f: // SETZ AL
    eax=(eax&0xffffff00u)|((zero?1u:0u)&255u);goto Lb4442;
   Lb4442: // LEA EAX,[EAX + EAX*0x4 + 0x1]
    eax=unchecked(eax+eax*0x4u+1u);goto Lb4446;
   Lb4446: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto Lb4449;
   Lb4449: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb444b;
   Lb444b: // JMP 0x000b4406
    goto Lb4406;
   Lb4450: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb4452;
   Lb4452: // CMP dword ptr [ESP + 0x54],-0x2
    a=R(unchecked(esp+84u));b=4294967294u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4457;
   Lb4457: // JNZ 0x000b4406
    if(!zero)goto Lb4406;goto Lb4459;
   Lb4459: // MOV EDX,dword ptr [ESP + 0x6c]
    edx=R(unchecked(esp+108u));goto Lb445d;
   Lb445d: // MOV dword ptr [EDX + 0xc],0x1
    W(unchecked(edx+12u),1u);goto Lb4464;
   Lb4464: // JMP 0x000b4406
    goto Lb4406;
   Lb4468: // MOV EAX,dword ptr [ESP + 0x64]
    eax=R(unchecked(esp+100u));goto Lb446c;
   Lb446c: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto Lb446f;
   Lb446f: // CMP ESI,EAX
    a=esi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4471;
   Lb4471: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto Lb4475;
   Lb4475: // JZ 0x000b449c
    if(zero)goto Lb449c;goto Lb4477;
   Lb4477: // CMP CL,0x2a
    a=(ecx&255u);b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb447a;
   Lb447a: // JZ 0x000b43cb
    if(zero)goto Lb43cb;goto Lb4480;
   Lb4480: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb4484;
   Lb4484: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto Lb4487;
   Lb4487: // MOV dword ptr [ESP + 0x28],EDX
    W(unchecked(esp+40u),edx);goto Lb448b;
   Lb448b: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb4490;
   Lb4490: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto Lb4494;
   Lb4494: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb4496;
   Lb4496: // JNZ 0x000b43cb
    if(!zero)goto Lb43cb;goto Lb449c;
   Lb449c: // MOV ECX,dword ptr [ESP + 0x58]
    ecx=R(unchecked(esp+88u));goto Lb44a0;
   Lb44a0: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb44a2;
   Lb44a2: // MOV EDX,dword ptr [ESP + 0x6c]
    edx=R(unchecked(esp+108u));goto Lb44a6;
   Lb44a6: // MOV dword ptr [EDX + 0x8],ECX
    W(unchecked(edx+8u),ecx);goto Lb44a9;
   Lb44a9: // JMP 0x000b4406
    goto Lb4406;
  }
 }
}
