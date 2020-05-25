using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAggregator.EGames.Models
{
    #region Intefaces

    //Базовые поля игры
    public interface IEGame
    {
        string Id { get; set; }
        string Name { get; set; }
        string Namespace { get; set; }
    }

    //Поля описания установленной игры
    public interface IEGameInstalled : IEGame
    {
        string ExePath { get; set; }
        string LaunchName { get; set; }
    }

    //Поля описания игры из магазина
    public interface IEGameStore : IEGame
    {
        string UrlName { get; set; }

        string ImageUrlWide { get; set; } //Широкий формат изображения
        string ImageUrlTall { get; set; } //Длинный формат изображения (если отсутствует, указывается URL логотипа, если он есть)

        double Price { get; set; }
        string PriceStr { get; set; } //Example: "999,00 ₽"
        double PriceWithDiscount { get; set; }
        string PriceWithDiscountStr { get; set; }
    }

    //Дополнительные поля описания, доступные при обращении к конкретной игре в магазине
    public interface IEGameStoreFull : IEGameStore
    {
        string DeveloperName { get; set; }
        string PublisherName { get; set; }

        DateTime? Date { get; set; }
        string DateStr { get; set; } //Example: "16.04.2020" (для вышедших), "2021", "Летом 2021", "-" (для предстоящих)

        IList<string> Platforms { get; set; }
        string Description { get; set; }
        string Languages { get; set; }
    }

    #endregion

    public class EGame : IEGameStore, IEGameStoreFull, IEGameInstalled
    {
        //IEGame
        public string Id { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }

        //IEGameStore
        public string UrlName { get; set; }

        public string ImageUrlWide { get; set; } //Широкий формат изображения
        public string ImageUrlTall { get; set; } //Длинный формат изображения (если отсутствует, указывается URL логотипа, если он есть)

        public double Price { get; set; }
        public string PriceStr { get; set; } //Example: "999,00 ₽"
        public double PriceWithDiscount { get; set; }
        public string PriceWithDiscountStr { get; set; }

        //IEGameStoreFull
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }

        public DateTime? Date { get; set; }
        public string DateStr { get; set; } //Example: "16.04.2020" (для вышедших), "2021", "Летом 2021", "-" (для предстоящих)

        public IList<string> Platforms { get; set; }
        public string Description { get; set; }
        public string Languages { get; set; }

        //IEGameInstalled
        public string ExePath { get; set; }
        public string LaunchName { get; set; }
    }

}
