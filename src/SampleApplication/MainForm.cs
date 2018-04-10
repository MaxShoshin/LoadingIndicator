using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoadingIndicator.Winforms;

namespace ProgressIndicator
{
    public sealed partial class MainForm : Form
    {
        private readonly LongOperation _longOperation;

        public MainForm()
        {
            InitializeComponent();

            pictureBox1.Image = Images.SampleImage;
            pictureBox2.Image = Images.SampleImage;

            _longOperation = new LongOperation(this);
        }

        private async void ButtonClick(object sender, System.EventArgs e)
        {
            await StartLongOperationAsync();
        }

        private async Task StartLongOperationAsync(double seconds = 10)
        {
            using (_longOperation.Start())
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds)).ConfigureAwait(false);
            }
        }

        private async void TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                await StartLongOperationAsync(0.8);
            }
        }
    }
}
