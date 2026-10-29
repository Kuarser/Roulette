using System;
using System.Text;
using System.Collections.Generic;

namespace RouletteDeluxe
{
    public static class OriginalTextScreenWrappingValidation
    {
        public static int Validate()
        {
            int checks = 0;
            Action<bool> check = value => { if (!value) throw new Exception("TextScreen wrapping regression " + checks); ++checks; };
            Func<int, byte[], short> mono = (slot, text) => (short)(text.Length * 10);
            Func<int, OriginalHandpayBannerLayout> panel = width => { var p = new OriginalHandpayBannerLayout(width + 5, 600); p.NormalizedWidth = 1; return p; };
            Func<OriginalHandpayBannerLayout, string, bool> add = (p, text) => OriginalTextScreenWrapping.AddText(p, Encoding.ASCII.GetBytes(text), 3, 20, 0, false, mono);
            Func<OriginalHandpayBannerLayout, int, string> line = (p, index) => Encoding.ASCII.GetString(p.Lines[index].Text);
            var p = panel(40); check(add(p, "abcdef"));
            check(p.Lines.Count == 2 && line(p, 0) == "abc-" && line(p, 1) == "def-"); // equality wraps
            check(p.Lines[0].BitmapFontPresent && p.Lines[0].FontSlot == 3 && p.Lines[0].Spacing == 6);
            p = panel(41); check(add(p, "abcdef")); check(line(p, 0) == "abcd-" && line(p, 1) == "ef");
            p = panel(50); check(add(p, "ab cde")); check(p.Lines.Count == 2 && line(p, 0) == "ab " && line(p, 1) == "cde");
            p = panel(40); check(add(p, " abc")); check(line(p, 0) == " ab-" && line(p, 1) == "c"); // index zero sentinel
            p = panel(40); check(add(p, "ab  cd")); check(p.Lines.Count == 3 && line(p, 0) == "ab " && line(p, 1) == " " && line(p, 2) == "cd");
            p = panel(100); check(!add(p, "")); check(p.Lines.Count == 0);
            p = panel(100); check(add(p, "a\0discard")); check(line(p, 0) == "a");
            p = panel(100); check(add(p, "a\nb")); check(line(p, 0) == "a\nb"); // LF is a measured byte
            p = panel(40); var calls = new List<string>();
            check(OriginalTextScreenWrapping.AddText(p, Encoding.ASCII.GetBytes("ab c"), 2, 12, 0, false,
                (slot, bytes) => { check(slot == 2); calls.Add(Encoding.ASCII.GetString(bytes)); return (short)(bytes.Length * 10); }));
            check(string.Join("|", calls) == "W|a|ab|ab |c"); check(line(p, 0) == "ab " && line(p, 1) == "c");
            p = panel(34);
            check(OriginalTextScreenWrapping.AddText(p, Encoding.ASCII.GetBytes("ab"), 0, 12, 0, true,
                (slot, bytes) => Encoding.ASCII.GetString(bytes) == "W" ? (short)17 : (short)(bytes.Length == 2 ? 15 : 10)));
            check(p.Lines.Count == 1 && line(p, 0) == "ab"); // whole-prefix measure, not sum of glyph widths
            p = panel(40); check(OriginalTextScreenWrapping.AddText(p, Encoding.ASCII.GetBytes("abcdef"), 7, 48, 1, false, null));
            check(p.Lines.Count == 1 && line(p, 0) == "abcdef" && !p.Lines[0].BitmapFontPresent);
            p = panel(40); check(OriginalTextScreenWrapping.AddText(p, new byte[0], 8, 96, 2, false, null)); check(p.Lines.Count == 1);
            p = panel(40); check(OriginalTextScreenWrapping.AddText(p, Encoding.ASCII.GetBytes("long"), 3, 20, 0, true, (slot, bytes) => -1)); check(line(p, 0) == "long");
            p = panel(1); check(!add(p, new string('a', 40))); check(p.Lines.Count == 32 && line(p, 31) == "a-");
            check(p.CurrentFontHeight == 20 && p.CurrentSpacing == 6);
            p = panel(3000); check(add(p, new string('a', 200))); check(p.Lines.Count == 1 && p.Lines[0].Text.Length == 127);
            return checks;
        }
    }
}
