using GameAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameAggregator.Controls;
using GameAggregator.EGames;
using GameAggregator.SteamStore;
using GameAggregator.OriginStore;
using System.ComponentModel;

namespace GameAggregator
{
    /// <summary>
    /// Логика взаимодействия для ShopAggregatorPage.xaml
    /// </summary>
    public partial class ShopAggregatorPage : Page
    {
        private BackgroundWorker bwSearcher;
        private List<StoreGameControl> games;
        private IEnumerator<IStoreGame> gamesEnumerator;

        public ShopAggregatorPage()
        {
            InitializeComponent();
            InitBackgroundWorker();
        }

        private void InitBackgroundWorker()
        {
            bwSearcher = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            bwSearcher.DoWork += (o, args) =>
            {
                for (int i = 0; i < 15; i++)
                {
                    if (bwSearcher.CancellationPending)
                    {
                        args.Cancel = true;
                    }
                    if (!gamesEnumerator.MoveNext())
                        break;

                    bwSearcher.ReportProgress(i, gamesEnumerator.Current);
                }
            };

            bwSearcher.ProgressChanged += (o, args) =>
            {
                games.Add(new StoreGameControl(args.UserState as IStoreGame));
                lvStoreGames.Items.Refresh();
            };
            bwSearcher.RunWorkerCompleted += (o, args) =>
            {
                tbSearchString.IsEnabled = true;
                btnSearch.IsEnabled = true;
            };
        }

        public IEnumerable<IStoreGame> GetGame(string keyword)
        {
            EpicGames epicGames = new EpicGames();
            Steam steam = new Steam();
            Origin origin = new Origin();

            IEnumerator<IStoreGame> sEnumerator = steam.GetGamePrice(keyword).GetEnumerator();
            IEnumerator<IStoreGame> egEnumerator = epicGames.GetStoreGames(keyword: keyword).GetEnumerator();
            IEnumerator<IStoreGame> oEnumerator = origin.GetGamePrice(keyword).GetEnumerator();

            bool fEpicGames = true, fOrigin = true, fSteam = true;
            while (fEpicGames || fOrigin || fSteam)
            {
                try
                {
                    fSteam = sEnumerator.MoveNext();
                }
                catch
                {
                    fSteam = false;
                }
                if (fSteam)
                    yield return sEnumerator.Current;

                try
                {
                    fEpicGames = egEnumerator.MoveNext();
                }
                catch
                {
                    fEpicGames = false;
                }

                if (fEpicGames)
                    yield return egEnumerator.Current;

                try
                {
                    fOrigin = oEnumerator.MoveNext();
                }
                catch
                {
                    fOrigin = false;
                }

                if (fOrigin)
                    yield return oEnumerator.Current;
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            tbSearchString.IsEnabled = false;
            btnSearch.IsEnabled = false;
            //this.IsEnabled = false;
            bwSearcher.CancelAsync();

            games = new List<StoreGameControl>();
            lvStoreGames.ItemsSource = games;

            gamesEnumerator = GetGame(tbSearchString.Text).GetEnumerator();

            InitBackgroundWorker();
            bwSearcher.RunWorkerAsync();
        }

        private void LvStoreGames_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)e.OriginalSource;
            if (scrollViewer.ScrollableHeight != 0 && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                if(!bwSearcher.IsBusy)
                    bwSearcher.RunWorkerAsync();
            }
        }
    }
}
