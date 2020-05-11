using System.Collections.Generic;

namespace GameAggregator
{
    public class OwnedGame
    {
        /// <summary>
        /// AppID игры
        /// </summary>
        public int Appid { get; set; }

        /// <summary>
        /// Наименование игры
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Время в игре
        /// </summary>
        public int Playtime_forever { get; set; }

        /// <summary>
        /// Часть URL картинки-иконки игры 
        /// (формат: http://media.steampowered.com/steamcommunity/public/images/apps/<Appid>/<Img_icon_url>.jpg)
        /// </summary>
        public string Img_icon_url { get; set; }

        /// <summary>
        /// Часть URL картинки-логотипа игры
        /// (формат: http://media.steampowered.com/steamcommunity/public/images/apps/<Appid>/<Img_logo_url>.jpg)
        /// </summary>
        public string Img_logo_url { get; set; }

        public bool Has_community_visible_stats { get; set; }
        public int Playtime_windows_forever { get; set; }
        public int Playtime_mac_forever { get; set; }
        public int Playtime_linux_forever { get; set; }
        public int? Playtime_2weeks { get; set; }
    }

    public class Response
    {
        /// <summary>
        /// Количество игр на аккаунте
        /// </summary>
        public int Game_count { get; set; }

        /// <summary>
        /// Список игр
        /// </summary>
        public IList<OwnedGame> Games { get; set; }
    }

    public class Steam_OwnedGameList
    {
        public Response Response { get; set; }
    }
}

