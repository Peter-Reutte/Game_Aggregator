using GameAggregator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace GameAggregator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Frame MainFrame { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            MainFrame = mainFrame;
            MainFrame.Content = new HomePage();
        }

        private void BtnLibrary_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new LibraryPage(GetAccounts());
        }

        private void BtnShop_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new ShopAggregatorPage();
        }

        private void BtnManageAccounts_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new AccountsPage(GetAccounts());
        }

        public Accounts GetAccounts()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            DirectoryInfo di = Directory.CreateDirectory(appDataPath + "\\GameAggregator");
            StreamReader sr;
            try
            {
                sr = new StreamReader(System.IO.Path.Combine(di.FullName, "steam.txt"));
                string steamProfileLink = sr.ReadLine();
                sr.Close();
                if (Regex.Match(steamProfileLink, "https://steamcommunity.com/").Success)
                {
                    Accounts accounts = new Accounts
                    {
                        SteamProfileLink = steamProfileLink
                    };
                    return accounts;
                }
                else return new Accounts();
            }
            catch
            {
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(di.FullName, "steam.txt"), false);
                sw.WriteLine();
                sw.Close();
                return new Accounts();
            }
        }
    }
}
