using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LoadingIndicator.Winforms
{
    internal static class NativeMethods
    {
        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);

        public const int VerticalResolution = 10;
        public const int HorizontalResolution = 8;
        public const int DesktopVerticalResolution = 117;
        public const int DesktopHorizontalResolution = 118;
    }
}