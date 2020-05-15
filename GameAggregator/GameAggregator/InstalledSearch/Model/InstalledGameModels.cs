using GameAggregator.SteamStore;

namespace GameAggregator.InstalledSearch
{
    /// <summary>
    /// Модель установленной игры Steam
    /// Можно запустить, используя метод Steam.LaunchGame, передав параметр Appid
    /// </summary>
    public class Steam_InstalledGame : SteamGame
    {
        /// <summary>Расположение установки</summary>
        public string Directory { get; set; }

        /// <summary>
        /// Информация об установленной игре Uplay
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="directory">Расположение установки</param>
        /// <param name="appid">AppID</param>
        public Steam_InstalledGame(string appid, string name, string directory)
        {
            Appid = appid;
            Name = name;
            Directory = directory;
        }
    }

    public class Origin_InstalledGame
    {
        /// <summary>Название игры</summary>
        public string Name { get; set; }

        /// <summary>Расположение установки</summary>
        public string Directory { get; set; }

        /// <summary>(Вероятный) Запускающий exe-файл</summary>
        public string LaunchExe { get; set; }

        /// <summary>
        /// Информация об установленной игре origin
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="directory">Расположение установки</param>
        public Origin_InstalledGame(string name, string directory)
        {
            Name = name;
            Directory = directory;
            LaunchExe = directory + name + ".exe";
        }
    }

    public class Uplay_InstalledGame
    {
        /// <summary>Название игры</summary>
        public string Name { get; set; }

        /// <summary>Расположение установки</summary>
        public string Directory { get; set; }

        /// <summary>AppID игры</summary>
        public string Appid { get; set; }

        /// <summary>
        /// Информация об установленной игре Uplay
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="directory">Расположение установки</param>
        /// <param name="appid">AppID</param>
        public Uplay_InstalledGame(string name, string directory, string appid)
        {
            Name = name;
            Directory = directory;
            Appid = appid;
        }
    }
}
