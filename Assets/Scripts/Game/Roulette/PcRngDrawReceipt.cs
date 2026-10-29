using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RouletteDeluxe
{
    // This context belongs to the PC consumer. Its JSON is kept byte-for-byte;
    // the RNG store does not interpret the consumer's financial schema.
    [Serializable]
    public sealed class PcRngOperationContext
    {
        public int schema = 1;
        public string operationId;
        public string scope;
        public string profile;
        public int kind; // 1: pocket, 2: original Double suit.
        public string contextJson;
    }

    [Serializable]
    public sealed class PcRngDrawReceipt
    {
        public const int MaximumScopeBytes = 512;
        public const int MaximumProfileBytes = 128;
        public const int MaximumContextBytes = 1024 * 1024;
        internal static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        public PcRngOperationContext context;
        public string contextSha256;
        public int rawResult;
        public int effectiveResult;

        public static void ValidateContext(PcRngOperationContext value)
        {
            Guid identity;
            if (value == null || value.schema != 1 ||
                string.IsNullOrEmpty(value.operationId) || value.operationId.Length != 32 ||
                !Guid.TryParseExact(value.operationId, "N", out identity) || identity == Guid.Empty ||
                identity.ToString("N") != value.operationId ||
                (value.kind != 1 && value.kind != 2))
                throw new ArgumentException("Invalid PC RNG operation identity or kind", "value");
            ValidateText(value.scope, MaximumScopeBytes, false, "scope");
            ValidateText(value.profile, MaximumProfileBytes, false, "profile");
            ValidateText(value.contextJson, MaximumContextBytes, true, "contextJson");
        }

        private static void ValidateText(string value, int maximumBytes, bool opaque, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || StrictUtf8.GetByteCount(value) > maximumBytes)
                throw new ArgumentException("Invalid PC RNG operation " + name, name);
            if (!opaque)
                foreach (char c in value)
                    if (char.IsControl(c))
                        throw new ArgumentException("Invalid PC RNG operation " + name, name);
        }

        public static string ContextDigest(PcRngOperationContext value)
        {
            value = CloneContext(value);
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, StrictUtf8, true))
                    WriteContext(writer, value);
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "");
            }
        }

        public static void Validate(PcRngDrawReceipt value)
        {
            if (value == null) throw new ArgumentNullException("value");
            ValidateContext(value.context);
            int exclusive = value.context.kind == 1 ? 37 : 4;
            if (value.rawResult < 0 || value.rawResult >= exclusive ||
                value.effectiveResult < 0 || value.effectiveResult >= exclusive ||
                !string.Equals(value.contextSha256, ContextDigest(value.context), StringComparison.Ordinal))
                throw new ArgumentException("Invalid PC RNG result receipt", "value");
        }

        public static PcRngOperationContext CloneContext(PcRngOperationContext value)
        {
            if (value == null) throw new ArgumentNullException("value");
            var copy = new PcRngOperationContext {
                schema = value.schema, operationId = value.operationId,
                scope = value.scope, profile = value.profile, kind = value.kind,
                contextJson = value.contextJson
            };
            ValidateContext(copy);
            return copy;
        }

        public static PcRngDrawReceipt Clone(PcRngDrawReceipt value)
        {
            if (value == null) return null;
            var copy = new PcRngDrawReceipt {
                context = CloneContext(value.context), contextSha256 = value.contextSha256,
                rawResult = value.rawResult, effectiveResult = value.effectiveResult
            };
            Validate(copy);
            return copy;
        }

        internal static void WriteContext(BinaryWriter writer, PcRngOperationContext value)
        {
            writer.Write(value.schema);
            WriteText(writer, value.operationId);
            WriteText(writer, value.scope);
            WriteText(writer, value.profile);
            writer.Write(value.kind);
            WriteText(writer, value.contextJson);
        }

        internal static PcRngOperationContext ReadContext(BinaryReader reader)
        {
            return new PcRngOperationContext {
                schema = reader.ReadInt32(),
                operationId = ReadText(reader, 32),
                scope = ReadText(reader, MaximumScopeBytes),
                profile = ReadText(reader, MaximumProfileBytes),
                kind = reader.ReadInt32(),
                contextJson = ReadText(reader, MaximumContextBytes)
            };
        }

        internal static void WriteText(BinaryWriter writer, string value)
        {
            byte[] bytes = StrictUtf8.GetBytes(value);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        internal static string ReadText(BinaryReader reader, int maximumBytes)
        {
            int length = reader.ReadInt32();
            if (length < 1 || length > maximumBytes ||
                length > reader.BaseStream.Length - reader.BaseStream.Position)
                throw new InvalidDataException("Invalid PC RNG text length");
            byte[] bytes = reader.ReadBytes(length);
            if (bytes.Length != length) throw new EndOfStreamException("Truncated PC RNG text");
            return StrictUtf8.GetString(bytes);
        }
    }

    public sealed class PcRngStoredState
    {
        private readonly uint[] snapshot;
        private readonly PcRngDrawReceipt receipt;
        public int Version { get; private set; }
        public uint[] Snapshot { get { return (uint[])snapshot.Clone(); } }
        public PcRngDrawReceipt Receipt { get { return PcRngDrawReceipt.Clone(receipt); } }

        internal PcRngStoredState(int version, uint[] words, PcRngDrawReceipt savedReceipt)
        {
            Version = version;
            snapshot = (uint[])words.Clone();
            receipt = PcRngDrawReceipt.Clone(savedReceipt);
        }
    }
}
