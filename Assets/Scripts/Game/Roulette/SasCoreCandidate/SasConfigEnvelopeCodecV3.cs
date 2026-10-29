using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RouletteDeluxe.SasDraftV1
{
    // Pure codec draft. It does not read storage, replay events or publish config.
    public sealed class SasConfigEnvelopeV1
    {
        public readonly string Scope, LastDurableEventSource, LastDurableEventId, LastDurableEventSha256;
        public readonly long Generation, LastDurableEventSequence;
        public readonly SasPayload Payload;

        public SasConfigEnvelopeV1(string scope, long generation, SasPayload payload,
            string source, string eventId, long sequence, string eventSha256)
        {
            if (string.IsNullOrWhiteSpace(scope) || generation < 0 || payload == null ||
                string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(eventId) ||
                sequence < 0 || !SasConfigEnvelopeCodecV3.IsDigest(eventSha256))
                throw new ArgumentException("Invalid config envelope identity or event provenance");
            Scope = scope; Generation = generation; Payload = payload;
            LastDurableEventSource = source; LastDurableEventId = eventId;
            LastDurableEventSequence = sequence; LastDurableEventSha256 = eventSha256;
        }
    }

    public static class SasConfigEnvelopeCodecV3
    {
        private static readonly string[] Fields = { "version", "scope", "generation", "raw32Base64",
            "raw32Sha256", "lastDurableEventSource", "lastDurableEventId",
            "lastDurableEventSequence", "lastDurableEventSha256" };
        internal static bool IsDigest(string value)
        { return value != null && Regex.IsMatch(value, "\\A[0-9A-F]{64}\\z"); }
        private static string Hash(byte[] raw)
        {
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(raw)).Replace("-", "");
        }
        private static string Quote(string value)
        {
            var result = new StringBuilder("\"");
            foreach (char c in value)
            {
                if (c == '"' || c == '\\') result.Append('\\').Append(c);
                else if (c < 32 || char.IsSurrogate(c))
                    result.Append("\\u").Append(((int)c).ToString("X4", CultureInfo.InvariantCulture));
                else result.Append(c);
            }
            return result.Append('"').ToString();
        }
        public static string Encode(SasConfigEnvelopeV1 envelope)
        {
            if (envelope == null) throw new ArgumentNullException("envelope");
            byte[] raw = envelope.Payload.CopyBytes();
            var fields = new Dictionary<string, object>(StringComparer.Ordinal) {
                { "version", 1 }, { "scope", envelope.Scope }, { "generation", envelope.Generation },
                { "raw32Base64", Convert.ToBase64String(raw) }, { "raw32Sha256", Hash(raw) },
                { "lastDurableEventSource", envelope.LastDurableEventSource },
                { "lastDurableEventId", envelope.LastDurableEventId },
                { "lastDurableEventSequence", envelope.LastDurableEventSequence },
                { "lastDurableEventSha256", envelope.LastDurableEventSha256 } };
            return "{" + string.Join(",", Fields.Select(name => Quote(name) + ":" +
                (fields[name] is string ? Quote((string)fields[name]) :
                 Convert.ToString(fields[name], CultureInfo.InvariantCulture))).ToArray()) + "}";
        }
        public static SasConfigEnvelopeV1 Decode(string encoded, string expectedScope)
        {
            if (string.IsNullOrWhiteSpace(expectedScope)) throw new ArgumentException("Expected scope required");
            var reader = new FlatReader(encoded);
            Dictionary<string, string> values = reader.Read();
            if (values.Count != Fields.Length || Fields.Any(name => !values.ContainsKey(name)))
                throw new ArgumentException("Config envelope fields differ from version 1");
            if (reader.Number(values["version"]) != 1) throw new ArgumentException("Unsupported config version");
            string scope = reader.String(values["scope"]);
            if (scope != expectedScope) throw new ArgumentException("Config scope mismatch");
            string base64 = reader.String(values["raw32Base64"]);
            byte[] raw = Convert.FromBase64String(base64);
            if (Convert.ToBase64String(raw) != base64 || raw.Length != 32 ||
                reader.String(values["raw32Sha256"]) != Hash(raw))
                throw new ArgumentException("Noncanonical config payload or hash mismatch");
            return new SasConfigEnvelopeV1(scope, reader.Number(values["generation"]), new SasPayload(raw),
                reader.String(values["lastDurableEventSource"]), reader.String(values["lastDurableEventId"]),
                reader.Number(values["lastDurableEventSequence"]), reader.String(values["lastDurableEventSha256"]));
        }
        public static SasConfigTransition ColdLoad(string encoded, string expectedScope)
        {
            // Null denotes missing storage only. Empty/malformed saved values fault.
            if (encoded == null) return SasConfigRules.ColdLoad(new byte[32]);
            return SasConfigRules.ColdLoad(Decode(encoded, expectedScope).Payload.CopyBytes());
        }

        // Flat exact-schema grammar. Decode names before duplicate checks, including
        // escaped spellings. JSON deserialization alone silently accepts duplicates.
        private sealed class FlatReader
        {
            private readonly string text;
            private int cursor;
            internal FlatReader(string text)
            {
                if (string.IsNullOrEmpty(text) || text.Length > 65536) throw new ArgumentException("Invalid config JSON length");
                this.text = text;
            }
            private void White() { while (cursor < text.Length && " \r\n\t".IndexOf(text[cursor]) >= 0) ++cursor; }
            private bool Take(char value) { White(); if (cursor < text.Length && text[cursor] == value) { ++cursor; return true; } return false; }
            private string Token()
            {
                White(); int start = cursor;
                if (cursor == text.Length) throw new ArgumentException("Missing JSON token");
                if (text[cursor] == '"')
                {
                    ++cursor; bool closed = false;
                    while (cursor < text.Length)
                    {
                        char c = text[cursor++];
                        if (c == '"') { closed = true; break; }
                        if (c < 32) throw new ArgumentException("JSON string control character");
                        if (c == '\\')
                        {
                            if (cursor == text.Length) throw new ArgumentException("Truncated escape");
                            char escape = text[cursor++];
                            if (escape == 'u')
                            {
                                for (int i = 0; i < 4; ++i)
                                    if (cursor == text.Length || !Uri.IsHexDigit(text[cursor++])) throw new ArgumentException("Invalid Unicode escape");
                            }
                            else if ("\"\\/bfnrt".IndexOf(escape) < 0) throw new ArgumentException("Invalid JSON escape");
                        }
                    }
                    if (!closed) throw new ArgumentException("Unclosed JSON string");
                }
                else while (cursor < text.Length && ",} \r\n\t".IndexOf(text[cursor]) < 0) ++cursor;
                if (cursor == start) throw new ArgumentException("Empty JSON token");
                return text.Substring(start, cursor - start);
            }
            internal string String(string token)
            {
                if (token.Length < 2 || token[0] != '"') throw new ArgumentException("Expected JSON string");
                var result = new StringBuilder();
                for (int i = 1; i < token.Length - 1; ++i)
                {
                    char c = token[i];
                    if (c != '\\') { result.Append(c); continue; }
                    char escape = token[++i];
                    switch (escape)
                    {
                        case '"': case '\\': case '/': result.Append(escape); break;
                        case 'b': result.Append('\b'); break;
                        case 'f': result.Append('\f'); break;
                        case 'n': result.Append('\n'); break;
                        case 'r': result.Append('\r'); break;
                        case 't': result.Append('\t'); break;
                        case 'u': result.Append((char)int.Parse(token.Substring(i + 1, 4),
                            NumberStyles.HexNumber, CultureInfo.InvariantCulture)); i += 4; break;
                        default: throw new ArgumentException("Invalid string escape");
                    }
                }
                return result.ToString();
            }
            internal long Number(string token)
            {
                long result;
                if (!Regex.IsMatch(token, "\\A(?:0|[1-9][0-9]*)\\z") ||
                    !long.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out result))
                    throw new ArgumentException("Expected canonical nonnegative signed64 integer");
                return result;
            }
            internal Dictionary<string, string> Read()
            {
                if (!Take('{')) throw new ArgumentException("Config JSON root is not object");
                var result = new Dictionary<string, string>(StringComparer.Ordinal);
                if (!Take('}'))
                {
                    do
                    {
                        string name = String(Token());
                        if (!Take(':') || result.ContainsKey(name)) throw new ArgumentException("Missing colon or duplicate field");
                        result.Add(name, Token());
                        if (Take('}')) break;
                        if (!Take(',')) throw new ArgumentException("Missing object delimiter");
                    } while (true);
                }
                White(); if (cursor != text.Length) throw new ArgumentException("Trailing JSON content");
                return result;
            }
        }
    }
}
