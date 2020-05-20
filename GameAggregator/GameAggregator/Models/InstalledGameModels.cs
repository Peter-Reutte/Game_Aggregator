using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace GameAggregator
{
    public enum Launchers
    {
        Steam,
        EpicGames,
        Origin,
        Uplay,
        Other
    }

    /// <summary>Описание установленной игры</summary>
    public interface IInstalledGame
    {
        /// <summary>Название игры</summary>
        string Name { get; set; }

        /// <summary>Строка для запуска игры</summary>
        string LaunchString { get; set; }

        /// <summary>Лаунчер, в котором приобретена игра</summary>
        Launchers Launcher { get; set; }

        /// <summary>Запуск установленной игры</summary>
        void LaunchGame();
    }

    public class Steam_InstalledGame : IInstalledGame
    {
        public string Name { get; set; }
        public string LaunchString { get; set; }
        public Launchers Launcher { get; set; }

        public Steam_InstalledGame(string name, string launchString, Launchers launcher)
        {
            Name = name;
            LaunchString = launchString;
            Launcher = launcher;
        }

        public void LaunchGame()
        {
            try
            {
                Process.Start(LaunchString);
            }
            catch
            {
                MessageBox.Show("Клиент Steam не установлен!");
            }
        }
    }

    public class Origin_InstalledGame : IInstalledGame
    {
        public string Name { get; set; }
        public string LaunchString { get; set; }
        public Launchers Launcher { get; set; }

        public Origin_InstalledGame(string name, string launchString, Launchers launcher)
        {
            Name = name;
            LaunchString = launchString;
            Launcher = launcher;
        }

        public void LaunchGame()
        {
            try
            {
                Process.Start(LaunchString);
            }
            catch //Если exe-файл игры нестандартный, запускается сам Origin; если Origin не установлен -- ничего
            {
                string regkey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(regkey);
                foreach (string ksubKey in key.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key.OpenSubKey(ksubKey))
                    {
                        string publisher = "";
                        try
                        {
                            publisher = subKey.GetValue("Publisher").ToString();
                            if (!publisher.Contains("Electronic Arts")) continue;
                            string title = subKey.GetValue("DisplayName").ToString();
                            string installLocation = subKey.GetValue("InstallLocation").ToString();
                            if (title == "Origin")
                            {
                                Process.Start(installLocation + "Origin.exe");
                                break;
                            }
                        }
                        catch { continue; }
                    }
                }
            }
        }
    }

    public class Uplay_InstalledGame : IInstalledGame
    {
        public string Name { get; set; }
        public string LaunchString { get; set; }
        public Launchers Launcher { get; set; }

        public Uplay_InstalledGame(string name, string launchString, Launchers launcher)
        {
            Name = name;
            LaunchString = launchString;
            Launcher = launcher;
        }

        public void LaunchGame()
        {
            try
            {
                Process.Start(LaunchString);
            }
            catch
            {
                MessageBox.Show("Клиент Uplay не установлен!");
            }
        }
    }

    public class EpicGames_InstalledGame : IInstalledGame
    {
        public string Name { get; set; }
        public string LaunchString { get; set; }
        public Launchers Launcher { get; set; }

        public EpicGames_InstalledGame(string name, string launchString)
        {
            Name = name;
            LaunchString = launchString;
            Launcher = Launchers.EpicGames;
        }

        public void LaunchGame()
        {
            try
            {
                Process.Start(LaunchString);
            }
            catch
            {
                MessageBox.Show("Клиент Epic Games не установлен!");
            }
        }
    }

}
