using System;
namespace RouletteDeluxe
{
 // Physical packaged ctype and C-locale lifetime. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue. Local classic initializer has no C++ input arguments. Platform locale, deletion and native exception owners remain required.
 public sealed class OriginalConfigurationLocaleFacetLifetime
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
  public OriginalConfigurationLocaleFacetLifetime(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint DestroyCtype(uint frame,uint owner,uint source,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x30u,owner);goto L54e42;
   L54e42: // MOV dword ptr [ESP + 0x28],ESI
    W(unchecked(esp+40u),esi);goto L54e46;
   L54e46: // MOV ESI,dword ptr [ESP + 0x30]
    esi=R(unchecked(esp+48u));goto L54e4a;
   L54e4a: // MOV EAX,dword ptr [EBX + 0xfffffe90]
    eax=R(unchecked(ebx+4294966928u));goto L54e50;
   L54e50: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L54e53;
   L54e53: // MOV dword ptr [ESI],EAX
    W(unchecked(esi),eax);goto L54e55;
   L54e55: // LEA EAX,[ESI + 0x8]
    eax=unchecked(esi+8u);goto L54e58;
   L54e58: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L54e5b;
   L54e5b: // CALL 0x00048d70
    try{eax=Call(0x48d70,R(unchecked(esp+0u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L54e89;}goto L54e60;
   L54e60: // CMP byte ptr [ESI + 0xc],0x0
    a=B(unchecked(esi+12u));b=0u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L54e64;
   L54e64: // JZ 0x00054e75
    if(zero)goto L54e75;goto L54e66;
   L54e66: // MOV EAX,dword ptr [ESI + 0x18]
    eax=R(unchecked(esi+24u));goto L54e69;
   L54e69: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L54e6b;
   L54e6b: // JZ 0x00054e75
    if(zero)goto L54e75;goto L54e6d;
   L54e6d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L54e70;
   L54e70: // CALL 0x00048270
    eax=Call(0x48270,R(unchecked(esp+0u)));goto L54e75;
   L54e75: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L54e78;
   L54e78: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L54e7d;
   L54e7d:return 0;
   L54e89: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L54e8d;
   L54e8d: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L54e90;
   L54e90: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L54e95;
   L54e95: // MOV EAX,dword ptr [ESP + 0x1c]
    eax=R(unchecked(esp+28u));goto L54e99;
   L54e99: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L54e9c;
   L54e9c: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DeleteCtype(uint frame,uint owner,uint source,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L54ec2;
   L54ec2: // MOV dword ptr [ESP + 0x18],ESI
    W(unchecked(esp+24u),esi);goto L54ec6;
   L54ec6: // MOV ESI,dword ptr [ESP + 0x20]
    esi=R(unchecked(esp+32u));goto L54eca;
   L54eca: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L54ecd;
   L54ecd: // CALL 0x00047f60
    eax=Call(0x47f60,R(unchecked(esp+0u)));goto L54ed2;
   L54ed2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L54ed5;
   L54ed5: // CALL 0x00048900
    eax=Call(0x48900,R(unchecked(esp+0u)));goto L54eda;
   L54eda:return 0;
  }
  public uint CreateCLocale(uint frame,uint owner,uint source,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=argument;W(frame+0x20u,owner);W(frame+0x24u,source);W(frame+0x28u,argument);goto L681f3;
   L681f3: // MOV dword ptr [ESP],0x40
    W(unchecked(esp),64u);goto L681fa;
   L681fa: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L681fe;
   L681fe: // MOV EAX,dword ptr [ESP + 0x24]
    eax=R(unchecked(esp+36u));goto L68202;
   L68202: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L68206;
   L68206: // CALL 0x0004a1a0
    eax=Call(0x4a1a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L6820b;
   L6820b: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L6820f;
   L6820f: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L68211;
   L68211: // MOV dword ptr [EDX],EAX
    W(unchecked(edx),eax);goto L68213;
   L68213: // JZ 0x0006821a
    if(zero)goto L6821a;goto L68215;
   L68215:return 0;
   L6821a: // LEA EAX,[EBX + 0xfffd9460]
    eax=unchecked(ebx+4294808672u);goto L68220;
   L68220: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L68223;
   L68223: // CALL 0x000494b0
    Call(0x494b0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint DestroyCLocale(uint frame,uint owner,uint source,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   esi=owner;goto L6824a;
   L6824a: // MOV EAX,dword ptr [ESI]
    eax=R(unchecked(esi));goto L6824c;
   L6824c: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6824e;
   L6824e: // JZ 0x00068263
    if(zero)goto L68263;goto L68250;
   L68250: // CALL 0x00049d10
    eax=Call(0x49d10);goto L68255;
   L68255: // MOV EDX,dword ptr [ESI]
    edx=R(unchecked(esi));goto L68257;
   L68257: // CMP EAX,EDX
    a=eax;b=edx;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L68259;
   L68259: // JZ 0x00068263
    if(zero)goto L68263;goto L6825b;
   L6825b: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L6825e;
   L6825e: // CALL 0x00049bd0
    eax=Call(0x49bd0,R(unchecked(esp+0u)));goto L68263;
   L68263:return 0;
  }
  public uint InitializeClassic(uint frame,uint owner,uint source,uint argument)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   goto L5971f;
   L5971f: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L59727;
   L59727: // MOV EAX,dword ptr [EBX + 0xfffffd74]
    eax=R(unchecked(ebx+4294966644u));goto L5972d;
   L5972d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L59731;
   L59731: // MOV EAX,dword ptr [EBX + 0xfffffbdc]
    eax=R(unchecked(ebx+4294966236u));goto L59737;
   L59737: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5973a;
   L5973a: // CALL 0x000489c0
    eax=Call(0x489c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));goto L5973f;
   L5973f:return 0;
  }
 }
}
