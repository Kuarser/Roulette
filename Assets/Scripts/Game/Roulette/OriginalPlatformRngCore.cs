using System;

namespace RouletteDeluxe
{
    // The ordinary gamemgr RngEngine core recovered from the original IMG.
    // This is a deterministic engine for checking the original arithmetic.
    // Cabinet NV state and its concurrently running mix thread are separate.
    public sealed class OriginalPlatformRngCore
    {
        private const int StateSize = 624;
        private const int TwistOffset = 397;
        public const int SnapshotWordCount = StateSize + 2;
        // Boost alternates two 624-word buffers. GetRngData exports only the
        // first buffer and reconstructs the second when restoring index >= 624.
        private readonly uint[] state = new uint[StateSize * 2];
        private readonly uint seed;
        private int index = StateSize;

        public OriginalPlatformRngCore(uint seed)
        {
            if (seed == 0) throw new ArgumentOutOfRangeException("seed");
            this.seed = seed;
            state[0] = seed;
            for (int i = 1; i < StateSize; ++i)
            {
                uint previous = state[i - 1];
                state[i] = unchecked(1812433253U *
                    (previous ^ (previous >> 30)) + (uint)i);
            }
        }

        private OriginalPlatformRngCore(uint[] snapshot)
        {
            seed = snapshot[0];
            index = (int)snapshot[1];
            Array.Copy(snapshot, 2, state, 0, StateSize);
            if (index >= StateSize) Twist(0, StateSize);
        }

        // Word layout from RngMgr::GetRngData: seed, index, first 624 words.
        // The enclosing PC file format remains distinct from cabinet NV2.
        public uint[] ExportSnapshot()
        {
            var snapshot = new uint[SnapshotWordCount];
            snapshot[0] = seed;
            snapshot[1] = (uint)index;
            Array.Copy(state, 0, snapshot, 2, StateSize);
            return snapshot;
        }

        public static OriginalPlatformRngCore FromSnapshot(uint[] snapshot)
        {
            if (snapshot == null || snapshot.Length != SnapshotWordCount ||
                snapshot[0] == 0 || snapshot[1] > StateSize * 2)
                throw new ArgumentException("Invalid RNG state snapshot", "snapshot");
            return new OriginalPlatformRngCore(snapshot);
        }

        public uint NextRaw()
        {
            if (index == StateSize) Twist(0, StateSize);
            else if (index == StateSize * 2)
            {
                Twist(StateSize, 0);
                index = 0;
            }
            uint value = state[index++];
            value ^= value >> 11;
            value ^= (value << 7) & 0x9d2c5680U;
            value ^= (value << 15) & 0xefc60000U;
            value ^= value >> 18;
            return value;
        }

        public int NextInclusive(int minimum, int maximum)
        {
            if (minimum < 0 || maximum < minimum ||
                (long)maximum - minimum >= int.MaxValue)
                throw new ArgumentOutOfRangeException("maximum");
            int exclusive = maximum - minimum + 1;
            for (;;)
            {
                int mapped = OriginalRngRange.MapRaw(NextRaw(), exclusive);
                if (mapped >= 0) return minimum + mapped;
            }
        }

        // Separate unsigned path used by the original RCG Feature. Preserve
        // singleton draw consumption and reversed-endpoint normalization.
        public uint NextUnsignedInclusive(uint minimum,uint maximum)
        {
            return OriginalUnsignedRngRange.DrawInclusive(minimum,maximum,NextRaw);
        }

        private void Twist(int source, int destination)
        {
            for (int i = 0; i < StateSize; ++i)
            {
                uint next = i + 1 < StateSize ?
                    state[source + i + 1] : state[destination];
                uint pair = (state[source + i] & 0x80000000U) |
                    (next & 0x7fffffffU);
                int offset = i + TwistOffset;
                uint other = offset < StateSize ? state[source + offset] :
                    state[destination + offset - StateSize];
                state[destination + i] = other ^
                    (pair >> 1) ^ ((pair & 1U) == 0 ? 0U : 0x9908b0dfU);
            }
        }
    }
}
