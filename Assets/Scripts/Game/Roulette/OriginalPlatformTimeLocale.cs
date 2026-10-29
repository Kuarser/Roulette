using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RouletteDeluxe
{
    // MachineData::GetDateTime output ABI, not a clock or financial timestamp.
    public struct OriginalMachineDateTime
    {
        public readonly ushort Year, Millisecond;
        public readonly byte Month, Day, Hour, Minute, Second;
        public OriginalMachineDateTime(ushort year, byte month, byte day,
            byte hour, byte minute, byte second, ushort millisecond)
        { Year = year; Month = month; Day = day; Hour = hour; Minute = minute; Second = second; Millisecond = millisecond; }
        public static OriginalMachineDateTime ReadPacked(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 9) throw new ArgumentException("Original date requires nine packed bytes");
            return new OriginalMachineDateTime((ushort)(bytes[0] | bytes[1] << 8), bytes[2], bytes[3],
                bytes[4], bytes[5], bytes[6], (ushort)(bytes[7] | bytes[8] << 8));
        }
    }

    // Original FormatText's %x %T for supplied LC_TIME formats only.
    // No system CultureInfo/clock/timezone substitution or general strftime.
    public sealed class OriginalPlatformTimeLocale
    {
        public string ResourceSha256 { get; private set; }
        public string DateFormat { get; private set; }
        private OriginalPlatformTimeLocale(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length < 452 || Word(bytes, 0) != 0x20031117 || Word(bytes, 4) != 111)
                throw new InvalidDataException("Original LC_TIME header differs");
            for (int i = 0; i < 111; ++i)
            {
                uint offset = Word(bytes, 8 + i * 4);
                // Shared offsets are valid in this category.
                if (offset < 452 || offset >= bytes.Length) throw new InvalidDataException("Original LC_TIME offset differs");
            }
            int at = (int)Word(bytes, 8 + 41 * 4), end = at;
            while (end < bytes.Length && bytes[end] != 0) ++end;
            if (end == bytes.Length) throw new InvalidDataException("Original D_FMT is not terminated");
            DateFormat = new UTF8Encoding(false, true).GetString(bytes, at, end - at);
            switch (DateFormat)
            {
                case "%d/%m/%Y": case "%Y/%m/%d": case "%d.%m.%Y": case "%d-%m-%Y":
                case "%m/%d/%Y": case "%Y-%m-%d": case "%Y.%m.%d": case "%d-%m-%y": break;
                default: throw new InvalidDataException("Unadmitted original D_FMT");
            }
            using (var sha = SHA256.Create()) ResourceSha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");
        }
        private static uint Word(byte[] bytes, int at)
        { return (uint)bytes[at] | (uint)bytes[at + 1] << 8 | (uint)bytes[at + 2] << 16 | (uint)bytes[at + 3] << 24; }
        public static OriginalPlatformTimeLocale Read(byte[] bytes)
        { return new OriginalPlatformTimeLocale(bytes); }
        public string Format(OriginalMachineDateTime fields)
        {
            var result = new StringBuilder();
            for (int i = 0; i < DateFormat.Length; ++i)
            {
                if (DateFormat[i] != '%') { result.Append(DateFormat[i]); continue; }
                char code = DateFormat[++i];
                if (code == 'Y') result.Append(fields.Year.ToString(CultureInfo.InvariantCulture));
                else if (code == 'y') result.Append((fields.Year % 100).ToString("D2", CultureInfo.InvariantCulture));
                else result.Append((code == 'm' ? fields.Month : fields.Day).ToString("D2", CultureInfo.InvariantCulture));
            }
            result.Append(' ');
            result.Append(fields.Hour.ToString("D2", CultureInfo.InvariantCulture)); result.Append(':');
            result.Append(fields.Minute.ToString("D2", CultureInfo.InvariantCulture)); result.Append(':');
            result.Append(fields.Second.ToString("D2", CultureInfo.InvariantCulture));
            return result.ToString();
        }
    }
}
