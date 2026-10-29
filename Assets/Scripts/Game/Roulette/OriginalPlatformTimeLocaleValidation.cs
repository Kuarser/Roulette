using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace RouletteDeluxe.Validation
{
    public static class OriginalPlatformTimeLocaleValidation
    {
        private static void Require(bool ok, string message)
        { if (!ok) throw new Exception("Original time validation: " + message); }
        public static int Validate(byte[] original)
        {
            var locale = OriginalPlatformTimeLocale.Read(original); int checks = 0;
            var stamp = OriginalMachineDateTime.ReadPacked(new byte[] { 0xea, 7, 10, 5, 6, 7, 8, 0xe7, 3 });
            Require(stamp.Year == 2026 && stamp.Month == 10 && stamp.Day == 5 && stamp.Hour == 6 &&
                stamp.Minute == 7 && stamp.Second == 8 && stamp.Millisecond == 999, "packed ABI"); ++checks;
            Require(locale.Format(stamp) == "05/10/2026 06:07:08", "ms_MY original format"); ++checks;
            Require(locale.Format(new OriginalMachineDateTime(2024, 2, 29, 23, 59, 60, 0)) == "29/02/2024 23:59:60", "leap day and leap second fields"); ++checks;
            Require(locale.Format(new OriginalMachineDateTime(2026, 10, 5, 6, 7, 8, 0)) == locale.Format(stamp), "milliseconds excluded by%x %T"); ++checks;
            var saved = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ar-SA");
                Require(locale.Format(stamp) == "05/10/2026 06:07:08", "system culture not substituted"); ++checks;
            }
            finally { Thread.CurrentThread.CurrentCulture = saved; }
            Require(OriginalLanguageTranslator.FormatTemplate("FMT_DateAndTime FMT_Number", new[] { "unused", "001" }, null,
                () => locale.Format(stamp)) == "05/10/2026 06:07:08 001", "translator real formatter connection"); ++checks;
            bool rejected = false;
            try { OriginalMachineDateTime.ReadPacked(new byte[8]); } catch (ArgumentException) { rejected = true; }
            Require(rejected, "packed length guard"); ++checks;
            foreach (int kind in new[] { 0, 1, 2 })
            {
                byte[] changed = (byte[])original.Clone();
                if (kind == 0) changed[0] ^= 1;
                if (kind == 1) changed[4] = 0;
                if (kind == 2) { changed[8] = 0; changed[9] = 0; changed[10] = 0; changed[11] = 0; }
                rejected = false;
                try { OriginalPlatformTimeLocale.Read(changed); } catch (InvalidDataException) { rejected = true; }
                Require(rejected, "header/offset guard " + kind); ++checks;
            }
            return checks;
        }
        public static string ExpectedSample(string format)
        {
            switch (format)
            {
                case "%d/%m/%Y": return "05/10/2026 06:07:08";
                case "%Y/%m/%d": return "2026/10/05 06:07:08";
                case "%d.%m.%Y": return "05.10.2026 06:07:08";
                case "%d-%m-%Y": return "05-10-2026 06:07:08";
                case "%m/%d/%Y": return "10/05/2026 06:07:08";
                case "%Y-%m-%d": return "2026-10-05 06:07:08";
                case "%Y.%m.%d": return "2026.10.05 06:07:08";
                case "%d-%m-%y": return "05-10-26 06:07:08";
                default: throw new Exception("Unknown source date format");
            }
        }
    }
}
