using System;
using System.Collections.Generic;
using System.IO;

namespace RouletteDeluxe
{
    public static class OriginalNv2StorageValidation
    {
        private sealed class Device : IOriginalNvDevice
        {
            public byte[] Bytes;
            public readonly List<string> Trace = new List<string>();
            public readonly List<byte[]> Boundaries = new List<byte[]>();
            public Device(byte[] bytes) { Bytes = (byte[])bytes.Clone(); }
            public int Size { get { return Bytes.Length; } }
            public byte[] Read(int offset, int count)
            {
                if (offset < 0 || count < 0 || offset > Size - count) throw new OriginalNvFault("Test device read range");
                byte[] data = new byte[count]; Array.Copy(Bytes, offset, data, 0, count); return data;
            }
            public void Write(int offset, byte[] data)
            {
                if (offset < 0 || offset > Size - data.Length) throw new OriginalNvFault("Test device write range");
                Array.Copy(data, 0, Bytes, offset, data.Length);
                Trace.Add("W" + offset + ":" + data.Length + (data.Length == 1 ? ":" + data[0] : ""));
                Boundaries.Add((byte[])Bytes.Clone());
            }
            public void SetSize(int size)
            {
                Array.Resize(ref Bytes, size); Trace.Add("S" + size); Boundaries.Add((byte[])Bytes.Clone());
            }
        }
        private static byte[] Pattern(int size, int salt)
        { byte[] data = new byte[size]; for (int i = 0; i < size; ++i) data[i] = (byte)(i * 73 + salt); return data; }
        private static bool Equal(byte[] a, byte[] b)
        { if (a.Length != b.Length) return false; for (int i = 0; i < a.Length; ++i) if (a[i] != b[i]) return false; return true; }
        public static int Validate(string freshFile)
        {
            int checks = 0;
            Action<bool> check = value => { if (!value) throw new Exception("NV2 storage regression " + checks); ++checks; };
            Action<Action> fault = action => { bool rejected = false; try { action(); } catch (OriginalNvFault error) { rejected = error.NativeCode == 2; } check(rejected); };
            byte[] old = Pattern(135, 19), next = Pattern(135, 23), current = old;
            var device = new Device(new byte[0]); var logs = new List<string>();
            var nv = new OriginalNv2Storage(device, () => current, data => current = data, logs.Add);
            nv.Reload(); nv.Validate(); check(device.Size == 290 && nv.State == 1);
            byte[] baseline = (byte[])device.Bytes.Clone();
            check(baseline[0] == 1 && baseline[1] == 0xa5 && baseline[2] == 0 && baseline[3] == 0);
            check(baseline[4] == 135 && baseline[8] == 135 && baseline[12] == 135);
            check(Equal(device.Read(20, 135), old) && Equal(device.Read(155, 135), old));
            string[] expected = { "W0:1:2", "W0:20", "W20:135", "W0:1:6", "W155:135", "W0:20", "S290", "W0:1:1" };
            device.Trace.Clear(); device.Boundaries.Clear(); current = next; nv.Save(); nv.Validate();
            check(string.Join(",", device.Trace) == string.Join(",", expected));
            check(Equal(device.Read(20, 135), next) && Equal(device.Read(155, 135), next));
            // Cold restart after each completed device mutation. These are call boundaries,
            // not torn writes or power-loss assertions. State2 rolls back; state6 completes.
            for (int i = 0; i < device.Boundaries.Count; ++i)
            {
                byte[] loaded = null; var restart = new Device(device.Boundaries[i]);
                var cold = new OriginalNv2Storage(restart, () => loaded, data => loaded = data, logs.Add);
                cold.Reload(); check(Equal(loaded, i < 3 ? old : next));
                var verify = new OriginalNv2Storage(restart, () => loaded, data => loaded = data, logs.Add);
                verify.Reload(); verify.Validate(); check(verify.State == 1);
            }
            current = old; device = new Device(baseline);
            nv = new OriginalNv2Storage(device, () => current, data => current = data, logs.Add); nv.Reload();
            byte[] grown = Pattern(140, 29); current = grown; device.Trace.Clear(); device.Boundaries.Clear(); nv.Save(); nv.Validate();
            check(string.Join(",", device.Trace) == "W0:1:3,S300,W0:20,W160:140,W0:1:7,W20:140,W0:20,W0:1:1");
            for (int i = 0; i < device.Boundaries.Count; ++i)
            {
                byte[] loaded = null; var restart = new Device(device.Boundaries[i]);
                var cold = new OriginalNv2Storage(restart, () => loaded, data => loaded = data, logs.Add);
                cold.Reload(); check(Equal(loaded, i < 4 ? old : grown)); cold.Validate(); check(cold.State == 1);
            }
            current = next; device.Boundaries.Clear(); nv.Save(); nv.Validate(); check(device.Size == 290 && Equal(device.Read(155, 135), next));
            for (int i = 0; i < device.Boundaries.Count; ++i)
            {
                byte[] loaded = null; var restart = new Device(device.Boundaries[i]);
                var cold = new OriginalNv2Storage(restart, () => loaded, data => loaded = data, logs.Add);
                cold.Reload(); check(Equal(loaded, i < 3 ? grown : next)); cold.Validate(); check(cold.State == 1);
            }
            // Explicit Save1 persists state4/5; Reload reads the OLD copy and leaves it pending.
            foreach (byte[] replacement in new[] { next, grown })
            {
                current = old; device = new Device(baseline);
                nv = new OriginalNv2Storage(device, () => current, data => current = data, logs.Add); nv.Reload();
                current = replacement; nv.Save1(); check(nv.State == (replacement.Length == 135 ? 4 : 5));
                byte[] loaded = null; var restart = new Device(device.Bytes);
                var cold = new OriginalNv2Storage(restart, () => loaded, data => loaded = data, logs.Add);
                cold.Reload(); check(Equal(loaded, old) && cold.State == nv.State);
                cold.Save2(); check(Equal(loaded, replacement) && cold.State == 1); cold.Validate();
                byte[] corruptPending = (byte[])device.Bytes.Clone(); corruptPending[20] ^= 1;
                fault(() => new OriginalNv2Storage(new Device(corruptPending), () => old, data => { }, logs.Add).Reload());
            }
            for (int which = 0; which < 2; ++which)
            {
                byte[] damaged = (byte[])baseline.Clone(); damaged[which == 0 ? 20 : 155] ^= 1;
                current = null; logs.Clear(); device = new Device(damaged);
                nv = new OriginalNv2Storage(device, () => current, data => current = data, logs.Add);
                nv.Reload(); nv.Validate(); check(Equal(current, old));
                check(logs.Count == 1 && logs[0] == (which == 0 ? "NVCP1" : "NVCP2"));
            }
            byte[] bad = (byte[])baseline.Clone(); bad[20] ^= 1; bad[155] ^= 1;
            fault(() => new OriginalNv2Storage(new Device(bad), () => old, data => { }, logs.Add).Reload());
            bad = (byte[])baseline.Clone(); bad[155] ^= 1;
            ushort validDifferent = OriginalNvCrc16.Calculate(bad, 155, 135); bad[18] = (byte)validDifferent; bad[19] = (byte)(validDifferent >> 8);
            fault(() => new OriginalNv2Storage(new Device(bad), () => old, data => { }, logs.Add).Reload());
            foreach (int offset in new[] { 0, 1 })
            {
                bad = (byte[])baseline.Clone(); bad[offset] = offset == 0 ? (byte)8 : (byte)0;
                fault(() => new OriginalNv2Storage(new Device(bad), () => old, data => { }, logs.Add).Reload());
            }
            bad = new byte[289]; Array.Copy(baseline, bad, bad.Length);
            fault(() => new OriginalNv2Storage(new Device(bad), () => old, data => { }, logs.Add).Reload());
            // Preserve the two reserved header bytes, not just logical fields.
            bad = (byte[])baseline.Clone(); bad[2] = 0x9a; bad[3] = 0xbc; current = old;
            nv = new OriginalNv2Storage(new Device(bad), () => current, data => current = data, logs.Add);
            nv.Reload(); current = next; nv.Save(); nv.Validate(); check(nv.Header[2] == 0x9a && nv.Header[3] == 0xbc);
            current = old; device = new Device(baseline);
            nv = new OriginalNv2Storage(device, () => current, data => current = data, logs.Add); nv.Reload();
            device.Bytes[2] ^= 1; fault(nv.Validate); device.Bytes[2] ^= 1;
            Array.Resize(ref device.Bytes, 291); fault(nv.Validate); Array.Resize(ref device.Bytes, 290);
            current = next; fault(nv.Validate);
            // Actual fresh PC file: close and reopen; never touch an existing financial scope.
            if (freshFile == null) return checks;
            if (File.Exists(freshFile)) throw new Exception("NV validation output already exists");
            current = old;
            using (var file = new OriginalNvFileDevice(freshFile))
            {
                nv = new OriginalNv2Storage(file, () => current, data => current = data, logs.Add);
                nv.Reload(); current = next; nv.Save(); nv.Validate(); check(file.Size == 290);
            }
            current = null;
            using (var file = new OriginalNvFileDevice(freshFile))
            {
                nv = new OriginalNv2Storage(file, () => current, data => current = data, logs.Add);
                nv.Reload(); nv.Validate(); check(Equal(current, next));
                fault(() => file.Read(289, 2)); fault(() => file.Write(290, new byte[1]));
            }
            check(File.ReadAllBytes(freshFile)[0] == 1);
            return checks;
        }
    }
}
