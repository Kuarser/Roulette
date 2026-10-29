using System;
namespace RouletteDeluxe
{
 // Physical packaged basic_ios state/cache and ostream flush control. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. State is a state-mask or locale address as appropriate. Imported facet, exception and virtual sync owners remain required.
 public sealed class OriginalConfigurationBasicIosControl
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
  public OriginalConfigurationBasicIosControl(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Clear(uint frame,uint owner,uint state)
  {
   uint eax=owner,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x24u),state);goto L716b3;
   L716b3: // MOV EDX,dword ptr [ESP + 0x24]
    edx=R(unchecked(esp+36u));goto L716b7;
   L716b7: // MOV ECX,dword ptr [EAX + 0x78]
    ecx=R(unchecked(eax+120u));goto L716ba;
   L716ba: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L716bc;
   L716bc: // JZ 0x000716d0
    if(zero)goto L716d0;goto L716be;
   L716be: // TEST dword ptr [EAX + 0x10],EDX
    a=R(unchecked(eax+16u));b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L716c1;
   L716c1: // MOV dword ptr [EAX + 0x14],EDX
    W(unchecked(eax+20u),edx);goto L716c4;
   L716c4: // JNZ 0x000716d5
    if(!zero)goto L716d5;goto L716c6;
   L716c6:return 0;
   L716d0: // OR EDX,0x1
    a=edx;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L716d3;
   L716d3: // JMP 0x000716be
    goto L716be;
   L716d5: // LEA EAX,[EBX + 0xfffd9af3]
    eax=unchecked(ebx+4294810355u);goto L716db;
   L716db: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L716de;
   L716de: // CALL 0x000491b0
    Call(0x491b0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint SetState(uint frame,uint owner,uint state)
  {
   uint eax=owner,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=state;goto L71737;
   L71737: // OR dword ptr [EAX + 0x14],EDX
    a=R(unchecked(eax+20u));b=edx;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;W(unchecked(eax+20u),v);goto L7173a;
   L7173a: // TEST dword ptr [EAX + 0x10],EDX
    a=R(unchecked(eax+16u));b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7173d;
   L7173d: // JNZ 0x00071744
    if(!zero)goto L71744;goto L7173f;
   L7173f:return 0;
   L71744: // CALL 0x0004a410
    Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint Flush(uint frame,uint owner,uint state)
  {
   uint eax=0,ebx=0xe5ff4,esi=owner,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L8c16a;
   L8c16a: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8c16c;
   L8c16c: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8c16f;
   L8c16f: // MOV EAX,dword ptr [ESI + EAX*0x1 + 0x78]
    eax=R(unchecked(esi+eax*0x1u+120u));goto L8c173;
   L8c173: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L8c175;
   L8c175: // JZ 0x0008c19d
    if(zero)goto L8c19d;goto L8c177;
   L8c177: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L8c179;
   L8c179: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8c17c;
   L8c17c: // CALL dword ptr [EDX + 0x18]
    try{eax=Call(R(unchecked(edx+24u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8c1ab;}goto L8c17f;
   L8c17f: // CMP EAX,-0x1
    a=eax;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L8c182;
   L8c182: // JNZ 0x0008c19d
    if(!zero)goto L8c19d;goto L8c184;
   L8c184: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8c186;
   L8c186: // MOV EDX,dword ptr [EAX + -0xc]
    edx=R(unchecked(eax+4294967284u));goto L8c189;
   L8c189: // ADD EDX,ESI
    a=edx;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L8c18b;
   L8c18b: // MOV EAX,dword ptr [EDX + 0x14]
    eax=R(unchecked(edx+20u));goto L8c18e;
   L8c18e: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L8c191;
   L8c191: // OR EAX,0x1
    a=eax;b=1u;v=unchecked(a|b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L8c194;
   L8c194: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L8c198;
   L8c198: // CALL 0x0004a2f0
    eax=Call(0x4a2f0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L8c19d;
   L8c19d: // MOV EAX,ESI
    eax=esi;goto L8c19f;
   L8c19f:return eax;
   L8c1ab: // SUB EDX,0x1
    a=edx;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;edx=v;goto L8c1ae;
   L8c1ae: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8c1b1;
   L8c1b1: // JZ 0x0008c1d6
    if(zero)goto L8c1d6;goto L8c1b3;
   L8c1b3: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L8c1b8;
   L8c1b8: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L8c1c0;
   L8c1c0: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8c1c2;
   L8c1c2: // MOV EAX,dword ptr [EAX + -0xc]
    eax=R(unchecked(eax+4294967284u));goto L8c1c5;
   L8c1c5: // ADD EAX,ESI
    a=eax;b=esi;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L8c1c7;
   L8c1c7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L8c1ca;
   L8c1ca: // CALL 0x00049080
    try{eax=Call(0x49080,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8c1f5;}goto L8c1cf;
   L8c1cf: // CALL 0x000497e0
    eax=Call(0x497e0);goto L8c1d4;
   L8c1d4: // JMP 0x0008c19d
    goto L8c19d;
   L8c1d6: // CALL 0x00048150
    eax=Call(0x48150,R(unchecked(esp+0u)));goto L8c1db;
   L8c1db: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L8c1e3;
   L8c1e3: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L8c1e5;
   L8c1e5: // ADD ESI,dword ptr [EAX + -0xc]
    a=esi;b=R(unchecked(eax+4294967284u));v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L8c1e8;
   L8c1e8: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8c1eb;
   L8c1eb: // CALL 0x00049080
    try{eax=Call(0x49080,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8c204;}goto L8c1f0;
   L8c1f0: // CALL 0x0004a410
    try{Call(0x4a410);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L8c204;}
   L8c1f5: // MOV ESI,EAX
    esi=eax;goto L8c1f7;
   L8c1f7: // CALL 0x000497e0
    eax=Call(0x497e0);goto L8c1fc;
   L8c1fc: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8c1ff;
   L8c1ff: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L8c204: // MOV ESI,EAX
    esi=eax;goto L8c206;
   L8c206: // CALL 0x000497e0
    eax=Call(0x497e0);goto L8c20b;
   L8c20b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L8c20e;
   L8c20e: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint CacheLocale(uint frame,uint owner,uint state)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x20u),owner);W(unchecked(frame+0x24u),state);goto L71af2;
   L71af2: // MOV dword ptr [ESP + 0x14],ESI
    W(unchecked(esp+20u),esi);goto L71af6;
   L71af6: // MOV ESI,dword ptr [ESP + 0x24]
    esi=R(unchecked(esp+36u));goto L71afa;
   L71afa: // MOV dword ptr [ESP + 0x18],EDI
    W(unchecked(esp+24u),edi);goto L71afe;
   L71afe: // MOV EDI,dword ptr [ESP + 0x20]
    edi=R(unchecked(esp+32u));goto L71b02;
   L71b02: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b05;
   L71b05: // CALL 0x0004a6b0
    eax=Call(0x4a6b0,R(unchecked(esp+0u)));goto L71b0a;
   L71b0a: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L71b0c;
   L71b0c: // JZ 0x00071b5d
    if(zero)goto L71b5d;goto L71b0e;
   L71b0e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b11;
   L71b11: // CALL 0x00047fa0
    eax=Call(0x47fa0,R(unchecked(esp+0u)));goto L71b16;
   L71b16: // MOV dword ptr [EDI + 0x7c],EAX
    W(unchecked(edi+124u),eax);goto L71b19;
   L71b19: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b1c;
   L71b1c: // CALL 0x00049c50
    eax=Call(0x49c50,R(unchecked(esp+0u)));goto L71b21;
   L71b21: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L71b23;
   L71b23: // JZ 0x00071b72
    if(zero)goto L71b72;goto L71b25;
   L71b25: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b28;
   L71b28: // CALL 0x00048200
    eax=Call(0x48200,R(unchecked(esp+0u)));goto L71b2d;
   L71b2d: // MOV dword ptr [EDI + 0x80],EAX
    W(unchecked(edi+128u),eax);goto L71b33;
   L71b33: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b36;
   L71b36: // CALL 0x00047ea0
    eax=Call(0x47ea0,R(unchecked(esp+0u)));goto L71b3b;
   L71b3b: // TEST AL,AL
    a=(eax&255u);b=(eax&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L71b3d;
   L71b3d: // JZ 0x00071b66
    if(zero)goto L71b66;goto L71b3f;
   L71b3f: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L71b42;
   L71b42: // CALL 0x00048ca0
    eax=Call(0x48ca0,R(unchecked(esp+0u)));goto L71b47;
   L71b47: // MOV dword ptr [EDI + 0x84],EAX
    W(unchecked(edi+132u),eax);goto L71b4d;
   L71b4d:return 0;
   L71b5d: // MOV dword ptr [EDI + 0x7c],0x0
    W(unchecked(edi+124u),0u);goto L71b64;
   L71b64: // JMP 0x00071b19
    goto L71b19;
   L71b66: // MOV dword ptr [EDI + 0x84],0x0
    W(unchecked(edi+132u),0u);goto L71b70;
   L71b70: // JMP 0x00071b4d
    goto L71b4d;
   L71b72: // MOV dword ptr [EDI + 0x80],0x0
    W(unchecked(edi+128u),0u);goto L71b7c;
   L71b7c: // JMP 0x00071b33
    goto L71b33;
  }
 }
}
