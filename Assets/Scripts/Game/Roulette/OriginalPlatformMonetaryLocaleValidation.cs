using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace RouletteDeluxe.Validation
{
    public static class OriginalPlatformMonetaryLocaleValidation
    {
        private static void Require(bool ok, string message)
        { if (!ok) throw new Exception("Platform monetary validation: " + message); }
        private static void Equal(OriginalPlatformMonetaryLocale locale, long amount,
            uint divisor, bool international, uint width, string expected)
        {
            Require(locale.FormatText(amount, divisor, international, width) == expected,
                amount + "/" + divisor + " expected " + expected);
        }
        public static int Validate(byte[] original)
        {
            int checks = 0;
            var rm = OriginalPlatformMonetaryLocale.Read(original);
            Equal(rm, 0, 100, false, 0, "RM0.00"); ++checks;
            Equal(rm, 995, 100, false, 0, "RM9.95"); ++checks;
            Equal(rm, -995, 100, false, 0, "(RM9.95)"); ++checks;
            Equal(rm, long.MaxValue, 100, false, 0, "RM92,233,720,368,547,758.07"); ++checks;
            Equal(rm, long.MinValue, 100, false, 0, "(RM92,233,720,368,547,758.08)"); ++checks;
            Equal(rm, 995, 10, false, 0, "RM99.50"); ++checks;
            Equal(rm, 995, 2, false, 0, "RM99.50"); ++checks;
            Equal(rm, 995, 1000, false, 0, "RM0.95"); ++checks;
            Equal(rm, 995, 1, false, 0, "RM995.00"); ++checks;
            Equal(rm, 995, 0, false, 0, "RM995.00"); ++checks;
            Equal(rm, 995, 100, false, 10, "    RM9.95"); ++checks;
            string[] after = { "(9.95 C)", "S9.95 C", "9.95 CS", "9.95S C", "9.95 CS" };
            string[] before = { "(C 9.95)", "SC 9.95", "C 9.95S", "SC 9.95", "C S9.95" };
            for (int position = 0; position < 5; ++position)
                for (int precedes = 0; precedes < 2; ++precedes)
                {
                    var fixture = Fixture(original, new Dictionary<int, byte[]> {
                        { 1, Encoding.ASCII.GetBytes("C\0") }, { 5, Encoding.ASCII.GetBytes("S\0") },
                        { 9, new byte[] { (byte)precedes } }, { 10, new byte[] { 2 } },
                        { 13, new byte[] { (byte)position } } });
                    Equal(OriginalPlatformMonetaryLocale.Read(fixture), 995, 100, false, 0,
                        precedes == 0 ? after[position] : before[position]); ++checks;
                }
            foreach (int precision in new[] { 0, 1, 127, 255 })
            {
                var fixture = Fixture(original, new Dictionary<int, byte[]> { { 8, new byte[] { (byte)precision } } });
                string expected = precision == 1 ? "RM9.5" : precision == 127 ? "RM9." : "RM9";
                Equal(OriginalPlatformMonetaryLocale.Read(fixture), 995, 100, false, 0, expected); ++checks;
            }
            var internationalFixture = Fixture(original, new Dictionary<int, byte[]> {
                { 0, Encoding.ASCII.GetBytes("INT \0") }, { 7, new byte[] { 0 } },
                // Intl sign/spacing fields intentionally disagree with domestic.
                { 15, new byte[] { 0 } }, { 16, new byte[] { 1 } }, { 19, new byte[] { 2 } } });
            Equal(OriginalPlatformMonetaryLocale.Read(internationalFixture), 995, 100, true, 0, "INT 9"); ++checks;
            var invalidPosition = Fixture(original, new Dictionary<int, byte[]> { { 13, new byte[] { 255 } } });
            Equal(OriginalPlatformMonetaryLocale.Read(invalidPosition), 995, 100, false, 0, "9.95"); ++checks;
            var multi = OriginalPlatformMonetaryLocale.Read(Fixture(original, new Dictionary<int, byte[]> {
                { 2, new byte[] { 0xd9, 0xab, 0 } }, { 3, Encoding.ASCII.GetBytes("ab\0") } }));
            byte[] raw = multi.FormatBytes(100000001, 100, false, 0);
            Require(BitConverter.ToString(raw) == "52-4D-31-62-61-30-30-30-62-61-30-30-30-AB-D9-30-31", "separator bytes reverse"); ++checks;
            bool rejected = false;
            try { multi.FormatText(100000001, 100, false, 0); } catch (DecoderFallbackException) { rejected = true; }
            Require(rejected, "invalid native UTF8 is explicit"); ++checks;
            rejected = false;
            try { rm.FormatBytes(1, 100, false, uint.MaxValue); } catch (ArgumentOutOfRangeException) { rejected = true; }
            Require(rejected, "managed width limit"); ++checks;
            var stopped = OriginalPlatformMonetaryLocale.Read(Fixture(original, new Dictionary<int, byte[]> { { 4, new byte[] { 3, 127, 0 } } }));
            Equal(stopped, 100000001, 100, false, 0, "RM1000,000.01"); ++checks;
            var indian = OriginalPlatformMonetaryLocale.Read(Fixture(original, new Dictionary<int, byte[]> { { 4, new byte[] { 3, 2, 0 } } }));
            Equal(indian, 100000001, 100, false, 0, "RM10,00,000.01"); ++checks;
            return checks;
        }
        private static int Word(byte[] bytes, int at)
        { return bytes[at] | bytes[at + 1] << 8 | bytes[at + 2] << 16 | bytes[at + 3] << 24; }
        private static byte[] Fixture(byte[] original, Dictionary<int, byte[]> replacements)
        {
            // Synthetic locale fixtures are never admitted through catalog loader.
            var fields = new byte[46][];
            for (int i = 0; i < 46; ++i)
            {
                if (replacements.TryGetValue(i, out fields[i])) continue;
                int from = Word(original, 8 + i * 4);
                int to = i == 45 ? original.Length : Word(original, 12 + i * 4);
                fields[i] = new byte[to - from]; Array.Copy(original, from, fields[i], 0, to - from);
            }
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(0x20031111); writer.Write(46); int offset = 192;
                foreach (byte[] field in fields) { writer.Write(offset); offset += field.Length; }
                foreach (byte[] field in fields) writer.Write(field);
                return stream.ToArray();
            }
        }
    }
}
