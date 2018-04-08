using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.Controls
{
    public sealed class LoadingIndicatorControl : Control
    {
        private const float Tolerance = 0.000001f;

        private int _value;
        private readonly Container _components;
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

            var timerAnimation = new Timer(_components);
            timerAnimation.Interval = 150;
            timerAnimation.Tick += AnimationTick;

            timerAnimation.Start();
        }

        public int NumberOfCircles
        {
            get => _numberOfCircles;
            set
            {
                if (_numberOfCircles == value)
                {
                    return;
                }

                _numberOfCircles = value;
                _angle = 360.0F / value;

                Invalidate();
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
                if (Math.Abs(_circleSize - value) < Tolerance)
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
            e.Graphics.RotateTransform(_angle * _value);

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
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
            if (_value + 1 <= NumberOfCircles)
            {
                _value++;
            }
            else
            {
                _value = 1;
            }

            Invalidate();
        }
    }
}