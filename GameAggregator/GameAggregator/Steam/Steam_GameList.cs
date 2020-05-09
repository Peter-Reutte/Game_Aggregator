using System.Collections.Generic;

namespace GameAggregator
{
    public class Game
    {
        public int Appid { get; set; }
        public string Name { get; set; }
        public int Playtime_forever { get; set; }
        public string Img_icon_url { get; set; }
        public string Img_logo_url { get; set; }
        public bool Has_community_visible_stats { get; set; }
        public int Playtime_windows_forever { get; set; }
        public int Playtime_mac_forever { get; set; }
        public int Playtime_linux_forever { get; set; }
        public int? Playtime_2weeks { get; set; }
    }

    public class Response
    {
        public int Game_count { get; set; }
        public IList<Game> Games { get; set; }
    }

    public class Steam_GameList
    {
        public Response Response { get; set; }
    }
}

