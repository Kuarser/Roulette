using System;
namespace RouletteDeluxe
{
 // Physical packaged wide ctype and timepunct initialization. Static C# labels
 // preserve original initialization control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Locale conversion/query/clone, heap and native exception owners remain required.
 public sealed class OriginalConfigurationLocaleInitializers
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
  public OriginalConfigurationLocaleInitializers(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadHalf==null||host.WriteHalf==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadHalf=host.ReadHalf,WriteHalf=host.WriteHalf,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  uint H(uint at){return h.ReadHalf(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint InitializeCtypeWide(uint frame,uint owner,uint locale)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edi=owner;W(frame+0x40u,owner);goto L64098;
   L64098: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L6409b;
   L6409b: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6409e;
   L6409e: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L640a3;
   L640a3: // MOV EBP,EAX
    ebp=eax;goto L640a5;
   L640a5: // JMP 0x000640bb
    goto L640bb;
   L640a8: // MOV byte ptr [EDI + ESI*0x1 + 0xd],AL
    h.WriteByte(unchecked(edi+esi*0x1u+13u),unchecked((byte)((eax&255u))));goto L640ac;
   L640ac: // ADD ESI,0x1
    a=esi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L640af;
   L640af: // CMP ESI,0x80
    a=esi;b=128u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L640b5;
   L640b5: // JZ 0x00064141
    if(zero)goto L64141;goto L640bb;
   L640bb: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L640be;
   L640be: // CALL 0x00049040
    try{eax=Call(0x49040,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64147;}goto L640c3;
   L640c3: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L640c6;
   L640c6: // JNZ 0x000640a8
    if(!zero)goto L640a8;goto L640c8;
   L640c8: // MOV byte ptr [EDI + 0xc],0x0
    h.WriteByte(unchecked(edi+12u),unchecked((byte)(0u)));goto L640cc;
   L640cc: // XOR ESI,ESI
    a=esi;b=esi;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;esi=v;goto L640ce;
   L640ce: // NOP
    goto L640d0;
   L640d0: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L640d3;
   L640d3: // CALL 0x0004a510
    try{eax=Call(0x4a510,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L64154;}goto L640d8;
   L640d8: // MOV dword ptr [EDI + ESI*0x4 + 0x90],EAX
    W(unchecked(edi+esi*0x4u+144u),eax);goto L640df;
   L640df: // ADD ESI,0x1
    a=esi;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L640e2;
   L640e2: // CMP ESI,0x100
    a=esi;b=256u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L640e8;
   L640e8: // JNZ 0x000640d0
    if(!zero)goto L640d0;goto L640ea;
   L640ea: // XOR ECX,ECX
    a=ecx;b=ecx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L640ec;
   L640ec: // MOV SI,0x1
    esi=(esi&0xffff0000u)|((1u)&65535u);goto L640f0;
   L640f0: // MOV EAX,ESI
    eax=esi;goto L640f2;
   L640f2: // SHL EAX,CL
    a=eax;b=((ecx&255u))&31u;if(b!=0){v=unchecked(a<<(int)b);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&(1u<<(int)(32u-b)))!=0;overflow=b==1u&&((v&0x80000000u)!=0)!=carry;eax=v;}goto L640f4;
   L640f4: // SHR EAX,0x8
    a=eax;b=(8u)&31u;if(b!=0){v=unchecked(a>>(int)b);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&(1u<<(int)(b-1u)))!=0;overflow=b==1u&&(a&0x80000000u)!=0;eax=v;}goto L640f7;
   L640f7: // CMP ECX,0x7
    a=ecx;b=7u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L640fa;
   L640fa: // JA 0x00064103
    if(!carry&&!zero)goto L64103;goto L640fc;
   L640fc: // MOV EAX,ESI
    eax=esi;goto L640fe;
   L640fe: // SHL EAX,CL
    a=eax;b=((ecx&255u))&31u;if(b!=0){v=unchecked(a<<(int)b);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&(1u<<(int)(32u-b)))!=0;overflow=b==1u&&((v&0x80000000u)!=0)!=carry;eax=v;}goto L64100;
   L64100: // SHL EAX,0x8
    a=eax;b=(8u)&31u;if(b!=0){v=unchecked(a<<(int)b);zero=v==0;sign=(v&0x80000000u)!=0;carry=(a&(1u<<(int)(32u-b)))!=0;overflow=b==1u&&((v&0x80000000u)!=0)!=carry;eax=v;}goto L64103;
   L64103: // MOV word ptr [EDI + ECX*0x2 + 0x490],AX
    h.WriteHalf(unchecked(edi+ecx*0x2u+1168u),unchecked((ushort)((eax&65535u))));goto L6410b;
   L6410b: // MOVZX EAX,AX
    eax=(eax&65535u);goto L6410e;
   L6410e: // MOV dword ptr [ESP + 0x1c],ECX
    W(unchecked(esp+28u),ecx);goto L64112;
   L64112: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L64116;
   L64116: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L64119;
   L64119: // CALL 0x00048fd0
    eax=Call(0x48fd0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6411e;
   L6411e: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L64122;
   L64122: // MOV dword ptr [EDI + ECX*0x4 + 0x4b0],EAX
    W(unchecked(edi+ecx*0x4u+1200u),eax);goto L64129;
   L64129: // ADD ECX,0x1
    a=ecx;b=1u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ecx=v;goto L6412c;
   L6412c: // CMP ECX,0xc
    a=ecx;b=12u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6412f;
   L6412f: // JNZ 0x000640f0
    if(!zero)goto L640f0;goto L64131;
   L64131: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L64134;
   L64134: // CALL 0x00049aa0
    eax=Call(0x49aa0,R(unchecked(esp+0u)));goto L64139;
   L64139:return 0;
   L64141: // MOV byte ptr [EDI + 0xc],0x1
    h.WriteByte(unchecked(edi+12u),unchecked((byte)(1u)));goto L64145;
   L64145: // JMP 0x000640cc
    goto L640cc;
   L64147: // CMP EDX,-0x1
    a=edx;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6414a;
   L6414a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L6414d;
   L6414d: // JZ 0x00064156
    if(zero)goto L64156;goto L6414f;
   L6414f: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L64154: // JMP 0x00064147
    goto L64147;
   L64156: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint InitializeTimepunctChar(uint frame,uint owner,uint locale)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x20u,owner);W(frame+0x24u,locale);goto L666c5;
   L666c5: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L666c8;
   L666c8: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L666ca;
   L666ca: // JZ 0x00066dc0
    if(zero)goto L66dc0;goto L666d0;
   L666d0: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L666d4;
   L666d4: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L666d6;
   L666d6: // JZ 0x00066bf0
    if(zero)goto L66bf0;goto L666dc;
   L666dc: // LEA EAX,[ESP + 0x24]
    eax=unchecked(esp+36u);goto L666e0;
   L666e0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L666e3;
   L666e3: // CALL 0x00047fb0
    eax=Call(0x47fb0,R(unchecked(esp+0u)));goto L666e8;
   L666e8: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L666eb;
   L666eb: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L666ee;
   L666ee: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L666f2;
   L666f2: // MOV dword ptr [ESP],0x20029
    W(unchecked(esp),131113u);goto L666f9;
   L666f9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L666fd;
   L666fd: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66702;
   L66702: // MOV dword ptr [EDI + 0x8],EAX
    W(unchecked(edi+8u),eax);goto L66705;
   L66705: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66709;
   L66709: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6670c;
   L6670c: // MOV dword ptr [ESP],0x2002e
    W(unchecked(esp),131118u);goto L66713;
   L66713: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66717;
   L66717: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6671c;
   L6671c: // MOV dword ptr [EDI + 0xc],EAX
    W(unchecked(edi+12u),eax);goto L6671f;
   L6671f: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66723;
   L66723: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66726;
   L66726: // MOV dword ptr [ESP],0x2002a
    W(unchecked(esp),131114u);goto L6672d;
   L6672d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66731;
   L66731: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66736;
   L66736: // MOV dword ptr [EDI + 0x10],EAX
    W(unchecked(edi+16u),eax);goto L66739;
   L66739: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6673d;
   L6673d: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66740;
   L66740: // MOV dword ptr [ESP],0x20031
    W(unchecked(esp),131121u);goto L66747;
   L66747: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6674b;
   L6674b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66750;
   L66750: // MOV dword ptr [EDI + 0x14],EAX
    W(unchecked(edi+20u),eax);goto L66753;
   L66753: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66757;
   L66757: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6675a;
   L6675a: // MOV dword ptr [ESP],0x20028
    W(unchecked(esp),131112u);goto L66761;
   L66761: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66765;
   L66765: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6676a;
   L6676a: // MOV dword ptr [EDI + 0x18],EAX
    W(unchecked(edi+24u),eax);goto L6676d;
   L6676d: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66771;
   L66771: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66774;
   L66774: // MOV dword ptr [ESP],0x20030
    W(unchecked(esp),131120u);goto L6677b;
   L6677b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6677f;
   L6677f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66784;
   L66784: // MOV dword ptr [EDI + 0x1c],EAX
    W(unchecked(edi+28u),eax);goto L66787;
   L66787: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6678b;
   L6678b: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6678e;
   L6678e: // MOV dword ptr [ESP],0x20026
    W(unchecked(esp),131110u);goto L66795;
   L66795: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66799;
   L66799: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6679e;
   L6679e: // MOV dword ptr [EDI + 0x20],EAX
    W(unchecked(edi+32u),eax);goto L667a1;
   L667a1: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L667a5;
   L667a5: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L667a8;
   L667a8: // MOV dword ptr [ESP],0x20027
    W(unchecked(esp),131111u);goto L667af;
   L667af: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L667b3;
   L667b3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L667b8;
   L667b8: // MOV dword ptr [EDI + 0x24],EAX
    W(unchecked(edi+36u),eax);goto L667bb;
   L667bb: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L667bf;
   L667bf: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L667c2;
   L667c2: // MOV dword ptr [ESP],0x2002b
    W(unchecked(esp),131115u);goto L667c9;
   L667c9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L667cd;
   L667cd: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L667d2;
   L667d2: // MOV dword ptr [EDI + 0x28],EAX
    W(unchecked(edi+40u),eax);goto L667d5;
   L667d5: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L667d9;
   L667d9: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L667dc;
   L667dc: // MOV dword ptr [ESP],0x20007
    W(unchecked(esp),131079u);goto L667e3;
   L667e3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L667e7;
   L667e7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L667ec;
   L667ec: // MOV dword ptr [EDI + 0x2c],EAX
    W(unchecked(edi+44u),eax);goto L667ef;
   L667ef: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L667f3;
   L667f3: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L667f6;
   L667f6: // MOV dword ptr [ESP],0x20008
    W(unchecked(esp),131080u);goto L667fd;
   L667fd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66801;
   L66801: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66806;
   L66806: // MOV dword ptr [EDI + 0x30],EAX
    W(unchecked(edi+48u),eax);goto L66809;
   L66809: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6680d;
   L6680d: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66810;
   L66810: // MOV dword ptr [ESP],0x20009
    W(unchecked(esp),131081u);goto L66817;
   L66817: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6681b;
   L6681b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66820;
   L66820: // MOV dword ptr [EDI + 0x34],EAX
    W(unchecked(edi+52u),eax);goto L66823;
   L66823: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66827;
   L66827: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6682a;
   L6682a: // MOV dword ptr [ESP],0x2000a
    W(unchecked(esp),131082u);goto L66831;
   L66831: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66835;
   L66835: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6683a;
   L6683a: // MOV dword ptr [EDI + 0x38],EAX
    W(unchecked(edi+56u),eax);goto L6683d;
   L6683d: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66841;
   L66841: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66844;
   L66844: // MOV dword ptr [ESP],0x2000b
    W(unchecked(esp),131083u);goto L6684b;
   L6684b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6684f;
   L6684f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66854;
   L66854: // MOV dword ptr [EDI + 0x3c],EAX
    W(unchecked(edi+60u),eax);goto L66857;
   L66857: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6685b;
   L6685b: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6685e;
   L6685e: // MOV dword ptr [ESP],0x2000c
    W(unchecked(esp),131084u);goto L66865;
   L66865: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66869;
   L66869: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6686e;
   L6686e: // MOV dword ptr [EDI + 0x40],EAX
    W(unchecked(edi+64u),eax);goto L66871;
   L66871: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66875;
   L66875: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66878;
   L66878: // MOV dword ptr [ESP],0x2000d
    W(unchecked(esp),131085u);goto L6687f;
   L6687f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66883;
   L66883: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66888;
   L66888: // MOV dword ptr [EDI + 0x44],EAX
    W(unchecked(edi+68u),eax);goto L6688b;
   L6688b: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6688f;
   L6688f: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66892;
   L66892: // MOV dword ptr [ESP],0x20000
    W(unchecked(esp),131072u);goto L66899;
   L66899: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6689d;
   L6689d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L668a2;
   L668a2: // MOV dword ptr [EDI + 0x48],EAX
    W(unchecked(edi+72u),eax);goto L668a5;
   L668a5: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L668a9;
   L668a9: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L668ac;
   L668ac: // MOV dword ptr [ESP],0x20001
    W(unchecked(esp),131073u);goto L668b3;
   L668b3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L668b7;
   L668b7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L668bc;
   L668bc: // MOV dword ptr [EDI + 0x4c],EAX
    W(unchecked(edi+76u),eax);goto L668bf;
   L668bf: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L668c3;
   L668c3: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L668c6;
   L668c6: // MOV dword ptr [ESP],0x20002
    W(unchecked(esp),131074u);goto L668cd;
   L668cd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L668d1;
   L668d1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L668d6;
   L668d6: // MOV dword ptr [EDI + 0x50],EAX
    W(unchecked(edi+80u),eax);goto L668d9;
   L668d9: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L668dd;
   L668dd: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L668e0;
   L668e0: // MOV dword ptr [ESP],0x20003
    W(unchecked(esp),131075u);goto L668e7;
   L668e7: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L668eb;
   L668eb: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L668f0;
   L668f0: // MOV dword ptr [EDI + 0x54],EAX
    W(unchecked(edi+84u),eax);goto L668f3;
   L668f3: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L668f7;
   L668f7: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L668fa;
   L668fa: // MOV dword ptr [ESP],0x20004
    W(unchecked(esp),131076u);goto L66901;
   L66901: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66905;
   L66905: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6690a;
   L6690a: // MOV dword ptr [EDI + 0x58],EAX
    W(unchecked(edi+88u),eax);goto L6690d;
   L6690d: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66911;
   L66911: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66914;
   L66914: // MOV dword ptr [ESP],0x20005
    W(unchecked(esp),131077u);goto L6691b;
   L6691b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6691f;
   L6691f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66924;
   L66924: // MOV dword ptr [EDI + 0x5c],EAX
    W(unchecked(edi+92u),eax);goto L66927;
   L66927: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6692b;
   L6692b: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6692e;
   L6692e: // MOV dword ptr [ESP],0x20006
    W(unchecked(esp),131078u);goto L66935;
   L66935: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66939;
   L66939: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6693e;
   L6693e: // MOV dword ptr [EDI + 0x60],EAX
    W(unchecked(edi+96u),eax);goto L66941;
   L66941: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66945;
   L66945: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66948;
   L66948: // MOV dword ptr [ESP],0x2001a
    W(unchecked(esp),131098u);goto L6694f;
   L6694f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66953;
   L66953: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66958;
   L66958: // MOV dword ptr [EDI + 0x64],EAX
    W(unchecked(edi+100u),eax);goto L6695b;
   L6695b: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6695f;
   L6695f: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66962;
   L66962: // MOV dword ptr [ESP],0x2001b
    W(unchecked(esp),131099u);goto L66969;
   L66969: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6696d;
   L6696d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66972;
   L66972: // MOV dword ptr [EDI + 0x68],EAX
    W(unchecked(edi+104u),eax);goto L66975;
   L66975: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66979;
   L66979: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L6697c;
   L6697c: // MOV dword ptr [ESP],0x2001c
    W(unchecked(esp),131100u);goto L66983;
   L66983: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66987;
   L66987: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6698c;
   L6698c: // MOV dword ptr [EDI + 0x6c],EAX
    W(unchecked(edi+108u),eax);goto L6698f;
   L6698f: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66993;
   L66993: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66996;
   L66996: // MOV dword ptr [ESP],0x2001d
    W(unchecked(esp),131101u);goto L6699d;
   L6699d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L669a1;
   L669a1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L669a6;
   L669a6: // MOV dword ptr [EDI + 0x70],EAX
    W(unchecked(edi+112u),eax);goto L669a9;
   L669a9: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L669ad;
   L669ad: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L669b0;
   L669b0: // MOV dword ptr [ESP],0x2001e
    W(unchecked(esp),131102u);goto L669b7;
   L669b7: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L669bb;
   L669bb: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L669c0;
   L669c0: // MOV dword ptr [EDI + 0x74],EAX
    W(unchecked(edi+116u),eax);goto L669c3;
   L669c3: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L669c7;
   L669c7: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L669ca;
   L669ca: // MOV dword ptr [ESP],0x2001f
    W(unchecked(esp),131103u);goto L669d1;
   L669d1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L669d5;
   L669d5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L669da;
   L669da: // MOV dword ptr [EDI + 0x78],EAX
    W(unchecked(edi+120u),eax);goto L669dd;
   L669dd: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L669e1;
   L669e1: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L669e4;
   L669e4: // MOV dword ptr [ESP],0x20020
    W(unchecked(esp),131104u);goto L669eb;
   L669eb: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L669ef;
   L669ef: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L669f4;
   L669f4: // MOV dword ptr [EDI + 0x7c],EAX
    W(unchecked(edi+124u),eax);goto L669f7;
   L669f7: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L669fb;
   L669fb: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L669fe;
   L669fe: // MOV dword ptr [ESP],0x20021
    W(unchecked(esp),131105u);goto L66a05;
   L66a05: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a09;
   L66a09: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a0e;
   L66a0e: // MOV dword ptr [EDI + 0x80],EAX
    W(unchecked(edi+128u),eax);goto L66a14;
   L66a14: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66a18;
   L66a18: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66a1b;
   L66a1b: // MOV dword ptr [ESP],0x20022
    W(unchecked(esp),131106u);goto L66a22;
   L66a22: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a26;
   L66a26: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a2b;
   L66a2b: // MOV dword ptr [EDI + 0x84],EAX
    W(unchecked(edi+132u),eax);goto L66a31;
   L66a31: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66a35;
   L66a35: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66a38;
   L66a38: // MOV dword ptr [ESP],0x20023
    W(unchecked(esp),131107u);goto L66a3f;
   L66a3f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a43;
   L66a43: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a48;
   L66a48: // MOV dword ptr [EDI + 0x88],EAX
    W(unchecked(edi+136u),eax);goto L66a4e;
   L66a4e: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66a52;
   L66a52: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66a55;
   L66a55: // MOV dword ptr [ESP],0x20024
    W(unchecked(esp),131108u);goto L66a5c;
   L66a5c: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a60;
   L66a60: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a65;
   L66a65: // MOV dword ptr [EDI + 0x8c],EAX
    W(unchecked(edi+140u),eax);goto L66a6b;
   L66a6b: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66a6f;
   L66a6f: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66a72;
   L66a72: // MOV dword ptr [ESP],0x20025
    W(unchecked(esp),131109u);goto L66a79;
   L66a79: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a7d;
   L66a7d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a82;
   L66a82: // MOV dword ptr [EDI + 0x90],EAX
    W(unchecked(edi+144u),eax);goto L66a88;
   L66a88: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66a8c;
   L66a8c: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66a8f;
   L66a8f: // MOV dword ptr [ESP],0x2000e
    W(unchecked(esp),131086u);goto L66a96;
   L66a96: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66a9a;
   L66a9a: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66a9f;
   L66a9f: // MOV dword ptr [EDI + 0x94],EAX
    W(unchecked(edi+148u),eax);goto L66aa5;
   L66aa5: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66aa9;
   L66aa9: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66aac;
   L66aac: // MOV dword ptr [ESP],0x2000f
    W(unchecked(esp),131087u);goto L66ab3;
   L66ab3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66ab7;
   L66ab7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66abc;
   L66abc: // MOV dword ptr [EDI + 0x98],EAX
    W(unchecked(edi+152u),eax);goto L66ac2;
   L66ac2: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66ac6;
   L66ac6: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66ac9;
   L66ac9: // MOV dword ptr [ESP],0x20010
    W(unchecked(esp),131088u);goto L66ad0;
   L66ad0: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66ad4;
   L66ad4: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66ad9;
   L66ad9: // MOV dword ptr [EDI + 0x9c],EAX
    W(unchecked(edi+156u),eax);goto L66adf;
   L66adf: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66ae3;
   L66ae3: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66ae6;
   L66ae6: // MOV dword ptr [ESP],0x20011
    W(unchecked(esp),131089u);goto L66aed;
   L66aed: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66af1;
   L66af1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66af6;
   L66af6: // MOV dword ptr [EDI + 0xa0],EAX
    W(unchecked(edi+160u),eax);goto L66afc;
   L66afc: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b00;
   L66b00: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b03;
   L66b03: // MOV dword ptr [ESP],0x20012
    W(unchecked(esp),131090u);goto L66b0a;
   L66b0a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b0e;
   L66b0e: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66b13;
   L66b13: // MOV dword ptr [EDI + 0xa4],EAX
    W(unchecked(edi+164u),eax);goto L66b19;
   L66b19: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b1d;
   L66b1d: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b20;
   L66b20: // MOV dword ptr [ESP],0x20013
    W(unchecked(esp),131091u);goto L66b27;
   L66b27: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b2b;
   L66b2b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66b30;
   L66b30: // MOV dword ptr [EDI + 0xa8],EAX
    W(unchecked(edi+168u),eax);goto L66b36;
   L66b36: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b3a;
   L66b3a: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b3d;
   L66b3d: // MOV dword ptr [ESP],0x20014
    W(unchecked(esp),131092u);goto L66b44;
   L66b44: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b48;
   L66b48: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66b4d;
   L66b4d: // MOV dword ptr [EDI + 0xac],EAX
    W(unchecked(edi+172u),eax);goto L66b53;
   L66b53: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b57;
   L66b57: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b5a;
   L66b5a: // MOV dword ptr [ESP],0x20015
    W(unchecked(esp),131093u);goto L66b61;
   L66b61: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b65;
   L66b65: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66b6a;
   L66b6a: // MOV dword ptr [EDI + 0xb0],EAX
    W(unchecked(edi+176u),eax);goto L66b70;
   L66b70: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b74;
   L66b74: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b77;
   L66b77: // MOV dword ptr [ESP],0x20016
    W(unchecked(esp),131094u);goto L66b7e;
   L66b7e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b82;
   L66b82: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66b87;
   L66b87: // MOV dword ptr [EDI + 0xb4],EAX
    W(unchecked(edi+180u),eax);goto L66b8d;
   L66b8d: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66b91;
   L66b91: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66b94;
   L66b94: // MOV dword ptr [ESP],0x20017
    W(unchecked(esp),131095u);goto L66b9b;
   L66b9b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66b9f;
   L66b9f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66ba4;
   L66ba4: // MOV dword ptr [EDI + 0xb8],EAX
    W(unchecked(edi+184u),eax);goto L66baa;
   L66baa: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66bae;
   L66bae: // MOV EDI,dword ptr [ESI + 0x8]
    edi=R(unchecked(esi+8u));goto L66bb1;
   L66bb1: // MOV dword ptr [ESP],0x20018
    W(unchecked(esp),131096u);goto L66bb8;
   L66bb8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66bbc;
   L66bbc: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66bc1;
   L66bc1: // MOV ESI,dword ptr [ESI + 0x8]
    esi=R(unchecked(esi+8u));goto L66bc4;
   L66bc4: // MOV dword ptr [EDI + 0xbc],EAX
    W(unchecked(edi+188u),eax);goto L66bca;
   L66bca: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L66bce;
   L66bce: // MOV dword ptr [ESP],0x20019
    W(unchecked(esp),131097u);goto L66bd5;
   L66bd5: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L66bd9;
   L66bd9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L66bde;
   L66bde: // MOV dword ptr [ESI + 0xc0],EAX
    W(unchecked(esi+192u),eax);goto L66be4;
   L66be4:return 0;
   L66bf0: // CALL 0x00049d10
    eax=Call(0x49d10);goto L66bf5;
   L66bf5: // LEA EDX,[EBX + 0xfffd8ea0]
    edx=unchecked(ebx+4294807200u);goto L66bfb;
   L66bfb: // LEA ECX,[EBX + 0xfffd8eb2]
    ecx=unchecked(ebx+4294807218u);goto L66c01;
   L66c01: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L66c04;
   L66c04: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66c07;
   L66c07: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L66c0a;
   L66c0a: // MOV dword ptr [EAX + 0xc],EDX
    W(unchecked(eax+12u),edx);goto L66c0d;
   L66c0d: // LEA EDX,[EBX + 0xfffd8ea9]
    edx=unchecked(ebx+4294807209u);goto L66c13;
   L66c13: // MOV dword ptr [EAX + 0x10],EDX
    W(unchecked(eax+16u),edx);goto L66c16;
   L66c16: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L66c19;
   L66c19: // LEA EDX,[EBX + 0xfffdb072]
    edx=unchecked(ebx+4294815858u);goto L66c1f;
   L66c1f: // MOV dword ptr [EAX + 0x18],EDX
    W(unchecked(eax+24u),edx);goto L66c22;
   L66c22: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L66c25;
   L66c25: // MOV dword ptr [EAX + 0x28],EDX
    W(unchecked(eax+40u),edx);goto L66c28;
   L66c28: // LEA EDX,[EBX + 0xfffd8eb8]
    edx=unchecked(ebx+4294807224u);goto L66c2e;
   L66c2e: // MOV dword ptr [EAX + 0x2c],EDX
    W(unchecked(eax+44u),edx);goto L66c31;
   L66c31: // LEA EDX,[EBX + 0xfffd8ebf]
    edx=unchecked(ebx+4294807231u);goto L66c37;
   L66c37: // MOV dword ptr [EAX + 0x30],EDX
    W(unchecked(eax+48u),edx);goto L66c3a;
   L66c3a: // LEA EDX,[EBX + 0xfffd8ec6]
    edx=unchecked(ebx+4294807238u);goto L66c40;
   L66c40: // MOV dword ptr [EAX + 0x34],EDX
    W(unchecked(eax+52u),edx);goto L66c43;
   L66c43: // LEA EDX,[EBX + 0xfffd8ece]
    edx=unchecked(ebx+4294807246u);goto L66c49;
   L66c49: // MOV dword ptr [EAX + 0x38],EDX
    W(unchecked(eax+56u),edx);goto L66c4c;
   L66c4c: // LEA EDX,[EBX + 0xfffd8ed8]
    edx=unchecked(ebx+4294807256u);goto L66c52;
   L66c52: // MOV dword ptr [EAX + 0x3c],EDX
    W(unchecked(eax+60u),edx);goto L66c55;
   L66c55: // LEA EDX,[EBX + 0xfffd8ee1]
    edx=unchecked(ebx+4294807265u);goto L66c5b;
   L66c5b: // MOV dword ptr [EAX + 0x40],EDX
    W(unchecked(eax+64u),edx);goto L66c5e;
   L66c5e: // LEA EDX,[EBX + 0xfffd8ee8]
    edx=unchecked(ebx+4294807272u);goto L66c64;
   L66c64: // MOV dword ptr [EAX + 0x44],EDX
    W(unchecked(eax+68u),edx);goto L66c67;
   L66c67: // LEA EDX,[EBX + 0xfffd8ef1]
    edx=unchecked(ebx+4294807281u);goto L66c6d;
   L66c6d: // MOV dword ptr [EAX + 0x48],EDX
    W(unchecked(eax+72u),edx);goto L66c70;
   L66c70: // LEA EDX,[EBX + 0xfffd8ef5]
    edx=unchecked(ebx+4294807285u);goto L66c76;
   L66c76: // MOV dword ptr [EAX + 0x4c],EDX
    W(unchecked(eax+76u),edx);goto L66c79;
   L66c79: // LEA EDX,[EBX + 0xfffd8ef9]
    edx=unchecked(ebx+4294807289u);goto L66c7f;
   L66c7f: // MOV dword ptr [EAX + 0x50],EDX
    W(unchecked(eax+80u),edx);goto L66c82;
   L66c82: // LEA EDX,[EBX + 0xfffd8efd]
    edx=unchecked(ebx+4294807293u);goto L66c88;
   L66c88: // MOV dword ptr [EAX + 0x54],EDX
    W(unchecked(eax+84u),edx);goto L66c8b;
   L66c8b: // LEA EDX,[EBX + 0xfffd8f01]
    edx=unchecked(ebx+4294807297u);goto L66c91;
   L66c91: // MOV dword ptr [EAX + 0x58],EDX
    W(unchecked(eax+88u),edx);goto L66c94;
   L66c94: // LEA EDX,[EBX + 0xfffd8f05]
    edx=unchecked(ebx+4294807301u);goto L66c9a;
   L66c9a: // MOV dword ptr [EAX + 0x5c],EDX
    W(unchecked(eax+92u),edx);goto L66c9d;
   L66c9d: // LEA EDX,[EBX + 0xfffd8f09]
    edx=unchecked(ebx+4294807305u);goto L66ca3;
   L66ca3: // MOV dword ptr [EAX + 0x20],ECX
    W(unchecked(eax+32u),ecx);goto L66ca6;
   L66ca6: // LEA ECX,[EBX + 0xfffd8eb5]
    ecx=unchecked(ebx+4294807221u);goto L66cac;
   L66cac: // MOV dword ptr [EAX + 0x60],EDX
    W(unchecked(eax+96u),edx);goto L66caf;
   L66caf: // LEA EDX,[EBX + 0xfffd8f0d]
    edx=unchecked(ebx+4294807309u);goto L66cb5;
   L66cb5: // MOV dword ptr [EAX + 0x24],ECX
    W(unchecked(eax+36u),ecx);goto L66cb8;
   L66cb8: // LEA ECX,[EBX + 0xfffd8f2e]
    ecx=unchecked(ebx+4294807342u);goto L66cbe;
   L66cbe: // MOV dword ptr [EAX + 0x64],EDX
    W(unchecked(eax+100u),edx);goto L66cc1;
   L66cc1: // LEA EDX,[EBX + 0xfffd8f15]
    edx=unchecked(ebx+4294807317u);goto L66cc7;
   L66cc7: // MOV dword ptr [EAX + 0x68],EDX
    W(unchecked(eax+104u),edx);goto L66cca;
   L66cca: // LEA EDX,[EBX + 0xfffd8f1e]
    edx=unchecked(ebx+4294807326u);goto L66cd0;
   L66cd0: // MOV dword ptr [EAX + 0x78],ECX
    W(unchecked(eax+120u),ecx);goto L66cd3;
   L66cd3: // LEA ECX,[EBX + 0xfffd8f33]
    ecx=unchecked(ebx+4294807347u);goto L66cd9;
   L66cd9: // MOV dword ptr [EAX + 0x6c],EDX
    W(unchecked(eax+108u),edx);goto L66cdc;
   L66cdc: // LEA EDX,[EBX + 0xfffd8f24]
    edx=unchecked(ebx+4294807332u);goto L66ce2;
   L66ce2: // MOV dword ptr [EAX + 0x7c],ECX
    W(unchecked(eax+124u),ecx);goto L66ce5;
   L66ce5: // LEA ECX,[EBX + 0xfffd8f38]
    ecx=unchecked(ebx+4294807352u);goto L66ceb;
   L66ceb: // MOV dword ptr [EAX + 0x70],EDX
    W(unchecked(eax+112u),edx);goto L66cee;
   L66cee: // LEA EDX,[EBX + 0xfffd8f2a]
    edx=unchecked(ebx+4294807338u);goto L66cf4;
   L66cf4: // MOV dword ptr [EAX + 0x80],ECX
    W(unchecked(eax+128u),ecx);goto L66cfa;
   L66cfa: // LEA ECX,[EBX + 0xfffd8f3f]
    ecx=unchecked(ebx+4294807359u);goto L66d00;
   L66d00: // MOV dword ptr [EAX + 0x74],EDX
    W(unchecked(eax+116u),edx);goto L66d03;
   L66d03: // MOV dword ptr [EAX + 0x84],ECX
    W(unchecked(eax+132u),ecx);goto L66d09;
   L66d09: // LEA ECX,[EBX + 0xfffd8f49]
    ecx=unchecked(ebx+4294807369u);goto L66d0f;
   L66d0f: // MOV dword ptr [EAX + 0xa4],EDX
    W(unchecked(eax+164u),edx);goto L66d15;
   L66d15: // LEA EDX,[EBX + 0xfffd8f73]
    edx=unchecked(ebx+4294807411u);goto L66d1b;
   L66d1b: // MOV dword ptr [EAX + 0x88],ECX
    W(unchecked(eax+136u),ecx);goto L66d21;
   L66d21: // LEA ECX,[EBX + 0xfffd8f51]
    ecx=unchecked(ebx+4294807377u);goto L66d27;
   L66d27: // MOV dword ptr [EAX + 0xa8],EDX
    W(unchecked(eax+168u),edx);goto L66d2d;
   L66d2d: // LEA EDX,[EBX + 0xfffd8f77]
    edx=unchecked(ebx+4294807415u);goto L66d33;
   L66d33: // MOV dword ptr [EAX + 0x8c],ECX
    W(unchecked(eax+140u),ecx);goto L66d39;
   L66d39: // LEA ECX,[EBX + 0xfffd8f5a]
    ecx=unchecked(ebx+4294807386u);goto L66d3f;
   L66d3f: // MOV dword ptr [EAX + 0xac],EDX
    W(unchecked(eax+172u),edx);goto L66d45;
   L66d45: // LEA EDX,[EBX + 0xfffd8f7b]
    edx=unchecked(ebx+4294807419u);goto L66d4b;
   L66d4b: // MOV dword ptr [EAX + 0x90],ECX
    W(unchecked(eax+144u),ecx);goto L66d51;
   L66d51: // LEA ECX,[EBX + 0xfffd8f63]
    ecx=unchecked(ebx+4294807395u);goto L66d57;
   L66d57: // MOV dword ptr [EAX + 0xb0],EDX
    W(unchecked(eax+176u),edx);goto L66d5d;
   L66d5d: // LEA EDX,[EBX + 0xfffd8f7f]
    edx=unchecked(ebx+4294807423u);goto L66d63;
   L66d63: // MOV dword ptr [EAX + 0x94],ECX
    W(unchecked(eax+148u),ecx);goto L66d69;
   L66d69: // LEA ECX,[EBX + 0xfffd8f67]
    ecx=unchecked(ebx+4294807399u);goto L66d6f;
   L66d6f: // MOV dword ptr [EAX + 0xb4],EDX
    W(unchecked(eax+180u),edx);goto L66d75;
   L66d75: // LEA EDX,[EBX + 0xfffd8f83]
    edx=unchecked(ebx+4294807427u);goto L66d7b;
   L66d7b: // MOV dword ptr [EAX + 0x98],ECX
    W(unchecked(eax+152u),ecx);goto L66d81;
   L66d81: // LEA ECX,[EBX + 0xfffd8f6b]
    ecx=unchecked(ebx+4294807403u);goto L66d87;
   L66d87: // MOV dword ptr [EAX + 0xb8],EDX
    W(unchecked(eax+184u),edx);goto L66d8d;
   L66d8d: // LEA EDX,[EBX + 0xfffd8f87]
    edx=unchecked(ebx+4294807431u);goto L66d93;
   L66d93: // MOV dword ptr [EAX + 0x9c],ECX
    W(unchecked(eax+156u),ecx);goto L66d99;
   L66d99: // LEA ECX,[EBX + 0xfffd8f6f]
    ecx=unchecked(ebx+4294807407u);goto L66d9f;
   L66d9f: // MOV dword ptr [EAX + 0xbc],EDX
    W(unchecked(eax+188u),edx);goto L66da5;
   L66da5: // LEA EDX,[EBX + 0xfffd8f8b]
    edx=unchecked(ebx+4294807435u);goto L66dab;
   L66dab: // MOV dword ptr [EAX + 0xa0],ECX
    W(unchecked(eax+160u),ecx);goto L66db1;
   L66db1: // MOV dword ptr [EAX + 0xc0],EDX
    W(unchecked(eax+192u),edx);goto L66db7;
   L66db7:return 0;
   L66dc0: // MOV dword ptr [ESP],0xc8
    W(unchecked(esp),200u);goto L66dc7;
   L66dc7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L66dcc;
   L66dcc: // MOV EDX,dword ptr [EBX + 0xfffffca0]
    edx=R(unchecked(ebx+4294966432u));goto L66dd2;
   L66dd2: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L66dd5;
   L66dd5: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L66ddc;
   L66ddc: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L66dde;
   L66dde: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L66de5;
   L66de5: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L66dec;
   L66dec: // MOV dword ptr [EAX + 0x10],0x0
    W(unchecked(eax+16u),0u);goto L66df3;
   L66df3: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L66dfa;
   L66dfa: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L66e01;
   L66e01: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L66e08;
   L66e08: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L66e0f;
   L66e0f: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L66e16;
   L66e16: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L66e1d;
   L66e1d: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L66e24;
   L66e24: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L66e2b;
   L66e2b: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L66e32;
   L66e32: // MOV dword ptr [EAX + 0x38],0x0
    W(unchecked(eax+56u),0u);goto L66e39;
   L66e39: // MOV dword ptr [EAX + 0x3c],0x0
    W(unchecked(eax+60u),0u);goto L66e40;
   L66e40: // MOV dword ptr [EAX + 0x40],0x0
    W(unchecked(eax+64u),0u);goto L66e47;
   L66e47: // MOV dword ptr [EAX + 0x44],0x0
    W(unchecked(eax+68u),0u);goto L66e4e;
   L66e4e: // MOV dword ptr [EAX + 0x48],0x0
    W(unchecked(eax+72u),0u);goto L66e55;
   L66e55: // MOV dword ptr [EAX + 0x4c],0x0
    W(unchecked(eax+76u),0u);goto L66e5c;
   L66e5c: // MOV dword ptr [EAX + 0x50],0x0
    W(unchecked(eax+80u),0u);goto L66e63;
   L66e63: // MOV dword ptr [EAX + 0x54],0x0
    W(unchecked(eax+84u),0u);goto L66e6a;
   L66e6a: // MOV dword ptr [EAX + 0x58],0x0
    W(unchecked(eax+88u),0u);goto L66e71;
   L66e71: // MOV dword ptr [EAX + 0x5c],0x0
    W(unchecked(eax+92u),0u);goto L66e78;
   L66e78: // MOV dword ptr [EAX + 0x60],0x0
    W(unchecked(eax+96u),0u);goto L66e7f;
   L66e7f: // MOV dword ptr [EAX + 0x64],0x0
    W(unchecked(eax+100u),0u);goto L66e86;
   L66e86: // MOV dword ptr [EAX + 0x68],0x0
    W(unchecked(eax+104u),0u);goto L66e8d;
   L66e8d: // MOV dword ptr [EAX + 0x6c],0x0
    W(unchecked(eax+108u),0u);goto L66e94;
   L66e94: // MOV dword ptr [EAX + 0x70],0x0
    W(unchecked(eax+112u),0u);goto L66e9b;
   L66e9b: // MOV dword ptr [EAX + 0x74],0x0
    W(unchecked(eax+116u),0u);goto L66ea2;
   L66ea2: // MOV dword ptr [EAX + 0x78],0x0
    W(unchecked(eax+120u),0u);goto L66ea9;
   L66ea9: // MOV dword ptr [EAX + 0x7c],0x0
    W(unchecked(eax+124u),0u);goto L66eb0;
   L66eb0: // MOV dword ptr [EAX + 0x80],0x0
    W(unchecked(eax+128u),0u);goto L66eba;
   L66eba: // MOV dword ptr [EAX + 0x84],0x0
    W(unchecked(eax+132u),0u);goto L66ec4;
   L66ec4: // MOV dword ptr [EAX + 0x88],0x0
    W(unchecked(eax+136u),0u);goto L66ece;
   L66ece: // MOV dword ptr [EAX + 0x8c],0x0
    W(unchecked(eax+140u),0u);goto L66ed8;
   L66ed8: // MOV dword ptr [EAX + 0x90],0x0
    W(unchecked(eax+144u),0u);goto L66ee2;
   L66ee2: // MOV dword ptr [EAX + 0x94],0x0
    W(unchecked(eax+148u),0u);goto L66eec;
   L66eec: // MOV dword ptr [EAX + 0x98],0x0
    W(unchecked(eax+152u),0u);goto L66ef6;
   L66ef6: // MOV dword ptr [EAX + 0x9c],0x0
    W(unchecked(eax+156u),0u);goto L66f00;
   L66f00: // MOV dword ptr [EAX + 0xa0],0x0
    W(unchecked(eax+160u),0u);goto L66f0a;
   L66f0a: // MOV dword ptr [EAX + 0xa4],0x0
    W(unchecked(eax+164u),0u);goto L66f14;
   L66f14: // MOV dword ptr [EAX + 0xa8],0x0
    W(unchecked(eax+168u),0u);goto L66f1e;
   L66f1e: // MOV dword ptr [EAX + 0xac],0x0
    W(unchecked(eax+172u),0u);goto L66f28;
   L66f28: // MOV dword ptr [EAX + 0xb0],0x0
    W(unchecked(eax+176u),0u);goto L66f32;
   L66f32: // MOV dword ptr [EAX + 0xb4],0x0
    W(unchecked(eax+180u),0u);goto L66f3c;
   L66f3c: // MOV dword ptr [EAX + 0xb8],0x0
    W(unchecked(eax+184u),0u);goto L66f46;
   L66f46: // MOV dword ptr [EAX + 0xbc],0x0
    W(unchecked(eax+188u),0u);goto L66f50;
   L66f50: // MOV dword ptr [EAX + 0xc0],0x0
    W(unchecked(eax+192u),0u);goto L66f5a;
   L66f5a: // MOV byte ptr [EAX + 0xc4],0x0
    h.WriteByte(unchecked(eax+196u),unchecked((byte)(0u)));goto L66f61;
   L66f61: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L66f64;
   L66f64: // JMP 0x000666d0
    goto L666d0;
  }
  public uint InitializeTimepunctWide(uint frame,uint owner,uint locale)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;W(frame+0x20u,owner);W(frame+0x24u,locale);goto L66fe4;
   L66fe4: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L66fe7;
   L66fe7: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66fe9;
   L66fe9: // JZ 0x000676e0
    if(zero)goto L676e0;goto L66fef;
   L66fef: // MOV ECX,dword ptr [ESP + 0x24]
    ecx=R(unchecked(esp+36u));goto L66ff3;
   L66ff3: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L66ff5;
   L66ff5: // JZ 0x00067510
    if(zero)goto L67510;goto L66ffb;
   L66ffb: // LEA EAX,[ESP + 0x24]
    eax=unchecked(esp+36u);goto L66fff;
   L66fff: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L67002;
   L67002: // CALL 0x00047fb0
    eax=Call(0x47fb0,R(unchecked(esp+0u)));goto L67007;
   L67007: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L6700a;
   L6700a: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6700e;
   L6700e: // MOV dword ptr [ESP],0x2005d
    W(unchecked(esp),131165u);goto L67015;
   L67015: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67019;
   L67019: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6701e;
   L6701e: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67021;
   L67021: // MOV dword ptr [EDX + 0x8],EAX
    W(unchecked(edx+8u),eax);goto L67024;
   L67024: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67028;
   L67028: // MOV dword ptr [ESP],0x20061
    W(unchecked(esp),131169u);goto L6702f;
   L6702f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67033;
   L67033: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67038;
   L67038: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6703b;
   L6703b: // MOV dword ptr [EDX + 0xc],EAX
    W(unchecked(edx+12u),eax);goto L6703e;
   L6703e: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67042;
   L67042: // MOV dword ptr [ESP],0x2005e
    W(unchecked(esp),131166u);goto L67049;
   L67049: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6704d;
   L6704d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67052;
   L67052: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67055;
   L67055: // MOV dword ptr [EDX + 0x10],EAX
    W(unchecked(edx+16u),eax);goto L67058;
   L67058: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6705c;
   L6705c: // MOV dword ptr [ESP],0x20064
    W(unchecked(esp),131172u);goto L67063;
   L67063: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67067;
   L67067: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6706c;
   L6706c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6706f;
   L6706f: // MOV dword ptr [EDX + 0x14],EAX
    W(unchecked(edx+20u),eax);goto L67072;
   L67072: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67076;
   L67076: // MOV dword ptr [ESP],0x2005c
    W(unchecked(esp),131164u);goto L6707d;
   L6707d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67081;
   L67081: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67086;
   L67086: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67089;
   L67089: // MOV dword ptr [EDX + 0x18],EAX
    W(unchecked(edx+24u),eax);goto L6708c;
   L6708c: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67090;
   L67090: // MOV dword ptr [ESP],0x20063
    W(unchecked(esp),131171u);goto L67097;
   L67097: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6709b;
   L6709b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L670a0;
   L670a0: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L670a3;
   L670a3: // MOV dword ptr [EDX + 0x1c],EAX
    W(unchecked(edx+28u),eax);goto L670a6;
   L670a6: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L670aa;
   L670aa: // MOV dword ptr [ESP],0x2005a
    W(unchecked(esp),131162u);goto L670b1;
   L670b1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L670b5;
   L670b5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L670ba;
   L670ba: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L670bd;
   L670bd: // MOV dword ptr [EDX + 0x20],EAX
    W(unchecked(edx+32u),eax);goto L670c0;
   L670c0: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L670c4;
   L670c4: // MOV dword ptr [ESP],0x2005b
    W(unchecked(esp),131163u);goto L670cb;
   L670cb: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L670cf;
   L670cf: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L670d4;
   L670d4: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L670d7;
   L670d7: // MOV dword ptr [EDX + 0x24],EAX
    W(unchecked(edx+36u),eax);goto L670da;
   L670da: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L670de;
   L670de: // MOV dword ptr [ESP],0x2005f
    W(unchecked(esp),131167u);goto L670e5;
   L670e5: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L670e9;
   L670e9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L670ee;
   L670ee: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L670f1;
   L670f1: // MOV dword ptr [EDX + 0x28],EAX
    W(unchecked(edx+40u),eax);goto L670f4;
   L670f4: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L670f8;
   L670f8: // MOV dword ptr [ESP],0x2003b
    W(unchecked(esp),131131u);goto L670ff;
   L670ff: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67103;
   L67103: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67108;
   L67108: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6710b;
   L6710b: // MOV dword ptr [EDX + 0x2c],EAX
    W(unchecked(edx+44u),eax);goto L6710e;
   L6710e: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67112;
   L67112: // MOV dword ptr [ESP],0x2003c
    W(unchecked(esp),131132u);goto L67119;
   L67119: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6711d;
   L6711d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67122;
   L67122: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67125;
   L67125: // MOV dword ptr [EDX + 0x30],EAX
    W(unchecked(edx+48u),eax);goto L67128;
   L67128: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6712c;
   L6712c: // MOV dword ptr [ESP],0x2003d
    W(unchecked(esp),131133u);goto L67133;
   L67133: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67137;
   L67137: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6713c;
   L6713c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6713f;
   L6713f: // MOV dword ptr [EDX + 0x34],EAX
    W(unchecked(edx+52u),eax);goto L67142;
   L67142: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67146;
   L67146: // MOV dword ptr [ESP],0x2003e
    W(unchecked(esp),131134u);goto L6714d;
   L6714d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67151;
   L67151: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67156;
   L67156: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67159;
   L67159: // MOV dword ptr [EDX + 0x38],EAX
    W(unchecked(edx+56u),eax);goto L6715c;
   L6715c: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67160;
   L67160: // MOV dword ptr [ESP],0x2003f
    W(unchecked(esp),131135u);goto L67167;
   L67167: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6716b;
   L6716b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67170;
   L67170: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67173;
   L67173: // MOV dword ptr [EDX + 0x3c],EAX
    W(unchecked(edx+60u),eax);goto L67176;
   L67176: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6717a;
   L6717a: // MOV dword ptr [ESP],0x20040
    W(unchecked(esp),131136u);goto L67181;
   L67181: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67185;
   L67185: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6718a;
   L6718a: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6718d;
   L6718d: // MOV dword ptr [EDX + 0x40],EAX
    W(unchecked(edx+64u),eax);goto L67190;
   L67190: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67194;
   L67194: // MOV dword ptr [ESP],0x20041
    W(unchecked(esp),131137u);goto L6719b;
   L6719b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6719f;
   L6719f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L671a4;
   L671a4: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L671a7;
   L671a7: // MOV dword ptr [EDX + 0x44],EAX
    W(unchecked(edx+68u),eax);goto L671aa;
   L671aa: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L671ae;
   L671ae: // MOV dword ptr [ESP],0x20034
    W(unchecked(esp),131124u);goto L671b5;
   L671b5: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L671b9;
   L671b9: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L671be;
   L671be: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L671c1;
   L671c1: // MOV dword ptr [EDX + 0x48],EAX
    W(unchecked(edx+72u),eax);goto L671c4;
   L671c4: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L671c8;
   L671c8: // MOV dword ptr [ESP],0x20035
    W(unchecked(esp),131125u);goto L671cf;
   L671cf: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L671d3;
   L671d3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L671d8;
   L671d8: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L671db;
   L671db: // MOV dword ptr [EDX + 0x4c],EAX
    W(unchecked(edx+76u),eax);goto L671de;
   L671de: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L671e2;
   L671e2: // MOV dword ptr [ESP],0x20036
    W(unchecked(esp),131126u);goto L671e9;
   L671e9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L671ed;
   L671ed: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L671f2;
   L671f2: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L671f5;
   L671f5: // MOV dword ptr [EDX + 0x50],EAX
    W(unchecked(edx+80u),eax);goto L671f8;
   L671f8: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L671fc;
   L671fc: // MOV dword ptr [ESP],0x20037
    W(unchecked(esp),131127u);goto L67203;
   L67203: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67207;
   L67207: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6720c;
   L6720c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6720f;
   L6720f: // MOV dword ptr [EDX + 0x54],EAX
    W(unchecked(edx+84u),eax);goto L67212;
   L67212: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67216;
   L67216: // MOV dword ptr [ESP],0x20038
    W(unchecked(esp),131128u);goto L6721d;
   L6721d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67221;
   L67221: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67226;
   L67226: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67229;
   L67229: // MOV dword ptr [EDX + 0x58],EAX
    W(unchecked(edx+88u),eax);goto L6722c;
   L6722c: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67230;
   L67230: // MOV dword ptr [ESP],0x20039
    W(unchecked(esp),131129u);goto L67237;
   L67237: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6723b;
   L6723b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67240;
   L67240: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67243;
   L67243: // MOV dword ptr [EDX + 0x5c],EAX
    W(unchecked(edx+92u),eax);goto L67246;
   L67246: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6724a;
   L6724a: // MOV dword ptr [ESP],0x2003a
    W(unchecked(esp),131130u);goto L67251;
   L67251: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67255;
   L67255: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6725a;
   L6725a: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6725d;
   L6725d: // MOV dword ptr [EDX + 0x60],EAX
    W(unchecked(edx+96u),eax);goto L67260;
   L67260: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67264;
   L67264: // MOV dword ptr [ESP],0x2004e
    W(unchecked(esp),131150u);goto L6726b;
   L6726b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6726f;
   L6726f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67274;
   L67274: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67277;
   L67277: // MOV dword ptr [EDX + 0x64],EAX
    W(unchecked(edx+100u),eax);goto L6727a;
   L6727a: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6727e;
   L6727e: // MOV dword ptr [ESP],0x2004f
    W(unchecked(esp),131151u);goto L67285;
   L67285: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67289;
   L67289: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6728e;
   L6728e: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67291;
   L67291: // MOV dword ptr [EDX + 0x68],EAX
    W(unchecked(edx+104u),eax);goto L67294;
   L67294: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67298;
   L67298: // MOV dword ptr [ESP],0x20050
    W(unchecked(esp),131152u);goto L6729f;
   L6729f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L672a3;
   L672a3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L672a8;
   L672a8: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L672ab;
   L672ab: // MOV dword ptr [EDX + 0x6c],EAX
    W(unchecked(edx+108u),eax);goto L672ae;
   L672ae: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L672b2;
   L672b2: // MOV dword ptr [ESP],0x20051
    W(unchecked(esp),131153u);goto L672b9;
   L672b9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L672bd;
   L672bd: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L672c2;
   L672c2: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L672c5;
   L672c5: // MOV dword ptr [EDX + 0x70],EAX
    W(unchecked(edx+112u),eax);goto L672c8;
   L672c8: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L672cc;
   L672cc: // MOV dword ptr [ESP],0x20052
    W(unchecked(esp),131154u);goto L672d3;
   L672d3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L672d7;
   L672d7: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L672dc;
   L672dc: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L672df;
   L672df: // MOV dword ptr [EDX + 0x74],EAX
    W(unchecked(edx+116u),eax);goto L672e2;
   L672e2: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L672e6;
   L672e6: // MOV dword ptr [ESP],0x20053
    W(unchecked(esp),131155u);goto L672ed;
   L672ed: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L672f1;
   L672f1: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L672f6;
   L672f6: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L672f9;
   L672f9: // MOV dword ptr [EDX + 0x78],EAX
    W(unchecked(edx+120u),eax);goto L672fc;
   L672fc: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67300;
   L67300: // MOV dword ptr [ESP],0x20054
    W(unchecked(esp),131156u);goto L67307;
   L67307: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6730b;
   L6730b: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67310;
   L67310: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67313;
   L67313: // MOV dword ptr [EDX + 0x7c],EAX
    W(unchecked(edx+124u),eax);goto L67316;
   L67316: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6731a;
   L6731a: // MOV dword ptr [ESP],0x20055
    W(unchecked(esp),131157u);goto L67321;
   L67321: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67325;
   L67325: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6732a;
   L6732a: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6732d;
   L6732d: // MOV dword ptr [EDX + 0x80],EAX
    W(unchecked(edx+128u),eax);goto L67333;
   L67333: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67337;
   L67337: // MOV dword ptr [ESP],0x20056
    W(unchecked(esp),131158u);goto L6733e;
   L6733e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67342;
   L67342: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67347;
   L67347: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6734a;
   L6734a: // MOV dword ptr [EDX + 0x84],EAX
    W(unchecked(edx+132u),eax);goto L67350;
   L67350: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67354;
   L67354: // MOV dword ptr [ESP],0x20057
    W(unchecked(esp),131159u);goto L6735b;
   L6735b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6735f;
   L6735f: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67364;
   L67364: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67367;
   L67367: // MOV dword ptr [EDX + 0x88],EAX
    W(unchecked(edx+136u),eax);goto L6736d;
   L6736d: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67371;
   L67371: // MOV dword ptr [ESP],0x20058
    W(unchecked(esp),131160u);goto L67378;
   L67378: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6737c;
   L6737c: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67381;
   L67381: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67384;
   L67384: // MOV dword ptr [EDX + 0x8c],EAX
    W(unchecked(edx+140u),eax);goto L6738a;
   L6738a: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6738e;
   L6738e: // MOV dword ptr [ESP],0x20059
    W(unchecked(esp),131161u);goto L67395;
   L67395: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67399;
   L67399: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6739e;
   L6739e: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L673a1;
   L673a1: // MOV dword ptr [EDX + 0x90],EAX
    W(unchecked(edx+144u),eax);goto L673a7;
   L673a7: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L673ab;
   L673ab: // MOV dword ptr [ESP],0x20042
    W(unchecked(esp),131138u);goto L673b2;
   L673b2: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L673b6;
   L673b6: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L673bb;
   L673bb: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L673be;
   L673be: // MOV dword ptr [EDX + 0x94],EAX
    W(unchecked(edx+148u),eax);goto L673c4;
   L673c4: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L673c8;
   L673c8: // MOV dword ptr [ESP],0x20043
    W(unchecked(esp),131139u);goto L673cf;
   L673cf: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L673d3;
   L673d3: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L673d8;
   L673d8: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L673db;
   L673db: // MOV dword ptr [EDX + 0x98],EAX
    W(unchecked(edx+152u),eax);goto L673e1;
   L673e1: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L673e5;
   L673e5: // MOV dword ptr [ESP],0x20044
    W(unchecked(esp),131140u);goto L673ec;
   L673ec: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L673f0;
   L673f0: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L673f5;
   L673f5: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L673f8;
   L673f8: // MOV dword ptr [EDX + 0x9c],EAX
    W(unchecked(edx+156u),eax);goto L673fe;
   L673fe: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67402;
   L67402: // MOV dword ptr [ESP],0x20045
    W(unchecked(esp),131141u);goto L67409;
   L67409: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6740d;
   L6740d: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67412;
   L67412: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67415;
   L67415: // MOV dword ptr [EDX + 0xa0],EAX
    W(unchecked(edx+160u),eax);goto L6741b;
   L6741b: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6741f;
   L6741f: // MOV dword ptr [ESP],0x20046
    W(unchecked(esp),131142u);goto L67426;
   L67426: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6742a;
   L6742a: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6742f;
   L6742f: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67432;
   L67432: // MOV dword ptr [EDX + 0xa4],EAX
    W(unchecked(edx+164u),eax);goto L67438;
   L67438: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L6743c;
   L6743c: // MOV dword ptr [ESP],0x20047
    W(unchecked(esp),131143u);goto L67443;
   L67443: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67447;
   L67447: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L6744c;
   L6744c: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6744f;
   L6744f: // MOV dword ptr [EDX + 0xa8],EAX
    W(unchecked(edx+168u),eax);goto L67455;
   L67455: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67459;
   L67459: // MOV dword ptr [ESP],0x20048
    W(unchecked(esp),131144u);goto L67460;
   L67460: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67464;
   L67464: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67469;
   L67469: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L6746c;
   L6746c: // MOV dword ptr [EDX + 0xac],EAX
    W(unchecked(edx+172u),eax);goto L67472;
   L67472: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67476;
   L67476: // MOV dword ptr [ESP],0x20049
    W(unchecked(esp),131145u);goto L6747d;
   L6747d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L67481;
   L67481: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L67486;
   L67486: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L67489;
   L67489: // MOV dword ptr [EDX + 0xb0],EAX
    W(unchecked(edx+176u),eax);goto L6748f;
   L6748f: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L67493;
   L67493: // MOV dword ptr [ESP],0x2004a
    W(unchecked(esp),131146u);goto L6749a;
   L6749a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L6749e;
   L6749e: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L674a3;
   L674a3: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L674a6;
   L674a6: // MOV dword ptr [EDX + 0xb4],EAX
    W(unchecked(edx+180u),eax);goto L674ac;
   L674ac: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L674b0;
   L674b0: // MOV dword ptr [ESP],0x2004b
    W(unchecked(esp),131147u);goto L674b7;
   L674b7: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L674bb;
   L674bb: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L674c0;
   L674c0: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L674c3;
   L674c3: // MOV dword ptr [EDX + 0xb8],EAX
    W(unchecked(edx+184u),eax);goto L674c9;
   L674c9: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L674cd;
   L674cd: // MOV dword ptr [ESP],0x2004c
    W(unchecked(esp),131148u);goto L674d4;
   L674d4: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L674d8;
   L674d8: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L674dd;
   L674dd: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L674e0;
   L674e0: // MOV dword ptr [EDX + 0xbc],EAX
    W(unchecked(edx+188u),eax);goto L674e6;
   L674e6: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L674ea;
   L674ea: // MOV dword ptr [ESP],0x2004d
    W(unchecked(esp),131149u);goto L674f1;
   L674f1: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L674f5;
   L674f5: // CALL 0x00047e10
    eax=Call(0x47e10,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L674fa;
   L674fa: // MOV EDX,dword ptr [ESI + 0x8]
    edx=R(unchecked(esi+8u));goto L674fd;
   L674fd: // MOV dword ptr [EDX + 0xc0],EAX
    W(unchecked(edx+192u),eax);goto L67503;
   L67503:return 0;
   L67510: // CALL 0x00049d10
    eax=Call(0x49d10);goto L67515;
   L67515: // LEA EDX,[EBX + 0xfffd8f90]
    edx=unchecked(ebx+4294807440u);goto L6751b;
   L6751b: // LEA ECX,[EBX + 0xfffd8fd8]
    ecx=unchecked(ebx+4294807512u);goto L67521;
   L67521: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L67524;
   L67524: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L67527;
   L67527: // MOV dword ptr [EAX + 0x8],EDX
    W(unchecked(eax+8u),edx);goto L6752a;
   L6752a: // MOV dword ptr [EAX + 0xc],EDX
    W(unchecked(eax+12u),edx);goto L6752d;
   L6752d: // LEA EDX,[EBX + 0xfffd8fb4]
    edx=unchecked(ebx+4294807476u);goto L67533;
   L67533: // MOV dword ptr [EAX + 0x10],EDX
    W(unchecked(eax+16u),edx);goto L67536;
   L67536: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L67539;
   L67539: // LEA EDX,[EBX + 0xfffd8e68]
    edx=unchecked(ebx+4294807144u);goto L6753f;
   L6753f: // MOV dword ptr [EAX + 0x18],EDX
    W(unchecked(eax+24u),edx);goto L67542;
   L67542: // MOV dword ptr [EAX + 0x1c],EDX
    W(unchecked(eax+28u),edx);goto L67545;
   L67545: // MOV dword ptr [EAX + 0x28],EDX
    W(unchecked(eax+40u),edx);goto L67548;
   L67548: // LEA EDX,[EBX + 0xfffd8ff0]
    edx=unchecked(ebx+4294807536u);goto L6754e;
   L6754e: // MOV dword ptr [EAX + 0x2c],EDX
    W(unchecked(eax+44u),edx);goto L67551;
   L67551: // LEA EDX,[EBX + 0xfffd900c]
    edx=unchecked(ebx+4294807564u);goto L67557;
   L67557: // MOV dword ptr [EAX + 0x30],EDX
    W(unchecked(eax+48u),edx);goto L6755a;
   L6755a: // LEA EDX,[EBX + 0xfffd9028]
    edx=unchecked(ebx+4294807592u);goto L67560;
   L67560: // MOV dword ptr [EAX + 0x34],EDX
    W(unchecked(eax+52u),edx);goto L67563;
   L67563: // LEA EDX,[EBX + 0xfffd9048]
    edx=unchecked(ebx+4294807624u);goto L67569;
   L67569: // MOV dword ptr [EAX + 0x38],EDX
    W(unchecked(eax+56u),edx);goto L6756c;
   L6756c: // LEA EDX,[EBX + 0xfffd9070]
    edx=unchecked(ebx+4294807664u);goto L67572;
   L67572: // MOV dword ptr [EAX + 0x3c],EDX
    W(unchecked(eax+60u),edx);goto L67575;
   L67575: // LEA EDX,[EBX + 0xfffd9094]
    edx=unchecked(ebx+4294807700u);goto L6757b;
   L6757b: // MOV dword ptr [EAX + 0x40],EDX
    W(unchecked(eax+64u),edx);goto L6757e;
   L6757e: // LEA EDX,[EBX + 0xfffd90b0]
    edx=unchecked(ebx+4294807728u);goto L67584;
   L67584: // MOV dword ptr [EAX + 0x44],EDX
    W(unchecked(eax+68u),edx);goto L67587;
   L67587: // LEA EDX,[EBX + 0xfffd90d4]
    edx=unchecked(ebx+4294807764u);goto L6758d;
   L6758d: // MOV dword ptr [EAX + 0x48],EDX
    W(unchecked(eax+72u),edx);goto L67590;
   L67590: // LEA EDX,[EBX + 0xfffd90e4]
    edx=unchecked(ebx+4294807780u);goto L67596;
   L67596: // MOV dword ptr [EAX + 0x4c],EDX
    W(unchecked(eax+76u),edx);goto L67599;
   L67599: // LEA EDX,[EBX + 0xfffd90f4]
    edx=unchecked(ebx+4294807796u);goto L6759f;
   L6759f: // MOV dword ptr [EAX + 0x50],EDX
    W(unchecked(eax+80u),edx);goto L675a2;
   L675a2: // LEA EDX,[EBX + 0xfffd9104]
    edx=unchecked(ebx+4294807812u);goto L675a8;
   L675a8: // MOV dword ptr [EAX + 0x54],EDX
    W(unchecked(eax+84u),edx);goto L675ab;
   L675ab: // LEA EDX,[EBX + 0xfffd9114]
    edx=unchecked(ebx+4294807828u);goto L675b1;
   L675b1: // MOV dword ptr [EAX + 0x58],EDX
    W(unchecked(eax+88u),edx);goto L675b4;
   L675b4: // LEA EDX,[EBX + 0xfffd9124]
    edx=unchecked(ebx+4294807844u);goto L675ba;
   L675ba: // MOV dword ptr [EAX + 0x5c],EDX
    W(unchecked(eax+92u),edx);goto L675bd;
   L675bd: // LEA EDX,[EBX + 0xfffd9134]
    edx=unchecked(ebx+4294807860u);goto L675c3;
   L675c3: // MOV dword ptr [EAX + 0x20],ECX
    W(unchecked(eax+32u),ecx);goto L675c6;
   L675c6: // LEA ECX,[EBX + 0xfffd8fe4]
    ecx=unchecked(ebx+4294807524u);goto L675cc;
   L675cc: // MOV dword ptr [EAX + 0x60],EDX
    W(unchecked(eax+96u),edx);goto L675cf;
   L675cf: // LEA EDX,[EBX + 0xfffd9144]
    edx=unchecked(ebx+4294807876u);goto L675d5;
   L675d5: // MOV dword ptr [EAX + 0x24],ECX
    W(unchecked(eax+36u),ecx);goto L675d8;
   L675d8: // LEA ECX,[EBX + 0xfffd91c8]
    ecx=unchecked(ebx+4294808008u);goto L675de;
   L675de: // MOV dword ptr [EAX + 0x64],EDX
    W(unchecked(eax+100u),edx);goto L675e1;
   L675e1: // LEA EDX,[EBX + 0xfffd9164]
    edx=unchecked(ebx+4294807908u);goto L675e7;
   L675e7: // MOV dword ptr [EAX + 0x68],EDX
    W(unchecked(eax+104u),edx);goto L675ea;
   L675ea: // LEA EDX,[EBX + 0xfffd9188]
    edx=unchecked(ebx+4294807944u);goto L675f0;
   L675f0: // MOV dword ptr [EAX + 0x78],ECX
    W(unchecked(eax+120u),ecx);goto L675f3;
   L675f3: // LEA ECX,[EBX + 0xfffd91dc]
    ecx=unchecked(ebx+4294808028u);goto L675f9;
   L675f9: // MOV dword ptr [EAX + 0x6c],EDX
    W(unchecked(eax+108u),edx);goto L675fc;
   L675fc: // LEA EDX,[EBX + 0xfffd91a0]
    edx=unchecked(ebx+4294807968u);goto L67602;
   L67602: // MOV dword ptr [EAX + 0x7c],ECX
    W(unchecked(eax+124u),ecx);goto L67605;
   L67605: // LEA ECX,[EBX + 0xfffd91f0]
    ecx=unchecked(ebx+4294808048u);goto L6760b;
   L6760b: // MOV dword ptr [EAX + 0x70],EDX
    W(unchecked(eax+112u),edx);goto L6760e;
   L6760e: // LEA EDX,[EBX + 0xfffd91b8]
    edx=unchecked(ebx+4294807992u);goto L67614;
   L67614: // MOV dword ptr [EAX + 0x80],ECX
    W(unchecked(eax+128u),ecx);goto L6761a;
   L6761a: // LEA ECX,[EBX + 0xfffd920c]
    ecx=unchecked(ebx+4294808076u);goto L67620;
   L67620: // MOV dword ptr [EAX + 0x74],EDX
    W(unchecked(eax+116u),edx);goto L67623;
   L67623: // MOV dword ptr [EAX + 0x84],ECX
    W(unchecked(eax+132u),ecx);goto L67629;
   L67629: // LEA ECX,[EBX + 0xfffd9234]
    ecx=unchecked(ebx+4294808116u);goto L6762f;
   L6762f: // MOV dword ptr [EAX + 0xa4],EDX
    W(unchecked(eax+164u),edx);goto L67635;
   L67635: // LEA EDX,[EBX + 0xfffd92dc]
    edx=unchecked(ebx+4294808284u);goto L6763b;
   L6763b: // MOV dword ptr [EAX + 0x88],ECX
    W(unchecked(eax+136u),ecx);goto L67641;
   L67641: // LEA ECX,[EBX + 0xfffd9254]
    ecx=unchecked(ebx+4294808148u);goto L67647;
   L67647: // MOV dword ptr [EAX + 0xa8],EDX
    W(unchecked(eax+168u),edx);goto L6764d;
   L6764d: // LEA EDX,[EBX + 0xfffd92ec]
    edx=unchecked(ebx+4294808300u);goto L67653;
   L67653: // MOV dword ptr [EAX + 0x8c],ECX
    W(unchecked(eax+140u),ecx);goto L67659;
   L67659: // LEA ECX,[EBX + 0xfffd9278]
    ecx=unchecked(ebx+4294808184u);goto L6765f;
   L6765f: // MOV dword ptr [EAX + 0xac],EDX
    W(unchecked(eax+172u),edx);goto L67665;
   L67665: // LEA EDX,[EBX + 0xfffd92fc]
    edx=unchecked(ebx+4294808316u);goto L6766b;
   L6766b: // MOV dword ptr [EAX + 0x90],ECX
    W(unchecked(eax+144u),ecx);goto L67671;
   L67671: // LEA ECX,[EBX + 0xfffd929c]
    ecx=unchecked(ebx+4294808220u);goto L67677;
   L67677: // MOV dword ptr [EAX + 0xb0],EDX
    W(unchecked(eax+176u),edx);goto L6767d;
   L6767d: // LEA EDX,[EBX + 0xfffd930c]
    edx=unchecked(ebx+4294808332u);goto L67683;
   L67683: // MOV dword ptr [EAX + 0x94],ECX
    W(unchecked(eax+148u),ecx);goto L67689;
   L67689: // LEA ECX,[EBX + 0xfffd92ac]
    ecx=unchecked(ebx+4294808236u);goto L6768f;
   L6768f: // MOV dword ptr [EAX + 0xb4],EDX
    W(unchecked(eax+180u),edx);goto L67695;
   L67695: // LEA EDX,[EBX + 0xfffd931c]
    edx=unchecked(ebx+4294808348u);goto L6769b;
   L6769b: // MOV dword ptr [EAX + 0x98],ECX
    W(unchecked(eax+152u),ecx);goto L676a1;
   L676a1: // LEA ECX,[EBX + 0xfffd92bc]
    ecx=unchecked(ebx+4294808252u);goto L676a7;
   L676a7: // MOV dword ptr [EAX + 0xb8],EDX
    W(unchecked(eax+184u),edx);goto L676ad;
   L676ad: // LEA EDX,[EBX + 0xfffd932c]
    edx=unchecked(ebx+4294808364u);goto L676b3;
   L676b3: // MOV dword ptr [EAX + 0x9c],ECX
    W(unchecked(eax+156u),ecx);goto L676b9;
   L676b9: // LEA ECX,[EBX + 0xfffd92cc]
    ecx=unchecked(ebx+4294808268u);goto L676bf;
   L676bf: // MOV dword ptr [EAX + 0xbc],EDX
    W(unchecked(eax+188u),edx);goto L676c5;
   L676c5: // LEA EDX,[EBX + 0xfffd933c]
    edx=unchecked(ebx+4294808380u);goto L676cb;
   L676cb: // MOV dword ptr [EAX + 0xa0],ECX
    W(unchecked(eax+160u),ecx);goto L676d1;
   L676d1: // MOV dword ptr [EAX + 0xc0],EDX
    W(unchecked(eax+192u),edx);goto L676d7;
   L676d7:return 0;
   L676e0: // MOV dword ptr [ESP],0xc8
    W(unchecked(esp),200u);goto L676e7;
   L676e7: // CALL 0x0004a450
    eax=Call(0x4a450,R(unchecked(esp+0u)));goto L676ec;
   L676ec: // MOV EDX,dword ptr [EBX + 0xfffffa9c]
    edx=R(unchecked(ebx+4294965916u));goto L676f2;
   L676f2: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L676f5;
   L676f5: // MOV dword ptr [EAX + 0x4],0x0
    W(unchecked(eax+4u),0u);goto L676fc;
   L676fc: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L676fe;
   L676fe: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L67705;
   L67705: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L6770c;
   L6770c: // MOV dword ptr [EAX + 0x10],0x0
    W(unchecked(eax+16u),0u);goto L67713;
   L67713: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L6771a;
   L6771a: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L67721;
   L67721: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L67728;
   L67728: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L6772f;
   L6772f: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L67736;
   L67736: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L6773d;
   L6773d: // MOV dword ptr [EAX + 0x2c],0x0
    W(unchecked(eax+44u),0u);goto L67744;
   L67744: // MOV dword ptr [EAX + 0x30],0x0
    W(unchecked(eax+48u),0u);goto L6774b;
   L6774b: // MOV dword ptr [EAX + 0x34],0x0
    W(unchecked(eax+52u),0u);goto L67752;
   L67752: // MOV dword ptr [EAX + 0x38],0x0
    W(unchecked(eax+56u),0u);goto L67759;
   L67759: // MOV dword ptr [EAX + 0x3c],0x0
    W(unchecked(eax+60u),0u);goto L67760;
   L67760: // MOV dword ptr [EAX + 0x40],0x0
    W(unchecked(eax+64u),0u);goto L67767;
   L67767: // MOV dword ptr [EAX + 0x44],0x0
    W(unchecked(eax+68u),0u);goto L6776e;
   L6776e: // MOV dword ptr [EAX + 0x48],0x0
    W(unchecked(eax+72u),0u);goto L67775;
   L67775: // MOV dword ptr [EAX + 0x4c],0x0
    W(unchecked(eax+76u),0u);goto L6777c;
   L6777c: // MOV dword ptr [EAX + 0x50],0x0
    W(unchecked(eax+80u),0u);goto L67783;
   L67783: // MOV dword ptr [EAX + 0x54],0x0
    W(unchecked(eax+84u),0u);goto L6778a;
   L6778a: // MOV dword ptr [EAX + 0x58],0x0
    W(unchecked(eax+88u),0u);goto L67791;
   L67791: // MOV dword ptr [EAX + 0x5c],0x0
    W(unchecked(eax+92u),0u);goto L67798;
   L67798: // MOV dword ptr [EAX + 0x60],0x0
    W(unchecked(eax+96u),0u);goto L6779f;
   L6779f: // MOV dword ptr [EAX + 0x64],0x0
    W(unchecked(eax+100u),0u);goto L677a6;
   L677a6: // MOV dword ptr [EAX + 0x68],0x0
    W(unchecked(eax+104u),0u);goto L677ad;
   L677ad: // MOV dword ptr [EAX + 0x6c],0x0
    W(unchecked(eax+108u),0u);goto L677b4;
   L677b4: // MOV dword ptr [EAX + 0x70],0x0
    W(unchecked(eax+112u),0u);goto L677bb;
   L677bb: // MOV dword ptr [EAX + 0x74],0x0
    W(unchecked(eax+116u),0u);goto L677c2;
   L677c2: // MOV dword ptr [EAX + 0x78],0x0
    W(unchecked(eax+120u),0u);goto L677c9;
   L677c9: // MOV dword ptr [EAX + 0x7c],0x0
    W(unchecked(eax+124u),0u);goto L677d0;
   L677d0: // MOV dword ptr [EAX + 0x80],0x0
    W(unchecked(eax+128u),0u);goto L677da;
   L677da: // MOV dword ptr [EAX + 0x84],0x0
    W(unchecked(eax+132u),0u);goto L677e4;
   L677e4: // MOV dword ptr [EAX + 0x88],0x0
    W(unchecked(eax+136u),0u);goto L677ee;
   L677ee: // MOV dword ptr [EAX + 0x8c],0x0
    W(unchecked(eax+140u),0u);goto L677f8;
   L677f8: // MOV dword ptr [EAX + 0x90],0x0
    W(unchecked(eax+144u),0u);goto L67802;
   L67802: // MOV dword ptr [EAX + 0x94],0x0
    W(unchecked(eax+148u),0u);goto L6780c;
   L6780c: // MOV dword ptr [EAX + 0x98],0x0
    W(unchecked(eax+152u),0u);goto L67816;
   L67816: // MOV dword ptr [EAX + 0x9c],0x0
    W(unchecked(eax+156u),0u);goto L67820;
   L67820: // MOV dword ptr [EAX + 0xa0],0x0
    W(unchecked(eax+160u),0u);goto L6782a;
   L6782a: // MOV dword ptr [EAX + 0xa4],0x0
    W(unchecked(eax+164u),0u);goto L67834;
   L67834: // MOV dword ptr [EAX + 0xa8],0x0
    W(unchecked(eax+168u),0u);goto L6783e;
   L6783e: // MOV dword ptr [EAX + 0xac],0x0
    W(unchecked(eax+172u),0u);goto L67848;
   L67848: // MOV dword ptr [EAX + 0xb0],0x0
    W(unchecked(eax+176u),0u);goto L67852;
   L67852: // MOV dword ptr [EAX + 0xb4],0x0
    W(unchecked(eax+180u),0u);goto L6785c;
   L6785c: // MOV dword ptr [EAX + 0xb8],0x0
    W(unchecked(eax+184u),0u);goto L67866;
   L67866: // MOV dword ptr [EAX + 0xbc],0x0
    W(unchecked(eax+188u),0u);goto L67870;
   L67870: // MOV dword ptr [EAX + 0xc0],0x0
    W(unchecked(eax+192u),0u);goto L6787a;
   L6787a: // MOV byte ptr [EAX + 0xc4],0x0
    h.WriteByte(unchecked(eax+196u),unchecked((byte)(0u)));goto L67881;
   L67881: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L67884;
   L67884: // JMP 0x00066fef
    goto L66fef;
  }
 }
}
