using System;
namespace RouletteDeluxe
{
 // Physical simple-option uninitialized move 09499720. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; begin/end/destination are normalized register inputs.
 public sealed class OriginalConfigurationSimpleOptionMove
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,uint> WriteWord;
   public Func<uint,int,int> AtomicAdd;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationSimpleOptionMove(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Move(uint frame,uint begin,uint end,uint destination)
  {
   uint eax=begin,ebx=begin,esi=destination,edi=0,ecx=destination,edx=end,ebp=end,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L949972d;
   L949972d: // CMP EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949972f;
   L949972f: // MOV dword ptr [ESP + 0x1c],ECX
    W(unchecked(esp+28u),ecx);goto L9499733;
   L9499733: // JZ 0x0949977c
    if(zero)goto L949977c;goto L9499735;
   L9499735: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9499738;
   L9499738: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L949973a;
   L949973a: // JZ 0x09499772
    if(zero)goto L9499772;goto L949973c;
   L949973c: // MOV dword ptr [ESP + 0x4],EBX
    W(unchecked(esp+4u),ebx);goto L9499740;
   L9499740: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L9499743;
   L9499743: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L9499786;}goto L9499748;
   L9499748: // LEA EDI,[ESI + 0x4]
    edi=unchecked(esi+4u);goto L949974b;
   L949974b: // LEA EAX,[EBX + 0x4]
    eax=unchecked(ebx+4u);goto L949974e;
   L949974e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499752;
   L9499752: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L9499755;
   L9499755: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94997c6;}goto L949975a;
   L949975a: // LEA EAX,[EBX + 0x8]
    eax=unchecked(ebx+8u);goto L949975d;
   L949975d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L9499761;
   L9499761: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L9499764;
   L9499764: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499767;
   L9499767: // CALL 0x080ac124
    try{eax=Call(0x80ac124,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94997b0;}goto L949976c;
   L949976c: // MOV EAX,dword ptr [EBX + 0xc]
    eax=R(unchecked(ebx+12u));goto L949976f;
   L949976f: // MOV dword ptr [ESI + 0xc],EAX
    W(unchecked(esi+12u),eax);goto L9499772;
   L9499772: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9499775;
   L9499775: // ADD ESI,0x10
    a=esi;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;esi=v;goto L9499778;
   L9499778: // CMP EBX,EBP
    a=ebx;b=ebp;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L949977a;
   L949977a: // JNZ 0x09499738
    if(!zero)goto L9499738;goto L949977c;
   L949977c:return esi;
   L9499786: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L9499789;
   L9499789: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L949978e;
   L949978e: // CMP dword ptr [ESP + 0x1c],ESI
    a=R(unchecked(esp+28u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9499792;
   L9499792: // JZ 0x094997ab
    if(zero)goto L94997ab;goto L9499794;
   L9499794: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L9499798;
   L9499798: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L949979b;
   L949979b: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L94997a0;
   L94997a0: // ADD dword ptr [ESP + 0x1c],0x10
    a=R(unchecked(esp+28u));b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;W(unchecked(esp+28u),v);goto L94997a5;
   L94997a5: // CMP dword ptr [ESP + 0x1c],ESI
    a=R(unchecked(esp+28u));b=esi;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94997a9;
   L94997a9: // JNZ 0x09499794
    if(!zero)goto L9499794;goto L94997ab;
   L94997ab: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning operation returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94997ca;}
   L94997b0: // MOV EBX,EAX
    ebx=eax;goto L94997b2;
   L94997b2: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94997b5;
   L94997b5: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94997ba;
   L94997ba: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94997bd;
   L94997bd: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94997c2;
   L94997c2: // MOV EAX,EBX
    eax=ebx;goto L94997c4;
   L94997c4: // JMP 0x09499786
    goto L9499786;
   L94997c6: // MOV EBX,EAX
    ebx=eax;goto L94997c8;
   L94997c8: // JMP 0x094997ba
    goto L94997ba;
   L94997ca: // MOV EBX,EAX
    ebx=eax;goto L94997cc;
   L94997cc: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L94997d0;
   L94997d0: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L94997d5;
   L94997d5: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94997d8;
   L94997d8: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
