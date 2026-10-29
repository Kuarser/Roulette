using System;
using System.IO;
using System.Security.Cryptography;
namespace RouletteDeluxe.SasDraftV1
{
    public sealed class SasIdleCompletedReceiptV1
    {
        public readonly string Scope, IdleEntryId, Raw32Sha256;
        public readonly long Generation;
        public SasIdleCompletedReceiptV1(string scope,string id,long generation,string digest)
        {
            if(string.IsNullOrWhiteSpace(scope)||string.IsNullOrWhiteSpace(id)||generation<0||!SasConfigEnvelopeCodecV3.IsDigest(digest))
                throw new ArgumentException("Invalid completed Idle receipt");
            Scope=scope;IdleEntryId=id;Generation=generation;Raw32Sha256=digest;
        }
    }
    public sealed class SasConfigSnapshotV2
    {
        public readonly SasConfigEnvelopeV1 Config;
        public readonly SasIdleCompletedReceiptV1 IdleReceipt;
        private readonly SasEventReceiptV1[] receipts;
        public SasConfigSnapshotV2(SasConfigEnvelopeV1 config,SasEventReceiptV1[] catalogue, SasIdleCompletedReceiptV1 idleReceipt = null)
        {
            if(config==null || catalogue==null)
                throw new ArgumentException("Snapshot requires configuration and durable host receipts");
            new SasEventIndexV1(catalogue);
            if(catalogue.Length==0)
            {
                if(config.LastDurableEventSource!="pc-bootstrap" || config.LastDurableEventId!="initial-zero" ||
                    config.LastDurableEventSequence!=0 || config.LastDurableEventSha256!=SasConfigSnapshotCodecV2.Digest(new byte[0]))
                    throw new ArgumentException("Empty catalogue requires explicit bootstrap identity");
                foreach(byte value in config.Payload.CopyBytes())if(value!=0)throw new ArgumentException("Bootstrap config must be all zero");
            }
            else
            {
            var last=catalogue[catalogue.Length-1];
            if(last.Generation>config.Generation || last.Event.Source!=config.LastDurableEventSource ||
                last.Event.Id!=config.LastDurableEventId || last.Event.Sequence!=config.LastDurableEventSequence ||
                SasConfigSnapshotCodecV2.Digest(last.Event.CopyWire())!=config.LastDurableEventSha256)
                throw new ArgumentException("Snapshot config and durable event catalogue differ");
            }
            if(idleReceipt!=null && (idleReceipt.Scope!=config.Scope || idleReceipt.Generation>config.Generation ||
                (idleReceipt.Generation==config.Generation && idleReceipt.Raw32Sha256!=SasConfigSnapshotCodecV2.Digest(config.Payload.CopyBytes()))))
                throw new ArgumentException("Idle receipt scope, generation or current payload differs");
            IdleReceipt=idleReceipt;
            Config=config;receipts=(SasEventReceiptV1[])catalogue.Clone();
        }
        public SasEventReceiptV1[] CopyReceipts(){return (SasEventReceiptV1[])receipts.Clone();}
    }
    // PC-only binary outer snapshot. Inner configuration remains strict codec V3.
    // No deferred-disable flag is encoded. UTF16 code units preserve event identities.
    public static class SasConfigSnapshotCodecV2
    {
        private static readonly byte[] Magic={83,65,83,67,70,71,51,0};
        public static string Digest(byte[] bytes)
        {using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","");}
        private static byte[] Hash(byte[] bytes)
        {using(var hash=SHA256.Create())return hash.ComputeHash(bytes);}
        private static void WriteText(BinaryWriter w,string text)
        {w.Write(text.Length);foreach(char c in text)w.Write((ushort)c);}
        private static string ReadText(BinaryReader r)
        {
            int length=r.ReadInt32();
            if(length<0 || length>(r.BaseStream.Length-r.BaseStream.Position)/2)
                throw new InvalidDataException("Snapshot text length exceeds remaining input");
            char[] chars=new char[length];for(int i=0;i<length;i++)chars[i]=(char)r.ReadUInt16();return new string(chars);
        }
        public static byte[] Encode(SasConfigSnapshotV2 snapshot)
        {
            if(snapshot==null)throw new ArgumentNullException("snapshot");
            byte[] body;
            using(var stream=new MemoryStream())
            using(var writer=new BinaryWriter(stream))
            {
                writer.Write(Magic);WriteText(writer,SasConfigEnvelopeCodecV3.Encode(snapshot.Config));
                var entries=snapshot.CopyReceipts();writer.Write(entries.Length);
                foreach(var receipt in entries)
                {
                    writer.Write(receipt.Generation);WriteText(writer,receipt.Event.Source);WriteText(writer,receipt.Event.Id);
                    writer.Write(receipt.Event.Sequence);writer.Write(receipt.Event.CopyWire());
                }
                writer.Write((byte)(snapshot.IdleReceipt==null?0:1));
                if(snapshot.IdleReceipt!=null)
                {
                    WriteText(writer,snapshot.IdleReceipt.Scope);WriteText(writer,snapshot.IdleReceipt.IdleEntryId);
                    writer.Write(snapshot.IdleReceipt.Generation);WriteText(writer,snapshot.IdleReceipt.Raw32Sha256);
                }
                writer.Flush();body=stream.ToArray();
            }
            byte[] checksum=Hash(body),result=new byte[checked(body.Length+32)];
            Buffer.BlockCopy(body,0,result,0,body.Length);Buffer.BlockCopy(checksum,0,result,body.Length,32);return result;
        }
        public static SasConfigSnapshotV2 Decode(byte[] bytes,string scope)
        {
            if(bytes==null || bytes.Length<44)throw new InvalidDataException("Snapshot missing or truncated");
            byte[] body=new byte[bytes.Length-32];Buffer.BlockCopy(bytes,0,body,0,body.Length);
            byte[] checksum=Hash(body);for(int i=0;i<32;i++)if(checksum[i]!=bytes[body.Length+i])throw new InvalidDataException("Snapshot checksum differs");
            using(var stream=new MemoryStream(body,false))
            using(var reader=new BinaryReader(stream))
            {
                for(int i=0;i<Magic.Length;i++)if(reader.ReadByte()!=Magic[i])throw new InvalidDataException("Unknown snapshot format");
                var config=SasConfigEnvelopeCodecV3.Decode(ReadText(reader),scope);int count=reader.ReadInt32();
                if(count<0 || count>(stream.Length-stream.Position)/56)throw new InvalidDataException("Invalid receipt count");
                var receipts=new SasEventReceiptV1[count];
                for(int i=0;i<count;i++)
                {
                    long generation=reader.ReadInt64();string source=ReadText(reader),id=ReadText(reader);long sequence=reader.ReadInt64();
                    byte[] wire=reader.ReadBytes(32);if(wire.Length!=32)throw new InvalidDataException("Truncated host wire");
                    receipts[i]=new SasEventReceiptV1(new SasQueuedHostEventV1(source,id,sequence,wire),generation);
                }
                byte marker=reader.ReadByte();if(marker>1)throw new InvalidDataException("Invalid Idle receipt presence marker");
                SasIdleCompletedReceiptV1 idle=null;
                if(marker==1)idle=new SasIdleCompletedReceiptV1(ReadText(reader),ReadText(reader),reader.ReadInt64(),ReadText(reader));
                if(stream.Position!=stream.Length)throw new InvalidDataException("Trailing snapshot data");
                return new SasConfigSnapshotV2(config,receipts,idle);
            }
        }
    }
}
