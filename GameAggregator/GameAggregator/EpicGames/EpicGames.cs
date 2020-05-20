using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using GameAggregator.EGames.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameAggregator.EGames
{
    #region enums

    /// <summary>
    /// Порядок сортировки
    /// </summary>
    public enum EGSortDir
    {
        ///<summary>по возрастанию</summary>
        Asc,
        ///<summary>по убыванию</summary>
        Desc
    }

    /// <summary>
    /// Ключ сортировки
    /// </summary>
    public enum EGSortBy
    {
        ///<summary>по актуальности (порядок сортировки не оказывает эффекта)</summary>
        Actuality,
        ///<summary>по алфавиту</summary>
        Title,
        ///<summary>по дате выхода</summary>
        Date
    }

    #endregion

    public class EpicGames
    {
        #region Fields

        #region URLs

        private readonly string egBackendUrl = "https://www.epicgames.com/store/backend/graphql-proxy";
        private readonly string egProductMappingUrl = "https://store-content.ak.epicgames.com/api/content/productmapping";
        private readonly string egProductUrl = "https://store-content.ak.epicgames.com/api/ru/content/products/";
    
        #endregion

        #region WebClients

        private WebClient egBackendClient;
        private WebClient egProductMappingClient;
        private WebClient egGetGameClient;
  
        #endregion

        #region Queries

        private string storeQueryVariables = "\"variables\":{{\"category\":\"games/edition/base|bundles/games|editors\",\"count\":{0},\"country\":\"RU\",\"keywords\":\"{1}\",\"locale\":\"ru\",\"sortBy\":\"{2}\",\"sortDir\":\"{3}\",\"start\":{4},\"tag\":\"\",\"allowCountries\":\"RU\",\"withPrice\":true";
        private string storeQuery = "{{\"query\":\"query searchStoreQuery($allowCountries: String, $category: String, $count: Int, $country: String!, $keywords: String, $locale: String, $namespace: String, $sortBy: String, $sortDir: String, $start: Int, $tag: String, $withPrice: Boolean = false, $withPromotions: Boolean = false) {{\\n  Catalog {{\\n    searchStore(allowCountries: $allowCountries, category: $category, count: $count, country: $country, keywords: $keywords, locale: $locale, namespace: $namespace, sortBy: $sortBy, sortDir: $sortDir, start: $start, tag: $tag) {{\\n      elements {{\\n        title\\n        id\\n        namespace\\n        description\\n        effectiveDate\\n        keyImages {{\\n          type\\n          url\\n        }}\\n        seller {{\\n          id\\n          name\\n        }}\\n        productSlug\\n        urlSlug\\n        url\\n        items {{\\n          id\\n          namespace\\n        }}\\n        customAttributes {{\\n          key\\n          value\\n        }}\\n        categories {{\\n          path\\n        }}\\n        price(country: $country) @include(if: $withPrice) {{\\n          totalPrice {{\\n            discountPrice\\n            originalPrice\\n            voucherDiscount\\n            discount\\n            currencyCode\\n            currencyInfo {{\\n              decimals\\n            }}\\n            fmtPrice(locale: $locale) {{\\n              originalPrice\\n              discountPrice\\n              intermediatePrice\\n            }}\\n          }}\\n          lineOffers {{\\n            appliedRules {{\\n              id\\n              endDate\\n              discountSetting {{\\n                discountType\\n              }}\\n            }}\\n          }}\\n        }}\\n        promotions(category: $category) @include(if: $withPromotions) {{\\n          promotionalOffers {{\\n            promotionalOffers {{\\n              startDate\\n              endDate\\n              discountSetting {{\\n                discountType\\n                discountPercentage\\n              }}\\n            }}\\n          }}\\n          upcomingPromotionalOffers {{\\n            promotionalOffers {{\\n              startDate\\n              endDate\\n              discountSetting {{\\n                discountType\\n                discountPercentage\\n              }}\\n            }}\\n          }}\\n        }}\\n      }}\\n      paging {{\\n        count\\n        total\\n      }}\\n    }}\\n  }}\\n}}\\n\",{0}}}}}";

        #endregion

        #region Registry

        private readonly string dataRegistryKey = @"SOFTWARE\Epic Games\EpicGamesLauncher";
        private readonly string dataRegistryName = "AppDataPath";

        #endregion

        private readonly string launchString = @"com.epicgames.launcher://apps/{0}?action=launch&silent=true";
        private readonly string productMappingCacheKey = "epicgames_productmapping";

        #endregion

        public EpicGames()
        {
            egBackendClient = new WebClient() { BaseAddress = egBackendUrl };
            egBackendClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            egBackendClient.Encoding = Encoding.UTF8;

            egProductMappingClient = new WebClient() { BaseAddress = egProductMappingUrl };
            egProductMappingClient.Encoding = Encoding.UTF8;

            egGetGameClient = new WebClient() { BaseAddress = egProductUrl };
            egGetGameClient.Encoding = Encoding.UTF8;

        }

        #region Store games

        /// <summary>
        /// Получает список игр из магазина Epic Games, в том числе производит поиск по ключевому слову.
        /// Возвращает null при выходе startIndex за пределы количества игр в полном списке
        /// </summary>
        /// <param name="startIndex">Позиция, с которой необходимо получить список игр</param>
        /// <param name="count">Количество игр начиная со startIndex, которое необходимо получить</param>
        /// <param name="sortBy">Ключ сортировки</param>
        /// <param name="sortDir">Порядок сортировки</param>
        /// <param name="keyword">Ключевое слово поиска (название)</param>
        /// <returns>Список игр</returns>
        public List<IEGameStore> GetStoreGames(int startIndex = 0, int count = 30, string keyword = "", EGSortBy sortBy = EGSortBy.Date, EGSortDir sortDir = EGSortDir.Desc)
        {
            var games = new List<IEGameStore>();

            MemoryCache memCache = MemoryCache.Default;
            if (!memCache.Contains(productMappingCacheKey))
            {
                string productMappingResponce = egProductMappingClient.DownloadString("");
                var jMapProd = JObject.Parse(productMappingResponce);
                memCache.Set(productMappingCacheKey, jMapProd, DateTimeOffset.Now.AddDays(7.0));
            }
            var jMap = (memCache.Get(productMappingCacheKey) as JObject).Properties();

            string sortByStr = (sortBy == EGSortBy.Title ? "title" : (sortBy == EGSortBy.Date ? "releaseDate" : null));
            string sortDirStr = sortDir.ToString().ToUpper();
            string variablesQuery = string.Format(storeQueryVariables, count, keyword, sortByStr, sortDirStr, startIndex);
            string query = string.Format(storeQuery, variablesQuery);

            string responce = egBackendClient.UploadString("", query);

            var jObj = JObject.Parse(responce);
            var elements = jObj["data"]["Catalog"]["searchStore"]["elements"];

            if (elements.Count() == 0)
                return null;

            foreach (var elem in elements.Children())
            {
                var name = elem["title"].ToString();
                var id = elem["id"].ToString();

                var gameNamespace = elem["namespace"].ToString();
                var urlName = jMap.First(x => x.Name == gameNamespace).Value.ToString();

                var imgWide = elem["keyImages"]
                    .First(x => x["type"].ToString() == "OfferImageWide")
                    ["url"].ToString();

                var imgTall = elem["keyImages"]
                        .FirstOrDefault(x => x["type"].ToString() == "OfferImageTall")?
                        ["url"].ToString();
                imgTall = imgTall ?? elem["keyImages"]
                        .FirstOrDefault(x => x["type"].ToString() == "DieselGameBoxLogo")?
                        ["url"].ToString();

                var jPrice = elem["price"]["totalPrice"];
                int dec = jPrice["currencyInfo"]["decimals"].Value<int>();
                double price = jPrice["originalPrice"].Value<int>() / Math.Pow(10, dec);
                double discountPrice = jPrice["discountPrice"].Value<int>() / Math.Pow(10, dec);
                string priceStr = jPrice["fmtPrice"]["originalPrice"].ToString();
                string discountPriceStr = jPrice["fmtPrice"]["discountPrice"].ToString();

                var game = new EGame()
                {
                    Name = name,
                    Id = id,
                    Namespace = gameNamespace,
                    UrlName = urlName,
                    ImageUrlWide = imgWide,
                    ImageUrlTall = imgTall,
                    Price = price,
                    PriceWithDiscount = discountPrice,
                    PriceStr = priceStr,
                    PriceWithDiscountStr = discountPriceStr
                };

                games.Add(game);
            }

            return games;
        }

        /// <summary>
        /// Делает отдельный запрос на получение полной информации о конкретной игре.
        /// Заполняет дополнительные поля в объекте EGame и возвращает его же в виде расширения EGameFull
        /// </summary>
        /// <param name="game">Игра, для которой нужно получить дополнительную информацию</param>
        /// <returns>EGameFull объект - расширение над переданным объектом game</returns>
        public IEGameStoreFull GetGameInfo(EGame game)
        {
            var gameInfo = game as IEGameStoreFull;

            string responce = egGetGameClient.DownloadString(game.UrlName);
            var jObj = JObject.Parse(responce);
            var data = jObj["pages"].FirstOrDefault()?["data"];

            var pNames = (data["meta"]?["publisher"] as JArray)?.FirstOrDefault() ?? data["about"]?["publisherAttribution"];
            var dNames = (data["meta"]?["developer"] as JArray)?.FirstOrDefault() ?? data["about"]?["developerAttribution"];
            gameInfo.PublisherName = pNames?.ToString() ?? "–";
            gameInfo.DeveloperName = dNames?.ToString() ?? "–";

            gameInfo.Date = data["meta"]?["releaseDate"]?.ToObject<DateTime>();
            gameInfo.DateStr = gameInfo.Date?.ToShortDateString() ?? "–";
            if (gameInfo.Date == null && data["meta"]?["customReleaseDate"] != null)
                gameInfo.DateStr = data["meta"]["customReleaseDate"].ToString();

            gameInfo.Languages = "–";
            var languagesList = data["requirements"]?["languages"]?.Select(x => x.ToString());
            if (languagesList != null && languagesList.Count() != 0)
                gameInfo.Languages = string.Join("\n", languagesList);

            var requirementsList = data["requirements"]?["systems"]?.Select(x => x.FirstOrDefault(y => (y as JProperty).Name == "systemType"));
            var platformsList = requirementsList?.Select(x => (x as JProperty).Value) ??
                (data["meta"]?["platform"] as JArray);
            gameInfo.Platforms = platformsList?.Select(x => x.ToString()).ToList() ?? new List<string>();

            gameInfo.Description = data["about"]?["description"]?.ToString() ?? "–";

            return gameInfo;
        }

        /// <summary>
        /// Получает изображение игры.
        /// </summary>
        /// <param name="game">Игра, для которой нужно скачать изображение</param>
        /// <param name="isWide">Если true - скачивается широкое представление изображения, иначе - длинное</param>
        /// <returns>Изображение в виде System.Drawing.Image объекта</returns>
        public Image GetGameImage(EGame game, bool isWide = false)
        {
            var client = new WebClient();
            var responce = client.DownloadData(isWide ? game.ImageUrlWide : game.ImageUrlTall);
            Image img;
            using (var stream = new MemoryStream(responce))
                img = Image.FromStream(stream);
            return img;
        }

        #endregion

        #region Installed games

        /// <summary>
        /// Возвращает список установленных игр
        /// </summary>
        /// <returns>Cписок объектов IEGameInstalled</returns>
        public List<IInstalledGame> GetInstalledGames()
        {
            try
            {
                string dataPath = "";
                using (RegistryKey dataKey = Registry.LocalMachine.OpenSubKey(dataRegistryKey))
                {
                    dataPath = dataKey.GetValue(dataRegistryName) as string;
                }

                if (dataPath == null || !Directory.Exists(Path.Combine(dataPath, "Manifests")))
                    return null;

                List<IInstalledGame> games = new List<IInstalledGame>();
                foreach (var file in Directory.GetFiles(Path.Combine(dataPath, "Manifests")))
                {
                    string fileContent = "";
                    using (StreamReader reader = new StreamReader(file))
                    {
                        fileContent = reader.ReadToEnd();
                    }

                    var jObj = JObject.Parse(fileContent);

                    if (string.IsNullOrEmpty(jObj["LaunchExecutable"].ToString()))
                        continue;

                    var name = jObj["DisplayName"].ToString();
                    var launchStr = string.Format(launchString, jObj["MainGameAppName"].ToString());

                    games.Add(new EpicGames_InstalledGame(name, launchStr));
                }

                return games;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Запускает выбранную игру
        /// </summary>
        /// <param name="game">Объект установленной игры</param>
        public void LaunchGame(IEGameInstalled game)
        {
            try
            {
                Process.Start(string.Format(launchString, game.LaunchName));
            }
            catch
            {
                throw new Exception($"Не удалось запустить игру {game.Name}");
            }
        }

        #endregion
    }
}
