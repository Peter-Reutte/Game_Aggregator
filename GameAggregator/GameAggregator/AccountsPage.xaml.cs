using GameAggregator.Controls;
using GameAggregator.Models;
using System.Windows;
using System.Windows.Controls;

namespace GameAggregator
{
    /// <summary>
    /// Логика взаимодействия для AccountsPage.xaml
    /// </summary>
    public partial class AccountsPage : Page
    {
        public AccountsPage(Accounts accounts)
        {
            InitializeComponent();
            //Скрываем кнопку "+ аккаунт", если аккаунт этой площадки привязан
            if (accounts.SteamProfileLink == null)
            {
                btnSteamAccount.Visibility = Visibility.Visible;
            }
            else
            {
                btnSteamAccount.Visibility = Visibility.Hidden;
                spAccounts.Children.Add(new AccountControl(Launchers.Steam, accounts.SteamProfileLink));
            }
        }

        private void BtnSteamAccount_Click(object sender, RoutedEventArgs e)
        {
            spAccounts.Children.Add(new AccountControl(Launchers.Steam));
            btnSteamAccount.Visibility = Visibility.Hidden;
        }
    }
}
