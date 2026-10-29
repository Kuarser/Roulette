using System;
using System.IO;

namespace RouletteDeluxe
{
    public interface IOriginalNvDevice
    {
        int Size { get; }
        byte[] Read(int offset, int count);
        void Write(int offset, byte[] data);
        void SetSize(int size);
    }

    public sealed class OriginalNvFault : IOException
    {
        public readonly int NativeCode = 2;
        public OriginalNvFault(string message) : base(message) { }
    }

    // PC transport for the original offset-based NV interface. WriteThrough/Flush(true)
    // are PC durability operations; they are not Linux sync() or a cabinet guarantee.
    public sealed class OriginalNvFileDevice : IOriginalNvDevice, IDisposable
    {
        private readonly FileStream stream;
        private int size;
        public OriginalNvFileDevice(string path)
        {
            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            stream = new FileStream(full, FileMode.OpenOrCreate, FileAccess.ReadWrite,
                FileShare.None, 4096, FileOptions.WriteThrough);
            if (stream.Length > int.MaxValue) { stream.Dispose(); throw new OriginalNvFault("NV file too large for managed admission"); }
            size = (int)stream.Length;
        }
        public int Size { get { return size; } }
        private void CheckRange(int offset, int count)
        {
            if (offset < 0 || count < 0 || offset > size - count)
                throw new OriginalNvFault("NV device range exceeded");
        }
        public byte[] Read(int offset, int count)
        {
            CheckRange(offset, count);
            byte[] result = new byte[count]; stream.Position = offset;
            int received = stream.Read(result, 0, count);
            if (received != count) throw new OriginalNvFault("NV short read");
            return result;
        }
        public void Write(int offset, byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            CheckRange(offset, data.Length); stream.Position = offset;
            stream.Write(data, 0, data.Length); stream.Flush(true);
        }
        public void SetSize(int requested)
        {
            if (requested < 0) throw new ArgumentOutOfRangeException(nameof(requested));
            if (size == requested) return;
            if (requested > size)
            {
                int previous = size; size = requested;
                Write(previous, new byte[requested - previous]);
            }
            stream.SetLength(requested); stream.Flush(true); size = requested;
        }
        public void Dispose() { stream.Dispose(); }
    }

