using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace GameAggregator
{
    public class Steam
    {
        /// <summary>
        /// Ключ WebAPI
        /// </summary>
        private readonly string Key;

        /// <summary>
        /// ID пользователя Steam
        /// </summary>
        private readonly string Id;

        public Steam(string id)
        {
            //temp
            StreamReader sr = new StreamReader(@"D:\WebApiKey.txt");
            Key = sr.ReadLine();
            sr.Close();

            if (!Regex.IsMatch(id, @"^\d{17}$"))
            {
                Steam_GetId getId = JsonConvert.DeserializeObject<Steam_GetId>(new WebClient().DownloadString(
                    "http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + Key + "&vanityurl=" + id));

                if (getId.Response.Success == 1)
                {
                    Id = getId.Response.Steamid;
                }
                else
                {
                    throw new Exception("Запрашиваемый профиль не существует.");
                }
            }
            else
            {
                Id = id;
            }

            //для теста; нужно будет убрать в хорошее место
            Steam_GameList list = GetSteamGamesList();
            Image image = GetGameIcon(list.Response.Games[0]);
            image.Save("test.jpg", ImageFormat.Jpeg);
            LaunchGame(list.Response.Games[0]);
        }

        public Steam_GameList GetSteamGamesList()
        {
            string responce;
            try
            {
                responce = new WebClient().DownloadString("http://api.steampowered.com/IPlayerService/GetOwnedGames/" +
                    "v0001/?key=" + Key + "&steamid=" + Id + "&include_appinfo=true&format=json");
            }
            catch
            {
                MessageBox.Show("Ошибка доступа. Возможно, Ваш профиль скрыт, либо сервера недоступны.");
                return null;
            }

            return JsonConvert.DeserializeObject<Steam_GameList>(responce);
        }

        /// <summary>
        /// Скачивает иконку игры
        /// </summary>
        /// <param name="game">Игра, для которой нужно получить иконку</param>
        /// <returns>Image-объект с иконкой</returns>
        public Image GetGameIcon(Game game)
        {
            byte[] data;
            try
            {
                data = new WebClient().DownloadData("http://media.steampowered.com/steamcommunity/public/images/apps/" +
                    +game.Appid + "/" + game.Img_icon_url + ".jpg");
            }
            catch
            {
                MessageBox.Show("Невозможно скачать иконку");
                return null;
            }

            return Image.FromStream(new MemoryStream(data));
        }

        /// <summary>
        /// Запускает выбранную игру; нужен установленный Steam-клиент
        /// Если игра не установлена -- запускает установку
        /// TODO: Если не установлен Steam-клиент -- отсылать на страницу для скачивания ?
        /// </summary>
        /// <param name="game">Игра для запуска</param>
        public void LaunchGame(Game game)
        {
            Process.Start("steam://rungameid/" + game.Appid);
        }
    }

}

