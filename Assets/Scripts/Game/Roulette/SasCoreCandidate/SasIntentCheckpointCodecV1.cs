using System;
using System.IO;
using System.Security.Cryptography;
namespace RouletteDeluxe.SasDraftV1
{
 // Versioned checkpoint transport. FrozenInputs requires separate full-round validation.
 public static class SasIntentCheckpointCodecV1
 {
  static readonly byte[] Magic={83,65,83,73,78,84,49,0};
  static byte[] Hash(byte[] raw){using(var h=SHA256.Create())return h.ComputeHash(raw);}
  static void Text(BinaryWriter w,string s){w.Write(s.Length);foreach(char c in s)w.Write((ushort)c);}
  static string Text(BinaryReader r){int n=r.ReadInt32();if(n<0||n>(r.BaseStream.Length-r.BaseStream.Position)/2)throw new InvalidDataException("Invalid intent text length");char[] s=new char[n];for(int i=0;i<n;i++)s[i]=(char)r.ReadUInt16();return new string(s);}
  static bool Flag(BinaryReader r){byte b=r.ReadByte();if(b>1)throw new InvalidDataException("Invalid intent flag");return b==1;}
  static void Receipt(BinaryWriter w,SasConfigObservationReceiptV1 v)
  {
   w.Write((byte)(v==null?0:1));if(v==null)return;
   w.Write((int)v.Observation.Point);w.Write(v.Observation.Generation);w.Write(v.Observation.Payload.CopyBytes());w.Write((byte)(v.Observation.RuntimePendingDisableObserved?1:0));Text(w,v.EventSource);Text(w,v.EventId);w.Write(v.EventSequence);
  }
  static SasConfigObservationReceiptV1 Receipt(BinaryReader r)
  {
   if(!Flag(r))return null;var point=(SasObservationPoint)r.ReadInt32();long generation=r.ReadInt64();byte[] raw=r.ReadBytes(32);bool disable=Flag(r);string source=Text(r),id=Text(r);long sequence=r.ReadInt64();
   return new SasConfigObservationReceiptV1(new SasObservation(point,generation,new SasPayload(raw),disable),source,id,sequence);
  }
  public static byte[] Encode(SasIntentCheckpointV1 v)
  {
   if(v==null)throw new ArgumentNullException("v");byte[] body;
   using(var m=new MemoryStream())using(var w=new BinaryWriter(m))
   {
    w.Write(Magic);Text(w,v.Scope);Text(w,v.RoundId);Text(w,v.SpinId);Text(w,v.FrozenInputs);w.Write(v.Revision);w.Write((int)v.Stage);Receipt(w,v.QualificationReceipt);
    if(v.Qualification!=null){w.Write(v.Qualification.EvaluatedAward);w.Write(v.Qualification.Wager);w.Write(v.Qualification.EffectiveMaximumWager);}
    Receipt(w,v.Display);w.Write((byte)(v.ShownSuccessfully?1:0));Receipt(w,v.Recorded);Receipt(w,v.Arithmetic);w.Write((byte)(v.ReadyDigest==null?0:1));if(v.ReadyDigest!=null)Text(w,v.ReadyDigest);
    w.Flush();body=m.ToArray();
   }
   byte[] result=new byte[checked(body.Length+32)];Buffer.BlockCopy(body,0,result,0,body.Length);Buffer.BlockCopy(Hash(body),0,result,body.Length,32);return result;
  }
  public static SasIntentCheckpointV1 Decode(byte[] bytes,string scope,string roundId,string spinId)
  {return DecodeCore(bytes,scope,roundId,spinId,false);}
  // The already admitted Pending/RNG consumer supplies scope and spin. A cold
  // process discovers the saved canonical round identity, never a new GUID.
  public static SasIntentCheckpointV1 DecodeForSpin(byte[] bytes,string scope,string spinId)
  {return DecodeCore(bytes,scope,null,spinId,true);}
  static SasIntentCheckpointV1 DecodeCore(byte[] bytes,string scope,string roundId,string spinId,bool discoverRound)
  {
   if(bytes==null||bytes.Length<40)throw new InvalidDataException("Truncated intent");byte[] body=new byte[bytes.Length-32];Buffer.BlockCopy(bytes,0,body,0,body.Length);byte[] hash=Hash(body);for(int i=0;i<32;i++)if(hash[i]!=bytes[body.Length+i])throw new InvalidDataException("Intent checksum differs");
   using(var m=new MemoryStream(body,false))using(var r=new BinaryReader(m))
   {
    foreach(byte b in Magic)if(r.ReadByte()!=b)throw new InvalidDataException("Unknown intent format");string s=Text(r),round=Text(r),spin=Text(r),frozen=Text(r);if(s!=scope||spin!=spinId||(discoverRound?!PcRoundLifecycleRecord.IsCanonicalId(round)||round==spin:round!=roundId))throw new InvalidDataException("Intent identity differs");long revision=r.ReadInt64();var stage=(SasRoundIntentStageV1)r.ReadInt32();var qr=Receipt(r);SasQualification q=qr==null?null:new SasQualification(qr.Observation,r.ReadInt64(),r.ReadInt64(),r.ReadInt64());var display=Receipt(r);bool shown=Flag(r);var recorded=Receipt(r);var arithmetic=Receipt(r);string digest=Flag(r)?Text(r):null;
    if(m.Position!=m.Length)throw new InvalidDataException("Trailing intent data");return new SasIntentCheckpointV1(s,round,spin,frozen,revision,stage,q,qr,display,shown,recorded,arithmetic,digest);
   }
  }
 }
}
