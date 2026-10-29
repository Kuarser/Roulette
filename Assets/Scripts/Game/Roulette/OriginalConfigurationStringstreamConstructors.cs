using System;
namespace RouletteDeluxe
{
 // Physical packaged char stringbuf/stringstream C1/C2 constructors. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Original owner/VTT/string/open-mode inputs are explicitly supplied; nested ios, locale, string and destruction operations remain required.
 public sealed class OriginalConfigurationStringstreamConstructors
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
  public OriginalConfigurationStringstreamConstructors(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ConstructStringbufMode(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);W(frame+0x24u,mode);goto L91be0;
   L91be0: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L91be4;
   L91be4: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L91beb;
   L91beb: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L91bf1;
   L91bf1: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L91bf8;
   L91bf8: // MOV dword ptr [ESI + 0xc],0x0
    W(unchecked(esi+12u),0u);goto L91bff;
   L91bff: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L91c06;
   L91c06: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L91c09;
   L91c09: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L91c0b;
   L91c0b: // LEA EAX,[ESI + 0x1c]
    eax=unchecked(esi+28u);goto L91c0e;
   L91c0e: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L91c15;
   L91c15: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L91c1c;
   L91c1c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L91c1f;
   L91c1f: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L91c24;
   L91c24: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L91c2a;
   L91c2a: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L91c2d;
   L91c2d: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L91c2f;
   L91c2f: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L91c33;
   L91c33: // MOV dword ptr [ESI + 0x20],EAX
    W(unchecked(esi+32u),eax);goto L91c36;
   L91c36: // MOV EAX,dword ptr [EBX + 0xfffffc8c]
    eax=R(unchecked(ebx+4294966412u));goto L91c3c;
   L91c3c: // ADD EAX,0xc
    a=eax;b=12u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L91c3f;
   L91c3f: // MOV dword ptr [ESI + 0x24],EAX
    W(unchecked(esi+36u),eax);goto L91c42;
   L91c42:return 0;
  }
  public uint ConstructStringbufString(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x40u,owner);W(frame+0x44u,source);W(frame+0x48u,mode);goto L92272;
   L92272: // MOV dword ptr [ESP + 0x30],ESI
    W(unchecked(esp+48u),esi);goto L92276;
   L92276: // MOV ESI,dword ptr [ESP + 0x40]
    esi=R(unchecked(esp+64u));goto L9227a;
   L9227a: // MOV dword ptr [ESP + 0x34],EDI
    W(unchecked(esp+52u),edi);goto L9227e;
   L9227e: // MOV dword ptr [ESP + 0x38],EBP
    W(unchecked(esp+56u),ebp);goto L92282;
   L92282: // MOV EBP,dword ptr [ESP + 0x48]
    ebp=R(unchecked(esp+72u));goto L92286;
   L92286: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L9228d;
   L9228d: // LEA EDI,[ESI + 0x24]
    edi=unchecked(esi+36u);goto L92290;
   L92290: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L92296;
   L92296: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9229d;
   L9229d: // MOV dword ptr [ESI + 0xc],0x0
    W(unchecked(esi+12u),0u);goto L922a4;
   L922a4: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L922ab;
   L922ab: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L922ae;
   L922ae: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L922b0;
   L922b0: // LEA EAX,[ESI + 0x1c]
    eax=unchecked(esi+28u);goto L922b3;
   L922b3: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L922ba;
   L922ba: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L922c1;
   L922c1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L922c4;
   L922c4: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L922c9;
   L922c9: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L922cf;
   L922cf: // LEA EDX,[ESP + 0x1f]
    edx=unchecked(esp+31u);goto L922d3;
   L922d3: // MOV dword ptr [ESI + 0x20],0x0
    W(unchecked(esi+32u),0u);goto L922da;
   L922da: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L922dd;
   L922dd: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L922df;
   L922df: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L922e3;
   L922e3: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto L922e5;
   L922e5: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L922e9;
   L922e9: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L922ec;
   L922ec: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L922f0;
   L922f0: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L922f3;
   L922f3: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L922f7;
   L922f7: // CALL 0x00048d30
    try{eax=Call(0x48d30,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9233f;}goto L922fc;
   L922fc: // MOV dword ptr [ESI + 0x20],EBP
    W(unchecked(esi+32u),ebp);goto L922ff;
   L922ff: // AND EBP,0x3
    a=ebp;b=3u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L92302;
   L92302: // JZ 0x00092338
    if(zero)goto L92338;goto L92304;
   L92304: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L92307;
   L92307: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L9230a;
   L9230a: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L9230e;
   L9230e: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L92316;
   L92316: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9231a;
   L9231a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9231d;
   L9231d: // CALL 0x00048480
    try{eax=Call(0x48480,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L92351;}goto L92322;
   L92322:return 0;
   L92338: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L9233a;
   L9233a: // MOV EAX,dword ptr [ESI + 0x24]
    eax=R(unchecked(esi+36u));goto L9233d;
   L9233d: // JMP 0x0009230a
    goto L9230a;
   L9233f: // MOV EDI,EAX
    edi=eax;goto L92341;
   L92341: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L92344;
   L92344: // CALL 0x00049ae0
    eax=Call(0x49ae0,R(unchecked(esp+0u)));goto L92349;
   L92349: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9234c;
   L9234c: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L92351: // MOV EBP,EAX
    ebp=eax;goto L92353;
   L92353: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L92356;
   L92356: // MOV EDI,EBP
    edi=ebp;goto L92358;
   L92358: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L9235d;
   L9235d: // JMP 0x00092341
    goto L92341;
  }
  public uint ConstructStringstreamBaseMode(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;edi=vtt;W(frame+0x40u,owner);W(frame+0x44u,vtt);W(frame+0x48u,mode);goto L9398a;
   L9398a: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L9398d;
   L9398d: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93990;
   L93990: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93992;
   L93992: // MOV ECX,dword ptr [EDI + 0xc]
    ecx=R(unchecked(edi+12u));goto L93995;
   L93995: // MOV dword ptr [ESI + EDX*0x1],ECX
    W(unchecked(esi+edx*0x1u),ecx);goto L93998;
   L93998: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L9399f;
   L9399f: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L939a7;
   L939a7: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L939aa;
   L939aa: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L939ac;
   L939ac: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L939af;
   L939af: // CALL 0x00049e90
    eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L939b4;
   L939b4: // MOV EDX,dword ptr [EDI + 0x10]
    edx=R(unchecked(edi+16u));goto L939b7;
   L939b7: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L939ba;
   L939ba: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L939bd;
   L939bd: // ADD EAX,dword ptr [EDX + -0xc]
    a=eax;b=R(unchecked(edx+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L939c0;
   L939c0: // MOV EDX,dword ptr [EDI + 0x14]
    edx=R(unchecked(edi+20u));goto L939c3;
   L939c3: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L939c5;
   L939c5: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L939cd;
   L939cd: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L939d0;
   L939d0: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93a7a;}goto L939d5;
   L939d5: // MOV EAX,dword ptr [EDI + 0x4]
    eax=R(unchecked(edi+4u));goto L939d8;
   L939d8: // LEA EBP,[ESI + 0xc]
    ebp=unchecked(esi+12u);goto L939db;
   L939db: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L939dd;
   L939dd: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L939e0;
   L939e0: // MOV EDX,dword ptr [EDI + 0x18]
    edx=R(unchecked(edi+24u));goto L939e3;
   L939e3: // MOV dword ptr [ESI + EAX*0x1],EDX
    W(unchecked(esi+eax*0x1u),edx);goto L939e6;
   L939e6: // MOV EAX,dword ptr [EDI + 0x1c]
    eax=R(unchecked(edi+28u));goto L939e9;
   L939e9: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L939ec;
   L939ec: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L939ee;
   L939ee: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L939f0;
   L939f0: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L939f3;
   L939f3: // MOV EDX,dword ptr [EDI + 0x20]
    edx=R(unchecked(edi+32u));goto L939f6;
   L939f6: // MOV dword ptr [ESI + EAX*0x1],EDX
    W(unchecked(esi+eax*0x1u),edx);goto L939f9;
   L939f9: // MOV EAX,dword ptr [EDI + 0x24]
    eax=R(unchecked(edi+36u));goto L939fc;
   L939fc: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L93a03;
   L93a03: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L93a0a;
   L93a0a: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L93a11;
   L93a11: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L93a14;
   L93a14: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L93a1a;
   L93a1a: // MOV dword ptr [ESI + 0x1c],0x0
    W(unchecked(esi+28u),0u);goto L93a21;
   L93a21: // MOV dword ptr [ESI + 0x20],0x0
    W(unchecked(esi+32u),0u);goto L93a28;
   L93a28: // MOV dword ptr [ESI + 0x24],0x0
    W(unchecked(esi+36u),0u);goto L93a2f;
   L93a2f: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93a32;
   L93a32: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93a35;
   L93a35: // LEA EAX,[ESI + 0x28]
    eax=unchecked(esi+40u);goto L93a38;
   L93a38: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93a3b;
   L93a3b: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L93a40;
   L93a40: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L93a46;
   L93a46: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93a49;
   L93a49: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93a4c;
   L93a4c: // MOV EAX,dword ptr [ESP + 0x48]
    eax=R(unchecked(esp+72u));goto L93a50;
   L93a50: // MOV dword ptr [ESI + 0x2c],EAX
    W(unchecked(esi+44u),eax);goto L93a53;
   L93a53: // MOV EAX,dword ptr [EBX + 0xfffffc8c]
    eax=R(unchecked(ebx+4294966412u));goto L93a59;
   L93a59: // ADD EAX,0xc
    a=eax;b=12u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93a5c;
   L93a5c: // MOV dword ptr [ESI + 0x30],EAX
    W(unchecked(esi+48u),eax);goto L93a5f;
   L93a5f: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L93a63;
   L93a63: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L93a65;
   L93a65: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L93a68;
   L93a68: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93a6a;
   L93a6a: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93a6d;
   L93a6d: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93a97;}goto L93a72;
   L93a72:return 0;
   L93a7a: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L93a7d;
   L93a7d: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93a7f;
   L93a7f: // MOV EDX,dword ptr [EDX + -0xc]
    edx=R(unchecked(edx+4294967284u));goto L93a82;
   L93a82: // MOV ECX,dword ptr [EDI + 0xc]
    ecx=R(unchecked(edi+12u));goto L93a85;
   L93a85: // MOV dword ptr [ESI + EDX*0x1],ECX
    W(unchecked(esi+edx*0x1u),ecx);goto L93a88;
   L93a88: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93a8f;
   L93a8f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93a92;
   L93a92: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L93a97: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L93a9a;
   L93a9a: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L93a9e;
   L93a9e: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93aa1;
   L93aa1: // CALL 0x00048b20
    eax=Call(0x48b20,R(unchecked(esp+0u)));goto L93aa6;
   L93aa6: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L93aaa;
   L93aaa: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L93aad;
   L93aad: // CALL 0x000489e0
    eax=Call(0x489e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93ab2;
   L93ab2: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L93ab6;
   L93ab6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93ab9;
   L93ab9: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructStringstreamMode(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x40u,owner);W(frame+0x44u,mode);goto L93ad2;
   L93ad2: // MOV dword ptr [ESP + 0x30],ESI
    W(unchecked(esp+48u),esi);goto L93ad6;
   L93ad6: // MOV ESI,dword ptr [ESP + 0x40]
    esi=R(unchecked(esp+64u));goto L93ada;
   L93ada: // MOV dword ptr [ESP + 0x34],EDI
    W(unchecked(esp+52u),edi);goto L93ade;
   L93ade: // MOV dword ptr [ESP + 0x38],EBP
    W(unchecked(esp+56u),ebp);goto L93ae2;
   L93ae2: // LEA EDI,[ESI + 0x34]
    edi=unchecked(esi+52u);goto L93ae5;
   L93ae5: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93ae8;
   L93ae8: // CALL 0x00048800
    eax=Call(0x48800,R(unchecked(esp+0u)));goto L93aed;
   L93aed: // MOV EBP,dword ptr [EBX + 0xfffffe80]
    ebp=R(unchecked(ebx+4294966912u));goto L93af3;
   L93af3: // MOV dword ptr [ESI + 0xa4],0x0
    W(unchecked(esi+164u),0u);goto L93afd;
   L93afd: // MOV byte ptr [ESI + 0xa8],0x0
    h.WriteByte(unchecked(esi+168u),unchecked((byte)(0u)));goto L93b04;
   L93b04: // MOV byte ptr [ESI + 0xa9],0x0
    h.WriteByte(unchecked(esi+169u),unchecked((byte)(0u)));goto L93b0b;
   L93b0b: // LEA EAX,[EBP + 0xc]
    eax=unchecked(ebp+12u);goto L93b0e;
   L93b0e: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93b10;
   L93b10: // LEA EAX,[EBP + 0x20]
    eax=unchecked(ebp+32u);goto L93b13;
   L93b13: // MOV dword ptr [ESI + 0xac],0x0
    W(unchecked(esi+172u),0u);goto L93b1d;
   L93b1d: // MOV dword ptr [ESI + 0xb0],0x0
    W(unchecked(esi+176u),0u);goto L93b27;
   L93b27: // MOV dword ptr [ESI + 0xb4],0x0
    W(unchecked(esi+180u),0u);goto L93b31;
   L93b31: // MOV dword ptr [ESI + 0xb8],0x0
    W(unchecked(esi+184u),0u);goto L93b3b;
   L93b3b: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L93b3e;
   L93b3e: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93b45;
   L93b45: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93b4d;
   L93b4d: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93b50;
   L93b50: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93c11;}goto L93b55;
   L93b55: // MOV EAX,dword ptr [EBX + 0xfffffdc4]
    eax=R(unchecked(ebx+4294966724u));goto L93b5b;
   L93b5b: // LEA EDX,[EAX + 0xc]
    edx=unchecked(eax+12u);goto L93b5e;
   L93b5e: // ADD EAX,0x20
    a=eax;b=32u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93b61;
   L93b61: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L93b64;
   L93b64: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L93b67;
   L93b67: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93b6f;
   L93b6f: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93b72;
   L93b72: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93c58;}goto L93b77;
   L93b77: // MOV EAX,dword ptr [EBX + 0xfffffa60]
    eax=R(unchecked(ebx+4294965856u));goto L93b7d;
   L93b7d: // LEA EBP,[ESI + 0xc]
    ebp=unchecked(esi+12u);goto L93b80;
   L93b80: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L93b87;
   L93b87: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L93b8e;
   L93b8e: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L93b95;
   L93b95: // LEA EDX,[EAX + 0xc]
    edx=unchecked(eax+12u);goto L93b98;
   L93b98: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93b9a;
   L93b9a: // LEA EDX,[EAX + 0x34]
    edx=unchecked(eax+52u);goto L93b9d;
   L93b9d: // ADD EAX,0x20
    a=eax;b=32u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93ba0;
   L93ba0: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L93ba3;
   L93ba3: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L93ba9;
   L93ba9: // MOV dword ptr [ESI + 0x34],EDX
    W(unchecked(esi+52u),edx);goto L93bac;
   L93bac: // MOV dword ptr [ESI + 0x1c],0x0
    W(unchecked(esi+28u),0u);goto L93bb3;
   L93bb3: // MOV dword ptr [ESI + 0x20],0x0
    W(unchecked(esi+32u),0u);goto L93bba;
   L93bba: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93bbd;
   L93bbd: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93bc0;
   L93bc0: // LEA EAX,[ESI + 0x28]
    eax=unchecked(esi+40u);goto L93bc3;
   L93bc3: // MOV dword ptr [ESI + 0x24],0x0
    W(unchecked(esi+36u),0u);goto L93bca;
   L93bca: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93bcd;
   L93bcd: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L93bd2;
   L93bd2: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L93bd8;
   L93bd8: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93bdb;
   L93bdb: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93bde;
   L93bde: // MOV EAX,dword ptr [ESP + 0x44]
    eax=R(unchecked(esp+68u));goto L93be2;
   L93be2: // MOV dword ptr [ESI + 0x2c],EAX
    W(unchecked(esi+44u),eax);goto L93be5;
   L93be5: // MOV EAX,dword ptr [EBX + 0xfffffc8c]
    eax=R(unchecked(ebx+4294966412u));goto L93beb;
   L93beb: // ADD EAX,0xc
    a=eax;b=12u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93bee;
   L93bee: // MOV dword ptr [ESI + 0x30],EAX
    W(unchecked(esi+48u),eax);goto L93bf1;
   L93bf1: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L93bf5;
   L93bf5: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93bf8;
   L93bf8: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93c2f;}goto L93bfd;
   L93bfd:return 0;
   L93c11: // MOV EBP,EAX
    ebp=eax;goto L93c13;
   L93c13: // MOV EAX,dword ptr [EBX + 0xfffffc40]
    eax=R(unchecked(ebx+4294966336u));goto L93c19;
   L93c19: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93c1c;
   L93c1c: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L93c1f;
   L93c1f: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93c22;
   L93c22: // CALL 0x00049b70
    eax=Call(0x49b70,R(unchecked(esp+0u)));goto L93c27;
   L93c27: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93c2a;
   L93c2a: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L93c2f: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L93c33;
   L93c33: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93c36;
   L93c36: // CALL 0x00048b20
    eax=Call(0x48b20,R(unchecked(esp+0u)));goto L93c3b;
   L93c3b: // MOV EDX,dword ptr [EBX + 0xfffffb30]
    edx=R(unchecked(ebx+4294966064u));goto L93c41;
   L93c41: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L93c44;
   L93c44: // ADD EDX,0x4
    a=edx;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L93c47;
   L93c47: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L93c4b;
   L93c4b: // CALL 0x000489e0
    eax=Call(0x489e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93c50;
   L93c50: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L93c54;
   L93c54: // MOV EBP,EAX
    ebp=eax;goto L93c56;
   L93c56: // JMP 0x00093c13
    goto L93c13;
   L93c58: // LEA EDX,[EBP + 0xc]
    edx=unchecked(ebp+12u);goto L93c5b;
   L93c5b: // MOV EBP,EAX
    ebp=eax;goto L93c5d;
   L93c5d: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93c5f;
   L93c5f: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93c66;
   L93c66: // JMP 0x00093c13
    goto L93c13;
  }
  public uint ConstructStringstreamBaseString(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;edi=vtt;W(frame+0x50u,owner);W(frame+0x54u,vtt);W(frame+0x58u,source);W(frame+0x5cu,mode);goto L93c8a;
   L93c8a: // MOV EAX,dword ptr [EDI + 0x8]
    eax=R(unchecked(edi+8u));goto L93c8d;
   L93c8d: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93c90;
   L93c90: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93c92;
   L93c92: // MOV ECX,dword ptr [EDI + 0xc]
    ecx=R(unchecked(edi+12u));goto L93c95;
   L93c95: // MOV dword ptr [ESI + EDX*0x1],ECX
    W(unchecked(esi+edx*0x1u),ecx);goto L93c98;
   L93c98: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93c9f;
   L93c9f: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93ca7;
   L93ca7: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L93caa;
   L93caa: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93cac;
   L93cac: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93caf;
   L93caf: // CALL 0x00049e90
    eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93cb4;
   L93cb4: // MOV EDX,dword ptr [EDI + 0x10]
    edx=R(unchecked(edi+16u));goto L93cb7;
   L93cb7: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L93cba;
   L93cba: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L93cbd;
   L93cbd: // ADD EAX,dword ptr [EDX + -0xc]
    a=eax;b=R(unchecked(edx+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93cc0;
   L93cc0: // MOV EDX,dword ptr [EDI + 0x14]
    edx=R(unchecked(edi+20u));goto L93cc3;
   L93cc3: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L93cc5;
   L93cc5: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93ccd;
   L93ccd: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93cd0;
   L93cd0: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93df8;}goto L93cd5;
   L93cd5: // MOV EAX,dword ptr [EDI + 0x4]
    eax=R(unchecked(edi+4u));goto L93cd8;
   L93cd8: // LEA EBP,[ESI + 0xc]
    ebp=unchecked(esi+12u);goto L93cdb;
   L93cdb: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93cdd;
   L93cdd: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L93ce0;
   L93ce0: // MOV EDX,dword ptr [EDI + 0x18]
    edx=R(unchecked(edi+24u));goto L93ce3;
   L93ce3: // MOV dword ptr [ESI + EAX*0x1],EDX
    W(unchecked(esi+eax*0x1u),edx);goto L93ce6;
   L93ce6: // MOV EAX,dword ptr [EDI + 0x1c]
    eax=R(unchecked(edi+28u));goto L93ce9;
   L93ce9: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L93cec;
   L93cec: // MOV EAX,dword ptr [EDI]
    eax=R(unchecked(edi));goto L93cee;
   L93cee: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93cf0;
   L93cf0: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L93cf3;
   L93cf3: // MOV EDX,dword ptr [EDI + 0x20]
    edx=R(unchecked(edi+32u));goto L93cf6;
   L93cf6: // MOV dword ptr [ESI + EAX*0x1],EDX
    W(unchecked(esi+eax*0x1u),edx);goto L93cf9;
   L93cf9: // MOV EAX,dword ptr [EDI + 0x24]
    eax=R(unchecked(edi+36u));goto L93cfc;
   L93cfc: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L93d03;
   L93d03: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L93d0a;
   L93d0a: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L93d11;
   L93d11: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L93d14;
   L93d14: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L93d1a;
   L93d1a: // MOV dword ptr [ESI + 0x1c],0x0
    W(unchecked(esi+28u),0u);goto L93d21;
   L93d21: // MOV dword ptr [ESI + 0x20],0x0
    W(unchecked(esi+32u),0u);goto L93d28;
   L93d28: // MOV dword ptr [ESI + 0x24],0x0
    W(unchecked(esi+36u),0u);goto L93d2f;
   L93d2f: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93d32;
   L93d32: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93d35;
   L93d35: // LEA EAX,[ESI + 0x28]
    eax=unchecked(esi+40u);goto L93d38;
   L93d38: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93d3b;
   L93d3b: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L93d40;
   L93d40: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L93d46;
   L93d46: // LEA EDX,[ESI + 0x30]
    edx=unchecked(esi+48u);goto L93d49;
   L93d49: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L93d4d;
   L93d4d: // LEA EDX,[ESP + 0x2f]
    edx=unchecked(esp+47u);goto L93d51;
   L93d51: // MOV dword ptr [ESI + 0x2c],0x0
    W(unchecked(esi+44u),0u);goto L93d58;
   L93d58: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93d5b;
   L93d5b: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93d5e;
   L93d5e: // MOV EAX,dword ptr [ESP + 0x58]
    eax=R(unchecked(esp+88u));goto L93d62;
   L93d62: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto L93d64;
   L93d64: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L93d68;
   L93d68: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93d6b;
   L93d6b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93d6f;
   L93d6f: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L93d73;
   L93d73: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L93d77;
   L93d77: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93d7a;
   L93d7a: // CALL 0x00048d30
    try{eax=Call(0x48d30,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93dcf;}goto L93d7f;
   L93d7f: // MOV EDX,dword ptr [ESP + 0x5c]
    edx=R(unchecked(esp+92u));goto L93d83;
   L93d83: // MOV dword ptr [ESI + 0x2c],EDX
    W(unchecked(esi+44u),edx);goto L93d86;
   L93d86: // AND EDX,0x3
    a=edx;b=3u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L93d89;
   L93d89: // JZ 0x00093dc8
    if(zero)goto L93dc8;goto L93d8b;
   L93d8b: // MOV EAX,dword ptr [ESI + 0x30]
    eax=R(unchecked(esi+48u));goto L93d8e;
   L93d8e: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93d91;
   L93d91: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L93d95;
   L93d95: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L93d9d;
   L93d9d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93da1;
   L93da1: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93da4;
   L93da4: // CALL 0x00048480
    try{eax=Call(0x48480,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93e29;}goto L93da9;
   L93da9: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L93dad;
   L93dad: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L93daf;
   L93daf: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L93db2;
   L93db2: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93db4;
   L93db4: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93db7;
   L93db7: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93e15;}goto L93dbc;
   L93dbc:return 0;
   L93dc8: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L93dca;
   L93dca: // MOV EAX,dword ptr [ESI + 0x30]
    eax=R(unchecked(esi+48u));goto L93dcd;
   L93dcd: // JMP 0x00093d91
    goto L93d91;
   L93dcf: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93dd2;
   L93dd2: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L93dd6;
   L93dd6: // CALL 0x00049ae0
    eax=Call(0x49ae0,R(unchecked(esp+0u)));goto L93ddb;
   L93ddb: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L93ddf;
   L93ddf: // MOV EBP,EAX
    ebp=eax;goto L93de1;
   L93de1: // ADD EDI,0x4
    a=edi;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edi=v;goto L93de4;
   L93de4: // MOV dword ptr [ESP + 0x4],EDI
    W(unchecked(esp+4u),edi);goto L93de8;
   L93de8: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L93deb;
   L93deb: // CALL 0x000489e0
    eax=Call(0x489e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93df0;
   L93df0: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93df3;
   L93df3: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L93df8: // MOV EDX,dword ptr [EDI + 0x8]
    edx=R(unchecked(edi+8u));goto L93dfb;
   L93dfb: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93dfd;
   L93dfd: // MOV EDX,dword ptr [EDX + -0xc]
    edx=R(unchecked(edx+4294967284u));goto L93e00;
   L93e00: // MOV ECX,dword ptr [EDI + 0xc]
    ecx=R(unchecked(edi+12u));goto L93e03;
   L93e03: // MOV dword ptr [ESI + EDX*0x1],ECX
    W(unchecked(esi+edx*0x1u),ecx);goto L93e06;
   L93e06: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93e0d;
   L93e0d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93e10;
   L93e10: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L93e15: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93e18;
   L93e18: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L93e1c;
   L93e1c: // CALL 0x00048b20
    eax=Call(0x48b20,R(unchecked(esp+0u)));goto L93e21;
   L93e21: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L93e25;
   L93e25: // MOV EBP,EAX
    ebp=eax;goto L93e27;
   L93e27: // JMP 0x00093de1
    goto L93de1;
   L93e29: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L93e2d;
   L93e2d: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L93e31;
   L93e31: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L93e34;
   L93e34: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L93e39;
   L93e39: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L93e3d;
   L93e3d: // JMP 0x00093dcf
    goto L93dcf;
  }
  public uint ConstructStringstreamString(uint frame,uint owner,uint vtt,uint source,uint mode)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x50u,owner);W(frame+0x54u,source);W(frame+0x58u,mode);goto L93e52;
   L93e52: // MOV ESI,dword ptr [ESP + 0x50]
    esi=R(unchecked(esp+80u));goto L93e56;
   L93e56: // LEA EDI,[ESI + 0x34]
    edi=unchecked(esi+52u);goto L93e59;
   L93e59: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93e5c;
   L93e5c: // CALL 0x00048800
    eax=Call(0x48800,R(unchecked(esp+0u)));goto L93e61;
   L93e61: // MOV EBP,dword ptr [EBX + 0xfffffe80]
    ebp=R(unchecked(ebx+4294966912u));goto L93e67;
   L93e67: // MOV dword ptr [ESI + 0xa4],0x0
    W(unchecked(esi+164u),0u);goto L93e71;
   L93e71: // MOV byte ptr [ESI + 0xa8],0x0
    h.WriteByte(unchecked(esi+168u),unchecked((byte)(0u)));goto L93e78;
   L93e78: // MOV byte ptr [ESI + 0xa9],0x0
    h.WriteByte(unchecked(esi+169u),unchecked((byte)(0u)));goto L93e7f;
   L93e7f: // LEA EAX,[EBP + 0xc]
    eax=unchecked(ebp+12u);goto L93e82;
   L93e82: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L93e84;
   L93e84: // LEA EAX,[EBP + 0x20]
    eax=unchecked(ebp+32u);goto L93e87;
   L93e87: // MOV dword ptr [ESI + 0xac],0x0
    W(unchecked(esi+172u),0u);goto L93e91;
   L93e91: // MOV dword ptr [ESI + 0xb0],0x0
    W(unchecked(esi+176u),0u);goto L93e9b;
   L93e9b: // MOV dword ptr [ESI + 0xb4],0x0
    W(unchecked(esi+180u),0u);goto L93ea5;
   L93ea5: // MOV dword ptr [ESI + 0xb8],0x0
    W(unchecked(esi+184u),0u);goto L93eaf;
   L93eaf: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L93eb2;
   L93eb2: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L93eb9;
   L93eb9: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93ec1;
   L93ec1: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93ec4;
   L93ec4: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L94022;}goto L93ec9;
   L93ec9: // MOV EAX,dword ptr [EBX + 0xfffffdc4]
    eax=R(unchecked(ebx+4294966724u));goto L93ecf;
   L93ecf: // LEA EDX,[EAX + 0xc]
    edx=unchecked(eax+12u);goto L93ed2;
   L93ed2: // ADD EAX,0x20
    a=eax;b=32u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93ed5;
   L93ed5: // MOV dword ptr [ESI + 0x8],EDX
    W(unchecked(esi+8u),edx);goto L93ed8;
   L93ed8: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L93edb;
   L93edb: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L93ee3;
   L93ee3: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93ee6;
   L93ee6: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L94012;}goto L93eeb;
   L93eeb: // MOV EAX,dword ptr [EBX + 0xfffffa60]
    eax=R(unchecked(ebx+4294965856u));goto L93ef1;
   L93ef1: // LEA EBP,[ESI + 0xc]
    ebp=unchecked(esi+12u);goto L93ef4;
   L93ef4: // MOV dword ptr [ESI + 0x10],0x0
    W(unchecked(esi+16u),0u);goto L93efb;
   L93efb: // MOV dword ptr [ESI + 0x14],0x0
    W(unchecked(esi+20u),0u);goto L93f02;
   L93f02: // MOV dword ptr [ESI + 0x18],0x0
    W(unchecked(esi+24u),0u);goto L93f09;
   L93f09: // LEA EDX,[EAX + 0xc]
    edx=unchecked(eax+12u);goto L93f0c;
   L93f0c: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93f0e;
   L93f0e: // LEA EDX,[EAX + 0x34]
    edx=unchecked(eax+52u);goto L93f11;
   L93f11: // ADD EAX,0x20
    a=eax;b=32u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93f14;
   L93f14: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L93f17;
   L93f17: // MOV EAX,dword ptr [EBX + 0xfffffda8]
    eax=R(unchecked(ebx+4294966696u));goto L93f1d;
   L93f1d: // MOV dword ptr [ESI + 0x34],EDX
    W(unchecked(esi+52u),edx);goto L93f20;
   L93f20: // MOV dword ptr [ESI + 0x1c],0x0
    W(unchecked(esi+28u),0u);goto L93f27;
   L93f27: // MOV dword ptr [ESI + 0x20],0x0
    W(unchecked(esi+32u),0u);goto L93f2e;
   L93f2e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93f31;
   L93f31: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93f34;
   L93f34: // LEA EAX,[ESI + 0x28]
    eax=unchecked(esi+40u);goto L93f37;
   L93f37: // MOV dword ptr [ESI + 0x24],0x0
    W(unchecked(esi+36u),0u);goto L93f3e;
   L93f3e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93f41;
   L93f41: // CALL 0x00048bc0
    eax=Call(0x48bc0,R(unchecked(esp+0u)));goto L93f46;
   L93f46: // MOV EAX,dword ptr [EBX + 0xfffffbf8]
    eax=R(unchecked(ebx+4294966264u));goto L93f4c;
   L93f4c: // LEA EDX,[ESI + 0x30]
    edx=unchecked(esi+48u);goto L93f4f;
   L93f4f: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L93f53;
   L93f53: // LEA EDX,[ESP + 0x2f]
    edx=unchecked(esp+47u);goto L93f57;
   L93f57: // MOV dword ptr [ESI + 0x2c],0x0
    W(unchecked(esi+44u),0u);goto L93f5e;
   L93f5e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93f61;
   L93f61: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L93f64;
   L93f64: // MOV EAX,dword ptr [ESP + 0x54]
    eax=R(unchecked(esp+84u));goto L93f68;
   L93f68: // MOV EAX,dword ptr [EAX]
    eax=R(unchecked(eax));goto L93f6a;
   L93f6a: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L93f6e;
   L93f6e: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93f71;
   L93f71: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93f75;
   L93f75: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L93f79;
   L93f79: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L93f7d;
   L93f7d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L93f80;
   L93f80: // CALL 0x00048d30
    try{eax=Call(0x48d30,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L93fcf;}goto L93f85;
   L93f85: // MOV EDX,dword ptr [ESP + 0x58]
    edx=R(unchecked(esp+88u));goto L93f89;
   L93f89: // MOV dword ptr [ESI + 0x2c],EDX
    W(unchecked(esi+44u),edx);goto L93f8c;
   L93f8c: // AND EDX,0x3
    a=edx;b=3u;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L93f8f;
   L93f8f: // JZ 0x00093fc8
    if(zero)goto L93fc8;goto L93f91;
   L93f91: // MOV EAX,dword ptr [ESI + 0x30]
    eax=R(unchecked(esi+48u));goto L93f94;
   L93f94: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L93f97;
   L93f97: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L93f9b;
   L93f9b: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L93fa3;
   L93fa3: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93fa7;
   L93fa7: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93faa;
   L93faa: // CALL 0x00048480
    try{eax=Call(0x48480,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9403a;}goto L93faf;
   L93faf: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L93fb3;
   L93fb3: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L93fb6;
   L93fb6: // CALL 0x00049e90
    try{eax=Call(0x49e90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L94026;}goto L93fbb;
   L93fbb:return 0;
   L93fc8: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L93fca;
   L93fca: // MOV EAX,dword ptr [ESI + 0x30]
    eax=R(unchecked(esi+48u));goto L93fcd;
   L93fcd: // JMP 0x00093f97
    goto L93f97;
   L93fcf: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L93fd2;
   L93fd2: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L93fd6;
   L93fd6: // CALL 0x00049ae0
    eax=Call(0x49ae0,R(unchecked(esp+0u)));goto L93fdb;
   L93fdb: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L93fdf;
   L93fdf: // MOV EBP,EAX
    ebp=eax;goto L93fe1;
   L93fe1: // MOV EAX,dword ptr [EBX + 0xfffffb30]
    eax=R(unchecked(ebx+4294966064u));goto L93fe7;
   L93fe7: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L93fea;
   L93fea: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93fed;
   L93fed: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93ff1;
   L93ff1: // CALL 0x000489e0
    eax=Call(0x489e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93ff6;
   L93ff6: // MOV EAX,dword ptr [EBX + 0xfffffc40]
    eax=R(unchecked(ebx+4294966336u));goto L93ffc;
   L93ffc: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93fff;
   L93fff: // MOV dword ptr [ESI + 0x34],EAX
    W(unchecked(esi+52u),eax);goto L94002;
   L94002: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94005;
   L94005: // CALL 0x00049b70
    eax=Call(0x49b70,R(unchecked(esp+0u)));goto L9400a;
   L9400a: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9400d;
   L9400d: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94012: // LEA EDX,[EBP + 0xc]
    edx=unchecked(ebp+12u);goto L94015;
   L94015: // MOV EBP,EAX
    ebp=eax;goto L94017;
   L94017: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L94019;
   L94019: // MOV dword ptr [ESI + 0x4],0x0
    W(unchecked(esi+4u),0u);goto L94020;
   L94020: // JMP 0x00093ff6
    goto L93ff6;
   L94022: // MOV EBP,EAX
    ebp=eax;goto L94024;
   L94024: // JMP 0x00093ff6
    goto L93ff6;
   L94026: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L94029;
   L94029: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L9402d;
   L9402d: // CALL 0x00048b20
    eax=Call(0x48b20,R(unchecked(esp+0u)));goto L94032;
   L94032: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L94036;
   L94036: // MOV EBP,EAX
    ebp=eax;goto L94038;
   L94038: // JMP 0x00093fe1
    goto L93fe1;
   L9403a: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L9403e;
   L9403e: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L94042;
   L94042: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94045;
   L94045: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L9404a;
   L9404a: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L9404e;
   L9404e: // JMP 0x00093fcf
    goto L93fcf;
  }
 }
}
