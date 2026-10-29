using System;
using System.Text;

namespace RouletteDeluxe
{
    public static class OriginalVerifyTextLayoutValidation
    {
        private static int checks;
        private static void Require(bool value)
        { if (!value) throw new Exception("Original VerifyScreen byte layout regression " + checks); ++checks; }
        private static byte[] Bytes(string value) { return Encoding.ASCII.GetBytes(value); }
        private static string Text(OriginalVerifyTextLayout.Line line) { return Encoding.ASCII.GetString(line.Bytes); }
        private static short Width(byte[] value) { return (short)(value.Length * 10); }
        private static string LowerAscii(string value)
        {
            char[] result = value.ToCharArray();
            for (int i = 0; i < result.Length; ++i)
                if (result[i] >= 'A' && result[i] <= 'Z') result[i] = (char)(result[i] + 32);
            return new string(result);
        }
        public static int Validate()
        {
            checks = 0;
            Require(OriginalVerifyFontSelection.TextResource("CHINESE", 0, LowerAscii) == "OS366J/simsun-14-white-uni");
            Require(OriginalVerifyFontSelection.TextResource("zh", 2, LowerAscii) == "OS366J/simsun-14-white-uni");
            Require(OriginalVerifyFontSelection.TextResource("zh-CN", 0, LowerAscii) == "OS366J/arial-bold-20-white");
            Require(OriginalVerifyFontSelection.ButtonResource("en", LowerAscii) == "OS366J/arial-bold-8-black");
            Require(OriginalVerifyFontSelection.ButtonResource("ZH", LowerAscii) == "OS366J/simsun-10-black-uni");
            Require(OriginalVerifyFontSelection.TextResource("CHINESE", 0, delegate(string v) { return "ch\u0131nese"; }) == "OS366J/arial-bold-20-white");
            var layout = new OriginalVerifyTextLayout(0, 100, 0.6f); // effective width40
            Require(layout.EffectiveWidth == 40);
            Require(layout.AddText(Bytes("abc"), 0, 20, Width) && Text(layout[0]) == "abc");
            Require(layout.AddText(Bytes("ab cd"), 1, 16, Width) && Text(layout[1]) == "ab" && Text(layout[2]) == " cd");
            var positions = layout.Align(5, 0, -1);
            Require(positions[0].X == 30 && positions[0].Y == 17 && positions[1].Y == 42 && positions[2].Y == 63);
            Require(positions[2].VerticalAlignment == -1 && layout[2].FontSlot == 1);
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(Bytes("abc def"), 0, 20, Width) && Text(layout[0]) == "abc " && Text(layout[1]) == "def");
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(Bytes("abcdef"), 0, 20, Width) && Text(layout[0]) == "abcd" && Text(layout[1]) == "ef");
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            int calls = 0;
            Require(layout.AddText(Bytes(new string('x', 128)), 0, 20, delegate(byte[] v) { ++calls; return Width(v); }) &&
                layout.Count == 1 && layout[0].Bytes.Length == 127 && calls == 0);
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(new byte[] { 65, 0, 66 }, 0, 20, Width) && Text(layout[0]) == "A");
            layout = new OriginalVerifyTextLayout(0, 100, 0.1f);
            Require(layout.AddText(Bytes("A"), 0, 20, Width) && layout.Count == 32 && layout[31].Bytes.Length == 0);
            Require(!layout.AddText(Bytes("B"), 1, 16, Width) && layout.LastFontHeight == 16 && layout.LastSpacing == 5);
            layout.Align(5, 0, 0);
            Require(layout.LastFontHeight == 20 && layout.LastSpacing == 5);
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(Bytes("ab"), 0, 20, delegate(byte[] v) { return (short)-1; }) && Text(layout[0]) == "ab");
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(Bytes("abc\ndef"), 0, 20, Width) && Text(layout[0]) == "abc\n");
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(new byte[0], 0, 20, Width) && layout.Count == 1 && layout[0].Bytes.Length == 0);
            layout = new OriginalVerifyTextLayout(0, 100, 0.6f);
            Require(layout.AddText(new byte[] { 0xe4, 0xb8, 0xad, 0xe6, 0x96, 0x87 }, 0, 20,
                delegate(byte[] v) { return v.Length == 1 ? (short)0 : (short)60; }) && layout.Count == 1 && layout[0].Bytes.Length == 6);
            return checks;
        }
    }
}
