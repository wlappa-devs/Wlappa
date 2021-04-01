using Server.Games.Meta;

namespace Server.Routing
{
    public interface IInitialMessage
    {
    }

    public enum GameType
    {
        Clicker,
    }

    public class GameCreate : IInitialMessage
    {
        public GameCreate(IGameConfiguration configuration, GameType gameType)
        {
            Configuration = configuration;
            CreatingGameType = gameType;
        }

        public GameType CreatingGameType { get; }

        public IGameConfiguration Configuration { get; }
    }

    public class GameJoin : IInitialMessage
    {
        public GameJoin(string gameCode)
        {
            GameCode = gameCode;
        }

        public string GameCode { get; }
    }
}