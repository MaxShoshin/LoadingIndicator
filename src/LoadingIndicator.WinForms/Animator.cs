using System;
using System.Windows.Forms;

namespace LoadingIndicator.WinForms
{
    public class Animator : IDisposable
    {
        private readonly Control _control;
        private int _animationFrame = 0;
        private readonly Timer _timerAnimation;
        private int _frameCount;

        public Animator(Control control, int frameCount)
        {
            _control = control;
            _frameCount = frameCount;

            _timerAnimation = new Timer();
            _timerAnimation.Interval = 150;
            _timerAnimation.Tick += AnimationTick;

            _timerAnimation.Start();
        }

        public int FrameCount
        {
            get => _frameCount;
            set
            {
                if (_frameCount == value)
                {
                    return;
                }

                _frameCount = value;

                if (_animationFrame > _frameCount)
                {
                    _animationFrame = 1;
                }

                Invalidate();
            }
        }

        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(_timerAnimation.Interval);
            set => _timerAnimation.Interval = (int) value.TotalMilliseconds;
        }

        public int CurrentFrame => _animationFrame;

        public void Dispose()
        {
            _timerAnimation.Stop();
            _timerAnimation.Dispose();
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (!_control.IsHandleCreated || _control.Parent == null || _control.Disposing || _control.IsDisposed)
            {
                return;
            }

            if (_animationFrame + 1 < FrameCount)
            {
                _animationFrame++;
            }
            else
            {
                _animationFrame = 0;
            }

            Invalidate();
        }

        private void Invalidate()
        {
            if (_control.IsControlVisibleToUser())
            {
                _control.Invalidate();
            }
        }
    }
}