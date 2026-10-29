using System;
using System.Text;

namespace RouletteDeluxe
{
    // Native NV Size/SetSize/Clear. System calls and virtual slots are mandatory
    // bindings on the actual owner; this does not supply a filesystem backend.
    public sealed class OriginalNvBackendSize
    {
        public sealed class State
        {
            public int Descriptor;
            public uint StoredSize;
            private byte[] path;
            public byte[] Path { get { return (byte[])path.Clone(); } }
            internal void SetPath(byte[] value) { path = (byte[])value.Clone(); }
            public State(int descriptor, uint size, byte[] path)
            {
                if (path == null) throw new ArgumentNullException("path");
                Descriptor = descriptor; StoredSize = size; this.path = (byte[])path.Clone();
            }
        }

        public sealed class Host
        {
            public Func<uint, uint, byte[]> Calloc;
            public Action<byte[]> Free;
            public Action<uint, byte[], uint> Write;
            public Func<int, uint, int> Truncate;
            public Action Sync;
            public Action<byte[], byte[], uint, uint> PrintfSizeWarning;
            public Func<int> Errno;
            public Func<int, byte[]> Strerror;
            // null errno/text means no variadic arguments, rather than errno 0.
            public Func<byte[], int?, byte[], byte[]> GetDisplayInfo;
            public Func<byte[]> GetDisplayName;
            public Action<byte[], uint, byte[]> NvFault;
            public Action<int> AssertMemory;
        }

        private readonly State state;
        private readonly Host host;
        public OriginalNvBackendSize(State state, Host h)
        {
            if (state == null || h == null || h.Calloc == null || h.Free == null ||
                h.Write == null || h.Truncate == null || h.Sync == null ||
                h.PrintfSizeWarning == null || h.Errno == null || h.Strerror == null ||
                h.GetDisplayInfo == null || h.GetDisplayName == null ||
                h.NvFault == null || h.AssertMemory == null)
                throw new ArgumentException("Actual NV owner, POSIX, allocation, virtual and fault bindings required");
            this.state = state;
            host = new Host { Calloc = h.Calloc, Free = h.Free, Write = h.Write,
                Truncate = h.Truncate, Sync = h.Sync, PrintfSizeWarning = h.PrintfSizeWarning,
                Errno = h.Errno, Strerror = h.Strerror, GetDisplayInfo = h.GetDisplayInfo,
                GetDisplayName = h.GetDisplayName, NvFault = h.NvFault, AssertMemory = h.AssertMemory };
        }

        private static byte[] Text(string s) { return Encoding.ASCII.GetBytes(s); }
        private void Fault(byte[] description, uint code)
        {
            byte[] name = host.GetDisplayName();
            host.NvFault(name, code, description);
            throw new InvalidOperationException("Native nonreturning NVFault binding returned");
        }
        private void RequireOpen(string message)
        {
            if (state.Descriptor == 0)
                Fault(host.GetDisplayInfo(Text(message), null, null), 0x10);
        }
        private byte[] ZeroBlock(uint count, int line)
        {
            byte[] block = host.Calloc(count, 1);
            if (block == null)
            {
                host.AssertMemory(line);
                throw new InvalidOperationException("Native nonreturning memory assertion returned");
            }
            if ((ulong)block.LongLength != count)
                throw new InvalidOperationException("Actual calloc byte frame required");
            for (int i = 0; i < block.Length; ++i)
                if (block[i] != 0) throw new InvalidOperationException("Actual calloc zero initialization required");
            return block;
        }
        public uint Size()
        {
            RequireOpen("NV::Size: no file open");
            return state.StoredSize;
        }
        public void SetSize(uint target)
        {
            RequireOpen("NV::SetSize: no file open");
            uint previous = state.StoredSize;
            if (previous == target) return;
            uint oldOffset = 0;
            if (previous != 0)
            {
                host.PrintfSizeWarning(Text("WARNING: NOVRAM FILE %s CHANGED SIZE FROM %ld to %ld\n"), state.Path, previous, target);
                oldOffset = state.StoredSize;
            }
            if (oldOffset < target)
            {
                uint count = unchecked(target - oldOffset);
                byte[] block = ZeroBlock(count, 0x10a);
                state.StoredSize = target;
                host.Write(oldOffset, block, count);
                host.Free(block);
            }
            if (host.Truncate(state.Descriptor, target) == -1)
            {
                int error = host.Errno();
                byte[] explanation = host.Strerror(error);
                error = host.Errno();
                Fault(host.GetDisplayInfo(Text("NV::SetSize: truncate error=%d\n%s"), error, explanation), 2);
            }
            host.Sync();
            state.StoredSize = target;
        }
        public void Clear()
        {
            RequireOpen("NV::Clear: no file open");
            uint count = state.StoredSize;
            if (count == 0) return;
            byte[] block = ZeroBlock(count, 0x149);
            host.Write(0, block, count);
            host.Free(block);
        }
    }
}
