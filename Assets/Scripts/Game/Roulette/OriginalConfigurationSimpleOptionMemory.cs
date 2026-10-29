using System;
namespace RouletteDeluxe
{
 // Physical simple-option ReHash 094b4900 and range constructor 094b44a0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Range frame is ESP after prologue; owner/begin/end are normalized register inputs.
 public sealed class OriginalConfigurationSimpleOptionMemory
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
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationSimpleOptionMemory(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint ConstructRange(uint frame,uint owner,uint begin,uint end,uint allocator)
  {
   uint eax=owner,ebx=0x1111,esi=0x2222,edi=0x3333,ecx=end,edx=begin,ebp=0x4444,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x40u),allocator);goto L94b44a3;
   L94b44a3: // CMP EDX,ECX
    a=edx;b=ecx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b44a5;
   L94b44a5: // MOV dword ptr [ESP + 0x2c],EBX
    W(unchecked(esp+44u),ebx);goto L94b44a9;
   L94b44a9: // MOV EBX,EAX
    ebx=eax;goto L94b44ab;
   L94b44ab: // MOV EAX,dword ptr [ESP + 0x40]
    eax=R(unchecked(esp+64u));goto L94b44af;
   L94b44af: // MOV dword ptr [ESP + 0x30],ESI
    W(unchecked(esp+48u),esi);goto L94b44b3;
   L94b44b3: // MOV ESI,EDX
    esi=edx;goto L94b44b5;
   L94b44b5: // MOV dword ptr [ESP + 0x34],EDI
    W(unchecked(esp+52u),edi);goto L94b44b9;
   L94b44b9: // MOV dword ptr [ESP + 0x38],EBP
    W(unchecked(esp+56u),ebp);goto L94b44bd;
   L94b44bd: // JZ 0x094b4528
    if(zero)goto L94b4528;goto L94b44bf;
   L94b44bf: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b44c1;
   L94b44c1: // JZ 0x094b4530
    if(zero)goto L94b4530;goto L94b44c3;
   L94b44c3: // MOV EBP,ECX
    ebp=ecx;goto L94b44c5;
   L94b44c5: // SUB EBP,EDX
    a=ebp;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebp=v;goto L94b44c7;
   L94b44c7: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L94b44cb;
   L94b44cb: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L94b44d3;
   L94b44d3: // MOV dword ptr [ESP],EBP
    W(unchecked(esp),ebp);goto L94b44d6;
   L94b44d6: // CALL 0x080ae694
    eax=Call(0x80ae694,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b44db;
   L94b44db: // CMP EBP,0x1
    a=ebp;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b44de;
   L94b44de: // MOV EDX,EAX
    edx=eax;goto L94b44e0;
   L94b44e0: // LEA EDI,[EAX + 0xc]
    edi=unchecked(eax+12u);goto L94b44e3;
   L94b44e3: // JZ 0x094b4520
    if(zero)goto L94b4520;goto L94b44e5;
   L94b44e5: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L94b44e9;
   L94b44e9: // MOV dword ptr [ESP + 0x8],EBP
    W(unchecked(esp+8u),ebp);goto L94b44ed;
   L94b44ed: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L94b44f1;
   L94b44f1: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b44f4;
   L94b44f4: // CALL 0x080ae3e4
    eax=Call(0x80ae3e4,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b44f9;
   L94b44f9: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L94b44fd;
   L94b44fd: // CMP EDX,0x9b482a0
    a=edx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L94b4503;
   L94b4503: // JNZ 0x094b4561
    if(!zero)goto L94b4561;goto L94b4505;
   L94b4505: // MOV dword ptr [EBX],EDI
    W(unchecked(ebx),edi);goto L94b4507;
   L94b4507:return 0;
   L94b4520: // MOVZX EAX,byte ptr [ESI]
    eax=B(unchecked(esi));goto L94b4523;
   L94b4523: // MOV byte ptr [EDX + 0xc],AL
    h.WriteByte(unchecked(edx+12u),unchecked((byte)((eax&255u))));goto L94b4526;
   L94b4526: // JMP 0x094b44fd
    goto L94b44fd;
   L94b4528: // MOV EDI,0x9b482ac
    edi=162824876u;goto L94b452d;
   L94b452d: // JMP 0x094b4505
    goto L94b4505;
   L94b4530: // TEST ECX,ECX
    a=ecx;b=ecx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L94b4532;
   L94b4532: // JZ 0x094b4540
    if(zero)goto L94b4540;goto L94b4534;
   L94b4534: // MOV dword ptr [ESP],0x969c258
    W(unchecked(esp),157925976u);goto L94b453b;
   L94b453b: // CALL 0x080b0064
    Call(0x80b0064,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L94b4540: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L94b4544;
   L94b4544: // XOR EBP,EBP
    a=ebp;b=ebp;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;ebp=v;goto L94b4546;
   L94b4546: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L94b454e;
   L94b454e: // MOV dword ptr [ESP],0x0
    W(unchecked(esp),0u);goto L94b4555;
   L94b4555: // CALL 0x080ae694
    eax=Call(0x80ae694,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L94b455a;
   L94b455a: // MOV EDX,EAX
    edx=eax;goto L94b455c;
   L94b455c: // LEA EDI,[EAX + 0xc]
    edi=unchecked(eax+12u);goto L94b455f;
   L94b455f: // JMP 0x094b44e5
    goto L94b44e5;
   L94b4561: // MOV dword ptr [EDX + 0x8],0x0
    W(unchecked(edx+8u),0u);goto L94b4568;
   L94b4568: // MOV dword ptr [EDX],EBP
    W(unchecked(edx),ebp);goto L94b456a;
   L94b456a: // MOV byte ptr [EDI + EBP*0x1],0x0
    h.WriteByte(unchecked(edi+ebp*0x1u),unchecked((byte)(0u)));goto L94b456e;
   L94b456e: // JMP 0x094b4505
    goto L94b4505;
  }
  // Exact observable memory order of ReHash, including intermediate stores.
  public void ReHash(uint owner)
  {
   uint value=0,data=R(owner);W(unchecked(owner+12u),0);uint length=R(unchecked(data-12u));
   for(uint i=0;i<length;++i){uint rotated=unchecked((value<<3)|(value>>29));W(unchecked(owner+12u),rotated);value=rotated^unchecked((uint)(int)(sbyte)B(unchecked(data+i)));W(unchecked(owner+12u),value);}
   data=R(unchecked(owner+8u));length=R(unchecked(data-12u));
   for(uint i=0;i<length;++i){uint rotated=unchecked((value<<3)|(value>>29));W(unchecked(owner+12u),rotated);value=rotated^unchecked((uint)(int)(sbyte)B(unchecked(data+i)));W(unchecked(owner+12u),value);}
  }
 }
}
