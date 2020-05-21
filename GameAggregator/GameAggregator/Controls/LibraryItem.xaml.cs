using GameAggregator.Models;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GameAggregator.Controls
{
    /// <summary>
    /// Логика взаимодействия для LibraryItem.xaml
    /// </summary>
    public partial class LibraryItem : UserControl
    {
        private readonly IInstalledGame InstalledGame;

        /// <summary>
        /// Элемент интерфейса для установленной игры
        /// </summary>
        /// <param name="installedGame">Установленная игра</param>
        public LibraryItem(IInstalledGame installedGame)
        {
            InitializeComponent();
            InstalledGame = installedGame;
            lbName.Content = installedGame.Name;
            imgLauncher.Source = GetImageStream(GetLauncherImage(installedGame.Launcher));
        }

        /// <summary>
        /// Элемент интерфейса для игры из библиотеки
        /// </summary>
        /// <param name="libraryGame">Игра из библиотеки</param>
        public LibraryItem(ILibraryGame libraryGame)
        {
            InitializeComponent();
            lbName.Content = libraryGame.Name;
            imgLauncher.Source = GetImageStream(GetLauncherImage(libraryGame.Launcher));
            btnPlay.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Обработка команды запуска установленной игры
        /// </summary>
        private void BtnPlay_Click(object sender, RoutedEventArgs e) => InstalledGame.LaunchGame();

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
