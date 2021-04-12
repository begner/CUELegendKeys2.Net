using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.Drawing;
using System.IO;

namespace CueLegendKey2
{

    public abstract class DataProviderImage : DataProvider
    {
        public Image image = null;
        public string imageFileName { get; set; } = "";
        
        protected string cacheSubPath { get; set; } = "";

        protected override FileCache GetCache()
        {
            FileCache fileCache = new FileCache();
            fileCache.SetSubPath($"{currentVersion}/{cacheSubPath}");
            fileCache.SetFileName(this.imageFileName);
            return fileCache;
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            Image image = Image.FromStream(stream, true, true);
            this.image = image;
        }

    }
}
