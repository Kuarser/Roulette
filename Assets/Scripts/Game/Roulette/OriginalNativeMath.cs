using System;
using System.Runtime.InteropServices;

namespace RouletteDeluxe
{
    public static class OriginalNativeMath
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("OriginalX87Math", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OriginalSinCos(double radians, out double sine, out double cosine);
#endif
        public static void SinCos(double radians, out double sine, out double cosine)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            OriginalSinCos(radians, out sine, out cosine);
#else
            // Android's source instruction backend is a separate acceptance boundary.
            sine = Math.Sin(radians);
            cosine = Math.Cos(radians);
#endif
        }
    }
}
