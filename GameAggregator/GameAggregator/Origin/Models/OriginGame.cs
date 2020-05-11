namespace GameAggregator.OriginStore
{
    public class OriginGame
    {
        public string Name { get; }

        public string Link { get; }

        /// <summary>
        /// Представление игры в магазине Origin
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="link">Ссылка на страницу в магазине</param>
        public OriginGame(string name, string link)
        {
            Name = name;
            Link = "https://www.origin.com/rus/en-us/store" + link;
        }
    }
}
