using System;
using System.IO;

namespace RouletteDeluxe.Validation
{
    public static class OriginalLanguageTranslatorValidation
    {
        private static void Require(bool ok, string message)
        { if (!ok) throw new Exception("Original translation validation: " + message); }
        public static int Validate(Func<string, byte[]> read, OriginalPlatformMonetaryLocale rm)
        {
            var translator = OriginalLanguageTranslator.Load(read);
            Func<long, string> currency = amount => rm.FormatText(amount, 100, false, 0);
            string[] empty = new string[0]; int checks = 0;
            foreach (string code in new[] { "en", "fr", "fr_FR", "fr_CA", "zh", "unknown", "" })
            {
                Require(translator.Translate("Hand_Pay", code, false, empty, currency, null, null) == "Hand Pay", "default handpay " + code); ++checks;
                Require(translator.Translate("cannot_be_paid_from_the_hopper", code, false, new[] { "995" }, currency, null, null) == "RM9.95 cannot be paid from the hopper.", "currency default " + code); ++checks;
            }
            Require(translator.Translate("cannot_be_paid_from_the_hopper", "en", false, empty, null, null, null) == "FMT_Currency cannot be paid from the hopper.", "empty vector keeps template"); ++checks;
            Require(translator.Translate("cannot_be_paid_from_the_hopper", "en", false, new[] { long.MaxValue.ToString() }, currency, null, null) == "RM92,233,720,368,547,758.07 cannot be paid from the hopper.", "integer currency injection"); ++checks;
            Require(translator.Translate("Hand_Pay", "es_CL", true, empty, null, null, null) == "Hand Pay", "resolved force default"); ++checks;
            int callbacks = 0;
            Require(translator.Translate("absent_original_key", "en", false, empty, null, null, () => ++callbacks) == "absent_original_key" && callbacks == 1, "key fallback callback"); ++checks;
            Require(OriginalLanguageTranslator.FormatTemplate("  FMT_Number   FMT_Currency  ", new[] { "001.230", "-995", "extra" }, currency, null) == "001.230 (RM9.95)", "token consumption and spaces"); ++checks;
            Require(OriginalLanguageTranslator.FormatTemplate("FMT_Currency, FMT_Number.", new[] { "ignored" }, null, null) == "FMT_Currency, FMT_Number.", "exact tokens only"); ++checks;
            Require(OriginalLanguageTranslator.FormatTemplate("  line\tbreak  stays  ", new[] { "ignored" }, null, null) == "line\tbreak stays", "literal space delimiter"); ++checks;
            Require(OriginalLanguageTranslator.FormatTemplate("FMT_Number x FMT_Number y", new[] { "", "" }, null, null) == "x  y", "empty replacements follow accumulator"); ++checks;
            int dates = 0;
            Require(OriginalLanguageTranslator.FormatTemplate("FMT_DateAndTime FMT_DateAndTime", new[] { "not-a-date" }, null, () => "date" + (++dates)) == "date1 date2" && dates == 2, "machineData dates ignore supplied value"); ++checks;
            Require(OriginalLanguageTranslator.FormatTemplate("FMT_DateAndTime FMT_Number", new[] { "unused", "second" }, null, () => "date") == "date second", "date advances argument index"); ++checks;
            foreach (string bad in new[] { "", " ", "995 ", " 995", "9.95", "+", "9223372036854775808", "1\0", "１２" })
            {
                bool rejected = false;
                try { OriginalLanguageTranslator.FormatTemplate("FMT_Currency", new[] { bad }, currency, null); }
                catch (FormatException) { rejected = true; } catch (OverflowException) { rejected = true; }
                Require(rejected, "managed malformed amount rejection " + bad); ++checks;
            }
            bool missing = false;
            try { OriginalLanguageTranslator.FormatTemplate("FMT_Number FMT_Number", new[] { "one" }, null, null); }
            catch (ArgumentException) { missing = true; }
            Require(missing, "managed missing argument guard"); ++checks;
            bool noHost = false;
            try { OriginalLanguageTranslator.FormatTemplate("FMT_DateAndTime", new[] { "unused" }, null, null); }
            catch (InvalidOperationException) { noHost = true; }
            Require(noHost, "date has no invented host"); ++checks;
            bool altered = false;
            try { OriginalLanguageTranslator.Load(name => { byte[] bytes = (byte[])read(name).Clone(); bytes[0] ^= 1; return bytes; }); }
            catch (InvalidDataException) { altered = true; }
            Require(altered, "translation hash admission"); ++checks;
            return checks;
        }
    }
}
