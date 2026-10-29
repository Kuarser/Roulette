using System;

namespace RouletteDeluxe
{
    // bally::NV::CRC16 at 0947d850. All 256 words of its original table
    // at 09764ea0 match the 0x1021 recurrence below. Only low 16 bits are returned.
    public static class OriginalNvCrc16
    {
        public static ushort Calculate(byte[] bytes, int offset, int count)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (offset < 0 || count < 0 || offset > bytes.Length - count)
                throw new ArgumentOutOfRangeException(nameof(count));
            uint crc = 0xffff;
            for (int index = offset; index < offset + count; ++index)
            {
                crc ^= (uint)bytes[index] << 8;
                for (int bit = 0; bit < 8; ++bit)
                    crc = ((crc << 1) ^ ((crc & 0x8000) != 0 ? 0x1021u : 0u)) & 0xffff;
            }
            return (ushort)crc;
        }
    }

    public static class OriginalNvCrc16Validation
    {
        // Independent reference uses the raw original ELF table, not the recurrence.
        public static int Validate(byte[] originalTable)
        {
            if (originalTable == null || originalTable.Length != 512)
                throw new ArgumentException("Expected original 256-word NV CRC table");
            int checks = 0;
            Action<byte[], int, int> compare = (bytes, offset, count) =>
            {
                uint accumulator = 0xffffffff;
                for (int i = offset; i < offset + count; ++i)
                {
                    int tableIndex = (int)(((accumulator >> 8) & 255) ^ bytes[i]) * 2;
                    uint word = (uint)(originalTable[tableIndex] | originalTable[tableIndex + 1] << 8);
                    accumulator = unchecked((accumulator << 8) ^ word);
                }
                if (OriginalNvCrc16.Calculate(bytes, offset, count) != (ushort)accumulator)
                    throw new Exception("Original NV CRC table reference mismatch");
                ++checks;
            };
            compare(new byte[0], 0, 0);
            for (int value = 0; value < 256; ++value)
            {
                compare(new[] { (byte)value }, 0, 1);
                compare(new[] { (byte)value, (byte)(255 - value) }, 0, 2);
            }
            byte[] payload = new byte[135];
            for (int i = 0; i < payload.Length; ++i) payload[i] = (byte)(i * 73 + 19);
            compare(payload, 0, payload.Length);
            compare(payload, 17, 99);
            compare(payload, payload.Length, 0);
            byte[] longer = new byte[4096];
            for (int i = 0; i < longer.Length; ++i) longer[i] = (byte)(i ^ (i >> 8));
            compare(longer, 0, longer.Length);
            byte[] digits = System.Text.Encoding.ASCII.GetBytes("123456789");
            compare(digits, 0, digits.Length);
            if (OriginalNvCrc16.Calculate(digits, 0, digits.Length) != 0x29b1)
                throw new Exception("Original NV CRC fixed vector mismatch");
            ++checks;
            return checks;
        }
    }
}
