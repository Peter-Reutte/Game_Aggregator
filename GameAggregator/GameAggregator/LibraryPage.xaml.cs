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
using GameAggregator.EGames;
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
            //  Как добавить иконку лаунчера
            //List<IInstalledGame> games = GetAllGames();
            //foreach (IInstalledGame game in games)
            //{
            //    System.Drawing.Image launcherIcon;
            //    if (game.Launcher == Launchers.EpicGames)
            //    {
            //        launcherIcon = Properties.Resources.EpicGamesIcon;
            //    }
            //    else if (game.Launcher == Launchers.Origin)
            //    {
            //        launcherIcon = Properties.Resources.OriginIcon;
            //    }
            //    else if (game.Launcher == Launchers.Other)
            //    {
            //        launcherIcon = Properties.Resources.Other;
            //    }
            //    else if (game.Launcher == Launchers.Steam)
            //    {
            //        launcherIcon = Properties.Resources.SteamIcon;
            //    }
            //    else if (game.Launcher == Launchers.Uplay)
            //    {
            //        launcherIcon = Properties.Resources.UplayIcon;
            //    }
            //}
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
