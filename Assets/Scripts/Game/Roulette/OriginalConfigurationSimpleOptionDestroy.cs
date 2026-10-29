using System;
namespace RouletteDeluxe
{
 // Physical simple-option range destruction 09498d90 and D1 081d7ac0. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; begin/end/destination are normalized register inputs.
 public sealed class OriginalConfigurationSimpleOptionDestroy
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
  public OriginalConfigurationSimpleOptionDestroy(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint DestroyRange(uint frame,uint begin,uint end)
  {
   uint eax=begin,ebx=0,esi=0,edi=0,ecx=0,edx=end,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L9498d97;
   L9498d97: // CMP EDX,EAX
    a=edx;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498d99;
   L9498d99: // MOV dword ptr [ESP + 0x18],EDX
    W(unchecked(esp+24u),edx);goto L9498d9d;
   L9498d9d: // JZ 0x09498dee
    if(zero)goto L9498dee;goto L9498d9f;
   L9498d9f: // LEA EDX,[EAX + 0x4]
    edx=unchecked(eax+4u);goto L9498da2;
   L9498da2: // MOV EBX,EAX
    ebx=eax;goto L9498da4;
   L9498da4: // LEA EDI,[EAX + 0x8]
    edi=unchecked(eax+8u);goto L9498da7;
   L9498da7: // MOV dword ptr [ESP + 0x1c],EDX
    W(unchecked(esp+28u),edx);goto L9498dab;
   L9498dab: // NOP
    goto L9498dac;
   L9498dac: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L9498db0;
   L9498db0: // MOV ESI,EBX
    esi=ebx;goto L9498db2;
   L9498db2: // SUB ESI,EAX
    a=esi;b=eax;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;esi=v;goto L9498db4;
   L9498db4: // MOV EDX,dword ptr [ESI + EDI*0x1]
    edx=R(unchecked(esi+edi*0x1u));goto L9498db7;
   L9498db7: // LEA ECX,[EDX + -0xc]
    ecx=unchecked(edx+4294967284u);goto L9498dba;
   L9498dba: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498dc0;
   L9498dc0: // JNZ 0x09498df6
    if(!zero)goto L9498df6;goto L9498dc2;
   L9498dc2: // MOV ECX,dword ptr [ESP + 0x1c]
    ecx=R(unchecked(esp+28u));goto L9498dc6;
   L9498dc6: // MOV EDX,dword ptr [ESI + ECX*0x1]
    edx=R(unchecked(esi+ecx*0x1u));goto L9498dc9;
   L9498dc9: // LEA ECX,[EDX + -0xc]
    ecx=unchecked(edx+4294967284u);goto L9498dcc;
   L9498dcc: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498dd2;
   L9498dd2: // JNZ 0x09498e64
    if(!zero)goto L9498e64;goto L9498dd8;
   L9498dd8: // MOV EDX,dword ptr [EBX]
    edx=R(unchecked(ebx));goto L9498dda;
   L9498dda: // LEA ECX,[EDX + -0xc]
    ecx=unchecked(edx+4294967284u);goto L9498ddd;
   L9498ddd: // CMP ECX,0x9b482a0
    a=ecx;b=162824864u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498de3;
   L9498de3: // JNZ 0x09498e33
    if(!zero)goto L9498e33;goto L9498de5;
   L9498de5: // ADD EBX,0x10
    a=ebx;b=16u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;ebx=v;goto L9498de8;
   L9498de8: // CMP EBX,dword ptr [ESP + 0x18]
    a=ebx;b=R(unchecked(esp+24u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L9498dec;
   L9498dec: // JNZ 0x09498db0
    if(!zero)goto L9498db0;goto L9498dee;
   L9498dee:return 0;
   L9498df6: // MOV EBP,0x80b0f04
    ebp=134942468u;goto L9498dfb;
   L9498dfb: // TEST EBP,EBP
    a=ebp;b=ebp;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498dfd;
   L9498dfd: // JZ 0x09498e9c
    if(zero)goto L9498e9c;goto L9498e03;
   L9498e03: // MOV EBP,0xffffffff
    ebp=4294967295u;goto L9498e08;
   L9498e08: // XADD.LOCK dword ptr [EDX + -0x4],EBP
    a=R(unchecked(edx+4294967292u));b=ebp;v=unchecked((uint)h.AtomicAdd(unchecked(edx+4294967292u),unchecked((int)b)));ebp=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498e0d;
   L9498e0d: // MOV dword ptr [ESP + 0x10],EBP
    W(unchecked(esp+16u),ebp);goto L9498e11;
   L9498e11: // MOV EDX,dword ptr [ESP + 0x10]
    edx=R(unchecked(esp+16u));goto L9498e15;
   L9498e15: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498e17;
   L9498e17: // JG 0x09498dc2
    if(!zero&&sign==overflow)goto L9498dc2;goto L9498e19;
   L9498e19: // LEA EDX,[ESP + 0x2d]
    edx=unchecked(esp+45u);goto L9498e1d;
   L9498e1d: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L9498e21;
   L9498e21: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L9498e25;
   L9498e25: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L9498e28;
   L9498e28: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498e2d;
   L9498e2d: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9498e31;
   L9498e31: // JMP 0x09498dc2
    goto L9498dc2;
   L9498e33: // MOV ESI,0x80b0f04
    esi=134942468u;goto L9498e38;
   L9498e38: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498e3a;
   L9498e3a: // JZ 0x09498eae
    if(zero)goto L9498eae;goto L9498e3c;
   L9498e3c: // MOV ESI,0xffffffff
    esi=4294967295u;goto L9498e41;
   L9498e41: // XADD.LOCK dword ptr [EDX + -0x4],ESI
    a=R(unchecked(edx+4294967292u));b=esi;v=unchecked((uint)h.AtomicAdd(unchecked(edx+4294967292u),unchecked((int)b)));esi=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498e46;
   L9498e46: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498e48;
   L9498e48: // JG 0x09498de5
    if(!zero&&sign==overflow)goto L9498de5;goto L9498e4a;
   L9498e4a: // LEA EDX,[ESP + 0x2f]
    edx=unchecked(esp+47u);goto L9498e4e;
   L9498e4e: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L9498e52;
   L9498e52: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L9498e56;
   L9498e56: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L9498e59;
   L9498e59: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498e5e;
   L9498e5e: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9498e62;
   L9498e62: // JMP 0x09498de5
    goto L9498de5;
   L9498e64: // MOV ESI,0x80b0f04
    esi=134942468u;goto L9498e69;
   L9498e69: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498e6b;
   L9498e6b: // JZ 0x09498eb9
    if(zero)goto L9498eb9;goto L9498e6d;
   L9498e6d: // MOV ESI,0xffffffff
    esi=4294967295u;goto L9498e72;
   L9498e72: // XADD.LOCK dword ptr [EDX + -0x4],ESI
    a=R(unchecked(edx+4294967292u));b=esi;v=unchecked((uint)h.AtomicAdd(unchecked(edx+4294967292u),unchecked((int)b)));esi=v;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;goto L9498e77;
   L9498e77: // TEST ESI,ESI
    a=esi;b=esi;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L9498e79;
   L9498e79: // JG 0x09498dd8
    if(!zero&&sign==overflow)goto L9498dd8;goto L9498e7f;
   L9498e7f: // LEA EBP,[ESP + 0x2e]
    ebp=unchecked(esp+46u);goto L9498e83;
   L9498e83: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L9498e87;
   L9498e87: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L9498e8b;
   L9498e8b: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L9498e8e;
   L9498e8e: // CALL 0x080ab9f4
    eax=Call(0x80ab9f4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L9498e93;
   L9498e93: // MOV EAX,dword ptr [ESP + 0x14]
    eax=R(unchecked(esp+20u));goto L9498e97;
   L9498e97: // JMP 0x09498dd8
    goto L9498dd8;
   L9498e9c: // MOV EBP,dword ptr [EDX + -0x4]
    ebp=R(unchecked(edx+4294967292u));goto L9498e9f;
   L9498e9f: // MOV dword ptr [ESP + 0x10],EBP
    W(unchecked(esp+16u),ebp);goto L9498ea3;
   L9498ea3: // SUB EBP,0x1
    a=ebp;b=1u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;ebp=v;goto L9498ea6;
   L9498ea6: // MOV dword ptr [EDX + -0x4],EBP
    W(unchecked(edx+4294967292u),ebp);goto L9498ea9;
   L9498ea9: // JMP 0x09498e11
    goto L9498e11;
   L9498eae: // MOV ESI,dword ptr [EDX + -0x4]
    esi=R(unchecked(edx+4294967292u));goto L9498eb1;
   L9498eb1: // LEA EBP,[ESI + -0x1]
    ebp=unchecked(esi+4294967295u);goto L9498eb4;
   L9498eb4: // MOV dword ptr [EDX + -0x4],EBP
    W(unchecked(edx+4294967292u),ebp);goto L9498eb7;
   L9498eb7: // JMP 0x09498e46
    goto L9498e46;
   L9498eb9: // MOV ESI,dword ptr [EDX + -0x4]
    esi=R(unchecked(edx+4294967292u));goto L9498ebc;
   L9498ebc: // LEA EBP,[ESI + -0x1]
    ebp=unchecked(esi+4294967295u);goto L9498ebf;
   L9498ebf: // MOV dword ptr [EDX + -0x4],EBP
    W(unchecked(edx+4294967292u),ebp);goto L9498ec2;
   L9498ec2: // JMP 0x09498e77
    goto L9498e77;
  }
  public void Destroy(uint owner){Call(0x80add34,unchecked(owner+8u));Call(0x80add34,unchecked(owner+4u));Call(0x80add34,owner); }
 }
}
