using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CueLegendKey2
{
    public class DataFetcherLoadedEventArgs : EventArgs
    {
        public TimeSpan loadDuration;

        public DateTime loadStartTime;

        public int getRequestDurationInMs()
        {
            return Convert.ToInt32(Math.Round(loadDuration.TotalMilliseconds));
        }
        public DataFetcherLoadedEventArgs(DateTime loadStartTime)
        {
            this.loadStartTime = loadStartTime;
            this.loadDuration = DateTime.Now.Subtract(loadStartTime);
        }
    }

    class DataFetcher
    {
        private DataProviderVersions dataProviderVersions = new DataProviderVersions();
        private DataProviderChampions dataProviderChampions = new DataProviderChampions();
        private DataProviderActivePlayer dataProviderActivePlayer = new DataProviderActivePlayer();
        private DataProviderChampionDetails dataProviderChampionDetails = new DataProviderChampionDetails();
        private DataProviderPlayerList dataProviderPlayerList = new DataProviderPlayerList();

        public DataProviderVersions GetVersion()
        {
            return dataProviderVersions;
        }

        public DataProviderChampions GetChampions()
        {
            return dataProviderChampions;
        }
        public DataProviderActivePlayer GetActivePlayer()
        {
            return dataProviderActivePlayer;
        }

        public DataProviderChampionDetails GetChampionDetails()
        {
            return dataProviderChampionDetails;
        }

        public DataProviderPlayerList GetPlayerList()
        {
            return dataProviderPlayerList;
        }


        private bool localClientMock = true;
        public bool GetUseLocalClientMock()
        {
            return this.localClientMock;
        }
        public void SetUseLocalClientMock(bool state)
        {
            this.localClientMock = state;
            this.dataProviderActivePlayer.useMocks = state;
            this.dataProviderPlayerList.useMocks = state;
        }


        private bool dataDragonMock = false;
        public bool GetUseDataDragonMocks()
        {
            return this.dataDragonMock;
        }
        public void SetUseDataDragonMocks(bool state)
        {
            this.dataDragonMock = state;
            this.dataProviderVersions.useMocks = state;
            this.dataProviderChampions.useMocks = state;
            this.dataProviderChampionDetails.useMocks = state;
        }

        private DateTime loadMainDataStartTime;
        private DateTime loadLiveDataStartTime;


        public DataFetcher()
        {
            // 1st Stream
            this.dataProviderVersions.OnData += this.VersionLoaded;
            this.dataProviderChampions.OnData += this.ChampionsLoaded;

            // 2nd Stream
            this.dataProviderActivePlayer.OnData += this.ActivePlayerLoaded;
            this.dataProviderPlayerList.OnData += this.PlayerListLoaded;
            this.dataProviderChampionDetails.OnData += this.ChampionDetailsLoaded;            
        }

        // 1st Stream
        // ###########################################################################
        public void LoadMainData()
        {
            this.loadMainDataStartTime = DateTime.Now;
            Logger.Instance.Debug("LoadMainData");
            this.dataProviderVersions.LoadData();
        }
        private void VersionLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("LoadMainData VersionLoaded");
            this.dataProviderChampions.currentVersion = this.dataProviderVersions.getCurrentVersion();
            this.dataProviderChampions.LoadData();
        }
        private void ChampionsLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("LoadMainData ChampionsLoaded");
            this.OnMainDataLoaded(new DataFetcherLoadedEventArgs(this.loadMainDataStartTime));
        }

        private void OnMainDataLoaded(DataFetcherLoadedEventArgs e)
        {
            Logger.Instance.Debug("LoadMainData DONE!");
            EventHandler<DataFetcherLoadedEventArgs> handler = OnMainData;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<DataFetcherLoadedEventArgs> OnMainData;


        // 2st Stream
        // ###########################################################################
        private bool loadLoveDataInProgress = false;

        public void LoadLiveData()
        {
            if (!loadLoveDataInProgress)
            {
                this.loadLoveDataInProgress = true;

                this.loadLiveDataStartTime = DateTime.Now;
                Logger.Instance.Debug("LoadLiveData");
                this.dataProviderActivePlayer.LoadData();
            }     
        }

        private void ActivePlayerLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            
            if (!dataProviderActivePlayer.loadHasErrors())
            {
                Logger.Instance.Debug("LoadLiveData ActivePlayerLoaded");
                this.dataProviderPlayerList.LoadData();
            }
            else
            {
                this.OnLiveDataLoadError(new DataFetcherLoadedEventArgs(this.loadLiveDataStartTime));
            }

        }

      
        private void PlayerListLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            if (!dataProviderPlayerList.loadHasErrors()) { 
                Logger.Instance.Debug("LoadLiveData PlayerListLoaded");
                string summonerName = this.dataProviderActivePlayer.summonerName;
                string championName = this.dataProviderPlayerList.GetChampionNameBySummonerName(summonerName);
                string championId = this.dataProviderChampions.GetChampionIDbyName(championName);
                this.dataProviderChampionDetails.championId = championId;
                this.dataProviderChampionDetails.currentVersion = this.dataProviderVersions.getCurrentVersion();
                this.dataProviderChampionDetails.LoadData();
            }
            else
            {
                this.OnLiveDataLoadError(new DataFetcherLoadedEventArgs(this.loadLiveDataStartTime));
            }
        }

        private void ChampionDetailsLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            if (!dataProviderChampionDetails.loadHasErrors())
            {
                Logger.Instance.Debug("LoadLiveData ChampionDetailsLoaded");

                // Merge cooldowns from ChampionDetails to Abilities
                foreach (ChampionSpell spell in dataProviderChampionDetails.championSpells)
                {
                    foreach (Skill skill in dataProviderActivePlayer.abilities.getSkillsAsList())
                    {
                        if (skill.id == spell.id)
                        {
                            skill.cooldown = spell.cooldown;
                            skill.image = spell.spellImage;
                        }
                    }
                }
                this.OnLiveDataLoaded(new DataFetcherLoadedEventArgs(this.loadLiveDataStartTime));
            }
            else
            {
                this.OnLiveDataLoadError(new DataFetcherLoadedEventArgs(this.loadLiveDataStartTime));
            }
            
        }

        private void OnLiveDataLoaded(DataFetcherLoadedEventArgs e)
        {
            this.loadLoveDataInProgress = false;
            Logger.Instance.Debug("LoadLiveData DONE!");
            EventHandler<DataFetcherLoadedEventArgs> handler = OnLiveData;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<DataFetcherLoadedEventArgs> OnLiveData;

        private void OnLiveDataLoadError(DataFetcherLoadedEventArgs e)
        {
            this.loadLoveDataInProgress = false;
            Logger.Instance.Debug("LoadLiveData ERROR!");
            EventHandler<DataFetcherLoadedEventArgs> handler = OnLiveDataError;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<DataFetcherLoadedEventArgs> OnLiveDataError;
        


    }
}
