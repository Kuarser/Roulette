using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RouletteDeluxe
{
    // Explicit PC virtual hopper provider. The caller must derive Identity
    // from an admitted, phase-independent financial transaction and config.
    // This receipt cannot admit a wallet or validate a financial transaction.
    public sealed class PcVirtualPaymentPresentationReceipt
    {
        public const string Provider = "PC.Virtual.Hopper.v1";
        public string Identity { get; private set; }
        public bool AuEnabled { get; private set; }
        public long MinorUnits { get; private set; }
        public long Denomination { get; private set; }
        public bool Completed { get; private set; }
        public OriginalPaymentMessageState Before { get; private set; }
        public OriginalPaymentMessageState After { get; private set; }
        private PcVirtualPaymentPresentationReceipt() { }
        public static PcVirtualPaymentPresentationReceipt Begin(string identity, bool au, long minorUnits,
            long denomination, OriginalPaymentMessageState before)
        {
            if (identity == null || !Regex.IsMatch(identity, "^[A-F0-9]{64}$")) throw new ArgumentException("identity");
            if (minorUnits <= 0 || denomination <= 0 || before == null) throw new ArgumentException("virtual payout facts");
            double credits = OriginalBinary80.Divide(OriginalBinary80.Integer64(minorUnits),
                OriginalBinary80.Integer64(denomination)).ToDouble();
            return new PcVirtualPaymentPresentationReceipt { Identity = identity, AuEnabled = au,
                MinorUnits = minorUnits, Denomination = denomination, Before = before,
                After = before.DisplayUpdate(true, au, 0x3400, credits) };
        }
        public PcVirtualPaymentPresentationReceipt Complete()
        {
            if (Completed) return this;
            return new PcVirtualPaymentPresentationReceipt { Identity = Identity, AuEnabled = AuEnabled,
                MinorUnits = MinorUnits, Denomination = Denomination, Before = Before,
                After = After.CashoutCompleted(), Completed = true };
        }
        private static void WriteState(BinaryWriter writer, OriginalPaymentMessageState state)
        {
            writer.Write(BitConverter.DoubleToInt64Bits(state.Collect));
            writer.Write(BitConverter.DoubleToInt64Bits(state.Cancel));
            writer.Write(BitConverter.DoubleToInt64Bits(state.TotalPaid));
            writer.Write((byte)(state.CollectPending ? 1 : 0));
        }
        private static bool ReadBool(BinaryReader reader)
        { byte value = reader.ReadByte(); if (value > 1) throw new InvalidDataException("Noncanonical boolean"); return value == 1; }
        private static OriginalPaymentMessageState ReadState(BinaryReader reader)
        {
            return new OriginalPaymentMessageState(BitConverter.Int64BitsToDouble(reader.ReadInt64()),
                BitConverter.Int64BitsToDouble(reader.ReadInt64()), BitConverter.Int64BitsToDouble(reader.ReadInt64()), ReadBool(reader));
        }
        private static bool Same(OriginalPaymentMessageState a, OriginalPaymentMessageState b)
        {
            return BitConverter.DoubleToInt64Bits(a.Collect) == BitConverter.DoubleToInt64Bits(b.Collect) &&
                BitConverter.DoubleToInt64Bits(a.Cancel) == BitConverter.DoubleToInt64Bits(b.Cancel) &&
                BitConverter.DoubleToInt64Bits(a.TotalPaid) == BitConverter.DoubleToInt64Bits(b.TotalPaid) && a.CollectPending == b.CollectPending;
        }
        public string Encode()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(0x31505042u); writer.Write(Provider); writer.Write(Identity);
                    writer.Write((byte)(AuEnabled ? 1 : 0)); writer.Write(MinorUnits); writer.Write(Denomination);
                    writer.Write((byte)(Completed ? 1 : 0)); WriteState(writer, Before); WriteState(writer, After);
                }
                byte[] payload = stream.ToArray(), hash;
                using (var sha = SHA256.Create()) hash = sha.ComputeHash(payload);
                stream.Write(hash, 0, hash.Length);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        // Expected identity/config/amount must come from separately admitted
        // source facts, never from this payload. Before is a presentation
        // snapshot; ownership of predecessor snapshots is a caller obligation.
        public static PcVirtualPaymentPresentationReceipt Decode(string encoded, string expectedIdentity,
            bool expectedAu, long expectedMinorUnits, long expectedDenomination, OriginalPaymentMessageState expectedBefore)
        {
            if (expectedBefore == null) throw new ArgumentNullException("expectedBefore");
            if (encoded == null || encoded.Length > 2048) throw new InvalidDataException("Receipt size");
            byte[] bytes = Convert.FromBase64String(encoded);
            if (bytes.Length < 32) throw new InvalidDataException("Receipt is truncated");
            byte[] digest;
            using (var sha = SHA256.Create()) digest = sha.ComputeHash(bytes, 0, bytes.Length - 32);
            for (int i = 0; i < 32; i++) if (digest[i] != bytes[bytes.Length - 32 + i]) throw new InvalidDataException("Receipt checksum");
            using (var stream = new MemoryStream(bytes, 0, bytes.Length - 32, false))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadUInt32() != 0x31505042u || reader.ReadString() != Provider) throw new InvalidDataException("Receipt provider");
                string identity = reader.ReadString(); bool au = ReadBool(reader);
                long amount = reader.ReadInt64(), denomination = reader.ReadInt64(); bool completed = ReadBool(reader);
                var before = ReadState(reader); var storedAfter = ReadState(reader);
                if (stream.Position != stream.Length || identity != expectedIdentity || au != expectedAu ||
                    amount != expectedMinorUnits || denomination != expectedDenomination || !Same(before, expectedBefore))
                    throw new InvalidDataException("Receipt ownership");
                var result = Begin(identity, au, amount, denomination, before);
                if (completed) result = result.Complete();
                if (!Same(storedAfter, result.After)) throw new InvalidDataException("Receipt derived state");
                if (result.Encode() != encoded) throw new InvalidDataException("Receipt canonical encoding");
                return result;
            }
        }
    }
}
