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
        private readonly Accounts Account;

        /// <summary>
        /// Страница "Библиотека"
        /// </summary>
        /// <param name="accounts">Привязанные аккаунты</param>
        public LibraryPage(Accounts accounts)
        {
            InitializeComponent();
            Account = accounts;
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

        private void BtnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string search = tbSearchString.Text.ToLower().Trim();
            spGames.Children.Clear();

            List<IInstalledGame> games = GetInstalledGames();
            foreach (IInstalledGame game in games)
            {
                if (game.Name.ToLower().Contains(search))
                    spGames.Children.Add(new LibraryItem(game));
            }

            List<ILibraryGame> library = GetLibraryGames(Account);
            foreach (ILibraryGame game in library)
            {
                if (game.Name.ToLower().Contains(search))
                {
                    if (games.Find(x => x.Name == game.Name) == null)
                    {
                        spGames.Children.Add(new LibraryItem(game));
                    }
                }
            }
        }

        private void TbSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSearch_Click(sender, new System.Windows.RoutedEventArgs());
            }
        }
    }
}
