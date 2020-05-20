using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;

namespace GameAggregator.OriginStore
{
    public class Origin
    {
        private readonly WebClient OriginWebClient;

        public Origin() => OriginWebClient = new WebClient() { Encoding = Encoding.UTF8 };


        /// <summary>
        /// Поиск цены игры в магазине Origin
        /// </summary>
        /// <param name="name">Название для поиска</param>
        /// <returns>Список найденных вариантов (включая базовые игры, DLC и проч.)</returns>
        public List<OriginGame> GetGamePrice(string name)
        {
            JObject jData;
            string responce;
            try
            {
                string cacheKey = "originstoreitems";
                MemoryCache memCache = MemoryCache.Default;
                if (!memCache.Contains(cacheKey))
                {
                    string jsonStoreDataUrl = "https://api4.origin.com/supercat/RU/en_RU/supercat-PCWIN_MAC-RU-en_RU.json.gz";
                    string jDataResponce = OriginWebClient.DownloadString(jsonStoreDataUrl);
                    jData = JObject.Parse(jDataResponce);
                    memCache.Set(cacheKey, jData, DateTimeOffset.Now.AddDays(7.0));
                }
                jData = memCache.Get(cacheKey) as JObject;

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
                    //Поиск в json-базе объектов с OfferPath, указанных в поле children
                    string[] gamePaths = token["children"].ToString().Split(',');
                    var withSamePaths = (jData["offers"] as JArray).Where(x => gamePaths.Contains(x["offerPath"].ToString()));
                    //Выбор тех вариантов, для которых указана цена
                    var withPrice = withSamePaths.Where(x => x["countries"]["catalogPrice"].ToString() != "");
                    if (withPrice.Count() > 1)
                    {
                        //Если несколько версий, берем standard-edition, если она есть
                        var standardEdition = withPrice.Where(x => x["offerPath"].ToString().Contains("standard-edition"));
                        if (standardEdition.Count() != 0)
                            withPrice = standardEdition;
                    }
                    //Выводим только минимальную цену, если нет standard edition?
                    //Такое в основном у валюты и игр с кастомными названиями изданий
                    //Если не удалось извлечь цену, на всякий случай указывается -1, хотя у всех текущих позиций цена есть
                    double price = withPrice.Count() == 0 ? -1 : withPrice.Select(x => x["countries"]["catalogPrice"].Value<double>()).Min();

                    links.Add(new OriginGame(token["gameName"].ToString(), token["path"].ToString(), price));
                }
            }

            return links;
        }

        #region Installed games

        /// <summary>
        /// Поиск установленных игр Origin
        /// </summary>
        /// <returns>Список установленных игр, с названиями, директорией установки и адресом (вероятного) запускающего файла</returns>
        private static List<IInstalledGame> Search_OriginInstalled()
        {
            List<IInstalledGame> originGames = new List<IInstalledGame>();
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

                    string name = subKey.GetValue("DisplayName").ToString();
                    if (name == "Origin") continue;

                    string installLocation = subKey.GetValue("InstallLocation").ToString();

                    if (!originGames.Exists(x => x.Name == name))
                    {
                        originGames.Add(new Origin_InstalledGame(name, installLocation + name + ".exe", Launchers.Origin));
                    }
                }
            }

            return originGames;
        }

        #endregion
    }
}
