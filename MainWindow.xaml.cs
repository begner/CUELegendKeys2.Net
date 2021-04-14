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
        private Timer reloadLiveDataTimer;

        private int reloadTimerIntervalDefault = 2000;
        private int reloadTimerMin = 150;

        DataFetcher dataFetcher = new DataFetcher();

        public MainWindow()
        {
            InitializeComponent();

            this.dataFetcher.OnMainData += this.MainDataLoaded;
            this.dataFetcher.OnLiveData += this.LiveDataLoaded;
            this.dataFetcher.OnLiveDataError += this.LiveDataError;

            this.uiUseLocalClientMock.IsChecked = this.dataFetcher.GetUseLocalClientMock();
            this.uiUseDataDragonMock.IsChecked = this.dataFetcher.GetUseDataDragonMocks();

            this.dataFetcher.LoadMainData();
        }

        public void MainDataLoaded(object sender, DataFetcherLoadedEventArgs e)
        {
            this.uiLolApiVersion.Text = this.dataFetcher.GetVersion().getCurrentVersion();
            this.uiChampionCount.Text = this.dataFetcher.GetChampions().getChampionCount().ToString();

            // Load first time...
            this.dataFetcher.LoadLiveData();

            // Start Reload Timer
            this.reloadLiveDataTimer = new System.Timers.Timer();
            setReloadLiveDataTimerInterval(this.reloadTimerIntervalDefault);
            this.reloadLiveDataTimer.Elapsed += this.ReloadLiveData;
            this.reloadLiveDataTimer.AutoReset = true;
            this.reloadLiveDataTimer.Enabled = true;
        }

        private void ReloadLiveData(Object source, System.Timers.ElapsedEventArgs e)
        {
            this.dataFetcher.LoadLiveData();
        }

        public void LiveDataLoaded(object sender, DataFetcherLoadedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.uiRequestTime.Text = e.loadStartTime.ToString();
                this.uiResponseDuration.Text = e.getRequestDurationInMs().ToString() + "ms";
                this.uiGameState.Text = "GAME IN PROGRESS";

                string summonerName = this.dataFetcher.GetActivePlayer().summonerName;
                Player playerFromPlayerList = this.dataFetcher.GetPlayerList().GetPlayerBySummonerName(summonerName);

                this.uiPlayer.Text = playerFromPlayerList.ToString();

                DataProviderActivePlayer activePlayer = this.dataFetcher.GetActivePlayer();
                PlayerState playerStats = activePlayer.playerStats;

                double abilityHaste = playerStats.abilityHaste;
                this.uiAbilities.Text = activePlayer.abilities.ToStringWithLiveCooldown(abilityHaste);
                this.uiPlayerStats.Text = playerStats.ToString();

                this.uiProgressHealth.Maximum = playerStats.maxHealth;
                this.uiProgressHealth.Value = playerStats.currentHealth;

                this.uiProgressRessource.Maximum = playerStats.resourceMax;
                this.uiProgressRessource.Value = playerStats.resourceValue;
                this.uiProgressRessource.Foreground = playerStats.getResourceColor();

                if (activePlayer.abilities.Passive.image != null)
                {
                    this.uiImageSpellPassive.Source = activePlayer.abilities.Passive.image.GetImageAsBitmap();
                }

                if (activePlayer.abilities.Q.image != null)
                {
                    this.uiImageSpellQ.Source = activePlayer.abilities.Q.image.GetImageAsBitmap();
                }

                if (activePlayer.abilities.W.image != null) {
                    this.uiImageSpellW.Source = activePlayer.abilities.W.image.GetImageAsBitmap();
                }

                if (activePlayer.abilities.E.image != null)
                {
                    this.uiImageSpellE.Source = activePlayer.abilities.E.image.GetImageAsBitmap();
                }

                if (activePlayer.abilities.R.image != null)
                {
                    this.uiImageSpellR.Source = activePlayer.abilities.R.image.GetImageAsBitmap();
                }

                SummonerSpell s1 = this.dataFetcher.GetSummonerSpells().GetSpellByName(playerFromPlayerList.summonerSpells.summonerSpellOne.displayName);
                if (s1.image != null)
                {
                    this.uiImageSummonerSpell1.Source = s1.image.GetImageAsBitmap();
                }

                SummonerSpell s2 = this.dataFetcher.GetSummonerSpells().GetSpellByName(playerFromPlayerList.summonerSpells.summonerSpellTwo.displayName);
                if (s2.image != null)
                {
                    this.uiImageSummonerSpell2.Source = s2.image.GetImageAsBitmap();
                }





                ChampionInfo championInfo = this.dataFetcher.GetChampionDetails().championInfo;

                this.uiChampName.Text = championInfo.name;
                this.uiChampTitle.Text = championInfo.title;
                if (championInfo.image != null)
                {
                    this.uiChampionImage.Source = championInfo.image.GetImageAsBitmap();
                }


            }));
        }

        public void LiveDataError(object sender, DataFetcherLoadedEventArgs e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.uiRequestTime.Text = e.loadStartTime.ToString();
                this.uiResponseDuration.Text = e.getRequestDurationInMs().ToString() + "ms";
                
                this.uiGameState.Text = "GAME NOT STARTED";

                this.uiPlayer.Text = "";

                this.uiAbilities.Text = "";
                this.uiPlayerStats.Text = "";

                this.uiProgressHealth.Maximum = 1;
                this.uiProgressHealth.Value = 0;

                this.uiProgressRessource.Maximum = 1;
                this.uiProgressRessource.Value = 0;

                this.uiImageSpellQ.Source = null;
                this.uiImageSpellW.Source = null;
                this.uiImageSpellE.Source = null;
                this.uiImageSpellR.Source = null;
                this.uiChampionImage.Source = null;

                this.uiChampName.Text = "";
                this.uiChampTitle.Text = "";
            }));
        }

        void setReloadLiveDataTimerInterval(double interval)
        {
            if (interval < this.reloadTimerMin)
            {
                interval = this.reloadTimerMin;
            }
            this.reloadLiveDataTimer.Interval = interval;
            this.uiUpdateIntervalInput.Text = interval.ToString();
        }

        private void uiSaveUpdateIntervalClick(object sender, RoutedEventArgs e)
        {
            Logger.Instance.Debug("saveUpdateIntervalClick");
            string stringValue = this.uiUpdateIntervalInput.Text;
            
            double defaultValue = this.reloadTimerIntervalDefault;
            
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

            this.setReloadLiveDataTimerInterval(doubleValue);
        }

        private void uiUseLocalClientMockChecked(object sender, RoutedEventArgs e)
        {
            this.dataFetcher.SetUseLocalClientMock(true);
        }

        private void uiUseLocalClientMockUnchecked(object sender, RoutedEventArgs e)
        {
            this.dataFetcher.SetUseLocalClientMock(false);
        }

        private void uiUseDataDragonMockChecked(object sender, RoutedEventArgs e)
        {
            this.dataFetcher.SetUseDataDragonMocks(true);
        }

        private void uiUseDataDragonMockUnchecked(object sender, RoutedEventArgs e)
        {
            this.dataFetcher.SetUseDataDragonMocks(false);
        }


        private bool showDebugPane = false;
        private void uiSwapGridPanels(object sender, RoutedEventArgs e)
        {
            int animationDurationMs = 500;

            GridLengthAnimation close = new GridLengthAnimation();
            close.From = new GridLength(0, GridUnitType.Star);
            close.To = new GridLength(0, 0);
            close.Duration = new TimeSpan(0, 0, 0, 0, animationDurationMs);

            GridLengthAnimation open = new GridLengthAnimation();
            open.From = new GridLength(0, 0);
            open.To = new GridLength(0, GridUnitType.Star);
            open.Duration = new TimeSpan(0, 0, 0, 0, animationDurationMs);

            mainGrid.ColumnDefinitions[1].BeginAnimation(ColumnDefinition.WidthProperty, (showDebugPane ? close : open));
            mainGrid.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, (showDebugPane ? open : close));
            showDebugPane = !showDebugPane;
        }
    }

  

}