    // NV2 fields +8..+1b form the original 20-byte header. Default offsets from
    // NV2 ctor: header/status=0, first data=20, second data=20+header[12..15].
    public sealed class OriginalNv2Storage
    {
        private readonly IOriginalNvDevice device;
        private readonly Func<byte[]> serialize;
        private readonly Action<byte[]> deserialize;
        private readonly Action<string> logRecovery;
        private byte[] header = new byte[20];
        private byte[] staged;
        private bool saving, dirty;
        public OriginalNv2Storage(IOriginalNvDevice device, Func<byte[]> serialize,
            Action<byte[]> deserialize, Action<string> logRecovery)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.serialize = serialize ?? throw new ArgumentNullException(nameof(serialize));
            this.deserialize = deserialize ?? throw new ArgumentNullException(nameof(deserialize));
            this.logRecovery = logRecovery ?? throw new ArgumentNullException(nameof(logRecovery));
        }
        public byte State { get { return header[0]; } }
        public byte[] Header { get { return (byte[])header.Clone(); } }
        public byte[] ReadDeviceImage() { return device.Read(0, device.Size); }
        private int Get32(int offset)
        {
            uint value = (uint)(header[offset] | header[offset + 1] << 8 | header[offset + 2] << 16 | header[offset + 3] << 24);
            if (value > int.MaxValue) throw new OriginalNvFault("NV size exceeds managed admission");
            return (int)value;
        }
        private void Put32(int offset, int value)
        { for (int i = 0; i < 4; ++i) header[offset + i] = (byte)((uint)value >> (8 * i)); }
        private ushort Get16(int offset) { return (ushort)(header[offset] | header[offset + 1] << 8); }
        private void Put16(int offset, ushort value) { header[offset] = (byte)value; header[offset + 1] = (byte)(value >> 8); }
        private int A { get { return Get32(4); } set { Put32(4, value); } }
        private int B { get { return Get32(8); } set { Put32(8, value); } }
        private int SecondOffset { get { return Get32(12); } set { Put32(12, value); } }
        private static ushort Crc(byte[] data) { return OriginalNvCrc16.Calculate(data, 0, data.Length); }
        private static int Total(int count) { return checked(count * 2 + 20); }
        private static bool Equal(byte[] first, byte[] second)
        {
            if (first.Length != second.Length) return false;
            for (int i = 0; i < first.Length; ++i) if (first[i] != second[i]) return false;
            return true;
        }
        private byte[] Snapshot() { return (byte[])serialize().Clone(); }
        private void WriteHeader() { device.Write(0, (byte[])header.Clone()); }
        private void SetState(byte state) { header[0] = state; device.Write(0, new[] { state }); }
        private byte[] ReadA() { return device.Read(20, A); }
        private byte[] ReadB() { return device.Read(checked(20 + SecondOffset), B); }
        private void Copy1To2(byte[] source)
        {
            if (State != 6) SetState(6);
            if (source == null) { source = ReadA(); if (Crc(source) != Get16(16)) throw new OriginalNvFault("Copy1to2 source CRC"); }
            Put16(18, Get16(16)); B = A; SecondOffset = A;
            device.Write(checked(20 + A), source); WriteHeader();
            device.SetSize(Total(A)); SetState(1);
        }
        private void Copy2To1(byte[] source)
        {
            if (State != 7) SetState(7);
            if (source == null) { source = ReadB(); if (Crc(source) != Get16(18)) throw new OriginalNvFault("Copy2to1 source CRC"); }
            Put16(16, Get16(18)); A = B;
            device.Write(20, source); WriteHeader(); SetState(1);
        }
        public void Save()
        {
            saving = true; Save1(); Save2(); saving = false;
        }
        public void Save1()
        {
            dirty = false; staged = Snapshot(); ushort crc = Crc(staged);
            if (staged.Length > A)
            {
                SetState(3); device.SetSize(Total(staged.Length));
                B = staged.Length; SecondOffset = staged.Length; Put16(18, crc);
                WriteHeader(); device.Write(checked(20 + SecondOffset), staged);
                if (saving) header[0] = 5; else SetState(5);
            }
            else
            {
                SetState(2); A = staged.Length; Put16(16, crc);
                WriteHeader(); device.Write(20, staged);
                if (saving) header[0] = 4; else SetState(4);
            }
        }
        public void Save2()
        {
            if (State == 5) Copy2To1(staged);
            else if (State == 4) Copy1To2(staged);
            else if (State != 1) throw new OriginalNvFault("Save2 invalid NV state");
            if (dirty) Reload();
        }
        public void Reload()
        {
            int observed = device.Size;
            if (observed < 20) { observed = 20; header = new byte[20]; }
            else header = device.Read(0, 20);
            if ((long)observed < (long)A + B + 20) throw new OriginalNvFault("NV file too short");
            if (State > 7) throw new OriginalNvFault("Invalid NV state");
            if (State != 0 && header[1] != 0xa5) throw new OriginalNvFault("Invalid NV magic");
            dirty = true; staged = null;
            while (true)
            {
                switch (State)
                {
                    case 0:
                        byte[] initial = Snapshot(); A = B = SecondOffset = initial.Length;
                        header[1] = 0xa5; Put16(16, Crc(initial)); Put16(18, Crc(initial));
                        device.SetSize(Total(A)); device.Write(20, initial);
                        device.Write(checked(20 + SecondOffset), initial); WriteHeader(); SetState(1); return;
                    case 2: case 7: Copy2To1(null); break;
                    case 3: case 6: Copy1To2(null); break;
                    case 1:
                        byte[] first = ReadA(), second = ReadB();
                        bool goodA = Crc(first) == Get16(16), goodB = Crc(second) == Get16(18);
                        if (goodA && goodB)
                        {
                            if (!Equal(first, second)) throw new OriginalNvFault("Valid NV copies differ");
                            deserialize(first); return;
                        }
                        if (!goodA && !goodB) throw new OriginalNvFault("Both NV CRCs invalid");
                        if (goodA) { logRecovery("NVCP2"); Copy1To2(first); }
                        else { logRecovery("NVCP1"); Copy2To1(second); }
                        break;
                    case 4: case 5:
                        byte[] pendingA = ReadA(), pendingB = ReadB();
                        if (Crc(pendingA) != Get16(16) || Crc(pendingB) != Get16(18))
                            throw new OriginalNvFault("Pending NV CRC invalid");
                        deserialize(State == 4 ? pendingB : pendingA); return;
                }
            }
        }
        // Native094803c0 checks internal NV2 metadata and current serialization only.
        // The caller supplies the original global softRamValidationEnabled state and
        // the device's +30 context-log operation. Disabled mode performs no reads.
        public void SoftValidate(bool enabled,string context,Action<string> deviceContextLog)
        {
            if(!enabled)return;
            if(deviceContextLog==null)throw new ArgumentNullException(nameof(deviceContextLog));
            deviceContextLog(context);int observed=device.Size;
            if(State!=1)throw new OriginalNvFault("NV2::SoftValidate: File state corrupted");
            if((long)observed!=(long)SecondOffset+B+20)throw new OriginalNvFault("NV2::SoftValidate: File size corrupted");
            byte[] current=Snapshot();
            if(A!=B||A!=current.Length)throw new OriginalNvFault("NV2::SoftValidate: File Headers size corrupted");
            if(Crc(current)!=Get16(16))throw new OriginalNvFault("NV2::SoftValidate: Data1 checksum failed");
        }
        public void Validate()
        {
            if (State != 1 || !Equal(header, device.Read(0, 20))) throw new OriginalNvFault("NV header validation failed");
            if ((long)device.Size != (long)SecondOffset + B + 20) throw new OriginalNvFault("NV device size validation failed");
            byte[] first = ReadA(), second = ReadB(), current = Snapshot();
            if (A != B || A != current.Length || Crc(first) != Get16(16) || Crc(second) != Get16(18)
                || !Equal(first, second) || !Equal(first, current)) throw new OriginalNvFault("NV data validation failed");
        }
    }
}
