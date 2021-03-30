using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoadingIndicator.WinForms;

namespace LoadingIndicator.Sample
{
    public sealed partial class MainControl : Control
    {
        private readonly LongOperation _longOperation;

        public MainControl()
        {
            InitializeComponent();

            pictureBox1.Image = Images.SampleImage;
            pictureBox2.Image = Images.SampleImage;

            var settings = LongOperationSettings.Default
                .WithBoxIndicator(boxSettings =>
                {
                    boxSettings.NumberOfBoxes = 5;
                })
                .AllowStopBeforeStartMethods();

            _longOperation = new LongOperation(this, settings);
        }

        private async void ButtonClick(object sender, System.EventArgs e)
        {
            await StartLongOperationAsync();
        }

        private async Task StartLongOperationAsync(double seconds = 10, bool immediatley = false)
        {
            using (_longOperation.Start(immediatley))
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
            }
        }

        private async void TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                await StartLongOperationAsync(0.8, true);
            }
        }
    }
}
