namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireSasStartupDisplayResourcesV4(RouletteDeluxe.SasDraftV1.SasDisplayStateV1 display)
        {
            if(display==null||display.hideCompleted)return;
            // A cold process has no existing shared JackpotBox window. Saved
            // shown=true is a reconstruction obligation, not the native early return.
            var selected=OriginalSasBoxLevels.LoadPackagedSnapshot().SelectMultiplierDisplay(false);
            // Resource lookup must succeed before reconstruction can be admitted.
            // The supplied image has no Type4; neither renderer path is implemented.
            throw new PcRoundLifecycleDataException(selected.UsesNativeDefaultPath
                ? "Native SAS default JackpotBox rendering has not been installed"
                : "Native SAS Type4 rendering has not been installed");
        }
    }
}
