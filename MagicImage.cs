using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CueLegendKey2
{
    public class MagicImage
    {
        private System.Drawing.Image image = null;

        public MagicImage()
        {

        }

        public MagicImage(System.Drawing.Image image)
        {
            this.SetImage(image);
        }

        public void SetImage(System.Drawing.Image image)
        {
            this.image = image;
        }

        public System.Windows.Media.Imaging.BitmapImage GetImageAsBitmap()
        {
            if (this.image != null)
            {
                Stream ms = new MemoryStream();
                this.image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                System.Windows.Media.Imaging.BitmapImage bi = new System.Windows.Media.Imaging.BitmapImage();
                bi.BeginInit();
                bi.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();

                return bi;
            }
            return null;
        }
    }

}
