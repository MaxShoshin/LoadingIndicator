#if NETFRAMEWORK
using LoadingIndicator.WinForms.Logging;
using System;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Threading.Timer;

namespace LoadingIndicator.WinForms
{
    internal static class SelectControlExtensionsNet45
    {
        private const string AbortReason = "Deadlock in Control.Select detected. Raised ThreadAbort to exit from deadlock.";

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SelectControlExtensionsNet45), "Invoke");

        // Try to avoid deadlock on Select method (sometimes infinite loop inside ContainerControl.UpdateFocusedControl)
        public static void SafeSelect(this Control control)
        {
            if (!control.CanSelect)
            {
                return;
            }

            // using Dispose to call ResetAbort always, if it was requested, even if no exception is thrown yet
            using (var threadWrapper = new ThreadWrapper(Thread.CurrentThread))
            {
                try
                {
                    using (CreateTimer(threadWrapper))
                    {
                        control.Select();
                    }
                }
                catch (ThreadAbortException ex)
                {
                    threadWrapper.ResetAbort();

                    Logger.WarnException(AbortReason, ex);
                }
            }
        }

        private static IDisposable CreateTimer(ThreadWrapper threadWrapper)
        {
            if (!LongOperationSettings.DetectDeadlocks) return null;

            return new Timer(CancelSelect, threadWrapper, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        private static void CancelSelect(object state)
        {
            var threadWrapper = (ThreadWrapper)state;

            threadWrapper.AbortThread();
        }

        private class ThreadWrapper : IDisposable
        {
            private const int InitialValue = 0;
            private const int AbortRequested = 1;
            private const int AbortNotAllowed = -1;

            private readonly Thread _thread;
            private int _flag;

            public ThreadWrapper(Thread thread)
            {
                _thread = thread;
            }

            public void AbortThread()
            {
                if (Interlocked.CompareExchange(ref _flag, AbortRequested, InitialValue) == InitialValue)
                {
                    _thread.Abort(AbortReason);
                }
            }

            public void ResetAbort()
            {
                if (Thread.CurrentThread != _thread) return;

                if (Interlocked.Exchange(ref _flag, AbortNotAllowed) == AbortRequested)
                {
                    // we should wait other thread requesting Thread.Abort() before we can call Thread.ResetAbort()
                    // when it called from Dispose method (inside finally block) thread is marked as `AbortRequested`
                    // but `ThreadAbortException` is not raised inside finally block
                    SpinWait.SpinUntil(() => _thread.ThreadState == ThreadState.AbortRequested);

                    Thread.ResetAbort();
                }
            }

            public void Dispose()
            {
                ResetAbort();
            }
        }
    }
}
#endif