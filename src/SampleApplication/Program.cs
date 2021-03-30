using System;
using System.Windows.Forms;

namespace LoadingIndicator.Sample
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private class MainForm : Form
        {
            public MainForm()
            {
                var mainControl = new MainControl();
                mainControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                Size = mainControl.Size;

                Controls.Add(mainControl);
            }
        }
    }
}
