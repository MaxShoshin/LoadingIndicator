using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.WinForms
{
    public sealed class LoadingIndicatorControl : Control
    {
        private const float Tolerance = 0.000001f;
        private const float MinCircleSize = 0.01f;
        private const int CirclesMinCount = 3;

        private readonly Container _components;
        [NotNull] private readonly Timer _timerAnimation;

        private int _animationFrame;
        private float _circleSize = 1;
        private Color _circleColor = Color.FromArgb(172, Color.Orange);
        [NotNull] private SolidBrush _circleBrush;
        private int _numberOfCircles = 8;
        private float _angle;

        public LoadingIndicatorControl()
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);

            Size = new Size(100, 100);

            _circleBrush = new SolidBrush(_circleColor);
            _angle = 360f / _numberOfCircles;

            _components = new Container();

            _timerAnimation = new Timer(_components);
            _timerAnimation.Interval = 150;
            _timerAnimation.Tick += AnimationTick;

            _timerAnimation.Start();
        }

        public int NumberOfCircles
        {
            get => _numberOfCircles;
            set
            {
                if (_numberOfCircles == value || value < CirclesMinCount)
                {
                    return;
                }

                _numberOfCircles = value;
                _angle = 360.0F / value;

                Invalidate();
            }
        }

        public int AnimationInterval
        {
            get => _timerAnimation.Interval;
            set
            {
                if (_timerAnimation.Interval == value)
                {
                    return;
                }

                _timerAnimation.Interval = value;
            }
        }

        public Color CircleColor
        {
            get => _circleColor;
            set
            {
                if (_circleColor == value)
                {
                    return;
                }

                _circleColor = value;
                _circleBrush = new SolidBrush(value);

                Invalidate();
            }
        }

        public float CircleSize
        {
            get => _circleSize;
            set
            {
                if (Math.Abs(_circleSize - value) < Tolerance || value < MinCircleSize)
                {
                    return;
                }

                _circleSize = value;

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(Width / 2.0F, Height / 2.0F);
            e.Graphics.RotateTransform(_angle * _animationFrame);

            e.Graphics.InterpolationMode = InterpolationMode.Bilinear;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (var i = 1; i <= NumberOfCircles; i++)
            {
                var sizeRate = 6F / CircleSize;
                var size = Width / sizeRate;

                if (i == NumberOfCircles - 1)
                {
                    size = size * 1.2f;
                }
                else if (i == NumberOfCircles)
                {
                    size = size * 1.4f;
                }

                var diff = (Width / 4.5F) - size;

                var x = (Width / 9.0F) + diff;
                var y = (Height / 9.0F) + diff;

                e.Graphics.FillEllipse(_circleBrush, x, y, size, size);
                e.Graphics.RotateTransform(_angle);
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components.Dispose();
                _circleBrush.Dispose();
            }

            base.Dispose(disposing);
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (_animationFrame + 1 <= NumberOfCircles)
            {
                _animationFrame++;
            }
            else
            {
                _animationFrame = 1;
            }

            Invalidate();
        }
    }
}