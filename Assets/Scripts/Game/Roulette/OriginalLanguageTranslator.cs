using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Security.Cryptography;

namespace RouletteDeluxe
{
    // Original OS LanguageTranslator text rules. Host language/jurisdiction,
    // monetary locale and machineData date remain explicit dependencies.
    public sealed class OriginalLanguageTranslator
    {
        private readonly Dictionary<string, Dictionary<string, string>> tables;
        private static readonly Dictionary<string, string> Hashes = new Dictionary<string, string>(StringComparer.Ordinal) {
            { "LanguageInterface.xml", "F0AF033AC7B98F18FAD946332B12E459B6DACA74268883C6B672E04AEA5D9DD0" },
            { "EnglishTranslation.xml", "EE3F1A63E1C4C1B34542C08FF2329B7B8C698EA2F733E4C678EC9A50DE63B2CD" },
            { "ChineseTranslation.xml", "A0E0CB5ECC4EAEA42DA5DCB86C1681BFCA5A8891C45124DB53FC29DE04DE034C" },
            { "FrenchCanadaTranslation.xml", "038D3FFB2FF0C9EBE8474F9977FFFB52B5A901F076319D48C5421456CC512D9B" },
            { "FrenchTranslation.xml", "2A92DAC2E2DC217864811566B4D501A04AC296CB5B94DE886AAAF7D2C7BFC37B" },
            { "GreekTranslation.xml", "8CB79997CEDF21E4460F294FD869DB276641C5F2E9E490DF2323B315B8222C6F" },
            { "SpanishChileanTranslation.xml", "F8C183F248DE7BD60B75C680B21309D8ED007595B83014E4B2E2EEDD25CB7EF7" }
        };
        private OriginalLanguageTranslator(Dictionary<string, Dictionary<string, string>> loaded)
        { tables = loaded; }

        public static OriginalLanguageTranslator Load(Func<string, byte[]> readOriginalFile)
        {
            if (readOriginalFile == null) throw new ArgumentNullException("readOriginalFile");
            XmlDocument interfaces = ReadXml(readOriginalFile, "LanguageInterface.xml");
            var loaded = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            var files = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            foreach (XmlNode node in interfaces.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element || node.Name != "LanguageInterface") continue;
                string code = Attribute(node, "ID"), filename = Attribute(node, "XMLFILE");
                Dictionary<string, string> table;
                if (!files.TryGetValue(filename, out table))
                {
                    XmlDocument xml = ReadXml(readOriginalFile, filename);
                    table = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (XmlNode item in xml.DocumentElement.ChildNodes)
                    {
                        if (item.NodeType != XmlNodeType.Element || item.Name != "string") continue;
                        table[Attribute(item, "ID")] = Attribute(item, "text");
                    }
                    files.Add(filename, table);
                }
                loaded.Add(code, table);
            }
            if (loaded.Count != 9 || !loaded.ContainsKey("en"))
                throw new InvalidDataException("Original language interface differs");
            return new OriginalLanguageTranslator(loaded);
        }
        private static string Attribute(XmlNode node, string name)
        {
            XmlAttribute attribute = node.Attributes[name];
            if (attribute == null) throw new InvalidDataException("Missing original translation attribute " + name);
            return attribute.Value;
        }
        private static XmlDocument ReadXml(Func<string, byte[]> read, string filename)
        {
            string expected;
            if (!Hashes.TryGetValue(filename, out expected)) throw new InvalidDataException("Unadmitted original translation file");
            byte[] bytes = read(filename);
            if (bytes == null) throw new InvalidDataException("Missing original translation file " + filename);
            using (var sha = SHA256.Create())
                if (BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "") != expected)
                    throw new InvalidDataException("Original translation hash differs " + filename);
            var xml = new XmlDocument(); xml.XmlResolver = null;
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using (var stream = new MemoryStream(bytes))
            using (var reader = XmlReader.Create(stream, settings)) xml.Load(reader);
            if (xml.DocumentElement == null || xml.DocumentElement.Name != "TranslationTable")
                throw new InvalidDataException("Original translation table root differs");
            return xml;
        }

        public string Translate(string key, string languageCode, bool forceDefault, string[] arguments,
            Func<long, string> formatCurrency, Func<string> machineDateText, Action unavailableText)
        {
            if (key == null) throw new ArgumentNullException("key");
            Dictionary<string, string> selected;
            if (forceDefault || string.IsNullOrEmpty(languageCode) || !tables.TryGetValue(languageCode, out selected))
                selected = tables["en"];
            string template;
            bool found = selected.TryGetValue(key, out template);
            if (!found) found = tables["en"].TryGetValue(key, out template);
            string text = found ? FormatTemplate(template, arguments, formatCurrency, machineDateText) : "";
            if (text.Length != 0) return text;
            // Original gameMgr+dc return is discarded. Concrete host receiver
            // binding is not claimed by this optional callback.
            if (unavailableText != null) unavailableText();
            return key;
        }

        public static string FormatTemplate(string template, string[] arguments,
            Func<long, string> formatCurrency, Func<string> machineDateText)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (arguments.Length == 0) return template;
            string result = ""; int index = 0;
            foreach (string token in template.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string value = token;
                if (token == "FMT_Currency" || token == "FMT_Number" || token == "FMT_DateAndTime")
                {
                    // Native vector indexing lacks a safe missing-argument
                    // contract. Managed rejection is explicit, not a fallback.
                    if (token != "FMT_DateAndTime" && index >= arguments.Length)
                        throw new ArgumentException("Missing original format argument");
                    if (token == "FMT_Number") value = arguments[index];
                    else if (token == "FMT_Currency")
                    {
                        if (formatCurrency == null) throw new InvalidOperationException("Original monetary host is not bound");
                        value = formatCurrency(ParseAmount(arguments[index]));
                    }
                    else
                    {
                        if (machineDateText == null) throw new InvalidOperationException("Original machineData date host is not bound");
                        value = machineDateText();
                    }
                    ++index;
                }
                if (value == null) throw new ArgumentException("Null original format result");
                result = result.Length == 0 ? value : result + " " + value;
            }
            return result;
        }
        private static long ParseAmount(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new FormatException("Original signed64 amount is empty");
            int at = text[0] == '+' || text[0] == '-' ? 1 : 0;
            if (at == text.Length) throw new FormatException("Original signed64 amount has no digits");
            for (int i = at; i < text.Length; ++i)
                if (text[i] < '0' || text[i] > '9') throw new FormatException("Original signed64 amount is not decimal digits");
            return long.Parse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }
    }
}
