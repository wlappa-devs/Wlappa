using System.Collections.Generic;

namespace Server.Games.Meta
{
    public interface IGame
    {
        void HandleEvent(IGameEvent e);
    }

    public interface IGameConfiguration
    {
    }

    public class GameCreationPayload
    {
        public GameCreationPayload(IReadOnlyDictionary<IPlayer, string> playerToRole)
        {
            PlayerToRole = playerToRole;
        }

        public IReadOnlyDictionary<IPlayer, string> PlayerToRole { get; }
    }

    public interface IGameFactoryMark<T> where T : IGame
    {
    }

    public interface IGameFactory
    {
        IGame Create(IGameConfiguration config, GameCreationPayload payload);
        IReadOnlyList<string> Roles { get; }
    }

    public interface IGameEvent
    {
    }

    public interface IPlayerEvent : IGameEvent
    {
        IPlayer Player { get; }
    }
}