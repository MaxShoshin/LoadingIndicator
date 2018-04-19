using System;
using System.Runtime.InteropServices;

namespace LoadingIndicator.WinForms
{
    internal static class NativeMethods
    {
        public const int VerticalResolution = 10;
        public const int HorizontalResolution = 8;
        public const int DesktopVerticalResolution = 117;
        public const int DesktopHorizontalResolution = 118;

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDc, int nIndex);
    }
}