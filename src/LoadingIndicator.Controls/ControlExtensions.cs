using System;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.Controls
{
    public static class ControlExtensions
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

                var result = (InvokeResult)control.Invoke(wrappedAction);
                result.EnsureSuccess();

                return true;
            }

            if (control.IsDisposed || control.Disposing)
            {
                return true;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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

            private InvokeResult()
            {
            }

            public InvokeResult([NotNull] Exception ex)
            {
                if (ex == null) throw new ArgumentNullException(nameof(ex));

                _capturedException = ExceptionDispatchInfo.Capture(ex);
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