using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.WinForms
{
    internal sealed class LayerControl : Control
    {
        private const int WsExComposited = 0x02000000;
        private const int WsClipChildren = 0x02000000;

        private readonly Func<Image, Image> _imageProcessor;

        [CanBeNull] private Control _indicator;
        [CanBeNull] private IDisposable _subscription;

        public LayerControl([NotNull] Image backgroundImage, [NotNull] Func<Image, Image> imageProcessor)
        {
            if (backgroundImage == null) throw new ArgumentNullException(nameof(backgroundImage));
            if (imageProcessor == null) throw new ArgumentNullException(nameof(imageProcessor));

            _imageProcessor = imageProcessor;
            BackgroundImage = backgroundImage;

            SetStyle(ControlStyles.Selectable, true);
        }

        // HACK: To reduce form flicker
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WsExComposited; // Turn on WS_EX_COMPOSITED
                cp.Style &= ~WsClipChildren;  // Turn off WS_CLIPCHILDREN
                return cp;
            }
        }

        public void Remove()
        {
            var parent = Parent;
            if (parent == null)
            {
                return;
            }

            _indicator?.Dispose();
            _indicator = null;

            UnsubscribeChildrenControlEnter();

            parent.Controls.Remove(this);

            Dispose();
        }

        public void PlaceIndicator([NotNull] Control indicator)
        {
            if (indicator == null) throw new ArgumentNullException(nameof(indicator));

            Cursor = Cursors.WaitCursor;
            BackgroundImage = _imageProcessor(BackgroundImage);

            _indicator = indicator;

            PlaceIndicator();
        }

        public void SubscribeChildrenControlEnter()
        {
            _subscription = new Subscription(Parent, OnControlEnter, ParentControlChanged);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (Parent == null)
            {
                return;
            }

            // TODO: Fix
            BackgroundImage = Parent.CaptureScreenshot();

            if (_indicator == null)
            {
                return;
            }

            PlaceIndicator();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _indicator?.Dispose();
                _subscription?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void PlaceIndicator()
        {
            var indicator = _indicator;
            if (indicator == null)
            {
                return;
            }

            var left = (Size.Width - indicator.Size.Width) / 2;
            var top = (Size.Height - indicator.Size.Height) / 2;

            if (left < 0 || top < 0)
            {
                Controls.Remove(indicator);
            }
            else
            {
                indicator.Location = new Point(left, top);

                if (!Contains(indicator))
                {
                    Controls.Add(indicator);
                    indicator.BringToFront();
                }
            }
        }

        private void ParentControlChanged(object sender, ControlEventArgs e)
        {
            UnsubscribeChildrenControlEnter();
            SubscribeChildrenControlEnter();
        }

        private void UnsubscribeChildrenControlEnter()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        private void OnControlEnter(object sender, EventArgs e)
        {
            this.SafeSelect();
        }

        private sealed class Subscription : IDisposable
        {
            [NotNull] private readonly Control _parent;
            [NotNull] private readonly EventHandler _onControlEnter;
            [NotNull] private readonly ControlEventHandler _onParentControlChanged;
            [NotNull] private readonly List<Control> _children;

            public Subscription([NotNull] Control parent, [NotNull] EventHandler onControlEnter, [NotNull] ControlEventHandler onParentControlChanged)
            {
                _parent = parent;
                _onControlEnter = onControlEnter;
                _onParentControlChanged = onParentControlChanged;
                _children = new List<Control>(_parent.Controls.Count);

                _parent.ControlAdded += onParentControlChanged;
                _parent.ControlRemoved += onParentControlChanged;

                foreach (Control childControl in _parent.Controls)
                {
                    if (!(childControl is LayerControl))
                    {
                        _children.Add(childControl);
                        childControl.Enter += onControlEnter;
                    }
                }
            }

            public void Dispose()
            {
                _parent.ControlAdded -= _onParentControlChanged;
                _parent.ControlRemoved -= _onParentControlChanged;

                foreach (var childControl in _children)
                {
                    childControl.Enter -= _onControlEnter;
                }
            }
        }
    }
}