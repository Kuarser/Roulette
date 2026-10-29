using System;
namespace RouletteDeluxe
{
 // Full packaged std::string::_M_mutate00099140/432 bytes.
 public sealed class OriginalConfigurationCowMutation
 {
  public sealed class Host
  {
   public Func<uint,uint> ReadWord;
   public Action<uint,uint> WriteWord;
   public Func<uint,byte> ReadByte;
   public Action<uint,byte> WriteByte;
   public Action<uint,uint,uint> CopyBytes,MoveBytes;
   public Func<uint,uint,uint> CreateRep;
   public Action<uint> ReleaseRep;
  }
  private readonly Host host;private readonly uint emptyRepSlot;
  public OriginalConfigurationCowMutation(Host h,uint emptySlot)
  {
   if(h==null||h.ReadWord==null||h.WriteWord==null||h.ReadByte==null||h.WriteByte==null||h.CopyBytes==null||h.MoveBytes==null||h.CreateRep==null||h.ReleaseRep==null||emptySlot==0)throw new ArgumentException("Actual COW mutation memory/copy/move/Rep create/release and relocated empty binding required");
   host=new Host{ReadWord=h.ReadWord,WriteWord=h.WriteWord,ReadByte=h.ReadByte,WriteByte=h.WriteByte,CopyBytes=h.CopyBytes,MoveBytes=h.MoveBytes,CreateRep=h.CreateRep,ReleaseRep=h.ReleaseRep};emptyRepSlot=emptySlot;
  }
  private static uint Add(uint a,uint b){return unchecked(a+b);}
  public void Mutate(uint source,uint position,uint removed,uint inserted)
  {
   uint data=host.ReadWord(source),length=host.ReadWord(unchecked(data-12));
   uint nextLength=unchecked(inserted-removed+length),tail=unchecked(length-position-removed),capacity=host.ReadWord(unchecked(data-8)),empty;
   if(nextLength>capacity||unchecked((int)host.ReadWord(unchecked(data-4)))>0)
   {
    uint rep=host.CreateRep(nextLength,capacity),nextData=Add(rep,12);
    if(position!=0)
    {
     uint current=host.ReadWord(source);
     if(position==1)host.WriteByte(nextData,host.ReadByte(current));else host.CopyBytes(nextData,current,position);
    }
    if(tail!=0)
    {
     uint from=Add(host.ReadWord(source),Add(position,removed)),to=Add(nextData,Add(position,inserted));
     if(tail==1)host.WriteByte(to,host.ReadByte(from));else host.CopyBytes(to,from,tail);
    }
    uint oldRep=unchecked(host.ReadWord(source)-12);empty=host.ReadWord(emptyRepSlot);
    if(oldRep!=empty)host.ReleaseRep(oldRep);
    data=nextData;host.WriteWord(source,data);
   }
   else
   {
    empty=host.ReadWord(emptyRepSlot);
    if(removed!=inserted&&tail!=0)
    {
     uint from=Add(data,Add(position,removed)),to=Add(data,Add(position,inserted));
     if(tail==1)host.WriteByte(to,host.ReadByte(from));else host.MoveBytes(to,from,tail);
     data=host.ReadWord(source);
    }
   }
   if(unchecked(data-12)!=empty){host.WriteWord(unchecked(data-4),0);host.WriteWord(unchecked(data-12),nextLength);host.WriteByte(Add(data,nextLength),0);}
  }
 }
}
