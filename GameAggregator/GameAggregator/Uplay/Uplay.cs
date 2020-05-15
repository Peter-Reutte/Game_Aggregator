using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameAggregator.UplayStore
{
    public class Uplay
    {
        /// <summary>
        /// Запускает выбранную игру
        /// </summary>
        /// <param name="appid">AppID игры</param>
        public void LaunchGame(string appid)
        {
            try
            {
                Process.Start(@"uplay://launch/" + appid);
            }
            catch
            {
                MessageBox.Show("Клиент Uplay не установлен!");
            }
        }
    }
}
