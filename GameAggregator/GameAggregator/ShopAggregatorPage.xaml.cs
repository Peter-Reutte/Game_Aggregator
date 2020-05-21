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
        public ShopAggregatorPage()
        {
            InitializeComponent();

            bwSearcher = new BackgroundWorker();
            bwSearcher.DoWork += (o, args) =>
            {
                string keyword = args.Argument as string;
                var games = GetGames(keyword);
                args.Result = games;
            };
            bwSearcher.RunWorkerCompleted += (o, args) =>
            {
                List<IStoreGame> games = args.Result as List<IStoreGame>;
                lvStoreGames.ItemsSource = games.Select(x => new StoreGameControl(x));
                this.IsEnabled = true;
            };
        }

        public List<IStoreGame> GetGames(string keyword)
        {
            List<IStoreGame> games = new List<IStoreGame>();

            EpicGames epicGames = new EpicGames();
            Steam steam = new Steam();
            Origin origin = new Origin();

            try
            {
                games.AddRange(epicGames.GetStoreGames(keyword: keyword));
            }
            catch { }
            try
            {
                games.AddRange(origin.GetGamePrice(keyword));
            }
            catch { }
            try
            {
                games.AddRange(steam.GetGamePrice(keyword));
            }
            catch { }
            return games;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            bwSearcher.RunWorkerAsync(tbSearchString.Text);
        }
    }
}
