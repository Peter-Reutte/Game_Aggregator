using GameAggregator.SteamStore;

namespace GameAggregator.InstalledSearch
{
    /// <summary>
    /// Модель установленной игры Steam. 
    /// Можно запустить, используя метод Steam.LaunchGame, передав параметр Appid
    /// </summary>
    public class Steam_InstalledGame : SteamGame
    {
        public string Directory { get; set; }

        public Steam_InstalledGame(int appid, string name, string directory)
        {
            Appid = appid;
            Name = name;
            Directory = directory;
        }
    }
}
