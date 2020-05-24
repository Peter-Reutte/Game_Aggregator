using GameAggregator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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

            Dictionary<Launchers, BitmapSource> bsDict;
            MemoryCache memCache = MemoryCache.Default;
            string memKey = "launchers_icons_dict";
            if (!memCache.Contains(memKey))
            {
                bsDict = new Dictionary<Launchers, BitmapSource>()
                {
                    { Launchers.EpicGames, GetBitmapSource(Launchers.EpicGames) },
                    { Launchers.Origin, GetBitmapSource(Launchers.Origin) },
                    { Launchers.Steam, GetBitmapSource(Launchers.Steam) },
                    { Launchers.Uplay, GetBitmapSource(Launchers.Uplay) },
                    { Launchers.Other, GetBitmapSource(Launchers.Other) }
                };
                memCache.Set(memKey, bsDict, DateTimeOffset.Now.AddDays(7.0));
            }
            else
            {
                bsDict = memCache.Get(memKey) as Dictionary<Launchers, BitmapSource>;
            }

            tbGameName.Text = game.Name;
            tbGamePrice.Text = game.Price.ToString() + " ₽";
            imgClientIcon.Source = bsDict[game.Launcher];
            btnGoTOStore.Click += (o, args) => Process.Start(game.Link);
        }

        /// <summary>
        /// Получает BitmapSource из иконки лаунчера, лежащей в ресурсах
        /// </summary>
        /// <param name="launcher">Лаунчер для которого необходимо получить иконку</param>
        /// <returns></returns>
        private BitmapSource GetBitmapSource(Launchers launcher)
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
        }
    }
}
