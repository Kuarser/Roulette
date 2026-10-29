using System;
namespace RouletteDeluxe
{
 // Physical packaged classic locale Impl constructor 0005a250. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; all facet/platform/exception owners remain required.
 public sealed class OriginalConfigurationLocaleImplConstructor
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
  public OriginalConfigurationLocaleImplConstructor(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadHalf==null||host.WriteHalf==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadHalf=host.ReadHalf,WriteHalf=host.WriteHalf,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  uint H(uint at){return h.ReadHalf(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint Construct(uint frame,uint owner,uint count)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(unchecked(frame+0x50u),owner);W(unchecked(frame+0x54u),count);goto L5a262;
   L5a262: // MOV EDI,dword ptr [ESP + 0x50]
    edi=R(unchecked(esp+80u));goto L5a266;
   L5a266: // MOV EAX,dword ptr [ESP + 0x54]
    eax=R(unchecked(esp+84u));goto L5a26a;
   L5a26a: // MOV dword ptr [EDI + 0x8],0x1c
    W(unchecked(edi+8u),28u);goto L5a271;
   L5a271: // LEA EDX,[EBX + 0x1aac]
    edx=unchecked(ebx+6828u);goto L5a277;
   L5a277: // LEA ECX,[EBX + 0x1b2c]
    ecx=unchecked(ebx+6956u);goto L5a27d;
   L5a27d: // MOV dword ptr [EDI],EAX
    W(unchecked(edi),eax);goto L5a27f;
   L5a27f: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L5a281;
   L5a281: // MOV dword ptr [EDI + 0x10],0x0
    W(unchecked(edi+16u),0u);goto L5a288;
   L5a288: // MOV dword ptr [EDI + 0x4],EDX
    W(unchecked(edi+4u),edx);goto L5a28b;
   L5a28b: // MOV dword ptr [EDI + 0xc],ECX
    W(unchecked(edi+12u),ecx);goto L5a28e;
   L5a28e: // JMP 0x0005a296
    goto L5a296;
   L5a290: // MOV EDX,dword ptr [EDI + 0x4]
    edx=R(unchecked(edi+4u));goto L5a293;
   L5a293: // MOV ECX,dword ptr [EDI + 0xc]
    ecx=R(unchecked(edi+12u));goto L5a296;
   L5a296: // MOV dword ptr [ECX + EAX*0x1],0x0
    W(unchecked(ecx+eax*0x1u),0u);goto L5a29d;
   L5a29d: // MOV dword ptr [EDX + EAX*0x1],0x0
    W(unchecked(edx+eax*0x1u),0u);goto L5a2a4;
   L5a2a4: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a2a7;
   L5a2a7: // CMP EAX,0x70
    a=eax;b=112u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5a2aa;
   L5a2aa: // JNZ 0x0005a290
    if(!zero)goto L5a290;goto L5a2ac;
   L5a2ac: // LEA ESI,[EBX + 0x1bac]
    esi=unchecked(ebx+7084u);goto L5a2b2;
   L5a2b2: // LEA EAX,[EBX + 0x1bdc]
    eax=unchecked(ebx+7132u);goto L5a2b8;
   L5a2b8: // MOV dword ptr [EDI + 0x10],ESI
    W(unchecked(edi+16u),esi);goto L5a2bb;
   L5a2bb: // MOV dword ptr [EBX + 0x1bac],EAX
    W(unchecked(ebx+7084u),eax);goto L5a2c1;
   L5a2c1: // CALL 0x000488e0
    eax=Call(0x488e0);goto L5a2c6;
   L5a2c6: // MOVZX EAX,word ptr [EAX]
    eax=H(unchecked(eax));goto L5a2c9;
   L5a2c9: // MOV word ptr [EBX + 0x1bdc],AX
    h.WriteHalf(unchecked(ebx+7132u),unchecked((ushort)((eax&65535u))));goto L5a2d0;
   L5a2d0: // MOV EAX,0x4
    eax=4u;goto L5a2d5;
   L5a2d5: // JMP 0x0005a2db
    goto L5a2db;
   L5a2d8: // MOV ESI,dword ptr [EDI + 0x10]
    esi=R(unchecked(edi+16u));goto L5a2db;
   L5a2db: // MOV dword ptr [ESI + EAX*0x1],0x0
    W(unchecked(esi+eax*0x1u),0u);goto L5a2e2;
   L5a2e2: // ADD EAX,0x4
    a=eax;b=4u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a2e5;
   L5a2e5: // CMP EAX,0x30
    a=eax;b=48u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5a2e8;
   L5a2e8: // JNZ 0x0005a2d8
    if(!zero)goto L5a2d8;goto L5a2ea;
   L5a2ea: // LEA ESI,[EBX + 0x1c0c]
    esi=unchecked(ebx+7180u);goto L5a2f0;
   L5a2f0: // MOV dword ptr [ESP + 0xc],0x1
    W(unchecked(esp+12u),1u);goto L5a2f8;
   L5a2f8: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L5a300;
   L5a300: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5a308;
   L5a308: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a30b;
   L5a30b: // CALL 0x00048d00
    try{eax=Call(0x48d00,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a310;
   L5a310: // MOV EAX,dword ptr [EBX + 0xffffffb8]
    eax=R(unchecked(ebx+4294967224u));goto L5a316;
   L5a316: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a31a;
   L5a31a: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a31d;
   L5a31d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a321;
   L5a321: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a326;
   L5a326: // LEA ESI,[EBX + 0x1e2c]
    esi=unchecked(ebx+7724u);goto L5a32c;
   L5a32c: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L5a334;
   L5a334: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a337;
   L5a337: // CALL 0x000495a0
    try{eax=Call(0x495a0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a33c;
   L5a33c: // MOV EAX,dword ptr [EBX + 0xfffffdc8]
    eax=R(unchecked(ebx+4294966728u));goto L5a342;
   L5a342: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a346;
   L5a346: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a349;
   L5a349: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a34d;
   L5a34d: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a352;
   L5a352: // MOV EAX,dword ptr [EBX + 0xfffffde4]
    eax=R(unchecked(ebx+4294966756u));goto L5a358;
   L5a358: // LEA ESI,[EBX + 0x1eb4]
    esi=unchecked(ebx+7860u);goto L5a35e;
   L5a35e: // MOV dword ptr [EBX + 0x1e50],0x1
    W(unchecked(ebx+7760u),1u);goto L5a368;
   L5a368: // MOV dword ptr [EBX + 0x1e54],0x0
    W(unchecked(ebx+7764u),0u);goto L5a372;
   L5a372: // MOV dword ptr [EBX + 0x1e58],0x0
    W(unchecked(ebx+7768u),0u);goto L5a37c;
   L5a37c: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a37f;
   L5a37f: // MOV dword ptr [EBX + 0x1e4c],EAX
    W(unchecked(ebx+7756u),eax);goto L5a385;
   L5a385: // MOV EAX,dword ptr [EBX + 0xfffffa58]
    eax=R(unchecked(ebx+4294965848u));goto L5a38b;
   L5a38b: // MOV byte ptr [EBX + 0x1e5c],0x0
    h.WriteByte(unchecked(ebx+7772u),unchecked((byte)(0u)));goto L5a392;
   L5a392: // MOV dword ptr [EBX + 0x1e60],0x0
    W(unchecked(ebx+7776u),0u);goto L5a39c;
   L5a39c: // MOV dword ptr [EBX + 0x1e64],0x0
    W(unchecked(ebx+7780u),0u);goto L5a3a6;
   L5a3a6: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a3a9;
   L5a3a9: // MOV dword ptr [EBX + 0x1eb4],EAX
    W(unchecked(ebx+7860u),eax);goto L5a3af;
   L5a3af: // LEA EAX,[EBX + 0x1e4c]
    eax=unchecked(ebx+7756u);goto L5a3b5;
   L5a3b5: // MOV dword ptr [EBX + 0x1e68],0x0
    W(unchecked(ebx+7784u),0u);goto L5a3bf;
   L5a3bf: // MOV dword ptr [EBX + 0x1e6c],0x0
    W(unchecked(ebx+7788u),0u);goto L5a3c9;
   L5a3c9: // MOV byte ptr [EBX + 0x1e70],0x0
    h.WriteByte(unchecked(ebx+7792u),unchecked((byte)(0u)));goto L5a3d0;
   L5a3d0: // MOV byte ptr [EBX + 0x1e71],0x0
    h.WriteByte(unchecked(ebx+7793u),unchecked((byte)(0u)));goto L5a3d7;
   L5a3d7: // MOV byte ptr [EBX + 0x1eb0],0x0
    h.WriteByte(unchecked(ebx+7856u),unchecked((byte)(0u)));goto L5a3de;
   L5a3de: // MOV dword ptr [EBX + 0x1eb8],0x1
    W(unchecked(ebx+7864u),1u);goto L5a3e8;
   L5a3e8: // MOV dword ptr [ESP + 0x14],EAX
    W(unchecked(esp+20u),eax);goto L5a3ec;
   L5a3ec: // MOV dword ptr [EBX + 0x1ebc],EAX
    W(unchecked(ebx+7868u),eax);goto L5a3f2;
   L5a3f2: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5a3fa;
   L5a3fa: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a3fd;
   L5a3fd: // CALL 0x00048a90
    try{eax=Call(0x48a90,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b2ab;}goto L5a402;
   L5a402: // MOV EDX,dword ptr [EBX + 0xfffffdfc]
    edx=R(unchecked(ebx+4294966780u));goto L5a408;
   L5a408: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a40c;
   L5a40c: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a40f;
   L5a40f: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5a413;
   L5a413: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a418;
   L5a418: // MOV EAX,dword ptr [EBX + 0xfffffa78]
    eax=R(unchecked(ebx+4294965880u));goto L5a41e;
   L5a41e: // MOV dword ptr [EBX + 0x1ec4],0x1
    W(unchecked(ebx+7876u),1u);goto L5a428;
   L5a428: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a42b;
   L5a42b: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a42e;
   L5a42e: // MOV dword ptr [EBX + 0x1ec0],EAX
    W(unchecked(ebx+7872u),eax);goto L5a434;
   L5a434: // LEA EAX,[EBX + 0x1ec0]
    eax=unchecked(ebx+7872u);goto L5a43a;
   L5a43a: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a43e;
   L5a43e: // MOV EAX,dword ptr [EBX + 0xfffffc74]
    eax=R(unchecked(ebx+4294966388u));goto L5a444;
   L5a444: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a448;
   L5a448: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a44d;
   L5a44d: // MOV EAX,dword ptr [EBX + 0xfffffbd8]
    eax=R(unchecked(ebx+4294966232u));goto L5a453;
   L5a453: // MOV dword ptr [EBX + 0x1ecc],0x1
    W(unchecked(ebx+7884u),1u);goto L5a45d;
   L5a45d: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a460;
   L5a460: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a463;
   L5a463: // MOV dword ptr [EBX + 0x1ec8],EAX
    W(unchecked(ebx+7880u),eax);goto L5a469;
   L5a469: // LEA EAX,[EBX + 0x1ec8]
    eax=unchecked(ebx+7880u);goto L5a46f;
   L5a46f: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a473;
   L5a473: // MOV EAX,dword ptr [EBX + 0xfffffccc]
    eax=R(unchecked(ebx+4294966476u));goto L5a479;
   L5a479: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a47d;
   L5a47d: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a482;
   L5a482: // MOV EAX,dword ptr [EBX + 0xfffffc80]
    eax=R(unchecked(ebx+4294966400u));goto L5a488;
   L5a488: // MOV dword ptr [EBX + 0x1ed4],0x1
    W(unchecked(ebx+7892u),1u);goto L5a492;
   L5a492: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a495;
   L5a495: // MOV dword ptr [EBX + 0x1ed0],EAX
    W(unchecked(ebx+7888u),eax);goto L5a49b;
   L5a49b: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b293;}goto L5a4a0;
   L5a4a0: // MOV dword ptr [EBX + 0x1ed8],EAX
    W(unchecked(ebx+7896u),eax);goto L5a4a6;
   L5a4a6: // LEA EAX,[EBX + 0x1ed0]
    eax=unchecked(ebx+7888u);goto L5a4ac;
   L5a4ac: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a4b0;
   L5a4b0: // MOV EAX,dword ptr [EBX + 0xfffffe64]
    eax=R(unchecked(ebx+4294966884u));goto L5a4b6;
   L5a4b6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a4b9;
   L5a4b9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a4bd;
   L5a4bd: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a4c2;
   L5a4c2: // MOV EAX,dword ptr [EBX + 0xfffffaac]
    eax=R(unchecked(ebx+4294965932u));goto L5a4c8;
   L5a4c8: // LEA ESI,[EBX + 0x1f30]
    esi=unchecked(ebx+7984u);goto L5a4ce;
   L5a4ce: // MOV dword ptr [EBX + 0x1ef0],0x1
    W(unchecked(ebx+7920u),1u);goto L5a4d8;
   L5a4d8: // MOV dword ptr [EBX + 0x1ef4],0x0
    W(unchecked(ebx+7924u),0u);goto L5a4e2;
   L5a4e2: // MOV dword ptr [EBX + 0x1ef8],0x0
    W(unchecked(ebx+7928u),0u);goto L5a4ec;
   L5a4ec: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a4ef;
   L5a4ef: // MOV dword ptr [EBX + 0x1eec],EAX
    W(unchecked(ebx+7916u),eax);goto L5a4f5;
   L5a4f5: // MOV EAX,dword ptr [EBX + 0xfffffbd0]
    eax=R(unchecked(ebx+4294966224u));goto L5a4fb;
   L5a4fb: // MOV byte ptr [EBX + 0x1efc],0x0
    h.WriteByte(unchecked(ebx+7932u),unchecked((byte)(0u)));goto L5a502;
   L5a502: // MOV byte ptr [EBX + 0x1efd],0x0
    h.WriteByte(unchecked(ebx+7933u),unchecked((byte)(0u)));goto L5a509;
   L5a509: // MOV byte ptr [EBX + 0x1efe],0x0
    h.WriteByte(unchecked(ebx+7934u),unchecked((byte)(0u)));goto L5a510;
   L5a510: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a513;
   L5a513: // MOV dword ptr [EBX + 0x1f30],EAX
    W(unchecked(ebx+7984u),eax);goto L5a519;
   L5a519: // LEA EAX,[EBX + 0x1eec]
    eax=unchecked(ebx+7916u);goto L5a51f;
   L5a51f: // MOV dword ptr [EBX + 0x1f00],0x0
    W(unchecked(ebx+7936u),0u);goto L5a529;
   L5a529: // MOV dword ptr [EBX + 0x1f04],0x0
    W(unchecked(ebx+7940u),0u);goto L5a533;
   L5a533: // MOV dword ptr [EBX + 0x1f08],0x0
    W(unchecked(ebx+7944u),0u);goto L5a53d;
   L5a53d: // MOV dword ptr [EBX + 0x1f0c],0x0
    W(unchecked(ebx+7948u),0u);goto L5a547;
   L5a547: // MOV dword ptr [EBX + 0x1f10],0x0
    W(unchecked(ebx+7952u),0u);goto L5a551;
   L5a551: // MOV dword ptr [EBX + 0x1f14],0x0
    W(unchecked(ebx+7956u),0u);goto L5a55b;
   L5a55b: // MOV dword ptr [EBX + 0x1f18],0x0
    W(unchecked(ebx+7960u),0u);goto L5a565;
   L5a565: // MOV byte ptr [EBX + 0x1f1c],0x0
    h.WriteByte(unchecked(ebx+7964u),unchecked((byte)(0u)));goto L5a56c;
   L5a56c: // MOV byte ptr [EBX + 0x1f1d],0x0
    h.WriteByte(unchecked(ebx+7965u),unchecked((byte)(0u)));goto L5a573;
   L5a573: // MOV byte ptr [EBX + 0x1f1e],0x0
    h.WriteByte(unchecked(ebx+7966u),unchecked((byte)(0u)));goto L5a57a;
   L5a57a: // MOV byte ptr [EBX + 0x1f1f],0x0
    h.WriteByte(unchecked(ebx+7967u),unchecked((byte)(0u)));goto L5a581;
   L5a581: // MOV byte ptr [EBX + 0x1f20],0x0
    h.WriteByte(unchecked(ebx+7968u),unchecked((byte)(0u)));goto L5a588;
   L5a588: // MOV byte ptr [EBX + 0x1f21],0x0
    h.WriteByte(unchecked(ebx+7969u),unchecked((byte)(0u)));goto L5a58f;
   L5a58f: // MOV byte ptr [EBX + 0x1f22],0x0
    h.WriteByte(unchecked(ebx+7970u),unchecked((byte)(0u)));goto L5a596;
   L5a596: // MOV byte ptr [EBX + 0x1f23],0x0
    h.WriteByte(unchecked(ebx+7971u),unchecked((byte)(0u)));goto L5a59d;
   L5a59d: // MOV byte ptr [EBX + 0x1f2f],0x0
    h.WriteByte(unchecked(ebx+7983u),unchecked((byte)(0u)));goto L5a5a4;
   L5a5a4: // MOV dword ptr [EBX + 0x1f34],0x1
    W(unchecked(ebx+7988u),1u);goto L5a5ae;
   L5a5ae: // MOV dword ptr [ESP + 0x18],EAX
    W(unchecked(esp+24u),eax);goto L5a5b2;
   L5a5b2: // MOV dword ptr [EBX + 0x1f38],EAX
    W(unchecked(ebx+7992u),eax);goto L5a5b8;
   L5a5b8: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L5a5c0;
   L5a5c0: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5a5c8;
   L5a5c8: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a5cb;
   L5a5cb: // CALL 0x000481c0
    try{eax=Call(0x481c0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b281;}goto L5a5d0;
   L5a5d0: // MOV EDX,dword ptr [EBX + 0xfffffb78]
    edx=R(unchecked(ebx+4294966136u));goto L5a5d6;
   L5a5d6: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a5da;
   L5a5da: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a5dd;
   L5a5dd: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5a5e1;
   L5a5e1: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a5e6;
   L5a5e6: // MOV EAX,dword ptr [EBX + 0xffffff90]
    eax=R(unchecked(ebx+4294967184u));goto L5a5ec;
   L5a5ec: // LEA ESI,[EBX + 0x1f90]
    esi=unchecked(ebx+8080u);goto L5a5f2;
   L5a5f2: // MOV dword ptr [EBX + 0x1f50],0x1
    W(unchecked(ebx+8016u),1u);goto L5a5fc;
   L5a5fc: // MOV dword ptr [EBX + 0x1f54],0x0
    W(unchecked(ebx+8020u),0u);goto L5a606;
   L5a606: // MOV dword ptr [EBX + 0x1f58],0x0
    W(unchecked(ebx+8024u),0u);goto L5a610;
   L5a610: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a613;
   L5a613: // MOV dword ptr [EBX + 0x1f4c],EAX
    W(unchecked(ebx+8012u),eax);goto L5a619;
   L5a619: // MOV EAX,dword ptr [EBX + 0xffffff0c]
    eax=R(unchecked(ebx+4294967052u));goto L5a61f;
   L5a61f: // MOV byte ptr [EBX + 0x1f5c],0x0
    h.WriteByte(unchecked(ebx+8028u),unchecked((byte)(0u)));goto L5a626;
   L5a626: // MOV byte ptr [EBX + 0x1f5d],0x0
    h.WriteByte(unchecked(ebx+8029u),unchecked((byte)(0u)));goto L5a62d;
   L5a62d: // MOV byte ptr [EBX + 0x1f5e],0x0
    h.WriteByte(unchecked(ebx+8030u),unchecked((byte)(0u)));goto L5a634;
   L5a634: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a637;
   L5a637: // MOV dword ptr [EBX + 0x1f90],EAX
    W(unchecked(ebx+8080u),eax);goto L5a63d;
   L5a63d: // LEA EAX,[EBX + 0x1f4c]
    eax=unchecked(ebx+8012u);goto L5a643;
   L5a643: // MOV dword ptr [EBX + 0x1f60],0x0
    W(unchecked(ebx+8032u),0u);goto L5a64d;
   L5a64d: // MOV dword ptr [EBX + 0x1f64],0x0
    W(unchecked(ebx+8036u),0u);goto L5a657;
   L5a657: // MOV dword ptr [EBX + 0x1f68],0x0
    W(unchecked(ebx+8040u),0u);goto L5a661;
   L5a661: // MOV dword ptr [EBX + 0x1f6c],0x0
    W(unchecked(ebx+8044u),0u);goto L5a66b;
   L5a66b: // MOV dword ptr [EBX + 0x1f70],0x0
    W(unchecked(ebx+8048u),0u);goto L5a675;
   L5a675: // MOV dword ptr [EBX + 0x1f74],0x0
    W(unchecked(ebx+8052u),0u);goto L5a67f;
   L5a67f: // MOV dword ptr [EBX + 0x1f78],0x0
    W(unchecked(ebx+8056u),0u);goto L5a689;
   L5a689: // MOV byte ptr [EBX + 0x1f7c],0x0
    h.WriteByte(unchecked(ebx+8060u),unchecked((byte)(0u)));goto L5a690;
   L5a690: // MOV byte ptr [EBX + 0x1f7d],0x0
    h.WriteByte(unchecked(ebx+8061u),unchecked((byte)(0u)));goto L5a697;
   L5a697: // MOV byte ptr [EBX + 0x1f7e],0x0
    h.WriteByte(unchecked(ebx+8062u),unchecked((byte)(0u)));goto L5a69e;
   L5a69e: // MOV byte ptr [EBX + 0x1f7f],0x0
    h.WriteByte(unchecked(ebx+8063u),unchecked((byte)(0u)));goto L5a6a5;
   L5a6a5: // MOV byte ptr [EBX + 0x1f80],0x0
    h.WriteByte(unchecked(ebx+8064u),unchecked((byte)(0u)));goto L5a6ac;
   L5a6ac: // MOV byte ptr [EBX + 0x1f81],0x0
    h.WriteByte(unchecked(ebx+8065u),unchecked((byte)(0u)));goto L5a6b3;
   L5a6b3: // MOV byte ptr [EBX + 0x1f82],0x0
    h.WriteByte(unchecked(ebx+8066u),unchecked((byte)(0u)));goto L5a6ba;
   L5a6ba: // MOV byte ptr [EBX + 0x1f83],0x0
    h.WriteByte(unchecked(ebx+8067u),unchecked((byte)(0u)));goto L5a6c1;
   L5a6c1: // MOV byte ptr [EBX + 0x1f8f],0x0
    h.WriteByte(unchecked(ebx+8079u),unchecked((byte)(0u)));goto L5a6c8;
   L5a6c8: // MOV dword ptr [EBX + 0x1f94],0x1
    W(unchecked(ebx+8084u),1u);goto L5a6d2;
   L5a6d2: // MOV dword ptr [ESP + 0x1c],EAX
    W(unchecked(esp+28u),eax);goto L5a6d6;
   L5a6d6: // MOV dword ptr [EBX + 0x1f98],EAX
    W(unchecked(ebx+8088u),eax);goto L5a6dc;
   L5a6dc: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L5a6e4;
   L5a6e4: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5a6ec;
   L5a6ec: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a6ef;
   L5a6ef: // CALL 0x0004a430
    try{eax=Call(0x4a430,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b26f;}goto L5a6f4;
   L5a6f4: // MOV EDX,dword ptr [EBX + 0xfffffec4]
    edx=R(unchecked(ebx+4294966980u));goto L5a6fa;
   L5a6fa: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a6fe;
   L5a6fe: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a701;
   L5a701: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5a705;
   L5a705: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a70a;
   L5a70a: // MOV EAX,dword ptr [EBX + 0xfffffb04]
    eax=R(unchecked(ebx+4294966020u));goto L5a710;
   L5a710: // MOV dword ptr [EBX + 0x1fa0],0x1
    W(unchecked(ebx+8096u),1u);goto L5a71a;
   L5a71a: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a71d;
   L5a71d: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a720;
   L5a720: // MOV dword ptr [EBX + 0x1f9c],EAX
    W(unchecked(ebx+8092u),eax);goto L5a726;
   L5a726: // LEA EAX,[EBX + 0x1f9c]
    eax=unchecked(ebx+8092u);goto L5a72c;
   L5a72c: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a730;
   L5a730: // MOV EAX,dword ptr [EBX + 0xffffffb4]
    eax=R(unchecked(ebx+4294967220u));goto L5a736;
   L5a736: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a73a;
   L5a73a: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a73f;
   L5a73f: // MOV EAX,dword ptr [EBX + 0xfffffc94]
    eax=R(unchecked(ebx+4294966420u));goto L5a745;
   L5a745: // MOV dword ptr [EBX + 0x1fa8],0x1
    W(unchecked(ebx+8104u),1u);goto L5a74f;
   L5a74f: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a752;
   L5a752: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a755;
   L5a755: // MOV dword ptr [EBX + 0x1fa4],EAX
    W(unchecked(ebx+8100u),eax);goto L5a75b;
   L5a75b: // LEA EAX,[EBX + 0x1fa4]
    eax=unchecked(ebx+8100u);goto L5a761;
   L5a761: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a765;
   L5a765: // MOV EAX,dword ptr [EBX + 0xfffffcb0]
    eax=R(unchecked(ebx+4294966448u));goto L5a76b;
   L5a76b: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a76f;
   L5a76f: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a774;
   L5a774: // MOV EAX,dword ptr [EBX + 0xfffffca0]
    eax=R(unchecked(ebx+4294966432u));goto L5a77a;
   L5a77a: // LEA ESI,[EBX + 0x2074]
    esi=unchecked(ebx+8308u);goto L5a780;
   L5a780: // MOV dword ptr [EBX + 0x1fb0],0x1
    W(unchecked(ebx+8112u),1u);goto L5a78a;
   L5a78a: // MOV dword ptr [EBX + 0x1fb4],0x0
    W(unchecked(ebx+8116u),0u);goto L5a794;
   L5a794: // MOV dword ptr [EBX + 0x1fb8],0x0
    W(unchecked(ebx+8120u),0u);goto L5a79e;
   L5a79e: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a7a1;
   L5a7a1: // MOV dword ptr [EBX + 0x1fac],EAX
    W(unchecked(ebx+8108u),eax);goto L5a7a7;
   L5a7a7: // LEA EAX,[EBX + 0x1fac]
    eax=unchecked(ebx+8108u);goto L5a7ad;
   L5a7ad: // MOV dword ptr [EBX + 0x1fbc],0x0
    W(unchecked(ebx+8124u),0u);goto L5a7b7;
   L5a7b7: // MOV dword ptr [EBX + 0x1fc0],0x0
    W(unchecked(ebx+8128u),0u);goto L5a7c1;
   L5a7c1: // MOV dword ptr [EBX + 0x1fc4],0x0
    W(unchecked(ebx+8132u),0u);goto L5a7cb;
   L5a7cb: // MOV dword ptr [EBX + 0x1fc8],0x0
    W(unchecked(ebx+8136u),0u);goto L5a7d5;
   L5a7d5: // MOV dword ptr [EBX + 0x1fcc],0x0
    W(unchecked(ebx+8140u),0u);goto L5a7df;
   L5a7df: // MOV dword ptr [EBX + 0x1fd0],0x0
    W(unchecked(ebx+8144u),0u);goto L5a7e9;
   L5a7e9: // MOV dword ptr [EBX + 0x1fd4],0x0
    W(unchecked(ebx+8148u),0u);goto L5a7f3;
   L5a7f3: // MOV dword ptr [EBX + 0x1fd8],0x0
    W(unchecked(ebx+8152u),0u);goto L5a7fd;
   L5a7fd: // MOV dword ptr [EBX + 0x1fdc],0x0
    W(unchecked(ebx+8156u),0u);goto L5a807;
   L5a807: // MOV dword ptr [EBX + 0x1fe0],0x0
    W(unchecked(ebx+8160u),0u);goto L5a811;
   L5a811: // MOV dword ptr [EBX + 0x1fe4],0x0
    W(unchecked(ebx+8164u),0u);goto L5a81b;
   L5a81b: // MOV dword ptr [EBX + 0x1fe8],0x0
    W(unchecked(ebx+8168u),0u);goto L5a825;
   L5a825: // MOV dword ptr [EBX + 0x1fec],0x0
    W(unchecked(ebx+8172u),0u);goto L5a82f;
   L5a82f: // MOV dword ptr [EBX + 0x1ff0],0x0
    W(unchecked(ebx+8176u),0u);goto L5a839;
   L5a839: // MOV dword ptr [EBX + 0x1ff4],0x0
    W(unchecked(ebx+8180u),0u);goto L5a843;
   L5a843: // MOV dword ptr [EBX + 0x1ff8],0x0
    W(unchecked(ebx+8184u),0u);goto L5a84d;
   L5a84d: // MOV dword ptr [EBX + 0x1ffc],0x0
    W(unchecked(ebx+8188u),0u);goto L5a857;
   L5a857: // MOV dword ptr [EBX + 0x2000],0x0
    W(unchecked(ebx+8192u),0u);goto L5a861;
   L5a861: // MOV dword ptr [EBX + 0x2004],0x0
    W(unchecked(ebx+8196u),0u);goto L5a86b;
   L5a86b: // MOV dword ptr [EBX + 0x2008],0x0
    W(unchecked(ebx+8200u),0u);goto L5a875;
   L5a875: // MOV dword ptr [EBX + 0x200c],0x0
    W(unchecked(ebx+8204u),0u);goto L5a87f;
   L5a87f: // MOV dword ptr [EBX + 0x2010],0x0
    W(unchecked(ebx+8208u),0u);goto L5a889;
   L5a889: // MOV dword ptr [EBX + 0x2014],0x0
    W(unchecked(ebx+8212u),0u);goto L5a893;
   L5a893: // MOV dword ptr [EBX + 0x2018],0x0
    W(unchecked(ebx+8216u),0u);goto L5a89d;
   L5a89d: // MOV dword ptr [EBX + 0x201c],0x0
    W(unchecked(ebx+8220u),0u);goto L5a8a7;
   L5a8a7: // MOV dword ptr [EBX + 0x2020],0x0
    W(unchecked(ebx+8224u),0u);goto L5a8b1;
   L5a8b1: // MOV dword ptr [EBX + 0x2024],0x0
    W(unchecked(ebx+8228u),0u);goto L5a8bb;
   L5a8bb: // MOV dword ptr [EBX + 0x2028],0x0
    W(unchecked(ebx+8232u),0u);goto L5a8c5;
   L5a8c5: // MOV dword ptr [EBX + 0x202c],0x0
    W(unchecked(ebx+8236u),0u);goto L5a8cf;
   L5a8cf: // MOV dword ptr [EBX + 0x2030],0x0
    W(unchecked(ebx+8240u),0u);goto L5a8d9;
   L5a8d9: // MOV dword ptr [EBX + 0x2034],0x0
    W(unchecked(ebx+8244u),0u);goto L5a8e3;
   L5a8e3: // MOV dword ptr [EBX + 0x2038],0x0
    W(unchecked(ebx+8248u),0u);goto L5a8ed;
   L5a8ed: // MOV dword ptr [EBX + 0x203c],0x0
    W(unchecked(ebx+8252u),0u);goto L5a8f7;
   L5a8f7: // MOV dword ptr [EBX + 0x2040],0x0
    W(unchecked(ebx+8256u),0u);goto L5a901;
   L5a901: // MOV dword ptr [EBX + 0x2044],0x0
    W(unchecked(ebx+8260u),0u);goto L5a90b;
   L5a90b: // MOV dword ptr [EBX + 0x2048],0x0
    W(unchecked(ebx+8264u),0u);goto L5a915;
   L5a915: // MOV dword ptr [EBX + 0x204c],0x0
    W(unchecked(ebx+8268u),0u);goto L5a91f;
   L5a91f: // MOV dword ptr [EBX + 0x2050],0x0
    W(unchecked(ebx+8272u),0u);goto L5a929;
   L5a929: // MOV dword ptr [EBX + 0x2054],0x0
    W(unchecked(ebx+8276u),0u);goto L5a933;
   L5a933: // MOV dword ptr [EBX + 0x2058],0x0
    W(unchecked(ebx+8280u),0u);goto L5a93d;
   L5a93d: // MOV dword ptr [EBX + 0x205c],0x0
    W(unchecked(ebx+8284u),0u);goto L5a947;
   L5a947: // MOV dword ptr [EBX + 0x2060],0x0
    W(unchecked(ebx+8288u),0u);goto L5a951;
   L5a951: // MOV dword ptr [EBX + 0x2064],0x0
    W(unchecked(ebx+8292u),0u);goto L5a95b;
   L5a95b: // MOV dword ptr [EBX + 0x2068],0x0
    W(unchecked(ebx+8296u),0u);goto L5a965;
   L5a965: // MOV dword ptr [EBX + 0x206c],0x0
    W(unchecked(ebx+8300u),0u);goto L5a96f;
   L5a96f: // MOV byte ptr [EBX + 0x2070],0x0
    h.WriteByte(unchecked(ebx+8304u),unchecked((byte)(0u)));goto L5a976;
   L5a976: // MOV dword ptr [ESP + 0x8],0x1
    W(unchecked(esp+8u),1u);goto L5a97e;
   L5a97e: // MOV dword ptr [ESP + 0x20],EAX
    W(unchecked(esp+32u),eax);goto L5a982;
   L5a982: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a986;
   L5a986: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5a989;
   L5a989: // CALL 0x00048c90
    try{eax=Call(0x48c90,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a98e;
   L5a98e: // MOV EDX,dword ptr [EBX + 0xffffff40]
    edx=R(unchecked(ebx+4294967104u));goto L5a994;
   L5a994: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5a998;
   L5a998: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a99b;
   L5a99b: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5a99f;
   L5a99f: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a9a4;
   L5a9a4: // MOV EAX,dword ptr [EBX + 0xfffffc7c]
    eax=R(unchecked(ebx+4294966396u));goto L5a9aa;
   L5a9aa: // MOV dword ptr [EBX + 0x208c],0x1
    W(unchecked(ebx+8332u),1u);goto L5a9b4;
   L5a9b4: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a9b7;
   L5a9b7: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a9ba;
   L5a9ba: // MOV dword ptr [EBX + 0x2088],EAX
    W(unchecked(ebx+8328u),eax);goto L5a9c0;
   L5a9c0: // LEA EAX,[EBX + 0x2088]
    eax=unchecked(ebx+8328u);goto L5a9c6;
   L5a9c6: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a9ca;
   L5a9ca: // MOV EAX,dword ptr [EBX + 0xfffffdf8]
    eax=R(unchecked(ebx+4294966776u));goto L5a9d0;
   L5a9d0: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5a9d4;
   L5a9d4: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5a9d9;
   L5a9d9: // MOV EAX,dword ptr [EBX + 0xffffffe8]
    eax=R(unchecked(ebx+4294967272u));goto L5a9df;
   L5a9df: // MOV dword ptr [EBX + 0x2094],0x1
    W(unchecked(ebx+8340u),1u);goto L5a9e9;
   L5a9e9: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5a9ec;
   L5a9ec: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5a9ef;
   L5a9ef: // MOV dword ptr [EBX + 0x2090],EAX
    W(unchecked(ebx+8336u),eax);goto L5a9f5;
   L5a9f5: // LEA EAX,[EBX + 0x2090]
    eax=unchecked(ebx+8336u);goto L5a9fb;
   L5a9fb: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5a9ff;
   L5a9ff: // MOV EAX,dword ptr [EBX + 0xfffffedc]
    eax=R(unchecked(ebx+4294967004u));goto L5aa05;
   L5aa05: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5aa09;
   L5aa09: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa0e;
   L5aa0e: // LEA ESI,[EBX + 0x2098]
    esi=unchecked(ebx+8344u);goto L5aa14;
   L5aa14: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L5aa1c;
   L5aa1c: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5aa1f;
   L5aa1f: // CALL 0x00048440
    try{eax=Call(0x48440,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa24;
   L5aa24: // MOV EAX,dword ptr [EBX + 0xfffffda0]
    eax=R(unchecked(ebx+4294966688u));goto L5aa2a;
   L5aa2a: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5aa2e;
   L5aa2e: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5aa31;
   L5aa31: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5aa35;
   L5aa35: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa3a;
   L5aa3a: // LEA ESI,[EBX + 0x20ac]
    esi=unchecked(ebx+8364u);goto L5aa40;
   L5aa40: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L5aa48;
   L5aa48: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5aa4b;
   L5aa4b: // CALL 0x0004a040
    try{eax=Call(0x4a040,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa50;
   L5aa50: // MOV EAX,dword ptr [EBX + 0xfffffe18]
    eax=R(unchecked(ebx+4294966808u));goto L5aa56;
   L5aa56: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5aa5a;
   L5aa5a: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5aa5d;
   L5aa5d: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5aa61;
   L5aa61: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa66;
   L5aa66: // LEA ESI,[EBX + 0x259c]
    esi=unchecked(ebx+9628u);goto L5aa6c;
   L5aa6c: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L5aa74;
   L5aa74: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5aa77;
   L5aa77: // CALL 0x00047eb0
    try{eax=Call(0x47eb0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa7c;
   L5aa7c: // MOV EAX,dword ptr [EBX + 0xfffffbbc]
    eax=R(unchecked(ebx+4294966204u));goto L5aa82;
   L5aa82: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5aa86;
   L5aa86: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5aa89;
   L5aa89: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5aa8d;
   L5aa8d: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aa92;
   L5aa92: // MOV EAX,dword ptr [EBX + 0xffffffd8]
    eax=R(unchecked(ebx+4294967256u));goto L5aa98;
   L5aa98: // LEA ESI,[EBX + 0x26d4]
    esi=unchecked(ebx+9940u);goto L5aa9e;
   L5aa9e: // MOV dword ptr [EBX + 0x25b0],0x1
    W(unchecked(ebx+9648u),1u);goto L5aaa8;
   L5aaa8: // MOV dword ptr [EBX + 0x25b4],0x0
    W(unchecked(ebx+9652u),0u);goto L5aab2;
   L5aab2: // MOV dword ptr [EBX + 0x25b8],0x0
    W(unchecked(ebx+9656u),0u);goto L5aabc;
   L5aabc: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5aabf;
   L5aabf: // MOV dword ptr [EBX + 0x25ac],EAX
    W(unchecked(ebx+9644u),eax);goto L5aac5;
   L5aac5: // MOV EAX,dword ptr [EBX + 0xfffffbe0]
    eax=R(unchecked(ebx+4294966240u));goto L5aacb;
   L5aacb: // MOV byte ptr [EBX + 0x25bc],0x0
    h.WriteByte(unchecked(ebx+9660u),unchecked((byte)(0u)));goto L5aad2;
   L5aad2: // MOV dword ptr [EBX + 0x25c0],0x0
    W(unchecked(ebx+9664u),0u);goto L5aadc;
   L5aadc: // MOV dword ptr [EBX + 0x25c4],0x0
    W(unchecked(ebx+9668u),0u);goto L5aae6;
   L5aae6: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5aae9;
   L5aae9: // MOV dword ptr [EBX + 0x26d4],EAX
    W(unchecked(ebx+9940u),eax);goto L5aaef;
   L5aaef: // LEA EAX,[EBX + 0x25ac]
    eax=unchecked(ebx+9644u);goto L5aaf5;
   L5aaf5: // MOV dword ptr [EBX + 0x25c8],0x0
    W(unchecked(ebx+9672u),0u);goto L5aaff;
   L5aaff: // MOV dword ptr [EBX + 0x25cc],0x0
    W(unchecked(ebx+9676u),0u);goto L5ab09;
   L5ab09: // MOV dword ptr [EBX + 0x25d0],0x0
    W(unchecked(ebx+9680u),0u);goto L5ab13;
   L5ab13: // MOV dword ptr [EBX + 0x25d4],0x0
    W(unchecked(ebx+9684u),0u);goto L5ab1d;
   L5ab1d: // MOV byte ptr [EBX + 0x26d0],0x0
    h.WriteByte(unchecked(ebx+9936u),unchecked((byte)(0u)));goto L5ab24;
   L5ab24: // MOV dword ptr [EBX + 0x26d8],0x1
    W(unchecked(ebx+9944u),1u);goto L5ab2e;
   L5ab2e: // MOV dword ptr [ESP + 0x24],EAX
    W(unchecked(esp+36u),eax);goto L5ab32;
   L5ab32: // MOV dword ptr [EBX + 0x26dc],EAX
    W(unchecked(ebx+9948u),eax);goto L5ab38;
   L5ab38: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5ab40;
   L5ab40: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5ab43;
   L5ab43: // CALL 0x000490e0
    try{eax=Call(0x490e0,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b2d8;}goto L5ab48;
   L5ab48: // MOV EDX,dword ptr [EBX + 0xfffffeec]
    edx=R(unchecked(ebx+4294967020u));goto L5ab4e;
   L5ab4e: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5ab52;
   L5ab52: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5ab55;
   L5ab55: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5ab59;
   L5ab59: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ab5e;
   L5ab5e: // MOV EAX,dword ptr [EBX + 0xffffffe0]
    eax=R(unchecked(ebx+4294967264u));goto L5ab64;
   L5ab64: // MOV dword ptr [EBX + 0x26e4],0x1
    W(unchecked(ebx+9956u),1u);goto L5ab6e;
   L5ab6e: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5ab71;
   L5ab71: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ab74;
   L5ab74: // MOV dword ptr [EBX + 0x26e0],EAX
    W(unchecked(ebx+9952u),eax);goto L5ab7a;
   L5ab7a: // LEA EAX,[EBX + 0x26e0]
    eax=unchecked(ebx+9952u);goto L5ab80;
   L5ab80: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5ab84;
   L5ab84: // MOV EAX,dword ptr [EBX + 0xfffffc70]
    eax=R(unchecked(ebx+4294966384u));goto L5ab8a;
   L5ab8a: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5ab8e;
   L5ab8e: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ab93;
   L5ab93: // MOV EAX,dword ptr [EBX + 0xfffffb64]
    eax=R(unchecked(ebx+4294966116u));goto L5ab99;
   L5ab99: // MOV dword ptr [EBX + 0x26ec],0x1
    W(unchecked(ebx+9964u),1u);goto L5aba3;
   L5aba3: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5aba6;
   L5aba6: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5aba9;
   L5aba9: // MOV dword ptr [EBX + 0x26e8],EAX
    W(unchecked(ebx+9960u),eax);goto L5abaf;
   L5abaf: // LEA EAX,[EBX + 0x26e8]
    eax=unchecked(ebx+9960u);goto L5abb5;
   L5abb5: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5abb9;
   L5abb9: // MOV EAX,dword ptr [EBX + 0xfffffe70]
    eax=R(unchecked(ebx+4294966896u));goto L5abbf;
   L5abbf: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5abc3;
   L5abc3: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5abc8;
   L5abc8: // MOV EAX,dword ptr [EBX + 0xfffffd88]
    eax=R(unchecked(ebx+4294966664u));goto L5abce;
   L5abce: // MOV dword ptr [EBX + 0x26f4],0x1
    W(unchecked(ebx+9972u),1u);goto L5abd8;
   L5abd8: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5abdb;
   L5abdb: // MOV dword ptr [EBX + 0x26f0],EAX
    W(unchecked(ebx+9968u),eax);goto L5abe1;
   L5abe1: // CALL 0x00049d10
    try{eax=Call(0x49d10);}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b2bd;}goto L5abe6;
   L5abe6: // MOV dword ptr [EBX + 0x26f8],EAX
    W(unchecked(ebx+9976u),eax);goto L5abec;
   L5abec: // LEA EAX,[EBX + 0x26f0]
    eax=unchecked(ebx+9968u);goto L5abf2;
   L5abf2: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5abf6;
   L5abf6: // MOV EAX,dword ptr [EBX + 0xfffffb38]
    eax=R(unchecked(ebx+4294966072u));goto L5abfc;
   L5abfc: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5abff;
   L5abff: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5ac03;
   L5ac03: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ac08;
   L5ac08: // MOV EAX,dword ptr [EBX + 0xfffffa74]
    eax=R(unchecked(ebx+4294965876u));goto L5ac0e;
   L5ac0e: // LEA ESI,[EBX + 0x277c]
    esi=unchecked(ebx+10108u);goto L5ac14;
   L5ac14: // MOV dword ptr [EBX + 0x2710],0x1
    W(unchecked(ebx+10000u),1u);goto L5ac1e;
   L5ac1e: // MOV dword ptr [EBX + 0x2714],0x0
    W(unchecked(ebx+10004u),0u);goto L5ac28;
   L5ac28: // MOV dword ptr [EBX + 0x2718],0x0
    W(unchecked(ebx+10008u),0u);goto L5ac32;
   L5ac32: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ac35;
   L5ac35: // MOV dword ptr [EBX + 0x270c],EAX
    W(unchecked(ebx+9996u),eax);goto L5ac3b;
   L5ac3b: // MOV EAX,dword ptr [EBX + 0xfffffad4]
    eax=R(unchecked(ebx+4294965972u));goto L5ac41;
   L5ac41: // MOV byte ptr [EBX + 0x271c],0x0
    h.WriteByte(unchecked(ebx+10012u),unchecked((byte)(0u)));goto L5ac48;
   L5ac48: // MOV dword ptr [EBX + 0x2720],0x0
    W(unchecked(ebx+10016u),0u);goto L5ac52;
   L5ac52: // MOV dword ptr [EBX + 0x2724],0x0
    W(unchecked(ebx+10020u),0u);goto L5ac5c;
   L5ac5c: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ac5f;
   L5ac5f: // MOV dword ptr [EBX + 0x277c],EAX
    W(unchecked(ebx+10108u),eax);goto L5ac65;
   L5ac65: // LEA EAX,[EBX + 0x270c]
    eax=unchecked(ebx+9996u);goto L5ac6b;
   L5ac6b: // MOV dword ptr [EBX + 0x2728],0x0
    W(unchecked(ebx+10024u),0u);goto L5ac75;
   L5ac75: // MOV dword ptr [EBX + 0x272c],0x0
    W(unchecked(ebx+10028u),0u);goto L5ac7f;
   L5ac7f: // MOV dword ptr [EBX + 0x2730],0x0
    W(unchecked(ebx+10032u),0u);goto L5ac89;
   L5ac89: // MOV dword ptr [EBX + 0x2734],0x0
    W(unchecked(ebx+10036u),0u);goto L5ac93;
   L5ac93: // MOV dword ptr [EBX + 0x2738],0x0
    W(unchecked(ebx+10040u),0u);goto L5ac9d;
   L5ac9d: // MOV dword ptr [EBX + 0x273c],0x0
    W(unchecked(ebx+10044u),0u);goto L5aca7;
   L5aca7: // MOV dword ptr [EBX + 0x2740],0x0
    W(unchecked(ebx+10048u),0u);goto L5acb1;
   L5acb1: // MOV byte ptr [EBX + 0x2744],0x0
    h.WriteByte(unchecked(ebx+10052u),unchecked((byte)(0u)));goto L5acb8;
   L5acb8: // MOV byte ptr [EBX + 0x2745],0x0
    h.WriteByte(unchecked(ebx+10053u),unchecked((byte)(0u)));goto L5acbf;
   L5acbf: // MOV byte ptr [EBX + 0x2746],0x0
    h.WriteByte(unchecked(ebx+10054u),unchecked((byte)(0u)));goto L5acc6;
   L5acc6: // MOV byte ptr [EBX + 0x2747],0x0
    h.WriteByte(unchecked(ebx+10055u),unchecked((byte)(0u)));goto L5accd;
   L5accd: // MOV byte ptr [EBX + 0x2748],0x0
    h.WriteByte(unchecked(ebx+10056u),unchecked((byte)(0u)));goto L5acd4;
   L5acd4: // MOV byte ptr [EBX + 0x2749],0x0
    h.WriteByte(unchecked(ebx+10057u),unchecked((byte)(0u)));goto L5acdb;
   L5acdb: // MOV byte ptr [EBX + 0x274a],0x0
    h.WriteByte(unchecked(ebx+10058u),unchecked((byte)(0u)));goto L5ace2;
   L5ace2: // MOV byte ptr [EBX + 0x274b],0x0
    h.WriteByte(unchecked(ebx+10059u),unchecked((byte)(0u)));goto L5ace9;
   L5ace9: // MOV byte ptr [EBX + 0x2778],0x0
    h.WriteByte(unchecked(ebx+10104u),unchecked((byte)(0u)));goto L5acf0;
   L5acf0: // MOV dword ptr [EBX + 0x2780],0x1
    W(unchecked(ebx+10112u),1u);goto L5acfa;
   L5acfa: // MOV dword ptr [ESP + 0x28],EAX
    W(unchecked(esp+40u),eax);goto L5acfe;
   L5acfe: // MOV dword ptr [EBX + 0x2784],EAX
    W(unchecked(ebx+10116u),eax);goto L5ad04;
   L5ad04: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L5ad0c;
   L5ad0c: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5ad14;
   L5ad14: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5ad17;
   L5ad17: // CALL 0x00049500
    try{eax=Call(0x49500,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b2ed;}goto L5ad1c;
   L5ad1c: // MOV EDX,dword ptr [EBX + 0xfffffd64]
    edx=R(unchecked(ebx+4294966628u));goto L5ad22;
   L5ad22: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5ad26;
   L5ad26: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5ad29;
   L5ad29: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5ad2d;
   L5ad2d: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ad32;
   L5ad32: // MOV EAX,dword ptr [EBX + 0xfffffc04]
    eax=R(unchecked(ebx+4294966276u));goto L5ad38;
   L5ad38: // LEA ESI,[EBX + 0x27fc]
    esi=unchecked(ebx+10236u);goto L5ad3e;
   L5ad3e: // MOV dword ptr [EBX + 0x2790],0x1
    W(unchecked(ebx+10128u),1u);goto L5ad48;
   L5ad48: // MOV dword ptr [EBX + 0x2794],0x0
    W(unchecked(ebx+10132u),0u);goto L5ad52;
   L5ad52: // MOV dword ptr [EBX + 0x2798],0x0
    W(unchecked(ebx+10136u),0u);goto L5ad5c;
   L5ad5c: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ad5f;
   L5ad5f: // MOV dword ptr [EBX + 0x278c],EAX
    W(unchecked(ebx+10124u),eax);goto L5ad65;
   L5ad65: // MOV EAX,dword ptr [EBX + 0xfffffd00]
    eax=R(unchecked(ebx+4294966528u));goto L5ad6b;
   L5ad6b: // MOV byte ptr [EBX + 0x279c],0x0
    h.WriteByte(unchecked(ebx+10140u),unchecked((byte)(0u)));goto L5ad72;
   L5ad72: // MOV dword ptr [EBX + 0x27a0],0x0
    W(unchecked(ebx+10144u),0u);goto L5ad7c;
   L5ad7c: // MOV dword ptr [EBX + 0x27a4],0x0
    W(unchecked(ebx+10148u),0u);goto L5ad86;
   L5ad86: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ad89;
   L5ad89: // MOV dword ptr [EBX + 0x27fc],EAX
    W(unchecked(ebx+10236u),eax);goto L5ad8f;
   L5ad8f: // LEA EAX,[EBX + 0x278c]
    eax=unchecked(ebx+10124u);goto L5ad95;
   L5ad95: // MOV dword ptr [EBX + 0x27a8],0x0
    W(unchecked(ebx+10152u),0u);goto L5ad9f;
   L5ad9f: // MOV dword ptr [EBX + 0x27ac],0x0
    W(unchecked(ebx+10156u),0u);goto L5ada9;
   L5ada9: // MOV dword ptr [EBX + 0x27b0],0x0
    W(unchecked(ebx+10160u),0u);goto L5adb3;
   L5adb3: // MOV dword ptr [EBX + 0x27b4],0x0
    W(unchecked(ebx+10164u),0u);goto L5adbd;
   L5adbd: // MOV dword ptr [EBX + 0x27b8],0x0
    W(unchecked(ebx+10168u),0u);goto L5adc7;
   L5adc7: // MOV dword ptr [EBX + 0x27bc],0x0
    W(unchecked(ebx+10172u),0u);goto L5add1;
   L5add1: // MOV dword ptr [EBX + 0x27c0],0x0
    W(unchecked(ebx+10176u),0u);goto L5addb;
   L5addb: // MOV byte ptr [EBX + 0x27c4],0x0
    h.WriteByte(unchecked(ebx+10180u),unchecked((byte)(0u)));goto L5ade2;
   L5ade2: // MOV byte ptr [EBX + 0x27c5],0x0
    h.WriteByte(unchecked(ebx+10181u),unchecked((byte)(0u)));goto L5ade9;
   L5ade9: // MOV byte ptr [EBX + 0x27c6],0x0
    h.WriteByte(unchecked(ebx+10182u),unchecked((byte)(0u)));goto L5adf0;
   L5adf0: // MOV byte ptr [EBX + 0x27c7],0x0
    h.WriteByte(unchecked(ebx+10183u),unchecked((byte)(0u)));goto L5adf7;
   L5adf7: // MOV byte ptr [EBX + 0x27c8],0x0
    h.WriteByte(unchecked(ebx+10184u),unchecked((byte)(0u)));goto L5adfe;
   L5adfe: // MOV byte ptr [EBX + 0x27c9],0x0
    h.WriteByte(unchecked(ebx+10185u),unchecked((byte)(0u)));goto L5ae05;
   L5ae05: // MOV byte ptr [EBX + 0x27ca],0x0
    h.WriteByte(unchecked(ebx+10186u),unchecked((byte)(0u)));goto L5ae0c;
   L5ae0c: // MOV byte ptr [EBX + 0x27cb],0x0
    h.WriteByte(unchecked(ebx+10187u),unchecked((byte)(0u)));goto L5ae13;
   L5ae13: // MOV byte ptr [EBX + 0x27f8],0x0
    h.WriteByte(unchecked(ebx+10232u),unchecked((byte)(0u)));goto L5ae1a;
   L5ae1a: // MOV dword ptr [EBX + 0x2800],0x1
    W(unchecked(ebx+10240u),1u);goto L5ae24;
   L5ae24: // MOV dword ptr [ESP + 0x2c],EAX
    W(unchecked(esp+44u),eax);goto L5ae28;
   L5ae28: // MOV dword ptr [EBX + 0x2804],EAX
    W(unchecked(ebx+10244u),eax);goto L5ae2e;
   L5ae2e: // MOV dword ptr [ESP + 0x8],0x0
    W(unchecked(esp+8u),0u);goto L5ae36;
   L5ae36: // MOV dword ptr [ESP + 0x4],0x0
    W(unchecked(esp+4u),0u);goto L5ae3e;
   L5ae3e: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5ae41;
   L5ae41: // CALL 0x00049c40
    try{eax=Call(0x49c40,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b25d;}goto L5ae46;
   L5ae46: // MOV EDX,dword ptr [EBX + 0xfffffbec]
    edx=R(unchecked(ebx+4294966252u));goto L5ae4c;
   L5ae4c: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5ae50;
   L5ae50: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5ae53;
   L5ae53: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L5ae57;
   L5ae57: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ae5c;
   L5ae5c: // MOV EAX,dword ptr [EBX + 0xfffffed0]
    eax=R(unchecked(ebx+4294966992u));goto L5ae62;
   L5ae62: // MOV dword ptr [EBX + 0x280c],0x1
    W(unchecked(ebx+10252u),1u);goto L5ae6c;
   L5ae6c: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5ae6f;
   L5ae6f: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5ae72;
   L5ae72: // MOV dword ptr [EBX + 0x2808],EAX
    W(unchecked(ebx+10248u),eax);goto L5ae78;
   L5ae78: // LEA EAX,[EBX + 0x2808]
    eax=unchecked(ebx+10248u);goto L5ae7e;
   L5ae7e: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5ae82;
   L5ae82: // MOV EAX,dword ptr [EBX + 0xffffff24]
    eax=R(unchecked(ebx+4294967076u));goto L5ae88;
   L5ae88: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5ae8c;
   L5ae8c: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5ae91;
   L5ae91: // MOV EAX,dword ptr [EBX + 0xfffffdac]
    eax=R(unchecked(ebx+4294966700u));goto L5ae97;
   L5ae97: // MOV dword ptr [EBX + 0x2814],0x1
    W(unchecked(ebx+10260u),1u);goto L5aea1;
   L5aea1: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5aea4;
   L5aea4: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5aea7;
   L5aea7: // MOV dword ptr [EBX + 0x2810],EAX
    W(unchecked(ebx+10256u),eax);goto L5aead;
   L5aead: // LEA EAX,[EBX + 0x2810]
    eax=unchecked(ebx+10256u);goto L5aeb3;
   L5aeb3: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5aeb7;
   L5aeb7: // MOV EAX,dword ptr [EBX + 0xffffffc4]
    eax=R(unchecked(ebx+4294967236u));goto L5aebd;
   L5aebd: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5aec1;
   L5aec1: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5aec6;
   L5aec6: // MOV EAX,dword ptr [EBX + 0xfffffa9c]
    eax=R(unchecked(ebx+4294965916u));goto L5aecc;
   L5aecc: // LEA EBP,[EBX + 0x282c]
    ebp=unchecked(ebx+10284u);goto L5aed2;
   L5aed2: // LEA ESI,[EBX + 0x28f4]
    esi=unchecked(ebx+10484u);goto L5aed8;
   L5aed8: // MOV dword ptr [EBX + 0x2830],0x1
    W(unchecked(ebx+10288u),1u);goto L5aee2;
   L5aee2: // MOV dword ptr [EBX + 0x2834],0x0
    W(unchecked(ebx+10292u),0u);goto L5aeec;
   L5aeec: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5aeef;
   L5aeef: // MOV dword ptr [EBX + 0x282c],EAX
    W(unchecked(ebx+10284u),eax);goto L5aef5;
   L5aef5: // MOV dword ptr [EBX + 0x2838],0x0
    W(unchecked(ebx+10296u),0u);goto L5aeff;
   L5aeff: // MOV dword ptr [EBX + 0x283c],0x0
    W(unchecked(ebx+10300u),0u);goto L5af09;
   L5af09: // MOV dword ptr [EBX + 0x2840],0x0
    W(unchecked(ebx+10304u),0u);goto L5af13;
   L5af13: // MOV dword ptr [EBX + 0x2844],0x0
    W(unchecked(ebx+10308u),0u);goto L5af1d;
   L5af1d: // MOV dword ptr [EBX + 0x2848],0x0
    W(unchecked(ebx+10312u),0u);goto L5af27;
   L5af27: // MOV dword ptr [EBX + 0x284c],0x0
    W(unchecked(ebx+10316u),0u);goto L5af31;
   L5af31: // MOV dword ptr [EBX + 0x2850],0x0
    W(unchecked(ebx+10320u),0u);goto L5af3b;
   L5af3b: // MOV dword ptr [EBX + 0x2854],0x0
    W(unchecked(ebx+10324u),0u);goto L5af45;
   L5af45: // MOV dword ptr [EBX + 0x2858],0x0
    W(unchecked(ebx+10328u),0u);goto L5af4f;
   L5af4f: // MOV dword ptr [EBX + 0x285c],0x0
    W(unchecked(ebx+10332u),0u);goto L5af59;
   L5af59: // MOV dword ptr [EBX + 0x2860],0x0
    W(unchecked(ebx+10336u),0u);goto L5af63;
   L5af63: // MOV dword ptr [EBX + 0x2864],0x0
    W(unchecked(ebx+10340u),0u);goto L5af6d;
   L5af6d: // MOV dword ptr [EBX + 0x2868],0x0
    W(unchecked(ebx+10344u),0u);goto L5af77;
   L5af77: // MOV dword ptr [EBX + 0x286c],0x0
    W(unchecked(ebx+10348u),0u);goto L5af81;
   L5af81: // MOV dword ptr [EBX + 0x2870],0x0
    W(unchecked(ebx+10352u),0u);goto L5af8b;
   L5af8b: // MOV dword ptr [EBX + 0x2874],0x0
    W(unchecked(ebx+10356u),0u);goto L5af95;
   L5af95: // MOV dword ptr [EBX + 0x2878],0x0
    W(unchecked(ebx+10360u),0u);goto L5af9f;
   L5af9f: // MOV dword ptr [EBX + 0x287c],0x0
    W(unchecked(ebx+10364u),0u);goto L5afa9;
   L5afa9: // MOV dword ptr [EBX + 0x2880],0x0
    W(unchecked(ebx+10368u),0u);goto L5afb3;
   L5afb3: // MOV dword ptr [EBX + 0x2884],0x0
    W(unchecked(ebx+10372u),0u);goto L5afbd;
   L5afbd: // MOV dword ptr [EBX + 0x2888],0x0
    W(unchecked(ebx+10376u),0u);goto L5afc7;
   L5afc7: // MOV dword ptr [EBX + 0x288c],0x0
    W(unchecked(ebx+10380u),0u);goto L5afd1;
   L5afd1: // MOV dword ptr [EBX + 0x2890],0x0
    W(unchecked(ebx+10384u),0u);goto L5afdb;
   L5afdb: // MOV dword ptr [EBX + 0x2894],0x0
    W(unchecked(ebx+10388u),0u);goto L5afe5;
   L5afe5: // MOV dword ptr [EBX + 0x2898],0x0
    W(unchecked(ebx+10392u),0u);goto L5afef;
   L5afef: // MOV dword ptr [EBX + 0x289c],0x0
    W(unchecked(ebx+10396u),0u);goto L5aff9;
   L5aff9: // MOV dword ptr [EBX + 0x28a0],0x0
    W(unchecked(ebx+10400u),0u);goto L5b003;
   L5b003: // MOV dword ptr [EBX + 0x28a4],0x0
    W(unchecked(ebx+10404u),0u);goto L5b00d;
   L5b00d: // MOV dword ptr [EBX + 0x28a8],0x0
    W(unchecked(ebx+10408u),0u);goto L5b017;
   L5b017: // MOV dword ptr [EBX + 0x28ac],0x0
    W(unchecked(ebx+10412u),0u);goto L5b021;
   L5b021: // MOV dword ptr [EBX + 0x28b0],0x0
    W(unchecked(ebx+10416u),0u);goto L5b02b;
   L5b02b: // MOV dword ptr [EBX + 0x28b4],0x0
    W(unchecked(ebx+10420u),0u);goto L5b035;
   L5b035: // MOV dword ptr [EBX + 0x28b8],0x0
    W(unchecked(ebx+10424u),0u);goto L5b03f;
   L5b03f: // MOV dword ptr [EBX + 0x28bc],0x0
    W(unchecked(ebx+10428u),0u);goto L5b049;
   L5b049: // MOV dword ptr [EBX + 0x28c0],0x0
    W(unchecked(ebx+10432u),0u);goto L5b053;
   L5b053: // MOV dword ptr [EBX + 0x28c4],0x0
    W(unchecked(ebx+10436u),0u);goto L5b05d;
   L5b05d: // MOV dword ptr [EBX + 0x28c8],0x0
    W(unchecked(ebx+10440u),0u);goto L5b067;
   L5b067: // MOV dword ptr [EBX + 0x28cc],0x0
    W(unchecked(ebx+10444u),0u);goto L5b071;
   L5b071: // MOV dword ptr [EBX + 0x28d0],0x0
    W(unchecked(ebx+10448u),0u);goto L5b07b;
   L5b07b: // MOV dword ptr [EBX + 0x28d4],0x0
    W(unchecked(ebx+10452u),0u);goto L5b085;
   L5b085: // MOV dword ptr [EBX + 0x28d8],0x0
    W(unchecked(ebx+10456u),0u);goto L5b08f;
   L5b08f: // MOV dword ptr [EBX + 0x28dc],0x0
    W(unchecked(ebx+10460u),0u);goto L5b099;
   L5b099: // MOV dword ptr [EBX + 0x28e0],0x0
    W(unchecked(ebx+10464u),0u);goto L5b0a3;
   L5b0a3: // MOV dword ptr [EBX + 0x28e4],0x0
    W(unchecked(ebx+10468u),0u);goto L5b0ad;
   L5b0ad: // MOV dword ptr [EBX + 0x28e8],0x0
    W(unchecked(ebx+10472u),0u);goto L5b0b7;
   L5b0b7: // MOV dword ptr [EBX + 0x28ec],0x0
    W(unchecked(ebx+10476u),0u);goto L5b0c1;
   L5b0c1: // MOV byte ptr [EBX + 0x28f0],0x0
    h.WriteByte(unchecked(ebx+10480u),unchecked((byte)(0u)));goto L5b0c8;
   L5b0c8: // MOV dword ptr [ESP + 0x8],0x1
    W(unchecked(esp+8u),1u);goto L5b0d0;
   L5b0d0: // MOV dword ptr [ESP + 0x4],EBP
    W(unchecked(esp+4u),ebp);goto L5b0d4;
   L5b0d4: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b0d7;
   L5b0d7: // CALL 0x0004a700
    try{eax=Call(0x4a700,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b0dc;
   L5b0dc: // MOV EAX,dword ptr [EBX + 0xfffffe3c]
    eax=R(unchecked(ebx+4294966844u));goto L5b0e2;
   L5b0e2: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5b0e6;
   L5b0e6: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5b0e9;
   L5b0e9: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5b0ed;
   L5b0ed: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b0f2;
   L5b0f2: // MOV EAX,dword ptr [EBX + 0xfffffff0]
    eax=R(unchecked(ebx+4294967280u));goto L5b0f8;
   L5b0f8: // MOV dword ptr [EBX + 0x290c],0x1
    W(unchecked(ebx+10508u),1u);goto L5b102;
   L5b102: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5b105;
   L5b105: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5b108;
   L5b108: // MOV dword ptr [EBX + 0x2908],EAX
    W(unchecked(ebx+10504u),eax);goto L5b10e;
   L5b10e: // LEA EAX,[EBX + 0x2908]
    eax=unchecked(ebx+10504u);goto L5b114;
   L5b114: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5b118;
   L5b118: // MOV EAX,dword ptr [EBX + 0xffffffc8]
    eax=R(unchecked(ebx+4294967240u));goto L5b11e;
   L5b11e: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5b122;
   L5b122: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b127;
   L5b127: // MOV EAX,dword ptr [EBX + 0xfffffa90]
    eax=R(unchecked(ebx+4294965904u));goto L5b12d;
   L5b12d: // MOV dword ptr [EBX + 0x2914],0x1
    W(unchecked(ebx+10516u),1u);goto L5b137;
   L5b137: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5b13a;
   L5b13a: // ADD EAX,0x8
    a=eax;b=8u;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;eax=v;goto L5b13d;
   L5b13d: // MOV dword ptr [EBX + 0x2910],EAX
    W(unchecked(ebx+10512u),eax);goto L5b143;
   L5b143: // LEA EAX,[EBX + 0x2910]
    eax=unchecked(ebx+10512u);goto L5b149;
   L5b149: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L5b14d;
   L5b14d: // MOV EAX,dword ptr [EBX + 0xffffff54]
    eax=R(unchecked(ebx+4294967124u));goto L5b153;
   L5b153: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5b157;
   L5b157: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b15c;
   L5b15c: // LEA ESI,[EBX + 0x2918]
    esi=unchecked(ebx+10520u);goto L5b162;
   L5b162: // MOV dword ptr [ESP + 0x4],0x1
    W(unchecked(esp+4u),1u);goto L5b16a;
   L5b16a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b16d;
   L5b16d: // CALL 0x0004a130
    try{eax=Call(0x4a130,R(unchecked(esp+0u)),R(unchecked(esp+4u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b172;
   L5b172: // MOV EAX,dword ptr [EBX + 0xfffffad0]
    eax=R(unchecked(ebx+4294965968u));goto L5b178;
   L5b178: // MOV dword ptr [ESP + 0x8],ESI
    W(unchecked(esp+8u),esi);goto L5b17c;
   L5b17c: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L5b17f;
   L5b17f: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L5b183;
   L5b183: // CALL 0x00049110
    try{eax=Call(0x49110,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)));}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);edx=h.ExceptionSelector(e);goto L5b24c;}goto L5b188;
   L5b188: // MOV EDX,dword ptr [EBX + 0xfffffdfc]
    edx=R(unchecked(ebx+4294966780u));goto L5b18e;
   L5b18e: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b191;
   L5b191: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L5b194;
   L5b194: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b199;
   L5b199: // MOV EDX,dword ptr [ESP + 0x14]
    edx=R(unchecked(esp+20u));goto L5b19d;
   L5b19d: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b1a0;
   L5b1a0: // MOV EAX,dword ptr [EBX + 0xfffffb78]
    eax=R(unchecked(ebx+4294966136u));goto L5b1a6;
   L5b1a6: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b1a9;
   L5b1a9: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b1ac;
   L5b1ac: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b1b1;
   L5b1b1: // MOV EDX,dword ptr [ESP + 0x18]
    edx=R(unchecked(esp+24u));goto L5b1b5;
   L5b1b5: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b1b8;
   L5b1b8: // MOV EAX,dword ptr [EBX + 0xfffffec4]
    eax=R(unchecked(ebx+4294966980u));goto L5b1be;
   L5b1be: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b1c1;
   L5b1c1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b1c4;
   L5b1c4: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b1c9;
   L5b1c9: // MOV EDX,dword ptr [ESP + 0x1c]
    edx=R(unchecked(esp+28u));goto L5b1cd;
   L5b1cd: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b1d0;
   L5b1d0: // MOV EAX,dword ptr [EBX + 0xffffff40]
    eax=R(unchecked(ebx+4294967104u));goto L5b1d6;
   L5b1d6: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b1d9;
   L5b1d9: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b1dc;
   L5b1dc: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b1e1;
   L5b1e1: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L5b1e5;
   L5b1e5: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b1e8;
   L5b1e8: // MOV EAX,dword ptr [EBX + 0xfffffeec]
    eax=R(unchecked(ebx+4294967020u));goto L5b1ee;
   L5b1ee: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b1f1;
   L5b1f1: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b1f4;
   L5b1f4: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b1f9;
   L5b1f9: // MOV EDX,dword ptr [ESP + 0x24]
    edx=R(unchecked(esp+36u));goto L5b1fd;
   L5b1fd: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b200;
   L5b200: // MOV EAX,dword ptr [EBX + 0xfffffd64]
    eax=R(unchecked(ebx+4294966628u));goto L5b206;
   L5b206: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b209;
   L5b209: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b20c;
   L5b20c: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b211;
   L5b211: // MOV EDX,dword ptr [ESP + 0x28]
    edx=R(unchecked(esp+40u));goto L5b215;
   L5b215: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b218;
   L5b218: // MOV EAX,dword ptr [EBX + 0xfffffbec]
    eax=R(unchecked(ebx+4294966252u));goto L5b21e;
   L5b21e: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b221;
   L5b221: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b224;
   L5b224: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b229;
   L5b229: // MOV EDX,dword ptr [ESP + 0x2c]
    edx=R(unchecked(esp+44u));goto L5b22d;
   L5b22d: // MOV dword ptr [ESI + EAX*0x4],EDX
    W(unchecked(esi+eax*0x4u),edx);goto L5b230;
   L5b230: // MOV EAX,dword ptr [EBX + 0xfffffe3c]
    eax=R(unchecked(ebx+4294966844u));goto L5b236;
   L5b236: // MOV ESI,dword ptr [EDI + 0xc]
    esi=R(unchecked(edi+12u));goto L5b239;
   L5b239: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b23c;
   L5b23c: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L5b241;
   L5b241: // MOV dword ptr [ESI + EAX*0x4],EBP
    W(unchecked(esi+eax*0x4u),ebp);goto L5b244;
   L5b244:return 0;
   L5b24c: // CMP EDX,-0x1
    a=edx;b=4294967295u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L5b24f;
   L5b24f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b252;
   L5b252: // JZ 0x0005b302
    if(zero)goto L5b302;goto L5b258;
   L5b258: // CALL 0x0004a490
    Call(0x4a490,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
   L5b25d: // MOV EDI,EAX
    edi=eax;goto L5b25f;
   L5b25f: // MOV EBP,EDX
    ebp=edx;goto L5b261;
   L5b261: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b264;
   L5b264: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b269;
   L5b269: // MOV EAX,EDI
    eax=edi;goto L5b26b;
   L5b26b: // MOV EDX,EBP
    edx=ebp;goto L5b26d;
   L5b26d: // JMP 0x0005b24c
    goto L5b24c;
   L5b26f: // MOV EDI,EAX
    edi=eax;goto L5b271;
   L5b271: // MOV EBP,EDX
    ebp=edx;goto L5b273;
   L5b273: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b276;
   L5b276: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b27b;
   L5b27b: // MOV EAX,EDI
    eax=edi;goto L5b27d;
   L5b27d: // MOV EDX,EBP
    edx=ebp;goto L5b27f;
   L5b27f: // JMP 0x0005b24c
    goto L5b24c;
   L5b281: // MOV EDI,EAX
    edi=eax;goto L5b283;
   L5b283: // MOV EBP,EDX
    ebp=edx;goto L5b285;
   L5b285: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b288;
   L5b288: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b28d;
   L5b28d: // MOV EAX,EDI
    eax=edi;goto L5b28f;
   L5b28f: // MOV EDX,EBP
    edx=ebp;goto L5b291;
   L5b291: // JMP 0x0005b24c
    goto L5b24c;
   L5b293: // MOV ESI,EAX
    esi=eax;goto L5b295;
   L5b295: // MOV EDI,EDX
    edi=edx;goto L5b297;
   L5b297: // LEA EAX,[EBX + 0x1ed0]
    eax=unchecked(ebx+7888u);goto L5b29d;
   L5b29d: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b2a0;
   L5b2a0: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b2a5;
   L5b2a5: // MOV EAX,ESI
    eax=esi;goto L5b2a7;
   L5b2a7: // MOV EDX,EDI
    edx=edi;goto L5b2a9;
   L5b2a9: // JMP 0x0005b24c
    goto L5b24c;
   L5b2ab: // MOV EDI,EAX
    edi=eax;goto L5b2ad;
   L5b2ad: // MOV EBP,EDX
    ebp=edx;goto L5b2af;
   L5b2af: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b2b2;
   L5b2b2: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b2b7;
   L5b2b7: // MOV EAX,EDI
    eax=edi;goto L5b2b9;
   L5b2b9: // MOV EDX,EBP
    edx=ebp;goto L5b2bb;
   L5b2bb: // JMP 0x0005b24c
    goto L5b24c;
   L5b2bd: // MOV ESI,EAX
    esi=eax;goto L5b2bf;
   L5b2bf: // MOV EDI,EDX
    edi=edx;goto L5b2c1;
   L5b2c1: // LEA EAX,[EBX + 0x26f0]
    eax=unchecked(ebx+9968u);goto L5b2c7;
   L5b2c7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L5b2ca;
   L5b2ca: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b2cf;
   L5b2cf: // MOV EAX,ESI
    eax=esi;goto L5b2d1;
   L5b2d1: // MOV EDX,EDI
    edx=edi;goto L5b2d3;
   L5b2d3: // JMP 0x0005b24c
    goto L5b24c;
   L5b2d8: // MOV EDI,EAX
    edi=eax;goto L5b2da;
   L5b2da: // MOV EBP,EDX
    ebp=edx;goto L5b2dc;
   L5b2dc: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b2df;
   L5b2df: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b2e4;
   L5b2e4: // MOV EAX,EDI
    eax=edi;goto L5b2e6;
   L5b2e6: // MOV EDX,EBP
    edx=ebp;goto L5b2e8;
   L5b2e8: // JMP 0x0005b24c
    goto L5b24c;
   L5b2ed: // MOV EDI,EAX
    edi=eax;goto L5b2ef;
   L5b2ef: // MOV EBP,EDX
    ebp=edx;goto L5b2f1;
   L5b2f1: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L5b2f4;
   L5b2f4: // CALL 0x0004a150
    eax=Call(0x4a150,R(unchecked(esp+0u)));goto L5b2f9;
   L5b2f9: // MOV EAX,EDI
    eax=edi;goto L5b2fb;
   L5b2fb: // MOV EDX,EBP
    edx=ebp;goto L5b2fd;
   L5b2fd: // JMP 0x0005b24c
    goto L5b24c;
   L5b302: // CALL 0x00048ba0
    Call(0x48ba0,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning operation returned");
  }
 }
}
