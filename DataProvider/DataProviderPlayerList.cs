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
    internal class PlayerSummonerSpell
    {
        public string displayName { get; set; }
        public string rawDescription { get; set; }
        public string rawDisplayName { get; set; }
    }

    internal class PlayerSummonerSpells
    {
        public PlayerSummonerSpell summonerSpellOne { get; set; } = new PlayerSummonerSpell();
        public PlayerSummonerSpell summonerSpellTwo { get; set; } = new PlayerSummonerSpell();
    }
    internal class Player
    {
        public string championName { get; set; }
        public string summonerName { get; set; }
        public string position { get; set; }
        public bool isDead { get; set; }
        public short level { get; set; }
        public double respawnTimer { get; set; }
        public PlayerSummonerSpells summonerSpells { get; set; } = new PlayerSummonerSpells();

        public override string ToString()
        {
            string retString = "";
            retString += $"Player: {championName}, Level {level} @ {position}";
            if (isDead)
            {
                retString += $" - DEAD {respawnTimer}\n";
            }
            else
            {
                retString += $" - ALIVE\n";
            }
            return retString;
        }
    }

    class DataProviderPlayerList : DataProvider
    {
        private List<Player> playerList = new List<Player>();

        public string GetChampionNameBySummonerName(string summonerName)
        {
            Player foundPlayer = this.GetPlayerBySummonerName(summonerName);
            if (foundPlayer != null)
            {
                return foundPlayer.championName;
            }
            return "";
        }

        public Player GetPlayerBySummonerName(string summonerName)
        {
            if (playerList.Count > 0)
            {
                Player foundPlayer = (from player in playerList where player.summonerName == summonerName select player).First();
                if (foundPlayer != null)
                {
                    return foundPlayer;
                }
            }
            return new Player();
        }

        public override string GetUri()
        {
            return $"{clientHost}/liveclientdata/playerlist";
        }

        public override string GetMockFile()
        {
            return "./mock/playerlist.json";
        }

        protected override void DecodeData(System.IO.Stream stream)
        {
            string jsonData = this.StreamToString(stream);
            JObject rawData = JObject.Parse("{\"data\": " + jsonData + "}");

            IList<JToken> playerList = rawData["data"].Children().ToList();

            // serialize JSON results into .NET objects
            this.playerList = new List<Player>();
            foreach (JToken playerData in playerList)
            {
                Player player = playerData.ToObject<Player>();
                this.playerList.Add(player);
            }
        }
       
    }
}
