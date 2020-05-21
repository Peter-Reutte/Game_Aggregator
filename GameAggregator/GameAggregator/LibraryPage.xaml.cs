using System.Collections.Generic;
using System.Windows.Controls;
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
        public LibraryPage(string steam_acc)
        {
            InitializeComponent();
            List<IInstalledGame> games = GetInstalledGames();
            foreach (IInstalledGame game in games)
            {
                spGames.Children.Add(new LibraryItem(game));
            }

            List<ILibraryGame> library = GetLibraryGames(steam_acc);
            foreach (ILibraryGame game in library)
            {
                if (games.Find(x => x.Name == game.Name) == null)
                {
                    spGames.Children.Add(new LibraryItem(game));
                }
            }
        }

        public List<IInstalledGame> GetInstalledGames()
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

        /// <summary>
        /// Получение игр из библиотек аккаунтов
        /// </summary>
        /// <param name="steam_acc">Ссылка на аккаунт Steam (временно)</param>
        /// <returns>Список игр в библиотеках</returns>
        public List<ILibraryGame> GetLibraryGames(string steam_acc)
        {
            List<ILibraryGame> libraryGames = new List<ILibraryGame>();
            try
            {
                Steam steam = new Steam();
                libraryGames.AddRange(steam.GetOwnedGamesList(steam_acc));
            }
            catch { }

            libraryGames.Sort((g1, g2) => g1.Name.CompareTo(g2.Name));
            return libraryGames;
        }
    }
}
