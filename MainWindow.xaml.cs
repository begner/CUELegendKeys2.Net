using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Globalization;

namespace CueLegendKey2
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        private Timer aTimer;

        private DataProviderVersions dataProviderVersions = new DataProviderVersions();
        private DataProviderChampions dataProviderChampions = new DataProviderChampions();
        private DataProviderActivePlayer dataProviderActivePlayer = new DataProviderActivePlayer();
        private DataProviderChampionDetails dataProviderChampionDetails = new DataProviderChampionDetails();
        private DataProviderPlayerList dataProviderPlayerList = new DataProviderPlayerList();
        private bool liveLoadInProgress = false;

        public MainWindow()
        {
            InitializeComponent();
            textBlockResponseTime.Text = "";
            textBlockRequestTime.Text = "";
            textBlockPlayer.Text = "";
            textBlockAbilities.Text = "";

            aTimer = new System.Timers.Timer();
            setTimerInterval(2000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            dataProviderVersions.OnData += this.VersionLoaded;
            dataProviderChampions.OnData += this.ChampionsLoaded;
            dataProviderActivePlayer.OnData += this.ActivePlayerLoaded;
            dataProviderPlayerList.OnData += this.PlayerListLoaded;
            dataProviderChampionDetails.OnData += this.ChampionDetailsLoaded;


            dataProviderVersions.LoadData();


        }

        public void VersionLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("VersionLoaded");
            dataProviderChampions.currentVersion = dataProviderVersions.getCurrentVersion();
            dataProviderChampions.LoadData();
        }
        public void ChampionsLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("ChampionsLoaded");
            this.LoadLiveData();
        }



        public void LoadLiveData()
        {
            if (!liveLoadInProgress)
            {
                liveLoadInProgress = true;
                Logger.Instance.Debug("LoadLiveData");
                dataProviderActivePlayer.LoadData();
            }
        }

        public void ActivePlayerLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("ActivePlayerLoaded");
            dataProviderPlayerList.LoadData();
        }

        public void PlayerListLoaded(object sender, DataProviderLoadedEventArgs e) 
        { 
            Logger.Instance.Debug("PlayerListLoaded");
            string championName = dataProviderPlayerList.GetChampionNameBySummonerName(dataProviderActivePlayer.summonerName);
            string championId = dataProviderChampions.GetChampionIDbyName(championName);
            dataProviderChampionDetails.championId = championId;
            dataProviderChampionDetails.currentVersion = dataProviderVersions.getCurrentVersion();
            dataProviderChampionDetails.LoadData();
        }

        public void ChampionDetailsLoaded(object sender, DataProviderLoadedEventArgs e)
        {
            Logger.Instance.Debug("ChampionDetailsLoaded");
            // Merge cooldowns from ChampionDetails to Abilities
            foreach (ChampionSpell spell in dataProviderChampionDetails.championSpells)
            {
                foreach (Skill skill in dataProviderActivePlayer.abilities.getSkillsAsList())
                {
                    if (skill.id == spell.id)
                    {
                        skill.cooldown = spell.cooldown;
                    }
                }
            }
            liveLoadInProgress = false;
            this.UpdateUI();
        }

        void setTimerInterval(double interval)
        {
            if (interval < 250)
            {
                interval = 250;
            }
            aTimer.Interval = interval;
            updateIntervalInput.Text = interval.ToString();
        }


        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.textBlockRequestTime.Text = e.SignalTime.ToString();
                this.textBlockResponseTime.Text = "...";
            }));
            this.LoadLiveData();
        }

        void UpdateUI()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.textBlockResponseTime.Text = DateTime.Now.ToString();
                this.textBlockPlayer.Text = dataProviderPlayerList.GetPlayerBySummonerName(dataProviderActivePlayer.summonerName).ToString();

                this.textBlockAbilities.Text = dataProviderActivePlayer.abilities.ToStringWithLiveCooldown(dataProviderActivePlayer.playerStats.abilityHaste);
                this.textBlockPlayerStats.Text = dataProviderActivePlayer.playerStats.ToString();

                this.progressHealth.Maximum = dataProviderActivePlayer.playerStats.maxHealth;
                this.progressHealth.Value = dataProviderActivePlayer.playerStats.currentHealth;

                this.progressRessource.Maximum = dataProviderActivePlayer.playerStats.resourceMax;
                this.progressRessource.Value = dataProviderActivePlayer.playerStats.resourceValue;
                this.progressRessource.Foreground = dataProviderActivePlayer.playerStats.getResourceColor();
            }));
            
        }
        

        private void saveUpdateIntervalClick(object sender, RoutedEventArgs e)
        {
            Logger.Instance.Debug("saveUpdateIntervalClick");
            string stringValue = updateIntervalInput.Text;
            
            double defaultValue = 2000;
            
            double doubleValue = 0;

            //Try parsing in the current culture
            if (!double.TryParse(stringValue, System.Globalization.NumberStyles.Any, CultureInfo.CurrentCulture, out doubleValue) &&
                //Then try in US english
                !double.TryParse(stringValue, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out doubleValue) &&
                //Then in neutral language
                !double.TryParse(stringValue, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
            {
                doubleValue = defaultValue;
            }

            this.setTimerInterval(doubleValue);

        }
    }

  

}

