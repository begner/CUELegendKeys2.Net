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
        public DataProviderImageSkill dataProviderImageSkill = null;
        public List<double> cooldown { get; set; }
        public List<double> cost { get; set; }
    }

    internal class ChampionPassive
    {
        public string name { get; set; }
        public string description { get; set; }
        public string fullImageName { get; set; }

        public DataProviderImagePassive dataProviderImagePassive = null;

    }
    internal class ChampionInfo
    {
        public DataProviderImageChampion dataProviderImageChampion = null;
        public MagicImage image { get; set; }

        public string fullImageName { get; set; }
        public string name { get; set; }
        public string title { get; set; }
        public string lore { get; set; }
        public string blurb { get; set; }
    }


    class DataProviderChampionDetails : DataProvider
    {
        public DataProviderChampionDetails() : base()
        {
            autoReportDoneAfterDecode = false;
        }

        public string championId = "";

        public List<ChampionSpell> championSpells = new List<ChampionSpell>();
        public ChampionPassive championPassive = new ChampionPassive();

        public ChampionInfo championInfo = new ChampionInfo();
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
                if (spell.dataProviderImageSkill.image != null)
                {
                    spell.spellImage = spell.dataProviderImageSkill.image;
                    loadedImages++;
                }
            }

            if (this.championInfo.dataProviderImageChampion.image != null)
            {
                this.championInfo.image = new MagicImage(this.championInfo.dataProviderImageChampion.image);
                loadedImages++;
            }
           
            // +1 = champion
            if (loadedImages >= this.championSpells.Count + 1) {
                this.DataLoaded();
            }
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);
            JObject rawData = JObject.Parse(jsonData);

            this.championInfo = rawData["data"][this.championId].ToObject<ChampionInfo>();
            this.championInfo.fullImageName = rawData["data"][this.championId]["image"]["full"].ToObject<string>();
            this.championInfo.dataProviderImageChampion = new DataProviderImageChampion();
            this.championInfo.dataProviderImageChampion.imageFileName = this.championInfo.fullImageName;
            this.championInfo.dataProviderImageChampion.currentVersion = currentVersion;
            this.championInfo.dataProviderImageChampion.OnData += ImageDataLoaded;


            this.championPassive = rawData["data"][this.championId]["passive"].ToObject<ChampionPassive>();
            this.championPassive.fullImageName = rawData["data"][this.championId]["passive"]["image"]["full"].ToObject<string>();
            this.championPassive.dataProviderImagePassive = new DataProviderImagePassive();
            this.championPassive.dataProviderImagePassive.imageFileName = this.championPassive.fullImageName;
            this.championPassive.dataProviderImagePassive.currentVersion = currentVersion;
            this.championPassive.dataProviderImagePassive.OnData += ImageDataLoaded;


            // get JSON result objects into a list
            IList<JToken> results = rawData["data"][this.championId]["spells"].Children().ToList();

            // serialize JSON results into .NET objects
            this.championSpells = new List<ChampionSpell>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                ChampionSpell spell = result.ToObject<ChampionSpell>();
                spell.fullImageName = result["image"]["full"].ToObject<string>();
                spell.dataProviderImageSkill = new DataProviderImageSkill();
                spell.dataProviderImageSkill.imageFileName = spell.fullImageName;
                spell.dataProviderImageSkill.currentVersion = currentVersion;
                spell.dataProviderImageSkill.OnData += ImageDataLoaded;

                this.championSpells.Add(spell);
            }

            try
            {
                this.championPassive.dataProviderImagePassive.LoadData();
                this.championInfo.dataProviderImageChampion.LoadData();
                foreach (ChampionSpell spell in this.championSpells)
                {
                    spell.dataProviderImageSkill.LoadData();
                }
            }
            catch (Exception exception)
            {
                Logger.Instance.Debug(exception.Message);
            }

            this.currentlyLoadedChampionId = this.championId;
            // this.DataLoaded();
        }
       
    }
}
