using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace GameAggregator.OriginStore
{
    public class Origin
    {
        private readonly WebClient OriginWebClient;

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
    }
}
