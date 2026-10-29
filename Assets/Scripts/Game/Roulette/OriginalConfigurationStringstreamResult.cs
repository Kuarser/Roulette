using System;
namespace RouletteDeluxe
{
 // Physical packaged char stringbuf, stringstream and ostringstream str getters. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Result is the original hidden string return owner. Range construction, assignment, COW disposal and unwind remain required imports.
 public sealed class OriginalConfigurationStringstreamResult
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
  public OriginalConfigurationStringstreamResult(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint GetStringbuf(uint frame,uint owner,uint result)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;esi=result;W(frame+0x40u,result);W(frame+0x44u,owner);goto L91c76;
   L91c76: // MOV EDI,dword ptr [EBX + 0xfffffc8c]
    edi=R(unchecked(ebx+4294966412u));goto L91c7c;
   L91c7c: // LEA EDX,[EDI + 0xc]
    edx=unchecked(edi+12u);goto L91c7f;
   L91c7f: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L91c81;
   L91c81: // MOV EDX,dword ptr [EAX + 0x14]
    edx=R(unchecked(eax+20u));goto L91c84;
   L91c84: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L91c86;
   L91c86: // JZ 0x00091d50
    if(zero)goto L91d50;goto L91c8c;
   L91c8c: // MOV ECX,dword ptr [EAX + 0xc]
    ecx=R(unchecked(eax+12u));goto L91c8f;
   L91c8f: // CMP EDX,ECX
    a=edx;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91c91;
   L91c91: // JA 0x00091ce8
    if(!carry&&!zero)goto L91ce8;goto L91c93;
   L91c93: // LEA EDX,[ESP + 0x1d]
    edx=unchecked(esp+29u);goto L91c97;
   L91c97: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L91c9b;
   L91c9b: // LEA EBP,[ESP + 0x18]
    ebp=unchecked(esp+24u);goto L91c9f;
   L91c9f: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto L91ca3;
   L91ca3: // MOV EAX,dword ptr [EAX + 0x10]
    eax=R(unchecked(eax+16u));goto L91ca6;
   L91ca6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L91ca9;
   L91ca9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91cad;
   L91cad: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L91dbf;}goto L91cb2;
   L91cb2: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L91cb6;
   L91cb6: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L91cb9;
   L91cb9: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L91dbd;}goto L91cbe;
   L91cbe: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L91cc2;
   L91cc2: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L91cc5;
   L91cc5: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91cc7;
   L91cc7: // JNZ 0x00091d64
    if(!zero)goto L91d64;goto L91ccd;
   L91ccd:return esi;
   L91ce8: // LEA ECX,[ESP + 0x1c]
    ecx=unchecked(esp+28u);goto L91cec;
   L91cec: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L91cf0;
   L91cf0: // LEA EBP,[ESP + 0x14]
    ebp=unchecked(esp+20u);goto L91cf4;
   L91cf4: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L91cf8;
   L91cf8: // MOV EAX,dword ptr [EAX + 0x10]
    eax=R(unchecked(eax+16u));goto L91cfb;
   L91cfb: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L91cfe;
   L91cfe: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91d02;
   L91d02: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L91d92;}goto L91d07;
   L91d07: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L91d0b;
   L91d0b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L91d0e;
   L91d0e: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L91da4;}goto L91d13;
   L91d13: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L91d17;
   L91d17: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L91d1a;
   L91d1a: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91d1c;
   L91d1c: // JZ 0x00091ccd
    if(zero)goto L91ccd;goto L91d1e;
   L91d1e: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91d25;
   L91d25: // JZ 0x00091dc4
    if(zero)goto L91dc4;goto L91d2b;
   L91d2b: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L91d2e;
   L91d2e: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L91d33;
   L91d33: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L91d35;
   L91d35: // JG 0x00091ccd
    if(!zero&&sign==overflow)goto L91ccd;goto L91d37;
   L91d37: // LEA EAX,[ESP + 0x1e]
    eax=unchecked(esp+30u);goto L91d3b;
   L91d3b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91d3f;
   L91d3f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L91d42;
   L91d42: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L91d47;
   L91d47: // JMP 0x00091ccd
    goto L91ccd;
   L91d50: // ADD EAX,0x24
    a=eax;b=36u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L91d53;
   L91d53: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91d57;
   L91d57: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L91d5a;
   L91d5a: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L91dc2;}goto L91d5f;
   L91d5f: // JMP 0x00091ccd
    goto L91ccd;
   L91d64: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L91d6b;
   L91d6b: // JZ 0x00091db2
    if(zero)goto L91db2;goto L91d6d;
   L91d6d: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L91d70;
   L91d70: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L91d75;
   L91d75: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L91d77;
   L91d77: // JG 0x00091ccd
    if(!zero&&sign==overflow)goto L91ccd;goto L91d7d;
   L91d7d: // LEA EAX,[ESP + 0x1f]
    eax=unchecked(esp+31u);goto L91d81;
   L91d81: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L91d85;
   L91d85: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L91d88;
   L91d88: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L91d8d;
   L91d8d: // JMP 0x00091ccd
    goto L91ccd;
   L91d92: // MOV EDI,EAX
    edi=eax;goto L91d94;
   L91d94: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L91d97;
   L91d97: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L91d9c;
   L91d9c: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L91d9f;
   L91d9f: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L91da4: // MOV EDI,EAX
    edi=eax;goto L91da6;
   L91da6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L91da9;
   L91da9: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L91dae;
   L91dae: // MOV EAX,EDI
    eax=edi;goto L91db0;
   L91db0: // JMP 0x00091d92
    goto L91d92;
   L91db2: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L91db5;
   L91db5: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L91db8;
   L91db8: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L91dbb;
   L91dbb: // JMP 0x00091d75
    goto L91d75;
   L91dbd: // JMP 0x00091da4
    goto L91da4;
   L91dbf: // NOP
    goto L91dc0;
   L91dc0: // JMP 0x00091d92
    goto L91d92;
   L91dc2: // JMP 0x00091d92
    goto L91d92;
   L91dc4: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L91dc7;
   L91dc7: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L91dca;
   L91dca: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L91dcd;
   L91dcd: // JMP 0x00091d33
    goto L91d33;
  }
  public uint GetStringstream(uint frame,uint owner,uint result)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;esi=result;W(frame+0x40u,result);W(frame+0x44u,owner);goto L94186;
   L94186: // MOV EDI,dword ptr [EBX + 0xfffffc8c]
    edi=R(unchecked(ebx+4294966412u));goto L9418c;
   L9418c: // LEA EDX,[EDI + 0xc]
    edx=unchecked(edi+12u);goto L9418f;
   L9418f: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L94191;
   L94191: // MOV EDX,dword ptr [EAX + 0x20]
    edx=R(unchecked(eax+32u));goto L94194;
   L94194: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94196;
   L94196: // JZ 0x00094260
    if(zero)goto L94260;goto L9419c;
   L9419c: // MOV ECX,dword ptr [EAX + 0x18]
    ecx=R(unchecked(eax+24u));goto L9419f;
   L9419f: // CMP EDX,ECX
    a=edx;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L941a1;
   L941a1: // JA 0x000941f8
    if(!carry&&!zero)goto L941f8;goto L941a3;
   L941a3: // LEA EDX,[ESP + 0x1d]
    edx=unchecked(esp+29u);goto L941a7;
   L941a7: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L941ab;
   L941ab: // LEA EBP,[ESP + 0x18]
    ebp=unchecked(esp+24u);goto L941af;
   L941af: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto L941b3;
   L941b3: // MOV EAX,dword ptr [EAX + 0x1c]
    eax=R(unchecked(eax+28u));goto L941b6;
   L941b6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L941b9;
   L941b9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L941bd;
   L941bd: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L942cf;}goto L941c2;
   L941c2: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L941c6;
   L941c6: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L941c9;
   L941c9: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L942cd;}goto L941ce;
   L941ce: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L941d2;
   L941d2: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L941d5;
   L941d5: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L941d7;
   L941d7: // JNZ 0x00094274
    if(!zero)goto L94274;goto L941dd;
   L941dd:return esi;
   L941f8: // LEA ECX,[ESP + 0x1c]
    ecx=unchecked(esp+28u);goto L941fc;
   L941fc: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L94200;
   L94200: // LEA EBP,[ESP + 0x14]
    ebp=unchecked(esp+20u);goto L94204;
   L94204: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L94208;
   L94208: // MOV EAX,dword ptr [EAX + 0x1c]
    eax=R(unchecked(eax+28u));goto L9420b;
   L9420b: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9420e;
   L9420e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94212;
   L94212: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L942a2;}goto L94217;
   L94217: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L9421b;
   L9421b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9421e;
   L9421e: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L942b4;}goto L94223;
   L94223: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L94227;
   L94227: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9422a;
   L9422a: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9422c;
   L9422c: // JZ 0x000941dd
    if(zero)goto L941dd;goto L9422e;
   L9422e: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94235;
   L94235: // JZ 0x000942d4
    if(zero)goto L942d4;goto L9423b;
   L9423b: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L9423e;
   L9423e: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94243;
   L94243: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94245;
   L94245: // JG 0x000941dd
    if(!zero&&sign==overflow)goto L941dd;goto L94247;
   L94247: // LEA EAX,[ESP + 0x1e]
    eax=unchecked(esp+30u);goto L9424b;
   L9424b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9424f;
   L9424f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94252;
   L94252: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94257;
   L94257: // JMP 0x000941dd
    goto L941dd;
   L94260: // ADD EAX,0x30
    a=eax;b=48u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L94263;
   L94263: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94267;
   L94267: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9426a;
   L9426a: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L942d2;}goto L9426f;
   L9426f: // JMP 0x000941dd
    goto L941dd;
   L94274: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9427b;
   L9427b: // JZ 0x000942c2
    if(zero)goto L942c2;goto L9427d;
   L9427d: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L94280;
   L94280: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L94285;
   L94285: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94287;
   L94287: // JG 0x000941dd
    if(!zero&&sign==overflow)goto L941dd;goto L9428d;
   L9428d: // LEA EAX,[ESP + 0x1f]
    eax=unchecked(esp+31u);goto L94291;
   L94291: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L94295;
   L94295: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94298;
   L94298: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9429d;
   L9429d: // JMP 0x000941dd
    goto L941dd;
   L942a2: // MOV EDI,EAX
    edi=eax;goto L942a4;
   L942a4: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L942a7;
   L942a7: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L942ac;
   L942ac: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L942af;
   L942af: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L942b4: // MOV EDI,EAX
    edi=eax;goto L942b6;
   L942b6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L942b9;
   L942b9: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L942be;
   L942be: // MOV EAX,EDI
    eax=edi;goto L942c0;
   L942c0: // JMP 0x000942a2
    goto L942a2;
   L942c2: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L942c5;
   L942c5: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L942c8;
   L942c8: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L942cb;
   L942cb: // JMP 0x00094285
    goto L94285;
   L942cd: // JMP 0x000942b4
    goto L942b4;
   L942cf: // NOP
    goto L942d0;
   L942d0: // JMP 0x000942a2
    goto L942a2;
   L942d2: // JMP 0x000942a2
    goto L942a2;
   L942d4: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L942d7;
   L942d7: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L942da;
   L942da: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L942dd;
   L942dd: // JMP 0x00094243
    goto L94243;
  }
  public uint GetOstringstream(uint frame,uint owner,uint result)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=owner;esi=result;W(frame+0x40u,result);W(frame+0x44u,owner);goto L93786;
   L93786: // MOV EDI,dword ptr [EBX + 0xfffffc8c]
    edi=R(unchecked(ebx+4294966412u));goto L9378c;
   L9378c: // LEA EDX,[EDI + 0xc]
    edx=unchecked(edi+12u);goto L9378f;
   L9378f: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L93791;
   L93791: // MOV EDX,dword ptr [EAX + 0x18]
    edx=R(unchecked(eax+24u));goto L93794;
   L93794: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L93796;
   L93796: // JZ 0x00093860
    if(zero)goto L93860;goto L9379c;
   L9379c: // MOV ECX,dword ptr [EAX + 0x10]
    ecx=R(unchecked(eax+16u));goto L9379f;
   L9379f: // CMP EDX,ECX
    a=edx;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L937a1;
   L937a1: // JA 0x000937f8
    if(!carry&&!zero)goto L937f8;goto L937a3;
   L937a3: // LEA EDX,[ESP + 0x1d]
    edx=unchecked(esp+29u);goto L937a7;
   L937a7: // MOV dword ptr [ESP + 0xc],EDX
    W(unchecked(esp+12u),edx);goto L937ab;
   L937ab: // LEA EBP,[ESP + 0x18]
    ebp=unchecked(esp+24u);goto L937af;
   L937af: // MOV dword ptr [ESP + 0x8],ECX
    W(unchecked(esp+8u),ecx);goto L937b3;
   L937b3: // MOV EAX,dword ptr [EAX + 0x14]
    eax=R(unchecked(eax+20u));goto L937b6;
   L937b6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L937b9;
   L937b9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L937bd;
   L937bd: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L938cf;}goto L937c2;
   L937c2: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L937c6;
   L937c6: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L937c9;
   L937c9: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L938cd;}goto L937ce;
   L937ce: // MOV EAX,dword ptr [ESP + 0x18]
    eax=R(unchecked(esp+24u));goto L937d2;
   L937d2: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L937d5;
   L937d5: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L937d7;
   L937d7: // JNZ 0x00093874
    if(!zero)goto L93874;goto L937dd;
   L937dd:return esi;
   L937f8: // LEA ECX,[ESP + 0x1c]
    ecx=unchecked(esp+28u);goto L937fc;
   L937fc: // MOV dword ptr [ESP + 0xc],ECX
    W(unchecked(esp+12u),ecx);goto L93800;
   L93800: // LEA EBP,[ESP + 0x14]
    ebp=unchecked(esp+20u);goto L93804;
   L93804: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L93808;
   L93808: // MOV EAX,dword ptr [EAX + 0x14]
    eax=R(unchecked(eax+20u));goto L9380b;
   L9380b: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L9380e;
   L9380e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93812;
   L93812: // CALL 0x00049840
    try{eax=Call(0x49840,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L938a2;}goto L93817;
   L93817: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L9381b;
   L9381b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9381e;
   L9381e: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L938b4;}goto L93823;
   L93823: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L93827;
   L93827: // LEA EDX,[EAX + -0xc]
    edx=unchecked(eax+4294967284u);goto L9382a;
   L9382a: // CMP EDX,EDI
    a=edx;b=edi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9382c;
   L9382c: // JZ 0x000937dd
    if(zero)goto L937dd;goto L9382e;
   L9382e: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L93835;
   L93835: // JZ 0x000938d4
    if(zero)goto L938d4;goto L9383b;
   L9383b: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L9383e;
   L9383e: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L93843;
   L93843: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L93845;
   L93845: // JG 0x000937dd
    if(!zero&&sign==overflow)goto L937dd;goto L93847;
   L93847: // LEA EAX,[ESP + 0x1e]
    eax=unchecked(esp+30u);goto L9384b;
   L9384b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9384f;
   L9384f: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L93852;
   L93852: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L93857;
   L93857: // JMP 0x000937dd
    goto L937dd;
   L93860: // ADD EAX,0x28
    a=eax;b=40u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L93863;
   L93863: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93867;
   L93867: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9386a;
   L9386a: // CALL 0x00049f60
    try{eax=Call(0x49f60,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L938d2;}goto L9386f;
   L9386f: // JMP 0x000937dd
    goto L937dd;
   L93874: // CMP dword ptr [EBX + 0xfffffe9c],0x0
    a=R(unchecked(ebx+4294966940u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9387b;
   L9387b: // JZ 0x000938c2
    if(zero)goto L938c2;goto L9387d;
   L9387d: // OR ECX,0xffffffff
    a=ecx;b=4294967295u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ecx=v;goto L93880;
   L93880: // XADD.LOCK dword ptr [EAX + -0x4],ECX
    a=R(unchecked(eax+4294967292u));b=ecx;v=unchecked((uint)h.AtomicAdd(unchecked(eax+4294967292u),unchecked((int)b)));ecx=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L93885;
   L93885: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L93887;
   L93887: // JG 0x000937dd
    if(!zero&&sign==overflow)goto L937dd;goto L9388d;
   L9388d: // LEA EAX,[ESP + 0x1f]
    eax=unchecked(esp+31u);goto L93891;
   L93891: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L93895;
   L93895: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L93898;
   L93898: // CALL 0x000481a0
    eax=Call(0x481a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9389d;
   L9389d: // JMP 0x000937dd
    goto L937dd;
   L938a2: // MOV EDI,EAX
    edi=eax;goto L938a4;
   L938a4: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L938a7;
   L938a7: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L938ac;
   L938ac: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L938af;
   L938af: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L938b4: // MOV EDI,EAX
    edi=eax;goto L938b6;
   L938b6: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L938b9;
   L938b9: // CALL 0x00048d10
    eax=Call(0x48d10,R(unchecked(esp+0u)));goto L938be;
   L938be: // MOV EAX,EDI
    eax=edi;goto L938c0;
   L938c0: // JMP 0x000938a2
    goto L938a2;
   L938c2: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L938c5;
   L938c5: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L938c8;
   L938c8: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L938cb;
   L938cb: // JMP 0x00093885
    goto L93885;
   L938cd: // JMP 0x000938b4
    goto L938b4;
   L938cf: // NOP
    goto L938d0;
   L938d0: // JMP 0x000938a2
    goto L938a2;
   L938d2: // JMP 0x000938a2
    goto L938a2;
   L938d4: // MOV ECX,dword ptr [EAX + -0x4]
    ecx=R(unchecked(eax+4294967292u));goto L938d7;
   L938d7: // LEA EDI,[ECX + -0x1]
    edi=unchecked(ecx+4294967295u);goto L938da;
   L938da: // MOV dword ptr [EAX + -0x4],EDI
    W(unchecked(eax+4294967292u),edi);goto L938dd;
   L938dd: // JMP 0x00093843
    goto L93843;
  }
 }
}
