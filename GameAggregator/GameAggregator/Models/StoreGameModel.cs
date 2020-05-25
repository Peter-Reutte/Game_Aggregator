using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAggregator.Models
{
    /// <summary>Описание игры из магазина</summary>
    public interface IStoreGame
    {
        /// <summary>Название игры</summary>
        string Name { get; set; }

        /// <summary>Цена игры</summary>
        double Price { get; set; }

        /// <summary>Лаунчер, в котором приобретена игра</summary>
        Launchers Launcher { get; set; }

        /// <summary>Ссылка для перехода в магазин</summary>
        string Link { get; set; }
    }

    public class Steam_StoreGame : IStoreGame
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Link { get; set; }
        public Launchers Launcher { get; set; }

        public Steam_StoreGame(string name, double price, string link)
        {
            Name = name;
            Price = price;
            Link = link;
            Launcher = Launchers.Steam;
        }
    }

    public class Origin_StoreGame : IStoreGame
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Link { get; set; }
        public Launchers Launcher { get; set; }

        public Origin_StoreGame(string name, double price, string link)
        {
            Name = name;
            Price = price;
            Link = link;
            Launcher = Launchers.Origin;
        }
    }

    public class Uplay_StoreGame : IStoreGame
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Link { get; set; }
        public Launchers Launcher { get; set; }

        public Uplay_StoreGame(string name, double price, string link)
        {
            Name = name;
            Price = price;
            Link = link;
            Launcher = Launchers.Uplay;
        }
    }

    public class EpicGames_StoreGame : IStoreGame
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Link { get; set; }
        public Launchers Launcher { get; set; }

        public EpicGames_StoreGame(string name, double price, string link)
        {
            Name = name;
            Price = price;
            Link = link;
            Launcher = Launchers.EpicGames;
        }
    }

}
