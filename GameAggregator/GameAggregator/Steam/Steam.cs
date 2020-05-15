using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Windows;

namespace GameAggregator.SteamStore
{
    public class Steam
    {
        private readonly string Key; // Ключ WebAPI
        private static WebClient SteamWebClient;

        public Steam()
        {
            SteamWebClient = new WebClient();

            //временно -- Steam Web API key
            //StreamReader sr = new StreamReader(@"D:\WebApiKey.txt");
            //Key = sr.ReadLine();
            //sr.Close();


            //для теста; нужно будет убрать в хорошее место
            //string price = GetGamePrice("Borderlands 3");
            //Steam_OwnedGameList list = GetOwnedGamesList("https://steamcommunity.com/profiles/76561198254132723"); //Dingo's profile (temp)
            //Image image = GetGameIcon(list.Response.Games[0]);
            //image.Save("test.jpg", ImageFormat.Jpeg);
            //LaunchGame(list.Response.Games[0].Appid);
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
        /// Получение форматированной строки с ценой игры в Steam
        /// </summary>
        /// <param name="name">Название игры</param>
        /// <returns>Форматированная строка с ценой</returns>
        public string GetGamePrice(string name)
        {
            MemoryCache memCache = MemoryCache.Default;
            if (!memCache.Contains("steamapps")) RefreshAppIDs();
            List<SteamGame> apps = memCache.Get("steamapps") as List<SteamGame>;

            SteamGame steamGame = apps.Find(x => x.Name == name);
            if (steamGame == null) return "не найдено";

            string appid = apps.Find(x => x.Name == name).Appid.ToString();

            try
            {
                string responce = SteamWebClient.DownloadString("https://store.steampowered.com/api/appdetails?appids=" + appid);
                JObject jsonObj = JObject.Parse(responce);
                return jsonObj[appid]["data"]["price_overview"]["final_formatted"].ToString();
            }
            catch
            {
                throw new Exception("Сервера Steam недоступны");
            }
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

        /// <summary>
        /// Запускает выбранную игру; нужен установленный Steam-клиент
        /// Если игра не установлена -- запускает установку
        /// </summary>
        /// <param name="appid">AppID игры</param>
        public void LaunchGame(string appid)
        {
            try
            {
                Process.Start("steam://rungameid/" + appid);
            }
            catch
            {
                MessageBox.Show("Клиент Steam не установлен!");
            }
        }
    }
}

