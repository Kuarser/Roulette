using System;

namespace RouletteDeluxe.SasDraftV1
{
    public static class SasEffectiveMaximumWager
    {
        // Roulette::GetCurrentGameSetup (08192360) multiplies paytable max by
        // NumberOfRouletteTables, applies a nonzero StaticConfig max-bet cap,
        // and stores the result in the signed 32-bit Game+4f64 field. The PC
        // supported domain rejects overflow instead of emulating integer wrap.
        public static int Resolve(int paytableMaximum, int tableCount, long staticMaxBetLimit)
        {
            if (paytableMaximum < 1 || tableCount < 1 || staticMaxBetLimit < 0)
                throw new ArgumentOutOfRangeException("Invalid maximum wager inputs");
            int tableMaximum = checked(paytableMaximum * tableCount);
            return staticMaxBetLimit == 0 || tableMaximum <= staticMaxBetLimit ?
                tableMaximum : checked((int)staticMaxBetLimit);
        }
    }
}
