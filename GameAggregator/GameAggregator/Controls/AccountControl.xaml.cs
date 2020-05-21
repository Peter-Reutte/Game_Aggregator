using GameAggregator.Models;
using GameAggregator.SteamStore;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GameAggregator.Controls
{
    /// <summary>
    /// Логика взаимодействия для AccountControl.xaml
    /// </summary>
    public partial class AccountControl : UserControl
    {
        readonly Launchers Launcher;

        public AccountControl(Launchers launcher, string link)
        {
            InitializeComponent();
            grView.Visibility = Visibility.Visible;
            grAdd.Visibility = Visibility.Hidden;
            Launcher = launcher;

            Steam steam = new Steam();
            try
            {
                lbName.Content = steam.GetUserInfo(link);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            imgLauncher.Source = GetImageStream(GetLauncherImage(launcher));
        }

        public AccountControl(Launchers launcher)
        {
            InitializeComponent();
            grView.Visibility = Visibility.Hidden;
            grAdd.Visibility = Visibility.Visible;
            Launcher = launcher;
            imgALauncher.Source = GetImageStream(GetLauncherImage(launcher));
            
        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите отвязать аккаунт?", "Внимание", 
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(appDataPath, "steam.txt"), false);
                sw.Close();

                grView.Visibility = Visibility.Hidden;
                grAdd.Visibility = Visibility.Visible;
                imgALauncher.Source = GetImageStream(GetLauncherImage(Launcher));

            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Steam steam = new Steam();
            try
            {
                string username = steam.GetUserInfo(tbLink.Text);
                lbName.Content = username;
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(appDataPath, "steam.txt"), false);
                sw.Write(tbLink.Text);
                sw.Close();
                grAdd.Visibility = Visibility.Hidden;
                grView.Visibility = Visibility.Visible;
                imgLauncher.Source = GetImageStream(GetLauncherImage(Launcher));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        /// <summary>
        /// Получение логотипа лаунчера
        /// </summary>
        /// <param name="launcher">Лаунчер</param>
        /// <returns>Логотип лаунчера</returns>
        private System.Drawing.Image GetLauncherImage(Launchers launcher)
        {
            if (launcher == Launchers.EpicGames)
                return Properties.Resources.EpicGamesIcon;
            else if (launcher == Launchers.Origin)
                return Properties.Resources.OriginIcon;
            else if (launcher == Launchers.Steam)
                return Properties.Resources.SteamIcon;
            else if (launcher == Launchers.Uplay)
                return Properties.Resources.UplayIcon;
            else return Properties.Resources.Other;
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        public static BitmapSource GetImageStream(System.Drawing.Image image)
        {
            var bitmap = new System.Drawing.Bitmap(image);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }
    }
}
