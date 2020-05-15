using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GameAggregator.InstalledSearch
{
    public static class InstalledGamesSearch
    {
        public static void SearchForinstalledGames()
        {
            //Search_SteamInstalled();
            //Search_OriginInstalled();
            //Search_UplayInstalled();
        }

        /// <summary>
        /// Поиск установленных игр Steam
        /// </summary>
        /// <returns>Список установленных игр, с AppID, названиями и директорией установки</returns>
        private static List<Steam_InstalledGame> Search_SteamInstalled()
        {
            string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
            string steam64path;
            string config64path;
            List<string> steamGameDirs = new List<string>();
            try
            {
                RegistryKey key64 = Registry.LocalMachine.OpenSubKey(steam64);
                foreach (string k64subKey in key64.GetSubKeyNames())
                {
                    using (RegistryKey subKey = key64.OpenSubKey(k64subKey))
                    {
                        steam64path = subKey.GetValue("InstallPath").ToString();
                        config64path = steam64path + @"\steamapps\libraryfolders.vdf";
                        string driveRegex = @"[A-Z]:\\";
                        if (File.Exists(config64path))
                        {
                            string[] configLines = File.ReadAllLines(config64path);
                            foreach (var line in configLines)
                            {
                                Match match = Regex.Match(line, driveRegex);
                                if (line != string.Empty && match.Success)
                                {
                                    string path = line.Substring(line.IndexOf(match.ToString())).Replace("\\\\", "\\").Replace("\"", "\\");
                                    if (Directory.Exists(path + "steamapps\\common"))
                                    {
                                        steamGameDirs.Add(path + "steamapps\\");
                                    }
                                }
                            }
                            steamGameDirs.Add(steam64path + "steamapps\\");
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Steam не установлен?");
            }

            List<Steam_InstalledGame> steam_InstalledGames = new List<Steam_InstalledGame>();
            foreach (string dir in steamGameDirs)
            {
                foreach (FileInfo file in new DirectoryInfo(dir).GetFiles("appmanifest_*.acf"))
                {
                    string appid = Regex.Match(file.Name, @"\d+").Value;
                    if (appid == "228980") continue; //Служебное приложение Steam

                    string directory = dir + "common\\";
                    string name = "";
                    string[] configLines = File.ReadAllLines(file.FullName);
                    foreach (string line in configLines)
                    {
                        if (line.Contains("\"installdir\""))
                        {
                            int begin = line.IndexOf('"', line.IndexOf("\"installdir\"") + "\"installdir\"".Length);
                            int end = line.IndexOf('"', begin + 1);
                            directory += line.Substring(begin + 1, end - begin - 1);
                        }
                        else if (line.Contains("\"name\""))
                        {
                            int begin = line.IndexOf('"', line.IndexOf("\"name\"") + "\"name\"".Length);
                            int end = line.IndexOf('"', begin + 1);
                            name = line.Substring(begin + 1, end - begin - 1);
                        }
                    }

                    steam_InstalledGames.Add(new Steam_InstalledGame(appid, name, directory));
                }
            }

            return steam_InstalledGames;
        }

        /// <summary>
        /// Поиск установленных игр Origin
        /// </summary>
        /// <returns>Список установленных игр, с названиями, директорией установки и адресом (вероятного) запускающего файла</returns>
        private static List<Origin_InstalledGame> Search_OriginInstalled()
        {
            List<Origin_InstalledGame> originGames = new List<Origin_InstalledGame>();
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
                    }
                    catch { continue; }

                    string title = subKey.GetValue("DisplayName").ToString();
                    if (title == "Origin") continue;

                    string installLocation = subKey.GetValue("InstallLocation").ToString();

                    if (!originGames.Exists(x => x.Name == title))
                    {
                        originGames.Add(new Origin_InstalledGame(title, installLocation));
                    }
                }
            }

            return originGames;
        }

        /// <summary>
        /// Поиск установленных игр Uplay
        /// </summary>
        /// <returns>Список установленных игр, с AppID, названиями и директорией установки</returns>
        private static List<Uplay_InstalledGame> Search_UplayInstalled()
        {
            List<Uplay_InstalledGame> uplayGames = new List<Uplay_InstalledGame>();
            string regkey = "SOFTWARE\\WOW6432Node\\Ubisoft\\Launcher\\Installs";
            RegistryKey key;
            try
            {
                key = Registry.LocalMachine.OpenSubKey(regkey);
            }
            catch
            {
                throw new Exception("Uplay не установлен?");
            }

            foreach (string ksubKey in key.GetSubKeyNames())
            {
                string dir = "", name = "";
                using (RegistryKey subKey = key.OpenSubKey(ksubKey))
                {
                    dir = subKey.GetValue("InstallDir").ToString();
                    name = dir.Remove(dir.Length - 1);
                    name = name.Substring(name.LastIndexOf('/') + 1);
                }
                uplayGames.Add(new Uplay_InstalledGame(name, dir, ksubKey));
            }

            return uplayGames;
        }
    }
}
