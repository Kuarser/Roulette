using System;

namespace RouletteDeluxe
{
    // Original Game and FeatureData byte contracts. These functions do not
    // choose a feature, supply awards or move money on behalf of a provider.
    public static class OriginalSpecialFeatureState
    {
        public const int GenericHistorySize = 0x30b;
        public const int FeatureDataOffset = 0xa54;
        // SaveFeatureData copies 0x800 dwords, including reserved tail bytes.
        public const int FeatureDataSize = 0x2000;

        public sealed class Host
        {
            public Action<int> SoftValidateNv;
            public Action<int> SaveNv;
            public Func<int, int, int> GetTriggerBet;
            public Action<int, bool, bool> SetBet;
            public Action<int> ReportWager;
            public Action<int, bool> SetFeatureDataBit;
            public Action SaveFeatureData;
        }

        // Game::CalculateHistoryFeatures, 0x080eaf70. Keep uint underflow;
        // DisplayHistoryPage separately treats the returned word as signed.
        public static uint CalculateHistoryFeatures(byte[] game, byte[] history)
        {
            Need(game, 0x4fe4, "game");
            Need(history, GenericHistorySize, "history");
            if (ReadInt32(game, 0x4fe0) == 0)
            {
                uint count = history[0x8e];
                if (count == 0) return history[0xfe] == 7 ? 1u : 0u;
                for (int i = 0; i < history[0x8e]; ++i)
                    if (history[0xe9 + i] != 0) count = unchecked(count - 1);
                if (history[0xfe] == 4 || history[0xfe] == 9)
                    count = unchecked(count - 1);
                return count;
            }
            uint selected = game[0x2a54];
            for (int i = 0; i < game[0x2a54]; ++i)
                if (game[0x2aaf + i] != 0) selected = unchecked(selected - 1);
            return unchecked(selected + (game[0x2c91] == 0 ? 0u : 1u));
        }

        // Game::DoTriggerFeature 0x080e8bc0 followed by the Roulette override
        // 0x0819b380. Preserve the two NV owners and their save order.
        public static void Trigger(byte[] game, int featureType, Host host)
        {
            Need(game, 0x919d, "game");
            if (host == null || host.SoftValidateNv == null || host.SaveNv == null ||
                host.GetTriggerBet == null || host.SetBet == null || host.ReportWager == null)
                throw new ArgumentException("Incomplete original feature trigger host");
            host.SoftValidateNv(0x20c);
            game[0x281] = unchecked((byte)featureType);
            CopyWord(game, 0x92c, 0x934);
            CopyWord(game, 0x928, 0x930);
            host.SaveNv(0x20c);
            if (game[0x281] == 9)
            {
                host.ReportWager(0);
                return;
            }
            int wager = game[0x281] == 7 || game[0x281] == 2 ?
                host.GetTriggerBet(ReadInt32(game, 0x264), ReadInt32(game, 0x4f64)) : 0;
            host.SoftValidateNv(0x8da0);
            WriteInt32(game, 0x9193, game[0x919c]);
            CopyWord(game, 0x918b, 0x9197);
            host.SaveNv(0x8da0);
            host.SetBet(wager, true, true);
            // SetBet is an original virtual boundary; re-read the feature
            // type after it, as the machine code does.
            if (game[0x281] == 4)
            {
                host.SoftValidateNv(0x20c);
                CopyWord(game, 0x930, 0x928);
                CopyWord(game, 0x934, 0x92c);
                host.SaveNv(0x20c);
            }
            host.ReportWager(wager);
        }

        // FeatGameAgent::NextFeature 0x082967d0 uses signed char for the
        // index, count and returned feature ID, with byte increment wrapping.
        public static int NextFeature(byte[] game, Host host)
        {
            Need(game, FeatureDataOffset + FeatureDataSize, "game");
            if (host == null || host.SetFeatureDataBit == null || host.SaveFeatureData == null)
                throw new ArgumentException("Incomplete original feature sequence host");
            game[0xa54] = unchecked((byte)(game[0xa54] + 1));
            int result;
            int index = unchecked((sbyte)game[0xa54]);
            if (index < unchecked((sbyte)game[0xa55]))
                result = unchecked((sbyte)game[0xa56 + index]);
            else
            {
                game[0xa54] = 0xff;
                result = 0;
            }
            host.SetFeatureDataBit(2, false);
            host.SaveFeatureData();
            return result;
        }

        public static byte[] CaptureFeatureData(byte[] game)
        {
            Need(game, FeatureDataOffset + FeatureDataSize, "game");
            var result = new byte[FeatureDataSize];
            Buffer.BlockCopy(game, FeatureDataOffset, result, 0, result.Length);
            return result;
        }

        private static void Need(byte[] bytes, int size, string name)
        {
            if (bytes == null || bytes.Length < size) throw new ArgumentException("Truncated original " + name);
        }

        private static int ReadInt32(byte[] bytes, int offset)
        {
            return unchecked((int)((uint)bytes[offset] | (uint)bytes[offset + 1] << 8 |
                (uint)bytes[offset + 2] << 16 | (uint)bytes[offset + 3] << 24));
        }

        private static void WriteInt32(byte[] bytes, int offset, int value)
        {
            for (int i = 0; i < 4; ++i) bytes[offset + i] = unchecked((byte)((uint)value >> (i * 8)));
        }

        private static void CopyWord(byte[] bytes, int from, int to)
        {
            Buffer.BlockCopy(bytes, from, bytes, to, 4);
        }
    }
}
