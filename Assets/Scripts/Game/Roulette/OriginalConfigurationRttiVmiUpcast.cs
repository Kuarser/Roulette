using System;
namespace RouletteDeluxe
{
 // Physical packaged vmi_class_type_info upcast 000b53b0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; four inputs are at +50..+5c. Class upcast, strcmp and recursive virtual upcast remain required. Return exposes the original low byte.
 public sealed class OriginalConfigurationRttiVmiUpcast
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
  public OriginalConfigurationRttiVmiUpcast(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Upcast(uint frame,uint owner,uint target,uint objectPtr,uint result)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x50u,owner);W(frame+0x54u,target);W(frame+0x58u,objectPtr);W(frame+0x5cu,result);goto Lb53c2;
   Lb53c2: // MOV EDX,dword ptr [ESP + 0x58]
    edx=R(unchecked(esp+88u));goto Lb53c6;
   Lb53c6: // MOV EDI,dword ptr [ESP + 0x50]
    edi=R(unchecked(esp+80u));goto Lb53ca;
   Lb53ca: // MOV EBP,dword ptr [ESP + 0x5c]
    ebp=R(unchecked(esp+92u));goto Lb53ce;
   Lb53ce: // MOV ECX,dword ptr [ESP + 0x54]
    ecx=R(unchecked(esp+84u));goto Lb53d2;
   Lb53d2: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto Lb53d6;
   Lb53d6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto Lb53d9;
   Lb53d9: // MOV dword ptr [ESP + 0xc],EBP
    W(unchecked(esp+12u),ebp);goto Lb53dd;
   Lb53dd: // MOV dword ptr [ESP + 0x4],ECX
    W(unchecked(esp+4u),ecx);goto Lb53e1;
   Lb53e1: // CALL 0x00048e00
    eax=Call(0x48e00,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto Lb53e6;
   Lb53e6: // MOV EDX,0x1
    edx=1u;goto Lb53eb;
   Lb53eb: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb53ed;
   Lb53ed: // JNZ 0x000b54e9
    if(!zero)goto Lb54e9;goto Lb53f3;
   Lb53f3: // MOV EDX,dword ptr [EBP + 0x8]
    edx=R(unchecked(ebp+8u));goto Lb53f6;
   Lb53f6: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto Lb53fa;
   Lb53fa: // AND EDX,0x10
    a=edx;b=16u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto Lb53fd;
   Lb53fd: // JNZ 0x000b55a8
    if(!zero)goto Lb55a8;goto Lb5403;
   Lb5403: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto Lb5406;
   Lb5406: // NOP
    goto Lb5408;
   Lb5408: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb540a;
   Lb540a: // JZ 0x000b54e1
    if(zero)goto Lb54e1;goto Lb5410;
   Lb5410: // SUB ESI,0x1
    a=esi;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto Lb5413;
   Lb5413: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto Lb5417;
   Lb5417: // MOV EAX,dword ptr [EDI + ESI*0x8 + 0x14]
    eax=R(unchecked(edi+esi*0x8u+20u));goto Lb541b;
   Lb541b: // MOV dword ptr [ESP + 0x20],0x0
    W(unchecked(esp+32u),0u);goto Lb5423;
   Lb5423: // MOV dword ptr [ESP + 0x24],0x0
    W(unchecked(esp+36u),0u);goto Lb542b;
   Lb542b: // MOV dword ptr [ESP + 0x28],EDX
    W(unchecked(esp+40u),edx);goto Lb542f;
   Lb542f: // TEST AL,0x2
    a=(eax&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5431;
   Lb5431: // MOV dword ptr [ESP + 0x2c],0x0
    W(unchecked(esp+44u),0u);goto Lb5439;
   Lb5439: // SETNZ byte ptr [ESP + 0x1f]
    h.WriteByte(unchecked(esp+31u),unchecked((byte)(zero?0u:1u)));goto Lb543e;
   Lb543e: // JNZ 0x000b5445
    if(!zero)goto Lb5445;goto Lb5440;
   Lb5440: // AND EDX,0x1
    a=edx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto Lb5443;
   Lb5443: // JZ 0x000b5408
    if(zero)goto Lb5408;goto Lb5445;
   Lb5445: // MOV ECX,EAX
    ecx=eax;goto Lb5447;
   Lb5447: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto Lb5449;
   Lb5449: // AND ECX,0x1
    a=ecx;b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto Lb544c;
   Lb544c: // CMP dword ptr [ESP + 0x58],0x0
    a=R(unchecked(esp+88u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5451;
   Lb5451: // MOV byte ptr [ESP + 0x1e],CL
    h.WriteByte(unchecked(esp+30u),unchecked((byte)((ecx&255u))));goto Lb5455;
   Lb5455: // JZ 0x000b546c
    if(zero)goto Lb546c;goto Lb5457;
   Lb5457: // SAR EAX,0x8
    a=eax;v=unchecked((uint)((int)a>>8));zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&128u)!=0;overflow=false;eax=v;goto Lb545a;
   Lb545a: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb545c;
   Lb545c: // JZ 0x000b5466
    if(zero)goto Lb5466;goto Lb545e;
   Lb545e: // MOV EDX,dword ptr [ESP + 0x58]
    edx=R(unchecked(esp+88u));goto Lb5462;
   Lb5462: // ADD EAX,dword ptr [EDX]
    a=eax;b=R(unchecked(edx));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto Lb5464;
   Lb5464: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto Lb5466;
   Lb5466: // MOV EDX,dword ptr [ESP + 0x58]
    edx=R(unchecked(esp+88u));goto Lb546a;
   Lb546a: // ADD EDX,EAX
    a=edx;b=eax;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto Lb546c;
   Lb546c: // MOV EAX,dword ptr [EDI + ESI*0x8 + 0x10]
    eax=R(unchecked(edi+esi*0x8u+16u));goto Lb5470;
   Lb5470: // MOV ECX,dword ptr [EAX]
    ecx=R(unchecked(eax));goto Lb5472;
   Lb5472: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto Lb5476;
   Lb5476: // MOV EDX,dword ptr [ESP + 0x54]
    edx=R(unchecked(esp+84u));goto Lb547a;
   Lb547a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb547d;
   Lb547d: // MOV dword ptr [ESP + 0x14],ECX
    W(unchecked(esp+20u),ecx);goto Lb5481;
   Lb5481: // LEA ECX,[ESP + 0x20]
    ecx=unchecked(esp+32u);goto Lb5485;
   Lb5485: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto Lb5489;
   Lb5489: // MOV ECX,dword ptr [ESP + 0x14]
    ecx=R(unchecked(esp+20u));goto Lb548d;
   Lb548d: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb5491;
   Lb5491: // CALL dword ptr [ECX + 0x18]
    eax=Call(R(unchecked(ecx+24u)),R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto Lb5494;
   Lb5494: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5496;
   Lb5496: // JZ 0x000b5408
    if(zero)goto Lb5408;goto Lb549c;
   Lb549c: // CMP dword ptr [ESP + 0x2c],0x8
    a=R(unchecked(esp+44u));b=8u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb54a1;
   Lb54a1: // JZ 0x000b54f8
    if(zero)goto Lb54f8;goto Lb54a3;
   Lb54a3: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto Lb54a7;
   Lb54a7: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb54aa;
   Lb54aa: // JLE 0x000b54ba
    if(zero||sign!=overflow)goto Lb54ba;goto Lb54ac;
   Lb54ac: // CMP byte ptr [ESP + 0x1f],0x0
    a=B(unchecked(esp+31u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb54b1;
   Lb54b1: // JNZ 0x000b54ba
    if(!zero)goto Lb54ba;goto Lb54b3;
   Lb54b3: // AND EAX,0xfffffffd
    a=eax;b=4294967293u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto Lb54b6;
   Lb54b6: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto Lb54ba;
   Lb54ba: // MOV EDX,dword ptr [EBP + 0xc]
    edx=R(unchecked(ebp+12u));goto Lb54bd;
   Lb54bd: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb54bf;
   Lb54bf: // JZ 0x000b5510
    if(zero)goto Lb5510;goto Lb54c1;
   Lb54c1: // MOV EAX,dword ptr [EBP]
    eax=R(unchecked(ebp));goto Lb54c4;
   Lb54c4: // CMP EAX,dword ptr [ESP + 0x20]
    a=eax;b=R(unchecked(esp+32u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb54c8;
   Lb54c8: // JNZ 0x000b55b4
    if(!zero)goto Lb55b4;goto Lb54ce;
   Lb54ce: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb54d0;
   Lb54d0: // JZ 0x000b5548
    if(zero)goto Lb5548;goto Lb54d2;
   Lb54d2: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto Lb54d6;
   Lb54d6: // OR dword ptr [EBP + 0x4],EAX
    a=R(unchecked(ebp+4u));b=eax;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;W(unchecked(ebp+4u),v);goto Lb54d9;
   Lb54d9: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb54db;
   Lb54db: // JNZ 0x000b5410
    if(!zero)goto Lb5410;goto Lb54e1;
   Lb54e1: // MOV EAX,dword ptr [EBP + 0x4]
    eax=R(unchecked(ebp+4u));goto Lb54e4;
   Lb54e4: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb54e6;
   Lb54e6: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto Lb54e9;
   Lb54e9:return (edx&255u);
   Lb54f8: // CMP byte ptr [ESP + 0x1e],0x0
    a=B(unchecked(esp+30u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb54fd;
   Lb54fd: // JZ 0x000b54a3
    if(zero)goto Lb54a3;goto Lb54ff;
   Lb54ff: // MOV EAX,dword ptr [EDI + ESI*0x8 + 0x10]
    eax=R(unchecked(edi+esi*0x8u+16u));goto Lb5503;
   Lb5503: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto Lb5507;
   Lb5507: // JMP 0x000b54a3
    goto Lb54a3;
   Lb5510: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto Lb5514;
   Lb5514: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto Lb5518;
   Lb5518: // MOV dword ptr [EBP],EAX
    W(unchecked(ebp),eax);goto Lb551b;
   Lb551b: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto Lb551f;
   Lb551f: // MOV dword ptr [EBP + 0x8],EDX
    W(unchecked(ebp+8u),edx);goto Lb5522;
   Lb5522: // MOV EDX,dword ptr [ESP + 0x2c]
    edx=R(unchecked(esp+44u));goto Lb5526;
   Lb5526: // CMP EAX,0x3
    a=eax;b=3u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5529;
   Lb5529: // MOV dword ptr [EBP + 0x4],EAX
    W(unchecked(ebp+4u),eax);goto Lb552c;
   Lb552c: // MOV dword ptr [EBP + 0xc],EDX
    W(unchecked(ebp+12u),edx);goto Lb552f;
   Lb552f: // JLE 0x000b553f
    if(zero||sign!=overflow)goto Lb553f;goto Lb5531;
   Lb5531: // TEST AL,0x2
    a=(eax&255u);b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5533;
   Lb5533: // JZ 0x000b5598
    if(zero)goto Lb5598;goto Lb5535;
   Lb5535: // TEST byte ptr [EDI + 0x8],0x1
    a=B(unchecked(edi+8u));b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5539;
   Lb5539: // JNZ 0x000b5408
    if(!zero)goto Lb5408;goto Lb553f;
   Lb553f: // MOV EDX,0x1
    edx=1u;goto Lb5544;
   Lb5544: // JMP 0x000b54e9
    goto Lb54e9;
   Lb5548: // MOV EAX,dword ptr [ESP + 0x2c]
    eax=R(unchecked(esp+44u));goto Lb554c;
   Lb554c: // CMP EAX,0x8
    a=eax;b=8u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb554f;
   Lb554f: // JZ 0x000b557d
    if(zero)goto Lb557d;goto Lb5551;
   Lb5551: // CMP EDX,0x8
    a=edx;b=8u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5554;
   Lb5554: // JZ 0x000b557d
    if(zero)goto Lb557d;goto Lb5556;
   Lb5556: // MOV EAX,dword ptr [EAX + 0x4]
    eax=R(unchecked(eax+4u));goto Lb5559;
   Lb5559: // MOV EDX,dword ptr [EDX + 0x4]
    edx=R(unchecked(edx+4u));goto Lb555c;
   Lb555c: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb555e;
   Lb555e: // JZ 0x000b54d2
    if(zero)goto Lb54d2;goto Lb5564;
   Lb5564: // CMP byte ptr [EAX],0x2a
    a=B(unchecked(eax));b=42u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto Lb5567;
   Lb5567: // JZ 0x000b557d
    if(zero)goto Lb557d;goto Lb5569;
   Lb5569: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto Lb556d;
   Lb556d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto Lb5570;
   Lb5570: // CALL 0x0004a500
    eax=Call(0x4a500,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto Lb5575;
   Lb5575: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb5577;
   Lb5577: // JZ 0x000b54d2
    if(zero)goto Lb54d2;goto Lb557d;
   Lb557d: // MOV dword ptr [EBP + 0x4],0x2
    W(unchecked(ebp+4u),2u);goto Lb5584;
   Lb5584: // MOV EDX,0x1
    edx=1u;goto Lb5589;
   Lb5589:return (edx&255u);
   Lb5598: // TEST AL,0x1
    a=(eax&255u);b=1u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb559a;
   Lb559a: // JZ 0x000b553f
    if(zero)goto Lb553f;goto Lb559c;
   Lb559c: // TEST byte ptr [EDI + 0x8],0x2
    a=B(unchecked(edi+8u));b=2u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto Lb55a0;
   Lb55a0: // JNZ 0x000b5408
    if(!zero)goto Lb5408;goto Lb55a6;
   Lb55a6: // JMP 0x000b553f
    goto Lb553f;
   Lb55a8: // MOV ECX,dword ptr [EDI + 0x8]
    ecx=R(unchecked(edi+8u));goto Lb55ab;
   Lb55ab: // MOV dword ptr [ESP + 0x18],ECX
    W(unchecked(esp+24u),ecx);goto Lb55af;
   Lb55af: // JMP 0x000b5403
    goto Lb5403;
   Lb55b4: // MOV dword ptr [EBP],0x0
    W(unchecked(ebp),0u);goto Lb55bb;
   Lb55bb: // MOV EDX,0x1
    edx=1u;goto Lb55c0;
   Lb55c0: // MOV dword ptr [EBP + 0x4],0x2
    W(unchecked(ebp+4u),2u);goto Lb55c7;
   Lb55c7:return (edx&255u);
  }
 }
}
