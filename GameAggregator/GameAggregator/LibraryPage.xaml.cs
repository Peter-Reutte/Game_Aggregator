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
using GameAggregator.Models;
using GameAggregator.OriginStore;
using GameAggregator.SteamStore;
using GameAggregator.UplayStore;

namespace GameAggregator
{
    /// <summary>
    /// Логика взаимодействия для LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPage()
        {
            InitializeComponent();
            List<IInstalledGame> games = GetAllGames();
            foreach(IInstalledGame game in games)
            {
                spInstalledGames.Children.Add(new LibraryItem(game));
            }
        }

        public List<IInstalledGame> GetAllGames()
        {
            List<IInstalledGame> installedGames = new List<IInstalledGame>();
            try
            {
                installedGames.AddRange(EpicGames.Search_EpicGamesInstalled());
            }
            catch { }
            try
            {
                installedGames.AddRange(Origin.Search_OriginInstalled());
            }
            catch { }
            try
            {
                installedGames.AddRange(Steam.Search_SteamInstalled());
            }
            catch { }
            try
            {
                installedGames.AddRange(Uplay.Search_UplayInstalled());
            }
            catch { }

            installedGames.Sort((g1, g2) => g1.Name.CompareTo(g2.Name));
            return installedGames;
        }
    }

    public class Game
    {
        public string Name { get; set; }
        public Image IconClient { get; set; }
    }
}
