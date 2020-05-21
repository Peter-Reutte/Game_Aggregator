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
        /// <summary>
        /// Страница "Библиотека"
        /// </summary>
        /// <param name="accounts">Привязанные аккаунты</param>
        public LibraryPage(Accounts accounts)
        {
            InitializeComponent();
            List<IInstalledGame> games = GetInstalledGames();
            foreach (IInstalledGame game in games)
            {
                spGames.Children.Add(new LibraryItem(game));
            }

            List<ILibraryGame> library = GetLibraryGames(accounts);
            foreach (ILibraryGame game in library)
            {
                if (games.Find(x => x.Name == game.Name) == null)
                {
                    spGames.Children.Add(new LibraryItem(game));
                }
            }
        }

        /// <summary>
        /// Получение списка установленных на компьютере игр
        /// </summary>
        /// <returns>Список установленных игр</returns>
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
        /// <param name="accounts">Привязанные аккаунты</param>
        /// <returns>Список игр в библиотеках</returns>
        public List<ILibraryGame> GetLibraryGames(Accounts accounts)
        {
            List<ILibraryGame> libraryGames = new List<ILibraryGame>();
            try
            {
                if (accounts.SteamProfileLink != null)
                {
                    Steam steam = new Steam();
                    libraryGames.AddRange(steam.GetOwnedGamesList(accounts.SteamProfileLink));
                }
            }
            catch { }

            libraryGames.Sort((g1, g2) => g1.Name.CompareTo(g2.Name));
            return libraryGames;
        }
    }
}
