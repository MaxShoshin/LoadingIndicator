using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.Winforms
{
    public class LongOperation : IDisposable
    {
        [NotNull] private readonly Control _parentControl;
        [NotNull] private readonly IDisposable _stopDisposable;
        private readonly TimeSpan _indicatorDelay;
        private readonly TimeSpan _minIndicatorShowTime;

        [CanBeNull] private LayerControl _layerControl;
        [CanBeNull] private Control _previouslyFocusedControl;

        private int _started;
        private CancellationTokenSource _cancelationSource;
        private DateTime? _indicatorShownAt;

        public LongOperation([NotNull] Control parentControl)
            : this(parentControl, TimeSpan.FromMilliseconds(700), TimeSpan.FromMilliseconds(400))
        {
        }

        public LongOperation([NotNull] Control parentControl, TimeSpan indicatorDelay, TimeSpan minIndicatorShowTime)
        {
            if (parentControl == null) throw new ArgumentNullException(nameof(parentControl));
            if (indicatorDelay < TimeSpan.FromMilliseconds(200)) throw new ArgumentException("Indicator delay should be greater then 200ms", nameof(indicatorDelay));
            if (minIndicatorShowTime < TimeSpan.Zero) throw new ArgumentException("Min indicator shown time should be greater then zero.", nameof(minIndicatorShowTime));

            _parentControl = parentControl;
            _indicatorDelay = indicatorDelay;
            _minIndicatorShowTime = minIndicatorShowTime;
            _stopDisposable = new DisposableAction(Stop);
        }

        [NotNull]
        public IDisposable Start()
        {
            if (Interlocked.Increment(ref _started) != 1)
            {
                return _stopDisposable;
            }

            if (_parentControl.InvokeIfRequired(() => Start()))
            {
                return _stopDisposable;
            }

            _indicatorShownAt = null;

            // HACK: To capture latest screenshot
            // i.e. when we select node in tree view just before start operation
            // without refresh this selection will be displayed only after layer control will disappear
            _parentControl.Refresh();

            _previouslyFocusedControl = FindFocusedControl(_parentControl.FindForm());

            var parentControlImage = _parentControl.CaptureScreenshot();

            _layerControl = new LayerControl(parentControlImage);
            _layerControl.Location = new Point(0, 0);
            _layerControl.Size = _parentControl.Size;
            _layerControl.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top;

            _parentControl.Controls.Add(_layerControl);

            _layerControl.SubscribeChildrenControlEnter();
            _layerControl.BringToFront();
            _layerControl.Select();

            _cancelationSource = new CancellationTokenSource();
            Task.Run(StartAsync);

            return _stopDisposable;
        }

        public async void Stop()
        {
            var indicatorShownAt = _indicatorShownAt;
            if (indicatorShownAt.HasValue)
            {
                var indicatorDisplayTime = DateTime.UtcNow - indicatorShownAt.Value;
                if (indicatorDisplayTime < _minIndicatorShowTime)
                {
                    await Task.Delay(_minIndicatorShowTime - indicatorDisplayTime).ConfigureAwait(false);
                }
            }

            var value = Interlocked.Decrement(ref _started);
            if (value > 0)
            {
                return;
            }

            if (value < 0)
            {
                throw new InvalidOperationException("Stop long operation more times then starts.");
            }

            StopInternal();
        }

        public void Dispose()
        {
            Stop();
        }

        [NotNull]
        protected virtual Image ChangeImage([NotNull] Image sourceImage)
        {
            if (sourceImage == null) throw new ArgumentNullException(nameof(sourceImage));

            return sourceImage.ToBitmap().ImageBlurFilter().MakeGrayscale();
        }

        [NotNull]
        protected virtual Control CreateProgressIndicator()
        {
            return new LoadingIndicatorControl();
        }

        [CanBeNull]
        private static Control FindFocusedControl([CanBeNull] ContainerControl control)
        {
            if (control == null)
            {
                return null;
            }

            var focusedControl = control.ActiveControl;

            if (focusedControl == control || focusedControl == null)
            {
                return control;
            }

            var compositeFocused = focusedControl as ContainerControl;

            if (compositeFocused == null)
            {
                return focusedControl;
            }

            return FindFocusedControl(compositeFocused);
        }

        private void StopInternal()
        {
            if (_parentControl.InvokeIfRequired(StopInternal))
            {
                return;
            }

            if (Volatile.Read(ref _started) != 0)
            {
                return;
            }

            _cancelationSource.Cancel();

            var form = _parentControl.FindForm();
            var currentFocused = FindFocusedControl(form);

            _layerControl.Remove();

            if (form != null && currentFocused == _layerControl)
            {
                RestoreFocus(form);
            }

            _layerControl = null;
        }

        private void RestoreFocus([NotNull] ContainerControl containerControl)
        {
            if (_previouslyFocusedControl == null ||
                _previouslyFocusedControl.IsDisposed ||
                _previouslyFocusedControl.Disposing ||
                _previouslyFocusedControl.Parent == null ||
                !_previouslyFocusedControl.Visible ||
                containerControl == _previouslyFocusedControl)
            {
                return;
            }

            containerControl.ActiveControl = _previouslyFocusedControl;
        }

        private async Task StartAsync()
        {
            var cancelToken = _cancelationSource.Token;

            await Task.Delay(_indicatorDelay).ConfigureAwait(false);

            if (_started == 0 || cancelToken.IsCancellationRequested)
            {
                return;
            }

            _indicatorShownAt = _minIndicatorShowTime == TimeSpan.Zero
                ? DateTime.MinValue
                : DateTime.UtcNow;

            DisplayIndicator(cancelToken);
        }

        private void DisplayIndicator(CancellationToken cancelToken)
        {
            if (_parentControl.InvokeIfRequired(() => DisplayIndicator(cancelToken)))
            {
                return;
            }

            if (_started == 0 || cancelToken.IsCancellationRequested || _layerControl == null)
            {
                return;
            }

            _layerControl.PlaceIndicator(
                CreateProgressIndicator(),
                ChangeImage(_layerControl.BackgroundImage));
        }

        private sealed class DisposableAction : IDisposable
        {
            [NotNull] private readonly Action _action;

            public DisposableAction([NotNull] Action action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }
    }
}