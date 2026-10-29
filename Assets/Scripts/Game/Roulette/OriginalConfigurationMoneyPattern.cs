using System;
namespace RouletteDeluxe {
 // Physical money_base::_S_construct_pattern 00064250, static C# instruction control.
 // Frame is original ESP after the eight-byte prologue. Native RET pops hidden result.
 public sealed class OriginalConfigurationMoneyPattern {
  public const uint HiddenResultCalleePop=4;
  public sealed class Host {public Func<uint,uint> ReadWord;public Func<uint,byte> ReadByte;public Action<uint,uint> WriteWord;public Action<uint,byte> WriteByte;}
  readonly Host h;
  public OriginalConfigurationMoneyPattern(Host host){if(host==null||host.ReadWord==null||host.ReadByte==null||host.WriteWord==null||host.WriteByte==null)throw new ArgumentException("Original frame, byte memory and packaged jump table required");h=new Host{ReadWord=host.ReadWord,ReadByte=host.ReadByte,WriteWord=host.WriteWord,WriteByte=host.WriteByte};byte[] expected=new byte[]{28,227,247,255,28,227,247,255,236,226,247,255,188,226,247,255,76,227,247,255};for(uint i=0;i<expected.Length;++i)if(h.ReadByte(781884u+i)!=expected[i])throw new ArgumentException("Original money jump table mismatch");}
  uint R(uint at){return h.ReadWord(at);}uint B(uint at){return h.ReadByte(at);}void W(uint at,uint value){h.WriteWord(at,value);}
  public uint Construct(uint frame,uint destination,uint precedes,uint space,uint position)
  {
   uint eax=0,ebx=0xe5ff4,esi=0,edi=0,ecx=0,edx=0,ebp=0,esp=frame,a=0,b=0,v=0;bool zero=false,carry=false,sign=false,overflow=false;
   eax=destination;edx=position&255u;W(frame+0xcu,destination);W(frame+0x10u,precedes);W(frame+0x14u,space);W(frame+0x18u,position);goto L6426a;
   L6426a: // MOVZX ECX,byte ptr [ESP + 0x14]
    ecx=B(unchecked(esp+20u));goto L6426f;
   L6426f: // MOV dword ptr [ESP + 0x4],ESI
    W(unchecked(esp+4u),esi);goto L64273;
   L64273: // MOVZX ESI,byte ptr [ESP + 0x10]
    esi=B(unchecked(esp+16u));goto L64278;
   L64278: // CMP DL,0x4
    a=(edx&255u);b=4u;v=unchecked(a-b);zero=v==0;sign=(v&0x80000000u)!=0;carry=a<b;overflow=((a^b)&(a^v)&0x80000000u)!=0;goto L6427b;
   L6427b: // JBE 0x000642a0
    if(carry||zero)goto L642a0;goto L6427d;
   L6427d: // MOV byte ptr [EAX],0x0
    h.WriteByte(unchecked(eax),unchecked((byte)(0u)));goto L64280;
   L64280: // MOV byte ptr [EAX + 0x1],0x0
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(0u)));goto L64284;
   L64284: // MOV byte ptr [EAX + 0x2],0x0
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(0u)));goto L64288;
   L64288: // MOV byte ptr [EAX + 0x3],0x0
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(0u)));goto L6428c;
   L6428c: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L6428f;
   L6428f: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L64293;
   L64293:return eax;
   L642a0: // MOVZX EDX,DL
    edx=(edx&255u);goto L642a3;
   L642a3: // MOV EDX,dword ptr [EBX + EDX*0x4 + 0xfffd8e48]
    edx=R(unchecked(ebx+edx*0x4u+4294807112u));goto L642aa;
   L642aa: // ADD EDX,EBX
    a=edx;b=ebx;v=unchecked(a+b);zero=v==0;sign=(v&0x80000000u)!=0;carry=v<a;overflow=((~(a^b))&(a^v)&0x80000000u)!=0;edx=v;goto L642ac;
   L642ac: // JMP EDX
    switch(edx){case 0x642b0u:goto L642b0;case 0x642e0u:goto L642e0;case 0x64310u:goto L64310;case 0x64340u:goto L64340;default:throw new InvalidOperationException("Unknown packaged money jump target");}
   L642b0: // MOV EDX,ESI
    edx=esi;goto L642b2;
   L642b2: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L642b4;
   L642b4: // JZ 0x000643c8
    if(zero)goto L643c8;goto L642ba;
   L642ba: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L642bc;
   L642bc: // MOV byte ptr [EAX],0x3
    h.WriteByte(unchecked(eax),unchecked((byte)(3u)));goto L642bf;
   L642bf: // MOV byte ptr [EAX + 0x1],0x2
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(2u)));goto L642c3;
   L642c3: // JNZ 0x00064355
    if(!zero)goto L64355;goto L642c9;
   L642c9: // MOV byte ptr [EAX + 0x2],0x4
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(4u)));goto L642cd;
   L642cd: // MOV byte ptr [EAX + 0x3],0x0
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(0u)));goto L642d1;
   L642d1: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L642d4;
   L642d4: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L642d8;
   L642d8:return eax;
   L642e0: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L642e2;
   L642e2: // MOV EDX,ESI
    edx=esi;goto L642e4;
   L642e4: // JZ 0x000643a8
    if(zero)goto L643a8;goto L642ea;
   L642ea: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L642ec;
   L642ec: // JZ 0x00064438
    if(zero)goto L64438;goto L642f2;
   L642f2: // MOV byte ptr [EAX],0x2
    h.WriteByte(unchecked(eax),unchecked((byte)(2u)));goto L642f5;
   L642f5: // MOV byte ptr [EAX + 0x2],0x4
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(4u)));goto L642f9;
   L642f9: // MOV byte ptr [EAX + 0x1],0x1
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(1u)));goto L642fd;
   L642fd: // MOV byte ptr [EAX + 0x3],0x3
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(3u)));goto L64301;
   L64301: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L64304;
   L64304: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L64308;
   L64308:return eax;
   L64310: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64312;
   L64312: // MOV EDX,ESI
    edx=esi;goto L64314;
   L64314: // MOV byte ptr [EAX],0x3
    h.WriteByte(unchecked(eax),unchecked((byte)(3u)));goto L64317;
   L64317: // JZ 0x00064388
    if(zero)goto L64388;goto L64319;
   L64319: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6431b;
   L6431b: // JZ 0x00064400
    if(zero)goto L64400;goto L64321;
   L64321: // MOV byte ptr [EAX + 0x1],0x2
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(2u)));goto L64325;
   L64325: // MOV byte ptr [EAX + 0x3],0x4
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(4u)));goto L64329;
   L64329: // MOV byte ptr [EAX + 0x2],0x1
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(1u)));goto L6432d;
   L6432d: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L64330;
   L64330: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L64334;
   L64334:return eax;
   L64340: // MOV EDX,ESI
    edx=esi;goto L64342;
   L64342: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64344;
   L64344: // JZ 0x00064370
    if(zero)goto L64370;goto L64346;
   L64346: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64348;
   L64348: // MOV byte ptr [EAX],0x2
    h.WriteByte(unchecked(eax),unchecked((byte)(2u)));goto L6434b;
   L6434b: // MOV byte ptr [EAX + 0x1],0x3
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(3u)));goto L6434f;
   L6434f: // JZ 0x000642c9
    if(zero)goto L642c9;goto L64355;
   L64355: // MOV byte ptr [EAX + 0x2],0x1
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(1u)));goto L64359;
   L64359: // MOV byte ptr [EAX + 0x3],0x4
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(4u)));goto L6435d;
   L6435d: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L64360;
   L64360: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L64364;
   L64364:return eax;
   L64370: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L64372;
   L64372: // MOV byte ptr [EAX],0x4
    h.WriteByte(unchecked(eax),unchecked((byte)(4u)));goto L64375;
   L64375: // JZ 0x000643e0
    if(zero)goto L643e0;goto L64377;
   L64377: // MOV byte ptr [EAX + 0x1],0x1
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(1u)));goto L6437b;
   L6437b: // MOV byte ptr [EAX + 0x2],0x2
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(2u)));goto L6437f;
   L6437f: // MOV byte ptr [EAX + 0x3],0x3
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(3u)));goto L64383;
   L64383: // JMP 0x0006428c
    goto L6428c;
   L64388: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L6438a;
   L6438a: // JZ 0x000643f0
    if(zero)goto L643f0;goto L6438c;
   L6438c: // MOV byte ptr [EAX + 0x1],0x2
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(2u)));goto L64390;
   L64390: // MOV byte ptr [EAX + 0x2],0x4
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(4u)));goto L64394;
   L64394: // MOV byte ptr [EAX + 0x3],0x0
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(0u)));goto L64398;
   L64398: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L6439b;
   L6439b: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L6439f;
   L6439f:return eax;
   L643a8: // TEST DL,DL
    a=(edx&255u);b=(edx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L643aa;
   L643aa: // JZ 0x00064428
    if(zero)goto L64428;goto L643ac;
   L643ac: // MOV byte ptr [EAX],0x2
    h.WriteByte(unchecked(eax),unchecked((byte)(2u)));goto L643af;
   L643af: // MOV byte ptr [EAX + 0x1],0x4
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(4u)));goto L643b3;
   L643b3: // MOV byte ptr [EAX + 0x2],0x3
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(3u)));goto L643b7;
   L643b7: // MOV byte ptr [EAX + 0x3],0x0
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(0u)));goto L643bb;
   L643bb: // MOV EBX,dword ptr [ESP]
    ebx=R(unchecked(esp));goto L643be;
   L643be: // MOV ESI,dword ptr [ESP + 0x4]
    esi=R(unchecked(esp+4u));goto L643c2;
   L643c2:return eax;
   L643c8: // TEST CL,CL
    a=(ecx&255u);b=(ecx&255u);v=unchecked(a&b);zero=v==0;sign=(v&0x80000000u)!=0;carry=false;overflow=false;goto L643ca;
   L643ca: // MOV byte ptr [EAX],0x4
    h.WriteByte(unchecked(eax),unchecked((byte)(4u)));goto L643cd;
   L643cd: // JZ 0x00064410
    if(zero)goto L64410;goto L643cf;
   L643cf: // MOV byte ptr [EAX + 0x1],0x1
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(1u)));goto L643d3;
   L643d3: // MOV byte ptr [EAX + 0x2],0x3
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(3u)));goto L643d7;
   L643d7: // MOV byte ptr [EAX + 0x3],0x2
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(2u)));goto L643db;
   L643db: // JMP 0x0006428c
    goto L6428c;
   L643e0: // MOV byte ptr [EAX + 0x1],0x2
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(2u)));goto L643e4;
   L643e4: // MOV byte ptr [EAX + 0x2],0x3
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(3u)));goto L643e8;
   L643e8: // JMP 0x00064394
    goto L64394;
   L643f0: // MOV byte ptr [EAX + 0x1],0x4
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(4u)));goto L643f4;
   L643f4: // MOV byte ptr [EAX + 0x2],0x2
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(2u)));goto L643f8;
   L643f8: // JMP 0x00064394
    goto L64394;
   L64400: // MOV byte ptr [EAX + 0x1],0x4
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(4u)));goto L64404;
   L64404: // MOV byte ptr [EAX + 0x3],0x2
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(2u)));goto L64408;
   L64408: // JMP 0x00064329
    goto L64329;
   L64410: // MOV byte ptr [EAX + 0x1],0x3
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(3u)));goto L64414;
   L64414: // MOV byte ptr [EAX + 0x2],0x2
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(2u)));goto L64418;
   L64418: // MOV byte ptr [EAX + 0x3],0x0
    h.WriteByte(unchecked(eax+3u),unchecked((byte)(0u)));goto L6441c;
   L6441c: // JMP 0x0006428c
    goto L6428c;
   L64428: // MOV byte ptr [EAX],0x4
    h.WriteByte(unchecked(eax),unchecked((byte)(4u)));goto L6442b;
   L6442b: // MOV byte ptr [EAX + 0x1],0x2
    h.WriteByte(unchecked(eax+1u),unchecked((byte)(2u)));goto L6442f;
   L6442f: // JMP 0x000643b3
    goto L643b3;
   L64438: // MOV byte ptr [EAX],0x4
    h.WriteByte(unchecked(eax),unchecked((byte)(4u)));goto L6443b;
   L6443b: // MOV byte ptr [EAX + 0x2],0x2
    h.WriteByte(unchecked(eax+2u),unchecked((byte)(2u)));goto L6443f;
   L6443f: // JMP 0x000642f9
    goto L642f9;
  }
 }
}
