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
    internal class ChampionSpell
    {
        public string name { get; set; }
        public string id { get; set; }
        public string fullImageName { get; set; }

        public Image spellImage = null;
        public DataProviderAbilityImage dataProviderAbilityImage = null;
        public List<double> cooldown { get; set; }
        public List<double> cost { get; set; }
    }

    class DataProviderChampionDetails : DataProvider
    {
        public DataProviderChampionDetails() : base()
        {
            autoReportDoneAfterDecode = false;
        }

        public string championId = "";

        public List<ChampionSpell> championSpells = new List<ChampionSpell>();

        private string currentlyLoadedChampionId = "";

        public override string GetUri()
        {
            string uri = $"{dataDragonHost}/cdn/{currentVersion}/data/{locale}/champion/{championId}.json";

            return uri;
        }

        public override string GetMockFile()
        {
            return "./mock/ddragon_Annie.json";
        }

        protected override FileCache GetCache()
        {
            FileCache fileCache = new FileCache();
            fileCache.SetSubPath($"{currentVersion}/champion");
            fileCache.SetFileName($"{championId}.json");
            return fileCache;
        }

        public override void LoadData()
        {
            // if championDetails were already loaded to matching champ...
            if (this.currentlyLoadedChampionId == this.championId) {
                this.DataLoaded();
            }
            else
            {
                this.LoadWebData(this.GetUri(), this.GetMockFile());
            }
        }

        public void ImageDataLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            short loadedImages = 0;
            foreach (ChampionSpell spell in this.championSpells)
            {
                if (spell.dataProviderAbilityImage.image != null)
                {
                    spell.spellImage = spell.dataProviderAbilityImage.image;
                    loadedImages++;
                }

            }
            if (loadedImages >= this.championSpells.Count) {
                this.DataLoaded();
            }
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);
            JObject rawData = JObject.Parse(jsonData);

            // get JSON result objects into a list
            IList<JToken> results = rawData["data"][this.championId]["spells"].Children().ToList();

            // serialize JSON results into .NET objects
            this.championSpells = new List<ChampionSpell>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                ChampionSpell spell = result.ToObject<ChampionSpell>();
                spell.fullImageName = result["image"]["full"].ToObject<string>();
                spell.dataProviderAbilityImage = new DataProviderAbilityImage();
                spell.dataProviderAbilityImage.imageFileName = spell.fullImageName;
                spell.dataProviderAbilityImage.currentVersion = currentVersion;
                spell.dataProviderAbilityImage.OnData += ImageDataLoaded;

                this.championSpells.Add(spell);
            }

            foreach(ChampionSpell spell in this.championSpells)
            {
                try
                {
                    spell.dataProviderAbilityImage.LoadData();
                } catch(Exception exception)
                {
                    Logger.Instance.Debug(exception.Message);
                }
            }

            this.currentlyLoadedChampionId = this.championId;
            this.DataLoaded();
        }
       
    }
}
