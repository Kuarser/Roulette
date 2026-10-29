using System.Globalization;

namespace RouletteDeluxe
{
    // The filmed cabinet uses 1-cent denominations and RM currency meters.
    // ProView calls GetFormattedLocalCurrency through its panel vtable.
    public static class RouletteDenominationFormatter
    {
        private const string FilmedCurrencyPrefix = "RM";

        public static string ForFilmedCabinet(long minorUnits)
        {
            if (minorUnits < 100)
                return minorUnits.ToString(CultureInfo.InvariantCulture) + "¢";
            long whole = minorUnits / 100;
            long fraction = minorUnits % 100;
            return fraction == 0
                ? FilmedCurrencyPrefix + whole.ToString(CultureInfo.InvariantCulture)
                : FilmedCurrencyPrefix + whole.ToString(CultureInfo.InvariantCulture) + "." +
                  fraction.ToString("D2", CultureInfo.InvariantCulture);
        }

        public static string ForFeatureCurrency(long minorUnits)
        {
            // Explicit PC virtual currency. Production locale is supplied by
            // platform CFGPARAM_LOCALE and is not inferred from the AU flag.
            return OriginalMonetaryLocales.Load(OriginalMonetaryLocales.PcVirtualLocaleName).FormatMinorUnits(minorUnits);
        }
    }
}
