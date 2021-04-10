using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CueLegendKey2
{
    class DataProviderVersions : DataProvider
    {
        public List<string> versions = new List<string>();

        public string getCurrentVersion()
        {
            if (this.versions.Count > 0)
            {
                return this.versions[0];
            }
            return "";
        }

        public override string GetUri()
        {
            return $"{dataDragonHost}/api/versions.json";
        }

        public override string GetMockFile()
        {
            return "./mock/versions.json";
        }

        public override void JsonDecode(string jsonData)
        {
            JObject playerListRawData = JObject.Parse("{\"versions\": " + jsonData + "}");
            IList<JToken> versionList = playerListRawData["versions"].Children().ToList();

            this.versions = new List<string>();
            foreach (JToken versionData in versionList)
            {
                string versionString = versionData.ToObject<string>();
                this.versions.Add(versionString);
            }
        }
       
    }
}
