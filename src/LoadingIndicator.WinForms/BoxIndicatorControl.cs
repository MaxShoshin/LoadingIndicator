using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.WinForms
{
    public class BoxIndicatorControl : Control
    {
        private const int BoxesMinCount = 3;

        [NotNull] private readonly Container _components;
        [NotNull] private readonly Timer _timerAnimation;

        private Color _boxColor;
        private Color _highlightedBoxColor;

        [NotNull] private SolidBrush _normalBoxBrush;
        [NotNull] private SolidBrush _highlightedBoxBrush;

        private int _numberOfBoxes;
        private int _animationFrame;
        private int _cornerRadius;
        private int _boxSize;

        public BoxIndicatorControl([NotNull] BoxIndicatorSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _boxSize = settings.BoxSize;
            _numberOfBoxes = settings.NumberOfBoxes;
            _cornerRadius = settings.RoundCornerRadius;
            _boxColor = settings.BoxColor;
            _highlightedBoxColor = settings.HighlightedBoxColor;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);

            Size = new Size((int)(_boxSize * _numberOfBoxes * 1.5), (int)(_boxSize * 1.1));

            _normalBoxBrush = new SolidBrush(_boxColor);
            _highlightedBoxBrush = new SolidBrush(_highlightedBoxColor);

            _components = new Container();

            _timerAnimation = new Timer(_components);
            _timerAnimation.Interval = settings.AnimationInteval;
            _timerAnimation.Tick += AnimationTick;

            _timerAnimation.Start();
        }

        public BoxIndicatorControl()
            : this(new BoxIndicatorSettings())
        {
        }

        public int NumberOfBoxes
        {
            get => _numberOfBoxes;
            set
            {
                if (_numberOfBoxes == value || value < BoxesMinCount)
                {
                    return;
                }

                _numberOfBoxes = value;

                Invalidate();
            }
        }

        public int AnimationInteval
        {
            get => _timerAnimation.Interval;
            set => _timerAnimation.Interval = value;
        }

        public Color BoxColor
        {
            get => _boxColor;
            set
            {
                if (_boxColor == value)
                {
                    return;
                }

                _boxColor = value;

                _normalBoxBrush.Dispose();
                _normalBoxBrush = new SolidBrush(value);

                Invalidate();
            }
        }

        public Color HighlightedBoxColor
        {
            get => _highlightedBoxColor;
            set
            {
                if (_highlightedBoxColor == value)
                {
                    return;
                }

                _highlightedBoxColor = value;

                _highlightedBoxBrush.Dispose();
                _highlightedBoxBrush = new SolidBrush(value);

                Invalidate();
            }
        }

        public int RoundCornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException();
                }

                if (_cornerRadius == value)
                {
                    return;
                }

                _cornerRadius = value;
                Invalidate();
            }
        }

        public int BoxSize
        {
            get => _boxSize;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException();
                }

                if (_boxSize == value)
                {
                    return;
                }

                _boxSize = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.Bilinear;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (var i = 0; i < NumberOfBoxes; i++)
            {
                var brush = _animationFrame == i ? _highlightedBoxBrush : _normalBoxBrush;

                var rectangle = new Rectangle(0, 0, _boxSize, _boxSize);
                var offset = (int)(i * rectangle.Width * 1.5);
                rectangle.Offset(offset, 0);

                e.Graphics.FillPath(brush, RoundedRect(rectangle, _cornerRadius));
            }

            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components.Dispose();

                _normalBoxBrush.Dispose();
                _highlightedBoxBrush.Dispose();
            }

            base.Dispose(disposing);
        }

        private void AnimationTick(object sender, EventArgs e)
        {
            if (_animationFrame + 1 < NumberOfBoxes)
            {
                _animationFrame++;
            }
            else
            {
                _animationFrame = 0;
            }

            Invalidate();
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}