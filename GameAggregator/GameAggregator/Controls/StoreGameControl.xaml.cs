using GameAggregator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameAggregator.Controls
{
    /// <summary>
    /// Interaction logic for ShopAggregatorUserControl.xaml
    /// </summary>
    public partial class StoreGameControl : UserControl
    {
        public StoreGameControl(IStoreGame game)
        {
            InitializeComponent();

            Dictionary<Launchers, BitmapSource> bsDict = new Dictionary<Launchers, BitmapSource>()
            {
                { Launchers.EpicGames, GetBitmapISource(Launchers.EpicGames) },
                { Launchers.Origin, GetBitmapISource(Launchers.Origin) },
                { Launchers.Steam, GetBitmapISource(Launchers.Steam) },
                { Launchers.Uplay, GetBitmapISource(Launchers.Uplay) },
                { Launchers.Other, GetBitmapISource(Launchers.Other) }
            };

            tbGameName.Text = game.Name;
            tbGamePrice.Text = game.Price.ToString() + " ₽";
            imgClientIcon.Source = bsDict[game.Launcher];
            btnGoTOStore.Click += (o, args) => Process.Start(game.Link);
        }

        private BitmapSource GetBitmapISource(Launchers launcher)
        {
            Bitmap img;
            switch (launcher)
            {
                case Launchers.EpicGames: img = Properties.Resources.EpicGamesIcon; break;
                case Launchers.Origin: img = Properties.Resources.OriginIcon; break;
                case Launchers.Steam: img = Properties.Resources.SteamIcon; break;
                case Launchers.Uplay: img = Properties.Resources.UplayIcon; break;
                default: img = Properties.Resources.Other; break;
            }

            return Imaging.CreateBitmapSourceFromHBitmap(img.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //BitmapImage bitmapImage = new BitmapImage();
            //using (MemoryStream memory = new MemoryStream())
            //{
            //    bitmap.Save(memory, ImageFormat.Png);
            //    memory.Position = 0;
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = memory;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();
            //}
            //return bitmapImage;
        }
    }
}
