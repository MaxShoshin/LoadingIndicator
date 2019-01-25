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

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT point);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }
    }
}