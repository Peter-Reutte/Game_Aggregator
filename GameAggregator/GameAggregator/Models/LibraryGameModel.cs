namespace GameAggregator.Models
{
    /// <summary>Игра в библиотеке</summary>
    public interface ILibraryGame
    {
        /// <summary>Название игры</summary>
        string Name { get; set; }
        
        /// <summary>Лаунчер, в котором приобретена игра</summary>
        Launchers Launcher { get; set; }
    }

    public class Steam_LibraryGame : ILibraryGame
    {
        public string Name { get; set; }
        public Launchers Launcher { get; set; }

        public Steam_LibraryGame(string name)
        {
            Name = name;
            Launcher = Launchers.Steam;
        }
    }
}
