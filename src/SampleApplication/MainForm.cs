using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoadingIndicator.Controls;

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

        private async void button1_Click(object sender, System.EventArgs e)
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

        private async void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                await StartLongOperationAsync(0.8);
            }
        }
    }
}
