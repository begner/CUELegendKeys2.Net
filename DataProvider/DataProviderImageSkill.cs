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

    public class DataProviderImageSkill : DataProviderImage
    {
        public DataProviderImageSkill()
        {
            cacheSubPath = "spell";
        }

        public override string GetUri()
        {
            return $"{dataDragonHost}/cdn/{currentVersion}/img/spell/{imageFileName}";
        }

        public override string GetMockFile()
        {
            return "./mock/AnnieQ.png";
        }

    }
}
