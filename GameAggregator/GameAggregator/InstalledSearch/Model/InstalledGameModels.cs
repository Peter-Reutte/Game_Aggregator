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

    public class Origin_InstalledGame
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public string LaunchExe { get; set; }

        public Origin_InstalledGame(string name, string directory)
        {
            Name = name;
            Directory = directory;
            LaunchExe = directory + name + ".exe";
        }
    }

    public class Uplay_InstalledGame
    {
        public string Name { get; set; }
        public string Directory { get; set; }
        public string Appid { get; set; }

        public Uplay_InstalledGame(string name, string directory, string appid)
        {
            Name = name;
            Directory = directory;
            Appid = appid;
        }
    }
}
