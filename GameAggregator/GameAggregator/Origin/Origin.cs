using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace GameAggregator.OriginStore
{
    public class Origin
    {
        private readonly WebClient OriginWebClient;

        public Origin() => OriginWebClient = new WebClient();


        /// <summary>
        /// Поиск игр в магазине Origin
        /// </summary>
        /// <param name="name">Название длдя поиска</param>
        /// <returns>Список найденных вариантов (включая базовые игры, DLC и проч.)</returns>
        public List<OriginGame> GetLinksToOriginGames(string name)
        {
            string responce;
            try
            {
                responce = OriginWebClient.DownloadString("https://api4.origin.com/xsearch/store/en_us/rus/products?searchTerm="
                    + name + "&sort=rank desc,releaseDate desc,title desc&start=0&rows=20&isGDP=true");
            }
            catch
            {
                throw new Exception("Сервера Origin недоступны");
            }

            JObject jsonObj = JObject.Parse(responce);
            List<OriginGame> links = new List<OriginGame>();
            if (jsonObj["games"]["game"] != null)
            {
                foreach (JToken token in jsonObj["games"]["game"].Children())
                {
                    links.Add(new OriginGame(token["gameName"].ToString(), token["path"].ToString()));
                }
            }

            return links;
        }

        /// <summary>
        /// Запускает выбранную игру
        /// </summary>
        /// <param name="game">Путь к exe-файу, запускающему игру</param>
        public void LaunchGame(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch //Если exe-файл игры нестандартный, запускается сам Origin
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
                            }
                        }
                        catch { continue; }
                    }
                }
            }
        }
    }
}
