using System.Collections.Generic;

namespace GameAggregator.SteamStore
{
    public class SteamGame
    {
        /// <summary>AppID игры</summary>
        public string Appid { get; set; }

        /// <summary>Наименование игры</summary>
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
