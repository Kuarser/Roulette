using System;

namespace RouletteDeluxe
{
    // The original RCG0 feature owns a result, not a player balance. Its
    // caller supplies initialized input fields and pays the received result.
    public static class OriginalResidualCreditFeature
    {
        public const int CoreDataSize = 0x14;
        public const int RegularDataSize = 0xb;
        public sealed class Host
        {
            public Action<string, int, uint> TopboxCommand;
            public Func<uint, uint, uint> DrawInclusive;
            public Func<bool> IsSuspended;
            public Action SaveSafeRam;
            public Action<string, int, uint, int> TopboxNotify;
            public Action<byte, ulong> FeatureComplete;
            public Action SetInactive;
            public Action PrepareForCompletion;
        }

        // RCGFeature::BeginFeature at 0x08091320 in RCGFeature ELF.
        public static void Begin(byte[] state, Host host)
        {
            Need(state, host);
            host.TopboxCommand("RCGTOP0", 0xb, 1);
            state[0xb80] = 0;
            uint maximum = Read32(state, 0xbaf);
            uint threshold = Read32(state, 0xbab);
            uint draw = host.DrawInclusive(1, maximum);
            if (host.IsSuspended()) return;
            Write32(state, 0xb9f, draw <= threshold ? maximum : 0);
            Write32(state, 0xba3, 0);
            state[0xba8] |= 0xf0;
            state[0xb80] = 2;
            host.SaveSafeRam();
            host.TopboxNotify("RCGTOP0", 0x12,
                Read32(state, 0xb9f) != 0 || Read32(state, 0xba3) != 0 ? 1u : 0u, 0x1b);
        }

        // IntroDone 0x08091460 and StartTally 0x080916a0 share this sequence.
        public static void IntroDone(byte[] state, Host host)
        {
            Need(state, host);
            state[0xb80] = 3;
            host.TopboxNotify("RCGTOP0", 0x14, 0, 0x1c);
        }

        // Both inherited CoreBaseFeature loaders copy the actual serialized
        // ranges. Topbox and RCG share the same 20-byte core record; the two
        // input words are core+3 and core+7, not arbitrary PC probabilities.
        public static void LoadCoreData(byte[] state, byte[] coreData)
        {
            if (state == null || state.Length < 0xbbc || coreData == null ||
                coreData.Length != CoreDataSize) throw new ArgumentException("Invalid RCG core record");
            Buffer.BlockCopy(coreData, 0, state, 0xba8, CoreDataSize);
        }

        public static void LoadRegularData(byte[] state, byte[] regularData)
        {
            if (state == null || state.Length < 0xba8 || regularData == null ||
                regularData.Length != RegularDataSize) throw new ArgumentException("Invalid RCG regular record");
            Buffer.BlockCopy(regularData, 0, state, 0xb9d, RegularDataSize);
        }

        // RCGFeature::ProcessEventMsg 0x08091100 and its physical jump table
        // at 0x082cfbc0. The caller owns the +0x68 event-dispatch enable byte.
        public static bool ProcessEvent(byte[] state, uint eventId, int argument1,
            int argument2, Host host)
        {
            Need(state, host);
            if (state[0x68] == 0) return false;
            byte current = state[0xb80];
            Write32(state, 0xb84, eventId);
            Write32(state, 0xb88, unchecked((uint)argument1));
            Write32(state, 0xb8c, unchecked((uint)argument2));
            if (current == 2 && eventId == 0x1b)
            {
                state[0xb80] = 2;
                IntroDone(state, host);
                return true;
            }
            if (current == 3 && eventId == 0x1c || current == 5 && eventId == 0x1d)
            {
                state[0xb80] = 0;
                host.PrepareForCompletion();
                return true;
            }
            // The original table's state-0/event-0 entry has a null member
            // function. Do not turn that invalid native dispatch into an award.
            if (current == 0 && eventId == 0)
                throw new InvalidOperationException("Original RCG state-0/event-0 dispatch has no target");
            return false;
        }

        // PrepFeatureForCompletion and RecallComplete both dispatch the
        // concrete RCG virtual +0x180 to FtrComplete(FEAT_OVER_TYPE=1).
        public static void PrepareForCompletion(byte[] state, Host host)
        {
            Complete(state, 1, host);
        }

        public static void Recall(byte[] state, Host host)
        {
            Need(state, host);
            state[0xb80] = 2;
            host.TopboxNotify("RCGTOP0", 0x12, 0, 0x1b);
        }

        // FeatureRestore 0x080914a0. The original sends zero as the restore
        // command argument even when a win is present in the saved fields.
        public static void Restore(byte[] state, Host host)
        {
            Need(state, host);
            host.TopboxCommand("RCGTOP0", 0xb, 0);
            if (state[0xb80] == 0x10) IntroDone(state, host);
            else if (state[0xb80] == 0xf)
            {
                state[0xb80] = 2;
                host.TopboxNotify("RCGTOP0", 0x12, 0, 0x1b);
            }
            else if (state[0xb80] == 0x11) host.PrepareForCompletion();
        }

        // FtrComplete 0x080911b0. Caller-specific group/index transport
        // remains in the host; the completion flags and 64-bit award match.
        public static void Complete(byte[] state, uint overType, Host host)
        {
            Need(state, host);
            if (host.IsSuspended()) return;
            ulong award = Read32(state, 0xb9f) | (ulong)Read32(state, 0xba3) << 32;
            host.FeatureComplete(unchecked((byte)(((overType & 3) << 6) | 0x20)), award);
            state[0xb90] = 0;
            host.SetInactive();
        }

        private static void Need(byte[] state, Host host)
        {
            if (state == null || state.Length < 0xbb3) throw new ArgumentException("Truncated RCG state");
            if (host == null || host.TopboxCommand == null || host.DrawInclusive == null ||
                host.IsSuspended == null || host.SaveSafeRam == null || host.TopboxNotify == null ||
                host.FeatureComplete == null || host.SetInactive == null || host.PrepareForCompletion == null)
                throw new ArgumentException("Incomplete RCG feature host");
        }

        private static uint Read32(byte[] state, int offset)
        {
            return (uint)state[offset] | (uint)state[offset + 1] << 8 |
                (uint)state[offset + 2] << 16 | (uint)state[offset + 3] << 24;
        }

        private static void Write32(byte[] state, int offset, uint value)
        {
            for (int i = 0; i < 4; ++i) state[offset + i] = unchecked((byte)(value >> (8 * i)));
        }
    }
}
