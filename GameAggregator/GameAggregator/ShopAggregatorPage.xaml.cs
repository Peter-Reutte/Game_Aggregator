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

namespace GameAggregator
{
    /// <summary>
    /// Логика взаимодействия для ShopAggregatorPage.xaml
    /// </summary>
    public partial class ShopAggregatorPage : Page
    {
        public ShopAggregatorPage()
        {
            InitializeComponent();
            var games = GetGames("");
            foreach(var game in games)
            {
                spStoreGamesList.Children.Add(new StoreGameControl(game));
            }
        }

        public List<IStoreGame> GetGames(string keyword)
        {
            List<IStoreGame> games = new List<IStoreGame>();

            EGames.EpicGames eg = new EGames.EpicGames();

            games.AddRange(eg.GetStoreGames(keyword: keyword));

            return games;
        }
    }
}
