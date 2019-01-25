using System.Drawing;
using System.Windows.Forms;

namespace LoadingIndicator.WinForms
{
    internal static class ControlVisibilityExtensions
    {
        public static bool IsControlVisibleToUser(this Control control)
        {
            // Note about different scale (so every control point transfered to screen coordinates)
            var pointsToCheck = new NativeMethods.POINT[]
            {
                // Corners
                control.PointToScreen(Point.Empty),
                control.PointToScreen(new Point(control.Width - 1, 0)),
                control.PointToScreen(new Point(0, control.Height - 1)),
                control.PointToScreen(new Point(control.Width - 1, control.Height - 1)),

                // Center
                control.PointToScreen(new Point(control.Width / 2, control.Height / 2))
            };

            foreach (var p in pointsToCheck)
            {
                var hwnd = NativeMethods.WindowFromPoint(p);
                var other = Control.FromChildHandle(hwnd);

                var mousePoint = Control.MousePosition;
                
                if (other == null)
                    continue;

                if (control == other || control.Contains(other))
                {
                    return true;
                }
            }

            return false;
        }
    }
}