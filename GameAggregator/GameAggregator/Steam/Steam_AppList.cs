using System.Collections.Generic;

namespace GameAggregator
{
    public class SteamGame
    {
        /// <summary>
        /// AppID игры
        /// </summary>
        public int Appid { get; set; }

        /// <summary>
        /// Наименование игры
        /// </summary>
        public string Name { get; set; }
    }

    public class Applist
    {
        public List<SteamGame> Apps { get; set; }
    }

    public class Steam_AppList
    {
        public Applist Applist { get; set; }
    }
}
