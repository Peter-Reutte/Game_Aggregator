using System.Collections.Generic;

namespace GameAggregator.SteamStore
{
    public class OwnedGame : SteamGame
    {
        /// <summary>Время в игре</summary>
        public int Playtime_forever { get; set; }

        /// <summary>Часть URL картинки-иконки игры</summary>
        public string Img_icon_url { get; set; }

        /// <summary>Часть URL картинки-логотипа игры</summary>
        public string Img_logo_url { get; set; }

        // (для картинок: http://media.steampowered.com/steamcommunity/public/images/apps/<Appid>/<url_part>.jpg)

        public bool Has_community_visible_stats { get; set; }
        public int Playtime_windows_forever { get; set; }
        public int Playtime_mac_forever { get; set; }
        public int Playtime_linux_forever { get; set; }
        public int? Playtime_2weeks { get; set; }
    }

    public class Response
    {
        /// <summary>Количество игр на аккаунте</summary>
        public int Game_count { get; set; }

        /// <summary>Список игр</summary>
        public IList<OwnedGame> Games { get; set; }
    }

    /// <summary>Для получения библиотеки игр пользователя</summary>
    public class Steam_OwnedGameList
    {
        public Response Response { get; set; }
    }
}

