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
    internal class ChampionSpell
    {
        public string name { get; set; }
        public string id { get; set; }
        public List<double> cooldown { get; set; }
        public List<double> cost { get; set; }
    }

    class DataProviderChampionDetails : DataProvider
    {
        public string championId = "";

        public List<ChampionSpell> championSpells = new List<ChampionSpell>();

        public string currentVersion { get; set; }

        public override string GetUri()
        {
            string uri = $"{dataDragonHost}/cdn/{currentVersion}/data/{locale}/champion/{championId}.json";

            return uri;
        }

        public override string GetMockFile()
        {
            return "./mock/ddragon_Annie.json";
        }

        public override void JsonDecode(string jsonData)
        {
            JObject rawData = JObject.Parse(jsonData);

            // get JSON result objects into a list
            IList<JToken> results = rawData["data"][this.championId]["spells"].Children().ToList();

            // serialize JSON results into .NET objects
            this.championSpells = new List<ChampionSpell>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                ChampionSpell spell = result.ToObject<ChampionSpell>();
                this.championSpells.Add(spell);
            }
        }
       
    }
}
