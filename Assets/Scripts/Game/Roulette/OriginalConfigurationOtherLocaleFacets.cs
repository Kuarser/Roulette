using System;
namespace RouletteDeluxe
{
 // Physical packaged codecvt, wide ctype, messages and timepunct constructors. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Locale getters, wide/time initialization and base facet/exception owners remain required.
 public sealed class OriginalConfigurationOtherLocaleFacets
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
  public OriginalConfigurationOtherLocaleFacets(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ConstructCodecvtChar(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L50ae8;
   L50ae8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L50aec;
   L50aec: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L50af0;
   L50af0: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L50af2;
   L50af2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L50af5;
   L50af5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L50af8;
   L50af8: // MOV EAX,dword ptr [EBX + 0xfffffc2c]
    eax=R(unchecked(ebx+4294966316u));goto L50afe;
   L50afe: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L50b01;
   L50b01: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L50b03;
   L50b03: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L50b17;}goto L50b08;
   L50b08: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L50b0b;
   L50b0b:return 0;
   L50b17: // MOV EDX,dword ptr [EBX + 0xffffff3c]
    edx=R(unchecked(ebx+4294967100u));goto L50b1d;
   L50b1d: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L50b20;
   L50b20: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L50b22;
   L50b22: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L50b26;
   L50b26: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L50b29;
   L50b29: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L50b2e;
   L50b2e: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L50b32;
   L50b32: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L50b35;
   L50b35: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCodecvtWide(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L50ba8;
   L50ba8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L50bac;
   L50bac: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L50bb0;
   L50bb0: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L50bb2;
   L50bb2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L50bb5;
   L50bb5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L50bb8;
   L50bb8: // MOV EAX,dword ptr [EBX + 0xfffffb4c]
    eax=R(unchecked(ebx+4294966092u));goto L50bbe;
   L50bbe: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L50bc1;
   L50bc1: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L50bc3;
   L50bc3: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L50bd7;}goto L50bc8;
   L50bc8: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L50bcb;
   L50bcb:return 0;
   L50bd7: // MOV EDX,dword ptr [EBX + 0xfffffb50]
    edx=R(unchecked(ebx+4294966096u));goto L50bdd;
   L50bdd: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L50be0;
   L50be0: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L50be2;
   L50be2: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L50be6;
   L50be6: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L50be9;
   L50be9: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L50bee;
   L50bee: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L50bf2;
   L50bf2: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L50bf5;
   L50bf5: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructCtypeWide(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L55458;
   L55458: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L5545c;
   L5545c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L55460;
   L55460: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L55462;
   L55462: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L55465;
   L55465: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L55468;
   L55468: // MOV EAX,dword ptr [EBX + 0xfffffdb0]
    eax=R(unchecked(ebx+4294966704u));goto L5546e;
   L5546e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L55471;
   L55471: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L55473;
   L55473: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L55493;}goto L55478;
   L55478: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L5547b;
   L5547b: // MOV byte ptr [ESI + 0xc],0x0
    h.WriteByte(unchecked(esi+12u),unchecked((byte)(0u)));goto L5547f;
   L5547f: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L55482;
   L55482: // CALL 0x00048d40
    eax=Call(0x48d40,R(unchecked(esp+0u)));goto L55487;
   L55487:return 0;
   L55493: // MOV EDX,dword ptr [EBX + 0xfffffbe8]
    edx=R(unchecked(ebx+4294966248u));goto L55499;
   L55499: // ADD EDX,0x8
    a=edx;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L5549c;
   L5549c: // MOV dword ptr [ESI],EDX
    W(unchecked(esi),edx);goto L5549e;
   L5549e: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L554a2;
   L554a2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L554a5;
   L554a5: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L554aa;
   L554aa: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L554ae;
   L554ae: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L554b1;
   L554b1: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructMessagesChar(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L7ce68;
   L7ce68: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7ce6c;
   L7ce6c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7ce70;
   L7ce70: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ce72;
   L7ce72: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ce75;
   L7ce75: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7ce78;
   L7ce78: // MOV EAX,dword ptr [EBX + 0xffffffdc]
    eax=R(unchecked(ebx+4294967260u));goto L7ce7e;
   L7ce7e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7ce81;
   L7ce81: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7ce83;
   L7ce83: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7ce9f;}goto L7ce88;
   L7ce88: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L7ce8b;
   L7ce8b: // CALL 0x000488e0
    eax=Call(0x488e0);goto L7ce90;
   L7ce90: // MOV EBX,dword ptr [ESP + 0x24]
    ebx=R(unchecked(esp+36u));goto L7ce94;
   L7ce94: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L7ce97;
   L7ce97:return 0;
   L7ce9f: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7cea3;
   L7cea3: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7cea6;
   L7cea6: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7ceab;
   L7ceab: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7ceaf;
   L7ceaf: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7ceb2;
   L7ceb2: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructMessagesWide(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,references);W(frame+0x38u,references);goto L9f258;
   L9f258: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9f25c;
   L9f25c: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9f260;
   L9f260: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9f262;
   L9f262: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9f265;
   L9f265: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9f268;
   L9f268: // MOV EAX,dword ptr [EBX + 0xfffffdb4]
    eax=R(unchecked(ebx+4294966708u));goto L9f26e;
   L9f26e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9f271;
   L9f271: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9f273;
   L9f273: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9f28f;}goto L9f278;
   L9f278: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L9f27b;
   L9f27b: // CALL 0x000488e0
    eax=Call(0x488e0);goto L9f280;
   L9f280: // MOV EBX,dword ptr [ESP + 0x24]
    ebx=R(unchecked(esp+36u));goto L9f284;
   L9f284: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L9f287;
   L9f287:return 0;
   L9f28f: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9f293;
   L9f293: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9f296;
   L9f296: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9f29b;
   L9f29b: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9f29f;
   L9f29f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9f2a2;
   L9f2a2: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructTimepunctChar(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,cache);W(frame+0x38u,references);goto L7c4b8;
   L7c4b8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L7c4bc;
   L7c4bc: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L7c4c0;
   L7c4c0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7c4c2;
   L7c4c2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7c4c5;
   L7c4c5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L7c4c8;
   L7c4c8: // MOV EAX,dword ptr [EBX + 0xfffffc4c]
    eax=R(unchecked(ebx+4294966348u));goto L7c4ce;
   L7c4ce: // MOV dword ptr [ESI + 0xc],0x0
    W(unchecked(esi+12u),0u);goto L7c4d5;
   L7c4d5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L7c4d8;
   L7c4d8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L7c4da;
   L7c4da: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L7c4de;
   L7c4de: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L7c4e1;
   L7c4e1: // CALL 0x000488e0
    eax=Call(0x488e0);goto L7c4e6;
   L7c4e6: // MOV dword ptr [ESI + 0x10],EAX
    W(unchecked(esi+16u),eax);goto L7c4e9;
   L7c4e9: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L7c4f1;
   L7c4f1: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7c4f4;
   L7c4f4: // CALL 0x000499a0
    try{eax=Call(0x499a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L7c505;}goto L7c4f9;
   L7c4f9:return 0;
   L7c505: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L7c509;
   L7c509: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L7c50c;
   L7c50c: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L7c511;
   L7c511: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L7c515;
   L7c515: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7c518;
   L7c518: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint ConstructTimepunctWide(uint frame,uint owner,uint cache,uint references)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   edx=references;ecx=references;W(frame+0x30u,owner);W(frame+0x34u,cache);W(frame+0x38u,references);goto L9e8a8;
   L9e8a8: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L9e8ac;
   L9e8ac: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L9e8b0;
   L9e8b0: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9e8b2;
   L9e8b2: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L9e8b5;
   L9e8b5: // MOV dword ptr [ESI + 0x4],EAX
    W(unchecked(esi+4u),eax);goto L9e8b8;
   L9e8b8: // MOV EAX,dword ptr [EBX + 0xfffffae8]
    eax=R(unchecked(ebx+4294965992u));goto L9e8be;
   L9e8be: // MOV dword ptr [ESI + 0xc],0x0
    W(unchecked(esi+12u),0u);goto L9e8c5;
   L9e8c5: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L9e8c8;
   L9e8c8: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L9e8ca;
   L9e8ca: // MOV EAX,dword ptr [ESP + 0x34]
    eax=R(unchecked(esp+52u));goto L9e8ce;
   L9e8ce: // MOV dword ptr [ESI + 0x8],EAX
    W(unchecked(esi+8u),eax);goto L9e8d1;
   L9e8d1: // CALL 0x000488e0
    eax=Call(0x488e0);goto L9e8d6;
   L9e8d6: // MOV dword ptr [ESI + 0x10],EAX
    W(unchecked(esi+16u),eax);goto L9e8d9;
   L9e8d9: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L9e8e1;
   L9e8e1: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9e8e4;
   L9e8e4: // CALL 0x00049290
    try{eax=Call(0x49290,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L9e8f5;}goto L9e8e9;
   L9e8e9:return 0;
   L9e8f5: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L9e8f9;
   L9e8f9: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9e8fc;
   L9e8fc: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L9e901;
   L9e901: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9e905;
   L9e905: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9e908;
   L9e908: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
