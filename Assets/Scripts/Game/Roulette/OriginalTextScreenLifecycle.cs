using System;

namespace RouletteDeluxe
{
    // Native TextScreen visibility byte is separate from root ownership.
    public sealed class OriginalTextScreenLifecycle
    {
        public sealed class Host
        {
            public Action CreateDisplaySprites, ClearSprites, ReleaseRoot, SetDefaults;
            public Action<bool> SetRootVisible;
            public Action<byte[], uint> Emit;
            internal void Validate()
            {
                if (CreateDisplaySprites == null || ClearSprites == null || ReleaseRoot == null || SetDefaults == null ||
                    SetRootVisible == null || Emit == null) throw new ArgumentException("Native lifecycle operations required");
            }
        }
        private readonly Host host;
        public bool Visible { get; private set; }
        public bool RootPresent { get; private set; }
        public OriginalTextScreenLifecycle(Host boundHost)
        { host = boundHost ?? throw new ArgumentNullException(nameof(boundHost)); host.Validate(); }
        public static bool UsingBMFonts(int fontMode) { return fontMode == 2; }
        private void Emit(uint code)
        {
            var raw = new byte[44]; raw[0] = 1; raw[2] = 44;
            for (int i = 0; i < 4; ++i) raw[24 + i] = (byte)(code >> (8 * i));
            host.Emit(raw, 0);
        }
        public bool Display()
        {
            host.CreateDisplaySprites(); RootPresent = true;
            Visible = true; Emit(0x18100); return true;
        }
        public bool Show()
        {
            if (RootPresent) host.SetRootVisible(true);
            if (!Visible) { Visible = true; Emit(0x18100); }
            return true;
        }
        public bool Hide()
        {
            if (RootPresent) host.SetRootVisible(false);
            if (Visible) { Visible = false; Emit(0x18200); }
            return true;
        }
        public bool Clear() { host.ClearSprites(); return true; }
        public bool End()
        {
            if (Visible) { Visible = false; Emit(0x18200); }
            Clear(); RootPresent = false; host.ReleaseRoot(); host.SetDefaults(); return true;
        }
        public void Pause(bool paused) { } // Native empty function.
    }
}
