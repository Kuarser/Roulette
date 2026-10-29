using System;

namespace RouletteDeluxe
{
    // Original eventMgr+24 receives these packed bytes with dispatch argument0.
    // Event timestamps and all unassigned fields are zero at these call sites.
    public static class OriginalHandpaySelectionEvents
    {
        public const uint TiltCode = 0x6020;
        private static void Put16(byte[] bytes, int offset, ushort value)
        { bytes[offset] = (byte)value; bytes[offset + 1] = (byte)(value >> 8); }
        private static void Put32(byte[] bytes, int offset, uint value)
        { for (int i = 0; i < 4; ++i) bytes[offset + i] = (byte)(value >> (i * 8)); }
        private static void PutAmount(byte[] bytes, int offset, long amount)
        { ulong value = unchecked((ulong)amount); for (int i = 0; i < 8; ++i) bytes[offset + i] = (byte)(value >> (i * 8)); }
        public static byte[] Tilt(bool activate, long capturedSelectionAmount)
        {
            byte[] bytes = new byte[0xb9]; Put16(bytes, 0, activate ? (ushort)0x100 : (ushort)0x200);
            Put16(bytes, 2, 0xb9); Put16(bytes, 0x12, 0x20);
            Put32(bytes, 0x18, TiltCode); PutAmount(bytes, 0x1c, capturedSelectionAmount); return bytes;
        }
        public static byte[] Residual(uint managerField18, long currentBannerAmount)
        {
            byte[] bytes = new byte[0x2c]; Put16(bytes, 0, 1); Put16(bytes, 2, 0x2c);
            Put32(bytes, 0x18, managerField18); PutAmount(bytes, 0x1c, currentBannerAmount); return bytes;
        }
    }

    // Host provides the pending-tilt inventory, actual screen and event receiver.
    // Native global tiltMgr is obj+4; virtual+c queries code only, not amount.
    public sealed class OriginalHandpaySelectionLifecycle
    {
        private readonly Func<long> selectionAmount;
        private readonly Func<uint, bool> tiltPending;
        private readonly Action<byte[], uint> emit;
        private readonly Action createScreen, releaseScreen;
        private long capturedAmount;
        public OriginalHandpaySelectionLifecycle(Func<long> selectionAmount, Func<uint, bool> tiltPending,
            Action<byte[], uint> emit, Action createScreen, Action releaseScreen)
        {
            this.selectionAmount = selectionAmount ?? throw new ArgumentNullException(nameof(selectionAmount));
            this.tiltPending = tiltPending ?? throw new ArgumentNullException(nameof(tiltPending));
            this.emit = emit ?? throw new ArgumentNullException(nameof(emit));
            this.createScreen = createScreen ?? throw new ArgumentNullException(nameof(createScreen));
            this.releaseScreen = releaseScreen ?? throw new ArgumentNullException(nameof(releaseScreen));
        }
        public void Prepare()
        {
            capturedAmount = selectionAmount();
            if (!tiltPending(OriginalHandpaySelectionEvents.TiltCode)) emit(OriginalHandpaySelectionEvents.Tilt(true, capturedAmount), 0);
            createScreen();
        }
        public void Release()
        {
            releaseScreen();
            if (tiltPending(OriginalHandpaySelectionEvents.TiltCode)) emit(OriginalHandpaySelectionEvents.Tilt(false, capturedAmount), 0);
        }
    }
}
