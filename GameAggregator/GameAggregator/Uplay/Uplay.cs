using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAggregator.UplayStore
{
    public class Uplay
    {
        /// <summary>
        /// Запускает выбранную игру
        /// </summary>
        /// <param name="game">AppID игры</param>
        public void LaunchGame(string appid) => Process.Start(@"uplay://launch/" + appid);
    }
}
