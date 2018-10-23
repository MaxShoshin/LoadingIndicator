using System;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.WinForms
{
    internal static class InvokeControlExtensions
    {
        public static bool InvokeIfRequired([NotNull] this Control control, [NotNull] Action action)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (control.InvokeRequired)
            {
                Func<InvokeResult> wrappedAction = () =>
                {
                    if (control.IsDisposed || control.Disposing)
                    {
                        return InvokeResult.Success;
                    }

                    return RunWithStackTrace(action);
                };

                InvokeResult result;
                try
                {
                    result = (InvokeResult)control.Invoke(wrappedAction);
                }
                catch (InvalidOperationException)
                {
                    // Control is disposed between InvokeRequired and Invoke
                    // or control is disposed during Invoke (invoked method is not started but control is disposed)
                    return true;
                }

                result.EnsureSuccess();

                return true;
            }

            if (control.IsDisposed || control.Disposing)
            {
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Reviewed")]
        [NotNull]
        private static InvokeResult RunWithStackTrace(Action action)
        {
            try
            {
                action();
                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                return new InvokeResult(ex);
            }
        }

        private sealed class InvokeResult
        {
            [NotNull] public static readonly InvokeResult Success = new InvokeResult();

            [CanBeNull] private readonly ExceptionDispatchInfo _capturedException;

            public InvokeResult([NotNull] Exception ex)
            {
                if (ex == null) throw new ArgumentNullException(nameof(ex));

                _capturedException = ExceptionDispatchInfo.Capture(ex);
            }

            private InvokeResult()
            {
            }

            public void EnsureSuccess()
            {
                if (_capturedException != null)
                {
                    _capturedException.Throw();
                }
            }
        }
    }
}