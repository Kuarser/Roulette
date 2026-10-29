using System;
namespace RouletteDeluxe
{
 // Physical packaged char ctype/num_put/num_get facet queries. Static C# labels
 // preserve original inlined vector control and LSDA cleanup; this is not a VM.
 // Frame is original ESP after prologue; locale owner is at +20. ID, dynamic cast and bad-cast operations remain required. Boolean methods return the original low byte.
 public sealed class OriginalConfigurationFacetLookup
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
  public OriginalConfigurationFacetLookup(Host host)
  {
   if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null||host.AtomicAdd==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null||host.ExceptionSelector==null)throw new ArgumentException("Original frame, strings/vector memory, COW atomics, imported token/parser/heap/destruction operations and native exception tokens required");
   h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte,AtomicAdd=host.AtomicAdd,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken,ExceptionSelector=host.ExceptionSelector};
  }
  uint R(uint at){return h.ReadWord(at);}
  uint B(uint at){return h.ReadByte(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public uint UseCtype(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7d52f;
   L7d52f: // MOV EAX,dword ptr [EBX + 0xffffffb8]
    eax=R(unchecked(ebx+4294967224u));goto L7d535;
   L7d535: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7d538;
   L7d538: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7d53d;
   L7d53d: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L7d541;
   L7d541: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L7d543;
   L7d543: // CMP EAX,dword ptr [EDX + 0x8]
    a=eax;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7d546;
   L7d546: // MOV ECX,dword ptr [EDX + 0x4]
    ecx=R(unchecked(edx+4u));goto L7d549;
   L7d549: // JNC 0x0007d57f
    if(!carry)goto L7d57f;goto L7d54b;
   L7d54b: // MOV EAX,dword ptr [ECX + EAX*0x4]
    eax=R(unchecked(ecx+eax*0x4u));goto L7d54e;
   L7d54e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7d550;
   L7d550: // JZ 0x0007d57f
    if(zero)goto L7d57f;goto L7d552;
   L7d552: // MOV EDX,dword ptr [EBX + 0xfffffc90]
    edx=R(unchecked(ebx+4294966416u));goto L7d558;
   L7d558: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7d560;
   L7d560: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7d563;
   L7d563: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L7d567;
   L7d567: // MOV EDX,dword ptr [EBX + 0xfffffe54]
    edx=R(unchecked(ebx+4294966868u));goto L7d56d;
   L7d56d: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L7d571;
   L7d571: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7d576;
   L7d576: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7d578;
   L7d578: // JZ 0x0007d585
    if(zero)goto L7d585;goto L7d57a;
   L7d57a:return eax;
   L7d57f: // NOP
    goto L7d580;
   L7d580: // CALL 0x00048a40
    Call(0x48a40);throw new InvalidOperationException("Native nonreturning operation returned");
   L7d585: // CALL 0x00048cb0
    Call(0x48cb0);throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint UseNumPut(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7dcbf;
   L7dcbf: // MOV EAX,dword ptr [EBX + 0xfffffccc]
    eax=R(unchecked(ebx+4294966476u));goto L7dcc5;
   L7dcc5: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7dcc8;
   L7dcc8: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7dccd;
   L7dccd: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L7dcd1;
   L7dcd1: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L7dcd3;
   L7dcd3: // CMP EAX,dword ptr [EDX + 0x8]
    a=eax;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7dcd6;
   L7dcd6: // MOV ECX,dword ptr [EDX + 0x4]
    ecx=R(unchecked(edx+4u));goto L7dcd9;
   L7dcd9: // JNC 0x0007dd0f
    if(!carry)goto L7dd0f;goto L7dcdb;
   L7dcdb: // MOV EAX,dword ptr [ECX + EAX*0x4]
    eax=R(unchecked(ecx+eax*0x4u));goto L7dcde;
   L7dcde: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7dce0;
   L7dce0: // JZ 0x0007dd0f
    if(zero)goto L7dd0f;goto L7dce2;
   L7dce2: // MOV EDX,dword ptr [EBX + 0xfffffd1c]
    edx=R(unchecked(ebx+4294966556u));goto L7dce8;
   L7dce8: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7dcf0;
   L7dcf0: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7dcf3;
   L7dcf3: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L7dcf7;
   L7dcf7: // MOV EDX,dword ptr [EBX + 0xfffffe54]
    edx=R(unchecked(ebx+4294966868u));goto L7dcfd;
   L7dcfd: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L7dd01;
   L7dd01: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7dd06;
   L7dd06: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7dd08;
   L7dd08: // JZ 0x0007dd15
    if(zero)goto L7dd15;goto L7dd0a;
   L7dd0a:return eax;
   L7dd0f: // NOP
    goto L7dd10;
   L7dd10: // CALL 0x00048a40
    Call(0x48a40);throw new InvalidOperationException("Native nonreturning operation returned");
   L7dd15: // CALL 0x00048cb0
    Call(0x48cb0);throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint UseNumGet(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7dd2f;
   L7dd2f: // MOV EAX,dword ptr [EBX + 0xfffffc74]
    eax=R(unchecked(ebx+4294966388u));goto L7dd35;
   L7dd35: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7dd38;
   L7dd38: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7dd3d;
   L7dd3d: // MOV EDX,dword ptr [ESP + 0x20]
    edx=R(unchecked(esp+32u));goto L7dd41;
   L7dd41: // MOV EDX,dword ptr [EDX]
    edx=R(unchecked(edx));goto L7dd43;
   L7dd43: // CMP EAX,dword ptr [EDX + 0x8]
    a=eax;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7dd46;
   L7dd46: // MOV ECX,dword ptr [EDX + 0x4]
    ecx=R(unchecked(edx+4u));goto L7dd49;
   L7dd49: // JNC 0x0007dd7f
    if(!carry)goto L7dd7f;goto L7dd4b;
   L7dd4b: // MOV EAX,dword ptr [ECX + EAX*0x4]
    eax=R(unchecked(ecx+eax*0x4u));goto L7dd4e;
   L7dd4e: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7dd50;
   L7dd50: // JZ 0x0007dd7f
    if(zero)goto L7dd7f;goto L7dd52;
   L7dd52: // MOV EDX,dword ptr [EBX + 0xfffffe5c]
    edx=R(unchecked(ebx+4294966876u));goto L7dd58;
   L7dd58: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7dd60;
   L7dd60: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7dd63;
   L7dd63: // MOV dword ptr [ESP + 0x8],EDX
    W(unchecked(esp+8u),edx);goto L7dd67;
   L7dd67: // MOV EDX,dword ptr [EBX + 0xfffffe54]
    edx=R(unchecked(ebx+4294966868u));goto L7dd6d;
   L7dd6d: // MOV dword ptr [ESP + 0x4],EDX
    W(unchecked(esp+4u),edx);goto L7dd71;
   L7dd71: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7dd76;
   L7dd76: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7dd78;
   L7dd78: // JZ 0x0007dd85
    if(zero)goto L7dd85;goto L7dd7a;
   L7dd7a:return eax;
   L7dd7f: // NOP
    goto L7dd80;
   L7dd80: // CALL 0x00048a40
    Call(0x48a40);throw new InvalidOperationException("Native nonreturning operation returned");
   L7dd85: // CALL 0x00048cb0
    Call(0x48cb0);throw new InvalidOperationException("Native nonreturning operation returned");
  }
  public uint HasCtype(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7eb86;
   L7eb86: // MOV EAX,dword ptr [EBX + 0xffffffb8]
    eax=R(unchecked(ebx+4294967224u));goto L7eb8c;
   L7eb8c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7eb8f;
   L7eb8f: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7eb94;
   L7eb94: // MOV ECX,EAX
    ecx=eax;goto L7eb96;
   L7eb96: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L7eb9a;
   L7eb9a: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L7eb9c;
   L7eb9c: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L7eb9e;
   L7eb9e: // CMP ECX,dword ptr [EDX + 0x8]
    a=ecx;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7eba1;
   L7eba1: // MOV ESI,dword ptr [EDX + 0x4]
    esi=R(unchecked(edx+4u));goto L7eba4;
   L7eba4: // JNC 0x0007ebd6
    if(!carry)goto L7ebd6;goto L7eba6;
   L7eba6: // MOV EDX,dword ptr [ESI + ECX*0x4]
    edx=R(unchecked(esi+ecx*0x4u));goto L7eba9;
   L7eba9: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ebab;
   L7ebab: // JZ 0x0007ebd6
    if(zero)goto L7ebd6;goto L7ebad;
   L7ebad: // MOV EAX,dword ptr [EBX + 0xfffffc90]
    eax=R(unchecked(ebx+4294966416u));goto L7ebb3;
   L7ebb3: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7ebbb;
   L7ebbb: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L7ebbe;
   L7ebbe: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L7ebc2;
   L7ebc2: // MOV EAX,dword ptr [EBX + 0xfffffe54]
    eax=R(unchecked(ebx+4294966868u));goto L7ebc8;
   L7ebc8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7ebcc;
   L7ebcc: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7ebd1;
   L7ebd1: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ebd3;
   L7ebd3: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ebd6;
   L7ebd6:return (eax&255u);
  }
  public uint HasNumPut(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7ed86;
   L7ed86: // MOV EAX,dword ptr [EBX + 0xfffffccc]
    eax=R(unchecked(ebx+4294966476u));goto L7ed8c;
   L7ed8c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7ed8f;
   L7ed8f: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7ed94;
   L7ed94: // MOV ECX,EAX
    ecx=eax;goto L7ed96;
   L7ed96: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L7ed9a;
   L7ed9a: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L7ed9c;
   L7ed9c: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L7ed9e;
   L7ed9e: // CMP ECX,dword ptr [EDX + 0x8]
    a=ecx;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7eda1;
   L7eda1: // MOV ESI,dword ptr [EDX + 0x4]
    esi=R(unchecked(edx+4u));goto L7eda4;
   L7eda4: // JNC 0x0007edd6
    if(!carry)goto L7edd6;goto L7eda6;
   L7eda6: // MOV EDX,dword ptr [ESI + ECX*0x4]
    edx=R(unchecked(esi+ecx*0x4u));goto L7eda9;
   L7eda9: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7edab;
   L7edab: // JZ 0x0007edd6
    if(zero)goto L7edd6;goto L7edad;
   L7edad: // MOV EAX,dword ptr [EBX + 0xfffffd1c]
    eax=R(unchecked(ebx+4294966556u));goto L7edb3;
   L7edb3: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7edbb;
   L7edbb: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L7edbe;
   L7edbe: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L7edc2;
   L7edc2: // MOV EAX,dword ptr [EBX + 0xfffffe54]
    eax=R(unchecked(ebx+4294966868u));goto L7edc8;
   L7edc8: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7edcc;
   L7edcc: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7edd1;
   L7edd1: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7edd3;
   L7edd3: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7edd6;
   L7edd6:return (eax&255u);
  }
  public uint HasNumGet(uint frame,uint owner)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   W(frame+0x20u,owner);goto L7ee06;
   L7ee06: // MOV EAX,dword ptr [EBX + 0xfffffc74]
    eax=R(unchecked(ebx+4294966388u));goto L7ee0c;
   L7ee0c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L7ee0f;
   L7ee0f: // CALL 0x00048b50
    eax=Call(0x48b50,R(unchecked(esp+0u)));goto L7ee14;
   L7ee14: // MOV ECX,EAX
    ecx=eax;goto L7ee16;
   L7ee16: // MOV EAX,dword ptr [ESP + 0x20]
    eax=R(unchecked(esp+32u));goto L7ee1a;
   L7ee1a: // MOV EDX,dword ptr [EAX]
    edx=R(unchecked(eax));goto L7ee1c;
   L7ee1c: // XOR EAX,EAX
    a=eax;b=eax;v=unchecked(a^b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;eax=v;goto L7ee1e;
   L7ee1e: // CMP ECX,dword ptr [EDX + 0x8]
    a=ecx;b=R(unchecked(edx+8u));v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L7ee21;
   L7ee21: // MOV ESI,dword ptr [EDX + 0x4]
    esi=R(unchecked(edx+4u));goto L7ee24;
   L7ee24: // JNC 0x0007ee56
    if(!carry)goto L7ee56;goto L7ee26;
   L7ee26: // MOV EDX,dword ptr [ESI + ECX*0x4]
    edx=R(unchecked(esi+ecx*0x4u));goto L7ee29;
   L7ee29: // TEST EDX,EDX
    a=edx;b=edx;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ee2b;
   L7ee2b: // JZ 0x0007ee56
    if(zero)goto L7ee56;goto L7ee2d;
   L7ee2d: // MOV EAX,dword ptr [EBX + 0xfffffe5c]
    eax=R(unchecked(ebx+4294966876u));goto L7ee33;
   L7ee33: // MOV dword ptr [ESP + 0xc],0x0
    W(unchecked(esp+12u),0u);goto L7ee3b;
   L7ee3b: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L7ee3e;
   L7ee3e: // MOV dword ptr [ESP + 0x8],EAX
    W(unchecked(esp+8u),eax);goto L7ee42;
   L7ee42: // MOV EAX,dword ptr [EBX + 0xfffffe54]
    eax=R(unchecked(ebx+4294966868u));goto L7ee48;
   L7ee48: // MOV dword ptr [ESP + 0x4],EAX
    W(unchecked(esp+4u),eax);goto L7ee4c;
   L7ee4c: // CALL 0x00049ca0
    eax=Call(0x49ca0,R(unchecked(esp+0u)),R(unchecked(esp+4u)),R(unchecked(esp+8u)),R(unchecked(esp+12u)));goto L7ee51;
   L7ee51: // TEST EAX,EAX
    a=eax;b=eax;v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L7ee53;
   L7ee53: // SETNZ AL
    eax=(eax&0xffffff00u)|((zero?0u:1u)&255u);goto L7ee56;
   L7ee56:return (eax&255u);
  }
 }
}
