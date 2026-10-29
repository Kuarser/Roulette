using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalMonetaryLocales
    {
        public const string PcVirtualLocaleName = "ms_MY.utf8";
        public const string ManifestSha256 = "0378BC8E3632B3F590E3AA1BFF4DCBD56E310F08055FC6B1B101E36464A3815D";
        [Serializable] private sealed class Entry { public string name, sha256; }
        [Serializable] private sealed class Catalog { public int schema; public string source; public Entry[] entries; }
        private static Dictionary<string, string> hashes;
        private static readonly Dictionary<string, OriginalMonetaryLocale> loaded = new Dictionary<string, OriginalMonetaryLocale>(StringComparer.Ordinal);
        private static readonly Dictionary<string, OriginalPlatformMonetaryLocale> platformLoaded = new Dictionary<string, OriginalPlatformMonetaryLocale>(StringComparer.Ordinal);
        private static void ReadCatalog()
        {
            if (hashes != null) return;
            var asset = Resources.Load<TextAsset>("Original/Locale/manifest");
            if (asset == null || Hash(asset.bytes) != ManifestSha256) throw new InvalidDataException("Original monetary catalog differs from admitted IMG export");
            var catalog = JsonUtility.FromJson<Catalog>(asset.text);
            if (catalog == null || catalog.schema != 1 || catalog.source != "BAL_OS366J/usr/lib/locale" || catalog.entries == null || catalog.entries.Length != 136)
                throw new InvalidDataException("Original monetary catalog shape is invalid");
            var next = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var e in catalog.entries)
            {
                if (e == null || string.IsNullOrEmpty(e.name) || !Regex.IsMatch(e.name, "^[A-Za-z0-9_.@]+$") ||
                    string.IsNullOrEmpty(e.sha256) || !Regex.IsMatch(e.sha256, "^[0-9A-F]{64}$") || next.ContainsKey(e.name))
                    throw new InvalidDataException("Original monetary catalog entry is invalid");
                next.Add(e.name, e.sha256);
            }
            hashes = next;
        }
        private static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", ""); }
        public static string[] Names()
        { ReadCatalog(); var names = new List<string>(hashes.Keys); names.Sort(StringComparer.Ordinal); return names.ToArray(); }
        public static OriginalMonetaryLocale Load(string localeName)
        {
            ReadCatalog(); string expected;
            if (localeName == null || !hashes.TryGetValue(localeName, out expected)) throw new InvalidDataException("Locale has no admitted original monetary resource");
            OriginalMonetaryLocale value;
            if (loaded.TryGetValue(localeName, out value)) return value;
            var asset = Resources.Load<TextAsset>("Original/Locale/" + localeName + "/LC_MONETARY");
            if (asset == null || Hash(asset.bytes) != expected) throw new InvalidDataException("Original monetary resource differs from catalog: " + localeName);
            value = OriginalMonetaryLocale.Read(asset.bytes); loaded.Add(localeName, value); return value;
        }
        public static OriginalPlatformMonetaryLocale LoadPlatform(string localeName)
        {
            ReadCatalog(); string expected;
            if (localeName == null || !hashes.TryGetValue(localeName, out expected)) throw new InvalidDataException("Locale has no admitted original monetary resource");
            OriginalPlatformMonetaryLocale value;
            if (platformLoaded.TryGetValue(localeName, out value)) return value;
            var asset = Resources.Load<TextAsset>("Original/Locale/" + localeName + "/LC_MONETARY");
            if (asset == null || Hash(asset.bytes) != expected) throw new InvalidDataException("Original monetary resource differs from catalog: " + localeName);
            value = OriginalPlatformMonetaryLocale.Read(asset.bytes); platformLoaded.Add(localeName, value); return value;
        }
    }
}
