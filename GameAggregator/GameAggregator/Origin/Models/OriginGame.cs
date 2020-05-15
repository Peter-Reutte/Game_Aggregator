namespace GameAggregator.OriginStore
{
    public class OriginGame
    {
        public string Name { get; }

        public string Link { get; }

        public double Price { get; }

        /// <summary>
        /// Представление игры в магазине Origin
        /// </summary>
        /// <param name="name">Название</param>
        /// <param name="link">Ссылка на страницу в магазине</param>
        /// <param name="price">Цена в магазине</param>
        public OriginGame(string name, string link, double price)
        {
            Name = name;
            Link = "https://www.origin.com/rus/en-us/store" + link;
            Price = price;
        }
    }
}
