using System;
namespace RouletteDeluxe
{
 // Physical packaged class, single and multiple-inheritance public source search. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame follows each original method entry contract. Type, hint and object inputs are explicit; strcmp and recursive virtual search remain required.
 public sealed class OriginalConfigurationRttiPublicSource
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
  public OriginalConfigurationRttiPublicSource(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ClassFind(uint frame,uint owner,uint hint,uint objectPtr,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+4u,owner);W(frame+8u,hint);W(frame+12u,objectPtr);W(frame+16u,sourceType);W(frame+20u,sourcePtr);goto Lb18b0;
   Lb18b0: // MOV EAX,dword ptr [ESP + 0xc]
    eax=R(unchecked(esp+12u));goto Lb18b4;
   Lb18b4: // CMP dword ptr [ESP + 0x14],EAX
    a=R(unchecked(esp+20u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb18b8;
   Lb18b8: // SETZ AL
    eax=(eax&0xffffff00u)|((zero?1u:0u)&255u);goto Lb18bb;
   Lb18bb: // MOVZX EAX,AL
    eax=(eax&255u);goto Lb18be;
   Lb18be: // LEA EAX,[EAX + EAX*0x4 + 0x1]
    eax=unchecked(eax+eax*0x4u+1u);goto Lb18c2;
   Lb18c2:return eax;
  }
  public uint SingleFind(uint frame,uint owner,uint hint,uint objectPtr,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=owner;esi=objectPtr;edi=sourcePtr;W(frame+0x50u,owner);W(frame+0x54u,hint);W(frame+0x58u,objectPtr);W(frame+0x5cu,sourceType);W(frame+0x60u,sourcePtr);goto Lb44d6;
   Lb44d6: // MOV ECX,dword ptr [ESP + 0x5c]
    ecx=R(unchecked(esp+92u));goto Lb44da;
   Lb44da: // MOV dword ptr [ESP + 0x48],EBP
    W(unchecked(esp+72u),ebp);goto Lb44de;
   Lb44de: // CMP EDI,ESI
    a=edi;b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb44e0;
   Lb44e0: // JZ 0x000b4520
    if(zero)goto Lb4520;goto Lb44e2;
   Lb44e2: // MOV EAX,dword ptr [EDX + 0x8]
    eax=R(unchecked(edx+8u));goto Lb44e5;
   Lb44e5: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto Lb44e7;
   Lb44e7: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto Lb44eb;
   Lb44eb: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto Lb44ef;
   Lb44ef: // MOV dword ptr [ESP + 0x10],EDI
    W(unchecked(esp+16u),edi);goto Lb44f3;
   Lb44f3: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto Lb44f7;
   Lb44f7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb44fa;
   Lb44fa: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto Lb44fe;
   Lb44fe: // CALL dword ptr [EDX + 0x20]
    eax=Call(R(unchecked(edx+32u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)));goto Lb4501;
   Lb4501: // MOV EBP,EAX
    ebp=eax;goto Lb4503;
   Lb4503: // MOV EAX,EBP
    eax=ebp;goto Lb4505;
   Lb4505:return eax;
   Lb4520: // MOV EAX,dword ptr [EDX + 0x4]
    eax=R(unchecked(edx+4u));goto Lb4523;
   Lb4523: // MOV EBP,dword ptr [ECX + 0x4]
    ebp=R(unchecked(ecx+4u));goto Lb4526;
   Lb4526: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto Lb452a;
   Lb452a: // MOV dword ptr [ESP + 0x2c],EBP
    W(unchecked(esp+44u),ebp);goto Lb452e;
   Lb452e: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto Lb4532;
   Lb4532: // MOV EBP,0x6
    ebp=6u;goto Lb4537;
   Lb4537: // CMP dword ptr [ESP + 0x20],EAX
    a=R(unchecked(esp+32u));b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb453b;
   Lb453b: // JZ 0x000b4503
    if(zero)goto Lb4503;goto Lb453d;
   Lb453d: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto Lb4541;
   Lb4541: // CMP byte ptr [EAX],0x2a
    a=B(unchecked(eax));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb4544;
   Lb4544: // JZ 0x000b44e2
    if(zero)goto Lb44e2;goto Lb4546;
   Lb4546: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto Lb454a;
   Lb454a: // MOV dword ptr [ESP + 0x28],EDX
    W(unchecked(esp+40u),edx);goto Lb454e;
   Lb454e: // MOV dword ptr [ESP + 0x24],ECX
    W(unchecked(esp+36u),ecx);goto Lb4552;
   Lb4552: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto Lb4556;
   Lb4556: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto Lb455a;
   Lb455a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb455d;
   Lb455d: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb4562;
   Lb4562: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto Lb4566;
   Lb4566: // MOV ECX,dword ptr [ESP + 0x24]
    ecx=R(unchecked(esp+36u));goto Lb456a;
   Lb456a: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb456c;
   Lb456c: // JNZ 0x000b44e2
    if(!zero)goto Lb44e2;goto Lb4572;
   Lb4572: // JMP 0x000b4503
    goto Lb4503;
  }
  public uint MultipleFind(uint frame,uint owner,uint hint,uint objectPtr,uint sourceType,uint sourcePtr)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=sourcePtr;W(frame+0x50u,owner);W(frame+0x54u,hint);W(frame+0x58u,objectPtr);W(frame+0x5cu,sourceType);W(frame+0x60u,sourcePtr);goto Lb55f6;
   Lb55f6: // MOV EDI,dword ptr [ESP + 0x50]
    edi=R(unchecked(esp+80u));goto Lb55fa;
   Lb55fa: // CMP dword ptr [ESP + 0x58],EDX
    a=R(unchecked(esp+88u));b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb55fe;
   Lb55fe: // JZ 0x000b5690
    if(zero)goto Lb5690;goto Lb5604;
   Lb5604: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto Lb5607;
   Lb5607: // NOP
    goto Lb5608;
   Lb5608: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb560a;
   Lb560a: // JZ 0x000b5680
    if(zero)goto Lb5680;goto Lb560c;
   Lb560c: // SUB ESI,0x1
    a=esi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto Lb560f;
   Lb560f: // MOV EDX,dword ptr [EDI + ESI*0x8 + 0x14]
    edx=R(unchecked(edi+esi*0x8u+20u));goto Lb5613;
   Lb5613: // TEST DL,0x2
    a=(edx&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5616;
   Lb5616: // JZ 0x000b5608
    if(zero)goto Lb5608;goto Lb5618;
   Lb5618: // MOV EAX,EDX
    eax=edx;goto Lb561a;
   Lb561a: // MOV ECX,EDX
    ecx=edx;goto Lb561c;
   Lb561c: // SAR EAX,0x8
    a=eax;v=unchecked((uint)((int)a>>8));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&128u)!=0;overflow=false;eax=v;goto Lb561f;
   Lb561f: // AND ECX,0x1
    a=ecx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb5622;
   Lb5622: // MOV byte ptr [ESP + 0x2f],CL
    h.WriteByte(unchecked(esp+47u),unchecked((byte)((ecx&255u))));goto Lb5626;
   Lb5626: // JZ 0x000b5638
    if(zero)goto Lb5638;goto Lb5628;
   Lb5628: // CMP dword ptr [ESP + 0x54],-0x3
    a=R(unchecked(esp+84u));b=4294967293u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb562d;
   Lb562d: // JZ 0x000b5608
    if(zero)goto Lb5608;goto Lb562f;
   Lb562f: // MOV EBP,dword ptr [ESP + 0x58]
    ebp=R(unchecked(esp+88u));goto Lb5633;
   Lb5633: // ADD EAX,dword ptr [EBP]
    a=eax;b=R(unchecked(ebp));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5636;
   Lb5636: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto Lb5638;
   Lb5638: // MOV EDX,dword ptr [EDI + ESI*0x8 + 0x10]
    edx=R(unchecked(edi+esi*0x8u+16u));goto Lb563c;
   Lb563c: // MOV EBP,dword ptr [ESP + 0x60]
    ebp=R(unchecked(esp+96u));goto Lb5640;
   Lb5640: // ADD EAX,dword ptr [ESP + 0x58]
    a=eax;b=R(unchecked(esp+88u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5644;
   Lb5644: // MOV ECX,dword ptr [EDX]
    ecx=R(unchecked(edx));goto Lb5646;
   Lb5646: // MOV dword ptr [ESP + 0x10],EBP
    W(unchecked(esp+16u),ebp);goto Lb564a;
   Lb564a: // MOV EBP,dword ptr [ESP + 0x5c]
    ebp=R(unchecked(esp+92u));goto Lb564e;
   Lb564e: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto Lb5652;
   Lb5652: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto Lb5655;
   Lb5655: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb5659;
   Lb5659: // MOV EBP,dword ptr [ESP + 0x54]
    ebp=R(unchecked(esp+84u));goto Lb565d;
   Lb565d: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto Lb5661;
   Lb5661: // CALL dword ptr [ECX + 0x20]
    eax=Call(R(unchecked(ecx+32u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)),R(unchecked(esp+16u)));goto Lb5664;
   Lb5664: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5667;
   Lb5667: // JLE 0x000b5608
    if(zero||sign!=overflow)goto Lb5608;goto Lb5669;
   Lb5669: // MOVZX EDX,byte ptr [ESP + 0x2f]
    edx=B(unchecked(esp+47u));goto Lb566e;
   Lb566e: // MOV EBP,EAX
    ebp=eax;goto Lb5670;
   Lb5670: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5672;
   Lb5672: // JZ 0x000b5685
    if(zero)goto Lb5685;goto Lb5674;
   Lb5674: // OR EBP,0x1
    a=ebp;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto Lb5677;
   Lb5677: // JMP 0x000b5685
    goto Lb5685;
   Lb5680: // MOV EBP,0x1
    ebp=1u;goto Lb5685;
   Lb5685:return ebp;
   Lb5690: // MOV ECX,dword ptr [ESP + 0x5c]
    ecx=R(unchecked(esp+92u));goto Lb5694;
   Lb5694: // MOV EBP,0x6
    ebp=6u;goto Lb5699;
   Lb5699: // MOV EAX,dword ptr [EDI + 0x4]
    eax=R(unchecked(edi+4u));goto Lb569c;
   Lb569c: // MOV EDX,dword ptr [ECX + 0x4]
    edx=R(unchecked(ecx+4u));goto Lb569f;
   Lb569f: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb56a1;
   Lb56a1: // JZ 0x000b5685
    if(zero)goto Lb5685;goto Lb56a3;
   Lb56a3: // CMP byte ptr [EAX],0x2a
    a=B(unchecked(eax));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb56a6;
   Lb56a6: // JZ 0x000b5604
    if(zero)goto Lb5604;goto Lb56ac;
   Lb56ac: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb56b0;
   Lb56b0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb56b3;
   Lb56b3: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb56b8;
   Lb56b8: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb56ba;
   Lb56ba: // JNZ 0x000b5604
    if(!zero)goto Lb5604;goto Lb56c0;
   Lb56c0: // JMP 0x000b5685
    goto Lb5685;
  }
 }
}
