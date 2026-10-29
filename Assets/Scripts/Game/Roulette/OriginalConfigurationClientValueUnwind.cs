using System;
namespace RouletteDeluxe
{
 // Original GetOptionValue landing control; generated static C# labels from
 // byte-verified native assembly. Imported cleanup bodies remain mandatory.
 public sealed class OriginalConfigurationClientValueUnwind
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,uint[],uint> Invoke;
   public Func<Exception,bool> IsNativeUnwind;
   public Func<Exception,uint> ExceptionToken;
  }
  readonly Host h;
  public OriginalConfigurationClientValueUnwind(Host host)
  {
   if(host==null||host.ReadWord==null||host.WriteWord==null||host.Invoke==null||host.IsNativeUnwind==null||host.ExceptionToken==null)throw new ArgumentException("Physical frame, preserved registers, native exception token and original cleanup/catch/resume imports required");
   h=new Host{ReadWord=host.ReadWord,WriteWord=host.WriteWord,Invoke=host.Invoke,IsNativeUnwind=host.IsNativeUnwind,ExceptionToken=host.ExceptionToken};
  }
  uint R(uint at){return h.ReadWord(at);}
  void W(uint at,uint value){h.WriteWord(at,value);}
  uint Call(uint method,params uint[] args){return h.Invoke(method,args);}
  public void Dispatch(uint landing,uint frame,uint exceptionToken,uint incomingEbx,uint incomingEsi,uint incomingEdi)
  {
   uint eax=exceptionToken,ebx=incomingEbx,esi=incomingEsi,edi=incomingEdi,ecx=0,edx=0,ebp=frame,esp=unchecked(frame-0x1c8u);bool zero=false;
   switch(landing)
   {
    case 0x94b2388:goto L94b2388;
    case 0x94b23f6:goto L94b23f6;
    case 0x94b2460:goto L94b2460;
    case 0x94b2472:goto L94b2472;
    case 0x94b2487:goto L94b2487;
    case 0x94b24d7:goto L94b24d7;
    case 0x94b2595:goto L94b2595;
    case 0x94b2607:goto L94b2607;
    case 0x94b262f:goto L94b262f;
    case 0x94b2647:goto L94b2647;
    case 0x94b2902:goto L94b2902;
    case 0x94b2920:goto L94b2920;
    case 0x94b292f:goto L94b292f;
    case 0x94b293e:goto L94b293e;
    case 0x94b2959:goto L94b2959;
    case 0x94b29b0:goto L94b29b0;
    case 0x94b29d6:goto L94b29d6;
    case 0x94b29ef:goto L94b29ef;
    case 0x94b29fd:goto L94b29fd;
    case 0x94b2a04:goto L94b2a04;
    case 0x94b2a73:goto L94b2a73;
    case 0x94b2aa9:goto L94b2aa9;
    case 0x94b2ab5:goto L94b2ab5;
    case 0x94b2aca:goto L94b2aca;
    case 0x94b2add:goto L94b2add;
    case 0x94b2af9:goto L94b2af9;
    case 0x94b2b50:goto L94b2b50;
    case 0x94b2b57:goto L94b2b57;
    case 0x94b2b91:goto L94b2b91;
    case 0x94b2b93:goto L94b2b93;
    case 0x94b2ba2:goto L94b2ba2;
    case 0x94b2bbf:goto L94b2bbf;
    case 0x94b2bd4:goto L94b2bd4;
    case 0x94b2be2:goto L94b2be2;
    case 0x94b2c5a:goto L94b2c5a;
    case 0x94b2c7d:goto L94b2c7d;
    case 0x94b2c8e:goto L94b2c8e;
    case 0x94b2d21:goto L94b2d21;
    case 0x94b2da6:goto L94b2da6;
    case 0x94b2de6:goto L94b2de6;
    case 0x94b2e0b:goto L94b2e0b;
    case 0x94b2e34:goto L94b2e34;
    case 0x94b2e7b:goto L94b2e7b;
    case 0x94b2e9c:goto L94b2e9c;
    default:throw new ArgumentException("Unknown original GetOptionValue landing pad");
   }
   L94b2388: // MOV EBX,EAX
    ebx=eax;goto L94b238a;
   L94b238a: // LEA EDX,[EBP + 0xfffffeb0]
    edx=unchecked(ebp+4294966960u);goto L94b2390;
   L94b2390: // MOV dword ptr [EBP + 0xfffffeb0],0x9b49c10
    W(unchecked(ebp+4294966960u),162831376u);goto L94b239a;
   L94b239a: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b239d;
   L94b239d: // CALL 0x080b0fc4
    eax=Call(0x80b0fc4,R(unchecked(esp+0u)));goto L94b23a2;
   L94b23a2: // LEA EAX,[EBP + 0xffffff78]
    eax=unchecked(ebp+4294967160u);goto L94b23a8;
   L94b23a8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b23ab;
   L94b23ab: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b23b0;
   L94b23b0: // LEA EDX,[EBP + 0xffffff58]
    edx=unchecked(ebp+4294967128u);goto L94b23b6;
   L94b23b6: // MOV dword ptr [EBP + 0xfffffe4c],EDX
    W(unchecked(ebp+4294966860u),edx);goto L94b23bc;
   L94b23bc: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b23bf;
   L94b23bf: // CALL 0x08df1640
    eax=Call(0x8df1640,R(unchecked(esp+0u)));goto L94b23c4;
   L94b23c4: // LEA EDX,[EBP + 0xffffff6c]
    edx=unchecked(ebp+4294967148u);goto L94b23ca;
   L94b23ca: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b23cd;
   L94b23cd: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b23d2;
   L94b23d2: // MOV EAX,dword ptr [EBP + 0xfffffe4c]
    eax=R(unchecked(ebp+4294966860u));goto L94b23d8;
   L94b23d8: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b23db;
   L94b23db: // CALL 0x084cb610
    eax=Call(0x84cb610,R(unchecked(esp+0u)));goto L94b23e0;
   L94b23e0: // MOV EDX,dword ptr [EBP + 0xfffffe70]
    edx=R(unchecked(ebp+4294966896u));goto L94b23e6;
   L94b23e6: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b23e9;
   L94b23e9: // CALL 0x08df1640
    eax=Call(0x8df1640,R(unchecked(esp+0u)));goto L94b23ee;
   L94b23ee: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b23f1;
   L94b23f1: // CALL 0x080aed64
    Call(0x80aed64,R(unchecked(esp+0u)));throw new InvalidOperationException("Native nonreturning Resume returned");
   L94b23f6: // MOV EBX,EAX
    ebx=eax;goto L94b23f8;
   L94b23f8: // LEA EAX,[EBP + 0xfffffeac]
    eax=unchecked(ebp+4294966956u);goto L94b23fe;
   L94b23fe: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2401;
   L94b2401: // MOV dword ptr [EBP + 0xfffffe88],0x9b497c8
    W(unchecked(ebp+4294966920u),162830280u);goto L94b240b;
   L94b240b: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2410;
   L94b2410: // MOV dword ptr [EBP + 0xfffffe88],0x9b48a28
    W(unchecked(ebp+4294966920u),162826792u);goto L94b241a;
   L94b241a: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b241d;
   L94b241d: // CALL 0x080ad934
    eax=Call(0x80ad934,R(unchecked(esp+0u)));goto L94b2422;
   L94b2422: // LEA EAX,[EBP + 0xfffffe7c]
    eax=unchecked(ebp+4294966908u);goto L94b2428;
   L94b2428: // MOV dword ptr [ESP + 0x4],0x9b49de4
    W(unchecked(esp+4u),162831844u);goto L94b2430;
   L94b2430: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2433;
   L94b2433: // CALL 0x080b0dd4
    eax=Call(0x80b0dd4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b2438;
   L94b2438: // LEA EDX,[EBP + 0xfffffeb0]
    edx=unchecked(ebp+4294966960u);goto L94b243e;
   L94b243e: // MOV dword ptr [EBP + 0xfffffeb0],0x9b49c10
    W(unchecked(ebp+4294966960u),162831376u);goto L94b2448;
   L94b2448: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b244b;
   L94b244b: // CALL 0x080b0fc4
    eax=Call(0x80b0fc4,R(unchecked(esp+0u)));goto L94b2450;
   L94b2450: // LEA ECX,[EBP + -0x60]
    ecx=unchecked(ebp+4294967200u);goto L94b2453;
   L94b2453: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b2456;
   L94b2456: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b245b;
   L94b245b: // JMP 0x094b23bc
    goto L94b23bc;
   L94b2460: // MOV EBX,EAX
    ebx=eax;goto L94b2462;
   L94b2462: // LEA EDX,[EBP + 0xfffffe7c]
    edx=unchecked(ebp+4294966908u);goto L94b2468;
   L94b2468: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b246b;
   L94b246b: // CALL 0x080ac134
    eax=Call(0x80ac134,R(unchecked(esp+0u)));goto L94b2470;
   L94b2470: // JMP 0x094b2450
    goto L94b2450;
   L94b2472: // LEA EDX,[EBP + -0x4c]
    edx=unchecked(ebp+4294967220u);goto L94b2475;
   L94b2475: // MOV EBX,EAX
    ebx=eax;goto L94b2477;
   L94b2477: // MOV dword ptr [EBP + 0xfffffe58],EDX
    W(unchecked(ebp+4294966872u),edx);goto L94b247d;
   L94b247d: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b2480;
   L94b2480: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2485;
   L94b2485: // JMP 0x094b2462
    goto L94b2462;
   L94b2487: // MOV EBX,EAX
    ebx=eax;goto L94b2489;
   L94b2489: // LEA EAX,[EBP + -0x44]
    eax=unchecked(ebp+4294967228u);goto L94b248c;
   L94b248c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b248f;
   L94b248f: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2494;
   L94b2494: // MOV EDX,dword ptr [EBP + 0xfffffe58]
    edx=R(unchecked(ebp+4294966872u));goto L94b249a;
   L94b249a: // JMP 0x094b247d
    goto L94b247d;
   L94b24d7: // MOV EBX,EAX
    ebx=eax;goto L94b24d9;
   L94b24d9: // MOV EAX,dword ptr [EBP + 0xfffffe58]
    eax=R(unchecked(ebp+4294966872u));goto L94b24df;
   L94b24df: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b24e2;
   L94b24e2: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b24e7;
   L94b24e7: // JMP 0x094b2462
    goto L94b2462;
   L94b2595: // MOV EBX,EAX
    ebx=eax;goto L94b2597;
   L94b2597: // JMP 0x094b2450
    goto L94b2450;
   L94b2607: // MOV EBX,EAX
    ebx=eax;goto L94b2609;
   L94b2609: // LEA EAX,[EBP + -0x4c]
    eax=unchecked(ebp+4294967220u);goto L94b260c;
   L94b260c: // MOV dword ptr [EBP + 0xfffffe58],EAX
    W(unchecked(ebp+4294966872u),eax);goto L94b2612;
   L94b2612: // MOV EDX,EAX
    edx=eax;goto L94b2614;
   L94b2614: // JMP 0x094b247d
    goto L94b247d;
   L94b262f: // LEA ECX,[EBP + -0x48]
    ecx=unchecked(ebp+4294967224u);goto L94b2632;
   L94b2632: // MOV EBX,EAX
    ebx=eax;goto L94b2634;
   L94b2634: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b2637;
   L94b2637: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b263c;
   L94b263c: // MOV EDX,dword ptr [EBP + 0xfffffe58]
    edx=R(unchecked(ebp+4294966872u));goto L94b2642;
   L94b2642: // JMP 0x094b247d
    goto L94b247d;
   L94b2647: // MOV EBX,EAX
    ebx=eax;goto L94b2649;
   L94b2649: // MOV EDX,dword ptr [EBP + 0xfffffe58]
    edx=R(unchecked(ebp+4294966872u));goto L94b264f;
   L94b264f: // JMP 0x094b247d
    goto L94b247d;
   L94b2902: // MOV EBX,EAX
    ebx=eax;goto L94b2904;
   L94b2904: // LEA EDX,[EBP + 0xffffff58]
    edx=unchecked(ebp+4294967128u);goto L94b290a;
   L94b290a: // MOV dword ptr [EBP + 0xfffffe4c],EDX
    W(unchecked(ebp+4294966860u),edx);goto L94b2910;
   L94b2910: // JMP 0x094b23bc
    goto L94b23bc;
   L94b2920: // MOV EBX,EAX
    ebx=eax;goto L94b2922;
   L94b2922: // LEA EAX,[EBP + -0x6c]
    eax=unchecked(ebp+4294967188u);goto L94b2925;
   L94b2925: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2928;
   L94b2928: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b292d;
   L94b292d: // JMP 0x094b2904
    goto L94b2904;
   L94b292f: // MOV EBX,EAX
    ebx=eax;goto L94b2931;
   L94b2931: // LEA EAX,[EBP + -0x70]
    eax=unchecked(ebp+4294967184u);goto L94b2934;
   L94b2934: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2937;
   L94b2937: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b293c;
   L94b293c: // JMP 0x094b2922
    goto L94b2922;
   L94b293e: // MOV EBX,EAX
    ebx=eax;goto L94b2940;
   L94b2940: // JMP 0x094b23e0
    goto L94b23e0;
   L94b2959: // LEA EDX,[EBP + -0x64]
    edx=unchecked(ebp+4294967196u);goto L94b295c;
   L94b295c: // MOV EBX,EAX
    ebx=eax;goto L94b295e;
   L94b295e: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b2961;
   L94b2961: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2966;
   L94b2966: // JMP 0x094b2931
    goto L94b2931;
   L94b29b0: // MOV EBX,EAX
    ebx=eax;goto L94b29b2;
   L94b29b2: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b29b5;
   L94b29b5: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b29ba;
   L94b29ba: // LEA EDX,[EBP + 0xffffff58]
    edx=unchecked(ebp+4294967128u);goto L94b29c0;
   L94b29c0: // MOV dword ptr [EBP + 0xfffffe4c],EDX
    W(unchecked(ebp+4294966860u),edx);goto L94b29c6;
   L94b29c6: // JMP 0x094b23c4
    goto L94b23c4;
   L94b29d6: // LEA EDI,[EBP + 0xffffff48]
    edi=unchecked(ebp+4294967112u);goto L94b29dc;
   L94b29dc: // LEA ECX,[EBP + 0xffffff58]
    ecx=unchecked(ebp+4294967128u);goto L94b29e2;
   L94b29e2: // MOV EBX,EAX
    ebx=eax;goto L94b29e4;
   L94b29e4: // MOV dword ptr [EBP + 0xfffffe4c],ECX
    W(unchecked(ebp+4294966860u),ecx);goto L94b29ea;
   L94b29ea: // JMP 0x094b23bc
    goto L94b23bc;
   L94b29ef: // MOV ESI,EAX
    esi=eax;goto L94b29f1;
   L94b29f1: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b29f4;
   L94b29f4: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b29f9;
   L94b29f9: // MOV EAX,ESI
    eax=esi;goto L94b29fb;
   L94b29fb: // JMP 0x094b29dc
    goto L94b29dc;
   L94b29fd: // MOV EBX,EAX
    ebx=eax;goto L94b29ff;
   L94b29ff: // JMP 0x094b2438
    goto L94b2438;
   L94b2a04: // MOV EDX,dword ptr [EBX + -0xc]
    edx=R(unchecked(ebx+4294967284u));goto L94b2a07;
   L94b2a07: // MOV dword ptr [EBP + 0xfffffe7c],EBX
    W(unchecked(ebp+4294966908u),ebx);goto L94b2a0d;
   L94b2a0d: // MOV EBX,EAX
    ebx=eax;goto L94b2a0f;
   L94b2a0f: // MOV dword ptr [EBP + EDX*0x1 + 0xfffffe7c],ESI
    W(unchecked(ebp+edx*0x1u+4294966908u),esi);goto L94b2a16;
   L94b2a16: // MOV dword ptr [EBP + 0xfffffe80],0x0
    W(unchecked(ebp+4294966912u),0u);goto L94b2a20;
   L94b2a20: // JMP 0x094b2438
    goto L94b2438;
   L94b2a73: // MOV EBX,EAX
    ebx=eax;goto L94b2a75;
   L94b2a75: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b2a78;
   L94b2a78: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2a7d;
   L94b2a7d: // MOV EAX,EBX
    eax=ebx;goto L94b2a7f;
   L94b2a7f: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2a82;
   L94b2a82: // CALL 0x080adff4
    eax=Call(0x80adff4,R(unchecked(esp+0u)));goto L94b2a87;
   L94b2a87: // CMP dword ptr [EBP + 0xfffffe58],ESI
    zero=R(unchecked(ebp+4294966872u))==esi;goto L94b2a8d;
   L94b2a8d: // JZ 0x094b2aa4
    if(zero)goto L94b2aa4;goto L94b2a8f;
   L94b2a8f: // MOV EBX,dword ptr [EBP + 0xfffffe58]
    ebx=R(unchecked(ebp+4294966872u));goto L94b2a95;
   L94b2a95: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b2a98;
   L94b2a98: // ADD EBX,0x10
    ebx=unchecked(ebx+16u);goto L94b2a9b;
   L94b2a9b: // CALL 0x081d7ac0
    eax=Call(0x81d7ac0,R(unchecked(esp+0u)));goto L94b2aa0;
   L94b2aa0: // CMP EBX,ESI
    zero=ebx==esi;goto L94b2aa2;
   L94b2aa2: // JNZ 0x094b2a95
    if(!zero)goto L94b2a95;goto L94b2aa4;
   L94b2aa4: // CALL 0x080b1284
    try{Call(0x80b1284);throw new InvalidOperationException("Native nonreturning rethrow returned");}catch(Exception e){if(!h.IsNativeUnwind(e))throw;eax=h.ExceptionToken(e);goto L94b2add;}
   L94b2aa9: // MOV EBX,EAX
    ebx=eax;goto L94b2aab;
   L94b2aab: // MOV dword ptr [ESP],EDI
    W(unchecked(esp),edi);goto L94b2aae;
   L94b2aae: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2ab3;
   L94b2ab3: // JMP 0x094b2a75
    goto L94b2a75;
   L94b2ab5: // LEA ECX,[EBP + 0xffffff58]
    ecx=unchecked(ebp+4294967128u);goto L94b2abb;
   L94b2abb: // MOV EBX,EAX
    ebx=eax;goto L94b2abd;
   L94b2abd: // MOV dword ptr [EBP + 0xfffffe4c],ECX
    W(unchecked(ebp+4294966860u),ecx);goto L94b2ac3;
   L94b2ac3: // MOV EAX,ECX
    eax=ecx;goto L94b2ac5;
   L94b2ac5: // JMP 0x094b23d8
    goto L94b23d8;
   L94b2aca: // MOV EBX,EAX
    ebx=eax;goto L94b2acc;
   L94b2acc: // LEA EAX,[EBP + 0xffffff58]
    eax=unchecked(ebp+4294967128u);goto L94b2ad2;
   L94b2ad2: // MOV dword ptr [EBP + 0xfffffe4c],EAX
    W(unchecked(ebp+4294966860u),eax);goto L94b2ad8;
   L94b2ad8: // JMP 0x094b23c4
    goto L94b23c4;
   L94b2add: // MOV EBX,EAX
    ebx=eax;goto L94b2adf;
   L94b2adf: // CALL 0x080b1144
    eax=Call(0x80b1144);goto L94b2ae4;
   L94b2ae4: // MOV EAX,dword ptr [EBP + 0xffffff58]
    eax=R(unchecked(ebp+4294967128u));goto L94b2aea;
   L94b2aea: // CALL 0x094abad0
    eax=Call(0x94abad0,eax);goto L94b2aef;
   L94b2aef: // JMP 0x094b23e0
    goto L94b23e0;
   L94b2af9: // LEA ESI,[ESI]
    esi=unchecked(esi);goto L94b2b00;
   L94b2b00: // JMP 0x094b2a7f
    goto L94b2a7f;
   L94b2b50: // MOV EBX,EAX
    ebx=eax;goto L94b2b52;
   L94b2b52: // JMP 0x094b23bc
    goto L94b23bc;
   L94b2b57: // MOV EBX,EAX
    ebx=eax;goto L94b2b59;
   L94b2b59: // LEA ECX,[EBP + -0x7c]
    ecx=unchecked(ebp+4294967172u);goto L94b2b5c;
   L94b2b5c: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b2b5f;
   L94b2b5f: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2b64;
   L94b2b64: // JMP 0x094b23bc
    goto L94b23bc;
   L94b2b91: // JMP 0x094b2b50
    goto L94b2b50;
   L94b2b93: // MOV EBX,EAX
    ebx=eax;goto L94b2b95;
   L94b2b95: // LEA EAX,[EBP + -0x80]
    eax=unchecked(ebp+4294967168u);goto L94b2b98;
   L94b2b98: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2b9b;
   L94b2b9b: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2ba0;
   L94b2ba0: // JMP 0x094b2b59
    goto L94b2b59;
   L94b2ba2: // MOV EBX,EAX
    ebx=eax;goto L94b2ba4;
   L94b2ba4: // LEA EAX,[EBP + -0x74]
    eax=unchecked(ebp+4294967180u);goto L94b2ba7;
   L94b2ba7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2baa;
   L94b2baa: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2baf;
   L94b2baf: // JMP 0x094b2b95
    goto L94b2b95;
   L94b2bbf: // MOV EBX,EAX
    ebx=eax;goto L94b2bc1;
   L94b2bc1: // LEA EAX,[EBP + 0xffffff64]
    eax=unchecked(ebp+4294967140u);goto L94b2bc7;
   L94b2bc7: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2bca;
   L94b2bca: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2bcf;
   L94b2bcf: // JMP 0x094b23e0
    goto L94b23e0;
   L94b2bd4: // MOV ESI,EAX
    esi=eax;goto L94b2bd6;
   L94b2bd6: // MOV dword ptr [ESP],EBX
    W(unchecked(esp),ebx);goto L94b2bd9;
   L94b2bd9: // MOV EBX,ESI
    ebx=esi;goto L94b2bdb;
   L94b2bdb: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2be0;
   L94b2be0: // JMP 0x094b2bc1
    goto L94b2bc1;
   L94b2be2: // JMP 0x094b2bd4
    goto L94b2bd4;
   L94b2c5a: // MOV ECX,dword ptr [EBP + 0xfffffe5c]
    ecx=R(unchecked(ebp+4294966876u));goto L94b2c60;
   L94b2c60: // MOV EBX,EAX
    ebx=eax;goto L94b2c62;
   L94b2c62: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b2c65;
   L94b2c65: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2c6a;
   L94b2c6a: // LEA EDX,[EBP + 0xfffffe7c]
    edx=unchecked(ebp+4294966908u);goto L94b2c70;
   L94b2c70: // MOV dword ptr [ESP],EDX
    W(unchecked(esp),edx);goto L94b2c73;
   L94b2c73: // CALL 0x080ac134
    eax=Call(0x80ac134,R(unchecked(esp+0u)));goto L94b2c78;
   L94b2c78: // JMP 0x094b23a2
    goto L94b23a2;
   L94b2c7d: // MOV EBX,EAX
    ebx=eax;goto L94b2c7f;
   L94b2c7f: // LEA ECX,[EBP + -0x54]
    ecx=unchecked(ebp+4294967212u);goto L94b2c82;
   L94b2c82: // MOV dword ptr [ESP],ECX
    W(unchecked(esp),ecx);goto L94b2c85;
   L94b2c85: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2c8a;
   L94b2c8a: // MOV EAX,EBX
    eax=ebx;goto L94b2c8c;
   L94b2c8c: // JMP 0x094b2c5a
    goto L94b2c5a;
   L94b2c8e: // LEA EDX,[EBP + -0x58]
    edx=unchecked(ebp+4294967208u);goto L94b2c91;
   L94b2c91: // MOV dword ptr [EBP + 0xfffffe5c],EDX
    W(unchecked(ebp+4294966876u),edx);goto L94b2c97;
   L94b2c97: // JMP 0x094b2c5a
    goto L94b2c5a;
   L94b2d21: // MOV EBX,EAX
    ebx=eax;goto L94b2d23;
   L94b2d23: // JMP 0x094b23a2
    goto L94b23a2;
   L94b2da6: // JMP 0x094b2c8e
    goto L94b2c8e;
   L94b2de6: // MOV EBX,EAX
    ebx=eax;goto L94b2de8;
   L94b2de8: // LEA EAX,[EBP + -0x50]
    eax=unchecked(ebp+4294967216u);goto L94b2deb;
   L94b2deb: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2dee;
   L94b2dee: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2df3;
   L94b2df3: // MOV EAX,EBX
    eax=ebx;goto L94b2df5;
   L94b2df5: // JMP 0x094b2c5a
    goto L94b2c5a;
   L94b2e0b: // MOV EBX,EAX
    ebx=eax;goto L94b2e0d;
   L94b2e0d: // MOV EAX,dword ptr [EBP + 0xfffffe5c]
    eax=R(unchecked(ebp+4294966876u));goto L94b2e13;
   L94b2e13: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2e16;
   L94b2e16: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2e1b;
   L94b2e1b: // JMP 0x094b2c6a
    goto L94b2c6a;
   L94b2e34: // MOV EBX,EAX
    ebx=eax;goto L94b2e36;
   L94b2e36: // LEA EAX,[EBP + 0xfffffeac]
    eax=unchecked(ebp+4294966956u);goto L94b2e3c;
   L94b2e3c: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2e3f;
   L94b2e3f: // MOV dword ptr [EBP + 0xfffffe88],0x9b497c8
    W(unchecked(ebp+4294966920u),162830280u);goto L94b2e49;
   L94b2e49: // CALL 0x080add34
    eax=Call(0x80add34,R(unchecked(esp+0u)));goto L94b2e4e;
   L94b2e4e: // MOV dword ptr [EBP + 0xfffffe88],0x9b48a28
    W(unchecked(ebp+4294966920u),162826792u);goto L94b2e58;
   L94b2e58: // MOV dword ptr [ESP],ESI
    W(unchecked(esp),esi);goto L94b2e5b;
   L94b2e5b: // CALL 0x080ad934
    eax=Call(0x80ad934,R(unchecked(esp+0u)));goto L94b2e60;
   L94b2e60: // LEA EAX,[EBP + 0xfffffe7c]
    eax=unchecked(ebp+4294966908u);goto L94b2e66;
   L94b2e66: // MOV dword ptr [ESP + 0x4],0x9b49de4
    W(unchecked(esp+4u),162831844u);goto L94b2e6e;
   L94b2e6e: // MOV dword ptr [ESP],EAX
    W(unchecked(esp),eax);goto L94b2e71;
   L94b2e71: // CALL 0x080b0dd4
    eax=Call(0x80b0dd4,R(unchecked(esp+0u)),R(unchecked(esp+4u)));goto L94b2e76;
   L94b2e76: // JMP 0x094b238a
    goto L94b238a;
   L94b2e7b: // MOV EDX,dword ptr [EBX + -0xc]
    edx=R(unchecked(ebx+4294967284u));goto L94b2e7e;
   L94b2e7e: // MOV dword ptr [EBP + 0xfffffe7c],EBX
    W(unchecked(ebp+4294966908u),ebx);goto L94b2e84;
   L94b2e84: // MOV EBX,EAX
    ebx=eax;goto L94b2e86;
   L94b2e86: // MOV dword ptr [EBP + EDX*0x1 + 0xfffffe7c],ESI
    W(unchecked(ebp+edx*0x1u+4294966908u),esi);goto L94b2e8d;
   L94b2e8d: // MOV dword ptr [EBP + 0xfffffe80],0x0
    W(unchecked(ebp+4294966912u),0u);goto L94b2e97;
   L94b2e97: // JMP 0x094b238a
    goto L94b238a;
   L94b2e9c: // MOV EBX,EAX
    ebx=eax;goto L94b2e9e;
   L94b2e9e: // JMP 0x094b2c6a
    goto L94b2c6a;
  }
 }
}
