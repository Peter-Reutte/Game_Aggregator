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
using GameAggregator.Models;
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
        private readonly string egGamePageUrl = "https://www.epicgames.com/store/ru/product/";

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

        private static readonly string dataRegistryKey = @"SOFTWARE\Epic Games\EpicGamesLauncher";
        private static readonly string dataRegistryName = "AppDataPath";

        #endregion

        private static readonly string launchString = @"com.epicgames.launcher://apps/{0}?action=launch&silent=true";
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
        public List<IStoreGame> GetStoreGames(int startIndex = 0, int count = 30, string keyword = "", EGSortBy sortBy = EGSortBy.Date, EGSortDir sortDir = EGSortDir.Desc)
        {
            var games = new List<IStoreGame>();

            string sortByStr = (sortBy == EGSortBy.Title ? "title" : (sortBy == EGSortBy.Date ? "releaseDate" : null));
            string sortDirStr = sortDir.ToString().ToUpper();
            string variablesQuery = string.Format(storeQueryVariables, count, keyword, sortByStr, sortDirStr, startIndex);
            string query = string.Format(storeQuery, variablesQuery);
            string response;
            IEnumerable<JProperty> jMap;

            try
            {
                MemoryCache memCache = MemoryCache.Default;
                if (!memCache.Contains(productMappingCacheKey))
                {
                    string productMappingResponce = egProductMappingClient.DownloadString("");
                    var jMapProd = JObject.Parse(productMappingResponce);
                    memCache.Set(productMappingCacheKey, jMapProd, DateTimeOffset.Now.AddDays(7.0));
                }
                jMap = (memCache.Get(productMappingCacheKey) as JObject).Properties();

                response = egBackendClient.UploadString("", query);
            }
            catch
            {
                throw new Exception("Сервер Epic Games недоступен");
            }

            var jObj = JObject.Parse(response);
            var elements = jObj["data"]["Catalog"]["searchStore"]["elements"];

            if (elements.Count() == 0)
                return null;

            foreach (var elem in elements.Children())
            {
                try
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
                    string link = egGamePageUrl + elem["productSlug"].ToString();

                    games.Add(new EpicGames_StoreGame(name, price, link));
                }
                catch
                {
                    continue;
                }
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

            string response;

            try
            {
                response = egGetGameClient.DownloadString(game.UrlName);
            }
            catch
            {
                throw new Exception("Сервер Epic Games недоступен");
            }

            try
            {
                var jObj = JObject.Parse(response);
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
            }
            catch { }

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
            byte[] response;

            try
            {
                var client = new WebClient();
                response = client.DownloadData(isWide ? game.ImageUrlWide : game.ImageUrlTall);
            }
            catch
            {
                throw new Exception("Сервер Epic Games недоступен");
            }
            Image img;
            using (var stream = new MemoryStream(response))
                img = Image.FromStream(stream);
            return img;
        }

        #endregion

        #region Installed games

        /// <summary>
        /// Возвращает список установленных игр
        /// </summary>
        /// <returns>Cписок объектов IEGameInstalled</returns>
        public static List<IInstalledGame> Search_EpicGamesInstalled()
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
            catch
            {
                throw new Exception("Клиент Epic Games возможно не установлен на ваш компьютер.");
            }
        }

        /// <summary>
        /// Запускает выбранную игру
        /// </summary>
        /// <param name="game">Объект установленной игры</param>
        public void LaunchGame(IInstalledGame game)
        {
            try
            {
                Process.Start(game.LaunchString);
            }
            catch
            {
                throw new Exception($"Не удалось запустить игру {game.Name}");
            }
        }

        #endregion
    }
}
