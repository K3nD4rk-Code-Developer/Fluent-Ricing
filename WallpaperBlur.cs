using System;
using System.Runtime.InteropServices;

namespace Fluent_Ricing
{
    public static class WindowBlur
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttribData data);

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttribData
        {
            public int Attrib;
            public IntPtr pvData;
            public int cbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public int AccentState;
            public int AccentFlags;
            public uint GradientColor;
            public int AnimationId;
        }

        // AccentState values
        private const int ACCENT_DISABLED = 0;
        private const int ACCENT_ENABLE_GRADIENT = 1;
        private const int ACCENT_ENABLE_TRANSPARENTGRADIENT = 2;
        private const int ACCENT_ENABLE_BLURBEHIND = 3; // classic Aero blur
        private const int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4; // Win11 acrylic
        private const int ACCENT_ENABLE_HOSTBACKDROP = 5; // Mica-like

        private const int WCA_ACCENT_POLICY = 19;

        // Call this once after the window HWND exists
        // GradientColor is AABBGGRR — controls tint
        // 0x20FFFFFF = very light white tint (mac-like frost)
        // 0x40000000 = slight dark tint
        public static void EnableBlur(IntPtr hwnd, uint gradientColor = 0x20FFFFFF)
        {
            var accent = new AccentPolicy
            {
                AccentState = ACCENT_ENABLE_ACRYLICBLURBEHIND,
                AccentFlags = 2,
                GradientColor = gradientColor,
                AnimationId = 0
            };

            int accentSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentSize);

            try
            {
                Marshal.StructureToPtr(accent, accentPtr, false);

                var data = new WindowCompositionAttribData
                {
                    Attrib = WCA_ACCENT_POLICY,
                    pvData = accentPtr,
                    cbData = accentSize
                };

                SetWindowCompositionAttribute(hwnd, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }

        public static void DisableBlur(IntPtr hwnd)
        {
            var accent = new AccentPolicy { AccentState = ACCENT_DISABLED };
            int accentSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentSize);

            try
            {
                Marshal.StructureToPtr(accent, accentPtr, false);
                var data = new WindowCompositionAttribData
                {
                    Attrib = WCA_ACCENT_POLICY,
                    pvData = accentPtr,
                    cbData = accentSize
                };
                SetWindowCompositionAttribute(hwnd, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(accentPtr);
            }
        }
    }
}