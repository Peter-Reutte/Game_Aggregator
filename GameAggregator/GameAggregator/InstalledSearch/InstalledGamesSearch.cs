﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameAggregator.InstalledSearch
{
    public static class InstalledGamesSearch
    {
        public static void SearchForinstalledGames()
        {
            Search_SteamInstalled();
        }

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

                    steam_InstalledGames.Add(new Steam_InstalledGame(int.Parse(appid), name, directory));
                }
            }

            return steam_InstalledGames;
        }
    }
}
