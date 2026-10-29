using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalTimeLocales
    {
        public const string ManifestSha256 = "DFC121D800B159538974D9970E295D6F2217DA84446B5779CCF3642614F05E2D";
        [Serializable] private sealed class Entry { public string name, sha256; }
        [Serializable] private sealed class Catalog { public int schema; public string source, category; public Entry[] entries; }
        private static Dictionary<string, string> hashes;
        private static readonly Dictionary<string, OriginalPlatformTimeLocale> loaded = new Dictionary<string, OriginalPlatformTimeLocale>(StringComparer.Ordinal);
        private static string Hash(byte[] bytes)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", ""); }
        private static void ReadCatalog()
        {
            if (hashes != null) return;
            var asset = Resources.Load<TextAsset>("Original/TimeLocale/manifest");
            if (asset == null || Hash(asset.bytes) != ManifestSha256) throw new InvalidDataException("Original time catalog differs");
            var catalog = JsonUtility.FromJson<Catalog>(asset.text);
            if (catalog == null || catalog.schema != 1 || catalog.source != "BAL_OS366J/usr/lib/locale" ||
                catalog.category != "LC_TIME" || catalog.entries == null || catalog.entries.Length != 136)
                throw new InvalidDataException("Original time catalog shape differs");
            var next = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in catalog.entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.name) || !Regex.IsMatch(entry.name, "^[A-Za-z0-9_.@]+$") ||
                    string.IsNullOrEmpty(entry.sha256) || !Regex.IsMatch(entry.sha256, "^[0-9A-F]{64}$") || next.ContainsKey(entry.name))
                    throw new InvalidDataException("Original time catalog entry differs");
                next.Add(entry.name, entry.sha256);
            }
            hashes = next;
        }
        public static string[] Names()
        { ReadCatalog(); var names = new List<string>(hashes.Keys); names.Sort(StringComparer.Ordinal); return names.ToArray(); }
        public static OriginalPlatformTimeLocale Load(string localeName)
        {
            ReadCatalog(); string expected;
            if (localeName == null || !hashes.TryGetValue(localeName, out expected)) throw new InvalidDataException("Locale has no admitted original time resource");
            OriginalPlatformTimeLocale value;
            if (loaded.TryGetValue(localeName, out value)) return value;
            var asset = Resources.Load<TextAsset>("Original/TimeLocale/" + localeName + "/LC_TIME");
            if (asset == null || Hash(asset.bytes) != expected) throw new InvalidDataException("Original LC_TIME hash differs: " + localeName);
            value = OriginalPlatformTimeLocale.Read(asset.bytes); loaded.Add(localeName, value); return value;
        }
    }
}
