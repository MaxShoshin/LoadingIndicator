using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using LoadingIndicator.WinForms.Logging;
using Timer = System.Threading.Timer;

namespace LoadingIndicator.WinForms
{
    internal static class SelectControlExtensions
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SelectControlExtensions), "Invoke");
        private static bool _own;

        // Try to avoid deadlock on Select method
        public static void SafeSelect(this Control control)
        {
            if (!control.CanSelect)
            {
                return;
            }

            using (new Timer(CancelSelect, Thread.CurrentThread, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan))
            {
                try
                {
                    control.Select();
                }
                catch (ThreadAbortException ex)
                {
                    if (_own)
                    {
                        Thread.ResetAbort();

                        Logger.DebugException("Deadlock in Control.Select detected. Raised ThreadAbort to exit from deadlock.", ex);
                    }
                }
                finally
                {
                    _own = false;
                }
            }
        }

        private static void CancelSelect(object state)
        {
            Thread thread = (Thread)state;

            _own = true;
            thread.Abort();
        }
    }
}