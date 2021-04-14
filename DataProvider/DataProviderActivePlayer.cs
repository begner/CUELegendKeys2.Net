using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;

namespace CueLegendKey2
{
    internal class PlayerState
    {
        public double currentHealth { get; set; }
        public double maxHealth { get; set; }
        public double resourceValue { get; set; }
        public double resourceMax { get; set; }
        public double abilityHaste { get; set; }
        public double cooldownReduction { get; set; }
        public string resourceType { get; set; }

        public Brush getResourceColor()
        {
            Brush resourceColor = Brushes.Blue;
            if (resourceType == "ENERGY")
            {
                resourceColor = Brushes.Yellow;
            }
            else if (resourceType == "DRAGONFURY")
            {
                if (resourceValue == resourceMax)
                {
                    resourceColor = Brushes.Red;
                }
                else
                {
                    resourceColor = Brushes.Orange;
                }
            }
            else if (resourceType == "GNARFURY")
            {
                if (resourceValue >= 100)
                {
                    resourceColor = Brushes.Red;
                }
                else if (resourceValue >= 60)
                {
                    resourceColor = Brushes.Yellow;
                }
                else
                {
                    resourceColor = Brushes.Gray;
                }
            }
            return resourceColor;
        }

        public override string ToString()
        {
            return $"Health: {currentHealth}/{maxHealth}\nResource ({resourceType}): {resourceValue}/{resourceMax}";
        }
    }

    internal class Abilities
    {
        public Skill Q { get; set; } = new Skill();
        public Skill W { get; set; } = new Skill();
        public Skill E { get; set; } = new Skill();
        public Skill R { get; set; } = new Skill();
        public Passive Passive { get; set; } = new Passive();

        public List<Skill> getSkillsAsList()
        {
            return new List<Skill> { Q, W, E, R };
        }

        public string ToStringWithLiveCooldown(double abilityHaste)
        {
            return $"Q: {Q.ToStringWithLiveCooldown(abilityHaste)}\nW: {W.ToStringWithLiveCooldown(abilityHaste)}\nE: {E.ToStringWithLiveCooldown(abilityHaste)}\nR: {R.ToStringWithLiveCooldown(abilityHaste)}\nPassive: {Passive}\n";
        }

        public override string ToString()
        {
            return $"Q: {Q}\nW: {W}\nE: {E}\nR: {R}\nPassive: {Passive}\n";
        }
    }

    internal class Skill
    {
        public short abilityLevel { get; set; }
        public string displayName { get; set; }
        public string id { get; set; }
        public List<double> cooldown { get; set; }
        public MagicImage image { get; set; }
      
        public double getCurrentCooldown(double abilityHaste = 0)
        {
            if (abilityLevel > 0)
            {
                if (this.cooldown != null)
                {
                    double currentCooldown = this.cooldown[this.abilityLevel - 1];
                    currentCooldown = currentCooldown * 100 / (100 + abilityHaste);
                    currentCooldown = Math.Round(currentCooldown, 2);
                    return currentCooldown;
                }
                return 0;

            }
            else
            {
                return 0;
            }
        }

        public string ToStringWithLiveCooldown(double abilityHaste)
        {
            return $"Level: {abilityLevel}, Cooldown: {getCurrentCooldown(abilityHaste)}, {displayName}";
        }
        public override string ToString()
        {
            return $"Level: {abilityLevel}, Cooldown: {getCurrentCooldown()}, {displayName}";
        }
    }
    internal class Passive
    {
        public string displayName { get; set; }
        public string id { get; set; }
        public MagicImage image { get; set; }

        public override string ToString()
        {
            return $"{displayName}";
        }
    }

    class DataProviderActivePlayer : DataProvider
    {
        public string summonerName = "";
        public PlayerState playerStats = new PlayerState();
        public Abilities abilities = new Abilities();

        private List<Champion> champions = new List<Champion>();
       
        public override string GetUri()
        {
            return $"{clientHost}/liveclientdata/activeplayer";
        }

        public override string GetMockFile()
        {
            return "./mock/activeplayer.json";
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);

            JObject rawData = JObject.Parse(jsonData);
            this.summonerName = rawData["summonerName"].ToObject<string>();

            JToken rawChampionStats = rawData["championStats"];
            this.playerStats = rawChampionStats.ToObject<PlayerState>();

            // get JSON result objects into a list
            JToken rawAbilities = rawData["abilities"];
            this.abilities = rawAbilities.ToObject<Abilities>();
        }
       
    }
}
