using System;
using System.IO;
using System.Security.Cryptography;

namespace RouletteDeluxe
{
    // This is a PC persistence format, not a native NV2 file reader/writer.
    // Header: 8-byte magic + little-endian uint version. Payload: 128 bytes,
    // with the eleven scalar values in the native AutoData-relative layout.
    // The final SHA-256 covers the complete header and payload.
    public sealed class PcAutoPlayStateStore
    {
        private static readonly byte[] Magic = { 0x50, 0x43, 0x41, 0x55, 0x54, 0x4e, 0x56, 0 };
        private const uint Version = 1;
        private const int HeaderLength = 12;
        private const int PayloadLength = 128;
        private const int ScalarLength = 20;
        private const int DigestLength = 32;
        private const int ContentLength = HeaderLength + PayloadLength;
        private const int RecordLength = ContentLength + DigestLength;
        private bool seenExisting;

        public string StatePath { get; private set; }

        public PcAutoPlayStateStore(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("AutoData path is missing", "path");
            StatePath = Path.GetFullPath(path);
        }

        public bool TryLoad(out OriginalAutoPlayData data)
        {
            data = null;
            byte[] bytes;
            try
            {
                // Opening the file distinguishes absence from denied access;
                // File.Exists would hide some I/O errors as a missing record.
                using (var stream = new FileStream(StatePath, FileMode.Open,
                    FileAccess.Read, FileShare.Read | FileShare.Delete))
                {
                    if (stream.Length != RecordLength)
                        throw new InvalidDataException("PC AutoData record length is invalid");
                    bytes = new byte[RecordLength];
                    int received = 0;
                    while (received < bytes.Length)
                    {
                        int count = stream.Read(bytes, received, bytes.Length - received);
                        if (count == 0)
                            throw new InvalidDataException("PC AutoData record is truncated");
                        received += count;
                    }
                    if (stream.ReadByte() != -1)
                        throw new InvalidDataException("PC AutoData record has trailing data");
                }
            }
            catch (FileNotFoundException error)
            {
                if (seenExisting)
                    throw new InvalidDataException("Previously loaded PC AutoData record disappeared", error);
                return false;
            }
            catch (DirectoryNotFoundException error)
            {
                if (seenExisting)
                    throw new InvalidDataException("Previously loaded PC AutoData directory disappeared", error);
                return false;
            }
            data = Decode(bytes);
            seenExisting = true;
            return true;
        }

        public void Save(OriginalAutoPlayData data)
        {
            if (data == null) throw new ArgumentNullException("data");
            // Validate the current main record before overwriting it, as the
            // native setter validates its opened NV before changing data.
            // A leftover .new file never replaces a corrupt/missing main on load.
            OriginalAutoPlayData previous;
            bool replace = TryLoad(out previous);
            byte[] bytes = Encode(data);
            string directory = Path.GetDirectoryName(StatePath);
            Directory.CreateDirectory(directory);
            string nextPath = StatePath + ".new";
            using (var stream = new FileStream(nextPath, FileMode.Create,
                FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (replace) File.Replace(nextPath, StatePath, null);
            else File.Move(nextPath, StatePath);
            seenExisting = true;
        }

        private static byte[] Encode(OriginalAutoPlayData data)
        {
            var bytes = new byte[RecordLength];
            using (var stream = new MemoryStream(bytes, true))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(data.AutoPlay);
                WriteBoolean(writer, data.ReplenishCredits);
                WriteBoolean(writer, data.DemoOnly);
                WriteBoolean(writer, data.PlayerInput);
                WriteBoolean(writer, data.Turbo);
                writer.Write(data.TurboOption);
                WriteBoolean(writer, data.AutoPlayOffWhenGameFinished);
                writer.Write(data.SecsToAutoCompleteWarning);
                writer.Write(data.SecsToAutoCompleteStart);
                WriteBoolean(writer, data.AutoCompleteShowCountdown);
                WriteBoolean(writer, data.DisableWhenGameFinished);
                // All reserved bytes have a defined zero representation.
                writer.Write(new byte[PayloadLength - ScalarLength]);
                writer.Flush();
            }
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(bytes, 0, ContentLength);
                Buffer.BlockCopy(digest, 0, bytes, ContentLength, DigestLength);
            }
            return bytes;
        }

        private static OriginalAutoPlayData Decode(byte[] bytes)
        {
            for (int index = 0; index < Magic.Length; ++index)
                if (bytes[index] != Magic[index])
                    throw new InvalidDataException("PC AutoData magic is invalid");
            using (SHA256 sha = SHA256.Create())
            {
                byte[] expected = sha.ComputeHash(bytes, 0, ContentLength);
                int difference = 0;
                for (int index = 0; index < DigestLength; ++index)
                    difference |= expected[index] ^ bytes[ContentLength + index];
                if (difference != 0)
                    throw new InvalidDataException("PC AutoData checksum failed");
            }
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = new BinaryReader(stream))
            {
                stream.Position = Magic.Length;
                if (reader.ReadUInt32() != Version)
                    throw new InvalidDataException("PC AutoData version is unsupported");
                int autoPlay = reader.ReadInt32();
                bool replenish = ReadBoolean(reader);
                bool demoOnly = ReadBoolean(reader);
                bool playerInput = ReadBoolean(reader);
                bool turbo = ReadBoolean(reader);
                byte turboOption = reader.ReadByte();
                bool offWhenFinished = ReadBoolean(reader);
                uint secondsWarning = reader.ReadUInt32();
                uint secondsStart = reader.ReadUInt32();
                bool countdown = ReadBoolean(reader);
                bool disableWhenFinished = ReadBoolean(reader);
                for (int index = ScalarLength; index < PayloadLength; ++index)
                    if (reader.ReadByte() != 0)
                        throw new InvalidDataException("PC AutoData reserved bytes are invalid");

                // Newly constructed values are detached. Loading cannot call
                // a writer or replace the owner's current persistent record.
                var data = new OriginalAutoPlayData();
                data.SetAutoPlay(autoPlay);
                data.SetReplenishCredits(replenish);
                data.SetAutoPlayDemoOnly(demoOnly);
                data.SetPlayerInput(playerInput);
                data.SetTurbo(turbo);
                data.SetTurboOption(turboOption);
                data.SetAutoPlayOffWhenGameFinished(offWhenFinished);
                data.SetSecsToAutoCompleteWarning(secondsWarning);
                data.SetSecsToAutoCompleteStart(secondsStart);
                data.SetAutoCompleteShowCountdown(countdown);
                data.SetDisableWhenGameFinished(disableWhenFinished);
                return data;
            }
        }

        private static void WriteBoolean(BinaryWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }

        private static bool ReadBoolean(BinaryReader reader)
        {
            byte value = reader.ReadByte();
            if (value > 1) throw new InvalidDataException("PC AutoData boolean is invalid");
            return value != 0;
        }
    }
}
