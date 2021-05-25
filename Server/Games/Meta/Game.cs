using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Server.Routing;
using Server.Routing.Helpers;
using Shared.Protos;

namespace Server.Games.Meta
{
    public abstract class Game
    {
        public abstract Task HandleEvent(IInGameClient? client, InGameClientMessage e);

        public SemaphoreSlim semaphore { get; } = new SemaphoreSlim(1, 1);
    }

    public class GameCreationPayload
    {
        public GameCreationPayload(IReadOnlyDictionary<Guid, string> playerToRole)
        {
            PlayerToRole = playerToRole;
        }

        public IReadOnlyDictionary<Guid, string> PlayerToRole { get; }
    }

    public interface IGameFactory
    {
        public string DefaultRole { get; }
        Game Create(GameConfiguration config, GameCreationPayload payload, IReadOnlyCollection<IInGameClient> clients,
            Func<Task> finished);

        IReadOnlyList<string> Roles { get; }
        GameTypes Type { get; }

        string? ValidateConfig(GameConfiguration config, GameCreationPayload payload);
    }

    public interface IGameEvent
    {
    }

    public interface IPlayerEvent : IGameEvent
    {
        IPlayer Player { get; }
    }
}