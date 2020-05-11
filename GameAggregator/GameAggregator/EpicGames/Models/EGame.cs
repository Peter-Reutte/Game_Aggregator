using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAggregator.EGames.Models
{
    public class EGame
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Namespace { get; set; }
        public string UrlName { get; set; }

        public string ImageUrlWide { get; set; } //Широкий формат изображения
        public string ImageUrlTall { get; set; } //Длинный формат изображения (если отсутствует, указывается URL логотипа, если он есть)

        public double Price { get; set; }
        public string PriceStr { get; set; } //Example: "999,00 ₽"
        public double PriceWithDiscount { get; set; }
        public string PriceWithDiscountStr { get; set; }
    }

    //Дополнительные поля описания, доступные при обращении к конкретной игре
    public class EGameFull : EGame
    {
        public string DeveloperName { get; set; }
        public string PublisherName { get; set; }

        public DateTime? Date { get; set; }
        public string DateStr => Date.Value.ToShortDateString() ?? "–";

        public IList<string> Platforms { get; set; }
        public string Description { get; set; }
        public string Languages { get; set; }
    }

}
