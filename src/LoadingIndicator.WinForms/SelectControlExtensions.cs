#if NET
using System.Windows.Forms;

namespace LoadingIndicator.WinForms
{
    internal static class SelectControlExtensions
    {

        // Try to avoid deadlock on Select method (sometimes infinite loop inside ContainerControl.UpdateFocusedControl)
        public static void SafeSelect(this Control control)
        {
            if (!control.CanSelect)
            {
                return;
            }

            // Unfortunatelly Thread handling was changed in .NET (Core)
            // The methods Thread.Abort and ResetAbort will raise a PlatformNotSupportedException on .NET 5+
            // So the logic from SelectControlExtensionsNet45 should be migrated using CancellationToken and Tasks

            control.Select();
        }
    }
}
#endif