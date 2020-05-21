﻿using GameAggregator.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;

namespace GameAggregator.SteamStore
{
    public class Steam
    {
        private readonly string Key; // Ключ WebAPI
        private static WebClient SteamWebClient;

        public Steam()
        {
            SteamWebClient = new WebClient() { Encoding = Encoding.UTF8 };

            //временно -- Steam Web API key
            //StreamReader sr = new StreamReader(@"D:\WebApiKey.txt");
            //Key = sr.ReadLine();
            //sr.Close();
        }

        /// <summary>
        /// Получение и кеширование списка AppID всех игр Steam
        /// </summary>
        private void RefreshAppIDs()
        {
            string result;
            try
            {
                result = SteamWebClient.DownloadString("http://api.steampowered.com/ISteamApps/GetAppList/v0002");
            }
            catch
            {
                throw new Exception("Сервера Steam недоступны");
            }

            MemoryCache memCache = MemoryCache.Default;
            memCache.Set("steamapps", 
                JsonConvert.DeserializeObject<Steam_AppList>(result).Applist.Apps, DateTimeOffset.Now.AddDays(7.0));
        }

        /// <summary>
        /// Поиск игр в магазине Steam
        /// </summary>
        /// <param name="name">Название игры</param>
        /// <returns>Результат поиска; null, если не найдено</returns>
        public List<IStoreGame> GetGamePrice(string name)
        {
            MemoryCache memCache = MemoryCache.Default;
            if (!memCache.Contains("steamapps")) RefreshAppIDs();
            List<SteamGame> apps = memCache.Get("steamapps") as List<SteamGame>;

            List<SteamGame> steamGames = apps.FindAll(x => x.Name.ToLower().Contains(name.ToLower()));
            if (steamGames == null) return null;

            List<IStoreGame> storeGames = new List<IStoreGame>();
            foreach (SteamGame game in steamGames)
            {
                try
                {
                    string responce = SteamWebClient.DownloadString("https://store.steampowered.com/api/appdetails?appids=" + game.Appid);
                    JObject jsonObj = JObject.Parse(responce);
                    Steam_StoreGame steam_StoreGame = new Steam_StoreGame(game.Name,
                        double.Parse(jsonObj[game.Appid]["data"]["price_overview"]["final"].ToString()) / 100,
                        "https://store.steampowered.com/app/" + game.Appid);
                    storeGames.Add(steam_StoreGame);
                }
                catch
                {
                    throw new Exception("Сервера Steam недоступны");
                }
            }

            return storeGames;
        }

        /// <summary>
        /// Получает список игр из Steam-библиотеки пользозвателя
        /// </summary>
        /// <param name="playerProfileLink">Ссылка на профиль Steam</param>
        /// <returns>Список игр из Steam</returns>
        public Steam_OwnedGameList GetOwnedGamesList(string playerProfileLink)
        {
            string id = GetUserId(playerProfileLink);

            string responce;
            try
            {
                responce = SteamWebClient.DownloadString("http://api.steampowered.com/IPlayerService/GetOwnedGames/" +
                    "v0001/?key=" + Key + "&steamid=" + id + "&include_appinfo=true&format=json");
            }
            catch
            {
                throw new Exception("Ошибка доступа. Возможно, Ваш профиль скрыт, либо сервера недоступны.");
            }

            return JsonConvert.DeserializeObject<Steam_OwnedGameList>(responce);
        }

        /// <summary>
        /// Получает SteamID пользозвателя из ссылки на профиль
        /// </summary>
        /// <param name="playerProfileLink">Ссылка на профиль Steam</param>
        /// <returns>ID пользователя Steam (17 цифр)</returns>
        private string GetUserId(string playerProfileLink)
        {
            Uri uri = new UriBuilder(playerProfileLink).Uri;
            string id = uri.Segments[uri.Segments.Length - 1];

            if (!Regex.IsMatch(id, @"^\d{17}$"))
            {
                JObject jsonObj;
                try
                {
                    jsonObj = JObject.Parse(SteamWebClient.DownloadString(
                        "http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + Key + "&vanityurl=" + id));
                }
                catch
                {
                    throw new Exception("Сервера Steam недоступны");
                }

                if (jsonObj["response"]["success"].ToString() == "1")
                {
                    return jsonObj["response"]["steamid"].ToString();
                }
                else
                {
                    throw new Exception("Запрашиваемый профиль не существует.");
                }
            }

            return id;
        }

        /// <summary>
        /// Скачивает иконку игры
        /// </summary>
        /// <param name="game">Игра, для которой нужно получить иконку</param>
        /// <returns>Image-объект с иконкой</returns>
        public Image GetGameIcon(OwnedGame game)
        {
            byte[] data;
            try
            {
                data = SteamWebClient.DownloadData("http://media.steampowered.com/steamcommunity/public/images/apps/" + 
                    game.Appid + "/" + game.Img_icon_url + ".jpg");
            }
            catch
            {
                throw new Exception("Невозможно скачать иконку");
            }

            return Image.FromStream(new MemoryStream(data));
        }

        #region Installed game

        /// <summary>
        /// Поиск установленных игр Steam
        /// </summary>
        /// <returns>Список установленных игр, с AppID, названиями и директорией установки</returns>
        public static List<IInstalledGame> Search_SteamInstalled()
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
                            steamGameDirs.Add(steam64path + "\\steamapps\\");
                        }
                    }
                }
            }
            catch
            {
                throw new Exception("Steam не установлен?");
            }

            List<IInstalledGame> steam_InstalledGames = new List<IInstalledGame>();
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

                    steam_InstalledGames.Add(new Steam_InstalledGame(name, "steam://rungameid/" + appid, Launchers.Steam));
                }
            }

            return steam_InstalledGames;
        }

        #endregion
    }
}

