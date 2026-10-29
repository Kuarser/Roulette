using System;
using System.Globalization;

namespace RouletteDeluxe
{
    // RouletteCycleResults::ShowWin composes the ordinary line message from
    // the win type phrase, PHRASE_PAYS ("Pays %s"), and ConvertToCash(mode 1).
    // All packaged roulette pay records have integral credit amounts.
    public static class RoulettePayMessageFormatter
    {
        public static string LinePays(WinRecord record, long amount)
        {
            if (record == null) throw new ArgumentNullException("record");
            string type;
            switch (record.Type)
            {
                case "ROULETTE_STRAIGHT": type = "Straight Up"; break;
                case "ROULETTE_SPLIT": type = "Split"; break;
                case "ROULETTE_STREET": type = "Street"; break;
                case "ROULETTE_TRIPLE": type = "Trio"; break;
                case "ROULETTE_CORNER": type = "Corner"; break;
                case "ROULETTE_FIRST_FOUR": type = "First Four"; break;
                case "ROULETTE_SIXER": type = "Six Line"; break;
                case "ROULETTE_COLUMNS": type = "Column"; break;
                case "ROULETTE_DOZENS": type = "Dozen"; break;
                case "ROULETTE_RED_BLACK": type = "Red/Black"; break;
                case "ROULETTE_LOW_HIGH": type = "Low/High"; break;
                case "ROULETTE_ODD_EVEN": type = "Even/Odd"; break;
                default: throw new FormatException("Unknown roulette win type: " + record.Type);
            }
            return type + " Pays " + amount.ToString(CultureInfo.InvariantCulture);
        }
    }
}
