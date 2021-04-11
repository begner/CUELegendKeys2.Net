using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CueLegendKey2
{
    internal class Champion
    {
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string blurb { get; set; }

        public override string ToString()
        {
            return $"Champion: {name} - {title}";
        }
    }

    class DataProviderChampions : DataProvider
    {
        private List<Champion> champions = new List<Champion>();

        public int getChampionCount()
        {
            return this.champions.Count;
        }

        public string GetChampionIDbyName(string championName)
        {
            if (championName == null)
            {
                return "";
            }
            if (champions.Count > 0)
            {
                Champion foundChampion = (from champion in champions where champion.name == championName select champion).First();
                if (foundChampion != null)
                {
                    return foundChampion.id;
                }
            }
            return "";
        }

        protected override FileCache GetCache()
        {
            FileCache fileCache = new FileCache();
            fileCache.SetSubPath($"{currentVersion}");
            fileCache.SetFileName("champions.json");
            return fileCache;
        }

        public override string GetUri()
        {
            return $"{dataDragonHost}/cdn/{currentVersion}/data/{locale}/champion.json";
        }

        public override string GetMockFile()
        {
            return "./mock/champion.json";
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);
            JObject rawData = JObject.Parse(jsonData);
            IList<JToken> championList = rawData["data"].ToList();

            this.champions = new List<Champion>();
            foreach (JToken championData in championList)
            {
                JToken championDataX = championData.First;
                Champion champion = championDataX.ToObject<Champion>();
                this.champions.Add(champion);
            }
        }
       
    }
}
