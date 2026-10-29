using System;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalFeatureRecallMessages
    {
        // FeatGameAgent::SendRecallMsg1, 0x082960c0. The returned bytes
        // represent the original request; transport and feature execution
        // remain separate owners.
        public static byte[] First(byte[] featureData, int featureIndex,
            int gameType, byte[] coreData, bool preserveRecallFields)
        {
            RequireData(featureData);
            if (!preserveRecallFields)
            {
                int pay = checked((featureIndex - 1) * 0x2a);
                if (gameType == 0 && coreData != null)
                {
                    Need(coreData, 0xb1);
                    featureData[0x175] = coreData[0x2a];
                    Buffer.BlockCopy(featureData, pay + 0x11, featureData, 0x150, 8);
                    int count = coreData[0xb0];
                    Need(coreData, 0x2e + count * 2);
                    Write16(featureData, 0x177, count);
                    Buffer.BlockCopy(coreData, 0x2e, featureData, 0xd7f, count * 2);
                }
                else if (gameType == 1)
                {
                    Need(coreData, 20);
                    featureData[0x175] = 0;
                    Buffer.BlockCopy(featureData, pay + 0x11, featureData, 0x150, 8);
                    Write16(featureData, 0x177, 5);
                    for (int i = 0; i < 5; ++i)
                    {
                        featureData[0xd7f + i * 2] = coreData[i * 4];
                        featureData[0xd80 + i * 2] = 0;
                    }
                }
            }
            var message = new byte[0x13b4];
            Write32(message, 0, 0x308);
            Write32(message, 4, message.Length);
            Buffer.BlockCopy(featureData, 0x122, message, 8, 0x13ac);
            return message;
        }

        // FeatGameAgent::SendRecallMsg2, 0x08296300. StartRecallFeature
        // sends First then Second, with topbox=false and command=0.
        public static byte[] Second(byte[] featureData, int featureIndex,
            string featureName, int extraDataLength, bool topbox,
            int command, bool freeGameRecall)
        {
            RequireData(featureData);
            var message = new byte[0x558];
            Write32(message, 0, 0x309);
            Write32(message, 4, message.Length);
            Buffer.BlockCopy(featureData, 0, message, 0x34, 0x122);
            Buffer.BlockCopy(featureData, 0x14ce, message, 0x156, 0x3c0);
            Buffer.BlockCopy(featureData, 0x188e, message, 0x516, 60);
            Buffer.BlockCopy(featureData, 0x18ca, message, 0x554, 4);
            message[0x28] = topbox ? (byte)1 : (byte)0;
            CopyCString(Encoding.ASCII.GetBytes(featureName ?? ""), message, 8, 11);
            // strncpy copies at most ten bytes and pads after the first NUL.
            var language = new byte[10];
            Buffer.BlockCopy(featureData, 0x18ce, language, 0, language.Length);
            CopyCString(language, message, 0x1e, 10);
            if (!topbox)
            {
                message[0x14] = 0;
                message[0x13] = unchecked((byte)(featureIndex - 1));
                message[0x15] = freeGameRecall ? (byte)6 :
                    extraDataLength == 0 ? (byte)3 : (byte)4;
            }
            Write32(message, 0x2c, command);
            return message;
        }

        private static void RequireData(byte[] data)
        {
            Need(data, OriginalSpecialFeatureState.FeatureDataSize);
        }

        private static void Need(byte[] bytes, int size)
        {
            if (bytes == null || bytes.Length < size)
                throw new ArgumentException("Truncated original feature data");
        }

        private static void Write16(byte[] bytes, int offset, int value)
        {
            for (int i = 0; i < 2; ++i) bytes[offset + i] = unchecked((byte)((uint)value >> (8 * i)));
        }

        private static void Write32(byte[] bytes, int offset, int value)
        {
            for (int i = 0; i < 4; ++i) bytes[offset + i] = unchecked((byte)((uint)value >> (8 * i)));
        }

        private static void CopyCString(byte[] source, byte[] target, int offset, int length)
        {
            bool ended = false;
            for (int i = 0; i < length; ++i)
            {
                byte value = i < source.Length ? source[i] : (byte)0;
                if (value == 0) ended = true;
                target[offset + i] = ended ? (byte)0 : value;
            }
        }
    }
}
