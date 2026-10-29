using System;
using System.Collections.Generic;
using System.IO;

namespace RouletteDeluxe
{
    public static class OriginalHandpaySelectionPersistenceValidation
    {
        private sealed class Device : IOriginalNvDevice
        {
            public byte[] Bytes = new byte[0];
            public readonly List<OriginalHandpaySelectionNvState> Commits = new List<OriginalHandpaySelectionNvState>();
            public bool FailWrite;
            public int Size { get { return Bytes.Length; } }
            public byte[] Read(int offset, int count)
            { byte[] data = new byte[count]; Array.Copy(Bytes, offset, data, 0, count); return data; }
            public void Write(int offset, byte[] data)
            {
                if (FailWrite) throw new OriginalNvFault("Injected selection NV write failure");
                Array.Copy(data, 0, Bytes, offset, data.Length);
                if (offset == 0 && data.Length == 1 && data[0] == 1)
                    Commits.Add(new OriginalCreditOutNvPayload(Read(20, 135)).ReadSelection());
            }
            public void SetSize(int size) { Array.Resize(ref Bytes, size); }
        }
        private static byte[] Slice(byte[] bytes, int offset, int size)
        { byte[] result = new byte[size]; Array.Copy(bytes, offset, result, 0, size); return result; }
        private static bool Equal(byte[] a, byte[] b)
        { if (a.Length != b.Length) return false; for (int i = 0; i < a.Length; ++i) if (a[i] != b[i]) return false; return true; }
        private static byte[] Seed()
        {
            byte[] bytes = new byte[135]; for (int i = 0; i < bytes.Length; ++i) bytes[i] = (byte)(i * 19 + 7);
            for (int i = 0; i < 8; ++i) bytes[0x24 + i] = (byte)(0xfedcba9876543210UL >> (8 * i));
            var payload = new OriginalCreditOutNvPayload(bytes);
            payload.UpdateSelection(new OriginalHandpaySelectionNvState { InProgress = 0, ResidualSubstate = 2, HandpaySelectedByte = 0x7f });
            return payload.Bytes;
        }
        public static int Validate(byte[] nativeFixtures, string freshDirectory)
        {
            int checks = 0;
            Action<bool> check = value => { if (!value) throw new Exception("Persistent handpay regression " + checks); ++checks; };
            if (nativeFixtures == null || nativeFixtures.Length != 414) throw new ArgumentException("Original handpay fixtures required");
            long selectionAmount = unchecked((long)0xfedcba9876543210UL);
            byte[] activation = Slice(nativeFixtures, 0, 185), deactivation = Slice(nativeFixtures, 185, 185), residual = Slice(nativeFixtures, 370, 44);
            check(Equal(OriginalHandpaySelectionEvents.Tilt(true, selectionAmount), activation));
            check(Equal(OriginalHandpaySelectionEvents.Tilt(false, selectionAmount), deactivation));
            check(Equal(OriginalHandpaySelectionEvents.Residual(0x89abcdef, -123456789), residual));
            for (int mask = 0; mask < 4; ++mask)
            {
                bool before = (mask & 1) != 0, after = (mask & 2) != 0, released = false;
                long amount = selectionAmount; var trace = new List<string>(); var events = new List<byte[]>();
                var lifecycle = new OriginalHandpaySelectionLifecycle(() => { trace.Add("amount"); return amount; },
                    code => { check(code == 0x6020); trace.Add("pending"); return released ? after : before; },
                    (data, flags) => { check(flags == 0); trace.Add("event"); events.Add(data); },
                    () => trace.Add("create"), () => { trace.Add("release"); released = true; });
                lifecycle.Prepare(); amount = 7; lifecycle.Release();
                string expected = "amount,pending," + (!before ? "event," : "") + "create,release,pending" + (after ? ",event" : "");
                check(string.Join(",", trace) == expected);
                check(events.Count == (!before ? 1 : 0) + (after ? 1 : 0));
                if (!before) check(Equal(events[0], activation));
                if (after) check(Equal(events[events.Count - 1], deactivation));
            }
            byte[] seed = Seed();
            foreach (bool disabled in new[] { false, true }) foreach (int result in new[] { 0, 1, -1 })
            {
                var device = new Device(); bool selected = disabled || result != 0;
                using (var owner = new OriginalHandpaySelectionPersistence(device, seed, text => { }))
                {
                    var stateReference = owner.State; check(owner.SelectionAmount == selectionAmount);
                    device.Commits.Clear(); var callbacks = new List<string>(); bool released = false;
                    var coordinator = owner.CreateCoordinator(new OriginalHandpaySelectionCoordinator.Host
                    {
                        BeforeResidualSelection = () => callbacks.Add("before"),
                        SetTransactionInProgress = value => { check(value); callbacks.Add("transaction"); },
                        PrepareScreen = () => callbacks.Add("prepare"),
                        ReleaseScreenAndTiltEvents = () => { released = true; callbacks.Add("release"); },
                        MachineDisabled = () => { check(released && owner.State.InProgress == 0); owner.ValidateStorage(); callbacks.Add("disabled"); return disabled; },
                        ResidualGamble = () => { check(owner.State.ResidualSubstate == 9); owner.ValidateStorage(); callbacks.Add("gamble"); },
                        AfterResidualGamble = () => callbacks.Add("after"), EmitResidualEvent = () => callbacks.Add("event")
                    });
                    check(coordinator.Begin() && !coordinator.Begin());
                    check(device.Commits.Count == 2 && device.Commits[0].InProgress == 1 && device.Commits[0].ResidualSubstate == 2
                        && device.Commits[1].ResidualSubstate == 3 && device.Commits[1].HandpaySelectedByte == 0x7f);
                    check(coordinator.Complete(result) == selected);
                    check(string.Join(",", callbacks) == "before,transaction,prepare,release,disabled" + (selected ? "" : ",gamble,after,event"));
                    check(device.Commits.Count == (selected ? 5 : 4) && device.Commits[2].InProgress == 0 && device.Commits[2].ResidualSubstate == 3);
                    check(owner.State.ResidualSubstate == (selected ? 4 : 9) && owner.State.HandpaySelectedByte == (selected ? 1 : 0x7f));
                    owner.State.InProgress = 99; owner.Reload();
                    check(ReferenceEquals(owner.State, stateReference) && ReferenceEquals(coordinator.State, stateReference) && owner.State.InProgress == 0);
                    byte[] saved = owner.PayloadBytes;
                    for (int i = 0; i < saved.Length; ++i)
                        if (!((i >= 0x48 && i < 0x4c) || i == 0x5d || (i >= 0x63 && i < 0x67))) check(saved[i] == seed[i]);
                }
                using (var cold = new OriginalHandpaySelectionPersistence(device, new byte[135], text => { }))
                { check(cold.SelectionAmount == selectionAmount && cold.State.ResidualSubstate == (selected ? 4 : 9)); cold.ValidateStorage(); }
            }
            // Write faults propagate before the residual-substate write or completion callbacks.
            var failing = new Device();
            using (var owner = new OriginalHandpaySelectionPersistence(failing, seed, text => { }))
            {
                int lateCalls = 0;
                var coordinator = owner.CreateCoordinator(new OriginalHandpaySelectionCoordinator.Host
                {
                    BeforeResidualSelection = () => { }, SetTransactionInProgress = value => { }, PrepareScreen = () => { },
                    ReleaseScreenAndTiltEvents = () => ++lateCalls, MachineDisabled = () => { ++lateCalls; return false; },
                    ResidualGamble = () => ++lateCalls, AfterResidualGamble = () => ++lateCalls, EmitResidualEvent = () => ++lateCalls
                });
                failing.FailWrite = true; bool rejected = false;
                try { coordinator.Begin(); } catch (OriginalNvFault) { rejected = true; }
                check(rejected && lateCalls == 0 && owner.State.ResidualSubstate == 2);
                failing.FailWrite = false; owner.Reload(); check(owner.State.InProgress == 0);
            }
            if (freshDirectory == null) return checks;
            if (Directory.Exists(freshDirectory)) throw new Exception("Persistent handpay output already exists");
            Directory.CreateDirectory(freshDirectory);
            foreach (bool disabled in new[] { false, true }) foreach (int result in new[] { 0, 1, -1 })
            {
                string path = Path.Combine(freshDirectory, (disabled ? "disabled_" : "enabled_") + result + ".bin");
                using (var owner = OriginalHandpaySelectionPersistence.OpenFile(path, seed, text => { }))
                {
                    var coordinator = owner.CreateCoordinator(Host(owner, false, (data, flags) => { }, () => { }, () => { }));
                    coordinator.Begin(); owner.ValidateStorage(); check(owner.State.InProgress == 1 && owner.State.ResidualSubstate == 3);
                }
                var events = new List<byte[]>(); bool released = false; int gambles = 0;
                using (var owner = OriginalHandpaySelectionPersistence.OpenFile(path, new byte[135], text => { }))
                {
                    check(owner.State.InProgress == 1 && owner.State.ResidualSubstate == 3 && owner.SelectionAmount == selectionAmount);
                    var coordinator = owner.CreateCoordinator(Host(owner, disabled, (data, flags) => { check(flags == 0); events.Add(data); },
                        () => released = true, () => { ++gambles; check(owner.State.ResidualSubstate == 9); }));
                    coordinator.Begin(); check(coordinator.Complete(result) == (disabled || result != 0)); owner.ValidateStorage();
                    check(released && gambles == (!disabled && result == 0 ? 1 : 0));
                    check(events.Count == (!disabled && result == 0 ? 3 : 2) && Equal(events[0], activation) && Equal(events[1], deactivation));
                    if (!disabled && result == 0) check(Equal(events[2], residual));
                }
                using (var cold = OriginalHandpaySelectionPersistence.OpenFile(path, new byte[135], text => { }))
                { check(cold.State.InProgress == 0 && cold.State.ResidualSubstate == (!disabled && result == 0 ? 9 : 4)); cold.ValidateStorage(); }
            }
            return checks;
        }
        private static OriginalHandpaySelectionCoordinator.Host Host(OriginalHandpaySelectionPersistence owner, bool disabled,
            Action<byte[], uint> emit, Action release, Action gamble)
        {
            bool pending = false;
            var lifecycle = new OriginalHandpaySelectionLifecycle(() => owner.SelectionAmount, code => pending,
                (data, flags) => { emit(data, flags); pending = data[1] == 1; }, () => { }, release);
            return new OriginalHandpaySelectionCoordinator.Host
            {
                BeforeResidualSelection = () => { }, SetTransactionInProgress = value => { },
                PrepareScreen = lifecycle.Prepare, ReleaseScreenAndTiltEvents = lifecycle.Release,
                MachineDisabled = () => disabled, ResidualGamble = gamble, AfterResidualGamble = () => { },
                EmitResidualEvent = () => emit(OriginalHandpaySelectionEvents.Residual(0x89abcdef, -123456789), 0)
            };
        }
    }
}
