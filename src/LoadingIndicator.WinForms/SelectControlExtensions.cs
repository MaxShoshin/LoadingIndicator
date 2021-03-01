﻿using System;
using System.Threading;
using System.Windows.Forms;
using LoadingIndicator.WinForms.Logging;
using Timer = System.Threading.Timer;

namespace LoadingIndicator.WinForms
{
    internal static class SelectControlExtensions
    {
        private const string AbortReason = "Deadlock in Control.Select detected. Raised ThreadAbort to exit from deadlock.";

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SelectControlExtensions), "Invoke");

        // Try to avoid deadlock on Select method
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
            private readonly Thread _thread;

            public ThreadWrapper(Thread thread)
            {
                _thread = thread;
            }

            public void AbortThread()
            {
                if (_thread.ThreadState != ThreadState.AbortRequested)
                {
                    _thread.Abort(AbortReason);
                }
            }

            public void ResetAbort()
            {
                if (_thread == Thread.CurrentThread && _thread.ThreadState == ThreadState.AbortRequested)
                {
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
