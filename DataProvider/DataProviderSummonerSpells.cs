using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CueLegendKey2
{
    internal class SummonerSpell
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<double> cooldown { get; set; }

        public string fullImageName { get; set; }

        public DataProviderImageSkill dataProviderImageSkill = null;
        public MagicImage image { get; set; }

        public override string ToString()
        {
            return $"SummonerSpell: {name}";
        }
    }

    class DataProviderSummonerSpells : DataProvider
    {
        private List<SummonerSpell> spells = new List<SummonerSpell>();

        public DataProviderSummonerSpells() : base()
        {
            autoReportDoneAfterDecode = false;
        }

        public SummonerSpell GetSpellById(string spellId)
        {
            foreach (SummonerSpell spell in this.spells)
            {
                if (spell.id == spellId)
                {
                    return spell;
                }
            }
            return null;
        }
        public SummonerSpell GetSpellByName(string spellName)
        {
            foreach (SummonerSpell spell in this.spells)
            {
                if (spell.name == spellName)
                {
                    return spell;
                }
            }
            return null;
        }

        protected override FileCache GetCache()
        {
            FileCache fileCache = new FileCache();
            fileCache.SetSubPath($"{currentVersion}");
            fileCache.SetFileName("summoner.json");
            return fileCache;
        }

        public override string GetUri()
        {
            return $"{dataDragonHost}/cdn/{currentVersion}/data/{locale}/summoner.json";
        }

        public override string GetMockFile()
        {
            return "./mock/summoner.json";
        }

        public void ImageDataLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            short loadedImages = 0;
            foreach (SummonerSpell spell in this.spells)
            {
                if (spell.dataProviderImageSkill.image != null)
                {
                    spell.image = new MagicImage(spell.dataProviderImageSkill.image);
                    loadedImages++;
                }
            }

         
            // +1 = champion
            if (loadedImages >= this.spells.Count)
            {
                this.DataLoaded();
            }
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);
            JObject rawData = JObject.Parse(jsonData);
            IList<JToken> spellList = rawData["data"].ToList();

            this.spells = new List<SummonerSpell>();
            foreach (JToken spellData in spellList)
            {
                JToken spellDataX = spellData.First;
                SummonerSpell spell = spellDataX.ToObject<SummonerSpell>();

                spell.fullImageName = spellDataX["image"]["full"].ToObject<string>();
                spell.dataProviderImageSkill = new DataProviderImageSkill();
                spell.dataProviderImageSkill.imageFileName = spell.fullImageName;
                spell.dataProviderImageSkill.currentVersion = currentVersion;
                spell.dataProviderImageSkill.OnData += ImageDataLoaded;
                this.spells.Add(spell);
            }

            foreach (SummonerSpell spell in this.spells)
            {
                try
                {
                    spell.dataProviderImageSkill.LoadData();
                }
                catch (Exception exception)
                {
                    Logger.Instance.Debug(exception.Message);
                }
            }
        }
       
    }
}
