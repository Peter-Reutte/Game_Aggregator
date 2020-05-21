using GameAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace GameAggregator.Controls
{
    /// <summary>
    /// Логика взаимодействия для LibraryItem.xaml
    /// </summary>
    public partial class LibraryItem : UserControl
    {
        private readonly IInstalledGame InstalledGame;

        public LibraryItem(IInstalledGame installedGame)
        {
            InitializeComponent();
            InstalledGame = installedGame;
            lbName.Content = installedGame.Name;
            System.Drawing.Image launcherIcon = null;
            if (installedGame.Launcher == Launchers.EpicGames)
                launcherIcon = Properties.Resources.EpicGamesIcon;
            else if (installedGame.Launcher == Launchers.Origin)
                launcherIcon = Properties.Resources.OriginIcon;
            else if (installedGame.Launcher == Launchers.Steam)
                launcherIcon = Properties.Resources.SteamIcon;
            else if (installedGame.Launcher == Launchers.Uplay)
                launcherIcon = Properties.Resources.UplayIcon;
            else launcherIcon = Properties.Resources.Other;

            imgLauncher.Source = GetImageStream(launcherIcon);
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e) => InstalledGame.LaunchGame();

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
