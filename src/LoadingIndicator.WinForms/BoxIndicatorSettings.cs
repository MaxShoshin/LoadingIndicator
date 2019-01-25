using System;
using System.Drawing;
using System.Windows.Forms;

namespace LoadingIndicator.WinForms
{
    public sealed class BoxIndicatorSettings
    {
        public int NumberOfBoxes { get; set; } = 3;

        public TimeSpan AnimationInterval { get; set; } = TimeSpan.FromMilliseconds(200);

        public Color BoxColor { get; set; } = Color.FromArgb(162, 199, 214);

        public Color HighlightedBoxColor { get; set; } = Color.FromArgb(67, 143, 174);

        public int RoundCornerRadius { get; set; } = 3;

        public int BoxSize { get; set; } = 12;
    }
}