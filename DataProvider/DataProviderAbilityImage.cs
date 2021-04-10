using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media;

namespace CueLegendKey2
{

    class DataProviderAbilityImage : DataProvider
    {
        private string imageFileName { get; set; } = "";
        public override string GetUri()
        {
            return $"{dataDragonHost}/cdn/{currentVersion}/img/spell/{imageFileName}";
        }

        public override string GetMockFile()
        {
            return "./mock/AnnieQ.json";
        }

        public override void JsonDecode(string jsonData)
        {
            
        }
       
    }
}
