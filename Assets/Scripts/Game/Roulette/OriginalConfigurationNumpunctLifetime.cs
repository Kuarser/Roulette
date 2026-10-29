using System;
namespace RouletteDeluxe
{
 // Physical packaged char/wchar numpunct facet and cache lifetime. Static C# labels
 // preserve original lifetime control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Imported initializer/destructor, heap and exception owners remain required.
 public sealed class OriginalConfigurationNumpunctLifetime
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
  public OriginalConfigurationNumpunctLifetime(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ConstructLocaleWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);goto L9dc18;
   L9dc18: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9dc1c;
   L9dc1c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9dc20;
   L9dc20: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9dc22;
   L9dc22: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9dc25;
   L9dc25: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9dc28;
   L9dc28: // MOV EAX,dword ptr [EBX + 0xfffffbe0]
    eax=R(unchecked(ebx+4294966240u));goto L9dc2e;
   L9dc2e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9dc35;
   L9dc35: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9dc38;
   L9dc38: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9dc3a;
   L9dc3a: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9dc3e;
   L9dc3e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9dc41;
   L9dc41: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9dc45;
   L9dc45: // CALL 0x000490e0
    try{eax=Call(0x490e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9dc56;}goto L9dc4a;
   L9dc4a:return 0;
   L9dc56: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9dc5a;
   L9dc5a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9dc5d;
   L9dc5d: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9dc62;
   L9dc62: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9dc66;
   L9dc66: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9dc69;
   L9dc69: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructDefaultChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L7b738;
   L7b738: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7b73c;
   L7b73c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7b740;
   L7b740: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7b742;
   L7b742: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7b745;
   L7b745: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7b748;
   L7b748: // MOV EAX,dword ptr [EBX + 0xfffffa58]
    eax=R(unchecked(ebx+4294965848u));goto L7b74e;
   L7b74e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7b755;
   L7b755: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7b758;
   L7b758: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7b75a;
   L7b75a: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7b762;
   L7b762: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b765;
   L7b765: // CALL 0x00048a90
    try{eax=Call(0x48a90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7b776;}goto L7b76a;
   L7b76a:return 0;
   L7b776: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7b77a;
   L7b77a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b77d;
   L7b77d: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7b782;
   L7b782: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7b786;
   L7b786: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7b789;
   L7b789: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCacheWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L9dd4b;
   L9dd4b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L9dd4f;
   L9dd4f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L9dd51;
   L9dd51: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9dd56;
   L9dd56: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L9dd59;
   L9dd59: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L9dd5c;
   L9dd5c: // MOV EDX,dword ptr [ECX + 0xffffffd8]
    edx=R(unchecked(ecx+4294967256u));goto L9dd62;
   L9dd62: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L9dd69;
   L9dd69: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L9dd70;
   L9dd70: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L9dd74;
   L9dd74: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L9dd77;
   L9dd77: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L9dd79;
   L9dd79: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L9dd80;
   L9dd80: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L9dd87;
   L9dd87: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L9dd8e;
   L9dd8e: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L9dd95;
   L9dd95: // MOV dword ptr [EAX + 0x24],0x0
    W(unchecked(eax+36u),0u);goto L9dd9c;
   L9dd9c: // MOV dword ptr [EAX + 0x28],0x0
    W(unchecked(eax+40u),0u);goto L9dda3;
   L9dda3: // MOV byte ptr [EAX + 0x124],0x0
    h.WriteByte(unchecked(eax+292u),unchecked((byte)(0u)));goto L9ddaa;
   L9ddaa:return 0;
  }
  public uint DeleteCacheChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L79c72;
   L79c72: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79c76;
   L79c76: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79c7a;
   L79c7a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79c7d;
   L79c7d: // CALL 0x0004a580
    eax=Call(0x4a580,R(unchecked(esp+0u)));goto L79c82;
   L79c82: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79c85;
   L79c85: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L79c8a;
   L79c8a:return 0;
  }
  public uint DestroyCacheChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L79c02;
   L79c02: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L79c06;
   L79c06: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L79c0a;
   L79c0a: // MOV EAX,dword ptr [EBX + 0xfffffde4]
    eax=R(unchecked(ebx+4294966756u));goto L79c10;
   L79c10: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L79c13;
   L79c13: // CMP byte ptr [ESI + 0x64],0x0
    a=B(unchecked(esi+100u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L79c17;
   L79c17: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L79c19;
   L79c19: // JZ 0x00079c48
    if(zero)goto L79c48;goto L79c1b;
   L79c1b: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L79c1e;
   L79c1e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79c20;
   L79c20: // JZ 0x00079c2a
    if(zero)goto L79c2a;goto L79c22;
   L79c22: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79c25;
   L79c25: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79c2a;
   L79c2a: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L79c2d;
   L79c2d: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79c2f;
   L79c2f: // JZ 0x00079c39
    if(zero)goto L79c39;goto L79c31;
   L79c31: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79c34;
   L79c34: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79c39;
   L79c39: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L79c3c;
   L79c3c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L79c3e;
   L79c3e: // JZ 0x00079c48
    if(zero)goto L79c48;goto L79c40;
   L79c40: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L79c43;
   L79c43: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L79c48;
   L79c48: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L79c4b;
   L79c4b: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L79c50;
   L79c50:return 0;
  }
  public uint ConstructLocaleChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);goto L7b818;
   L7b818: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7b81c;
   L7b81c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7b820;
   L7b820: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7b822;
   L7b822: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7b825;
   L7b825: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7b828;
   L7b828: // MOV EAX,dword ptr [EBX + 0xfffffa58]
    eax=R(unchecked(ebx+4294965848u));goto L7b82e;
   L7b82e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L7b835;
   L7b835: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7b838;
   L7b838: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7b83a;
   L7b83a: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7b83e;
   L7b83e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b841;
   L7b841: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7b845;
   L7b845: // CALL 0x00048a90
    try{eax=Call(0x48a90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7b856;}goto L7b84a;
   L7b84a:return 0;
   L7b856: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7b85a;
   L7b85a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b85d;
   L7b85d: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7b862;
   L7b862: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7b866;
   L7b866: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7b869;
   L7b869: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructDefaultWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L9db38;
   L9db38: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9db3c;
   L9db3c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9db40;
   L9db40: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9db42;
   L9db42: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9db45;
   L9db45: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9db48;
   L9db48: // MOV EAX,dword ptr [EBX + 0xfffffbe0]
    eax=R(unchecked(ebx+4294966240u));goto L9db4e;
   L9db4e: // MOV dword ptr [ESI + 0x8],0x0
    W(unchecked(esi+8u),0u);goto L9db55;
   L9db55: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9db58;
   L9db58: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9db5a;
   L9db5a: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9db62;
   L9db62: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9db65;
   L9db65: // CALL 0x000490e0
    try{eax=Call(0x490e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9db76;}goto L9db6a;
   L9db6a:return 0;
   L9db76: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9db7a;
   L9db7a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9db7d;
   L9db7d: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9db82;
   L9db82: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9db86;
   L9db86: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9db89;
   L9db89: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructSuppliedCacheWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);goto L9dba8;
   L9dba8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9dbac;
   L9dbac: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9dbb0;
   L9dbb0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9dbb2;
   L9dbb2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9dbb5;
   L9dbb5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9dbb8;
   L9dbb8: // MOV EAX,dword ptr [EBX + 0xfffffbe0]
    eax=R(unchecked(ebx+4294966240u));goto L9dbbe;
   L9dbbe: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9dbc1;
   L9dbc1: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9dbc3;
   L9dbc3: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9dbc7;
   L9dbc7: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L9dbca;
   L9dbca: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9dbd2;
   L9dbd2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9dbd5;
   L9dbd5: // CALL 0x000490e0
    try{eax=Call(0x490e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9dbe6;}goto L9dbda;
   L9dbda:return 0;
   L9dbe6: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9dbea;
   L9dbea: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9dbed;
   L9dbed: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9dbf2;
   L9dbf2: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9dbf6;
   L9dbf6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9dbf9;
   L9dbf9: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteFacetChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L66342;
   L66342: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L66346;
   L66346: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L6634a;
   L6634a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6634d;
   L6634d: // CALL 0x00049670
    eax=Call(0x49670,R(unchecked(esp+0u)));goto L66352;
   L66352: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66355;
   L66355: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L6635a;
   L6635a:return 0;
  }
  public uint DeleteCacheWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L9c052;
   L9c052: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9c056;
   L9c056: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9c05a;
   L9c05a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c05d;
   L9c05d: // CALL 0x00048a60
    eax=Call(0x48a60,R(unchecked(esp+0u)));goto L9c062;
   L9c062: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c065;
   L9c065: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L9c06a;
   L9c06a:return 0;
  }
  public uint DestroyFacetChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x30u,owner);goto L662c2;
   L662c2: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L662c6;
   L662c6: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L662ca;
   L662ca: // MOV EAX,dword ptr [EBX + 0xfffffa58]
    eax=R(unchecked(ebx+4294965848u));goto L662d0;
   L662d0: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L662d3;
   L662d3: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L662d5;
   L662d5: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L662d8;
   L662d8: // MOV EDX,dword ptr [EAX + 0xc]
    edx=R(unchecked(eax+12u));goto L662db;
   L662db: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L662dd;
   L662dd: // JZ 0x000662f1
    if(zero)goto L662f1;goto L662df;
   L662df: // MOV EDX,dword ptr [EAX + 0x8]
    edx=R(unchecked(eax+8u));goto L662e2;
   L662e2: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L662e4;
   L662e4: // JZ 0x000662f1
    if(zero)goto L662f1;goto L662e6;
   L662e6: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L662e9;
   L662e9: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L662ee;
   L662ee: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L662f1;
   L662f1: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L662f3;
   L662f3: // JZ 0x000662fd
    if(zero)goto L662fd;goto L662f5;
   L662f5: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L662f7;
   L662f7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L662fa;
   L662fa: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L66311;}goto L662fd;
   L662fd: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66300;
   L66300: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L66305;
   L66305:return 0;
   L66311: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L66315;
   L66315: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66318;
   L66318: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L6631d;
   L6631d: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L66321;
   L66321: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L66324;
   L66324: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DestroyCacheWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L9bfe2;
   L9bfe2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L9bfe6;
   L9bfe6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L9bfea;
   L9bfea: // MOV EAX,dword ptr [EBX + 0xffffffd8]
    eax=R(unchecked(ebx+4294967256u));goto L9bff0;
   L9bff0: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9bff3;
   L9bff3: // CMP byte ptr [ESI + 0x124],0x0
    a=B(unchecked(esi+292u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9bffa;
   L9bffa: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9bffc;
   L9bffc: // JZ 0x0009c02b
    if(zero)goto L9c02b;goto L9bffe;
   L9bffe: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L9c001;
   L9c001: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c003;
   L9c003: // JZ 0x0009c00d
    if(zero)goto L9c00d;goto L9c005;
   L9c005: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c008;
   L9c008: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c00d;
   L9c00d: // MOV EAX,dword ptr [ESI + 0x14]
    eax=R(unchecked(esi+20u));goto L9c010;
   L9c010: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c012;
   L9c012: // JZ 0x0009c01c
    if(zero)goto L9c01c;goto L9c014;
   L9c014: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c017;
   L9c017: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c01c;
   L9c01c: // MOV EAX,dword ptr [ESI + 0x1c]
    eax=R(unchecked(esi+28u));goto L9c01f;
   L9c01f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9c021;
   L9c021: // JZ 0x0009c02b
    if(zero)goto L9c02b;goto L9c023;
   L9c023: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9c026;
   L9c026: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L9c02b;
   L9c02b: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9c02e;
   L9c02e: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9c033;
   L9c033:return 0;
  }
  public uint DeleteFacetWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L66622;
   L66622: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L66626;
   L66626: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L6662a;
   L6662a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L6662d;
   L6662d: // CALL 0x00048830
    eax=Call(0x48830,R(unchecked(esp+0u)));goto L66632;
   L66632: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L66635;
   L66635: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L6663a;
   L6663a:return 0;
  }
  public uint DestroyFacetWide(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x30u,owner);goto L665a2;
   L665a2: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L665a6;
   L665a6: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L665aa;
   L665aa: // MOV EAX,dword ptr [EBX + 0xfffffbe0]
    eax=R(unchecked(ebx+4294966240u));goto L665b0;
   L665b0: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L665b3;
   L665b3: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L665b5;
   L665b5: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L665b8;
   L665b8: // MOV EDX,dword ptr [EAX + 0xc]
    edx=R(unchecked(eax+12u));goto L665bb;
   L665bb: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L665bd;
   L665bd: // JZ 0x000665d1
    if(zero)goto L665d1;goto L665bf;
   L665bf: // MOV EDX,dword ptr [EAX + 0x8]
    edx=R(unchecked(eax+8u));goto L665c2;
   L665c2: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L665c4;
   L665c4: // JZ 0x000665d1
    if(zero)goto L665d1;goto L665c6;
   L665c6: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L665c9;
   L665c9: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L665ce;
   L665ce: // MOV EAX,dword ptr [ESI + 0x8]
    eax=R(unchecked(esi+8u));goto L665d1;
   L665d1: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L665d3;
   L665d3: // JZ 0x000665dd
    if(zero)goto L665dd;goto L665d5;
   L665d5: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L665d7;
   L665d7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L665da;
   L665da: // CALL dword ptr [EDX + 0x4]
    try{eax=Call(R(unchecked(edx+4u)),R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L665f1;}goto L665dd;
   L665dd: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L665e0;
   L665e0: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L665e5;
   L665e5:return 0;
   L665f1: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L665f5;
   L665f5: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L665f8;
   L665f8: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L665fd;
   L665fd: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L66601;
   L66601: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L66604;
   L66604: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCacheChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=0xe5ff4u;W(frame+4u,owner);W(frame+8u,references);goto L7b94b;
   L7b94b: // MOV EAX,dword ptr [ESP + 0x4]
    eax=R(unchecked(esp+4u));goto L7b94f;
   L7b94f: // XOR EDX,EDX
    a=edx;b=edx;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;edx=v;goto L7b951;
   L7b951: // CMP dword ptr [ESP + 0x8],0x0
    a=R(unchecked(esp+8u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7b956;
   L7b956: // SETNZ DL
    edx=(edx&0xffffff00u)|((zero?0u:1u)&255u);goto L7b959;
   L7b959: // MOV dword ptr [EAX + 0x4],EDX
    W(unchecked(eax+4u),edx);goto L7b95c;
   L7b95c: // MOV EDX,dword ptr [ECX + 0xfffffde4]
    edx=R(unchecked(ecx+4294966756u));goto L7b962;
   L7b962: // MOV dword ptr [EAX + 0x8],0x0
    W(unchecked(eax+8u),0u);goto L7b969;
   L7b969: // MOV dword ptr [EAX + 0xc],0x0
    W(unchecked(eax+12u),0u);goto L7b970;
   L7b970: // MOV byte ptr [EAX + 0x10],0x0
    h.WriteByte(unchecked(eax+16u),unchecked((byte)(0u)));goto L7b974;
   L7b974: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L7b977;
   L7b977: // MOV dword ptr [EAX],EDX
    W(unchecked(eax),edx);goto L7b979;
   L7b979: // MOV dword ptr [EAX + 0x14],0x0
    W(unchecked(eax+20u),0u);goto L7b980;
   L7b980: // MOV dword ptr [EAX + 0x18],0x0
    W(unchecked(eax+24u),0u);goto L7b987;
   L7b987: // MOV dword ptr [EAX + 0x1c],0x0
    W(unchecked(eax+28u),0u);goto L7b98e;
   L7b98e: // MOV dword ptr [EAX + 0x20],0x0
    W(unchecked(eax+32u),0u);goto L7b995;
   L7b995: // MOV byte ptr [EAX + 0x24],0x0
    h.WriteByte(unchecked(eax+36u),unchecked((byte)(0u)));goto L7b999;
   L7b999: // MOV byte ptr [EAX + 0x25],0x0
    h.WriteByte(unchecked(eax+37u),unchecked((byte)(0u)));goto L7b99d;
   L7b99d: // MOV byte ptr [EAX + 0x64],0x0
    h.WriteByte(unchecked(eax+100u),unchecked((byte)(0u)));goto L7b9a1;
   L7b9a1:return 0;
  }
  public uint ConstructSuppliedCacheChar(uint frame,uint owner,uint argument,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   ecx=references;W(frame+0x30u,owner);W(frame+0x34u,argument);W(frame+0x38u,references);goto L7b7a8;
   L7b7a8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7b7ac;
   L7b7ac: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7b7b0;
   L7b7b0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7b7b2;
   L7b7b2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7b7b5;
   L7b7b5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7b7b8;
   L7b7b8: // MOV EAX,dword ptr [EBX + 0xfffffa58]
    eax=R(unchecked(ebx+4294965848u));goto L7b7be;
   L7b7be: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7b7c1;
   L7b7c1: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7b7c3;
   L7b7c3: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7b7c7;
   L7b7c7: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L7b7ca;
   L7b7ca: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7b7d2;
   L7b7d2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b7d5;
   L7b7d5: // CALL 0x00048a90
    try{eax=Call(0x48a90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7b7e6;}goto L7b7da;
   L7b7da:return 0;
   L7b7e6: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7b7ea;
   L7b7ea: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7b7ed;
   L7b7ed: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7b7f2;
   L7b7f2: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7b7f6;
   L7b7f6: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7b7f9;
   L7b7f9: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
