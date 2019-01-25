using System.Drawing;
using System.Windows.Forms;

namespace LoadingIndicator.WinForms
{
    public class ImageIndicatorControl : PictureBox
    {
        public ImageIndicatorControl(Image image)
        {
            Anchor = AnchorStyles.None;
            BackColor = Color.Transparent;

            Image = image;
            Size = image.Size;
            SizeMode = PictureBoxSizeMode.CenterImage;
            TabStop = false;
        }
    }
}