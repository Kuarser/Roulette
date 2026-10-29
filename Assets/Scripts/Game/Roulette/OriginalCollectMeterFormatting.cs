using System;
using System.Globalization;

namespace RouletteDeluxe
{
    // Game mode 1 and the distinct ProView numerical collect meter entry.
    // Formats presentation only; it neither owns money nor admits a receipt.
    public static class OriginalCollectMeterFormatting
    {
        public static string Mode1(double credits, bool displayFlag)
        { return Mode1(OriginalBinary80.FromDouble(credits), displayFlag); }
        internal static string Mode1(OriginalBinary80 credits, bool displayFlag)
        {
            long whole = credits.TruncateInt64();
            if (OriginalBinary80.Compare(credits, OriginalBinary80.Integer64(whole)) <= 0)
                return whole.ToString(CultureInfo.InvariantCulture);
            if (!displayFlag) return credits.FormatFixed(2);
            // ConvertAmtToString first rounds to ten places, then cuts at
            // dot+4 (three places) before stripping trailing zero and dot.
            string value = credits.FormatFixed(10);
            int point = value.IndexOf('.');
            value = value.Substring(0, point + 4);
            return value.TrimEnd('0').TrimEnd('.');
        }
        public static string PanelCredits(double credits)
        {
            var amount = OriginalBinary80.FromDouble(credits);
            return OriginalBinary80.Compare(amount, OriginalBinary80.FromDouble(0.001)) < 0 ? " " : Mode1(amount, true);
        }
        public static string PanelCash(double credits, long denomination, OriginalMonetaryLocale locale)
        {
            if (locale == null) throw new ArgumentNullException("locale");
            if (denomination <= 0) throw new ArgumentOutOfRangeException("denomination");
            var amount = OriginalBinary80.FromDouble(credits);
            return OriginalBinary80.Compare(amount, OriginalBinary80.FromDouble(0.001)) < 0 ? " " :
                locale.FormatPaymentCredits(amount, denomination);
        }
    }
}
