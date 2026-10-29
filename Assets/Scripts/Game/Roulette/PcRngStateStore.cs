using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace RouletteDeluxe
{
    // Local Windows Player persistence for the recovered engine. This file is
    // not the original cabinet NV format or an Android/SBox state contract.
    public static class PcRngStateStore
    {
        private const uint Magic = 0x31474e52U; // RNG1 in little-endian bytes.
        private const uint Version = 2;
        private const uint ReceiptVersion = 3;
        private const uint CanonicalSingleBufferVersion = 1;
        private const int DigestLength = 32;
        private const int PayloadLength = 8 + OriginalPlatformRngCore.SnapshotWordCount * 4;
        private const int FileLength = PayloadLength + DigestLength;
        private const int MaximumFileLength = FileLength +
            PcRngDrawReceipt.MaximumContextBytes + PcRngDrawReceipt.MaximumScopeBytes +
            PcRngDrawReceipt.MaximumProfileBytes + 1024;

        public static string DefaultPath
        {
            get { return Path.Combine(Application.persistentDataPath,
                "roulette_platform_rng_v1.bin"); }
        }

        // Keep compiled-Player RNG regressions away from the playable PC
        // stream. Ordinary launches continue to use DefaultPath.
        public static string PathForCurrentProcess
        {
            get
            {
                string[] args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-rouletteRngStateTestKey");
                if (index < 0) return DefaultPath;
                if (index + 1 >= args.Length || args[index + 1].Length == 0 ||
                    args[index + 1].Length > 40)
                    throw new ArgumentException("Invalid PC RNG test key");
                string key = args[index + 1];
                foreach (char c in key)
                    if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                        throw new ArgumentException("Invalid PC RNG test key");
                return Path.Combine(Application.persistentDataPath,
                    "roulette_platform_rng_test_" + key + ".bin");
            }
        }

        public static bool TryLoad(string path, out uint[] snapshot)
        {
            snapshot = null;
            try
            {
                PcRngStoredState stored = LoadStrict(path);
                if (stored == null) return false;
                snapshot = stored.Snapshot;
                return true;
            }
            catch (InvalidDataException) { return false; }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (ArgumentException) { return false; }
        }

        // Missing is the only non-error result. In particular, a damaged or
        // inaccessible existing store must never cause a fresh seed.
        public static PcRngStoredState LoadStrict(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("RNG state path is empty", "path");
            byte[] bytes;
            try { bytes = ReadBoundedFile(path); }
            catch (FileNotFoundException) { return MissingState(path); }
            catch (DirectoryNotFoundException) { return MissingState(path); }
            if (bytes.Length < FileLength)
                throw new InvalidDataException("PC RNG state is truncated");
            int payloadLength = bytes.Length - DigestLength;
            using (var sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(bytes, 0, payloadLength);
                for (int i = 0; i < DigestLength; ++i)
                    if (digest[i] != bytes[payloadLength + i])
                        throw new InvalidDataException("PC RNG state checksum failed");
            }
            try
            {
                using (var stream = new MemoryStream(bytes, 0, payloadLength, false))
                using (var reader = new BinaryReader(stream, PcRngDrawReceipt.StrictUtf8))
                {
                    if (reader.ReadUInt32() != Magic)
                        throw new InvalidDataException("PC RNG state magic is invalid");
                    uint version = reader.ReadUInt32();
                    if (version != CanonicalSingleBufferVersion && version != Version &&
                        version != ReceiptVersion)
                        throw new InvalidDataException("PC RNG state version is unsupported");
                    if (version != ReceiptVersion && bytes.Length != FileLength)
                        throw new InvalidDataException("PC RNG legacy state length is invalid");
                    var words = new uint[OriginalPlatformRngCore.SnapshotWordCount];
                    for (int i = 0; i < words.Length; ++i) words[i] = reader.ReadUInt32();
                    // A v1 export held the active block with an index <= 624.
                    // Interpreting it as buffer zero preserves its continuation.
                    if (version == CanonicalSingleBufferVersion && words[1] > 624)
                        throw new InvalidDataException("PC RNG v1 index is invalid");
                    OriginalPlatformRngCore.FromSnapshot(words);
                    PcRngDrawReceipt receipt = null;
                    if (version == ReceiptVersion)
                    {
                        uint present = reader.ReadUInt32();
                        if (present > 1) throw new InvalidDataException("PC RNG receipt marker is invalid");
                        if (present == 1)
                        {
                            receipt = new PcRngDrawReceipt {
                                context = PcRngDrawReceipt.ReadContext(reader),
                                contextSha256 = PcRngDrawReceipt.ReadText(reader, 64),
                                rawResult = reader.ReadInt32(),
                                effectiveResult = reader.ReadInt32()
                            };
                            PcRngDrawReceipt.Validate(receipt);
                        }
                    }
                    if (stream.Position != stream.Length)
                        throw new InvalidDataException("PC RNG state contains trailing data");
                    return new PcRngStoredState((int)version, words, receipt);
                }
            }
            catch (EndOfStreamException error)
            {
                throw new InvalidDataException("PC RNG state is truncated", error);
            }
            catch (ArgumentException error)
            {
                throw new InvalidDataException("PC RNG state contains invalid fields", error);
            }
        }

        private static byte[] ReadBoundedFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length < 0 || stream.Length > MaximumFileLength)
                    throw new InvalidDataException("PC RNG state exceeds its size limit");
                var bytes = new byte[(int)stream.Length];
                int read = 0;
                while (read < bytes.Length)
                {
                    int count = stream.Read(bytes, read, bytes.Length - read);
                    if (count == 0) throw new InvalidDataException("PC RNG state changed while reading");
                    read += count;
                }
                if (stream.ReadByte() != -1)
                    throw new InvalidDataException("PC RNG state changed while reading");
                return bytes;
            }
        }

        private static PcRngStoredState MissingState(string path)
        {
            // Never promote or discard an orphan candidate implicitly. A
            // valid main file is authoritative; a candidate alone is ambiguous.
            try { File.GetAttributes(path + ".new"); }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
            throw new InvalidDataException("PC RNG main state is missing with an uncommitted candidate");
        }

        public static void Save(string path, uint[] snapshot)
        {
            SaveEnvelope(path, snapshot, null, Version);
        }

        internal static void InitializeNew(string path, uint[] snapshot)
        {
            SaveEnvelope(path, snapshot, null, Version, null, true);
        }

        public static void SaveWithReceipt(string path, uint[] snapshot, PcRngDrawReceipt receipt)
        {
            SaveEnvelope(path, snapshot, receipt, ReceiptVersion);
        }

        internal static void SaveWithReceipt(string path, uint[] snapshot,
            PcRngDrawReceipt receipt, Action beforeReplace)
        {
            SaveEnvelope(path, snapshot, receipt, ReceiptVersion, beforeReplace);
        }

        private static void SaveEnvelope(string path, uint[] snapshot, PcRngDrawReceipt receipt,
            uint version, Action beforeReplace = null, bool initialOnly = false)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            uint[] words = (uint[])snapshot.Clone();
            OriginalPlatformRngCore.FromSnapshot(words);
            PcRngDrawReceipt savedReceipt = PcRngDrawReceipt.Clone(receipt);
            byte[] bytes;
            using (var stream = new MemoryStream(FileLength))
            {
                using (var writer = new BinaryWriter(stream,
                    PcRngDrawReceipt.StrictUtf8, true))
                {
                    writer.Write(Magic);
                    writer.Write(version);
                    foreach (uint word in words) writer.Write(word);
                    if (version == ReceiptVersion)
                    {
                        writer.Write(savedReceipt == null ? 0U : 1U);
                        if (savedReceipt != null)
                        {
                            PcRngDrawReceipt.WriteContext(writer, savedReceipt.context);
                            PcRngDrawReceipt.WriteText(writer, savedReceipt.contextSha256);
                            writer.Write(savedReceipt.rawResult);
                            writer.Write(savedReceipt.effectiveResult);
                        }
                    }
                }
                using (var sha = SHA256.Create())
                {
                    byte[] payload = stream.ToArray();
                    byte[] digest = sha.ComputeHash(payload);
                    stream.Write(digest, 0, digest.Length);
                }
                bytes = stream.ToArray();
            }
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentException("RNG state path needs a directory", "path");
            Directory.CreateDirectory(directory);
            string pending = path + ".new";
            using (var stream = new FileStream(pending, initialOnly ? FileMode.CreateNew : FileMode.Create, FileAccess.Write,
                FileShare.None, 4096, FileOptions.WriteThrough))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (beforeReplace != null) beforeReplace();
            if (!initialOnly && File.Exists(path)) File.Replace(pending, path, null);
            else File.Move(pending, path);
        }
    }
}
