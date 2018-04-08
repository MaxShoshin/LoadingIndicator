using System;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace LoadingIndicator.Controls
{
    internal sealed class LayerControl : Control
    {
        [CanBeNull] private Control _indicator;

        public LayerControl([NotNull] Image backgroundImage)
        {
            if (backgroundImage == null) throw new ArgumentNullException(nameof(backgroundImage));

            BackgroundImage = backgroundImage;

            SetStyle(ControlStyles.Selectable, true);
        }

        public void Remove()
        {
            UnSubscribeFocus();
            Parent.Controls.Remove(this);
            SizeChanged -= PlaceIndicator;
        }

        public void PlaceIndicator([NotNull] Control indicator, [NotNull] Image backgroundImage)
        {
            if (indicator == null) throw new ArgumentNullException(nameof(indicator));
            if (backgroundImage == null) throw new ArgumentNullException(nameof(backgroundImage));

            Cursor = Cursors.WaitCursor;
            BackgroundImage = backgroundImage;

            _indicator = indicator;

            SizeChanged += PlaceIndicator;

            PlaceIndicator();
        }

        public void PreventOtherControlFocus()
        {
            Parent.ControlAdded += ParentControlChanged;
            Parent.ControlRemoved += ParentControlChanged;

            foreach (Control childControl in Parent.Controls)
            {
                if (childControl != this)
                {
                    childControl.Enter += OnControlEnter;
                }
            }
        }

        // HACK: To reduce form flicker
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // Turn on WS_EX_COMPOSITED
                cp.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                return cp;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void PlaceIndicator([CanBeNull] object sender = null, [CanBeNull] EventArgs e = null)
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
            UnSubscribeFocus();
            PreventOtherControlFocus();
        }

        private void UnSubscribeFocus()
        {
            Parent.ControlAdded -= ParentControlChanged;
            Parent.ControlRemoved -= ParentControlChanged;

            foreach (Control childControl in Parent.Controls)
            {
                if (childControl != this)
                {
                    childControl.Enter -= OnControlEnter;
                }
            }
        }

        private void OnControlEnter(object sender, EventArgs e)
        {
            Select();
        }
    }
}